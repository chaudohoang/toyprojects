using System.Text;

namespace FtpUpload;

/// <summary>
/// In-process HTML report builder — the C# equivalent of _htmllog.ps1 / _nghtmllog.ps1, so the
/// "View Log" buttons don't need PowerShell. Same parsing and (near-identical) markup/CSS as the
/// scripts. The scripts are kept for one-click / scheduled use; this is what the app calls.
/// </summary>
public static class HtmlLog
{
    private static string Enc(string? s) =>
        (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    /// <summary>A small "(NN.N%)" of n out of total, or "" when total is 0.</summary>
    private static string Pct(int n, int total) =>
        total > 0 ? $"<span class='pct'>{(n * 100.0 / total):0.#}%</span>" : "";

    private static string Role(string ip, string primary, string secondary)
    {
        if (!string.IsNullOrEmpty(ip) && ip == primary) return "Primary";
        if (!string.IsNullOrEmpty(ip) && ip == secondary) return "Secondary";
        return string.IsNullOrEmpty(ip) ? "?" : ip;
    }

    private sealed class FileEntry
    {
        public string Pid = "", File = "";
        public readonly List<(bool Ok, string Ip, string Time)> Events = new();
        public string Status = "", Succeed = "", FailTimes = "";
        public int Attempts;
        public int MaxRetries;
        public bool Recovered;
        public bool PendingManifest;   // an index/host row injected for a panel still in NG (not yet sent)
        public string LastTime = "";
        /// <summary>11th rawlog field. "SOURCE_GONE" = the local file was deleted, so no retry can
        /// ever help — worth showing differently from a transfer that failed.</summary>
        public string Reason = "";
    }

    // =====================================================================================
    //  Day upload report (rawlog + jobs + snapshot)  ->  {day}_htmllog.html
    //  Returns the output path, or null if there was nothing to report.
    // =====================================================================================
    public static string? BuildDayLog(Config cfg, string day)
    {
        var raw = cfg.RawLogPathForDay(day);
        var jobs = cfg.JobsPathForDay(day);
        if (!File.Exists(raw) && !File.Exists(jobs)) return null;

        string primary = cfg.PrimaryHost, secondary = cfg.SecondaryHost;
        // Retries now come from the single RetryCount (uploads target one host, no failover), so the
        // old PrimaryRetries + SecondaryRetries sum would report a ceiling the engine no longer uses.
        var maxRetriesCfg = Math.Max(0, cfg.MaxAttempts - 1);

        var order = new List<string>();
        var byKey = new Dictionary<string, FileEntry>();

        // Seed the full file list from the jobs file (so not-yet-attempted files show as Pending).
        if (File.Exists(jobs))
            foreach (var line in SafeReadLines(jobs))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
                if (p.Length < 2) continue;
                var key = p[0] + "|" + p[1];
                if (!byKey.ContainsKey(key))
                {
                    byKey[key] = new FileEntry { Pid = p[0], File = p[1], MaxRetries = maxRetriesCfg };
                    order.Add(key);
                }
            }

        // Overlay the rawlog (per-attempt), keeping every host-bearing attempt in order.
        if (File.Exists(raw))
            foreach (var line in SafeReadLines(raw))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
                if (p.Length < 8) continue;
                var key = p[0] + "|" + p[1];
                if (!byKey.TryGetValue(key, out var e))
                {
                    e = new FileEntry { Pid = p[0], File = p[1] };
                    byKey[key] = e;
                    order.Add(key);
                }
                var ip = p.Length >= 9 ? p[8] : "";
                if (!string.IsNullOrEmpty(ip))
                    e.Events.Add((p[2] == "SUCCEEDED", ip, ""));
                e.Status = p[2];
                e.Succeed = p[3];
                e.FailTimes = p[5];
                e.Attempts = int.TryParse(p[6], out var at) ? at : 0;
                e.MaxRetries = Math.Max(0, (int.TryParse(p[7], out var ma) ? ma : 1) - 1);
            if (p.Length >= 11) e.Reason = p[10];
            }

        // Summary
        int tot = order.Count, ok = 0, fail = 0, pend = 0, timeout = 0;
        // Per-host tallies taken from the LOG, not from the current config: keying them to the
        // configured Primary/Secondary made a report read 0/0 as soon as the hosts were changed,
        // because the logged IP matched neither name any more.
        var hostStats = new Dictionary<string, (int Ok, int Fail)>();
        foreach (var k in order)
        {
            var e = byKey[k];
            switch (e.Status) { case "SUCCEEDED": ok++; break; case "FAILED": fail++; break; case "TIMEDOUT": timeout++; break; default: pend++; break; }

            // Count each FILE once, under the host of its LAST attempt, by its FINAL status — not
            // once per attempt. Per-attempt counting made these cards exceed the file total (a file
            // needing 3 tries added 3) and counted a file as failed while it was still retrying.
            // A file is a failure only once every attempt is spent and it still has not landed.
            if (e.Events.Count > 0)
            {
                var ip = e.Events[^1].Ip;
                if (!string.IsNullOrEmpty(ip))
                {
                    hostStats.TryGetValue(ip, out var cur);
                    hostStats[ip] = e.Status == "SUCCEEDED" ? (cur.Ok + 1, cur.Fail) : (cur.Ok, cur.Fail + 1);
                }
            }
        }

