namespace FtpUpload;

/// <summary>
/// The NG-retry console — a manual recovery tool that runs completely SEPARATELY from the live
/// upload engine (its own queue, its own pump, its own log). It browses any day's NG items
/// (files that ended FAILED or TIMEDOUT), which it rebuilds purely from that day's jobs + raw
/// logs, and re-uploads them with:
///   • a manually chosen IP (Auto / Primary / Secondary),
///   • UNLIMITED retries (the count is logged, not capped),
///   • outcomes written to a separate per-day ng-retry log.
/// It never touches the live engine's objects, so recovering old days can't disturb today's line.
/// </summary>
public sealed class NgRetryEngine(Config cfg, NgRetryLog ngLog)
{
    private readonly IFtpTransfer _ftp = FtpEngineFactory.Create(cfg, reuseConnections: true);   // NG pump reuses one connection too
    private readonly ManifestWriter _manifest = new(cfg);   // update a panel's index/host on NG recovery
    private readonly object _gate = new();
    private List<NgItem> _items = new();     // the loaded day's NG items
    private readonly List<NgItem> _queue = new();
    private NgItem? _inFlight;
    private NgItem? _current;                 // actively worked item (persists across cooldown)

    public NgIpMode IpMode { get; set; } = NgIpMode.Auto;
    public bool AutoRunning { get; private set; }
    public string LoadedDay { get; private set; } = "";

    public IReadOnlyList<NgItem> Items { get { lock (_gate) return _items.ToList(); } }
    public NgItem? InFlight { get { lock (_gate) return _inFlight; } }
    /// <summary>The item the pump is actively working — set through its whole attempt+cooldown,
    /// so the highlight and auto-scroll have a stable target even when uploads are instant.</summary>
    public NgItem? Current { get { lock (_gate) return _current; } }
    public DateTime InFlightStarted { get; private set; }
    /// <summary>Work remaining: items not yet recovered (plus any manual one-shot retries).</summary>
    /// <summary>Items the auto-sweep should still work: not recovered, not terminally gone.
    /// (A Gone item can still be retried manually via the row's Retry button.)</summary>
    private static bool Actionable(NgItem x) => !x.DisplayOnly
        && x.State != NgItemState.Succeeded && x.State != NgItemState.Gone;

    public int QueueLength { get { lock (_gate) return _items.Count(Actionable) + _queue.Count; } }

    public event Action? Changed;
    private void NotifyChanged() => Changed?.Invoke();

    public event Action<string>? Logged;
    private void Log(string msg) => Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

    // ---------------- loading a day ----------------

    /// <summary>
    /// Rebuilds the NG list for one day (yyyyMMdd) from files: the jobs file gives local/remote
    /// paths, the raw log gives each file's final status, and the ng-retry log removes anything
    /// already recovered and carries its prior retry count. Read-only until the operator retries.
    /// </summary>
    private int _loadGen;   // bumped on every LoadDay; a running sweep aborts if this changes

    public void LoadDay(string day)
    {
        var items = BuildItems(day);
        lock (_gate)
        {
            AutoRunning = false;
            _queue.Clear();
            if (_current is not null) { _current.IsCurrent = false; _current = null; }
            _items = items.OrderBy(i => i.Pid).ThenBy(i => i.FileName).ToList();
            LoadedDay = day;
            _loadGen++;
        }
        Log($"NG list loaded for {day}: {items.Count} item(s)");
        NotifyChanged();
    }

    private DateTime _lastMerge = DateTime.MinValue;

    /// <summary>
    /// While the console is showing TODAY, merge in any files that have since failed or timed out
    /// in the live run — without disturbing items already loaded/in-flight or stopping an
    /// auto-retry. Called on a throttle by the watch loop. Past days are static (no merge).
    /// </summary>
    public void RefreshLoadedDayLive()
    {
        var today = Clock.Now.ToString("yyyyMMdd");
        if (LoadedDay != today) return;
        if ((DateTime.Now - _lastMerge).TotalSeconds < 2) return;
        _lastMerge = DateTime.Now;

        var fresh = BuildItems(today);
        var freshKeys = new HashSet<string>(fresh.Select(i => i.Key));
        var added = new List<NgItem>();
        int removed;
        lock (_gate)
        {
            var existing = new HashSet<string>(_items.Select(i => i.Key));
            foreach (var it in fresh)
                if (!existing.Contains(it.Key)) { _items.Add(it); added.Add(it); }

            // Display-only manifest rows are purely derived from BuildItems (which drops them once the
            // manifest is sent/recovered or the panel leaves NG). Remove any that are no longer in the
            // fresh build so they don't linger showing Pending after the manifests are delivered.
            removed = _items.RemoveAll(i => i.DisplayOnly && !freshKeys.Contains(i.Key));

            if (added.Count > 0 || removed > 0)
                _items = _items.OrderBy(i => i.Pid).ThenBy(i => i.FileName).ToList();
        }
        if (added.Count > 0 || removed > 0)
        {
            if (added.Count > 0) Log($"NG list: +{added.Count} new item(s) for today");
            NotifyChanged();
        }
    }

