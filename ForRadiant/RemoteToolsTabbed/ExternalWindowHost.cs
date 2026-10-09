using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// Hosts a window owned by another process inside the WPF tree - which is what turns a
    /// separate tvnviewer window into the content of a tab.
    ///
    /// HwndHost needs a window of our own to hand to WPF, so we create a plain child
    /// ("holder") window and re-parent the viewer into that. The holder also lets a tab
    /// exist before the viewer window does, which is the whole "Connecting ..." phase.
    /// </summary>
    public sealed class ExternalWindowHost : HwndHost
    {
        private IntPtr _holder;
        private IntPtr _child;
        private long _childStyle;
        private long _childExStyle;
        private bool _attached;

        public ExternalWindowHost()
        {
            // HwndHost only re-places its window by itself when the element's visibility
            // changes, so a plain window resize would leave the session at its old size
            // until the next tab switch. Drive it from the layout instead.
            SizeChanged += (s, e) => Refit();
            IsVisibleChanged += (s, e) => Refit();
        }

        private void Refit()
        {
            if (_holder == IntPtr.Zero)
                return;

            UpdateWindowPos();   // moves/sizes the holder to the element's layout slot
            ResizeChild();       // then fill the holder with the hosted window
        }

        public IntPtr ChildHandle
        {
            get { return _child; }
        }

        public bool HasChild
        {
            get { return _attached && _child != IntPtr.Zero && NativeMethods.IsWindow(_child); }
        }

        /// <summary>
        /// Takes ownership of a top-level window. Safe to call before the host itself has a
        /// handle - the re-parenting then happens as soon as it gets one.
        /// </summary>
        public void Adopt(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd))
                return;

            _child = hWnd;
            _childStyle = NativeMethods.GetWindowStyle(hWnd, NativeMethods.GWL_STYLE);
            _childExStyle = NativeMethods.GetWindowStyle(hWnd, NativeMethods.GWL_EXSTYLE);
            Attach();
        }

        private void Attach()
        {
            if (_attached || _child == IntPtr.Zero || _holder == IntPtr.Zero)
                return;
            if (!NativeMethods.IsWindow(_child))
                return;

            // A maximised window re-parents badly - put it back to normal first.
            NativeMethods.ShowWindow(_child, NativeMethods.SW_RESTORE);

            long style = _childStyle;
            style &= ~(NativeMethods.WS_POPUP | NativeMethods.WS_CAPTION | NativeMethods.WS_THICKFRAME |
                       NativeMethods.WS_MINIMIZEBOX | NativeMethods.WS_MAXIMIZEBOX | NativeMethods.WS_SYSMENU);
            style |= NativeMethods.WS_CHILD;
            NativeMethods.SetWindowStyle(_child, NativeMethods.GWL_STYLE, style);

            long exStyle = _childExStyle;
            exStyle &= ~(NativeMethods.WS_EX_APPWINDOW | NativeMethods.WS_EX_WINDOWEDGE |
                         NativeMethods.WS_EX_CLIENTEDGE | NativeMethods.WS_EX_DLGMODALFRAME);
            NativeMethods.SetWindowStyle(_child, NativeMethods.GWL_EXSTYLE, exStyle);

            NativeMethods.SetParent(_child, _holder);
            _attached = true;

            ResizeChild();
            NativeMethods.SetWindowPos(_child, IntPtr.Zero, 0, 0, 0, 0,
                NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE |
                NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_SHOWWINDOW);
        }

        /// <summary>Puts the window back the way we found it: top-level, framed and visible.</summary>
        public IntPtr Release(bool showAfterwards)
        {
            IntPtr hWnd = _child;
            _child = IntPtr.Zero;

            if (!_attached || hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd))
            {
                _attached = false;
                return IntPtr.Zero;
            }

            _attached = false;

            NativeMethods.SetParent(hWnd, IntPtr.Zero);
            NativeMethods.SetWindowStyle(hWnd, NativeMethods.GWL_STYLE, _childStyle);
            NativeMethods.SetWindowStyle(hWnd, NativeMethods.GWL_EXSTYLE, _childExStyle);

            if (showAfterwards)
            {
                NativeMethods.RECT holder;
                int width = 1024;
                int height = 768;
                if (_holder != IntPtr.Zero && NativeMethods.GetClientRect(_holder, out holder))
                {
                    width = Math.Max(640, holder.Width);
                    height = Math.Max(480, holder.Height);
                }

                NativeMethods.MoveWindow(hWnd, 60, 60, width, height, true);
                NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                    NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE |
                    NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_SHOWWINDOW);
                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOWNORMAL);
            }
            else
            {
                NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                    NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE |
                    NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_NOACTIVATE);
            }

            return hWnd;
        }

        public void ResizeChild()
        {
            if (!HasChild || _holder == IntPtr.Zero)
                return;

            NativeMethods.RECT r;
            if (NativeMethods.GetClientRect(_holder, out r))
                NativeMethods.MoveWindow(_child, 0, 0, r.Width, r.Height, true);
        }

        /// <summary>
        /// Hands keyboard focus to the hosted window. It runs on another thread, so a plain
        /// SetFocus would be ignored unless the input queues are attached first.
        /// </summary>
        public void FocusChild()
        {
            if (!HasChild)
                return;

            Keyboard.Focus(this);

            uint targetPid;
            uint targetThread = NativeMethods.GetWindowThreadProcessId(_child, out targetPid);
            uint ownThread = NativeMethods.GetCurrentThreadId();

            if (targetThread == ownThread)
            {
                NativeMethods.SetFocus(_child);
                return;
            }

            if (NativeMethods.AttachThreadInput(ownThread, targetThread, true))
            {
                try
                {
                    NativeMethods.SetFocus(_child);
                }
                finally
                {
                    NativeMethods.AttachThreadInput(ownThread, targetThread, false);
                }
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _holder = NativeMethods.CreateWindowEx(
                0, "static", null,
                unchecked((int)(NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE | NativeMethods.WS_CLIPCHILDREN)),
                0, 0, 1, 1, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            Attach();   // a viewer adopted before the host had a handle
            return new HandleRef(this, _holder);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Never destroy a window we do not own - give it its frame back instead.
            if (_attached)
                Release(true);

            if (_holder != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(_holder);
                _holder = IntPtr.Zero;
            }
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZE)
                ResizeChild();

            return IntPtr.Zero;
        }

        private const int WM_SIZE = 0x0005;
    }
}
