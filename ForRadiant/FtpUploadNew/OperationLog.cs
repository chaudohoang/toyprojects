using System.Text;

namespace FtpUpload;

/// <summary>
/// The day as a TIMELINE, one row per event, as CSV: "{day}_operation.csv".
///
/// Replaces an earlier design that collapsed the day to one row per file. That was compact but it
/// hid the sequence, which is what anyone actually asks about: when did this file first fail, what
/// did the engine do next, how long until recovery, what else was happening to the panel at the
/// time. Aggregation is what Excel is for — give it the events and let it group.
///
/// Merges three raw logs into one chronological list and says what each row MEANS in words:
///   - the totallog   — every upload attempt and its outcome
///   - the ng-retry log — every recovery attempt
///   - the panel-events log — early manifest sends and skips, source-gone, pump errors, warnings
///
/// CSV rather than fixed-width text so it sorts, filters and pivots. Written with a UTF-8 BOM so
/// Excel reads it directly.
/// </summary>
public static class OperationLog
{
    /// <summary>One event in the day timeline. Internal so the Trace Panel can render the
    /// same events for one panel - one parser, two presentations.</summary>
    internal sealed record Ev(
        string When,      // yyyy-MM-dd HH:mm:ss — the sort key
        string Pid,
        string File,
        string Event,     // what happened, in words
        string Result,    // OK / FAILED / TIMED OUT / SKIPPED / -
        string Try,       // attempt number, where the event has one
        string Host,
        string Reason,
        string Source,    // which log the row came from: day job / ng retry / panel event
        int Order);       // tie-break within the same second (the log is second-resolution)

