using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace CameraViewer
{
    sealed class MainForm : Form
    {
        readonly Config _cfg;
        readonly LibVLC _vlc;

        readonly Panel _toolbar = new Panel();
        readonly Label _hint = new Label();
        readonly Panel _gridHost = new Panel();
        readonly List<CameraTile> _tiles = new List<CameraTile>();
        readonly List<CamInfo> _shown = new List<CamInfo>();
        CameraTile _single;
        int _singleIndex = -1;

        readonly Timer _watchdog = new Timer { Interval = 3000 };
        readonly ContextMenuStrip _menu = new ContextMenuStrip();

        bool _fullscreen;
        FormWindowState _preFullState;

        static readonly Color Bg = Color.FromArgb(18, 18, 18);

        public MainForm()
        {
            _cfg = Config.Load();

            Text = "Camera Viewer";
            BackColor = Bg;
            KeyPreview = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = _cfg.WindowBounds;
            MinimumSize = new Size(640, 400);
            if (_cfg.Maximized) WindowState = FormWindowState.Maximized;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            Core.Initialize();
            _vlc = new LibVLC("--no-video-title-show", "--no-osd");
            StartLog();

            BuildToolbar();

            _gridHost.Dock = DockStyle.Fill;
            _gridHost.BackColor = Bg;
            _gridHost.Resize += (s, e) => LayoutTiles();

            _single = new CameraTile(_vlc, large: true) { Dock = DockStyle.Fill, Visible = false };
            _single.TileClicked += (s, e) =>
            {
                if (e.Button == MouseButtons.Right) _menu.Show(Cursor.Position);
                else ShowGrid();
            };

            Controls.Add(_gridHost);
            Controls.Add(_single);
            Controls.Add(_toolbar);

            _menu.Items.Add("Cài đặt camera…", null, (s, e) => OpenSettings());
            _menu.Items.Add("Toàn màn hình (F11)", null, (s, e) => ToggleFullscreen());
            _menu.Items.Add("Kết nối lại tất cả (F5)", null, (s, e) => ReloadAll());
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("Thoát", null, (s, e) => Close());
            ContextMenuStrip = _menu;

            _recheck.Tick += RecheckTick;
            _watchdog.Tick += (s, e) =>
            {
                foreach (var t in _tiles) t.Watchdog();
                if (_single.Visible) _single.Watchdog();
            };

            Shown += (s, e) =>
            {
                BuildTiles();
                _watchdog.Start();
                if (string.IsNullOrEmpty(_cfg.Password) && !_cfg.FileExisted)
                    BeginInvoke((Action)OpenSettings);
            };
        }

        void BuildToolbar()
        {
            _toolbar.Dock = DockStyle.Top;
            _toolbar.Height = 34;
            _toolbar.BackColor = Color.FromArgb(32, 32, 32);
            _toolbar.Padding = new Padding(6, 4, 6, 4);

            Button MakeButton(string text, EventHandler click)
            {
                var b = new Button
                {
                    Text = text,
                    Dock = DockStyle.Left,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.Gainsboro,
                    BackColor = Color.FromArgb(48, 48, 48),
                    Font = new Font("Segoe UI", 9f),
                    Margin = new Padding(0),
                    Cursor = Cursors.Hand,
                };
                b.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
                b.Click += click;
                return b;
            }

            _hint.Dock = DockStyle.Fill;
            _hint.ForeColor = Color.Gray;
            _hint.Font = new Font("Segoe UI", 9f);
            _hint.TextAlign = ContentAlignment.MiddleRight;

            // Dock order: last added docks first, so add right-to-left.
            _toolbar.Controls.Add(_hint);
            var gap = new Panel { Dock = DockStyle.Left, Width = 6 };
            _toolbar.Controls.Add(MakeButton("⟳  Kết nối lại", (s, e) => ReloadAll()));
            _toolbar.Controls.Add(gap);
            _toolbar.Controls.Add(MakeButton("⛶  Toàn màn hình", (s, e) => ToggleFullscreen()));
            _toolbar.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6 });
            _toolbar.Controls.Add(MakeButton("⚙  Cài đặt", (s, e) => OpenSettings()));
            UpdateHint();
        }

        void UpdateHint()
        {
            _hint.Text = _singleIndex >= 0
                ? "← → đổi camera   ·   bấm vào hình hoặc Esc để quay lại lưới   ·   M bật/tắt tiếng"
                : "Bấm vào 1 camera để xem lớn   ·   F11 toàn màn hình   ·   chuột phải để mở menu";
        }

        // ---------------------------------------------------------------- log

        readonly object _logGate = new object();
        string _logPath;
        long _logBytes;

        /// <summary>camera-viewer.log next to the exe: libvlc warnings/errors, plus the
        /// RTSP module's debug lines (minus any Authorization header) for diagnosing
        /// connection failures. Rewritten on every start, capped at 2 MB.</summary>
        void StartLog()
        {
            _logPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "camera-viewer.log");
            try { System.IO.File.WriteAllText(_logPath, $"=== Camera Viewer {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n"); }
            catch { _logPath = null; return; }

            _vlc.Log += (s, e) =>
            {
                if (e.Message == "authentication failed" && !_authFailed)
                {
                    _authFailed = true;
                    try { BeginInvoke((Action)OnAuthFailed); } catch { }
                }
                bool rtsp = e.Module != null && (e.Module.Contains("live555") || e.Module.Contains("rtsp"));
                if (e.Level < LogLevel.Warning && !rtsp) return;
                var msg = e.Message ?? "";
                if (msg.IndexOf("Authorization", StringComparison.OrdinalIgnoreCase) >= 0) msg = "(Authorization header omitted)";
                var line = $"{DateTime.Now:HH:mm:ss.fff} [{e.Level}] {e.Module}: {msg}\r\n";
                lock (_logGate)
                {
                    if (_logPath == null || _logBytes > 2_000_000) return;
                    _logBytes += line.Length;
                    try { System.IO.File.AppendAllText(_logPath, line); } catch { }
                }
            };
        }

        // ---------------------------------------------------------------- login failures

        volatile bool _authFailed;

        /// <summary>A camera/NVR rejected the login. Retrying every few seconds on 11
        /// channels would get the account locked out (Uniview locks after a handful of bad
        /// logins), so halt every tile that isn't already playing and wait for the user
        /// to fix the password (Settings → Save) or press F5.</summary>
        void OnAuthFailed()
        {
            const string msg = "sai tài khoản / mật khẩu – đã dừng thử";
            foreach (var t in _tiles)
                if (!t.IsPlaying) t.Halt(msg);
            if (_singleIndex >= 0 && !_single.IsPlaying) _single.Halt(msg);
            ShowAuthHint();
        }

        void ShowAuthHint()
        {
            _hint.ForeColor = Color.FromArgb(240, 120, 90);
            _hint.Text = "Đầu ghi/camera từ chối tài khoản hoặc mật khẩu. Sửa trong ⚙ Cài đặt rồi Lưu, hoặc F5 để thử lại. " +
                         "(Đăng nhập sai nhiều lần có thể bị khóa tạm, đợi khoảng 30 phút.)";
        }

        void ClearAuthFailed()
        {
            _authFailed = false;
            _loginOkKeys.Clear();
            _loginBadKeys.Clear();
            _hint.ForeColor = Color.Gray;
            UpdateHint();
        }

        // ---------------------------------------------------------------- grid

        void BuildTiles()
        {
            ClearAuthFailed();
            ShowGrid();
            StopAndDispose(_tiles);
            _tiles.Clear();
            _shown.Clear();
            _gridHost.Controls.Clear();

            _shown.AddRange(_cfg.Cameras.Where(c => c.Enabled && !string.IsNullOrWhiteSpace(c.Main)));
            for (int i = 0; i < _shown.Count; i++)
            {
                var tile = new CameraTile(_vlc, large: false) { Index = i };
                tile.TileClicked += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right) _menu.Show(Cursor.Position);
                    else if (e.Button == MouseButtons.Left) ShowSingle(((CameraTile)s).Index);
                };
                _tiles.Add(tile);
                _gridHost.Controls.Add(tile);
            }
            LayoutTiles();
            StartTiles();

            if (_shown.Count == 0)
            {
                var empty = new Label
                {
                    Dock = DockStyle.Fill,
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12f),
                    Text = "Chưa có camera nào. Bấm ⚙ Cài đặt để thêm.",
                };
                _gridHost.Controls.Add(empty);
            }
        }

        int _startGen;
        readonly Timer _recheck = new Timer { Interval = 15000 };
        // Outcome of the one-off login check per device + login (Config.LoginKey).
        readonly HashSet<string> _loginOkKeys = new HashSet<string>();
        readonly HashSet<string> _loginBadKeys = new HashSet<string>();

        /// <summary>
        /// Starts the grid. Cameras are grouped by device + login, and each group waits for ONE
        /// test login (RtspProbe.Check) before any of its channels connect: opening 11 NVR channels
        /// with a wrong password costs 11+ bad-login strikes per attempt, which got `viewer` locked
        /// out once. EZVIZ cameras (own admin + verification code) get the same treatment.
        /// </summary>
        /// <param name="onlyIdle">Re-check pass: leave tiles that are already connecting/playing alone.</param>
        void StartTiles(bool onlyIdle = false)
        {
            int gen = ++_startGen;
            _recheck.Stop();
            var groups = new Dictionary<string, List<int>>();
            for (int i = 0; i < _shown.Count; i++)
            {
                if (onlyIdle && _tiles[i].IsActive) continue;
                var cam = _shown[i];
                _cfg.LoginFor(cam, cam.GridUrl, out var user, out var pass);
                if (string.IsNullOrEmpty(user)) { _tiles[i].Play(cam.Name, cam.GridUrl, "", "", audio: false); continue; }
                if (string.IsNullOrEmpty(pass)) { _tiles[i].Hold(cam.Name, "chưa nhập mật khẩu (⚙ Cài đặt)"); continue; }
                var key = Config.LoginKey(cam.GridUrl, user, pass);
                if (_loginBadKeys.Contains(key)) continue;
                if (_loginOkKeys.Contains(key)) { PlayGrid(i); continue; }
                if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<int>();
                list.Add(i);
            }
            foreach (var kv in groups) CheckGroup(gen, kv.Key, kv.Value);
        }

        void PlayGrid(int i)
        {
            var cam = _shown[i];
            _cfg.LoginFor(cam, cam.GridUrl, out var user, out var pass);
            _tiles[i].Play(cam.Name, cam.GridUrl, user, pass, audio: false);
        }

        void CheckGroup(int gen, string key, List<int> idx)
        {
            foreach (int i in idx) _tiles[i].Hold(_shown[i].Name, "đang kiểm tra đăng nhập…");
            var cam = _shown[idx[0]];
            string url = cam.GridUrl;
            _cfg.LoginFor(cam, url, out var user, out var pass);

            Task.Run(() => RtspProbe.Check(url, user, pass)).ContinueWith(t =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (gen != _startGen) return; // settings changed / reloaded meanwhile
                        switch (t.Result)
                        {
                            case RtspProbe.Login.Rejected:
                                _loginBadKeys.Add(key);
                                foreach (int i in idx) _tiles[i].Hold(_shown[i].Name, "sai tài khoản / mật khẩu – đã dừng thử");
                                ShowAuthHint();
                                break;
                            case RtspProbe.Login.Unreachable:
                                foreach (int i in idx) _tiles[i].Hold(_shown[i].Name, "không kết nối được – thử lại sau 15 giây");
                                _recheck.Start();
                                break;
                            default:
                                _loginOkKeys.Add(key);
                                foreach (int i in idx) PlayGrid(i);
                                break;
                        }
                    }));
                }
                catch { }
            });
        }

        void RecheckTick(object sender, EventArgs e)
        {
            _recheck.Stop();
            StartTiles(onlyIdle: true);
        }

        void LayoutTiles()
        {
            int n = _tiles.Count;
            if (n == 0) return;
            var r = _gridHost.ClientRectangle;
            if (r.Width < 10 || r.Height < 10) return;

            int cols = _cfg.Columns > 0 ? Math.Min(_cfg.Columns, n) : BestColumns(n, r.Width, r.Height);
            int rows = (n + cols - 1) / cols;
            int w = r.Width / cols, h = r.Height / rows;

            _gridHost.SuspendLayout();
            for (int i = 0; i < n; i++)
            {
                int row = i / cols, col = i % cols;
                // Centre an incomplete last row.
                int inRow = row == rows - 1 ? n - row * cols : cols;
                int offset = (r.Width - inRow * w) / 2;
                _tiles[i].SetBounds(offset + col * w, row * h, w, h);
            }
            _gridHost.ResumeLayout();
        }

        /// <summary>Column count that makes a 16:9 picture as large as possible.</summary>
        static int BestColumns(int n, int width, int height)
        {
            int best = 1;
            double bestArea = 0;
            for (int c = 1; c <= n; c++)
            {
                int rows = (n + c - 1) / c;
                double tw = (double)width / c;
                double th = (double)height / rows - 22; // name strip
                if (th <= 0) continue;
                double vw = Math.Min(tw, th * 16 / 9);
                double area = vw * vw * 9 / 16;
                if (area > bestArea + 1) { bestArea = area; best = c; }
            }
            return best;
        }

        // ---------------------------------------------------------------- single view

        void ShowSingle(int index)
        {
            if (index < 0 || index >= _shown.Count) return;
            _singleIndex = index;
            var cam = _shown[index];
            _cfg.LoginFor(cam, cam.Main, out var user, out var pass);
            var key = Config.LoginKey(cam.Main, user, pass);
            if (string.IsNullOrEmpty(user) || _loginOkKeys.Contains(key))
                _single.Play(cam.Name, cam.Main, user, pass, audio: true);
            else
                _single.Hold(cam.Name, _loginBadKeys.Contains(key) ? "sai tài khoản / mật khẩu – đã dừng thử" : "chưa đăng nhập được");
            _single.Visible = true;
            _single.BringToFront();
            _gridHost.Visible = false;
            UpdateHint();
        }

        void ShowGrid()
        {
            if (_singleIndex < 0) return;
            _singleIndex = -1;
            _single.Stop();
            _single.Visible = false;
            _gridHost.Visible = true;
            UpdateHint();
        }

        // ---------------------------------------------------------------- commands

        void ReloadAll()
        {
            ClearAuthFailed();
            if (_singleIndex >= 0) ShowGrid();
            StartTiles();
        }

        void OpenSettings()
        {
            using (var dlg = new SettingsForm(_cfg))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
            }
            _cfg.Save();
            BuildTiles();
        }

        void ToggleFullscreen()
        {
            if (!_fullscreen)
            {
                _preFullState = WindowState;
                _toolbar.Visible = false;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal; // needed so Maximized re-applies borderless
                WindowState = FormWindowState.Maximized;
                _fullscreen = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = _preFullState;
                _toolbar.Visible = true;
                _fullscreen = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    if (_singleIndex >= 0) ShowGrid();
                    else if (_fullscreen) ToggleFullscreen();
                    return true;
                case Keys.F11:
                    ToggleFullscreen();
                    return true;
                case Keys.F5:
                    ReloadAll();
                    return true;
                case Keys.Left:
                case Keys.Right:
                    if (_singleIndex >= 0 && _shown.Count > 0)
                    {
                        int d = keyData == Keys.Left ? -1 : 1;
                        ShowSingle((_singleIndex + d + _shown.Count) % _shown.Count);
                        return true;
                    }
                    break;
                case Keys.M:
                    if (_singleIndex >= 0) { _single.ToggleMute(); return true; }
                    break;
            }
            if (_singleIndex < 0 && keyData >= Keys.D1 && keyData <= Keys.D9)
            {
                ShowSingle(keyData - Keys.D1);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------------------------------------------------------------- shutdown

        static void StopAndDispose(IEnumerable<CameraTile> tiles)
        {
            var list = tiles.ToList();
            if (list.Count == 0) return;
            Task.WaitAll(list.Select(t => t.Stop()).ToArray(), 4000);
            foreach (var t in list) t.Dispose();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _watchdog.Stop();
            _recheck.Stop();
            _startGen++;
            if (!_fullscreen)
            {
                _cfg.Maximized = WindowState == FormWindowState.Maximized;
                if (WindowState == FormWindowState.Normal) _cfg.WindowBounds = Bounds;
                else _cfg.WindowBounds = RestoreBounds;
            }
            if (_cfg.FileExisted || !string.IsNullOrEmpty(_cfg.Password)) _cfg.Save();

            Hide();
            var all = new List<CameraTile>(_tiles) { _single };
            StopAndDispose(all);
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { _vlc.Dispose(); } catch { }
            base.OnFormClosed(e);
        }

    }
}
