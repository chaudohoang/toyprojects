using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace KitHelper
{
    // Log sink: text plus a level, so the UI can colour it. Called from worker threads.
    public enum LogLevel { Info, Detail, Ok, Warn, Error }
    public delegate void LogFn(string text, LogLevel level);

    public static class TrueTest
    {
        public const string Root = @"C:\Program Files\Radiant Vision Systems";

        // Every "TrueTest*" folder actually installed here. The old tools hardcoded five
        // paths that did not match each other or this PC (e.g. "1.9 debug" vs the installed
        // "1.9 debug Mobile"), so the list is read from disk instead.
        public static List<string> Installs(string root = Root)
        {
            if (!Directory.Exists(root)) return new List<string>();
            return Directory.GetDirectories(root, "TrueTest*")
                .OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Every TrueTest install, recognised by TrueTest.exe rather than by name: before
        // installing a new TrueTest, engineers rename the old folder ("TrueTest 1.8" ->
        // "1.8.5756.961"), and that renamed one is what kits get copied back from.
        public static List<string> AllInstalls(string root = Root)
        {
            if (!Directory.Exists(root)) return new List<string>();
            return Directory.GetDirectories(root)
                .Where(d => File.Exists(Path.Combine(d, "TrueTest.exe")))
                .OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // "TrueTest 1.8", "TrueTest 1.9 debug Mobile": what an installer creates. Anything else
        // ("1.8.5756.961", "TrueTest 1.8.5756.961") was renamed by hand.
        static readonly Regex InstallerName = new Regex(@"^TrueTest \d+\.\d+( .*)?$", RegexOptions.IgnoreCase);
        public static bool LooksRenamed(string dir) => !InstallerName.IsMatch(Path.GetFileName(dir.TrimEnd('\\')));

        // The old install to copy kits from: a renamed-looking folder first, then the most
        // recently renamed. A rename leaves created/modified dates alone but updates NTFS's
        // change time, which is read here.
        public static string GuessSource(IEnumerable<string> candidates) =>
            candidates.OrderByDescending(LooksRenamed).ThenByDescending(ChangeTime).FirstOrDefault();

        public static DateTime ChangeTime(string dir)
        {
            using (var h = CreateFile(dir, 0x80 /* FILE_READ_ATTRIBUTES */, 7, IntPtr.Zero, 3 /* OPEN_EXISTING */,
                                      0x02000000 /* FILE_FLAG_BACKUP_SEMANTICS: needed for a directory */, IntPtr.Zero))
            {
                if (!h.IsInvalid && GetFileInformationByHandleEx(h, 0 /* FileBasicInfo */, out FILE_BASIC_INFO info,
                                                                (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(FILE_BASIC_INFO))))
                    return DateTime.FromFileTimeUtc(info.ChangeTime);
            }
            try { return Directory.GetLastWriteTimeUtc(dir); } catch { return DateTime.MinValue; }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct FILE_BASIC_INFO { public long CreationTime, LastAccessTime, LastWriteTime, ChangeTime; public uint FileAttributes; }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security,
                                                                            uint disposition, uint flags, IntPtr template);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetFileInformationByHandleEx(Microsoft.Win32.SafeHandles.SafeFileHandle h, int infoClass,
                                                        out FILE_BASIC_INFO info, uint size);

        public static Process FindRunning() => Process.GetProcessesByName("TrueTest").FirstOrDefault();

        // MainModule throws for an elevated process when we are not elevated.
        public static string PathOf(Process p)
        {
            try { return p.MainModule.FileName; } catch { return null; }
        }
    }

    // One "Copy all to X folder" tool. SubFolders are created under the chosen install.
    public sealed class CopyTarget
    {
        public string Name;
        public string Label;                 // button text after "Copy all → "; default: SubFolders joined
        public string[] SubFolders;
        public bool IncludeSubdirs;          // xcopy /E of subfolders
        // Copies into <database>\InputIMG\<kit folder name> instead of the TrueTest install
        // (SubFolders is not used). Not offered when migrating kits between installs.
        public bool ToDatabase;
        // Optional per-file routing: kit-relative path -> install-relative destination, or null
        // to leave the file out. Used when one kit feeds more than one folder (POCBMobile).
        // A SubFolders entry that gets no file is skipped, not created.
        public Func<string, string, string> Route;   // (kit folder name, kit-relative path)
        public Func<string, bool> Skip = _ => false;

        public static readonly CopyTarget[] All =
        {
            new CopyTarget
            {
                Name = "POCB4.1Net",
                SubFolders = new[] { "POCB4.1Net", "POCB4.1" },
                // Same exclusions as "Copy all to POCB4.1Net folder.exe".
                Skip = f =>
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    return ext == ".tif" || ext == ".csv"
                        || string.Equals(Path.GetFileName(f), "LGD_VNTT.ini", StringComparison.OrdinalIgnoreCase);
                },
            },
            // POCB4.2 kits carry X<model>\ (crop params, panel shapes) and mainTemplate\ beside
            // the dlls, mirroring TrueTest's own POCB4.2 folder, so subfolders are copied.
            new CopyTarget { Name = "POCB4.2", SubFolders = new[] { "POCB4.2" }, IncludeSubdirs = true },
            // POCBMobile reads its input templates (inputTemplate<model><step>.txt) from
            // POCBMobile\inputTemplate; the filled offline-test inputs (inputTest<model><step>.txt)
            // sit beside them in POCBMobile\inputTest. Kits ship templates in a folder like
            // inputTemplateGIBD992_D997, sometimes with inputTest*.txt next to them, or as
            // inputTemplate\ + inputTest\ subfolders. See Route: whichever half the kit lacks is
            // skipped.
            new CopyTarget
            {
                Name = "POCBMobile",
                Label = @"POCBMobile\inputTemplate + inputTest",
                SubFolders = new[] { @"POCBMobile\inputTemplate", @"POCBMobile\inputTest" },
                Route = (kitName, rel) =>
                {
                    string first = rel.Split('\\')[0];
                    if (rel.Contains("\\"))     // a subfolder: only inputTemplate\ / inputTest\ mean something
                        return first.Equals("inputTemplate", StringComparison.OrdinalIgnoreCase) ? @"POCBMobile\" + rel
                             : first.Equals("inputTest", StringComparison.OrdinalIgnoreCase) ? @"POCBMobile\" + rel
                             : null;
                    // A kit folder named inputTest... holds test inputs only; elsewhere the file name decides.
                    bool test = kitName.StartsWith("inputTest", StringComparison.OrdinalIgnoreCase)
                             || Path.GetFileName(rel).StartsWith("inputTest", StringComparison.OrdinalIgnoreCase);
                    return (test ? @"POCBMobile\inputTest\" : @"POCBMobile\inputTemplate\") + rel;
                },
            },
            new CopyTarget { Name = "PUC", SubFolders = new[] { "PUC" } },
            // Some Omit kits carry subfolders too (the old .bat copied top-level files only).
            new CopyTarget { Name = "ScratchDustDetect_NY", SubFolders = new[] { "ScratchDustDetect_NY" }, IncludeSubdirs = true },
            new CopyTarget { Name = "LGDVHCustomApp", SubFolders = new[] { "LGDVHCustomApp" }, IncludeSubdirs = true },
            // A model folder (InputIMG\X4023-CB-MP00_MATHON: LGD_PO_Compensation.dll, panel
            // shapes...). TrueTest copies the PUC dll from here at run time, so it goes to the
            // database, never into the install.
            new CopyTarget { Name = "Database model", Label = @"Database\InputIMG\<this folder's name>", SubFolders = new[] { "InputIMG" }, ToDatabase = true },
        };
    }

    // Every copy action saves the files it is about to overwrite, so any copy can be undone.
    // One folder per action under BaseDir (beside the installed Kit Helper, the same path on
    // every PC - not inside the kit, where a later subfolder copy would carry it into TrueTest):
    //   <BaseDir>\<yyyyMMdd_HHmmss> <action>\TrueTest\<sub>\...    files from the install
    //   <BaseDir>\<yyyyMMdd_HHmmss> <action>\DATABASE\InputIMG\... files from the database
    //   <BaseDir>\<yyyyMMdd_HHmmss> <action>\backup.txt            what TrueTest\ / DATABASE\ stood for
    // Undo = copy TrueTest\ back over the install and DATABASE\ over the database.
    // Nothing is created when the action overwrites nothing.
    public sealed class Backup
    {
        public static string BaseDir { get; set; } = Path.Combine(InstalledCopy.Folder, "Kit Helper Backups");

        public readonly string Root;
        public int Count { get; private set; }
        readonly string header;

        // install / database: what TrueTest\ and DATABASE\ inside the backup map back to.
        public Backup(string action, string source, string install, string database)
        {
            string safe = new string(action.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            Root = Path.Combine(BaseDir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + " " + safe);
            for (int i = 2; Directory.Exists(Root); i++)       // two actions in the same second
                Root = Path.Combine(BaseDir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + " " + safe + " (" + i + ")");
            header = $"Kit Helper backup - {action}\r\nTaken    {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\nFrom kit {source}\r\n"
                   + (install != null ? $"TrueTest\\ = {install}\r\n" : "")
                   + (database != null ? $"DATABASE\\ = {database}\r\n" : "")
                   + "Undo: copy TrueTest\\ and DATABASE\\ back over the folders above.\r\n";
        }

        // Saves `dest` as Root\rel when it exists. False = it exists and could not be saved;
        // the caller must then leave it alone.
        public bool Save(string dest, string rel, LogFn log)
        {
            if (!File.Exists(dest)) return true;
            try
            {
                if (Count == 0)
                {
                    Directory.CreateDirectory(Root);
                    File.WriteAllText(Path.Combine(Root, "backup.txt"), header);
                }
                string to = Path.Combine(Root, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                File.Copy(dest, to, true);
                Count++;
                return true;
            }
            catch (Exception ex)
            {
                log($"  FAILED  {rel}: could not back up the existing file ({ex.Message}), not replaced.", LogLevel.Error);
                return false;
            }
        }

        public void Report(LogFn log)
        {
            if (Count > 0) log($"Backed up {Count} replaced file(s) to {Root}", LogLevel.Info);
            else log("Nothing was overwritten, so no backup was needed.", LogLevel.Detail);
        }
    }

    // What happened to one file in a copy, for the summary window. Category is where it went,
    // relative to the install / database ("TrueTest\POCB4.2\mainTemplate"), or NotCopied.
    // Changed / Kept: an INI's vignetting value after the copy (set, or left alone as DX).
    public enum CopyStatus { New, Replaced, Failed, Skipped, Changed, Kept }
    public sealed class CopyItem
    {
        public const string NotCopied = "Not copied";
        public string Category, Name, Detail;
        public string From, To;                  // source / destination file, when known
        public CopyStatus Status;
    }

    public static class Copier
    {
        // Destination files unticked in the confirmation. Set around one copy with Excluding(), which
        // runs on a single worker thread; CopyOne, the one place every copy path writes through,
        // skips them. Keyed by destination path, so one source copied to three places can be
        // unticked in one place only.
        [ThreadStatic] static ISet<string> excluded;

        public static T Excluding<T>(ISet<string> files, Func<T> copy)
        {
            var previous = excluded;
            excluded = files != null && files.Count > 0 ? files : null;
            try { return copy(); }
            finally { excluded = previous; }
        }

        public sealed class Result
        {
            public int Copied, Failed, Skipped;
            public bool AccessDenied;
            public readonly List<CopyItem> Items = new List<CopyItem>();

            // rel: path as shown, its folder becomes the category ("TrueTest\PUC\x.exe").
            public void Add(string rel, CopyStatus status, string detail = null, string from = null, string to = null) =>
                Items.Add(new CopyItem
                {
                    Category = status == CopyStatus.Skipped ? CopyItem.NotCopied : (Path.GetDirectoryName(rel) ?? ""),
                    Name = status == CopyStatus.Skipped ? rel : Path.GetFileName(rel),
                    Status = status,
                    Detail = detail,
                    From = from,
                    To = to,
                });

            // Files that were (or, in a dry run, would be) written.
            public IEnumerable<CopyItem> Written =>
                Items.Where(i => (i.Status == CopyStatus.New || i.Status == CopyStatus.Replaced) && i.To != null);
        }

        // Copies the top-level files of `source` (and subfolders when the target asks for it)
        // into each of target.SubFolders under `install`. `ownExe` is never copied.
        // Overwritten files go to `backup` as <backupPrefix>\<sub>\<rel>.
        // dryRun: touch nothing, only fill Items with what would happen (New / Replaced / Not
        // copied) - the confirmation uses the same code as the copy, so they cannot disagree.
        public static Result Run(string source, string install, CopyTarget target, string ownExe, LogFn log,
                                 Backup backup, string backupPrefix = Package.TrueTestDir, bool dryRun = false)
        {
            if (target.Route != null) return RunRouted(source, install, target, ownExe, log, backup, backupPrefix, dryRun);
            var r = new Result();
            var files = Directory.GetFiles(source)
                .Where(f => !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var skipped = files.Where(target.Skip).ToList();
            files = files.Except(skipped).ToList();
            r.Skipped = skipped.Count;
            if (skipped.Count > 0)
                log($"Skipping {skipped.Count} file(s) by rule: {string.Join(", ", skipped.Select(Path.GetFileName))}", LogLevel.Detail);
            foreach (string f in skipped) r.Add(Path.GetFileName(f), CopyStatus.Skipped, $"{target.Name} skips this kind of file");

            string root = source.TrimEnd('\\');
            var dirs = target.IncludeSubdirs ? Directory.GetDirectories(source) : new string[0];
            // Subfolder files as (full path, path relative to the kit), gathered once for all
            // destinations.
            var subFiles = dirs.SelectMany(d => Directory.GetFiles(d, "*", SearchOption.AllDirectories))
                               .Select(f => (from: f, rel: f.Substring(root.Length + 1))).ToList();

            // Helper tools and Explorer's cache files are never copied, at any depth (kits keep
            // an UnblockFiles.exe in subfolders too).
            var never = files.Where(Kit.NeverCopy).Select(Path.GetFileName)
                .Concat(subFiles.Where(s => Kit.NeverCopy(s.from)).Select(s => s.rel)).ToList();
            files = files.Where(f => !Kit.NeverCopy(f)).ToList();
            subFiles = subFiles.Where(s => !Kit.NeverCopy(s.from)).ToList();
            if (never.Count > 0)
                log($"Not copying helper tools / Thumbs.db: {string.Join(", ", never)}", LogLevel.Detail);
            foreach (string n in never) r.Add(n, CopyStatus.Skipped, "helper tool / Explorer file, never copied");

            foreach (string sub in target.SubFolders)
            {
                string dest = Path.Combine(install, sub);
                log($"→ {dest}", LogLevel.Info);
                try { if (!dryRun) Directory.CreateDirectory(dest); }
                catch (Exception ex)
                {
                    log($"Cannot create {dest}: {ex.Message}", LogLevel.Error);
                    r.Failed += files.Count + subFiles.Count;
                    r.AccessDenied |= ex is UnauthorizedAccessException;
                    foreach (string f in files) r.Add(Path.Combine(backupPrefix, sub, Path.GetFileName(f)), CopyStatus.Failed, "cannot create the folder: " + ex.Message);
                    foreach (var s in subFiles) r.Add(Path.Combine(backupPrefix, sub, s.rel), CopyStatus.Failed, "cannot create the folder: " + ex.Message);
                    continue;
                }

                foreach (string f in files)
                    CopyOne(f, Path.Combine(dest, Path.GetFileName(f)), r, log, null,
                            backup, Path.Combine(backupPrefix, sub, Path.GetFileName(f)), dryRun);

                foreach (var (from, rel) in subFiles)
                {
                    string to = Path.Combine(dest, rel);
                    if (!dryRun) try { Directory.CreateDirectory(Path.GetDirectoryName(to)); } catch { }
                    CopyOne(from, to, r, log, rel, backup, Path.Combine(backupPrefix, sub, rel), dryRun);
                }
                if (dryRun) continue;
                // xcopy /E also recreates empty folders, including each top subfolder itself.
                foreach (string d in dirs)
                    foreach (string e in Directory.GetDirectories(d, "*", SearchOption.AllDirectories).Prepend(d))
                        try { Directory.CreateDirectory(Path.Combine(dest, e.Substring(root.Length + 1))); } catch { }
            }
            return r;
        }

        // A target that routes each file (CopyTarget.Route). Every SubFolders entry that receives
        // no file is reported and skipped - never created empty.
        static Result RunRouted(string source, string install, CopyTarget target, string ownExe, LogFn log,
                                Backup backup, string backupPrefix, bool dryRun)
        {
            var r = new Result();
            string root = source.TrimEnd('\\'), kitName = Path.GetFileName(root);
            var routed = Directory.GetFiles(source, "*", SearchOption.AllDirectories)
                .Where(f => !Kit.NeverCopy(f) && !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase))
                .Where(f => !target.Skip(f))
                .Select(f => (from: f, rel: f.Substring(root.Length + 1)))
                .Select(x => (x.from, x.rel, to: target.Route(kitName, x.rel)))
                .ToList();
            var left = routed.Where(x => x.to == null).Select(x => x.rel).ToList();
            if (left.Count > 0) log($"Not for {target.Name}, not copied: {string.Join(", ", left)}", LogLevel.Detail);
            foreach (string l in left) r.Add(l, CopyStatus.Skipped, $"not an input for {target.Name}");

            foreach (string sub in target.SubFolders)
            {
                var mine = routed.Where(x => x.to != null && x.to.StartsWith(sub + "\\", StringComparison.OrdinalIgnoreCase)).ToList();
                string dest = Path.Combine(install, sub);
                if (mine.Count == 0)
                {
                    log($"{sub}: nothing in the kit for it, skipped.", LogLevel.Info);
                    r.Add(sub + "\\", CopyStatus.Skipped, "nothing in the kit for this folder");
                    continue;
                }
                log($"→ {dest}", LogLevel.Info);
                foreach (var (from, rel, to) in mine)
                {
                    string full = Path.Combine(install, to);
                    try { if (!dryRun) Directory.CreateDirectory(Path.GetDirectoryName(full)); }
                    catch (Exception ex)
                    {
                        log($"  FAILED  {rel}: {ex.Message}", LogLevel.Error);
                        r.Failed++;
                        r.AccessDenied |= ex is UnauthorizedAccessException;
                        r.Add(Path.Combine(backupPrefix, to), CopyStatus.Failed, "cannot create the folder: " + ex.Message);
                        continue;
                    }
                    CopyOne(from, full, r, log, rel, backup, Path.Combine(backupPrefix, to), dryRun);
                }
            }
            return r;
        }

        // 3 attempts on IOException (a file briefly held by a closing TrueTest), as the POCB exe did.
        // With a backup, an existing `to` is saved first as backupRel; if that fails it is not overwritten.
        internal static void CopyOne(string from, string to, Result r, LogFn log, string label = null,
                                     Backup backup = null, string backupRel = null, bool dryRun = false)
        {
            label = label ?? Path.GetFileName(from);
            // Where it lands, as the summary groups it: the backup layout when there is one.
            string shown = backupRel ?? to;
            bool existed = File.Exists(to);
            if (excluded != null && excluded.Contains(to))
            {
                log($"  skipped {label} (unticked)", LogLevel.Detail);
                r.Skipped++;
                r.Add(shown, CopyStatus.Skipped, "unticked in the confirmation, left as it was", from, to);
                return;
            }
            if (dryRun) { r.Add(shown, existed ? CopyStatus.Replaced : CopyStatus.New, null, from, to); return; }
            if (backup != null && !backup.Save(to, backupRel, log))
            {
                r.Failed++;
                r.Add(shown, CopyStatus.Failed, "could not back up the existing file, not replaced", from, to);
                return;
            }
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.Copy(from, to, true);
                    log($"  copied  {label}", LogLevel.Detail);
                    r.Copied++;
                    r.Add(shown, existed ? CopyStatus.Replaced : CopyStatus.New, null, from, to);
                    return;
                }
                catch (IOException ex) when (attempt < 3)
                {
                    log($"  retry {attempt}/3  {label}: {ex.Message}", LogLevel.Warn);
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    log($"  FAILED  {label}: {ex.Message}", LogLevel.Error);
                    r.Failed++;
                    r.AccessDenied |= ex is UnauthorizedAccessException;
                    r.Add(shown, CopyStatus.Failed, ex.Message, from, to);
                    return;
                }
            }
        }
    }

    // After a TrueTest update: copy the kit folders already deployed in the old (renamed)
    // install into the freshly installed one. Whole folders, subfolders included, as they are
    // in the old install; same-name files in the new one are overwritten.
    public static class KitMigration
    {
        // EDT_Mobile.csv in LGDVHCustomApp ships with each TrueTest; by default the new
        // install's copy is kept.
        public const string EdtMobile = "EDT_Mobile.csv";

        public static Copier.Result Run(string from, string to, IEnumerable<CopyTarget> targets, bool skipEdtMobile, LogFn log, Backup backup)
        {
            var r = new Copier.Result();
            foreach (string sub in targets.SelectMany(t => t.SubFolders))
            {
                string src = Path.Combine(from, sub), dst = Path.Combine(to, sub);
                // Not every install has every folder (POCBMobile\inputTest is newer): skip quietly.
                if (!Directory.Exists(src))
                {
                    log($"{sub}: not in {from}, skipped", LogLevel.Info);
                    r.Add(sub + "\\", CopyStatus.Skipped, "not in the old install");
                    continue;
                }
                log($"{sub} → {dst}", LogLevel.Info);
                foreach (string f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
                {
                    string rel = f.Substring(src.TrimEnd('\\').Length + 1);
                    string name = Path.GetFileName(f);
                    bool edt = skipEdtMobile && sub == "LGDVHCustomApp" && string.Equals(name, EdtMobile, StringComparison.OrdinalIgnoreCase);
                    if (edt || string.Equals(name, "Thumbs.db", StringComparison.OrdinalIgnoreCase))
                    {
                        r.Skipped++;
                        log($"  skipped {sub}\\{rel}{(edt ? " (keeping the new install's)" : "")}", LogLevel.Detail);
                        r.Add(sub + "\\" + rel, CopyStatus.Skipped, edt ? "keeping the new install's" : "Explorer file, never copied");
                        continue;
                    }
                    string target = Path.Combine(dst, rel);
                    try { Directory.CreateDirectory(Path.GetDirectoryName(target)); }
                    catch (Exception ex)
                    {
                        log($"  FAILED  {sub}\\{rel}: {ex.Message}", LogLevel.Error);
                        r.Failed++;
                        r.AccessDenied |= ex is UnauthorizedAccessException;
                        r.Add(Path.Combine(Package.TrueTestDir, sub, rel), CopyStatus.Failed, "cannot create the folder: " + ex.Message);
                        continue;
                    }
                    Copier.CopyOne(f, target, r, log, sub + "\\" + rel, backup, Path.Combine(Package.TrueTestDir, sub, rel));
                }
                // Empty folders too, as xcopy /E would.
                foreach (string d in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                    try { Directory.CreateDirectory(Path.Combine(dst, d.Substring(src.TrimEnd('\\').Length + 1))); } catch { }
            }
            return r;
        }
    }

    public static class Vignetting
    {
        public const string Key = "STEP_3_VIGNETTING_CORRECTION";
        static readonly Regex Line = new Regex(@"^(" + Key + @"\s*=\s*)(\d+\.?\d*)", RegexOptions.Multiline);

        public static bool IsDx(string path) =>
            Path.GetFileName(path).IndexOf("DX", StringComparison.OrdinalIgnoreCase) >= 0;

        public static string[] IniFiles(string folder) =>
            Directory.Exists(folder) ? Directory.GetFiles(folder, "*.ini").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray()
                                     : new string[0];

        // Current value(s), comma-joined; null when the key is absent.
        public static string Read(string path)
        {
            var m = Line.Matches(ReadText(path, out _));
            return m.Count == 0 ? null : string.Join(",", m.Cast<Match>().Select(x => x.Groups[2].Value));
        }

        // Returns the old value(s), or null when the file has no such key. Writes only on change,
        // in the file's own encoding (ANSI unless it carries a BOM).
        public static string Set(string path, string value)
        {
            string text = ReadText(path, out Encoding enc);
            var m = Line.Matches(text);
            if (m.Count == 0) return null;
            string old = string.Join(",", m.Cast<Match>().Select(x => x.Groups[2].Value));
            string updated = Line.Replace(text, x => x.Groups[1].Value + value);
            if (updated != text) File.WriteAllText(path, updated, enc);
            return old;
        }

        // INIs a copy writes (or would write) that hold the key, read from their source.
        public static List<CopyItem> CopiedInis(Copier.Result r) =>
            r.Written.Where(i => i.From != null && i.From.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)
                              && File.Exists(i.From) && Read(i.From) != null).ToList();

        // After a copy: set the key in the copied INIs (the kit is not touched) and record each in
        // r as Changed ("was 0") or Kept (a DX ini while includeDx is off). The original
        // destination files were already backed up by the copy.
        public static void ApplyToCopied(Copier.Result r, string value, bool includeDx, LogFn log)
        {
            string category = $"Vignetting set to {value}  ({Key})";
            foreach (var i in r.Written.Where(i => i.To.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)).ToList())
            {
                string rel = Path.Combine(i.Category, i.Name);
                string current;
                try { current = Read(i.To); } catch { continue; }
                if (current == null) continue;
                if (!includeDx && IsDx(i.To))
                {
                    r.Items.Add(new CopyItem { Category = category, Name = rel, Status = CopyStatus.Kept, Detail = $"DX ini, left at {current}", To = i.To });
                    continue;
                }
                try
                {
                    string old = Set(i.To, value);
                    log($"  vignetting {old} → {value}  {rel}", LogLevel.Detail);
                    r.Items.Add(new CopyItem { Category = category, Name = rel, Status = CopyStatus.Changed,
                                               Detail = old == value ? $"already {value}" : $"was {old}", To = i.To });
                }
                catch (Exception ex)
                {
                    log($"  FAILED  vignetting {rel}: {ex.Message}", LogLevel.Error);
                    r.Failed++;
                    r.Items.Add(new CopyItem { Category = category, Name = rel, Status = CopyStatus.Failed, Detail = ex.Message, To = i.To });
                }
            }
        }

        static string ReadText(string path, out Encoding enc)
        {
            using (var r = new StreamReader(path, Encoding.Default, true))
            {
                string t = r.ReadToEnd();
                enc = r.CurrentEncoding;
                return t;
            }
        }
    }

    // What a kit folder is, judged from its contents.
    public static class Kit
    {
        // The per-folder helper tools this app replaces. Copying them into TrueTest was never
        // intended - the old scripts only excluded themselves - so every copy target skips them.
        public static bool IsHelper(string fileName)
        {
            string n = Path.GetFileName(fileName);
            return n.StartsWith("Copy all to ", StringComparison.OrdinalIgnoreCase)
                || new[] { "Kit Helper.exe", "ScriptToolbox.exe", "UnblockFiles.exe", "VignetingOn.exe", "VignetingOff.exe",
                           "RunTestOmit.exe", "RunTestPUC.exe" }
                   .Contains(n, StringComparer.OrdinalIgnoreCase);
        }

        // Never copied by any target: the helper tools, and Explorer's per-folder cache files.
        public static bool NeverCopy(string fileName)
        {
            string n = Path.GetFileName(fileName);
            return IsHelper(n)
                || string.Equals(n, "Thumbs.db", StringComparison.OrdinalIgnoreCase)
                || string.Equals(n, "desktop.ini", StringComparison.OrdinalIgnoreCase);
        }

        // The copy target this folder is meant for, or null. A leftover "Copy all to X folder"
        // script is the surest sign; otherwise recognise the payload.
        public static CopyTarget Suggest(string folder, out string why)
        {
            why = null;
            if (!Directory.Exists(folder)) return null;
            var names = Directory.GetFiles(folder).Select(Path.GetFileName).ToList();
            CopyTarget T(string name) => CopyTarget.All.First(t => t.Name == name);
            bool Has(string pattern) => names.Any(n => Regex.IsMatch(n, pattern, RegexOptions.IgnoreCase));

            foreach (var t in CopyTarget.All)
                if (Has("^Copy all to " + Regex.Escape(t.Name) + " folder")) { why = "old copy script found"; return t; }

            // Before the payload rules: a model folder holds LGD_PO_Compensation.dll, which the
            // POCB4.1Net rule below would otherwise claim - copying an X4023 PUC dll into
            // TrueTest's POCB4.1Net / POCB4.1.
            if (string.Equals(Path.GetFileName(Path.GetDirectoryName(folder.TrimEnd('\\')) ?? ""), "InputIMG", StringComparison.OrdinalIgnoreCase))
            { why = "folder inside InputIMG"; return T("Database model"); }

            if (string.Equals(Path.GetFileName(folder.TrimEnd('\\')), "POCB4.2", StringComparison.OrdinalIgnoreCase))
            { why = "folder named POCB4.2"; return T("POCB4.2"); }
            if (Has(@"^Crop_params\.json$") || Directory.Exists(Path.Combine(folder, "mainTemplate")))
            { why = "Crop_params.json / mainTemplate folder"; return T("POCB4.2"); }
            string own = Path.GetFileName(folder.TrimEnd('\\'));
            if (Has(@"^input(Template|Test).*\.txt$") || own.StartsWith("inputTemplate", StringComparison.OrdinalIgnoreCase)
                || own.StartsWith("inputTest", StringComparison.OrdinalIgnoreCase)
                || Directory.Exists(Path.Combine(folder, "inputTemplate")) || Directory.Exists(Path.Combine(folder, "inputTest")))
            { why = "inputTemplate / inputTest files"; return T("POCBMobile"); }
            if (Has(@"^ScratchDustDetect.*\.exe$") || Has("^LGD_OMIT_INI")) { why = "ScratchDustDetect / LGD_OMIT_INI files"; return T("ScratchDustDetect_NY"); }
            if (Has(@"^VNTT_Interp.*\.exe$")) { why = "VNTT_Interp exes"; return T("LGDVHCustomApp"); }
            if (Has(@"^LGD_PO_Compensation") || Has("^LGD_COMP_INI")) { why = "LGD_PO_Compensation / LGD_COMP_INI files"; return T("POCB4.1Net"); }
            if (Has(@"PUC.*\.exe$")) { why = "PUC exe"; return T("PUC"); }
            return null;
        }

        // .txt files in the folder that are run inputs: first line an exe (RunTest), or a
        // POCBMobile input (inputTemplate / inputTest). A POCBMobile folder laid out as installed
        // (inputTemplate\ + inputTest\, as in a package) is read one level down as well.
        public static string[] RunInputs(string folder)
        {
            if (!Directory.Exists(folder)) return new string[0];
            bool IsInput(string f) => RunTest.ExeOf(f) != null || RunTest.IsPocbMobileInput(f);
            var top = Directory.GetFiles(folder, "*.txt").Where(IsInput).OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
            var nested = new[] { "inputTemplate", "inputTest" }
                .Select(d => Path.Combine(folder, d)).Where(Directory.Exists)
                .SelectMany(d => Directory.GetFiles(d, "*.txt").Where(IsInput).OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
            return top.Concat(nested).ToArray();
        }
    }

    // A combined package: several kits in one folder. Two layouts, which may be mixed:
    //
    //  1. Mirrored, laid out as installed (what we build ourselves):
    //       <package>\TrueTest\<sub>\...   -> <TrueTest install>\<sub>\...
    //       <package>\DATABASE\...         -> <database>\...
    //  2. LGD's kit layout: category folders, some with a model folder inside, e.g.
    //       OMIT\<model>\  PUC\<model>\  InputIMG\<model>\  POCB4.2\  R2R\  inputTemplateGIB...\
    //     Every folder is offered to Kit.Suggest, the same rule as opening it alone. A folder it
    //     recognises becomes one unit, copied by that target (an "InputIMG\<model>" folder →
    //     <database>\InputIMG\<model>, unless its own "Copy all to X" script says otherwise). A
    //     folder it doesn't recognise is a container: its subfolders are tried in turn.
    //
    // Files no unit takes (.seqxc, README, a loose .bmp) are listed as Not copied. Anything
    // overwritten is saved first by the caller's Backup (TrueTest\ / DATABASE\ layout).
    public static class Package
    {
        public const string TrueTestDir = "TrueTest", DatabaseDir = "DATABASE";
        const int MaxDepth = 3;                      // package\InputIMG\<model> is depth 2

        // Mirror = TrueTestDir / DatabaseDir for layout 1; else Target is the kit folder's target.
        public sealed class Unit
        {
            public string Folder, Rel, Mirror, Why;
            public CopyTarget Target;
            public bool NeedsDatabase => Mirror == DatabaseDir || Target?.ToDatabase == true;
            public bool NeedsInstall => !NeedsDatabase;
            public string Destination => Mirror == TrueTestDir ? "TrueTest install"
                                       : Mirror == DatabaseDir ? "Database"
                                       : Target.ToDatabase ? $@"Database\InputIMG\{Path.GetFileName(Folder)}"
                                       : Target.Label ?? string.Join(" + ", Target.SubFolders);
        }

        public sealed class Layout
        {
            public readonly List<Unit> Units = new List<Unit>();
            public readonly List<(string rel, string why)> Left = new List<(string, string)>();   // not copied
            public bool TooBig;                      // scan stopped: not a package (e.g. D:\Log itself)
            public bool NeedsDatabase => Units.Any(u => u.NeedsDatabase);
            public bool NeedsInstall => Units.Any(u => u.NeedsInstall);
        }

        // LGD kits nest model folders only inside these; nothing else is searched deeper.
        static readonly string[] Containers = { "OMIT", "PUC", "InputIMG" };
        static bool IsContainer(string name) => Containers.Contains(name, StringComparer.OrdinalIgnoreCase);

        // Top-level folder names LGD kits use (containers + kits named after their target).
        static bool IsKnownCategory(string name) =>
            IsContainer(name)
            || new[] { "POCB4.2", "POCB4.1", "POCB4.1Net", "POCBMobile", "R2R", "LGDVHCustomApp", "ScratchDustDetect_NY" }
                   .Contains(name, StringComparer.OrdinalIgnoreCase)
            || name.StartsWith("inputTemplate", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("inputTest", StringComparison.OrdinalIgnoreCase);

        // A folder is a package when:
        //  - it is not itself a kit for one target (a POCB4.2 kit has subfolders that look like units),
        //  - it looks like a package root: TrueTest\ / DATABASE\, a .seqxc (every LGD kit ships its
        //    sequence), or at least two known category folders - so a parent folder full of kits
        //    (D:\Log, D:\Log\Kit) is not mistaken for one,
        //  - and it holds at least one unit.
        public static bool Is(string folder)
        {
            if (!Directory.Exists(folder) || Kit.Suggest(folder, out _) != null) return false;
            var dirs = Directory.GetDirectories(folder).Select(Path.GetFileName).ToList();
            bool rooted = dirs.Any(n => n.Equals(TrueTestDir, StringComparison.OrdinalIgnoreCase) || n.Equals(DatabaseDir, StringComparison.OrdinalIgnoreCase))
                       || Directory.EnumerateFiles(folder, "*.seqxc").Any()
                       || dirs.Count(IsKnownCategory) >= 2;
            if (!rooted) return false;
            var l = Scan(folder);
            return !l.TooBig && l.Units.Count > 0;
        }

        // Runs on every folder opened, so it gives up on a big tree instead of walking it.
        const int MaxFolders = 150;

        public static Layout Scan(string folder)
        {
            var l = new Layout();
            string root = folder.TrimEnd('\\');
            string Rel(string p) => p.Substring(root.Length + 1);
            bool HasFiles(string d) => Directory.EnumerateFiles(d, "*", SearchOption.AllDirectories).Any(f => !Kit.NeverCopy(f));
            int visited = 0;

            foreach (string f in Directory.GetFiles(root).Where(f => !Kit.NeverCopy(f)))
                l.Left.Add((Rel(f), "not in a kit folder"));

            void Discover(string dir, int depth)
            {
                if (l.TooBig) return;
                if (++visited > MaxFolders) { l.TooBig = true; return; }
                if (!HasFiles(dir)) return;                                  // empty, or only Thumbs.db / helpers
                var t = Kit.Suggest(dir, out string why);
                if (t != null)
                {
                    l.Units.Add(new Unit { Folder = dir, Rel = Rel(dir), Target = t, Why = why });
                    // Every InputIMG\<model> also belongs in the database, including one whose own
                    // copy script sends it elsewhere (D997's CGDIM_TLSM → POCB4.1Net).
                    bool inInputImg = string.Equals(Path.GetFileName(Path.GetDirectoryName(dir)), "InputIMG", StringComparison.OrdinalIgnoreCase);
                    if (inInputImg && !t.ToDatabase)
                        l.Units.Add(new Unit { Folder = dir, Rel = Rel(dir), Target = CopyTarget.All.First(x => x.ToDatabase),
                                               Why = "InputIMG folder: also kept in the database" });
                    return;
                }
                // Only OMIT\ PUC\ InputIMG\ hold model folders; anything else unrecognised is reported, not searched.
                if (depth >= MaxDepth || !IsContainer(Path.GetFileName(dir)))
                {
                    l.Left.Add((Rel(dir) + "\\", "kit type not recognised"));
                    return;
                }
                foreach (string f in Directory.GetFiles(dir).Where(f => !Kit.NeverCopy(f)))
                    l.Left.Add((Rel(f), "not in a kit folder"));
                foreach (string d in Directory.GetDirectories(dir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
                    Discover(d, depth + 1);
            }

            foreach (string d in Directory.GetDirectories(root).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                string name = Path.GetFileName(d);
                if (name.Equals(TrueTestDir, StringComparison.OrdinalIgnoreCase) || name.Equals(DatabaseDir, StringComparison.OrdinalIgnoreCase))
                {
                    if (HasFiles(d)) l.Units.Add(new Unit { Folder = d, Rel = name, Mirror = name.Equals(TrueTestDir, StringComparison.OrdinalIgnoreCase) ? TrueTestDir : DatabaseDir, Why = "mirrors the install layout" });
                }
                else if (name.StartsWith("Backup_", StringComparison.OrdinalIgnoreCase)) continue;   // a 1.2.0 in-package backup
                else Discover(d, 1);
                if (l.TooBig) { l.Units.Clear(); break; }
            }
            return l;
        }

        // One line per unit, for the panel and the confirmation:  OMIT\X4023-… → ScratchDustDetect_NY
        public static string Describe(Layout l) =>
            string.Join("\n", l.Units.Select(u => $"{u.Rel}\\  →  {u.Destination}"));

        // What Run would do (CopyResultWindow preview). Nothing is touched.
        public static Copier.Result Preview(string folder, string install, string database, string ownExe) =>
            Execute(folder, install, database, ownExe, (s, l) => { }, null, dryRun: true);

        public static Copier.Result Run(string folder, string install, string database, string ownExe, LogFn log, Backup backup) =>
            Execute(folder, install, database, ownExe, log, backup, dryRun: false);

        static Copier.Result Execute(string folder, string install, string database, string ownExe, LogFn log, Backup backup, bool dryRun)
        {
            var layout = Scan(folder);
            var all = new Copier.Result();
            void Merge(Copier.Result r)
            {
                if (r == null) return;
                all.Copied += r.Copied; all.Failed += r.Failed; all.Skipped += r.Skipped;
                all.AccessDenied |= r.AccessDenied;
                all.Items.AddRange(r.Items);
            }

            foreach (var u in layout.Units)
            {
                if (u.NeedsDatabase && database == null) { log($"{u.Rel}: no database picked, skipped.", LogLevel.Warn); continue; }
                if (u.NeedsInstall && install == null) { log($"{u.Rel}: no TrueTest install picked, skipped.", LogLevel.Warn); continue; }
                log($"{u.Rel}  →  {u.Destination}", LogLevel.Info);
                if (u.Mirror != null)
                    Merge(Mirror(u.Folder, u.Mirror, u.Mirror == TrueTestDir ? install : database, ownExe, log, backup, dryRun));
                else if (u.Target.ToDatabase)
                {
                    var into = new CopyTarget { Name = u.Target.Name, SubFolders = new[] { Path.GetFileName(u.Folder) } };
                    Merge(Copier.Run(u.Folder, Path.Combine(database, "InputIMG"), into, ownExe, log, backup,
                                     Path.Combine(DatabaseDir, "InputIMG"), dryRun));
                }
                else
                    Merge(Copier.Run(u.Folder, install, u.Target, ownExe, log, backup, TrueTestDir, dryRun));
            }
            foreach (var (rel, why) in layout.Left) all.Add(rel, CopyStatus.Skipped, why);
            return all;
        }

        // Layout 1: every file at its relative path under destRoot (shown as <part>\...).
        static Copier.Result Mirror(string src, string part, string destRoot, string ownExe, LogFn log, Backup backup, bool dryRun)
        {
            var r = new Copier.Result();
            if (part == TrueTestDir && !dryRun)
                // A TrueTest\<sub> the install lacks is most likely misnamed (a folder TrueTest never reads).
                foreach (string d in Directory.GetDirectories(src))
                    if (!Directory.Exists(Path.Combine(destRoot, Path.GetFileName(d))))
                        log($@"{TrueTestDir}\{Path.GetFileName(d)}: not in {destRoot} - a new folder will be created there.", LogLevel.Warn);

            string root = src.TrimEnd('\\');
            foreach (string f in Directory.GetFiles(src, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string rel = f.Substring(root.Length + 1), shown = Path.Combine(part, rel);
                if (Kit.NeverCopy(f) || string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase))
                { r.Add(shown, CopyStatus.Skipped, "helper tool / Explorer file, never copied"); continue; }
                string to = Path.Combine(destRoot, rel);
                if (!dryRun)
                    try { Directory.CreateDirectory(Path.GetDirectoryName(to)); }
                    catch (Exception ex)
                    {
                        log($"  FAILED  {shown}: {ex.Message}", LogLevel.Error);
                        r.Failed++;
                        r.AccessDenied |= ex is UnauthorizedAccessException;
                        r.Add(shown, CopyStatus.Failed, "cannot create the folder: " + ex.Message);
                        continue;
                    }
                Copier.CopyOne(f, to, r, log, shown, backup, shown, dryRun);
            }
            return r;
        }
    }

    public static class Unblocker
    {
        // Deletes the Zone.Identifier stream ("downloaded from the internet" mark). Recursive,
        // unlike UnblockFiles.exe, because extracted kits carry subfolders too.
        public static (int unblocked, int total, int failed) Run(string folder, LogFn log)
        {
            int un = 0, total = 0, failed = 0;
            foreach (string f in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                total++;
                string ads = @"\\?\" + f + ":Zone.Identifier";
                try
                {
                    if (File.Exists(ads))
                    {
                        File.Delete(ads);
                        un++;
                        log($"  unblocked  {f.Substring(folder.TrimEnd('\\').Length + 1)}", LogLevel.Detail);
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    log($"  FAILED  {f}: {ex.Message}", LogLevel.Error);
                }
            }
            return (un, total, failed);
        }
    }

    public static class RunTest
    {
        public const string CustomWorkDir = @"D:\Batch\ScratchNY_Custom\Exe Files";

        // Line 1 of an input file is the executable. Used to tell run inputs from other .txt.
        public static string ExeOf(string inputFile)
        {
            try
            {
                using (var r = new StreamReader(inputFile))
                {
                    string first = r.ReadLine()?.Trim();
                    return first != null && first.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? first : null;
                }
            }
            catch { return null; }
        }

        // A POCBMobile input (inputTemplate*.txt / inputTest*.txt): "key: value" lines starting
        // with debugMode. POCBMobile.exe takes no arguments; it reads input.txt beside itself.
        public static bool IsPocbMobileInput(string inputFile)
        {
            try
            {
                using (var r = new StreamReader(inputFile))
                    for (string line; (line = r.ReadLine()) != null;)
                        if (line.Trim() != "")
                            return line.TrimStart().StartsWith("debugMode:", StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            return false;
        }

        // Copies the model's files and writes the input to <install>\POCBMobile\input.txt, and returns how to start
        // POCBMobile.exe there. A template (inputTemplate*.txt) is filled first with the picked
        // database / model / panel ID (see PocbTemplate); a filled input (inputTest*.txt) is
        // copied as is.
        public static ProcessStartInfo PreparePocbMobile(string inputFile, string install,
            string database, string model, string pid, LogFn log)
        {
            string dir = Path.Combine(install, "POCBMobile"), exe = Path.Combine(dir, "POCBMobile.exe");
            if (!File.Exists(exe)) throw new FileNotFoundException("POCBMobile.exe not found: " + exe);
            string text = File.ReadAllText(inputFile, Encoding.Default);
            if (PocbTemplate.Tokens(text).Length > 0)
            {
                text = PocbTemplate.Fill(text, PocbTemplate.Values(inputFile, database, model, pid, log));
                var left = PocbTemplate.Tokens(text);
                if (left.Length > 0)
                    throw new InvalidDataException($"Could not fill {string.Join(", ", left)} in {Path.GetFileName(inputFile)}.");
            }
            // A filled input (inputTest*.txt) names its own database and model; a template uses the picked ones.
            string fileDb = Line(text, "defaultFolder"), fileModel = Line(text, "model");
            CopyModelFiles(fileDb ?? database, fileModel ?? model, dir, log);

            string input = Path.Combine(dir, "input.txt");
            File.WriteAllText(input, text, Encoding.Default);   // content only: no Zone.Identifier from the kit
            log($"Wrote {Path.GetFileName(inputFile)} → {input}", LogLevel.Info);
            return new ProcessStartInfo
            {
                FileName = exe,
                WorkingDirectory = dir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // It ends with "Press Enter to exit...": stdin is closed after start so that
                // read returns at once instead of hanging the run.
                RedirectStandardInput = true,
            };
        }

        // "key: value" line of a POCBMobile input; null if missing or still a placeholder.
        static string Line(string text, string key)
        {
            var m = Regex.Match(text, "^" + key + @":\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return m.Success && PocbTemplate.Tokens(m.Groups[1].Value).Length == 0 ? m.Groups[1].Value : null;
        }

        // POCBMobile.exe loads the model's files (LGD_PO_Compensation.dll, PANEL_SHAPE_*.tif, LUT,
        // csv...) from its own folder, so the files of <database>\InputIMG\<model> are copied there
        // first. Files only: subfolders are panel image folders (TEMP\...), read where they are.
        static void CopyModelFiles(string database, string model, string dir, LogFn log)
        {
            if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(model))
                throw new InvalidDataException("No database / model to copy the model files from.");
            string src = Path.Combine(database, "InputIMG", model);
            if (!Directory.Exists(src)) throw new DirectoryNotFoundException("Model folder not found: " + src);
            var files = Directory.GetFiles(src).Where(f => !Kit.NeverCopy(f)).ToList();
            log($"Copy {files.Count} model file(s) {src} → {dir}", LogLevel.Info);
            var r = new Copier.Result();
            foreach (string f in files) Copier.CopyOne(f, Path.Combine(dir, Path.GetFileName(f)), r, log);
            if (r.Failed > 0)
                throw new IOException($"{r.Failed} model file(s) could not be copied"
                    + (r.AccessDenied ? " - access denied: use \"Run as admin\"." : "."));
        }

        // RunTestPUC.exe was used for inputs whose exe is a PUC tool; RunTestOmit.exe for the rest.
        public static bool LooksLikePuc(string inputFile) =>
            (ExeOf(inputFile) ?? "").IndexOf("PUC", StringComparison.OrdinalIgnoreCase) >= 0;

        // Input format shared by RunTestOmit.exe and RunTestPUC.exe: line 1 = executable, each
        // further line = one argument. An argument that ends in "\" (or is an existing folder)
        // is a folder and is created if missing; arguments with spaces are quoted.
        //
        // puc = RunTestPUC rules on top: every folder argument gets "Test\" appended, and a bare
        // file name containing "puc" ending .hex/.txt (not "dummy") is placed in that Test folder.
        //
        // The old launchers sat in the kit folder, which was therefore their current
        // directory: a relative exe or folder was looked up there. That folder is the input
        // file's folder, so relative names resolve against it here (exe: then workDir).
        public static ProcessStartInfo Parse(string inputFile, string workDir, bool puc, LogFn log)
        {
            var lines = File.ReadAllLines(inputFile);
            if (lines.Length == 0 || lines[0].Trim() == "")
                throw new InvalidDataException("Input file is empty - line 1 must be the executable.");
            string pkg = Path.GetDirectoryName(Path.GetFullPath(inputFile));

            string exe = lines[0].Trim();
            if (!Path.IsPathRooted(exe))
            {
                string hit = new[] { pkg, workDir }.Select(d => Path.Combine(d, exe)).FirstOrDefault(File.Exists);
                if (hit != null) exe = hit;
                else log($"{exe} not found in the kit folder or working dir - relying on PATH.", LogLevel.Warn);
            }

            var args = new List<string>();
            string testOutput = "";
            foreach (string raw in lines.Skip(1))
            {
                string arg = raw.Trim();
                bool isFolder = arg.EndsWith("\\") || IsExistingDir(pkg, arg);
                if (isFolder)
                {
                    if (puc)
                    {
                        if (!arg.EndsWith("\\")) arg += "\\";
                        arg += "Test\\";
                        testOutput = arg;
                    }
                    try
                    {
                        string full = Path.Combine(pkg, arg);
                        if (!Directory.Exists(full)) { Directory.CreateDirectory(full); log($"Created folder: {arg}", LogLevel.Detail); }
                    }
                    catch (Exception ex) { log($"Error creating folder {arg}: {ex.Message}", LogLevel.Warn); }
                }
                // "Bare file name" = no path separator. Path.GetFileName is not used for this: on
                // .NET Framework it throws for < > | in the argument.
                if (puc && arg.IndexOfAny(new[] { '\\', '/' }) < 0
                        && arg.IndexOf("puc", StringComparison.OrdinalIgnoreCase) >= 0
                        && (arg.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) || arg.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        && arg.IndexOf("dummy", StringComparison.OrdinalIgnoreCase) < 0)
                    arg = testOutput + arg;

                // A trailing "\" right before the closing quote would escape it ("C:\a b\" → C:\a b"),
                // so double it. Both old launchers had this bug for folder arguments with spaces.
                if (arg.Contains(" ")) arg = "\"" + (arg.EndsWith("\\") ? arg + "\\" : arg) + "\"";
                args.Add(arg);
            }

            return new ProcessStartInfo
            {
                FileName = exe,
                Arguments = string.Join(" ", args),
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        }

        // An argument with < > | (e.g. ">nul") makes Path.Combine throw on .NET Framework, which
        // used to abort the whole run with "Illegal characters in path". Such an argument is
        // simply not a folder.
        static bool IsExistingDir(string pkg, string arg)
        {
            if (arg == "") return false;
            try { return Directory.Exists(Path.Combine(pkg, arg)); }
            catch (ArgumentException) { return false; }
        }
    }
    // POCBMobile input templates (inputTemplate*.txt) hold placeholders that TrueTest replaces
    // when it writes POCBMobile\input.txt for a PUC step (worked out by comparing with TrueTest's
    // filled POCBMobile\inputTest\*.txt):
    //   DLLDefFolder, mPUCDLLModel, PID   picked in the Run test panel (Database / Model / Panel ID)
    //   nMonth, nDay                      today (2 digits)
    //   ExeDebugMode (false/true -> 0/1), INIFile   from the sequence step naming the template in
    //                                     <PUCTempplateArgFile>, else 0 and LGD_NY_COMP_INI.ini
    public static class PocbTemplate
    {
        public static readonly string[] Databases = { @"C:\DATABASE", @"D:\DATABASE", @"E:\DATABASE" };

        static readonly Regex Token = new Regex(@"\b(ExeDebugMode|nMonth|nDay|DLLDefFolder|mPUCDLLModel|PID|INIFile)\b");

        public static string[] Tokens(string text) =>
            Token.Matches(text).Cast<Match>().Select(m => m.Value).Distinct().ToArray();

        public static string Fill(string text, IDictionary<string, string> values) =>
            Token.Replace(text, m => values.TryGetValue(m.Value, out string v) && !string.IsNullOrEmpty(v) ? v : m.Value);

        // Model folders to pick from: <database>\InputIMG\*. The PUC model is the one with "00".
        public static string[] Models(string database)
        {
            try
            {
                string dir = Path.Combine(database, "InputIMG");
                return Directory.Exists(dir)
                    ? Directory.GetDirectories(dir).Select(Path.GetFileName).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray()
                    : new string[0];
            }
            catch { return new string[0]; }
        }

        public static bool IsPucModel(string model) => model.Contains("00");

        // <PUCDLLModel> of the kit's sequence step for this template: the default pick in the list.
        public static string SequenceModel(string template)
        {
            try
            {
                string seq = FindSequence(template);
                return seq == null ? null : StepOf(seq, Path.GetFileName(template), out _)?.Element("PUCDLLModel")?.Value.Trim();
            }
            catch { return null; }
        }

        public static Dictionary<string, string> Values(string template, string database, string model, string pid, LogFn log)
        {
            var v = new Dictionary<string, string>
            {
                ["ExeDebugMode"] = "0",
                ["nMonth"] = DateTime.Now.ToString("MM"),
                ["nDay"] = DateTime.Now.ToString("dd"),
                ["DLLDefFolder"] = database,
                ["mPUCDLLModel"] = model,
                ["PID"] = pid,
                ["INIFile"] = "LGD_NY_COMP_INI.ini",
            };
            string seq = FindSequence(template);
            bool exact = false;
            var step = seq == null ? null : StepOf(seq, Path.GetFileName(template), out exact);
            if (step != null)
            {
                string Get(string tag) => step.Element(tag)?.Value.Trim();
                string d = Get("ExeDebugMode");
                if (!string.IsNullOrEmpty(d))
                    v["ExeDebugMode"] = d.Equals("true", StringComparison.OrdinalIgnoreCase) ? "1"
                                      : d.Equals("false", StringComparison.OrdinalIgnoreCase) ? "0" : d;
                if (!string.IsNullOrEmpty(Get("INIFile"))) v["INIFile"] = Get("INIFile");
                log("ExeDebugMode / INIFile from " + Path.GetFileName(seq)
                    + (exact ? "" : " (no step names this template: first PUC step used)"), LogLevel.Detail);
            }
            log("Template values: " + string.Join(", ", v.Select(kv => kv.Key + " = " + kv.Value)), LogLevel.Info);
            return v;
        }

        // The kit's sequence: in the template's folder or up to two folders above
        // (kit\inputTemplateXXX\inputTemplate*.txt sits one below the .seqxc).
        static string FindSequence(string template)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(template));
            for (int up = 0; up <= 2 && dir != null; up++, dir = Path.GetDirectoryName(dir))
            {
                var seq = Directory.GetFiles(dir, "*.seqxc").OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
                if (seq != null) return seq;
            }
            return null;
        }

        // The step naming this template, else the first one with a PUC model.
        static System.Xml.Linq.XElement StepOf(string seq, string templateName, out bool exact)
        {
            exact = false;
            var doc = System.Xml.Linq.XDocument.Load(seq);
            var named = doc.Descendants("PUCTempplateArgFile")
                .FirstOrDefault(e => string.Equals(e.Value.Trim(), templateName, StringComparison.OrdinalIgnoreCase));
            if (named != null) { exact = true; return named.Parent; }
            return doc.Descendants("PUCDLLModel").FirstOrDefault(e => e.Value.Trim() != "")?.Parent;
        }
    }
}
