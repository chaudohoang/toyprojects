using System.Text;

namespace FtpUpload;

/// <summary>
/// One CSV per day: a row per panel, a column per recipe file, O/X per cell.
///
/// The HTML reports answer "what happened to this file"; this answers "is this panel good", in a
/// shape that opens in Excel and can be diffed against LGD's own O/X sheet. It is rebuilt from the
/// logs each time, so it is never out of step with them.
///
/// O = the file reached the server, whether on the live pump or via NG recovery.
/// X = it did not.
/// - = the panel has no such file (the recipe lists it, this panel didn't produce it).
/// </summary>
public static class SummaryLog
{
    /// <summary>Path of the CSV for <paramref name="day"/>, or null if there is nothing to report.</summary>
    public static string? Build(Config cfg, string day)
    {
        var jobsPath = cfg.JobsPathForDay(day);
        if (!File.Exists(jobsPath)) return null;

        var recipe = Recipe.Load(cfg.RecipeFullPath).Patterns.ToList();

        // Which files landed, from BOTH logs: the rawlog carries live-pump successes and NG
        // write-backs, the ng-retry log carries recovery. A file counts as O from either.
        var landed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // PID -> most recent activity, "yyyyMMddHHmmss". Answers "when did this panel last move?"
        // without opening the rawlog — the first thing asked when a panel looks stuck.
        var lastSeen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in new[] { cfg.RawLogPathForDay(day), cfg.NgRetryLogPath(day) })
            foreach (var line in SafeFile.ReadLines(path))
            {
                var p = LogRow.Fields(line);   // field 0 = PID, whichever row shape
                if (p.Length >= 3 && p[2] == "SUCCEEDED") landed.Add(p[0] + "|" + p[1]);

                // Most recent activity per panel, for the "Last activity" column. Prefer the row's
                // own write-time (field 0 on newer rows). Older 10/11-field rows carry no date, so
                // fall back to the day plus the row's clock time.
                if (p.Length >= 2)
                {
                    // WrittenAt gives "yyyy-MM-dd HH:mm:ss" or "" on pre-timestamp rows.
                    //
                    // Kept as "yyyyMMdd-HHmmss" rather than 14 bare digits: Excel reads a 14-digit
                    // run as a number and shows "2.02609E+13", losing the value on screen and in
                    // anything pasted out of it. The hyphen makes it text in every reader while
                    // staying sortable and greppable.
                    var stamp = LogRow.WrittenAt(line);
                    var v = stamp.Length >= 19
                        // Seconds only: the stamp may now carry ".fff", and this column has an agreed
                // shape ("20260910-000555") that Excel must not read as a number.
                ? stamp[..10].Replace("-", "") + "-" + stamp.Substring(11, 8).Replace(":", "")
                        : day + "-" + ((p.Length > 3 && p[3].Length == 8) ? p[3].Replace(":", "") : "000000");
                    if (!lastSeen.TryGetValue(p[0], out var prev) || string.CompareOrdinal(v, prev) > 0)
                        lastSeen[p[0]] = v;
                }
            }

