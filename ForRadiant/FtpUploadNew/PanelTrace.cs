using System.Text;

namespace FtpUpload;

/// <summary>
/// Everything known about one panel, gathered from every source, in one report.
///
/// A panel's story is spread across five places: the jobs file (what files it has), the rawlog
/// (what the live pump did), the ng-retry log (what recovery did), the marker files on disk (what
/// finalize believes), and the WinSCP session logs (what the server actually received). Answering
/// "did this panel really upload?" has meant cross-referencing all five by hand — that is how the
/// field report of 97 host manifests reading FAILED while sitting on the server was settled.
///
/// The last section is the one that matters: expected files against what each source says landed.
/// </summary>
public static class PanelTrace
{
    public static string Build(Config cfg, string pid, string? day = null)
    {
        pid = (pid ?? "").Trim();
        var sb = new StringBuilder();
        if (pid.Length == 0) return "Type a panel ID and press Search.";

        var days = new List<string>();
        if (!string.IsNullOrWhiteSpace(day)) days.Add(day!);
        else days.AddRange(DaysAvailable(cfg));

        // Resolve a PARTIAL id first. Everywhere else in the app the PID box is a substring filter,
        // so "382" is expected to find TSN000000000382 — and the Raw tab did exactly that while this
        // one demanded an exact match, so the two tabs disagreed about whether the panel existed.
        var matches = ResolvePids(cfg, pid, days);
        if (matches.Count > 1)
        {
            sb.AppendLine($"TRACE PANEL   {pid}");
            sb.AppendLine(new string('=', 84));
            sb.AppendLine();
            sb.AppendLine($"  \"{pid}\" matches {matches.Count} panels. Type more of the id:");
            sb.AppendLine();
            foreach (var m in matches.Take(40)) sb.AppendLine("     " + m);
            if (matches.Count > 40) sb.AppendLine($"     ... and {matches.Count - 40} more");
            return sb.ToString();
        }
        if (matches.Count == 1) pid = matches[0];      // exact id from here on

        sb.AppendLine($"TRACE PANEL   {pid}");
        sb.AppendLine($"generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}   ·   searched {days.Count} day(s)");
        sb.AppendLine(new string('=', 84));

        var jobFiles = new List<JobsLine>();
        var rawRows = new List<string>();
        var ngRows = new List<string>();
        var foundDay = "";

        foreach (var d in days)
        {
            foreach (var line in SafeFile.ReadLines(cfg.JobsPathForDay(d)))
            {
                var jl = JobsLine.Parse(line);
                if (jl is not null && jl.Pid.Equals(pid, StringComparison.OrdinalIgnoreCase))
                { jobFiles.Add(jl); foundDay = d; }
            }
            foreach (var line in SafeFile.ReadLines(cfg.RawLogPathForDay(d)))
                if (line.StartsWith(pid + "|", StringComparison.OrdinalIgnoreCase)) { rawRows.Add(line); foundDay = d; }
            foreach (var line in SafeFile.ReadLines(cfg.NgRetryLogPath(d)))
                if (line.StartsWith(pid + "|", StringComparison.OrdinalIgnoreCase)) { ngRows.Add(line); foundDay = d; }
        }

        if (jobFiles.Count == 0 && rawRows.Count == 0 && ngRows.Count == 0)
        {
            sb.AppendLine();
            sb.AppendLine("  Nothing found for that panel ID in any log.");
            sb.AppendLine();
            sb.AppendLine("  - check the spelling (the match is exact, not partial)");
            sb.AppendLine("  - the day's logs may have been removed by log retention");
            sb.AppendLine("  - the panel may never have been ingested (look in the queue's rejected folder)");
            return sb.ToString();
        }
        if (foundDay.Length > 0) sb.AppendLine($"day: {foundDay}");
        return Sections(cfg, pid, jobFiles, rawRows, ngRows, sb);
    }

