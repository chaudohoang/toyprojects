using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// The machine list slid out over the session: the Visual Studio auto-hide behaviour.
    /// It comes out of the collapsed strip and tucks away again as soon as it loses focus.
    ///
    /// It has to be a real top-level window. A docked session is a child HWND
    /// belonging to tvnviewer, and a window handle always paints over WPF content, so an
    /// overlay inside the main window would simply disappear behind the session.
    /// </summary>
    public partial class SidebarWindow : Window
    {
        private readonly bool _flyout;
        private readonly DateTime _created = DateTime.UtcNow;
        private bool _reallyClosing;

        public SidebarWindow(bool flyout)
        {
            InitializeComponent();
            _flyout = flyout;

            if (!flyout)
                return;

            // a flyout has no chrome of its own: it is an extension of the strip it came from
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Shell.BorderThickness = new Thickness(0, 1, 1, 1);
            Hint.Text = "Click away to hide";
            Deactivated += Flyout_Deactivated;
        }

        /// <summary>Raised when the user asks for the list back in the main window.</summary>
        public event EventHandler DockRequested;

        /// <summary>Raised when a flyout lost focus and should tuck itself away.</summary>
        public event EventHandler AutoHideRequested;

        /// <summary>
        /// Holds the flyout open across something that has to take the focus away - the viewer
        /// options put up a dialog and then run tvnviewer itself. Without it the list tucks
        /// away the instant the dialog appears and leaves the user looking at the session.
        /// </summary>
        public bool AutoHideSuspended { get; set; }

        public void CloseForGood()
        {
            if (_reallyClosing)
                return;

            _reallyClosing = true;

            try
            {
                Close();
            }
            catch (InvalidOperationException)
            {
                // already on its way out; nothing left to do
            }
        }

        private void DockBack_Click(object sender, RoutedEventArgs e)
        {
            Raise(DockRequested);
        }

        private void Flyout_Deactivated(object sender, EventArgs e)
        {
            // Docking closes this window, and closing deactivates it. Without this the dock
            // would be followed by an auto-hide that collapses the list straight back again.
            if (_reallyClosing)
                return;

            // Opening the window can itself produce a Deactivated before it ever settles - for
            // instance when the app is not the foreground app at the time. Closing on that would
            // make the flyout look like it never opened at all.
            if ((DateTime.UtcNow - _created).TotalMilliseconds < 500)
                return;

            if (AutoHideSuspended)
                return;

            // Hand it back after this deactivation has finished. Changing window state from
            // inside the event is what made the application lose focus altogether.
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_reallyClosing || AutoHideSuspended)
                        return;

                    // A message box opened from the list (Save Lists, File Share errors, the
                    // viewer options) deactivates us too, and tucking away behind it would
                    // strand the dialog. Asked here rather than in the handler: the box
                    // deactivates us *before* it becomes the foreground window, so asking any
                    // earlier only ever sees the window that is going away.
                    if (IsOwnDialogInFront())
                        return;

                    Raise(AutoHideRequested);
                }),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private static bool IsOwnDialogInFront()
        {
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero)
                return false;

            uint pid;
            GetWindowThreadProcessId(foreground, out pid);
            if (pid != (uint)System.Diagnostics.Process.GetCurrentProcess().Id)
                return false;

            StringBuilder className = new StringBuilder(64);
            GetClassName(foreground, className, className.Capacity);
            return className.ToString() == "#32770";     // a Win32 dialog, i.e. a message box
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_reallyClosing)
                return;

            // The X would otherwise throw the list away for the rest of the session, so dock
            // instead - but only once this close has finished unwinding. Docking from here
            // calls Close() again on a window that is already closing, which throws
            // InvalidOperationException out of WmClose and takes the whole application with it.
            e.Cancel = true;
            Dispatcher.BeginInvoke(new Action(() => Raise(DockRequested)),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Raise(EventHandler handler)
        {
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);
    }
}
