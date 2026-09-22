param(
    [ValidateSet('108','109')] [string]$Version = '109',
    [int]$Panels     = 40,
    [int]$RunSeconds = 180,
    [int]$KillEvery  = 12
)
$ErrorActionPreference = 'Stop'

# Destroys the WinSCP process behind the live sessions from OUTSIDE the app.
# The Session objects survive but are dead - every later call on them throws
# "Session was aborted" / "Element session@0 already read to the end", which is
# exactly the state the LGD machines were stuck in. Needs no slow network.
#
# Kills hit the MANIFEST session too, which the panel-timeout test could not
# reach. The question this answers: does index/host come back by itself?

$srcFolder = if ($Version -eq '108') { 'D:\Log\FtpUpload 1.0.0.8' } else { 'D:\Log\FtpUpload 1.0.0.9' }
$dev   = 'E:\Github\toyprojects\ForRadiant\FtpUploadNew'
$root  = "D:\FtpUploadTest\k$Version"
$queue = "D:\FtpUploadTest\kq$Version"

Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Remove-Item $root, $queue -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $root, $queue -Force | Out-Null
Get-ChildItem $srcFolder -File | Where-Object { $_.Name -notlike 'config.json*' } | Copy-Item -Destination $root -Force
foreach ($f in 'gen_panels.ps1','clean_panels.ps1') { Copy-Item "$dev\publish\$f" $root -Force }
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
    $cfg.TimeoutSecondsOverride = 30
    $cfg.PanelTimeoutSeconds    = 0     # OFF - isolate the killed-session effect
    $cfg.SimulateUploadMs       = 250   # slow enough that kills land mid-panel
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

Write-Host "running v$Version for $RunSeconds s, killing winscp.exe every $KillEvery s..."
$app = Start-Process "$root\FtpUpload.exe" -PassThru -WorkingDirectory $root
$kills = 0
$deadline = (Get-Date).AddSeconds($RunSeconds)
Start-Sleep -Seconds 20                     # let it get going and finalize a few cleanly
while ((Get-Date) -lt $deadline) {
    $w = Get-Process WinSCP -ErrorAction SilentlyContinue
    if ($w) { $w | Stop-Process -Force -ErrorAction SilentlyContinue; $kills++ }
    Start-Sleep -Seconds $KillEvery
}
if ($app -and -not $app.HasExited) { Stop-Process -Id $app.Id -Force -ErrorAction SilentlyContinue }
Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Write-Host "winscp.exe killed $kills time(s). logs: $root\logs"
