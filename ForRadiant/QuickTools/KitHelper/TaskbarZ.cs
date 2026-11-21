using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace KitHelper
{
    // Keeps the floating icon above the taskbar when it sits on it. Both are topmost windows, and
    // among those the last one activated wins: one click on the taskbar would bury the icon.
    //
    // A 1 s timer alone left it buried for up to ~1.4 s after each click: a visible flash. So it
    // also reacts at once, through a WinEvent hook, to a window coming to the front
    // (EVENT_SYSTEM_FOREGROUND) and to a window taking the mouse (EVENT_SYSTEM_CAPTURESTART, which
    // a click on the taskbar fires even when the taskbar was in front already). The timer stays
    // as a backstop. All of it acts only while the icon overlaps a taskbar.
    static class TaskbarZ
    {
        public static void Keep(Window w, Func<bool> overTaskbar)
        {
            void Raise() { if (overTaskbar()) BringToTop(w); }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => Raise();
            timer.Start();

            // Out-of-context hooks call back on this (the UI) thread through its message loop.
            WinEventProc proc = (hook, ev, hwnd, idObject, idChild, thread, time) => Raise();
            IntPtr handle = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_CAPTURESTART, IntPtr.Zero, proc, 0, 0,
                                            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
            w.Closed += (s, e) => { timer.Stop(); if (handle != IntPtr.Zero) UnhookWinEvent(handle); GC.KeepAlive(proc); };
            Hooks.Add(proc);   // the delegate must outlive the hook: never let the GC collect it
        }

        static readonly System.Collections.Generic.List<WinEventProc> Hooks = new System.Collections.Generic.List<WinEventProc>();

        public static void BringToTop(Window w)
        {
            var h = new WindowInteropHelper(w).Handle;
            if (h != IntPtr.Zero)
                SetWindowPos(h, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOACTIVATE = 0x10;
        // 0x0003..0x0008: foreground, menu start/end, popup start/end, capture start.
        const uint EVENT_SYSTEM_FOREGROUND = 0x0003, EVENT_SYSTEM_CAPTURESTART = 0x0008;
        const uint WINEVENT_OUTOFCONTEXT = 0x0000, WINEVENT_SKIPOWNPROCESS = 0x0002;

        delegate void WinEventProc(IntPtr hook, uint ev, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventProc proc, uint pid, uint tid, uint flags);
        [DllImport("user32.dll")] static extern bool UnhookWinEvent(IntPtr hook);
    }
}
