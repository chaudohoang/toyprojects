# Stop the FtpUpload worker deliberately: write a STOP command into the worker's StateFolder,
# read from config.json next to the exe (a relative value is resolved against this folder, exactly
# like the app). The worker finishes its current file, exits with code 3, and its watchdog loop
# ends quietly. The keep-alive task restarts it within ~5 min unless uninstall_task.bat is also run.
$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }

$sf = 'state'
$cfgPath = Join-Path $root 'config.json'
if (Test-Path $cfgPath) {
    try {
        $cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json
        if ($cfg.StateFolder) { $sf = [string]$cfg.StateFolder }
    } catch { }
}
if (-not [System.IO.Path]::IsPathRooted($sf)) { $sf = Join-Path $root $sf }

try {
    New-Item -ItemType Directory -Force -Path $sf | Out-Null
    $cmd = Join-Path $sf 'commands.txt'
    Add-Content -Path $cmd -Value 'STOP'
    Write-Host "STOP written to $cmd"
    Write-Host "The worker will shut down within a few seconds."
    Write-Host "Run uninstall_task.bat too if it must stay stopped."
} catch {
    Write-Host "Could not write STOP command: $($_.Exception.Message)"
    exit 1
}
