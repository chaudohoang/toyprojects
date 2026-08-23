# =============================================================================
#  _htmllog.ps1 - build an HTML report of a day's upload log and open it.
#
#  Reads YYYYMMDD_rawlog.txt (the append-only per-attempt log) and turns it into
#  a sortable, colour-coded report: one row per file with final status, succeed /
#  fail times, retries used, and every attempt's primary/secondary IP + outcome.
#  If a snapshot log exists it is summarised too.
#
#  Reads config.json (next to this script / the exe) to find LogFolder and to label
#  each attempt's host as Primary or Secondary.
#
#     .\_htmllog.ps1                 # today
#     .\_htmllog.ps1 20260816        # a specific day (yyyyMMdd)
#     .\_htmllog.ps1 -LogFolder D:\FtpUploadDemo\logs -NoOpen
# =============================================================================
param(
    [string]$Day = (Get-Date -Format 'yyyyMMdd'),
    [string]$LogFolder = '',
    [string]$JobsFolder = '',
    [switch]$NoOpen
)

function Enc([string]$s) {
    if ($null -eq $s) { return '' }
    return ($s -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;')
}

$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }

# config.json (next to the exe) gives the log/jobs folders and the host roles
$primary = ''; $secondary = ''; $cfg = $null
$cfgPath = Join-Path $root 'config.json'
if (Test-Path $cfgPath) {
    try {
        $cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json
        if ([string]::IsNullOrEmpty($LogFolder))  { $LogFolder  = [string]$cfg.LogFolder }
        if ([string]::IsNullOrEmpty($JobsFolder)) { $JobsFolder = [string]$cfg.JobsFolder }
        $primary   = [string]$cfg.PrimaryHost
        $secondary = [string]$cfg.SecondaryHost
    } catch { }
}
if ([string]::IsNullOrEmpty($LogFolder)) { $LogFolder = Join-Path $root 'logs' }
# Resolve a relative LogFolder against the script/exe folder, exactly like the app does, so a seed
# config using "logs" works regardless of the current working directory.
if (-not [System.IO.Path]::IsPathRooted($LogFolder)) { $LogFolder = Join-Path $root $LogFolder }

# if the jobs folder wasn't given/known, assume it sits next to the logs folder
if ([string]::IsNullOrEmpty($JobsFolder)) {
    $parent = Split-Path $LogFolder -Parent
    if ($parent) { $JobsFolder = Join-Path $parent 'jobs' }
}
if ($JobsFolder -and -not [System.IO.Path]::IsPathRooted($JobsFolder)) { $JobsFolder = Join-Path $root $JobsFolder }

# retries for a file that has no attempt yet (derived from config; default 2+2)
$maxRetriesCfg = 4
if ($cfg) {
    $pr = if ($cfg.PSObject.Properties.Name -contains 'PrimaryRetries')   { [int]$cfg.PrimaryRetries }   else { 2 }
    $sr = if ($cfg.PSObject.Properties.Name -contains 'SecondaryRetries') { [int]$cfg.SecondaryRetries } else { 2 }
    $maxRetriesCfg = $pr + $sr
}

# No day given (e.g. double-clicked): list days that actually have a raw log and let the user pick.
if (-not $PSBoundParameters.ContainsKey('Day')) {
    $days = @(Get-ChildItem -Path $LogFolder -Filter '*_rawlog.txt' -ErrorAction SilentlyContinue |
              ForEach-Object { $_.Name -replace '_rawlog\.txt$', '' } |
              Sort-Object -Unique)
    if ($days.Count -eq 0) {
        Write-Host "No log days found in $LogFolder"
        exit 1
    }
    Write-Host ""
    Write-Host "Days with a report available (in $LogFolder):"
    for ($i = 0; $i -lt $days.Count; $i++) { Write-Host ("  [{0}] {1}" -f ($i + 1), $days[$i]) }
    Write-Host ""
    $sel = Read-Host "Pick a number, or type a yyyyMMdd (blank = newest)"
    if ([string]::IsNullOrWhiteSpace($sel)) { $Day = $days[-1] }
    elseif ($sel -match '^\d{8}$')          { $Day = $sel }
    elseif ($sel -match '^\d+$' -and [int]$sel -ge 1 -and [int]$sel -le $days.Count) { $Day = $days[[int]$sel - 1] }
    else { Write-Host "Not understood; using newest ($($days[-1]))."; $Day = $days[-1] }
}

