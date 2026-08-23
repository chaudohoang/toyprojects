using FluentFTP;

namespace FtpUpload;

public enum TransferOutcome { Success, Timeout, Preempted, Error, LocalMissing }

public sealed record TransferResult(TransferOutcome Outcome, string? Message = null);

/// <summary>
/// Wraps FluentFTP with the two behaviours the spec needs:
///  • a HARD per-file timeout (spec §2) — the transfer is cancelled mid-stream at T,
///    not merely "reported slow";
///  • temp-name-then-rename, so an aborted or timed-out transfer never leaves a
///    partially-written file visible under its real name on CNS.
/// </summary>
public sealed class FtpTransfer(Config cfg, bool reuseConnections = false)
{
    // A persistent connection reused across files when reuseConnections is true. Used serially by
    // a single pump, so no locking is needed. A "session" is one open connection: it is renewed on
    // host change, connection loss, the per-session file cap, or any failed/cancelled transfer.
    private AsyncFtpClient? _client;
    private string? _clientHost;
    private int _filesThisSession;

    /// <summary>Increments each time a fresh connection is opened (the current session's number).</summary>
    public int SessionNumber { get; private set; }
    /// <summary>Files successfully sent on the CURRENT open connection.</summary>
    public int FilesThisSession => _filesThisSession;

    /// <summary>
    /// The first <see cref="Config.PrimaryAttempts"/> attempts go to the primary IP, the rest
    /// fail over to the secondary (spec §2). With the defaults that is: initial attempt plus
    /// 2 retries on the primary, then 2 retries on the secondary.
    /// </summary>
    public string HostForAttempt(int attempt) =>
        attempt <= cfg.PrimaryAttempts ? cfg.PrimaryHost : cfg.SecondaryHost;

    public Task<TransferResult> UploadAsync(JobFile file, int attempt, CancellationToken preemptToken)
        => UploadCore(file, HostForAttempt(attempt), preemptToken);

    /// <summary>Upload to a specific host — used by the NG-retry pump, which chooses the IP.</summary>
    public Task<TransferResult> UploadToHostAsync(JobFile file, string host, CancellationToken preemptToken)
        => UploadCore(file, host, preemptToken);

    /// <summary>Close the current connection if any (call when the pump goes idle or is paused, so
    /// an idle FTP session isn't held open on the server). Safe to call repeatedly.</summary>
    public async Task EndSession()
    {
        if (_client is null) return;
        try { await _client.Disconnect(); } catch { /* best effort */ }
        try { _client.Dispose(); } catch { }
        _client = null; _clientHost = null; _filesThisSession = 0;
    }

    /// <summary>Return a connected client for <paramref name="host"/>, reusing the open one when
    /// possible, otherwise opening a fresh session (closing any previous one first).</summary>
    private async Task<AsyncFtpClient> EnsureClient(string host, CancellationToken ct)
    {
        var cap = cfg.MaxFilesPerSession;               // 0 = unlimited
        var underCap = cap <= 0 || _filesThisSession < cap;
        if (reuseConnections && _client is not null && _client.IsConnected
            && _clientHost == host && underCap)
            return _client;   // reuse the live session

        await EndSession();   // host changed / cap reached / lost / non-reuse → renew

        var c = new AsyncFtpClient(host, cfg.User, cfg.Password, cfg.Port);
        c.Config.EncryptionMode = cfg.FtpSecure.ToLowerInvariant() switch
        {
            "explicit" => FtpEncryptionMode.Explicit,
            "implicit" => FtpEncryptionMode.Implicit,
            _ => FtpEncryptionMode.None
        };
        c.Config.ValidateAnyCertificate = true;   // CNS uses a self-signed cert in most installs
        c.Config.RetryAttempts = 0;               // retries are managed by UploadEngine, not the library

        await c.Connect(ct);
        _client = c; _clientHost = host; _filesThisSession = 0; SessionNumber++;
        return c;
    }

    private async Task<TransferResult> UploadCore(JobFile file, string host, CancellationToken preemptToken)
    {
        var tempRemote = file.RemotePath + ".part";

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(cfg.TimeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, preemptToken);

        try
        {
            if (!File.Exists(file.LocalPath))
                return new TransferResult(TransferOutcome.LocalMissing, "local file missing: " + file.LocalPath);

            // TESTING ONLY — mimic real transfer time so the demo isn't instant: elapsed
            // counters tick, throughput is realistic, and panel timeouts actually trigger under
            // load. Cancellable, so pause / preemption / per-file timeout still interrupt it.
            // 0 (production) = no delay. Applies to both simulated failures and real successes.
            if (cfg.SimulateUploadMs > 0)
                await Task.Delay(cfg.SimulateUploadMs, linked.Token);

            // TESTING ONLY — fault injection, off unless SimulateFailurePercent > 0 in config.
            // Deliberately fails the attempt before anything is sent, so the retry/failover
            // paths can be exercised against a healthy server. Never active in production.
            if (cfg.SimulateFailurePercent > 0 &&
                Random.Shared.Next(100) < cfg.SimulateFailurePercent)
            {
                // Soft failure — the connection is fine, so keep the session open (reconnect only
                // if the connection itself dies). Matches real per-file rejections.
                return new TransferResult(TransferOutcome.Error,
                    $"SIMULATED failure on {host} (SimulateFailurePercent={cfg.SimulateFailurePercent})");
            }

            var client = await EnsureClient(host, linked.Token);

            var status = await client.UploadFile(
                file.LocalPath, tempRemote,
                FtpRemoteExists.Overwrite,
                createRemoteDir: true,
                token: linked.Token);

            if (status != FtpStatus.Success)
            {
                // Server rejected this file but the connection is still good — keep the session.
                return new TransferResult(TransferOutcome.Error, "upload returned " + status);
            }

            // Only now does the file appear under its real name.
            if (await client.FileExists(file.RemotePath, linked.Token))
                await client.DeleteFile(file.RemotePath, linked.Token);
            await client.Rename(tempRemote, file.RemotePath, linked.Token);

            _filesThisSession++;                       // this file rode the current session
            if (!reuseConnections) await EndSession(); // non-reuse = one connection per file (old behaviour)
            return new TransferResult(TransferOutcome.Success);
        }
        catch (OperationCanceledException)
        {
            await TryCleanupTemp(_client, tempRemote);
            await EndSession();   // a cancelled transfer leaves the connection mid-stream — drop it
            return preemptToken.IsCancellationRequested
                ? new TransferResult(TransferOutcome.Preempted)
                : new TransferResult(TransferOutcome.Timeout, $"exceeded {cfg.TimeoutSeconds}s on {host}");
        }
        catch (Exception ex)
        {
            await TryCleanupTemp(_client, tempRemote);
            // Keep the session unless the connection actually died (reconnect-only-if-dead).
            if (_client is null || !_client.IsConnected) await EndSession();
            return new TransferResult(TransferOutcome.Error, $"{host}: {ex.Message}");
        }
    }

    /// <summary>Remove the .part leftover so a cancelled transfer doesn't accumulate junk on CNS.</summary>
    private static async Task TryCleanupTemp(AsyncFtpClient? client, string tempRemote)
    {
        if (client is null || !client.IsConnected) return;
        try
        {
            using var quick = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (await client.FileExists(tempRemote, quick.Token))
                await client.DeleteFile(tempRemote, quick.Token);
        }
        catch { /* best effort — a stale .part is harmless, it is never renamed */ }
    }
}

