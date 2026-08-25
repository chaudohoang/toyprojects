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

        // NG console loads today's list; whether it auto-retries on launch is configurable.
        NgRetry.LoadDay(Clock.Now.ToString("yyyyMMdd"));
        if (Cfg.AutoStartRetrying) NgRetry.StartAutoRetry();

        // Live pump likewise: start paused if the operator opted out of auto-uploading.
        if (!Cfg.AutoStartUploading) Engine.Pause();

        // Housekeeping: prune old date-stamped logs/reports on startup (no-op if retention = 0).
        LogRetention.Purge(Cfg, Append);

        Append($"[{DateTime.Now:HH:mm:ss}] FTP Upload started — primary {Cfg.PrimaryHost}:{Cfg.Port}, " +
               $"secondary {Cfg.SecondaryHost}, timeout {Cfg.TimeoutSeconds}s, {Cfg.MaxAttempts} attempts");
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

        // Intake + commands, polled independently so a long transfer can still be
        // preempted the moment a RESULT arrives.
        var watchDay = Clock.Today;
        var lastDayAdvance = DateTime.Now;   // REAL time, for the fast-day test knob
        _watch = Task.Run(async () =>
        {
            while (!_stopping.IsCancellationRequested)
            {
                try
                {
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
                        Engine.RequestRollover(watchDay);              // engine resets once idle

                        // Only pull the NG console to the new day if it was following the day that
                        // just ended. If the operator has navigated to a past day to review/rerun
                        // it, leave them there — don't yank them to the new day mid-review.
                        if (NgRetry.LoadedDay == watchDay.ToString("yyyyMMdd"))
                        {
                            AppendNg($"[{DateTime.Now:HH:mm:ss}] day rolled over -> {today:yyyyMMdd}; NG console following");
                            NgRetry.LoadDay(today.ToString("yyyyMMdd"));
                            NgRetry.StartAutoRetry();
                        }
                        else
                        {
                            AppendNg($"[{DateTime.Now:HH:mm:ss}] day rolled over -> {today:yyyyMMdd}; NG staying on {NgRetry.LoadedDay} (as selected)");
                        }
                        watchDay = today;

                        // Prune old date-stamped logs/reports once per day at rollover.
                        LogRetention.Purge(Cfg, AppendNg);
                    }

                    Intake.Poll();
                    Panels.Poll();
                    Commands.Poll();
                    Engine.CheckPanelTimeouts();
                    await Engine.FinalizeReadyPanels();
                    Engine.ProcessRolloverIfReady();
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

    public void Dispose()
    {
        _stopping.Cancel();
        try { Task.WaitAll(new[] { _pump, _ngPump, _watch }.Where(t => t is not null).ToArray()!, 3000); } catch { }
        _stopping.Dispose();
    }
}
