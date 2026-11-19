using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KitHelper
{
    // The always-on-top round icon (modelled on CharlieTheSlave's). It is the app: the panel is
    // only shown on demand.
    //   click        show / hide the panel (it also hides itself when focus goes elsewhere)
    //   drag         move (position remembered)
    //   drop         a folder opens it in the panel; a run-input .txt opens its folder and
    //                selects it. The icon is the only drop target: starting a drag in Explorer
    //                deactivates the panel, which then hides.
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

            toastTimer.Tick += (s2, e) => { toastTimer.Stop(); Toast.IsOpen = false; };

            panel.BusyChanged += SetBusy;
            Drop += OnDrop;
        }

        // First-start position: top centre of the main screen. Not top right, where it would
        // cover the close / minimize buttons of maximized windows (the icon is always on top).
        void MoveToDefault()
        {
            var wa = SystemParameters.WorkArea;
            Left = wa.Left + (wa.Width - Width) / 2;
            Top = wa.Top + 4;
        }

        // Menu "Default position": back to the first-start spot, and forget the dragged one.
        void DefaultPosition_Click(object sender, RoutedEventArgs e)
        {
            MoveToDefault();
            Settings.Current.Remove("IconLeft");
            Settings.Current.Remove("IconTop");
            Settings.Current.Save();
            if (panel.IsVisible) panel.ShowNear(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // A click toggles at once. The second click of a double-click is ignored, so a
            // double-click acts like one click instead of opening and closing the panel.
            if (e.ClickCount >= 2) return;

            double startLeft = Left, startTop = Top;
            DragMove(); // returns on mouse up

            if (Math.Abs(Left - startLeft) < 3 && Math.Abs(Top - startTop) < 3)
                panel.Toggle();
            else
            {
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

        // Run as administrator, Explorer cannot OLE-drag into this window: see AdminDrop.
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (!AdminDrop.IsElevated) return;
            var src = (HwndSource)PresentationSource.FromVisual(this);
            AdminDrop.Enable(src, OpenDropped);
            Loaded += (s, e2) => Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(() => AdminDrop.Use(src.Handle)));
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths) OpenDropped(paths);
        }

        void OpenDropped(string[] paths)
        {
            if (paths == null || paths.Length == 0) return;
            string p = paths[0];
            if (Directory.Exists(p)) panel.OpenFolder(p);
            else if (File.Exists(p)) panel.OpenFile(p);
            else ShowToast("Drop a kit folder or a run-input .txt here to open it.");
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

        void Menu_Opened(object sender, RoutedEventArgs e)
        {
            MenuToggle.Header = panel.IsVisible ? "Hide panel" : "Show panel";
            MenuRightClick.Header = panel.MenuPointsAtInstalled()
                ? "Remove \"Open in Kit Helper\" from right-click"
                : "Add \"Open in Kit Helper\" to right-click";
        }

        void Toggle_Click(object sender, RoutedEventArgs e) => panel.Toggle();
        void RightClick_Click(object sender, RoutedEventArgs e) => ShowToast(panel.ToggleExplorerMenu());
        void OpenFolder_Click(object sender, RoutedEventArgs e) => panel.OpenInExplorer();
        void Migrate_Click(object sender, RoutedEventArgs e) => panel.ShowMigrate();
        void Exit_Click(object sender, RoutedEventArgs e) => Program.Exit();
    }
}
