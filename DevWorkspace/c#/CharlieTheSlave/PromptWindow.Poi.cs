using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace CharlieTheSlave;

/// <summary>
/// "export poi": reads every measurement's points of interest out of TrueTest and writes them to a
/// workbook (see <see cref="TrueTestPoi"/>). It drives the real mouse, so it holds the busy state and
/// can be stopped with the global hotkey or by typing "stop".
/// </summary>
public partial class PromptWindow
{
    /// <summary>A question Charlie has asked; whatever is typed next answers it instead of being a command.</summary>
    Func<string, Task>? _answering;

    /// <summary>Feeds <paramref name="text"/> to a waiting question. True if it was an answer.</summary>
    async Task<bool> HandleAnswerAsync(string text)
    {
        if (_answering is not { } answer) return false;
        _answering = null;
        await answer(text);
        return true;
    }

    Task ExportPoiAsync(string? setName)
    {
        setName = string.IsNullOrWhiteSpace(setName) ? _config.DefaultPoiSet : setName!.Trim().Trim('"', '\'');

        if (!TrueTestAutomation.IsRunning)
        {
            var detail = $"Start TrueTest and prepare the \"{setName}\" point-of-interest set, " +
                         "then ask me to export poi again.";
            Say("TrueTest isn't running. " + detail);
            AutomationBanner.Flash("⚠  TRUETEST ISN'T RUNNING", detail);
            return Task.CompletedTask;
        }

        Say($"\"{setName}\": every point, or just one?\n" +
            "Say \"all\", or name a point like \"P1\" — a number works too (\"1\" is the first point in the set).");
        _answering = answer => AnswerPoiPointsAsync(setName, answer);
        return Task.CompletedTask;
    }

    /// <summary>Turns the reply to that question into a choice, then runs the export.</summary>
    async Task AnswerPoiPointsAsync(string setName, string answer)
    {
        answer = answer.Trim().Trim('"', '\'');

        if (Regex.IsMatch(answer, @"^(n|no|cancel|stop|abort|never ?mind|forget it)\b", RegexOptions.IgnoreCase))
        {
            Say("OK, cancelled.");
            return;
        }

        var points =
            answer.Length == 0 || Regex.IsMatch(answer, @"^(all|every|everything|all points|both)$", RegexOptions.IgnoreCase)
                ? PoiPoints.All
                : int.TryParse(answer, out int number) && number > 0
                    ? PoiPoints.Numbered(number)
                    : PoiPoints.Named(answer);

        await RunPoiExportAsync(setName, points);
    }

    async Task RunPoiExportAsync(string setName, PoiPoints points)
    {
        Say($"Reading {points.Describe} of \"{setName}\" from every measurement — " +
            "watch the banner at the top of the screen.");

        _cts = new CancellationTokenSource();
        BusyChanged?.Invoke(true);
        var banner = new AutomationBanner();
        banner.Show();

        // Held until the running banner has gone, so the warning doesn't land on top of it.
        (string Headline, string Detail)? warn = null;
        try
        {
            // Progress goes to the banner, which the user is actually looking at; only warnings, which
            // they need after the run as well, are also written to the chat.
            var progress = new Progress<string>(text =>
            {
                banner.Report(text);
                if (text.StartsWith("⚠", StringComparison.Ordinal)) Say(text);
            });
            var export = await Task.Run(() => TrueTestPoi.ExportAll(setName, points, progress, _cts.Token), _cts.Token);
            ReportPoi(export, setName, points);
        }
        catch (OperationCanceledException)
        {
            Say($"Stopped. TrueTest is as I left it — close any \"{TrueTestAutomation.Names.PoiWindowTitle}\" window that's still open.");
        }
        catch (PoiSetMissingException ex)
        {
            Say("❌ " + ex.Message);
            warn = ("⚠  POINT-OF-INTEREST SET MISSING", ex.Message);
        }
        catch (TrueTestGoneException ex)
        {
            Say("❌ " + ex.Message);
            warn = ("⚠  TRUETEST ISN'T RUNNING", ex.Message);
        }
        catch (TrueTestNotReadyException ex)
        {
            Say("❌ " + ex.Message);
            warn = ("⚠  TRUETEST ISN'T READY", ex.Message);
        }
        catch (Exception ex)
        {
            Say("❌ " + ex.Message);
        }
        finally
        {
            banner.Close();
            _cts.Dispose();
            _cts = null;
            BusyChanged?.Invoke(false);
        }

        if (warn is { } w) AutomationBanner.Flash(w.Headline, w.Detail);
    }

