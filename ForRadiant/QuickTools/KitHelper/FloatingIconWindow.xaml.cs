using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KitHelper
{
    // The always-on-top round icon (modelled on CharlieTheSlave's). It is the app: the panel is
    // only shown on demand.
    //   click        show / hide the panel (it also hides itself when focus goes elsewhere)
    //   double-click install + add "Open in Kit Helper" to the folder right-click menu
    //   drag         move (position remembered)
    //   drop folder  open it in the panel
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
            {
                var wa = SystemParameters.WorkArea;
                Left = wa.Right - Width - 24;
                Top = wa.Bottom - Height - 24;
            }

            toastTimer.Tick += (s2, e) => { toastTimer.Stop(); Toast.IsOpen = false; };

            panel.BusyChanged += SetBusy;
            Drop += OnDrop;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // A click toggles at once. A double-click's first click has therefore already
            // toggled the panel; the second only adds the right-click entry. Waiting out the
            // double-click time to tell them apart made every single click feel ~0.5 s late.
            if (e.ClickCount == 2)
            {
                ShowToast(panel.AddRightClickEntry());
                return;
            }

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

        void OnDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] paths) || paths.Length == 0) return;
            string p = paths[0];
            string folder = Directory.Exists(p) ? p : File.Exists(p) ? Path.GetDirectoryName(p) : null;
            if (folder == null) ShowToast("Drop a kit folder here to open it.");
            else panel.OpenFolder(folder);
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
