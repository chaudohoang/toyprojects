param(
    [string]$Mode = "release"
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core' -or $IsWindows) { } else {
    throw "Khong ho tro PowerShell nay, dung mo PowerShell/PS7 tren Windows nhe Boss."
}

Set-Location "$PSScriptRoot"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Chua tim thay dotnet tren may. Boss can cai dotnet SDK 10 truoc khi build nha."
}

Write-Host "Restoring..." -ForegroundColor Cyan
dotnet restore

if ($Mode -eq 'release') {
    Write-Host "Build release..." -ForegroundColor Cyan
    dotnet build -c Release --nologo

    Write-Host "Publish self-contained (no runtime needed)..." -ForegroundColor Green
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o "$PSScriptRoot\bin\publish"
}
else {
    Write-Host "Build debug..." -ForegroundColor Cyan
    dotnet build -c Debug --nologo

    Write-Host "Publish framework-dependent debug..." -ForegroundColor Green
    dotnet publish -c Debug -r win-x64 -o "$PSScriptRoot\bin\publish-debug"
}

$exe = Join-Path "$PSScriptRoot\bin\publish" "SnakeGame.exe"
if (-not (Test-Path $exe)) {
    $exe = Join-Path "$PSScriptRoot\bin\publish-debug" "SnakeGame.exe"
}

if (Test-Path $exe) {
    Write-Host "Build xong roi Boss! Nhan vao file: $exe" -ForegroundColor Magenta
    if ((Read-Host "Co muon chay ngay ko? (y/n)").Trim().ToLower() -eq 'y') {
        Start-Process -FilePath $exe
    }
}
else {
    Write-Host "Khong tim thay exe sau khi build!" -ForegroundColor Red
}
