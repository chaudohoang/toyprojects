using System.Text.Json;
using System.Text.Json.Serialization;

namespace FtpUpload;

/// <summary>
/// All settings that used to live in TrueTest's DI options now live here
/// (spec §1 "Integrated Configuration UI"). Loaded from config.json next to the exe.
/// </summary>
public sealed class Config
{
    // ---- CNS connection (dual IP, spec §2) ----
    public string PrimaryHost { get; set; } = "192.168.0.10";
    public string SecondaryHost { get; set; } = "192.168.0.11";
    /// <summary>Which host the INITIAL upload attempt targets: "Primary" (default) or "Secondary".
    /// Failover then goes to the other host. Set to "Secondary" on some machines to split load across
    /// the two servers. The retry budgets (PrimaryRetries = tries on the initial host,
    /// SecondaryRetries = tries on the failover host) stay the same; only which IP is "first" changes.</summary>
    public string InitialHost { get; set; } = "Primary";
    public int Port { get; set; } = 21;
    public string User { get; set; } = "user";
    public string Password { get; set; } = "";
    /// <summary>None | Explicit | Implicit — FTPS mode.</summary>
    public string FtpSecure { get; set; } = "None";
    public string RemoteBaseFolder { get; set; } = "/upload/LGD";

    /// <summary>Which transfer engine to use: "FluentFTP" or "WinSCP". Unknown/empty = FluentFTP.
    /// WinSCP requires winscp.exe + WinSCPnet.dll next to the exe; if they're missing at runtime the
    /// app falls back to FluentFTP and logs it.</summary>
    public string Engine { get; set; } = "WinSCP";
    /// <summary>FTP transfer mode for the WinSCP engine: "Passive" (default) or "Active".</summary>
    public string FtpMode { get; set; } = "Passive";
    /// <summary>When true (default), each file is uploaded under a temporary "{name}.part" name and
    /// renamed to the final name only after the bytes fully land — so a half-uploaded file never
    /// appears under its real name. When false, the file is uploaded DIRECTLY to its final name (no
    /// .part, no rename, no existence-check round-trips). Set false when the server/downstream must
    /// never see ".part" files, or when an interrupted transfer stranding a ".part" is a problem —
    /// the panel's index/host manifest is sent last and gates downstream, so a partial data file is
    /// overwritten on retry before the panel is considered complete.</summary>
    public bool UseTempFile { get; set; } = true;
    /// <summary>WinSCP only. When true, an uploaded file keeps the LOCAL file's modified time — but
    /// the server shows it in the SERVER's timezone, so a file made late on one day can display as the
    /// next day on a server in a timezone ahead (the "date 26 vs 25" problem). Default false = let the
    /// server stamp each file with the actual upload time (its own clock), which is predictable.</summary>
    public bool PreserveTimestamp { get; set; } = false;
    /// <summary>When true, the active engine writes its own session log — the full FTP conversation
    /// (commands + server responses) — to the log folder, one file per connection:
    /// WinSCP -> {yyyyMMdd}_winscp_{HHmmss}.log, FluentFTP -> {yyyyMMdd}_fluentftp_{HHmmss}.log.
    /// Useful for diagnosing "uploaded but not right" issues; set false to turn it off.
    /// (Name kept as WinScpLog for config compatibility; it governs both engines.)</summary>
    public bool WinScpLog { get; set; } = true;

    // ---- Timing (spec §2) ----
    /// <summary>Per-file FTP operation timeout in seconds (connect + transfer). Set directly; a
    /// floor of 5 s is enforced. (Formerly derived from a "total tact" budget — now explicit.)</summary>
    public int TimeoutSecondsOverride { get; set; } = 20;

    /// <summary>
    /// Retry policy per IP (spec §2, dual-IP failover). Change these to re-shape failover
    /// however you like — they are the knobs you tune.
    ///   • the first attempt always uses the PRIMARY IP
    ///   • PrimaryRetries   more attempts stay on the primary
    ///   • then it fails over to the SECONDARY IP for SecondaryRetries attempts
    /// Default 2 + 2 = initial + 2 primary retries, then 2 secondary retries (5 attempts total).
    /// Set SecondaryRetries = 0 to disable failover; set a retry count to 0 to skip that stage.
    /// </summary>
    public int PrimaryRetries { get; set; } = 2;
    public int SecondaryRetries { get; set; } = 2;

    /// <summary>
    /// How many files a single reused FTP connection ("session") handles before it is closed and
    /// a fresh one is opened. 0 = unlimited (reuse one connection until it fails or the target host
    /// changes). Presets in the UI: Unlimited, 100, 300, 500. Reconnection also happens on a dead
    /// connection or a primary→secondary failover regardless of this cap.
    /// </summary>
    public int MaxFilesPerSession { get; set; } = 0;

    /// <summary>
    /// Total attempts per file, counting the initial one = 1 + PrimaryRetries + SecondaryRetries.
    /// Derived — set PrimaryRetries / SecondaryRetries instead. (Retries shown in the UI are
    /// MaxAttempts - 1.)
    /// </summary>
    [JsonIgnore] public int MaxAttempts => 1 + Math.Max(0, PrimaryRetries) + Math.Max(0, SecondaryRetries);

