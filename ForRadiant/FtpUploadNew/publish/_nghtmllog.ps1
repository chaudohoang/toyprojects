# =============================================================================
#  _nghtmllog.ps1 - HTML report of a day's NG-RETRY log (the manual recovery pump).
#
#  Reads YYYYMMDD_ngretrylog.txt and turns it into a colour-coded report: one row
#  per NG item, its original reason (Failed / Timed Out from the raw log), whether
#  it was RECOVERED via NG retry, how many retries it took, and each attempt's IP
#  + outcome. Reads config.json (next to the exe) for the log folder + host labels.
#
#     .\_nghtmllog.ps1                 # today
#     .\_nghtmllog.ps1 20260816        # a specific day (yyyyMMdd)

#     .\_nghtmllog.ps1 -LogFolder D:\FtpUploadDemo\logs -NoOpen
# =============================================================================
param(
    [string]$Day = (Get-Date -Format 'yyyyMMdd'),
    [string]$LogFolder = '',
    [switch]$NoOpen
)

# --- log-name resolution -----------------------------------------------------
#  The per-attempt log is "{day}_totallog.txt" and the NG one "{day}_ngretrytotallog.txt".
#  Days recorded before the rename use "_rawlog.txt" / "_ngretrylog.txt", and copied log
#  sets from site still do, so every read resolves the new name first and falls back.
function Resolve-DayLog {
    param([string]$Folder, [string]$Day, [string]$NewSuffix, [string]$LegacySuffix)
    $n = Join-Path $Folder ("{0}{1}" -f $Day, $NewSuffix)
    if (Test-Path $n) { return $n }
    $l = Join-Path $Folder ("{0}{1}" -f $Day, $LegacySuffix)
    if (Test-Path $l) { return $l }
    return $n
}
function Get-LogDays {
    param([string]$Folder, [string[]]$Filters)
    $set = @{}
    foreach ($f in $Filters) {
        Get-ChildItem -Path $Folder -Filter $f -ErrorAction SilentlyContinue | ForEach-Object {
            if ($_.Name.Length -ge 8) { $set[$_.Name.Substring(0,8)] = $true } } }
    return @($set.Keys | Sort-Object)
}

# Rows may start with a write time ("yyyy-MM-dd HH:mm:ss") as field 0. Strip it so index 0
# is always the PID - matches LogRow.Fields on the C# side. Logs collected before the
# timestamp existed have no stamp and pass through unchanged.
function Split-LogRow([string]$line) {
    $p = $line.Split("|")
    if ($p.Count -gt 1 -and $p[0].Length -eq 19 -and $p[0][4] -eq "-" -and $p[0][7] -eq "-") { return $p[1..($p.Count-1)] }
    return $p
}


function Enc([string]$s) {
    if ($null -eq $s) { return '' }
    return ($s -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;')
}

function Pct($n, $total) {
    if ($total -gt 0) { return ("<span class='pct'>{0:0.#}%</span>" -f ($n * 100.0 / $total)) }
    return ''
}

$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }

$primary = ''; $secondary = ''; $JobsFolder = ''
$cfgPath = Join-Path $root 'config.json'
if (Test-Path $cfgPath) {
    try {
        $cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json
        if ([string]::IsNullOrEmpty($LogFolder)) { $LogFolder = [string]$cfg.LogFolder }
        $JobsFolder = [string]$cfg.JobsFolder
        $primary   = [string]$cfg.PrimaryHost
        $secondary = [string]$cfg.SecondaryHost
    } catch { }
}
if ([string]::IsNullOrEmpty($LogFolder)) { $LogFolder = Join-Path $root 'logs' }
# Resolve a relative LogFolder against the script/exe folder, exactly like the app does, so a seed
# config using "logs" works regardless of the current working directory.
if (-not [System.IO.Path]::IsPathRooted($LogFolder)) { $LogFolder = Join-Path $root $LogFolder }
if ([string]::IsNullOrEmpty($JobsFolder)) { $JobsFolder = Join-Path $root 'jobs' }
if (-not [System.IO.Path]::IsPathRooted($JobsFolder)) { $JobsFolder = Join-Path $root $JobsFolder }

