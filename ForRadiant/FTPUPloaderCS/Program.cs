using System;
using System.Threading;
using System.Windows.Forms;

namespace FTPUPloaderCS
{
    internal static class Program
    {
        private static readonly Mutex mutex = new Mutex(true, "FTPUPloaderCS");

        [STAThread]
        private static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                mutex.ReleaseMutex();
            }
            else
            {
                NativeMethods.PostMessage(new IntPtr(NativeMethods.HWND_BROADCAST), NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
