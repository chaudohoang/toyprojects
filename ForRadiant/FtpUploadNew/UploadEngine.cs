using System.Collections.Concurrent;

namespace FtpUpload;

/// <summary>
/// The heart of the program (spec §2 and §3).
///
/// Exactly ONE file is in flight at any moment — so the per-file timeout and the
/// preemption rule always apply to a single, unambiguous transfer.
///
/// There is now a SINGLE list of work: the in-memory job list (_jobs), rebuilt from the
/// jobs file plus today's raw log on startup. The old separate ng_waitlist.txt is gone.
///
/// A file that runs out of attempts is marked FAILED. It is NEVER retried automatically —
/// it is held on the UI's NG tab and only goes again when the operator clicks Retry
/// (RetryFailed / RetryAllFailed). Idle catch-up only re-runs files that are still PENDING
/// (e.g. panels left behind by a preempt), never FAILED ones.
///
/// Work ordering:
///   1. files force-queued from the UI
///   2. files of the current (newest) panel — a PREEMPT keeps only the new panel queued
///      and leaves the remainder of the previous panel PENDING in the job list
///   3. when nothing is active, the job list is swept for any still-pending file
///      (idle catch-up)
/// </summary>
public sealed class UploadEngine(Config cfg, RawLog rawLog, SnapshotLog snapshot)
{
    private readonly ConcurrentDictionary<string, Job> _jobs = new();      // pid -> job
    private readonly List<JobFile> _queue = new();                          // run order
    private readonly object _gate = new();
    private readonly IFtpTransfer _ftp = FtpEngineFactory.Create(cfg, reuseConnections: true);   // live pump reuses one connection

    /// <summary>Current session number and files-on-this-session, surfaced for the live strip.</summary>
    public int SessionNumber => _ftp.SessionNumber;
    public int FilesThisSession => _ftp.FilesThisSession;

    // Manifest (index/host) writer + a SEPARATE transfer used only to send finalized manifests,
    // so completion uploads never collide with the single-in-flight live transfer. It REUSES one
    // session (finalize runs serially on the watch loop), so panel completions share a single
    // connection instead of opening a fresh one per panel — otherwise every finalize would spawn a
    // new WinSCP.exe + session (and a new engine log file).
    private readonly ManifestWriter _manifest = new(cfg);
    private readonly IFtpTransfer _manifestFtp = FtpEngineFactory.Create(cfg, reuseConnections: true);

    // Rolling upload-speed average over the last few files (MB/s), shown in the live strip.
    private readonly object _speedGate = new();
    private readonly Queue<double> _recentSpeeds = new();
    private const int SpeedWindow = 20;

    /// <summary>Average throughput (MB/s) over the last few successful files, 0 if none yet.</summary>
    public double RollingMBps { get { lock (_speedGate) return _recentSpeeds.Count == 0 ? 0 : _recentSpeeds.Average(); } }

    private void RecordSpeed(long bytes, double seconds)
    {
        if (bytes <= 0 || seconds <= 0) return;
        var mbps = bytes / (1024.0 * 1024.0) / seconds;
        lock (_speedGate)
        {
            _recentSpeeds.Enqueue(mbps);
            while (_recentSpeeds.Count > SpeedWindow) _recentSpeeds.Dequeue();
        }
    }

    private CancellationTokenSource? _currentTransfer;
    private JobFile? _inFlight;
    private DateTime _inFlightStarted;

    public JobFile? InFlight { get { lock (_gate) return _inFlight; } }
    public DateTime InFlightStarted { get { lock (_gate) return _inFlightStarted; } }
    public int QueueLength { get { lock (_gate) return _queue.Count; } }
    public IReadOnlyCollection<Job> Jobs => _jobs.Values.OrderBy(j => j.Pid).ToList();

    /// <summary>Raised whenever anything the UI displays has changed.</summary>
    public event Action? Changed;
    private void NotifyChanged() => Changed?.Invoke();

    /// <summary>Log line sink — the UI shows these instead of a console window.</summary>
    public event Action<string>? Logged;
    private void Log(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
        Logged?.Invoke(line);
    }

