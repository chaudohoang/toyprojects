using WinSCP;

namespace FtpUpload;

/// <summary>
/// WinSCP-backed transfer engine (drives WinSCP.exe through WinSCPnet.dll). Behaves like
/// <see cref="FluentFtpTransfer"/> so the pumps don't care which engine runs:
///  • a HARD per-file timeout — the transfer is aborted mid-stream at T (WinSCP is synchronous,
///    so each transfer runs on a worker thread and is cancelled via Session.Abort());
///  • temp-name-then-rename, so an aborted/timed-out transfer never shows under the real name;
///  • per-attempt primary/secondary host failover.
///
/// SESSION POLICY: open ONE session and send every file through it. It is reconnected when it is
/// disconnected (Session.Opened == false), on a host change (failover), when the per-session file
/// cap is hit (0 = unlimited), and — critically — after ANY abort.
///
/// Session.Abort() does NOT cancel a single transfer. It tears down the whole session and kills the
/// underlying WinSCP.exe. Every later call on that Session throws "InvalidOperationException:
/// Session was aborted", and the manifest path additionally throws "Element session@0 already read
/// to the end". Worse, Session.Opened STAYS TRUE after an abort, so an Opened-only check silently
/// reuses the corpse forever. Measured at LGD (302L 2026-09-18 13:08, 502L 2026-09-18 19:55): one
/// timed-out transfer poisoned the session and index/host upload never recovered again — four days,
/// ~1800 panels, zero manifests. Hence _aborted, and CloseSession() on every cancellation path.
/// </summary>
public sealed class WinScpTransfer : IFtpTransfer
{
    private readonly Config _cfg;
    private readonly bool _reuse;
    private readonly string _exePath;

    // One WinSCP session reused across files by a single pump (serial use, no locking needed).
    private Session? _session;
    private string? _sessionHost;
    private int _filesThisSession;

    // Set by the cancellation callback the moment Session.Abort() is called. Session.Opened is NOT
    // a reliable liveness test after an abort (it stays true), so this is the flag EnsureSession
    // actually trusts. Cleared only when a fresh session is opened.
    private volatile bool _aborted;

    // NOTE on capturing the server's own status code (421 / 530 / 550):
    // Tried and reverted 2026-09-21. WinSCP's exception text does NOT carry the code — a refused
    // connection is just "Connection failed." — and neither route to the code works from here:
    //   * Session.OutputDataReceived carries the scripting console output, not the FTP protocol
    //     lines, so the "< 421 ..." reply never reaches it (verified: nothing captured).
    //   * Reading the session log while it is open fails — winscp.exe does not share it for
    //     reading, so the FileStream throws and yields nothing (verified: nothing captured).
    // The code IS in the per-session log under logs\winscp\, which is where to look when a reason
    // says "connection to the server was lost" and you need to know why. Doing it in-process would
    // need WinSCP's XML log parsed after the session closes — worth having, not worth faking.

    // Remote directories this engine has already confirmed/created. Survives a reconnect on purpose
    // — the folder is still on the server after our connection drops. Serial use, no locking.
    private readonly HashSet<string> _ensuredDirs = new(StringComparer.Ordinal);

    public int SessionNumber { get; private set; }
    public int FilesThisSession => _filesThisSession;

    public WinScpTransfer(Config cfg, bool reuseConnections)
    {
        _cfg = cfg;
        _reuse = reuseConnections;
        _exePath = ExecutablePath();
    }

    /// <summary>Path to WinSCP.exe next to the app (WinSCPnet.dll shells out to it).</summary>
    public static string ExecutablePath() => Path.Combine(AppContext.BaseDirectory, "WinSCP.exe");

    /// <summary>True when WinSCP.exe is present, so the factory can fall back to FluentFTP if not.</summary>
    public static bool IsAvailable()
    {
        try { return File.Exists(ExecutablePath()); } catch { return false; }
    }

    /// <summary>Delegates to <see cref="Config.HostForAttempt"/> — one shared routing rule.</summary>
    public string HostForAttempt(int attempt) => _cfg.HostForAttempt(attempt);

    public Task<TransferResult> UploadAsync(JobFile file, int attempt, CancellationToken preemptToken)
        => UploadCore(file, HostForAttempt(attempt), preemptToken);

    public Task<TransferResult> UploadToHostAsync(JobFile file, string host, CancellationToken preemptToken)
        => UploadCore(file, host, preemptToken);

