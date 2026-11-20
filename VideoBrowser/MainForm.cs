using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.VisualBasic.FileIO;

namespace VideoBrowser
{
    sealed class VideoItem
    {
        public string Path;
        public string Display;
        public long Size;
    }

    sealed class MainForm : Form
    {
        static readonly HashSet<string> VideoExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".m4v", ".mkv", ".avi", ".ts", ".m2ts", ".mts", ".mov", ".wmv", ".asf", ".flv", ".f4v",
            ".webm", ".mpg", ".mpeg", ".mpe", ".m2v", ".vob", ".3gp", ".3g2", ".rm", ".rmvb", ".divx",
            ".ogv", ".ogm", ".dv", ".mxf", ".tp", ".trp",
        };

        // ---- colors
        static readonly Color BgDark = Color.FromArgb(24, 24, 24);
        static readonly Color BgPanel = Color.FromArgb(32, 32, 32);
        static readonly Color BgInput = Color.FromArgb(45, 45, 45);
        static readonly Color Fg = Color.FromArgb(225, 225, 225);
        static readonly Color FgDim = Color.FromArgb(140, 140, 140);
        static readonly Color Accent = Color.FromArgb(0, 150, 255);

        // ---- state
        readonly Settings _settings = Settings.Load();
        readonly List<VideoItem> _all = new List<VideoItem>();
        List<VideoItem> _view = new List<VideoItem>();
        VideoItem _playing;
        string _folder;
        int _scanToken;
        bool _inDialog, _endPending;

        // ---- player
        readonly LibVLC _libVLC;
        readonly MediaPlayer _mp;

