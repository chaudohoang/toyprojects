using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    // (Replace / New / Not copied) with Cancel / OK; ShowDialog() == true means OK (copy).
    public partial class CopyResultWindow : Window
    {
        readonly string backupRoot;
        readonly string text;
        readonly bool preview;

        // where: one line saying what was copied where. backupRoot: null when nothing was backed up.
        public CopyResultWindow(string title, string where, Copier.Result r, string backupRoot, bool preview = false)
        {
            InitializeComponent();
            this.backupRoot = backupRoot;
            this.preview = preview;
            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            TxtTitle.Text = preview ? "Confirmation of " + title : title;
            TxtWhere.Text = where;

            int nNew = r.Items.Count(i => i.Status == CopyStatus.New);
            int nRep = r.Items.Count(i => i.Status == CopyStatus.Replaced);
            int nFail = r.Items.Count(i => i.Status == CopyStatus.Failed);
            int nSkip = r.Items.Count(i => i.Status == CopyStatus.Skipped);
            void Run(string s, string brush, bool bold = false) =>
                TxtSummary.Inlines.Add(new Run(s) { Foreground = B(brush), FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal });
            if (preview)
            {
                Run($"Copy {nNew + nRep} file(s)?  ", "Fg", true);
                Run($"{nRep} will be replaced", nRep > 0 ? "Accent" : "Dim");
                Run("  ·  ", "Dim");
                Run($"{nNew} new", nNew > 0 ? "Ok" : "Dim");
                if (nSkip > 0) { Run("  ·  ", "Dim"); Run($"{nSkip} not copied", "Dim"); }
                TxtBackup.Text = nRep > 0 ? "Files that get replaced are backed up first to " + Backup.BaseDir
                                          : "Nothing will be overwritten.";
                BtnOpenBackup.Visibility = Visibility.Collapsed;
                BtnCopyText.Visibility = Visibility.Collapsed;     // a confirmation, not a report
                BtnCancel.Visibility = Visibility.Visible;
                BtnPrimary.Content = "OK";
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
                var header = new DockPanel { Margin = new Thickness(0, first ? 0 : 12, 0, 4) };
                var countText = new TextBlock { Text = counts, Foreground = B("Dim"), VerticalAlignment = VerticalAlignment.Bottom };
                DockPanel.SetDock(countText, Dock.Right);
                header.Children.Add(countText);
                header.Children.Add(new TextBlock
                {
                    Text = g.Key == "" ? "(top level)" : g.Key,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = B(g.Key == CopyItem.NotCopied ? "Dim" : "Fg"),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                });
                Groups.Children.Add(header);
                Groups.Children.Add(new Border { Height = 1, Background = B("Line"), Margin = new Thickness(0, 0, 0, 4) });
                sb.AppendLine().AppendLine($"[{g.Key}]  {counts}");
                first = false;

                foreach (var it in items)
                {
                    var row = new Grid { Margin = new Thickness(0, 1, 0, 1) };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    var tag = new TextBlock { Text = Label(it.Status), Foreground = B(BrushOf(it.Status)), FontWeight = FontWeights.SemiBold };
                    var name = new TextBlock { Text = it.Name, Margin = new Thickness(0, 0, 12, 0), Foreground = B(it.Status == CopyStatus.Skipped ? "Dim" : "Fg") };
                    var detail = new TextBlock { Text = it.Detail ?? "", Foreground = B(it.Status == CopyStatus.Failed ? "Err" : "Dim"), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(name, 1);
                    Grid.SetColumn(detail, 2);
                    row.Children.Add(tag); row.Children.Add(name); row.Children.Add(detail);
                    Groups.Children.Add(row);
                    sb.AppendLine($"  {Label(it.Status),-10} {it.Name}{(string.IsNullOrEmpty(it.Detail) ? "" : "  - " + it.Detail)}");
                }
            }
            text = sb.ToString();
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

        // "1" / "0" to set, null to keep.
        public string VignettingValue => RbVigOn.IsChecked == true ? "1" : RbVigOff.IsChecked == true ? "0" : null;
        public bool VignettingIncludeDx => ChkVigDx.IsChecked == true;

        Brush B(string key) => (Brush)FindResource(key);

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        void Close_Click(object sender, RoutedEventArgs e) => Close();

        // Preview: "OK" confirms (ShowDialog returns true). Result: "Close".
        void Primary_Click(object sender, RoutedEventArgs e)
        {
            if (preview) DialogResult = true;
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
