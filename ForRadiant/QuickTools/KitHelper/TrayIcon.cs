using System;
using System.Drawing;
using System.Windows.Forms;

namespace KitHelper
{
    // Notification-area icon: the way back to the floating K when it sits on a monitor that is
    // hard to reach (the taskbar is always on the main screen). Same as Quick Launcher's.
    //   click                   show / hide the panel
    //   Bring icon back here    K to its default spot on the main screen
    sealed class TrayIcon : IDisposable
    {
        readonly NotifyIcon tray;

        public TrayIcon(FloatingIconWindow icon, MainWindow panel)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show panel", null, (s, e) => { if (panel.InModal) panel.FocusModal(); else panel.ShowNear(icon); })
                .Font = new Font(menu.Font, FontStyle.Bold);
            menu.Items.Add("Bring icon back here", null, (s, e) => icon.MoveToMainScreen());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit Kit Helper", null, (s, e) => Program.Exit());

            tray = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Text = Truncate("Kit Helper " + InstalledCopy.Self?.ToString(), 63),   // NotifyIcon limit
                ContextMenuStrip = menu,
                Visible = true,
            };
            tray.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                if (panel.InModal) panel.FocusModal(); else panel.Toggle();
            };
        }

        static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

        // Without this the icon lingers in the tray until the mouse passes over it.
        public void Dispose()
        {
            tray.Visible = false;
            tray.Dispose();
        }
    }
}
