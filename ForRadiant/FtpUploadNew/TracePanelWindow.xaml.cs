using System.Windows;
using System.Windows.Input;

namespace FtpUpload;

/// <summary>
/// "Trace Panel" — one window that answers "what happened to this panel?".
///
/// The panel's story is spread across the jobs file, the rawlog, the ng-retry log, the marker files
/// on disk and the WinSCP session logs. Cross-referencing them by hand is what it took to settle the
/// field report where manifests read FAILED while sitting on the server. PanelTrace does that work.
/// </summary>
public partial class TracePanelWindow : Window
{
    private readonly Config _cfg;

    /// <summary>
    /// Optional auto-refresh. A trace is a SNAPSHOT of five log sources, and while NG is working a
    /// file can read "not uploaded" and land seconds later — so a stale report is easy to misread as
    /// a problem. Off by default: each run re-scans every session log, which is real work.
    /// </summary>
    private System.Windows.Threading.DispatcherTimer? _timer;

    private void AutoRefresh_Changed(object sender, RoutedEventArgs e)
    {
        if (AutoRefresh.IsChecked == true)
        {
            _timer ??= new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            _timer.Tick -= OnTick;
            _timer.Tick += OnTick;
            _timer.Start();
        }
        else _timer?.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (PidBox.Text.Trim().Length == 0) return;
        // Keep the reader's place: re-running the trace would otherwise jump them back to the top.
        var offset = ReportBox.VerticalOffset;
        Run();
        ReportBox.ScrollToVerticalOffset(offset);
    }

    public TracePanelWindow(Config cfg, string? initialPid = null)
    {
        InitializeComponent();
        _cfg = cfg;
        if (!string.IsNullOrWhiteSpace(initialPid))
        {
            PidBox.Text = initialPid!.Trim();
            Loaded += (_, _) => Run();          // pre-filled: search straight away
        }
        Loaded += (_, _) => { PidBox.Focus(); PidBox.SelectAll(); };
        Closed += (_, _) => _timer?.Stop();   // never tick against a closed window
    }

    private void PidBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter) { Run(); e.Handled = true; }
    }

    /// <summary>Hidden, not Collapsed: Hidden keeps the layout slot so the box never changes width.</summary>
    private void PidBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (PidClear is not null)
            PidClear.Visibility = PidBox.Text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
    }

    private void PidClear_Click(object sender, RoutedEventArgs e)
    {
        PidBox.Text = "";
        PidBox.Focus();
    }

    private void Search_Click(object sender, RoutedEventArgs e) { Run(); ReportBox.ScrollToHome(); }

    private void Run()
    {
        var pid = PidBox.Text.Trim();
        if (pid.Length == 0) { ReportBox.Text = "Type a panel ID and press Search."; return; }
        try
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            // Scanning every session log can take a moment on a busy day, so it runs off the UI
            // thread — the window would otherwise freeze mid-search.
            // Both tabs are built together: the raw lines are the evidence for the report, and
            // fetching them separately would let the two drift apart between clicks.
            var pair = System.Threading.Tasks.Task.Run(() =>
                (Report: PanelTrace.Build(_cfg, pid), Raw: PanelTrace.BuildRaw(_cfg, pid))).Result;
            ReportBox.Text = pair.Report;
            RawBox.Text = pair.Raw;
        }
        catch (Exception ex)
        {
            ReportBox.Text = "Trace failed: " + ex.Message;
        }
        finally { Mouse.OverrideCursor = null; }
    }

    /// <summary>Whichever tab the reader is looking at — Copy/Save/Open follow the eye.</summary>
    private string VisibleText()
    {
        try { return (RawBox is not null && RawBox.IsVisible ? RawBox.Text : ReportBox.Text) ?? ""; }
        catch { return ReportBox.Text ?? ""; }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try { System.Windows.Clipboard.SetText(VisibleText()); } catch { }
    }

    /// <summary>Save Report — pick a location and keep it. For attaching to a ticket or an email.</summary>
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var pid = PidBox.Text.Trim();
        if (pid.Length == 0 || string.IsNullOrWhiteSpace(VisibleText())) return;
        try
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"{pid}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                DefaultExt = ".txt",
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Save panel trace"
            };
            if (dlg.ShowDialog(this) != true) return;
            System.IO.File.WriteAllText(dlg.FileName, VisibleText(), new System.Text.UTF8Encoding(true));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, "Could not save the report: " + ex.Message,
                            "Trace Panel", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    /// <summary>Open as file — a throwaway copy in TEMP, opened in whatever handles .txt.</summary>
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var pid = PidBox.Text.Trim();
            if (pid.Length == 0) return;
            var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FtpUploadTrace");
            System.IO.Directory.CreateDirectory(dir);
            foreach (var old in System.IO.Directory.GetFiles(dir, "*.txt"))
                if ((DateTime.Now - System.IO.File.GetLastWriteTime(old)).TotalDays > 1)
                    try { System.IO.File.Delete(old); } catch { }

            var path = System.IO.Path.Combine(dir, $"{pid}_trace_{DateTime.Now:HHmmss}.txt");
            System.IO.File.WriteAllText(path, VisibleText(), new System.Text.UTF8Encoding(true));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, "Could not save the report: " + ex.Message,
                            "Trace Panel", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }
}
