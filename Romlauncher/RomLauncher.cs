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
using System.Text.RegularExpressions;
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

    // One named emulator choice (an alternate to a system's primary emulator).
    class EmuChoice
    {
        public string Name = "";
        public string Emulator = "";
        public string Args = "\"{rom}\"";
        public override string ToString() { return Name; }
    }

    class SystemConfig
    {
        public string Name = "";
        public string Folder = "";
        public string Emulator = "";
        public string Args = "\"{rom}\"";
        public string Extensions = "";   // csv whitelist, empty = auto
        public bool Enabled = true;
        public List<EmuChoice> Alts = new List<EmuChoice>();   // pick at launch

        public override string ToString() { return Name; }
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
                    else if (key.StartsWith("Alt", StringComparison.OrdinalIgnoreCase))
                        ParseAltKey(cur, key, val);
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
                int an = 1;
                foreach (EmuChoice a in s.Alts)
                {
                    if (string.IsNullOrEmpty(a.Name) && string.IsNullOrEmpty(a.Emulator)) continue;
                    sb.AppendLine("Alt" + an + "Name=" + a.Name);
                    sb.AppendLine("Alt" + an + "Emulator=" + a.Emulator);
                    sb.AppendLine("Alt" + an + "Args=" + a.Args);
                    an++;
                }
            }
            try { File.WriteAllText(Paths.Ini, sb.ToString(), Encoding.UTF8); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        static bool Eq(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        // Parse "Alt<n><Field>" keys (Alt1Name / Alt1Emulator / Alt1Args, 1-based)
        // into the system's alternate-emulator list, growing it as needed.
        static void ParseAltKey(SystemConfig cur, string key, string val)
        {
            Match m = Regex.Match(key, @"^Alt(\d+)(Name|Emulator|Args)$", RegexOptions.IgnoreCase);
            if (!m.Success) return;
            int idx = int.Parse(m.Groups[1].Value);
            if (idx < 1 || idx > 999) return;
            while (cur.Alts.Count < idx) cur.Alts.Add(new EmuChoice());
            EmuChoice a = cur.Alts[idx - 1];
            string field = m.Groups[2].Value;
            if (Eq(field, "Name")) a.Name = val;
            else if (Eq(field, "Emulator")) a.Emulator = val;
            else if (Eq(field, "Args")) a.Args = val;
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
            ".fs",".fers",".dsv",".sta",".st0",".st1",".st2",".sgm",".sn0",".sn1",".gci",
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
            StringBuilder log = new StringBuilder();
            log.AppendLine("RomLauncher scan log  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            log.AppendLine("RootPath = " + cfg.RootPath);
            log.AppendLine("Root exists = " + Directory.Exists(cfg.RootPath));
            log.AppendLine("Systems in config = " + cfg.Systems.Count);
            log.AppendLine("HideDiscDupes = " + cfg.HideDiscDupes);
            log.AppendLine(new string('-', 60));

            foreach (SystemConfig sys in cfg.Systems)
            {
                if (!sys.Enabled) { log.AppendLine("[skip:disabled] " + sys.Name); continue; }
                if (string.IsNullOrEmpty(sys.Folder))
                { log.AppendLine("[skip:no-folder] " + sys.Name); continue; }
                if (!Directory.Exists(sys.Folder))
                { log.AppendLine("[skip:missing]  " + sys.Name + "  ->  " + sys.Folder); continue; }
                if (cancelled()) break;

                progress("Scanning " + sys.Name + " ...");

                HashSet<string> white = null;
                if (!string.IsNullOrEmpty(sys.Extensions))
                {
                    white = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string e in sys.Extensions.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        white.Add(e.StartsWith(".") ? e.Trim() : "." + e.Trim());
                }

                int before = all.Count;
                int rawFiles = 0;

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
                    catch (Exception ex) { log.AppendLine("   [getfiles-error] " + dir + " : " + ex.Message); continue; }
                    rawFiles += files.Length;

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

                log.AppendLine("[ok] " + sys.Name + "  folder=" + sys.Folder
                    + "  rawFiles=" + rawFiles + "  kept=" + (all.Count - before));
            }

            log.AppendLine(new string('-', 60));
            log.AppendLine("TOTAL kept = " + all.Count);
            try { File.WriteAllText(Path.Combine(Paths.ExeDir, "RomLauncher.scan.log"), log.ToString(), Encoding.UTF8); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }

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
        // Bump whenever scan rules change (ignore list, dedup, etc.) so an old
        // cache written by a previous build is rejected and a rescan is forced.
        const int CACHE_VER = 2;

        public static void SaveCache(string root, List<RomEntry> list)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(Paths.Cache, false, Encoding.UTF8))
                {
                    w.WriteLine("#ver\t" + CACHE_VER.ToString(CultureInfo.InvariantCulture) + "\t" + root);
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

                // Header: "#ver \t <n> \t <root>"
                string[] h = lines[0].Split('\t');
                if (h.Length < 3 || h[0] != "#ver") return list;      // old/unknown format -> rescan
                int ver;
                if (!int.TryParse(h[1], out ver) || ver != CACHE_VER) return list;   // stale rules -> rescan
                if (!string.Equals(h[2], root, StringComparison.OrdinalIgnoreCase)) return list;

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

    // ------------------------------------------------------------------ native

    static class Native
    {
        const int EM_SETCUEBANNER = 0x1501;
        const int EM_SETMARGINS = 0xD3;
        const int EC_RIGHTMARGIN = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        const int SW_SHOWMAXIMIZED = 3;
        const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Maximize(IntPtr hWnd)
        {
            try { if (hWnd != IntPtr.Zero) ShowWindow(hWnd, SW_SHOWMAXIMIZED); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static void Restore(IntPtr hWnd)
        {
            try { if (hWnd != IntPtr.Zero) ShowWindow(hWnd, SW_RESTORE); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_RESTORE = 0xF120;

        [DllImport("user32.dll")]
        static extern bool ClipCursor(IntPtr lpRect);

        // Replicate the title-bar maximize/restore *buttons* (WM_SYSCOMMAND), which
        // is not quite the same as ShowWindow and is what makes RetroArch re-evaluate
        // its viewport and cursor clip.
        public static void SysRestore(IntPtr hWnd)
        {
            try { if (hWnd != IntPtr.Zero) SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static void SysMaximize(IntPtr hWnd)
        {
            try { if (hWnd != IntPtr.Zero) SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        // Release any cursor confinement (a stale ClipCursor rect is what leaves the
        // mouse "locked" to the old window size after a programmatic maximize).
        public static void ReleaseCursorClip()
        {
            try { ClipCursor(IntPtr.Zero); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static void SetRightMargin(TextBox box, int px)
        {
            try { if (box.IsHandleCreated) SendMessage(box.Handle, EM_SETMARGINS, (IntPtr)EC_RIGHTMARGIN, (IntPtr)(px << 16)); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        public static void SetPlaceholder(TextBox box, string text)
        {
            try { if (box.IsHandleCreated) SendMessage(box.Handle, EM_SETCUEBANNER, (IntPtr)1, text); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
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
        Button btnRescan, btnSystems, btnLaunch, btnClear;
        FastListView list;
        StatusStrip status;
        ToolStripStatusLabel lblStatus, lblCount;
        System.Windows.Forms.Timer debounce;
        Thread scanThread;
        volatile bool cancelScan;
        bool resizing;

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
            LoadWindowIcon();
            Load += delegate { LoadInitial(); };
        }

        // Use the icon embedded in the exe for the title bar / taskbar.
        void LoadWindowIcon()
        {
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
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
            txtSearch.TextChanged += delegate
            {
                if (btnClear != null) btnClear.Visible = txtSearch.TextLength > 0;
                debounce.Stop(); debounce.Start();
            };
            txtSearch.KeyDown += SearchKeyDown;
            top.Controls.Add(txtSearch);

            // Clear "x", overlaid on the right edge of the search box.
            btnClear = new Button();
            btnClear.Text = "\u2715";                 // ✕
            btnClear.Font = new Font("Segoe UI", 9f);
            btnClear.Size = new Size(22, 22);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 90, 90);
            btnClear.BackColor = txtSearch.BackColor;
            btnClear.ForeColor = Color.Gray;
            btnClear.TabStop = false;
            btnClear.Cursor = Cursors.Default;
            btnClear.Visible = false;
            btnClear.Click += delegate { txtSearch.Clear(); txtSearch.Focus(); };
            top.Controls.Add(btnClear);
            btnClear.BringToFront();

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
            ToolStripMenuItem launchWith = new ToolStripMenuItem("Launch with");
            menu.Items.Add(launchWith);
            menu.Items.Add("Toggle favorite (F2)", null, delegate { ToggleFav(); });
            menu.Items.Add("Open containing folder", null, delegate { OpenFolder(); });
            menu.Items.Add("Copy path", null, delegate { CopyPath(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Configure this system...", null, delegate
            {
                RomEntry r = Current();
                OpenSystems(r == null ? null : r.SystemName);
            });
            menu.Opening += delegate { BuildLaunchWithMenu(launchWith); };
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
                Native.SetPlaceholder(txtSearch, "Type a game name...");
                Native.SetRightMargin(txtSearch, 26);
                PositionClearButton();
                txtSearch.Focus();
                LayoutColumns();
            };
            FormClosing += delegate
            {
                cancelScan = true;
                cfg.LastSystem = SelectedSystemName() ?? "";
                Ini.Save(cfg);
                Store.SaveStats(all);
            };
        }

        void PositionClearButton()
        {
            if (btnClear == null) return;
            int x = txtSearch.Left + txtSearch.Width - btnClear.Width - 3;
            int y = txtSearch.Top + (txtSearch.Height - btnClear.Height) / 2;
            btnClear.Location = new Point(x, y);
            btnClear.BringToFront();
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

            // Re-anchor existing systems to the current root. This is what makes
            // the INI portable: copy it to another PC, change RootPath, and every
            // system folder is recomputed as <root>\<name> instead of pointing at
            // the old machine's drive letter.
            foreach (SystemConfig s in cfg.Systems)
            {
                string underRoot = Path.Combine(cfg.RootPath, s.Name);
                if (string.IsNullOrEmpty(s.Folder) || !Directory.Exists(s.Folder))
                {
                    if (Directory.Exists(underRoot)) s.Folder = underRoot;
                }
                else if (!IsUnder(s.Folder, cfg.RootPath) && Directory.Exists(underRoot))
                {
                    // Folder is valid but on a foreign root (stale from another PC).
                    s.Folder = underRoot;
                }
            }

            try
            {
                foreach (string dir in Directory.GetDirectories(cfg.RootPath))
                {
                    string name = Path.GetFileName(dir);
                    if (name.StartsWith(".") || name.StartsWith("!")) continue;
                    SystemConfig existing = cfg.Find(name);
                    if (existing != null) { existing.Folder = dir; continue; }
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

        static bool IsUnder(string path, string root)
        {
            try
            {
                string p = Path.GetFullPath(path).TrimEnd('\\').ToLowerInvariant();
                string r = Path.GetFullPath(root).TrimEnd('\\').ToLowerInvariant();
                return p == r || p.StartsWith(r + "\\", StringComparison.Ordinal);
            }
            catch (Exception) { return false; }
        }

        // Dropdown item that shows "name (1,234)" but reports the bare system name.
        class SysItem
        {
            public string Name;      // "" means the "All systems" entry
            public int Count;
            public override string ToString()
            {
                string label = Name.Length == 0 ? "All systems" : Name;
                return label + "  (" + Count.ToString("N0", CultureInfo.CurrentCulture) + ")";
            }
        }

        // The currently selected system name, or null for "All systems".
        string SelectedSystemName()
        {
            SysItem it = cboSystem.SelectedItem as SysItem;
            if (it == null || it.Name.Length == 0) return null;
            return it.Name;
        }

        void FillSystems()
        {
            // Remember the previously selected system by name.
            SysItem prevItem = cboSystem.SelectedItem as SysItem;
            string prev = prevItem != null ? prevItem.Name : cfg.LastSystem;

            // Count ROMs per system in a single pass.
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < all.Count; i++)
            {
                int c;
                counts.TryGetValue(all[i].SystemName, out c);
                counts[all[i].SystemName] = c + 1;
            }

            cboSystem.BeginUpdate();
            cboSystem.Items.Clear();
            cboSystem.Items.Add(new SysItem { Name = "", Count = all.Count });

            int selIdx = 0, row = 0;
            foreach (SystemConfig s in cfg.Systems)
            {
                if (!s.Enabled) continue;
                int c;
                counts.TryGetValue(s.Name, out c);
                cboSystem.Items.Add(new SysItem { Name = s.Name, Count = c });
                row++;
                if (!string.IsNullOrEmpty(prev) && string.Equals(s.Name, prev, StringComparison.OrdinalIgnoreCase))
                    selIdx = row;
            }
            cboSystem.EndUpdate();
            cboSystem.SelectedIndex = selIdx;
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
                        lblStatus.Text = found.Count == 0
                            ? "Scan found 0 ROMs. See RomLauncher.scan.log next to the exe for why."
                            : "Scan complete: " + found.Count.ToString("N0", CultureInfo.CurrentCulture) + " ROMs.";
                        FillSystems();   // refresh per-system counts now that all[] is populated
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

            string sysFilter = SelectedSystemName();

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

        void OpenSystems() { OpenSystems(null); }

        void OpenSystems(string selectSystem)
        {
            string oldRoot = cfg.RootPath;
            using (SystemsForm f = new SystemsForm(cfg, selectSystem))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                Ini.Save(cfg);
                FillSystems();

                // Changing the root invalidates the cached index (it was built for
                // the old root), so rescan rather than showing an empty list.
                if (!string.Equals(oldRoot, cfg.RootPath, StringComparison.OrdinalIgnoreCase))
                    StartScan();
                else
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
                    OpenSystems(r.SystemName);
                return;
            }

            LaunchWith(r, emu, sys.Args);
        }

        // Launch a specific ROM with a specific emulator + argument template.
        // Used by the default launch and by the "Launch with" alternate picker.
        void LaunchWith(RomEntry r, string emu, string argsTemplate)
        {
            if (r == null) return;
            if (string.IsNullOrEmpty(emu))
            {
                MessageBox.Show(this, "This emulator entry has no executable set.", "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(emu))
            {
                MessageBox.Show(this, "Emulator not found:\n" + emu, "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string args = (string.IsNullOrEmpty(argsTemplate) ? "\"{rom}\"" : argsTemplate)
                .Replace("{rom}", r.FullPath)
                .Replace("{romdir}", Path.GetDirectoryName(r.FullPath))
                .Replace("{romname}", r.Name)
                .Replace("{system}", r.SystemName);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(emu, args);
                psi.WorkingDirectory = Path.GetDirectoryName(emu);
                psi.UseShellExecute = false;
                Process p = Process.Start(psi);
                if (p != null && args.IndexOf("romlauncher_maximized.cfg", StringComparison.OrdinalIgnoreCase) >= 0)
                    MaximizeWhenReady(p);
                MarkPlayed(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Launch failed:\n" + ex.Message, "ROM Launcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Populate the "Launch with" submenu for the currently selected ROM:
        // the system's default emulator plus each configured alternate.
        void BuildLaunchWithMenu(ToolStripMenuItem parent)
        {
            parent.DropDownItems.Clear();
            RomEntry r = Current();
            SystemConfig sys = r == null ? null : cfg.Find(r.SystemName);
            if (sys == null) { parent.Enabled = false; return; }

            string defName = string.IsNullOrEmpty(sys.Emulator)
                ? "Default"
                : "Default (" + Path.GetFileName(sys.Emulator) + ")";
            parent.DropDownItems.Add(new ToolStripMenuItem(defName, null, delegate { LaunchSelected(); }));

            foreach (EmuChoice c in sys.Alts)
            {
                if (string.IsNullOrEmpty(c.Emulator) && string.IsNullOrEmpty(c.Name)) continue;
                EmuChoice cc = c;   // capture per iteration
                string label = string.IsNullOrEmpty(cc.Name) ? Path.GetFileName(cc.Emulator) : cc.Name;
                parent.DropDownItems.Add(new ToolStripMenuItem(label, null, delegate
                {
                    RomEntry rr = Current();
                    if (rr != null) LaunchWith(rr, cc.Emulator, cc.Args);
                }));
            }
            parent.Enabled = parent.DropDownItems.Count > 0;
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

        // For the "Maximized" window mode: wait for the emulator's main window to
        // appear, then maximize it. RetroArch can't reliably start maximized, so we
        // do it here. Some cores (notably PS2) bring their video context up a beat
        // later and ignore an early maximize -- leaving the viewport/mouse offset --
        // so after the core settles we do a real restore->maximize "nudge", which is
        // exactly what manually un-maximizing and re-maximizing does. Runs on a
        // background thread.
        static void MaximizeWhenReady(Process p)
        {
            Thread t = new Thread(delegate()
            {
                try
                {
                    IntPtr h = IntPtr.Zero;
                    for (int i = 0; i < 100; i++)
                    {
                        if (p.HasExited) return;
                        p.Refresh();
                        h = p.MainWindowHandle;
                        if (h != IntPtr.Zero) break;
                        Thread.Sleep(100);
                    }
                    if (h == IntPtr.Zero) return;

                    // Do NOT maximize early: doing it before the core's video/input
                    // is up leaves the mouse clipped to the initial window size (the
                    // "locked mouse"). Wait for the core to settle, then maximize once
                    // via the title-bar button path (WM_SYSCOMMAND) -- that's the path
                    // RetroArch reacts to, the same as clicking maximize by hand. Then
                    // clear any stale cursor clip for good measure.
                    Thread.Sleep(1300);
                    if (p.HasExited) return;
                    p.Refresh();
                    IntPtr h2 = p.MainWindowHandle;
                    if (h2 == IntPtr.Zero) h2 = h;
                    Native.SysMaximize(h2);
                    for (int k = 0; k < 5; k++)
                    {
                        if (p.HasExited) return;
                        Thread.Sleep(250);
                        Native.ReleaseCursorClip();
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
            t.IsBackground = true;
            t.Start();
        }
    }

    // ---------------------------------------------------------- systems dialog

    // A standalone-emulator preset: candidate exe names plus the argument
    // templates for fullscreen and windowed launches. Args verified against each
    // emulator's current CLI docs.
    class EmuPreset
    {
        public string Name;
        public string[] ExeNames;
        public string ArgsFull;
        public string ArgsWin;
        public EmuPreset(string name, string[] exeNames, string argsFull, string argsWin)
        {
            Name = name; ExeNames = exeNames; ArgsFull = argsFull; ArgsWin = argsWin;
        }
        public override string ToString() { return Name; }
    }

    class SystemsForm : Form
    {
        AppConfig cfg;
        ListBox lst;
        TextBox txtSysFilter;
        TextBox txtRoot, txtEmu, txtArgs, txtExt, txtRetro;
        ComboBox cboCore;
        ComboBox cboWinMode;
        ComboBox cboPreset;
        CheckBox chkEnabled, chkMinimize, chkDupes;
        bool loading;
        SystemConfig cur;
        string initialSelect;

        public SystemsForm(AppConfig config) : this(config, null) { }

        public SystemsForm(AppConfig config, string selectSystem)
        {
            cfg = config;
            initialSelect = selectSystem;

            Text = "Systems & emulators";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 610);
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

            txtSysFilter = new TextBox();
            txtSysFilter.Location = new Point(12, y);
            txtSysFilter.Width = 240;
            txtSysFilter.TextChanged += delegate { RebuildList(); };
            txtSysFilter.KeyDown += FilterKeyDown;
            Controls.Add(txtSysFilter);

            lst = new ListBox();
            lst.Location = new Point(12, y + 28);
            lst.Size = new Size(240, 352);
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

            ry += 30;
            Controls.Add(MakeLabel("Window mode:", rx, ry + 4));
            cboWinMode = new ComboBox();
            cboWinMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWinMode.Location = new Point(rx + 95, ry);
            cboWinMode.Width = 250;
            cboWinMode.Items.AddRange(new object[] { "Window", "Maximized", "Fullscreen" });
            cboWinMode.SelectedIndex = 1;   // default to maximized (real window)
            cboWinMode.SelectedIndexChanged += delegate
            {
                if (loading || cur == null) return;
                string na = SetWinModeInArgs(txtArgs.Text, cboWinMode.SelectedIndex);
                if (na != txtArgs.Text) txtArgs.Text = na;   // persisted via txtArgs.TextChanged
            };
            Controls.Add(cboWinMode);
            Label hintWin = new Label();
            hintWin.Text = "rewrites RetroArch args";
            hintWin.AutoSize = true;
            hintWin.ForeColor = Color.Gray;
            hintWin.Location = new Point(rx + 350, ry + 4);
            Controls.Add(hintWin);

            ry += 30;
            Controls.Add(MakeLabel("Emulator preset:", rx, ry + 4));
            cboPreset = new ComboBox();
            cboPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPreset.Location = new Point(rx + 95, ry);
            cboPreset.Width = 380;
            cboPreset.Items.AddRange(Presets);
            Controls.Add(cboPreset);

            Button bPreset = new Button();
            bPreset.Text = "Apply\u2026";
            bPreset.Location = new Point(rx + 483, ry - 1);
            bPreset.Size = new Size(75, 24);
            bPreset.Click += delegate { ApplyPreset(); };
            Controls.Add(bPreset);

            ry += 26;
            Label hintPre = new Label();
            hintPre.Text = "Standalone emulators: fills exe + args. Window mode = Window gives windowed, else fullscreen.";
            hintPre.AutoSize = true;
            hintPre.ForeColor = Color.Gray;
            hintPre.Location = new Point(rx + 95, ry);
            Controls.Add(hintPre);

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

            Button bAlts = new Button();
            bAlts.Text = "Alternate emulators\u2026";
            bAlts.Location = new Point(rx + 250, ry);
            bAlts.Size = new Size(180, 28);
            bAlts.Click += delegate
            {
                if (cur == null) { MessageBox.Show(this, "Select a system on the left first."); return; }
                using (AltEmusForm f = new AltEmusForm(cur))
                    f.ShowDialog(this);
            };
            Controls.Add(bAlts);

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
                string newRetro = txtRetro.Text.Trim();
                // Changing the RetroArch path re-anchors every RetroArch system to
                // the new folder (only when it points at a real retroarch.exe).
                if (!string.Equals(cfg.RetroArchPath, newRetro, StringComparison.OrdinalIgnoreCase)
                    && File.Exists(newRetro))
                    ReanchorRetroArch(newRetro);
                cfg.RetroArchPath = newRetro;
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

            RebuildList();
            FillCores();

            // If opened to configure a specific system, select and reveal it.
            if (!string.IsNullOrEmpty(initialSelect))
            {
                SystemConfig target = cfg.Find(initialSelect);
                if (target != null)
                {
                    int i = lst.Items.IndexOf(target);
                    if (i >= 0)
                    {
                        lst.SelectedIndex = i;
                        lst.TopIndex = i;   // scroll it into view
                    }
                }
            }

            Shown += delegate
            {
                Native.SetPlaceholder(txtSysFilter, "Filter systems...");
                // Focus the emulator field when configuring a known system,
                // otherwise the filter box for browsing.
                if (!string.IsNullOrEmpty(initialSelect) && cur != null) txtEmu.Focus();
                else txtSysFilter.Focus();
            };
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

            string args = "-L \"" + core + "\" ";
            // 0 = Window, 1 = Maximized (launcher maximizes it), 2 = Fullscreen
            args += "--appendconfig \"" + WriteWinModeCfg(exe, cboWinMode.SelectedIndex) + "\" ";
            args += "\"{rom}\"";
            txtArgs.Text = args;
        }

        // Verified standalone-emulator presets (CLI checked against current docs).
        internal static readonly EmuPreset[] Presets = new EmuPreset[]
        {
            new EmuPreset("DuckStation (PS1)",
                new string[] { "duckstation-qt-x64-ReleaseLTCG.exe", "duckstation-qt.exe", "duckstation.exe" },
                "-batch -fullscreen \"{rom}\"", "-batch \"{rom}\""),
            new EmuPreset("PCSX2 (PS2)",
                new string[] { "pcsx2-qt.exe", "pcsx2x64-avx2.exe", "pcsx2x64.exe", "pcsx2.exe" },
                "-batch -fullscreen -- \"{rom}\"", "-batch -- \"{rom}\""),
            new EmuPreset("PPSSPP (PSP)",
                new string[] { "PPSSPPWindows64.exe", "PPSSPPWindows.exe" },
                "--escape-exit --fullscreen \"{rom}\"", "--escape-exit \"{rom}\""),
            new EmuPreset("Dolphin (GC / Wii)",
                new string[] { "Dolphin.exe" },
                "-b --config=Dolphin.Display.Fullscreen=True -e \"{rom}\"",
                "-b --config=Dolphin.Display.Fullscreen=False -e \"{rom}\""),
            new EmuPreset("Flycast (Dreamcast / NAOMI)",
                new string[] { "flycast.exe" },
                "-config window:fullscreen=yes \"{rom}\"", "-config window:fullscreen=no \"{rom}\""),
            new EmuPreset("Yaba Sanshiro (Saturn)",
                new string[] { "yabasanshiro.exe" },
                "-i \"{rom}\" -a -fullscreen", "-i \"{rom}\" -a"),
        };

        // Fill the emulator exe + args for the selected system from a preset.
        // Window mode drives fullscreen vs windowed: only "Window" gives windowed;
        // "Maximized"/"Fullscreen" both give the preset's fullscreen args (Maximized
        // isn't meaningful for standalone emulators, so it maps to fullscreen).
        void ApplyPreset()
        {
            if (cur == null) { MessageBox.Show(this, "Select a system on the left first."); return; }
            EmuPreset p = cboPreset.SelectedItem as EmuPreset;
            if (p == null) { MessageBox.Show(this, "Pick an emulator preset first."); return; }

            OpenFileDialog od = new OpenFileDialog();
            od.Title = "Locate the " + p.Name + " executable";
            od.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*";
            if (p.ExeNames.Length > 0) od.FileName = p.ExeNames[0];
            if (od.ShowDialog(this) != DialogResult.OK) return;

            txtEmu.Text = od.FileName;
            bool windowed = (cboWinMode.SelectedIndex == 0);
            txtArgs.Text = windowed ? p.ArgsWin : p.ArgsFull;
        }

        // Layers a tiny config over the main one via --appendconfig. Each mode gets
        // its own filename so it can be recognised later:
        //   0 Window     - normal decorated window (fullscreen off)
        //   1 Maximized  - decorated window; the launcher maximizes it after it opens
        //                  (RetroArch has no reliable start-maximized setting)
        //   2 Fullscreen - fills the screen, no title bar (borderless fullscreen)
        static string WriteWinModeCfg(string retroArchExe, int mode)
        {
            string dir = Path.GetDirectoryName(retroArchExe);
            string name;
            bool full;
            if (mode == 2) { name = "romlauncher_fullscreen.cfg"; full = true; }
            else if (mode == 1) { name = "romlauncher_maximized.cfg"; full = false; }
            else { name = "romlauncher_windowed.cfg"; full = false; }
            string file = Path.Combine(dir, name);
            try
            {
                string v = full ? "true" : "false";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("video_fullscreen = \"" + v + "\"");
                sb.AppendLine("video_windowed_fullscreen = \"" + v + "\"");
                File.WriteAllText(file, sb.ToString(), Encoding.ASCII);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return file;
        }

        // Infer which window-mode entry matches an existing argument string,
        // so the dropdown reflects what's actually saved.
        // 0 = Window, 1 = Maximized, 2 = Fullscreen. Legacy borderless/"-f" -> Fullscreen.
        static int DetectWinMode(string args)
        {
            if (string.IsNullOrEmpty(args)) return 1;
            if (args.IndexOf("romlauncher_windowed.cfg", StringComparison.OrdinalIgnoreCase) >= 0) return 0;
            if (args.IndexOf("romlauncher_maximized.cfg", StringComparison.OrdinalIgnoreCase) >= 0) return 1;
            if (args.IndexOf("romlauncher_fullscreen.cfg", StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            if (args.IndexOf("romlauncher_borderless.cfg", StringComparison.OrdinalIgnoreCase) >= 0) return 2;  // legacy fill-screen
            if (Regex.IsMatch(args, @"(^|\s)-f(\s|$)")) return 2;   // legacy exclusive fullscreen
            return 1;   // default: maximized
        }

        // Rewrite the RetroArch window-mode part of an argument string in place.
        // Only touches RetroArch invocations (ones that load a core via -L);
        // leaves other emulators' args alone. Returns the (maybe unchanged) args.
        string SetWinModeInArgs(string args, int mode)
        {
            if (string.IsNullOrEmpty(args)) return args;
            if (args.IndexOf("-L ", StringComparison.Ordinal) < 0) return args;  // not RetroArch

            // Locate the RetroArch folder for the --appendconfig path.
            string retroDir = null;
            string re = txtRetro.Text.Trim();
            if (re.Length > 0 && File.Exists(re)) retroDir = Path.GetDirectoryName(re);
            if (retroDir == null)
            {
                Match m = Regex.Match(args, "-L\\s+\"([^\"]+)\"");
                if (m.Success)
                {
                    try { retroDir = Path.GetDirectoryName(Path.GetDirectoryName(m.Groups[1].Value)); }
                    catch (Exception ex) { Debug.WriteLine(ex.Message); }
                }
            }

            // Strip any existing window-mode tokens.
            args = Regex.Replace(args, "--appendconfig\\s+\"[^\"]*romlauncher_(?:borderless|windowed|maximized|fullscreen)\\.cfg\"\\s*", " ");
            args = Regex.Replace(args, "(^|\\s)-f(?=\\s|$)", " ");
            args = Regex.Replace(args, "\\s{2,}", " ").Trim();

            // Build the replacement token for the chosen mode.
            // 0 = Window, 1 = Maximized (maximized at launch), 2 = Fullscreen.
            string token = null;
            if (retroDir != null)
            {
                string cfg = WriteWinModeCfg(Path.Combine(retroDir, "retroarch.exe"), mode);
                token = "--appendconfig \"" + cfg + "\"";
            }
            if (token == null) return args;                    // couldn't resolve RetroArch dir

            // Insert before the {rom} placeholder, otherwise append.
            int idx = args.IndexOf("\"{rom}\"", StringComparison.Ordinal);
            if (idx < 0) idx = args.IndexOf("{rom}", StringComparison.Ordinal);
            if (idx >= 0) args = args.Substring(0, idx) + token + " " + args.Substring(idx);
            else args = args + " " + token;

            return Regex.Replace(args, "\\s{2,}", " ").Trim();
        }

        // Point every RetroArch system at a new RetroArch folder: rewrites the
        // emulator exe, the -L core path (keeping the core filename), and the
        // --appendconfig path (keeping the mode filename). Standalone emulators
        // (no -L, non-retroarch.exe) are left alone. Mirrors ROM-root re-anchoring.
        void ReanchorRetroArch(string newRetroExe)
        {
            if (string.IsNullOrEmpty(newRetroExe)) return;
            string newDir = Path.GetDirectoryName(newRetroExe);
            if (string.IsNullOrEmpty(newDir)) return;
            string coresDir = Path.Combine(newDir, "cores");

            foreach (SystemConfig s in cfg.Systems)
            {
                bool isRetro =
                    (!string.IsNullOrEmpty(s.Emulator) &&
                     s.Emulator.EndsWith("retroarch.exe", StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(s.Args) &&
                        s.Args.IndexOf("-L ", StringComparison.Ordinal) >= 0);
                if (!isRetro) continue;

                if (!string.IsNullOrEmpty(s.Emulator) &&
                    s.Emulator.EndsWith("retroarch.exe", StringComparison.OrdinalIgnoreCase))
                    s.Emulator = newRetroExe;

                if (string.IsNullOrEmpty(s.Args)) continue;

                // -L "...\cores\<core>.dll"  ->  keep <core>.dll, swap the folder
                s.Args = Regex.Replace(s.Args,
                    "(-L\\s+\")[^\"]*[\\\\/]cores[\\\\/]([^\"\\\\/]+)\"",
                    delegate(Match m) { return m.Groups[1].Value + Path.Combine(coresDir, m.Groups[2].Value) + "\""; });

                // --appendconfig "...\romlauncher_*.cfg"  ->  keep filename, swap folder
                s.Args = Regex.Replace(s.Args,
                    "(--appendconfig\\s+\")[^\"]*[\\\\/](romlauncher_[^\"\\\\/]+\\.cfg)\"",
                    delegate(Match m) { return m.Groups[1].Value + Path.Combine(newDir, m.Groups[2].Value) + "\""; });
            }
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

        // Rebuilds the list from the filter box. Substring match, so "ne" finds
        // "nes", "neogeo", "snes" and "turbografx-16 (pce)" style names too.
        void RebuildList()
        {
            SystemConfig keep = cur;
            string f = txtSysFilter.Text.Trim().ToLowerInvariant();

            lst.BeginUpdate();
            lst.Items.Clear();
            foreach (SystemConfig s in cfg.Systems)
            {
                if (f.Length > 0 && s.Name.ToLowerInvariant().IndexOf(f, StringComparison.Ordinal) < 0) continue;
                lst.Items.Add(s);
            }
            lst.EndUpdate();

            if (lst.Items.Count == 0) { cur = null; ClearFields(); return; }

            int idx = keep == null ? 0 : lst.Items.IndexOf(keep);
            lst.SelectedIndex = idx < 0 ? 0 : idx;
        }

        void FilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (lst.Items.Count == 0) return;
                int next = lst.SelectedIndex + (e.KeyCode == Keys.Down ? 1 : -1);
                if (next < 0) next = 0;
                if (next >= lst.Items.Count) next = lst.Items.Count - 1;
                lst.SelectedIndex = next;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Don't let Enter trigger the OK button while filtering.
                txtEmu.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && txtSysFilter.Text.Length > 0)
            {
                txtSysFilter.Clear();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void ClearFields()
        {
            loading = true;
            chkEnabled.Checked = false;
            txtEmu.Text = "";
            txtArgs.Text = "";
            txtExt.Text = "";
            loading = false;
        }

        void LoadSelected()
        {
            SystemConfig s = lst.SelectedItem as SystemConfig;
            if (s == null) { cur = null; return; }
            loading = true;
            cur = s;
            chkEnabled.Checked = cur.Enabled;
            txtEmu.Text = cur.Emulator;
            txtArgs.Text = cur.Args;
            txtExt.Text = cur.Extensions;
            cboWinMode.SelectedIndex = DetectWinMode(cur.Args);
            loading = false;
        }
    }

    // ---------------------------------------------- alternate emulators dialog

    // Manage a system's alternate emulators (its "Launch with" choices). Edits the
    // system's Alts list in place; the parent Systems dialog saves on OK.
    class AltEmusForm : Form
    {
        SystemConfig sys;
        ListBox lstAlts;
        TextBox txtName, txtEmu, txtArgs;
        ComboBox cboPreset;
        bool loading;

        public AltEmusForm(SystemConfig system)
        {
            sys = system;
            Text = "Alternate emulators - " + sys.Name;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(720, 340);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Font = new Font("Segoe UI", 9f);

            Label intro = new Label();
            intro.Text = "Add other emulators for \"" + sys.Name + "\". At launch, right-click a game and choose Launch with.";
            intro.AutoSize = true;
            intro.Location = new Point(12, 10);
            Controls.Add(intro);

            lstAlts = new ListBox();
            lstAlts.Location = new Point(12, 34);
            lstAlts.Size = new Size(200, 230);
            lstAlts.SelectedIndexChanged += delegate { LoadFields(); };
            Controls.Add(lstAlts);

            Button bNew = new Button();
            bNew.Text = "New";
            bNew.Location = new Point(12, 270);
            bNew.Size = new Size(95, 26);
            bNew.Click += delegate
            {
                EmuChoice a = new EmuChoice();
                a.Name = "New emulator";
                a.Args = "\"{rom}\"";
                sys.Alts.Add(a);
                Repopulate(sys.Alts.Count - 1);
                txtName.Focus();
                txtName.SelectAll();
            };
            Controls.Add(bNew);

            Button bRemove = new Button();
            bRemove.Text = "Remove";
            bRemove.Location = new Point(117, 270);
            bRemove.Size = new Size(95, 26);
            bRemove.Click += delegate
            {
                int i = lstAlts.SelectedIndex;
                if (i < 0) return;
                sys.Alts.RemoveAt(i);
                Repopulate(i - 1 >= 0 ? i - 1 : 0);
            };
            Controls.Add(bRemove);

            int rx = 228;
            Controls.Add(MakeLabel("Name:", rx, 38));
            txtName = new TextBox();
            txtName.Location = new Point(rx + 80, 34);
            txtName.Width = 380;
            txtName.TextChanged += delegate { if (!loading) { EmuChoice a = Sel(); if (a != null) a.Name = txtName.Text; } };
            txtName.Leave += delegate { Repopulate(lstAlts.SelectedIndex); };
            Controls.Add(txtName);

            Controls.Add(MakeLabel("Emulator:", rx, 70));
            txtEmu = new TextBox();
            txtEmu.Location = new Point(rx + 80, 66);
            txtEmu.Width = 300;
            txtEmu.TextChanged += delegate { if (!loading) { EmuChoice a = Sel(); if (a != null) a.Emulator = txtEmu.Text; } };
            Controls.Add(txtEmu);

            Button bBrowse = new Button();
            bBrowse.Text = "Browse";
            bBrowse.Location = new Point(rx + 383, 65);
            bBrowse.Size = new Size(77, 24);
            bBrowse.Click += delegate
            {
                OpenFileDialog od = new OpenFileDialog();
                od.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*";
                if (od.ShowDialog(this) == DialogResult.OK) txtEmu.Text = od.FileName;
            };
            Controls.Add(bBrowse);

            Controls.Add(MakeLabel("Arguments:", rx, 102));
            txtArgs = new TextBox();
            txtArgs.Location = new Point(rx + 80, 98);
            txtArgs.Width = 380;
            txtArgs.TextChanged += delegate { if (!loading) { EmuChoice a = Sel(); if (a != null) a.Args = txtArgs.Text; } };
            Controls.Add(txtArgs);

            Label hint = new Label();
            hint.Text = "Placeholders: {rom}  {romdir}  {romname}  {system}";
            hint.AutoSize = true;
            hint.ForeColor = Color.Gray;
            hint.Location = new Point(rx + 80, 126);
            Controls.Add(hint);

            Controls.Add(MakeLabel("Preset:", rx, 162));
            cboPreset = new ComboBox();
            cboPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPreset.Location = new Point(rx + 80, 158);
            cboPreset.Width = 250;
            cboPreset.Items.AddRange(SystemsForm.Presets);
            Controls.Add(cboPreset);

            Button bFill = new Button();
            bFill.Text = "Fill";
            bFill.Location = new Point(rx + 335, 157);
            bFill.Size = new Size(77, 24);
            bFill.Click += delegate { FillFromPreset(); };
            Controls.Add(bFill);

            Label hint2 = new Label();
            hint2.Text = "Fill uses the preset's fullscreen args. Edit afterward if you want windowed.";
            hint2.AutoSize = true;
            hint2.ForeColor = Color.Gray;
            hint2.Location = new Point(rx + 80, 188);
            Controls.Add(hint2);

            Button bClose = new Button();
            bClose.Text = "Close";
            bClose.DialogResult = DialogResult.OK;
            bClose.Size = new Size(90, 28);
            bClose.Location = new Point(ClientSize.Width - 102, ClientSize.Height - 40);
            Controls.Add(bClose);
            AcceptButton = bClose;

            Repopulate(0);
        }

        static Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Location = new Point(x, y);
            return l;
        }

        EmuChoice Sel() { return lstAlts.SelectedItem as EmuChoice; }

        void LoadFields()
        {
            loading = true;
            EmuChoice a = Sel();
            if (a != null) { txtName.Text = a.Name; txtEmu.Text = a.Emulator; txtArgs.Text = a.Args; }
            else { txtName.Text = ""; txtEmu.Text = ""; txtArgs.Text = ""; }
            loading = false;
        }

        void Repopulate(int sel)
        {
            lstAlts.BeginUpdate();
            lstAlts.Items.Clear();
            foreach (EmuChoice a in sys.Alts) lstAlts.Items.Add(a);
            lstAlts.EndUpdate();
            if (lstAlts.Items.Count > 0)
                lstAlts.SelectedIndex = Math.Max(0, Math.Min(sel, lstAlts.Items.Count - 1));
            else LoadFields();
        }

        void FillFromPreset()
        {
            EmuChoice a = Sel();
            if (a == null) { MessageBox.Show(this, "Add or select an alternate first."); return; }
            EmuPreset p = cboPreset.SelectedItem as EmuPreset;
            if (p == null) { MessageBox.Show(this, "Pick a preset first."); return; }

            OpenFileDialog od = new OpenFileDialog();
            od.Title = "Locate the " + p.Name + " executable";
            od.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*";
            if (p.ExeNames.Length > 0) od.FileName = p.ExeNames[0];
            if (od.ShowDialog(this) != DialogResult.OK) return;

            if (string.IsNullOrEmpty(txtName.Text) || txtName.Text == "New emulator") txtName.Text = p.Name;
            txtEmu.Text = od.FileName;
            txtArgs.Text = p.ArgsFull;
            Repopulate(lstAlts.SelectedIndex);
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
