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
    /// FIXED, not configurable: uploads go straight to the final name. [JsonIgnore] keeps it out
    /// of config.json entirely, so no file can suggest a setting that has no effect.
    [JsonIgnore] public bool UseTempFile => false;
    /// <summary>WinSCP only. When true, an uploaded file keeps the LOCAL file's modified time — but
    /// the server shows it in the SERVER's timezone, so a file made late on one day can display as the
    /// next day on a server in a timezone ahead (the "date 26 vs 25" problem). Default false = let the
    /// server stamp each file with the actual upload time (its own clock), which is predictable.</summary>
    /// FIXED, not configurable: the server stamps each file with the actual upload time.
    [JsonIgnore] public bool PreserveTimestamp => false;
    /// <summary>When true, the active engine writes its own session log — the full FTP conversation
    /// (commands + server responses) — to the log folder, one file per connection:
    /// WinSCP -> {yyyyMMdd}_winscp_{HHmmss}.log, FluentFTP -> {yyyyMMdd}_fluentftp_{HHmmss}.log.
    /// Useful for diagnosing "uploaded but not right" issues; set false to turn it off.
    /// (Name kept as WinScpLog for config compatibility; it governs both engines.)</summary>
    /// FIXED, not configurable: the session log is ALWAYS written. It is the only record of the
    /// actual FTP dialogue, and the one artefact that settled "was this really uploaded?" when the
    /// rawlog and the server disagreed. Turning it off costs nothing at upload time and blinds
    /// every later investigation.
    [JsonIgnore] public bool WinScpLog => true;

    // ---- Timing (spec §2) ----
    /// <summary>Per-file FTP operation timeout in seconds (connect + transfer). Set directly; a
    /// floor of 5 s is enforced. (Formerly derived from a "total tact" budget — now explicit.)</summary>
    public int TimeoutSecondsOverride { get; set; } = 20;

    // PrimaryRetries / SecondaryRetries were removed with failover. Configs written before that
    // still contain them, so Load() reads them straight from the JSON to derive RetryCount — see
    // MigrateRetries. They are no longer properties, so they stop being written to new configs.

    /// <summary>
    /// How many files a single reused FTP connection ("session") handles before it is closed and
    /// a fresh one is opened. 0 = unlimited (reuse one connection until it fails or the target host
    /// changes). Presets in the UI: Unlimited, 100, 300, 500. Reconnection also happens on a dead
    /// connection or a primary→secondary failover regardless of this cap.
    /// </summary>
    public int MaxFilesPerSession { get; set; } = 0;

    /// <summary>
    /// Retries after the initial attempt, all on the ONE selected host. -1 means "not set in this
    /// config file"; <see cref="Load"/> then derives it from the old PrimaryRetries/SecondaryRetries
    /// pair so existing deployments keep their effective attempt count.
    /// </summary>
    public int RetryCount { get; set; } = -1;

    /// <summary>
    /// Total attempts per file, counting the initial one = 1 + RetryCount. Uploading targets a
    /// single host (<see cref="InitialHost"/>) with no failover, so there is one retry budget.
    /// </summary>
    [JsonIgnore] public int MaxAttempts => 1 + Math.Max(0, RetryCount);

    /// <summary>
    /// The ONE host every upload targets, chosen by <see cref="InitialHost"/> ("Primary" or
    /// "Secondary"). There is no failover: a file that exhausts its retries goes to NG rather than
    /// to the other server.
    /// </summary>
    [JsonIgnore] public string FirstHost =>
        InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase) ? SecondaryHost : PrimaryHost;

    /// <summary>
    /// THE routing rule, in one place so the data pump, the manifest sender, the NG pump and the
    /// Settings preview all agree: every attempt goes to <see cref="FirstHost"/>.
    ///
    /// Failover was removed deliberately. Sending a file to whichever server answered made "which
    /// machine holds this panel" unanswerable, and a site configured for one IP still saw uploads
    /// arriving on the other.
    /// </summary>
    public string HostForAttempt(int attempt) => FirstHost;

    /// <summary>
    /// One-line, plain-language description of where files go — shown live in Settings.
    /// </summary>
    [JsonIgnore] public string RoutePlan
    {
        get
        {
            var label = InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase) ? "secondary" : "primary";
            var other = InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase) ? PrimaryHost : SecondaryHost;
            var n = MaxAttempts;
            var a = n == 1 ? "1 attempt" : $"{n} attempts";
            return $"all uploads -> {FirstHost} ({label}), {a} per file; " +
                   $"no failover - {other} is never used, failures go to NG";
        }
    }

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
    /// Send the HOST manifest early when a file in the panel runs out of attempts, listing only the
    /// files that DID land (every " -pending" line stripped, exactly as the final manifest is built).
    ///
    /// Off by default, and deliberately so: the host manifest is normally the panel-complete signal,
    /// so an early copy tells the host system about a panel that is still being retried. It writes
    /// no .idxsent/.hostsent marker and no rawlog row, so the real manifest is still sent when the
    /// panel finishes — but whether the host side tolerates a partial list is a question about THEIR
    /// parser, not this app.
    ///
    /// Re-sent only when more files have landed since the last early send, so a panel with several
    /// failures does not upload the same list repeatedly.
    /// </summary>
    public bool MidFailHostUpload { get; set; } = false;

    /// <summary>
    /// Name the host manifest for the moment it is UPLOADED rather than when the panel was measured.
    ///
    /// The host manifest is "&lt;PID&gt;_&lt;yyyyMMddHHmmss&gt;.txt", and that stamp normally comes from the
    /// panel's DateTime, so every send of a panel writes to the same name and overwrites the last.
    /// With this on, the stamp is taken when each upload starts, so an early (mid-fail) send and the
    /// final one land under different names and BOTH are kept.
    ///
    /// LGD asked for this so they get the progression rather than one file replaced in place. The
    /// cost is volume: any panel that ever fails leaves two or three host files instead of one. Off
    /// by default — it changes what appears on their server, so it is theirs to turn on.
    ///
    /// The index has no stamp and is untouched; the local file keeps its name, so markers, the
    /// ".midfail" record and every log row are unaffected.
    /// </summary>
    public bool StampManifestNameAtUpload { get; set; } = false;

    /// <summary>
    /// Send only what is NEW since the panel's last manifest upload, instead of the full list.
    ///
    /// Each send — early or final — then carries just the files that landed since the one before, so
    /// LGD can read the increments rather than re-reading a growing list. The LOCAL manifests are
    /// untouched and stay complete: only a temp copy is uploaded, so markers, resume, DropLine and
    /// the no-pending gate all behave exactly as they do now.
    ///
    /// REQUIRES <see cref="StampManifestNameAtUpload"/>. Without per-upload names every send writes
    /// to the same remote file, so the last delta would overwrite all the earlier ones and the
    /// server would be left holding one increment instead of the panel. The UI refuses the
    /// combination and the engine ignores this flag when stamping is off.
    ///
    /// Note for the host system: with this on, NO single file lists the whole panel — the files must
    /// be accumulated. That is a real change to what the panel-complete manifest means.
    /// </summary>
    public bool DeltaManifests { get; set; } = false;

    /// <summary>
    /// How often the day + NG HTML reports are rewritten automatically, in seconds. 0 = off (only
    /// built on demand from the UI or the .bat scripts). The reports are only rebuilt when the
    /// rawlog has actually grown, so an idle machine does no work.
    /// </summary>
    public int HtmlLogRefreshSeconds { get; set; } = 60;

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
    /// rollover. 30 by default; 0 keeps everything forever. Only date-stamped files are ever
    /// removed, never today's, never anything else.
    ///
    /// NOTE the interaction with <see cref="NgRecoveryDays"/>: purging a day deletes its rawlog,
    /// and with it any record of files that were still unrecovered on that day. Keep retention
    /// comfortably larger than the recovery window, or work can be discarded before it has had a
    /// realistic chance of being retried.
    /// </summary>
    public int LogRetentionDays { get; set; } = 30;

    /// <summary>
    /// How many PAST days the NG console loads and auto-retries alongside today. Files still
    /// unfinished at a day rollover are filed under the OLD day, so with 0 (the previous behaviour)
    /// nothing abandoned at midnight is ever retried automatically — an engineer had to open
    /// yesterday by hand. 1 (default) covers exactly that case: today plus yesterday.
    /// Recovery always logs to the day the file belongs to, whatever day it is recovered on.
    /// </summary>
    public int NgRecoveryDays { get; set; } = 1;

    /// <summary>
    /// Cap on how many days outside the recovery window are scanned for the "n older" backlog
    /// figure. 0 (default) = all retained days; the scan is cached per day and runs off the pumps,
    /// so this is a safety valve for a site that has accumulated something pathological, not a
    /// knob anyone should normally need.
    /// </summary>
    public int NgBacklogScanDays { get; set; } = 0;

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
    // These two keep their old NAMES so the ~30 call sites do not all have to change, but they now
    // resolve through the totallog helpers above — new name for writing, old name honoured when a
    // day was recorded before the rename.
    public string RawLogPath(DateTime day) => TotalLogPath(day);
    public string SnapshotPath(DateTime day) => Path.Combine(LogFullPath, $"{day:yyyyMMdd}_snapshot.txt");
    public string JobsPath(DateTime day) => Path.Combine(JobsFullPath, $"{day:yyyyMMdd}_jobs.txt");
    /// <summary>
    /// The full per-file transfer log — every attempt, every status change. Named "totallog"
    /// because that is what the UI has always called it.
    ///
    /// READING a day must also accept the old "_rawlog.txt" name: every log set recorded before
    /// the rename still uses it, including the ones copied off site for analysis. Use
    /// <see cref="TotalLogPathForDay"/> to resolve, never build the name inline.
    /// </summary>
    public string TotalLogPath(DateTime day) => TotalLogPathForDay(day.ToString("yyyyMMdd"));

    public string TotalLogPathForDay(string day)
    {
        var now = Path.Combine(LogFullPath, $"{day}_totallog.txt");
        if (File.Exists(now)) return now;
        var legacy = Path.Combine(LogFullPath, $"{day}_rawlog.txt");
        return File.Exists(legacy) ? legacy : now;   // new name when neither exists (we are writing)
    }

    /// <summary>The NG-retry equivalent, with the same legacy fallback ("_ngretrylog.txt").</summary>
    public string NgRetryTotalLogPath(string day)
    {
        var now = Path.Combine(LogFullPath, $"{day}_ngretrytotallog.txt");
        if (File.Exists(now)) return now;
        var legacy = Path.Combine(LogFullPath, $"{day}_ngretrylog.txt");
        return File.Exists(legacy) ? legacy : now;
    }

    /// <summary>
    /// The APP's own log: startups and shutdowns with reasons, day rollovers, settings changes,
    /// crashes, and a pump dying. Nothing about panels.
    ///
    /// Kept separate and never purged. It is tiny (a few hundred bytes a day) and it is the only
    /// evidence available when a site reports "it stopped uploading last month".
    /// </summary>
    public string AppLogPath(DateTime day) => Path.Combine(LogFullPath, $"{day:yyyyMMdd}_aplog.txt");
    public string AppLogPathForDay(string day) => Path.Combine(LogFullPath, $"{day}_aplog.txt");

    /// <summary>
    /// PANEL EVENTS, raw: early (mid-fail) manifest sends and why one was skipped, source files
    /// that vanished, the transport's own error text when a transfer threw, and panels whose source
    /// folder was empty or gone.
    ///
    /// Written by the engines; the Operation report is built FROM this. Named "panelevents" and not
    /// "oplog" because the readable report is "{day}_operation.txt", and two files one letter apart
    /// meaning different things fooled nobody and confused everybody.
    ///
    /// READING accepts the old "_oplog.txt" name so days recorded before the rename still work.
    /// </summary>
    public string PanelEventsPath(DateTime day) => PanelEventsPathForDay(day.ToString("yyyyMMdd"));

    public string PanelEventsPathForDay(string day)
    {
        var now = Path.Combine(LogFullPath, $"{day}_panelevents.txt");
        if (File.Exists(now)) return now;
        var legacy = Path.Combine(LogFullPath, $"{day}_oplog.txt");
        return File.Exists(legacy) ? legacy : now;   // new name when neither exists (we are writing)
    }

    /// <summary>The readable per-file report, kept on disk so it travels with a copied log folder.</summary>
    public string OperationReportPath(string day) => Path.Combine(LogFullPath, $"{day}_operation.csv");

    // Day-string variants (yyyyMMdd) for the NG-retry console, which browses arbitrary days.
    // Both resolve through the totallog helpers, so a legacy day still reads.
    public string RawLogPathForDay(string day) => TotalLogPathForDay(day);
    public string JobsPathForDay(string day) => Path.Combine(JobsFullPath, $"{day}_jobs.txt");
    public string NgRetryLogPath(string day) => NgRetryTotalLogPath(day);

    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static Config Load(string path)
    {
        if (!File.Exists(path))
        {
            var fresh = new Config();
            if (fresh.RetryCount < 0) fresh.RetryCount = 3;
            fresh.Save(path);
            return fresh;
        }
        var json = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<Config>(json) ?? new Config();
        cfg.MigrateRetries(json);
        return cfg;
    }


    /// <summary>
    /// Upgrade a config written before failover was removed. RetryCount takes the retries of the
    /// host that config actually selected, so the attempt count on that host is unchanged: a site
    /// running Secondary / PrimaryRetries=0 / SecondaryRetries=3 keeps 4 attempts on the secondary.
    ///
    /// The legacy keys are read straight from the JSON because they are no longer properties. If
    /// neither RetryCount nor the old pair is present, fall back to 3 (4 attempts) rather than the
    /// -1 sentinel — leaving it would silently reduce a production machine to a single attempt.
    /// </summary>
    public void MigrateRetries(string? json = null)
    {
        if (RetryCount >= 0) return;

        int? legacy = null;
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var wantSecondary = InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase);
                var key = wantSecondary ? "SecondaryRetries" : "PrimaryRetries";
                if (doc.RootElement.TryGetProperty(key, out var el) && el.TryGetInt32(out var v)) legacy = v;
            }
            catch { /* malformed config — fall through to the default */ }
        }
        RetryCount = Math.Max(0, legacy ?? 3);
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
