namespace FtpUpload;

/// <summary>
/// Deletes old date-stamped logs/reports AND old per-day panel-backup subfolders so they don't
/// accumulate forever. Deliberately very conservative and fully defensive: any failure (a file
/// open/locked, a permissions issue, a folder in use) is swallowed per-item so housekeeping can
/// never break the app. It ONLY removes:
///   • files named "{yyyyMMdd}_....(txt|html)" in the Log and Jobs folders, and
///   • per-day subfolders named exactly "{yyyyMMdd}" inside the Backup Jobs folder,
/// and only when that date is older than the retention cutoff. Never today's, never non-date-named
/// items, never a folder that holds the exe, and never a {yyyyMMdd}_oplog.txt — the oplog is the
/// diagnostic record and is exempt from retention at any setting.
/// </summary>
public static class LogRetention
{
    public static void Purge(Config cfg, Action<string>? log = null)
    {
        try
        {
            var keepDays = cfg.LogRetentionDays;
            if (keepDays <= 0) return;   // 0 = keep everything forever (feature disabled)

            // Keep items dated within the last keepDays; delete anything strictly older.
            var cutoff = DateTime.Today.AddDays(-keepDays);

            foreach (var dir in new[] { cfg.LogFullPath, cfg.JobsFullPath })
                PurgeLogFiles(dir, cutoff, log);

            PurgeBackupFolders(cfg.PanelBackupFullPath, cutoff, log);
        }
        catch { /* never let housekeeping break the app */ }
    }

    /// <summary>Delete {yyyyMMdd}_*.txt / *.html older than cutoff in a log/jobs folder.</summary>
    private static void PurgeLogFiles(string dir, DateTime cutoff, Action<string>? log)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;
            if (File.Exists(Path.Combine(dir, "FtpUpload.exe"))) return;   // never the exe folder

            var deleted = 0;
            foreach (var f in Directory.GetFiles(dir))
            {
                try
                {
                    var name = Path.GetFileName(f);

                    // NEVER purge the oplog. It is the diagnostic record — startups, shutdowns and
                    // why, crashes, day rollovers, NG backlog — and it is minuscule next to the
                    // rawlogs and WinSCP session logs (a few hundred bytes a day, ~100 KB a year).
                    // Deleting it reclaims nothing and destroys the only evidence available when a
                    // site reports "it stopped uploading last month".
                    if (name.EndsWith("_oplog.txt", StringComparison.OrdinalIgnoreCase)) continue;

                    if (name.Length < 10 || name[8] != '_') continue;
                    var datePart = name.Substring(0, 8);
                    if (!datePart.All(char.IsDigit)) continue;

                    var ext = Path.GetExtension(name).ToLowerInvariant();
                    if (ext != ".txt" && ext != ".html" && ext != ".log") continue;

                    if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out var d)) continue;
                    if (d.Date >= cutoff) continue;   // recent (and today) — keep

                    File.Delete(f);
                    deleted++;
                }
                catch { /* one stuck file must not stop the rest */ }
            }

            if (deleted > 0)
                log?.Invoke($"[{DateTime.Now:HH:mm:ss}] log retention: removed {deleted} file(s) older than {cutoff:yyyy-MM-dd} from {dir}");
        }
        catch { /* never let housekeeping break the app */ }
    }

    /// <summary>Delete "{yyyyMMdd}" per-day subfolders older than cutoff in the backup folder.</summary>
    private static void PurgeBackupFolders(string backupRoot, DateTime cutoff, Action<string>? log)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(backupRoot) || !Directory.Exists(backupRoot)) return;
            if (File.Exists(Path.Combine(backupRoot, "FtpUpload.exe"))) return;   // never the exe folder

            var removed = 0;
            foreach (var sub in Directory.GetDirectories(backupRoot))
            {
                try
                {
                    var name = Path.GetFileName(sub);
                    if (name.Length != 8 || !name.All(char.IsDigit)) continue;   // must be exactly {yyyyMMdd}
                    if (!DateTime.TryParseExact(name, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out var d)) continue;
                    if (d.Date >= cutoff) continue;   // recent (and today) — keep

                    Directory.Delete(sub, recursive: true);
                    removed++;
                }
                catch { /* a folder with an open/locked file — skip it, try again next time */ }
            }

            if (removed > 0)
                log?.Invoke($"[{DateTime.Now:HH:mm:ss}] backup retention: removed {removed} day-folder(s) older than {cutoff:yyyy-MM-dd} from {backupRoot}");
        }
        catch { /* never let housekeeping break the app */ }
    }
}
