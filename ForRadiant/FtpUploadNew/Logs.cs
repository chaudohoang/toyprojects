namespace FtpUpload;

/// <summary>
/// One place that knows the row layout of the rawlog and the ng-retry log.
///
/// Field 0 is the WRITE TIME ("yyyy-MM-dd HH:mm:ss"), then PID, FileName, Status, ... Putting it
/// first makes these files readable the way the operation log is: when each line was appended is
/// visible without counting columns, and that matters because a row can be written long after the
/// event it describes (an NG write-back, or the sweep recording a manifest).
///
/// Rows written before the move have no timestamp and start with the PID. BOTH shapes are accepted,
/// so logs already collected — including a set downloaded from a site for analysis — keep working.
/// Every reader goes through Fields() instead of splitting for itself: sixteen places each deciding
/// what a column meant is how a field gets misread.
/// </summary>
public static class LogRow
{
    /// <summary>Row fields WITHOUT the leading write time, so index 0 is always the PID.</summary>
    public static string[] Fields(string line)
    {
        var p = line.Split('|');
        return HasStamp(p) ? p[1..] : p;
    }

    /// <summary>The row's write time, or "" for a row written before the timestamp existed.</summary>
    public static string WrittenAt(string line)
    {
        var p = line.Split('|');
        return HasStamp(p) ? p[0] : "";
    }

    /// <summary>True when this row belongs to the given panel, whichever shape it is.</summary>
    public static bool IsPanel(string line, string pid)
    {
        var p = line.Split('|');
        var i = HasStamp(p) ? 1 : 0;
        return p.Length > i && p[i].Equals(pid, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The row's PID, or "" when the line is too short to have one.</summary>
    public static string Pid(string line)
    {
        var f = Fields(line);
        return f.Length > 0 ? f[0] : "";
    }

    // "yyyy-MM-dd HH:mm:ss" (19 chars) or "yyyy-MM-dd HH:mm:ss.fff" (23) — dashes at 4 and 7 make
    // it cheap to recognise, and no PID looks like that.
    //
    // BOTH lengths, because milliseconds were added later: every day recorded before that, and
    // every log set copied off site, still has the 19-character form. Accepting only one length
    // would make those rows look like they had no timestamp at all, and the whole Operation report
    // is ordered by it.
    private static bool HasStamp(string[] p)
        => p.Length > 1 && (p[0].Length == 19 || p[0].Length == 23)
           && p[0][4] == '-' && p[0][7] == '-';
}

/// <summary>
/// Log 1 (spec §4) — YYYYMMDD_rawlog.txt, strictly append-only.
/// One line is appended per event (attempt finished / file resolved); each line is a
/// full snapshot of that file's state at that instant, so a reader can reduce the file
/// to "current state" by taking the LAST line per PID+FileName.
///
///   PID|FileName|Status|SucceedTime|FailCount|FailTimes|Attempts|MaxRetries|Host
///
/// The trailing Host field is an addition beyond the customer spec: it records which CNS
/// IP that attempt actually used, which is the only way to confirm after the fact that
/// failover to the secondary happened. It is appended LAST on purpose — any reader that
/// expects the original 8 fields keeps working and simply ignores it.
///
/// This log is also what makes the single job list restart-safe: on startup the engine
/// replays it to recover which files already SUCCEEDED or FAILED, so neither is queued
/// again. (See UploadEngine.AddFiles / LoadHistory.)
/// </summary>
public sealed class RawLog(Config cfg)
{
    public void Write(JobFile f, int maxAttempts, string host, Job? job = null, DateTime? day = null,
                      string reason = "")
    {
        var line = string.Join("|",
            // Field 0: when this row was appended. First, so these files read like the operation
            // log. See LogRow, which every reader uses.
            Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            f.Pid,
            f.FileName,
            f.Status switch
            {
                FileStatus.Succeeded => "SUCCEEDED",
                FileStatus.Failed => "FAILED",
                FileStatus.TimedOut => "TIMEDOUT",
                _ => "PENDING"
            },
            f.SucceedTime,
            f.FailCount.ToString(),
            string.Join(",", f.FailTimes),
            f.Attempts.ToString(),
            maxAttempts.ToString(),
            host,
            job?.PanelStatus ?? "",      // 10th field: the panel's status at this instant
            // 11th: why, when FAILED needs distinguishing. "SOURCE_GONE" means the local file was
            // deleted, so no retry can ever help — a transfer failure looks identical in the status
            // column without it, and an operator cannot tell "the server refused" from "the file
            // is not there any more".
            reason);

        SafeFile.Append(cfg.RawLogPath(day ?? Clock.Now), line);
    }

    /// <summary>
    /// Reduces today's append-only log to "current state per file" by keeping the LAST line
    /// for each PID+FileName.
    ///
    /// This is what makes a restart safe: without it the program re-reads the whole jobs file
    /// on startup, finds an empty in-memory job list, and re-uploads everything already sent.
    /// The watchdog restarts this program routinely, so that would mean re-sending the entire
    /// day on every restart.
    /// </summary>
    public Dictionary<string, JobFile> ReadState(DateTime day)
    {
        var result = new Dictionary<string, JobFile>();

        foreach (var line in SafeFile.ReadLines(cfg.RawLogPath(day)))
        {
            var p = LogRow.Fields(line);
            if (p.Length < 8) continue;

            var rec = new JobFile { Pid = p[0], FileName = p[1] };
            rec.Status = p[2] switch
            {
                "SUCCEEDED" => FileStatus.Succeeded,
                "FAILED" => FileStatus.Failed,
                "TIMEDOUT" => FileStatus.TimedOut,
                _ => FileStatus.Pending
            };
            rec.SucceedTime = p[3];
            rec.FailCount = int.TryParse(p[4], out var fc) ? fc : 0;
            if (!string.IsNullOrEmpty(p[5]))
                rec.FailTimes.AddRange(p[5].Split(','));
            rec.Attempts = int.TryParse(p[6], out var at) ? at : 0;

            result[rec.Key] = rec;     // later lines overwrite earlier ones
        }

        return result;
    }
}

/// <summary>
/// Log 2 (spec §4) — the Result-Timing Snapshot, written at the exact moment TrueTest
/// sends the Result value to the fixture.
///
/// Overall is O only when 100% of the panel's files are already uploaded at that instant;
/// anything else (in progress, pending, failed) is X. Per-file O/X likewise reflects the
/// state at that instant, NOT the eventual outcome.
/// </summary>
public sealed class SnapshotLog(Config cfg)
{
    public void Write(Job job)
    {
        var overall = job.AllSucceeded ? "O" : "X";
        var perFile = job.Files.Select(f =>
            $"{f.FileName}:{(f.Status == FileStatus.Succeeded ? "O" : "X")}");

        var line = string.Join("|",
            new[] { Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), job.Pid, overall }
                .Concat(perFile));

        SafeFile.Append(cfg.SnapshotPath(Clock.Now), line);
    }
}

/// <summary>
/// The NG-retry log (one file per original day: YYYYMMDD_ngretrylog.txt). Same pipe-delimited
/// shape as the raw log, but records ONLY NG-retry attempts — the manual recovery pump, separate
/// from the live upload log. "Attempts"/"FailCount" carry the running retry count (unlimited), and
/// the last field is NGRETRY so a reader can tell it apart from the normal raw log.
///
///   PID|FileName|Status|SucceedTime|TotalRetries|Time|TotalRetries|0|Host|NGRETRY
/// </summary>
public sealed class NgRetryLog(Config cfg)
{
    public void Write(NgItem item, bool succeeded, string host)
    {
        var now = Clock.Now.ToString("HH:mm:ss");
        var line = string.Join("|",
            Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),   // field 0 — see LogRow
            item.Pid,
            item.FileName,
            succeeded ? "SUCCEEDED" : "FAILED",
            succeeded ? now : "",
            item.TotalRetries.ToString(),
            now,
            item.TotalRetries.ToString(),
            "0",                       // 0 = unlimited retries
            host,
            "NGRETRY",
            "");                       // reason - kept so the columns line up with the rawlog

