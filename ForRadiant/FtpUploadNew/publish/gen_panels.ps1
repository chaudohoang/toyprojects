# =============================================================================
#  gen_panels.ps1 - test harness for the NEW .panel intake (100 panels default).
#
#  Wipes a demo tree, writes per-panel source folders + fake files, drops a
#  {PID}_{DateTime}.panel handoff for each into the QueueFolder, writes a recipe
#  (allowed_filenames.txt) beside the exe, points config.json at it all, and
#  launches FtpUpload so you can watch the panels ingest and upload.
#
#  Scenarios (weighted, adapted to the new design where THIS tool makes manifests):
#    FULL      all recipe files present, no manifest    -> ingest, upload, finalize
#    MISSING   a few recipe files absent from folder    -> count = folder n recipe
#    JUNK      extra non-recipe files in the folder      -> filtered out, not counted
#    RESUME    a partial {PID}.idx/host already on disk  -> create-or-resume
#    NOTREADY  .panel written WITHOUT SourceFolder       -> intake skips (phase-1)
# =============================================================================
param(
    [int]$Panels      = 100,
    [int]$FileKB      = 1,
    [int]$FailPercent = 30,
    [int]$RandomSeed  = 20260821,
    [int]$PctFull     = 45,
    [int]$PctMissing  = 20,
    [int]$PctJunk     = 15,
    [int]$PctResume   = 15,
    [int]$PctNotReady = 5,
    [switch]$NoLaunch
)

# Publish folder = where FtpUpload.exe + config.json live. Derived from this script's location so
# it works whether the script runs from the project root (use the .\publish subfolder) or from
# inside publish itself (use its own folder).
$pub = if (Test-Path (Join-Path $PSScriptRoot 'publish\FtpUpload.exe')) { Join-Path $PSScriptRoot 'publish' } else { $PSScriptRoot }
$cfgPath = Join-Path $pub 'config.json'
# If there's no config yet (fresh publish / after a full clean), seed it from the known-good
# config.default.json so gen works with your real settings instead of bare code defaults.
if (-not (Test-Path $cfgPath)) {
    $seed = Join-Path $pub 'config.default.json'
    if (-not (Test-Path $seed)) { $seed = Join-Path $PSScriptRoot 'config.default.json' }
    if (Test-Path $seed) {
        Copy-Item $seed $cfgPath -Force
        Write-Host "Seeded config.json from config.default.json (known-good values)."
    }
}
if (-not (Test-Path $cfgPath)) {
    Write-Host "config.json not found at $cfgPath"
    Write-Host "Launch FtpUpload once (or run build.bat) so it creates the config, then re-run this script."
    exit 1
}
$cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json

# Resolve a possibly-relative config path against the publish (exe) folder, exactly like the app.
function Resolve-Cfg([string]$p, [string]$fallback) {
    if ([string]::IsNullOrWhiteSpace($p)) { $p = $fallback }
    if ([System.IO.Path]::IsPathRooted($p)) { return $p }
    return (Join-Path $pub $p)
}
# Read (never write) the queue + recipe from the app's own config. This script no longer touches
# config.json, the recipe, or the app's jobs/logs/state -- it only drops test .panel files into the
# config's real QueueFolder and writes their fake source files into a scratch tree.
$queue   = Resolve-Cfg ([string]$cfg.QueueFolder) 'queue'
$recipe  = Resolve-Cfg ([string]$cfg.RecipePath)  'allowed_filenames.txt'
$scratch = 'D:\FtpUploadDemo\src'

# Seed the recipe too if it's missing, so the canonical test filenames have patterns to match.
if (-not (Test-Path $recipe)) {
    $rseed = Join-Path $PSScriptRoot 'allowed_filenames.txt'
    if ((Test-Path $rseed) -and ($rseed -ne $recipe)) {
        Copy-Item $rseed $recipe -Force
        Write-Host "Seeded recipe at $recipe from allowed_filenames.txt."
    }
}

$rng   = New-Object System.Random $RandomSeed
$tally = @{}
function Bump([string]$k){ if($tally.ContainsKey($k)){$tally[$k]++}else{$tally[$k]=1} }

function Pick-Scenario {
    $names   = @('FULL','MISSING','JUNK','RESUME','NOTREADY')
    $weights = @($PctFull,$PctMissing,$PctJunk,$PctResume,$PctNotReady)
    $total = 0; foreach($w in $weights){ $total += $w }
    if ($total -le 0) { return 'FULL' }
    $roll = $rng.Next(0,$total); $acc = 0
    for ($z=0; $z -lt $names.Count; $z++){ $acc += $weights[$z]; if($roll -lt $acc){ return $names[$z] } }
    return 'FULL'
}

# Canonical per-panel file set (matches the recipe below); JUNK adds non-matching names.
$canonical = @(
    'puc_otp_read.txt','d994_gamma.hex','1st_nypucdata.hex','2nd_nypucdata.hex',
    'step01_R064.tif','step02_G064.tif','step03_B064.tif','step04_W064.tif',
    'step05_R128.tif','step06_W255.tif'
)
$junkNames = @('readme.log','preview.bmp','sequence.xml','Thumbs.db')

# stop any running worker so it re-scans cleanly (optional; harmless if not running)
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# fresh SCRATCH tree only (the fake per-panel source files). The real QueueFolder is NOT wiped --
# test panels are ADDED to it -- and config.json / recipe / jobs / logs / state are left untouched.
Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $scratch | Out-Null
New-Item -ItemType Directory -Force -Path $queue   | Out-Null

