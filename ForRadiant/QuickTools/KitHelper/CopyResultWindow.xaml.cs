using System;
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
    public partial class CopyResultWindow : Window
    {
        readonly string backupRoot;
        readonly string text;

        // where: one line saying what was copied where. backupRoot: null when nothing was backed up.
        public CopyResultWindow(string title, string where, Copier.Result r, string backupRoot)
        {
            InitializeComponent();
            this.backupRoot = backupRoot;
            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            TxtTitle.Text = title;
            TxtWhere.Text = where;

            int nNew = r.Items.Count(i => i.Status == CopyStatus.New);
            int nRep = r.Items.Count(i => i.Status == CopyStatus.Replaced);
            int nFail = r.Items.Count(i => i.Status == CopyStatus.Failed);
            int nSkip = r.Items.Count(i => i.Status == CopyStatus.Skipped);
            void Run(string s, string brush, bool bold = false) =>
                TxtSummary.Inlines.Add(new Run(s) { Foreground = B(brush), FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal });
            Run(nFail == 0 ? "Done.  " : "Finished with errors.  ", nFail == 0 ? "Ok" : "Err", true);
            Run($"{nRep} replaced", nRep > 0 ? "Accent" : "Dim");
            Run("  ·  ", "Dim");
            Run($"{nNew} new", nNew > 0 ? "Ok" : "Dim");
            Run("  ·  ", "Dim");
            Run($"{nFail} failed", nFail > 0 ? "Err" : "Dim", nFail > 0);
            if (nSkip > 0) { Run("  ·  ", "Dim"); Run($"{nSkip} not copied", "Dim"); }

            if (backupRoot != null) TxtBackup.Text = "Replaced files were backed up to " + backupRoot;
            else { TxtBackup.Text = "Nothing was overwritten, so no backup was made."; BtnOpenBackup.Visibility = Visibility.Collapsed; }

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

        static string Label(CopyStatus s) =>
            s == CopyStatus.New ? "New" : s == CopyStatus.Replaced ? "Replaced" : s == CopyStatus.Failed ? "Failed" : "Not copied";

        static string BrushOf(CopyStatus s) =>
            s == CopyStatus.New ? "Ok" : s == CopyStatus.Replaced ? "Accent" : s == CopyStatus.Failed ? "Err" : "Dim";

        Brush B(string key) => (Brush)FindResource(key);

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        void Close_Click(object sender, RoutedEventArgs e) => Close();

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
