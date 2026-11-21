using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuickLauncher
{
    // Finds games: every direct subfolder of a root is one game, and its most likely game exe
    // is picked by score. The others stay as alternatives (right-click → Choose exe).
    static class Scanner
    {
        const int MaxDepth = 5;            // levels below the game folder
        const int MaxExesPerGame = 400;    // RetroBat-sized folders: stop counting, keep what we have

        // An exe whose name contains one of these is never a game.
        static readonly string[] BadName =
        {
            "unins", "setup", "install", "redist", "vcredist", "dxsetup", "dxwebsetup", "directx",
            "crash", "bugsplat", "bssndrpt", "easyanticheat", "eac_", "battleye", "beservice",
            "updater", "update.exe", "cefsharp", "quicksfv", "language selector", "languageselector",
            "dotnet", "ue4prereq", "prereq", "cleaner", "downloader", "broker", "attestation",
            "notification", "uninst", "7z", "unrar", "winrar", "dxdiag", "physx", "oalinst",
            "vulkanrt", "steamerrorreporter", "errorreporter", "reporter", "webhelper", "helper",
            "touchup", "activation", "register", "vc_redist", "dxwebsetup", "nvidia",
            "unitycrashhandler", "zfgamebrowser", "pers.exe", "start_protected_game", "runtime",
            "scraper", "chdman", "unecm", "ffmpeg", "junction", "md5.exe", "elevate",
            "crs-", "handler", "uploader", "eosauth", "anticheat",
        };

        // A folder whose name contains one of these is skipped with everything under it.
        static readonly string[] BadDir =
        {
            "_crack", "crack", "original", "redist", "commonredist", "support", "directx", "vcredist",
            "easyanticheat", "battleye", "installer", "prereq", "extras", "artbook", "soundtrack",
            "crashpad", "dotnet", "node_modules", ".git", "__macosx", "update", "backup", "devtools",
            "guide", "jre", "bonusapp",
        };
        // Exact folder names skipped (substring would hit too much: "engine" in "GameEngine").
        static readonly HashSet<string> BadDirExact = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Engine", "Tools", "Plugins", "mono", "jre", "java", "python", "_Redist", "ost", "Docs",
            "Manual", "Bonus", "DLC", "Mods",
        };

        // Name parts that make an exe a worse guess, but still a valid choice.
        static readonly (string part, double penalty)[] Weak =
        {
            ("launcher", 2), ("server", 4), ("editor", 4), ("benchmark", 4), ("config", 3),
            ("settings", 3), ("tool", 3), ("sandbox", 3), ("test", 2), ("debug", 3), ("dedicated", 4),
            ("x86", 1), ("32", 0.5),
        };

        public static IEnumerable<string> GameFolders(string root)
        {
            IEnumerable<string> dirs;
            try { dirs = Directory.EnumerateDirectories(root).ToList(); }
            catch { yield break; }
            foreach (string d in dirs)
            {
                string n = Path.GetFileName(d);
                if (n.StartsWith("$") || n.StartsWith(".") || n.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase)) continue;
                yield return d;
            }
        }

        // Ranked candidate exes for one game folder, best first. Empty: not a game.
        public static List<string> Candidates(string gameFolder)
        {
            var found = new List<(string path, long size, int depth)>();
            string folderKey = Squash(Path.GetFileName(gameFolder));
            Walk(gameFolder, 0, found, folderKey);
            if (found.Count == 0) return new List<string>();

            var folderTokens = new HashSet<string>(Tokens(Path.GetFileName(gameFolder)));

            // Unreal: Dir\Name.exe starts Dir\Proj\Binaries\Win64\Proj-Win64-Shipping.exe. An exe
            // sitting in Dir (the stub the store or a shortcut runs) is the intended entry point.
            var stubDirs = new HashSet<string>(found
                .Where(f => Path.GetFileName(f.path).IndexOf("-shipping", StringComparison.OrdinalIgnoreCase) > 0)
                .Select(f => Path.GetDirectoryName(f.path))
                .Where(d => Path.GetFileName(Path.GetDirectoryName(d) ?? "").Equals("Binaries", StringComparison.OrdinalIgnoreCase))
                .Select(d => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(d))))
                .Where(d => d != null), StringComparer.OrdinalIgnoreCase);

            return found
                .Select(f => (f.path, score: Score(f.path, f.size, f.depth, folderKey, folderTokens, stubDirs)))
                .OrderByDescending(x => x.score)
                .Select(x => x.path)
                .ToList();
        }

        static double Score(string path, long size, int depth, string folderKey, HashSet<string> folderTokens, HashSet<string> stubDirs)
        {
            string stem = Path.GetFileNameWithoutExtension(path);
            string lower = stem.ToLowerInvariant();
            string key = Squash(stem);

            double s = 2 * Math.Log10(Math.Max(size, 1024));   // games are big: 100 MB ≈ 16, 1 MB ≈ 12
            s -= 1.5 * depth;

            if (key.Length >= 3 && (folderKey.Contains(key) || key.Contains(folderKey))) s += 3;
            s += Math.Min(3, Tokens(stem).Count(folderTokens.Contains));

            if (stubDirs.Contains(Path.GetDirectoryName(path))) s += 5;

            foreach (var (part, penalty) in Weak)
                if (lower.Contains(part)) s -= penalty;
            return s;
        }

        // A bad word that is part of the folder's own name says nothing about the exe:
        // "JDownloaderPortable.exe" in "jdownloader-portable" must not fall to the "downloader"
        // rule meant for Call of Duty's ccsdownloader.exe. Only that word is excused, so
        // "harddisksentinelupdate.exe" in "Hard Disk Sentinel" still falls to "update".
        static bool Excused(string exeName, string folderKey) =>
            BadName.Where(exeName.Contains).All(b => Squash(b).Length >= 4 && folderKey.Contains(Squash(b)));

        static void Walk(string dir, int depth, List<(string, long, int)> found, string folderKey)
        {
            if (found.Count >= MaxExesPerGame) return;
            try
            {
                foreach (string f in Directory.EnumerateFiles(dir, "*.exe"))
                {
                    string name = Path.GetFileName(f).ToLowerInvariant();
                    if (Regex.IsMatch(name, @"^(net\d+|vcx?\d+)\.exe$")) continue;   // bundled runtimes
                    if (BadName.Any(name.Contains) && !Excused(name, folderKey)) continue;
                    long size;
                    try { size = new FileInfo(f).Length; } catch { continue; }
                    if (size < 20 * 1024) continue;   // stubs, shims
                    found.Add((f, size, depth));
                }
                if (depth >= MaxDepth) return;
                foreach (string sub in Directory.EnumerateDirectories(dir))
                {
                    string n = Path.GetFileName(sub);
                    string nl = n.ToLowerInvariant();
                    if (BadDirExact.Contains(n) || BadDir.Any(nl.Contains)) continue;
                    try { if ((new DirectoryInfo(sub).Attributes & FileAttributes.ReparsePoint) != 0) continue; } catch { continue; }
                    Walk(sub, depth + 1, found, folderKey);
                }
            }
            catch { }   // access denied etc.
        }

        // ------------------------------------------------------------ names

        static string Squash(string s) => Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]", "");

        static IEnumerable<string> Tokens(string s) =>
            Regex.Split(Regex.Replace(s, "([a-z])([A-Z])", "$1 $2"), "[^A-Za-z0-9]+")
                 .Select(t => t.ToLowerInvariant())
                 .Where(t => t.Length >= 3 && !Noise.Contains(t));

        static readonly HashSet<string> Noise = new HashSet<string>
        {
            "the", "and", "edition", "deluxe", "ultimate", "complete", "collection", "remastered",
            "win64", "win32", "shipping", "x64", "x86", "game", "goty", "directors", "cut",
        };

        // Scene/repack tags that end folder names: "Atomfall-InsaneRamZes".
        static readonly string[] Groups =
        {
            "InsaneRamZes", "CODEX", "RUNE", "TENOKE", "FLT", "EMPRESS", "PLAZA", "SKIDROW", "RELOADED",
            "CPY", "HOODLUM", "DOGE", "P2P", "GOG", "ElAmigos", "FitGirl", "DODI", "Razor1911",
            "TiNYiSO", "SiMPLEX", "DARKSiDERS", "KaOs", "Repack", "CHRONOS", "ANOMALY", "PROPHET",
            "HI2U", "Unleashed", "voices38", "GoldBerg", "Portable",
        };

        // "Assassins.Creed.Mirage.Master.Assassin.Edition" → "Assassins Creed Mirage Master Assassin Edition",
        // "Dying Light The Beast (2025)" → "Dying Light The Beast", "Breakers.Collection.v1.0" → "Breakers Collection".
        public static string NiceName(string folderName)
        {
            string n = folderName;
            foreach (string g in Groups)
                n = Regex.Replace(n, @"[-_. ]\[?" + Regex.Escape(g) + @"\]?$", "", RegexOptions.IgnoreCase);
            // Build / platform tails of portable-app downloads: "deckcraft-windows-x64",
            // "ImageFanReloaded_Windows_x64", "Core2D.Desktop-net7.0-win7-x64", "…-Binaries-Latest".
            n = Regex.Replace(n, @"[-_. ](windows|win\d*|net\d[\d.]*|binaries|x64|x86|amd64|arm64|64bit|32bit)([-_. ].*)?$", "", RegexOptions.IgnoreCase);
            n = Regex.Replace(n, @"[ .]v\d+(\.\d+)*[a-z]?$", "", RegexOptions.IgnoreCase);       // " v1.03"
            n = Regex.Replace(n, @"[-_. ]v?\d+(\.\d+)+([-_. ].*)?$", "", RegexOptions.IgnoreCase); // "-1.4", "-v1.7.4609-windows-64bit-Qt", ".0.20.0"
            n = Regex.Replace(n, @"(?<=[A-Za-z])\d+(_\d+)+$", "");                              // "CrystalDiskInfo8_11_2"
            // Dots as word separators (scene style), but not "Clair Obscur. Expedition" or "2.7".
            if (n.Count(c => c == '.') >= 2 || (n.Contains('.') && !n.Contains(' ')))
                n = Regex.Replace(n, @"(?<!\d)\.|\.(?!\d)", " ");
            n = n.Replace('_', ' ');
            n = Regex.Replace(n, @"\s*[\(\[]\d{4}[\)\]]\s*$", "");                             // (2025)
            n = Regex.Replace(n, @"\s+", " ").Trim();
            return n == "" ? folderName : n;
        }
    }
}