    public Task EndSession() { CloseSession(); return Task.CompletedTask; }

    private void CloseSession()
    {
        var s = _session;
        _session = null; _sessionHost = null; _filesThisSession = 0; _aborted = false;
        if (s is null) return;
        try { s.Dispose(); } catch { /* best effort */ }
    }

    private Session EnsureSession(string host)
    {
        var cap = _cfg.MaxFilesPerSession;                        // 0 = unlimited
        var underCap = cap <= 0 || _filesThisSession < cap;
        // !_aborted comes FIRST: after Session.Abort() the object still reports Opened == true, so
        // the Opened check alone would hand back a dead session for the life of the process.
        if (_reuse && !_aborted && _session is not null && _session.Opened && _sessionHost == host && underCap)
            return _session;                                      // reuse the live session

        CloseSession();                                           // aborted / host changed / cap / lost / non-reuse

        var opts = new SessionOptions
        {
            Protocol   = Protocol.Ftp,
            HostName   = host,
            PortNumber = _cfg.Port,
            UserName   = _cfg.User,
            Password   = _cfg.Password,
            FtpMode    = _cfg.FtpMode.Equals("Active", StringComparison.OrdinalIgnoreCase)
                            ? WinSCP.FtpMode.Active : WinSCP.FtpMode.Passive,
            FtpSecure  = _cfg.FtpSecure.ToLowerInvariant() switch
            {
                "explicit" => WinSCP.FtpSecure.Explicit,
                "implicit" => WinSCP.FtpSecure.Implicit,
                _          => WinSCP.FtpSecure.None
            },
            Timeout    = TimeSpan.FromSeconds(Math.Max(5, _cfg.TimeoutSeconds))
        };
        if (opts.FtpSecure != WinSCP.FtpSecure.None)
            opts.GiveUpSecurityAndAcceptAnyTlsHostCertificate = true;   // CNS self-signed cert

        var s = new Session { ExecutablePath = _exePath };


        // WinSCP's own session log (the full FTP conversation) — one file per connection, in the log
        // folder, like a normal WinSCP setup. Best-effort: never let logging stop an upload.
        if (_cfg.WinScpLog)
        {
            try
            {
                // Session logs go in their OWN folder. A busy day produces hundreds of them — 292
                // on one LGD machine — which buries the handful of files a person actually reads
                // (the day log, the summary, the reports) in the same directory listing.
                var sessionDir = Path.Combine(_cfg.LogFullPath, "winscp");
                Directory.CreateDirectory(sessionDir);
                s.SessionLogPath = Path.Combine(sessionDir,
                    $"{DateTime.Now:yyyyMMdd}_winscp_{DateTime.Now:HHmmssfff}.log");
            }
            catch { /* logging is optional — proceed without it */ }
        }

        s.Open(opts);
        _session = s; _sessionHost = host; _filesThisSession = 0; SessionNumber++;
        return s;
    }

