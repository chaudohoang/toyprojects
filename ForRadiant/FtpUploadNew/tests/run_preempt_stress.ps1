param(
    [ValidateSet('108','109')] [string]$Version = '109',
    [int]$Panels     = 40,
    [int]$RunSeconds = 150
)
$ErrorActionPreference = 'Stop'

# Triggers Session.Abort() the same way the LGD failure did - the cancellation
# callback in UploadCore - but via the PANEL budget, which is reproducible on a
# fast loopback where a file transfer can never be made slow enough to time out.
# Abort() is Abort(): whichever token trips it, the session is destroyed and the
# question is only whether the app notices and reopens it.

$srcFolder = if ($Version -eq '108') { 'D:\Log\FtpUpload 1.0.0.8' } else { 'D:\Log\FtpUpload 1.0.0.9' }
$dev   = 'E:\Github\toyprojects\ForRadiant\FtpUploadNew'
$root  = "D:\FtpUploadTest\p$Version"
$queue = "D:\FtpUploadTest\pq$Version"

Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Remove-Item $root, $queue -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $root, $queue -Force | Out-Null
Get-ChildItem $srcFolder -File | Where-Object { $_.Name -notlike 'config.json*' } | Copy-Item -Destination $root -Force
foreach ($f in 'gen_panels.ps1','clean_panels.ps1') { Copy-Item "$dev\publish\$f" $root -Force }
# SAME recipe for both builds, or the panels differ in file count and the runs
# aren't comparable - the deploy folders ship different allowed_filenames.txt.
Copy-Item "$dev\allowed_filenames.txt" $root -Force

function Write-TestConfig {
    $cfg = Get-Content "$dev\config.default.json" -Raw | ConvertFrom-Json
    $cfg.PrimaryHost            = '127.0.0.1'
    $cfg.SecondaryHost          = '127.0.0.1'
    $cfg.Port                   = 21
    $cfg.User                   = 'admin'
    $cfg.Password               = 'admin'
    $cfg.RemoteBaseFolder       = '/'
    $cfg.QueueFolder            = $queue
    $cfg.TimeoutSecondsOverride = 30    # not the lever here
    $cfg.PanelTimeoutSeconds    = 3     # THE lever: preempt mid-panel -> Session.Abort()
    $cfg.SimulateUploadMs       = 400   # 10+ files x 0.4s per panel, so 3s is always exceeded
    $cfg.SimulateFailurePercent = 0
    $cfg.AutoStartUploading     = $true
    $cfg.AutoStartRetrying      = $true
    $cfg | ConvertTo-Json -Depth 8 | Set-Content "$root\config.json" -Encoding UTF8
}
Write-TestConfig
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\gen_panels.ps1" `
    -Panels $Panels -FileKB 8 -PctFull 100 -PctMissing 0 -PctJunk 0 `
    -PctResume 0 -PctNotReady 0 -NoLaunch *>&1 | Out-Null
Write-TestConfig

Write-Host "running v$Version for $RunSeconds s (panel budget 3s, real FTP on :21)..."
$app = Start-Process "$root\FtpUpload.exe" -PassThru -WorkingDirectory $root
Start-Sleep -Seconds $RunSeconds
if ($app -and -not $app.HasExited) { Stop-Process -Id $app.Id -Force -ErrorAction SilentlyContinue }
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Write-Host "done - logs in $root\logs"
