<#
.SYNOPSIS
    Clone one template panel into N test panels for exercising FTPRecovery.

.DESCRIPTION
    Takes a folder holding the queue .txt files of ONE real panel and produces
    N copies with fresh PIDs, plus small dummy source files on disk so the
    uploads have something to send.

    The PID is substituted textually everywhere it occurs - inside the queue
    filename and inside every line of the queue body - so all the derived
    paths (source, dest, index, host) follow automatically without this
    script needing to know the folder layout.

.EXAMPLE
    # dry run first - shows what it would create
    .\MakeTestQueues.ps1 -Template D:\test\template -Root D:\test\UploadQueue -Count 1000

    # actually create, 4 KB dummy files, 6 of each panel pre-recorded in the host
    .\MakeTestQueues.ps1 -Template D:\test\template -Root D:\test\UploadQueue `
        -Count 1000 -FileSizeKB 4 -SeedRecorded 6 -HostOverride 127.0.0.1 -Go

    # remove everything this script made
    .\MakeTestQueues.ps1 -Root D:\test\UploadQueue -Clean -Go
#>

[CmdletBinding()]
param(
    [string]$Template,
    [Parameter(Mandatory=$true)][string]$Root,
    [int]$Count = 1000,
    [int]$StartIndex = 1,
    [string]$PidPrefix = "TSTPID",
    [int]$PidDigits = 6,
    [string]$OldPid = "",
    [string]$OldServerPid = "",
    [string]$ServerPidPrefix = "TSN",
    [int]$ServerPidDigits = 12,
    [string]$OldStamp = "",
    [string]$OldServerStamp = "",
    [datetime]$StampBase = [datetime]::MinValue,
    [datetime]$ServerStampBase = [datetime]::MinValue,
    [int]$StampStepSec = 37,
    [int]$FileSizeKB = 4,
    [int]$SeedRecorded = 0,
    [int]$SeedFailed = 0,
    [switch]$Random,
    [int]$RandomSeed = 20260819,
    [int]$PctFresh = 20,
    [int]$PctPartial = 25,
    [int]$PctDup = 15,
    [int]$PctRetry = 20,
    [int]$PctOrphanFail = 10,
    [int]$PctIncomplete = 10,
    [int]$PctMissingSource = 15,
    # Scenarios taken from the real LGD production report, which the earlier
    # weights did not cover at all:
    #   ALLGONE     every image deleted, all queue files present  (1453 panels)
    #   MOSTLYLOST  nearly every queue file gone, images intact    (322 panels)
    #   INDEXONLY   only the index/host queue file left            (12 panels)
    [int]$PctAllGone = 20,
    [int]$PctMostlyLost = 10,
    [int]$PctIndexOnly = 2,
    # Real panels are not all the same size - the report shows 10, 12, 16 and 17
    # file recipes. Off by default so existing tests keep the template's own count.
    [switch]$VaryTotal,
    [int[]]$TotalSizes = @(10, 12, 16, 17),
    [string]$HostOverride = "",
    [string]$WinScpOverride = "",
    [switch]$FixTotal,
    [switch]$Clean,
    [switch]$Go
)

$ErrorActionPreference = "Stop"
$script:created = 0
$script:fakes   = 0
$script:rng     = New-Object System.Random $RandomSeed
$script:tally   = @{}

# Weighted pick of a panel scenario. Each models a real state the queue folder
# can end up in, so the recovery tool gets exercised on all of them.
function Pick-Scenario {
    $names   = @('FRESH','PARTIAL','DUP','RETRY','ORPHANFAIL','INCOMPLETE',
                 'ALLGONE','MOSTLYLOST','INDEXONLY')
    $weights = @($PctFresh, $PctPartial, $PctDup, $PctRetry, $PctOrphanFail, $PctIncomplete,
                 $PctAllGone, $PctMostlyLost, $PctIndexOnly)
    $total = 0
    foreach ($w in $weights) { $total += $w }
    if ($total -le 0) { return 'FRESH' }
    $roll = $script:rng.Next(0, $total)
    $acc = 0
    for ($z = 0; $z -lt $names.Count; $z++) {
        $acc += $weights[$z]
        if ($roll -lt $acc) { return $names[$z] }
    }
    return 'FRESH'
}

function Bump([string]$key) {
    if ($script:tally.ContainsKey($key)) { $script:tally[$key]++ } else { $script:tally[$key] = 1 }
}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Ensure-Dir([string]$path) {
    if ([string]::IsNullOrWhiteSpace($path)) { return }
    if (-not (Test-Path -LiteralPath $path)) {
        if ($Go) { New-Item -ItemType Directory -Path $path -Force | Out-Null }
    }
}

function Ensure-DirFor([string]$filePath) {
    Ensure-Dir ([System.IO.Path]::GetDirectoryName($filePath))
}

# Write a dummy file of the requested size, filled with a repeating pattern so
# it compresses badly and behaves a bit like a real image on the wire.
$script:blob = $null
function New-FakeFile([string]$path, [int]$sizeKB) {
    if (Test-Path -LiteralPath $path) { return }
    if ($null -eq $script:blob) {
        $n = [Math]::Max(1, $sizeKB) * 1024
        $script:blob = New-Object byte[] $n
        $rng = New-Object System.Random 12345
        $rng.NextBytes($script:blob)
    }
    Ensure-DirFor $path
    if ($Go) { [System.IO.File]::WriteAllBytes($path, $script:blob) }
    $script:fakes++
}

# ---------------------------------------------------------------------------
# Clean mode
# ---------------------------------------------------------------------------

if ($Clean) {
    Write-Host "Cleaning test queues under $Root ..."
    $targets = Get-ChildItem -LiteralPath $Root -Filter "*$PidPrefix*" -Recurse -ErrorAction SilentlyContinue
    Write-Host ("  {0} item(s) match '*{1}*'" -f $targets.Count, $PidPrefix)
    if ($Go) {
        $targets | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
        Write-Host "  removed."
    } else {
        Write-Host "  DRY RUN - add -Go to delete."
    }
    return
}

# ---------------------------------------------------------------------------
# Load and validate the template panel
# ---------------------------------------------------------------------------

if ([string]::IsNullOrWhiteSpace($Template)) {
    throw "-Template is required unless -Clean is used."
}
if (-not (Test-Path -LiteralPath $Template)) {
    throw "Template folder not found: $Template"
}

$tplFiles = @(Get-ChildItem -LiteralPath $Template -Filter *.txt -File)
if ($tplFiles.Count -eq 0) {
    throw "No .txt queue files found in $Template"
}

# Parse each template queue file; keep only well-formed ones (>= 17 lines).
$tpl = @()
foreach ($f in $tplFiles) {
    $lines = [System.IO.File]::ReadAllLines($f.FullName)
    if ($lines.Count -lt 17) {
        Write-Warning "  skipping $($f.Name): only $($lines.Count) lines"
        continue
    }
    $tpl += [pscustomobject]@{
        Name  = $f.Name
        Lines = $lines
    }
}
if ($tpl.Count -eq 0) { throw "No valid 17-line queue files in template." }

# Separate data queues from index/host queues (line 7 = source file).
$dataTpl = @($tpl | Where-Object {
    $_.Lines[7] -ne $_.Lines[10] -and $_.Lines[7] -ne $_.Lines[13]
})
$idxTpl = @($tpl | Where-Object {
    $_.Lines[7] -eq $_.Lines[10] -or $_.Lines[7] -eq $_.Lines[13]
})

# Server PID = filename of sourceIndexFile (line 10) - this is what
# FTPUploaderVB calls the PID, and it drives every remote destination path.
if ([string]::IsNullOrWhiteSpace($OldServerPid)) {
    $OldServerPid = [System.IO.Path]::GetFileNameWithoutExtension($tpl[0].Lines[10].Trim())
}
# File PID is the local naming token (e.g. AAA). No reliable derivation - must be given.
if ([string]::IsNullOrWhiteSpace($OldPid)) {
    throw "Pass -OldPid : the local file PID token used in the template (e.g. AAA)."
}
if ([string]::IsNullOrWhiteSpace($OldServerPid)) {
    throw "Could not derive the server PID from line 10. Pass -OldServerPid explicitly."
}

# Timestamps: 14-digit yyyyMMddHHmmss runs. The local one lives in the source
# folder (line 7); the server one lives in the host destination (line 14).
if ([string]::IsNullOrWhiteSpace($OldStamp)) {
    $m = [regex]::Match($tpl[0].Lines[7], '\d{14}')
    if ($m.Success) { $OldStamp = $m.Value }
}
if ([string]::IsNullOrWhiteSpace($OldServerStamp)) {
    $m = [regex]::Match($tpl[0].Lines[14], '\d{14}')
    if ($m.Success) { $OldServerStamp = $m.Value }
}

# Anchor the stamp bases to the template's own timestamps so panel N always
# gets the SAME generated stamp no matter when or how often this script runs.
# (Defaulting to Get-Date would make a second pass over a subset produce new
# folders and orphan the ones from the first pass.)
if ($StampBase -eq [datetime]::MinValue -and $OldStamp) {
    $StampBase = [datetime]::ParseExact($OldStamp, 'yyyyMMddHHmmss', $null)
}
if ($ServerStampBase -eq [datetime]::MinValue -and $OldServerStamp) {
    $ServerStampBase = [datetime]::ParseExact($OldServerStamp, 'yyyyMMddHHmmss', $null)
}

Write-Host "Template     : $Template"
Write-Host "  queue files: $($tpl.Count)  (data: $($dataTpl.Count), index/host: $($idxTpl.Count))"
Write-Host "  file PID   : $OldPid  ->  ${PidPrefix}nnnnnn"
Write-Host "  server PID : $OldServerPid  ->  ${ServerPidPrefix}nnnnnnnnnnnn"
Write-Host "  local stamp: $OldStamp  ->  varies per panel"
Write-Host "  srvr stamp : $OldServerStamp  ->  varies per panel"
Write-Host "  line15 total: $($tpl[0].Lines[15].Trim())"
Write-Host "Target root  : $Root"
Write-Host "Panels       : $Count  starting at $StartIndex"
Write-Host "Dummy size   : $FileSizeKB KB"
Write-Host "Mode         : $(if ($Go) { 'CREATE' } else { 'DRY RUN (add -Go)' })"
Write-Host ""

# Sanity check: a short or numeric PID risks matching unrelated text.
if ($OldPid.Length -lt 4) {
    Write-Warning "Template PID '$OldPid' is very short - textual replacement may hit unintended matches."
}
if ($dataTpl.Count -ne [int]$tpl[0].Lines[15].Trim() -and -not $FixTotal) {
    Write-Warning ("Template has {0} data queue files but line 15 says {1}. Use -FixTotal to rewrite line 15 to the real count." -f $dataTpl.Count, $tpl[0].Lines[15].Trim())
}

Ensure-Dir $Root

$sw = [System.Diagnostics.Stopwatch]::StartNew()

for ($i = 0; $i -lt $Count; $i++) {

    $newPid = "{0}{1}" -f $PidPrefix, ($StartIndex + $i).ToString().PadLeft($PidDigits, '0')
    $newSrv = "{0}{1}" -f $ServerPidPrefix, ($StartIndex + $i).ToString().PadLeft($ServerPidDigits, '0')

    # Each panel gets its own measurement time, walking backwards from the base
    # so later panels look older - mirrors a real queue that built up over time.
    $newStamp    = $StampBase.AddSeconds(       -1 * ($StartIndex + $i) * $StampStepSec).ToString("yyyyMMddHHmmss")
    $newSrvStamp = $ServerStampBase.AddSeconds( -1 * ($StartIndex + $i) * $StampStepSec).ToString("yyyyMMddHHmmss")

    $hostSrc  = ""
    $indexSrc = ""
    $records  = New-Object System.Collections.Generic.List[string]
    $qPaths   = New-Object System.Collections.Generic.List[string]
    $srcPaths = New-Object System.Collections.Generic.List[string]

    # Real panels come in several recipe sizes. Take the first N template files and
    # write N into line 15, so totalFileCount genuinely varies between panels.
    $useTpl = $dataTpl
    if ($VaryTotal) {
        $want = $TotalSizes[$script:rng.Next(0, $TotalSizes.Count)]
        if ($want -lt $dataTpl.Count) { $useTpl = $dataTpl[0..($want - 1)] }
    }
    $panelTotal = $useTpl.Count

    foreach ($t in $useTpl) {

        $lines = @($t.Lines | ForEach-Object {
            $s = $_.Replace($OldPid, $newPid).Replace($OldServerPid, $newSrv)
            if ($OldStamp)       { $s = $s.Replace($OldStamp, $newStamp) }
            if ($OldServerStamp) { $s = $s.Replace($OldServerStamp, $newSrvStamp) }
            $s
        })

        if ($HostOverride)    { $lines[0] = $HostOverride }
        if ($WinScpOverride)  { $lines[3] = $WinScpOverride }
        if ($FixTotal -or $VaryTotal) { $lines[15] = $panelTotal.ToString() }

        $srcFile  = $lines[7].Trim()
        $destFile = $lines[8].Trim()
        $channel  = $lines[16].Trim()
        $indexSrc = $lines[10].Trim()
        $hostSrc  = $lines[13].Trim()

        # dummy image on disk
        New-FakeFile $srcFile $FileSizeKB
        $records.Add("$destFile@$channel") | Out-Null

        # the queue file itself
        $qName = $t.Name.Replace($OldPid, $newPid).Replace($OldServerPid, $newSrv)
        if ($OldStamp)       { $qName = $qName.Replace($OldStamp, $newStamp) }
        if ($OldServerStamp) { $qName = $qName.Replace($OldServerStamp, $newSrvStamp) }
        $qPath = Join-Path $Root $qName
        if ($Go) { [System.IO.File]::WriteAllLines($qPath, $lines) }
        $qPaths.Add($qPath)   | Out-Null
        $srcPaths.Add($srcFile) | Out-Null
        $lastLines = $lines       # kept for INDEXONLY, which needs lines 9-14
        $script:created++
    }

    # -----------------------------------------------------------------------
    # RANDOM MODE - give this panel one of six real-world states.
    # -----------------------------------------------------------------------
    if ($Random) {

        $sc = Pick-Scenario
        Bump $sc
        $n = $records.Count
        $seedLines = New-Object System.Collections.Generic.List[string]
        $killQueues = New-Object System.Collections.Generic.List[int]
        $script:killAllImages   = $false
        $script:writeIndexQueue = $false

        switch ($sc) {

            'FRESH' {
                # nothing recorded, every queue file present - the normal state
            }

            'PARTIAL' {
                # host/index hold clean records and those queue files are gone.
                # This is the classic stall: count short, remainder pending.
                $k = $script:rng.Next([int]($n * 0.3), [int]($n * 0.85))
                for ($j = 0; $j -lt $k; $j++) {
                    $seedLines.Add($records[$j]) | Out-Null
                    $killQueues.Add($j) | Out-Null
                }
            }

            'DUP' {
                # host/index hold clean records but the queue files are STILL
                # there - re-running must NOT upload these again.
                $k = $script:rng.Next(2, [Math]::Max(3, [int]($n * 0.4)))
                for ($j = 0; $j -lt $k; $j++) { $seedLines.Add($records[$j]) | Out-Null }
            }

            'RETRY' {
                # " - failed" placeholders WITH live queue files - must retry
                # and replace the placeholder rather than append.
                $k = $script:rng.Next(1, 5)
                for ($j = 0; $j -lt $k; $j++) {
                    $seedLines.Add($records[$j] + " - failed") | Out-Null
                }
            }

            'ORPHANFAIL' {
                # clean records plus placeholders, all their queue files gone.
                # Placeholders get stripped at fire time and never upload.
                $ok = $script:rng.Next([int]($n * 0.5), $n - 2)
                $bad = $script:rng.Next(1, 4)
                for ($j = 0; $j -lt $ok; $j++) {
                    $seedLines.Add($records[$j]) | Out-Null
                    $killQueues.Add($j) | Out-Null
                }
                for ($j = $ok; $j -lt [Math]::Min($ok + $bad, $n); $j++) {
                    $seedLines.Add($records[$j] + " - failed") | Out-Null
                    $killQueues.Add($j) | Out-Null
                }
            }

            'INCOMPLETE' {
                # queue files vanished with nothing recorded - can never reach
                # totalFileCount, so recovery should refuse without -force.
                $k = $script:rng.Next(2, 6)
                for ($j = 0; $j -lt $k; $j++) { $killQueues.Add($j) | Out-Null }
            }

            'ALLGONE' {
                # EVERY image deleted from disk, all queue files present. This is
                # the 1,453-panel case from the LGD report: without a guard the
                # tool marks all of them failed, strips every placeholder and
                # uploads an EMPTY manifest. Recovery must refuse to send.
                $script:killAllImages = $true
            }

            'MOSTLYLOST' {
                # Nearly every queue file gone, images still on disk - the 322-panel
                # case (Total 17, Pending 1). Reconstruction should recover these.
                $keep = $script:rng.Next(1, 4)
                for ($j = 0; $j -lt ($n - $keep); $j++) { $killQueues.Add($j) | Out-Null }
            }

            'INDEXONLY' {
                # Every data queue file gone, but an index/host queue file left
                # behind - the 12-panel case, where the panel is visible with
                # nothing pending.
                for ($j = 0; $j -lt $n; $j++) { $killQueues.Add($j) | Out-Null }
                $script:writeIndexQueue = $true
            }
        }

        # Independently, some panels lose local source files from disk.
        if ($script:rng.Next(0, 100) -lt $PctMissingSource) {
            Bump 'missing-source'
            $howMany = $script:rng.Next(1, 4)
            for ($m = 0; $m -lt $howMany; $m++) {
                $pick = $script:rng.Next(0, $n)
                if ($killQueues.Contains($pick)) { continue }
                if ($Go -and (Test-Path -LiteralPath $srcPaths[$pick])) {
                    Remove-Item -LiteralPath $srcPaths[$pick] -Force
                }
            }
        }

        if ($Go) {
            if ($seedLines.Count -gt 0) {
                Ensure-DirFor $indexSrc
                Ensure-DirFor $hostSrc
                [System.IO.File]::WriteAllLines($indexSrc, $seedLines)
                [System.IO.File]::WriteAllLines($hostSrc,  $seedLines)
            }
            foreach ($j in $killQueues) {
                if (Test-Path -LiteralPath $qPaths[$j]) {
                    Remove-Item -LiteralPath $qPaths[$j] -Force
                }
            }
            # ALLGONE: wipe every image, leaving the queue files intact.
            if ($script:killAllImages) {
                foreach ($sp in $srcPaths) {
                    if (Test-Path -LiteralPath $sp) { Remove-Item -LiteralPath $sp -Force }
                }
            }
            # INDEXONLY: leave an index/host queue file so the panel is still
            # discovered even though no data queue file remains.
            if ($script:writeIndexQueue -and $lastLines) {
                foreach ($pair in @(@($lastLines[9],  $lastLines[10], $lastLines[11]),
                                    @($lastLines[12], $lastLines[13], $lastLines[14]))) {
                    if ([string]::IsNullOrWhiteSpace($pair[0])) { continue }
                    $ql = @($lastLines)
                    $ql[7] = $pair[1]
                    $ql[8] = $pair[2]
                    Ensure-DirFor $pair[0]
                    [System.IO.File]::WriteAllLines($pair[0], $ql)
                }
            }
        }
    }
    # -----------------------------------------------------------------------
    # Explicit seeding (non-random): first N recorded, next M failed.
    # -----------------------------------------------------------------------
    elseif ($SeedRecorded -gt 0 -or $SeedFailed -gt 0) {

        $seedLines = New-Object System.Collections.Generic.List[string]
        $take = [Math]::Min($SeedRecorded, $records.Count)
        for ($k = 0; $k -lt $take; $k++) { $seedLines.Add($records[$k]) | Out-Null }

        $failFrom = $take
        $failTo   = [Math]::Min($take + $SeedFailed, $records.Count)
        for ($k = $failFrom; $k -lt $failTo; $k++) {
            $seedLines.Add($records[$k] + " - failed") | Out-Null
        }

        if ($Go -and $seedLines.Count -gt 0) {
            Ensure-DirFor $indexSrc
            Ensure-DirFor $hostSrc
            [System.IO.File]::WriteAllLines($indexSrc, $seedLines)
            [System.IO.File]::WriteAllLines($hostSrc,  $seedLines)

            for ($k = 0; $k -lt $failTo; $k++) {
                if (Test-Path -LiteralPath $qPaths[$k]) {
                    Remove-Item -LiteralPath $qPaths[$k] -Force
                }
            }
        }
    }

    if ((($i + 1) % 50) -eq 0) {
        Write-Host ("  ... {0} / {1} panels  ({2:n1}s)" -f ($i + 1), $Count, $sw.Elapsed.TotalSeconds)
    }
}

$sw.Stop()

Write-Host ""
Write-Host "================ DONE ================"
Write-Host "Panels        : $Count"
Write-Host "Queue files   : $script:created"
Write-Host "Dummy files   : $script:fakes"
Write-Host ("Elapsed       : {0:n1}s" -f $sw.Elapsed.TotalSeconds)
if ($Random -and $script:tally.Count -gt 0) {
    Write-Host ""
    Write-Host "Scenario mix (seed $RandomSeed):"
    foreach ($k in ($script:tally.Keys | Sort-Object)) {
        Write-Host ("  {0,-15} {1}" -f $k, $script:tally[$k])
    }
}
if (-not $Go) {
    Write-Host ""
    Write-Host "DRY RUN - nothing was written. Re-run with -Go."
} else {
    Write-Host ""
    Write-Host "Now test with:  FTPRecovery.exe -root `"$Root`""
}
