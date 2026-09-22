# Fix the PUC hex source folder in SaveTif across every branch that has it.
#
# HexPath is a Protected instance field, assigned during the DLL step of whichever panel is
# running. SaveTif reads it whenever it happens to run - and when a panel finishes late, the
# NEXT panel has already repointed it. The late SaveTif then copies from the new panel's
# FlashData folder, which is still empty, so the finishing panel's last hex never reaches its
# output folder and the .panel file is written one file short.
#
# Measured on 2026-09-12: A4XN68002030BE4 (5th hex 00:01:26, panel written 00:01:51) and
# A4XN68007018ABE (5th hex 03:38:19, panel written 03:38:30). Both show the FTP panel RunID
# mismatch that marks "a newer panel has started", and neither logged a SKIPPED foreign file
# because the new panel's folder held nothing at all.
#
# Fix: derive the hex folder from THIS panel's PID. The path shape is
#   <...>\<model>\<MM>\<dd>\<PID>\FlashData\
# so when the PID segment is not ours, swap it back. Local to the call, so a later panel
# cannot change it underneath us. Also logs the folder actually used, which the old line
# omitted - the one value needed to diagnose this was the one not recorded.

$targets = @(
  'D:\Branch\LGD-Mobile_Release_20231120',
  'D:\Branch\LGD-Mobile_Release_20260702',
  'D:\Branch\LGD-Mobile-CrystalView_Release_20260121',
  'D:\Branch\Azure-13964-CrystalView-InitialFactoryRelease-11-26-2025_TROY-StagingBranch'
)

$done = 0; $skipped = 0
foreach ($root in $targets) {
    if (-not (Test-Path $root)) { Write-Host ("MISSING branch: " + $root); continue }
    $f = Get-ChildItem $root -Recurse -Filter 'DemuraLGDBase.vb' -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $f) { Write-Host ("   no DemuraLGDBase.vb under " + $root); $skipped++; continue }

    $lines = [System.Collections.Generic.List[string]]::new()
    [IO.File]::ReadAllLines($f.FullName) | ForEach-Object { $lines.Add([string]$_) }

    if (-not ($lines -match 'CopyFilesWithRetry\(HexPath, outputPath')) { Write-Host ("   no hex copy: " + $root); $skipped++; continue }
    if ($lines -match 'hexSrc') { Write-Host ("   already patched: " + $root); $skipped++; continue }

    $bak = $f.FullName + '.bak_hexsrc'
    if (-not (Test-Path $bak)) { Copy-Item $f.FullName $bak -Force }

    $changed = 0
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        $L = $lines[$i]

        # (2) use the corrected folder for the PUC hex copy
        if ($L -match 'CopyFilesWithRetry\(HexPath, outputPath, "\*\.\*", PID\)') {
            $ind = ($L -replace '^(\s*).*$', '$1')
            $lines[$i] = $ind + 'CopyFilesWithRetry(hexSrc, outputPath, "*.*", PID)'
            $changed++
            continue
        }

        # (1) work out that folder, and log it
        if ($L -match 'WriteOperationLog\("SaveTif: PID=\[" & PID') {
            $ind = ($L -replace '^(\s*).*$', '$1')
            $repl = @(
                ($ind + "' HexPath is shared instance state: the DLL step of the NEXT panel repoints it, and a"),
                ($ind + "' panel that finishes late then copies its hex files from the new panel's folder -"),
                ($ind + "' which is empty, so its last hex never arrives and the .panel goes out one short."),
                ($ind + "' Rebuild the path for THIS panel. Shape: <...>\<model>\<MM>\<dd>\<PID>\FlashData\"),
                ($ind + 'Dim hexSrc As String = HexPath'),
                ($ind + 'Dim hexRepointed As Boolean = False'),
                ($ind + 'If Not String.IsNullOrEmpty(PID) AndAlso Not String.IsNullOrEmpty(hexSrc) Then'),
                ($ind + '    If hexSrc.IndexOf("\" & PID & "\", StringComparison.OrdinalIgnoreCase) < 0 Then'),
                ($ind + '        Dim mHex As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(hexSrc, "^(.*\\)[^\\]+(\\FlashData\\?)$")'),
                ($ind + '        If mHex.Success Then'),
                ($ind + '            hexSrc = mHex.Groups(1).Value & PID & mHex.Groups(2).Value'),
                ($ind + '            hexRepointed = True'),
                ($ind + '        End If'),
                ($ind + '    End If'),
                ($ind + 'End If'),
                ($ind + 'WriteOperationLog("SaveTif: PID=[" & PID & "] dest=[" & outputPath & "] src=[" & imagePath & "] hexSrc=[" & hexSrc & "]" & If(hexRepointed, "  (hex folder had been repointed to another panel - corrected)", "") & If(destOk, "", "  *** DEST PID MISMATCH - writing into another panel''s folder ***"))')
            )
            $lines.RemoveAt($i); $lines.InsertRange($i, [string[]]$repl); $changed++
            continue
        }
    }

    if ($changed -lt 2) { Write-Host ("   INCOMPLETE ({0} edits): {1}" -f $changed, $f.FullName); $skipped++; continue }
    [IO.File]::WriteAllLines($f.FullName, $lines, (New-Object Text.UTF8Encoding($true)))
    Write-Host ("   patched DemuraLGDBase.vb  edits={0}  [{1}]" -f $changed, (Split-Path $root -Leaf))
    $done++
}
Write-Host ""
Write-Host ("patched: {0}   skipped: {1}" -f $done, $skipped)
