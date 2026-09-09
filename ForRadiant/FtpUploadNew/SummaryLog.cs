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
        foreach (var path in new[] { cfg.RawLogPathForDay(day), cfg.NgRetryLogPath(day) })
            foreach (var line in SafeFile.ReadLines(path))
            {
                var p = line.Split('|');
                if (p.Length >= 3 && p[2] == "SUCCEEDED") landed.Add(p[0] + "|" + p[1]);
            }

        // Panels and the files each one actually has, in first-seen order.
        var order = new List<string>();
        var files = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var idxName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var hostName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in SafeFile.ReadLines(jobsPath))
        {
            var jl = JobsLine.Parse(line);
            if (jl is null || jl.Pid.Length == 0) continue;
            if (!files.TryGetValue(jl.Pid, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                files[jl.Pid] = set; order.Add(jl.Pid);
            }
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
                    var header = pid.Length > 0
                        ? f.Replace(pid, "@PID@", StringComparison.OrdinalIgnoreCase)
                        : f;
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
        head.Append("PID,LGD Result");
        foreach (var s in slots) head.Append(',').Append(Csv(s));
        // Manifests last, so the recipe columns stay contiguous and line up with LGD's own sheet.
        head.Append(",Index manifest,Host manifest");
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

                if (match is null) { cells.Add("-"); continue; }   // panel has no such file
                var ok = landed.Contains(pid + "|" + match);
                cells.Add(ok ? "O" : "X");
                if (!ok) allOk = false;
            }

            // A panel is only good if its manifests landed too — without them the server has the
            // data but nothing telling it the panel is complete. Shown as their own columns so a
            // panel that is "all data O but X on the host manifest" is visible at a glance.
            var idxCell = Cell(pid, idxName, landed, ref allOk);
            var hostCell = Cell(pid, hostName, landed, ref allOk);

            if (allOk) panelsOk++;

            body.Append(Csv(pid)).Append(',').Append(allOk ? "O" : "X");
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

    /// <summary>O/X for a manifest, or "-" if this panel has no such manifest line at all.</summary>
    private static string Cell(string pid, Dictionary<string, string> names,
                               HashSet<string> landed, ref bool allOk)
    {
        if (!names.TryGetValue(pid, out var name) || name.Length == 0) return "-";
        var ok = landed.Contains(pid + "|" + name);
        if (!ok) allOk = false;
        return ok ? "O" : "X";
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

    private static string Csv(string s)
        => s.Contains(',') || s.Contains('"') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
}
