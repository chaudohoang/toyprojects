namespace FtpUpload;

/// <summary>
/// The transfer-engine contract shared by both pumps (live + NG). Implementations wrap a specific
/// FTP library but expose the SAME behaviour the engines rely on: a hard per-file timeout, temp-name
/// then-rename, per-attempt host failover, and a reusable session with a visible number + file count.
/// This lets the app switch engines (FluentFTP / WinSCP) without touching the pumps or their logging.
/// </summary>
public interface IFtpTransfer
{
    /// <summary>Upload for the LIVE pump: the attempt number selects primary vs secondary host.</summary>
    Task<TransferResult> UploadAsync(JobFile file, int attempt, CancellationToken preemptToken);

    /// <summary>Which host a given attempt number targets (primary first, then secondary failover).</summary>
    string HostForAttempt(int attempt);

    /// <summary>Upload to a specific host — used by the NG-retry pump, which chooses the IP.</summary>
    Task<TransferResult> UploadToHostAsync(JobFile file, string host, CancellationToken preemptToken);

    /// <summary>Close the current connection if any (idle/pause), so no idle session is held open.</summary>
    Task EndSession();

    /// <summary>The current open session's number (increments each time a fresh connection opens).</summary>
    int SessionNumber { get; }

    /// <summary>Files successfully sent on the CURRENT open connection.</summary>
    int FilesThisSession { get; }
}

/// <summary>
/// Builds the transfer engine chosen in config (<see cref="Config.Engine"/>). "WinSCP" is used when
/// WinSCP.exe is present next to the app; otherwise (or for any other value) it falls back to
/// FluentFTP. <see cref="ActiveEngine"/> records what was actually chosen, for logging/UI.
/// </summary>
public static class FtpEngineFactory
{
    /// <summary>The engine actually in use (set by the last Create call). For the log / status strip.</summary>
    public static string ActiveEngine { get; private set; } = "FluentFTP";

    public static IFtpTransfer Create(Config cfg, bool reuseConnections)
    {
        if (cfg.Engine.Equals("WinSCP", StringComparison.OrdinalIgnoreCase))
        {
            if (WinScpTransfer.IsAvailable())
            {
                ActiveEngine = "WinSCP";
                return new WinScpTransfer(cfg, reuseConnections);
            }
            // Asked for WinSCP but WinSCP.exe isn't next to the app — degrade gracefully.
            ActiveEngine = "FluentFTP (WinSCP.exe missing \u2014 fell back)";
            return new FluentFtpTransfer(cfg, reuseConnections);
        }

        ActiveEngine = "FluentFTP";
        return new FluentFtpTransfer(cfg, reuseConnections);
    }
}
