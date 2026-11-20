# Builds the native WPF version of YDKE Deck Builder into ..\YdkeDeckBuilder\publish\wpf\
#   YdkeDeckBuilderWpf.exe          small, needs the .NET 8 Desktop Runtime
#   share\YdkeDeckBuilderWpf.exe    self-contained, runs on any Windows PC   (with -Share)
# and points the Start Menu and Desktop "YDKE Deck Builder" shortcuts at the small exe (-NoShortcuts to skip).
#
# It goes under ..\YdkeDeckBuilder\publish\ because that folder carries a Medium integrity label; the rest
# of E:\Dev is labelled Low, and an exe stored there runs at low integrity and can't write outside E:\Dev
# (see CLAUDE.md). This version doesn't use WebView2, but saving a .ydk would still fail at low integrity.
param([switch]$Share, [switch]$NoShortcuts)
$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'YdkeDeckBuilderWpf.csproj'
$publish = Join-Path $PSScriptRoot '..\YdkeDeckBuilder\publish'
$out = Join-Path $publish 'wpf'

Get-Process YdkeDeckBuilderWpf -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*\publish\wpf\*" } | ForEach-Object {
    [void]$_.CloseMainWindow()
    if (-not $_.WaitForExit(5000)) { $_ | Stop-Process -Force }
}

New-Item -ItemType Directory -Force $publish | Out-Null
icacls $publish /setintegritylevel '(OI)(CI)medium' | Out-Null

dotnet publish $project -c Release -o $out --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

if ($Share) {
    dotnet publish $project -c Release -o (Join-Path $out 'share') --nologo -v quiet `
        -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true
    if ($LASTEXITCODE -ne 0) { throw "self-contained publish failed ($LASTEXITCODE)" }
}

# Files that already existed keep the label they were created with. art\ (card images the app downloads
# for screenshot reading, ~15k files) is left out: the app creates those itself, so they inherit Medium.
$artDir = Join-Path (Resolve-Path $out).Path 'art\*'
$appFiles = { Get-ChildItem $out -Recurse -File | Where-Object { $_.FullName -notlike $artDir } }
& $appFiles | ForEach-Object { icacls $_.FullName /setintegritylevel medium | Out-Null }

if (-not $NoShortcuts) {
    $exe = (Resolve-Path (Join-Path $out 'YdkeDeckBuilderWpf.exe')).Path
    $shell = New-Object -ComObject WScript.Shell
    foreach ($dir in @([Environment]::GetFolderPath('Programs'), [Environment]::GetFolderPath('Desktop'))) {
        $link = $shell.CreateShortcut((Join-Path $dir 'YDKE Deck Builder.lnk'))
        $link.TargetPath = $exe
        $link.WorkingDirectory = Split-Path $exe
        $link.IconLocation = "$exe,0"
        $link.Description = 'Paste a Yu-Gi-Oh! deck list, get a ydke:// code'
        $link.Save()
    }
}

& $appFiles |
    Select-Object @{n='File';e={$_.FullName.Substring((Resolve-Path $out).Path.Length + 1)}}, @{n='MB';e={[math]::Round($_.Length / 1MB, 1)}}