# NOTE: the recipe is NOT overwritten. This uses whatever recipe the app already points at
# ($recipe). The canonical fake filenames below (otp / gamma / nypucdata / step*.tif) must match
# that recipe's patterns for intake to pick them up.

# ---- fake file blob ---------------------------------------------------------
$blob = [byte[]]::new($FileKB * 1024)
(New-Object Random 42).NextBytes($blob)
function Write-Fake([string]$path){ [IO.File]::WriteAllBytes($path,$blob) }

Write-Host ("building {0} panels ({1} KB/file)..." -f $Panels,$FileKB)
$stampBase = Get-Date

for ($i = 1; $i -le $Panels; $i++) {
    $pidName = 'PNL-{0:D4}' -f $i
    $srvPid  = 'TSN{0:D12}' -f $i
    $stamp   = $stampBase.AddSeconds(-1 * $i * 37).ToString('yyyyMMddHHmmss')
    $src     = "$scratch\${pidName}_${stamp}"
    New-Item -ItemType Directory -Force -Path $src | Out-Null

    $sc = Pick-Scenario; Bump $sc

    # which canonical files to actually place on disk (MISSING drops a few)
    $present = [System.Collections.Generic.List[string]]::new()
    foreach ($f in $canonical) { $present.Add($f) | Out-Null }
    if ($sc -eq 'MISSING') {
        $drop = $rng.Next(1,4)
        for ($d=0; $d -lt $drop; $d++){ if($present.Count -gt 1){ $present.RemoveAt($rng.Next(0,$present.Count)) } }
    }
    foreach ($f in $present) { Write-Fake (Join-Path $src $f) }
    if ($sc -eq 'JUNK') { foreach ($j in $junkNames) { Write-Fake (Join-Path $src $j) } }

    # server-side manifest destinations (hash segments are opaque to the tool)
    $h1 = '{0:D8}' -f $rng.Next(0,99999999)
    $h2 = '{0:D8}' -f $rng.Next(0,99999999)
    $idxDst  = "data1h1/HN_DATA/POCB/INDEX/$h1/$h2/$srvPid.idx"
    $hostDst = "data1h1/HN_DATA/POCB/HOST/HNAMAL34DD01/${srvPid}_${stamp}.txt"
    $channel = $rng.Next(0,5).ToString()

    # RESUME: pre-seed a partial manifest (some clean, some -pending) in the source folder
    if ($sc -eq 'RESUME') {
        $root = 'data1h1/HN_DATA/'
        $mm = $stamp.Substring(4,2); $dd = $stamp.Substring(6,2)
        $seed = [System.Collections.Generic.List[string]]::new()
        $k = 0
        foreach ($f in $present) {
            $dst = "${root}POCB/HEX/$mm/$dd/HNAMAL34DD01/TESTMODEL/$srvPid/$stamp/$f"
            if ($k % 2 -eq 0) { $seed.Add("$dst@$channel") | Out-Null }
            else              { $seed.Add("$dst@$channel -pending") | Out-Null }
            $k++
        }
        [IO.File]::WriteAllLines((Join-Path $src "$srvPid.idx"), $seed)
        [IO.File]::WriteAllLines((Join-Path $src "${srvPid}_${stamp}.txt"), $seed)
    }

    # write the .panel handoff (atomic: .tmp then rename). NOTREADY omits SourceFolder.
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("Model=TESTMODEL")            | Out-Null
    $lines.Add("EQPID=HNAMAL34DD01")         | Out-Null
    $lines.Add("PID=$srvPid")                | Out-Null
    $lines.Add("DateTime=$stamp")            | Out-Null
    $lines.Add("UploadIndexPath=$idxDst")    | Out-Null
    $lines.Add("UploadHostPath=$hostDst")    | Out-Null
    if ($sc -ne 'NOTREADY') { $lines.Add("SourceFolder=$src") | Out-Null }
    $lines.Add("ChannelIndex=$channel")      | Out-Null

    $panelFinal = "$queue\${srvPid}_${stamp}.panel"
    $panelTmp   = "$panelFinal.tmp"
    [IO.File]::WriteAllLines($panelTmp, $lines)
    Move-Item -LiteralPath $panelTmp -Destination $panelFinal -Force

    if ($i % 25 -eq 0) { Write-Host "  $i panels..." }
}

Write-Host ''
Write-Host ('queue folder    : ' + $queue + '   (from config.json -- not wiped)')
Write-Host ('source scratch  : ' + $scratch)
Write-Host ('recipe          : ' + $recipe + '   (from config.json -- not overwritten)')
Write-Host ('host / user     : ' + [string]$cfg.PrimaryHost + ' / ' + [string]$cfg.User + '   (from config.json)')
Write-Host ''
Write-Host 'scenario mix:'
foreach ($k in ($tally.Keys | Sort-Object)) { Write-Host ('  {0,-9} {1}' -f $k,$tally[$k]) }
Write-Host ''
Write-Host 'NOTE: config.json / recipe / jobs / logs / state were NOT modified. Failure injection is'
Write-Host '      whatever SimulateFailurePercent is set to in config.json (the -FailPercent switch is'
Write-Host '      no longer written to config). NOTREADY panels have no SourceFolder and are skipped by'
Write-Host '      intake (they simulate a phase-1 handoff still waiting for phase 2).'
Write-Host ''

if ($NoLaunch) {
    Write-Host 'done (-NoLaunch: not starting the app).'
} else {
    Write-Host 'starting FtpUpload -- watch the panels ingest and upload.'
    Start-Process "$pub\FtpUpload.exe" -ArgumentList '--show' | Out-Null
}
