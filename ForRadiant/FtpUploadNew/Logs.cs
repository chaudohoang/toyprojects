namespace FtpUpload;

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
    public void Write(JobFile f, int maxAttempts, string host, Job? job = null, DateTime? day = null)
    {
        var line = string.Join("|",
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
            job?.PanelStatus ?? "");     // 10th field: the panel's status at this instant

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
            var p = line.Split('|');
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
            new[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), job.Pid, overall }
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
        var now = DateTime.Now.ToString("HH:mm:ss");
        var line = string.Join("|",
            item.Pid,
            item.FileName,
            succeeded ? "SUCCEEDED" : "FAILED",
            succeeded ? now : "",
            item.TotalRetries.ToString(),
            now,
            item.TotalRetries.ToString(),
            "0",                       // 0 = unlimited retries
            host,
            "NGRETRY");

        SafeFile.Append(cfg.NgRetryLogPath(item.Day), line);
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
            var p = line.Split('|');
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