$raw      = Join-Path $LogFolder  ("{0}_rawlog.txt" -f $Day)
$jobsPath = Join-Path $JobsFolder ("{0}_jobs.txt"   -f $Day)
if (-not (Test-Path $raw) -and -not (Test-Path $jobsPath)) {
    Write-Host "Nothing found for $Day."
    Write-Host "  rawlog: $raw"
    Write-Host "  jobs  : $jobsPath"
    Write-Host "Pass a day (e.g. _htmllog.bat 20260816), or -LogFolder / -JobsFolder <path>."
    exit 1
}

function Role($ip) {
    if ($ip -and $ip -eq $primary)   { return 'Primary' }
    if ($ip -and $ip -eq $secondary) { return 'Secondary' }
    if ($ip) { return $ip }
    return '?'
}

# ---- parse: one entry per file (PID|File), keeping every attempt in order --------
$order = New-Object System.Collections.ArrayList
$byKey = @{}

# First seed the FULL file list from today's jobs file (like the UI), so files that
# have not been attempted yet still show up (as Pending). Overlaid by the rawlog below.
if (Test-Path $jobsPath) {
    foreach ($line in Get-Content $jobsPath) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $p = $line.Split('|')
        if ($p.Count -lt 2) { continue }
        $key = $p[0] + '|' + $p[1]
        if (-not $byKey.ContainsKey($key)) {
            $byKey[$key] = [pscustomobject]@{
                Pid = $p[0]; File = $p[1]; Events = (New-Object System.Collections.ArrayList)
                Status = ''; Succeed = ''; FailTimes = ''; Attempts = 0; MaxRetries = $maxRetriesCfg
            }
            [void]$order.Add($key)
        }
    }
}

$rawLines = if (Test-Path $raw) { Get-Content $raw } else { @() }
foreach ($line in $rawLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $p = $line.Split('|')
    if ($p.Count -lt 8) { continue }
    $key = $p[0] + '|' + $p[1]
    if (-not $byKey.ContainsKey($key)) {
        $byKey[$key] = [pscustomobject]@{
            Pid = $p[0]; File = $p[1]; Events = (New-Object System.Collections.ArrayList)
            Status = ''; Succeed = ''; FailTimes = ''; Attempts = 0; MaxRetries = 0
        }
        [void]$order.Add($key)
    }
    $e  = $byKey[$key]
    $ip = if ($p.Count -ge 9) { $p[8] } else { '' }
    # Only real attempts have a host. A timeout-skip line (empty host, 0 attempts) is not an
    # attempt, so it gets no chip and no fail-time row.
    if ($ip) { [void]$e.Events.Add([pscustomobject]@{ Ok = ($p[2] -eq 'SUCCEEDED'); Ip = $ip }) }
    $e.Status    = $p[2]
    $e.Succeed   = $p[3]
    $e.FailTimes = $p[5]
    $e.Attempts  = [int]$p[6]
    # field 8 of the rawlog is MaxAttempts (counts the initial attempt); the UI shows RETRIES,
    # so the ceiling is MaxAttempts - 1 (initial attempt is not a retry).
    $e.MaxRetries = [Math]::Max(0, [int]$p[7] - 1)
}

# ---- summary ---------------------------------------------------------------------
$tot = $order.Count; $ok = 0; $fail = 0; $pend = 0; $timeout = 0
$priOk = 0; $priFail = 0; $secOk = 0; $secFail = 0
foreach ($k in $order) {
    $e = $byKey[$k]
    switch ($e.Status) {
        'SUCCEEDED' { $ok++ }
        'FAILED'    { $fail++ }
        'TIMEDOUT'  { $timeout++ }
        default     { $pend++ }
    }
    foreach ($ev in $e.Events) {
        $r = Role $ev.Ip
        if     ($r -eq 'Primary')   { if ($ev.Ok) { $priOk++ } else { $priFail++ } }
        elseif ($r -eq 'Secondary') { if ($ev.Ok) { $secOk++ } else { $secFail++ } }
    }
}

# ---- group files into panels (PID) so the report mirrors the UI cards ------------
$panelOrder = New-Object System.Collections.ArrayList
$panels = @{}
foreach ($k in $order) {
    $pidv = $byKey[$k].Pid
    if (-not $panels.ContainsKey($pidv)) {
        $panels[$pidv] = New-Object System.Collections.ArrayList
        [void]$panelOrder.Add($pidv)
    }
    [void]$panels[$pidv].Add($k)
}