    void ReportPoi(PoiExport export, string setName, PoiPoints points)
    {
        var stopped = export.Stopped
            ? $"Stopped with {StopHotkeyName} after {export.Results.Count + export.Skipped.Count} of " +
              $"{export.Total} measurements" + (export.NotRead > 0 ? $"; the remaining {export.NotRead} weren't read. " : ". ")
            : "";
        int written = export.Results.Sum(r => r.Readings.Count);
        if (written == 0)
        {
            Say(stopped + $"No measurement gave me {points.Describe} of \"{setName}\"." +
                (export.Skipped.Count > 0 ? "\n" + SkipLines(export) : ""));
            return;
        }

        // Whatever was read is worth keeping, even when the run was cut short.
        var file = WritePoiWorkbook(export, setName, points);
        Say(stopped + $"{(export.Stopped ? "Kept" : "✅")} {written} points from " +
            $"{export.Results.Count} measurements → {file}" +
            (export.Skipped.Count > 0 ? $"\nSkipped {export.Skipped.Count}:\n{SkipLines(export)}" : ""));
        Open(file);
    }

    static string SkipLines(PoiExport export) =>
        string.Join("\n", export.Skipped.Take(10).Select(s => $"• {s.Measurement.Setup}: {s.Reason}")) +
        (export.Skipped.Count > 10 ? $"\n…and {export.Skipped.Count - 10} more." : "");

    /// <summary>One row per point: the measurement setup, the point's name, then Cx, Cy and luminance.</summary>
    static string WritePoiWorkbook(PoiExport export, string setName, PoiPoints points)
    {
        Directory.CreateDirectory(ExportFolder);
        var only = points.IsAll ? "" : " " + Safe(points.Name ?? $"point {points.Number}");
        var path = Path.Combine(ExportFolder, $"TrueTest {Safe(setName)}{only} {DateTime.Now:yyyy-MM-dd HHmmss}.xlsx");

        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Points of Interest");
        var header = new[] { "Measurement Setup", "Point", "Color Cx", "Color Cy", "Luminance" };
        for (int c = 0; c < header.Length; c++) sheet.Cell(1, c + 1).Value = header[c];
        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);

        int row = 2;
        var runStart = row;          // first row of the run of identical setups being written
        string? runSetup = null;
        foreach (var result in export.Results)
        {
            // Measurements that share a setup name are one block, so re-measurements merge with their original.
            if (runSetup != null && result.Measurement.Setup != runSetup)
            {
                MergeSetup(sheet, runStart, row - 1);
                runStart = row;
            }
            runSetup = result.Measurement.Setup;

            foreach (var p in result.Readings)
            {
                sheet.Cell(row, 1).Value = result.Measurement.Setup;
                sheet.Cell(row, 2).Value = p.Point;
                Put(sheet.Cell(row, 3), p.Cx);
                Put(sheet.Cell(row, 4), p.Cy);
                Put(sheet.Cell(row, 5), p.Luminance);
                row++;
            }
        }
        if (runSetup != null) MergeSetup(sheet, runStart, row - 1);
        sheet.Columns().AdjustToContents();
        book.SaveAs(path);
        return path;
    }

    /// <summary>Shows the finished workbook in whatever opens .xlsx, so the run ends with the result on screen.</summary>
    void Open(string file)
    {
        try
        {
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Say($"(I couldn't open it for you: {ex.Message})");
        }
    }

    /// <summary>Merges one measurement setup's cells into a single cell spanning all of its points.</summary>
    static void MergeSetup(IXLWorksheet sheet, int firstRow, int lastRow)
    {
        if (lastRow <= firstRow) return; // a single row has nothing to merge
        var cells = sheet.Range(firstRow, 1, lastRow, 1).Merge();
        cells.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    /// <summary>Writes a reading as a number when TrueTest gave us one, so Excel can chart it.</summary>
    static void Put(IXLCell cell, string reading)
    {
        if (double.TryParse(reading, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            cell.Value = number;
        else
            cell.Value = reading;
    }

    static string Safe(string name) =>
        string.Join("", name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
