using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CharlieTheSlave;

public partial class FloatingIconWindow : Window
{
    readonly PromptWindow _prompt;

    public FloatingIconWindow(PromptWindow prompt)
    {
        InitializeComponent();
        _prompt = prompt;

        var s = AppSettings.Current;
        if (s.IconLeft is double l && s.IconTop is double t && IsOnScreen(l, t))
        {
            Left = l;
            Top = t;
        }
        else
            MoveToDefault();

        // A saved spot may be half off a screen now (resolution changed, monitor unplugged):
        // pull it in once the window has a handle to find its monitor with, and again whenever
        // the screen layout changes.
        SourceInitialized += (_, _) => KeepOnScreen();
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_, _) =>
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(KeepOnScreen));

        _prompt.BusyChanged += SetBusy;
    }

    // First-start spot: bottom right of the main screen.
    void MoveToDefault()
    {
        var wa = SystemParameters.WorkArea;
        Left = wa.Right - Width - 24;
        Top = wa.Bottom - Height - 24;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        double startLeft = Left, startTop = Top;
        DragMove(); // returns on mouse up

        if (Math.Abs(Left - startLeft) < 3 && Math.Abs(Top - startTop) < 3)
        {
            TogglePrompt();
        }
        else
        {
            KeepOnScreen();   // dragged (partly) off the screen: snap back onto the nearest one
            AppSettings.Current.IconLeft = Left;
            AppSettings.Current.IconTop = Top;
            AppSettings.Current.Save();
            if (_prompt.IsVisible) _prompt.ShowNear(this);
        }
    }

    // Moves the icon fully inside the work area (screen minus taskbar) of the monitor it is on,
    // or the nearest one. Without it the icon could be dragged off-screen with no way to grab it.
    public void KeepOnScreen()
    {
        var h = new WindowInteropHelper(this).Handle;
        var src = PresentationSource.FromVisual(this);
        if (h == IntPtr.Zero || src?.CompositionTarget == null) return;
        var px = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;    // device pixels, nearest monitor
        var toDip = src.CompositionTarget.TransformFromDevice;
        var tl = toDip.Transform(new Point(px.Left, px.Top));
        var br = toDip.Transform(new Point(px.Right, px.Bottom));
        Left = Math.Max(tl.X, Math.Min(Left, br.X - Width));
        Top = Math.Max(tl.Y, Math.Min(Top, br.Y - Height));
    }

    // The tray icon's "Bring icon back here": back to the first-start spot on the main screen,
    // forgetting the dragged one. The way back when the icon sits on a monitor hard to reach.
    public void MoveToMainScreen()
    {
        MoveToDefault();
        AppSettings.Current.IconLeft = null;
        AppSettings.Current.IconTop = null;
        AppSettings.Current.Save();
        Activate();
        if (_prompt.IsVisible) _prompt.ShowNear(this);
    }

    public void TogglePrompt()
    {
        if (_prompt.IsVisible) _prompt.Hide();
        else _prompt.ShowNear(this);
    }

    void SetBusy(bool busy)
    {
        BusyRing.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (busy)
            BusyRotation.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(0, 360, TimeSpan.FromSeconds(2)) { RepeatBehavior = RepeatBehavior.Forever });
        else
            BusyRotation.BeginAnimation(RotateTransform.AngleProperty, null);
    }

    static bool IsOnScreen(double left, double top) =>
        left >= SystemParameters.VirtualScreenLeft - 10 &&
        top >= SystemParameters.VirtualScreenTop - 10 &&
        left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 30 &&
        top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 30;

    void Open_Click(object sender, RoutedEventArgs e) => _prompt.ShowNear(this);

    void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
