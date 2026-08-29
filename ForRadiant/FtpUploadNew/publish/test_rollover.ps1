# =============================================================================
#  test_rollover.ps1 - drives the DAY ROLLOVER path end to end.
#
#  Why this exists and stress_500.bat does not cover it: stress_500 stages all
#  500 panels BEFORE launching the app, so the simulated day never turns over
#  while work is in flight -- which is exactly the condition the LGD stall
#  needed. gen_panels.ps1 also kills a running FtpUpload.exe every time it is
#  called, so it cannot drip-feed waves into a live run. This script writes its
#  own panels and leaves the app alone.
#
#  What it does:
#    1. stops the app, FULL clean (jobs / logs / state / panels / scratch)
#    2. patches config.json for the test (fast simulated days, slow uploads so a
#       file is genuinely mid-transfer at each rollover), backing up the original
#    3. stages wave 0, launches the app
#    4. staggers further waves so new panels land WHILE each rollover settles
#    5. restores config.json, then reads back the oplog and PASSES / FAILS
#
#  Every wave uses its own PID block, so waves never dedup against each other.
#
#      .\test_rollover.ps1                 3 rollovers, 12 panels/wave
#      .\test_rollover.ps1 -Days 5         longer soak
#      .\test_rollover.ps1 -KeepConfig     leave the test config in place
# =============================================================================
param(
    [int]$Days          = 3,      # how many simulated rollovers to drive
    [int]$DaySeconds    = 45,     # SimulateFastDaySeconds during the test
    [int]$PanelsPerWave = 12,
    [int]$UploadMs      = 1200,   # per-file delay: guarantees an in-flight file
    [int]$FileKB        = 1,
    [int]$FailPercent   = 0,      # inject transfer failures, so NG recovery runs across rollovers
    [double]$StallFactor = 2.0,   # FAIL if the worst day-advance gap exceeds DaySeconds * this
    [int]$DrainMinutes  = 10,     # after the last wave, wait (up to this) for work to actually finish
    [switch]$KeepConfig           # skip restoring the original config.json
)

$ErrorActionPreference = 'Stop'

$pub = if (Test-Path (Join-Path $PSScriptRoot 'publish\FtpUpload.exe')) { Join-Path $PSScriptRoot 'publish' } else { $PSScriptRoot }
$exe     = Join-Path $pub 'FtpUpload.exe'
$cfgPath = Join-Path $pub 'config.json'
$backup  = Join-Path $pub 'config.rollovertest.bak'
$scratch = 'D:\FtpUploadDemo\rollover'

if (-not (Test-Path $exe))     { Write-Host "FtpUpload.exe not found at $pub - run build.bat first."; exit 1 }
if (-not (Test-Path $cfgPath)) { Write-Host "config.json not found at $cfgPath - launch the app once or run build.bat."; exit 1 }

# Set-Content / Copy-Item open the target with FileShare.Read only, so they fail outright if
# anything else (a dying FtpUpload, an editor, a file-watcher) holds the file - even when that
# holder would happily allow a shared write. Go through a FileStream with FileShare.ReadWrite
# instead, which succeeds in exactly the cases a fixed sleep-and-retry cannot fix.
function Write-TextShared([string]$path, [string]$text) {
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($text)
    for ($try = 1; $try -le 20; $try++) {
        try {
            $fs = [IO.File]::Open($path, [IO.FileMode]::Create, [IO.FileAccess]::Write, [IO.FileShare]::ReadWrite)
            try { $fs.Write($bytes, 0, $bytes.Length) } finally { $fs.Close() }
            return $true
        } catch { Start-Sleep -Milliseconds 500 }
    }
    return $false
}

Write-Host ''
Write-Host '=== ROLLOVER TEST ========================================================'

