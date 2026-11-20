using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace YdkeDeckBuilder;

public partial class MainWindow : Window
{
    // The page is served from a virtual https host rather than file://, so it gets a real origin:
    // fetch to YGOPRODeck, the clipboard API and localStorage all behave as they do on a website.
    const string Host = "ydke.example"; // .example is reserved, so it can never shadow a real site

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await InitAsync();
    }

    async Task InitAsync()
    {
        try
        {
            // Keep WebView2's profile (cache, localStorage) out of the program folder.
            var userData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YdkeDeckBuilder");
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userData);
            await Web.EnsureCoreWebView2Async(env);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(this, "This app needs the Microsoft Edge WebView2 Runtime.\n\n" +
                "Download it from https://developer.microsoft.com/microsoft-edge/webview2/",
                Title, MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }
        catch (COMException ex)
        {
            // E_UNEXPECTED here (then ERROR_BUSY on later runs) is what a low-integrity process gets; see
            // App.LowIntegrityHint. Startup normally catches that case earlier, when writing the loader.
            MessageBox.Show(this, ex.Message + "\n\n" + App.LowIntegrityHint, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        var core = Web.CoreWebView2;
        // The page is embedded in the exe (single-file publish), so serve it from resources.
        core.AddWebResourceRequestedFilter($"https://{Host}/*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += (_, e) => e.Response = ServeEmbedded(core.Environment, new Uri(e.Request.Uri));
        core.Settings.IsStatusBarEnabled = false;
#if !DEBUG
        core.Settings.AreDevToolsEnabled = false;
#endif

        // Card links (target=_blank) open in the user's normal browser, not in a bare pop-up window.
        core.NewWindowRequested += (_, e) =>
        {
            e.Handled = true;
            OpenExternal(e.Uri);
        };
        core.NavigationStarting += (_, e) =>
        {
            if (!new Uri(e.Uri).Host.Equals(Host, StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                OpenExternal(e.Uri);
            }
        };
        // "Save .ydk file" asks where to save, like a desktop app, instead of dropping it in Downloads.
        core.DownloadStarting += (_, e) =>
        {
            var dialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(e.ResultFilePath),
                Filter = "YGOPro deck (*.ydk)|*.ydk|All files (*.*)|*.*",
            };
            if (dialog.ShowDialog(this) == true)
            {
                e.ResultFilePath = dialog.FileName;
                e.Handled = true; // no download flyout
            }
            else
            {
                e.Cancel = true;
            }
        };
        core.DocumentTitleChanged += (_, _) => Title = core.DocumentTitle;

        core.Navigate($"https://{Host}/index.html");
    }

    static CoreWebView2WebResourceResponse ServeEmbedded(CoreWebView2Environment env, Uri uri)
    {
        var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
        var stream = typeof(MainWindow).Assembly.GetManifestResourceStream("wwwroot/" + path);
        if (stream == null) return env.CreateWebResourceResponse(null, 404, "Not Found", "");

        var type = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css",
            ".js" => "text/javascript",
            ".json" => "application/json",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream",
        };
        return env.CreateWebResourceResponse(stream, 200, "OK", $"Content-Type: {type}");
    }

    static void OpenExternal(string uri)
    {
        if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }
}
