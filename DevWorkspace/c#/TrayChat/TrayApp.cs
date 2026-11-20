using System.Drawing.Drawing2D;

namespace TrayChat;

sealed class TrayApp : ApplicationContext
{
    readonly Messenger messenger = new();
    readonly ContextMenuStrip menu = new();
    readonly NotifyIcon tray;
    readonly Dictionary<string, ChatForm> chats = new();
    readonly FloatingIcon bubble = new();

    public static Icon AppIcon { get; } = CreateIcon();

    public TrayApp()
    {
        BuildMenu();
        menu.Opening += (_, e) => { BuildMenu(); e.Cancel = false; };

        tray = new NotifyIcon { Icon = AppIcon, Text = "TrayChat", ContextMenuStrip = menu, Visible = true };
        Log.Write("Tray icon added");

        bubble.Clicked += () =>
        {
            bubble.HasUnread = false;
            ShowMenuBesideBubble();
        };
        if (bubble.WantedVisible) bubble.Show();
        Log.Write($"Floating icon {(bubble.WantedVisible ? $"shown at {bubble.Location}" : "turned off")}");

        messenger.MessageReceived += (peer, text) =>
        {
            var form = OpenChat(peer);
            form.Receive(text);
            if (!form.ContainsFocus) bubble.HasUnread = true;
        };
        messenger.Start();
        Log.Write($"Listening as {messenger.Name}");
    }

    void BuildMenu()
    {
        menu.Items.Clear();
        menu.Items.Add(new ToolStripMenuItem($"Me: {messenger.Name}") { Enabled = false });
        menu.Items.Add(new ToolStripSeparator());

        var peers = messenger.Peers.ToList();
        if (peers.Count == 0)
            menu.Items.Add(new ToolStripMenuItem("Looking for PCs...") { Enabled = false });
        foreach (var peer in peers)
            menu.Items.Add($"{peer.Name}  [{peer.Address}]", null, (_, _) => OpenChat(peer).Activate());

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Floating icon", null, (_, _) => ToggleBubble())
        {
            Checked = bubble.WantedVisible,
        });
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
    }

    void ToggleBubble()
    {
        bubble.WantedVisible = !bubble.WantedVisible;
        if (!bubble.WantedVisible)
        {
            // The tray icon is often tucked behind the taskbar's ^ arrow, so say where the app went.
            tray.ShowBalloonTip(5000, "TrayChat is still running",
                "Right-click the TrayChat icon in the system tray (it may be behind the ^ arrow) to chat " +
                "or turn the floating icon back on.", ToolTipIcon.Info);
        }
    }

    /// <summary>
    /// Opens the menu towards the middle of the bubble's own screen. Letting WinForms place it
    /// near a screen edge pushes it onto a neighbouring monitor, where it is easy to miss.
    /// </summary>
    void ShowMenuBesideBubble()
    {
        var area = Screen.FromControl(bubble).WorkingArea;
        var b = bubble.Bounds;
        bool openLeft = b.Left + b.Width / 2 > area.Left + area.Width / 2;
        bool openUp = b.Top + b.Height / 2 > area.Top + area.Height / 2;

        var anchor = new Point(openLeft ? b.Left : b.Right, openUp ? b.Bottom : b.Top);
        var direction = (openUp, openLeft) switch
        {
            (true, true) => ToolStripDropDownDirection.AboveLeft,
            (true, false) => ToolStripDropDownDirection.AboveRight,
            (false, true) => ToolStripDropDownDirection.BelowLeft,
            (false, false) => ToolStripDropDownDirection.BelowRight,
        };
        BuildMenu(); // size the menu with the current peer list before positioning it
        bubble.Activate(); // so the menu closes normally when clicking elsewhere
        menu.Show(anchor, direction);
    }

    ChatForm OpenChat(Peer peer)
    {
        if (!chats.TryGetValue(peer.Id, out var form) || form.IsDisposed)
        {
            form = chats[peer.Id] = new ChatForm(messenger, peer);
            form.Activated += (_, _) => bubble.HasUnread = false;
        }
        if (!form.Visible) form.Show();
        if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
        return form;
    }

    protected override void ExitThreadCore()
    {
        Log.Write("Exit chosen from menu");
        messenger.Dispose();
        tray.Visible = false;
        tray.Dispose();
        foreach (var form in chats.Values) form.Dispose();
        bubble.Dispose();
        base.ExitThreadCore();
    }

    static Icon CreateIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(0, 120, 215));
            g.FillEllipse(brush, 2, 3, 28, 22);
            g.FillPolygon(brush, new Point[] { new(8, 20), new(6, 30), new(16, 22) });
            using var dot = new SolidBrush(Color.White);
            foreach (var x in new[] { 9, 15, 21 }) g.FillEllipse(dot, x, 12, 4, 4);
        }
        return Icon.FromHandle(bmp.GetHicon());
    }
}
