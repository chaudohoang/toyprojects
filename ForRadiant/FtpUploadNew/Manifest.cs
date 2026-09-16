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
            // Nothing to list — every source file is gone. Do NOT write.
            //
            // An empty manifest is meaningless to the host: it says the panel has no files. And the
            // folder itself may be gone too, in which case WriteRaw declines rather than recreating
            // it (see WriteRaw). Seen on disk: a folder holding only two 0-byte manifests.
            if (lines.Count == 0)
            {
                // Only report it if the manifests are not already there; a panel that finished and
                // was then cleaned up is normal housekeeping, not a fault.
                if (!File.Exists(job.IndexSrc) && !File.Exists(job.HostSrc))
                    Warned?.Invoke($"SEED SKIPPED {job.Pid}: every source file is missing " +
                                   $"(folder empty or gone) - not creating empty manifests in {Path.GetDirectoryName(job.IndexSrc)}");
                return;
            }

            if (!WriteRaw(job.IndexSrc, lines) || !WriteRaw(job.HostSrc, lines))
                Warned?.Invoke($"SEED SKIPPED {job.Pid}: source folder is gone " +
                               $"({Path.GetDirectoryName(job.IndexSrc)}) - manifests not written");
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
    ///   - no ".sending" claim, so it never blocks or races a real finalize
    ///   - the caller logs to the oplog only, never the rawlog — a rawlog row would make _history
    ///     dedup skip the real send, and would make the fixture's bottom-up scan read the panel as
    ///     complete while it is still retrying
    ///
    /// Re-send guard: the number of clean lines last sent is kept in "<indexSrc>.midfail". If no more
    /// files have landed since, there is nothing new to tell the host and the send is skipped.
    /// </summary>
    /// <returns>(sent, host, cleanLineCount). sent=false means skipped, not failed.</returns>
    /// <summary>Serialised per panel - see PanelGate.</summary>
    public async Task<(bool Sent, string Host, List<string> Files, string Why, string RemoteName, List<ManifestSend> Sends)> SendMidFailAsync(
        string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath,
        IFtpTransfer ftp, string? forceHost = null)
    {
        var gate = PanelGate(indexSrc);
        await gate.WaitAsync();
        try { return await SendMidFailCoreAsync(indexSrc, hostSrc, uploadIndexPath, uploadHostPath, ftp, forceHost); }
        finally { gate.Release(); }
    }

    private async Task<(bool Sent, string Host, List<string> Files, string Why, string RemoteName, List<ManifestSend> Sends)> SendMidFailCoreAsync(
        string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath,
        IFtpTransfer ftp, string? forceHost = null)
    {
        var target = forceHost ?? cfg.FirstHost;
        if (string.IsNullOrEmpty(indexSrc) || string.IsNullOrEmpty(hostSrc)) return (false, target, new(), "no manifest paths on the job", "", new());
        if (!File.Exists(hostSrc)) return (false, target, new(), "host manifest file does not exist", "", new());
        if (string.IsNullOrWhiteSpace(uploadHostPath) && string.IsNullOrWhiteSpace(uploadIndexPath))
            return (false, target, new(), "no remote manifest paths", "", new());

        var clean = ReadRaw(hostSrc)
                    .Where(l => !l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal))
                    .Where(l => l.Trim().Length > 0)
                    .ToList();
        if (clean.Count == 0) return (false, target, new(), "nothing has landed yet", "", new());

        // With stamping on, the HOST copy carries only what it has not already been given.
        //
        // One setting does both: per-upload names and host increments. They belong together — names
        // without increments just repeats the list, and increments without names would overwrite the
        // previous one. The INDEX is always the full checklist: its remote name is fixed, so every
        // send lands on the same file and an increment there would leave the server holding a
        // fragment.
        var hostDelta = cfg.StampManifestNameAtUpload;
        // ONE read of the record gives both facts: what the host has, and what the index last sent.
        var already = Delivered(indexSrc);
        var delivered = hostDelta ? already.HostNames : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hostLines = hostDelta
            ? clean.Where(l => !delivered.Contains(Path.GetFileName((l.Split('@')[0]).Trim()))).ToList()
            : clean;
        var hostNames = hostLines.Select(l => Path.GetFileName((l.Split('@')[0]).Trim())).ToList();

        var names = clean.Select(l => Path.GetFileName((l.Split('@')[0]).Trim())).ToList();

        // Nothing new for the host: skip. With increments there is nothing to send, and an empty
        // host file would replace a real manifest with nothing.
        if (hostDelta && hostNames.Count == 0)
            return (false, target, new(), "no new uploaded files", "", new());

        var stampFile = Path.ChangeExtension(indexSrc, ".midfail");     // "PID.midfail"

        // Re-send guard: only when the host would learn about MORE files than last time.
        //
        // The count, not a content hash. Equal-or-fewer means the host gains nothing, so the send is
        // skipped — which also means the server can never be moved from a longer list to a shorter
        // one. DropLine can legitimately shrink the set when a source file is gone for good; that
        // shrink is recorded in the manifest but deliberately not pushed, since the previous, longer
        // list is still true about what is on the server.
        var lastCount = -1;
        try
        {
            if (File.Exists(stampFile))
                foreach (var l in File.ReadAllLines(stampFile))
                {
                    // Read the HOST line of each block: "    host  <name>  N file(s)  sent".
                    //
                    // This used to match the block HEADER, which carried the count in the old
                    // one-line-per-send format. Reformatting the record per manifest moved the
                    // count onto the manifest lines, so the regex stopped matching, lastCount
                    // stayed -1, and this guard could never fire — the no-stamp path re-sent the
                    // same list on every trigger with nothing to stop it.
                    var m = System.Text.RegularExpressions.Regex.Match(
                                l, @"^\s*host\s+\S+\s+(\d+) files?\b.*\bsent\b");
                    if (m.Success && int.TryParse(m.Groups[1].Value, out var n)) lastCount = n;   // keep the LAST
                }
        }
        catch { }
        if (!hostDelta && clean.Count <= lastCount)
            return (false, target, new(), "no new uploaded files", "", new());

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
        // HOST FIRST, then the index.
        if (!hostDone && !string.IsNullOrWhiteSpace(uploadHostPath)) targets.Add(uploadHostPath);
        if (!idxDone && !string.IsNullOrWhiteSpace(uploadIndexPath)) targets.Add(uploadIndexPath);
        if (targets.Count == 0) return (false, target, new(), "both manifests already sent in full", "", new());

        // Claim the SAME lock the real finalize uses, so the two can never write these remote paths
        // at once. Held by a finalize -> skip: it is about to send the complete manifests.
        var lockFile = indexSrc + ".sending";
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
        if (!claimed) return (false, target, new(), "a finalize holds the lock", "", new());

        // Upload a filtered COPY: the real manifests keep their " -pending" lines for the final send.
        // The copy is LOCAL only - the remote name is always the real manifest path.
        //
        // TWO copies in delta mode. The INDEX is never a delta: its remote name is fixed
        // ("PID.idx", no stamp), so every send overwrites the same file and only the last one
        // survives — a delta index would leave the server holding one increment, and an empty final
        // delta wiped four of them outright in testing. So the index always carries the full list
        // and only the HOST goes delta.
        var tmp = Path.ChangeExtension(indexSrc, ".midfail.tmp");        // full list, for the index
        var tmpHost = Path.ChangeExtension(indexSrc, ".midfail.host.tmp"); // what the host still needs
        try
        {
            SafeFile.WithLock(() => WriteRaw(tmp, clean));
            if (hostDelta) SafeFile.WithLock(() => WriteRaw(tmpHost, hostLines));

            var sentAny = false;
            // The name the HOST manifest actually went out as - it differs from the panel's own
            // when stamping is on, and nothing else records it, so the logs could not say which
            // file on the server a given early send produced.
            var hostRemoteName = "";
            var hostLanded = false;
            // One entry per MANIFEST, so the caller can log the index send and the host send as
            // separate lines naming their own remote file. A single combined line could only name
            // one of them, and said "index+host sent" even when just one had landed.
            var sends = new List<ManifestSend>();
            // The index only needs re-sending when the checklist has actually grown; otherwise it
            // would upload a byte-identical file over itself.
            var idxAlready = already.IndexCount;
            var indexNeeded = clean.Count > idxAlready;
            foreach (var remote in targets)   // host first, then index (the order targets was built)
            {
                var isHost = string.Equals(remote, uploadHostPath, StringComparison.OrdinalIgnoreCase);
                if (!isHost && !indexNeeded) continue;   // the index is already up to date
                // Stamped here too. This path uploads DIRECTLY rather than through SendAsync, so it
                // was silently exempt: every early send kept the panel's own DateTime while finalize
                // used the upload time, which is the opposite of what was asked for and left the two
                // able to collide anyway.
                var remoteName = cfg.StampManifestNameAtUpload ? StampRemoteName(remote) : remote;
                var jf = new JobFile
                {
                    Pid = "", FileName = Path.GetFileName(remoteName),
                    LocalPath = (hostDelta && isHost) ? tmpHost : tmp,
                    RemotePath = remoteName
                };
                var r = await ftp.UploadToHostAsync(jf, target, CancellationToken.None);
                var lineCount = (hostDelta && isHost) ? hostNames.Count : names.Count;
                sends.Add(new ManifestSend(isHost ? "host" : "index", Path.GetFileName(remoteName),
                                           r.Outcome == TransferOutcome.Success, lineCount, target,
                                           (r.Message ?? "").Trim()));
                if (r.Outcome == TransferOutcome.Success)
                {
                    sentAny = true;
                    if (isHost)
                    {
                        hostRemoteName = Path.GetFileName(remoteName); hostLanded = true;
                    }
                }
                // Nothing landed: give the name back so the next send can use it rather than
                // burning the panel's original and forcing a rename it never needed.
                else if (cfg.StampManifestNameAtUpload) ReleaseStamp(remoteName);
            }
            // Say WHICH manifest failed and why, in the transport's own words. "the upload itself
            // failed" told you neither, so a skip reason could not be acted on.
            if (!sentAny)
            {
                var why = string.Join("; ", sends.Select(s =>
                          $"{s.Kind} upload failed" + (s.Error.Length > 0 ? $": {s.Error}" : "")));
                return (false, target, new(), why.Length > 0 ? why : "the upload itself failed", "", sends);
            }

            // Mark that a PARTIAL manifest is on the server. Deliberately a different name from
            // .idxsent/.hostsent: those mean "the complete manifest is up there", and the finalize
            // guard keys on them, so reusing them would stop the real manifest ever being sent.
            // The summary reads these to distinguish "manifest missing" from "an early, incomplete
            // manifest is on the server".
            foreach (var (remote, marker) in new[]
                     {
                         (uploadIndexPath, Path.ChangeExtension(indexSrc, ".idxpartial")),
                         (uploadHostPath,  Path.ChangeExtension(indexSrc, ".hostpartial"))
                     })
            {
                if (string.IsNullOrWhiteSpace(remote) || !targets.Contains(remote)) continue;
                SafeFile.WithLock(() =>
                {
                    try { File.WriteAllText(marker, $"{DateTime.Now:o} {names.Count} file(s)"); }
                    catch (Exception mex) { MarkerWriteFailed(marker, mex); }
                });
            }

            var block = new System.Text.StringBuilder();
            // ONE ENTRY PER MANIFEST, with the count that manifest actually carried.
            //
            // It used to write the panel's whole list under a single header, so a record saying
            // "9 file(s)" sat beside a host file containing 7 — the increment. Now each manifest
            // reports its own name, its own count and its own outcome, and the names listed are
            // the ones that manifest delivered.
            //
            // Clock.Now, matching the logs: this record is read side by side with the panel-events
            // line announcing the same send.
            block.AppendLine($"{Clock.Now:yyyy-MM-dd HH:mm:ss}  -> {target}");
            foreach (var s in sends)
            {
                block.AppendLine($"    {s.Kind,-5} {s.RemoteName}  {s.Files} file{(s.Files == 1 ? "" : "s")}  " +
                                 (s.Ok ? "sent" : "FAILED"));
                if (!s.Ok) continue;
                foreach (var n in (s.Kind == "host" ? hostNames : names)) block.AppendLine("        " + n);
            }
            block.AppendLine();
            SafeFile.WithLock(() =>
            {
                try { File.AppendAllText(stampFile, block.ToString()); }
                catch (Exception mex) { MarkerWriteFailed(stampFile, mex); }
            });
            return (true, target, names, "", hostRemoteName, sends);
        }
        finally
        {
            SafeFile.WithLock(() =>
            {
                try { File.Delete(tmp); } catch { }
                try { File.Delete(tmpHost); } catch { }
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
    /// the server". It used to test the lock file, which is now ".sending" - a transient lock deleted in a
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
    /// is simply "both markers exist". A separate ".sending" file is a short-lived LOCK so the live
    /// engine, NG's post-recovery finalize and the idle sweep can never send the same panel at once;
    /// it is released in a finally and is not a record of success.
    /// </summary>
    /// <summary>How long a .sending lock is trusted before it is treated as abandoned. It is deleted in
    /// a finally, so it only outlives a call if the process died mid-send; without a timeout that
    /// panel could never be finalized again.</summary>
    private const int LockStaleMinutes = 10;

    /// <summary>Serialised per panel - see PanelGate.</summary>
    public async Task<FinalizeResult> TryFinalizeAsync(string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath, IFtpTransfer ftp, string? forceHost = null)
    {
        var gate = PanelGate(indexSrc);
        await gate.WaitAsync();
        try { return await TryFinalizeCoreAsync(indexSrc, hostSrc, uploadIndexPath, uploadHostPath, ftp, forceHost); }
        finally { gate.Release(); }
    }

    private async Task<FinalizeResult> TryFinalizeCoreAsync(string indexSrc, string hostSrc, string uploadIndexPath, string uploadHostPath, IFtpTransfer ftp, string? forceHost = null)
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
        var lockFile = indexSrc + ".sending";

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

            // RE-CHECK the markers now that we hold the lock.
            //
            // The check above the lock is not enough: the live sweep and NG's post-recovery finalize
            // can both pass it in the same instant, and then take the lock one after the other and
            // each send the panel. Measured on SS0001: NG sent both manifests at 16:19:41 and wrote
            // the markers, and the sweep — already past the outer check — sent the index AGAIN at
            // 16:19:42. Same content, so no data harm, but a wasted transfer and a log that reads
            // as though the manifest needed two goes.
            if (File.Exists(idxSent) && File.Exists(hostSent)) return;

            try { File.WriteAllText(lockFile, DateTime.Now.ToString("o")); claimed = true; } catch { }
        });
        if (!claimed) return new FinalizeResult(false, false, target);

        try
        {
            // Send only what has not landed yet.
            var idxOk = File.Exists(idxSent);
            var landedOn = target;
            // The name the host manifest went out as. With stamping on it differs from the panel's
            // own, and nothing else recorded it - so a file on the server could not be traced back
            // to a log line, and the Trace Panel verdict flagged it as unexplained.
            var hostName = "";
            var idxSentNow = false;
            var hostSentNow = false;
            // Did THIS call actually put something on the server? The caller logs a manifest send
            // only when it did — otherwise every no-op call adds another pair of log rows.
            var uploaded = false;

            // With stamping on the host gets only what it has not been given yet; the index always
            // gets the full checklist, because its remote name is fixed and cannot hold versions.
            var hostDelta = cfg.StampManifestNameAtUpload;
            var delivered = hostDelta ? Delivered(indexSrc).HostNames : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // How many files each manifest describes, for the log line.
            var idxFiles = ReadRaw(hostSrc)
                           .Count(l => l.Trim().Length > 0 &&
                                       !l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal));
            var hostToSend = hostSrc;
            var tmpHost = "";
            var hostNames = new List<string>();
            var hostNothingNew = false;
            if (hostDelta)
            {
                var hostLines = ReadRaw(hostSrc)
                                .Where(l => l.Trim().Length > 0)
                                .Where(l => !l.TrimEnd().EndsWith(Pending, StringComparison.Ordinal))
                                .Where(l => !delivered.Contains(Path.GetFileName((l.Split('@')[0]).Trim())))
                                .ToList();
                hostNames = hostLines.Select(l => Path.GetFileName((l.Split('@')[0]).Trim())).ToList();
                if (hostLines.Count == 0) hostNothingNew = true;
                else
                {
                    try
                    {
                        tmpHost = hostSrc + ".host.tmp";
                        File.WriteAllLines(tmpHost, hostLines);
                        hostToSend = tmpHost;
                    }
                    catch { hostToSend = hostSrc; hostNames = new List<string>(); }  // full list rather than risk an empty one
                }
            }

            // HOST FIRST, then the index.
            var hostOk = File.Exists(hostSent);
            if (!hostOk && hostNothingNew)
            {
                // Every file has already been delivered to the host by the earlier sends. Sending an
                // empty file would replace a real manifest with nothing, so mark it done instead.
                hostOk = true;
                SafeFile.WithLock(() => { try { File.WriteAllText(hostSent, DateTime.Now.ToString("o")); } catch (Exception mex) { MarkerWriteFailed(hostSent, mex); } });
            }
            else if (!hostOk)
            {
                var r = await SendAsync(ftp, hostToSend, uploadHostPath, forceHost);
                hostOk = r.Ok; landedOn = r.Host;
                if (r.Ok)
                {
                    uploaded = true; hostSentNow = true; hostName = r.RemoteName;
                    // NOT recording the delivered names here, for the same reason as the index
                    // count below: finalize is the LAST send for this panel, so nothing reads them
                    // afterwards. Recording would leave a file in every panel's folder rather than
                    // only the ones whose early sends actually needed the bookkeeping.
                }
                if (hostOk) SafeFile.WithLock(() => { try { File.WriteAllText(hostSent, DateTime.Now.ToString("o")); } catch (Exception mex) { MarkerWriteFailed(hostSent, mex); } });
            }
            try { if (tmpHost.Length > 0) File.Delete(tmpHost); } catch { }

            if (!idxOk)
            {
                var r = await SendAsync(ftp, indexSrc, uploadIndexPath, forceHost);
                idxOk = r.Ok; landedOn = r.Host;
                // NOT recording the index count here. Once finalize succeeds both markers exist, so
                // no further send happens for this panel and the count is never read again —
                // writing it would leave a file in EVERY panel's folder, including the ones that
                // never failed, for a guard only the early sends use.
                if (r.Ok) { uploaded = true; idxSentNow = true; }
                if (idxOk) SafeFile.WithLock(() => { try { File.WriteAllText(idxSent, DateTime.Now.ToString("o")); } catch (Exception mex) { MarkerWriteFailed(idxSent, mex); } });
            }

            return new FinalizeResult(idxOk, hostOk, landedOn, uploaded, hostName, idxSentNow, hostSentNow,
                                      idxFiles, hostDelta ? hostNames.Count : idxFiles);
        }
        finally
        {
            // Always release. A lock that survives a crash is bounded by LockStaleMinutes instead.
            SafeFile.WithLock(() => { try { File.Delete(lockFile); } catch { } });
        }
    }

    /// <summary>
    /// Last stamp handed out per remote manifest, so two sends in the same second cannot collide.
    /// Keyed by directory + PID prefix, so panels never interfere with one another.
    ///
    /// STATIC, and deliberately so. UploadEngine and NgRetryEngine each hold their own Manifest,
    /// and a panel's early send can come from one while its finalize comes from the other — with a
    /// per-instance dictionary neither sees the other's stamps, and the two collide anyway. Measured
    /// over 200 panels: per-instance left 7 collisions, which is the whole point of the bump. The
    /// remote namespace is one shared thing, so the register of names used has to be shared too.
    /// </summary>
    private static readonly Dictionary<string, DateTime> _lastStamp = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// What <see cref="_lastStamp"/> held before the current send claimed it, so a failed send can
    /// put it BACK rather than clear it.
    ///
    /// Clearing was wrong: a panel whose first send landed under its original name and whose second
    /// send then failed had the key removed, so the third send saw no entry, decided it was the
    /// first, and reused the original name — overwriting the file already on the server. Seen on
    /// BG0001: 5 files sent as ..._121544, then 9 files sent as ..._121544 again, and the 5-file
    /// version was gone.
    /// </summary>
    private static readonly Dictionary<string, DateTime> _prevStamp = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _stampGate = new();

    /// <summary>
    /// Re-stamp a manifest's remote FILENAME with the time this upload starts.
    ///
    /// LGD asked for this: the host manifest is named "&lt;PID&gt;_&lt;yyyyMMddHHmmss&gt;.txt", and they want
    /// that stamp to be the moment of the upload rather than the moment the panel was measured.
    /// The FIRST send keeps the panel's own name, so a panel that never fails looks exactly as it
    /// always has. Only a REPEAT send — an early (mid-fail) one followed by the complete manifest —
    /// is renamed, so both survive instead of the second overwriting the first.
    ///
    /// BUMPED ON COLLISION. The stamp has one-second resolution and mid-fail to finalize is often
    /// quicker than that: measured over 200 panels against a local server, 16 of them produced two
    /// sends in the same second, so the second silently overwrote the first — losing exactly the
    /// early manifest this feature exists to preserve. When a panel has already used a stamp, the
    /// next one is pushed a second later. The name then drifts slightly from the true upload time,
    /// which is the lesser evil: a file that survives with a name a second out beats a file that
    /// quietly replaces its predecessor.
    ///
    /// Only a name ending in _ plus exactly 14 digits is touched, so the index ("&lt;PID&gt;.idx", no
    /// stamp at all) passes through unchanged and nothing else can be caught by accident.
    ///
    /// The LOCAL file keeps its name: markers, the ".midfail" record and every log row key off it,
    /// and renaming it would break all three for no gain.
    /// </summary>
    private string StampRemoteName(string remotePath)
    {
        if (string.IsNullOrWhiteSpace(remotePath)) return remotePath;
        var slash = remotePath.LastIndexOf('/');
        var dir = slash >= 0 ? remotePath[..(slash + 1)] : "";
        var name = slash >= 0 ? remotePath[(slash + 1)..] : remotePath;

        var m = System.Text.RegularExpressions.Regex.Match(name, @"^(.*)_(\d{14})(\.[^.]+)$");
        if (!m.Success) return remotePath;

        var key = dir + m.Groups[1].Value;
        lock (_stampGate)
        {
            // The FIRST send of a panel keeps the name exactly as the panel was measured.
            //
            // A panel that never fails must look precisely as it always has — one host file, named
            // for its measurement time. Re-stamping it would rename every normal upload on the line
            // for the sake of a case that never happened, and LGD reconcile against that name.
            // Only a SECOND or later send needs a name of its own, and that is the duplicate this
            // feature exists to preserve.
            if (!_lastStamp.TryGetValue(key, out var prev))
            {
                _prevStamp.Remove(key);           // nothing to go back to
                _lastStamp[key] = DateTime.TryParseExact(m.Groups[2].Value, "yyyyMMddHHmmss",
                                      null, System.Globalization.DateTimeStyles.None, out var orig)
                                  ? orig : Clock.Now;
                return remotePath;
            }

            // A repeat send: name it for now, pushed a second past the last name this panel used so
            // two sends in the same second cannot overwrite one another.
            var t = Clock.Now;
            t = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);
            if (t <= prev) t = prev.AddSeconds(1);
            _prevStamp[key] = prev;               // so a failure can restore it
            _lastStamp[key] = t;
            return dir + m.Groups[1].Value + "_" + t.ToString("yyyyMMddHHmmss") + m.Groups[3].Value;
        }
    }

    /// <summary>
    /// Give a name back when its upload FAILED, so the next try can reuse it.
    ///
    /// A name is only truly spent once a file lands. Claiming it at generation meant a panel whose
    /// first send failed on every attempt had already burnt its original name: the later finalize
    /// sweep saw the key registered and issued a fresh one, so a panel that never mid-failed came
    /// out renamed anyway. Measured at 12 of 200. Rolling back on failure makes the rule exact —
    /// no mid-fail, no rename, however many attempts the one send needed.
    ///
    /// Only rolls back if the stored value is still the one this send claimed; a later send that has
    /// already moved the key on must not be disturbed.
    /// </summary>
    private void ReleaseStamp(string remotePath)
    {
        if (string.IsNullOrWhiteSpace(remotePath)) return;
        var slash = remotePath.LastIndexOf('/');
        var dir = slash >= 0 ? remotePath[..(slash + 1)] : "";
        var name = slash >= 0 ? remotePath[(slash + 1)..] : remotePath;

        var m = System.Text.RegularExpressions.Regex.Match(name, @"^(.*)_(\d{14})(\.[^.]+)$");
        if (!m.Success) return;
        if (!DateTime.TryParseExact(m.Groups[2].Value, "yyyyMMddHHmmss", null,
                System.Globalization.DateTimeStyles.None, out var mine)) return;

        var key = dir + m.Groups[1].Value;
        lock (_stampGate)
        {
            if (!_lastStamp.TryGetValue(key, out var cur) || cur != mine) return;

            // Put BACK what was there, don't clear. Clearing makes the next send look like the
            // panel's first, which reuses the original name and overwrites a file already on the
            // server. Only when nothing preceded this claim is removing correct.
            if (_prevStamp.TryGetValue(key, out var back))
            {
                _lastStamp[key] = back;
                _prevStamp.Remove(key);
            }
            else _lastStamp.Remove(key);
        }
    }



    /// <summary>
    /// One gate per panel, held for the WHOLE send — checks, uploads, markers and the record.
    ///
    /// The ".sending" file is claimed under a cross-process mutex, but the uploads then run outside
    /// it, so two callers in this process could both get past it: the live engine and NG finalized
    /// MS0120 within 400 ms of each other, each uploading both manifests, and each reading the
    /// stamp register before the other had written to it — so both used the panel's ORIGINAL host
    /// name and the second overwrote the first. With different stamps it would instead have left a
    /// spurious extra file on the server.
    ///
    /// Keyed by the panel, so panels still run concurrently — only one send per panel at a time.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _panelGates
        = new(StringComparer.OrdinalIgnoreCase);

    private static SemaphoreSlim PanelGate(string indexSrc)
        => _panelGates.GetOrAdd(indexSrc ?? "", _ => new SemaphoreSlim(1, 1));

    /// <summary>
    /// What the panel's manifests have already DELIVERED, read from the ".midfail" record.
    ///
    /// The record is the single source of truth. Each block lists one line per manifest, marked
    /// "sent" or "FAILED", with the names beneath - so the host's delivered set is the names under
    /// its successful lines, and the index's last count is the number on its last successful line.
    ///
    /// Two separate files used to hold this (".hostnames" and ".idxcount"). They were added when
    /// the record was one line per send and could not answer either question - which is what lost
    /// 7 of DL0006's 10 files, because delivery was read from a log of SENDS. The record now
    /// reports per manifest, so the extra files were three sources of truth that had to agree, and
    /// a file in every panel's folder. One reader, one record.
    /// </summary>
    private static (HashSet<string> HostNames, int IndexCount) Delivered(string indexSrc)
    {
        var hostNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var idxCount = 0;
        try
        {
            var rec = Path.ChangeExtension(indexSrc, ".midfail");
            if (!File.Exists(rec)) return (hostNames, 0);
            var collecting = false;   // inside a successful HOST block, gathering its names
            foreach (var raw in File.ReadAllLines(rec))
            {
                var l = raw.Trim();
                if (l.Length == 0) { collecting = false; continue; }
                var m = System.Text.RegularExpressions.Regex.Match(
                            l, @"^(host|index)\s+\S+\s+(\d+) files?\b.*\b(sent|FAILED)\b");
                if (m.Success)
                {
                    var ok = m.Groups[3].Value == "sent";
                    if (m.Groups[1].Value == "index")
                    {
                        if (ok && int.TryParse(m.Groups[2].Value, out var n)) idxCount = n;
                        collecting = false;
                    }
                    else collecting = ok;   // only a SUCCESSFUL host send delivered anything
                    continue;
                }
                // An indented name under the block above.
                if (collecting) hostNames.Add(l);
            }
        }
        catch { }
        return (hostNames, idxCount);
    }

    private async Task<(bool Ok, string Host, string RemoteName)> SendAsync(IFtpTransfer ftp, string localPath, string remotePath, string? forceHost)
    {
        // Stamped HERE, the point finalize and the finalize sweep both funnel through. The name is
        // RELEASED again if nothing lands, so a send that fails every attempt does not burn the
        // panel's original name and leave the next try to invent a new one.
        if (cfg.StampManifestNameAtUpload) remotePath = StampRemoteName(remotePath);

        var jf = new JobFile
        {
            Pid = "", FileName = Path.GetFileName(localPath),
            LocalPath = localPath, RemotePath = remotePath
        };

        if (!string.IsNullOrWhiteSpace(forceHost))
        {
            var one = await ftp.UploadToHostAsync(jf, forceHost!, CancellationToken.None);
            var okOne = one.Outcome == TransferOutcome.Success;
            if (!okOne && cfg.StampManifestNameAtUpload) ReleaseStamp(remotePath);
            return (okOne, forceHost!, Path.GetFileName(remotePath));
        }

        var last = cfg.HostForAttempt(1);
        for (var attempt = 1; attempt <= Math.Max(1, cfg.MaxAttempts); attempt++)
        {
            last = cfg.HostForAttempt(attempt);
            var r = await ftp.UploadToHostAsync(jf, last, CancellationToken.None);
            if (r.Outcome == TransferOutcome.Success) return (true, last, Path.GetFileName(remotePath));
        }
        // Nothing landed on any host: hand the name back for the next attempt to reuse.
        if (cfg.StampManifestNameAtUpload) ReleaseStamp(remotePath);
        return (false, last, Path.GetFileName(remotePath));
    }

    // ---- raw manifest file helpers (callers already hold the SafeFile lock) ----

    private static List<string> ReadRaw(string path)
    {
        try { return File.Exists(path) ? File.ReadAllLines(path).Where(l => l.Length > 0).ToList() : new(); }
        catch { return new(); }
    }

    /// <summary>
    /// Write a manifest — but NEVER create the folder it lives in.
    ///
    /// The manifests sit in the panel's own source folder, which TrueTest creates. If that folder
    /// is gone the panel is gone: the files were deleted or cleaned up, and there is nothing left
    /// to upload. This used to call Directory.CreateDirectory, which RESURRECTED the deleted folder
    /// and left two manifests in it — seen on disk as a folder holding only "&lt;PID&gt;.idx" and
    /// "&lt;PID&gt;_&lt;stamp&gt;.txt" at 0 bytes each, with no data files anywhere near them.
    ///
    /// Returns false when the folder is missing, so callers can tell "did not write" from "wrote".
    /// </summary>
    private static bool WriteRaw(string path, IEnumerable<string> lines)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
        File.WriteAllLines(path, lines);
        return true;
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