# No day given (e.g. double-clicked): list days that actually have an ng-retry log and let the user pick.
if (-not $PSBoundParameters.ContainsKey('Day')) {
    $days = @(Get-LogDays $LogFolder @('*_ngretrytotallog.txt','*_ngretrylog.txt'))
    if ($days.Count -eq 0) {
        Write-Host "No NG-retry log days found in $LogFolder"
        exit 1
    }
    Write-Host ""
    Write-Host "Days with an NG-retry report available (in $LogFolder):"
    for ($i = 0; $i -lt $days.Count; $i++) { Write-Host ("  [{0}] {1}" -f ($i + 1), $days[$i]) }
    Write-Host ""
    $sel = Read-Host "Pick a number, or type a yyyyMMdd (blank = newest)"
    if ([string]::IsNullOrWhiteSpace($sel)) { $Day = $days[-1] }
    elseif ($sel -match '^\d{8}$')          { $Day = $sel }
    elseif ($sel -match '^\d+$' -and [int]$sel -ge 1 -and [int]$sel -le $days.Count) { $Day = $days[[int]$sel - 1] }
    else { Write-Host "Not understood; using newest ($($days[-1]))."; $Day = $days[-1] }
}

$ngPath  = Resolve-DayLog $LogFolder $Day '_ngretrytotallog.txt' '_ngretrylog.txt'
$rawPath = Resolve-DayLog $LogFolder $Day '_totallog.txt' '_rawlog.txt'
if (-not (Test-Path $ngPath)) {
    Write-Host "No NG-retry log for $Day at:`n  $ngPath"
    Write-Host "Pass a day (e.g. _nghtmllog.bat 20260816) or -LogFolder <path>."
    exit 1
}

function Role($ip) {
    if ($ip -and $ip -eq $primary)   { return 'Primary' }
    if ($ip -and $ip -eq $secondary) { return 'Secondary' }
    if ($ip) { return $ip }
    return '?'
}

# original reason (Failed / Timed Out) from that day's raw log, last line per file
$orig = @{}
if (Test-Path $rawPath) {
    foreach ($line in Get-Content $rawPath) {
        $p = Split-LogRow $line
        if ($p.Count -lt 3) { continue }
        $orig[$p[0] + '|' + $p[1]] = $p[2]
    }
}

# parse the ng-retry log: one entry per file, every attempt in order
$order = New-Object System.Collections.ArrayList
$byKey = @{}
foreach ($line in Get-Content $ngPath) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $p = Split-LogRow $line
    if ($p.Count -lt 3) { continue }
    $key = $p[0] + '|' + $p[1]
    if (-not $byKey.ContainsKey($key)) {
        $byKey[$key] = [pscustomobject]@{
            Pid = $p[0]; File = $p[1]; Events = (New-Object System.Collections.ArrayList)
            Recovered = $false; Retries = 0; LastTime = ''; PendingManifest = $false
        }
        [void]$order.Add($key)
    }
    $e = $byKey[$key]
    $ok = ($p[2] -eq 'SUCCEEDED')
    $ip = if ($p.Count -ge 9) { $p[8] } else { '' }
    $tm = if ($p.Count -ge 6) { $p[5] } else { '' }
    [void]$e.Events.Add([pscustomobject]@{ Ok = $ok; Ip = $ip; Time = $tm })
    if ($ok) { $e.Recovered = $true }
    if ($p.Count -ge 6) { $e.LastTime = $p[5] }
}

# Inject Pending rows for the index/host of any panel in this report whose manifests haven't been
# sent yet. "Sent" must come from BOTH logs: a manifest uploaded by the LIVE pump appears only in
# the rawlog, so checking the ng-retry log alone reported it Pending forever.
$sentInRawLog = @{}
$rawPathForPending = Resolve-DayLog $LogFolder $Day '_totallog.txt' '_rawlog.txt'
if (Test-Path $rawPathForPending) {
    foreach ($rl in Get-Content $rawPathForPending) {
        $rp = Split-LogRow $rl
        if ($rp.Count -ge 3 -and $rp[2] -eq 'SUCCEEDED') { $sentInRawLog[$rp[0] + '|' + $rp[1]] = $true }
    }
}
$manifestNames = @{}
$jobsPath = Join-Path $JobsFolder ("{0}_jobs.txt" -f $Day)
if (Test-Path $jobsPath) {
    foreach ($jl in Get-Content $jobsPath) {
        $jp = $jl.Split('|')
        if ($jp.Count -lt 10 -or $jp[9].Trim() -ne '1') { continue }
        $mp = $jp[0]
        if (-not $manifestNames.ContainsKey($mp)) { $manifestNames[$mp] = New-Object System.Collections.ArrayList }
        [void]$manifestNames[$mp].Add($jp[1])
    }
}
$pidsInReport = @{}
foreach ($k in $order) { $pidsInReport[$byKey[$k].Pid] = $true }
foreach ($mp in $pidsInReport.Keys) {
    if (-not $manifestNames.ContainsKey($mp)) { continue }
    foreach ($name in $manifestNames[$mp]) {
        $key = $mp + '|' + $name
        if ($byKey.ContainsKey($key)) { continue }        # already in the ng-retry log
        if ($sentInRawLog.ContainsKey($key)) { continue } # sent by the live pump - not pending
        $byKey[$key] = [pscustomobject]@{
            Pid = $mp; File = $name; Events = (New-Object System.Collections.ArrayList)
            Recovered = $false; Retries = 0; LastTime = ''; PendingManifest = $true
        }
        [void]$order.Add($key)
    }
}

