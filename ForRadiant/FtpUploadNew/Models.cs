using System.Text;

namespace FtpUpload;

public enum FileStatus
{
    Pending,    // not resolved yet: never attempted, or failed with retries remaining
    Succeeded,
    Failed,     // final — all attempts exhausted
    TimedOut    // final — the panel's timeout elapsed before this file finished
}

/// <summary>One file belonging to one panel (PID).</summary>
public sealed class JobFile
{
    public string Pid { get; init; } = "";
    public string FileName { get; init; } = "";
    public string LocalPath { get; init; } = "";
    public string RemotePath { get; init; } = "";

    public FileStatus Status { get; set; } = FileStatus.Pending;
    public string SucceedTime { get; set; } = "";
    public int FailCount { get; set; }
    public List<string> FailTimes { get; } = new();
    public int Attempts { get; set; }

    public string Key => Pid + "|" + FileName;
}

/// <summary>
/// The day's jobs-file line format, shared by every reader/writer so it stays in one place.
///
///   Legacy (jobs.txt from TrueTest / demo):  PID|FileName|LocalPath[|RemotePath]
///   Panel (written by PanelIntake):          PID|FileName|LocalPath|RemotePath|IndexSrc|HostSrc|UploadIndexPath|UploadHostPath|ChannelIndex
///
/// Extra fields are ignored by legacy readers (they only read the first 3–4), so appending the
/// panel fields is backward compatible. A line is a "panel line" when it has all 9 fields.
/// </summary>
public sealed class JobsLine
{
    public string Pid = "", FileName = "", LocalPath = "", RemotePath = "";
    public string IndexSrc = "", HostSrc = "", UploadIndexPath = "", UploadHostPath = "", ChannelIndex = "";
    public bool IsPanel;

    public string ToLine() => IsPanel
        ? string.Join('|', Pid, FileName, LocalPath, RemotePath, IndexSrc, HostSrc, UploadIndexPath, UploadHostPath, ChannelIndex)
        : string.Join('|', Pid, FileName, LocalPath, RemotePath);

    public static JobsLine? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;
        var p = line.Split('|');
        if (p.Length < 3) return null;
        var j = new JobsLine
        {
            Pid = p[0].Trim(), FileName = p[1].Trim(), LocalPath = p[2].Trim(),
            RemotePath = p.Length >= 4 ? p[3].Trim() : ""
        };
        if (p.Length >= 9)
        {
            j.IsPanel = true;
            j.IndexSrc = p[4].Trim(); j.HostSrc = p[5].Trim();
            j.UploadIndexPath = p[6].Trim(); j.UploadHostPath = p[7].Trim();
            j.ChannelIndex = p[8].Trim();
        }
        return j;
    }
}

/// <summary>One panel: all files captured for a single PID.</summary>
public sealed class Job
{
    public string Pid { get; init; } = "";
    public DateTime Day { get; init; }
    public List<JobFile> Files { get; } = new();

    // ---- panel metadata from the .panel handoff (used by manifest/completion) ----
    // Empty on panels that came from the legacy jobs.txt path; set by RegisterPanel for .panel intake.
    public string SourceFolder { get; set; } = "";
    public string ChannelIndex { get; set; } = "";
    /// <summary>Remote destination for the index manifest (from the .panel file).</summary>
    public string UploadIndexPath { get; set; } = "";
    /// <summary>Remote destination for the host manifest (from the .panel file).</summary>
    public string UploadHostPath { get; set; } = "";
    /// <summary>Local index manifest this program creates/updates: {SourceFolder}\{PID}.idx</summary>
    public string IndexSrc { get; set; } = "";
    /// <summary>Local host manifest: {SourceFolder}\{PID}_{DateTime}.txt</summary>
    public string HostSrc { get; set; } = "";
    /// <summary>folder ∩ recipe at intake — the number of data files this panel should upload.</summary>
    public int TotalFileCount { get; set; }
    /// <summary>True once this panel's index+host manifests have been sent (finalized).</summary>
    public bool Finalized { get; set; }

    /// <summary>True if this panel carries .panel metadata (vs. a legacy jobs.txt panel).</summary>
    public bool IsPanelJob => SourceFolder.Length > 0;

    /// <summary>When the panel's first file began uploading — the panel-timeout clock starts
    /// here (default/unset until then), so a panel waiting its turn is not yet on the clock.</summary>
    public DateTime Started { get; set; }
    /// <summary>Set once the panel-timeout elapsed with files still unfinished.</summary>
    public bool TimedOut { get; set; }

    public bool AllSucceeded => Files.Count > 0 && Files.All(f => f.Status == FileStatus.Succeeded);
    public bool AnyPending => Files.Any(f => f.Status == FileStatus.Pending);

    /// <summary>
    /// Derived panel status, logged on each raw-log line and shown in the UI/report:
    /// TIMEDOUT (panel timed out) &gt; INPROGRESS (any file still pending) &gt; SUCCESS
    /// (all uploaded) &gt; FAILED (otherwise).
    /// </summary>
    public string PanelStatus =>
        TimedOut ? "TIMEDOUT"
        : Files.Count == 0 ? "EMPTY"
        : AnyPending ? "INPROGRESS"
        : AllSucceeded ? "SUCCESS"
        : "FAILED";
}