# ---- 1. stop the app + full clean ------------------------------------------
Write-Host "Step 1/6: stopping FtpUpload and running a FULL clean..."
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
$clean = Join-Path $PSScriptRoot 'clean_panels.ps1'
# -Quiet matters: without it clean_panels ends on a Read-Host and this script hangs forever.
if (Test-Path $clean) { & powershell -NoProfile -ExecutionPolicy Bypass -File $clean -Full -Quiet | Out-Null }
Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue

# A leftover backup means a previous run died before restoring. Put it back FIRST, so we
# never snapshot an already-patched config and "restore" test values at the end.
if (Test-Path $backup) {
    Write-Host "  found a stale $(Split-Path $backup -Leaf) from an interrupted run - restoring it before starting."
    Write-TextShared $cfgPath (Get-Content $backup -Raw) | Out-Null
    Remove-Item $backup -Force
}

# ---- 2. patch config for the test ------------------------------------------
Write-Host "Step 2/6: patching config.json (backup -> $(Split-Path $backup -Leaf))..."
Copy-Item $cfgPath $backup -Force
$cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json

$origPanelTimeout = [int]$cfg.PanelTimeoutSeconds
$cfg.SimulateFastDaySeconds = $DaySeconds
$cfg.SimulateUploadMs       = $UploadMs
# Panel timeouts also produce TIMEDOUT rows. Off during the test so every TIMEDOUT
# in the result is unambiguously from the rollover and nothing else.
$cfg.PanelTimeoutSeconds    = 0
$cfg.AutoStartUploading     = $true
$cfg.SimulateFailurePercent = $FailPercent

# A just-killed FtpUpload can still hold config.json for a moment while Windows tears the
# process down, so a fixed sleep is not enough. Retry until the handle is actually released.
$json = $cfg | ConvertTo-Json -Depth 8
if (-not (Write-TextShared $cfgPath $json)) {
    Write-Host "  config.json could not be written after 10s - restoring the backup and aborting."
    Write-TextShared $cfgPath (Get-Content $backup -Raw) | Out-Null
    Remove-Item $backup -Force -ErrorAction SilentlyContinue
    exit 1
}

function Resolve-Cfg([string]$p, [string]$fallback) {
    if ([string]::IsNullOrWhiteSpace($p)) { $p = $fallback }
    if ([System.IO.Path]::IsPathRooted($p)) { return $p }
    return (Join-Path $pub $p)
}
$queue  = Resolve-Cfg ([string]$cfg.QueueFolder) 'queue'
$logDir = Resolve-Cfg ([string]$cfg.LogFolder)   'logs'
New-Item -ItemType Directory -Force -Path $queue, $scratch | Out-Null

Write-Host ("  fast day     : {0}s   upload delay: {1}ms   panel timeout: OFF (was {2}s)   fail inject: {3}%" -f $DaySeconds, $UploadMs, $origPanelTimeout, $FailPercent)
Write-Host ("  queue folder : {0}" -f $queue)

# ---- panel writer (self-contained; does NOT touch the running app) ---------
$canonical = @(
    'puc_otp_read.txt','d994_gamma.hex','1st_nypucdata.hex','2nd_nypucdata.hex',
    'step01_R064.tif','step02_G064.tif','step03_B064.tif','step04_W064.tif',
    'step05_R128.tif','step06_W255.tif'
)
$blob = [byte[]]::new($FileKB * 1024)
(New-Object Random 42).NextBytes($blob)
$rng = New-Object System.Random 20260829

