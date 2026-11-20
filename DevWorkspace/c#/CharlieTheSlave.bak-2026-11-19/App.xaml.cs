using System.Windows;

namespace CharlieTheSlave;

public partial class App : Application
{
    Mutex? _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // "--install <path>" is passed when we restart ourselves as administrator.
        var i = Array.IndexOf(e.Args, "--install");
        string? resumeInstaller = i >= 0 && i + 1 < e.Args.Length ? e.Args[i + 1] : null;

        _singleInstance = new Mutex(false, @"Local\CharlieTheSlave.SingleInstance");
        bool owned;
        try { owned = _singleInstance.WaitOne(resumeInstaller != null ? 5000 : 0); }
        catch (AbandonedMutexException) { owned = true; }
        if (!owned)
        {
            Shutdown();
            return;
        }

        var prompt = new PromptWindow();
        var icon = new FloatingIconWindow(prompt);
        icon.Show();

        if (resumeInstaller != null)
        {
            prompt.ShowNear(icon);
            prompt.ResumeInstall(resumeInstaller);
        }
    }
}