        // Group into panels (PID)
        var panelOrder = new List<string>();
        var panels = new Dictionary<string, List<string>>();
        foreach (var k in order)
        {
            var pid = byKey[k].Pid;
            if (!panels.TryGetValue(pid, out var l)) { l = new List<string>(); panels[pid] = l; panelOrder.Add(pid); }
            l.Add(k);
        }

        var cards = new StringBuilder();
        var panelsComplete = 0;
        foreach (var pid in panelOrder)
        {
            var files = panels[pid].Select(k => byKey[k]).ToList();
            int total = files.Count;
            int nSucc = files.Count(f => f.Status == "SUCCEEDED");
            int nFail = files.Count(f => f.Status == "FAILED");
            int nTO = files.Count(f => f.Status == "TIMEDOUT");
            int nPend = total - nSucc - nFail - nTO;

            string ovText, ovCls;
            if (nPend > 0) { ovText = "In Progress"; ovCls = "pend"; }
            else if (nSucc == total) { ovText = "Success"; ovCls = "ok"; }
            else if (nTO > 0) { ovText = "Timed Out"; ovCls = "to"; }
            else { ovText = "Failed"; ovCls = "bad"; }

            var frows = new StringBuilder();
            foreach (var e in files) frows.Append(FileRow(e, primary, secondary));

            // Statuses present in this panel, so the summary-card filters can show/hide it.
            var stTokens = new List<string>();
            if (nSucc > 0) stTokens.Add("SUCCEEDED");
            if (nFail > 0) stTokens.Add("FAILED");
            if (nTO > 0) stTokens.Add("TIMEDOUT");
            if (nPend > 0) stTokens.Add("PENDING");
            var stAttr = string.Join(" ", stTokens);

            // A panel counts as complete only when every one of its files landed — the figure
            // behind the percentage on the Panels card.
            if (total > 0 && nSucc == total) panelsComplete++;

            cards.Append($@"
<div class='panel' data-statuses='{stAttr}'>
  <div class='phead'>
    <span class='ppid'>{Enc(pid)}</span>
    <span class='ptally'>{nSucc}/{total} succeeded &middot; {nFail + nTO} failed</span>
    <span class='b {ovCls} pov'>{ovText}</span>
  </div>
  <table class='ptable'>
    <thead><tr><th>File</th><th>Status</th><th>Succeeded</th><th>Failed at</th><th>Retries</th><th>Attempts (IP &amp; outcome)</th></tr></thead>
    <tbody>{frows}</tbody>
  </table>
</div>");
        }

        // Snapshot section (optional)
        var snapPath = Path.Combine(cfg.LogFullPath, $"{day}_snapshot.txt");
        var snapRows = new StringBuilder();
        if (File.Exists(snapPath))
            foreach (var line in SafeReadLines(snapPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
                if (p.Length < 3) continue;
                var ov = p[2] == "O" ? "<span class='b ok'>O</span>" : "<span class='b bad'>X</span>";
                snapRows.Append($"<tr><td class='t'>{Enc(p[0])}</td><td class='pid'>{Enc(p[1])}</td><td>{ov}</td></tr>");
            }
        var snapSection = snapRows.Length == 0 ? "" : $@"
<h2>Result-timing snapshots</h2>
<table class='snap'>
  <thead><tr><th>Time</th><th>PID</th><th>Overall</th></tr></thead>
  <tbody>{snapRows}</tbody>
</table>";

        var generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        // Which machine produced this day's log (from its oplog), not who is viewing the report.
        var client = ClientFor(cfg, day);
        var clientTag = client.Length > 0 ? $" &nbsp;&middot;&nbsp; client {Enc(client)}" : "";
        var pTag = string.IsNullOrEmpty(primary) ? "" : $" ({Enc(primary)})";
        var sTag = string.IsNullOrEmpty(secondary) ? "" : $" ({Enc(secondary)})";

        // One card per host that actually appears in the log, named by its role where the current
        // config still recognises the IP, otherwise by the IP itself.
        var hostSb = new StringBuilder();
        foreach (var h in hostStats.Keys.OrderBy(x => x))
        {
            var role = Role(h, primary, secondary);
            var label = role is "Primary" or "Secondary" ? $"{role} ok / fail ({Enc(h)})" : $"{Enc(h)} ok / fail";
            hostSb.AppendLine($"  <div class='card'><div class='n'>{hostStats[h].Ok}&nbsp;/&nbsp;{hostStats[h].Fail}</div><div class='l'>{label}</div></div>");
        }
        if (hostSb.Length == 0)
            hostSb.AppendLine("  <div class='card'><div class='n'>0&nbsp;/&nbsp;0</div><div class='l'>no host recorded</div></div>");
        var hostCards = hostSb.ToString().TrimEnd();

        var html = $@"<!doctype html>
<html><head><meta charset='utf-8'>
<title>FTP Upload log {day}</title>
<style>{DayCss}</style></head><body>
<div class='hdr'>
  <div class='hdrleft'>
  <h1>FTP Upload &mdash; {day}</h1>
  <div class='sub'>from {Enc(raw)} &nbsp;&middot;&nbsp; generated {generated}{clientTag}</div>
  </div>
  <div class='pidbar'>
    <span class='pidlbl'>PID</span>
    <span class='pidwrap'>
      <input id='pidFilter' type='text' placeholder='filter by PID' autocomplete='off'/>
      <span id='pidClear' title='Clear'>&#215;</span>
    </span>
  </div>
</div>
<div class='cards'>
  <div class='card'><div class='n'>{panelOrder.Count}{Pct(panelsComplete, panelOrder.Count)}</div><div class='l'>Panels succeeded</div></div>
  <div class='card'><div class='n'>{tot}</div><div class='l'>Files</div></div>
  <div class='card clickable' data-filter='SUCCEEDED'><div class='n ok'>{ok}{Pct(ok, tot)}</div><div class='l'>Succeeded</div></div>
  <div class='card clickable' data-filter='FAILED'><div class='n bad'>{fail}{Pct(fail, tot)}</div><div class='l'>Failed</div></div>
  <div class='card clickable' data-filter='TIMEDOUT'><div class='n to'>{timeout}{Pct(timeout, tot)}</div><div class='l'>Timed out</div></div>
  <div class='card clickable' data-filter='PENDING'><div class='n pend'>{pend}{Pct(pend, tot)}</div><div class='l'>Pending</div></div>
{hostCards}
</div>
<div class='fhint'>Click <b>Succeeded / Failed / Timed out / Pending</b> to show only the matching files (panels with none are hidden; click several to combine; click again to clear). <span id='fcount'></span></div>
<div class='legend'>Attempts:
  <span class='chip ok pri'>Primary &#10003;</span>
  <span class='chip bad pri'>Primary &#10007;</span>
  <span class='chip ok sec'>Secondary &#10003;</span>
  <span class='chip bad sec'>Secondary &#10007;</span>
  &nbsp;(blue border = primary IP, amber = secondary; green = uploaded, red = failed)
</div>
<h2>Files</h2>
<div id='noMatch' style='display:none;padding:14px;background:#fff;border:1px solid #ECEFF5;border-radius:10px;color:#8891A3;font-size:12.5px;'>No panels match the current filter.</div>
{cards}
{snapSection}
<script>
(function(){{
  var active = new Set();
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fcount = document.getElementById('fcount');
  var pidBox = document.getElementById('pidFilter');
  // PID filter composes with the status cards: a panel must match BOTH to stay visible.
  function pidOk(p){{
    if (!pidBox) return true;
    var q = pidBox.value.trim().toUpperCase();
    if (!q) return true;
    var el = p.querySelector('.ppid');
    return el && el.textContent.toUpperCase().indexOf(q) !== -1;
  }}
  function apply(){{
    var shown = 0;
    panels.forEach(function(p){{
      if (!pidOk(p)) {{ p.style.display = 'none'; return; }}
      var rows = p.querySelectorAll('tbody tr');
      if (active.size === 0) {{
        // no filter: show every panel and every row
        p.style.display = '';
        rows.forEach(function(r){{ r.style.display = ''; }});
        shown++;
        return;
      }}
      // filter on: show only rows whose status is selected, and hide panels with no match
      var any = false;
      rows.forEach(function(r){{
        var match = active.has(r.getAttribute('data-status'));
        r.style.display = match ? '' : 'none';
        if (match) any = true;
      }});
      p.style.display = any ? '' : 'none';
      if (any) shown++;
    }});
      // Count reflects BOTH filters, and an empty result says so instead of leaving a blank page.
      var filtering = active.size > 0 || (pidBox && pidBox.value.trim() !== '');
      if (fcount) fcount.textContent = filtering
        ? ('Showing ' + shown + ' of ' + panels.length + ' panels') : '';
      var nm = document.getElementById('noMatch');
      if (nm) nm.style.display = (filtering && shown === 0) ? 'block' : 'none';
  }}
  cards.forEach(function(c){{
    c.addEventListener('click', function(){{
      var f = c.getAttribute('data-filter');
      if (active.has(f)) {{ active.delete(f); c.classList.remove('active'); }}
      else {{ active.add(f); c.classList.add('active'); }}
      apply();
    }});
    }});
    var pidClear = document.getElementById('pidClear');
  function pidSync(){{ if (pidClear && pidBox) pidClear.style.display = pidBox.value ? 'block' : 'none'; }}
  if (pidBox) pidBox.addEventListener('input', function(){{ pidSync(); apply(); }});
  if (pidClear) pidClear.addEventListener('click', function(){{ pidBox.value = ''; pidSync(); apply(); pidBox.focus(); }});
  pidSync();
    var pidClear = document.getElementById('pidClear');
    if (pidClear) pidClear.addEventListener('click', function(){{ if (pidBox) {{ pidBox.value = ''; apply(); }} }});
}})();
</script>
</body></html>";

        var outPath = Path.Combine(cfg.LogFullPath, $"{day}_htmllog.html");
        Directory.CreateDirectory(cfg.LogFullPath);
        File.WriteAllText(outPath, html, Encoding.UTF8);
        return outPath;
    }

    private static string FileRow(FileEntry e, string primary, string secondary)
    {
        var badge = e.Status switch
        {
            "SUCCEEDED" => "<span class='b ok'>Succeeded</span>",
            "FAILED" => e.Reason == "SOURCE_GONE"
                    ? "<span class='b gone'>Source gone</span>"
                    : "<span class='b bad'>Failed</span>",
            "TIMEDOUT" => "<span class='b to'>Timed Out</span>",
            _ => "<span class='b pend'>Pending</span>"
        };
            // Retries used, split between the live pump and NG.
            //
            // Live attempts can NEVER exceed MaxAttempts - that is what sends a file to NG - so any
            // excess must have come from NG, whose retries are deliberately uncapped. Showing the
            // total against the live ceiling produced "6 / 3", a ratio that cannot be true.
            // Attribute the excess instead: "3 / 3 +3 NG".
            var used = Math.Max(0, e.Attempts - 1);
            var retryCell = used > e.MaxRetries
                ? $"{e.MaxRetries} / {e.MaxRetries} <span class='ngx'>+{used - e.MaxRetries} NG</span>"
                : $"{used} / {e.MaxRetries}";

        var ftList = string.IsNullOrEmpty(e.FailTimes)
            ? new List<string>()
            : e.FailTimes.Split(',').Where(t => t.Length > 0).ToList();

        var fp = 0;
        var succLine = "";
        var failLines = new List<string>();
        var chips = new StringBuilder();
        var i = 0;
        foreach (var ev in e.Events)
        {
            i++;
            var r = Role(ev.Ip, primary, secondary);
            var rc = r == "Primary" ? "pri" : r == "Secondary" ? "sec" : "oth";
            string oc, mk;
            if (ev.Ok)
            {
                oc = "ok"; mk = "&#10003;";
                if (!string.IsNullOrEmpty(e.Succeed)) succLine = $"{i}.&nbsp;{Enc(e.Succeed)}";
            }
            else
            {
                oc = "bad"; mk = "&#10007;";
                var t = fp < ftList.Count ? ftList[fp] : "";
                fp++;
                if (!string.IsNullOrEmpty(t)) failLines.Add($"{i}.&nbsp;{Enc(t)}");
            }
            chips.Append($"<span class='chip {oc} {rc}' title='attempt {i} via {Enc(ev.Ip)}'>{i}&nbsp;{r}&nbsp;{mk}</span> ");
        }

        var succ = string.IsNullOrEmpty(succLine) ? "&mdash;" : succLine;
        var ft = failLines.Count > 0 ? string.Join("<br>", failLines) : "&mdash;";
        var rowStatus = string.IsNullOrEmpty(e.Status) ? "PENDING" : e.Status;

        return $@"<tr data-status='{rowStatus}'>
  <td class='file'>{Enc(e.File)}</td>
  <td>{badge}</td>
  <td class='t'>{succ}</td>
  <td class='t'>{ft}</td>
  <td class='r'>{retryCell}</td>
  <td class='chips'>{chips}</td>
</tr>";
    }

    // =====================================================================================
    //  NG-retry report (ngretrylog + rawlog for the original reason)  ->  {day}_nghtmllog.html
    // =====================================================================================
    /// <summary>Row order within a panel: data files (0), then index manifest (1), then host (2).</summary>
    private static int NgManifestRank(string file, string pid)
    {
        if (file.EndsWith(".idx", StringComparison.OrdinalIgnoreCase)) return 1;
        if (file.StartsWith(pid + "_", StringComparison.OrdinalIgnoreCase) &&
            file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) return 2;
        return 0;
    }

    private static string NgRow(FileEntry e, string reason, string primary, string secondary)
    {
        var rBadge = e.PendingManifest
            ? "<span class='b pend'>Pending</span>"
            : reason switch
            {
                "TIMEDOUT" => "<span class='b to'>Timed Out</span>",
                "FAILED" => e.Reason == "SOURCE_GONE"
                    ? "<span class='b gone'>Source gone</span>"
                    : "<span class='b bad'>Failed</span>",
                _ => "<span class='b pend'>&mdash;</span>"
            };
        var state = e.PendingManifest ? "<span class='b pend'>Pending</span>"
            : e.Recovered ? "<span class='b ok'>Recovered</span>" : "<span class='b bad'>Still failing</span>";
        var stateAttr = e.PendingManifest ? "PENDING" : e.Recovered ? "RECOVERED" : "FAILING";

        var chips = new StringBuilder();
        var i = 0;
        foreach (var ev in e.Events)
        {
            i++;
            var r = Role(ev.Ip, primary, secondary);
            var oc = ev.Ok ? "ok" : "bad";
            var rc = r == "Primary" ? "pri" : r == "Secondary" ? "sec" : "oth";
            var mk = ev.Ok ? "&#10003;" : "&#10007;";
            var tt = string.IsNullOrEmpty(ev.Time) ? "" : " " + Enc(ev.Time);
            chips.Append($"<span class='chip {oc} {rc}' title='retry {i} via {Enc(ev.Ip)}{tt}'>{i}&nbsp;{r}&nbsp;{mk}</span> ");
        }

        return $@"<tr data-state='{stateAttr}'>
  <td class='file'>{Enc(e.File)}</td>
  <td>{rBadge}</td>
  <td>{state}</td>
  <td class='r'>{e.Events.Count}</td>
  <td class='t'>{Enc(e.LastTime)}</td>
  <td class='chips'>{chips}</td>
</tr>";
    }

    public static string? BuildNgLog(Config cfg, string day)
    {
        var ngPath = cfg.NgRetryLogPath(day);
        var rawPath = cfg.RawLogPathForDay(day);
        if (!File.Exists(ngPath)) return null;

        string primary = cfg.PrimaryHost, secondary = cfg.SecondaryHost;

        // original reason (last status per file) from the raw log
        var orig = new Dictionary<string, string>();
        if (File.Exists(rawPath))
            foreach (var line in SafeReadLines(rawPath))
            {
                var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
                if (p.Length < 3) continue;
                orig[p[0] + "|" + p[1]] = p[2];
            }

        var order = new List<string>();
        var byKey = new Dictionary<string, FileEntry>();
        foreach (var line in SafeReadLines(ngPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
            if (p.Length < 3) continue;
            var key = p[0] + "|" + p[1];
            if (!byKey.TryGetValue(key, out var e))
            {
                e = new FileEntry { Pid = p[0], File = p[1] };
                byKey[key] = e; order.Add(key);
            }
            var okv = p[2] == "SUCCEEDED";
            var ip = p.Length >= 9 ? p[8] : "";
            var tm = p.Length >= 6 ? p[5] : "";
            e.Events.Add((okv, ip, tm));
            if (okv) e.Recovered = true;
            if (p.Length >= 6) e.LastTime = p[5];
        }

        int recovered = 0, failing = 0, pendingMan = 0, totRetries = 0;

        // Inject Pending rows for the index/host of any panel in this report whose manifests have
        // not been sent yet. "Sent" must be judged from BOTH logs: a manifest uploaded by the LIVE
        // pump appears only in the rawlog, so checking the ng-retry log alone reported it Pending
        // forever (seen on WB007.idx — SUCCEEDED in the rawlog at 10:13:46, no ng-retry row at all).
        var sentInRawLog = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in SafeReadLines(cfg.RawLogPathForDay(day)))
        {
            var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
            if (p.Length >= 3 && p[2] == "SUCCEEDED") sentInRawLog.Add(p[0] + "|" + p[1]);
        }

        var manifestNames = new Dictionary<string, List<string>>();
        foreach (var line in SafeReadLines(cfg.JobsPathForDay(day)))
        {
            var jl = JobsLine.Parse(line);
            if (jl is null || !jl.IsManifest) continue;
            if (!manifestNames.TryGetValue(jl.Pid, out var l)) { l = new(); manifestNames[jl.Pid] = l; }
            l.Add(jl.FileName);
        }
        var pidsInReport = order.Select(k => byKey[k].Pid).ToHashSet();
        foreach (var pid in pidsInReport)
        {
            if (!manifestNames.TryGetValue(pid, out var names)) continue;
            foreach (var name in names)
            {
                var key = pid + "|" + name;
                if (byKey.ContainsKey(key)) continue;     // already in the ng-retry log
                if (sentInRawLog.Contains(key)) continue; // sent by the live pump — not pending
                byKey[key] = new FileEntry { Pid = pid, File = name, PendingManifest = true };
                order.Add(key);
            }
        }

        foreach (var k in order)
        {
            var e = byKey[k];
            // Three distinct states. Lumping pending manifests in with failures made the header read
            // "100% recovered" and "N still failing" at once, and clicking Still failing showed
            // nothing, because those rows are tagged PENDING rather than FAILING.
            if (e.Recovered) recovered++;
            else if (e.PendingManifest) pendingMan++;
            else failing++;
            totRetries += e.Events.Count;
        }

        // Total AFTER injection, so the cards sum to it. Computing it beforehand excluded the
        // pending manifests, which made "155 recovered (100%)" sit next to "16 manifests pending"
        // under a total of 155 — the parts didn't add up to the whole.
        int tot = recovered + failing + pendingMan;

        // Group NG items into panel cards by PID (preserve first-seen order), like the day log.
        var panelOrder = new List<string>();
        var panels = new Dictionary<string, List<string>>();
        foreach (var k in order)
        {
            var pid = byKey[k].Pid;
            if (!panels.TryGetValue(pid, out var l)) { l = new List<string>(); panels[pid] = l; panelOrder.Add(pid); }
            l.Add(k);
        }

        var cards = new StringBuilder();
        var panelsComplete = 0;
        foreach (var pid in panelOrder)
        {
            // Order rows like the main rawlog / jobs file: data files first, then index, then host.
            var keys = panels[pid]
                .Select((k, i) => (k, i))
                .OrderBy(x => NgManifestRank(byKey[x.k].File, pid))
                .ThenBy(x => x.i)
                .Select(x => x.k)
                .ToList();
            int pRec = keys.Count(k => byKey[k].Recovered);
            int pPend = keys.Count(k => byKey[k].PendingManifest && !byKey[k].Recovered);
            int pFail = keys.Count - pRec - pPend;
            var ovText = pFail > 0 ? "Still failing" : pPend > 0 ? "Manifests pending" : "Recovered";
            var ovCls = pFail > 0 ? "bad" : pPend > 0 ? "pend" : "ok";
            var stTokens = new List<string>();
            if (pRec > 0) stTokens.Add("RECOVERED");
            if (pFail > 0) stTokens.Add("FAILING");
            if (pPend > 0) stTokens.Add("PENDING");
            var stAttr = string.Join(" ", stTokens);

            // A panel counts as fully recovered only when nothing is failing or still pending.
            if (keys.Count > 0 && pRec == keys.Count) panelsComplete++;

            var tally = $"{pRec} recovered";
            if (pPend > 0) tally += $" &middot; {pPend} manifest(s) pending";
            if (pFail > 0) tally += $" &middot; {pFail} still failing";

            var frows = new StringBuilder();
            foreach (var k in keys)
                frows.Append(NgRow(byKey[k], orig.TryGetValue(k, out var rs) ? rs : "", primary, secondary));

            cards.Append($@"
<div class='panel' data-states='{stAttr}'>
  <div class='phead'>
    <span class='ppid'>{Enc(pid)}</span>
    <span class='ptally'>{tally}</span>
    <span class='b {ovCls} pov'>{ovText}</span>
  </div>
  <table class='ptable'>
    <thead><tr><th>File</th><th>Original</th><th>Result</th><th>Retries</th><th>Last</th><th>Attempts (IP &amp; outcome)</th></tr></thead>
    <tbody>{frows}</tbody>
  </table>
</div>");
        }

        var generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var client = ClientFor(cfg, day);
        var clientTag = client.Length > 0 ? $" &nbsp;&middot;&nbsp; client {Enc(client)}" : "";
        var html = $@"<!doctype html>
<html><head><meta charset='utf-8'>
<title>NG-retry log {day}</title>
<style>{NgCss}</style></head><body>
<div class='hdr'>
  <div class='hdrleft'>
  <h1>NG-retry &mdash; {day}</h1>
  <div class='sub'>from {Enc(ngPath)} &nbsp;&middot;&nbsp; generated {generated}{clientTag}</div>
  </div>
  <div class='pidbar'>
    <span class='pidlbl'>PID</span>
    <span class='pidwrap'>
      <input id='pidFilter' type='text' placeholder='filter by PID' autocomplete='off'/>
      <span id='pidClear' title='Clear'>&#215;</span>
    </span>
  </div>
</div>
<div class='cards'>
  <div class='card'><div class='n'>{panelOrder.Count}{Pct(panelsComplete, panelOrder.Count)}</div><div class='l'>Panels recovered</div></div>
  <div class='card'><div class='n'>{tot}</div><div class='l'>Files</div></div>
  <div class='card clickable' data-filter='RECOVERED'><div class='n ok'>{recovered}{Pct(recovered, tot)}</div><div class='l'>Recovered</div></div>
  <div class='card clickable' data-filter='FAILING'><div class='n bad'>{failing}{Pct(failing, tot)}</div><div class='l'>Still failing</div></div>
  <div class='card clickable' data-filter='PENDING'><div class='n pend'>{pendingMan}{Pct(pendingMan, tot)}</div><div class='l'>Manifests pending</div></div>
  <div class='card'><div class='n pend'>{totRetries}</div><div class='l'>Total retries</div></div>
</div>
<div class='fhint'>Click <b>Recovered / Still failing</b> to show only the matching files (panels with none are hidden; click both to show all again, or click one to clear). <span id='fcount'></span></div>
<h2>NG-retry items</h2>
<div id='noMatch' style='display:none;padding:14px;background:#fff;border:1px solid #ECEFF5;border-radius:10px;color:#8891A3;font-size:12.5px;'>No panels match the current filter.</div>
{cards}
<script>
(function(){{
  var active = new Set();
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fcount = document.getElementById('fcount');
  var pidBox = document.getElementById('pidFilter');
  // PID filter composes with the status cards: a panel must match BOTH to stay visible.
  function pidOk(p){{
    if (!pidBox) return true;
    var q = pidBox.value.trim().toUpperCase();
    if (!q) return true;
    var el = p.querySelector('.ppid');
    return el && el.textContent.toUpperCase().indexOf(q) !== -1;
  }}
  function apply(){{
    var shown = 0;
    panels.forEach(function(p){{
      if (!pidOk(p)) {{ p.style.display = 'none'; return; }}
      var rows = p.querySelectorAll('tbody tr');
      if (active.size === 0) {{
        p.style.display = '';
        rows.forEach(function(r){{ r.style.display = ''; }});
        shown++;
        return;
      }}
      var any = false;
      rows.forEach(function(r){{
        var match = active.has(r.getAttribute('data-state'));
        r.style.display = match ? '' : 'none';
        if (match) any = true;
      }});
      p.style.display = any ? '' : 'none';
      if (any) shown++;
    }});
      // Count reflects BOTH filters, and an empty result says so instead of leaving a blank page.
      var filtering = active.size > 0 || (pidBox && pidBox.value.trim() !== '');
      if (fcount) fcount.textContent = filtering
        ? ('Showing ' + shown + ' of ' + panels.length + ' panels') : '';
      var nm = document.getElementById('noMatch');
      if (nm) nm.style.display = (filtering && shown === 0) ? 'block' : 'none';
  }}
  cards.forEach(function(c){{
    c.addEventListener('click', function(){{
      var f = c.getAttribute('data-filter');
      if (active.has(f)) {{ active.delete(f); c.classList.remove('active'); }}
      else {{ active.add(f); c.classList.add('active'); }}
      apply();
    }});
    }});
    var pidClear = document.getElementById('pidClear');
  function pidSync(){{ if (pidClear && pidBox) pidClear.style.display = pidBox.value ? 'block' : 'none'; }}
  if (pidBox) pidBox.addEventListener('input', function(){{ pidSync(); apply(); }});
  if (pidClear) pidClear.addEventListener('click', function(){{ pidBox.value = ''; pidSync(); apply(); pidBox.focus(); }});
  pidSync();
    var pidClear = document.getElementById('pidClear');
    if (pidClear) pidClear.addEventListener('click', function(){{ if (pidBox) {{ pidBox.value = ''; apply(); }} }});
}})();
</script>
</body></html>";

        var outPath = Path.Combine(cfg.LogFullPath, $"{day}_nghtmllog.html");
        Directory.CreateDirectory(cfg.LogFullPath);
        File.WriteAllText(outPath, html, Encoding.UTF8);
        return outPath;
    }

    private static IEnumerable<string> SafeReadLines(string path)
    {
        try { return File.ReadAllLines(path); }
        catch { return Array.Empty<string>(); }
    }

    /// <summary>
    /// "client MACHINE 10.119.211.42" for the machine that PRODUCED this day's log, taken from the
    /// last STARTUP line in that day's oplog — not from whoever happens to be viewing the report.
    /// Those differ whenever a log is copied off a line PC and opened elsewhere, and the producer is
    /// the one worth knowing. Falls back to this machine when the oplog has no STARTUP line.
    /// </summary>
    private static string ClientFor(Config cfg, string day)
    {
        try
        {
            string? found = null;
            foreach (var line in SafeReadLines(cfg.PanelEventsPath(ParseDay(day))))
            {
                var i = line.IndexOf("client ", StringComparison.OrdinalIgnoreCase);
                if (i < 0 || !line.Contains("STARTUP", StringComparison.OrdinalIgnoreCase)) continue;
                var rest = line[(i + 7)..].Trim();
                // Just the machine name and the IP; the rest of the STARTUP line is other detail.
                var tok = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tok.Length >= 2) found = tok[0] + " " + tok[1];
                else if (tok.Length == 1) found = tok[0];
            }
            if (!string.IsNullOrWhiteSpace(found)) return found!;
        }
        catch { }
        try { return NetInfo.Describe(cfg.FirstHost); } catch { return ""; }
    }

    private static DateTime ParseDay(string day)
        => DateTime.TryParseExact(day, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var d)
           ? d : DateTime.Today;

    private const string DayCss = @"
  html{overflow-y:scroll;}   /* reserve the scrollbar so filtering can't change the page width */
  .hdr{display:flex;justify-content:space-between;align-items:flex-start;gap:16px;}
  .hdrleft{min-width:0;}
  body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#14203C;background:#F4F6FA;}
  h1{font-size:22px;margin:0 0 2px;} h2{font-size:15px;margin:26px 0 10px;color:#3A4256;}
  .sub{color:#6B7386;font-size:12.5px;margin-bottom:18px;}
  .cards{display:flex;gap:12px;flex-wrap:wrap;margin-bottom:8px;}
  .card{background:#fff;border:1px solid #ECEFF5;border-radius:10px;padding:12px 16px;min-width:120px;}
  .card.clickable{cursor:pointer;user-select:none;transition:transform .05s ease,box-shadow .1s ease,border-color .1s ease;}
  .card.clickable:hover{box-shadow:0 3px 10px rgba(20,32,60,.13);transform:translateY(-1px);}
  .card.clickable.active{border-color:#4D8CFF;box-shadow:0 0 0 2px rgba(77,140,255,.35);}
  .fhint{font-size:11.5px;color:#8891A3;margin:2px 0 4px;}
  .pidbar{display:flex;align-items:center;gap:8px;flex:0 0 auto;}
  .ngx{color:#8A6BFF;font-weight:600;}
  .b.gone{background:#FFF4E5;color:#8A5A00;border:1px solid #F0D9A8;}
  .pidlbl{font-size:11px;font-weight:600;color:#6B7386;}
  .pidwrap{position:relative;display:inline-block;}
  .pidwrap input{padding:5px 26px 5px 10px;font-size:12px;border:1px solid #C9DAF8;border-radius:8px;width:210px;outline:none;}
  .pidwrap input:focus{border-color:#4D8CFF;}
  #pidClear{position:absolute;right:7px;top:50%;transform:translateY(-50%);cursor:pointer;color:#8A97BD;font-size:15px;line-height:1;display:none;user-select:none;}
  #pidClear:hover{color:#E0483F;}
  .fhint b{color:#6B7386;}
  .fhint #fcount{color:#4D8CFF;font-weight:600;margin-left:6px;}
  .card .n{font-size:22px;font-weight:700;} .card .l{font-size:11px;color:#8891A3;text-transform:uppercase;letter-spacing:.04em;}
  .card .n .pct{display:block;font-size:11px;font-weight:600;color:#9AA2B1;letter-spacing:0;margin-top:1px;}
  .n.ok{color:#1F9D55;} .n.bad{color:#E0483F;} .n.pend{color:#4D8CFF;} .n.to{color:#7C3AED;}
  table{width:100%;border-collapse:collapse;background:#fff;border:1px solid #ECEFF5;border-radius:10px;overflow:hidden;}
  th{font-size:10.5px;text-transform:uppercase;letter-spacing:.05em;color:#8891A3;text-align:left;padding:9px 12px;background:#F8F9FC;border-bottom:1px solid #ECEFF5;}
  td{padding:9px 12px;border-bottom:1px solid #F2F4F8;font-size:12.5px;vertical-align:top;}
  td.pid{font-weight:700;} td.file{font-weight:600;color:#3A4256;} td.t{color:#4A5268;} td.r{color:#4A5268;white-space:nowrap;}
  .panel{background:#fff;border:1px solid #ECEFF5;border-radius:12px;margin-bottom:14px;overflow:hidden;}
  .phead{display:flex;align-items:center;gap:14px;padding:12px 16px;border-bottom:1px solid #F2F4F8;background:#FCFDFF;}
  .phead .ppid{font-weight:700;font-size:14px;}
  .phead .ptally{color:#6B7386;font-size:12px;}
  .phead .pov{margin-left:auto;}
  .panel .ptable{border:0;border-radius:0;}
  .panel .ptable th{background:#F8F9FC;}
  .b{display:inline-block;padding:2px 9px;border-radius:8px;font-size:11px;font-weight:600;}
  .b.ok{background:#E4F7EA;color:#1F9D55;} .b.bad{background:#FDECEB;color:#E0483F;} .b.pend{background:#EEF0F4;color:#8891A3;}
  .b.to{background:#F3E8FF;color:#7C3AED;}
  .chips{line-height:2;}
  .chip{display:inline-block;padding:2px 8px;margin:0 2px 2px 0;border-radius:7px;font-size:11px;border:1px solid transparent;white-space:nowrap;}
  .chip.ok{background:#E4F7EA;color:#1F9D55;} .chip.bad{background:#FDECEB;color:#E0483F;}
  .chip.pri{border-color:#4D8CFF;} .chip.sec{border-color:#B8860B;}
  .legend{font-size:11.5px;color:#6B7386;margin:8px 0 0;}
  .legend .chip{cursor:default;}";

    private const string NgCss = @"
  html{overflow-y:scroll;}   /* reserve the scrollbar so filtering can't change the page width */
  .hdr{display:flex;justify-content:space-between;align-items:flex-start;gap:16px;}
  .hdrleft{min-width:0;}
  body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#14203C;background:#F4F6FA;}
  h1{font-size:22px;margin:0 0 2px;} h2{font-size:15px;margin:26px 0 10px;color:#3A4256;}
  .sub{color:#6B7386;font-size:12.5px;margin-bottom:18px;}
  .cards{display:flex;gap:12px;flex-wrap:wrap;margin-bottom:8px;}
  .card{background:#fff;border:1px solid #ECEFF5;border-radius:10px;padding:12px 16px;min-width:120px;}
  .card.clickable{cursor:pointer;user-select:none;transition:transform .05s ease,box-shadow .1s ease,border-color .1s ease;}
  .card.clickable:hover{box-shadow:0 3px 10px rgba(20,32,60,.13);transform:translateY(-1px);}
  .card.clickable.active{border-color:#4D8CFF;box-shadow:0 0 0 2px rgba(77,140,255,.35);}
  .fhint{font-size:11.5px;color:#8891A3;margin:2px 0 4px;}
  .pidbar{display:flex;align-items:center;gap:8px;flex:0 0 auto;}
  .ngx{color:#8A6BFF;font-weight:600;}
  .b.gone{background:#FFF4E5;color:#8A5A00;border:1px solid #F0D9A8;}
  .pidlbl{font-size:11px;font-weight:600;color:#6B7386;}
  .pidwrap{position:relative;display:inline-block;}
  .pidwrap input{padding:5px 26px 5px 10px;font-size:12px;border:1px solid #C9DAF8;border-radius:8px;width:210px;outline:none;}
  .pidwrap input:focus{border-color:#4D8CFF;}
  #pidClear{position:absolute;right:7px;top:50%;transform:translateY(-50%);cursor:pointer;color:#8A97BD;font-size:15px;line-height:1;display:none;user-select:none;}
  #pidClear:hover{color:#E0483F;}
  .fhint b{color:#6B7386;}
  .fhint #fcount{color:#4D8CFF;font-weight:600;margin-left:6px;}
  .card .n{font-size:22px;font-weight:700;} .card .l{font-size:11px;color:#8891A3;text-transform:uppercase;letter-spacing:.04em;}
  .card .n .pct{display:block;font-size:11px;font-weight:600;color:#9AA2B1;letter-spacing:0;margin-top:1px;}
  .n.ok{color:#1F9D55;} .n.bad{color:#E0483F;} .n.pend{color:#4D8CFF;} .n.to{color:#7C3AED;}
  table{width:100%;border-collapse:collapse;background:#fff;border:1px solid #ECEFF5;border-radius:10px;overflow:hidden;}
  th{font-size:10.5px;text-transform:uppercase;letter-spacing:.05em;color:#8891A3;text-align:left;padding:9px 12px;background:#F8F9FC;border-bottom:1px solid #ECEFF5;}
  td{padding:9px 12px;border-bottom:1px solid #F2F4F8;font-size:12.5px;vertical-align:top;}
  td.pid{font-weight:700;} td.file{font-weight:600;color:#3A4256;} td.t{color:#4A5268;} td.r{color:#4A5268;text-align:center;}
  .panel{background:#fff;border:1px solid #ECEFF5;border-radius:12px;margin-bottom:14px;overflow:hidden;}
  .phead{display:flex;align-items:center;gap:14px;padding:12px 16px;border-bottom:1px solid #F2F4F8;background:#FCFDFF;}
  .phead .ppid{font-weight:700;font-size:14px;}
  .phead .ptally{color:#6B7386;font-size:12px;}
  .phead .pov{margin-left:auto;}
  .panel .ptable{border:0;border-radius:0;}
  .panel .ptable th{background:#F8F9FC;}
  .b{display:inline-block;padding:2px 9px;border-radius:8px;font-size:11px;font-weight:600;}
  .b.ok{background:#E4F7EA;color:#1F9D55;} .b.bad{background:#FDECEB;color:#E0483F;} .b.pend{background:#EEF0F4;color:#8891A3;} .b.to{background:#F3E8FF;color:#7C3AED;}
  .chips{line-height:2;}
  .chip{display:inline-block;padding:2px 8px;margin:0 2px 2px 0;border-radius:7px;font-size:11px;border:1px solid transparent;white-space:nowrap;}
  .chip.ok{background:#E4F7EA;color:#1F9D55;} .chip.bad{background:#FDECEB;color:#E0483F;}
  .chip.pri{border-color:#4D8CFF;} .chip.sec{border-color:#B8860B;}";
}
