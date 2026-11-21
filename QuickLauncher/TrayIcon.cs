using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickLauncher
{
    // Notification-area icon: the way back to the orb when it sits on a monitor that is hard to
    // reach (the taskbar is always on the main screen).
    //   click                       show / hide the panel
    //   Bring icon back here        orb to its default spot on the main screen
    sealed class TrayIcon : IDisposable
    {
        readonly NotifyIcon tray;

        public TrayIcon(FloatingIconWindow orb, MainWindow panel)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show panel", null, (s, e) => panel.ShowNear(orb)).Font = new Font(menu.Font, FontStyle.Bold);
            menu.Items.Add("Bring icon back here", null, (s, e) => orb.MoveToMainScreen());
            menu.Items.Add("Rescan games and apps", null, (s, e) => panel.RescanAll());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit Quick Launcher", null, (s, e) => Program.Exit());

            tray = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Text = "Quick Launcher " + BuildInfo.Version,
                ContextMenuStrip = menu,
                Visible = true,
            };
            tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) panel.Toggle(); };
        }

        // Without this the icon lingers in the tray until the mouse passes over it.
        public void Dispose()
        {
            tray.Visible = false;
            tray.Dispose();
        }
    }
}