        // Panels and the files each one actually has, in first-seen order.
        var order = new List<string>();
        var files = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var idxName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var hostName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // PID -> local "<PID>.idx" path. The ".idxpartial"/".hostpartial" markers sit beside it.
        var idxSrc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in SafeFile.ReadLines(jobsPath))
        {
            var jl = JobsLine.Parse(line);
            if (jl is null || jl.Pid.Length == 0) continue;
            if (!files.TryGetValue(jl.Pid, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                files[jl.Pid] = set; order.Add(jl.Pid);
            }
            // Keep the panel's local index path: the ".idxpartial"/".hostpartial" markers sit
            // beside it, and they are what distinguishes "an early, short manifest is on the
            // server" from "nothing is".
            if (jl.IndexSrc.Length > 0) idxSrc[jl.Pid] = jl.IndexSrc;
            if (jl.IsManifest)
            {
                // The index is "<PID>.idx"; the host manifest is "<PID>_<stamp>.txt".
                if (jl.FileName.EndsWith(".idx", StringComparison.OrdinalIgnoreCase)) idxName[jl.Pid] = jl.FileName;
                else hostName[jl.Pid] = jl.FileName;
            }
            else set.Add(jl.FileName);
        }
        if (order.Count == 0) return null;

        // Build the columns so that a column ALWAYS means the same file.
        //
        // Walk the panels and, for each recipe entry, add a column the first time a matching
        // filename is seen; reuse the existing column when it appears again. So step01_R064.tif is
        // one column across the whole sheet, and a panel that lacks it gets "-" rather than
        // shifting its neighbours along. Positional "#1/#2" slots were rejected for exactly that
        // reason: slot 3 could be step03 on one panel and step07 on another.
        //
        // Filenames that embed the panel id (NyPucData_<PID>_1st.hex) are normalised back to the
        // recipe's @PID@ form for the header, so "NyPucData_@PID@_1st.hex" and "..._2nd.hex" are
        // each one shared column and align like any other file.
        var slots = new List<string>();
        var seenCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in recipe)
        {
            if (!IsPattern(entry))
            {
                if (seenCols.Add(entry)) slots.Add(entry);
                continue;
            }

            var added = 0;
            foreach (var pid in order)
                foreach (var f in files[pid].OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    if (!MatchesPattern(f, entry, pid)) continue;
                    // Put @PID@ back so the header is panel-independent.
                    //
                    // Normalise ANY embedded panel id, not just this panel's own. A file carrying a
                    // DIFFERENT panel's id (NyPucData_<otherPID>_2nd.hex, which TrueTest can leave in
                    // the folder) otherwise kept its literal name and became its own column — one
                    // column per foreign id, dozens of them, every cell but one a dash.
                    // The cell value marks it as foreign, so collapsing the columns does not hide it.
                    var header = NormalisePid(f, pid);
                    if (seenCols.Add(header)) { slots.Add(header); added++; }
                }
            if (added == 0 && seenCols.Add(entry)) slots.Add(entry);   // nothing matched today
        }

        var sb = new StringBuilder();
        // Panel rows are built separately so the day total can be written as the FIRST line, above
        // the header — the one figure worth seeing before anything else.
        var body = new StringBuilder();
        var panelsOk = 0;

        var head = new StringBuilder();
        head.Append("PID,Last activity,LGD Result");
        foreach (var s in slots) head.Append(',').Append(Csv(s));
        // Manifests last, so the recipe columns stay contiguous and line up with LGD's own sheet.
        // Short headers: the column is narrow and "Index manifest" only ever forced it wider.
        head.Append(",Index,Host");
        head.AppendLine();

        foreach (var pid in order)
        {
            var have = files[pid];
            var cells = new List<string>(slots.Count);
            var allOk = true;

            foreach (var col in slots)
            {
                // A header holding @PID@ resolves to this panel's own filename; everything else is
                // already a concrete name.
                var name = col.Contains("@PID@") ? col.Replace("@PID@", pid) : col;
                var match = have.Contains(name) ? name : null;

                // Not this panel's own file? The column may still be filled by a file carrying
                // ANOTHER panel's id — TrueTest can leave one in the folder, and it gets uploaded
                // into this panel's tree. Mark it "F" rather than "O": the data is present but it
                // belongs to a different panel, which is a defect, not a success.
                var foreign = false;
                if (match is null && col.Contains("@PID@"))
                {
                    foreach (var h in have)
                        if (NormalisePid(h, pid).Equals(col, StringComparison.OrdinalIgnoreCase))
                        { match = h; foreign = true; break; }
                }

                if (match is null) { cells.Add("-"); continue; }   // panel has no such file
                var ok = landed.Contains(pid + "|" + match);
                cells.Add(foreign ? (ok ? "F" : "X") : (ok ? "O" : "X"));
                if (!ok || foreign) allOk = false;
            }

            // A panel is only good if its manifests landed too — without them the server has the
            // data but nothing telling it the panel is complete. Shown as their own columns so a
            // panel that is "all data O but X on the host manifest" is visible at a glance.
            idxSrc.TryGetValue(pid, out var panelIdxSrc);
            var idxCell = Cell(pid, idxName, landed, ref allOk, panelIdxSrc, isIndex: true);
            var hostCell = Cell(pid, hostName, landed, ref allOk, panelIdxSrc, isIndex: false);

            if (allOk) panelsOk++;

            lastSeen.TryGetValue(pid, out var seenAt);
            body.Append(Csv(pid)).Append(',').Append(seenAt ?? "").Append(',').Append(allOk ? "O" : "X");
            foreach (var c in cells) body.Append(',').Append(c);
            body.Append(',').Append(idxCell).Append(',').Append(hostCell);
            body.AppendLine();
        }

        // Line 0: the day's upload succeed rate, above the header. Deliberately succeeded-of-total
        // rather than succeeded/failed: a panel that is not yet O may simply still be uploading or
        // waiting on a manifest, and calling that "failed" overstates the problem mid-run.
        var panelPct = order.Count > 0 ? (panelsOk * 100.0 / order.Count) : 0.0;
        sb.Append("Upload succeed rate,")
          .Append(panelPct.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture))
          .Append("%,Succeeded,").Append(panelsOk)
          .Append(",Total panels,").Append(order.Count)
          .AppendLine();
        sb.Append(head);
        sb.Append(body);

        var outPath = Path.Combine(cfg.LogFullPath, $"{day}_summary.csv");
        try
        {
            Directory.CreateDirectory(cfg.LogFullPath);
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(true));   // BOM: Excel reads it as UTF-8
            return outPath;
        }
        catch { return null; }
    }

    /// <summary>
    /// O / triangle / X for a manifest, or "-" if this panel has no such manifest line at all.
    ///
    /// A TRIANGLE means an EARLY, INCOMPLETE manifest is on the server: mid-fail sent one listing only the
    /// files that had landed, and the complete manifest never followed. That is a different state
    /// from X (nothing on the server at all) — the host has a readable manifest, just a short one —
    /// so it gets its own character. It does NOT count as success.
    ///
    /// Detected from the ".idxpartial"/".hostpartial" markers next to the panel's manifests, because
    /// mid-fail deliberately writes no rawlog row. Those files live on the machine that uploaded, so
    /// P only appears when the summary is built there — reading a copied log set elsewhere shows X.
    /// </summary>
    private static string Cell(string pid, Dictionary<string, string> names,
                               HashSet<string> landed, ref bool allOk,
                               string? indexSrc = null, bool isIndex = false)
    {
        if (!names.TryGetValue(pid, out var name) || name.Length == 0) return "-";
        var ok = landed.Contains(pid + "|" + name);
        if (ok) return "O";
        allOk = false;

        if (!string.IsNullOrEmpty(indexSrc))
        {
            try
            {
                var marker = System.IO.Path.ChangeExtension(indexSrc,
                                 isIndex ? ".idxpartial" : ".hostpartial");
                // HOLLOW triangle (U+25B3), not a letter: it reads as "warning" at a glance in a sheet of
                // O / X / -, and cannot be mistaken for a status code. The CSV is written
                // with a UTF-8 BOM, so Excel renders it correctly.
                if (File.Exists(marker)) return "\u25B3";
            }
            catch { }
        }
        return "X";
    }

    private static bool IsPattern(string s) => s.Contains('*') || s.Contains('?') || s.Contains("@PID@");

    /// <summary>Recipe-style glob: * and ?, with @PID@ standing in for the panel id.</summary>
    private static bool MatchesPattern(string fileName, string pattern, string pid)
    {
        var p = pattern.Replace("@PID@", pid);
        var rx = "^" + System.Text.RegularExpressions.Regex.Escape(p)
                          .Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(fileName, rx,
                   System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Replace an embedded panel id with "@PID@" so one column serves every panel.
    ///
    /// Replaces the panel's OWN id first, then any other id-shaped token in the same position:
    /// a file left behind by another panel must land in the same column as this panel's own, or the
    /// sheet grows one near-empty column per foreign id.
    /// </summary>
    private static string NormalisePid(string fileName, string pid)
    {
        if (pid.Length > 0 && fileName.Contains(pid, StringComparison.OrdinalIgnoreCase))
            return fileName.Replace(pid, "@PID@", StringComparison.OrdinalIgnoreCase);

        // "NyPucData_<id>_2nd.hex" -> "NyPucData_@PID@_2nd.hex". Anchored on the id's shape (the
        // same length as this panel's id) so ordinary names are never rewritten.
        if (pid.Length > 0)
        {
            var m = System.Text.RegularExpressions.Regex.Match(
                fileName, @"_([A-Za-z0-9]{" + pid.Length + @"})_");
            if (m.Success) return fileName.Remove(m.Groups[1].Index, pid.Length)
                                          .Insert(m.Groups[1].Index, "@PID@");
        }
        return fileName;
    }
    private static string Csv(string s)
        => s.Contains(',') || s.Contains('"') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
}
