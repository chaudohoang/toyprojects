using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace VideoBrowser
{
    /// <summary>
    /// Seek-bar hover thumbnails (like MPC-HC / VLC). A second, hidden, muted MediaPlayer opens the same
    /// file paused; on hover it seeks there (fast keyframe seek) and its decoded frames arrive through
    /// libvlc video callbacks into a small buffer, which is shown in a no-activate popup above the bar.
    /// </summary>
    sealed class SeekPreview : IDisposable
    {
        const int MaxW = 224, MaxH = 126;   // thumbnail box (16:9); the frame keeps its own aspect inside it
        const int Buckets = 400;            // cache granularity along the bar
        const int SeekTimeoutMs = 600;      // give up waiting for a frame after this

        readonly Control _owner;
        // Own LibVLC instance with a dummy audio output: in libvlc 3 every player gets an audio output at
        // creation, and on Windows they share one audio session, so muting / setting the volume of the
        // hidden player muted the real one too. This one never touches the Windows audio session.
        readonly LibVLC _libVLC = new LibVLC("--aout=adummy", "--no-audio", "--no-video-title-show", "--no-osd");
        MediaPlayer _pp;
        readonly PreviewPopup _popup = new PreviewPopup();

        // keep delegates alive while libvlc holds them
        readonly MediaPlayer.LibVLCVideoFormatCb _formatCb;
        readonly MediaPlayer.LibVLCVideoCleanupCb _cleanupCb;
        readonly MediaPlayer.LibVLCVideoLockCb _lockCb;
        readonly MediaPlayer.LibVLCVideoDisplayCb _displayCb;

        // frame buffer (written by the libvlc thread)
        readonly object _bufLock = new object();
        IntPtr _buf;
        int _w, _h;

        // seek state (UI thread)
        readonly Dictionary<int, Bitmap> _cache = new Dictionary<int, Bitmap>();
        readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = 40 };
        string _path;
        int _gen;               // bumps on every Open/Close so stale frames are dropped
        bool _ready;            // first frame decoded and player paused
        bool _busy;             // a seek is in flight
        int _busyBucket = -1;
        DateTime _busySince;
        int _wantBucket = -1;
        bool _visible;

        public SeekPreview(Control owner)
        {
            _owner = owner;
            _formatCb = OnFormat;
            _cleanupCb = OnCleanup;
            _lockCb = OnLock;
            _displayCb = OnDisplay;
            _timer.Tick += (s, e) => Pump();
        }

        /// <summary>Prepare thumbnails for this file (call when the main player opens it).</summary>
        public void Open(string path)
        {
            Close();
            _path = path;
            _gen++;

            _pp = new MediaPlayer(_libVLC) { EnableKeyInput = false, EnableMouseInput = false };
            _pp.SetVideoFormatCallbacks(_formatCb, _cleanupCb);
            _pp.SetVideoCallbacks(_lockCb, null, _displayCb);
            using (var media = new Media(_libVLC, path, FromType.FromPath,
                       ":no-audio", ":no-spu", ":no-sub-autodetect-file", ":input-fast-seek",
                       ":avcodec-hw=none", ":avcodec-threads=2"))
                _pp.Play(media);
        }

        /// <summary>Release the file (stop + drop the hidden player). Safe to call repeatedly.</summary>
        public void Close()
        {
            _gen++;
            Hide();
            _ready = _busy = false;
            _wantBucket = _busyBucket = -1;
            _path = null;
            foreach (var b in _cache.Values) b.Dispose();
            _cache.Clear();
            var pp = _pp;
            _pp = null;
            if (pp != null)
            {
                pp.Stop();      // synchronous; releases the file handle
                pp.Dispose();
            }
        }

        /// <summary>Show the popup for a hovered position. <paramref name="screenAnchor"/> = point on the bar's top edge.</summary>
        public void Hover(double pos, string time, Point screenAnchor, Rectangle screenBounds)
        {
            if (_path == null) { Hide(); return; }
            int bucket = Math.Max(0, Math.Min(Buckets - 1, (int)(pos * Buckets)));
            _wantBucket = bucket;
            _popup.Time = time;
            if (_cache.TryGetValue(bucket, out var bmp)) _popup.Frame = bmp;
            else if (_popup.Frame == null) _popup.Frame = Nearest(bucket);

            var sz = _popup.Size;
            int x = screenAnchor.X - sz.Width / 2;
            int y = screenAnchor.Y - sz.Height - 6;
            x = Math.Max(screenBounds.Left, Math.Min(screenBounds.Right - sz.Width, x));
            if (y < screenBounds.Top) y = screenBounds.Top;
            _popup.ShowAt(new Point(x, y), _owner.FindForm());
            _visible = true;
            _popup.Invalidate();
            _timer.Start();
            Pump();
        }

        public void Hide()
        {
            _visible = false;
            _wantBucket = -1;
            _timer.Stop();
            if (_popup.Visible) _popup.Hide();
            _popup.Frame = null;
        }

        Bitmap Nearest(int bucket)
        {
            Bitmap best = null; int bestD = int.MaxValue;
            foreach (var kv in _cache)
            {
                int d = Math.Abs(kv.Key - bucket);
                if (d < bestD) { bestD = d; best = kv.Value; }
            }
            return bestD <= Buckets / 20 ? best : null;
        }

        // UI thread: issue the next seek when idle
        void Pump()
        {
            if (_pp == null || !_visible) return;
            if (_busy && (DateTime.UtcNow - _busySince).TotalMilliseconds > SeekTimeoutMs) _busy = false;
            if (!_ready || _busy || _wantBucket < 0 || _cache.ContainsKey(_wantBucket)) return;
            _busy = true;
            _busyBucket = _wantBucket;
            _busySince = DateTime.UtcNow;
            try { _pp.Position = (_busyBucket + 0.5f) / Buckets; } catch { _busy = false; }
        }

        // UI thread: a decoded frame arrived
        void OnFrame(int gen, Bitmap bmp)
        {
            if (gen != _gen || _pp == null) { bmp.Dispose(); return; }
            if (!_ready)
            {
                // first frame after opening: freeze the hidden player; later seeks decode while paused
                _ready = true;
                _pp.SetPause(true);
                bmp.Dispose();
                Pump();
                return;
            }
            if (!_busy) { bmp.Dispose(); return; }
            _busy = false;
            if (_cache.TryGetValue(_busyBucket, out var old)) old.Dispose();
            _cache[_busyBucket] = bmp;
            if (_visible && (_busyBucket == _wantBucket || !_cache.ContainsKey(_wantBucket)))
            {
                _popup.Frame = bmp;
                _popup.Invalidate();
            }
            Pump();
        }

        // ---------------- libvlc callbacks (decoder thread) ----------------

        uint OnFormat(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
        {
            double sw = Math.Max(1, width), sh = Math.Max(1, height);
            double k = Math.Min(MaxW / sw, MaxH / sh);
            int w = Math.Max(2, (int)Math.Round(sw * k)) & ~1;
            int h = Math.Max(2, (int)Math.Round(sh * k)) & ~1;
            Marshal.Copy(new[] { (byte)'R', (byte)'V', (byte)'3', (byte)'2' }, 0, chroma, 4);
            width = (uint)w;
            height = (uint)h;
            pitches = (uint)(w * 4);
            lines = (uint)h;
            lock (_bufLock)
            {
                if (_buf != IntPtr.Zero) Marshal.FreeHGlobal(_buf);
                _buf = Marshal.AllocHGlobal(w * h * 4);
                _w = w; _h = h;
            }
            return 1;
        }

        void OnCleanup(ref IntPtr opaque)
        {
            lock (_bufLock)
            {
                if (_buf != IntPtr.Zero) Marshal.FreeHGlobal(_buf);
                _buf = IntPtr.Zero;
                _w = _h = 0;
            }
        }

        IntPtr OnLock(IntPtr opaque, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, _buf);
            return IntPtr.Zero;
        }

        // lock (decoder thread) and display (vout thread) run on different threads, so the buffer is only
        // guarded during the copy; a single buffer can tear in theory, which a thumbnail can live with.
        void OnDisplay(IntPtr opaque, IntPtr picture)
        {
            Bitmap bmp = null;
            lock (_bufLock)
            {
                if (_buf != IntPtr.Zero && _w > 0)
                {
                    bmp = new Bitmap(_w, _h, PixelFormat.Format32bppRgb);
                    var data = bmp.LockBits(new Rectangle(0, 0, _w, _h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                    CopyMemory(data.Scan0, _buf, (uint)(_w * _h * 4));
                    bmp.UnlockBits(data);
                }
            }

            if (bmp == null) return;
            int gen = _gen;
            try { _owner.BeginInvoke((Action)(() => OnFrame(gen, bmp))); }
            catch { bmp.Dispose(); } // owner gone
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public void Dispose()
        {
            Close();
            _timer.Dispose();
            _popup.Dispose();
            _libVLC.Dispose();
        }

        // ---------------- popup ----------------

        sealed class PreviewPopup : Form
        {
            public Bitmap Frame;
            public string Time = "";
            const int Pad = 3, TextH = 20;
            static readonly Color Border = Color.FromArgb(90, 90, 90);
            readonly Font _font = new Font("Consolas", 10f, FontStyle.Bold);

            public PreviewPopup()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                BackColor = Color.FromArgb(20, 20, 20);
                ClientSize = new Size(MaxW + 2 * Pad, MaxH + 2 * Pad + TextH);
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000 /*WS_EX_NOACTIVATE*/ | 0x00000080 /*WS_EX_TOOLWINDOW*/ |
                                  0x00000008 /*WS_EX_TOPMOST*/ | 0x00000020 /*WS_EX_TRANSPARENT*/;
                    return cp;
                }
            }

            // never steal the mouse from the seek bar underneath
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x0084 /*WM_NCHITTEST*/) { m.Result = (IntPtr)(-1) /*HTTRANSPARENT*/; return; }
                base.WndProc(ref m);
            }

            public void ShowAt(Point p, Form owner)
            {
                Location = p;
                if (!Visible)
                {
                    if (owner != null && Owner != owner) Owner = owner;
                    Show();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(BackColor);
                var box = new Rectangle(Pad, Pad, MaxW, MaxH);
                if (Frame != null)
                {
                    double k = Math.Min(MaxW / (double)Frame.Width, MaxH / (double)Frame.Height);
                    int w = (int)(Frame.Width * k), h = (int)(Frame.Height * k);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                    g.DrawImage(Frame, new Rectangle(box.X + (MaxW - w) / 2, box.Y + (MaxH - h) / 2, w, h));
                }
                else
                {
                    using (var b = new SolidBrush(Color.Black)) g.FillRectangle(b, box);
                }
                TextRenderer.DrawText(g, Time, _font, new Rectangle(0, Pad + MaxH, ClientSize.Width, TextH + Pad),
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                using (var p = new Pen(Border)) g.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _font.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