# summary
$recovered = 0; $failing = 0; $pendingMan = 0; $totRetries = 0
foreach ($k in $order) {
    $e = $byKey[$k]
    # Three distinct states. Lumping pending manifests in with failures made the header read
    # "192 recovered (100%)" and "124 still failing" at the same time, and clicking Still failing
    # showed nothing - because those rows are tagged PENDING, not FAILING.
    if     ($e.Recovered)       { $recovered++ }
    elseif ($e.PendingManifest) { $pendingMan++ }
    else                        { $failing++ }
    $totRetries += $e.Events.Count
}
$tot = $recovered + $failing + $pendingMan

# build one NG item row (no PID column; that's in the panel header)
function NgRow($e, $reason) {
    if ($e.PendingManifest) {
        $rBadge = "<span class='b pend'>Pending</span>"
    } else {
        $rBadge = switch ($reason) {
            'TIMEDOUT' { "<span class='b to'>Timed Out</span>" }
            'FAILED'   { "<span class='b bad'>Failed</span>" }
            default    { "<span class='b pend'>&mdash;</span>" }
        }
    }
    $state = if ($e.PendingManifest) { "<span class='b pend'>Pending</span>" }
             elseif ($e.Recovered) { "<span class='b ok'>Recovered</span>" } else { "<span class='b bad'>Still failing</span>" }
    $stateAttr = if ($e.PendingManifest) { 'PENDING' } elseif ($e.Recovered) { 'RECOVERED' } else { 'FAILING' }

    $chips = ''
    $i = 0
    foreach ($ev in $e.Events) {
        $i++
        $r = Role $ev.Ip
        $oc = if ($ev.Ok) { 'ok' } else { 'bad' }
        $rc = if ($r -eq 'Primary') { 'pri' } elseif ($r -eq 'Secondary') { 'sec' } else { 'oth' }
        $mk = if ($ev.Ok) { '&#10003;' } else { '&#10007;' }
        $tt = if ($ev.Time) { " " + (Enc $ev.Time) } else { '' }
        $chips += "<span class='chip $oc $rc' title='retry $i via $(Enc $ev.Ip)$tt'>$i&nbsp;$r&nbsp;$mk</span> "
    }

    return @"
<tr data-state='$stateAttr'>
  <td class='file'>$(Enc $e.File)</td>
  <td>$rBadge</td>
  <td>$state</td>
  <td class='r'>$($e.Events.Count)</td>
  <td class='t'>$(Enc $e.LastTime)</td>
  <td class='chips'>$chips</td>
</tr>
"@
}

# group NG items into panel cards by PID (preserve first-seen order)
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