    /// <summary>Every event of a day, in order, optionally narrowed to one panel. Shared by
    /// the CSV and the Trace Panel so the two can never disagree about what happened.</summary>
    internal static List<Ev> Collect(Config cfg, string day, string? onlyPid = null)
    {
        var all = Gather(cfg, day);
        return string.IsNullOrEmpty(onlyPid) ? all
            : all.Where(e => e.Pid.Equals(onlyPid, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static string? Build(Config cfg, string day)
    {
        var list = Gather(cfg, day);
        return list.Count == 0 ? null : WriteCsv(cfg, day, list);
    }

    private static List<Ev> Gather(Config cfg, string day)
    {
        var totalPath = cfg.TotalLogPathForDay(day);
        if (!File.Exists(totalPath)) return new List<Ev>();

        var evs = new List<Ev>(9000);
        var dayPrefix = $"{day[..4]}-{day[4..6]}-{day[6..8]}";

        // Which panels belong to THIS day. A panel's life does not stop at midnight: the rollover
        // hands its unfinished files to NG, and NG can still be retrying them days later — with
        // every one of those events written to the LATER day's files. Reading only this day's logs
        // showed a panel's first early manifest send and silently dropped the second, because the
        // second happened after the rollover. Collected here, then used below to pull the panel's
        // continuation out of later days.
        var ownPids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in SafeFile.ReadLines(totalPath))
        {
            var q = LogRow.Fields(line);
            if (q.Length >= 1 && q[0].Length > 0) ownPids.Add(q[0]);
        }

        // Which panels have panel-event rows for their manifest sends.
        //
        // ONE ROW PER ATTEMPT. A single manifest transfer is written by three loggers — the ng log,
        // the transfer log, and the panel events — so finalizing one panel produced "ng retry
        // succeeded", "index uploaded" and "final index sent" for the same upload. The panel event
        // is the informative one: it names the remote file and says index or host. So for any panel
        // that has those events, the manifest rows from the other two logs are dropped as repeats.
        // Panels without them (older log sets) keep their transfer-log rows, or they would vanish.
        var manifestFromEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var evPath in DayFilesFrom(cfg, day, d => cfg.PanelEventsPathForDay(d)))
        foreach (var line in SafeFile.ReadLines(evPath))
        {
            var m = System.Text.RegularExpressions.Regex.Match(line, @"\]\s*(?:MIDFAIL|FINALIZE)\s+(\S+?):");
            if (m.Success) manifestFromEvents.Add(m.Groups[1].Value);
        }

        // ---- 1. the live pump: one row per attempt --------------------------------------------
        foreach (var line in SafeFile.ReadLines(totalPath))
        {
            var p = LogRow.Fields(line);
            if (p.Length < 3) continue;
            var stamp = LogRow.WrittenAt(line);
            var when = stamp.Length >= 19 ? stamp : dayPrefix + " " + ClockOf(p, dayPrefix);
            var isIndex = p[1].EndsWith(".idx", StringComparison.OrdinalIgnoreCase);
            var isManifest = isIndex
                          || System.Text.RegularExpressions.Regex.IsMatch(p[1], @"_\d{14}\.txt$");
            // The panel event already reports this send, with the remote name. See above.
            if (isManifest && manifestFromEvents.Contains(p[0])) continue;
            // SKIP NG write-backs.
            //
            // When NG recovers a file it writes the result back into the totallog as well, so the
            // day report and summary count it. That row and the ng-retry log's own row are the SAME
            // event, so listing both double-counts every recovery — and the write-back sorted
            // BEFORE the retry that caused it, because ties fall back to file read order.
            //
            // The write-back carries no NGRETRY tag (only the ng log does). What identifies it is a
            // fail count driven up by NG while the panel-status field is blank.
            var panelStatus = p.Length > 9 ? p[9] : "";
            var failCount = p.Length > 4 && int.TryParse(p[4], out var fc) ? fc : 0;
            var attempts = p.Length > 6 && int.TryParse(p[6], out var at) ? at : 0;
            if (panelStatus.Length == 0 && failCount > 0 && attempts > failCount) continue;

            var viaNg = panelStatus.Equals("NGRETRY", StringComparison.OrdinalIgnoreCase);

            // "PENDING" is not a state anyone waits in — it is written when an attempt FAILED and
            // the file was put back in the queue. Calling it "pending" in a timeline reads as
            // "nothing happened yet", which is the opposite of the truth.
            var reasonTxt = p.Length > 10 ? p[10] : "";
            // The PER-FILE time limit, named. Only the PANEL timeout had a row of its own
            // ("cut off by the panel timeout"), so a file hitting its own limit read as an
            // ordinary "attempt failed" — and that limit is what aborted the session and stopped
            // index/host at LGD on 2026-09-18. Detected from the reason the transport recorded.
            var fileTimeout = reasonTxt.Contains("exceeded", StringComparison.OrdinalIgnoreCase)
                           && reasonTxt.Contains("s on ", StringComparison.OrdinalIgnoreCase);
            var limit = fileTimeout ? " (file time limit)" : "";

            var (ev, res) = p[2] switch
            {
                // Name WHICH manifest. "manifest uploaded" left you checking the filename to see
                // whether it was the index or the host, on every row.
                "SUCCEEDED" => (isManifest ? (isIndex ? "index uploaded" : "host uploaded") : "uploaded", "ok"),
                "FAILED"    => (isManifest ? (isIndex ? "index gave up" : "host gave up") : "gave up after last attempt" + limit, "failed"),
                "TIMEDOUT"  => ("cut off by the panel timeout", "timed out"),
                "PENDING"   => ("attempt failed, queued to try again" + limit, fileTimeout ? "timed out" : "retrying"),
                _           => (p[2].ToLowerInvariant(), p[2]),
            };
            if (viaNg) ev += " (via ng retry)";

            evs.Add(new Ev(when, p[0], p[1], ev, res,
                           p.Length > 6 ? p[6] : "",
                           p.Length > 8 ? p[8] : "",
                           reasonTxt, "day job", 2));
        }

        // ---- 2. NG recovery attempts ------------------------------------------------------------
        //
        // This day's NG log, plus every LATER day's — filtered to this day's panels. After a
        // rollover the panel's retries are written to the new day's file, so reading only this
        // day's log stops the story at midnight.
        foreach (var ngPath in DayFilesFrom(cfg, day, d => cfg.NgRetryTotalLogPath(d)))
        foreach (var line in SafeFile.ReadLines(ngPath))
        {
            var p = LogRow.Fields(line);
            if (p.Length < 3) continue;
            if (!ownPids.Contains(p[0])) continue;      // a later day's own panels are not ours
            // Manifest sends are reported once, by the panel event. See the note above.
            if (manifestFromEvents.Contains(p[0]) &&
                (p[1].EndsWith(".idx", StringComparison.OrdinalIgnoreCase) ||
                 System.Text.RegularExpressions.Regex.IsMatch(p[1], @"_\d{14}\.txt$"))) continue;
            var stamp = LogRow.WrittenAt(line);
            var when = stamp.Length >= 19 ? stamp : dayPrefix + " " + ClockOf(p, dayPrefix);
            var ok = p[2] == "SUCCEEDED";
            evs.Add(new Ev(when, p[0], p[1],
                           ok ? "ng retry succeeded" : "ng retry failed",
                           ok ? "ok" : "failed",
                           p.Length > 6 ? p[6] : "",
                           p.Length > 8 ? p[8] : "",
                           p.Length > 10 ? p[10] : "", "ng retry", 1));
        }

        // ---- 3. panel-level events --------------------------------------------------------------
        // Same treatment: a panel's second early-manifest send often lands after a rollover.
        foreach (var evPath in DayFilesFrom(cfg, day, d => cfg.PanelEventsPathForDay(d)))
        foreach (var line in SafeFile.ReadLines(evPath))
        {
            // 19 chars ("yyyy-MM-dd HH:mm:ss") or 23 with milliseconds. Pinning it to 19 dropped
            // every event once milliseconds were added — silently, because a non-match just skips
            // the line, so the report simply lost its whole panel-event section.
            var m = System.Text.RegularExpressions.Regex.Match(line, @"^\[([^\]]{19,23})\]\s+(.+)$");
            if (!m.Success) continue;
            var when = m.Groups[1].Value;
            var body = m.Groups[2].Value.Trim();

            var mid = System.Text.RegularExpressions.Regex.Match(body, @"^MIDFAIL\s+(\S+?):\s*(.+)$");
            if (mid.Success)
            {
                var pid = mid.Groups[1].Value;
                if (!ownPids.Contains(pid)) continue;   // a later day's own panels are not ours
                var rest = mid.Groups[2].Value;

                // THREE outcomes, not two. A "send error:" line means the upload threw — the
                // session was aborted mid-flight — and nothing reached the server. Testing only
                // for "NOT sent" classified those as successes, so the report would state the
                // manifest had gone out when the connection had just died under it. Seen on
                // 2026-09-13, where every mid-fail for one panel ended "send error: Session was
                // aborted" and no host file existed.
                // The lines are now per MANIFEST: "early index manifest sent ... as NAME" or
                // "early host manifest FAILED to send ...". Report the kind, so the timeline says
                // which manifest each row is about rather than lumping them together.
                var kind = rest.StartsWith("early host", StringComparison.OrdinalIgnoreCase) ? "host"
                         : rest.StartsWith("early index", StringComparison.OrdinalIgnoreCase) ? "index" : "";
                var failedToSend = rest.Contains("FAILED to send", StringComparison.OrdinalIgnoreCase);
                var errored = failedToSend || rest.StartsWith("send error", StringComparison.OrdinalIgnoreCase);
                var skipped = rest.StartsWith("NOT sent", StringComparison.OrdinalIgnoreCase);
                var sent = !errored && !skipped;
                var files = System.Text.RegularExpressions.Regex.Match(rest, @"(\d+) files?\b");
                // Put the remote NAME in the file column when the line has one, so the column reads
                // the same for early and final sends and a reader can scan the progression of names
                // down the page instead of hunting for them in the reason text.
                // Name the actual manifest - ".idx" as well as the stamped ".txt". Matching only
                // ".txt" left every index row reading "(index + host)" while the host
                // rows named their file.
                var midName = System.Text.RegularExpressions.Regex.Match(rest, @"\bas (\S+?\.(?:txt|idx))\b");
                evs.Add(new Ev(when, pid,
                    midName.Success ? midName.Groups[1].Value : "(index + host)",
                    // The count belongs IN the event text: "early host sent (5 files)" reads as one
                    // fact, where a separate column made you look across to see how much of the
                    // panel that send covered.
                    sent ? (kind.Length > 0 ? $"early {kind} sent{Cnt(files)}" : $"early send done{Cnt(files)}")
                         : errored ? (kind.Length > 0 ? $"early {kind} send FAILED{Cnt(files)}" : $"early send FAILED{Cnt(files)}")
                                   : "early send skipped",
                    sent ? "ok" : errored ? "failed" : "skipped",
                    // The TRY column stays empty for panel events: a manifest send is not a numbered
                    // attempt, and putting the file count here made "7" read as "attempt 7" when it
                    // meant "7 files". The count is in the event text now.
                    "",
                    HostIn(rest),
                    ReasonOnly(rest), "panel event", 3));
                continue;
            }

            // FINALIZE: the panel's real manifests going out, and with stamping on the only event
            // that names the COMPLETE host file. It was written to the panel-events log but never
            // parsed here, so the timeline said nothing about the very file the panel exists to
            // produce — a reader saw the early partial sends and then nothing, while the server had
            // the finished manifest all along.
            var finl = System.Text.RegularExpressions.Regex.Match(body, @"^FINALIZE\s+(\S+?):\s*(.+)$");
            if (finl.Success)
            {
                var fpid = finl.Groups[1].Value;
                if (!ownPids.Contains(fpid)) continue;
                var frest = finl.Groups[2].Value;
                var fname = System.Text.RegularExpressions.Regex.Match(frest, @"\bas (\S+\.txt)\b");
                var fkind = frest.StartsWith("final host", StringComparison.OrdinalIgnoreCase) ? "host"
                          : frest.StartsWith("final index", StringComparison.OrdinalIgnoreCase) ? "index" : "";
                var fidx = System.Text.RegularExpressions.Regex.Match(frest, @"\bas (\S+\.idx)\b");
                var fcount = System.Text.RegularExpressions.Regex.Match(frest, @"(\d+) files?\b");
                evs.Add(new Ev(when, fpid,
                    fname.Success ? fname.Groups[1].Value : fidx.Success ? fidx.Groups[1].Value : "(index + host)",
                    fkind.Length > 0 ? $"final {fkind} sent{Cnt(fcount)}" : $"final send{Cnt(fcount)}",
                    "ok", "", HostIn(frest), "", "panel event", 3));
                continue;
            }

            var gone = System.Text.RegularExpressions.Regex.Match(body, @"^SOURCE GONE\s+(\S+?)/(\S+?)\s*-\s*(.+)$");
            if (gone.Success)
            {
                if (!ownPids.Contains(gone.Groups[1].Value)) continue;
                evs.Add(new Ev(when, gone.Groups[1].Value, gone.Groups[2].Value,
                    "source file no longer exists", "gone", "", "", gone.Groups[3].Value, "panel event", 3));
                continue;
            }

            var pump = System.Text.RegularExpressions.Regex.Match(body, @"^PUMP ERROR on (\S+?)/(\S+?):\s*(.+?)\s*(?:—|-)\s*recovered");
            if (pump.Success)
            {
                if (!ownPids.Contains(pump.Groups[1].Value)) continue;
                evs.Add(new Ev(when, pump.Groups[1].Value, pump.Groups[2].Value,
                    "transfer threw an error, pump recovered", "failed", "", "", pump.Groups[3].Value, "panel event", 3));
                continue;
            }

            var warn = System.Text.RegularExpressions.Regex.Match(body, @"^(SEED SKIPPED|MARKER WRITE FAILED)\s+(\S+?):\s*(.+)$");
            if (warn.Success)
            {
                if (!ownPids.Contains(warn.Groups[2].Value.TrimEnd(':'))) continue;
                evs.Add(new Ev(when, warn.Groups[2].Value.TrimEnd(':'),
                    warn.Groups[1].Value == "SEED SKIPPED" ? "(whole panel)" : "(finalize marker)",
                    warn.Groups[1].Value == "SEED SKIPPED"
                        ? "no source files - manifests not written"
                        : "finalize marker could not be written",
                    "warn", "", "", warn.Groups[3].Value, "panel event", 3));
            }
        }

        // Within one second the log cannot say what came first, so order by SOURCE: the NG retry (1)
        // precedes the pump row it produces (2), and panel events (3) follow the transfer that
        // triggered them. Sorted HERE, once, so the CSV and the Trace Panel share the same order.
        return evs.OrderBy(x => x.When, StringComparer.Ordinal)
                  .ThenBy(x => x.Order)
                  .ThenBy(x => x.Pid, StringComparer.OrdinalIgnoreCase)
                  .ToList();
    }

    private static string? WriteCsv(Config cfg, string day, List<Ev> evs)
    {
        var sb = new StringBuilder(evs.Count * 110);
        sb.AppendLine("date,time,PID,file,event,result,try,host,source,reason");
        foreach (var e in evs)
            sb.Append(Csv(e.When.Length >= 19 ? e.When[..10] : e.When)).Append(',')
              // "06-58-17.613", not "06:58:17.613". Excel reads a colon-separated time as a NUMBER,
              // converts it, and applies its own default format — which renders 06:58:17.613 as
              // "58:17.6", hiding the very milliseconds this column exists to show. Dashes keep it
              // text, so it opens readable with no column formatting. The Trace Panel timeline is
              // plain text, not a spreadsheet, and keeps the colons.
              .Append(e.When.Length >= 19 ? e.When[11..].Replace(':', '-') : "").Append(',')
              .Append(Csv(e.Pid)).Append(',')
              .Append(Csv(e.File)).Append(',')
              .Append(Csv(e.Event)).Append(',')
              .Append(Csv(e.Result)).Append(',')
              .Append(Csv(e.Try)).Append(',')
              .Append(Csv(e.Host)).Append(',')
              .Append(Csv(e.Source)).Append(',')
              .AppendLine(Csv(e.Reason));

        var outPath = cfg.OperationReportPath(day);
        try
        {
            Directory.CreateDirectory(cfg.LogFullPath);
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(true));
            return outPath;
        }
        catch (Exception ex)
        {
            // Locked by a reader (Excel holds a CSV open exclusively). Hand back what is on disk so
            // the caller can still show something, but SAY SO: silently returning the old path made
            // "--reports" print "built:" while writing nothing, and the stale file then looked like
            // fresh output. "No data for this day" and "could not overwrite" are different answers.
            if (File.Exists(outPath))
            {
                Console.WriteLine($"  ! {Path.GetFileName(outPath)} is open in another program - " +
                                  $"kept the existing file from {File.GetLastWriteTime(outPath):yyyy-MM-dd HH:mm:ss} " +
                                  $"({ex.GetType().Name})");
                return outPath;
            }
            return null;
        }
    }

