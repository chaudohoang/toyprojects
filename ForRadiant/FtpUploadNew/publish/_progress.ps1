$demo = 'D:\FtpUploadDemo'
$day  = Get-Date -Format 'yyyyMMdd'
$raw  = "$demo\logs\${day}_rawlog.txt"

Start-Sleep -Seconds 25

if (-not (Test-Path $raw)) { Write-Host 'no raw log yet'; exit }

$lines = Get-Content $raw
# current state = last line per PID|File
$state = @{}
foreach ($l in $lines) {
    $p = $l -split '\|'
    if ($p.Count -ge 8) { $state["$($p[0])|$($p[1])"] = $p[2] }
}

$ok   = ($state.Values | Where-Object { $_ -eq 'SUCCEEDED' }).Count
$fail = ($state.Values | Where-Object { $_ -eq 'FAILED' }).Count
$pend = ($state.Values | Where-Object { $_ -eq 'PENDING' }).Count

Write-Host ('raw log lines   : ' + $lines.Count)
Write-Host ('succeeded       : ' + $ok)
Write-Host ('failed          : ' + $fail)
Write-Host ('pending/retry   : ' + $pend)

$onDisk = @(Get-ChildItem 'D:\FTP\upload\LGD' -Recurse -File -ErrorAction SilentlyContinue)
Write-Host ('files on server : ' + $onDisk.Count)
$part = @($onDisk | Where-Object { $_.Name -like '*.part' })
Write-Host ('stray .part     : ' + $part.Count + '   <- should be 0')

Write-Host ''
Write-Host '--- last raw log lines ---'
$lines | Select-Object -Last 5

$p = Get-Process FtpUpload -ErrorAction SilentlyContinue
if ($p) {
    $p.Refresh()
    Write-Host ''
    Write-Host ('app memory : ' + [math]::Round($p.PrivateMemorySize64/1MB,1) + ' MB')
}
