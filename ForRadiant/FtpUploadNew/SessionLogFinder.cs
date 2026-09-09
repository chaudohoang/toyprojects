using System.Text;

namespace FtpUpload;

/// <summary>
/// Finds the WinSCP session-log records for one panel.
///
/// A day produces hundreds of "{day}_winscp_{hhmmssfff}.log" files, one per FTP session, and
/// nothing in the name says which panel it covers. Answering "was this file really uploaded?"
/// otherwise means grepping the lot by hand — which is exactly what it took to settle the field
/// report where 97 host manifests read FAILED in the rawlog while sitting on the server.
///
/// Produces a text report: the dialogue lines that matter (the put command, the server reply, the
/// completed transfer) plus a per-file list of what actually landed, with byte counts.
/// </summary>
public static class SessionLogFinder
{
    /// <summary>Build the report and return its path, or null if the PID appears in no session log.</summary>
    public static string? Build(Config cfg, string pid, string? day = null)
    {
        if (string.IsNullOrWhiteSpace(pid)) return null;
        var dir = cfg.LogFullPath;
        if (!Directory.Exists(dir)) return null;

        var pattern = string.IsNullOrWhiteSpace(day) ? "*_winscp_*.log" : $"{day}_winscp_*.log";
        var logs = Directory.GetFiles(dir, pattern).OrderBy(f => f).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"WinSCP session records for PID {pid}");
        sb.AppendLine($"searched {logs.Count} session log(s) in {dir}");
        sb.AppendLine($"generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('=', 78));

        var landed = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var hitFiles = 0;
        var stores = 0; var done = 0; var errs = 0;

        foreach (var file in logs)
        {
            string[] lines;
            try { lines = File.ReadAllLines(file); } catch { continue; }
            if (!lines.Any(l => l.Contains(pid, StringComparison.OrdinalIgnoreCase))) continue;

            hitFiles++;
            sb.AppendLine();
            sb.AppendLine($"--- {Path.GetFileName(file)} " + new string('-', 40));

            foreach (var l in lines)
            {
                var keep = l.Contains("Script: put", StringComparison.Ordinal)
                        || l.Contains("Script: open", StringComparison.Ordinal)
                        || l.Contains("> STOR ", StringComparison.Ordinal)
                        || l.Contains("Transfer done:", StringComparison.Ordinal)
                        || l.Contains("Script: Failed", StringComparison.Ordinal)
                        || l.Contains("Exit code:", StringComparison.Ordinal)
                        || System.Text.RegularExpressions.Regex.IsMatch(l, @"^\<.*\b(150|226|550|553|530|421)\b");
                if (!keep) continue;

                if (l.Contains("> STOR ", StringComparison.Ordinal)) stores++;
                if (l.Contains("Transfer done:", StringComparison.Ordinal)) done++;
                if (System.Text.RegularExpressions.Regex.IsMatch(l, @"\b(550|553|530|421)\b")) errs++;

                var t = l.Trim();
                sb.AppendLine("   " + (t.Length > 220 ? t[..220] + " ..." : t));

                if (l.Contains("Transfer done:", StringComparison.Ordinal) &&
                    l.Contains(pid, StringComparison.OrdinalIgnoreCase))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(l, @"=> '([^']+)'\s*\[(\d+)\]");
                    if (m.Success && long.TryParse(m.Groups[2].Value, out var bytes))
                        landed[Path.GetFileName(m.Groups[1].Value)] = bytes;
                }
            }
        }

        if (hitFiles == 0) return null;

        sb.AppendLine();
        sb.AppendLine("=== files this PID got onto the server (from \"Transfer done\") ===");
        if (landed.Count == 0)
            sb.AppendLine("   (none — the PID appears in the logs but no transfer completed)");
        else
            foreach (var kv in landed.OrderBy(k => k.Key))
                sb.AppendLine($"   {kv.Key,-46} {kv.Value,12:N0} bytes");

        sb.AppendLine();
        sb.AppendLine(new string('=', 78));
        sb.AppendLine($"session logs mentioning the PID : {hitFiles}");
        sb.AppendLine($"completed transfers             : {done}");
        sb.AppendLine($"distinct files landed           : {landed.Count}");
        sb.AppendLine($"4xx/5xx replies seen            : {errs}");
        sb.AppendLine();
        sb.AppendLine("Note: \"Script: Failed\" and a non-zero exit code are usually just the directory");
        sb.AppendLine("probe (a stat on a folder that does not exist yet) — the upload right after it");
        sb.AppendLine("normally succeeds. Trust \"Transfer done\" and the 226 reply, not the exit code.");

        try
        {
            var outDir = Path.Combine(Path.GetTempPath(), "FtpUploadSession");
            Directory.CreateDirectory(outDir);
            foreach (var old in Directory.GetFiles(outDir, "*.txt"))
                if ((DateTime.Now - File.GetLastWriteTime(old)).TotalDays > 1)
                    try { File.Delete(old); } catch { }

            var outPath = Path.Combine(outDir, $"{pid}_session_{DateTime.Now:HHmmss}.txt");
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(true));
            return outPath;
        }
        catch { return null; }
    }
}