    /// <summary>Build the NG items for a day from files (jobs paths + raw-log final status, minus
    /// anything already recovered in the ng-retry log, carrying prior retry counts).</summary>
    private List<NgItem> BuildItems(string day)
    {
        var paths = new Dictionary<string, (string local, string remote, string indexSrc, string hostSrc, string upIdx, string upHost, bool isManifest)>();
        foreach (var line in SafeFile.ReadLines(cfg.JobsPathForDay(day)))
        {
            var jl = JobsLine.Parse(line);
            if (jl is null) continue;
            var remote = jl.RemotePath.Length > 0
                ? jl.RemotePath
                : $"{cfg.RemoteBaseFolder.TrimEnd('/')}/{jl.Pid}/{jl.FileName}";
            paths[jl.Pid + "|" + jl.FileName] = (jl.LocalPath, remote, jl.IndexSrc, jl.HostSrc, jl.UploadIndexPath, jl.UploadHostPath, jl.IsManifest);
        }

        var status = new Dictionary<string, string>();     // last line per file wins
        foreach (var line in SafeFile.ReadLines(cfg.RawLogPathForDay(day)))
        {
            var p = line.Split('|');
            if (p.Length < 3) continue;
            status[p[0] + "|" + p[1]] = p[2];
        }

        var prior = ngLog.ReadState(day);

        var items = new List<NgItem>();
        foreach (var kv in status)
        {
            if (kv.Value != "FAILED" && kv.Value != "TIMEDOUT") continue;
            var (retries, recovered) = prior.TryGetValue(kv.Key, out var e) ? e : (0, false);
            if (recovered) continue;                       // already fixed via NG retry

            var parts = kv.Key.Split('|');
            paths.TryGetValue(kv.Key, out var pp);
            items.Add(new NgItem
            {
                Day = day,
                Pid = parts[0],
                FileName = parts[1],
                LocalPath = pp.local ?? "",
                RemotePath = pp.remote ?? "",
                IndexSrc = pp.indexSrc ?? "",
                HostSrc = pp.hostSrc ?? "",
                UploadIndexPath = pp.upIdx ?? "",
                UploadHostPath = pp.upHost ?? "",
                IsManifest = pp.isManifest,
                OrigStatus = kv.Value,
                PriorRetries = retries
            });
        }

        // Show the index + host for any panel whose DATA files are in NG but whose manifests are NOT
        // themselves NG items (the panel's data failed/timed-out, so the manifests were never sent
        // live). These are DISPLAY-ONLY rows: the pump never retries them (their content isn't ready);
        // the post-step sends the real manifest once the data files recover. Their ORIGINAL is
        // "Pending" (they never failed — they're just waiting); once recovered they drop off.
        var pidsWithItems = items.Where(it => !it.IsManifest).Select(it => it.Pid).ToHashSet();
        foreach (var kv in paths)
        {
            if (!kv.Value.isManifest) continue;                 // only manifest jobs lines
            if (status.ContainsKey(kv.Key)) continue;           // already a real (retryable) NG item
            var parts = kv.Key.Split('|');
            var pid = parts[0];
            if (!pidsWithItems.Contains(pid)) continue;         // panel not in NG — skip
            var (retries, recovered) = prior.TryGetValue(kv.Key, out var e) ? e : (0, false);
            if (recovered) continue;                            // already sent (post-step) — drop off
            var pp = kv.Value;
            items.Add(new NgItem
            {
                Day = day, Pid = pid, FileName = parts[1],
                LocalPath = pp.local ?? "", RemotePath = pp.remote ?? "",
                IndexSrc = pp.indexSrc ?? "", HostSrc = pp.hostSrc ?? "",
                UploadIndexPath = pp.upIdx ?? "", UploadHostPath = pp.upHost ?? "",
                IsManifest = true, DisplayOnly = true,
                OrigStatus = "PENDING", PriorRetries = retries,
                State = NgItemState.Waiting
            });
        }

        // Order to match the main rawlog / jobs file: within each panel, data files first, then the
        // index manifest, then the host manifest (host last). Preserves panel order and the original
        // data-file order; only pushes the two manifests to the end (index before host).
        var pidOrder = new Dictionary<string, int>();
        foreach (var it in items)
            if (!pidOrder.ContainsKey(it.Pid)) pidOrder[it.Pid] = pidOrder.Count;
        static int Rank(NgItem it) => !it.IsManifest ? 0 : (it.RemotePath == it.UploadIndexPath ? 1 : 2);
        items = items
            .Select((it, i) => (it, i))
            .OrderBy(x => pidOrder[x.it.Pid])
            .ThenBy(x => Rank(x.it))
            .ThenBy(x => x.i)
            .Select(x => x.it)
            .ToList();

        return items;
    }

