# Patch TrueTest's "FTP prev-panel" lookup in every branch that has it.
#
# Two changes in the uploader each break this lookup on its own:
#   1. the per-file log was renamed  {day}_rawlog.txt -> {day}_totallog.txt
#   2. rows gained a leading write-time field, shifting every index by one
#
# Machines update at different times and a day's file keeps whatever shape it was written
# with for its whole retention window, so BOTH shapes must keep working.
#
# Line-based, not block-based: indentation and line endings vary between branches.

$targets = @(
  'D:\Branch\LGD-Mobile_Release_20231120',
  'D:\Branch\LGD-Mobile_Release_20260702',
  'D:\Branch\LGD-Mobile-CrystalView_Release_20260121',
  'D:\Branch\Azure-13964-CrystalView-InitialFactoryRelease-11-26-2025_TROY-StagingBranch'
)

$done = 0; $skipped = 0
foreach ($root in $targets) {
    if (-not (Test-Path $root)) { Write-Host ("MISSING branch: " + $root); continue }
    # Dove3p1 is the one running on the Mobile line today - it was missed on the first pass
    # because only the 2p0/3p0 pair was searched.
    foreach ($name in @('Dove2p0_PG.vb', 'Dove3p0_PG.vb', 'Dove3p1_PG.vb')) {
        $f = Get-ChildItem $root -Recurse -Filter $name -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $f) { continue }

        $lines = [System.Collections.Generic.List[string]]::new()
        [IO.File]::ReadAllLines($f.FullName) | ForEach-Object { $lines.Add([string]$_) }

        if (-not ($lines -match '_rawlog\.txt')) { Write-Host ("   no lookup: " + $f.Name); $skipped++; continue }
        if ($lines -match '_totallog\.txt')      { Write-Host ("   already patched: " + $f.Name); $skipped++; continue }

        $bak = $f.FullName + '.bak_lognames'
        if (-not (Test-Path $bak)) { Copy-Item $f.FullName $bak -Force }

        $changed = 0
        for ($i = $lines.Count - 1; $i -ge 0; $i--) {
            $L = $lines[$i]

            # (1) the file name: try the new one, fall back to the old
            if ($L -match 'Dim rawLogPath As String = .*_rawlog\.txt"\)') {
                $ind = ($L -replace '^(\s*).*$', '$1')
                $repl = @(
                    ($ind + "' The uploader renamed this log ""_rawlog.txt"" -> ""_totallog.txt"". Try the new"),
                    ($ind + "' name first and fall back, so this works whichever build the machine is on and"),
                    ($ind + "' for days recorded before the rename."),
                    ($ind + 'Dim rawLogPath As String = System.IO.Path.Combine(FTPUploadLogPath, dayStr & "_totallog.txt")'),
                    ($ind + 'If Not System.IO.File.Exists(rawLogPath) Then'),
                    ($ind + '    rawLogPath = System.IO.Path.Combine(FTPUploadLogPath, dayStr & "_rawlog.txt")'),
                    ($ind + 'End If')
                )
                $lines.RemoveAt($i); $lines.InsertRange($i, [string[]]$repl); $changed++
                continue
            }

            # (2) the PID match: allow a leading write-time field
            if ($L -match 'Dim bar As Integer = ln\.IndexOf\("\|"c\)') {
                $ind = ($L -replace '^(\s*).*$', '$1')
                $repl = @(
                    ($ind + "' Newer rows start with a ""yyyy-MM-dd HH:mm:ss"" write-time, which pushes the PID"),
                    ($ind + "' from the first field to the second. Detected per LINE, not per file: a day can"),
                    ($ind + "' hold both shapes if the app was updated mid-day."),
                    ($ind + 'Dim fp() As String = ln.Split("|"c)'),
                    ($ind + 'Dim off As Integer = 0'),
                    ($ind + 'If fp.Length > 0 AndAlso fp(0).Length = 19 AndAlso fp(0).Contains("-") AndAlso fp(0).Contains(":") Then off = 1')
                )
                $lines.RemoveAt($i); $lines.InsertRange($i, [string[]]$repl); $changed++
                continue
            }
            if ($L -match 'If bar > 0 AndAlso ln\.Substring\(0, bar\) = previousFTPUploadPID Then') {
                $ind = ($L -replace '^(\s*).*$', '$1')
                $lines[$i] = $ind + 'If fp.Length > off AndAlso fp(off) = previousFTPUploadPID Then'
                $changed++
                continue
            }

            # (3) the status field: same offset
            if ($L -match 'fileUploaded = \(mp\.Length >= 10 AndAlso mp\(9\) = "SUCCESS"\)') {
                $ind = ($L -replace '^(\s*).*$', '$1')
                $repl = @(
                    ($ind + "' Panel status is the 10th field; a leading write-time shifts it to the 11th."),
                    ($ind + 'Dim soff As Integer = 0'),
                    ($ind + 'If mp.Length > 0 AndAlso mp(0).Length = 19 AndAlso mp(0).Contains("-") AndAlso mp(0).Contains(":") Then soff = 1'),
                    ($ind + 'fileUploaded = (mp.Length >= soff + 10 AndAlso mp(soff + 9) = "SUCCESS")')
                )
                $lines.RemoveAt($i); $lines.InsertRange($i, [string[]]$repl); $changed++
                continue
            }
        }

        if ($changed -eq 0) { Write-Host ("   NOTHING MATCHED: " + $f.FullName); $skipped++; continue }
        [IO.File]::WriteAllLines($f.FullName, $lines, (New-Object Text.UTF8Encoding($true)))
        Write-Host ("   patched {0,-16} edits={1}  [{2}]" -f $f.Name, $changed, (Split-Path $root -Leaf))
        $done++
    }
}
Write-Host ""
Write-Host ("patched: {0}   skipped: {1}" -f $done, $skipped)
