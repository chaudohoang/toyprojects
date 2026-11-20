using System.Windows;

namespace YdkeDeckBuilderWpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Theme.Init(this);
        base.OnStartup(e);
    }
}
