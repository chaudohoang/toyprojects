# Rebuilds the WebView2 version of YDKE Deck Builder into publish\.
# The shortcuts belong to the WPF version (..\YdkeDeckBuilderWpf\build.ps1) since 2026-10-28; pass
# -Shortcuts to point them back at this one.
#   publish\YdkeDeckBuilder.exe          small (~1 MB), needs the .NET 8 Desktop Runtime - for this PC
#   publish\share\YdkeDeckBuilder.exe    self-contained (~69 MB), runs on any PC with WebView2
#   publish\YdkeDeckBuilder-win-x64.zip  the self-contained exe, zipped for sharing
#
# Why the integrity label: E:\Dev carries a Low Mandatory Level label that everything inside inherits,
# so an exe stored there runs at low integrity, and WebView2 can't start at low integrity. publish\ alone
# is given a Medium label (set at Charlie's request, 2026-10-28). The label is re-applied here because
# deleting publish\ and rebuilding recreates it as Low.
#
# Re-run it after changing the app or web\YdkeDeckBuilder\index.html.
param([switch]$NoShare, [switch]$Shortcuts)
$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'YdkeDeckBuilder.csproj'
$publish = Join-Path $PSScriptRoot 'publish'
$exe = Join-Path $publish 'YdkeDeckBuilder.exe'

# A running copy would lock its files.
Get-Process YdkeDeckBuilder -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$publish\*" } | ForEach-Object {
    [void]$_.CloseMainWindow()
    if (-not $_.WaitForExit(5000)) { $_ | Stop-Process -Force }
}

New-Item -ItemType Directory -Force $publish | Out-Null
icacls $publish /setintegritylevel '(OI)(CI)medium' | Out-Null

dotnet publish $project -c Release -o $publish --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

if (-not $NoShare) {
    $share = Join-Path $publish 'share'
    # WPF's own native DLLs must be bundled too, or they'd sit loose next to the exe.
    dotnet publish $project -c Release -o $share --nologo -v quiet `
        -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true
    if ($LASTEXITCODE -ne 0) { throw "self-contained publish failed ($LASTEXITCODE)" }
    Compress-Archive -Path (Join-Path $share 'YdkeDeckBuilder.exe') `
        -DestinationPath (Join-Path $publish 'YdkeDeckBuilder-win-x64.zip') -CompressionLevel Optimal -Force
}

# Files that already existed keep the label they were created with, so label the outputs explicitly too.
# wpf\ belongs to ..\YdkeDeckBuilderWpf\build.ps1 (and holds ~15k card images), so it's left alone.
$ours = { Get-ChildItem $publish -Recurse -File | Where-Object { $_.FullName -notlike (Join-Path $publish 'wpf\*') } }
& $ours | ForEach-Object { icacls $_.FullName /setintegritylevel medium | Out-Null }

if ($Shortcuts) {
    $shell = New-Object -ComObject WScript.Shell
    foreach ($dir in @([Environment]::GetFolderPath('Programs'), [Environment]::GetFolderPath('Desktop'))) {
        $link = $shell.CreateShortcut((Join-Path $dir 'YDKE Deck Builder.lnk'))
        $link.TargetPath = $exe
        $link.WorkingDirectory = $publish
        $link.IconLocation = "$exe,0"
        $link.Description = 'Paste a Yu-Gi-Oh! deck list, get a ydke:// code'
        $link.Save()
    }
}

& $ours | Where-Object Name -ne '_labeltest.txt' |
    Select-Object @{n='File';e={$_.FullName.Substring($publish.Length + 1)}}, @{n='MB';e={[math]::Round($_.Length / 1MB, 1)}}