    /// <summary>
    /// How many attempts use the PRIMARY IP before failover = 1 (initial) + PrimaryRetries.
    /// Attempts beyond this use the secondary. Derived from PrimaryRetries.
    /// </summary>
    [JsonIgnore] public int PrimaryAttempts => 1 + Math.Max(0, PrimaryRetries);

    /// <summary>The host the INITIAL attempt uses (primary unless InitialHost="Secondary").</summary>
    [JsonIgnore] public string FirstHost =>
        InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase) ? SecondaryHost : PrimaryHost;
    /// <summary>The host failover switches to after the initial host's attempts are exhausted.</summary>
    [JsonIgnore] public string FailoverHost =>
        InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase) ? PrimaryHost : SecondaryHost;

    /// <summary>Per-file operation timeout, with a 5 s floor.</summary>
    [JsonIgnore]
    public int TimeoutSeconds => Math.Max(5, TimeoutSecondsOverride);

    // ---- Folders ----
    // Defaults are RELATIVE and resolve against the exe folder (see ResolveDir / *FullPath), so a
    // freshly-copied publish folder is self-contained and portable. Absolute paths are used as-is.
    public string JobsFolder { get; set; } = "jobs";
    public string LogFolder { get; set; } = "logs";
    public string StateFolder { get; set; } = "state";

    /// <summary>
    /// Resolve a folder value to an absolute path: absolute stays as-is; a relative one (".", "..",
    /// "jobs", @"data\logs", etc.) resolves against the EXE folder, not the process working
    /// directory (which varies by how the app was launched). Empty stays empty (optional folders).
    /// </summary>
    public static string ResolveDir(string path) =>
        string.IsNullOrWhiteSpace(path) ? ""
        : Path.IsPathRooted(path) ? path
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));

    [JsonIgnore] public string JobsFullPath => ResolveDir(JobsFolder);
    [JsonIgnore] public string LogFullPath => ResolveDir(LogFolder);
    [JsonIgnore] public string StateFullPath => ResolveDir(StateFolder);
    [JsonIgnore] public string QueueFullPath => ResolveDir(QueueFolder);

    // ---- .panel intake (the new TrueTest handoff) ----
    /// <summary>
    /// Where TrueTest drops "{PID}_{DateTime}.panel" handoff files (and where the old WinSCP
    /// queue lived). This program watches it for *.panel and derives everything else.
    /// </summary>
    public string QueueFolder { get; set; } = @"D:\Program\RVS\UploadQueue";

    /// <summary>
    /// Pattern filter listing which filenames in a panel's source folder are uploadable
    /// (one pattern per line, '#' comments, '@PID@' = the panel's local PID, '*' wildcard).
    /// folder ∩ recipe = the files to upload AND the total file count. Editable in Settings.
    /// Default: allowed_filenames.txt beside the exe.
    /// </summary>
    public string RecipePath { get; set; } = "allowed_filenames.txt";

    /// <summary>
    /// RecipePath resolved to an absolute path: an absolute value is used as-is; a relative one is
    /// resolved against the EXE folder (not the process working directory, which can be anything
    /// depending on how the app was launched). This is what the app should actually read.
    /// </summary>
    [JsonIgnore]
    public string RecipeFullPath =>
        Path.IsPathRooted(RecipePath) ? RecipePath : Path.Combine(AppContext.BaseDirectory, RecipePath);

    /// <summary>
    /// Where successfully ingested .panel files are kept as a backup, filed under a per-day
    /// subfolder ({PanelBackupFolder}\yyyyMMdd\). Empty = use the default, a "Backup Jobs" folder
    /// inside QueueFolder (see <see cref="PanelBackupFullPath"/>). Old day-subfolders are pruned
    /// by the log-retention setting.
    /// </summary>
    public string PanelBackupFolder { get; set; } = "";

    /// <summary>
    /// The backup folder actually used: PanelBackupFolder if set, otherwise a "Backup Jobs"
    /// subfolder alongside the incoming panels in QueueFolder. So by default backups sit right
    /// next to where the .panel files arrive, with no configuration required.
    /// </summary>
    [JsonIgnore]
    public string PanelBackupFullPath =>
        string.IsNullOrWhiteSpace(PanelBackupFolder)
            ? Path.Combine(QueueFullPath, "Backup Jobs")
            : ResolveDir(PanelBackupFolder);

    // ---- Startup behaviour ----
    /// <summary>If true (default), the live upload pump runs on launch; if false it starts paused
    /// and the operator presses "Auto Upload" to begin.</summary>
    public bool AutoStartUploading { get; set; } = true;

    /// <summary>If true (default), the NG-retry pump auto-retries on launch; if false it stays idle
    /// until the operator presses "Auto Retry".</summary>
    public bool AutoStartRetrying { get; set; } = true;

    /// <summary>
    /// Testing only: artificial per-attempt delay in milliseconds, to mimic real transfer time so
    /// the demo isn't instant (elapsed counters tick, panel timeouts trigger under load, the NG
    /// highlight moves at a readable pace). 0 = no delay (production). Has no effect once the
    /// simulator is off; it just makes simulated runs representative.
    /// </summary>
    public int SimulateUploadMs { get; set; } = 0;

    /// <summary>
    /// Testing only: if &gt; 0, the app advances a SIMULATED calendar day this many real seconds,
    /// so the day-rollover (abandon old day's pending files to NG, reset for the new day) can be
    /// exercised without waiting for real midnight. 0 = use the real clock (production).
    /// </summary>
    public int SimulateFastDaySeconds { get; set; } = 0;

    /// <summary>Poll interval for the command file / new work, in ms.</summary>
    public int PollIntervalMs { get; set; } = 500;

    /// <summary>
    /// Per-panel deadline in seconds. If a panel still has unfinished files this long after it
    /// was first received, the remaining files (and any in-flight file of that panel) are
    /// skipped and marked TimedOut — they go to the NG list for manual retry. 0 disables the
    /// panel timeout entirely. Example: 120.
    /// </summary>
    public int PanelTimeoutSeconds { get; set; } = 0;

    /// <summary>Delay between attempts in the NG-retry pump (unlimited retries), in seconds.</summary>
    public int NgRetryCooldownSeconds { get; set; } = 5;

    /// <summary>
    /// How many days of date-stamped log/report files to keep in the Log and Jobs folders.
    /// Older {yyyyMMdd}_*.txt / _*.html files are auto-deleted on startup and at each day
    /// rollover. 0 (default) keeps everything forever. Only date-stamped files are ever removed,
    /// never today's, never anything else. Example: 90.
    /// </summary>
    public int LogRetentionDays { get; set; } = 0;

    /// <summary>
    /// Build the manager window in the background shortly after startup so that opening
    /// it from the tray is instant. Costs roughly 150 MB of WebView2 processes sitting
    /// idle. Set false on a memory-tight PC — the window then takes a second or two to
    /// appear the first time it is opened.
    /// </summary>
    public bool PrewarmUi { get; set; } = true;

    /// <summary>Seconds to wait after startup before pre-warming, so uploads get going first.</summary>
    public int PrewarmDelaySeconds { get; set; } = 5;

    /// <summary>
    /// TESTING ONLY — percentage chance (0-100) that any single attempt is failed on purpose,
    /// before the file is actually sent. Used to exercise the retry and failover paths against
    /// a healthy server. MUST be 0 in production; leave it 0 unless you are demonstrating.
    /// </summary>
    public int SimulateFailurePercent { get; set; } = 0;

    // Outstanding work now lives in ONE place — the append-only job file:
    //   YYYYMMDD_jobs.txt  - appended by TrueTest, never written by this program
    // "What still needs uploading" is derived from the in-memory job list (any file still
    // PENDING with retries left); there is no second ng_waitlist.txt any more. Logs go to
    // LogFolder, and the command channel stays in StateFolder since it is transient
    // plumbing rather than a record of work.
    [JsonIgnore] public string CommandPath => Path.Combine(StateFullPath, "commands.txt");
    public string RawLogPath(DateTime day) => Path.Combine(LogFullPath, $"{day:yyyyMMdd}_rawlog.txt");
    public string SnapshotPath(DateTime day) => Path.Combine(LogFullPath, $"{day:yyyyMMdd}_snapshot.txt");
    public string JobsPath(DateTime day) => Path.Combine(JobsFullPath, $"{day:yyyyMMdd}_jobs.txt");
    /// <summary>Durable, auditable operation log (manifest sends etc.). Pruned by log retention.</summary>
    public string OpLogPath(DateTime day) => Path.Combine(LogFullPath, $"{day:yyyyMMdd}_oplog.txt");

    // Day-string variants (yyyyMMdd) for the NG-retry console, which browses arbitrary days.
    public string RawLogPathForDay(string day) => Path.Combine(LogFullPath, $"{day}_rawlog.txt");
    public string JobsPathForDay(string day) => Path.Combine(JobsFullPath, $"{day}_jobs.txt");
    // The NG-retry log lives beside the day's other logs, one per original day.
    public string NgRetryLogPath(string day) => Path.Combine(LogFullPath, $"{day}_ngretrylog.txt");

    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static Config Load(string path)
    {
        if (!File.Exists(path))
        {
            var fresh = new Config();
            fresh.Save(path);
            return fresh;
        }
        return JsonSerializer.Deserialize<Config>(File.ReadAllText(path)) ?? new Config();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, Opts));
    }

    public void EnsureFolders()
    {
        Directory.CreateDirectory(JobsFullPath);
        Directory.CreateDirectory(LogFullPath);
        Directory.CreateDirectory(StateFullPath);
        if (!string.IsNullOrWhiteSpace(QueueFullPath))
            Directory.CreateDirectory(QueueFullPath);
        Directory.CreateDirectory(PanelBackupFullPath);
    }
}
