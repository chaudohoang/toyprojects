using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace KitHelper
{
    // Keeps the floating icon above the taskbar when it sits on it. Both are topmost windows, and
    // among those the last one activated wins: one click on the taskbar would bury the icon.
    static class TaskbarZ
    {
        public static void BringToTop(Window w)
        {
            var h = new WindowInteropHelper(w).Handle;
            if (h != IntPtr.Zero)
                SetWindowPos(h, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOACTIVATE = 0x10;

        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    }
}