    // ---------------- controls ----------------

    /// <summary>Queue every not-yet-recovered item and keep retrying (unlimited) until stopped.</summary>
    public void StartAutoRetry()
    {
        int n;
        lock (_gate)
        {
            AutoRunning = true;
            _queue.Clear();
            foreach (var it in _items)
                if (Actionable(it)) it.State = NgItemState.Waiting;
            n = _items.Count(Actionable);
        }
        Log($"NG auto-retry started: {n} item(s), IP={IpMode}");
        NotifyChanged();
    }

    public void StopAutoRetry()
    {
        lock (_gate) { AutoRunning = false; _queue.Clear(); }
        ClearCurrent();
        Log("NG auto-retry stopped");
        NotifyChanged();
    }

    /// <summary>Retry a single item once (does not start the unlimited loop).</summary>
    public void RetryOne(string key)
    {
        lock (_gate)
        {
            var it = _items.FirstOrDefault(x => x.Key == key);
            // Display-only manifest rows are informational — never queue them (their content isn't
            // ready; the post-step sends the real manifest when the panel's data files recover).
            if (it is not null && !it.DisplayOnly && it.State != NgItemState.Succeeded && !_queue.Contains(it))
            {
                it.State = NgItemState.Waiting;
                _queue.Add(it);
            }
        }
        NotifyChanged();
    }

    private string HostFor(NgItem item) => IpMode switch
    {
        NgIpMode.Primary => cfg.PrimaryHost,
        NgIpMode.Secondary => cfg.SecondaryHost,
        // Auto: alternate primary/secondary each attempt so a dead IP doesn't trap it.
        _ => (item.SessionRetries % 2 == 0 ? cfg.PrimaryHost : cfg.SecondaryHost)
    };

    private void SetCurrent(NgItem item)
    {
        lock (_gate)
        {
            if (_current is not null && !ReferenceEquals(_current, item)) _current.IsCurrent = false;
            _current = item;
            item.IsCurrent = true;
        }
        NotifyChanged();
    }

    private void ClearCurrent()
    {
        var changed = false;
        lock (_gate)
        {
            if (_current is not null) { _current.IsCurrent = false; _current = null; changed = true; }
        }
        if (changed) NotifyChanged();
    }

    // ---------------- the pump ----------------

