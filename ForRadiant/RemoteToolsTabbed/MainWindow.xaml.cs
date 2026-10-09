using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace RemoteToolsTabbed
{
    public static class AppCommands
    {
        public static readonly RoutedUICommand CloseTab =
            new RoutedUICommand("Close session", "CloseTab", typeof(AppCommands));

        public static readonly RoutedUICommand FocusSearch =
            new RoutedUICommand("Focus search", "FocusSearch", typeof(AppCommands));

        public static readonly RoutedUICommand ToggleSidebar =
            new RoutedUICommand("Show or hide the machine list", "ToggleSidebar", typeof(AppCommands));
    }

    public partial class MainWindow : Window
    {
        private const int ConnectTimeoutMs = 60000;
        private const double DefaultSidebarWidth = 340;

        /// <summary>
        /// Narrower than this and the action buttons and the IP column start getting clipped.
        /// It is applied to the column, not to the Border inside it: the splitter resizes the
        /// column, so a minimum on the Border only lets the content be cut off instead.
        /// </summary>
        private const double MinSidebarWidth = 260;

        private readonly ObservableCollection<VncSession> _sessions = new ObservableCollection<VncSession>();
        private readonly HashSet<IntPtr> _claimedWindows = new HashSet<IntPtr>();
        private readonly DispatcherTimer _watchdog = new DispatcherTimer();
        private double _sidebarWidth = DefaultSidebarWidth;
        private ComputersView _computersView;
        private SidebarWindow _sidebarWindow;       // used by the flyout and the floating mode
        private SidebarMode _sidebarMode = SidebarMode.Docked;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = string.Format("{0} - {1}", Title, version);

            _computersView = new ComputersView();
            _computersView.ConnectRequested += (s, targets) => OpenSessions(targets);
            _computersView.StatusChanged += (s, message) => SetStatus(message);
            _computersView.ViewerOptionsRequested += ViewerOptions_Click;
            _computersView.CollapseRequested += (s, args) => ToggleSidebar();
            SidebarHost.Content = _computersView;

            Tabs.ItemsSource = _sessions;
            UpdateSessionCount();

            if (!File.Exists(Paths.Viewer))
                SetStatus("tvnviewer.exe is missing from " + Paths.AppFolder + " - remote control is unavailable.");
            else if (ViewerProfile.Exists)
                SetStatus("Double-click a machine, or select several and press Remote Control.  Using viewer-profile.vnc.");
            else
                // Worth saying: without a profile every machine falls back to whatever TightVNC
                // has stored for that host, which is why options set through the gear appear to
                // have no effect - the gear writes a profile this folder does not have.
                SetStatus("Double-click a machine, or select several and press Remote Control.  " +
                          "No viewer-profile.vnc here - each machine uses TightVNC's own stored settings.");

            ViewerProfile.CleanupStaleSessionFiles();

            _watchdog.Interval = TimeSpan.FromSeconds(1);
            _watchdog.Tick += Watchdog_Tick;
            _watchdog.Start();

            _computersView.FocusSearch();
        }

        /// <summary>
        /// The machine total moves when a CSV is added or unloaded, and it is shown in two
        /// places - the sidebar header, and the floating list's own title bar.
        /// </summary>
        // ---------------------------------------------------------------- sessions

        private void OpenSessions(IReadOnlyList<Computer> targets)
        {
            if (!File.Exists(Paths.Viewer))
            {
                MessageBox.Show(this, "tvnviewer.exe was not found in" + Environment.NewLine + Paths.AppFolder,
                    "Remote Tools", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (Computer target in targets)
            {
                VncSession existing = _sessions.FirstOrDefault(
                    s => string.Equals(s.Address, target.Address, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                    Tabs.SelectedItem = existing;    // already open - just show it
                else
                    OpenSession(target.Name, target.Address);
            }
        }

        private async void OpenSession(string name, string address)
        {
            VncSession session = new VncSession(name, address);
            session.CloseRequested += (s, e) => CloseSession((VncSession)s);

            _sessions.Add(session);
            Tabs.SelectedItem = session;
            UpdateSessionCount();

            bool connected = await session.ConnectAsync(Paths.Viewer, _claimedWindows, ConnectTimeoutMs);

            if (connected && ReferenceEquals(Tabs.SelectedItem, session))
            {
                session.Host.ResizeChild();
                session.Host.FocusChild();
            }

            SetStatus(connected
                ? "Connected to " + address
                : "Could not open a session for " + address);
        }

        private void CloseSession(VncSession session)
        {
            if (session == null)
                return;

            if (session.Host.ChildHandle != IntPtr.Zero)
                _claimedWindows.Remove(session.Host.ChildHandle);

            session.Close();
            RemoveSession(session);
        }

        private void RemoveSession(VncSession session)
        {
            _sessions.Remove(session);      // the tab control drops its presenter with it
            session.Dispose();
            UpdateSessionCount();

            if (Tabs.SelectedItem == null && _sessions.Count > 0)
                Tabs.SelectedIndex = _sessions.Count - 1;
        }

        private VncSession CurrentSession
        {
            get { return Tabs.SelectedItem as VncSession; }
        }

        /// <summary>Drops tabs whose viewer window went away on its own (disconnect, quit, crash).</summary>
        private void Watchdog_Tick(object sender, EventArgs e)
        {
            for (int i = _sessions.Count - 1; i >= 0; i--)
            {
                VncSession session = _sessions[i];
                if (!session.IsWindowGone)
                    continue;

                _claimedWindows.Remove(session.Host.ChildHandle);
                SetStatus(session.Name + " disconnected.");
                RemoveSession(session);
            }
        }

        private void SetStatus(string message)
        {
            StatusText.Text = message;
        }

        private void UpdateSessionCount()
        {
            SessionCount.Text = _sessions.Count == 0
                ? string.Empty
                : string.Format("{0} session{1} open", _sessions.Count, _sessions.Count == 1 ? "" : "s");

            bool any = _sessions.Count > 0;
            EmptyState.Visibility = any ? Visibility.Collapsed : Visibility.Visible;
            Tabs.Visibility = any ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------------------------------------------------------------- viewer options

        /// <summary>
        /// Opens tvnviewer on its own so its "Connection Options" dialog can be used, then
        /// keeps whatever was set there as the profile every session starts from.
        ///
        /// It has to be done this way round: options typed into a *running* session are not
        /// written back, while the ones set before connecting are stored immediately - so the
        /// viewer's own pre-connection dialog is the thing worth capturing.
        /// </summary>
        private async void ViewerOptions_Click(object sender, EventArgs e)
        {
            // Both the dialog below and tvnviewer after it take the focus. The gear travels
            // with the list, so it can be pressed while the list is slid out over a session -
            // and a flyout that auto-hid here would vanish the moment the first box appeared.
            SidebarWindow flyout = _sidebarWindow;
            if (flyout != null)
                flyout.AutoHideSuspended = true;

            try
            {
                await CaptureViewerOptionsAsync();
            }
            finally
            {
                if (flyout != null)
                    flyout.AutoHideSuspended = false;
            }
        }

        private async Task CaptureViewerOptionsAsync()
        {
            if (!File.Exists(Paths.Viewer))
            {
                MessageBox.Show(this, "tvnviewer.exe was not found in" + Environment.NewLine + Paths.AppFolder,
                    "Remote Tools", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult go = MessageBox.Show(this,
                "TightVNC Viewer will open." + Environment.NewLine + Environment.NewLine +
                "Press \"Options...\", set them the way you want, press OK, then close the viewer." +
                Environment.NewLine + Environment.NewLine +
                "Those options are then used for every session this tool opens.",
                "Viewer options", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (go != MessageBoxResult.OK)
                return;

            _computersView.ViewerOptionsEnabled = false;
            SetStatus("Set the options in TightVNC Viewer, then close it ...");

            try
            {
                await RunViewerForOptionsAsync();

                int captured = ViewerProfile.CaptureFromViewerDefaults();
                SetStatus(captured > 0
                    ? "Viewer options saved - every new session will use them (" + ViewerProfile.ProfilePath + ")"
                    : "TightVNC Viewer had no options stored, so nothing was saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not save the viewer options:" + Environment.NewLine + ex.Message,
                    "Remote Tools", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _computersView.ViewerOptionsEnabled = true;
            }
        }

        private static Task RunViewerForOptionsAsync()
        {
            TaskCompletionSource<bool> finished = new TaskCompletionSource<bool>();

            Process viewer = new Process
            {
                StartInfo = new ProcessStartInfo(Paths.Viewer)
                {
                    UseShellExecute = false,
                    WorkingDirectory = Paths.AppFolder
                },
                EnableRaisingEvents = true
            };

            viewer.Exited += (s, e) =>
            {
                viewer.Dispose();
                finished.TrySetResult(true);
            };

            viewer.Start();
            return finished.Task;
        }

        // ---------------------------------------------------------------- sidebar

        /// <summary>
        /// Docked    - a column of the main window.
        /// Collapsed - a thin strip; clicking it slides the list out.
        /// Flyout    - slid out over the session in a window of its own, tucking away when it
        ///             loses focus, the way a Visual Studio tool window does.
        /// </summary>
        private enum SidebarMode
        {
            Docked,
            Collapsed,
            Flyout
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            ToggleSidebar();
        }

        private void ToggleSidebarCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleSidebar();
        }

        /// <summary>Clicking the strip slides the list out over the session, and back.</summary>
        /// <summary>
        /// The collapsed strip: slides the list out over the session, and puts it away again.
        /// Handled on the press rather than a click, so that it beats the flyout's own
        /// deferred auto-hide to the decision - taken afterwards it only ever sees a list that
        /// has already closed itself, and opens it straight back up.
        /// </summary>
        private void CollapsedSidebar_Click(object sender, MouseButtonEventArgs e)
        {
            SetSidebarMode(_sidebarMode == SidebarMode.Flyout ? SidebarMode.Collapsed : SidebarMode.Flyout);
            e.Handled = true;
        }

        /// <summary>Ctrl+B: put the list away, or bring it back into the window.</summary>
        private void ToggleSidebar()
        {
            SetSidebarMode(_sidebarMode == SidebarMode.Docked ? SidebarMode.Collapsed : SidebarMode.Docked);
        }

        private void ShowSidebar()
        {
            if (_sidebarMode != SidebarMode.Docked)
                SetSidebarMode(SidebarMode.Docked);
        }

        private void SetSidebarMode(SidebarMode mode)
        {
            if (mode == _sidebarMode)
                return;

            if (_sidebarMode == SidebarMode.Docked)
                _sidebarWidth = Math.Max(SidebarColumn.ActualWidth, MinSidebarWidth);

            _sidebarMode = mode;

            switch (mode)
            {
                case SidebarMode.Docked:
                    CloseFlyout();                  // takes the list back with it
                    Sidebar.Visibility = Visibility.Visible;
                    CollapsedSidebar.Visibility = Visibility.Collapsed;
                    SidebarSplitter.Visibility = Visibility.Visible;
                    SidebarColumn.MinWidth = MinSidebarWidth;   // what stops the splitter
                    SidebarColumn.Width = new GridLength(Math.Max(_sidebarWidth, MinSidebarWidth));
                    break;

                case SidebarMode.Collapsed:
                    // Hide the flyout, do not destroy it. Tearing a window down from inside its
                    // own deactivation is what used to drop the whole application out of focus.
                    if (_sidebarWindow != null)
                        _sidebarWindow.Hide();

                    Sidebar.Visibility = Visibility.Collapsed;
                    CollapsedSidebar.Visibility = Visibility.Visible;
                    SidebarSplitter.Visibility = Visibility.Collapsed;
                    SidebarColumn.MinWidth = 0;                 // let it shrink to the strip
                    SidebarColumn.Width = GridLength.Auto;
                    break;

                case SidebarMode.Flyout:
                    Sidebar.Visibility = Visibility.Collapsed;
                    CollapsedSidebar.Visibility = Visibility.Visible;   // the strip stays under it
                    SidebarSplitter.Visibility = Visibility.Collapsed;
                    SidebarColumn.MinWidth = 0;
                    SidebarColumn.Width = GridLength.Auto;
                    OpenFlyout();
                    break;
            }
        }

        private void OpenFlyout()
        {
            if (_sidebarWindow == null)
            {
                SidebarHost.Content = null;     // an element lives in one visual tree at a time

                _sidebarWindow = new SidebarWindow(true)
                {
                    Owner = this,
                    Title = string.Format("Computers ({0})", _computersView.MachineCount)
                };
                    // Both arrive deferred, so check they still come from the window we are using:
                // a request from one we have already dropped would undo what replaced it.
                _sidebarWindow.DockRequested += (s, e) =>
                {
                    if (ReferenceEquals(s, _sidebarWindow))
                        SetSidebarMode(SidebarMode.Docked);
                };
                _sidebarWindow.AutoHideRequested += (s, e) =>
                {
                    if (ReferenceEquals(s, _sidebarWindow))
                        SetSidebarMode(SidebarMode.Collapsed);
                };
                _sidebarWindow.Host.Content = _computersView;
            }

            _computersView.ShowCollapseButton = false;

            PlaceFlyout(_sidebarWindow);
            _sidebarWindow.Show();
            _sidebarWindow.Activate();          // a flyout that never had focus can never lose it
            _computersView.FocusSearch();
        }

        private void CloseFlyout()
        {
            if (_sidebarWindow == null)
                return;

            SidebarWindow window = _sidebarWindow;
            _sidebarWindow = null;              // before closing, so handlers do not re-enter

            window.Host.Content = null;
            window.CloseForGood();
            SidebarHost.Content = _computersView;
            _computersView.ShowCollapseButton = true;
        }

        /// <summary>Flush against the collapsed strip, as tall as the strip itself.</summary>
        private void PlaceFlyout(SidebarWindow window)
        {
            Point topRight = CollapsedSidebar.PointToScreen(new Point(CollapsedSidebar.ActualWidth, 0));

            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
                topRight = source.CompositionTarget.TransformFromDevice.Transform(topRight);

            window.Width = Math.Max(Math.Max(_sidebarWidth, MinSidebarWidth), window.MinWidth);
            window.Height = Math.Max(CollapsedSidebar.ActualHeight, window.MinHeight);
            window.Left = topRight.X;
            window.Top = topRight.Y;
        }

        // ---------------------------------------------------------------- tab chrome

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, Tabs))
                return;     // a selection inside a hosted control, not a tab change

            VncSession session = CurrentSession;
            if (session == null || !session.Host.HasChild)
                return;

            // a hidden tab gets no layout, so re-fit the window now that it is visible
            Dispatcher.BeginInvoke(new Action(() =>
            {
                session.Host.ResizeChild();
                session.Host.FocusChild();
            }), DispatcherPriority.Background);
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            CloseSession(((FrameworkElement)sender).DataContext as VncSession);
        }

        private void TabHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            CloseSession(((FrameworkElement)sender).DataContext as VncSession);
            e.Handled = true;
        }

        private void SessionCancel_Click(object sender, RoutedEventArgs e)
        {
            VncSession session = ((FrameworkElement)sender).DataContext as VncSession;
            if (session != null)
                session.Cancel();
        }

        private void MenuCloseTab_Click(object sender, RoutedEventArgs e)
        {
            CloseSession(((FrameworkElement)sender).DataContext as VncSession);
        }

        private void MenuCloseOthers_Click(object sender, RoutedEventArgs e)
        {
            VncSession keep = ((FrameworkElement)sender).DataContext as VncSession;
            foreach (VncSession session in _sessions.ToArray())
            {
                if (!ReferenceEquals(session, keep))
                    CloseSession(session);
            }
        }

        private void MenuPopOut_Click(object sender, RoutedEventArgs e)
        {
            VncSession session = ((FrameworkElement)sender).DataContext as VncSession;
            if (session == null)
                return;

            IntPtr hWnd = session.Detach();
            if (hWnd != IntPtr.Zero)
                _claimedWindows.Remove(hWnd);

            string name = session.Name;
            RemoveSession(session);
            SetStatus(name + " popped out into its own window.");
        }

        private void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseSession(CurrentSession);
        }

        private void FocusSearchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowSidebar();
            _computersView.FocusSearch();
        }

        // ---------------------------------------------------------------- shutdown

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseFlyout();
            _watchdog.Stop();
            if (_computersView != null)
                _computersView.StopPing();

            foreach (VncSession session in _sessions.ToArray())
            {
                session.Close();
                _sessions.Remove(session);
            }
        }
    }
}
