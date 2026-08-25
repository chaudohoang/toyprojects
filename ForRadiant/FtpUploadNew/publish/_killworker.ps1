# =============================================================================
#  _killworker.ps1 - forcibly stop the FTP Upload worker AND its watchdog, so it
#  is NOT relaunched. Killing FtpUpload.exe alone is not enough: the watchdog
#  (cmd.exe running run_watchdog.bat, started by wscript.exe run_hidden.vbs)
#  restarts it on any exit code other than 2/3. So we also kill the watchdog and
#  the hidden launcher for THIS folder, then clear any stale STOP command so a
#  fresh start is not immediately shut down again.
#
#  Used by install_task.bat (kill before starting fresh) and uninstall_task.bat.
# =============================================================================
$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }

# 1) the worker exe
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 2) the watchdog (run_watchdog.bat) + hidden launcher (run_hidden.vbs) - matched by command line
#    so we only kill THIS worker's cmd/wscript, never unrelated ones.
try {
    Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and ($_.CommandLine -match 'run_watchdog' -or $_.CommandLine -match 'run_hidden') } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
} catch { }

# 3) clear a stale STOP command (from a previous stop_worker.bat) so the next start runs clean
$sf = 'state'
$cfgPath = Join-Path $root 'config.json'
if (Test-Path $cfgPath) {
    try { $cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json; if ($cfg.StateFolder) { $sf = [string]$cfg.StateFolder } } catch { }
}
if (-not [System.IO.Path]::IsPathRooted($sf)) { $sf = Join-Path $root $sf }
$cmd = Join-Path $sf 'commands.txt'
if (Test-Path $cmd) { Remove-Item $cmd -Force -ErrorAction SilentlyContinue }

Write-Host 'Worker + watchdog stopped.'