# Wave N owns PID block TSN(900000 + N*1000 + i), so no wave can ever dedup
# against another wave or against a previous stress_500 run.
function Stage-Wave([int]$Wave, [int]$Count) {
    $stampBase = Get-Date
    for ($i = 1; $i -le $Count; $i++) {
        $srvPid = 'TSN{0:D12}' -f (900000 + $Wave * 1000 + $i)
        $stamp  = $stampBase.AddSeconds(-1 * $i * 37).ToString('yyyyMMddHHmmss')
        $src    = "$scratch\W$Wave`_${srvPid}_${stamp}"
        New-Item -ItemType Directory -Force -Path $src | Out-Null
        foreach ($f in $canonical) { [IO.File]::WriteAllBytes((Join-Path $src $f), $blob) }

        $h1 = '{0:D8}' -f $rng.Next(0,99999999)
        $h2 = '{0:D8}' -f $rng.Next(0,99999999)
        $channel = $rng.Next(0,5).ToString()

        $lines = @(
            "Model=TESTMODEL"
            "EQPID=HNAMAL34DD01"
            "PID=$srvPid"
            "DateTime=$stamp"
            "UploadIndexPath=data1h1/HN_DATA/POCB/INDEX/$h1/$h2/$srvPid.idx"
            "UploadHostPath=data1h1/HN_DATA/POCB/HOST/HNAMAL34DD01/${srvPid}_${stamp}.txt"
            "SourceFolder=$src"
            "ChannelIndex=$channel"
        )
        $final = "$queue\${srvPid}_${stamp}.panel"
        [IO.File]::WriteAllLines("$final.tmp", $lines)
        Move-Item -LiteralPath "$final.tmp" -Destination $final -Force
    }
    Write-Host ("  wave {0}: staged {1} panels (PID block TSN{2:D12}+)" -f $Wave, $Count, (900000 + $Wave * 1000 + 1))
}

# ---- 3. wave 0 + launch -----------------------------------------------------
Write-Host "Step 3/6: staging wave 0 and launching the app..."
Stage-Wave 0 $PanelsPerWave
$started = Get-Date
Start-Process $exe -ArgumentList '--show' | Out-Null
Start-Sleep -Seconds 3

# ---- 4. drip-feed waves across each rollover -------------------------------
# The app advances a simulated day every $DaySeconds of REAL time from ITS start.
# Landing each wave at ~60% of a day window means those panels are still mid-upload
# when the day flips -- the in-flight condition the stall needed.
Write-Host "Step 4/6: driving $Days rollover(s), one wave each..."
for ($d = 1; $d -le $Days; $d++) {
    $target = $started.AddSeconds(($d - 1) * $DaySeconds + [int]($DaySeconds * 0.6) + 3)
    $wait   = [int]([math]::Max(0, ($target - (Get-Date)).TotalSeconds))
    if ($wait -gt 0) { Start-Sleep -Seconds $wait }
    Write-Host ("  ~{0}s in - day {1} about to roll:" -f [int]((Get-Date) - $started).TotalSeconds, $d)
    Stage-Wave $d $PanelsPerWave
}

# ---- 5. drain --------------------------------------------------------------
# Wait for the work to actually FINISH rather than sampling after a fixed tail. A fixed wait reads
# the logs mid-flight, so every count is a snapshot of a moving system and the totals never
# reconcile. Poll until neither the live pump nor NG recovery has completed anything for a full
# minute, then everything outstanding is genuinely outstanding.
function Get-Completed {
    $n = 0
    foreach ($f in (Get-ChildItem $logDir -Filter '*_rawlog.txt' -ErrorAction SilentlyContinue)) {
        $n += @(Get-Content $f.FullName | Where-Object { $_ -match '\|SUCCEEDED\|' }).Count
    }
    foreach ($f in (Get-ChildItem $logDir -Filter '*_ngretrylog.txt' -ErrorAction SilentlyContinue)) {
        $n += @(Get-Content $f.FullName | Where-Object { $_ -match '\|SUCCEEDED\|' }).Count
    }
    return $n
}

