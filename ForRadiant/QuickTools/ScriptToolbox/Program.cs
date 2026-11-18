using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScriptToolbox
{
    static class Program
    {
        // Passed by "Run as admin": the old window is still closing when the new process
        // starts, so without this the new one would hand its folder back to it and exit.
        public const string NewInstanceFlag = "/new";

        [STAThread]
        static void Main(string[] args)
        {
            bool forceNew = args.Contains(NewInstanceFlag, StringComparer.OrdinalIgnoreCase);
            string folder = args.FirstOrDefault(a => !string.Equals(a, NewInstanceFlag, StringComparison.OrdinalIgnoreCase));

            // One window: a second launch (e.g. Explorer's "Open in Script Toolbox") hands its
            // folder to the running instance and exits.
            if (!forceNew && SingleInstance.ForwardToRunning(folder)) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(folder));
        }
    }

    static class SingleInstance
    {
        public const int WM_COPYDATA = 0x004A;
        public static readonly IntPtr Magic = (IntPtr)0x5354424F;   // "STBO": ignore anyone else's WM_COPYDATA

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT { public IntPtr dwData; public int cbData; public IntPtr lpData; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, ref COPYDATASTRUCT l);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int cmd);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
        [DllImport("user32.dll")] static extern bool ChangeWindowMessageFilterEx(IntPtr h, int msg, int action, IntPtr info);

        // Any other process with this exe's name and a window counts, wherever it was started from.
        public static bool ForwardToRunning(string folder)
        {
            var self = Process.GetCurrentProcess();
            var other = Process.GetProcessesByName(self.ProcessName)
                .FirstOrDefault(p => p.Id != self.Id && p.MainWindowHandle != IntPtr.Zero);
            if (other == null) return false;

            IntPtr hwnd = other.MainWindowHandle;
            string text = folder ?? "";             // empty = just bring the window forward
            var data = new COPYDATASTRUCT { dwData = Magic, cbData = (text.Length + 1) * 2, lpData = Marshal.StringToHGlobalUni(text) };
            try { SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref data); }
            finally { Marshal.FreeHGlobal(data.lpData); }

            // This process was just launched by the user, so it is allowed to hand over the
            // foreground; the receiving window could not take it on its own.
            if (IsIconic(hwnd)) ShowWindow(hwnd, 9 /* SW_RESTORE */);
            SetForegroundWindow(hwnd);
            return true;
        }

        // An elevated window drops WM_COPYDATA from a non-elevated sender (UIPI) unless it
        // opts in. Called once the receiving window has a handle.
        public static void AllowFromLowerIntegrity(IntPtr hwnd) =>
            ChangeWindowMessageFilterEx(hwnd, WM_COPYDATA, 1 /* MSGFLT_ALLOW */, IntPtr.Zero);

        public static string Read(ref Message m)
        {
            var cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
            if (cds.dwData != Magic || cds.lpData == IntPtr.Zero) return null;
            return Marshal.PtrToStringUni(cds.lpData);
        }
    }
}
