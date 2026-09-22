using System.Windows;
using System.Windows.Controls;

namespace FtpUpload;

/// <summary>
/// One window for every log, a tab each: Day Log, NG Retry, Summary, Operation, Activity.
///
/// These were five separate buttons — two of them ("Total Log") duplicated on both strips and
/// labelled the same as each other while opening different reports. Anyone who did not already
/// know the difference had to press them to find out. One button, named tabs, and a day list per
/// tab, so the question "which log do I want" is answered by reading rather than by clicking.
///
/// Built on demand per day, so a finished day or a log set copied from another machine works the
/// same as today.
/// </summary>
public sealed class LogViewerWindow : Window
{
    private sealed record Kind(
        string Tab,
        string Blurb,
        string FilePattern,                   // how to find which days exist
        Func<Config, string, string?> Build);  // build (or locate) the file for a day

    private readonly Config _cfg;
    private readonly List<Kind> _kinds;
    private readonly System.Windows.Controls.TabControl _tabs = new();
    private readonly List<Action> _refreshers = new();

    public LogViewerWindow(Config cfg)
    {
        _cfg = cfg;
        Title = "View Log";
        SizeToContent = SizeToContent.WidthAndHeight;
        // Same chrome as the Trace Panel: a tool window, close only, out of the taskbar. There is
        // nothing to gain from maximising a calendar, and it should read as a panel belonging to the
        // app rather than a second application window.
        WindowStyle = WindowStyle.ToolWindow;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new System.Windows.Media.SolidColorBrush(
                         System.Windows.Media.Color.FromRgb(0xF5, 0xF7, 0xFB));

        // Tab order follows what you reach for: the day's work first, then the panel-by-panel
        // record, and the machine's own history last. "App" is deliberately last and separate —
        // nothing in it is about panels, so it does not belong among the upload logs.
        _kinds = new List<Kind>
        {
            new("Day Total",
                "Every panel and file for the day, as an HTML report: status, attempts, which IP, and what failed.",
                "*_htmllog.html",
                (c, d) => HtmlLog.BuildDayLog(c, d)),
            new("NG Retry Total",
                "Only the files that failed and what recovery did with them — recovered, still failing, or waiting on a manifest.",
                "*_nghtmllog.html",
                (c, d) => HtmlLog.BuildNgLog(c, d)),
            new("Summary",
                "One row per panel, one column per recipe file. O uploaded, X missing, \u25B3 an early incomplete manifest, - not part of this panel.",
                "*_summary.csv",
                SummaryLog.Build),
            new("Operation",
                "What happened to each PANEL, one line per file: result, times, tries, whether a retry rescued it, why it failed, and every early manifest send.",
                "*_operation.csv",
                OperationLog.Build),
            new("App",
                "The program itself, nothing about panels: startups and shutdowns with reasons, day rollovers, settings changes and crashes.",
                "*_aplog.txt",
                (c, d) => { var p = c.AppLogPath(ParseDay(d)); return File.Exists(p) ? p : null; }),
        };

        foreach (var k in _kinds) _tabs.Items.Add(BuildTab(k));
        Content = _tabs;
    }

    private System.Windows.Controls.TabItem BuildTab(Kind kind)
    {
        // Just the calendar, sized to it. The per-tab description was useful while the tabs were
        // new, but it pushed the calendar off-centre and made each tab a different height as the
        // text wrapped differently. The tab name and the tooltip on the View Log button say what
        // each log is.
        var grid = new System.Windows.Controls.Grid();

        // The SAME calendar the single-log buttons used: days that have a log are bold red and
        // clickable, the rest greyed and inert. Reused rather than reimplemented per tab.
        var (panel, refresh) = LogCalendarWindow.CreateEmbedded(
            kind.Tab,
            () => new HashSet<string>(DaysFor(kind)),
            day =>
            {
                try
                {
                    var path = kind.Build(_cfg, day);
                    if (path is null || !File.Exists(path))
                    {
                        System.Windows.MessageBox.Show(this, $"Nothing to show for {day}.", Title,
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        return;
                    }
                    // A COPY, never the live file: a viewer holding it open denies writers, and the
                    // report pump then cannot rewrite that day. See LogSnapshot.
                    LogSnapshot.OpenCopy(path, kind.Tab);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(this, "Could not open it: " + ex.Message, Title,
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            });
        panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        panel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        panel.Margin = new Thickness(10, 8, 10, 4);
        grid.Children.Add(panel);

        var tab = new System.Windows.Controls.TabItem { Header = kind.Tab, Content = grid };
        // Re-read the days when this tab is opened: a day can gain a log while the window is up.
        tab.GotFocus += (_, _) => refresh();
        _refreshers.Add(refresh);
        return tab;
    }

    /// <summary>Days that actually have this kind of log on disk, newest first.</summary>
    private IEnumerable<string> DaysFor(Kind kind)
    {
        var days = new SortedSet<string>(StringComparer.Ordinal);
        try
        {
            var dir = _cfg.LogFullPath;
            if (!Directory.Exists(dir)) return days.Reverse();

            // Derived reports may not exist yet — they are built on demand — so for those, look for
            // the log they are built FROM. Both names: "_totallog.txt" now, "_rawlog.txt" for any
            // day recorded before the rename (including log sets copied off site).
            var patterns = kind.Tab is "Day Total" or "Summary" or "Operation" or "NG Retry Total"
                           ? new[] { "*_totallog.txt", "*_rawlog.txt" }
                           : new[] { kind.FilePattern };
            foreach (var pattern in patterns)
            foreach (var f in Directory.GetFiles(dir, pattern))
            {
                var name = Path.GetFileName(f);
                if (name.Length >= 8 && name[..8].All(char.IsDigit)) days.Add(name[..8]);
            }
            // ...and include any already-built report for a day whose rawlog has since been purged.
            foreach (var f in Directory.GetFiles(dir, kind.FilePattern))
            {
                var name = Path.GetFileName(f);
                if (name.Length >= 8 && name[..8].All(char.IsDigit)) days.Add(name[..8]);
            }
        }
        catch { }
        return days.Reverse();
    }

    private static DateTime ParseDay(string day)
        => DateTime.TryParseExact(day, "yyyyMMdd", null,
               System.Globalization.DateTimeStyles.None, out var d) ? d : DateTime.Today;
}
