using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CharlieTheSlave;

/// <summary>
/// The on-screen warning shown while an automation run drives the mouse and keyboard, with the key
/// that stops it. It sits above everything and is deliberately inert: WS_EX_NOACTIVATE keeps it from
/// ever taking the foreground (which would close the pop-up menu the run is in the middle of reading)
/// and WS_EX_TRANSPARENT lets clicks fall through to whatever is underneath.
/// </summary>
public partial class AutomationBanner : Window
{
    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x00000020, WS_EX_TOOLWINDOW = 0x00000080, WS_EX_NOACTIVATE = 0x08000000;

    public AutomationBanner()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Reposition();
    }

    /// <summary>Replaces the progress line, e.g. "Measurement 3 of 20: W192".</summary>
    public void Report(string text) => Status.Text = text;

    /// <summary>
    /// Puts a one-off warning on screen in the same place as the running banner, pulses it so it is hard
    /// to miss, and takes it away again. For things the user has to go and fix before trying once more.
    /// </summary>
    public static void Flash(string headline, string detail, int seconds = 8)
    {
        var banner = new AutomationBanner();
        banner.Headline.Text = headline;
        banner.Status.Text = detail;
        banner.HotkeyBox.Visibility = Visibility.Collapsed; // nothing is running, so there's nothing to stop
        banner.Show();
        banner.BeginAnimation(OpacityProperty, new DoubleAnimation(1.0, 0.3, TimeSpan.FromMilliseconds(350))
        {
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(3),
        });

        var life = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
        life.Tick += (_, _) =>
        {
            life.Stop();
            banner.Close();
        };
        life.Start();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowLong(hwnd, GWL_EXSTYLE,
            GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        Reposition();
    }

    /// <summary>Centred near the top of the working area, clear of where TrueTest's grids and menus sit.</summary>
    void Reposition()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Left + (area.Width - ActualWidth) / 2;
        Top = area.Top + 28;
    }

    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hwnd, int index, int value);
}