    private async Task<TransferResult> UploadCore(JobFile file, string host, CancellationToken preemptToken)
    {
        if (!File.Exists(file.LocalPath))
            return new TransferResult(TransferOutcome.LocalMissing, "local file missing: " + file.LocalPath);

        // TESTING ONLY — mirror FluentFtpTransfer's simulation hooks (0 in production).
        if (_cfg.SimulateUploadMs > 0)
        {
            try { await Task.Delay(_cfg.SimulateUploadMs, preemptToken); }
            catch (OperationCanceledException) { return new TransferResult(TransferOutcome.Preempted); }
        }
        if (_cfg.SimulateFailurePercent > 0 && Random.Shared.Next(100) < _cfg.SimulateFailurePercent)
            return new TransferResult(TransferOutcome.Error,
                $"SIMULATED failure on {host} (SimulateFailurePercent={_cfg.SimulateFailurePercent})");

        var tempRemote = file.RemotePath + ".part";

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, _cfg.TimeoutSeconds)));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, preemptToken);

        Session? live = null;
        // Hard timeout / preemption: abort the WinSCP session mid-transfer when the token trips.
        // Session.Abort() is explicitly safe to call from another thread. Flag the session dead in
        // the SAME callback: whoever aborted it, it must never be handed out by EnsureSession again.
        //
        // The registration is ONE-SHOT and can fire before the worker has a session to abort — the
        // timer starts here, but `live` is only assigned after EnsureSession returns, and opening a
        // fresh connection to an unresponsive server can itself burn the whole budget. The old code
        // did `live?.Abort()`, so in that window nothing was aborted, nothing was flagged, and the
        // callback never ran again: the file then transferred with NO time limit at all. Record the
        // request instead, and let the worker act on it the moment it has the session.
        var cancelRequested = false;
        using var reg = linked.Token.Register(() =>
        {
            cancelRequested = true;
            var s = live;
            if (s is null) return;          // worker will see cancelRequested and abort itself
            _aborted = true;
            try { s.Abort(); } catch { }
        });

        try
        {
            return await Task.Run(() =>
            {
                var s = EnsureSession(host);
                live = s;

                // Close the race: the deadline may have passed while EnsureSession was opening the
                // connection, in which case the callback above found `live` null and did nothing.
                // Honour it here rather than starting a transfer that can no longer be cancelled.
                if (cancelRequested || linked.Token.IsCancellationRequested)
                {
                    _aborted = true;
                    try { s.Abort(); } catch { }
                    linked.Token.ThrowIfCancellationRequested();
                }

                // WinSCP won't create the upload's target directory — ensure it exists first.
                // Once per DIRECTORY, not once per file. All ~13 files of a panel share one remote
                // folder, so the old per-file probe spent PWD+CWD+SIZE round trips (and produced
                // two 550s) on every single file: 16,285 of the 16,460 server errors in four days
                // of LGD session logs were this check re-asking a question it had already answered.
                var parent = RemoteParent(file.RemotePath);
                if (parent.Length > 0 && !_ensuredDirs.Contains(parent))
                {
                    if (!s.FileExists(parent))
                    {
                        try { s.CreateDirectory(parent); }   // creates superior directories too
                        catch { /* created concurrently by another panel, or exists — ignore */ }
                    }
                    // Only cache once it is known to exist; a throw above leaves it uncached so the
                    // next file re-checks rather than uploading into a directory that isn't there.
                    if (_ensuredDirs.Count > 4000) _ensuredDirs.Clear();   // long-running process
                    _ensuredDirs.Add(parent);
                }

                var to = new TransferOptions { OverwriteMode = OverwriteMode.Overwrite, PreserveTimestamp = _cfg.PreserveTimestamp };
                if (_cfg.UseTempFile)
                {
                    // Upload to a temp name so a failed/aborted transfer never appears under the real
                    // name, then swap into place (remove any stale real file, then rename temp->real).
                    s.PutFiles(file.LocalPath, tempRemote, false, to).Check();
                    if (s.FileExists(file.RemotePath))
                        s.RemoveFiles(RemotePath.EscapeFileMask(file.RemotePath)).Check();
                    s.MoveFile(RemotePath.EscapeFileMask(tempRemote), file.RemotePath);
                }
                else
                {
                    // Direct upload to the final name — no .part, no rename, no existence-check
                    // round-trips. WinSCP overwrites in place. Avoids stranded .part files on abort.
                    s.PutFiles(file.LocalPath, file.RemotePath, false, to).Check();
                }

                // The file is fully on the server under its real name. Retire the abort callback
                // NOW so a timer expiring during the return path cannot destroy a session over a
                // transfer that already succeeded. Dispose() waits out a callback already running
                // and blocks any future one, so this is the point of no return for this file.
                // Real case: LGD 302L 2026-09-18, "226 Transfer complete" at 13:08:48.389, timeout
                // due at 13:08:48.790 — the upload was thrown away with 400 ms to spare, and took
                // the session with it.
                reg.Dispose();

                _filesThisSession++;
                if (!_reuse) CloseSession();   // non-reuse = one connection per file
                return new TransferResult(TransferOutcome.Success);
            }, linked.Token);
        }
        catch (OperationCanceledException)
        {
            // Order matters: try the .part cleanup while the session may still be usable, THEN drop
            // it. An aborted session cannot clean up (every call throws), so don't bother.
            if (!_aborted) TryCleanupTemp(tempRemote);
            CloseSession();   // the abort killed WinSCP.exe — this session is finished, not paused
            return preemptToken.IsCancellationRequested
                ? new TransferResult(TransferOutcome.Preempted)
                : new TransferResult(TransferOutcome.Timeout, $"exceeded {_cfg.TimeoutSeconds}s on {host}");
        }
        catch (Exception ex)
        {
            // A Session.Abort() (timeout / preempt) surfaces here too — classify by which token tripped.
            if (linked.IsCancellationRequested)
            {
                if (!_aborted) TryCleanupTemp(tempRemote);
                CloseSession();   // same as above — an aborted session is dead, drop it
                return preemptToken.IsCancellationRequested
                    ? new TransferResult(TransferOutcome.Preempted)
                    : new TransferResult(TransferOutcome.Timeout, $"exceeded {_cfg.TimeoutSeconds}s on {host}");
            }

            // Not our cancellation. If the session reports itself aborted anyway (an abort raced in
            // from elsewhere, or a previous one poisoned it), it is unusable — drop it rather than
            // hand it to the next file. These are the two messages that appeared ~35,000 times at
            // LGD between 18 and 21 Sep because the session was reused after an abort.
            var poisoned = _aborted
                        || ex is InvalidOperationException
                        || ex.Message.Contains("Session was aborted", StringComparison.OrdinalIgnoreCase)
                        || ex.Message.Contains("already read to the end", StringComparison.OrdinalIgnoreCase);

            if (poisoned) CloseSession();
            else TryCleanupTemp(tempRemote);

            // Soft / remote error on a healthy session — keep it open; WinSCP reconnects itself if
            // the connection actually dropped.
            return new TransferResult(TransferOutcome.Error, $"{host}: {Explain(ex)}");
        }
    }

    /// <summary>
    /// A readable reason for the log's reason column, with WinSCP's own wording kept on the end.
    ///
    /// The raw text is written for someone reading WinSCP's source, not someone reading a day log
    /// at 3am: "Element session@0 already read to the end" means the WinSCP process behind this
    /// session had already exited. Four days of LGD logs carried that string and nobody could act
    /// on it. Plain sentence first, original after it in brackets so nothing is lost for a deeper
    /// investigation.
    /// </summary>
    private static string Explain(Exception ex)
    {
        var m = ex.Message?.Trim() ?? "";
        string? plain = null;

        if (m.Contains("Session was aborted", StringComparison.OrdinalIgnoreCase))
            plain = "connection was cancelled by the upload time limit and is being reopened";
        else if (m.Contains("already read to the end", StringComparison.OrdinalIgnoreCase))
            plain = "connection had already closed (WinSCP process ended) and is being reopened";
        else if (m.Contains("Timeout detected", StringComparison.OrdinalIgnoreCase))
            plain = "no reply from the server";
        else if (m.Contains("Service not available", StringComparison.OrdinalIgnoreCase))
            plain = "server refused the connection (it may be at its connection limit)";
        else if (m.Contains("Connection failed", StringComparison.OrdinalIgnoreCase)
              || m.Contains("Lost connection", StringComparison.OrdinalIgnoreCase))
            plain = "connection to the server was lost";
        else if (m.Contains("cannot find the path", StringComparison.OrdinalIgnoreCase)
              || m.Contains("cannot find the file", StringComparison.OrdinalIgnoreCase))
            plain = "the folder or file does not exist on the server";
        else if (m.Contains("Access is denied", StringComparison.OrdinalIgnoreCase)
              || m.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
            plain = "the server refused it (permission)";

        // Lead the bracket with the server's own status code when there is one. The code is the
        // part an FTP admin can act on - 421 (refused / at its connection limit) and 550 (no such
        // path) mean different things to LGD's side - and it was getting buried mid-sentence in
        // WinSCP's wording, or lost entirely once a plain sentence was put in front of it.
        var code = System.Text.RegularExpressions.Regex.Match(m, @"\b([45]\d\d)\b");
        var detail = code.Success && !m.TrimStart().StartsWith(code.Value, StringComparison.Ordinal)
                   ? $"{code.Value}: {m}"
                   : m;


        return plain is null ? detail : $"{plain} [{detail}]";
    }

    private void TryCleanupTemp(string tempRemote)
    {
        var s = _session;
        if (s is null || _aborted || !s.Opened) return;   // Opened lies after an abort; _aborted doesn't
        try { if (s.FileExists(tempRemote)) s.RemoveFiles(RemotePath.EscapeFileMask(tempRemote)); }
        catch { /* best effort — a stale .part is harmless, it is never renamed */ }
    }

    /// <summary>The remote directory portion of a path (everything before the last '/').</summary>
    private static string RemoteParent(string remotePath)
    {
        var i = remotePath.LastIndexOf('/');
        return i <= 0 ? "" : remotePath.Substring(0, i);
    }
}
