# Pass the hex folder INTO SaveTif instead of letting it read shared state.
#
# SaveTif already takes imagePath as a parameter; the PUC hex folder was the one input it
# still read from the instance field HexPath, which the NEXT panel's DLL step repoints. A
# panel finishing late then copied from the new panel's (empty) folder and lost its last hex.
#
# PerformDLL both ASSIGNS HexPath and calls SaveTif, so capturing the value into a method-scope
# local at the point of assignment and passing it down closes the window completely - no string
# rebuilding, no guessing. The PID-based correction stays as a safety net for the other call
# site (PerformExportTif), which has no assignment of its own to capture.

$targets = @(
  'D:\Branch\LGD-Mobile_Release_20231120',
  'D:\Branch\LGD-Mobile_Release_20260702',
  'D:\Branch\LGD-Mobile-CrystalView_Release_20260121',
  'D:\Branch\Azure-13964-CrystalView-InitialFactoryRelease-11-26-2025_TROY-StagingBranch'
)

$done = 0; $skipped = 0
foreach ($root in $targets) {
    if (-not (Test-Path $root)) { Write-Host ("MISSING branch: " + $root); continue }

    # ---- 1. DemuraLGDBase.vb : accept the folder as a parameter -------------------------
    $fb = Get-ChildItem $root -Recurse -Filter 'DemuraLGDBase.vb' -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $fb) { Write-Host ("   no DemuraLGDBase.vb: " + $root); $skipped++; continue }
    $tb = [IO.File]::ReadAllText($fb.FullName)
    if ($tb -notmatch 'hexSrc') { Write-Host ("   run patch_hexsrc.ps1 first: " + $root); $skipped++; continue }

    if ($tb -notmatch 'hexFolder As String') {
        if (-not (Test-Path ($fb.FullName + '.bak_hexparam'))) { Copy-Item $fb.FullName ($fb.FullName + '.bak_hexparam') -Force }
        $tb = $tb.Replace(
            'Public Overridable Sub SaveTif(outputPath As String, imagePath As String, Optional PID As String = "")',
            'Public Overridable Sub SaveTif(outputPath As String, imagePath As String, Optional PID As String = "", Optional hexFolder As String = "")')
        $newHexSrc = @(
            "        ' Prefer the folder the CALLER captured: PerformDLL assigns HexPath and calls this,",
            "        ' so a value taken there belongs to this panel and no later panel can repoint it.",
            "        ' Fall back to the shared field (with the PID correction below) for callers that",
            "        ' have nothing of their own to capture.",
            '        Dim hexSrc As String = If(String.IsNullOrEmpty(hexFolder), HexPath, hexFolder)'
        ) -join "`r`n"
        $tb = $tb.Replace('        Dim hexSrc As String = HexPath', $newHexSrc)
        [IO.File]::WriteAllText($fb.FullName, $tb, (New-Object Text.UTF8Encoding($true)))
        Write-Host ("   DemuraLGDBase.vb   : hexFolder parameter added   [{0}]" -f (Split-Path $root -Leaf))
    } else { Write-Host ("   DemuraLGDBase.vb   : already has the parameter") }

    # ---- 2. DemuraLGDMobile.vb : capture at assignment, pass at the call ----------------
    $fm = Get-ChildItem $root -Recurse -Filter 'DemuraLGDMobile.vb' -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $fm) { Write-Host ("   no DemuraLGDMobile.vb: " + $root); $skipped++; continue }

    $lines = [System.Collections.Generic.List[string]]::new()
    [IO.File]::ReadAllLines($fm.FullName) | ForEach-Object { $lines.Add([string]$_) }
    if ($lines -match 'runHexPath') { Write-Host ("   DemuraLGDMobile.vb : already patched"); $skipped++; continue }
    if (-not (Test-Path ($fm.FullName + '.bak_hexparam'))) { Copy-Item $fm.FullName ($fm.FullName + '.bak_hexparam') -Force }

    $changed = 0
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        $L = $lines[$i]

        # the PerformDLL call site: hand over what we captured
        if ($L -match '^\s*SaveTif\(cTempOutputPath, cImagePath, PID\)\s*$' -and $i -gt 800 -and $i -lt 1000) {
            $ind = ($L -replace '^(\s*).*$', '$1')
            $lines[$i] = $ind + 'SaveTif(cTempOutputPath, cImagePath, PID, runHexPath)'
            $changed++
            continue
        }

        # every HexPath assignment in this method: keep a copy that cannot be repointed
        if ($L -match '^\s*HexPath = DLLDefFolder') {
            $ind = ($L -replace '^(\s*).*$', '$1')
            $lines.Insert($i + 1, ($ind + 'runHexPath = HexPath   ' + "' this panel's own folder; HexPath itself is shared and gets repointed"))
            $changed++
            continue
        }
    }

    # the method-scope declaration, right after the PerformDLL signature
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s*Protected Overrides Sub PerformDLL') {
            $ind = ($lines[$i] -replace '^(\s*).*$', '$1') + '    '
            $lines.Insert($i + 1, ($ind + "Dim runHexPath As String = """"   ' captured copy of HexPath - see SaveTif"))
            $changed++
            break
        }
    }

    if ($changed -lt 3) { Write-Host ("   INCOMPLETE ({0} edits): {1}" -f $changed, $fm.FullName); $skipped++; continue }
    [IO.File]::WriteAllLines($fm.FullName, $lines, (New-Object Text.UTF8Encoding($true)))
    Write-Host ("   DemuraLGDMobile.vb : edits={0}" -f $changed)
    $done++
}
Write-Host ""
Write-Host ("branches done: {0}   skipped: {1}" -f $done, $skipped)