Write-Host "Step 5/6: draining - waiting for uploads + NG recovery to stop progressing (max ${DrainMinutes}m)..."
$deadline = (Get-Date).AddMinutes($DrainMinutes)
$last = -1; $flat = 0; $drained = $false; $died = $false
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 20
    # A dead app also stops making progress. Without this check a CRASH reads as a clean drain,
    # and every count below gets reported as a settled final state when it is really a wreck.
    if (-not (Get-Process FtpUpload -ErrorAction SilentlyContinue)) {
        $died = $true
        Write-Host "   *** FtpUpload.exe is NO LONGER RUNNING - it exited during the drain. ***"
        break
    }
    $now = Get-Completed
    $delta = if ($last -lt 0) { 0 } else { $now - $last }
    Write-Host ("   {0}  completed {1,6}  (+{2})" -f (Get-Date -Format 'HH:mm:ss'), $now, $delta)
    if ($now -eq $last) { $flat++ } else { $flat = 0 }
    $last = $now
    if ($flat -ge 3) { $drained = $true; Write-Host "   no progress for 60s - drained."; break }
}
if (-not $drained -and -not $died) { Write-Host "   drain window expired - work was still moving; totals below are a snapshot." }

# ---- 5. restore config -----------------------------------------------------
if (-not $KeepConfig) {
    Write-TextShared $cfgPath (Get-Content $backup -Raw) | Out-Null
    Remove-Item $backup -Force            # gone = this run restored cleanly
    Write-Host "Step 6/6: config.json restored from backup."
} else {
    Write-Host "Step 6/6: -KeepConfig - test config.json left in place (backup at $(Split-Path $backup -Leaf))."
}

# ---- 6. verdict from the oplog ---------------------------------------------
Write-Host ''
Write-Host '=== RESULT ==============================================================='

# The real assertion is requested == completed: a rollover that was requested and never
# completed IS the stall. The COUNT of rollovers is timing-dependent (tick pacing, app
# start-up), so it is reported but never failed on - asserting on it just produces flaky
# runs. Give a late rollover a moment to land first, so we don't read mid-flight.
Start-Sleep -Seconds 5

$opFiles = Get-ChildItem $logDir -Filter '*_oplog.txt' -ErrorAction SilentlyContinue
$op = @()
foreach ($f in $opFiles) { $op += Get-Content $f.FullName }

$requested = @($op | Where-Object { $_ -match 'ROLLOVER REQUESTED' })
$complete  = @($op | Where-Object { $_ -match 'ROLLOVER COMPLETE'  })
$blocked   = @($op | Where-Object { $_ -match 'ROLLOVER BLOCKED'   })
$pumpErr   = @($op | Where-Object { $_ -match 'PUMP ERROR'         })

Write-Host ("  rollovers requested : {0}" -f $requested.Count)
Write-Host ("  rollovers completed : {0}" -f $complete.Count)
Write-Host ("  rollovers blocked   : {0}   (grace period fired - transfer was wedged)" -f $blocked.Count)
Write-Host ("  pump errors caught  : {0}" -f $pumpErr.Count)

# Day-advance lag. The rollover itself is fast (request -> complete is sub-second); what shows
# watch-loop starvation is how late the NEXT day arrives. Simulated days are DaySeconds apart, so
# any gap much larger than that means the loop was blocked - the failure mode this test exists for.
function Get-Stamp([string]$line) {
    if ($line -match '^\W*\[(.+?)\]') { return [datetime]::Parse($Matches[1]) }
    return $null
}
$stamps = @($requested | ForEach-Object { Get-Stamp $_ } | Where-Object { $_ } | Sort-Object)
$gaps = @()
for ($i = 1; $i -lt $stamps.Count; $i++) { $gaps += [math]::Round(($stamps[$i] - $stamps[$i-1]).TotalSeconds, 1) }
$maxGap = 0; $lag = 0
if ($gaps.Count -gt 0) {
    $maxGap = ($gaps | Measure-Object -Maximum).Maximum
    $avgGap = [math]::Round((($gaps | Measure-Object -Average).Average), 1)
    $lag    = [math]::Round($maxGap / $DaySeconds, 1)
    Write-Host ''
    Write-Host ("  day-advance gap     : avg {0}s   max {1}s   (expected ~{2}s -> worst lag {3}x, limit {4}x)" -f $avgGap, $maxGap, $DaySeconds, $lag, $StallFactor)
    Write-Host ("  gaps                : {0}" -f ($gaps -join ', '))
}
$gapStall = ($gaps.Count -gt 0 -and $maxGap -gt ($DaySeconds * $StallFactor))

