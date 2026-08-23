# =============================================================================
#  clean_panels.ps1 - remove the synthetic test panels that gen_panels.ps1
#  (and gen_500 / gen_5000) created, so the queue and UI go back to clean.
#
#  Targets ONLY files named like the generator's output:  TSN<12 digits>_<stamp>.panel
#  so real production panels are never touched.
#
#  By default it clears:
#     - test .panel (+ .panel.tmp) files from the config's QueueFolder
#     - test panels from the ingest backup folder (dated subfolders)
#     - the whole scratch source tree  (D:\FtpUploadDemo\src)
#
#  Switches:
#     -Full     ALSO wipe the app's jobs / logs / state / processed folders
#               (removes ALL day history - use only in a test environment)
#     -DryRun   show what would be deleted, delete nothing
#     -Quiet    don't pause at the end
#
#  Reads config.json for the real folder locations; never writes it.
# =============================================================================
param(
    [switch]$Full,
    [switch]$DryRun,
    [switch]$Quiet
)

$pub = if (Test-Path (Join-Path $PSScriptRoot 'publish\FtpUpload.exe')) { Join-Path $PSScriptRoot 'publish' } else { $PSScriptRoot }
$cfgPath = Join-Path $pub 'config.json'
if (-not (Test-Path $cfgPath)) { Write-Host "config.json not found at $cfgPath"; exit 1 }
$cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json

function Resolve-Cfg([string]$p, [string]$fallback) {
    if ([string]::IsNullOrWhiteSpace($p)) { $p = $fallback }
    if ([string]::IsNullOrWhiteSpace($p)) { return '' }   # nothing configured -> no folder (NOT $pub)
    if ([System.IO.Path]::IsPathRooted($p)) { return $p }
    return (Join-Path $pub $p)
}

$queue     = Resolve-Cfg ([string]$cfg.QueueFolder) 'queue'
# PanelBackupFolder empty => "{QueueFolder}\Backup Jobs" (matches the app's PanelBackupFullPath)
$backupRaw = [string]$cfg.PanelBackupFolder
$backup    = if ([string]::IsNullOrWhiteSpace($backupRaw)) { Join-Path $queue 'Backup Jobs' } else { Resolve-Cfg $backupRaw '' }
$scratch   = 'D:\FtpUploadDemo\src'

# Matches the generator's PID+stamp naming:  TSN000000000001_20260822123456.panel[.tmp]
$rxTest = '^TSN\d{12}_\d{14}\.panel(\.tmp)?$'

# stop the app so it isn't re-ingesting / holding files while we clean
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

$act = if ($DryRun) { 'WOULD delete' } else { 'deleted' }
Write-Host ''
Write-Host ('clean_panels ' + $(if($DryRun){'(dry run)'}else{''}))
Write-Host '-----------------------------------------------------------'

function Remove-TestPanels([string]$folder, [string]$label) {
    if ([string]::IsNullOrWhiteSpace($folder) -or -not (Test-Path $folder)) {
        Write-Host ('  {0,-16}: (folder not present) {1}' -f $label, $folder); return 0
    }
    $hits = Get-ChildItem -Path $folder -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match $rxTest }
    $n = 0
    foreach ($f in $hits) {
        if (-not $DryRun) { Remove-Item -LiteralPath $f.FullName -Force -ErrorAction SilentlyContinue }
        $n++
    }
    Write-Host ('  {0,-16}: {1} {2} test panel(s)  <- {3}' -f $label, $act, $n, $folder)
    return $n
}

$total = 0
$total += Remove-TestPanels $queue  'queue'
$total += Remove-TestPanels $backup 'backup'

# scratch tree is entirely ours - wipe it wholesale
if (Test-Path $scratch) {
    if (-not $DryRun) { Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue }
    Write-Host ('  {0,-16}: {1} scratch tree  <- {2}' -f 'scratch', $act, $scratch)
} else {
    Write-Host ('  {0,-16}: (not present) {1}' -f 'scratch', $scratch)
}

if ($Full) {
    Write-Host ''
    Write-Host '  -Full: clearing app state folders (ALL day history)...'
    $jobs      = Resolve-Cfg ([string]$cfg.JobsFolder)  'jobs'
    $logs      = Resolve-Cfg ([string]$cfg.LogFolder)   'logs'
    $state     = Resolve-Cfg ([string]$cfg.StateFolder) 'state'

    # SAFETY: never clear the publish folder itself (or anything that would take the exe/config/
    # scripts with it). We only clear a folder that is strictly a DESCENDANT of nothing critical
    # and that does not contain FtpUpload.exe. This guards against a misconfigured/empty path
    # resolving onto the exe folder.
    $pubFull = [System.IO.Path]::GetFullPath($pub).TrimEnd('\')
    function Is-SafeToClear([string]$dir) {
        if ([string]::IsNullOrWhiteSpace($dir)) { return $false }
        if (-not (Test-Path $dir)) { return $false }
        $full = [System.IO.Path]::GetFullPath($dir).TrimEnd('\')
        if ($full -ieq $pubFull) { return $false }                       # it's the publish folder
        if ($pubFull.StartsWith($full + '\', [StringComparison]::OrdinalIgnoreCase)) { return $false } # a parent of publish
        if (Test-Path (Join-Path $full 'FtpUpload.exe')) { return $false } # holds the exe
        return $true
    }

    foreach ($pair in @(@('jobs',$jobs),@('logs',$logs),@('state',$state))) {
        $lbl = $pair[0]; $dir = $pair[1]
        if ([string]::IsNullOrWhiteSpace($dir)) {
            Write-Host ('  {0,-16}: (not configured - skipped)' -f $lbl); continue
        }
        if (-not (Test-Path $dir)) {
            Write-Host ('  {0,-16}: (not present) {1}' -f $lbl, $dir); continue
        }
        if (-not (Is-SafeToClear $dir)) {
            Write-Host ('  {0,-16}: *** SKIPPED for safety (would touch the exe folder): {1}' -f $lbl, $dir); continue
        }
        $files = Get-ChildItem -Path $dir -Recurse -File -ErrorAction SilentlyContinue
        if (-not $DryRun) { $files | Remove-Item -Force -ErrorAction SilentlyContinue }
        Write-Host ('  {0,-16}: {1} {2} file(s)  <- {3}' -f $lbl, $act, $files.Count, $dir)
    }
}

Write-Host '-----------------------------------------------------------'
Write-Host ('  {0} {1} test panel file(s) from queue + backup.' -f $act, $total)
if ($DryRun) { Write-Host '  (dry run - nothing was actually removed.)' }
Write-Host ''

if (-not $Quiet) { Read-Host 'Press Enter to close' | Out-Null }
