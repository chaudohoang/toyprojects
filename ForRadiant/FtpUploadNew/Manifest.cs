namespace FtpUpload;

/// <summary>
/// Owns the per-panel index + host manifests ({PID}.idx and {PID}_{DateTime}.txt) that live in
/// the panel's source folder and are the customer-facing "panel complete" signal.
///
/// Lifecycle (all keyed on a file's server destination path):
///   Seed        - create (or resume) both manifests with one line per data file:
///                     {destPath}@{channel} -pending
///   MarkUploaded- strip the " -pending" suffix on that file's line (index first, then host)
///   DropLine    - remove that file's line entirely (source gone / terminal)
///   AllResolved - true once no line still carries " -pending"
///   Finalize    - when resolved, upload index->UploadIndexPath and host->UploadHostPath
///
/// A clean line is "{destPath}@{channel}"; a pending line is that plus " -pending". We only ever
/// strip exactly what we wrote, so the customer never sees the marker.
/// </summary>
public sealed class ManifestWriter(Config cfg)
{
    private const string Pending = " -pending";

    /// <summary>Does this line belong to the given destination file?</summary>
    private static bool IsFor(string line, string destPath) =>
        line.StartsWith(destPath + "@", StringComparison.OrdinalIgnoreCase);

    /// <summary>Create or resume both manifests for a panel from its current DATA files.</summary>
    public void Seed(Job job)
    {
        if (!job.IsPanelJob) return;
        var dataFiles = job.Files.Where(f => !f.IsManifest).ToList();   // manifests never list themselves
        if (dataFiles.Count == 0) return;
        SafeFile.WithLock(() =>
        {
            // Resume: keep whatever state (clean or -pending) an existing host manifest already has.
            var existing = ReadMap(job.HostSrc);
            var lines = new List<string>(dataFiles.Count);
            foreach (var f in dataFiles)
            {
                var dest = f.RemotePath;
                lines.Add(existing.TryGetValue(dest, out var prev)
                    ? prev
                    : dest + "@" + job.ChannelIndex + Pending);
            }
            WriteRaw(job.IndexSrc, lines);
            WriteRaw(job.HostSrc, lines);
        });
    }

    /// <summary>On a successful upload: strip " -pending" from that file's line, index then host.</summary>
    public void MarkUploaded(Job job, string destPath)
    {
        if (!job.IsPanelJob) return;
        MarkUploaded(job.IndexSrc, job.HostSrc, destPath);
    }