        // ---- UI
        readonly MenuStrip _menu = new MenuStrip();
        readonly SplitContainer _split = new SplitContainer();
        readonly VideoView _video = new VideoView();
        readonly Label _overlay = new Label();
        readonly Panel _controls = new Panel();
        readonly SeekBar _seek = new SeekBar();
        readonly SeekBar _volume = new SeekBar();
        readonly Label _time = new Label();
        readonly Button _btnPlay, _btnMute;
        readonly ListView _list = new ListView();
        readonly TextBox _filter = new TextBox();
        readonly Label _folderLabel = new Label();
        readonly Label _countLabel = new Label();
        readonly StatusStrip _status = new StatusStrip();
        readonly ToolStripStatusLabel _statusState = new ToolStripStatusLabel();
        readonly ToolStripStatusLabel _statusInfo = new ToolStripStatusLabel();
        readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = 250 };
        ToolStripMenuItem _miRecursive, _miConfirm, _miAutoNext;

        // ---- fullscreen
        bool _fullscreen;
        FormWindowState _prevState;
        Rectangle _prevBounds;

        public MainForm(string startFolder)
        {
            Text = "Video Browser";
            BackColor = BgDark;
            ForeColor = Fg;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;
            AllowDrop = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = _settings.WindowBounds;
            if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(Bounds)))
            {
                StartPosition = FormStartPosition.CenterScreen;
                Size = new Size(1400, 850);
            }
            MinimumSize = new Size(700, 400);

            Core.Initialize();
            _libVLC = new LibVLC("--no-video-title-show");
            _mp = new MediaPlayer(_libVLC) { EnableKeyInput = false, EnableMouseInput = false };
            _mp.EndReached += (s, e) => BeginInvoke((Action)OnEndReached);
            _mp.EncounteredError += (s, e) => BeginInvoke((Action)(() => SetStatus("Error: cannot play this file")));

            // ---------- menu
            BuildMenu();

            // ---------- video area (left)
            _video.Dock = DockStyle.Fill;
            _video.BackColor = Color.Black;
            _video.MediaPlayer = _mp;
            _video.DoubleClick += (s, e) => ToggleFullscreen();
            _video.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) _list.Focus(); };

            _overlay.Dock = DockStyle.Fill;
            _overlay.TextAlign = ContentAlignment.MiddleCenter;
            _overlay.ForeColor = FgDim;
            _overlay.BackColor = Color.Black;
            _overlay.Font = new Font("Segoe UI", 12f);
            _overlay.Text = "File → Open Folder… (Ctrl+O), or drop a folder here.\r\n\r\n" +
                            "Double-click / Enter: play   •   Space: pause   •   ←/→: seek   •   Del: Recycle Bin   •   F: fullscreen";
            _overlay.DoubleClick += (s, e) => ToggleFullscreen();

            _split.Dock = DockStyle.Fill;
            _split.FixedPanel = FixedPanel.Panel2;
            _split.BackColor = Color.FromArgb(50, 50, 50);
            _split.SplitterWidth = 4;
            _split.Panel1.BackColor = Color.Black;
            _split.Panel2.BackColor = BgPanel;
            _split.Panel1.Controls.Add(_video);
            _split.Panel1.Controls.Add(_overlay);
            _overlay.BringToFront();

            // ---------- playlist (right)
            BuildPlaylist();

            // ---------- controls (bottom)
            _controls.Dock = DockStyle.Bottom;
            _controls.Height = 56;
            _controls.BackColor = BgPanel;

            _seek.Dock = DockStyle.Top;
            _seek.BackColor = BgPanel;
            _seek.Enabled = false;
            _seek.Scrubbed += (s, e) => { if (_mp.IsSeekable) _mp.Position = (float)_seek.Value; UpdateTime(); };
            _seek.HoverText = v => _mp.Length > 0 ? Fmt((long)(v * _mp.Length)) : null;

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false, BackColor = BgPanel,
                Padding = new Padding(4, 2, 0, 0),
            };
            row.Controls.Add(MakeButton("⏮", "Previous (P)", PlayPrev));
            row.Controls.Add(MakeButton("⏪", "Back 10 s (Shift+←)", () => SeekBy(-10000)));
            row.Controls.Add(_btnPlay = MakeButton("▶", "Play / Pause (Space)", TogglePause));
            row.Controls.Add(MakeButton("⏹", "Stop (S)", StopPlayback));
            row.Controls.Add(MakeButton("⏩", "Forward 10 s (Shift+→)", () => SeekBy(10000)));
            row.Controls.Add(MakeButton("⏭", "Next (N)", PlayNext));
            _time.AutoSize = false;
            _time.Size = new Size(140, 30);
            _time.TextAlign = ContentAlignment.MiddleLeft;
            _time.ForeColor = Fg;
            _time.Font = new Font("Consolas", 10f);
            _time.Margin = new Padding(10, 0, 0, 0);
            _time.Text = "--:-- / --:--";
            row.Controls.Add(_time);
            var del = MakeButton("🗑", "Delete selected → Recycle Bin (Del)\r\nShift+Del = delete permanently", () => DeleteSelected(false));
            del.ForeColor = Color.FromArgb(255, 110, 110);
            row.Controls.Add(del);

            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, Width = 190, WrapContents = false, BackColor = BgPanel,
                Padding = new Padding(0, 2, 8, 0),
            };
            right.Controls.Add(_btnMute = MakeButton("🔊", "Mute (M)", ToggleMute));
            _volume.Size = new Size(140, 30);
            _volume.BackColor = BgPanel;
            _volume.Value = _settings.Volume / 100.0;
            _volume.HoverText = v => $"{(int)Math.Round(v * 100)}%";
            _volume.Scrubbed += (s, e) => SetVolume((int)Math.Round(_volume.Value * 100));
            right.Controls.Add(_volume);

            _controls.Controls.Add(row);
            _controls.Controls.Add(right);
            _controls.Controls.Add(_seek);

            // ---------- status
            _status.BackColor = BgDark;
            _status.ForeColor = FgDim;
            _status.SizingGrip = true;
            _status.Renderer = new ToolStripProfessionalRenderer(new DarkColors());
            _statusState.Text = "Stopped";
            _statusState.Width = 80;
            _statusInfo.Spring = true;
            _statusInfo.TextAlign = ContentAlignment.MiddleLeft;
            _status.Items.Add(_statusState);
            _status.Items.Add(_statusInfo);

            // dock order: fill first, then edges (last added = outermost)
            Controls.Add(_split);
            Controls.Add(_controls);
            Controls.Add(_status);
            Controls.Add(_menu);
            MainMenuStrip = _menu;

            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();

            DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
            DragDrop += (s, e) => OnDropped((string[])e.Data.GetData(DataFormats.FileDrop));

            SetVolume(_settings.Volume);

            Load += (s, e) =>
            {
                if (_settings.Maximized) WindowState = FormWindowState.Maximized;
                _split.SplitterDistance = Math.Max(200, _split.Width - _settings.PlaylistWidth - _split.SplitterWidth);
                var folder = startFolder ?? _settings.LastFolder;
                if (startFolder != null && File.Exists(startFolder))
                    _ = LoadFolder(Path.GetDirectoryName(startFolder), startFolder);
                else if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                    _ = LoadFolder(folder);
            };
        }

        // =====================================================================
        // UI building
        // =====================================================================

        void BuildMenu()
        {
            _menu.BackColor = BgPanel;
            _menu.ForeColor = Fg;
            _menu.Renderer = new ToolStripProfessionalRenderer(new DarkColors());

            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.Add(new ToolStripMenuItem("&Open Folder…", null, (s, e) => BrowseFolder()) { ShortcutKeys = Keys.Control | Keys.O });
            file.DropDownItems.Add(new ToolStripMenuItem("&Refresh", null, (s, e) => Refresh_()) { ShortcutKeys = Keys.F5 });
            file.DropDownItems.Add(new ToolStripMenuItem("Show folder in &Explorer", null, (s, e) => { if (_folder != null) Process.Start("explorer.exe", Quote(_folder)); }));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => Close()));

            var play = new ToolStripMenuItem("&Play");
            play.DropDownItems.Add(new ToolStripMenuItem("Play selected\tEnter", null, (s, e) => PlaySelected()));
            play.DropDownItems.Add(new ToolStripMenuItem("Play / Pause\tSpace", null, (s, e) => TogglePause()));
            play.DropDownItems.Add(new ToolStripMenuItem("Stop\tS", null, (s, e) => StopPlayback()));
            play.DropDownItems.Add(new ToolStripMenuItem("Next\tN", null, (s, e) => PlayNext()));
            play.DropDownItems.Add(new ToolStripMenuItem("Previous\tP", null, (s, e) => PlayPrev()));
            play.DropDownItems.Add(new ToolStripSeparator());
            play.DropDownItems.Add(new ToolStripMenuItem("Fullscreen\tF / double-click", null, (s, e) => ToggleFullscreen()));
            play.DropDownItems.Add(new ToolStripMenuItem("Mute\tM", null, (s, e) => ToggleMute()));

            var edit = new ToolStripMenuItem("&Edit");
            edit.DropDownItems.Add(new ToolStripMenuItem("Delete → Recycle Bin\tDel", null, (s, e) => DeleteSelected(false)));
            edit.DropDownItems.Add(new ToolStripMenuItem("Delete permanently\tShift+Del", null, (s, e) => DeleteSelected(true)));
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(new ToolStripMenuItem("Find / filter", null, (s, e) => { _filter.Focus(); _filter.SelectAll(); }) { ShortcutKeys = Keys.Control | Keys.F });

            var opts = new ToolStripMenuItem("&Options");
            _miRecursive = new ToolStripMenuItem("Include &subfolders", null, (s, e) =>
            {
                _settings.Recursive = _miRecursive.Checked = !_miRecursive.Checked;
                Refresh_();
            }) { Checked = _settings.Recursive };
            _miConfirm = new ToolStripMenuItem("&Confirm before moving to Recycle Bin", null, (s, e) =>
                _settings.ConfirmRecycle = _miConfirm.Checked = !_miConfirm.Checked) { Checked = _settings.ConfirmRecycle };
            _miAutoNext = new ToolStripMenuItem("&Auto-play next video", null, (s, e) =>
                _settings.AutoNext = _miAutoNext.Checked = !_miAutoNext.Checked) { Checked = _settings.AutoNext };
            opts.DropDownItems.Add(_miRecursive);
            opts.DropDownItems.Add(_miConfirm);
            opts.DropDownItems.Add(_miAutoNext);

            var help = new ToolStripMenuItem("&Help");
            help.DropDownItems.Add(new ToolStripMenuItem("Keyboard shortcuts", null, (s, e) => MessageBox.Show(this,
                "Enter / double-click\tPlay selected\r\n" +
                "Space\t\tPlay / pause\r\n" +
                "S\t\tStop\r\n" +
                "N / P\t\tNext / previous video\r\n" +
                "← / →\t\tSeek 5 s\r\n" +
                "Shift+← / →\tSeek 30 s\r\n" +
                "Ctrl+↑ / ↓\tVolume\r\n" +
                "M\t\tMute\r\n" +
                "F, F11, Alt+Enter\tFullscreen (Esc to leave)\r\n" +
                "Del\t\tMove selected to Recycle Bin\r\n" +
                "Shift+Del\tDelete selected permanently\r\n" +
                "Ctrl+F\t\tFilter the playlist\r\n" +
                "Ctrl+O\t\tOpen folder\r\n" +
                "F5\t\tRefresh",
                "Keyboard shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information)));

            _menu.Items.AddRange(new ToolStripItem[] { file, play, edit, opts, help });
            foreach (ToolStripMenuItem top in _menu.Items) ColorMenu(top);
        }

        static void ColorMenu(ToolStripMenuItem item)
        {
            item.ForeColor = Fg;
            foreach (ToolStripItem child in item.DropDownItems)
            {
                child.ForeColor = Fg;
                if (child is ToolStripMenuItem mi) ColorMenu(mi);
            }
        }

        void BuildPlaylist()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = BgPanel, Padding = new Padding(6, 4, 6, 4) };
            _folderLabel.Dock = DockStyle.Top;
            _folderLabel.Height = 20;
            _folderLabel.ForeColor = FgDim;
            _folderLabel.AutoEllipsis = true;
            _folderLabel.Text = "(no folder)";
            _filter.Dock = DockStyle.Top;
            _filter.BackColor = BgInput;
            _filter.ForeColor = Fg;
            _filter.BorderStyle = BorderStyle.FixedSingle;
            _filter.TextChanged += (s, e) => ApplyFilter();
            SetCue(_filter, "Filter… (Ctrl+F)");
            top.Controls.Add(_filter);
            top.Controls.Add(_folderLabel);

            _countLabel.Dock = DockStyle.Bottom;
            _countLabel.Height = 20;
            _countLabel.ForeColor = FgDim;
            _countLabel.Padding = new Padding(6, 2, 0, 0);

            _list.Dock = DockStyle.Fill;
            _list.View = View.Details;
            _list.HeaderStyle = ColumnHeaderStyle.None;
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.MultiSelect = true;
            _list.BorderStyle = BorderStyle.None;
            _list.BackColor = BgPanel;
            _list.ForeColor = Fg;
            _list.VirtualMode = true;
            _list.ShowItemToolTips = true;
            _list.Columns.Add("#", 50);
            _list.Columns.Add("Name", 300);
            _list.Columns.Add("Size", 70, HorizontalAlignment.Right);
            _list.RetrieveVirtualItem += OnRetrieveItem;
            _list.DoubleClick += (s, e) => PlaySelected();
            _list.Resize += (s, e) => FitColumns();
            _list.HandleCreated += (s, e) => SetWindowTheme(_list.Handle, "DarkMode_Explorer", null);

            var ctx = new ContextMenuStrip { Renderer = new ToolStripProfessionalRenderer(new DarkColors()), BackColor = BgPanel, ForeColor = Fg };
            ctx.Items.Add("Play", null, (s, e) => PlaySelected());
            ctx.Items.Add("Show in Explorer", null, (s, e) => ShowInExplorer());
            ctx.Items.Add("Copy path", null, (s, e) => CopyPaths());
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("Delete → Recycle Bin   (Del)", null, (s, e) => DeleteSelected(false));
            ctx.Items.Add("Delete permanently   (Shift+Del)", null, (s, e) => DeleteSelected(true));
            foreach (ToolStripItem i in ctx.Items) i.ForeColor = Fg;
            _list.ContextMenuStrip = ctx;

            _split.Panel2.Controls.Add(_list);
            _split.Panel2.Controls.Add(_countLabel);
            _split.Panel2.Controls.Add(top);
        }

        Button MakeButton(string glyph, string tip, Action click)
        {
            var b = new Button
            {
                Text = glyph,
                Size = new Size(38, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Fg,
                BackColor = BgPanel,
                Font = new Font("Segoe UI Symbol", 12f),
                Margin = new Padding(1, 0, 1, 0),
                TabStop = false,
                UseVisualStyleBackColor = false,
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
            b.Click += (s, e) => { click(); _list.Focus(); };
            new ToolTip().SetToolTip(b, tip);
            return b;
        }

        void FitColumns()
        {
            if (_list.Columns.Count < 3) return;
            int digits = Math.Max(3, _view.Count.ToString().Length);
            _list.Columns[0].Width = TextRenderer.MeasureText(new string('0', digits + 2), _list.Font).Width + 6;
            _list.Columns[2].Width = 72;
            _list.Columns[1].Width = Math.Max(80, _list.ClientSize.Width - _list.Columns[0].Width - _list.Columns[2].Width - 2);
        }

        // =====================================================================
        // Folder scanning
        // =====================================================================

        void BrowseFolder()
        {
            using (var dlg = new FolderBrowserDialog { Description = "Choose a folder with videos", SelectedPath = _folder ?? _settings.LastFolder ?? "" })
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _ = LoadFolder(dlg.SelectedPath);
        }

        void Refresh_()
        {
            if (_folder != null) _ = LoadFolder(_folder, _playing?.Path);
        }

        void OnDropped(string[] paths)
        {
            if (paths == null || paths.Length == 0) return;
            var p = paths[0];
            if (Directory.Exists(p)) _ = LoadFolder(p);
            else if (File.Exists(p)) _ = LoadFolder(Path.GetDirectoryName(p), p);
        }

        async Task LoadFolder(string folder, string selectPath = null)
        {
            int token = ++_scanToken;
            _folder = folder;
            _settings.LastFolder = folder;
            _folderLabel.Text = folder;
            new ToolTip().SetToolTip(_folderLabel, folder);
            _countLabel.Text = "Scanning…";
            bool recursive = _settings.Recursive;

            var items = await Task.Run(() => Scan(folder, recursive));
            if (token != _scanToken) return; // a newer scan started

            _all.Clear();
            _all.AddRange(items);
            // keep the "now playing" marker pointing at the new object for the same file
            if (_playing != null)
                _playing = _all.FirstOrDefault(i => PathEq(i.Path, _playing.Path)) ?? _playing;
            ApplyFilter();

            var sel = selectPath ?? _playing?.Path;
            int idx = sel == null ? -1 : _view.FindIndex(i => PathEq(i.Path, sel));
            SelectIndex(idx >= 0 ? idx : 0);
            if (!_filter.Focused) _list.Focus();
            if (_playing == null) Text = "Video Browser — " + folder;
        }

        static List<VideoItem> Scan(string root, bool recursive)
        {
            var result = new List<VideoItem>();
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                try
                {
                    foreach (var f in Directory.EnumerateFiles(dir))
                    {
                        if (!VideoExt.Contains(Path.GetExtension(f))) continue;
                        long size = 0;
                        try { size = new FileInfo(f).Length; } catch { }
                        var rel = f.Substring(root.Length).TrimStart('\\', '/');
                        result.Add(new VideoItem { Path = f, Display = rel, Size = size });
                    }
                    if (recursive)
                        foreach (var d in Directory.EnumerateDirectories(dir))
                        {
                            var attr = File.GetAttributes(d);
                            if ((attr & (FileAttributes.Hidden | FileAttributes.System)) == 0) stack.Push(d);
                        }
                }
                catch { /* access denied etc. — skip this folder */ }
            }
            result.Sort((a, b) => StrCmpLogicalW(a.Display, b.Display));
            return result;
        }

        void ApplyFilter()
        {
            var q = _filter.Text.Trim();
            var terms = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _view = terms.Length == 0
                ? new List<VideoItem>(_all)
                : _all.Where(i => terms.All(t => i.Display.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();

            _list.SelectedIndices.Clear();
            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            FitColumns();
            _list.Invalidate();
            UpdateCount();
        }

        void UpdateCount()
        {
            long total = _view.Sum(i => i.Size);
            _countLabel.Text = _view.Count == _all.Count
                ? $"{_all.Count} videos  •  {FmtSize(total)}"
                : $"{_view.Count} of {_all.Count} videos  •  {FmtSize(total)}";
        }

        void OnRetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // WinForms may ask for a stale index while VirtualListSize is shrinking
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                var blank = new ListViewItem("");
                blank.SubItems.Add("");
                blank.SubItems.Add("");
                e.Item = blank;
                return;
            }
            var v = _view[e.ItemIndex];
            int digits = Math.Max(3, _view.Count.ToString().Length);
            var it = new ListViewItem("[" + (e.ItemIndex + 1).ToString().PadLeft(digits, '0') + "]")
            {
                ToolTipText = v.Path,
                ForeColor = FgDim,
                UseItemStyleForSubItems = false,
            };
            var name = it.SubItems.Add(v.Display);
            var size = it.SubItems.Add(FmtSize(v.Size));
            size.ForeColor = FgDim;
            if (v == _playing)
            {
                it.ForeColor = Accent;
                name.ForeColor = Accent;
                name.Font = new Font(_list.Font, FontStyle.Bold);
            }
            else name.ForeColor = Fg;
            e.Item = it;
        }

        void SelectIndex(int idx)
        {
            _list.SelectedIndices.Clear();
            if (idx < 0 || idx >= _view.Count) return;
            _list.SelectedIndices.Add(idx);
            _list.FocusedItem = _list.Items[idx];
            _list.EnsureVisible(idx);
        }

        List<VideoItem> SelectedItems() =>
            _list.SelectedIndices.Cast<int>().OrderBy(i => i).Where(i => i < _view.Count).Select(i => _view[i]).ToList();

        // =====================================================================
        // Playback
        // =====================================================================

        void PlaySelected()
        {
            var sel = SelectedItems();
            if (sel.Count > 0) Play(sel[0]);
        }

        void Play(VideoItem item)
        {
            if (item == null) return;
            if (!File.Exists(item.Path))
            {
                SetStatus("File no longer exists: " + item.Path);
                return;
            }
            _playing = item;
            using (var media = new Media(_libVLC, item.Path, FromType.FromPath))
                _mp.Play(media);
            _overlay.Visible = false;
            _seek.Enabled = true;
            Text = "Video Browser — " + Path.GetFileName(item.Path);
            _statusInfo.Text = $"{FmtSize(item.Size)}   {item.Path}";
            _list.Invalidate();
            int idx = _view.IndexOf(item);
            if (idx >= 0) { SelectIndex(idx); }
        }

        void TogglePause()
        {
            if (_playing == null) { PlaySelected(); return; }
            var st = _mp.State;
            if (st == VLCState.Stopped || st == VLCState.Ended || st == VLCState.NothingSpecial || st == VLCState.Error)
                Play(_playing);
            else
                _mp.Pause();
        }

        void StopPlayback()
        {
            _mp.Stop();
            _seek.Value = 0;
            _seek.Enabled = false;
            UpdateTime();
        }

        void PlayNext()
        {
            if (_view.Count == 0) return;
            int idx = _playing == null ? -1 : _view.IndexOf(_playing);
            if (idx < 0) { var s = SelectedItems(); idx = s.Count > 0 ? _view.IndexOf(s[0]) - 1 : -1; }
            if (idx + 1 < _view.Count) Play(_view[idx + 1]);
        }

        void PlayPrev()
        {
            if (_view.Count == 0) return;
            int idx = _playing == null ? _view.Count : _view.IndexOf(_playing);
            if (idx < 0) idx = _view.Count;
            if (_mp.Time > 3000 && _playing != null && idx < _view.Count) { _mp.Time = 0; return; } // restart current first
            if (idx - 1 >= 0) Play(_view[idx - 1]);
        }

        void OnEndReached()
        {
            // don't change playback/selection under an open dialog (e.g. delete confirmation)
            if (_inDialog) { _endPending = true; return; }
            if (_settings.AutoNext && _playing != null)
            {
                int idx = _view.IndexOf(_playing);
                if (idx >= 0 && idx + 1 < _view.Count) { Play(_view[idx + 1]); return; }
            }
            _mp.Stop();
        }

        void SeekBy(long ms)
        {
            if (!_mp.IsSeekable || _mp.Length <= 0) return;
            _mp.Time = Math.Max(0, Math.Min(_mp.Length - 500, _mp.Time + ms));
            UpdateTime();
        }

        void SetVolume(int v)
        {
            v = Math.Max(0, Math.Min(100, v));
            _settings.Volume = v;
            _mp.Volume = v;
            if (!_volume.Dragging) _volume.Value = v / 100.0;
            if (v > 0 && _mp.Mute) _mp.Mute = false;
            UpdateMuteGlyph();
        }

        void ToggleMute()
        {
            _mp.Mute = !_mp.Mute;
            UpdateMuteGlyph();
        }

        void UpdateMuteGlyph()
        {
            _btnMute.Text = _mp.Mute || _settings.Volume == 0 ? "🔇" : "🔊";
        }

        void UpdateTime()
        {
            long len = _mp.Length, t = _mp.Time;
            var st = _mp.State;
            bool active = st == VLCState.Playing || st == VLCState.Paused || st == VLCState.Buffering || st == VLCState.Opening;
            if (active && len > 0)
            {
                _time.Text = Fmt(t) + " / " + Fmt(len);
                if (!_seek.Dragging) _seek.Value = t / (double)len;
            }
            else if (!active)
            {
                _time.Text = "--:-- / --:--";
            }
            _btnPlay.Text = st == VLCState.Playing ? "⏸" : "▶";
            _statusState.Text = st == VLCState.NothingSpecial ? "Ready" : st.ToString();
        }

        void SetStatus(string text) => _statusInfo.Text = text;

        // =====================================================================
        // Delete
        // =====================================================================

        void DeleteSelected(bool permanent)
        {
            var items = SelectedItems();
            if (items.Count == 0) return;

            bool endPending = false;
            if (permanent || _settings.ConfirmRecycle)
            {
                var what = items.Count == 1
                    ? "\"" + Path.GetFileName(items[0].Path) + "\"  (" + FmtSize(items[0].Size) + ")"
                    : items.Count + " files  (" + FmtSize(items.Sum(i => i.Size)) + ")";
                var msg = permanent
                    ? "PERMANENTLY delete " + what + "?\r\n\r\nThis cannot be undone."
                    : "Move " + what + " to the Recycle Bin?";
                DialogResult r;
                _inDialog = true;
                try
                {
                    r = MessageBox.Show(this, msg, permanent ? "Delete permanently" : "Move to Recycle Bin",
                        MessageBoxButtons.YesNo, permanent ? MessageBoxIcon.Warning : MessageBoxIcon.Question,
                        permanent ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1);
                }
                finally { _inDialog = false; }
                if (r != DialogResult.Yes)
                {
                    if (_endPending) { _endPending = false; OnEndReached(); }
                    return;
                }
                endPending = _endPending;
                _endPending = false;
            }

            int firstIdx = _view.IndexOf(items[0]);
            bool deletingPlaying = _playing != null && items.Contains(_playing);
            bool wasRunning = deletingPlaying && (_mp.State == VLCState.Playing || _mp.State == VLCState.Paused ||
                                                  _mp.State == VLCState.Buffering || (endPending && _settings.AutoNext));
            if (deletingPlaying)
            {
                _mp.Stop();          // releases the file handle
                _playing = null;
                _overlay.Visible = true;
                _seek.Value = 0;
                _seek.Enabled = false;
                Text = "Video Browser — " + _folder;
            }

            int ok = 0;
            var errors = new StringBuilder();
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (var item in items)
                {
                    try
                    {
                        WaitUntilUnlocked(item.Path);
                        if (permanent)
                            File.Delete(item.Path);
                        else
                            FileSystem.DeleteFile(item.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        _all.Remove(item);
                        ok++;
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        if (!File.Exists(item.Path)) { _all.Remove(item); continue; }
                        errors.AppendLine(Path.GetFileName(item.Path) + ": " + ex.Message);
                    }
                }
            }
            finally { Cursor = Cursors.Default; }

            ApplyFilter();
            int next = Math.Min(firstIdx, _view.Count - 1);
            SelectIndex(next);
            SetStatus($"{(permanent ? "Deleted" : "Moved to Recycle Bin")}: {ok} file(s)");

            if (errors.Length > 0)
                MessageBox.Show(this, errors.ToString(), "Some files could not be deleted", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // keep the flow going: if we just deleted what was playing, play the next one
            if (wasRunning && next >= 0) Play(_view[next]);
            else if (endPending && !deletingPlaying) OnEndReached();
        }

        static void WaitUntilUnlocked(string path)
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) return;
                }
                catch (FileNotFoundException) { return; }
                catch (IOException) { Thread.Sleep(100); }
                catch { return; }
            }
        }

        // =====================================================================
        // Misc actions
        // =====================================================================

        void ShowInExplorer()
        {
            var sel = SelectedItems();
            if (sel.Count > 0) Process.Start("explorer.exe", "/select," + Quote(sel[0].Path));
        }

        void CopyPaths()
        {
            var sel = SelectedItems();
            if (sel.Count > 0) Clipboard.SetText(string.Join(Environment.NewLine, sel.Select(i => i.Path)));
        }

        void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
            SuspendLayout();
            if (_fullscreen)
            {
                _prevState = WindowState;
                _prevBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                _menu.Visible = false;
                _controls.Visible = false;
                _status.Visible = false;
                _split.Panel2Collapsed = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Bounds = Screen.FromControl(this).Bounds;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                _menu.Visible = true;
                _controls.Visible = true;
                _status.Visible = true;
                _split.Panel2Collapsed = false;
                Bounds = _prevBounds;
                WindowState = _prevState;
            }
            ResumeLayout();
            _list.Focus();
        }

        // =====================================================================
        // Keyboard
        // =====================================================================

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_filter.Focused)
            {
                if (keyData == Keys.Escape) { if (_filter.Text.Length > 0) _filter.Clear(); else _list.Focus(); return true; }
                if (keyData == Keys.Enter || keyData == Keys.Down) { _list.Focus(); if (_list.SelectedIndices.Count == 0) SelectIndex(0); return true; }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (keyData)
            {
                case Keys.Space: TogglePause(); return true;
                case Keys.Enter: PlaySelected(); return true;
                case Keys.Left: SeekBy(-5000); return true;
                case Keys.Right: SeekBy(5000); return true;
                case Keys.Shift | Keys.Left: SeekBy(-30000); return true;
                case Keys.Shift | Keys.Right: SeekBy(30000); return true;
                case Keys.Control | Keys.Up: SetVolume(_settings.Volume + 5); return true;
                case Keys.Control | Keys.Down: SetVolume(_settings.Volume - 5); return true;
                case Keys.Delete: DeleteSelected(false); return true;
                case Keys.Shift | Keys.Delete: DeleteSelected(true); return true;
                case Keys.N: case Keys.MediaNextTrack: PlayNext(); return true;
                case Keys.P: case Keys.MediaPreviousTrack: PlayPrev(); return true;
                case Keys.MediaPlayPause: TogglePause(); return true;
                case Keys.S: case Keys.MediaStop: StopPlayback(); return true;
                case Keys.M: ToggleMute(); return true;
                case Keys.F: case Keys.F11: case Keys.Alt | Keys.Enter: ToggleFullscreen(); return true;
                case Keys.Escape: if (_fullscreen) { ToggleFullscreen(); return true; } break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // =====================================================================
        // Lifecycle
        // =====================================================================

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int on = 1;
            DwmSetWindowAttribute(Handle, 20, ref on, sizeof(int)); // dark title bar
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_fullscreen) ToggleFullscreen();
            _settings.Maximized = WindowState == FormWindowState.Maximized;
            _settings.WindowBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            _settings.PlaylistWidth = _split.Panel2.Width;
            _settings.Save();
            _timer.Stop();
            _mp.Stop();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _video.MediaPlayer = null;
            _mp.Dispose();
            _libVLC.Dispose();
            base.OnFormClosed(e);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        static string Fmt(long ms)
        {
            if (ms < 0) ms = 0;
            var t = TimeSpan.FromMilliseconds(ms);
            return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes:00}:{t.Seconds:00}";
        }

        static string FmtSize(long b)
        {
            if (b >= 1L << 30) return (b / (double)(1L << 30)).ToString("0.00") + " GB";
            if (b >= 1L << 20) return (b / (double)(1L << 20)).ToString("0") + " MB";
            if (b >= 1L << 10) return (b / (double)(1L << 10)).ToString("0") + " KB";
            return b + " B";
        }

        static bool PathEq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        static string Quote(string s) => "\"" + s + "\"";

        static void SetCue(TextBox tb, string cue) =>
            tb.HandleCreated += (s, e) => SendMessage(tb.Handle, 0x1501 /*EM_SETCUEBANNER*/, (IntPtr)1, cue);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        static extern int StrCmpLogicalW(string a, string b);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowTheme(IntPtr hwnd, string app, string idList);

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
    }

    sealed class DarkColors : ProfessionalColorTable
    {
        static readonly Color Bg = Color.FromArgb(32, 32, 32);
        static readonly Color Hi = Color.FromArgb(60, 60, 60);
        static readonly Color Border = Color.FromArgb(70, 70, 70);
        public override Color MenuStripGradientBegin => Bg;
        public override Color MenuStripGradientEnd => Bg;
        public override Color ToolStripDropDownBackground => Bg;
        public override Color ImageMarginGradientBegin => Bg;
        public override Color ImageMarginGradientMiddle => Bg;
        public override Color ImageMarginGradientEnd => Bg;
        public override Color MenuItemSelected => Hi;
        public override Color MenuItemBorder => Hi;
        public override Color MenuBorder => Border;
        public override Color MenuItemSelectedGradientBegin => Hi;
        public override Color MenuItemSelectedGradientEnd => Hi;
        public override Color MenuItemPressedGradientBegin => Hi;
        public override Color MenuItemPressedGradientMiddle => Hi;
        public override Color MenuItemPressedGradientEnd => Hi;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Border;
        public override Color StatusStripGradientBegin => Color.FromArgb(24, 24, 24);
        public override Color StatusStripGradientEnd => Color.FromArgb(24, 24, 24);
        public override Color CheckBackground => Hi;
        public override Color CheckSelectedBackground => Border;
        public override Color CheckPressedBackground => Border;
    }
}