        SafeFile.Append(cfg.NgRetryLogPath(item.Day), line);
    }

    /// <summary>
    /// Record that a panel's index + host manifests were sent by NG's post-recovery finalize, so the
    /// NG report shows them (index first, then host — host last). This goes ONLY to the ng-retry log,
    /// never the main rawlog, so the main rawlog stays a record of the live pump.
    /// </summary>
    public void WriteManifestSent(string pid, string day, string uploadIndexPath, string uploadHostPath,
                                  int retries, string host)
    {
        var now = Clock.Now.ToString("HH:mm:ss");
        foreach (var remote in new[] { uploadIndexPath, uploadHostPath })   // index first, host last
        {
            if (string.IsNullOrWhiteSpace(remote)) continue;
            var line = string.Join("|",
                Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),   // field 0 - see LogRow  // Clock.Now, not DateTime.Now: under SimulateFastDaySeconds the simulated
                                                        // day advances, and two loggers on different clocks put
                                                        // the same moment on different dates - the timeline then
                                                        // sorted rollover rows after the retries that followed them.
                pid, Path.GetFileName(remote), "SUCCEEDED", now,
                retries.ToString(), now, retries.ToString(), "0", host, "NGRETRY", "");
            SafeFile.Append(cfg.NgRetryLogPath(day), line);
        }
    }

    /// <summary>
    /// Reduces a day's ng-retry log to per-file state: how many retries recorded, and whether it
    /// eventually SUCCEEDED via NG retry (so the console can drop it from the list).
    /// </summary>
    public Dictionary<string, (int retries, bool succeeded)> ReadState(string day)
    {
        var result = new Dictionary<string, (int, bool)>();
        foreach (var line in SafeFile.ReadLines(cfg.NgRetryLogPath(day)))
        {
            var p = LogRow.Fields(line);
            if (p.Length < 3) continue;
            var key = p[0] + "|" + p[1];
            var retries = p.Length > 6 && int.TryParse(p[6], out var r) ? r : 0;
            var succeeded = p[2] == "SUCCEEDED";
            var prev = result.TryGetValue(key, out var e) ? e : (0, false);
            result[key] = (Math.Max(prev.Item1, retries), prev.Item2 || succeeded);
        }
        return result;
    }
}
