using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        {
            var wa = SystemParameters.WorkArea;
            Left = wa.Right - Width - 24;
            Top = wa.Bottom - Height - 24;
        }

        _prompt.BusyChanged += SetBusy;
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
            AppSettings.Current.IconLeft = Left;
            AppSettings.Current.IconTop = Top;
            AppSettings.Current.Save();
            if (_prompt.IsVisible) _prompt.ShowNear(this);
        }
    }

    void TogglePrompt()
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