    /// <summary>
    /// Durable counterpart to <see cref="Log"/> for events that must survive a restart. The UI log
    /// is in-memory and capped, so on a headless machine (watchdog-restarted, window never opened)
    /// it is gone by the time anyone asks what happened. Day-rollover events go here so "did the
    /// day turn over cleanly?" is answerable from disk, days later.
    /// </summary>
    private void OpLog(string msg)
    {
        try { SafeFile.Append(cfg.OpLogPath(Clock.Now), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}"); }
        catch { /* housekeeping must never break the pump */ }
    }

    // ---------------- intake ----------------

    /// <summary>
    /// State recovered from today's raw log at startup, so a restart does not re-upload work
    /// that is already done. Key is "PID|FileName".
    /// </summary>
    private Dictionary<string, JobFile> _history = new();

    public void LoadHistory(Dictionary<string, JobFile> history)
    {
        _history = history;
        var done = history.Values.Count(f => f.Status == FileStatus.Succeeded);
        if (history.Count > 0)
            Log($"recovered {history.Count} file record(s) from today's raw log — {done} already uploaded");
    }

    /// <summary>Adds files from a job file line. Ignores duplicates on restart.</summary>
    public void AddFiles(IEnumerable<JobFile> files)
    {
        var skipped = 0;

        lock (_gate)
        {
            foreach (var f in files)
            {
                var job = _jobs.GetOrAdd(f.Pid, _ => new Job { Pid = f.Pid, Day = Clock.Today });
                // NB: the panel-timeout clock (job.Started) is NOT set here. It starts when the
                // panel's first file actually begins uploading (see UploadOne), so a panel still
                // waiting its turn in the queue is not on the clock yet.
                if (job.Files.Any(x => x.FileName == f.FileName)) continue;

                // Already resolved earlier today (before a restart): keep it visible in the UI
                // with its original outcome, but do NOT queue it again. This carries a final
                // FAILED or TIMEDOUT result forward, so such files stay in the NG list (and out
                // of the upload queue) after a restart.
                if (_history.TryGetValue(f.Key, out var prev) &&
                    prev.Status != FileStatus.Pending)
                {
                    f.Status = prev.Status;
                    f.SucceedTime = prev.SucceedTime;
                    f.FailCount = prev.FailCount;
                    f.FailTimes.AddRange(prev.FailTimes);
                    f.Attempts = prev.Attempts;
                    job.Files.Add(f);
                    if (prev.Status == FileStatus.TimedOut) job.TimedOut = true;   // keep panel status
                    skipped++;
                    continue;
                }

                job.Files.Add(f);
                // Manifest files are counted but never enter the normal pump — the finalize step
                // sends them once all data files resolve, then marks them Succeeded/Failed.
                if (!f.IsManifest) _queue.Add(f);
            }
        }

        if (skipped > 0) Log($"skipped {skipped} file(s) already resolved earlier today");
        NotifyChanged();
    }

    /// <summary>
    /// Attaches .panel handoff metadata to a panel's Job (creating the Job if needed), BEFORE its
    /// files are added via AddFiles. Idempotent: re-registering an existing panel refreshes the
    /// metadata but never clears its files. The manifest/completion logic reads these fields.
    /// </summary>
    public void RegisterPanel(string pid, DateTime day, string sourceFolder, string channelIndex,
                              string uploadIndexPath, string uploadHostPath,
                              string indexSrc, string hostSrc, int totalFileCount)
    {
        lock (_gate)
        {
            var job = _jobs.GetOrAdd(pid, _ => new Job { Pid = pid, Day = day });
            job.SourceFolder = sourceFolder;
            job.ChannelIndex = channelIndex;
            job.UploadIndexPath = uploadIndexPath;
            job.UploadHostPath = uploadHostPath;
            job.IndexSrc = indexSrc;
            job.HostSrc = hostSrc;
            if (totalFileCount > 0) job.TotalFileCount = totalFileCount;   // don't clobber on restore
        }
        NotifyChanged();
    }

    /// <summary>
    /// Seed (create-or-resume) a panel's index/host manifests from its data files. Called by
    /// PanelIntake AFTER the files have been added, so job.Files is populated.
    /// </summary>
    public void SeedPanelManifest(string pid)
    {
        if (_jobs.TryGetValue(pid, out var job) && job.IsPanelJob)
            _manifest.Seed(job);
    }

    /// <summary>
    /// For every panel that is fully resolved — every data file uploaded (clean) or dropped (source
    /// gone), so no " -pending" remains — upload its index + host manifests. Idempotent; a failed
    /// send is retried on the next sweep.
    ///
    /// Runs on its OWN task (see AppHost), never on the watch loop. Every call here can make real FTP
    /// requests, and against a slow or unreachable host a single manifest send takes tens of seconds —
    /// on the watch loop that starved the day rollover, intake and command polling badly enough to
    /// stall uploading completely under load. It gets its own thread of control instead.
    /// </summary>
    public async Task FinalizeReadyPanels()
    {
        // A pending rollover means the live pump is GATED — settling it is the priority, and the
        // rollover is about to clear the job list anyway. Re-checked inside the loop below so a
        // rollover raised mid-sweep stops us promptly rather than after every remaining panel.
        if (_rolloverPending) return;

        foreach (var job in _jobs.Values)
        {
            if (_rolloverPending) return;

            if (job.Finalized || !job.IsPanelJob) continue;
            var manifests = job.Files.Where(f => f.IsManifest).ToList();
            if (manifests.Count == 0) continue;   // legacy panel with no manifest files

            try
            {
                // Already resolved to a terminal state (e.g. carried forward from the rawlog after a
                // restart, or already handed to NG): the live engine is done with it.
                if (manifests.All(m => m.Status == FileStatus.Succeeded)) { job.Finalized = true; continue; }
                if (manifests.Any(m => m.Status is FileStatus.Failed or FileStatus.TimedOut))
                    { job.Finalized = true; continue; }

                var dataFiles = job.Files.Where(f => !f.IsManifest).ToList();
                if (dataFiles.Count == 0) continue;

                // Wait until every data file has finished on the live pump — nothing still Pending
                // (queued or in-flight). The manifests stay Pending in the meantime; they are only
                // ever resolved once the panel's data files are all done, one way or the other. (This
                // is what stops a manifest flipping to Failed while other files are still uploading.)
                if (dataFiles.Any(f => f.Status == FileStatus.Pending)) continue;

                // All data files are resolved now. If any FAILED/TIMED-OUT with its source still
                // present, it went to the NG pump — the panel is being recovered there (NG log only),
                // so write NOTHING to the main rawlog; just mark the manifests terminal in-memory so
                // the panel isn't stuck In Progress. (A file whose SOURCE is gone was legitimately
                // dropped from the panel and does NOT block a live completion.)
                if (dataFiles.Any(f => (f.Status is FileStatus.Failed or FileStatus.TimedOut)
                                       && File.Exists(f.LocalPath)))
                {
                    foreach (var mf in manifests.Where(m => m.Status == FileStatus.Pending))
                        mf.Status = FileStatus.Failed;   // in-memory only — never written to the rawlog
                    job.Finalized = true;
                    NotifyChanged();
                    continue;
                }

                // Nothing actually uploaded (all dropped) — nothing to finalize.
                if (!dataFiles.Any(f => f.Status == FileStatus.Succeeded)) { job.Finalized = true; continue; }

                // Clean live completion (every data file Succeeded live, plus any source-gone drops):
                // send the manifests and record them in the main rawlog (host last).
                if (await _manifest.TryFinalizeAsync(job, _manifestFtp))
                {
                    MarkManifestsSucceeded(job, manifests);
                }
                else
                {
                    // Data done live, but the manifest SEND itself failed. Count it; after MaxAttempts,
                    // hand the manifests to NG as real FAILED files (a genuine manifest failure — this
                    // IS recorded in the main rawlog, because the panel completed live).
                    job.FinalizeAttempts++;
                    Log($"panel {job.Pid}: manifest send failed (attempt {job.FinalizeAttempts}/{cfg.MaxAttempts})");
                    if (job.FinalizeAttempts >= cfg.MaxAttempts)
                    {
                        foreach (var mf in manifests)
                        {
                            mf.Attempts = job.FinalizeAttempts;
                            mf.FailCount++;
                            mf.FailTimes.Add(DateTime.Now.ToString("HH:mm:ss"));
                            mf.Status = FileStatus.Failed;
                        }
                        foreach (var mf in manifests.OrderBy(f => f.RemotePath == job.UploadHostPath ? 1 : 0))
                            rawLog.Write(mf, cfg.MaxAttempts, cfg.PrimaryHost, job, job.Day);
                        job.Finalized = true;   // live engine is done; the NG pump owns it now
                        Log($"panel {job.Pid}: manifest NG after {cfg.MaxAttempts} attempts — moved to NG list");
                    }
                    NotifyChanged();
                }
            }
            catch (Exception ex)
            {
                Log($"panel {job.Pid}: finalize error: {ex.Message}");
            }
        }
    }

    /// <summary>Mark a panel's two manifest files Succeeded and log them like any file, so they count,
    /// show in the day report, and (only now) make the panel AllSucceeded -> SUCCESS/O.</summary>
    private void MarkManifestsSucceeded(Job job, List<JobFile> manifests)
    {
        var pending = manifests.Where(f => f.Status != FileStatus.Succeeded).ToList();
        foreach (var mf in pending)
        {
            mf.Attempts = Math.Max(1, mf.Attempts);
            mf.Status = FileStatus.Succeeded;
            mf.SucceedTime = DateTime.Now.ToString("HH:mm:ss");
        }
        // Write the INDEX line first and the HOST line LAST, so a completed panel's final rawlog
        // line is the host manifest with SUCCESS — that is the line the fixture's previous-panel
        // O/X check reads (it scans bottom-up for the PID's last line).
        foreach (var mf in pending.OrderBy(f => f.RemotePath == job.UploadHostPath ? 1 : 0))
            rawLog.Write(mf, cfg.MaxAttempts, cfg.PrimaryHost, job, job.Day);
        job.Finalized = true;
        if (pending.Count > 0)
        {
            Log($"panel {job.Pid}: index + host manifest sent — panel complete");
            NotifyChanged();
        }
    }

    /// <summary>
    /// Spec §3 — Result timing for a NEW panel. Writes the snapshot for the panel whose
    /// result is being sent, then immediately preempts: whatever is uploading is abandoned
    /// and only the new panel stays queued. The rest of the previous panel is left PENDING
    /// in the job list, so idle catch-up picks it up again once the new panel is done.
    /// </summary>
    public void OnResultTiming(string newPid)
    {
        if (_jobs.TryGetValue(newPid, out var job))
            snapshot.Write(job);

        lock (_gate)
        {
            // Keep only the new panel queued. Everything else simply remains PENDING in the
            // job list — there is no wait-list file to write any more.
            var newPanel = _queue.Where(f => f.Pid == newPid).ToList();
            _queue.Clear();
            _queue.AddRange(newPanel);

            if (_inFlight is not null && _inFlight.Pid != newPid)
                _currentTransfer?.Cancel();   // abandon the in-flight transfer of the old panel
        }
        NotifyChanged();
    }

    /// <summary>UI "Force Upload" — jump this file to the front of the line.</summary>
    public void ForceUpload(string pid, string fileName)
    {
        lock (_gate)
        {
            var f = _jobs.TryGetValue(pid, out var job)
                ? job.Files.FirstOrDefault(x => x.FileName == fileName)
                : null;
            if (f is null || f.Status == FileStatus.Succeeded) return;
            if (f.Attempts >= cfg.MaxAttempts) return;

            _queue.RemoveAll(x => ReferenceEquals(x, f));
            _queue.Insert(0, f);
        }
        Log($"force upload queued: {pid}/{fileName}");
        NotifyChanged();
    }

    /// <summary>
    /// NG-list manual retry (manual only). A file that ran out of attempts (FAILED) or was
    /// skipped by a panel timeout (TIMEDOUT) waits on the NG tab until the operator asks for it.
    /// This resets its attempt budget, re-queues it, and clears its panel's timeout so the retry
    /// gets a fresh window instead of being killed again immediately. History is kept.
    /// </summary>
    public void RetryFailed(string pid, string fileName)
    {
        lock (_gate)
        {
            if (!_jobs.TryGetValue(pid, out var job)) return;
            var f = job.Files.FirstOrDefault(x => x.FileName == fileName);
            if (f is null || (f.Status != FileStatus.Failed && f.Status != FileStatus.TimedOut)) return;

            RearmPanel(job);
            f.Attempts = 0;
            f.Status = FileStatus.Pending;
            if (!_queue.Contains(f)) _queue.Insert(0, f);
        }
        Log($"manual retry: {pid}/{fileName}");
        NotifyChanged();
    }

    /// <summary>NG-list "Retry All" — re-queues every FAILED or TIMEDOUT file. Returns how many.</summary>
    public int RetryAllFailed()
    {
        var n = 0;
        lock (_gate)
        {
            foreach (var job in _jobs.Values)
            {
                var any = false;
                foreach (var f in job.Files)
                {
                    if (f.Status != FileStatus.Failed && f.Status != FileStatus.TimedOut) continue;
                    f.Attempts = 0;
                    f.Status = FileStatus.Pending;
                    if (!_queue.Contains(f)) _queue.Add(f);
                    any = true;
                    n++;
                }
                if (any) RearmPanel(job);
            }
        }
        if (n > 0) Log($"manual retry all: {n} file(s) re-queued");
        NotifyChanged();
        return n;
    }

    /// <summary>Clear a panel's timeout so a manual retry gets a fresh window. The clock
    /// re-starts (lazily) when the retried file next begins uploading.</summary>
    private static void RearmPanel(Job job)
    {
        job.TimedOut = false;
        job.Started = default;
    }

    public void DeletePanel(string pid)
    {
        lock (_gate)
        {
            _queue.RemoveAll(f => f.Pid == pid);
            _jobs.TryRemove(pid, out _);
        }
        Log($"deleted job {pid}");
        NotifyChanged();
    }

    /// <summary>Remove a single file from a job (UI "Delete" on one row).</summary>
    public void DeleteFile(string pid, string fileName)
    {
        lock (_gate)
        {
            _queue.RemoveAll(f => f.Pid == pid && f.FileName == fileName);
            if (_jobs.TryGetValue(pid, out var job))
                job.Files.RemoveAll(f => f.FileName == fileName);
        }
        Log($"deleted {pid}/{fileName}");
        NotifyChanged();
    }

    // ---------------- the pump ----------------

    /// <summary>
    /// Pause switch for the live pump. When paused, the engine keeps accepting and queuing jobs
    /// from TrueTest — it just stops pulling new files to upload (the in-flight one finishes).
    /// Default running; the operator toggles it from the UI. Does not affect the NG-retry pump.
    /// </summary>
    public bool Paused { get; private set; }
    public void Pause() { Paused = true; Log("live pump paused — jobs still queued, uploads held"); NotifyChanged(); }
    public void Resume() { Paused = false; Log("live pump resumed"); NotifyChanged(); }

    private readonly Queue<DateTime> _rolloverDays = new();
    private DateTime _rolloverRequestedAt;

    /// <summary>Lock-free view of "a rollover is outstanding", for the pump and finalize hot paths.</summary>
    private volatile bool _rolloverPending;

    /// <summary>
    /// How long the rollover waits for the in-flight file to finish before it gives up and cancels
    /// that transfer. The live pump is GATED while a rollover is pending, so an in-flight file that
    /// never completes would otherwise stop uploading permanently from midnight onward.
    /// </summary>
    private const int RolloverGraceSeconds = 60;

    /// <summary>
    /// True while a day rollover is still settling. The live pump is GATED in this state — jobs
    /// keep arriving and show in the list, but nothing is pulled for upload. Surfaced to the UI so
    /// this can never again look identical to a plain idle ("jobs OK but not running").
    /// </summary>
    public bool RolloverPending => _rolloverPending;

    /// <summary>Wall-clock time the last day rollover COMPLETED this run, or null if none has yet.
    /// Shown in the header so an operator can see at a glance that the day turned over cleanly.</summary>
    public DateTime? LastRolloverAt { get; private set; }

    /// <summary>The day the last completed rollover closed out (yyyyMMdd), or "" if none yet.</summary>
    public string LastRolloverFromDay { get; private set; } = "";

    /// <summary>Files carried into NG by the last completed rollover.</summary>
    public int LastRolloverAbandoned { get; private set; }

    /// <summary>
    /// Ask the engine to roll over to a new day. The reset is DEFERRED: the pump stops pulling new
    /// files, the in-flight file is allowed to finish, and then ProcessRolloverIfReady() abandons
    /// the old day's unfinished files to NG and clears the live list for the new day.
    ///
    /// Days QUEUE rather than overwrite. If the watch loop is slow enough that a second midnight
    /// arrives before the first rollover settles, overwriting would silently discard the first —
    /// its unfinished files would never reach that day's NG list and would be cleared under the
    /// wrong date. Each requested day gets its own settle-and-log.
    /// </summary>
    public void RequestRollover(DateTime oldDay)
    {
        int depth;
        lock (_gate)
        {
            if (_rolloverDays.Contains(oldDay)) return;    // already queued — don't double-count
            if (_rolloverDays.Count == 0) _rolloverRequestedAt = DateTime.Now;
            _rolloverDays.Enqueue(oldDay);
            _rolloverPending = true;
            depth = _rolloverDays.Count;
        }
        var queued = depth > 1 ? $" (queued behind {depth - 1} unsettled)" : "";
        Log($"day rollover requested ({oldDay:yyyyMMdd}){queued} — finishing current upload, then resetting");
        OpLog($"ROLLOVER REQUESTED from {oldDay:yyyyMMdd}{queued} — pump held until it settles");
        NotifyChanged();
    }

    /// <summary>
    /// Completes a requested rollover once nothing is in flight: every still-pending file from the
    /// old day is marked TimedOut and logged to the OLD day's raw log (so it lands in that day's NG
    /// list), then the live job list, queue and restart-history are cleared for the fresh day. New
    /// work flows in from the new day's jobs file via intake. Called from the watch loop.
    /// </summary>
    public void ProcessRolloverIfReady()
    {
        // Gate check. Let the current upload finish first — but NOT forever: the live pump is
        // gated while a rollover is pending, so an in-flight file that never returns would stop
        // all uploading from midnight onward. Past the grace period, cancel it and settle next tick.
        JobFile? wedged = null;
        bool notReady;
        lock (_gate)
        {
            notReady = _rolloverDays.Count == 0;
            if (!notReady && _inFlight is not null)
            {
                notReady = true;
                if ((DateTime.Now - _rolloverRequestedAt).TotalSeconds > RolloverGraceSeconds)
                {
                    wedged = _inFlight;
                    try { _currentTransfer?.Cancel(); } catch { }
                    _rolloverRequestedAt = DateTime.Now;   // re-arm, so we don't cancel every tick
                }
            }
        }
        if (wedged is not null)
        {
            Log($"day rollover blocked {RolloverGraceSeconds}s by {wedged.Pid}/{wedged.FileName} — " +
                $"cancelling that transfer so the new day can start");
            OpLog($"ROLLOVER BLOCKED {RolloverGraceSeconds}s by {wedged.Pid}/{wedged.FileName} — transfer cancelled");
        }
        if (notReady) return;

        DateTime oldDay;
        int remaining;
        var abandoned = new List<JobFile>();
        lock (_gate)
        {
            if (_rolloverDays.Count == 0) return;
            oldDay = _rolloverDays.Dequeue();
            remaining = _rolloverDays.Count;
            _rolloverPending = remaining > 0;
            if (remaining > 0) _rolloverRequestedAt = DateTime.Now;   // grace restarts for the next one

            foreach (var job in _jobs.Values)
                foreach (var f in job.Files)
                    if (f.Status == FileStatus.Pending)
                    {
                        f.Status = FileStatus.TimedOut;
                        job.TimedOut = true;
                        abandoned.Add(f);
                    }

            _jobs.Clear();
            _queue.Clear();
            _history = new Dictionary<string, JobFile>();
        }

        foreach (var f in abandoned)
            rawLog.Write(f, cfg.MaxAttempts, "", null, oldDay);   // into the OLD day's log → its NG

        LastRolloverAt = DateTime.Now;
        LastRolloverFromDay = oldDay.ToString("yyyyMMdd");
        LastRolloverAbandoned = abandoned.Count;

        var more = remaining > 0 ? $"; {remaining} more rollover(s) still queued" : "";
        Log($"day rollover complete — {abandoned.Count} unfinished file(s) from {oldDay:yyyyMMdd} moved to NG (TIMEDOUT); new day started{more}");
        OpLog($"ROLLOVER COMPLETE {oldDay:yyyyMMdd} -> {Clock.Today:yyyyMMdd} — {abandoned.Count} unfinished file(s) to NG; pump released{more}");
        NotifyChanged();
    }

    public async Task RunAsync(CancellationToken stopping)
    {
        while (!stopping.IsCancellationRequested)
        {
            // Paused, or waiting for a day-rollover to settle: don't pull new files.
            if (Paused || _rolloverPending)
            {
                await _ftp.EndSession();   // don't hold an idle FTP session open while paused
                await Task.Delay(cfg.PollIntervalMs, stopping);
                continue;
            }

            var next = TakeNext() ?? TakeCatchUp();

            if (next is null)
            {
                await _ftp.EndSession();   // nothing to send — close the reused connection until work arrives
                await Task.Delay(cfg.PollIntervalMs, stopping);
                continue;
            }

            try
            {
                await UploadOne(next, stopping);
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                // One bad file must never kill the pump. Before this guard, an unexpected throw ended
                // RunAsync for the life of the process — the app looked alive but uploaded nothing.
                // Count it as a spent attempt so a poison file can't be retried in a tight loop
                // forever — it runs out of attempts and lands in NG like any other failure.
                next.Attempts++;
                next.FailCount++;
                next.FailTimes.Add(DateTime.Now.ToString("HH:mm:ss"));
                next.Status = next.Attempts >= cfg.MaxAttempts ? FileStatus.Failed : FileStatus.Pending;
                _jobs.TryGetValue(next.Pid, out var errJob);
                rawLog.Write(next, cfg.MaxAttempts, "", errJob, errJob?.Day);
                Log($"   upload error on {next.Pid}/{next.FileName}: {ex.Message} — pump continuing");
                OpLog($"PUMP ERROR on {next.Pid}/{next.FileName}: {ex.Message} — recovered, pump continuing");
                await Task.Delay(cfg.PollIntervalMs, stopping);
            }
        }
    }

    private JobFile? TakeNext()
    {
        lock (_gate)
        {
            for (var i = 0; i < _queue.Count; i++)
            {
                var f = _queue[i];
                if (f.Status == FileStatus.Succeeded || f.Attempts >= cfg.MaxAttempts)
                {
                    _queue.RemoveAt(i--);
                    continue;
                }
                _queue.RemoveAt(i);
                return f;
            }
            return null;
        }
    }

    /// <summary>
    /// Idle-time sweep of the single job list (spec §3, idle catch-up). Returns any file that
    /// is still PENDING and has retries left but is not already queued or in flight.
    ///
    /// This IS the old "NG wait-list", now computed on the fly instead of kept in a second
    /// file: succeeded files and files that ran out of attempts (final X) are just skipped.
    /// A preempted file keeps its earned attempt count, so it dies for good after
    /// MaxAttempts real failures rather than being retried forever.
    /// </summary>
    private JobFile? TakeCatchUp()
    {
        lock (_gate)
        {
            foreach (var job in _jobs.Values)
                foreach (var f in job.Files)
                {
                    if (f.Status != FileStatus.Pending) continue;   // succeeded or final-failed
                    if (f.Attempts >= cfg.MaxAttempts) continue;     // out of road
                    if (ReferenceEquals(f, _inFlight)) continue;
                    if (_queue.Contains(f)) continue;
                    return f;
                }
            return null;
        }
    }

    private async Task UploadOne(JobFile f, CancellationToken stopping)
    {
        var attempt = f.Attempts + 1;
        _jobs.TryGetValue(f.Pid, out var job);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        lock (_gate)
        {
            _inFlight = f; _currentTransfer = cts; _inFlightStarted = DateTime.Now;
            // The panel-timeout clock starts when the panel's FIRST file begins uploading — not
            // when it was received. Panels are worked one at a time, so this gives each panel its
            // own window measured from its turn, and the next panel starts fresh.
            if (job is not null && job.Started == default) job.Started = DateTime.Now;
        }

        var host = _ftp.HostForAttempt(attempt);
        Log($"upload {f.Pid}/{f.FileName} -> ftp://{host}/{f.RemotePath.TrimStart('/')} attempt {attempt}/{cfg.MaxAttempts} via {host}");
        NotifyChanged();

        // Throughput sample: capture size + start time; recorded on success below.
        long sizeBytes = 0;
        try { sizeBytes = new FileInfo(f.LocalPath).Length; } catch { }
        var startedAt = DateTime.Now;
        var sessionBefore = _ftp.SessionNumber;

        TransferResult result;
        try
        {
            result = await _ftp.UploadAsync(f, attempt, cts.Token);
        }
        finally
        {
            // ALWAYS release the in-flight slot. If UploadAsync throws, leaving _inFlight set would
            // block ProcessRolloverIfReady() forever — and the live pump is gated on the rollover,
            // so uploading would stop dead at the next midnight and never resume.
            lock (_gate) { _inFlight = null; _currentTransfer = null; }
        }

        // A fresh connection was opened for this file (first file, cap reached, host change, or a
        // dropped connection was renewed) — surface it as a new session in the log.
        if (_ftp.SessionNumber > sessionBefore)
            Log($"   session #{_ftp.SessionNumber} opened on {host}");

        var timedOut = job is { TimedOut: true };

        switch (result.Outcome)
        {
            case TransferOutcome.Success:
                f.Attempts = attempt;
                f.Status = FileStatus.Succeeded;
                f.SucceedTime = DateTime.Now.ToString("HH:mm:ss");
                rawLog.Write(f, cfg.MaxAttempts, host, job, job?.Day);
                RecordSpeed(sizeBytes, (DateTime.Now - startedAt).TotalSeconds);
                Log($"   OK  {f.Pid}/{f.FileName} -> ftp://{host}/{f.RemotePath.TrimStart('/')}");

                // Manifest: this file is done — strip its " -pending" marker (index then host).
                if (job is not null && job.IsPanelJob) _manifest.MarkUploaded(job, f.RemotePath);

                if (job is not null && job.AllSucceeded)
                    Log($"   job {f.Pid} complete (O)");
                break;

            case TransferOutcome.Preempted:
                if (timedOut)
                {
                    // The panel timeout cancelled this file mid-attempt — a clean casualty. The
                    // attempt did not complete, so attempts/host are left as-is (no extra chip),
                    // and the line is written with an empty host.
                    f.Status = FileStatus.TimedOut;
                    rawLog.Write(f, cfg.MaxAttempts, "", job, job?.Day);
                    Log($"   panel timeout — {f.Pid}/{f.FileName} cut off (TIMEDOUT)");
                }
                else
                {
                    // Normal RESULT preemption: not a failed attempt; stays PENDING for catch-up.
                    Log($"   preempted — {f.Pid}/{f.FileName} left pending for catch-up");
                }
                break;

            case TransferOutcome.LocalMissing:
                // Source file gone — retrying can't help. Terminal failure, no requeue.
                f.Attempts = attempt;
                f.FailCount++;
                f.FailTimes.Add(DateTime.Now.ToString("HH:mm:ss"));
                f.Status = FileStatus.Failed;
                rawLog.Write(f, cfg.MaxAttempts, "", job, job?.Day);
                Log($"   local file missing — {f.Pid}/{f.FileName} (FAILED, no retry)");

                // Manifest: drop this file's line so the panel can still finalize (short by it).
                if (job is not null && job.IsPanelJob) _manifest.DropLine(job, f.RemotePath);
                break;

            default:    // Timeout or Error — a completed, failed attempt
                f.Attempts = attempt;
                f.FailCount++;
                f.FailTimes.Add(DateTime.Now.ToString("HH:mm:ss"));
                Log($"   {result.Outcome}: {result.Message}");

                if (f.Attempts >= cfg.MaxAttempts)
                {
                    // Used up every attempt on its own merits — a genuine NG (Failed).
                    f.Status = FileStatus.Failed;
                    rawLog.Write(f, cfg.MaxAttempts, host, job, job?.Day);
                    Log($"   NG after {cfg.MaxAttempts} attempts — {f.Pid}/{f.FileName}");
                }
                else if (timedOut)
                {
                    // The panel timed out; don't retry the remainder — mark it TimedOut.
                    f.Status = FileStatus.TimedOut;
                    rawLog.Write(f, cfg.MaxAttempts, host, job, job?.Day);
                    Log($"   panel timeout — {f.Pid}/{f.FileName} (TIMEDOUT)");
                }
                else
                {
                    f.Status = FileStatus.Pending;
                    rawLog.Write(f, cfg.MaxAttempts, host, job, job?.Day);
                    lock (_gate) _queue.Insert(0, f);   // retry, switching IP at attempt 3
                }
                break;
        }

        NotifyChanged();
    }

    /// <summary>
    /// Panel-timeout sweep. For any panel still holding unfinished (Pending) files longer than
    /// PanelTimeoutSeconds since it was first received, the remaining files — and the in-flight
    /// file if it belongs to that panel — are skipped and marked TimedOut, landing in the NG
    /// list for manual retry. Called periodically by the watch loop; a no-op when the timeout
    /// is 0. The in-flight file is cancelled here but marked by UploadOne (see above).
    /// </summary>
    public void CheckPanelTimeouts()
    {
        if (cfg.PanelTimeoutSeconds <= 0) return;

        var now = DateTime.Now;
        var skipped = new List<JobFile>();
        var pannedOut = new List<string>();

        lock (_gate)
        {
            foreach (var job in _jobs.Values)
            {
                if (job.TimedOut || job.Started == default) continue;
                if (!job.Files.Any(x => x.Status == FileStatus.Pending)) continue;   // resolved
                if ((now - job.Started).TotalSeconds < cfg.PanelTimeoutSeconds) continue;

                job.TimedOut = true;
                pannedOut.Add(job.Pid);

                foreach (var x in job.Files.Where(x => x.Status == FileStatus.Pending).ToList())
                {
                    if (ReferenceEquals(x, _inFlight)) continue;   // UploadOne marks this one
                    // Skipped before it ever uploaded: mark TimedOut only. No FailCount / FailTime
                    // (it never made an attempt), so it shows a clean 0/N with no phantom chip.
                    x.Status = FileStatus.TimedOut;
                    _queue.RemoveAll(q => ReferenceEquals(q, x));
                    // Data files go to the main rawlog + NG (retryable). Manifests do NOT: their
                    // content still has -pending (the data files never finished), so retrying them in
                    // NG would push an incomplete manifest. They stay a terminal in-memory TimedOut so
                    // the panel isn't stuck; NG sends them via its post-step once the data files recover.
                    if (!x.IsManifest) skipped.Add(x);
                }

                if (_inFlight is not null && _inFlight.Pid == job.Pid)
                    _currentTransfer?.Cancel();   // abandon the in-flight file of the timed-out panel
            }
        }

        foreach (var x in skipped)
        {
            _jobs.TryGetValue(x.Pid, out var job);
            rawLog.Write(x, cfg.MaxAttempts, "", job, job?.Day);
        }
        foreach (var pid in pannedOut)
            Log($"panel timeout: {pid} exceeded {cfg.PanelTimeoutSeconds}s — remaining files skipped to NG (TIMEDOUT)");

        if (pannedOut.Count > 0) NotifyChanged();
    }
}
