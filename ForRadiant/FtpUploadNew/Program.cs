namespace FtpUpload;

/// <summary>
/// Single-exe entry point: the upload engine AND its manager UI in one process.
///
/// Starts hidden in the system tray. The engine runs on background threads from the
/// moment the process starts, whether or not the window is ever opened.
///
/// Exit codes, read by run_watchdog.bat:
///   0 / other : unexpected exit or crash    -> watchdog RESTARTS
///   2         : another instance is live    -> watchdog exits quietly
///   3         : STOP command received       -> watchdog exits quietly
/// </summary>
internal static class Program
{
    private const int ExitCrashed = 0;
    private const int ExitAlreadyRunning = 2;
    private const int ExitStopRequested = 3;

    [STAThread]
    private static int Main(string[] args)
    {
        // Single-instance guard: the keep-alive task may fire while a healthy copy is
        // already running, and two uploaders would double-transfer.
        using var single = new Mutex(true, @"Global\FtpUpload_Worker_Instance", out var isOnlyInstance);
        if (!isOnlyInstance) return ExitAlreadyRunning;

        using var host = new AppHost();
        var app = new App(host);

        // A UI-thread exception must NEVER kill the engine. This process is an uploader that happens
        // to have a window: the live, NG, finalize and watch pumps all die with it. A mouse click on
        // a text label once took the whole thing down mid-production, and because the process was
        // gone before it could write anything, the only symptom anyone saw was "it stopped
        // uploading". Swallow it, record it, keep transferring.
        app.DispatcherUnhandledException += (_, e) =>
        {
            host.LogCrash("UI", e.Exception);
            e.Handled = true;
        };

        // Background-thread escapes can't be cancelled — the runtime tears the process down
        // regardless — but logging first means the restart leaves a reason behind instead of a gap.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            host.LogCrash("BACKGROUND", e.ExceptionObject as Exception);

        // Same for a faulted Task nobody awaited: observe it so it is recorded rather than silent.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            host.LogCrash("TASK", e.Exception);
            e.SetObserved();
        };

        // A STOP from TrueTest must close the whole program, not just the window.
        host.StopSignalled += app.ShutdownFromCommand;

        host.Start();

        // Show the window by default. The watchdog passes "--hidden" so a background restart
        // starts in the tray without throwing a window in the operator's face. ("--show" is
        // kept as a harmless alias for the default.)
        if (!args.Any(a => a.Equals("--hidden", StringComparison.OrdinalIgnoreCase)))
            app.Startup += (_, _) => app.ShowWindow();

        app.Run();

        return host.StopRequested ? ExitStopRequested : ExitCrashed;
    }
}
