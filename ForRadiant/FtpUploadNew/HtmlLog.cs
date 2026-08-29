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
        var maxRetriesCfg = Math.Max(0, cfg.PrimaryRetries) + Math.Max(0, cfg.SecondaryRetries);

        var order = new List<string>();
        var byKey = new Dictionary<string, FileEntry>();

        // Seed the full file list from the jobs file (so not-yet-attempted files show as Pending).
        if (File.Exists(jobs))
            foreach (var line in SafeReadLines(jobs))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var p = line.Split('|');
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
                var p = line.Split('|');
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
            }

        // Summary
        int tot = order.Count, ok = 0, fail = 0, pend = 0, timeout = 0;
        int priOk = 0, priFail = 0, secOk = 0, secFail = 0;
        foreach (var k in order)
        {
            var e = byKey[k];
            switch (e.Status) { case "SUCCEEDED": ok++; break; case "FAILED": fail++; break; case "TIMEDOUT": timeout++; break; default: pend++; break; }
            foreach (var ev in e.Events)
            {
                var r = Role(ev.Ip, primary, secondary);
                if (r == "Primary") { if (ev.Ok) priOk++; else priFail++; }
                else if (r == "Secondary") { if (ev.Ok) secOk++; else secFail++; }
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
                var p = line.Split('|');
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
        var pTag = string.IsNullOrEmpty(primary) ? "" : $" ({Enc(primary)})";
        var sTag = string.IsNullOrEmpty(secondary) ? "" : $" ({Enc(secondary)})";

        var html = $@"<!doctype html>
<html><head><meta charset='utf-8'>
<title>FTP Upload log {day}</title>
<style>{DayCss}</style></head><body>
<h1>FTP Upload &mdash; {day}</h1>
<div class='sub'>from {Enc(raw)} &nbsp;&middot;&nbsp; generated {generated}</div>
<div class='cards'>
  <div class='card'><div class='n'>{tot}</div><div class='l'>Files</div></div>
  <div class='card clickable' data-filter='SUCCEEDED'><div class='n ok'>{ok}{Pct(ok, tot)}</div><div class='l'>Succeeded</div></div>
  <div class='card clickable' data-filter='FAILED'><div class='n bad'>{fail}{Pct(fail, tot)}</div><div class='l'>Failed</div></div>
  <div class='card clickable' data-filter='TIMEDOUT'><div class='n to'>{timeout}{Pct(timeout, tot)}</div><div class='l'>Timed out</div></div>
  <div class='card clickable' data-filter='PENDING'><div class='n pend'>{pend}{Pct(pend, tot)}</div><div class='l'>Pending</div></div>
  <div class='card'><div class='n'>{priOk}&nbsp;/&nbsp;{priFail}</div><div class='l'>Primary ok / fail{pTag}</div></div>
  <div class='card'><div class='n'>{secOk}&nbsp;/&nbsp;{secFail}</div><div class='l'>Secondary ok / fail{sTag}</div></div>
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
{cards}
{snapSection}
<script>
(function(){{
  var active = new Set();
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fcount = document.getElementById('fcount');
  function apply(){{
    var shown = 0;
    panels.forEach(function(p){{
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
    if (fcount) fcount.textContent = active.size === 0
      ? '' : ('Showing ' + shown + ' of ' + panels.length + ' panels (matching rows only)');
  }}
  cards.forEach(function(c){{
    c.addEventListener('click', function(){{
      var f = c.getAttribute('data-filter');
      if (active.has(f)) {{ active.delete(f); c.classList.remove('active'); }}
      else {{ active.add(f); c.classList.add('active'); }}
      apply();
    }});
  }});
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
            "FAILED" => "<span class='b bad'>Failed</span>",
            "TIMEDOUT" => "<span class='b to'>Timed Out</span>",
            _ => "<span class='b pend'>Pending</span>"
        };
        var used = Math.Max(0, e.Attempts - 1);

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
  <td class='r'>{used} / {e.MaxRetries}</td>
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
                "FAILED" => "<span class='b bad'>Failed</span>",
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
                var p = line.Split('|');
                if (p.Length < 3) continue;
                orig[p[0] + "|" + p[1]] = p[2];
            }

        var order = new List<string>();
        var byKey = new Dictionary<string, FileEntry>();
        foreach (var line in SafeReadLines(ngPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split('|');
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

        int tot = order.Count, recovered = 0, pending = 0, totRetries = 0;

        // Inject Pending rows for the index/host of any panel present in this report whose manifests
        // haven't been sent yet (they're not in the ng-retry log). Filenames come from the jobs file's
        // manifest lines. This mirrors the NG list, which shows the manifests as Pending too.
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
                if (byKey.ContainsKey(key)) continue;   // already sent/recorded — leave as-is
                byKey[key] = new FileEntry { Pid = pid, File = name, PendingManifest = true };
                order.Add(key);
            }
        }

        foreach (var k in order)
        {
            var e = byKey[k];
            if (e.Recovered) recovered++; else pending++;
            totRetries += e.Events.Count;
        }

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
            int pFail = keys.Count - pRec;
            var ovText = pFail == 0 ? "Recovered" : "Still failing";
            var ovCls = pFail == 0 ? "ok" : "bad";
            var stTokens = new List<string>();
            if (pRec > 0) stTokens.Add("RECOVERED");
            if (pFail > 0) stTokens.Add("FAILING");
            var stAttr = string.Join(" ", stTokens);

            var frows = new StringBuilder();
            foreach (var k in keys)
                frows.Append(NgRow(byKey[k], orig.TryGetValue(k, out var rs) ? rs : "", primary, secondary));

            cards.Append($@"
<div class='panel' data-states='{stAttr}'>
  <div class='phead'>
    <span class='ppid'>{Enc(pid)}</span>
    <span class='ptally'>{pRec} recovered &middot; {pFail} still failing</span>
    <span class='b {ovCls} pov'>{ovText}</span>
  </div>
  <table class='ptable'>
    <thead><tr><th>File</th><th>Original</th><th>Result</th><th>Retries</th><th>Last</th><th>Attempts (IP &amp; outcome)</th></tr></thead>
    <tbody>{frows}</tbody>
  </table>
</div>");
        }

        var generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var html = $@"<!doctype html>
<html><head><meta charset='utf-8'>
<title>NG-retry log {day}</title>
<style>{NgCss}</style></head><body>
<h1>NG-retry &mdash; {day}</h1>
<div class='sub'>from {Enc(ngPath)} &nbsp;&middot;&nbsp; generated {generated}</div>
<div class='cards'>
  <div class='card'><div class='n'>{tot}</div><div class='l'>NG items</div></div>
  <div class='card clickable' data-filter='RECOVERED'><div class='n ok'>{recovered}{Pct(recovered, tot)}</div><div class='l'>Recovered</div></div>
  <div class='card clickable' data-filter='FAILING'><div class='n bad'>{pending}{Pct(pending, tot)}</div><div class='l'>Still failing</div></div>
  <div class='card'><div class='n pend'>{totRetries}</div><div class='l'>Total retries</div></div>
</div>
<div class='fhint'>Click <b>Recovered / Still failing</b> to show only the matching files (panels with none are hidden; click both to show all again, or click one to clear). <span id='fcount'></span></div>
<h2>NG-retry items</h2>
{cards}
<script>
(function(){{
  var active = new Set();
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fcount = document.getElementById('fcount');
  function apply(){{
    var shown = 0;
    panels.forEach(function(p){{
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
    if (fcount) fcount.textContent = active.size === 0
      ? '' : ('Showing ' + shown + ' of ' + panels.length + ' panels (matching rows only)');
  }}
  cards.forEach(function(c){{
    c.addEventListener('click', function(){{
      var f = c.getAttribute('data-filter');
      if (active.has(f)) {{ active.delete(f); c.classList.remove('active'); }}
      else {{ active.add(f); c.classList.add('active'); }}
      apply();
    }});
  }});
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

    private const string DayCss = @"
  body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#14203C;background:#F4F6FA;}
  h1{font-size:22px;margin:0 0 2px;} h2{font-size:15px;margin:26px 0 10px;color:#3A4256;}
  .sub{color:#6B7386;font-size:12.5px;margin-bottom:18px;}
  .cards{display:flex;gap:12px;flex-wrap:wrap;margin-bottom:8px;}
  .card{background:#fff;border:1px solid #ECEFF5;border-radius:10px;padding:12px 16px;min-width:120px;}
  .card.clickable{cursor:pointer;user-select:none;transition:transform .05s ease,box-shadow .1s ease,border-color .1s ease;}
  .card.clickable:hover{box-shadow:0 3px 10px rgba(20,32,60,.13);transform:translateY(-1px);}
  .card.clickable.active{border-color:#4D8CFF;box-shadow:0 0 0 2px rgba(77,140,255,.35);}
  .fhint{font-size:11.5px;color:#8891A3;margin:2px 0 4px;}
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
  body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#14203C;background:#F4F6FA;}
  h1{font-size:22px;margin:0 0 2px;} h2{font-size:15px;margin:26px 0 10px;color:#3A4256;}
  .sub{color:#6B7386;font-size:12.5px;margin-bottom:18px;}
  .cards{display:flex;gap:12px;flex-wrap:wrap;margin-bottom:8px;}
  .card{background:#fff;border:1px solid #ECEFF5;border-radius:10px;padding:12px 16px;min-width:120px;}
  .card.clickable{cursor:pointer;user-select:none;transition:transform .05s ease,box-shadow .1s ease,border-color .1s ease;}
  .card.clickable:hover{box-shadow:0 3px 10px rgba(20,32,60,.13);transform:translateY(-1px);}
  .card.clickable.active{border-color:#4D8CFF;box-shadow:0 0 0 2px rgba(77,140,255,.35);}
  .fhint{font-size:11.5px;color:#8891A3;margin:2px 0 4px;}
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
