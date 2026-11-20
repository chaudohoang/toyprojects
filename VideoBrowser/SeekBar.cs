using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VideoBrowser
{
    /// <summary>Flat owner-drawn slider, value 0..1. Used for the seek bar and the volume.</summary>
    sealed class SeekBar : Control
    {
        double _value;
        bool _hover;

        public Color TrackColor { get; set; } = Color.FromArgb(70, 70, 70);
        public Color FillColor { get; set; } = Color.FromArgb(0, 150, 255);
        public Color KnobColor { get; set; } = Color.White;

        /// <summary>True while the user holds the mouse button on the bar.</summary>
        public bool Dragging { get; private set; }

        /// <summary>Raised whenever the user moves the value (press, drag, release).</summary>
        public event EventHandler Scrubbed;

        /// <summary>Optional: returns tooltip text for a hovered value (e.g. a time).</summary>
        public Func<double, string> HoverText { get; set; }

        readonly ToolTip _tip = new ToolTip { InitialDelay = 0, ReshowDelay = 0 };
        string _lastTip;

        public SeekBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
            Height = 18;
        }

        public double Value
        {
            get => _value;
            set
            {
                var v = Math.Max(0, Math.Min(1, double.IsNaN(value) ? 0 : value));
                if (Math.Abs(v - _value) < 0.0001) return;
                _value = v;
                Invalidate();
            }
        }

        const int Pad = 7;

        double ValueAt(int x) => Math.Max(0, Math.Min(1, (x - Pad) / (double)Math.Max(1, Width - 2 * Pad)));

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Dragging = true;
            Capture = true;
            Value = ValueAt(e.X);
            Scrubbed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Dragging)
            {
                Value = ValueAt(e.X);
                Scrubbed?.Invoke(this, EventArgs.Empty);
            }
            if (HoverText != null)
            {
                var t = HoverText(ValueAt(e.X));
                if (t != _lastTip) { _lastTip = t; _tip.Show(t, this, e.X + 12, -22); }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!Dragging) return;
            Dragging = false;
            Capture = false;
            Value = ValueAt(e.X);
            Scrubbed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false; _lastTip = null; _tip.Hide(this); Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            int th = _hover || Dragging ? 6 : 4;
            int y = (Height - th) / 2;
            int w = Width - 2 * Pad;
            using (var b = new SolidBrush(TrackColor)) g.FillRectangle(b, Pad, y, w, th);
            int fw = (int)Math.Round(w * _value);
            using (var b = new SolidBrush(Enabled ? FillColor : TrackColor)) g.FillRectangle(b, Pad, y, fw, th);

            if (_hover || Dragging)
            {
                int r = 6;
                using (var b = new SolidBrush(KnobColor))
                    g.FillEllipse(b, Pad + fw - r, Height / 2 - r, r * 2, r * 2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
