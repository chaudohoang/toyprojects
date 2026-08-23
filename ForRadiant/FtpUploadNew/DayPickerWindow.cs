using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
// This project references both WPF and WinForms, so these names are ambiguous without aliases.
using WpfColor = System.Windows.Media.Color;
using WpfButton = System.Windows.Controls.Button;
using WpfOrientation = System.Windows.Controls.Orientation;
using HAlign = System.Windows.HorizontalAlignment;

namespace FtpUpload;

/// <summary>
/// Small modal calendar dialog for picking which day's log to view. Only days that actually have a
/// log are selectable (others are blacked out); the newest is pre-selected. Returns the chosen day
/// as yyyyMMdd via <see cref="SelectedDay"/> when ShowDialog() returns true.
/// </summary>
public sealed class DayPickerWindow : Window
{
    public string? SelectedDay { get; private set; }

    public DayPickerWindow(IEnumerable<string> availableDays, string title)
    {
        Title = title;
        Width = 300;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = new SolidColorBrush(WpfColor.FromRgb(0xF4, 0xF6, 0xFA));

        var days = availableDays.Where(d => d != null && d.Length == 8 && d.All(char.IsDigit))
                                .Distinct().OrderBy(d => d).ToList();
        var dates = new List<DateTime>();
        foreach (var d in days)
            if (DateTime.TryParseExact(d, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var dt))
                dates.Add(dt);

        var root = new StackPanel { Margin = new Thickness(12) };

        var cal = new Calendar
        {
            SelectionMode = CalendarSelectionMode.SingleDate,
            HorizontalAlignment = HAlign.Center
        };
        DateTime? chosen = null;
        if (dates.Count > 0)
        {
            var min = dates.First();
            var max = dates.Last();
            cal.DisplayDateStart = min;
            cal.DisplayDateEnd = max;
            cal.DisplayDate = max;
            cal.SelectedDate = max;
            chosen = max;
            // Black out every day in the range that has no log, so only real log days are pickable.
            var have = new HashSet<DateTime>(dates);
            for (var d = min; d <= max; d = d.AddDays(1))
                if (!have.Contains(d))
                    cal.BlackoutDates.Add(new CalendarDateRange(d));
        }
        // Track selection ourselves: clicking an already-selected date makes WPF briefly null out
        // SelectedDate mid-click, which otherwise swallows the first View click. We keep the last
        // non-null pick instead of reading SelectedDate at click time.
        cal.SelectedDatesChanged += (_, _) =>
        {
            if (cal.SelectedDate is DateTime d) chosen = d;
        };
        // WPF Calendar grabs the mouse capture when a date is clicked and doesn't release it, which
        // makes the NEXT click (on View/Cancel) get swallowed just to release the capture — the
        // classic "have to click twice" bug. Release capture after each calendar click so the very
        // next click on a button registers immediately.
        cal.PreviewMouseUp += (_, _) =>
        {
            if (System.Windows.Input.Mouse.Captured is not null)
                System.Windows.Input.Mouse.Capture(null);
        };
        root.Children.Add(cal);

        var hint = new TextBlock
        {
            Text = dates.Count > 0
                ? $"{dates.Count} day(s) with logs. Pick a day, then View."
                : "No logs found yet.",
            FontSize = 11.5,
            Foreground = new SolidColorBrush(WpfColor.FromRgb(0x8A, 0x91, 0xA3)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(2, 8, 2, 10)
        };
        root.Children.Add(hint);

        var buttons = new StackPanel { Orientation = WpfOrientation.Horizontal, HorizontalAlignment = HAlign.Right };
        var view = new WpfButton { Content = "View", Width = 84, Height = 30, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        var cancel = new WpfButton { Content = "Cancel", Width = 84, Height = 30, IsCancel = true };
        view.IsEnabled = dates.Count > 0;
        view.Click += (_, _) =>
        {
            if (chosen is DateTime dt)
            {
                SelectedDay = dt.ToString("yyyyMMdd");
                DialogResult = true;
            }
        };
        buttons.Children.Add(view);
        buttons.Children.Add(cancel);
        root.Children.Add(buttons);

        Content = root;
    }
}
