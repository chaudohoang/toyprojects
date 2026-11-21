using System.Windows;

namespace CharlieTheSlave;

public partial class App : Application
{
    // A second launch signals this, and the running copy brings its icon back on screen and
    // opens the prompt (both copies run elevated, so the signal is allowed across them).
    const string ShowEventName = @"Local\CharlieTheSlave.Show";

    Mutex? _singleInstance;
    TrayIcon? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // "--install <path>" is passed when we restart ourselves as administrator.
        var i = Array.IndexOf(e.Args, "--install");
        string? resumeInstaller = i >= 0 && i + 1 < e.Args.Length ? e.Args[i + 1] : null;

        var show = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        _singleInstance = new Mutex(false, @"Local\CharlieTheSlave.SingleInstance");
        bool owned;
        try { owned = _singleInstance.WaitOne(resumeInstaller != null ? 5000 : 0); }
        catch (AbandonedMutexException) { owned = true; }
        if (!owned)
        {
            show.Set();
            Shutdown();
            return;
        }

        var prompt = new PromptWindow();
        var icon = new FloatingIconWindow(prompt);
        icon.Show();
        _tray = new TrayIcon(icon, prompt);

        new Thread(() =>
        {
            while (show.WaitOne())
                Dispatcher.BeginInvoke(new Action(() => { icon.KeepOnScreen(); prompt.ShowNear(icon); }));
        }) { IsBackground = true }.Start();

        if (resumeInstaller != null)
        {
            prompt.ShowNear(icon);
            prompt.ResumeInstall(resumeInstaller);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
