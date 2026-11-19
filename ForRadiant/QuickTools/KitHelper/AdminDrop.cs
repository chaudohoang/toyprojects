using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Interop;

namespace KitHelper
{
    // Drag-and-drop onto a window running as administrator.
    //
    // Explorer runs at medium integrity. Windows (UIPI) blocks an OLE drag from it into an
    // elevated window, so WPF's normal Drop event never fires and the cursor shows "no entry".
    // OLE does not help there, but the old WM_DROPFILES path can be let through: drop the OLE
    // target, mark the window WS_EX_ACCEPTFILES, and allow WM_DROPFILES (plus the internal
    // WM_COPYGLOBALDATA that carries the file list) past the message filter.
    // Only used when elevated; a normal window keeps WPF's OLE drop.
    static class AdminDrop
    {
        const int WM_DROPFILES = 0x0233, WM_COPYDATA = 0x004A, WM_COPYGLOBALDATA = 0x0049;
        const uint MSGFLT_ALLOW = 1;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ChangeWindowMessageFilterEx(IntPtr hwnd, int msg, uint action, IntPtr info);
        [DllImport("shell32.dll")] static extern void DragAcceptFiles(IntPtr hwnd, bool accept);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint DragQueryFile(IntPtr hDrop, uint index, StringBuilder file, uint size);
        [DllImport("shell32.dll")] static extern void DragFinish(IntPtr hDrop);
        [DllImport("ole32.dll")] static extern int RevokeDragDrop(IntPtr hwnd);

        public static bool IsElevated
        {
            get
            {
                try
                {
                    using (var id = WindowsIdentity.GetCurrent())
                        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch { return false; }
            }
        }

        // Switch hwnd to WM_DROPFILES; onDrop gets the dropped paths.
        public static void Enable(HwndSource src, Action<string[]> onDrop)
        {
            IntPtr hwnd = src.Handle;
            Use(hwnd);
            src.AddHook((IntPtr h, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (msg != WM_DROPFILES) return IntPtr.Zero;
                handled = true;
                try
                {
                    uint n = DragQueryFile(wParam, 0xFFFFFFFF, null, 0);
                    var paths = new string[n];
                    for (uint i = 0; i < n; i++)
                    {
                        var sb = new StringBuilder((int)DragQueryFile(wParam, i, null, 0) + 1);
                        DragQueryFile(wParam, i, sb, (uint)sb.Capacity);
                        paths[i] = sb.ToString();
                    }
                    onDrop(paths);
                }
                finally { DragFinish(wParam); }
                return IntPtr.Zero;
            });
        }

        // WPF registers its OLE drop target while the window is being built and may do it again
        // later, so this is also re-applied once the window has loaded.
        public static void Use(IntPtr hwnd)
        {
            RevokeDragDrop(hwnd);   // with an OLE target present, OLE never falls back to WM_DROPFILES
            DragAcceptFiles(hwnd, true);
            ChangeWindowMessageFilterEx(hwnd, WM_DROPFILES, MSGFLT_ALLOW, IntPtr.Zero);
            ChangeWindowMessageFilterEx(hwnd, WM_COPYDATA, MSGFLT_ALLOW, IntPtr.Zero);
            ChangeWindowMessageFilterEx(hwnd, WM_COPYGLOBALDATA, MSGFLT_ALLOW, IntPtr.Zero);
        }
    }
}
