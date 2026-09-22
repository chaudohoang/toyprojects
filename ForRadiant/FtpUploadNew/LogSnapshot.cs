namespace FtpUpload;

/// <summary>
/// Open a log by handing the viewer a COPY, never the live file.
///
/// Excel opens a .csv with FileShare.Read, which denies writers: while it is open the report pump
/// cannot rewrite that file, and "--reports" silently keeps the stale one. That cost several rounds
/// of chasing data that was fine — the file simply could not be replaced. Notepad is friendlier but
/// not guaranteed, and a browser holding an .html has the same effect.
///
/// The copy is a point-in-time snapshot: reopen it to get newer data. That is the right trade for a
/// log, where blocking the writer is far worse than looking at a view from a minute ago.
/// </summary>
public static class LogSnapshot
{
    /// <summary>
    /// Open a log for viewing. A SPREADSHEET gets a temp copy; everything else opens live.
    /// Returns what was opened, or null if the source is missing.
    ///
    /// Excel is the one viewer that holds a log hostage: it opens a .csv with FileShare.Read, which
    /// denies writers, so the report pump cannot rewrite that day and "--reports" silently keeps the
    /// stale file. That cost several rounds of chasing data that was fine — it simply could not be
    /// replaced.
    ///
    /// Text and HTML open live, deliberately. Notepad reads the file and closes the handle; browsers
    /// do the same. And the HTML reports carry an auto-refresh tag, so a copy would sit in temp
    /// reloading itself and showing the moment it was taken — looking current while being stale,
    /// which is the very failure the copying was meant to prevent.
    /// </summary>
    public static string? OpenCopy(string? path, string label = "log")
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

        var ext = Path.GetExtension(path);
        var locksIt = ext.Equals(".csv", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".tsv", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".xls", StringComparison.OrdinalIgnoreCase)
                   || ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
        if (!locksIt)
        {
            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                return path;
            }
            catch { return null; }
        }

        try
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "FtpUploadView");
            Directory.CreateDirectory(tmpDir);

            // Prune yesterday's snapshots so this folder cannot grow without bound.
            try
            {
                foreach (var old in Directory.GetFiles(tmpDir))
                    if ((DateTime.Now - File.GetLastWriteTime(old)).TotalDays > 1)
                        try { File.Delete(old); } catch { }
            }
            catch { }

            // Timestamped: a previous copy may still be open, and therefore locked, in the viewer.
            var name = Path.GetFileNameWithoutExtension(path);
            var copy = Path.Combine(tmpDir, $"{name}_{DateTime.Now:HHmmss}{ext}");
            File.Copy(path, copy, overwrite: true);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(copy) { UseShellExecute = true });
            return copy;
        }
        catch
        {
            // Could not copy (disk full, temp not writable). Open the real file rather than fail:
            // the lock is a risk, showing nothing is a certainty.
            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                return path;
            }
            catch { return null; }
        }
    }
}
