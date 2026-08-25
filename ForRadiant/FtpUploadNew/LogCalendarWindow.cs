using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
// This project references both WPF and WinForms, so these names are ambiguous without aliases.
using WpfColor = System.Windows.Media.Color;
using WpfButton = System.Windows.Controls.Button;
using WpfBrushes = System.Windows.Media.Brushes;
using HAlign = System.Windows.HorizontalAlignment;
using VAlign = System.Windows.VerticalAlignment;
using WpfBrush = System.Windows.Media.Brush;
using WpfCursors = System.Windows.Input.Cursors;

namespace FtpUpload;

/// <summary>
/// Non-modal calendar popup for viewing a day's log. Days that HAVE a log are shown BOLD + RED and
/// are clickable â€” clicking one opens that day's report â€” while days without a log are greyed and
/// inert. The window stays open (click several days), does not block the main UI, and is meant to be
/// a single instance (the caller reuses/refreshes it instead of opening a second).
/// </summary>
public sealed class LogCalendarWindow : Window
{
    private readonly Func<HashSet<string>> _getDays;   // yyyyMMdd set, re-read on refresh
    private readonly Action<string> _onPick;           // called with yyyyMMdd when a red day is clicked

    private HashSet<string> _days = new();
    private DateTime _view;                             // first day of the month currently shown
    private bool _viewInitialised;

    private readonly TextBlock _monthLbl;
    private readonly TextBlock _info;
    private readonly WpfButton[] _cells = new WpfButton[42];

    private static readonly WpfBrush RedBrush  = new SolidColorBrush(WpfColor.FromRgb(0xD3, 0x2F, 0x2F));
    private static readonly WpfBrush GreyBrush = new SolidColorBrush(WpfColor.FromRgb(0xB0, 0xB4, 0xBC));
    private static readonly WpfBrush HdrBrush  = new SolidColorBrush(WpfColor.FromRgb(0x5A, 0x64, 0x7D));
    private static readonly WpfBrush TodayBrush = new SolidColorBrush(WpfColor.FromRgb(0x4D, 0x8C, 0xFF));

    public LogCalendarWindow(string title, Func<HashSet<string>> getDays, Action<string> onPick)
    {
        _getDays = getDays;
        _onPick = onPick;

        Title = title;
        Width = 322;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        Background = new SolidColorBrush(WpfColor.FromRgb(0xF4, 0xF6, 0xFA));

        var root = new StackPanel { Margin = new Thickness(12) };

        // header:  <   Month Year   >
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var prev = new WpfButton { Content = "\u25C0", Width = 30, Height = 26 };
        var next = new WpfButton { Content = "\u25B6", Width = 30, Height = 26 };
        prev.Click += (_, _) => { _view = _view.AddMonths(-1); Render(); };
        next.Click += (_, _) => { _view = _view.AddMonths(1); Render(); };

        _monthLbl = new TextBlock
        {
            FontSize = 14, FontWeight = FontWeights.Bold,
            HorizontalAlignment = HAlign.Center, VerticalAlignment = VAlign.Center, TextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(prev, 0); Grid.SetColumn(_monthLbl, 1); Grid.SetColumn(next, 2);
        header.Children.Add(prev); header.Children.Add(_monthLbl); header.Children.Add(next);
        root.Children.Add(header);

        // day-of-week header
        var dow = new UniformGrid { Columns = 7, Margin = new Thickness(0, 10, 0, 2) };
        foreach (var d in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            dow.Children.Add(new TextBlock { Text = d, TextAlignment = TextAlignment.Center, Foreground = HdrBrush, FontSize = 11 });
        root.Children.Add(dow);

        // 6 x 7 day cells
        var grid = new UniformGrid { Columns = 7 };
        for (int i = 0; i < 42; i++)
        {
            var b = new WpfButton
            {
                Height = 34, Margin = new Thickness(1),
                Background = WpfBrushes.Transparent, BorderThickness = new Thickness(0),
                FontSize = 12, Focusable = false
            };
            b.Click += Cell_Click;
            _cells[i] = b;
            grid.Children.Add(b);
        }
        root.Children.Add(grid);

        _info = new TextBlock
        {
            FontSize = 11.5, Foreground = HdrBrush, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(2, 8, 2, 0)
        };
        root.Children.Add(_info);

        Content = root;
        RefreshDays();
    }

    /// <summary>Re-read which days have a log (call when re-showing so new days appear).</summary>
    public void RefreshDays()
    {
        _days = _getDays() ?? new HashSet<string>();
        if (!_viewInitialised)
        {
            // Open on the most recent log day's month (else today).
            var maxDs = _days.Count > 0 ? _days.Max() : DateTime.Today.ToString("yyyyMMdd");
            var latest = DateTime.TryParseExact(maxDs, "yyyyMMdd", null,
                System.Globalization.DateTimeStyles.None, out var m) ? m : DateTime.Today;
            _view = new DateTime(latest.Year, latest.Month, 1);
            _viewInitialised = true;
        }
        Render();
    }

    private void Cell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is WpfButton b && b.Tag is string ds && _days.Contains(ds))
            _onPick(ds);   // opens the report; window stays open for more picks
    }

    private void Render()
    {
        var first = new DateTime(_view.Year, _view.Month, 1);
        _monthLbl.Text = first.ToString("MMMM yyyy");
        int startCol = (int)first.DayOfWeek;                    // 0 = Sunday
        int dim = DateTime.DaysInMonth(first.Year, first.Month);
        var today = DateTime.Today.ToString("yyyyMMdd");

        for (int i = 0; i < 42; i++)
        {
            var b = _cells[i];
            b.Content = ""; b.Tag = null; b.IsEnabled = false;
            b.Foreground = GreyBrush; b.FontWeight = FontWeights.Normal;
            b.Cursor = WpfCursors.Arrow; b.BorderThickness = new Thickness(0);
        }
        for (int d = 1; d <= dim; d++)
        {
            int idx = startCol + d - 1;
            if (idx >= 42) break;
            var b = _cells[idx];
            var ds = first.AddDays(d - 1).ToString("yyyyMMdd");
            b.Content = d.ToString();
            b.Tag = ds;
            if (_days.Contains(ds))
            {
                b.Foreground = RedBrush; b.FontWeight = FontWeights.Bold;
                b.IsEnabled = true; b.Cursor = WpfCursors.Hand;
            }
            else
            {
                b.Foreground = GreyBrush; b.FontWeight = FontWeights.Normal;
                b.IsEnabled = false; b.Cursor = WpfCursors.Arrow;
            }
            if (ds == today) { b.BorderThickness = new Thickness(1); b.BorderBrush = TodayBrush; }
        }

        _info.Text = _days.Count > 0
            ? $"Red days have a log ({_days.Count}) \u2014 click one to open its report."
            : "No logs found yet.";
    }
}

