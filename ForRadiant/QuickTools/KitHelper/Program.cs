using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace KitHelper
{
    static class Program
    {
        // Passed by "Run as admin": the old process is still exiting when the new one starts,
        // so without this the new one would hand its folder back to it and exit.
        public const string NewInstanceFlag = "/new";

        public static bool Exiting { get; private set; }
        static MainWindow panel;

        [STAThread]
        static void Main(string[] args)
        {
            bool forceNew = args.Contains(NewInstanceFlag, StringComparer.OrdinalIgnoreCase);
            string folder = args.FirstOrDefault(a => !a.StartsWith("/"));

            if (!forceNew)
            {
                // Any copy outside D:\Program\RVS installs itself there if newer, then runs the
                // installed copy instead - so the app in use is always the latest build.
                if (InstalledCopy.RunInstalled(args)) return;
                // One app: a second launch (e.g. Explorer's "Open in Kit Helper") hands its
                // folder to the running instance and exits.
                if (SingleInstance.ForwardToRunning(folder)) return;
            }

            // Claimed before the slow part of start-up, so a launch arriving while this one is
            // still building its windows waits for it instead of starting a second app.
            var running = new System.Threading.Mutex(false, SingleInstance.RunningMutex);

            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.Resources.MergedDictionaries.Add(new Theme());

            panel = new MainWindow(folder);
            var icon = new FloatingIconWindow(panel);
            panel.Anchor = icon;
            icon.Show();
            var ipc = new IpcHost(panel);

            panel.MigrateFromScriptToolbox();
            // Started for a folder (right-click, a copy double-clicked in a kit): show it.
            // Started bare: just the icon.
            if (folder != null) panel.ShowNear(icon);

            app.Run();
            GC.KeepAlive(ipc);
            GC.KeepAlive(running);
        }

        public static void Exit()
        {
            Exiting = true;
            panel?.SaveSettings();
            Application.Current.Shutdown();
        }
    }

    // Receives a second launch's folder. A message-only window, so it exists whether or not the
    // panel is shown, and never appears on screen or in Alt+Tab.
    sealed class IpcHost
    {
        public const string WindowName = "KitHelper.Ipc";
        readonly HwndSource source;
        readonly MainWindow panel;

        public IpcHost(MainWindow panel)
        {
            this.panel = panel;
            source = new HwndSource(new HwndSourceParameters(WindowName) { ParentWindow = SingleInstance.HWND_MESSAGE, WindowStyle = 0 });
            source.AddHook(Hook);
            SingleInstance.AllowFromLowerIntegrity(source.Handle);
        }

        IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != SingleInstance.WM_COPYDATA) return IntPtr.Zero;
            string text = SingleInstance.Read(lParam);
            if (text == null) return IntPtr.Zero;
            handled = true;
            // Reply first; the sender is blocked in SendMessage until this returns.
            panel.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (text == SingleInstance.QuitToken) Program.Exit();   // a newer build takes over
                else panel.OpenForwarded(text);
            }));
            return (IntPtr)1;
        }
    }

    static class SingleInstance
    {
        public const int WM_COPYDATA = 0x004A;
        public static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        public static readonly IntPtr Magic = (IntPtr)0x5354424F;   // "STBO": ignore anyone else's WM_COPYDATA
        public const string QuitToken = "\u0001quit";             // a newer copy is taking over
        // Exists from the moment an instance commits to running (see Main), well before its
        // IpcHost window does.
        public const string RunningMutex = @"Local\KitHelper.Running";

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT { public IntPtr dwData; public int cbData; public IntPtr lpData; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, ref COPYDATASTRUCT l);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string name);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out int pid);
        [DllImport("user32.dll")] static extern bool AllowSetForegroundWindow(int pid);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")] static extern bool ChangeWindowMessageFilterEx(IntPtr h, int msg, int action, IntPtr info);

        // The running instance's receiving window and process. 1.4.0+ has the message-only
        // IpcHost window; the WinForms builds before it (1.3.0) received on their main window.
        static (IntPtr hwnd, Process proc) FindOther()
        {
            int self = Process.GetCurrentProcess().Id;
            for (IntPtr h = FindWindowEx(HWND_MESSAGE, IntPtr.Zero, null, IpcHost.WindowName); h != IntPtr.Zero;
                 h = FindWindowEx(HWND_MESSAGE, h, null, IpcHost.WindowName))
            {
                GetWindowThreadProcessId(h, out int pid);
                if (pid == self) continue;
                try { return (h, Process.GetProcessById(pid)); } catch { }
            }
            var legacy = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
                .FirstOrDefault(p => p.Id != self && p.MainWindowHandle != IntPtr.Zero);
            return legacy == null ? (IntPtr.Zero, null) : (legacy.MainWindowHandle, legacy);
        }

        // Closes a running instance that is an OLDER build than `than` (it may hold the installed
        // exe open, and should not keep serving folders). True if one was closed.
        public static bool CloseOlder(BuildInfo than)
        {
            var (hwnd, other) = FindOther();
            if (other == null) return false;
            string otherExe = null;
            try { otherExe = other.MainModule.FileName; } catch { }   // elevated: unreadable, leave it
            var otherBuild = otherExe == null ? null : BuildInfo.Of(otherExe);
            if (otherBuild == null || otherBuild.CompareTo(than) >= 0) return false;
            Send(hwnd, QuitToken);
            return other.WaitForExit(5000);
        }

        // An older build is closed instead and this copy takes over.
        public static bool ForwardToRunning(string folder)
        {
            if (CloseOlder(InstalledCopy.Self)) return false;
            var (hwnd, other) = FindOther();
            // Another instance is starting but has no window yet (two launches seconds apart):
            // wait for it rather than becoming a second app.
            for (int i = 0; other == null && i < 100 && AnotherIsStarting(); i++)
            {
                System.Threading.Thread.Sleep(100);
                (hwnd, other) = FindOther();
            }
            if (other == null) return false;

            // This process was just launched by the user, so it may hand over the foreground:
            // without this the receiving panel could not come to the front on its own.
            AllowSetForegroundWindow(other.Id);
            Send(hwnd, folder ?? "");                 // empty = just show the panel
            return true;
        }

        static bool AnotherIsStarting()
        {
            try { System.Threading.Mutex.OpenExisting(RunningMutex).Dispose(); return true; }
            catch { return false; }
        }

        static void Send(IntPtr hwnd, string text)
        {
            var data = new COPYDATASTRUCT { dwData = Magic, cbData = (text.Length + 1) * 2, lpData = Marshal.StringToHGlobalUni(text) };
            try { SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref data); }
            finally { Marshal.FreeHGlobal(data.lpData); }
        }

        // An elevated instance drops WM_COPYDATA from a non-elevated sender (UIPI) unless the
        // receiving window opts in.
        public static void AllowFromLowerIntegrity(IntPtr hwnd) =>
            ChangeWindowMessageFilterEx(hwnd, WM_COPYDATA, 1 /* MSGFLT_ALLOW */, IntPtr.Zero);

        public static string Read(IntPtr lParam)
        {
            var cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
            if (cds.dwData != Magic || cds.lpData == IntPtr.Zero) return null;
            return Marshal.PtrToStringUni(cds.lpData);
        }
    }
}