    /// <summary>
    /// The given day's file, then every LATER day's, for one kind of log.
    ///
    /// A panel does not finish when the day does. After a rollover its retries and manifest sends
    /// are written to the new day's files, so a report built from one day alone stops mid-story —
    /// a panel's second early-manifest send simply vanished. Ordered oldest-first; the caller
    /// filters to the panels that belong to the starting day.
    /// </summary>
    private static IEnumerable<string> DayFilesFrom(Config cfg, string day, Func<string, string> pathFor)
    {
        var days = new SortedSet<string>(StringComparer.Ordinal) { day };
        try
        {
            foreach (var f in Directory.GetFiles(cfg.LogFullPath))
            {
                var n = Path.GetFileName(f);
                if (n.Length >= 8 && n[..8].All(char.IsDigit) &&
                    string.CompareOrdinal(n[..8], day) >= 0) days.Add(n[..8]);
            }
        }
        catch { }
        foreach (var d in days)
        {
            var p = pathFor(d);
            if (File.Exists(p)) yield return p;
        }
    }

    /// <summary>
    /// What the reason column should carry for a panel-event row: the part the other columns do
    /// NOT already say. The event, file, host and count each have a column, so repeating the whole
    /// log line there doubled every row's width and buried the real failures in the noise.
    /// </summary>
    private static string ReasonOnly(string rest)
    {
        // A transport error follows " - " at the end; a skip reason follows "NOT sent".
        var dash = rest.LastIndexOf(" - ", StringComparison.Ordinal);
        if (dash > 0 && dash + 3 < rest.Length) return rest[(dash + 3)..].Trim();
        var idx = rest.IndexOf("NOT sent", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) return rest[(idx + 8)..].TrimStart(' ', '-');
        return "";
    }

    /// <summary>" (N files)" when the match found a count, otherwise nothing.</summary>
    private static string Cnt(System.Text.RegularExpressions.Match m) =>
        m.Success ? $" ({m.Groups[1].Value} file{(m.Groups[1].Value == "1" ? "" : "s")})" : "";

    /// <summary>Best clock time from a legacy row that carries no date: succeeded, else last fail.</summary>
    private static string ClockOf(string[] p, string dayPrefix)
    {
        if (p.Length > 3 && p[3].Length == 8) return p[3];
        if (p.Length > 5 && p[5].Length >= 8) { var f = p[5].Split(','); return f[^1].Trim(); }
        return "00:00:00";
    }

    private static string HostIn(string s)
    {
        var m = System.Text.RegularExpressions.Regex.Match(s, @"->\s*([0-9A-Za-z_.\-]+)");
        return m.Success ? m.Groups[1].Value : "";
    }

    /// <summary>Quote a CSV field only when it needs it, and double any embedded quotes.</summary>
    private static string Csv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var needs = s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        s = s.Replace("\r", " ").Replace("\n", " ");
        return needs ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }
}
