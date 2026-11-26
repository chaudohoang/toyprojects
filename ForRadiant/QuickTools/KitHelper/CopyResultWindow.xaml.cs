using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KitHelper
{
    // Shown when a copy finishes: every file, grouped by the folder it went to, with what
    // happened to it (New / Replaced / Failed / Not copied). Easier to check than the log.
    // Preview mode is the confirmation before a copy: the same list as it WILL happen
    // (Replace / New / Not copied) with Cancel / Copy. Shown non-modal (MainWindow.Confirm);
    // Confirmed says whether Copy was clicked once it closes.
    public partial class CopyResultWindow : Window
    {
        readonly string backupRoot;
        readonly string text;
        readonly bool preview;
        readonly Copier.Result result;

        // Confirmation only: one checkbox per file that would be written (ticked = copy it), and
        // one per folder that ticks / unticks its files (null = some).
        readonly Dictionary<CheckBox, (CopyItem item, TextBlock name)> fileChecks = new Dictionary<CheckBox, (CopyItem, TextBlock)>();
        readonly Dictionary<CheckBox, List<CheckBox>> groupChecks = new Dictionary<CheckBox, List<CheckBox>>();
        readonly Dictionary<CheckBox, CheckBox> groupOf = new Dictionary<CheckBox, CheckBox>();
        bool syncing;

        // where: one line saying what was copied where. backupRoot: null when nothing was backed up.
        public CopyResultWindow(string title, string where, Copier.Result r, string backupRoot, bool preview = false)
        {
            InitializeComponent();
            this.backupRoot = backupRoot;
            this.preview = preview;
            result = r;
            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            TxtTitle.Text = (preview ? "Confirmation of " : "Result of ") + title;
            TxtWhere.Text = where;
            bool anyFailed = r.Items.Any(i => i.Status == CopyStatus.Failed);
            if (preview) SetMood(this, "Warn", "CONFIRM");
            else if (anyFailed) SetMood(this, "Err", "ERRORS");
            else SetMood(this, "Ok", "DONE");

            int nNew = r.Items.Count(i => i.Status == CopyStatus.New);
            int nRep = r.Items.Count(i => i.Status == CopyStatus.Replaced);
            int nFail = r.Items.Count(i => i.Status == CopyStatus.Failed);
            int nSkip = r.Items.Count(i => i.Status == CopyStatus.Skipped);
            void Run(string s, string brush, bool bold = false) =>
                TxtSummary.Inlines.Add(new Run(s) { Foreground = B(brush), FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal });
            if (preview)
            {
                // Counts: UpdatePreviewSummary, once the checkboxes exist (and on every tick).
                BtnOpenBackup.Visibility = Visibility.Collapsed;
                BtnCopyText.Visibility = Visibility.Collapsed;     // a confirmation, not a report
                BtnCancel.Visibility = Visibility.Visible;
                BtnSelectAll.Visibility = BtnSelectNone.Visibility = Visibility.Visible;
                BtnPrimary.Content = "Copy";
            }
            else
            {
                Run(nFail == 0 ? "Done.  " : "Finished with errors.  ", nFail == 0 ? "Ok" : "Err", true);
                Run($"{nRep} replaced", nRep > 0 ? "Accent" : "Dim");
                Run("  ·  ", "Dim");
                Run($"{nNew} new", nNew > 0 ? "Ok" : "Dim");
                Run("  ·  ", "Dim");
                Run($"{nFail} failed", nFail > 0 ? "Err" : "Dim", nFail > 0);
                if (nSkip > 0) { Run("  ·  ", "Dim"); Run($"{nSkip} not copied", "Dim"); }

                if (backupRoot != null) TxtBackup.Text = "Replaced files were backed up to " + backupRoot;
                else { TxtBackup.Text = "Nothing was overwritten, so no backup was made."; BtnOpenBackup.Visibility = Visibility.Collapsed; }
            }

            // Folders with a failure first, then by name; "Not copied" last.
            var groups = r.Items.GroupBy(i => i.Category, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key == CopyItem.NotCopied)
                .ThenByDescending(g => g.Any(i => i.Status == CopyStatus.Failed))
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (groups.Count == 0)
                Groups.Children.Add(new TextBlock { Text = "No files were copied.", Foreground = B("Dim") });

            var sb = new StringBuilder().AppendLine(title).AppendLine(where).AppendLine(TxtBackup.Text);
            bool first = true;
            foreach (var g in groups)
            {
                var items = g.OrderBy(i => i.Status == CopyStatus.Failed ? 0 : i.Status == CopyStatus.Replaced ? 1 : i.Status == CopyStatus.New ? 2 : 3)
                             .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
                string counts = string.Join(" · ", items.GroupBy(i => i.Status).OrderBy(x => x.Key)
                                                        .Select(x => $"{x.Count()} {Label(x.Key).ToLowerInvariant()}"));
                // The real destination folder ("C:\Program Files\...\TrueTest 1.9\ScratchDustDetect_NY")
                // when every file of the group goes to the same one; else the group's own name
                // ("Not copied", "Vignetting set to 1" across several folders).
                var dirs = items.Where(i => i.To != null).Select(i => Path.GetDirectoryName(i.To))
                                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                string dest = g.Key != CopyItem.NotCopied && dirs.Count == 1 ? dirs[0] : null;
                var header = new DockPanel { Margin = new Thickness(0, first ? 0 : 12, 0, 4) };
                var countText = new TextBlock { Text = counts, Foreground = B("Dim"), VerticalAlignment = VerticalAlignment.Bottom };
                DockPanel.SetDock(countText, Dock.Right);
                header.Children.Add(countText);
                // Confirmation: a folder checkbox when the folder has files that would be written.
                CheckBox groupBox = null;
                if (preview && items.Any(Checkable))
                {
                    groupBox = new CheckBox { IsChecked = true, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center,
                                              ToolTip = "Tick / untick every file in this folder" };
                    groupBox.Click += (s, e) => SetGroup((CheckBox)s);
                    groupChecks[groupBox] = new List<CheckBox>();
                    header.Children.Add(groupBox);
                }
                var heading = new TextBlock
                {
                    Text = dest ?? (g.Key == "" ? "(top level)" : g.Key),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = B(g.Key == CopyItem.NotCopied ? "Dim" : "Fg"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };
                if (dest != null)
                {
                    // A long path keeps its end ("…\TrueTest 1.9\ScratchDustDetect_NY"); hover shows it whole.
                    heading.TextTrimming = TextTrimming.None;
                    heading.Tag = dest;
                    heading.ToolTip = dest;
                    // Start short so the row doesn't grow to the full path (a vertical StackPanel gives a
                    // child its own desired width when wider); the row's real width then decides.
                    heading.Text = "…" + dest.Substring(Math.Max(0, dest.LastIndexOf('\\')));
                    header.SizeChanged += (s, e) => FitPathEnd(heading);
                }
                header.Children.Add(heading);
                Groups.Children.Add(header);
                Groups.Children.Add(new Border { Height = 1, Background = B("Line"), Margin = new Thickness(0, 0, 0, 4) });
                sb.AppendLine().AppendLine($"[{dest ?? g.Key}]  {counts}");
                first = false;

                foreach (var it in items)
                {
                    var row = new Grid { Margin = new Thickness(0, 1, 0, 1) };
                    // Confirmation: a checkbox column first (empty for rows that won't be written).
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(preview ? 24 : 0) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    var tag = new TextBlock { Text = Label(it.Status), Foreground = B(BrushOf(it.Status)), FontWeight = FontWeights.SemiBold };
                    var name = new TextBlock { Text = it.Name, Margin = new Thickness(0, 0, 12, 0), Foreground = B(it.Status == CopyStatus.Skipped ? "Dim" : "Fg") };
                    var detail = new TextBlock { Text = it.Detail ?? "", Foreground = B(it.Status == CopyStatus.Failed ? "Err" : "Dim"), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(tag, 1);
                    Grid.SetColumn(name, 2);
                    Grid.SetColumn(detail, 3);
                    if (groupBox != null && Checkable(it))
                    {
                        var box = new CheckBox { IsChecked = true, VerticalAlignment = VerticalAlignment.Center,
                                                 ToolTip = "Untick to leave this file as it is" };
                        box.Checked += File_Toggled;
                        box.Unchecked += File_Toggled;
                        fileChecks[box] = (it, name);
                        groupChecks[groupBox].Add(box);
                        groupOf[box] = groupBox;
                        row.Children.Add(box);
                    }
                    row.Children.Add(tag); row.Children.Add(name); row.Children.Add(detail);
                    Groups.Children.Add(row);
                    sb.AppendLine($"  {Label(it.Status),-10} {it.Name}{(string.IsNullOrEmpty(it.Detail) ? "" : "  - " + it.Detail)}");
                }
            }
            text = sb.ToString();
            if (preview) UpdatePreviewSummary();
        }

        // Shows as much of the path in t.Tag as fits, cutting the START: whole trailing folders when
        // possible ("…\TrueTest 1.9\ScratchDustDetect_NY"), else the end of the last one. The space is
        // the heading row's width minus its other parts (folder checkbox, counts).
        static void FitPathEnd(TextBlock t)
        {
            string full = t.Tag as string;
            if (full == null || !(t.Parent is Panel row)) return;
            double w = row.ActualWidth - 4;
            foreach (FrameworkElement c in row.Children)
                if (c != t) w -= c.ActualWidth + c.Margin.Left + c.Margin.Right + 8;
            if (w <= 0) return;
            var face = new Typeface(t.FontFamily, t.FontStyle, t.FontWeight, t.FontStretch);
            double dip = VisualTreeHelper.GetDpi(t).PixelsPerDip;
            double Width(string x) => new FormattedText(x, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face,
                                                        t.FontSize, Brushes.Black, dip).WidthIncludingTrailingWhitespace;
            string text = full;
            if (Width(full) > w)
            {
                text = null;
                for (int i = full.IndexOf('\\'); i >= 0 && text == null; i = full.IndexOf('\\', i + 1))
                    if (Width("…" + full.Substring(i)) <= w) text = "…" + full.Substring(i);
                if (text == null)
                {
                    int k = full.LastIndexOf('\\') + 1;
                    while (k < full.Length - 1 && Width("…" + full.Substring(k)) > w) k++;
                    text = "…" + full.Substring(k);
                }
            }
            if (t.Text != text) t.Text = text;
        }

        // ---- confirmation: tick / untick

        static bool Checkable(CopyItem i) => (i.Status == CopyStatus.New || i.Status == CopyStatus.Replaced) && i.To != null;

        // Destination files to leave alone (pass to Copier.Excluding).
        public ISet<string> Excluded =>
            new HashSet<string>(fileChecks.Where(kv => kv.Key.IsChecked != true).Select(kv => kv.Value.item.To), StringComparer.OrdinalIgnoreCase);

        void File_Toggled(object sender, RoutedEventArgs e)
        {
            var box = (CheckBox)sender;
            fileChecks[box].name.Foreground = B(box.IsChecked == true ? "Fg" : "Dim");
            if (syncing) return;
            SyncGroup(groupOf[box]);
            UpdatePreviewSummary();
        }

        // Folder checkbox clicked: all its files follow (a mixed folder becomes all ticked).
        void SetGroup(CheckBox group)
        {
            bool on = group.IsChecked == true;
            syncing = true;
            foreach (var f in groupChecks[group]) f.IsChecked = on;
            syncing = false;
            SyncGroup(group);
            UpdatePreviewSummary();
        }

        void SyncGroup(CheckBox group)
        {
            var files = groupChecks[group];
            int on = files.Count(f => f.IsChecked == true);
            group.IsChecked = on == files.Count ? true : on == 0 ? (bool?)false : null;
        }

        void SelectAll_Click(object sender, RoutedEventArgs e) => SetAll(true);
        void SelectNone_Click(object sender, RoutedEventArgs e) => SetAll(false);

        void SetAll(bool on)
        {
            syncing = true;
            foreach (var box in fileChecks.Keys) box.IsChecked = on;
            syncing = false;
            foreach (var g in groupChecks.Keys) SyncGroup(g);
            UpdatePreviewSummary();
        }

        // "Copy 80 file(s)?  66 will be replaced · 14 new · 13 not copied · 5 unticked"
        void UpdatePreviewSummary()
        {
            var ticked = fileChecks.Where(kv => kv.Key.IsChecked == true).Select(kv => kv.Value.item).ToList();
            int nRep = ticked.Count(i => i.Status == CopyStatus.Replaced);
            int nNew = ticked.Count(i => i.Status == CopyStatus.New);
            int nOff = fileChecks.Count - ticked.Count;
            int nSkip = result.Items.Count(i => i.Status == CopyStatus.Skipped);
            TxtSummary.Inlines.Clear();
            void Run(string s, string brush, bool bold = false) =>
                TxtSummary.Inlines.Add(new Run(s) { Foreground = B(brush), FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal });
            Run($"Copy {nNew + nRep} file(s)?  ", "Fg", true);
            Run($"{nRep} will be replaced", nRep > 0 ? "Accent" : "Dim");
            Run("  ·  ", "Dim");
            Run($"{nNew} new", nNew > 0 ? "Ok" : "Dim");
            if (nSkip > 0) { Run("  ·  ", "Dim"); Run($"{nSkip} not copied", "Dim"); }
            if (nOff > 0) { Run("  ·  ", "Dim"); Run($"{nOff} unticked", "Warn", true); }
            TxtBackup.Text = nRep > 0 ? "Files that get replaced are backed up first to " + Backup.BaseDir
                                      : "Nothing will be overwritten.";
            BtnPrimary.IsEnabled = nNew + nRep > 0;
        }

        string Label(CopyStatus s) =>
            s == CopyStatus.New ? "New" : s == CopyStatus.Replaced ? (preview ? "Replace" : "Replaced")
            : s == CopyStatus.Failed ? "Failed" : s == CopyStatus.Changed ? "Set" : s == CopyStatus.Kept ? "Kept" : "Not copied";

        static string BrushOf(CopyStatus s) =>
            s == CopyStatus.New ? "Ok" : s == CopyStatus.Replaced || s == CopyStatus.Changed ? "Accent"
            : s == CopyStatus.Failed ? "Err" : "Dim";

        // ---- vignetting option (confirmation only)

        // inis: CopiedInis of the preview. Shows the panel with their current values.
        public void OfferVignetting(IList<CopyItem> inis)
        {
            if (!preview || inis == null || inis.Count == 0) return;
            string now = string.Join(", ", inis.GroupBy(i => (value: Vignetting.Read(i.From), dx: Vignetting.IsDx(i.From)))
                                               .OrderBy(g => g.Key.dx).ThenBy(g => g.Key.value)
                                               .Select(g => $"{g.Key.value} in {g.Count()}{(g.Key.dx ? " DX" : "")}"));
            TxtVignetting.Inlines.Clear();
            TxtVignetting.Inlines.Add(new Run("Vignetting  ") { FontWeight = FontWeights.SemiBold });
            TxtVignetting.Inlines.Add(new Run($"{inis.Count} INI file(s) being copied have {Vignetting.Key}; now {now}.") { Foreground = B("Dim") });
            PnlVignetting.Visibility = Visibility.Visible;
        }

        // ---- unblock option (confirmation only)

        // files: the kit files this copy would write from. Shows the option with how many are marked;
        // ticked by default, the caller unblocks the kit before copying (UnblockFirst).
        public void OfferUnblock(IEnumerable<string> files)
        {
            if (!preview) return;
            var list = files.Where(f => f != null).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            int marked = list.Count(Unblocker.IsMarked);
            TxtUnblock.Text = marked == 0
                ? $"None of the {list.Count} file(s) is marked as downloaded from the internet."
                : $"{marked} of {list.Count} file(s) are marked as downloaded from the internet: they would arrive blocked in TrueTest.";
            PnlUnblock.Visibility = Visibility.Visible;
        }

        public bool UnblockFirst => PnlUnblock.Visibility == Visibility.Visible && ChkUnblock.IsChecked == true;

        // "1" / "0" to set, null to keep.
        public string VignettingValue => RbVigOn.IsChecked == true ? "1" : RbVigOff.IsChecked == true ? "0" : null;
        public bool VignettingIncludeDx => ChkVigDx.IsChecked == true;

        Brush B(string key) => (Brush)FindResource(key);

        // The state colour, so a confirmation / result / failure is told apart at a glance:
        // outline, tinted title band, badge and title text. Also used by AskWindow.
        internal static void SetMood(Window w, string brushKey, string badge)
        {
            var color = ((SolidColorBrush)w.FindResource(brushKey)).Color;
            var solid = new SolidColorBrush(color);
            ((Border)w.FindName("Frame")).BorderBrush = solid;
            ((Border)w.FindName("HeaderBand")).Background = new SolidColorBrush(Color.FromArgb(0x26, color.R, color.G, color.B));
            ((Border)w.FindName("BadgeBox")).Background = solid;
            ((TextBlock)w.FindName("TxtBadge")).Text = badge;
            ((TextBlock)w.FindName("TxtTitle")).Foreground = solid;
        }

        // Result mode, shown as a tab of ResultsWindow instead of on its own: hands over the content,
        // minus the outer frame and shadow. This window stays unshown and keeps handling the
        // content's buttons; its ✕ / Close then close the tab (close).
        Action closeInstead;
        internal UIElement TakeContent(Action close)
        {
            closeInstead = close;
            Frame.Margin = new Thickness(0);
            Frame.BorderThickness = new Thickness(0);
            Frame.Effect = null;
            Frame.Background = Brushes.Transparent;
            var content = (UIElement)Content;
            Content = null;
            return content;
        }

        // The header drags whichever window the content is in (this one, or ResultsWindow).
        // The header drags whichever window the content is in (this one, or ResultsWindow).
        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => GetWindow((DependencyObject)sender)?.DragMove();
        void Close_Click(object sender, RoutedEventArgs e) { if (closeInstead != null) closeInstead(); else Close(); }

        public bool Confirmed { get; private set; }

        // Preview: "Copy" confirms. Result: "Close".
        void Primary_Click(object sender, RoutedEventArgs e)
        {
            if (preview) { Confirmed = true; Close(); }
            else if (closeInstead != null) closeInstead();
            else Close();
        }

        void OpenBackup_Click(object sender, RoutedEventArgs e)
        {
            if (backupRoot != null && Directory.Exists(backupRoot)) Process.Start("explorer.exe", "\"" + backupRoot + "\"");
        }

        void CopyText_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(text); } catch { }
        }
    }
}
