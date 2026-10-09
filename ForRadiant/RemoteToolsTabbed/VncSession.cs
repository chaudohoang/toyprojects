using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RemoteToolsTabbed
{
    public enum SessionState
    {
        Connecting,
        Connected,
        Failed,
        Closed
    }

    /// <summary>
    /// One tab: a tvnviewer process plus the host its window gets docked into.
    /// </summary>
    public sealed class VncSession : INotifyPropertyChanged, IDisposable
    {
        public const string ViewerProcessName = "tvnviewer";

        /// <summary>
        /// The only switch ever passed to tvnviewer, with a profile or without one. It
        /// suppresses a popup; it is not a stored setting, so it overrides nothing.
        ///
        /// Nothing else belongs here. **A switch on the command line beats whatever the viewer
        /// has stored**, so anything added becomes an option the user can no longer change.
        /// `-scale=auto` used to be passed whenever there was no profile, which quietly undid
        /// the viewer's own scale setting - and no profile is exactly the state you are in
        /// after deleting one to go back to the viewer's own per-host settings, so the one
        /// documented way of using them was the one case that could not work.
        ///
        /// An unknown switch makes the viewer stop with "Error in format of command line" and
        /// never open a window, so check any addition against tvnviewer -help first.
        /// </summary>
        public const string ViewerSwitches = "-warnfullscr=no";

        private static readonly Brush InfoBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0xD8, 0xD8));
        private static readonly Brush FailBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x7A));

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Process _viewer;
        private string _sessionProfile;
        private string _header;
        private string _statusMessage;
        private string _cancelButtonText = "Cancel";
        private Brush _statusBrush = InfoBrush;
        private bool _isOverlayVisible = true;

        public VncSession(string name, string address)
        {
            Name = string.IsNullOrEmpty(name) ? address : name;
            Address = address;
            State = SessionState.Connecting;
            Host = new ExternalWindowHost();

            _header = Name + "  (" + address + ")";
            _statusMessage = "Connecting to " + address + " ...";
        }

        public string Name { get; private set; }
        public string Address { get; private set; }
        public ExternalWindowHost Host { get; private set; }
        public SessionState State { get; private set; }

        public bool IsClosable
        {
            get { return true; }
        }

        public string Header
        {
            get { return _header; }
            private set { Set(ref _header, value, "Header"); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { Set(ref _statusMessage, value, "StatusMessage"); }
        }

        public string CancelButtonText
        {
            get { return _cancelButtonText; }
            private set { Set(ref _cancelButtonText, value, "CancelButtonText"); }
        }

        public Brush StatusBrush
        {
            get { return _statusBrush; }
            private set { Set(ref _statusBrush, value, "StatusBrush"); }
        }

        /// <summary>The "Connecting ..." / failure panel drawn over the host.</summary>
        public bool IsOverlayVisible
        {
            get { return _isOverlayVisible; }
            private set { Set(ref _isOverlayVisible, value, "IsOverlayVisible"); }
        }

        public bool IsWindowGone
        {
            get
            {
                return State == SessionState.Connected &&
                       (Host.ChildHandle == IntPtr.Zero || !NativeMethods.IsWindow(Host.ChildHandle));
            }
        }

        public event EventHandler CloseRequested;

        /// <summary>
        /// Launches the viewer and waits for its window to show up, then docks it.
        /// Returns false if it never appeared (wrong address, refused, cancelled, ...).
        /// <paramref name="claimed"/> is shared by every session so two connections started
        /// at once cannot both grab the same window; the window we adopt is added to it.
        /// </summary>
        public async Task<bool> ConnectAsync(string viewerExePath, HashSet<IntPtr> claimed, int timeoutMs)
        {
            HashSet<IntPtr> before = new HashSet<IntPtr>();
            foreach (IntPtr h in NativeMethods.FindMainWindows(ViewerProcessName))
                before.Add(h);

            try
            {
                _viewer = Process.Start(new ProcessStartInfo(viewerExePath, BuildArguments())
                {
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(viewerExePath)
                });
            }
            catch (Exception ex)
            {
                Fail("Could not start the viewer:" + Environment.NewLine + ex.Message);
                return false;
            }

            IntPtr window = await WaitForNewWindowAsync(before, claimed, timeoutMs, _cts.Token).ConfigureAwait(true);

            if (window == IntPtr.Zero)
            {
                if (State != SessionState.Closed)
                    Fail(_cts.IsCancellationRequested
                        ? "Cancelled."
                        : "No viewer window appeared for " + Address + "." + Environment.NewLine +
                          "The host may be unreachable, or the connection was refused.");
                return false;
            }

            claimed.Add(window);
            DeleteSessionProfile();     // the viewer has started, it has read the file
            Host.Adopt(window);
            IsOverlayVisible = false;
            State = SessionState.Connected;
            return true;
        }

        private static async Task<IntPtr> WaitForNewWindowAsync(HashSet<IntPtr> before, HashSet<IntPtr> claimed,
            int timeoutMs, CancellationToken token)
        {
            int waited = 0;
            const int step = 150;

            while (waited < timeoutMs && !token.IsCancellationRequested)
            {
                foreach (IntPtr h in NativeMethods.FindMainWindows(ViewerProcessName))
                {
                    if (!before.Contains(h) && !claimed.Contains(h))
                        return h;
                }

                try
                {
                    await Task.Delay(step, token).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                waited += step;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// With a profile the whole connection comes from the file - the viewer ignores a host
        /// given on the command line next to -optionsfile, so the host goes in the file.
        /// </summary>
        private string BuildArguments()
        {
            _sessionProfile = ViewerProfile.CreateSessionFile(Address);

            if (_sessionProfile != null)
                return "-optionsfile=\"" + _sessionProfile + "\" " + ViewerSwitches;

            return Address + " " + ViewerSwitches;
        }

        private void DeleteSessionProfile()
        {
            if (_sessionProfile == null)
                return;

            try
            {
                File.Delete(_sessionProfile);
            }
            catch (Exception)
            {
                // a leftover temp file is not worth bothering the user about
            }

            _sessionProfile = null;
        }

        private void Fail(string message)
        {
            DeleteSessionProfile();
            State = SessionState.Failed;
            StatusMessage = message;
            StatusBrush = FailBrush;
            CancelButtonText = "Close";
            IsOverlayVisible = true;
            Header = Name + "  (offline)";
        }

        public void Cancel()
        {
            _cts.Cancel();
            EventHandler handler = CloseRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Detaches the viewer window and asks it to close. We never kill the process while a
        /// window is alive - one tvnviewer process can own several sessions.
        /// </summary>
        public void Close()
        {
            if (State == SessionState.Closed)
                return;

            State = SessionState.Closed;
            _cts.Cancel();
            DeleteSessionProfile();

            IntPtr hWnd = Host.HasChild ? Host.Release(false) : IntPtr.Zero;

            if (hWnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_HIDE);
                NativeMethods.PostMessage(hWnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            else if (_viewer != null)
            {
                try
                {
                    if (!_viewer.HasExited)
                        _viewer.Kill();
                }
                catch (Exception)
                {
                    // the viewer was already gone
                }
            }
        }

        /// <summary>Pops the session back out into its own ordinary window.</summary>
        public IntPtr Detach()
        {
            State = SessionState.Closed;
            return Host.HasChild ? Host.Release(true) : IntPtr.Zero;
        }

        /// <summary>Used as the tab's accessible name.</summary>
        public override string ToString()
        {
            return Header;
        }

        public void Dispose()
        {
            DeleteSessionProfile();
            _cts.Dispose();
            if (_viewer != null)
            {
                _viewer.Dispose();
                _viewer = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T field, T value, string property)
        {
            if (Equals(field, value))
                return;

            field = value;
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }
    }
}