    public async Task RunAsync(CancellationToken stopping)
    {
        while (!stopping.IsCancellationRequested)
        {
            // 1) Manual one-shot retries (RetryOne) are drained first, even when auto-retry is off.
            NgItem? manual = null;
            lock (_gate) if (_queue.Count > 0) { manual = _queue[0]; _queue.RemoveAt(0); }
            if (manual is not null)
            {
                if (manual.State != NgItemState.Succeeded) await Attempt(manual, stopping);
                continue;
            }

            // 2) Auto-retry SWEEP: attempt every not-yet-recovered item once, back to back (no
            // per-item wait), then a SINGLE cooldown before the next sweep. This retries the whole
            // list promptly without hammering any one dead item, and the highlight isn't stalled
            // 5s per item.
            if (AutoRunning)
            {
                List<NgItem> sweep;
                int gen;
                lock (_gate) { sweep = _items.Where(Actionable).ToList(); gen = _loadGen; }

                if (sweep.Count > 0)
                {
                    var anyFailed = false;
                    foreach (var item in sweep)
                    {
                        if (stopping.IsCancellationRequested || !AutoRunning) break;
                        if (gen != _loadGen) break;                          // day reloaded — abandon this sweep
                        if (!Actionable(item)) continue;                     // recovered or gone mid-sweep
                        await Attempt(item, stopping);
                        if (Actionable(item)) anyFailed = true;
                    }

                    if (AutoRunning && anyFailed && gen == _loadGen)
                    {
                        ClearCurrent();   // no highlight during the between-sweep pause
                        try { await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, cfg.NgRetryCooldownSeconds)), stopping); }
                        catch (OperationCanceledException) { break; }
                    }
                    continue;
                }
            }

            // 3) Nothing to do — idle.
            ClearCurrent();
            await _ftp.EndSession();   // close the reused connection while idle
            await Task.Delay(300, stopping);
        }
    }

    private async Task Attempt(NgItem item, CancellationToken ct)
    {
        var host = HostFor(item);            // uses the count BEFORE this attempt (Auto alternates)
        item.LastHost = host;
        SetCurrent(item);                    // highlight moves only when a real upload begins
        item.State = NgItemState.Uploading;
        lock (_gate) { _inFlight = item; InFlightStarted = DateTime.Now; }
        Log($"NG retry {item.Day} {item.Pid}/{item.FileName} -> ftp://{host}/{item.RemotePath.TrimStart('/')} (retry #{item.TotalRetries + 1})");
        NotifyChanged();

        var result = await _ftp.UploadToHostAsync(item.ToJobFile(), host, ct);

        lock (_gate) _inFlight = null;
        item.SessionRetries++;               // this attempt counts toward the retry total

        if (result.Outcome == TransferOutcome.Success)
        {
            item.State = NgItemState.Succeeded;
            item.LastResult = "OK";
            ngLog.Write(item, true, host);
            Log($"   OK  {item.Pid}/{item.FileName} -> ftp://{host}/{item.RemotePath.TrimStart('/')} after {item.TotalRetries} retry/ies");

            // A manifest item IS the index/host file — it was just uploaded directly, so there is no
            // manifest to update or finalize (doing so would re-send it). Only DATA files trigger the
            // manifest-update + finalize below.
            if (!item.IsManifest)
            {
                // Panel file recovered — strip its " -pending" from the index/host manifest, then try
                // to finalize the panel (send index+host). The claim-sentinel means this is safe even
                // if the live engine also tries: only one of them sends. Crucial for PAST-day panels,
                // which have no live Job for the live engine to finalize.
                _manifest.MarkUploaded(item.IndexSrc, item.HostSrc, item.RemotePath);
                if (item.IndexSrc.Length > 0)
                {
                    try
                    {
                        if (await _manifest.TryFinalizeAsync(item.IndexSrc, item.HostSrc,
                                item.UploadIndexPath, item.UploadHostPath, _ftp))
                        {
                            // Record the index + host send in the NG log (index first, host last) so
                            // the NG report shows the manifests too. Not written to the main rawlog.
                            ngLog.WriteManifestSent(item.Pid, item.Day, item.UploadIndexPath,
                                                    item.UploadHostPath, item.TotalRetries, host);
                            Log($"   panel {item.Pid}: index + host manifest sent — panel complete");
                        }
                    }
                    catch (Exception ex) { Log($"   panel {item.Pid}: finalize error: {ex.Message}"); }
                }
            }
        }
        else if (result.Outcome == TransferOutcome.LocalMissing)
        {
            // The source file no longer exists on disk — retrying can never succeed, so stop
            // hammering it. Terminal: excluded from further sweeps (operator can still force Retry).
            item.State = NgItemState.Gone;
            item.LastResult = result.Message ?? "local file missing";
            Log($"   local file missing — {item.Pid}/{item.FileName} skipped (no retry)");

            // Only DATA files touch the manifest. A missing manifest source can't be dropped/finalized.
            if (!item.IsManifest)
            {
                // Drop its manifest line so the panel can still finalize (short by this file), then
                // try to finalize in case that was the last unresolved file.
                _manifest.DropLine(item.IndexSrc, item.HostSrc, item.RemotePath);
                if (item.IndexSrc.Length > 0)
                {
                    try
                    {
                        if (await _manifest.TryFinalizeAsync(item.IndexSrc, item.HostSrc,
                                item.UploadIndexPath, item.UploadHostPath, _ftp))
                        {
                            ngLog.WriteManifestSent(item.Pid, item.Day, item.UploadIndexPath,
                                                    item.UploadHostPath, item.TotalRetries, host);
                            Log($"   panel {item.Pid}: index + host manifest sent — panel complete");
                        }
                    }
                    catch (Exception ex) { Log($"   panel {item.Pid}: finalize error: {ex.Message}"); }
                }
            }
        }
        else
        {
            item.State = NgItemState.Failed;
            item.LastResult = result.Message ?? result.Outcome.ToString();
            ngLog.Write(item, false, host);
            Log($"   {result.Outcome}: {item.LastResult}");
        }
        NotifyChanged();
    }
}
