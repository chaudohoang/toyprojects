namespace FtpUpload;

/// <summary>
/// The running program: engine + intake + command channel, started once at launch and
/// living for the life of the process. The tray icon and the Blazor UI both talk to
/// this same instance, which is why uploading continues when the window is closed.
/// </summary>
public sealed class AppHost : IDisposable
{
    private readonly CancellationTokenSource _stopping = new();
    private readonly List<string> _log = new();       // live pump + intake + commands + system
    private readonly List<string> _ngLog = new();     // NG-retry pump only
    private readonly object _logGate = new();
    private Task? _pump;
    private Task? _ngPump;
    private Task? _finalizePump;
    private Task? _watch;

    public Config Cfg { get; }
    public UploadEngine Engine { get; }
    public NgRetryEngine NgRetry { get; }
    public CommandChannel Commands { get; }
    public JobIntake Intake { get; }
    public PanelIntake Panels { get; }
    public string ConfigPath { get; }

    /// <summary>True once a STOP command has been processed (watchdog must not restart).</summary>
    public bool StopRequested => Commands.StopRequested;

    public event Action? Changed;
    public event Action? LogChanged;
    public event Action? NgLogChanged;
    /// <summary>Raised when a STOP command arrives, so the UI shell can close the process.</summary>
    public event Action? StopSignalled;

