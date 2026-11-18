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
        public string[] SubFolders;
        public bool IncludeSubdirs;          // xcopy /E of subfolders
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
            new CopyTarget { Name = "PUC", SubFolders = new[] { "PUC" } },
            // Some Omit kits carry subfolders too (the old .bat copied top-level files only).
            new CopyTarget { Name = "ScratchDustDetect_NY", SubFolders = new[] { "ScratchDustDetect_NY" }, IncludeSubdirs = true },
            new CopyTarget { Name = "LGDVHCustomApp", SubFolders = new[] { "LGDVHCustomApp" }, IncludeSubdirs = true },
        };
    }

    public static class Copier
    {
        public sealed class Result { public int Copied, Failed, Skipped; public bool AccessDenied; }

        // Copies the top-level files of `source` (and subfolders when the target asks for it)
        // into each of target.SubFolders under `install`. `ownExe` is never copied.
        public static Result Run(string source, string install, CopyTarget target, string ownExe, LogFn log)
        {
            var r = new Result();
            var files = Directory.GetFiles(source)
                .Where(f => !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var skipped = files.Where(target.Skip).ToList();
            files = files.Except(skipped).ToList();
            r.Skipped = skipped.Count;
            if (skipped.Count > 0)
                log($"Skipping {skipped.Count} file(s) by rule: {string.Join(", ", skipped.Select(Path.GetFileName))}", LogLevel.Detail);

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

            foreach (string sub in target.SubFolders)
            {
                string dest = Path.Combine(install, sub);
                log($"→ {dest}", LogLevel.Info);
                try { Directory.CreateDirectory(dest); }
                catch (Exception ex)
                {
                    log($"Cannot create {dest}: {ex.Message}", LogLevel.Error);
                    r.Failed += files.Count;
                    r.AccessDenied |= ex is UnauthorizedAccessException;
                    continue;
                }

                foreach (string f in files)
                    CopyOne(f, Path.Combine(dest, Path.GetFileName(f)), r, log);

                foreach (var (from, rel) in subFiles)
                {
                    string to = Path.Combine(dest, rel);
                    try { Directory.CreateDirectory(Path.GetDirectoryName(to)); } catch { }
                    CopyOne(from, to, r, log, rel);
                }
                // xcopy /E also recreates empty folders, including each top subfolder itself.
                foreach (string d in dirs)
                    foreach (string e in Directory.GetDirectories(d, "*", SearchOption.AllDirectories).Prepend(d))
                        try { Directory.CreateDirectory(Path.Combine(dest, e.Substring(root.Length + 1))); } catch { }
            }
            return r;
        }

        // 3 attempts on IOException (a file briefly held by a closing TrueTest), as the POCB exe did.
        internal static void CopyOne(string from, string to, Result r, LogFn log, string label = null)
        {
            label = label ?? Path.GetFileName(from);
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.Copy(from, to, true);
                    log($"  copied  {label}", LogLevel.Detail);
                    r.Copied++;
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

        public static Copier.Result Run(string from, string to, IEnumerable<CopyTarget> targets, bool skipEdtMobile, LogFn log)
        {
            var r = new Copier.Result();
            foreach (string sub in targets.SelectMany(t => t.SubFolders))
            {
                string src = Path.Combine(from, sub), dst = Path.Combine(to, sub);
                if (!Directory.Exists(src)) { log($"{sub}: not in {from}, skipped", LogLevel.Warn); continue; }
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
                        continue;
                    }
                    string target = Path.Combine(dst, rel);
                    try { Directory.CreateDirectory(Path.GetDirectoryName(target)); }
                    catch (Exception ex)
                    {
                        log($"  FAILED  {sub}\\{rel}: {ex.Message}", LogLevel.Error);
                        r.Failed++;
                        r.AccessDenied |= ex is UnauthorizedAccessException;
                        continue;
                    }
                    Copier.CopyOne(f, target, r, log, sub + "\\" + rel);
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

            if (string.Equals(Path.GetFileName(folder.TrimEnd('\\')), "POCB4.2", StringComparison.OrdinalIgnoreCase))
            { why = "folder named POCB4.2"; return T("POCB4.2"); }
            if (Has(@"^Crop_params\.json$") || Directory.Exists(Path.Combine(folder, "mainTemplate")))
            { why = "Crop_params.json / mainTemplate folder"; return T("POCB4.2"); }
            if (Has(@"^ScratchDustDetect.*\.exe$") || Has("^LGD_OMIT_INI")) { why = "ScratchDustDetect / LGD_OMIT_INI files"; return T("ScratchDustDetect_NY"); }
            if (Has(@"^VNTT_Interp.*\.exe$")) { why = "VNTT_Interp exes"; return T("LGDVHCustomApp"); }
            if (Has(@"^LGD_PO_Compensation") || Has("^LGD_COMP_INI")) { why = "LGD_PO_Compensation / LGD_COMP_INI files"; return T("POCB4.1Net"); }
            if (Has(@"PUC.*\.exe$")) { why = "PUC exe"; return T("PUC"); }
            return null;
        }

        // .txt files in the folder whose first line is an exe, i.e. RunTest inputs.
        public static string[] RunInputs(string folder) =>
            Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.txt").Where(f => RunTest.ExeOf(f) != null)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray()
                : new string[0];
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
}