Write-Host ''
foreach ($l in $complete) { Write-Host "    $l" }
foreach ($l in $blocked)  { Write-Host "    $l" }
foreach ($l in $pumpErr)  { Write-Host "    $l" }
Write-Host ''

# Per-day throughput. ADVISORY ONLY - see the verdict block for why. Under this synthetic load
# (hundreds of files crammed into a DaySeconds-long "day" against a possibly-dead host) a given
# day landing on zero is machine noise, not the bug: the live pump is not even reachable from the
# NG ordering or rollover code these runs are exercising. Reported, never failed on.
$jobsDir = Resolve-Cfg ([string]$cfg.JobsFolder) 'jobs'
$dayFiles = Get-ChildItem $jobsDir -Filter '*_jobs.txt' -ErrorAction SilentlyContinue | Sort-Object Name
Write-Host '  per simulated day (jobs staged -> files uploaded)   [advisory]:'
$quietDays = @()
$totalJobs = 0; $totalUploaded = 0
foreach ($jf in $dayFiles) {
    $day  = $jf.Name.Substring(0,8)
    $raw  = Join-Path $logDir "${day}_rawlog.txt"
    $njob = @(Get-Content $jf.FullName -ErrorAction SilentlyContinue).Count
    $nok  = 0
    if (Test-Path $raw) { $nok = @(Get-Content $raw | Where-Object { $_ -match '\|SUCCEEDED\|' }).Count }
    $totalJobs += $njob; $totalUploaded += $nok
    $flag = ''
    if ($njob -gt 0 -and $nok -eq 0) { $flag = '   <-- quiet day'; $quietDays += $day }
    Write-Host ("    {0}   {1,5} job lines   {2,5} uploaded{3}" -f $day, $njob, $nok, $flag)
}
Write-Host ("    {0,-8}   {1,5} job lines   {2,5} uploaded" -f 'TOTAL', $totalJobs, $totalUploaded)
# Only a run-wide zero proves the pump was dead; a single quiet day does not.
$pumpDead = ($totalJobs -gt 0 -and $totalUploaded -eq 0)
Write-Host ''

# ---- NG recovery across the rollover ---------------------------------------
# Files still unfinished at a rollover are marked TIMEDOUT into the OLD day's rawlog, which is what
# puts them in that day's NG list. The question this answers: does the NG pump then actually work
# them, or do they just sit there? Recovery is recorded in {day}_ngretrylog.txt (SUCCEEDED = sent).
Write-Host '  NG recovery per day (sent to NG at rollover -> retried -> recovered):'
$ngTotal = 0; $ngOk = 0; $ngTried = 0; $ngLeft = 0
$ngQuiet = @()
foreach ($jf in $dayFiles) {
    $day = $jf.Name.Substring(0,8)
    $raw = Join-Path $logDir "${day}_rawlog.txt"
    $ng  = Join-Path $logDir "${day}_ngretrylog.txt"

    # last rawlog line per PID|File wins - that is how the app itself derives the NG list
    $final = @{}
    if (Test-Path $raw) {
        foreach ($line in Get-Content $raw) {
            $p = $line.Split('|')
            if ($p.Length -lt 3) { continue }
            $final[$p[0] + '|' + $p[1]] = $p[2]
        }
    }
    $inNg = @($final.Values | Where-Object { $_ -eq 'TIMEDOUT' -or $_ -eq 'FAILED' }).Count

    $tried = 0; $ok = 0
    if (Test-Path $ng) {
        $seen = @{}; $good = @{}
        foreach ($line in Get-Content $ng) {
            $p = $line.Split('|')
            if ($p.Length -lt 3) { continue }
            $k = $p[0] + '|' + $p[1]
            $seen[$k] = $true
            if ($p[2] -eq 'SUCCEEDED') { $good[$k] = $true }
        }
        $tried = $seen.Count; $ok = $good.Count
    }
    $ngTotal += $inNg; $ngOk += $ok; $ngTried += $tried
    # NG recovery is written ONLY to the ng-retry log, never back into the rawlog, so a recovered
    # file still reads TIMEDOUT there forever. "in NG" is therefore ever-sent-to-NG, not still-owed.
    # The app gets this right (BuildItems consults the ng-retry log); anything reading the rawlog
    # alone - this script, or a day report built the same way - must subtract recoveries.
    $left = $inNg - $ok
    $ngLeft += $left

    $note = ''
    if ($inNg -gt 0 -and $tried -eq 0) { $note = '   <-- no retries this day'; $ngQuiet += $day }
    Write-Host ("    {0}   {1,5} to NG   {2,5} retried   {3,5} recovered   {4,5} outstanding{5}" -f $day, $inNg, $tried, $ok, $left, $note)
}
Write-Host ("    {0,-8}   {1,5} to NG   {2,5} retried   {3,5} recovered   {4,5} outstanding" -f 'TOTAL', $ngTotal, $ngTried, $ngOk, $ngLeft)
$ngIdle = ($ngTotal -gt 0 -and $ngTried -eq 0)
Write-Host ''

