using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// The Win32 calls needed to pull another process's top-level window into one of
    /// our tab pages and to push it back out again.
    /// </summary>
    internal static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const long WS_CHILD = 0x40000000L;
        public const long WS_POPUP = 0x80000000L;
        public const long WS_CLIPCHILDREN = 0x02000000L;
        public const long WS_CAPTION = 0x00C00000L;
        public const long WS_THICKFRAME = 0x00040000L;
        public const long WS_MINIMIZEBOX = 0x00020000L;
        public const long WS_MAXIMIZEBOX = 0x00010000L;
        public const long WS_SYSMENU = 0x00080000L;
        public const long WS_VISIBLE = 0x10000000L;

        public const long WS_EX_APPWINDOW = 0x00040000L;
        public const long WS_EX_WINDOWEDGE = 0x00000100L;
        public const long WS_EX_CLIENTEDGE = 0x00000200L;
        public const long WS_EX_DLGMODALFRAME = 0x00000001L;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;

        public const uint GW_OWNER = 4;

        public const uint WM_CLOSE = 0x0010;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint command);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int command);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName,
            int style, int x, int y, int width, int height,
            IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width { get { return Right - Left; } }
            public int Height { get { return Bottom - Top; } }
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint attachTo, uint attachFrom, bool attach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int index, IntPtr value);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int index, int value);

        public static long GetWindowStyle(IntPtr hWnd, int index)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, index).ToInt64()
                : GetWindowLong32(hWnd, index);
        }

        public static void SetWindowStyle(IntPtr hWnd, int index, long value)
        {
            if (IntPtr.Size == 8)
                SetWindowLongPtr64(hWnd, index, new IntPtr(value));
            else
                SetWindowLong32(hWnd, index, unchecked((int)value));
        }

        public static string GetClassNameOf(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            int n = GetClassName(hWnd, sb, sb.Capacity);
            return n > 0 ? sb.ToString() : string.Empty;
        }

        public static string GetTitleOf(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length <= 0)
                return string.Empty;
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Top-level windows belonging to a process of the given name that look like a real
        /// viewer window: visible, un-owned, titled, and not a dialog ("#32770" is the class
        /// TightVNC uses for its connection / authentication / error boxes).
        /// </summary>
        public static List<IntPtr> FindMainWindows(string processName)
        {
            HashSet<uint> pids = new HashSet<uint>();
            foreach (Process p in Process.GetProcessesByName(processName))
            {
                pids.Add((uint)p.Id);
                p.Dispose();
            }

            List<IntPtr> found = new List<IntPtr>();
            if (pids.Count == 0)
                return found;

            EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (!pids.Contains(pid))
                    return true;
                if (!IsWindowVisible(hWnd))
                    return true;
                if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                    return true;
                if (GetTitleOf(hWnd).Length == 0)
                    return true;

                string cls = GetClassNameOf(hWnd);
                if (cls == "#32770" || cls == "ComboLBox" || cls == "NotifyIconWindowClass")
                    return true;

                found.Add(hWnd);
                return true;
            }, IntPtr.Zero);

            return found;
        }
    }
}
