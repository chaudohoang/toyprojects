using System;
using System.IO;
using System.Linq;
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

            // A saved spot may be half off a screen now (resolution changed, monitor unplugged):
            // pull it in again whenever the screen layout changes (and at start, see OnSourceInitialized).
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s2, e) =>
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => KeepOnScreen()));

            // The taskbar is topmost too and comes to the front whenever it is clicked, which
            // would bury an icon sitting on it: see TaskbarZ. Acts only while the icon overlaps a
            // taskbar, so it never fights other topmost windows elsewhere.
            TaskbarZ.Keep(this, OverTaskbar);

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
        void DefaultPosition_Click(object sender, RoutedEventArgs e) => MoveToMainScreen();

        // Also the tray icon's "Bring icon back here": the way back when the icon sits on a
        // monitor that is hard to reach, where its own menu can't be.
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

        // Moves the icon fully onto the monitor it is on, or the nearest one. Without it the icon
        // could be dragged off-screen with no way to grab it again. The whole screen counts,
        // taskbar included: the icon may sit on the taskbar (it fits in it), never partly off.
        public void KeepOnScreen()
        {
            var h = new WindowInteropHelper(this).Handle;
            var src = PresentationSource.FromVisual(this);
            if (h == IntPtr.Zero || src?.CompositionTarget == null) return;
            var px = System.Windows.Forms.Screen.FromHandle(h).Bounds;    // device pixels, nearest monitor
            var toDip = src.CompositionTarget.TransformFromDevice;
            var tl = toDip.Transform(new Point(px.Left, px.Top));
            var br = toDip.Transform(new Point(px.Right, px.Bottom));
            Left = Math.Max(tl.X, Math.Min(Left, br.X - Width));
            Top = Math.Max(tl.Y, Math.Min(Top, br.Y - Height));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // A dialog is open on the panel (copy report, a question): bring it back instead of
            // toggling, which would hide the panel under the dialog. See MainWindow.Modal.
            if (panel.InModal) { panel.FocusModal(); return; }
            // A click toggles at once. The second click of a double-click is ignored, so a
            // double-click acts like one click instead of opening and closing the panel.
            if (e.ClickCount >= 2) return;

            double startLeft = Left, startTop = Top;
            DragMove(); // returns on mouse up

            if (Math.Abs(Left - startLeft) < 3 && Math.Abs(Top - startTop) < 3)
                panel.Toggle();
            else
            {
                KeepOnScreen();   // dragged (partly) off the screen: snap back onto the nearest one
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
            KeepOnScreen();   // needs the handle to find the monitor
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
            if (panel.InModal) { panel.FocusModal(); ShowToast("Close the open dialog first, then drop it again."); return; }
            // Several folders from one Explorer window: copied together, like a package of them.
            var dirs = paths.Where(Directory.Exists).Select(d => d.TrimEnd('\\')).ToArray();
            if (dirs.Length > 1)
            {
                var parents = dirs.Select(d => Path.GetDirectoryName(d)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (parents.Count == 1 && parents[0] != null)
                {
                    panel.OpenFolder(parents[0], dirs.Select(Path.GetFileName).ToArray());
                    return;
                }
                ShowToast("Those folders are in different places: opened the first one only.");
            }
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

        // True when part of the icon lies outside its monitor's work area, i.e. on the taskbar.
        bool OverTaskbar()
        {
            var h = new WindowInteropHelper(this).Handle;
            if (h == IntPtr.Zero || !IsVisible) return false;
            var wa = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;
            return TaskbarZ.GetWindowRect(h, out var r) && (r.L < wa.Left || r.T < wa.Top || r.R > wa.Right || r.B > wa.Bottom);
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

        // While a dialog is open, menu items that would hide the panel or open a second dialog
        // bring the open one back instead.
        void Toggle_Click(object sender, RoutedEventArgs e) { if (panel.InModal) panel.FocusModal(); else panel.Toggle(); }
        void RightClick_Click(object sender, RoutedEventArgs e) => ShowToast(panel.ToggleExplorerMenu());
        void OpenFolder_Click(object sender, RoutedEventArgs e) => panel.OpenInExplorer();
        void Migrate_Click(object sender, RoutedEventArgs e) { if (panel.InModal) panel.FocusModal(); else panel.ShowMigrate(); }
        void Exit_Click(object sender, RoutedEventArgs e) => Program.Exit();
    }
}
