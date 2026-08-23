// RomLauncher.cs - single-file WinForms ROM launcher (LaunchBox-lite)
// Build with build.bat (csc.exe from .NET Framework 4.x). No VS, no NuGet.
// C# 5 compatible on purpose (Framework csc does not support C# 6+ syntax).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RomLauncher
{
    // ------------------------------------------------------------------ model

    class RomEntry
    {
        public string Name;        // file name without extension
        public string LowerName;   // lower-cased, for matching
        public string CleanName;   // lower-cased, (tags) and [tags] stripped
        public string FullPath;
        public string SystemName;
        public string LowerSystem;
        public string Ext;         // lower-cased, with dot
        public int PlayCount;
        public DateTime LastPlayed;
        public bool Fav;

        public int Score;          // transient, recomputed on every search
    }

    class SystemConfig
    {
        public string Name = "";
        public string Folder = "";
        public string Emulator = "";
        public string Args = "\"{rom}\"";
        public string Extensions = "";   // csv whitelist, empty = auto
        public bool Enabled = true;
    }

    class AppConfig
    {
        public string RootPath = @"F:\Games\Roms";
        public string RetroArchPath = "";
        public bool MinimizeOnLaunch = true;
        public bool HideDiscDupes = true;
        public int SortMode = 0;
        public string LastSystem = "";
        public List<SystemConfig> Systems = new List<SystemConfig>();

        public SystemConfig Find(string name)
        {
            foreach (SystemConfig s in Systems)
                if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) return s;
            return null;
        }
    }

    // ------------------------------------------------------------------ paths

    static class Paths
    {
        public static string ExeDir
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }
        public static string Ini { get { return Path.Combine(ExeDir, "RomLauncher.ini"); } }
        public static string Cache { get { return Path.Combine(ExeDir, "RomLauncher.cache.tsv"); } }
        public static string Stats { get { return Path.Combine(ExeDir, "RomLauncher.stats.tsv"); } }
    }

    // ------------------------------------------------------------------- ini

    static class Ini
    {
        public static AppConfig Load()
        {
            AppConfig cfg = new AppConfig();
            if (!File.Exists(Paths.Ini)) return cfg;

            string section = "";
            SystemConfig cur = null;

            foreach (string raw in File.ReadAllLines(Paths.Ini, Encoding.UTF8))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2);
                    if (section.StartsWith("System:", StringComparison.OrdinalIgnoreCase))
                    {
                        cur = new SystemConfig();
                        cur.Name = section.Substring(7);
                        cfg.Systems.Add(cur);
                    }
                    else cur = null;
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();

                if (cur != null)
                {
                    if (Eq(key, "Folder")) cur.Folder = val;
                    else if (Eq(key, "Emulator")) cur.Emulator = val;
                    else if (Eq(key, "Args")) cur.Args = val;
                    else if (Eq(key, "Extensions")) cur.Extensions = val;
                    else if (Eq(key, "Enabled")) cur.Enabled = val != "0";
                }
                else if (Eq(section, "General"))
                {
                    if (Eq(key, "RootPath")) cfg.RootPath = val;
                    else if (Eq(key, "RetroArchPath")) cfg.RetroArchPath = val;
                    else if (Eq(key, "MinimizeOnLaunch")) cfg.MinimizeOnLaunch = val != "0";
                    else if (Eq(key, "HideDiscDupes")) cfg.HideDiscDupes = val != "0";
                    else if (Eq(key, "SortMode")) int.TryParse(val, out cfg.SortMode);
                    else if (Eq(key, "LastSystem")) cfg.LastSystem = val;
                }
            }
            return cfg;
        }

        public static void Save(AppConfig cfg)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; RomLauncher settings - portable, lives next to the exe");
            sb.AppendLine("[General]");
            sb.AppendLine("RootPath=" + cfg.RootPath);
            sb.AppendLine("RetroArchPath=" + cfg.RetroArchPath);
            sb.AppendLine("MinimizeOnLaunch=" + (cfg.MinimizeOnLaunch ? "1" : "0"));
            sb.AppendLine("HideDiscDupes=" + (cfg.HideDiscDupes ? "1" : "0"));
            sb.AppendLine("SortMode=" + cfg.SortMode.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine("LastSystem=" + cfg.LastSystem);

            foreach (SystemConfig s in cfg.Systems)
            {
                sb.AppendLine();
                sb.AppendLine("[System:" + s.Name + "]");
                sb.AppendLine("Folder=" + s.Folder);
                sb.AppendLine("Emulator=" + s.Emulator);
                sb.AppendLine("Args=" + s.Args);
                sb.AppendLine("Extensions=" + s.Extensions);
                sb.AppendLine("Enabled=" + (s.Enabled ? "1" : "0"));
            }
            try { File.WriteAllText(Paths.Ini, sb.ToString(), Encoding.UTF8); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        static bool Eq(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }

    // --------------------------------------------------------------- scanning

    static class Scanner
    {
        static readonly HashSet<string> IgnoreExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".txt",".dat",".xml",".json",".ini",".cfg",".conf",".log",".nfo",".diz",".md",".pdf",
            ".png",".jpg",".jpeg",".bmp",".gif",".webp",".ico",".mp4",".avi",".mkv",".mp3",".wav",
            ".sav",".srm",".state",".ss0",".ss1",".rtc",".nv",".mcr",".mcd",".vmp",".gme",".bak",
            ".db",".sqlite",".xdelta",".ips",".bps",".ups",".patch",".torrent",".part",".tmp",
            ".sub",".ccd",".img",".bin",".raw",".mds",".dll",".sh"
        };

        // Disc-image family, used for de-duplicating multi-file sets.
        static readonly string[] DiscPriority = { ".m3u", ".chd", ".cue", ".gdi", ".cdi", ".iso", ".pbp", ".nrg" };

        static readonly HashSet<string> IgnoreDir = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "media","images","image","artwork","art","boxart","snap","snaps","titles","videos","video",
            "manuals","cheats","saves","savestates","states","bios","system","systems","!bios","screenshots",
            "downloaded_images","downloaded_videos","gamelists","overlays","themes","hashes","_metadata"
        };

        public static List<RomEntry> Scan(AppConfig cfg, Action<string> progress, Func<bool> cancelled)
        {
            List<RomEntry> all = new List<RomEntry>();

            foreach (SystemConfig sys in cfg.Systems)
            {
                if (!sys.Enabled) continue;
                if (string.IsNullOrEmpty(sys.Folder) || !Directory.Exists(sys.Folder)) continue;
                if (cancelled()) break;

                progress("Scanning " + sys.Name + " ...");

                HashSet<string> white = null;
                if (!string.IsNullOrEmpty(sys.Extensions))
                {
                    white = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string e in sys.Extensions.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        white.Add(e.StartsWith(".") ? e.Trim() : "." + e.Trim());
                }

                Stack<string> dirs = new Stack<string>();
                dirs.Push(sys.Folder);

                while (dirs.Count > 0)
                {
                    if (cancelled()) break;
                    string dir = dirs.Pop();

                    string[] subs;
                    try { subs = Directory.GetDirectories(dir); }
                    catch (Exception) { subs = new string[0]; }
                    foreach (string sub in subs)
                    {
                        string n = Path.GetFileName(sub);
                        if (n.Length == 0 || n.StartsWith(".")) continue;
                        if (IgnoreDir.Contains(n)) continue;
                        dirs.Push(sub);
                    }

                    string[] files;
                    try { files = Directory.GetFiles(dir); }
                    catch (Exception) { continue; }

                    List<string> keep = new List<string>();
                    foreach (string f in files)
                    {
                        string ext = Path.GetExtension(f);
                        if (ext.Length == 0) continue;
                        if (white != null) { if (!white.Contains(ext)) continue; }
                        else if (IgnoreExt.Contains(ext)) continue;
                        keep.Add(f);
                    }

                    if (cfg.HideDiscDupes) keep = Collapse(keep);

                    foreach (string f in keep)
                        all.Add(MakeEntry(f, sys.Name));
                }
            }
            return all;
        }

        // Collapse multi-disc / multi-format sets down to one launchable file.
        static List<string> Collapse(List<string> files)
        {
            bool hasM3u = false;
            foreach (string f in files)
                if (string.Equals(Path.GetExtension(f), ".m3u", StringComparison.OrdinalIgnoreCase)) { hasM3u = true; break; }

            List<string> stage = new List<string>();
            foreach (string f in files)
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                // An .m3u in the folder means the folder is one multi-disc game.
                if (hasM3u && ext != ".m3u" && Array.IndexOf(DiscPriority, ext) >= 0) continue;
                stage.Add(f);
            }

            // Same base name, several formats -> keep the best one.
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string f in stage)
            {
                string b = Path.GetFileNameWithoutExtension(f);
                List<string> g;
                if (!groups.TryGetValue(b, out g)) { g = new List<string>(); groups[b] = g; }
                g.Add(f);
            }

            List<string> result = new List<string>();
            foreach (KeyValuePair<string, List<string>> kv in groups)
            {
                if (kv.Value.Count == 1) { result.Add(kv.Value[0]); continue; }

                int best = int.MaxValue; string bestFile = null;
                foreach (string f in kv.Value)
                {
                    int p = Array.IndexOf(DiscPriority, Path.GetExtension(f).ToLowerInvariant());
                    if (p >= 0 && p < best) { best = p; bestFile = f; }
                }
                if (bestFile != null) result.Add(bestFile);   // disc set collapsed
                else result.AddRange(kv.Value);               // not disc-related, keep all
            }
            return result;
        }

        public static RomEntry MakeEntry(string file, string system)
        {
            RomEntry e = new RomEntry();
            e.FullPath = file;
            e.Name = Path.GetFileNameWithoutExtension(file);
            e.LowerName = e.Name.ToLowerInvariant();
            e.CleanName = StripTags(e.LowerName);
            e.SystemName = system;
            e.LowerSystem = system.ToLowerInvariant();
            e.Ext = Path.GetExtension(file).ToLowerInvariant();
            e.LastPlayed = DateTime.MinValue;
            return e;
        }

        // "Chrono Trigger (USA) [!]" -> "chrono trigger"
        static string StripTags(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            int depth = 0;
            foreach (char c in s)
            {
                if (c == '(' || c == '[' || c == '{') depth++;
                else if (c == ')' || c == ']' || c == '}') { if (depth > 0) depth--; }
                else if (depth == 0) sb.Append(c);
            }
            return sb.ToString().Trim();
        }
    }

    // ------------------------------------------------------------ persistence

    static class Store
    {
        public static void SaveCache(string root, List<RomEntry> list)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(Paths.Cache, false, Encoding.UTF8))
                {
                    w.WriteLine("#root\t" + root);
                    foreach (RomEntry e in list)
                        w.WriteLine(e.SystemName + "\t" + e.FullPath);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static List<RomEntry> LoadCache(string root, out DateTime stamp)
        {
            stamp = DateTime.MinValue;
            List<RomEntry> list = new List<RomEntry>();
            if (!File.Exists(Paths.Cache)) return list;
            try
            {
                string[] lines = File.ReadAllLines(Paths.Cache, Encoding.UTF8);
                if (lines.Length == 0) return list;
                if (!lines[0].StartsWith("#root\t")) return list;
                if (!string.Equals(lines[0].Substring(6), root, StringComparison.OrdinalIgnoreCase)) return list;

                for (int i = 1; i < lines.Length; i++)
                {
                    int t = lines[i].IndexOf('\t');
                    if (t <= 0) continue;
                    list.Add(Scanner.MakeEntry(lines[i].Substring(t + 1), lines[i].Substring(0, t)));
                }
                stamp = File.GetLastWriteTime(Paths.Cache);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); list.Clear(); }
            return list;
        }

        public static void SaveStats(List<RomEntry> list)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(Paths.Stats, false, Encoding.UTF8))
                    foreach (RomEntry e in list)
                    {
                        if (e.PlayCount == 0 && !e.Fav) continue;
                        w.WriteLine(e.FullPath + "\t" + e.PlayCount.ToString(CultureInfo.InvariantCulture)
                            + "\t" + e.LastPlayed.Ticks.ToString(CultureInfo.InvariantCulture)
                            + "\t" + (e.Fav ? "1" : "0"));
                    }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static void ApplyStats(List<RomEntry> list)
        {
            if (!File.Exists(Paths.Stats)) return;
            Dictionary<string, string[]> map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string line in File.ReadAllLines(Paths.Stats, Encoding.UTF8))
                {
                    string[] p = line.Split('\t');
                    if (p.Length >= 4) map[p[0]] = p;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); return; }

            foreach (RomEntry e in list)
            {
                string[] p;
                if (!map.TryGetValue(e.FullPath, out p)) continue;
                int c; long ticks;
                if (int.TryParse(p[1], out c)) e.PlayCount = c;
                if (long.TryParse(p[2], out ticks) && ticks > 0 && ticks <= DateTime.MaxValue.Ticks)
                    e.LastPlayed = new DateTime(ticks);
                e.Fav = p[3] == "1";
            }
        }
    }

    // -------------------------------------------------------------- searching

    static class Fuzzy
    {
        // Returns a score, or -1 when the needle does not match at all.
        public static int Match(string hay, string needle)
        {
            if (needle.Length == 0) return 0;
            if (hay.Length == 0) return -1;

            if (hay == needle) return 1000;

            int idx = hay.IndexOf(needle, StringComparison.Ordinal);
            if (idx == 0) return 800 - Math.Min(100, hay.Length - needle.Length);
            if (idx > 0)
            {
                char prev = hay[idx - 1];
                bool wordStart = prev == ' ' || prev == '-' || prev == '_' || prev == '.' || prev == '(' || prev == '[';
                return (wordStart ? 700 : 600) - Math.Min(80, idx) - Math.Min(60, (hay.Length - needle.Length) / 2);
            }

            // Subsequence fallback: "sm64" matches "super mario 64".
            int score = 380, h = 0, lastHit = -2;
            for (int n = 0; n < needle.Length; n++)
            {
                char c = needle[n];
                int found = -1;
                for (; h < hay.Length; h++)
                    if (hay[h] == c) { found = h; break; }
                if (found < 0) return -1;

                if (found == lastHit + 1) score += 10;
                if (found == 0) score += 12;
                else
                {
                    char prev = hay[found - 1];
                    if (prev == ' ' || prev == '-' || prev == '_' || prev == '.') score += 14;
                }
                score -= Math.Min(6, found - lastHit - 1);
                lastHit = found;
                h = found + 1;
            }
            return Math.Max(1, score - Math.Min(60, hay.Length / 3));
        }

        // Every token must hit the name or the system name (AND semantics).
        public static int Score(RomEntry e, string[] tokens)
        {
            int total = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                string t = tokens[i];
                int best = Match(e.LowerName, t);
                int c = Match(e.CleanName, t);
                if (c > best) best = c;
                int s = Match(e.LowerSystem, t);
                if (s > 0)
                {
                    s = s / 2 + 100;           // system hits count, but weaker than title hits
                    if (s > best) best = s;
                }
                if (best < 0) return -1;
                total += best;
            }
            return total / tokens.Length;
        }
    }

    // ------------------------------------------------------------- list view

    class FastListView : ListView
    {
        public FastListView()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }

    // -------------------------------------------------------------- main form

    class MainForm : Form
    {
        const int SORT_RELEVANCE = 0, SORT_NAME = 1, SORT_SYSTEM = 2, SORT_RECENT = 3, SORT_PLAYED = 4;

        AppConfig cfg;
        List<RomEntry> all = new List<RomEntry>();
        List<RomEntry> view = new List<RomEntry>();

        TextBox txtSearch;
        ComboBox cboSystem, cboSort;
        CheckBox chkFav;
        Button btnRescan, btnSystems, btnLaunch;
        FastListView list;
        StatusStrip status;
        ToolStripStatusLabel lblStatus, lblCount;
        System.Windows.Forms.Timer debounce;
        Thread scanThread;
        volatile bool cancelScan;
        bool resizing;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public MainForm()
        {
            cfg = Ini.Load();

            Text = "ROM Launcher";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 620);
            MinimumSize = new Size(720, 400);
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;

            BuildUi();
            Load += delegate { LoadInitial(); };
        }

        // ----------------------------------------------------------- ui setup

        void BuildUi()
        {
            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Height = 76;
            top.Padding = new Padding(8, 8, 8, 4);
            Controls.Add(top);

            txtSearch = new TextBox();
            txtSearch.Font = new Font("Segoe UI", 14f);
            txtSearch.Location = new Point(8, 8);
            txtSearch.Width = 520;
            txtSearch.TextChanged += delegate { debounce.Stop(); debounce.Start(); };
            txtSearch.KeyDown += SearchKeyDown;
            top.Controls.Add(txtSearch);

            btnLaunch = new Button();
            btnLaunch.Text = "Launch";
            btnLaunch.Size = new Size(90, 32);
            btnLaunch.Location = new Point(536, 9);
            btnLaunch.Click += delegate { LaunchSelected(); };
            top.Controls.Add(btnLaunch);

            btnRescan = new Button();
            btnRescan.Text = "Rescan (F5)";
            btnRescan.Size = new Size(100, 32);
            btnRescan.Location = new Point(632, 9);
            btnRescan.Click += delegate { StartScan(); };
            top.Controls.Add(btnRescan);

            btnSystems = new Button();
            btnSystems.Text = "Systems...";
            btnSystems.Size = new Size(100, 32);
            btnSystems.Location = new Point(738, 9);
            btnSystems.Click += delegate { OpenSystems(); };
            top.Controls.Add(btnSystems);

            Label l1 = new Label();
            l1.Text = "System:";
            l1.AutoSize = true;
            l1.Location = new Point(10, 51);
            top.Controls.Add(l1);

            cboSystem = new ComboBox();
            cboSystem.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSystem.Location = new Point(64, 47);
            cboSystem.Width = 220;
            cboSystem.SelectedIndexChanged += delegate { Refilter(); };
            top.Controls.Add(cboSystem);

            Label l2 = new Label();
            l2.Text = "Sort:";
            l2.AutoSize = true;
            l2.Location = new Point(298, 51);
            top.Controls.Add(l2);

            cboSort = new ComboBox();
            cboSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSort.Location = new Point(336, 47);
            cboSort.Width = 150;
            cboSort.Items.AddRange(new object[] { "Relevance", "Name", "System", "Recently played", "Most played" });
            cboSort.SelectedIndex = Math.Max(0, Math.Min(4, cfg.SortMode));
            cboSort.SelectedIndexChanged += delegate { cfg.SortMode = cboSort.SelectedIndex; Refilter(); };
            top.Controls.Add(cboSort);

            chkFav = new CheckBox();
            chkFav.Text = "Favorites only";
            chkFav.AutoSize = true;
            chkFav.Location = new Point(500, 50);
            chkFav.CheckedChanged += delegate { Refilter(); };
            top.Controls.Add(chkFav);

            list = new FastListView();
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.VirtualMode = true;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.MultiSelect = false;
            list.GridLines = false;
            list.Columns.Add("Name");
            list.Columns.Add("System");
            list.Columns.Add("Type");
            list.Columns.Add("Played");
            list.Columns.Add("Path");
            list.RetrieveVirtualItem += RetrieveItem;
            list.DoubleClick += delegate { LaunchSelected(); };
            list.KeyDown += ListKeyDown;
            Controls.Add(list);
            list.BringToFront();

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Launch", null, delegate { LaunchSelected(); });
            menu.Items.Add("Toggle favorite (F2)", null, delegate { ToggleFav(); });
            menu.Items.Add("Open containing folder", null, delegate { OpenFolder(); });
            menu.Items.Add("Copy path", null, delegate { CopyPath(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Configure this system...", null, delegate { OpenSystems(); });
            list.ContextMenuStrip = menu;

            status = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Ready");
            lblStatus.Spring = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblCount = new ToolStripStatusLabel("");
            status.Items.Add(lblStatus);
            status.Items.Add(lblCount);
            Controls.Add(status);

            debounce = new System.Windows.Forms.Timer();
            debounce.Interval = 120;
            debounce.Tick += delegate { debounce.Stop(); Refilter(); };

            Resize += delegate { LayoutColumns(); };
            list.Resize += delegate { LayoutColumns(); };
            Shown += delegate
            {
                SendMessage(txtSearch.Handle, 0x1501, (IntPtr)1, "Type a game name...");
                txtSearch.Focus();
                LayoutColumns();
            };
            FormClosing += delegate
            {
                cancelScan = true;
                cfg.LastSystem = cboSystem.SelectedItem == null ? "" : cboSystem.SelectedItem.ToString();
                Ini.Save(cfg);
                Store.SaveStats(all);
            };
        }

        // Purely proportional columns; must never leave a horizontal scrollbar.
        void LayoutColumns()
        {
            if (resizing || list == null || list.Columns.Count < 5) return;
            resizing = true;
            try
            {
                int w = list.ClientSize.Width - 4;
                if (w < 300) w = 300;
                int c0 = w * 40 / 100;
                int c1 = w * 15 / 100;
                int c2 = w * 7 / 100;
                int c3 = w * 12 / 100;
                int c4 = w - c0 - c1 - c2 - c3;
                list.Columns[0].Width = c0;
                list.Columns[1].Width = c1;
                list.Columns[2].Width = c2;
                list.Columns[3].Width = c3;
                list.Columns[4].Width = c4;
            }
            finally { resizing = false; }
        }

        // -------------------------------------------------------- data loading

        void LoadInitial()
        {
            if (cfg.Systems.Count == 0) DiscoverSystems();

            DateTime stamp;
            List<RomEntry> cached = Store.LoadCache(cfg.RootPath, out stamp);
            if (cached.Count > 0)
            {
                all = cached;
                Store.ApplyStats(all);
                FillSystems();
                Refilter();
                lblStatus.Text = "Loaded cache from " + stamp.ToString("g", CultureInfo.CurrentCulture) + "  -  press F5 to rescan";
            }
            else
            {
                FillSystems();
                StartScan();
            }
        }

        void DiscoverSystems()
        {
            if (!Directory.Exists(cfg.RootPath))
            {
                FolderBrowserDialog fb = new FolderBrowserDialog();
                fb.Description = "Where is your ROMs folder?";
                fb.SelectedPath = cfg.RootPath;
                if (fb.ShowDialog(this) != DialogResult.OK) return;
                cfg.RootPath = fb.SelectedPath;
            }

            try
            {
                foreach (string dir in Directory.GetDirectories(cfg.RootPath))
                {
                    string name = Path.GetFileName(dir);
                    if (name.StartsWith(".") || name.StartsWith("!")) continue;
                    if (cfg.Find(name) != null) continue;
                    SystemConfig s = new SystemConfig();
                    s.Name = name;
                    s.Folder = dir;
                    cfg.Systems.Add(s);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

            cfg.Systems.Sort(delegate(SystemConfig a, SystemConfig b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            Ini.Save(cfg);
        }

        void FillSystems()
        {
            string prev = cboSystem.SelectedItem == null ? cfg.LastSystem : cboSystem.SelectedItem.ToString();
            cboSystem.Items.Clear();
            cboSystem.Items.Add("All systems");
            foreach (SystemConfig s in cfg.Systems)
                if (s.Enabled) cboSystem.Items.Add(s.Name);

            int idx = string.IsNullOrEmpty(prev) ? 0 : cboSystem.Items.IndexOf(prev);
            cboSystem.SelectedIndex = idx < 0 ? 0 : idx;
        }

        void StartScan()
        {
            if (scanThread != null && scanThread.IsAlive) return;
            DiscoverSystems();
            FillSystems();

            btnRescan.Enabled = false;
            lblStatus.Text = "Scanning...";
            cancelScan = false;

            scanThread = new Thread(delegate()
            {
                List<RomEntry> found = Scanner.Scan(cfg,
                    delegate(string msg)
                    {
                        try { if (IsHandleCreated) BeginInvoke((MethodInvoker)delegate { lblStatus.Text = msg; }); }
                        catch (Exception ex) { Debug.WriteLine(ex.Message); }
                    },
                    delegate { return cancelScan; });

                Store.ApplyStats(found);
                Store.SaveCache(cfg.RootPath, found);

                try
                {
                    if (!IsHandleCreated) return;
                    BeginInvoke((MethodInvoker)delegate
                    {
                        all = found;
                        btnRescan.Enabled = true;
                        lblStatus.Text = "Scan complete.";
                        Refilter();
                    });
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
            scanThread.IsBackground = true;
            scanThread.Start();
        }

        // ------------------------------------------------------------ filtering

        void Refilter()
        {
            string q = txtSearch.Text.Trim().ToLowerInvariant();
            string[] tokens = q.Length == 0
                ? new string[0]
                : q.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string sysFilter = null;
            if (cboSystem.SelectedIndex > 0 && cboSystem.SelectedItem != null)
                sysFilter = cboSystem.SelectedItem.ToString();

            bool favOnly = chkFav.Checked;
            List<RomEntry> res = new List<RomEntry>(Math.Min(all.Count, 4096));

            for (int i = 0; i < all.Count; i++)
            {
                RomEntry e = all[i];
                if (favOnly && !e.Fav) continue;
                if (sysFilter != null && !string.Equals(e.SystemName, sysFilter, StringComparison.OrdinalIgnoreCase)) continue;

                if (tokens.Length == 0) { e.Score = 0; res.Add(e); continue; }

                int s = Fuzzy.Score(e, tokens);
                if (s < 0) continue;
                if (e.Fav) s += 40;
                s += Math.Min(60, e.PlayCount * 6);
                e.Score = s;
                res.Add(e);
            }

            int mode = cfg.SortMode;
            if (mode == SORT_RELEVANCE && tokens.Length == 0) mode = SORT_SYSTEM;

            res.Sort(delegate(RomEntry a, RomEntry b)
            {
                int c;
                switch (mode)
                {
                    case SORT_RELEVANCE:
                        c = b.Score.CompareTo(a.Score);
                        if (c != 0) return c;
                        c = a.Name.Length.CompareTo(b.Name.Length);
                        if (c != 0) return c;
                        break;
                    case SORT_RECENT:
                        c = b.LastPlayed.CompareTo(a.LastPlayed);
                        if (c != 0) return c;
                        break;
                    case SORT_PLAYED:
                        c = b.PlayCount.CompareTo(a.PlayCount);
                        if (c != 0) return c;
                        break;
                    case SORT_SYSTEM:
                        c = string.Compare(a.SystemName, b.SystemName, StringComparison.OrdinalIgnoreCase);
                        if (c != 0) return c;
                        break;
                }
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            view = res;
            list.VirtualListSize = 0;
            list.VirtualListSize = view.Count;
            list.Invalidate();
            if (view.Count > 0 && list.IsHandleCreated)
            {
                list.SelectedIndices.Clear();
                list.SelectedIndices.Add(0);
            }

            lblCount.Text = view.Count.ToString("N0", CultureInfo.CurrentCulture) + " / "
                          + all.Count.ToString("N0", CultureInfo.CurrentCulture) + " ROMs";
        }

        void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= view.Count)
            {
                e.Item = new ListViewItem("");
                return;
            }
            RomEntry r = view[e.ItemIndex];
            ListViewItem it = new ListViewItem((r.Fav ? "* " : "") + r.Name);
            it.SubItems.Add(r.SystemName);
            it.SubItems.Add(r.Ext.TrimStart('.'));
            it.SubItems.Add(r.PlayCount == 0
                ? ""
                : r.PlayCount.ToString(CultureInfo.CurrentCulture) + "x  " + r.LastPlayed.ToString("dd/MM/yy", CultureInfo.CurrentCulture));
            it.SubItems.Add(r.FullPath);
            if (r.Fav) it.ForeColor = Color.FromArgb(160, 90, 0);
            e.Item = it;
        }

        // ------------------------------------------------------------- keyboard

        void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up
                || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp)
            {
                if (view.Count == 0) return;
                int cur = list.SelectedIndices.Count > 0 ? list.SelectedIndices[0] : 0;
                int step = (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up) ? 1 : 12;
                int next = (e.KeyCode == Keys.Down || e.KeyCode == Keys.PageDown) ? cur + step : cur - step;
                if (next < 0) next = 0;
                if (next >= view.Count) next = view.Count - 1;
                list.SelectedIndices.Clear();
                list.SelectedIndices.Add(next);
                list.EnsureVisible(next);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                LaunchSelected();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                txtSearch.Clear();
                e.Handled = true;
            }
        }

        void ListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { LaunchSelected(); e.Handled = true; }
            else if (e.KeyCode == Keys.F2) { ToggleFav(); e.Handled = true; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5) { StartScan(); return true; }
            if (keyData == Keys.F2) { ToggleFav(); return true; }
            if (keyData == (Keys.Control | Keys.F)) { txtSearch.Focus(); txtSearch.SelectAll(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // -------------------------------------------------------------- actions

        RomEntry Current()
        {
            if (list.SelectedIndices.Count == 0) return null;
            int i = list.SelectedIndices[0];
            if (i < 0 || i >= view.Count) return null;
            return view[i];
        }

        void ToggleFav()
        {
            RomEntry r = Current();
            if (r == null) return;
            r.Fav = !r.Fav;
            Store.SaveStats(all);
            list.Invalidate();
        }

        void OpenFolder()
        {
            RomEntry r = Current();
            if (r == null) return;
            try { Process.Start("explorer.exe", "/select,\"" + r.FullPath + "\""); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void CopyPath()
        {
            RomEntry r = Current();
            if (r == null) return;
            try { Clipboard.SetText(r.FullPath); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        void OpenSystems()
        {
            using (SystemsForm f = new SystemsForm(cfg))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                Ini.Save(cfg);
                FillSystems();
                Refilter();
            }
        }

        void LaunchSelected()
        {
            RomEntry r = Current();
            if (r == null) return;

            SystemConfig sys = cfg.Find(r.SystemName);
            string emu = sys == null ? "" : sys.Emulator;

            // No emulator configured: shell-execute if the ROM is itself runnable.
            if (string.IsNullOrEmpty(emu))
            {
                if (r.Ext == ".exe" || r.Ext == ".bat" || r.Ext == ".lnk" || r.Ext == ".cmd")
                {
                    RunShell(r);
                    return;
                }
                if (MessageBox.Show(this,
                        "No emulator is configured for \"" + r.SystemName + "\".\n\nConfigure it now?",
                        "ROM Launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    OpenSystems();
                return;
            }

            if (!File.Exists(emu))
            {
                MessageBox.Show(this, "Emulator not found:\n" + emu, "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string args = (string.IsNullOrEmpty(sys.Args) ? "\"{rom}\"" : sys.Args)
                .Replace("{rom}", r.FullPath)
                .Replace("{romdir}", Path.GetDirectoryName(r.FullPath))
                .Replace("{romname}", r.Name)
                .Replace("{system}", r.SystemName);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(emu, args);
                psi.WorkingDirectory = Path.GetDirectoryName(emu);
                psi.UseShellExecute = false;
                Process.Start(psi);
                MarkPlayed(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Launch failed:\n" + ex.Message, "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void RunShell(RomEntry r)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(r.FullPath);
                psi.WorkingDirectory = Path.GetDirectoryName(r.FullPath);
                psi.UseShellExecute = true;
                Process.Start(psi);
                MarkPlayed(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Launch failed:\n" + ex.Message, "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void MarkPlayed(RomEntry r)
        {
            r.PlayCount++;
            r.LastPlayed = DateTime.Now;
            Store.SaveStats(all);
            lblStatus.Text = "Launched: " + r.Name;
            list.Invalidate();
            if (cfg.MinimizeOnLaunch) WindowState = FormWindowState.Minimized;
        }
    }

    // ---------------------------------------------------------- systems dialog

    class SystemsForm : Form
    {
        AppConfig cfg;
        ListBox lst;
        TextBox txtRoot, txtEmu, txtArgs, txtExt, txtRetro;
        ComboBox cboCore;
        CheckBox chkEnabled, chkMinimize, chkDupes;
        bool loading;
        SystemConfig cur;

        public SystemsForm(AppConfig config)
        {
            cfg = config;

            Text = "Systems & emulators";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 520);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Font = new Font("Segoe UI", 9f);

            int y = 12;

            Label lRoot = new Label();
            lRoot.Text = "ROMs root:";
            lRoot.AutoSize = true;
            lRoot.Location = new Point(12, y + 4);
            Controls.Add(lRoot);

            txtRoot = new TextBox();
            txtRoot.Location = new Point(90, y);
            txtRoot.Width = 600;
            txtRoot.Text = cfg.RootPath;
            Controls.Add(txtRoot);

            Button bRoot = new Button();
            bRoot.Text = "Browse";
            bRoot.Location = new Point(698, y - 1);
            bRoot.Size = new Size(75, 24);
            bRoot.Click += delegate
            {
                FolderBrowserDialog fb = new FolderBrowserDialog();
                fb.SelectedPath = txtRoot.Text;
                if (fb.ShowDialog(this) == DialogResult.OK) txtRoot.Text = fb.SelectedPath;
            };
            Controls.Add(bRoot);

            y += 32;

            Label lRetro = new Label();
            lRetro.Text = "RetroArch:";
            lRetro.AutoSize = true;
            lRetro.Location = new Point(12, y + 4);
            Controls.Add(lRetro);

            txtRetro = new TextBox();
            txtRetro.Location = new Point(90, y);
            txtRetro.Width = 600;
            txtRetro.Text = cfg.RetroArchPath;
            txtRetro.TextChanged += delegate { FillCores(); };
            Controls.Add(txtRetro);

            Button bRetro = new Button();
            bRetro.Text = "Browse";
            bRetro.Location = new Point(698, y - 1);
            bRetro.Size = new Size(75, 24);
            bRetro.Click += delegate
            {
                OpenFileDialog od = new OpenFileDialog();
                od.Filter = "retroarch.exe|retroarch.exe|Executables (*.exe)|*.exe";
                if (od.ShowDialog(this) == DialogResult.OK) txtRetro.Text = od.FileName;
            };
            Controls.Add(bRetro);

            y += 36;

            lst = new ListBox();
            lst.Location = new Point(12, y);
            lst.Size = new Size(240, 380);
            lst.IntegralHeight = false;
            lst.SelectedIndexChanged += delegate { LoadSelected(); };
            Controls.Add(lst);

            int rx = 268, ry = y;

            chkEnabled = new CheckBox();
            chkEnabled.Text = "Enabled (include in scan)";
            chkEnabled.AutoSize = true;
            chkEnabled.Location = new Point(rx, ry);
            chkEnabled.CheckedChanged += delegate { if (!loading && cur != null) cur.Enabled = chkEnabled.Checked; };
            Controls.Add(chkEnabled);

            ry += 30;
            Controls.Add(MakeLabel("Emulator exe:", rx, ry + 4));
            txtEmu = new TextBox();
            txtEmu.Location = new Point(rx + 95, ry);
            txtEmu.Width = 380;
            txtEmu.TextChanged += delegate { if (!loading && cur != null) cur.Emulator = txtEmu.Text.Trim(); };
            Controls.Add(txtEmu);

            Button bEmu = new Button();
            bEmu.Text = "Browse";
            bEmu.Location = new Point(rx + 483, ry - 1);
            bEmu.Size = new Size(75, 24);
            bEmu.Click += delegate
            {
                OpenFileDialog od = new OpenFileDialog();
                od.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                if (od.ShowDialog(this) == DialogResult.OK) txtEmu.Text = od.FileName;
            };
            Controls.Add(bEmu);

            ry += 32;
            Controls.Add(MakeLabel("Arguments:", rx, ry + 4));
            txtArgs = new TextBox();
            txtArgs.Location = new Point(rx + 95, ry);
            txtArgs.Width = 463;
            txtArgs.TextChanged += delegate { if (!loading && cur != null) cur.Args = txtArgs.Text; };
            Controls.Add(txtArgs);

            ry += 28;
            Label hint = new Label();
            hint.Text = "Placeholders: {rom}  {romdir}  {romname}  {system}   -  e.g.  -f \"{rom}\"";
            hint.AutoSize = true;
            hint.ForeColor = Color.Gray;
            hint.Location = new Point(rx + 95, ry);
            Controls.Add(hint);

            ry += 30;
            Controls.Add(MakeLabel("RetroArch core:", rx, ry + 4));
            cboCore = new ComboBox();
            cboCore.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCore.Location = new Point(rx + 95, ry);
            cboCore.Width = 380;
            Controls.Add(cboCore);

            Button bApply = new Button();
            bApply.Text = "Use core";
            bApply.Location = new Point(rx + 483, ry - 1);
            bApply.Size = new Size(75, 24);
            bApply.Click += delegate { ApplyCore(); };
            Controls.Add(bApply);

            ry += 34;
            Controls.Add(MakeLabel("Extensions:", rx, ry + 4));
            txtExt = new TextBox();
            txtExt.Location = new Point(rx + 95, ry);
            txtExt.Width = 463;
            txtExt.TextChanged += delegate { if (!loading && cur != null) cur.Extensions = txtExt.Text.Trim(); };
            Controls.Add(txtExt);

            ry += 28;
            Label hint2 = new Label();
            hint2.Text = "Comma separated whitelist, e.g.  sfc,smc,zip   -  leave empty to auto-detect";
            hint2.AutoSize = true;
            hint2.ForeColor = Color.Gray;
            hint2.Location = new Point(rx + 95, ry);
            Controls.Add(hint2);

            ry += 40;
            Button bCopy = new Button();
            bCopy.Text = "Copy emulator + args to all systems";
            bCopy.Location = new Point(rx, ry);
            bCopy.Size = new Size(240, 28);
            bCopy.Click += delegate { CopyToAll(); };
            Controls.Add(bCopy);

            ry += 46;
            chkMinimize = new CheckBox();
            chkMinimize.Text = "Minimize launcher after launching a game";
            chkMinimize.AutoSize = true;
            chkMinimize.Checked = cfg.MinimizeOnLaunch;
            chkMinimize.Location = new Point(rx, ry);
            Controls.Add(chkMinimize);

            ry += 26;
            chkDupes = new CheckBox();
            chkDupes.Text = "Collapse multi-disc / duplicate-format sets (m3u, chd, cue...)";
            chkDupes.AutoSize = true;
            chkDupes.Checked = cfg.HideDiscDupes;
            chkDupes.Location = new Point(rx, ry);
            Controls.Add(chkDupes);

            Button ok = new Button();
            ok.Text = "OK";
            ok.DialogResult = DialogResult.OK;
            ok.Size = new Size(90, 30);
            ok.Location = new Point(ClientSize.Width - 196, ClientSize.Height - 42);
            ok.Click += delegate
            {
                cfg.RootPath = txtRoot.Text.Trim();
                cfg.RetroArchPath = txtRetro.Text.Trim();
                cfg.MinimizeOnLaunch = chkMinimize.Checked;
                cfg.HideDiscDupes = chkDupes.Checked;
            };
            Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Size = new Size(90, 30);
            cancel.Location = new Point(ClientSize.Width - 100, ClientSize.Height - 42);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;

            foreach (SystemConfig s in cfg.Systems) lst.Items.Add(s.Name);
            FillCores();
            if (lst.Items.Count > 0) lst.SelectedIndex = 0;
        }

        static Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Location = new Point(x, y);
            return l;
        }

        void FillCores()
        {
            cboCore.Items.Clear();
            string exe = txtRetro.Text.Trim();
            if (exe.Length == 0 || !File.Exists(exe)) return;
            string dir = Path.Combine(Path.GetDirectoryName(exe), "cores");
            if (!Directory.Exists(dir)) return;
            try
            {
                foreach (string dll in Directory.GetFiles(dir, "*_libretro.dll"))
                    cboCore.Items.Add(Path.GetFileName(dll));
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        void ApplyCore()
        {
            if (cur == null || cboCore.SelectedItem == null) return;
            string exe = txtRetro.Text.Trim();
            if (!File.Exists(exe)) { MessageBox.Show(this, "Set the RetroArch path first."); return; }
            string core = Path.Combine(Path.Combine(Path.GetDirectoryName(exe), "cores"), cboCore.SelectedItem.ToString());
            txtEmu.Text = exe;
            txtArgs.Text = "-L \"" + core + "\" -f \"{rom}\"";
        }

        void CopyToAll()
        {
            if (cur == null) return;
            if (MessageBox.Show(this, "Apply this emulator and arguments to every system?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (SystemConfig s in cfg.Systems)
            {
                s.Emulator = cur.Emulator;
                s.Args = cur.Args;
            }
        }

        void LoadSelected()
        {
            int i = lst.SelectedIndex;
            if (i < 0 || i >= cfg.Systems.Count) { cur = null; return; }
            loading = true;
            cur = cfg.Systems[i];
            chkEnabled.Checked = cur.Enabled;
            txtEmu.Text = cur.Emulator;
            txtArgs.Text = cur.Args;
            txtExt.Text = cur.Extensions;
            loading = false;
        }
    }

    // -------------------------------------------------------------- entry point

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
