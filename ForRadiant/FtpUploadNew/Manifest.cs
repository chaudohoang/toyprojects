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
                var hasPrev = existing.TryGetValue(dest, out var prev);

                // A file whose LOCAL source no longer exists must not be re-listed as pending.
                //
                // DropLine removes such a line, but the jobs file still names the file, so this
                // "resume" step read the absent line as "not uploaded yet" and put it back with
                // " -pending" on the next startup. The panel could then never satisfy the
                // no-pending gate and would never finalize again — seen on a panel that HAD
                // finalized, whose manifests were resurrected one second after a restart.
                //
                // If it already uploaded before the source vanished, keep that clean line: the file
                // is on the server and the manifest should still name it.
                if (!string.IsNullOrEmpty(f.LocalPath) && !File.Exists(f.LocalPath))
                {
                    if (hasPrev && !prev!.TrimEnd().EndsWith(Pending, StringComparison.Ordinal))
                        lines.Add(prev!);
                    continue;
                }

                lines.Add(hasPrev ? prev! : dest + "@" + job.ChannelIndex + Pending);
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

    /// <summary>
    /// EARLY host-manifest send, used when a file in the panel has run out of attempts.
    ///
    /// Content is built the same way the final manifest is: every " -pending" line removed, so the
    /// host receives a valid list of exactly what is on the server — not a half-written file.
    ///
    /// Deliberately leaves no trace in the finalize machinery:
    ///   - no .idxsent / .hostsent marker, so the REAL manifest is still sent when the panel finishes
    ///   - no ".sent" claim, so it never blocks or races a real finalize
    ///   - the caller logs to the oplog only, never the rawlog — a rawlog row would make _history
    ///     dedup skip the real send, and would make the fixture's bottom-up scan read the panel as
    ///     complete while it is still retrying
    ///
    /// Re-send guard: the number of clean lines last sent is kept in "<indexSrc>.midfail". If no more
    /// files have landed since, there is nothing new to tell the host and the send is skipped.
    /// </summary>
    /// <returns>(sent, host, cleanLineCount). sent=false means skipped, not failed.</returns>
    public async Task<(bool Sent, string Host, List<string> Files)> SendMidFailAsync(
        string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath,
        IFtpTransfer ftp, string? forceHost = null)
    {
        var target = forceHost ?? cfg.FirstHost;
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc)) return (false, target, new());
        if (!File.Exists(hostSrc)) return (false, target, new());
        if (string.IsNullOrWhiteSpace(uploadHostPath) && string.IsNullOrWhiteSpace(uploadIndexPath))
            return (false, target, new());

        var clean = ReadRaw(hostSrc)
                    .Where(l => !l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal))
                    .Where(l => l.Trim().Length > 0)
                    .ToList();
        if (clean.Count == 0) return (false, target, new());   // nothing has landed yet

        var names = clean.Select(l => Path.GetFileName((l.Split('@')[0]).Trim())).ToList();

        var stampFile = Path.ChangeExtension(indexSrc, ".midfail");     // "PID.midfail"
        var signature = Sig(clean);
        var already = "";
        try
        {
            if (File.Exists(stampFile))
                foreach (var l in File.ReadAllLines(stampFile))
                {
                    var i = l.IndexOf("sig=", StringComparison.Ordinal);
                    if (i >= 0) already = l[(i + 4)..].Trim();      // keep the LAST one
                }
        }
        catch { }
        if (string.Equals(already, signature, StringComparison.Ordinal))
            return (false, target, new());               // nothing new since the last early send

        // Never REGRESS a manifest that is already on the server in full.
        //
        // The lock stops the early send and a real finalize writing at the same time, but not a
        // stale write landing later. Finalize is per-file: the host manifest can succeed while the
        // index fails, which sets ".hostsent" and leaves the panel incomplete. The panel keeps
        // retrying, another file exhausts its attempts, and the early send would then overwrite that
        // COMPLETE host manifest with a partial one. So a manifest whose marker exists is skipped —
        // the server only ever moves from less complete to more complete, never back.
        var idxDone = File.Exists(indexSrc + ".idxsent");
        var hostDone = File.Exists(indexSrc + ".hostsent");
        var targets = new List<string>();
        if (!idxDone && !string.IsNullOrWhiteSpace(uploadIndexPath)) targets.Add(uploadIndexPath);
        if (!hostDone && !string.IsNullOrWhiteSpace(uploadHostPath)) targets.Add(uploadHostPath);
        if (targets.Count == 0) return (false, target, new());   // both already sent in full

        // Claim the SAME lock the real finalize uses, so the two can never write these remote paths
        // at once. Held by a finalize -> skip: it is about to send the complete manifests.
        var lockFile = indexSrc + ".sent";
        var claimed = false;
        SafeFile.WithLock(() =>
        {
            if (File.Exists(lockFile))
            {
                try { if ((DateTime.Now - File.GetLastWriteTime(lockFile)).TotalMinutes < LockStaleMinutes) return; }
                catch { return; }
            }
            try { File.WriteAllText(lockFile, DateTime.Now.ToString("o")); claimed = true; } catch { }
        });
        if (!claimed) return (false, target, new());

        // Upload a filtered COPY: the real manifests keep their " -pending" lines for the final send.
        // The copy is LOCAL only - the remote name is always the real manifest path.
        var tmp = Path.ChangeExtension(indexSrc, ".midfail.tmp");
        try
        {
            SafeFile.WithLock(() => WriteRaw(tmp, clean));

            var sentAny = false;
            foreach (var remote in targets)   // index first, host last (the order targets was built)
            {
                var jf = new JobFile
                {
                    Pid = "", FileName = Path.GetFileName(remote),
                    LocalPath = tmp, RemotePath = remote
                };
                var r = await ftp.UploadToHostAsync(jf, target, CancellationToken.None);
                if (r.Outcome == TransferOutcome.Success) sentAny = true;
            }
            if (!sentAny) return (false, target, new());

            var block = new System.Text.StringBuilder();
            block.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {names.Count} file(s)  -> {target}  sig={signature}");
            foreach (var n in names) block.AppendLine("    " + n);
            block.AppendLine();
            SafeFile.WithLock(() =>
            {
                try { File.AppendAllText(stampFile, block.ToString()); }
                catch (Exception mex) { MarkerWriteFailed(stampFile, mex); }
            });
            return (true, target, names);
        }
        finally
        {
            SafeFile.WithLock(() =>
            {
                try { File.Delete(tmp); } catch { }
                try { File.Delete(lockFile); } catch { }   // always release
            });
        }
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

    /// <summary>
    /// Raised when something that is normally invisible goes wrong — currently a failed marker
    /// write. Subscribed to the oplog.
    ///
    /// The marker writes used to be `try { ... } catch { }`. A panel then turned up with its host
    /// manifest recorded SUCCEEDED and confirmed on the server by the session log, but no
    /// ".hostsent" file — and because the failure was swallowed there was nothing to explain it.
    /// A silent catch on a bookkeeping write is exactly the kind of thing that costs an afternoon.
    /// </summary>
    public event Action<string>? Warned;

    /// <summary>True if this manifest still lists a file that has not been uploaded.</summary>
    private static bool HasPending(string path)
    {
        var lines = ReadRaw(path);
        return lines.Count == 0 || lines.Any(l => l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal));
    }

    /// <summary>Short, stable signature of the clean manifest lines — order-independent so a rewrite
    /// that only reorders does not look like new content.</summary>
    private static string Sig(IEnumerable<string> lines)
    {
        var joined = string.Join("\n", lines.OrderBy(l => l, StringComparer.Ordinal));
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(bytes)[..8];
    }
    private void MarkerWriteFailed(string path, Exception ex)
        => Warned?.Invoke($"MARKER WRITE FAILED {Path.GetFileName(path)}: {ex.GetType().Name}: {ex.Message}");

    /// <summary>
    /// True once BOTH per-file markers exist — the current meaning of "this panel's manifests are on
    /// the server". It used to test the ".sent" file, which is now only a transient lock deleted in a
    /// finally, so that test had become almost always false.
    /// </summary>
    public bool IsSent(Job job)
    {
        if (!job.IsPanelJob || string.IsNullOrEmpty(job.IndexSrc)) return false;
        try { return File.Exists(job.IndexSrc + ".idxsent") && File.Exists(job.IndexSrc + ".hostsent"); }
        catch { return false; }
    }

    /// <summary>True once BOTH manifests exist and neither still carries a " -pending" line.</summary>
    public bool AllResolved(Job job)
    {
        if (!job.IsPanelJob) return false;
        var resolved = false;
        SafeFile.WithLock(() =>
        {
            // Both manifests, for the same reason as the finalize gate: they are stripped by two
            // separate writes and can disagree.
            if (!File.Exists(job.HostSrc) || !File.Exists(job.IndexSrc)) return;
            resolved = !HasPending(job.HostSrc) && !HasPending(job.IndexSrc);
        });
        return resolved;
    }

    /// <summary>
    /// Job overload: finalize a live panel. Sets job.Finalized on success (fast in-memory skip).
    /// </summary>
    public async Task<FinalizeResult> TryFinalizeAsync(Job job, IFtpTransfer ftp)
    {
        if (job.Finalized || !job.IsPanelJob) return new FinalizeResult(false, false, cfg.FirstHost);
        var sent = await TryFinalizeAsync(job.IndexSrc, job.HostSrc, job.UploadIndexPath, job.UploadHostPath, ftp);
        if (sent.Ok) job.Finalized = true;
        return sent;
    }

    /// <summary>
    /// Path-based finalize, usable by BOTH the live engine and the NG pump (the latter for past-day
    /// panels that have no live Job). Sends index -> UploadIndexPath and host -> UploadHostPath only
    /// when the panel is fully resolved (no " -pending" left) and not already sent.
    ///
    /// A pair of durable per-file markers beside the index manifest records what has actually landed:
    /// ".idxsent" and ".hostsent". A retry therefore sends only the missing one, and "panel complete"
    /// is simply "both markers exist". A separate ".sent" file is a short-lived LOCK so the live
    /// engine, NG's post-recovery finalize and the idle sweep can never send the same panel at once;
    /// it is released in a finally and is not a record of success.
    /// </summary>
    /// <summary>How long a .sent lock is trusted before it is treated as abandoned. It is deleted in
    /// a finally, so it only outlives a call if the process died mid-send; without a timeout that
    /// panel could never be finalized again.</summary>
    private const int LockStaleMinutes = 10;

    public async Task<FinalizeResult> TryFinalizeAsync(string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath, IFtpTransfer ftp, string? forceHost = null)
    {
        var target = forceHost ?? cfg.FirstHost;
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc))
            return new FinalizeResult(false, false, target);

        // Durable, per-file "this one is on the server" markers. Symmetric, so a retry sends only
        // what is actually missing. "Panel complete" is simply: both exist.
        var idxSent = indexSrc + ".idxsent";
        var hostSent = indexSrc + ".hostsent";
        // Transient lock, purely to stop the live engine, NG's post-recovery finalize and the sweep
        // from sending the same panel at once. Released in a finally — it is NOT a record of success.
        var lockFile = indexSrc + ".sent";

        if (File.Exists(idxSent) && File.Exists(hostSent))
            return new FinalizeResult(true, true, target, Uploaded: false);   // already done; sent nothing now

        var claimed = false;
        SafeFile.WithLock(() =>
        {
            if (File.Exists(lockFile))
            {
                // Fresh lock = another caller is mid-send. Stale = a crashed attempt; take it over.
                try
                {
                    if ((DateTime.Now - File.GetLastWriteTime(lockFile)).TotalMinutes < LockStaleMinutes) return;
                }
                catch { return; }
            }
            if (!File.Exists(hostSrc) || !File.Exists(indexSrc)) return;
            // BOTH manifests must be resolved, not just the host.
            //
            // MarkUploaded strips " -pending" from the index and the host as two separate writes, so
            // they can diverge: the host reads clean while the index still carries pending lines.
            // Checking only the host shipped an index manifest listing six files that were not on
            // the server (measured: an 810-byte index with six " -pending" suffixes, later rewritten
            // to 756 clean bytes). The index is what the fixture uses to FIND the data, so a pending
            // line there points at a file that does not exist.
            if (HasPending(hostSrc) || HasPending(indexSrc)) return;
            try { File.WriteAllText(lockFile, DateTime.Now.ToString("o")); claimed = true; } catch { }
        });
        if (!claimed) return new FinalizeResult(false, false, target);

        try
        {
            // Send only what has not landed yet.
            var idxOk = File.Exists(idxSent);
            var landedOn = target;
            // Did THIS call actually put something on the server? The caller logs a manifest send
            // only when it did — otherwise every no-op call adds another pair of log rows.
            var uploaded = false;
            if (!idxOk)
            {
                var r = await SendAsync(ftp, indexSrc, uploadIndexPath, forceHost);
                idxOk = r.Ok; landedOn = r.Host; if (r.Ok) uploaded = true;
                if (idxOk) SafeFile.WithLock(() => { try { File.WriteAllText(idxSent, DateTime.Now.ToString("o")); } catch (Exception mex) { MarkerWriteFailed(idxSent, mex); } });
            }

            var hostOk = File.Exists(hostSent);
            if (!hostOk)
            {
                var r = await SendAsync(ftp, hostSrc, uploadHostPath, forceHost);
                hostOk = r.Ok; landedOn = r.Host; if (r.Ok) uploaded = true;
                if (hostOk) SafeFile.WithLock(() => { try { File.WriteAllText(hostSent, DateTime.Now.ToString("o")); } catch (Exception mex) { MarkerWriteFailed(hostSent, mex); } });
            }

            return new FinalizeResult(idxOk, hostOk, landedOn, uploaded);
        }
        finally
        {
            // Always release. A lock that survives a crash is bounded by LockStaleMinutes instead.
            SafeFile.WithLock(() => { try { File.Delete(lockFile); } catch { } });
        }
    }

    /// <summary>
    /// Send one manifest.
    ///
    /// <paramref name="forceHost"/> is the NG console's chosen IP. The NG list is INDEPENDENT of the
    /// upload routing settings: anything in it goes to the IP picked in the dropdown, so a manifest
    /// sent as part of an NG recovery must go there too — not via HostForAttempt, which would let a
    /// panel pinned to one IP push its index/host files to the other.
    ///
    /// With no override (the live pump) it walks the SAME attempt sequence as the data pump via
    /// <see cref="Config.HostForAttempt"/>: attempt 1 to the initial host, then the primary's
    /// retries, then the secondary's. A host with 0 retries is never in the sequence.
    ///
    /// Returns the host the file actually landed on, so the caller logs where it really went.
    /// </summary>
    private async Task<(bool Ok, string Host)> SendAsync(IFtpTransfer ftp, string localPath, string remotePath, string? forceHost)
    {
        var jf = new JobFile
        {
            Pid = "", FileName = Path.GetFileName(localPath),
            LocalPath = localPath, RemotePath = remotePath
        };

        if (!string.IsNullOrWhiteSpace(forceHost))
        {
            var one = await ftp.UploadToHostAsync(jf, forceHost!, CancellationToken.None);
            return (one.Outcome == TransferOutcome.Success, forceHost!);
        }

        var last = cfg.HostForAttempt(1);
        for (var attempt = 1; attempt <= Math.Max(1, cfg.MaxAttempts); attempt++)
        {
            last = cfg.HostForAttempt(attempt);
            var r = await ftp.UploadToHostAsync(jf, last, CancellationToken.None);
            if (r.Outcome == TransferOutcome.Success) return (true, last);
        }
        return (false, last);
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