# ---- verdict ---------------------------------------------------------------
# FAIL only on evidence of the bug this test exists for. Every hard condition below is either
# structural (a rollover that never settled) or run-wide (nothing moved at all) - none of them can
# be tripped by one unlucky day on a busy machine. Per-day throughput dips are reported as
# warnings: earlier revisions failed on those and produced FAILs that had nothing to do with the
# rollover, which is worse than useless in a regression guard.
$hard = @()
if ($died)                                   { $hard += "FtpUpload.exe EXITED mid-run - check the Windows Application event log for the crash." }
if ($requested.Count -lt 1)                  { $hard += "no rollover fired at all - is SimulateFastDaySeconds set?" }
if ($complete.Count -lt $requested.Count)    { $hard += "a rollover was requested but never completed (the LGD stall)." }
if ($gapStall)                               { $hard += ("watch loop starved: worst day-advance gap {0}s vs {1}s expected (>{2}x limit)." -f $maxGap, $DaySeconds, $StallFactor) }
if ($pumpDead)                               { $hard += "jobs were staged but NOTHING uploaded across the whole run." }
if ($ngIdle)                                 { $hard += "files were sent to NG but the NG pump never retried any of them." }

if ($hard.Count -eq 0) {
    Write-Host "  PASS - every rollover settled, the watch loop kept pace, and NG recovery ran." -ForegroundColor Green
} else {
    Write-Host "  FAIL" -ForegroundColor Red
    foreach ($h in $hard) { Write-Host "         $h" }
}

# Advisory notes - never affect the verdict.
if ($requested.Count -lt $Days) {
    Write-Host ("  note: only {0} of {1} rollovers fired in the window - timing, not a failure." -f $requested.Count, $Days)
}
if ($quietDays.Count -gt 0) {
    Write-Host ("  note: days with no uploads: {0}" -f ($quietDays -join ', '))
    Write-Host "        expected under this synthetic load; only a run-wide zero indicates a dead pump."
}
if ($ngQuiet.Count -gt 0) {
    Write-Host ("  note: days with NG items but no retries: {0}" -f ($ngQuiet -join ', '))
    Write-Host ("        days older than NgRecoveryDays fall outside the window and are expected here.")
}
Write-Host ''
Write-Host '  The app is still running - check the header Day badge and the live log.'
Write-Host '  Stop it with stop_worker.bat when you are done.'
Write-Host '========================================================================='
Write-Host ''

# Non-zero on failure so this can gate a build or run unattended.
exit ($(if ($hard.Count -eq 0) { 0 } else { 1 }))