    /// <summary>
    /// The VERBATIM lines every source holds for this panel — no interpretation.
    ///
    /// Build() renders a judgement; this shows the evidence. When the two disagree you need the raw
    /// rows: the missing ".hostsent" was invisible for exactly this reason — the summary said "no
    /// session log to confirm" while the truth sat in a line the summary never printed.
    /// </summary>
    public static string BuildRaw(Config cfg, string pid, string? day = null)
    {
        pid = (pid ?? "").Trim();
        if (pid.Length == 0) return "Type a panel ID and press Search.";

        var days = new List<string>();
        if (!string.IsNullOrWhiteSpace(day)) days.Add(day!);
        else days.AddRange(DaysAvailable(cfg));

        // Resolve the same way the Report tab does, so the two tabs never disagree about which
        // panel — or whether one exists at all.
        var matches = ResolvePids(cfg, pid, days);
        var sb = new StringBuilder();
        if (matches.Count > 1)
        {
            sb.AppendLine($"RAW LOG LINES   {pid}");
            sb.AppendLine(new string('=', 84));
            sb.AppendLine();
            sb.AppendLine($"  \"{pid}\" matches {matches.Count} panels. Type more of the id:");
            sb.AppendLine();
            foreach (var m in matches.Take(40)) sb.AppendLine("     " + m);
            if (matches.Count > 40) sb.AppendLine($"     ... and {matches.Count - 40} more");
            return sb.ToString();
        }
        if (matches.Count == 1) pid = matches[0];

        sb.AppendLine($"RAW LOG LINES   {pid}");
        sb.AppendLine($"generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}   ·   exact lines, unedited");
        sb.AppendLine(new string('=', 84));

        var any = false;
        foreach (var d in days)
        {
            foreach (var (label, path) in new[]
                     {
                         ($"jobs file : {d}_jobs.txt",      cfg.JobsPathForDay(d)),
                         ($"rawlog    : {d}_rawlog.txt",    cfg.RawLogPathForDay(d)),
                         ($"ng-retry  : {d}_ngretrylog.txt", cfg.NgRetryLogPath(d)),
                         ($"oplog     : {d}_oplog.txt",     cfg.OpLogPath(ParseDay(d)))
                     })
            {
                var hits = SafeFile.ReadLines(path)
                                   .Where(l => l.Contains(pid, StringComparison.OrdinalIgnoreCase))
                                   .ToList();
                if (hits.Count == 0) continue;
                any = true;
                sb.AppendLine();
                sb.AppendLine($"--- {label}   ({hits.Count} line(s)) " + new string('-', 20));
                foreach (var l in hits) sb.AppendLine("   " + l);
            }
        }

        // the manifests themselves, and the .midfail record
        foreach (var d in days)
        {
            var idxSrc = "";
            var hostSrc = "";
            foreach (var line in SafeFile.ReadLines(cfg.JobsPathForDay(d)))
            {
                var jl = JobsLine.Parse(line);
                if (jl is not null && jl.Pid.Equals(pid, StringComparison.OrdinalIgnoreCase) && jl.IndexSrc.Length > 0)
                { idxSrc = jl.IndexSrc; hostSrc = jl.HostSrc; break; }
            }
            if (idxSrc.Length == 0) continue;
            // BOTH manifests, not just the index. They are written and stripped as two separate
            // files and CAN disagree — a host manifest reading clean while the index still carried
            // " -pending" lines is what shipped an index naming six files that were not on the
            // server. Seeing them side by side is the only way to spot that.
            foreach (var (label, p) in new[]
                     {
                         ($"index manifest : {Path.GetFileName(idxSrc)}", idxSrc),
                         ($"host manifest  : {Path.GetFileName(hostSrc)}", hostSrc),
                         ($"midfail record : {Path.GetFileName(Path.ChangeExtension(idxSrc, ".midfail"))}",
                          Path.ChangeExtension(idxSrc, ".midfail"))
                     })
            {
                if (string.IsNullOrWhiteSpace(p) || !File.Exists(p)) continue;
                any = true;
                sb.AppendLine();
                sb.AppendLine($"--- {label} " + new string('-', 20));
                foreach (var l in ReadEvenIfOpen(p)) sb.AppendLine("   " + l);
            }

            // Say so explicitly when they differ, rather than leaving it to be eyeballed.
            if (File.Exists(idxSrc) && !string.IsNullOrWhiteSpace(hostSrc) && File.Exists(hostSrc))
            {
                var a = string.Join("\n", ReadEvenIfOpen(idxSrc));
                var bb = string.Join("\n", ReadEvenIfOpen(hostSrc));
                sb.AppendLine();
                sb.AppendLine(a == bb
                    ? "   index and host manifests are IDENTICAL (expected)"
                    : "   *** index and host manifests DIFFER - they should normally match ***");
            }
            break;
        }

        // WinSCP dialogue
        try
        {
            foreach (var f in Directory.GetFiles(cfg.LogFullPath, "*_winscp_*.log").OrderBy(x => x))
            {
                var lines = ReadEvenIfOpen(f);
                var hits = lines.Where(l => l.Contains(pid, StringComparison.OrdinalIgnoreCase)).ToList();
                if (hits.Count == 0) continue;
                any = true;
                sb.AppendLine();
                sb.AppendLine($"--- session log : {Path.GetFileName(f)}   ({hits.Count} line(s)) " + new string('-', 12));
                foreach (var l in hits) sb.AppendLine("   " + l.Trim());
            }
        }
        catch { }

        if (!any) sb.AppendLine().AppendLine("  Nothing found for that panel ID in any file.");
        return sb.ToString();
    }