function FileRow($e) {
    $badge = switch ($e.Status) {
        'SUCCEEDED' { "<span class='b ok'>Succeeded</span>" }
        'FAILED'    { "<span class='b bad'>Failed</span>" }
        'TIMEDOUT'  { "<span class='b to'>Timed Out</span>" }
        default     { "<span class='b pend'>Pending</span>" }
    }
    $used = [Math]::Max(0, $e.Attempts - 1)

    # Pair each time with its attempt number using the reconstructed event sequence: the
    # succeeded event carries the succeed time; each failed event takes the next fail time.
    # Use a typed List<string> so indexing always returns a string element (never a character
    # from a single-element split collapsing to a scalar string).
    $ftList = New-Object 'System.Collections.Generic.List[string]'
    if ($e.FailTimes) {
        foreach ($t in ($e.FailTimes -split ',')) {
            if ($t) { [void]$ftList.Add([string]$t) }
        }
    }
    $fp = 0
    $succLine = ''
    $failLines = New-Object System.Collections.ArrayList

    $chips = ''
    $i = 0
    foreach ($ev in $e.Events) {
        $i++
        $r = Role $ev.Ip
        $rc = if ($r -eq 'Primary') { 'pri' } elseif ($r -eq 'Secondary') { 'sec' } else { 'oth' }
        if ($ev.Ok) {
            $oc = 'ok'; $mk = '&#10003;'
            if ($e.Succeed) { $succLine = "$i.&nbsp;" + (Enc $e.Succeed) }
        } else {
            $oc = 'bad'; $mk = '&#10007;'
            $t = if ($fp -lt $ftList.Count) { $ftList[$fp] } else { '' }
            $fp++
            if ($t) { [void]$failLines.Add("$i.&nbsp;" + (Enc $t)) }
        }
        $chips += "<span class='chip $oc $rc' title='attempt $i via $(Enc $ev.Ip)'>$i&nbsp;$r&nbsp;$mk</span> "
    }

    $succ = if ($succLine) { $succLine } else { '&mdash;' }
    $ft   = if ($failLines.Count -gt 0) { $failLines -join '<br>' } else { '&mdash;' }
    $rowStatus = if ($e.Status) { $e.Status } else { 'PENDING' }

    return @"
<tr data-status='$rowStatus'>
  <td class='file'>$(Enc $e.File)</td>
  <td>$badge</td>
  <td class='t'>$succ</td>
  <td class='t'>$ft</td>
  <td class='r'>$used / $($e.MaxRetries)</td>
  <td class='chips'>$chips</td>
</tr>
"@
}

# ---- one card per panel ----------------------------------------------------------
$cards = New-Object System.Text.StringBuilder
foreach ($pidv in $panelOrder) {
    $files   = @($panels[$pidv] | ForEach-Object { $byKey[$_] })
    $total   = $files.Count
    $nSucc   = @($files | Where-Object { $_.Status -eq 'SUCCEEDED' }).Count
    $nFail   = @($files | Where-Object { $_.Status -eq 'FAILED' }).Count
    $nTO     = @($files | Where-Object { $_.Status -eq 'TIMEDOUT' }).Count
    $nPend   = $total - $nSucc - $nFail - $nTO

    if     ($nPend -gt 0)        { $ovText = 'In Progress'; $ovCls = 'pend' }
    elseif ($nSucc -eq $total)   { $ovText = 'Success';     $ovCls = 'ok' }
    elseif ($nTO -gt 0)          { $ovText = 'Timed Out';   $ovCls = 'to' }
    else                         { $ovText = 'Failed';      $ovCls = 'bad' }

    $frows = ''
    foreach ($e in $files) { $frows += (FileRow $e) }

    $stTokens = New-Object System.Collections.ArrayList
    if ($nSucc -gt 0) { [void]$stTokens.Add('SUCCEEDED') }
    if ($nFail -gt 0) { [void]$stTokens.Add('FAILED') }
    if ($nTO   -gt 0) { [void]$stTokens.Add('TIMEDOUT') }
    if ($nPend -gt 0) { [void]$stTokens.Add('PENDING') }
    $stAttr = $stTokens -join ' '

    [void]$cards.Append(@"
<div class='panel' data-statuses='$stAttr'>
  <div class='phead'>
    <span class='ppid'>$(Enc $pidv)</span>
    <span class='ptally'>$nSucc/$total succeeded &middot; $($nFail + $nTO) failed</span>
    <span class='b $ovCls pov'>$ovText</span>
  </div>
  <table class='ptable'>
    <thead><tr><th>File</th><th>Status</th><th>Succeeded</th><th>Failed at</th><th>Retries</th><th>Attempts (IP &amp; outcome)</th></tr></thead>
    <tbody>
$frows
    </tbody>
  </table>
</div>
"@)
}

# ---- snapshot summary (optional) -------------------------------------------------
$snapPath = Join-Path $LogFolder ("{0}_snapshot.txt" -f $Day)
$snapRows = ''
if (Test-Path $snapPath) {
    $sb = New-Object System.Text.StringBuilder
    foreach ($line in Get-Content $snapPath) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $p = $line.Split('|')
        if ($p.Count -lt 3) { continue }
        $ovOk = ($p[2] -eq 'O')
        $ov = if ($ovOk) { "<span class='b ok'>O</span>" } else { "<span class='b bad'>X</span>" }
        [void]$sb.Append("<tr><td class='t'>$(Enc $p[0])</td><td class='pid'>$(Enc $p[1])</td><td>$ov</td></tr>")
    }
    $snapRows = $sb.ToString()
}