    /// <summary>Path-based: used by the NG-retry pump, which has no live Job.</summary>
    public void MarkUploaded(string indexSrc, string hostSrc, string destPath)
    {
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc)) return;
        SafeFile.WithLock(() =>
        {
            StripOne(indexSrc, destPath);
            StripOne(hostSrc, destPath);
        });
    }

    /// <summary>Source file is gone for good: remove its line from both manifests.</summary>
    public void DropLine(Job job, string destPath)
    {
        if (!job.IsPanelJob) return;
        DropLine(job.IndexSrc, job.HostSrc, destPath);
    }

    /// <summary>Path-based: used by the NG-retry pump.</summary>
    public void DropLine(string indexSrc, string hostSrc, string destPath)
    {
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc)) return;
        SafeFile.WithLock(() =>
        {
            RemoveOne(indexSrc, destPath);
            RemoveOne(hostSrc, destPath);
        });
    }

    /// <summary>True once the panel's manifests have been sent (the ".sent" sentinel exists).</summary>
    public bool IsSent(Job job)
    {
        if (!job.IsPanelJob || string.IsNullOrEmpty(job.IndexSrc)) return false;
        try { return File.Exists(job.IndexSrc + ".sent"); } catch { return false; }
    }

    /// <summary>True once the host manifest exists and no line still carries " -pending".</summary>
    public bool AllResolved(Job job)
    {
        if (!job.IsPanelJob) return false;
        var resolved = false;
        SafeFile.WithLock(() =>
        {
            if (!File.Exists(job.HostSrc)) return;
            var lines = ReadRaw(job.HostSrc);
            resolved = lines.Count > 0 && !lines.Any(l => l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal));
        });
        return resolved;
    }

    /// <summary>
    /// Job overload: finalize a live panel. Sets job.Finalized on success (fast in-memory skip).
    /// </summary>
    public async Task<bool> TryFinalizeAsync(Job job, IFtpTransfer ftp)
    {
        if (job.Finalized || !job.IsPanelJob) return false;
        var sent = await TryFinalizeAsync(job.IndexSrc, job.HostSrc, job.UploadIndexPath, job.UploadHostPath, ftp);
        if (sent) job.Finalized = true;
        return sent;
    }

    /// <summary>
    /// Path-based finalize, usable by BOTH the live engine and the NG pump (the latter for past-day
    /// panels that have no live Job). Sends index -> UploadIndexPath and host -> UploadHostPath only
    /// when the panel is fully resolved (no " -pending" left) and not already sent.
    ///
    /// A ".sent" sentinel beside the index manifest is CLAIMED under the shared file lock before
    /// uploading, so live and NG can never both send the same panel; on upload failure the claim is
    /// released so a later tick retries. The sentinel also survives restart, so a completed panel is
    /// never re-sent.
    /// </summary>
    public async Task<bool> TryFinalizeAsync(string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath, IFtpTransfer ftp)
    {
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc)) return false;
        var sentinel = indexSrc + ".sent";

        var claimed = false;
        SafeFile.WithLock(() =>
        {
            if (File.Exists(sentinel)) return;                       // already sent, or being sent
            if (!File.Exists(hostSrc) || !File.Exists(indexSrc)) return;
            var lines = ReadRaw(hostSrc);
            if (lines.Count == 0 || lines.Any(l => l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal))) return;
            try { File.WriteAllText(sentinel, DateTime.Now.ToString("o")); claimed = true; } catch { }
        });
        if (!claimed) return false;

        var okIdx = await SendAsync(ftp, indexSrc, uploadIndexPath);
        var okHost = await SendAsync(ftp, hostSrc, uploadHostPath);
        if (okIdx && okHost) return true;

        // Upload failed — release the claim so a later tick retries.
        SafeFile.WithLock(() => { try { File.Delete(sentinel); } catch { } });
        return false;
    }

    private async Task<bool> SendAsync(IFtpTransfer ftp, string localPath, string remotePath)
    {
        var jf = new JobFile
        {
            Pid = "", FileName = Path.GetFileName(localPath),
            LocalPath = localPath, RemotePath = remotePath
        };
        var r = await ftp.UploadToHostAsync(jf, cfg.PrimaryHost, CancellationToken.None);
        if (r.Outcome != TransferOutcome.Success && !string.IsNullOrWhiteSpace(cfg.SecondaryHost))
            r = await ftp.UploadToHostAsync(jf, cfg.SecondaryHost, CancellationToken.None);
        return r.Outcome == TransferOutcome.Success;
    }

    // ---- raw manifest file helpers (callers already hold the SafeFile lock) ----

    private static List<string> ReadRaw(string path)
    {
        try { return File.Exists(path) ? File.ReadAllLines(path).Where(l => l.Length > 0).ToList() : new(); }
        catch { return new(); }
    }

    private static void WriteRaw(string path, IEnumerable<string> lines)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllLines(path, lines);
    }

    private static Dictionary<string, string> ReadMap(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in ReadRaw(path))
        {
            var at = line.IndexOf('@');
            if (at <= 0) continue;
            map[line[..at]] = line;   // key = destPath (text before '@')
        }
        return map;
    }

    private static void StripOne(string path, string destPath)
    {
        var lines = ReadRaw(path);
        var changed = false;
        for (var i = 0; i < lines.Count; i++)
            if (IsFor(lines[i], destPath) && lines[i].EndsWith(Pending, StringComparison.Ordinal))
            {
                lines[i] = lines[i][..^Pending.Length];
                changed = true;
            }
        if (changed) WriteRaw(path, lines);
    }

    private static void RemoveOne(string path, string destPath)
    {
        var lines = ReadRaw(path);
        var kept = lines.Where(l => !IsFor(l, destPath)).ToList();
        if (kept.Count != lines.Count) WriteRaw(path, kept);
    }
}
