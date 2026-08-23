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
