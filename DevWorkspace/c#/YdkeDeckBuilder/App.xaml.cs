using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace YdkeDeckBuilder;

public partial class App : Application
{
    public const string Name = "YDKE Deck Builder";

    /// <summary>
    /// Shown when the app can't write to %LocalAppData%. The usual cause is running the exe from inside
    /// E:\Dev: that folder carries a Low Mandatory Level integrity label, every exe stored under it runs at
    /// low integrity, and a low-integrity process can't write to normal folders or start WebView2
    /// (E_UNEXPECTED). The app can't raise its own integrity level, so the exe has to live elsewhere.
    /// </summary>
    public const string LowIntegrityHint =
        "Windows is running this copy at low integrity, which usually means the exe is stored in a folder " +
        "with a low integrity label (such as E:\\Dev). WebView2 can't run like that.\n\n" +
        "Run install.ps1, or copy YdkeDeckBuilder.exe to another folder (e.g. the Desktop) and run that copy.";

    static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YdkeDeckBuilder");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // A windowless app that dies at startup tells nobody, so always say what went wrong.
        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), Name, MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
            Shutdown(1);
        };

        try
        {
            PrepareWebView2Loader();
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(LowIntegrityHint, Name, MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show("YDKE Deck Builder couldn't start:\n\n" + ex.Message, Name, MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }
        new MainWindow().Show();
    }

    /// <summary>
    /// WebView2Loader.dll is embedded as a plain resource rather than bundled as a native library, so the
    /// .NET host never has to extract anything to %TEMP%\.net. Write it out once per build and tell WebView2
    /// to load it from there.
    /// </summary>
    static void PrepareWebView2Loader()
    {
        using var embedded = typeof(App).Assembly.GetManifestResourceStream("WebView2Loader.dll")
                             ?? throw new InvalidOperationException("WebView2Loader.dll is missing from the exe.");
        var dir = Path.Combine(DataDir, "loader");
        var dll = Path.Combine(dir, "WebView2Loader.dll");
        if (!File.Exists(dll) || new FileInfo(dll).Length != embedded.Length)
        {
            Directory.CreateDirectory(dir);
            var temp = dll + ".tmp";
            using (var file = File.Create(temp)) embedded.CopyTo(file);
            File.Move(temp, dll, overwrite: true);
        }
        CoreWebView2Environment.SetLoaderDllFolderPath(dir);
    }
}
