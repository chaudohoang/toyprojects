param(
    [ValidateSet('108','109')] [string]$Version = '109',
    [int]$Panels     = 60,
    [int]$RunSeconds = 200,
    [int]$CutEvery   = 2
)
$ErrorActionPreference = 'Stop'

$srcFolder = if ($Version -eq '108') { 'D:\Log\FtpUpload 1.0.0.8' } else { 'D:\Log\FtpUpload 1.0.0.9' }
$dev       = 'E:\Github\toyprojects\ForRadiant\FtpUploadNew'
$root      = "D:\FtpUploadTest\v$Version"
$queue     = "D:\FtpUploadTest\queue$Version"

Write-Host "=== v$Version : preparing ==="
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Remove-Item $root, $queue -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $root, $queue -Force | Out-Null

Get-ChildItem $srcFolder -File | Where-Object { $_.Name -notlike 'config.json*' } |
    Copy-Item -Destination $root -Force
foreach ($f in 'gen_panels.ps1','clean_panels.ps1') { Copy-Item "$dev\publish\$f" $root -Force }

function Write-TestConfig {
    $cfg = Get-Content "$dev\config.default.json" -Raw | ConvertFrom-Json
    $cfg.PrimaryHost            = '127.0.0.1'
    $cfg.SecondaryHost          = '127.0.0.1'
    $cfg.Port                   = 2121
    $cfg.User                   = 'admin'
    $cfg.Password               = 'admin'
    $cfg.RemoteBaseFolder       = '/'
    $cfg.QueueFolder            = $queue
    $cfg.TimeoutSecondsOverride = 5
    $cfg.PanelTimeoutSeconds    = 0
    $cfg.SimulateUploadMs       = 0
    $cfg.SimulateFailurePercent = 0
    $cfg.AutoStartUploading     = $true
    $cfg.AutoStartRetrying      = $true
    $cfg | ConvertTo-Json -Depth 8 | Set-Content "$root\config.json" -Encoding UTF8
}

Write-TestConfig
Write-Host "staging $Panels panels..."
& powershell -NoProfile -ExecutionPolicy Bypass -File "$root\gen_panels.ps1" `
    -Panels $Panels -FileKB 64 -PctFull 100 -PctMissing 0 `
    -PctJunk 0 -PctResume 0 -PctNotReady 0 -NoLaunch *>&1 | Out-Null
Write-TestConfig

Write-Host "starting flaky proxy (cut every $CutEvery connections)..."
$proxy = Start-Process powershell -PassThru -WindowStyle Hidden -ArgumentList @(
    '-NoProfile','-ExecutionPolicy','Bypass','-File',"$dev\publish\flaky_proxy.ps1",
    '-ListenPort','2121','-TargetPort','21','-CutAfterSeconds','2',
    '-CutEvery',"$CutEvery",'-RunSeconds',"$($RunSeconds + 40)")
Start-Sleep -Seconds 3

Write-Host "running FtpUpload v$Version for $RunSeconds s..."
$app = Start-Process "$root\FtpUpload.exe" -PassThru -WorkingDirectory $root
Start-Sleep -Seconds $RunSeconds

foreach ($p in @($app,$proxy)) { if ($p -and -not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue } }
Get-Process FtpUpload -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

$day = (Get-Date).ToString('yyyyMMdd')
$csv = Get-ChildItem "$queue\Log\${day}_operation.csv" -ErrorAction SilentlyContinue
if (-not $csv) { Write-Host "NO OPERATION LOG under $queue\Log"; Get-ChildItem $queue -Recurse -Depth 2 | Select-Object FullName; return }

$o = Import-Csv $csv.FullName
$panelsSeen = ($o.PID | Sort-Object -Unique).Count
$finalized  = (($o | Where-Object { $_.event -like 'final index sent*' }).PID | Sort-Object -Unique).Count
$aborts     = ($o | Where-Object { $_.reason -like '*abort*' -or $_.reason -like '*already read to the end*' }).Count
$uploaded   = ($o | Where-Object { $_.event -eq 'uploaded' -and $_.result -eq 'ok' }).Count
Write-Host ""
Write-Host "===== v$Version RESULT ====================================="
Write-Host ("panels staged     : {0}" -f $Panels)
Write-Host ("panels seen       : {0}" -f $panelsSeen)
Write-Host ("files uploaded ok : {0}" -f $uploaded)
Write-Host ("session aborts    : {0}" -f $aborts)
Write-Host ("INDEX/HOST SENT   : {0}" -f $finalized)
Write-Host "==========================================================="
