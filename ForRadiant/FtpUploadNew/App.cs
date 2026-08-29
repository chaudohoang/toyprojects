using System.Windows;
using Forms = System.Windows.Forms;
using Application = System.Windows.Application;

namespace FtpUpload;

/// <summary>
/// WPF application shell. Owns the tray icon and the (lazily created) window.
/// The upload engine lives in AppHost and runs whether or not the window exists.
/// </summary>
public sealed class App : Application
{
    public static bool ShuttingDown { get; private set; }

    private readonly AppHost _host;
    private Forms.NotifyIcon? _tray;
    private MainWindow? _window;

    public App(AppHost host)
    {
        _host = host;
        ShutdownMode = ShutdownMode.OnExplicitShutdown;   // closing the window must not end the process
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open manager", null, (_, _) => ShowWindow());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit (stops uploading)", null, (_, _) => ExitProgram("tray menu Exit"));

        _tray = new Forms.NotifyIcon
        {
            // Use the exe's own icon (set via <ApplicationIcon>) so the tray matches taskbar/alt-tab.
            Icon = TryLoadAppIcon() ?? System.Drawing.SystemIcons.Application,
            Text = "FTP Upload — running",
            Visible = true,
            ContextMenuStrip = menu
        };
        _tray.DoubleClick += (_, _) => ShowWindow();
    }

    /// <summary>The application icon embedded in the exe (falls back to null on any failure).</summary>
    private static System.Drawing.Icon? TryLoadAppIcon()
    {
        try
        {
            var exe = Environment.ProcessPath;
            return exe is null ? null : System.Drawing.Icon.ExtractAssociatedIcon(exe);
        }
        catch { return null; }
    }

    /// <summary>
    /// WPF windows are cheap to build (no browser to boot), so this is created on first
    /// open and then kept around — no pre-warming needed.
    /// </summary>
    public void ShowWindow()
    {
        if (_window is null)
        {
            _window = new MainWindow(_host);
            _window.Closed += (_, _) => _window = null;
        }

        _window.Show();
        if (_window.WindowState == WindowState.Minimized)
            _window.WindowState = WindowState.Maximized;
        _window.Activate();
        _window.Topmost = true;
        _window.Topmost = false;
    }

    private bool _hintShown;

    /// <summary>
    /// Shown the first time the window is hidden, so it is clear that closing the window
    /// did not stop uploading. Once per run — a balloon on every close would be nagging.
    /// </summary>
    public void NotifyHiddenOnce()
    {
        if (_hintShown || _tray is null) return;
        _hintShown = true;
        _tray.ShowBalloonTip(3000, "FTP Upload",
            "Still running and uploading. Double-click this icon to reopen.",
            Forms.ToolTipIcon.Info);
    }

    /// <summary>
    /// The single exit path. <paramref name="reason"/> is recorded to the oplog BEFORE anything is
    /// torn down: a clean exit is otherwise indistinguishable from a kill or a crash after the fact,
    /// which makes "it stopped uploading overnight" unanswerable on a machine nobody was watching.
    /// </summary>
    private void ExitProgram(string reason)
    {
        ShuttingDown = true;
        _host.LogEvent($"SHUTDOWN — {reason}; pumps stopping");
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); _tray = null; }
        _host.Stop();
        Shutdown();
    }

    /// <summary>Called when the engine processes a STOP command from TrueTest.</summary>
    public void ShutdownFromCommand()
    {
        Dispatcher.Invoke(() => ExitProgram("STOP command from TrueTest"));
    }

    /// <summary>
    /// Windows is ending the session (logoff, restart, shutdown). Recorded separately because it is
    /// nobody's mistake — but it still stops uploading, and without a line here it looks the same as
    /// an unexplained disappearance.
    /// </summary>
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        _host.LogEvent($"SHUTDOWN — Windows session ending ({e.ReasonSessionEnding}); pumps stopping");
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Catches any exit path that didn't go through ExitProgram (e.g. Shutdown() called
        // elsewhere), so the log never simply stops mid-sentence.
        if (!ShuttingDown) _host.LogEvent("SHUTDOWN — process exiting (reason not recorded)");
        base.OnExit(e);
    }
}
