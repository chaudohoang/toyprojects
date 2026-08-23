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
    private readonly FtpTransfer _ftp = new(cfg, reuseConnections: true);   // live pump reuses one connection

    /// <summary>Current session number and files-on-this-session, surfaced for the live strip.</summary>
    public int SessionNumber => _ftp.SessionNumber;
    public int FilesThisSession => _ftp.FilesThisSession;

    // Manifest (index/host) writer + a SEPARATE transfer used only to send finalized manifests,
    // so completion uploads never collide with the single-in-flight live transfer.
    private readonly ManifestWriter _manifest = new(cfg);
    private readonly FtpTransfer _manifestFtp = new(cfg);

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
                _queue.Add(f);
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
    /// Off the hot path (called from the watch loop): for every panel that is fully resolved —
    /// every data file uploaded (clean) or dropped (source gone), so no " -pending" remains —
    /// upload its index + host manifests. Idempotent; a failed send is retried next tick.
    /// </summary>
    public async Task FinalizeReadyPanels()
    {
        foreach (var job in _jobs.Values)
        {
            if (job.Finalized || !job.IsPanelJob) continue;
            try
            {
                if (await _manifest.TryFinalizeAsync(job, _manifestFtp))
                {
                    Log($"panel {job.Pid}: index + host manifest sent — panel complete");
                    NotifyChanged();
                }
            }
            catch (Exception ex)
            {
                Log($"panel {job.Pid}: finalize error: {ex.Message}");
            }
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

    private DateTime? _rolloverOldDay;

    /// <summary>
    /// Ask the engine to roll over to a new day. The reset is DEFERRED: the pump stops pulling new
    /// files, the in-flight file is allowed to finish, and then ProcessRolloverIfReady() abandons
    /// the old day's unfinished files to NG and clears the live list for the new day.
    /// </summary>
    public void RequestRollover(DateTime oldDay)
    {
        lock (_gate) _rolloverOldDay = oldDay;
        Log($"day rollover requested ({oldDay:yyyyMMdd}) — finishing current upload, then resetting");
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
        DateTime oldDay;
        var abandoned = new List<JobFile>();
        lock (_gate)
        {
            if (_rolloverOldDay is null) return;
            if (_inFlight is not null) return;      // let the current upload finish first
            oldDay = _rolloverOldDay.Value;
            _rolloverOldDay = null;

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

        Log($"day rollover complete — {abandoned.Count} unfinished file(s) from {oldDay:yyyyMMdd} moved to NG (TIMEDOUT); new day started");
        NotifyChanged();
    }

    public async Task RunAsync(CancellationToken stopping)
    {
        while (!stopping.IsCancellationRequested)
        {
            // Paused, or waiting for a day-rollover to settle: don't pull new files.
            if (Paused || _rolloverOldDay is not null)
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

            await UploadOne(next, stopping);
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

        var result = await _ftp.UploadAsync(f, attempt, cts.Token);

        // A fresh connection was opened for this file (first file, cap reached, host change, or a
        // dropped connection was renewed) — surface it as a new session in the log.
        if (_ftp.SessionNumber > sessionBefore)
            Log($"   session #{_ftp.SessionNumber} opened on {host}");

        lock (_gate) { _inFlight = null; _currentTransfer = null; }

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
                    skipped.Add(x);
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
