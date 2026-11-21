using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace QuickLauncher
{
    // The always-on-top round icon, same behaviour as Kit Helper's K:
    //   click  show / hide the panel     drag  move (remembered)
    //   drop   a game exe adds it; a folder adds it as a game folder (root) and rescans
    public partial class FloatingIconWindow : Window
    {
        readonly MainWindow panel;
        readonly DispatcherTimer toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };

        public FloatingIconWindow(MainWindow panel)
        {
            InitializeComponent();
            this.panel = panel;

            var s = Settings.Current;
            if (s.GetDouble("IconLeft") is double l && s.GetDouble("IconTop") is double t && IsOnScreen(l, t))
            {
                Left = l;
                Top = t;
            }
            else
                MoveToDefault();

            // The saved spot may now be half off a screen (resolution changed, monitor unplugged):
            // pull it in once the window has a handle to find its monitor with, and again whenever
            // the screen layout changes.
            SourceInitialized += (s2, e) => KeepOnScreen();

            // The taskbar is topmost too and comes to the front whenever it is clicked, which
            // would bury an icon sitting on it. While the icon overlaps a taskbar, put it back on
            // top every second (only then, so it never fights other topmost windows elsewhere).
            var onTop = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            onTop.Tick += (s2, e) => { if (OverTaskbar()) TaskbarZ.BringToTop(this); };
            onTop.Start();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s2, e) =>
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => KeepOnScreen()));

            toastTimer.Tick += (s2, e) => { toastTimer.Stop(); Toast.IsOpen = false; };
            panel.BusyChanged += SetBusy;
            panel.Notify += ShowToast;
            Drop += (s2, e) => { if (e.Data.GetData(DataFormats.FileDrop) is string[] p) panel.AddDropped(p); };
        }

        // Top of the main screen, a little right of centre so it does not sit on Kit Helper's K.
        void MoveToDefault()
        {
            var wa = SystemParameters.WorkArea;
            Left = wa.Left + (wa.Width - Width) / 2 + 70;
            Top = wa.Top + 4;
        }

        void DefaultPosition_Click(object sender, RoutedEventArgs e) => MoveToMainScreen();

        // Back to the first-start spot on the main screen, forgetting the dragged one. Used by
        // the menu here and by the tray icon's "Bring icon back here".
        public void MoveToMainScreen()
        {
            MoveToDefault();
            Settings.Current.Remove("IconLeft");
            Settings.Current.Remove("IconTop");
            Settings.Current.Save();
            Activate();
            ShowToast("Here I am. Drag me wherever you like.");
            if (panel.IsVisible) panel.ShowNear(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ClickCount >= 2) return;

            double startLeft = Left, startTop = Top;
            DragMove(); // returns on mouse up

            if (Math.Abs(Left - startLeft) < 3 && Math.Abs(Top - startTop) < 3)
                panel.Toggle();
            else
            {
                // Dragged (partly) off the screen: snap back fully onto the nearest one, or it
                // could end up where it can no longer be grabbed.
                KeepOnScreen();
                Settings.Current.SetDouble("IconLeft", Left);
                Settings.Current.SetDouble("IconTop", Top);
                Settings.Current.Save();
                if (panel.IsVisible) panel.ShowNear(this);
            }
        }

        public void ShowToast(string text)
        {
            ToastText.Text = text;
            Toast.IsOpen = false;   // re-open so the popup re-measures for the new text
            Toast.IsOpen = true;
            toastTimer.Stop();
            toastTimer.Start();
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

        // Moves the icon fully onto the monitor it is on, or the nearest one if it is on none.
        // The whole screen counts, taskbar included: the icon may sit on the taskbar (it is
        // small enough to fit in it), just never partly off a screen. True if it had to move.
        public bool KeepOnScreen()
        {
            var h = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var src = PresentationSource.FromVisual(this);
            if (h == IntPtr.Zero || src?.CompositionTarget == null) return false;
            var px = System.Windows.Forms.Screen.FromHandle(h).Bounds;    // device pixels, nearest monitor
            var toDip = src.CompositionTarget.TransformFromDevice;
            var tl = toDip.Transform(new Point(px.Left, px.Top));
            var br = toDip.Transform(new Point(px.Right, px.Bottom));
            double l = Math.Max(tl.X, Math.Min(Left, br.X - Width));
            double t = Math.Max(tl.Y, Math.Min(Top, br.Y - Height));
            if (Math.Abs(l - Left) < 0.5 && Math.Abs(t - Top) < 0.5) return false;
            Left = l;
            Top = t;
            return true;
        }

        // True when part of the icon lies outside its monitor's work area, i.e. on the taskbar.
        bool OverTaskbar()
        {
            var h = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (h == IntPtr.Zero || !IsVisible) return false;
            var wa = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;
            return TaskbarZ.GetWindowRect(h, out var r) && (r.L < wa.Left || r.T < wa.Top || r.R > wa.Right || r.B > wa.Bottom);
        }

        static bool IsOnScreen(double left, double top) =>
            left >= SystemParameters.VirtualScreenLeft - 10 &&
            top >= SystemParameters.VirtualScreenTop - 10 &&
            left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 30 &&
            top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 30;

        // Right-click → Recent: the last few games played, one click to start.
        void Menu_Opened(object sender, RoutedEventArgs e)
        {
            MenuToggle.Header = panel.IsVisible ? "Hide panel" : "Show panel";
            MenuRecent.Items.Clear();
            foreach (var g in panel.Recent(8))
            {
                var mi = new MenuItem { Header = g.Name.Replace("_", "__"), Icon = new Image { Source = g.Icon, Width = 16, Height = 16 } };
                mi.Click += (s2, e2) => panel.Launch(g, false);
                MenuRecent.Items.Add(mi);
            }
            MenuRecent.IsEnabled = MenuRecent.Items.Count > 0;
        }

        void Toggle_Click(object sender, RoutedEventArgs e) => panel.Toggle();
        void Rescan_Click(object sender, RoutedEventArgs e) => panel.RescanAll();
        void Exit_Click(object sender, RoutedEventArgs e) => Program.Exit();
    }
}