    public AppHost()
    {
        ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        Cfg = Config.Load(ConfigPath);
        Cfg.EnsureFolders();

        var rawLog = new RawLog(Cfg);
        Engine = new UploadEngine(Cfg, rawLog, new SnapshotLog(Cfg));
        NgRetry = new NgRetryEngine(Cfg, new NgRetryLog(Cfg));
        Intake = new JobIntake(Cfg, Engine);
        Panels = new PanelIntake(Cfg, Engine);
        Commands = new CommandChannel(Cfg, Engine);

        // Crash/restart recovery: today's raw log tells us what was already uploaded, so the
        // job file can be re-read from the top without re-sending completed work.
        Engine.LoadHistory(rawLog.ReadState(DateTime.Now));

        Engine.Logged += Append;
        Intake.Logged += Append;
        Panels.Logged += Append;
        Commands.Logged += Append;
        NgRetry.Logged += AppendNg;
        Engine.Changed += () => Changed?.Invoke();
        NgRetry.Changed += () => Changed?.Invoke();

        // NG console loads today PLUS the configured past days, so work abandoned at a rollover
        // (filed under the OLD day) is recovered automatically instead of waiting for an engineer
        // to open yesterday by hand. Whether it auto-retries on launch is configurable.
        //
        // Restore the operator's saved IP choice BEFORE auto-retry starts, so the first attempt
        // after a restart already uses the host they picked rather than silently reverting to Auto.
        // The watchdog restarts this app routinely, so a deliberate pin must survive that.
        NgRetry.IpMode = Cfg.NgIpMode switch
        {
            "Primary" => NgIpMode.Primary,
            "Secondary" => NgIpMode.Secondary,
            _ => NgIpMode.Auto
        };
        NgRetry.LoadWindow();
        if (Cfg.AutoStartRetrying) NgRetry.StartAutoRetry();

        // Count NG work stranded on days that have aged out of the recovery window. Off-thread: on
        // a cold cache this reads every retained day's logs. Logged as well as shown, so a headless
        // machine leaves a record — "NG List (0)" with a four-figure backlog is exactly the kind of
        // silent nothing-is-happening this codebase has already been bitten by once.
        _ = Task.Run(() =>
        {
            NgRetry.RefreshBacklog();
            if (NgRetry.BacklogOutstanding > 0)
            {
                var line = $"NG BACKLOG {NgRetry.BacklogOutstanding} unrecovered file(s) on {NgRetry.BacklogDays} day(s) " +
                           $"outside the {Cfg.NgRecoveryDays}-day recovery window — not being retried automatically";
                AppendNg($"[{DateTime.Now:HH:mm:ss}] {line}");
                try { SafeFile.Append(Cfg.OpLogPath(Clock.Now), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {line}"); } catch { }
            }
        });

        // Live pump likewise: start paused if the operator opted out of auto-uploading.
        if (!Cfg.AutoStartUploading) Engine.Pause();

        // Housekeeping: prune old date-stamped logs/reports on startup (no-op if retention = 0).
        LogRetention.Purge(Cfg, Append);

        Append($"[{DateTime.Now:HH:mm:ss}] FTP Upload started — primary {Cfg.PrimaryHost}:{Cfg.Port}, " +
               $"secondary {Cfg.SecondaryHost}, timeout {Cfg.TimeoutSeconds}s, {Cfg.MaxAttempts} attempts");

        // Durable startup marker, so the oplog reads as a clean START/SHUTDOWN pair per run and a
        // restart is obvious even when nobody was watching the window.
        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        try { SafeFile.Append(Cfg.OpLogPath(Clock.Now),
              $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] STARTUP v{ver} pid {Environment.ProcessId} — " +
              $"primary {Cfg.PrimaryHost}, secondary {Cfg.SecondaryHost}"); } catch { }
        Append($"[{DateTime.Now:HH:mm:ss}] transfer engine: {FtpEngineFactory.ActiveEngine}" +
               (FtpEngineFactory.ActiveEngine == "WinSCP" ? $" ({Cfg.FtpMode} mode)" : ""));
    }

    public void Start()
    {
        // Upload pump: one file in flight, for the life of the process.
        _pump = Task.Run(async () =>
        {
            try { await Engine.RunAsync(_stopping.Token); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Append($"[{DateTime.Now:HH:mm:ss}] pump error: {ex.Message}"); }
        });

        // Separate NG-retry pump — recovers old/failed items independently of the live pump.
        _ngPump = Task.Run(async () =>
        {
            try { await NgRetry.RunAsync(_stopping.Token); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { AppendNg($"[{DateTime.Now:HH:mm:ss}] NG pump error: {ex.Message}"); }
        });

        // Manifest finalize gets its OWN pump. Each finalize is a real FTP send that takes tens of
        // seconds against a slow or unreachable host; on the watch loop that starved the day rollover,
        // intake and command polling badly enough to stop uploading entirely under load.
        _finalizePump = Task.Run(async () =>
        {
            while (!_stopping.IsCancellationRequested)
            {
                try { await Engine.FinalizeReadyPanels(); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Append($"[{DateTime.Now:HH:mm:ss}] finalize error: {ex.Message}"); }
                try { await Task.Delay(Cfg.PollIntervalMs, _stopping.Token); }
                catch (OperationCanceledException) { }
            }
        });

        // Intake + commands, polled independently so a long transfer can still be
        // preempted the moment a RESULT arrives.
        var watchDay = Clock.Today;
        var lastDayAdvance = DateTime.Now;   // REAL time, for the fast-day test knob
        var lastWall = DateTime.Now;         // REAL time, to detect a system-clock / timezone jump
        _watch = Task.Run(async () =>
        {
            while (!_stopping.IsCancellationRequested)
            {
                try
                {
                    // Detect a system-clock / timezone change. Between ticks the REAL wall clock should
                    // advance by ~one poll interval; a big forward jump or ANY backward move means the
                    // clock or timezone was changed under us (e.g. an operator fixed the timezone). Log
                    // it clearly so a "day suddenly looks empty" is explainable; the rollover below then
                    // resyncs the current day automatically, no restart needed.
                    var nowWall = DateTime.Now;
                    var jump = nowWall - lastWall;
                    lastWall = nowWall;
                    if (jump > TimeSpan.FromMinutes(10) || jump < TimeSpan.FromSeconds(-30))
                    {
                        var jmsg = $"[{DateTime.Now:HH:mm:ss}] SYSTEM CLOCK CHANGED ({jump.TotalHours:+0.0;-0.0}h) \u2014 " +
                                   $"resyncing to {Clock.Today:yyyyMMdd}. Work filed before the change stays under its original date.";
                        Append(jmsg);
                        AppendNg(jmsg);
                    }

                    // TESTING: fast-forward a simulated calendar day so the rollover can be tried
                    // without waiting for real midnight.
                    if (Cfg.SimulateFastDaySeconds > 0 &&
                        (DateTime.Now - lastDayAdvance).TotalSeconds >= Cfg.SimulateFastDaySeconds)
                    {
                        Clock.Offset += TimeSpan.FromDays(1);
                        lastDayAdvance = DateTime.Now;
                        Append($"[{DateTime.Now:HH:mm:ss}] (sim) advanced to {Clock.Today:yyyyMMdd}");
                    }

                    // Day rollover: fire ONCE when the (simulated or real) calendar day changes.
                    // Single source of truth so the pieces can't roll over independently.
                    var today = Clock.Today;
                    if (today != watchDay)
                    {
                        // Forward = a normal midnight rollover OR the clock jumped ahead. Backward =
                        // the clock was moved back a day; don't "end"/archive the newer day in that case.
                        var forward = today > watchDay;
                        if (forward)
                            Engine.RequestRollover(watchDay);              // engine resets once idle
                        else
                            AppendNg($"[{DateTime.Now:HH:mm:ss}] clock moved BACK to {today:yyyyMMdd}; resyncing without a rollover");

                        // Re-window the NG console onto the new day — which KEEPS the day that just
                        // ended in the list, so everything the rollover abandoned to it is retried
                        // automatically. If the operator has picked a specific past day to review,
                        // leave them there; don't yank them to the new window mid-review.
                        if (NgRetry.WindowMode)
                        {
                            AppendNg($"[{DateTime.Now:HH:mm:ss}] day changed -> {today:yyyyMMdd}; NG window following (keeps {watchDay:yyyyMMdd} for recovery)");
                            NgRetry.LoadWindow();
                            NgRetry.StartAutoRetry();
                        }
                        else
                        {
                            AppendNg($"[{DateTime.Now:HH:mm:ss}] day changed -> {today:yyyyMMdd}; NG staying on {NgRetry.LoadedDay} (as selected)");
                        }
                        watchDay = today;

                        // Prune old date-stamped logs/reports once per day at rollover — but not on a
                        // backward clock move (that isn't a real new day).
                        if (forward) LogRetention.Purge(Cfg, AppendNg);

                        // A day just aged out of the recovery window (and retention may have just
                        // purged another), so the backlog figure is now stale. Off-thread — this
                        // reads logs and must not hold up the watch loop.
                        _ = Task.Run(() => NgRetry.RefreshBacklog());
                    }

                    // Settle a pending rollover FIRST, before anything else in the tick:
                    //  - it must run before intake, or work ingested for the NEW day gets caught by
                    //    the old day's reset (wiped from the live list and mis-filed as TIMEDOUT
                    //    under yesterday's NG);
                    //  - it must run before anything that can throw, or one bad poll swallows the
                    //    call for good and the live pump stays gated on _rolloverOldDay forever.
                    var rollBefore = Engine.LastRolloverAt;
                    Engine.ProcessRolloverIfReady();
                    // A rollover just settled: the OLD day's rawlog has only now gained its abandoned
                    // files, so pull them into the NG list immediately rather than after the throttle.
                    if (Engine.LastRolloverAt != rollBefore) NgRetry.ForcePastMerge();

                    Intake.Poll();
                    Panels.Poll();
                    Commands.Poll();
                    Engine.CheckPanelTimeouts();
                    NgRetry.RefreshLoadedDayLive();
                    if (Commands.StopRequested) { Stop(); StopSignalled?.Invoke(); break; }
                }
                catch (Exception ex) { Append($"[{DateTime.Now:HH:mm:ss}] watch error: {ex.Message}"); }
                try { await Task.Delay(Cfg.PollIntervalMs, _stopping.Token); }
                catch (OperationCanceledException) { }
            }
        });
    }

    public void Stop() => _stopping.Cancel();

    public IReadOnlyList<string> LogLines
    {
        get { lock (_logGate) return _log.ToArray(); }
    }

    public IReadOnlyList<string> NgLogLines
    {
        get { lock (_logGate) return _ngLog.ToArray(); }
    }

    private void Append(string line)
    {
        lock (_logGate)
        {
            _log.Add(line);
            if (_log.Count > 500) _log.RemoveRange(0, _log.Count - 500);   // keep memory bounded
        }
        LogChanged?.Invoke();
    }

    private void AppendNg(string line)
    {
        lock (_logGate)
        {
            _ngLog.Add(line);
            if (_ngLog.Count > 500) _ngLog.RemoveRange(0, _ngLog.Count - 500);
        }
        NgLogChanged?.Invoke();
    }

    /// <summary>
    /// Record a lifecycle event (startup, shutdown and why) to the oplog as well as the in-app log.
    /// A clean exit used to leave NO trace at all, so "it stopped uploading overnight" looked
    /// identical whether an operator clicked tray-Exit, TrueTest sent STOP, or the process was
    /// killed. The reason belongs on disk, where it can still be read tomorrow.
    /// </summary>
    public void LogEvent(string msg)
    {
        try { SafeFile.Append(Cfg.OpLogPath(Clock.Now), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}"); }
        catch { }
        try { Append($"[{DateTime.Now:HH:mm:ss}] {msg}"); }
        catch { }
    }

    /// <summary>
    /// Record an exception that escaped to a top-level handler, to the oplog as well as the in-app
    /// log. Written to disk because these are exactly the events that leave no trace otherwise: the
    /// in-memory log dies with the process, and on a headless machine nobody is watching the window.
    /// A crash that produced no evidence is indistinguishable from "it just stopped uploading".
    /// </summary>
    public void LogCrash(string where, Exception? ex)
    {
        var detail = ex?.ToString() ?? "(no exception object supplied)";
        try { SafeFile.Append(Cfg.OpLogPath(Clock.Now), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED {where} EXCEPTION — {detail}"); }
        catch { /* logging a crash must not cause one */ }
        try { Append($"[{DateTime.Now:HH:mm:ss}] unhandled {where} exception: {ex?.Message} — see oplog"); }
        catch { }
    }

    public void Dispose()
    {
        _stopping.Cancel();
        try { Task.WaitAll(new[] { _pump, _ngPump, _finalizePump, _watch }.Where(t => t is not null).ToArray()!, 3000); } catch { }
        _stopping.Dispose();
    }
}