$panelsComplete = 0
$cards = New-Object System.Text.StringBuilder
foreach ($pidv in $panelOrder) {
    # Order rows like the main rawlog / jobs file: data files first, then index, then host.
    $keys = @($panels[$pidv] | Sort-Object @{Expression={
        $f = $byKey[$_].File
        if ($f.ToLower().EndsWith('.idx')) { 1 }
        elseif ($f.StartsWith($pidv + '_') -and $f.ToLower().EndsWith('.txt')) { 2 }
        else { 0 }
    }}, @{Expression={ [array]::IndexOf($panels[$pidv], $_) }})
    $pRec  = @($keys | Where-Object { $byKey[$_].Recovered }).Count
    # Manifests that have not been sent yet are PENDING, not failures. They are injected with
    # Recovered=$false, so a plain "count - recovered" counted them as failing and every panel that
    # went through NG showed "Still failing" even when every data file had recovered - contradicting
    # the per-row label, which already says Pending. Keep the three states distinct.
    $pPend = @($keys | Where-Object { $byKey[$_].PendingManifest -and -not $byKey[$_].Recovered }).Count
    $pFail = $keys.Count - $pRec - $pPend
    $ovText = if ($pFail -gt 0) { 'Still failing' } elseif ($pPend -gt 0) { 'Manifests pending' } else { 'Recovered' }
    $ovCls  = if ($pFail -gt 0) { 'bad' } elseif ($pPend -gt 0) { 'pend' } else { 'ok' }
    $stTokens = New-Object System.Collections.ArrayList
    if ($pRec  -gt 0) { [void]$stTokens.Add('RECOVERED') }
    if ($pFail -gt 0) { [void]$stTokens.Add('FAILING') }
    if ($pPend -gt 0) { [void]$stTokens.Add('PENDING') }
    if ($keys.Count -gt 0 -and $pRec -eq $keys.Count) { $panelsComplete++ }
    $stAttr = $stTokens -join ' '

    $tally = "$pRec recovered"
    if ($pPend -gt 0) { $tally += " &middot; $pPend manifest(s) pending" }
    if ($pFail -gt 0) { $tally += " &middot; $pFail still failing" }

    $frows = ''
    foreach ($k in $keys) {
        $reason = if ($orig.ContainsKey($k)) { $orig[$k] } else { '' }
        $frows += (NgRow $byKey[$k] $reason)
    }

    [void]$cards.Append(@"
<div class='panel' data-states='$stAttr'>
  <div class='phead'>
    <span class='ppid'>$(Enc $pidv)</span>
    <span class='ptally'>$tally</span>
    <span class='b $ovCls pov'>$ovText</span>
  </div>
  <table class='ptable'>
    <thead><tr><th>File</th><th>Original</th><th>Result</th><th>Retries</th><th>Last</th><th>Attempts (IP &amp; outcome)</th></tr></thead>
    <tbody>
$frows
    </tbody>
  </table>
</div>
"@)
}

# Which machine PRODUCED this day's log, from its oplog STARTUP line - not whoever runs this script.
# A log copied off a line PC and analysed elsewhere must still name the line PC.
$clientTag = ''
$opPath = Resolve-DayLog $LogFolder $Day '_panelevents.txt' '_oplog.txt'
if (Test-Path $opPath) {
    foreach ($ol in Get-Content $opPath) {
        if ($ol -notmatch 'STARTUP') { continue }
        $ix = $ol.IndexOf('client ')
        if ($ix -lt 0) { continue }
        $rest = $ol.Substring($ix + 7)
        # just the machine name and the IP - the rest of the STARTUP line is other detail
        $tok = $rest.Trim() -split '\s+'
        $clientTag = if ($tok.Count -ge 2) { $tok[0] + ' ' + $tok[1] } else { $tok[0] }
    }
}
if ($clientTag) { $clientTag = " &nbsp;&middot;&nbsp; client " + (Enc $clientTag) }
$generated = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$pTag = if ($primary)   { " ($(Enc $primary))" } else { '' }
$sTag = if ($secondary) { " ($(Enc $secondary))" } else { '' }

$html = @"
<!doctype html>
<html><head><meta charset='utf-8'>
<title>NG-retry log $Day</title>
<style>
  html{overflow-y:scroll;}
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
  .pidlbl{font-size:11px;font-weight:600;color:#6B7386;}
  .pidwrap{position:relative;display:inline-block;}
  .pidwrap input{padding:5px 26px 5px 10px;font-size:12px;border:1px solid #C9DAF8;border-radius:8px;width:210px;outline:none;}
  .pidwrap input:focus{border-color:#4D8CFF;}
  #pidClear{position:absolute;right:7px;top:50%;transform:translateY(-50%);cursor:pointer;color:#8A97BD;font-size:15px;line-height:1;display:none;user-select:none;}
  #pidClear:hover{color:#E0483F;}
  .fhint b{color:#6B7386;}
  .fhint .fc{color:#4D8CFF;font-weight:600;margin-left:6px;}
  .card .n{font-size:22px;font-weight:700;} .card .l{font-size:11px;color:#8891A3;text-transform:uppercase;letter-spacing:.04em;}
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
  .chip.pri{border-color:#4D8CFF;} .chip.sec{border-color:#B8860B;}
</style></head><body>
<div class='hdr'>
  <div class='hdrleft'>
  <h1>NG-retry &mdash; $Day</h1>
  <div class='sub'>from $(Enc $ngPath) &nbsp;&middot;&nbsp; generated $generated$clientTag</div>
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
  <div class='card'><div class='n'>$($panelOrder.Count)$(Pct $panelsComplete $panelOrder.Count)</div><div class='l'>Panels recovered</div></div>
  <div class='card'><div class='n'>$tot</div><div class='l'>Files</div></div>
  <div class='card clickable' data-filter='RECOVERED'><div class='n ok'>$recovered$(Pct $recovered $tot)</div><div class='l'>Recovered</div></div>
  <div class='card clickable' data-filter='FAILING'><div class='n bad'>$failing$(Pct $failing $tot)</div><div class='l'>Still failing</div></div>
  <div class='card clickable' data-filter='PENDING'><div class='n pend'>$pendingMan$(Pct $pendingMan $tot)</div><div class='l'>Manifests pending</div></div>
  <div class='card'><div class='n pend'>$totRetries</div><div class='l'>Total retries</div></div>
</div>
<div class='fhint'>Click <b>Recovered / Still failing / Manifests pending</b> to show only the matching files (panels with none are hidden; click both to show all again, or click one to clear). <span class='fc'></span></div>

<h2>NG-retry items</h2>
<div id='noMatch' style='display:none;padding:14px;background:#fff;border:1px solid #ECEFF5;border-radius:10px;color:#8891A3;font-size:12.5px;'>No panels match the current filter.</div>
$($cards.ToString())
<script>
(function(){
  var active = new Set();
  var pidBox = document.getElementById('pidFilter');
  var pidClear = document.getElementById('pidClear');
  function pidOk(p){ if (!pidBox) return true; var q = pidBox.value.trim().toUpperCase(); if (!q) return true;
    var el = p.querySelector('.ppid'); return el && el.textContent.toUpperCase().indexOf(q) !== -1; }
  function pidSync(){ if (pidClear && pidBox) pidClear.style.display = pidBox.value ? 'block' : 'none'; }
  var cards  = document.querySelectorAll('.card.clickable');
  var panels = document.querySelectorAll('.panel');
  var fc     = document.querySelector('.fc');
  function apply(){
    var shown = 0;
    panels.forEach(function(p){
      if (!pidOk(p)) { p.style.display = 'none'; return; }
      var rows = p.querySelectorAll('tbody tr');
      if (active.size === 0) {
        p.style.display = '';
        rows.forEach(function(r){ r.style.display = ''; });
        shown++;
        return;
      }
      var any = false;
      rows.forEach(function(r){
        var match = active.has(r.getAttribute('data-state'));
        r.style.display = match ? '' : 'none';
        if (match) any = true;
      });
      p.style.display = any ? '' : 'none';
      if (any) shown++;
    });
    // Count reflects BOTH filters, and an empty result says so instead of a blank page.
    var filtering = active.size > 0 || (pidBox && pidBox.value.trim() !== '');
    if (fc) fc.textContent = filtering ? ('Showing ' + shown + ' of ' + panels.length + ' panels') : '';
    var nm = document.getElementById('noMatch');
    if (nm) nm.style.display = (filtering && shown === 0) ? 'block' : 'none';
  }
  cards.forEach(function(c){
    c.addEventListener('click', function(){
      var f = c.getAttribute('data-filter');
      if (active.has(f)) { active.delete(f); c.classList.remove('active'); }
      else { active.add(f); c.classList.add('active'); }
      apply();
    });
  });
  if (pidBox) pidBox.addEventListener('input', function(){ pidSync(); apply(); });
  if (pidClear) pidClear.addEventListener('click', function(){ pidBox.value = ''; pidSync(); apply(); pidBox.focus(); });
  pidSync();
  apply();
})();
</script>
</body></html>
"@

$outPath = Join-Path $LogFolder ("{0}_nghtmllog.html" -f $Day)
$html | Set-Content -Path $outPath -Encoding UTF8
Write-Host "wrote $outPath  ($tot NG items: $recovered recovered, $failing still failing, $pendingMan manifest(s) pending)"
if (-not $NoOpen) { Invoke-Item $outPath }
