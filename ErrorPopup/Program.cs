using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ErrorPopup
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Usage: ErrorPopup.exe "<cancelReason>" ["<cancelValue>"] [-t seconds]
            string reason = null;
            string value = null;
            int seconds = 4;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if ((a == "-t" || a == "/t") && i + 1 < args.Length)
                {
                    int s;
                    if (int.TryParse(args[i + 1], out s) && s > 0) seconds = s;
                    i++;
                }
                else if (reason == null) reason = a;
                else if (value == null) value = a;
            }

            if (string.IsNullOrEmpty(reason))
            {
                reason = "Sample error \u2014 double-clicked with no arguments";
                if (string.IsNullOrEmpty(value)) value = "ErrorPopup self-test";
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PopupForm(reason, value, seconds));
        }
    }

    public class PopupForm : Form
    {
        private readonly string mReason;
        private readonly string mValue;
        private readonly int mTotalMs;
        private readonly Timer mTimer;
        private int mElapsed;
        private float mProgress = 1f;

        private Font mReasonFont;   // computed once, scaled to the screen
        private Font mValueFont;

        private static readonly Color BackRed   = Color.FromArgb(150, 28, 32);
        private static readonly Color TextWhite  = Color.White;
        private static readonly Color ValueColor = Color.FromArgb(255, 214, 214);
        private static readonly Color BarColor    = Color.FromArgb(255, 110, 110);
        private const string FontFamily = "Segoe UI";

        public PopupForm(string reason, string value, int seconds)
        {
            mReason = reason;
            mValue = value;
            mTotalMs = seconds * 1000;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = true;                       // each instance gets its own taskbar button
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            BackColor = BackRed;
            DoubleBuffered = true;

            // Distinct taskbar label per alert so multiple are tellable apart.
            string label = reason.Length > 40 ? reason.Substring(0, 40) + "..." : reason;
            Text = "Error - " + label;

            // Small console-sized window (~default cmd footprint), centred on the
            // PRIMARY monitor, then cascaded so stacked alerts don't fully overlap.
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int w = 600, h = 370;
            int x = wa.Left + (wa.Width - w) / 2;
            int y = wa.Top + (wa.Height - h) / 2;

            int step = CascadeIndex() * 36;             // offset each later instance
            x = Math.Min(x + step, wa.Right - w);
            y = Math.Min(y + step, wa.Bottom - h);
            Bounds = new Rectangle(x, y, w, h);

            mTimer = new Timer { Interval = 30 };
            mTimer.Tick += OnTick;
            mTimer.Start();

            Click += (s, e) => Close();
        }

        // Appear on top without stealing focus from TrueTest, but stay a normal
        // taskbar window (no WS_EX_NOACTIVATE, which would hide the taskbar button).
        protected override bool ShowWithoutActivation { get { return true; } }

        // How many ErrorPopup instances were already running when this one started.
        private static int CascadeIndex()
        {
            try
            {
                string me = Process.GetCurrentProcess().ProcessName;
                int n = Process.GetProcessesByName(me).Length - 1; // exclude self
                return Math.Max(0, Math.Min(n, 8));                 // cap the cascade
            }
            catch { return 0; }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            using (var path = RoundedRect(new Rectangle(0, 0, Width, Height), 18))
                Region = new Region(path);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void OnTick(object sender, EventArgs e)
        {
            mElapsed += mTimer.Interval;
            mProgress = Math.Max(0f, 1f - (float)mElapsed / mTotalMs);

            int remaining = mTotalMs - mElapsed;
            if (remaining <= 350) Opacity = Math.Max(0, remaining / 350.0);

            if (mElapsed >= mTotalMs)
            {
                mTimer.Stop();
                Close();
                return;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int W = ClientSize.Width, H = ClientSize.Height;
            bool hasValue = !string.IsNullOrEmpty(mValue);

            // Header tag.
            int headerH = (int)(H * 0.10);
            using (var f = new Font(FontFamily, headerH * 0.38f, FontStyle.Bold))
                TextRenderer.DrawText(g, "ERROR", f,
                    new Rectangle(0, headerH / 3, W, headerH),
                    Color.FromArgb(255, 190, 190),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

            // Layout: reason fills the middle, value sits below it.
            int top = headerH;
            int bottom = H - (int)(H * 0.04);            // leave room for the bar
            int valueH = hasValue ? (int)(H * 0.18) : 0;
            var reasonRect = new Rectangle((int)(W * 0.05), top,
                                           (int)(W * 0.90), bottom - top - valueH);
            var valueRect  = new Rectangle((int)(W * 0.10), bottom - valueH,
                                           (int)(W * 0.80), valueH);

            // Build fonts once, sized to fit their boxes.
            if (mReasonFont == null)
                mReasonFont = FitFont(g, mReason, FontStyle.Bold, reasonRect.Size);
            if (hasValue && mValueFont == null)
                mValueFont = FitFont(g, "[" + mValue + "]", FontStyle.Regular, valueRect.Size);

            var center = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                       | TextFormatFlags.WordBreak;

            TextRenderer.DrawText(g, mReason, mReasonFont, reasonRect, TextWhite, center);
            if (hasValue)
                TextRenderer.DrawText(g, "[" + mValue + "]", mValueFont, valueRect, ValueColor, center);

            // Countdown bar across the bottom.
            int barH = Math.Max(6, (int)(H * 0.012));
            using (var b = new SolidBrush(BarColor))
                g.FillRectangle(b, 0, H - barH, (int)(W * mProgress), barH);
        }

        // Largest font for which the wrapped text fits inside box.
        private Font FitFont(Graphics g, string text, FontStyle style, Size box)
        {
            const TextFormatFlags flags = TextFormatFlags.WordBreak;
            var prop = new Size(box.Width, int.MaxValue);
            int lo = 8, hi = 600, best = 8;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                using (var f = new Font(FontFamily, mid, style))
                {
                    Size sz = TextRenderer.MeasureText(g, text, f, prop, flags);
                    if (sz.Height <= box.Height && sz.Width <= box.Width)
                    { best = mid; lo = mid + 1; }
                    else hi = mid - 1;
                }
            }
            return new Font(FontFamily, best, style);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (mReasonFont != null) mReasonFont.Dispose();
            if (mValueFont != null) mValueFont.Dispose();
        }
    }
}
