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
    // Own RawLog instance so recoveries can be written back into the day's main log. Appends go
    // through the shared file lock, so writing alongside the live engine's instance is safe.
    private readonly RawLog rawLog = new(cfg);

    /// <summary>
    /// Keys this engine has confirmed on the server, always in "PID|FileName" form — the same shape
    /// JobFile.Key uses, so the UI can match them against live job files.
    ///
    /// NgItem.Key is "Day|PID|FileName", so adding that directly put two formats in this set: the
    /// same file counted twice, and splitting a day-prefixed key on '|' produced the day string as a
    /// bogus panel. That read as "26 files / 7 panels" in the strip against the log's "19 / 6".
    ///
    /// The live engine keeps its own in-memory job state and never learns about NG recoveries, so
    /// without this the header strip counted a file NG had rescued as still Failed.
    /// </summary>
    private readonly HashSet<string> _recovered = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Normalise any key to "PID|FileName" — NgItem keys carry a leading day.</summary>
    private static string FileKey(string pid, string fileName) => pid + "|" + fileName;

    /// <summary>Snapshot of the recovered keys, for the UI. Cheap: in-memory, no file reads.</summary>
    public HashSet<string> RecoveredKeys
    {
        get { lock (_gate) return new HashSet<string>(_recovered, StringComparer.OrdinalIgnoreCase); }
    }
    private readonly object _gate = new();
    private List<NgItem> _items = new();     // the loaded day's NG items
    private readonly List<NgItem> _queue = new();
    private NgItem? _inFlight;
    private NgItem? _current;                 // actively worked item (persists across cooldown)

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

    /// <summary>
    /// The days currently loaded, newest first. Normally today plus <see cref="Config.NgRecoveryDays"/>
    /// past days; a single entry when the operator has picked one day from the calendar.
    /// </summary>
    private List<string> _days = new();

    /// <summary>True while showing the automatic window (today + past days) rather than one picked day.</summary>
    public bool WindowMode { get; private set; } = true;

    /// <summary>The days this console is working, for the UI strip.</summary>
    public IReadOnlyList<string> LoadedDays { get { lock (_gate) return _days.ToList(); } }

    private readonly NgBacklog _backlog = new(cfg);

    /// <summary>Files still unrecovered on days OUTSIDE the window — the "· n older" figure.
    /// Without it "NG List (0)" reads as "nothing outstanding" when it only means "nothing
    /// outstanding in the loaded window".</summary>
    public int BacklogOutstanding => _backlog.Outstanding;
    public int BacklogDays => _backlog.Days;

    /// <summary>
    /// Recount the out-of-window backlog. Call from a background task only: on a cold cache this
    /// reads every retained day's logs, which must never happen on the UI thread or a pump.
    /// </summary>
    public void RefreshBacklog()
    {
        try
        {
            _backlog.Refresh(LoadedDays);
            NotifyChanged();
        }
        catch { /* an advisory counter must never take anything down */ }
    }

    /// <summary>
    /// Load the automatic recovery window: today plus the configured number of past days. This is
    /// what makes midnight-orphaned files recover on their own — they are filed under the OLD day,
    /// so a today-only list never touches them. Days with no rawlog are skipped.
    /// </summary>
    public void LoadWindow()
    {
        var days = new List<string>();
        var today = Clock.Today;
        for (var back = 0; back <= Math.Max(0, cfg.NgRecoveryDays); back++)
        {
            var d = today.AddDays(-back).ToString("yyyyMMdd");
            if (back == 0 || File.Exists(cfg.RawLogPathForDay(d))) days.Add(d);
        }

        var items = new List<NgItem>();
        foreach (var d in days) items.AddRange(BuildItems(d));

        lock (_gate)
        {
            AutoRunning = false;
            _queue.Clear();
            if (_current is not null) { _current.IsCurrent = false; _current = null; }
            _items = Order(items);
            _days = days;
            WindowMode = true;
            LoadedDay = days[0];
            _loadGen++;
        }
        var past = days.Count - 1;
        Log($"NG list loaded for {days[0]}" + (past > 0 ? $" + {past} past day(s) [{string.Join(", ", days.Skip(1))}]" : "") +
            $": {items.Count} item(s)");
        NotifyChanged();
    }

    /// <summary>
    /// Round-robin across days at PANEL granularity: one whole panel from each day in turn, newest
    /// day leading each round.
    ///
    /// Two failure modes this balances between. All-of-today-then-yesterday starves the oldest days:
    /// with ~5000 items queued a sweep never reaches its tail before the window slides on, and four
    /// separate days recovered zero files. But interleaving file-by-file starves throughput instead —
    /// Attempt() finalizes a panel's manifests as soon as its data files land, so consecutive items
    /// from different panels trigger a fresh manifest attempt almost every time; measured at 287
    /// retries versus 533 for the same run. Rotating whole panels keeps every day progressing while
    /// preserving the per-panel locality that makes finalize cheap.
    ///
    /// With a single day loaded this degrades to plain Pid-then-FileName order, as before.
    /// </summary>
    private static List<NgItem> Order(IEnumerable<NgItem> items)
    {
        var byDay = items
            .GroupBy(i => i.Day)
            .OrderByDescending(g => g.Key)
            .Select(g => g.GroupBy(i => i.Pid)
                          .OrderBy(p => p.Key)
                          .Select(p => p.OrderBy(i => i.FileName).ToList())
                          .ToList())
            .ToList();

        if (byDay.Count == 0) return new List<NgItem>();

        var ordered = new List<NgItem>();
        var deepest = byDay.Max(d => d.Count);
        for (var i = 0; i < deepest; i++)
            foreach (var day in byDay)
                if (i < day.Count) ordered.AddRange(day[i]);
        return ordered;
    }

    public void LoadDay(string day)
    {
        var items = BuildItems(day);
        lock (_gate)
        {
            AutoRunning = false;
            _queue.Clear();
            if (_current is not null) { _current.IsCurrent = false; _current = null; }
            _items = items.OrderBy(i => i.Pid).ThenBy(i => i.FileName).ToList();
            _days = new List<string> { day };
            WindowMode = false;                 // operator picked one day — don't yank them back
            LoadedDay = day;
            _loadGen++;
        }
        Log($"NG list loaded for {day}: {items.Count} item(s)");
        NotifyChanged();
    }

    private DateTime _lastMerge = DateTime.MinValue;
    private DateTime _lastPastMerge = DateTime.MinValue;

    /// <summary>
    /// Make the next RefreshLoadedDayLive rebuild the past days immediately instead of waiting out
    /// the 30s throttle. Called right after a rollover settles: that is the moment the OLD day's
    /// rawlog gains all the abandoned files, and they should enter the NG list at once rather than
    /// sitting idle for half a minute.
    /// </summary>
    public void ForcePastMerge() => _lastPastMerge = DateTime.MinValue;

    /// <summary>
    /// Merge in files that have since failed or timed out, without disturbing items already
    /// loaded/in-flight or stopping an auto-retry.
    ///
    /// TODAY is merged on a 2s throttle (it changes constantly as the live pump works). PAST days
    /// are merged on a 30s throttle: they only change when this pump itself recovers something, and
    /// BuildItems reads three files per day — doing that every 2s per day is exactly the kind of
    /// per-tick file work that starved the watch loop before.
    /// </summary>
    public void RefreshLoadedDayLive()
    {
        var today = Clock.Now.ToString("yyyyMMdd");
        List<string> days;
        lock (_gate) days = _days.ToList();
        if (days.Count == 0) return;

        var doToday = days.Contains(today) && (DateTime.Now - _lastMerge).TotalSeconds >= 2;
        var doPast  = days.Count > 1 && (DateTime.Now - _lastPastMerge).TotalSeconds >= 30;
        if (!doToday && !doPast) return;
        if (doToday) _lastMerge = DateTime.Now;
        if (doPast) _lastPastMerge = DateTime.Now;

        var rebuild = new List<string>();
        if (doToday) rebuild.Add(today);
        if (doPast) rebuild.AddRange(days.Where(d => d != today));

        var fresh = new List<NgItem>();
        foreach (var d in rebuild) fresh.AddRange(BuildItems(d));
        var freshKeys = new HashSet<string>(fresh.Select(i => i.Key));
        var scope = new HashSet<string>(rebuild);
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
            // Scoped to the days we actually rebuilt — never judge a day we didn't look at.
            removed = _items.RemoveAll(i => i.DisplayOnly && scope.Contains(i.Day) && !freshKeys.Contains(i.Key));

            if (added.Count > 0 || removed > 0) _items = Order(_items);
        }
        if (added.Count > 0 || removed > 0)
        {
            if (added.Count > 0) Log($"NG list: +{added.Count} new item(s)");
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
        // Files whose LOCAL source was deleted. These must NOT become NG items: NG retries without
        // limit, and no number of retries can upload a file that no longer exists. Left in, one
        // showed as "1 file, 100% failing" in the strip while the NG list showed nothing to work on
        // — a permanent phantom failure that an operator can neither fix nor clear.
        var gone = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in SafeFile.ReadLines(cfg.RawLogPathForDay(day)))
        {
            var p = line.Split('|');
            if (p.Length < 3) continue;
            var key = p[0] + "|" + p[1];
            status[key] = p[2];
            if (p.Length >= 11 && p[10].Trim() == "SOURCE_GONE") gone.Add(key);
            else gone.Remove(key);       // a later row supersedes: the file came back
        }

        var prior = ngLog.ReadState(day);

        // Seed the recovered set for the UI: anything the ng-retry state already marks as fixed.
        // State keys are "PID|FileName" like the rawlog, so they need no normalising — but strip a
        // leading day if one ever appears, so this set only ever holds one key shape.
        lock (_gate)
            foreach (var kv2 in prior)
                if (kv2.Value.Item2)
                {
                    var k = kv2.Key;
                    var parts = k.Split('|');
                    _recovered.Add(parts.Length >= 3 ? FileKey(parts[1], parts[^1]) : k);
                }

        var items = new List<NgItem>();
        foreach (var kv in status)
        {
            if (kv.Value != "FAILED" && kv.Value != "TIMEDOUT") continue;
            if (gone.Contains(kv.Key)) continue;           // source deleted — no retry can help
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
        Log($"NG auto-retry started: {n} item(s), IP={cfg.FirstHost}");
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

    /// <summary>
    /// Write an NG recovery back into that day's MAIN rawlog as SUCCEEDED.
    ///
    /// Recoveries used to be recorded only in the ng-retry log, so the rawlog's last word on a
    /// recovered file stayed FAILED forever. Anything reading the rawlog alone — the main HTML
    /// report, LGD's own checks — therefore saw failures for files that were on the server: on
    /// 2026-09-02, 97 of 124 host manifests WinSCP proves were uploaded still read FAILED. The
    /// rawlog is what people treat as the record of the day, so it has to be self-contained.
    ///
    /// The row goes to the file's OWN day (item.Day), not today, so a past-day recovery lands in
    /// the right day's log. Append-only: the original FAILED rows stay as history, and a
    /// last-row-wins read now gives the correct final status.
    /// </summary>
    private void WriteRecoveryToRawLog(NgItem item, string host)
    {
        try
        {
            if (!DateTime.TryParseExact(item.Day, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out var day)) return;

            var jf = item.ToJobFile();
            jf.Status = FileStatus.Succeeded;
            jf.Attempts = item.TotalRetries + 1;
            // Carry the succeed time and fail history across, or the write-back row records a
            // success with no timestamp and reports lose "when did this land".
            jf.SucceedTime = DateTime.Now.ToString("HH:mm:ss");
            jf.FailCount = item.TotalRetries;
            rawLog.Write(jf, cfg.MaxAttempts, host, null, day);
        }
        catch { /* the ng-retry log already has the authoritative record; never break recovery */ }
    }

    /// <summary>
    /// Write the per-file marker for a manifest that NG uploaded as its own NG item.
    ///
    /// TryFinalizeAsync owns these markers normally, but a manifest abandoned at a rollover becomes
    /// an ordinary NG item and is uploaded directly, bypassing it. Leaving the marker unwritten made
    /// a later finalize re-send a file already on the server and log it as FAILED.
    /// </summary>
    private void MarkManifestSentOnDisk(NgItem item)
    {
        try
        {
            if (!item.IsManifest || item.IndexSrc.Length == 0) return;
            var isIndex = !string.IsNullOrEmpty(item.UploadIndexPath)
                          && item.RemotePath.Equals(item.UploadIndexPath, StringComparison.OrdinalIgnoreCase);
            var marker = item.IndexSrc + (isIndex ? ".idxsent" : ".hostsent");
            SafeFile.WithLock(() => { try { File.WriteAllText(marker, DateTime.Now.ToString("o")); } catch (Exception mex) { Log($"MARKER WRITE FAILED {Path.GetFileName(marker)}: {mex.GetType().Name}: {mex.Message}"); } });
        }
        catch { }
    }

    /// <summary>
    /// Write the two manifests of an NG-finalized panel into that day's MAIN rawlog as SUCCEEDED,
    /// so the rawlog is self-contained: without it a completed panel's manifests appear only in the
    /// ng-retry log and read as "data uploaded, no host manifest" to anything reading the rawlog.
    /// </summary>
    private void WriteManifestToRawLog(NgItem item, string host)
        => WriteManifestToRawLog(item.Pid, item.Day, item.UploadIndexPath, item.UploadHostPath,
                                 item.TotalRetries + 1, host);

    private void WriteManifestToRawLog(string pid, string dayStr, string upIdx, string upHost, int attempts, string host)
    {
        try
        {
            if (!DateTime.TryParseExact(dayStr, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out var day)) return;

            // Index first, host last: the fixture scans bottom-up for a PID's last line, so the host
            // manifest must be the final entry for a completed panel.
            foreach (var remote in new[] { upIdx, upHost })
            {
                if (string.IsNullOrWhiteSpace(remote)) continue;
                var name = System.IO.Path.GetFileName(remote);
                var jf = new JobFile
                {
                    Pid = pid, FileName = name, RemotePath = remote,
                    Status = FileStatus.Succeeded,
                    Attempts = attempts,
                    SucceedTime = DateTime.Now.ToString("HH:mm:ss")
                };
                rawLog.Write(jf, cfg.MaxAttempts, host, null, day);
                lock (_gate) _recovered.Add(FileKey(pid, name));   // so the UI strip agrees with the log
            }
        }
        catch { /* the ng-retry log already holds the authoritative record */ }
    }

    /// <summary>
    /// Which IP this retry uses: the ONE host chosen in Settings. NG no longer has its own IP
    /// selector — uploads and recovery must agree on the destination.
    /// </summary>
    private string HostFor(NgItem item) => cfg.FirstHost;

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

                    // Finish any panels whose data completed during this cycle, BEFORE the cooldown.
                    // Running it here rather than only in the idle branch below is what makes it
                    // prompt: while NG had work the loop always `continue`d, so under sustained
                    // failure the finalize sweep never ran and manifests sat unsent for minutes.
                    // It is called on this same thread, between item attempts, so it never shares
                    // the reused FTP session with a transfer in flight.
                    await SweepUnfinalizedPanels(stopping);

                    if (AutoRunning && anyFailed && gen == _loadGen)
                    {
                        ClearCurrent();   // no highlight during the between-sweep pause
                        try { await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, cfg.NgRetryCooldownSeconds)), stopping); }
                        catch (OperationCanceledException) { break; }
                    }
                    continue;
                }
            }

            // 3) Nothing to retry — look for panels whose DATA is complete but whose manifests were
            // never sent, and finish them. See SweepUnfinalizedPanels for why these exist.
            ClearCurrent();
            if (AutoRunning) await SweepUnfinalizedPanels(stopping);
            await _ftp.EndSession();   // close the reused connection while idle
            await Task.Delay(300, stopping);
        }
    }

    private DateTime _lastFinalizeSweep = DateTime.MinValue;

    /// <summary>How often the unfinalized-panel sweep may run. Short, because it is now the primary
    /// path for finishing panels whose data recovered late — at 60s manifests sat unsent for minutes.
    /// Each run re-reads the loaded days' logs, so it is still throttled rather than run every tick.</summary>
    private const int FinalizeSweepSeconds = 10;

    /// <summary>
    /// Finish panels whose data files are all uploaded but whose index/host manifests were never
    /// sent — the one hole left in the recovery chain.
    ///
    /// How a panel gets stuck: a data file exhausts its live attempts and goes FAILED, so the live
    /// engine can never mark the panel ready and stops finalizing it. NG then recovers that file and
    /// calls finalize once — but if THAT send fails, the NG item is already marked Succeeded, so it
    /// is never revisited. The manifests were never attempted, so they have no rawlog row and never
    /// enter the NG list either. Nothing owns them. Measured at 8 panels in 100 under load, and it
    /// matches the field report of panels arriving with images+hex but no host manifest.
    ///
    /// This is a safety net, so it is deliberately conservative: it only sends when the index
    /// manifest on disk has no unresolved " -pending" line, i.e. every data file really did land.
    /// </summary>
    private async Task SweepUnfinalizedPanels(CancellationToken ct)
    {
        if ((DateTime.Now - _lastFinalizeSweep).TotalSeconds < FinalizeSweepSeconds) return;
        _lastFinalizeSweep = DateTime.Now;

        foreach (var day in LoadedDays)
        {
            if (ct.IsCancellationRequested) return;

            // Which manifest files are already recorded as sent, from either log.
            var sent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in new[] { cfg.RawLogPathForDay(day), cfg.NgRetryLogPath(day) })
                foreach (var line in SafeFile.ReadLines(path))
                {
                    var p = line.Split('|');
                    if (p.Length >= 3 && p[2] == "SUCCEEDED") sent.Add(p[0] + "|" + p[1]);
                }

            // Panel manifest paths come from that day's jobs file.
            var panels = new Dictionary<string, JobsLine>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in SafeFile.ReadLines(cfg.JobsPathForDay(day)))
            {
                var jl = JobsLine.Parse(line);
                if (jl is null || jl.IndexSrc.Length == 0) continue;
                panels.TryAdd(jl.Pid, jl);
            }

            foreach (var kv in panels)
            {
                if (ct.IsCancellationRequested) return;
                var jl = kv.Value;
                var idxName = Path.GetFileName(jl.UploadIndexPath);
                var hostName = Path.GetFileName(jl.UploadHostPath);
                if (idxName.Length == 0 || hostName.Length == 0) continue;
                if (sent.Contains(jl.Pid + "|" + idxName) && sent.Contains(jl.Pid + "|" + hostName)) continue;

                // Only finalize a panel whose data is genuinely complete.
                if (!File.Exists(jl.IndexSrc)) continue;

                // Skip panels already finished: both per-file markers present means the index and the
                // host manifest are on the server. TryFinalizeAsync checks this too, but testing it
                // here avoids claiming a lock and re-reading the manifest for nothing.
                if (File.Exists(jl.IndexSrc + ".idxsent") && File.Exists(jl.IndexSrc + ".hostsent")) continue;

                var stillPending = false;
                foreach (var l in SafeFile.ReadLines(jl.IndexSrc))
                    if (l.EndsWith(" -pending", StringComparison.OrdinalIgnoreCase)) { stillPending = true; break; }
                if (stillPending) continue;

                try
                {
                    var host = cfg.FirstHost;
                    var fin = await _manifest.TryFinalizeAsync(jl.IndexSrc, jl.HostSrc,
                                    jl.UploadIndexPath, jl.UploadHostPath, _ftp, host);
                    if (fin.Ok && fin.Uploaded)
                    {
                        ngLog.WriteManifestSent(jl.Pid, day, jl.UploadIndexPath, jl.UploadHostPath, 0, fin.Host);
                        WriteManifestToRawLog(jl.Pid, day, jl.UploadIndexPath, jl.UploadHostPath, 1, fin.Host);
                        Log($"   panel {jl.Pid} ({day}): manifests finished by sweep - panel complete");
                        NotifyChanged();
                    }
                }
                catch (Exception ex) { Log($"   panel {jl.Pid}: finalize sweep error: {ex.Message}"); }
            }
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
            WriteRecoveryToRawLog(item, host);
            lock (_gate) _recovered.Add(FileKey(item.Pid, item.FileName));
            // A manifest recovered as its OWN NG item never went through TryFinalizeAsync, so it had
            // no per-file marker written. Without one, the next finalize believes it is still missing
            // and re-sends it — which then records a FAILED row for a file already on the server.
            MarkManifestSentOnDisk(item);
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
                        // forceHost: the NG list is independent of the routing settings, so the
                        // manifests go to the IP this recovery used, not via HostForAttempt.
                        var fin = await _manifest.TryFinalizeAsync(item.IndexSrc, item.HostSrc,
                                item.UploadIndexPath, item.UploadHostPath, _ftp, host);
                        if (fin.Ok && fin.Uploaded)
                        {
                            // Record the index + host send in the NG log (index first, host last) so
                            // the NG report shows the manifests too.
                            ngLog.WriteManifestSent(item.Pid, item.Day, item.UploadIndexPath,
                                                    item.UploadHostPath, item.TotalRetries, fin.Host);
                            // ...and in the day's MAIN rawlog, so it is self-contained. Without this
                            // the rawlog shows a completed panel's manifests as never sent, which is
                            // exactly the false "no host manifest" signature we chased in the field.
                            WriteManifestToRawLog(item, fin.Host);
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
                        var fin2 = await _manifest.TryFinalizeAsync(item.IndexSrc, item.HostSrc,
                                item.UploadIndexPath, item.UploadHostPath, _ftp, host);
                        if (fin2.Ok && fin2.Uploaded)
                        {
                            ngLog.WriteManifestSent(item.Pid, item.Day, item.UploadIndexPath,
                                                    item.UploadHostPath, item.TotalRetries, fin2.Host);
                            WriteManifestToRawLog(item, fin2.Host);
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
