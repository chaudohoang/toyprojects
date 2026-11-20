using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace YdkeDeckBuilderWpf;

/// <summary>
/// Follows Windows' app mode (Settings > Personalization > Colors > "Choose your app mode"), like the web
/// version follows prefers-color-scheme, and switches live when it changes. The palette lives in
/// Themes\Light.xaml / Themes\Dark.xaml, which is why the XAML uses DynamicResource for every theme brush.
/// </summary>
public static class Theme
{
    const string PersonalizeKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public static bool IsDark { get; private set; }

    /// <summary>Raised on the UI thread after the palette has been swapped.</summary>
    public static event Action? Changed;

    public static void Init(Application app)
    {
        Apply(app, ReadIsDark());
        SystemEvents.UserPreferenceChanged += (_, e) =>
        {
            if (e.Category != UserPreferenceCategory.General) return;
            app.Dispatcher.Invoke(() =>
            {
                bool dark = ReadIsDark();
                if (dark != IsDark) Apply(app, dark);
            });
        };
    }

    static bool ReadIsDark() => Registry.GetValue(PersonalizeKey, "AppsUseLightTheme", 1) is int v && v == 0;

    static void Apply(Application app, bool dark)
    {
        var palette = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Themes/{(dark ? "Dark" : "Light")}.xaml"),
        };
        var merged = app.Resources.MergedDictionaries;
        if (merged.Count > 0) merged[0] = palette;
        else merged.Add(palette);
        IsDark = dark;
        foreach (Window w in app.Windows) ApplyTitleBar(w);
        Changed?.Invoke();
    }

    /// <summary>Dark or light caption bar to match (Windows 10 20H1 and later; ignored elsewhere).</summary>
    public static void ApplyTitleBar(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;
        int on = IsDark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int));
    }

    const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
