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
/// SESSION POLICY: open ONE session and send every file through it. It is reconnected ONLY when it
/// is actually disconnected (Session.Opened == false), on a host change (failover), or when the
/// per-session file cap is hit (0 = unlimited). A timed-out / preempted transfer does NOT drop the
/// session — Session.Abort() cancels just that transfer, and WinSCP reconnects internally if the
/// connection itself is lost.
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

    public string HostForAttempt(int attempt) =>
        attempt <= _cfg.PrimaryAttempts ? _cfg.FirstHost : _cfg.FailoverHost;

    public Task<TransferResult> UploadAsync(JobFile file, int attempt, CancellationToken preemptToken)
        => UploadCore(file, HostForAttempt(attempt), preemptToken);

    public Task<TransferResult> UploadToHostAsync(JobFile file, string host, CancellationToken preemptToken)
        => UploadCore(file, host, preemptToken);

    public Task EndSession() { CloseSession(); return Task.CompletedTask; }

    private void CloseSession()
    {
        var s = _session;
        _session = null; _sessionHost = null; _filesThisSession = 0;
        if (s is null) return;
        try { s.Dispose(); } catch { /* best effort */ }
    }

    private Session EnsureSession(string host)
    {
        var cap = _cfg.MaxFilesPerSession;                        // 0 = unlimited
        var underCap = cap <= 0 || _filesThisSession < cap;
        if (_reuse && _session is not null && _session.Opened && _sessionHost == host && underCap)
            return _session;                                      // reuse the live session

        CloseSession();                                           // host changed / cap / lost / non-reuse

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
                Directory.CreateDirectory(_cfg.LogFullPath);
                s.SessionLogPath = Path.Combine(_cfg.LogFullPath,
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
        // Session.Abort() is explicitly safe to call from another thread.
        using var reg = linked.Token.Register(() => { try { live?.Abort(); } catch { } });

        try
        {
            return await Task.Run(() =>
            {
                var s = EnsureSession(host);
                live = s;

                // WinSCP won't create the upload's target directory — ensure it exists first.
                var parent = RemoteParent(file.RemotePath);
                if (parent.Length > 0 && !s.FileExists(parent))
                {
                    try { s.CreateDirectory(parent); }   // creates superior directories too
                    catch { /* created concurrently by another panel, or exists — ignore */ }
                }

                var to = new TransferOptions { OverwriteMode = OverwriteMode.Overwrite };
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

                _filesThisSession++;
                if (!_reuse) CloseSession();   // non-reuse = one connection per file
                return new TransferResult(TransferOutcome.Success);
            }, linked.Token);
        }
        catch (OperationCanceledException)
        {
            TryCleanupTemp(tempRemote);
            // Keep the ONE session: WinSCP.Abort() cancels only THIS transfer and leaves the session
            // usable, so we do NOT reconnect here. A reconnect happens only when the session is
            // actually disconnected (EnsureSession's Opened check) or on host change / file cap.
            return preemptToken.IsCancellationRequested
                ? new TransferResult(TransferOutcome.Preempted)
                : new TransferResult(TransferOutcome.Timeout, $"exceeded {_cfg.TimeoutSeconds}s on {host}");
        }
        catch (Exception ex)
        {
            // A Session.Abort() (timeout / preempt) surfaces here too — classify by which token tripped.
            if (linked.IsCancellationRequested)
            {
                TryCleanupTemp(tempRemote);
                return preemptToken.IsCancellationRequested
                    ? new TransferResult(TransferOutcome.Preempted)
                    : new TransferResult(TransferOutcome.Timeout, $"exceeded {_cfg.TimeoutSeconds}s on {host}");
            }
            TryCleanupTemp(tempRemote);
            // Soft / remote error — keep the session open. WinSCP reconnects itself if the connection
            // actually dropped; EnsureSession renews the session only when Session.Opened goes false.
            return new TransferResult(TransferOutcome.Error, $"{host}: {ex.Message}");
        }
    }

    private void TryCleanupTemp(string tempRemote)
    {
        var s = _session;
        if (s is null || !s.Opened) return;
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