/// <summary>
/// Shared file access helpers. Every writer in this program goes through the same
/// named mutex, so appends from the UI/worker can never interleave destructively.
/// </summary>
public static class SafeFile
{
    private static readonly Mutex CrossProcess = new(false, @"Global\FtpUpload_Files");

    public static void WithLock(Action action)
    {
        var held = false;
        try
        {
            try { held = CrossProcess.WaitOne(TimeSpan.FromSeconds(10)); }
            catch (AbandonedMutexException) { held = true; }
            action();
        }
        finally
        {
            if (held) CrossProcess.ReleaseMutex();
        }
    }

    /// <summary>Append with retry — absorbs transient sharing violations (AV scans etc).</summary>
    public static void Append(string path, string line)
    {
        WithLock(() => Retry(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
        }));
    }

    public static string[] ReadLines(string path)
    {
        string[] result = Array.Empty<string>();
        WithLock(() => Retry(() =>
        {
            result = File.Exists(path) ? File.ReadAllLines(path, Encoding.UTF8) : Array.Empty<string>();
        }));
        return result;
    }

    /// <summary>
    /// Rewrites a file ATOMICALLY: write a temp file, flush it, then swap it into place.
    ///
    /// A plain File.WriteAllLines that is interrupted half way — power cut, watchdog kill,
    /// BSOD — leaves a truncated file. With the swap, the file on disk is always either the
    /// complete old version or the complete new one. Kept as a general utility; there is no
    /// longer a persisted wait-list that needs it, but it is the correct primitive for any
    /// full-file rewrite that must survive a crash.
    /// </summary>
    public static void WriteLines(string path, IEnumerable<string> lines)
    {
        WithLock(() => Retry(() =>
        {
            var full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);

            var tmp = full + ".tmp";
            using (var sw = new StreamWriter(tmp, false, Encoding.UTF8))
            {
                foreach (var l in lines) sw.WriteLine(l);
                sw.Flush();
                sw.BaseStream.Flush();
            }

            if (File.Exists(full)) File.Replace(tmp, full, null);
            else File.Move(tmp, full);
        }));
    }

    /// <summary>Read every line then truncate, atomically w.r.t. other writers.</summary>
    public static string[] DrainLines(string path)
    {
        string[] result = Array.Empty<string>();
        WithLock(() => Retry(() =>
        {
            if (!File.Exists(path)) { result = Array.Empty<string>(); return; }
            result = File.ReadAllLines(path, Encoding.UTF8);
            File.WriteAllText(path, "", Encoding.UTF8);
        }));
        return result;
    }

    private static void Retry(Action action, int attempts = 5, int delayMs = 150)
    {
        for (var i = 1; ; i++)
        {
            try { action(); return; }
            catch (IOException) when (i < attempts) { Thread.Sleep(delayMs); }
            catch (UnauthorizedAccessException) when (i < attempts) { Thread.Sleep(delayMs); }
        }
    }
}

/// <summary>Which IP the NG-retry pump uses for an attempt.</summary>
public enum NgIpMode { Auto, Primary, Secondary }

/// <summary>State of one NG item within the NG-retry console.</summary>
public enum NgItemState { Waiting, Uploading, Succeeded, Failed, Gone }

/// <summary>
/// One entry in the NG-retry console — a file that ended FAILED or TIMEDOUT on some day, loaded
/// from that day's jobs+raw logs. The NG-retry pump uploads these independently of the live
/// engine, with a manually chosen IP and unlimited retries, and records outcomes to that day's
/// separate ng-retry log. This is a value record; it does NOT share objects with the live engine.
/// </summary>
public sealed class NgItem
{
    public string Day { get; init; } = "";          // original day, yyyyMMdd
    public string Pid { get; init; } = "";
    public string FileName { get; init; } = "";
    public string LocalPath { get; init; } = "";
    public string RemotePath { get; init; } = "";
    public string OrigStatus { get; init; } = "";    // "FAILED" or "TIMEDOUT"

    // Panel manifest paths (from the panel jobs line); empty for legacy files. Let the NG pump
    // update the index/host manifest when it recovers a panel file, and finalize past-day panels
    // (which have no live Job) itself.
    public string IndexSrc { get; init; } = "";
    public string HostSrc { get; init; } = "";
    public string UploadIndexPath { get; init; } = "";
    public string UploadHostPath { get; init; } = "";

    public int PriorRetries { get; set; }            // retries already in the ng-retry log
    public int SessionRetries { get; set; }          // retries this session
    public NgItemState State { get; set; } = NgItemState.Waiting;
    public bool IsCurrent { get; set; }              // the item the NG pump is actively working
    public string LastHost { get; set; } = "";
    public string LastResult { get; set; } = "";

    public int TotalRetries => PriorRetries + SessionRetries;
    public string Key => Day + "|" + Pid + "|" + FileName;

    /// <summary>Adapter so the FtpTransfer primitive (which takes a JobFile) can upload this.</summary>
    public JobFile ToJobFile() => new()
    {
        Pid = Pid, FileName = FileName, LocalPath = LocalPath, RemotePath = RemotePath
    };
}
