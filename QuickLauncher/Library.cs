using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace QuickLauncher
{
    // One tile: a game (Games tab) or an app (Apps tab). Named Game from when that was all there was.
    public sealed class Game : INotifyPropertyChanged
    {
        public string Folder;               // key: the game/app folder; the exe for a manually added one;
                                            // shell:AppsFolder\… for a Start menu entry
        public string Root;                 // the scanned folder it came from (StartMenuKey for Start menu)
        public List<string> Candidates = new List<string>();
        public string AutoName;
        public string UserName;             // rename; null = AutoName
        public string UserExe;              // picked by hand; null = best candidate
        public bool Hidden;
        public bool Manual;                 // added by dropping an exe / shortcut, not found by a scan
        public DateTime? LastPlayed;
        public int Plays;

        public string Name => string.IsNullOrEmpty(UserName) ? AutoName : UserName;
        public string Exe => UserExe != null && Library.TargetExists(UserExe) ? UserExe : Candidates.FirstOrDefault();
        public bool IsStartMenu => Root == Library.StartMenuKey;
        public string Tooltip => $"{Name}\n{(StartMenu.IsShellTarget(Exe) ? "Start menu: " + Name : Exe)}"
            + (LastPlayed is DateTime d ? $"\nLast used {d:yyyy-MM-dd HH:mm} · {Plays}×" : "");
        public string TileTag => Hidden ? "hidden" : null;

        ImageSource icon;
        public ImageSource Icon { get => icon; set { icon = value; Changed(nameof(Icon)); } }

        public void Refresh()
        {
            foreach (string p in new[] { nameof(Name), nameof(Exe), nameof(Tooltip), nameof(TileTag) }) Changed(p);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void Changed(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    // %APPDATA%\QuickLauncher\games.tsv / apps.tsv: the last scan plus everything the user
    // changed. Loading it makes start-up instant; a rescan merges into it.
    static class Library
    {
        public const string StartMenuKey = "\u0001start";
        const string Header = "#folder\troot\tautoName\tuserName\tuserExe\thidden\tmanual\tlastPlayed\tplays\tcandidates";

        public static bool TargetExists(string p) => StartMenu.IsShellTarget(p) || File.Exists(p);

        public static List<Game> Load(string file)
        {
            var list = new List<Game>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(file)) return list;
                foreach (string line in File.ReadAllLines(file))
                {
                    if (line.StartsWith("#")) continue;
                    var c = line.Split('\t');
                    if (c.Length < 10 || !seen.Add(c[0])) continue;   // one entry per key, first wins
                    list.Add(new Game
                    {
                        Folder = c[0], Root = c[1], AutoName = c[2],
                        UserName = c[3] == "" ? null : c[3],
                        UserExe = c[4] == "" ? null : c[4],
                        Hidden = c[5] == "1", Manual = c[6] == "1",
                        LastPlayed = DateTime.TryParse(c[7], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var d) ? d : (DateTime?)null,
                        Plays = int.TryParse(c[8], out int p) ? p : 0,
                        Candidates = c[9].Split('|').Where(s => s != "").ToList(),
                    });
                }
            }
            catch { }
            return list;
        }

        // Returns null when saved, else why not (shown in the panel's status line).
        public static string Save(string file, IEnumerable<Game> games)
        {
            List<string> lines;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                lines = new List<string> { Header };
                lines.AddRange(games.GroupBy(g => g.Folder, StringComparer.OrdinalIgnoreCase).Select(grp => grp.First())
                    .Select(g => string.Join("\t",
                    g.Folder, g.Root, Clean(g.AutoName), Clean(g.UserName), g.UserExe ?? "",
                    g.Hidden ? "1" : "0", g.Manual ? "1" : "0",
                    g.LastPlayed?.ToString("o", CultureInfo.InvariantCulture) ?? "", g.Plays,
                    string.Join("|", g.Candidates))));
            }
            catch (Exception ex) { return ex.Message; }

            // Write a temp file and swap it in, so a crash mid-write cannot truncate the list.
            // If the swap fails (file briefly locked by a scanner / sync tool), write in place.
            string tmp = file + ".tmp";
            try
            {
                File.WriteAllLines(tmp, lines);
                if (File.Exists(file)) File.Replace(tmp, file, null);
                else File.Move(tmp, file);
                return null;
            }
            catch
            {
                try { File.WriteAllLines(file, lines); File.Delete(tmp); return null; }
                catch (Exception ex) { return ex.Message; }
            }
        }

        static string Clean(string s) => (s ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

        // Runs on a worker thread. Keeps the user's renames, picks, hides and play history;
        // drops entries whose folder is gone. Manual entries are merged back by the caller.
        //   startMenu   also list every Start menu app (Apps tab)
        //   skipUnder   folders whose programs belong to the other tab (game folders, for Apps):
        //               a Start menu entry for a game there is left out of Apps
        public static List<Game> Rescan(List<string> roots, List<Game> old, Action<string> progress,
                                        bool startMenu = false, IEnumerable<string> skipUnder = null)
        {
            var byFolder = old.GroupBy(g => g.Folder, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var result = new List<Game>();
            string self = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            Game Merge(string key, string root, List<string> cands, string autoName)
            {
                byFolder.TryGetValue(key, out Game prev);
                var g = prev ?? new Game { Folder = key };
                g.Root = root;
                g.Manual = false;
                g.Candidates = cands;
                g.AutoName = autoName;
                if (g.UserExe != null && !TargetExists(g.UserExe)) g.UserExe = null;
                return g;
            }

            foreach (string root in roots)
            {
                if (!Directory.Exists(root)) { progress?.Invoke($"{root} not found, skipped"); continue; }
                foreach (string folder in Scanner.GameFolders(root))
                {
                    if (string.Equals(folder, self, StringComparison.OrdinalIgnoreCase)) continue;   // Quick Launcher itself
                    progress?.Invoke(Path.GetFileName(folder));
                    var cands = Scanner.Candidates(folder);
                    if (cands.Count == 0) continue;
                    result.Add(Merge(folder, root, cands, Scanner.NiceName(Path.GetFileName(folder))));
                }
            }

            if (startMenu)
            {
                progress?.Invoke("Start menu");
                // A program already listed from a scanned folder is not listed again.
                var haveExes = new HashSet<string>(result.SelectMany(g => g.Candidates.Take(1)), StringComparer.OrdinalIgnoreCase);
                var skip = (skipUnder ?? Enumerable.Empty<string>()).Select(s => s.TrimEnd('\\') + "\\").ToList();
                foreach (var e in StartMenu.Scan())
                {
                    if (haveExes.Contains(e.Target)) continue;
                    if (skip.Any(s => e.Target.StartsWith(s, StringComparison.OrdinalIgnoreCase))) continue;
                    if (e.Target.StartsWith(self + "\\", StringComparison.OrdinalIgnoreCase)) continue;
                    result.Add(Merge(e.Key, StartMenuKey, new List<string> { e.Target }, e.Name));
                }
            }
            return result;
        }

        // name: e.g. the dropped shortcut's name; null = from the exe's folder.
        public static Game Manual(string exe, string name = null) => new Game
        {
            Folder = exe, Root = Path.GetDirectoryName(exe), Manual = true,
            Candidates = new List<string> { exe },
            AutoName = !string.IsNullOrWhiteSpace(name) ? name.Trim()
                : Scanner.NiceName(Path.GetFileName(Path.GetDirectoryName(exe)) is string d && d != "" ? d : Path.GetFileNameWithoutExtension(exe)),
        };
    }
}
