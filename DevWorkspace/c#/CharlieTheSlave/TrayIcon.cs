using System.Drawing;
using System.Windows.Forms;

namespace CharlieTheSlave;

// Notification-area icon: the way back to the floating icon when it sits on a monitor that is
// hard to reach (the taskbar is always on the main screen). Same as Quick Launcher's / Kit Helper's.
//   click                   open / hide the prompt
//   Bring icon back here    icon to its default spot on the main screen
sealed class TrayIcon : IDisposable
{
    readonly NotifyIcon _tray;

    public TrayIcon(FloatingIconWindow icon, PromptWindow prompt)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open prompt", null, (_, _) => prompt.ShowNear(icon)).Font = new Font(menu.Font, System.Drawing.FontStyle.Bold);
        menu.Items.Add("Bring icon back here", null, (_, _) => icon.MoveToMainScreen());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit Charlie", null, (_, _) => System.Windows.Application.Current.Shutdown());

        _tray = new NotifyIcon
        {
            // No .ico of its own: the gradient circle with ✦, drawn to match the floating icon.
            Icon = DrawIcon(),
            Text = "Charlie The Slave",
            ContextMenuStrip = menu,
            Visible = true,
        };
        _tray.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) icon.TogglePrompt(); };
    }

    static Icon DrawIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var r = new Rectangle(1, 1, 30, 30);
            using var fill = new System.Drawing.Drawing2D.LinearGradientBrush(r,
                System.Drawing.Color.FromArgb(0x7C, 0x5C, 0xFF), System.Drawing.Color.FromArgb(0x22, 0xB8, 0xCF), 45f);
            g.FillEllipse(fill, r);
            using var font = new Font("Segoe UI Symbol", 15f, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("✦", font, System.Drawing.Brushes.White, new RectangleF(0, 0, 32, 31), sf);
        }
        return Icon.FromHandle(bmp.GetHicon());
    }

    // Without this the icon lingers in the tray until the mouse passes over it.
    public void Dispose()
    {
        _tray.Visible = false;
        _tray.Dispose();
    }
}
