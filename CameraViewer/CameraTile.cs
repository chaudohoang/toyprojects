using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace CameraViewer
{
    /// <summary>One camera: a video surface with a name strip under it, and its own
    /// player that reconnects by itself (see <see cref="Watchdog"/>).</summary>
    sealed class CameraTile : Panel
    {
        readonly LibVLC _vlc;
        readonly MediaPlayer _mp;
        readonly VideoView _view;
        readonly Label _label;
        readonly object _gate = new object();

        Media _media;
        string _name = "", _url, _user, _pass;
        bool _audio;
        volatile bool _wanted;
        bool _disposed;
        volatile string _status = "";

        DateTime _lastStart = DateTime.MinValue, _lastProgress = DateTime.Now;
        int _lastDecoded = -1;
        long _lastTime = -1;

        public event MouseEventHandler TileClicked;
        public int Index { get; set; }

        public CameraTile(LibVLC vlc, bool large)
        {
            _vlc = vlc;
            BackColor = Color.FromArgb(10, 10, 10);
            Padding = new Padding(1);

            _view = new VideoView { Dock = DockStyle.Fill, BackColor = Color.Black };
            _mp = new MediaPlayer(vlc) { EnableKeyInput = false, EnableMouseInput = false };
            _view.MediaPlayer = _mp;

            _label = new Label
            {
                Dock = DockStyle.Bottom,
                Height = large ? 30 : 22,
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(28, 28, 28),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                Font = new Font("Segoe UI", large ? 11f : 9f),
                AutoEllipsis = true,
            };

            Controls.Add(_view);
            Controls.Add(_label);

            // MouseUp, not MouseClick: WinForms only raises Click when WindowFromPoint is the
            // control itself, and libvlc's (disabled) child window sits on top of it.
            _view.MouseUp += (s, e) => TileClicked?.Invoke(this, e);
            _label.MouseUp += (s, e) => TileClicked?.Invoke(this, e);
            MouseUp += (s, e) => TileClicked?.Invoke(this, e);

            // libvlc raises these on its own thread: only record and repaint the label here.
            _mp.Playing += (s, e) => SetStatus("");
            _mp.EncounteredError += (s, e) => SetStatus("lỗi kết nối – đang thử lại…");
            _mp.EndReached += (s, e) => SetStatus("mất tín hiệu – đang thử lại…");
        }

        public string CameraName => _name;

        public void Play(string name, string url, string user, string pass, bool audio)
        {
            _name = name ?? "";
            _url = url;
            _user = user;
            _pass = pass;
            _audio = audio;
            if (string.IsNullOrWhiteSpace(url)) { Halt("chưa có địa chỉ"); return; }
            // Shared NVR login without a password: don't knock on the NVR with an empty one,
            // repeated failed logins get the account/IP locked out.
            if (!HasUserInfo(url) && !string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
            { Halt("chưa nhập mật khẩu (⚙ Cài đặt)"); return; }
            _wanted = true;
            SetStatus("đang kết nối…");
            Restart();
        }

        /// <summary>Show the name and a status without connecting.</summary>
        public void Hold(string name, string status)
        {
            _name = name ?? "";
            _audio = false;
            Halt(status);
        }

        /// <summary>Stop and stay stopped (no watchdog retries) until Play is called again.</summary>
        public void Halt(string status)
        {
            Stop();
            SetStatus(status);
        }

        public bool IsPlaying => _mp.State == VLCState.Playing;
        public bool IsActive => _wanted;

        public Task Stop()
        {
            _wanted = false;
            return Task.Run(() =>
            {
                lock (_gate)
                {
                    try { _mp.Stop(); } catch { }
                    _media?.Dispose();
                    _media = null;
                }
            });
        }

        /// <summary>Stop + play off the UI thread: libvlc's Stop can block while the
        /// video output thread waits on the window, which would freeze the form.</summary>
        void Restart()
        {
            _lastStart = DateTime.Now;
            _lastProgress = DateTime.Now;
            _lastDecoded = -1;
            _lastTime = -1;
            string url = _url, user = _user, pass = _pass;
            bool audio = _audio;

            Task.Run(() =>
            {
                lock (_gate)
                {
                    try
                    {
                        _mp.Stop();
                        _media?.Dispose();
                        _media = null;
                        if (!_wanted) return;

                        var m = new Media(_vlc, url, FromType.FromLocation);
                        m.AddOption(":network-caching=500");
                        m.AddOption(":rtsp-tcp");
                        if (!audio) m.AddOption(":no-audio");
                        // Credentials written in the URL itself (rtsp://user:pass@host) win.
                        if (!HasUserInfo(url) && !string.IsNullOrEmpty(user))
                        {
                            m.AddOption(":rtsp-user=" + user);
                            m.AddOption(":rtsp-pwd=" + (pass ?? ""));
                        }
                        _media = m;
                        _mp.Mute = !audio;
                        _mp.Play(m);
                    }
                    catch (Exception ex)
                    {
                        SetStatus("lỗi: " + ex.Message);
                    }
                }
            });
        }

        static bool HasUserInfo(string url)
        {
            int scheme = url.IndexOf("://", StringComparison.Ordinal);
            if (scheme < 0) return false;
            int slash = url.IndexOf('/', scheme + 3);
            int at = url.IndexOf('@', scheme + 3);
            return at > 0 && (slash < 0 || at < slash);
        }

        /// <summary>Called every few seconds from the UI thread. Restarts a player that
        /// ended, failed, never got going, or stopped producing frames.</summary>
        public void Watchdog()
        {
            if (!_wanted) return;
            var since = (DateTime.Now - _lastStart).TotalSeconds;
            var st = _mp.State;

            switch (st)
            {
                case VLCState.Ended:
                case VLCState.Error:
                case VLCState.Stopped:
                case VLCState.NothingSpecial:
                    if (since > 6) Restart();
                    return;
                case VLCState.Opening:
                case VLCState.Buffering:
                    if (since > 25) { SetStatus("không phản hồi – đang thử lại…"); Restart(); }
                    return;
                case VLCState.Playing:
                    bool progressed = false;
                    if (System.Threading.Monitor.TryEnter(_gate))
                    {
                        try
                        {
                            if (_media != null)
                            {
                                int dec = _media.Statistics.DecodedVideo;
                                if (dec != _lastDecoded) { _lastDecoded = dec; progressed = true; }
                            }
                        }
                        catch { progressed = true; }
                        finally { System.Threading.Monitor.Exit(_gate); }
                    }
                    else progressed = true;

                    long t = _mp.Time;
                    if (t != _lastTime) { _lastTime = t; progressed = true; }

                    if (progressed) _lastProgress = DateTime.Now;
                    else if ((DateTime.Now - _lastProgress).TotalSeconds > 20)
                    {
                        SetStatus("đứng hình – đang kết nối lại…");
                        Restart();
                    }
                    return;
            }
        }

        public bool AudioOn => _audio && !_mp.Mute;

        public void ToggleMute()
        {
            if (!_audio) return;
            _mp.Mute = !_mp.Mute;
            UpdateLabel();
        }

        void SetStatus(string s)
        {
            _status = s;
            if (IsHandleCreated && !IsDisposed)
                try { BeginInvoke((Action)UpdateLabel); } catch { }
        }

        void UpdateLabel()
        {
            string text = _name;
            if (_audio) text += _mp.Mute ? "   🔇 tắt tiếng (M)" : "   🔊 (M để tắt tiếng)";
            if (!string.IsNullOrEmpty(_status)) text += "   — " + _status;
            _label.Text = text;
            _label.ForeColor = string.IsNullOrEmpty(_status) ? Color.Gainsboro : Color.FromArgb(240, 180, 90);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                // The player must be stopped before its window goes away. Callers normally
                // Stop() every tile first and wait; this is the fallback for a lone tile.
                Stop().Wait(3000);
                _view.MediaPlayer = null;
                try { _mp.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
