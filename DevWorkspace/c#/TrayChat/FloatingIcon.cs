using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrayChat;

/// <summary>
/// Always-on-top round bubble that can be dragged anywhere; a click (without dragging) raises <see cref="Clicked"/>.
/// </summary>
sealed class FloatingIcon : Form
{
    const int BubbleSize = 52;
    const int DragThreshold = 4;

    readonly Settings settings = Settings.Load();
    bool hasUnread;
    Point? pressedAt; // where the left button went down, until it becomes a drag or is released

    public event Action? Clicked;

    public FloatingIcon()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(BubbleSize, BubbleSize);
        BackColor = Color.FromArgb(0, 120, 215);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        using (var path = new GraphicsPath())
        {
            path.AddEllipse(0, 0, BubbleSize, BubbleSize);
            Region = new Region(path);
        }
        Location = settings.Location ?? DefaultLocation();
        new ToolTip().SetToolTip(this, "TrayChat - click to chat, drag to move");
    }

    /// <summary>Whether the user wants the bubble on screen; remembered across restarts.</summary>
    public bool WantedVisible
    {
        get => settings.Enabled;
        set
        {
            settings.Enabled = value;
            settings.Save();
            Visible = value;
        }
    }

    /// <summary>Red dot shown while a message has arrived since the bubble was last clicked.</summary>
    public bool HasUnread
    {
        get => hasUnread;
        set { hasUnread = value; Invalidate(); }
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TOOLWINDOW = 0x80; // keep it out of Alt+Tab
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var white = new SolidBrush(Color.White);
        // Small speech bubble with three dots, centred.
        g.FillEllipse(white, 12, 14, 28, 20);
        g.FillPolygon(white, new Point[] { new(17, 30), new(14, 39), new(24, 32) });
        using var blue = new SolidBrush(BackColor);
        foreach (var x in new[] { 18, 24, 30 }) g.FillEllipse(blue, x, 22, 4, 4);
        if (hasUnread)
        {
            using var red = new SolidBrush(Color.Crimson);
            g.FillEllipse(red, BubbleSize - 18, 4, 12, 12);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) pressedAt = e.Location;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (pressedAt is not Point start || e.Button != MouseButtons.Left) return;
        if (Math.Abs(e.X - start.X) <= DragThreshold && Math.Abs(e.Y - start.Y) <= DragThreshold) return;

        // Once the mouse really moves, hand the drag to Windows' own move loop (as if the bubble
        // were a title bar). SendMessage returns when the button is released.
        pressedAt = null;
        ReleaseCapture();
        SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
        settings.X = Location.X;
        settings.Y = Location.Y;
        settings.Save();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        bool wasClick = e.Button == MouseButtons.Right || (e.Button == MouseButtons.Left && pressedAt != null);
        pressedAt = null;
        if (wasClick) Clicked?.Invoke();
    }

    const int WM_NCLBUTTONDOWN = 0xA1;
    static readonly IntPtr HTCAPTION = 2;

    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    static Point DefaultLocation()
    {
        var area = Screen.PrimaryScreen!.WorkingArea;
        return new Point(area.Right - BubbleSize - 16, area.Top + area.Height / 3);
    }

    /// <summary>Remembers whether the bubble is shown and where it was dropped, in %AppData%\TrayChat\bubble.json.</summary>
    sealed class Settings
    {
        static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrayChat", "bubble.json");

        public bool Enabled { get; set; } = true;
        public int? X { get; set; }
        public int? Y { get; set; }

        /// <summary>The saved spot, unless it is no longer on any screen (e.g. monitor unplugged).</summary>
        [JsonIgnore]
        public Point? Location
        {
            get
            {
                if (X is not int x || Y is not int y) return null;
                var bounds = new Rectangle(x, y, BubbleSize, BubbleSize);
                return Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds)) ? bounds.Location : null;
            }
        }

        public static Settings Load()
        {
            try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); }
            catch (Exception) { return new(); }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
            }
            catch (Exception) { }
        }
    }
}
