param([int]$RunSeconds = 45)
$ErrorActionPreference = 'Stop'
# Forces a REAL server status code (530, bad credentials) so we can see exactly
# what the reason column preserves.
$dev   = 'E:\Github\toyprojects\ForRadiant\FtpUploadNew'
$root  = 'D:\FtpUploadTest\rsn'
$queue = 'D:\FtpUploadTest\rsnq'
Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Remove-Item $root,$queue -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $root,$queue -Force | Out-Null
Get-ChildItem 'D:\Log\FtpUpload 1.0.0.9' -File | Where-Object { $_.Name -notlike 'config.json*' } | Copy-Item -Destination $root -Force
foreach ($f in 'gen_panels.ps1','clean_panels.ps1') { Copy-Item "$dev\publish\$f" $root -Force }
Copy-Item "$dev\allowed_filenames.txt" $root -Force

function Write-TestConfig {
    $cfg = Get-Content "$dev\config.default.json" -Raw | ConvertFrom-Json
    $cfg.PrimaryHost = '127.0.0.1'; $cfg.SecondaryHost = '127.0.0.1'; $cfg.Port = 21
    $cfg.User = 'admin'; $cfg.Password = 'WRONG-ON-PURPOSE'
    $cfg.RemoteBaseFolder = '/'; $cfg.QueueFolder = $queue
    $cfg.TimeoutSecondsOverride = 30; $cfg.PanelTimeoutSeconds = 0
    $cfg.SimulateUploadMs = 0; $cfg.SimulateFailurePercent = 0
    $cfg.AutoStartUploading = $true; $cfg.AutoStartRetrying = $true
    $cfg | ConvertTo-Json -Depth 8 | Set-Content "$root\config.json" -Encoding UTF8
}
Write-TestConfig
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\gen_panels.ps1" -Panels 3 -FileKB 8 `
    -PctFull 100 -PctMissing 0 -PctJunk 0 -PctResume 0 -PctNotReady 0 -NoLaunch *>&1 | Out-Null
Write-TestConfig
$app = Start-Process "$root\FtpUpload.exe" -PassThru -WorkingDirectory $root
Start-Sleep -Seconds $RunSeconds
if ($app -and -not $app.HasExited) { Stop-Process -Id $app.Id -Force -ErrorAction SilentlyContinue }
Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "done - $root\logs"