    /// <summary>
    /// Panel ids that CONTAIN the typed text, across the jobs file and both logs. Returns the typed
    /// text itself if it is already an exact id, so a full id never needs resolving.
    /// </summary>
    private static List<string> ResolvePids(Config cfg, string query, List<string> days)
    {
        var found = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in days)
        {
            foreach (var line in SafeFile.ReadLines(cfg.JobsPathForDay(d)))
            {
                var i = line.IndexOf('|');
                if (i <= 0) continue;
                var p = line[..i];
                if (p.Contains(query, StringComparison.OrdinalIgnoreCase)) found.Add(p);
            }
            foreach (var path in new[] { cfg.RawLogPathForDay(d), cfg.NgRetryLogPath(d) })
                foreach (var line in SafeFile.ReadLines(path))
                {
                    var i = line.IndexOf('|');
                    if (i <= 0) continue;
                    var p = line[..i];
                    if (p.Contains(query, StringComparison.OrdinalIgnoreCase)) found.Add(p);
                }
        }
        // An exact hit wins outright: never make someone disambiguate an id they typed in full.
        if (found.Contains(query)) return new List<string> { query };
        return found.ToList();
    }

    private static DateTime ParseDay(string day)
        => DateTime.TryParseExact(day, "yyyyMMdd", null,
               System.Globalization.DateTimeStyles.None, out var d) ? d : DateTime.Today;

    private static IEnumerable<string> DaysAvailable(Config cfg)
    {
        var set = new SortedSet<string>(StringComparer.Ordinal);
        try
        {
            foreach (var f in Directory.GetFiles(cfg.LogFullPath, "*_rawlog.txt"))
            {
                var n = Path.GetFileName(f);
                if (n.Length >= 8) set.Add(n[..8]);
            }
        }
        catch { }
        return set.Reverse();      // newest first: the panel is usually recent
    }

    /// <summary>
    /// Read a file WinSCP may still have open.
    ///
    /// Session logs stay open for the life of a reused connection, and the manifest sender keeps its
    /// own long-lived session — so a plain File.ReadAllLines throws on exactly the log that holds the
    /// manifest transfers. That failure was being swallowed, and the trace then reported the index and
    /// host manifests as "no session log to confirm" while they were sitting in an open log file.
    /// </summary>
    private static string[] ReadEvenIfOpen(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
                                          FileShare.ReadWrite | FileShare.Delete);
            using var sr = new StreamReader(fs);
            var list = new List<string>();
            string? line;
            while ((line = sr.ReadLine()) is not null) list.Add(line);
            return list.ToArray();
        }
        catch { return Array.Empty<string>(); }
    }

    private static string Sections(Config cfg, string pid, List<JobsLine> jobFiles,
                                   List<string> rawRows, List<string> ngRows, StringBuilder sb)
    {
        // ---- 1. the panel's files, and where they came from on disk --------------------
        sb.AppendLine();
        sb.AppendLine("--- FILES IN THIS PANEL (from the jobs file) ---------------------------------");
        var indexSrc = "";
        foreach (var jl in jobFiles)
        {
            sb.AppendLine($"   {jl.FileName,-44} {(jl.IsManifest ? "manifest" : "data")}");
            if (jl.IndexSrc.Length > 0) indexSrc = jl.IndexSrc;
        }
        if (jobFiles.Count == 0) sb.AppendLine("   (no jobs-file lines — panel may predate the current jobs file)");

        // ---- 2. what the live pump did -------------------------------------------------
        sb.AppendLine();
        sb.AppendLine("--- UPLOAD HISTORY (rawlog: live pump + NG write-backs) ----------------------");
        if (rawRows.Count == 0) sb.AppendLine("   (nothing — no upload was ever attempted for this panel)");
        foreach (var r in rawRows)
        {
            var p = r.Split('|');
            if (p.Length < 9) continue;
            // 11th field carries the reason a FAILED row failed, when there is one.
            var why = p.Length >= 11 && p[10].Trim().Length > 0 ? "  " + p[10].Trim() : "";
            sb.AppendLine($"   {p[1],-44} {p[2],-10} at {(p[3].Length > 0 ? p[3] : "-"),-9} att {p[6]}/{p[7]}  {p[8]}{why}");
        }

        // ---- 3. what recovery did ------------------------------------------------------
        sb.AppendLine();
        sb.AppendLine("--- NG RECOVERY (ng-retry log) ----------------------------------------------");
        if (ngRows.Count == 0) sb.AppendLine("   (none — this panel never needed recovery)");
        foreach (var r in ngRows)
        {
            var p = r.Split('|');
            if (p.Length < 9) continue;
            sb.AppendLine($"   {p[1],-44} {p[2],-10} retries {p[4],-4} {p[8]}");
        }

        // ---- 4. finalize markers on disk ----------------------------------------------
        sb.AppendLine();
        sb.AppendLine("--- FINALIZE MARKERS (on disk, beside the index manifest) --------------------");
        if (indexSrc.Length == 0) sb.AppendLine("   (index source path unknown — cannot check)");
        else
        {
            sb.AppendLine($"   folder: {Path.GetDirectoryName(indexSrc)}");
            foreach (var (suffix, meaning) in new[]
                     {
                         (".idxsent",  "index manifest is on the server"),
                         (".hostsent", "host manifest is on the server"),
                         (".sent",     "finalize lock (transient — should NOT persist)"),
                         (".midfail",  "early host manifest sent; value = files landed at the time")
                     })
            {
                // ".midfail" replaces the ".idx" extension; the sent-markers append to the full name.
                var f = suffix == ".midfail" ? Path.ChangeExtension(indexSrc, ".midfail") : indexSrc + suffix;
                var present = File.Exists(f);
                var extra = "";
                if (present && suffix == ".midfail")
                {
                    // now a log: one block per early send. Report how many sends and the latest.
                    try
                    {
                        var ls = File.ReadAllLines(f).Where(x => x.Contains("sig=", StringComparison.Ordinal)).ToList();
                        extra = ls.Count == 0 ? "" : $" = {ls.Count} send(s), last {ls[^1].Split("  ")[0]}";
                    }
                    catch { }
                }
                sb.AppendLine($"   {suffix,-11} {(present ? "present" : "-"),-8}{extra,-6} {meaning}");
            }
        }
        return WinScpAndVerdict(cfg, pid, jobFiles, rawRows, ngRows, sb);
    }

    /// <summary>
    /// The server's own account (WinSCP "Transfer done" lines) and a verdict per file.
    ///
    /// This is the section worth reading: the logs record what the app BELIEVES, the session logs
    /// record what the server actually received. Every confusing case this week came from those two
    /// disagreeing.
    /// </summary>
    private static string WinScpAndVerdict(Config cfg, string pid, List<JobsLine> jobFiles,
                                           List<string> rawRows, List<string> ngRows, StringBuilder sb)
    {
        var onServer = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var sessionFiles = 0;
        try
        {
            foreach (var f in Directory.GetFiles(cfg.LogFullPath, "*_winscp_*.log"))
            {
                var lines = ReadEvenIfOpen(f);
                if (lines.Length == 0) continue;
                if (!lines.Any(l => l.Contains(pid, StringComparison.OrdinalIgnoreCase))) continue;
                sessionFiles++;
                foreach (var l in lines)
                {
                    if (!l.Contains("Transfer done:", StringComparison.Ordinal)) continue;
                    if (!l.Contains(pid, StringComparison.OrdinalIgnoreCase)) continue;
                    var m = System.Text.RegularExpressions.Regex.Match(l, @"=> '([^']+)'\s*\[(\d+)\]");
                    if (m.Success && long.TryParse(m.Groups[2].Value, out var b))
                        onServer[Path.GetFileName(m.Groups[1].Value)] = b;
                }
            }
        }
        catch { }

        sb.AppendLine();
        sb.AppendLine("--- ON THE SERVER (WinSCP session logs: what the server confirmed) ----------");
        sb.AppendLine($"   session logs mentioning this panel: {sessionFiles}");
        if (onServer.Count == 0)
            sb.AppendLine("   (no confirmed transfers — logs purged, or the engine is not WinSCP)");
        else
            foreach (var kv in onServer.OrderBy(k => k.Key))
                sb.AppendLine($"   {kv.Key,-46} {kv.Value,12:N0} bytes");

        // ---- verdict: per file, what each source says ---------------------------------
        var okLogs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Files whose LOCAL source was deleted. "not uploaded" is true but useless on its own:
        // a transfer failure is worth retrying, a deleted source never will be, and the reason
        // field is the only thing that tells them apart.
        var sourceGone = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in rawRows.Concat(ngRows))
        {
            var p = r.Split('|');
            if (p.Length >= 3 && p[2] == "SUCCEEDED") okLogs.Add(p[1]);
            if (p.Length >= 11 && p[10].Trim() == "SOURCE_GONE") sourceGone.Add(p[1]);
        }

        sb.AppendLine();
        sb.AppendLine("--- VERDICT ------------------------------------------------------------------");
        sb.AppendLine("   file                                          logs      server");
        var landedCount = 0;
        var names = jobFiles.Select(j => j.FileName)
                            .Union(onServer.Keys, StringComparer.OrdinalIgnoreCase)
                            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);
        foreach (var n in names)
        {
            var inLogs = okLogs.Contains(n);
            var inSrv = onServer.ContainsKey(n);
            if (inLogs || inSrv) landedCount++;
            var note = (inLogs, inSrv) switch
            {
                (true, true) => "",
                (true, false) => "   <- logs say OK; no session log to confirm (purged?)",
                (false, true) => "   <- ON THE SERVER but the logs do not say so",
                _ => sourceGone.Contains(n)
                        ? "   <- SOURCE GONE: the local file was deleted; no retry can recover it"
                        : "   <- not uploaded"
            };
            sb.AppendLine($"   {n,-44}  {(inLogs ? "OK" : "--"),-8}  {(inSrv ? "OK" : "--"),-6}{note}");
        }

        var expected = jobFiles.Count;
        sb.AppendLine();
        sb.AppendLine($"   {landedCount} of {expected} file(s) accounted for" +
                      (expected > 0 && landedCount >= expected ? "  — panel complete" : ""));
        sb.AppendLine();
        sb.AppendLine(new string('=', 84));
        sb.AppendLine("Reading this: \"Script: Failed\" and a non-zero WinSCP exit code are normally just");
        sb.AppendLine("the directory probe (a stat on a folder that does not exist yet) — the upload");
        sb.AppendLine("right after it usually succeeds. Trust \"Transfer done\" and the 226 reply.");
        return sb.ToString();
    }

}
