param([int]$RunSeconds = 40)
$ErrorActionPreference = 'Stop'
$dev='E:\Github\toyprojects\ForRadiant\FtpUploadNew'; $root='D:\FtpUploadTest\ft'; $queue='D:\FtpUploadTest\ftq'
Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Remove-Item $root,$queue -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $root,$queue -Force | Out-Null
Get-ChildItem 'D:\Log\FtpUpload 1.0.0.9' -File | Where-Object { $_.Name -notlike 'config.json*' } | Copy-Item -Destination $root -Force
foreach($f in 'gen_panels.ps1','clean_panels.ps1'){ Copy-Item "$dev\publish\$f" $root -Force }
Copy-Item "$dev\allowed_filenames.txt" $root -Force

# A listener that ACCEPTS then stays silent - no FTP banner, ever. The client waits.
$silent = Start-Process powershell -PassThru -WindowStyle Hidden -ArgumentList @('-NoProfile','-Command',
 '$l=[System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,2222);$l.Start();' +
 '$end=(Get-Date).AddSeconds(120);while((Get-Date) -lt $end){if($l.Pending()){$c=$l.AcceptTcpClient()}else{Start-Sleep -Milliseconds 100}};$l.Stop()')
Start-Sleep -Seconds 2

function Write-TestConfig {
    $cfg = Get-Content "$dev\config.default.json" -Raw | ConvertFrom-Json
    $cfg.PrimaryHost='127.0.0.1'; $cfg.SecondaryHost='127.0.0.1'; $cfg.Port=2222
    $cfg.User='admin'; $cfg.Password='admin'; $cfg.RemoteBaseFolder='/'; $cfg.QueueFolder=$queue
    $cfg.TimeoutSecondsOverride=5; $cfg.PanelTimeoutSeconds=0
    $cfg.SimulateUploadMs=0; $cfg.SimulateFailurePercent=0
    $cfg.AutoStartUploading=$true; $cfg.AutoStartRetrying=$true
    $cfg | ConvertTo-Json -Depth 8 | Set-Content "$root\config.json" -Encoding UTF8
}
Write-TestConfig
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\gen_panels.ps1" -Panels 2 -FileKB 8 `
    -PctFull 100 -PctMissing 0 -PctJunk 0 -PctResume 0 -PctNotReady 0 -NoLaunch *>&1 | Out-Null
Write-TestConfig
$app = Start-Process "$root\FtpUpload.exe" -PassThru -WorkingDirectory $root
Start-Sleep -Seconds $RunSeconds
foreach($p in @($app,$silent)){ if($p -and -not $p.HasExited){ Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue } }
Get-Process FtpUpload,WinSCP -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "done"