# ---- HTML ------------------------------------------------------------------------
$generated = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$pTag = if ($primary)   { " ($(Enc $primary))" } else { '' }
$sTag = if ($secondary) { " ($(Enc $secondary))" } else { '' }

$snapSection = ''
if ($snapRows) {
    $snapSection = @"
<h2>Result-timing snapshots</h2>
<table class='snap'>
  <thead><tr><th>Time</th><th>PID</th><th>Overall</th></tr></thead>
  <tbody>$snapRows</tbody>
</table>
"@
}

$html = @"
<!doctype html>
<html><head><meta charset='utf-8'>
<title>FTP Upload log $Day</title>
<style>
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
  .fhint .fc{color:#4D8CFF;font-weight:600;margin-left:6px;}
  .card .n{font-size:22px;font-weight:700;} .card .l{font-size:11px;color:#8891A3;text-transform:uppercase;letter-spacing:.04em;}
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
  .legend .chip{cursor:default;}
</style></head><body>
<h1>FTP Upload &mdash; $Day</h1>
<div class='sub'>from $(Enc $raw) &nbsp;&middot;&nbsp; generated $generated</div>

<div class='cards'>
  <div class='card'><div class='n'>$tot</div><div class='l'>Files</div></div>
  <div class='card clickable' data-filter='SUCCEEDED'><div class='n ok'>$ok</div><div class='l'>Succeeded</div></div>
  <div class='card clickable' data-filter='FAILED'><div class='n bad'>$fail</div><div class='l'>Failed</div></div>
  <div class='card clickable' data-filter='TIMEDOUT'><div class='n to'>$timeout</div><div class='l'>Timed out</div></div>
  <div class='card clickable' data-filter='PENDING'><div class='n pend'>$pend</div><div class='l'>Pending</div></div>
  <div class='card'><div class='n'>$priOk&nbsp;/&nbsp;$priFail</div><div class='l'>Primary ok / fail$pTag</div></div>
  <div class='card'><div class='n'>$secOk&nbsp;/&nbsp;$secFail</div><div class='l'>Secondary ok / fail$sTag</div></div>
</div>
<div class='fhint'>Click <b>Succeeded / Failed / Timed out / Pending</b> to show only the matching files (panels with none are hidden; click several to combine; click again to clear). <span class='fc'></span></div>
<div class='legend'>Attempts:
  <span class='chip ok pri'>Primary &#10003;</span>
  <span class='chip bad pri'>Primary &#10007;</span>
  <span class='chip ok sec'>Secondary &#10003;</span>
  <span class='chip bad sec'>Secondary &#10007;</span>
  &nbsp;(blue border = primary IP, amber = secondary; green = uploaded, red = failed)
</div>

<h2>Files</h2>
$($cards.ToString())

$snapSection
<script>
(function(){
  var active = new Set();
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fc     = document.querySelector('.fc');
  function apply(){
    var shown = 0;
    panels.forEach(function(p){
      var rows = p.querySelectorAll('tbody tr');
      if (active.size === 0) {
        p.style.display = '';
        rows.forEach(function(r){ r.style.display = ''; });
        shown++;
        return;
      }
      var any = false;
      rows.forEach(function(r){
        var match = active.has(r.getAttribute('data-status'));
        r.style.display = match ? '' : 'none';
        if (match) any = true;
      });
      p.style.display = any ? '' : 'none';
      if (any) shown++;
    });
    if (fc) fc.textContent = active.size === 0 ? '' : ('Showing ' + shown + ' of ' + panels.length + ' panels (matching rows only)');
  }
  cards.forEach(function(c){
    c.addEventListener('click', function(){
      var f = c.getAttribute('data-filter');
      if (active.has(f)) { active.delete(f); c.classList.remove('active'); }
      else { active.add(f); c.classList.add('active'); }
      apply();
    });
  });
})();
</script>
</body></html>
"@

$outPath = Join-Path $LogFolder ("{0}_htmllog.html" -f $Day)
$html | Set-Content -Path $outPath -Encoding UTF8

Write-Host "wrote $outPath  ($tot files: $ok ok, $fail failed, $pend pending)"
if (-not $NoOpen) { Invoke-Item $outPath }
