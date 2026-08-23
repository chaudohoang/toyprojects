using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
// pin the ambiguous names to their WPF versions (WinForms is in scope for the tray icon)
using MessageBox = System.Windows.MessageBox;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace FtpUpload;

public partial class MainWindow : Window
{
    private readonly AppHost _host;
    private readonly ObservableCollection<JobVm> _jobs = new();
    // The NG tab is now the NG-retry console: panels (NgGroupVm cards) grouped by PID, each
    // holding its NgItemVm rows, sourced from the separate NgRetryEngine (file-based, cross-day).
    private readonly ObservableCollection<NgGroupVm> _ng = new();
    private readonly DispatcherTimer _liveTimer;      // fast: live strip only
    private readonly DispatcherTimer _listTimer;      // slow: job cards + stats
    private volatile bool _dirty = true;
    private volatile bool _logDirty = true;
    private volatile bool _ngLogDirty = true;
    private string _filter = "";
    private string _ngFilter = "";

    // Auto-scroll: the All Jobs list follows the file being uploaded, highlighting and
    // scrolling to each one. Manual scrolling (wheel or scrollbar) pauses it; it resumes
    // 3 s after the last manual gesture.
    private DateTime _manualUntil = DateTime.MinValue;
    private bool _wasManual;
    private JobFile? _lastScrolledFile;

    // NG list auto-scroll (mirrors the live list): follows the NG-pump in-flight item.
    private DateTime _ngManualUntil = DateTime.MinValue;
    private bool _ngWasManual;
    private NgItem? _lastScrolledNg;

    public MainWindow(AppHost host)
    {
        _host = host;
        InitializeComponent();

        var build = "";
        try
        {
            // Environment.ProcessPath works even for a single-file publish (Assembly.Location
            // is blank there). Its last-write time is effectively the build time — a quick way
            // to confirm the running exe is actually the one you just built.
            var exe = Environment.ProcessPath;
            if (exe is not null)
                build = $"build {System.IO.File.GetLastWriteTime(exe):MMM d HH:mm}";
        }
        catch { /* non-critical */ }

        // Build stamp lives in the OS title bar now, keeping the in-window subtitle clean.
        Title = build.Length > 0 ? $"FTP Upload Job Manager — {build}" : "FTP Upload Job Manager";

        SubTitle.Text = $"{host.Cfg.PrimaryHost} / {host.Cfg.SecondaryHost}  ·  " +
                        $"{host.Cfg.TimeoutSeconds}s timeout  ·  {host.Cfg.MaxAttempts} attempts";

        // Rows/strips show the full destination as ftp://{host}/{path}. Primary host is the
        // main target (failover to secondary is reflected in the log's "via {host}").
        UiConfig.FtpHost = host.Cfg.PrimaryHost;

        JobList.ItemsSource = _jobs;
        NgList.ItemsSource = _ng;
        NgDate.SelectedDate = DateTime.Today;   // default to today; changing it loads another day
        LoadSettings();                          // populate the Settings tab from config + recipe

        // Title-bar / taskbar icon: reuse the exe's own icon (from <ApplicationIcon>).
        try
        {
            var exe = Environment.ProcessPath;
            if (exe is not null)
            {
                using var ico = System.Drawing.Icon.ExtractAssociatedIcon(exe);
                if (ico is not null)
                    Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        ico.Handle, System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }
        catch { /* non-critical */ }

        // Any manual scroll gesture on the job list pauses auto-scroll for 3 seconds.
        JobList.PreviewMouseWheel += (_, _) => PauseAutoScroll();
        JobList.PreviewMouseDown += (_, e) =>
        {
            if (IsFromScrollBar(e.OriginalSource)) PauseAutoScroll();
        };
        JobList.PreviewKeyDown += (_, e) =>
        {
            if (e.Key is System.Windows.Input.Key.PageUp or System.Windows.Input.Key.PageDown
                or System.Windows.Input.Key.Up or System.Windows.Input.Key.Down
                or System.Windows.Input.Key.Home or System.Windows.Input.Key.End)
                PauseAutoScroll();
        };

        // Same manual-scroll pause for the NG list, so its auto-follow yields to the operator.
        NgList.PreviewMouseWheel += (_, _) => PauseNgAutoScroll();
        NgList.PreviewMouseDown += (_, e) => { if (IsFromScrollBar(e.OriginalSource)) PauseNgAutoScroll(); };
        NgList.PreviewKeyDown += (_, e) =>
        {
            if (e.Key is System.Windows.Input.Key.PageUp or System.Windows.Input.Key.PageDown
                or System.Windows.Input.Key.Up or System.Windows.Input.Key.Down
                or System.Windows.Input.Key.Home or System.Windows.Input.Key.End)
                PauseNgAutoScroll();
        };

        // Two timers on purpose. The elapsed-seconds readout needs a twice-a-second
        // tick, but rebuilding the job list at that rate is what made the previous
        // version feel sluggish. The list only refreshes when the engine says
        // something changed, and at most 4x a second.
        _liveTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        // Hidden is the normal state in production — the operator closes the window and it
        // lives in the tray. Doing any UI work then is pure waste, so every timer bails out.
        _liveTimer.Tick += (_, _) => { if (IsVisible) RefreshLive(); };

        _listTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _listTimer.Tick += (_, _) =>
        {
            if (!IsVisible) return;
            if (_dirty) { _dirty = false; RefreshList(); }
            if (_logDirty) { _logDirty = false; RefreshLog(); }
            if (_ngLogDirty) { _ngLogDirty = false; RefreshNgLog(); }
        };

        _host.Changed += () => _dirty = true;
        _host.LogChanged += () => _logDirty = true;
        _host.NgLogChanged += () => _ngLogDirty = true;

        Loaded += (_, _) =>
        {
            RefreshLive();
            RefreshList();
            RefreshLog();
            RefreshNgLog();
            _liveTimer.Start();
            _listTimer.Start();
            StartPingLoop();
        };
    }

    // ---------------- refresh ----------------

    // frozen once, not reallocated on every tick
    private static readonly SolidColorBrush DotIdle = Freeze(0x5A, 0x64, 0x78);
    private static readonly SolidColorBrush DotBusy = Freeze(0xFF, 0x4D, 0x8C);   // hot pink = actively transferring
    private static SolidColorBrush Freeze(byte r, byte g, byte b)
    {
        var br = new SolidColorBrush(Color.FromRgb(r, g, b));
        br.Freeze();
        return br;
    }

    private JobFile? _lastLiveFile;
    private int _lastQueued = -1;

    private void RefreshLive()
    {
        var f = _host.Engine.InFlight;
        var queued = _host.Engine.QueueLength;

        // Only the elapsed counter changes every tick; everything else is left alone
        // unless the file being uploaded actually changed.
        if (queued != _lastQueued)
        {
            _lastQueued = queued;
            LiveCount.Text = queued > 0 ? $"{queued} waiting in line" : "";
        }

        if (!ReferenceEquals(f, _lastLiveFile))
        {
            _lastLiveFile = f;
            if (f is null)
            {
                LiveDot.Fill = DotIdle;
                LiveIdle.Visibility = Visibility.Visible;
                LiveRow.Visibility = Visibility.Collapsed;
            }
            else
            {
                LiveDot.Fill = DotBusy;
                LiveIdle.Visibility = Visibility.Collapsed;
                LiveRow.Visibility = Visibility.Visible;
                LivePid.Text = f.Pid;
                LiveFile.Text = f.FileName;
                LivePath.Text = UiConfig.WithHost(f.RemotePath);
            }
        }

        if (f is not null)
            LiveElapsed.Text = $"{(DateTime.Now - _host.Engine.InFlightStarted).TotalSeconds:0.0}s";

        // Session reuse status: which connection we're on and how many files it has carried
        // (out of the per-session cap, or ∞ when unlimited).
        var cap = _host.Cfg.MaxFilesPerSession;
        var capText = cap > 0 ? cap.ToString() : "\u221E";   // ∞
        StatSession.Text = f is not null
            ? $"·  Session #{_host.Engine.SessionNumber} · {_host.Engine.FilesThisSession}/{capText}"
            : "";

        var paused = _host.Engine.Paused;
        LiveStartBtn.IsEnabled = paused;
        LiveStopBtn.IsEnabled = !paused;
        // Badge = armed (not paused). Dot = a file is actually transferring right now.
        LiveDot.Fill = (f is not null) ? DotBusy : DotIdle;
        LiveRunningBadge.Visibility = paused ? Visibility.Collapsed : Visibility.Visible;
        if (f is null)
            LiveIdle.Text = paused
                ? "Paused — uploads held (jobs still queued)."
                : "Idle — no uploads in progress right now.";

        UpdateAutoScroll(f);
        UpdateNgStatus();
        UpdateNgAutoScroll();
    }

    // NG list follows the NG-retry pump's in-flight item (flat list, so a direct ScrollIntoView).
    private void PauseNgAutoScroll() => _ngManualUntil = DateTime.Now.AddSeconds(5);

    private void UpdateNgAutoScroll()
    {
        if (!ReferenceEquals(Tabs.SelectedItem, NgTab)) return;    // only when the NG tab is showing
        if (DateTime.Now < _ngManualUntil) { _ngWasManual = true; return; }
        var resumed = _ngWasManual;
        _ngWasManual = false;

        var it = _host.NgRetry.Current;
        if (it is null) { _lastScrolledNg = null; return; }
        if (!resumed && ReferenceEquals(it, _lastScrolledNg)) return;
        _lastScrolledNg = it;

        // Scroll to the panel card holding the in-flight item (the list items are groups now).
        var group = _ng.FirstOrDefault(g => g.Pid == it.Pid);
        if (group is not null) NgList.ScrollIntoView(group);
    }

    /// <summary>Live NG-console strip: current item, elapsed, queue, RUNNING state, buttons.</summary>
    private void UpdateNgStatus()
    {
        var ng = _host.NgRetry;
        var running = ng.AutoRunning;

        // Follow the loaded day (e.g. after a day rollover re-points the console). The date-changed
        // handler is a no-op when the day already matches, so this can't loop.
        if (!string.IsNullOrEmpty(ng.LoadedDay)
            && DateTime.TryParseExact(ng.LoadedDay, "yyyyMMdd", null,
                                      System.Globalization.DateTimeStyles.None, out var loaded)
            && NgDate.SelectedDate?.Date != loaded.Date)
        {
            NgDate.SelectedDate = loaded.Date;
        }

        NgRunningBadge.Visibility = running ? Visibility.Visible : Visibility.Collapsed;
        NgDot.Fill = ng.InFlight is not null ? DotBusy : DotIdle;   // blue only while transferring
        NgStartBtn.IsEnabled = !running;
        NgStopBtn.IsEnabled = running;
        NgCount.Text = running ? $"{ng.QueueLength} queued" : "";

        var cur = ng.Current;
        if (cur is not null)
        {
            NgIdle.Visibility = Visibility.Collapsed;
            NgRow.Visibility = Visibility.Visible;
            NgCurPid.Text = cur.Pid;
            NgCurFile.Text = cur.FileName;
            NgCurPath.Text = UiConfig.WithHost(cur.RemotePath);
            NgCurIp.Text = cur.LastHost;
            NgCurElapsed.Text = ng.InFlight is not null
                ? $"{(DateTime.Now - ng.InFlightStarted).TotalSeconds:0.0}s"
                : "cooldown…";
        }
        else
        {
            NgRow.Visibility = Visibility.Collapsed;
            NgIdle.Visibility = Visibility.Visible;
            var remaining = ng.QueueLength;
            NgIdle.Text = running
                ? (remaining > 0
                    ? $"Recovering — {remaining} item(s) still failing, retrying between sweeps…"
                    : "Monitoring — all recovered; watching for new failures.")
                : $"Stopped — day {ng.LoadedDay}, {_ng.Sum(g => g.Items.Count)} item(s). Press Auto Retry to recover them.";
        }
    }

    // ---------------- auto-scroll to the uploading file ----------------

    private void PauseAutoScroll() => _manualUntil = DateTime.Now.AddSeconds(5);

    /// <summary>
    /// Follows the in-flight file on the All Jobs tab: scrolls to it whenever it changes (or
    /// when auto-scroll resumes after a manual pause). Skipped while the user is scrolling, on
    /// other tabs, or when the window is hidden (the caller already gates on IsVisible).
    /// </summary>
    private void UpdateAutoScroll(JobFile? f)
    {
        if (DateTime.Now < _manualUntil) { _wasManual = true; return; }   // user is scrolling
        var resumed = _wasManual;
        _wasManual = false;

        if (!ReferenceEquals(Tabs.SelectedItem, AllTab)) return;          // only the All Jobs list
        if (f is null) { _lastScrolledFile = null; return; }

        if (!resumed && ReferenceEquals(f, _lastScrolledFile)) return;    // already parked on it
        _lastScrolledFile = f;
        ScrollToInFlight(f);
    }

    private void ScrollToInFlight(JobFile f)
    {
        var jobVm = _jobs.FirstOrDefault(j => j.Pid == f.Pid);
        if (jobVm is null) return;

        // If this panel is collapsed, open it so the active file is actually visible.
        jobVm.IsExpanded = true;

        // realize the panel card first, then bring the exact file row into view once laid out
        JobList.ScrollIntoView(jobVm);
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            if (JobList.ItemContainerGenerator.ContainerFromItem(jobVm) is not DependencyObject card)
                return;
            var files = FindDescendant<System.Windows.Controls.ItemsControl>(card);
            var rowVm = jobVm.Files.FirstOrDefault(r => r.FileName == f.FileName);
            if (files is null || rowVm is null) return;
            if (files.ItemContainerGenerator.ContainerFromItem(rowVm) is FrameworkElement row)
                row.BringIntoView();
        }));
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T hit) return hit;
            var deeper = FindDescendant<T>(child);
            if (deeper is not null) return deeper;
        }
        return null;
    }

    private static bool IsFromScrollBar(object? src)
    {
        var d = src as DependencyObject;
        while (d is not null)
        {
            if (d is System.Windows.Controls.Primitives.ScrollBar) return true;
            d = System.Windows.Media.VisualTreeHelper.GetParent(d);
        }
        return false;
    }

    /// <summary>A file is "terminal" when nothing more will be done with it: it succeeded, it
    /// failed / timed out, or it's out of attempts. (An in-flight file is Pending with attempts
    /// left, so it is NOT terminal — its panel stays visible.)</summary>
    private bool FileTerminal(JobFile f) =>
        f.Status == FileStatus.Succeeded || f.Status == FileStatus.Failed || f.Status == FileStatus.TimedOut
        || (f.Status == FileStatus.Pending && f.Attempts >= _host.Cfg.MaxAttempts);

    /// <summary>A live panel is hidden once EVERY file is terminal (all retries consumed, whether
    /// they succeeded or not). Failures still live on in the NG list.</summary>
    private bool PanelDone(Job j) => j.Files.Count > 0 && j.Files.All(FileTerminal);

    private void RefreshList()
    {
        var inFlight = _host.Engine.InFlight;

        // Everything matching the PID filter — used for the day-total stats strip.
        var matched = _host.Engine.Jobs
            .Where(j => _filter.Length == 0 ||
                        j.Pid.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Cards show only ACTIVE panels: finished ones (all files terminal) drop out of the list.
        var visible = matched.Where(j => !PanelDone(j)).ToList();

        // add new jobs
        foreach (var job in visible)
            if (_jobs.All(v => v.Pid != job.Pid))
                _jobs.Add(new JobVm(job, _host.Cfg.MaxAttempts));

        // drop jobs no longer visible (deleted, filtered out, or now finished)
        foreach (var stale in _jobs.Where(v => visible.All(j => j.Pid != v.Pid)).ToList())
            _jobs.Remove(stale);

        foreach (var vm in _jobs) vm.Refresh(inFlight);

        var files = matched.Sum(j => j.Files.Count);
        var ok = matched.Sum(j => j.Files.Count(f => f.Status == FileStatus.Succeeded));
        var bad = matched.Sum(j => j.Files.Count(f =>
            f.Status == FileStatus.Failed || f.Status == FileStatus.TimedOut));

        StatJobs.Text = matched.Count.ToString();
        StatFiles.Text = files.ToString();
        StatOk.Text = ok.ToString();
        StatPending.Text = (files - ok - bad).ToString();
        StatFailed.Text = bad.ToString();

        var mbps = _host.Engine.RollingMBps;
        StatSpeed.Text = mbps > 0 ? $"·  {mbps:0.0} MB/s avg" : "";

        SyncNg();

        // Tab headers show the ACTIVE/current count, so they rise as work arrives and fall as it
        // clears: live = panels still in the pump (not yet all-terminal); NG = items still needing
        // attention (Waiting / Uploading / Failed), i.e. recovered/gone ones drop off.
        AllTab.Header = $"Today Jobs ({visible.Count})";
        var ngActive = _host.NgRetry.Items.Count(i =>
            i.State is NgItemState.Waiting or NgItemState.Uploading or NgItemState.Failed);
        NgTab.Header = $"NG List ({ngActive})";
        NgEmpty.Visibility = _ng.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Reconciles the NG tab with the NG-retry engine's item list. Rows are keyed on the item's
    /// stable identity string (Day|PID|File), so a Retry just refreshes the row in place and a
    /// day reload REBINDS existing rows rather than clearing and rebuilding — the list never
    /// blanks out. Items that succeed stay visible (green) until the day is reloaded.
    /// </summary>
    private void SyncNg()
    {
        // The NG filter is view-only — the pump still works the full loaded list; this just
        // controls which rows are shown, using the NG tab's own PID box (separate from Today Jobs).
        var items = _host.NgRetry.Items
            .Where(it => _ngFilter.Length == 0 ||
                         it.Pid.Contains(_ngFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Group by PID, preserving first-seen order so cards don't jump around.
        var order = new List<string>();
        var byPid = new Dictionary<string, List<NgItem>>();
        foreach (var it in items)
        {
            if (!byPid.TryGetValue(it.Pid, out var list))
            {
                list = new List<NgItem>();
                byPid[it.Pid] = list;
                order.Add(it.Pid);
            }
            list.Add(it);
        }

        // Hide fully-recovered panels: keep a PID only while at least one item still needs
        // attention (Waiting / Uploading / Failed). All Succeeded (or Gone) → drop the card.
        static bool NeedsAttention(NgItem it) =>
            it.State is NgItemState.Waiting or NgItemState.Uploading or NgItemState.Failed;
        order = order.Where(pid => byPid[pid].Any(NeedsAttention)).ToList();

        // Reconcile group cards by PID — existing cards are refreshed in place (keeping their
        // expand/collapse state and row objects), new PIDs are added, vanished PIDs removed.
        var existing = new Dictionary<string, NgGroupVm>();
        foreach (var g in _ng) existing[g.Pid] = g;

        foreach (var pid in order)
        {
            if (!existing.TryGetValue(pid, out var group))
            {
                group = new NgGroupVm(pid);
                _ng.Add(group);
                existing[pid] = group;
            }
            group.Refresh(byPid[pid]);
        }

        var wantedPids = new HashSet<string>(order);
        for (var i = _ng.Count - 1; i >= 0; i--)
            if (!wantedPids.Contains(_ng[i].Pid))
                _ng.RemoveAt(i);
    }

    private string _lastLogText = "";

    private void RefreshLog()
    {
        // newest first, capped — the full history lives in the raw log on disk.
        // Re-laying out a wrapped TextBlock is not free, so skip it if nothing changed.
        var text = string.Join(Environment.NewLine, _host.LogLines.Reverse().Take(40));
        if (text == _lastLogText) return;
        _lastLogText = text;
        LogText.Text = text;
    }

    private string _lastNgLogText = "";

    private void RefreshNgLog()
    {
        var text = string.Join(Environment.NewLine, _host.NgLogLines.Reverse().Take(40));
        if (text == _lastNgLogText) return;
        _lastNgLogText = text;
        NgLogText.Text = text;
    }

    // ---------------- actions ----------------

    private static T? FindVm<T>(object sender) where T : class
        => (sender as FrameworkElement)?.DataContext as T;

    private void ForceUpload_Click(object sender, RoutedEventArgs e)
    {
        if (FindVm<FileRowVm>(sender) is not { } row) return;
        _host.Engine.ForceUpload(row.Model.Pid, row.Model.FileName);
        _dirty = true;
    }

    private void Retry_Click(object sender, RoutedEventArgs e)
    {
        if (FindVm<NgItemVm>(sender) is not { } row) return;
        _host.NgRetry.RetryOne(row.Model.Key);
        _dirty = true;
    }

    /// <summary>Expand every NG panel card.</summary>
    private void NgExpandAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var g in _ng) g.IsExpanded = true;
    }

    /// <summary>Collapse every NG panel card.</summary>
    private void NgCollapseAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var g in _ng) g.IsExpanded = false;
    }

    /// <summary>Expand every live (Today Jobs) panel card.</summary>
    private void LiveExpandAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var j in _jobs) j.IsExpanded = true;
    }

    /// <summary>Collapse every live (Today Jobs) panel card.</summary>
    private void LiveCollapseAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var j in _jobs) j.IsExpanded = false;
    }

    // ---------------- settings ----------------

    private static int ParseInt(string? s, int fallback) => int.TryParse(s?.Trim(), out var v) ? v : fallback;
    private static bool IsNonNegInt(string? s) => int.TryParse(s?.Trim(), out var v) && v >= 0;

    private static readonly System.Windows.Media.Brush OkBorder =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD6, 0xDB, 0xE6));
    private static readonly System.Windows.Media.Brush BadBorder = System.Windows.Media.Brushes.Crimson;

    /// <summary>Set a field's border to signal valid/invalid and return the ok value (so callers
    /// can accumulate). Uses '&amp;=' at call sites so every field is checked, not short-circuited.</summary>
    private bool Mark(System.Windows.Controls.Control c, bool ok)
    {
        c.BorderBrush = ok ? OkBorder : BadBorder;
        return ok;
    }

    /// <summary>Validate the form, highlighting any bad fields. Required: primary host, a valid
    /// port, the queue/recipe/jobs/log/state paths; numeric fields must be non-negative ints
    /// (poll interval &gt; 0). Backup/processed folders are optional. Returns true if all valid.</summary>
    private bool ValidateSettings()
    {
        var ok = true;
        ok &= Mark(SetPrimaryHost, SetPrimaryHost.Text.Trim().Length > 0);
        ok &= Mark(SetPort, int.TryParse(SetPort.Text.Trim(), out var p) && p is > 0 and <= 65535);

        ok &= Mark(SetQueueFolder, SetQueueFolder.Text.Trim().Length > 0);
        ok &= Mark(SetRecipePath, SetRecipePath.Text.Trim().Length > 0);
        ok &= Mark(SetJobsFolder, SetJobsFolder.Text.Trim().Length > 0);
        ok &= Mark(SetLogFolder, SetLogFolder.Text.Trim().Length > 0);
        ok &= Mark(SetStateFolder, SetStateFolder.Text.Trim().Length > 0);

        ok &= Mark(SetTimeout, IsNonNegInt(SetTimeout.Text));
        ok &= Mark(SetPrimaryRetries, IsNonNegInt(SetPrimaryRetries.Text));
        ok &= Mark(SetSecondaryRetries, IsNonNegInt(SetSecondaryRetries.Text));
        ok &= Mark(SetPanelTimeout, IsNonNegInt(SetPanelTimeout.Text));
        ok &= Mark(SetPollInterval, IsNonNegInt(SetPollInterval.Text) && ParseInt(SetPollInterval.Text, 0) > 0);
        ok &= Mark(SetLogRetention, IsNonNegInt(SetLogRetention.Text));
        // Max files/session is a combo (Unlimited/100/300/500) — always valid, nothing to check.
        return ok;
    }

    // Snapshot of restart-required values as last loaded/saved, so a Save can report exactly which
    // of them changed. Fields NOT listed here take effect live (queue/recipe/backup/processed
    // folders, panel timeout, poll interval, the recipe text) and never need a restart.
    private Dictionary<string, string> _restartSnapshot = new();

    private Dictionary<string, string> RestartRelevant()
    {
        var c = _host.Cfg;
        return new Dictionary<string, string>
        {
            ["Primary host"] = c.PrimaryHost,
            ["Secondary host"] = c.SecondaryHost,
            ["Port"] = c.Port.ToString(),
            ["User"] = c.User,
            ["Password"] = c.Password,
            ["FTP security"] = c.FtpSecure,
            ["Connect timeout"] = c.TimeoutSecondsOverride.ToString(),
            ["Primary retries"] = c.PrimaryRetries.ToString(),
            ["Secondary retries"] = c.SecondaryRetries.ToString(),
            ["Jobs folder"] = c.JobsFolder,
            ["Log folder"] = c.LogFolder,
            ["State folder"] = c.StateFolder,
        };
    }

    private static void SelectCombo(System.Windows.Controls.ComboBox cb, string content)
    {
        foreach (var it in cb.Items)
            if (it is System.Windows.Controls.ComboBoxItem ci &&
                string.Equals(ci.Content?.ToString(), content, StringComparison.OrdinalIgnoreCase))
            { cb.SelectedItem = it; return; }
        cb.SelectedIndex = 0;
    }

    /// <summary>Fill the Settings form from the live config + the recipe file on disk.</summary>
    private void LoadSettings()
    {
        var c = _host.Cfg;
        SetPrimaryHost.Text = c.PrimaryHost;
        SetSecondaryHost.Text = c.SecondaryHost;
        SetPort.Text = c.Port.ToString();
        SetUser.Text = c.User;
        SetPassword.Text = c.Password;
        SelectCombo(SetFtpSecure, c.FtpSecure);

        SetQueueFolder.Text = c.QueueFolder;
        SetRecipePath.Text = c.RecipePath;
        SetBackupFolder.Text = c.PanelBackupFolder;
        SetJobsFolder.Text = c.JobsFolder;
        SetLogFolder.Text = c.LogFolder;
        SetStateFolder.Text = c.StateFolder;

        SetTimeout.Text = c.TimeoutSecondsOverride.ToString();
        SetPrimaryRetries.Text = c.PrimaryRetries.ToString();
        SetSecondaryRetries.Text = c.SecondaryRetries.ToString();
        SetPanelTimeout.Text = c.PanelTimeoutSeconds.ToString();
        SetPollInterval.Text = c.PollIntervalMs.ToString();
        SetLogRetention.Text = c.LogRetentionDays.ToString();
        SelectCombo(SetMaxFilesPerSession, c.MaxFilesPerSession <= 0 ? "Unlimited" : c.MaxFilesPerSession.ToString());

        SetAutoUpload.IsChecked = c.AutoStartUploading;
        SetAutoRetry.IsChecked = c.AutoStartRetrying;

        SetRecipePathLabel.Text = "File: " + c.RecipeFullPath;
        try { SetRecipeText.Text = System.IO.File.Exists(c.RecipeFullPath) ? System.IO.File.ReadAllText(c.RecipeFullPath) : ""; }
        catch (Exception ex) { SetRecipeText.Text = "# could not read recipe: " + ex.Message; }

        _restartSnapshot = RestartRelevant();   // baseline for detecting restart-needing changes
    }

    /// <summary>Read the form into the live config, persist config.json, and write the recipe file.
    /// Returns true on success.</summary>
    private bool ApplySettings()
    {
        if (!ValidateSettings())
        {
            SettingsHint.Foreground = BadBorder;
            SettingsHint.Text = "Please fix the highlighted fields, then Save.";
            return false;
        }
        try
        {
            var c = _host.Cfg;
            c.PrimaryHost = SetPrimaryHost.Text.Trim();
            c.SecondaryHost = SetSecondaryHost.Text.Trim();
            c.Port = ParseInt(SetPort.Text, c.Port);
            c.User = SetUser.Text.Trim();
            c.Password = SetPassword.Text;
            c.FtpSecure = (SetFtpSecure.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? c.FtpSecure;

            c.QueueFolder = SetQueueFolder.Text.Trim();
            c.RecipePath = SetRecipePath.Text.Trim();
            c.PanelBackupFolder = SetBackupFolder.Text.Trim();
            c.JobsFolder = SetJobsFolder.Text.Trim();
            c.LogFolder = SetLogFolder.Text.Trim();
            c.StateFolder = SetStateFolder.Text.Trim();

            c.TimeoutSecondsOverride = ParseInt(SetTimeout.Text, c.TimeoutSecondsOverride);
            c.PrimaryRetries = ParseInt(SetPrimaryRetries.Text, c.PrimaryRetries);
            c.SecondaryRetries = ParseInt(SetSecondaryRetries.Text, c.SecondaryRetries);
            c.PanelTimeoutSeconds = ParseInt(SetPanelTimeout.Text, c.PanelTimeoutSeconds);
            c.PollIntervalMs = ParseInt(SetPollInterval.Text, c.PollIntervalMs);
            c.LogRetentionDays = ParseInt(SetLogRetention.Text, c.LogRetentionDays);
            // Combo: "Unlimited" -> 0, otherwise the numeric preset.
            var sessSel = (SetMaxFilesPerSession.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Unlimited";
            c.MaxFilesPerSession = sessSel.Equals("Unlimited", StringComparison.OrdinalIgnoreCase) ? 0 : ParseInt(sessSel, 0);

            c.AutoStartUploading = SetAutoUpload.IsChecked == true;
            c.AutoStartRetrying = SetAutoRetry.IsChecked == true;

            c.Save(_host.ConfigPath);

            // Create any missing folders (resolved against the exe for relative paths).
            try { c.EnsureFolders(); }
            catch (Exception ex) { SettingsHint.Text = "Saved; some folders could not be created: " + ex.Message; }

            // Recipe file: write to the resolved path (may have just changed via RecipePath).
            try
            {
                var rp = c.RecipeFullPath;
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(rp))!);
                System.IO.File.WriteAllText(rp, SetRecipeText.Text);
                SetRecipePathLabel.Text = "File: " + rp;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Settings saved, but the recipe could not be written:\n{ex.Message}",
                    "Recipe", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }

            // Report exactly which restart-required fields changed since load/last save.
            var now = RestartRelevant();
            var changed = now.Where(kv => !_restartSnapshot.TryGetValue(kv.Key, out var old) || old != kv.Value)
                             .Select(kv => kv.Key).ToList();
            _restartSnapshot = now;

            SettingsHint.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xA9, 0xB3, 0xCC));
            SettingsHint.Text = changed.Count > 0
                ? $"Saved {DateTime.Now:HH:mm:ss}. Restart to apply: {string.Join(", ", changed)}."
                : $"Saved {DateTime.Now:HH:mm:ss}. Changes applied (no restart needed).";
            return true;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Could not save settings:\n{ex.Message}",
                "Settings", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
    }

    private void SettingsSave_Click(object sender, RoutedEventArgs e)
    {
        // ApplySettings sets SettingsHint with the exact restart-required fields (if any).
        ApplySettings();
    }

    // ---------------- view HTML log ----------------

    private void ViewLiveLog_Click(object sender, RoutedEventArgs e)
    {
        var days = AvailableLogDays(liveMode: true);
        if (days.Count == 0) { System.Windows.MessageBox.Show("No upload logs yet."); return; }
        var dlg = new DayPickerWindow(days, "Choose a day — upload log") { Owner = this };
        if (dlg.ShowDialog() == true && dlg.SelectedDay is string day)
            BuildAndOpenLog(() => HtmlLog.BuildDayLog(_host.Cfg, day), day);
    }

    private void ViewNgLog_Click(object sender, RoutedEventArgs e)
    {
        var days = AvailableLogDays(liveMode: false);
        if (days.Count == 0) { System.Windows.MessageBox.Show("No NG-retry logs yet."); return; }
        var dlg = new DayPickerWindow(days, "Choose a day — NG-retry log") { Owner = this };
        if (dlg.ShowDialog() == true && dlg.SelectedDay is string day)
            BuildAndOpenLog(() => HtmlLog.BuildNgLog(_host.Cfg, day), day);
    }

    /// <summary>Days (yyyyMMdd) that have a log to show. Live = a raw log or a jobs file exists;
    /// NG = an ngretrylog exists.</summary>
    private List<string> AvailableLogDays(bool liveMode)
    {
        var set = new SortedSet<string>();
        static bool IsDay(string s) => s.Length == 8 && s.All(char.IsDigit);
        void Scan(string dir, string suffix)
        {
            try
            {
                if (!System.IO.Directory.Exists(dir)) return;
                foreach (var f in System.IO.Directory.GetFiles(dir, "*" + suffix))
                {
                    var name = System.IO.Path.GetFileName(f);
                    if (name.Length >= 8 && IsDay(name.Substring(0, 8))) set.Add(name.Substring(0, 8));
                }
            }
            catch { /* ignore unreadable folders */ }
        }
        if (liveMode)
        {
            Scan(_host.Cfg.LogFullPath, "_rawlog.txt");
            Scan(_host.Cfg.JobsFullPath, "_jobs.txt");
        }
        else
        {
            Scan(_host.Cfg.LogFullPath, "_ngretrylog.txt");
        }
        return set.ToList();
    }

    /// <summary>Build the HTML report in-process (no PowerShell) and open it in the browser.
    /// The _htmllog.ps1 / _nghtmllog.ps1 scripts remain for one-click / scheduled use.</summary>
    private void BuildAndOpenLog(Func<string?> build, string day)
    {
        try
        {
            var path = build();
            if (path is null || !System.IO.File.Exists(path))
            {
                System.Windows.MessageBox.Show($"No log to show for {day} yet.");
                return;
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Could not build or open the log:\n{ex.Message}");
        }
    }

    // ---------------- host ping (every 30s) ----------------

    private DispatcherTimer? _pingTimer;
    private static readonly System.Windows.Media.Brush PingUp =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1F, 0x9D, 0x55));
    private static readonly System.Windows.Media.Brush PingDown =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE0, 0x48, 0x3F));
    private static readonly System.Windows.Media.Brush PingIdle =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC7, 0xCD, 0xD9));

    private void StartPingLoop()
    {
        _ = PingBothAsync();   // once immediately
        _pingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _pingTimer.Tick += async (_, _) => await PingBothAsync();
        _pingTimer.Start();
    }

    private async Task PingBothAsync()
    {
        await PingOneAsync(_host.Cfg.PrimaryHost, PingPrimaryDot, PingPrimaryText);
        await PingOneAsync(_host.Cfg.SecondaryHost, PingSecondaryDot, PingSecondaryText);
    }

    private static async Task PingOneAsync(string host, System.Windows.Shapes.Ellipse dot, System.Windows.Controls.TextBlock text)
    {
        if (string.IsNullOrWhiteSpace(host)) { dot.Fill = PingIdle; text.Text = "—"; return; }
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync(host, 3000);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                dot.Fill = PingUp;
                text.Text = $"{reply.RoundtripTime} ms";
            }
            else
            {
                dot.Fill = PingDown;
                text.Text = "unreachable";
            }
        }
        catch
        {
            dot.Fill = PingDown;
            text.Text = "unreachable";
        }
    }

    private void SettingsSaveRestart_Click(object sender, RoutedEventArgs e)
    {
        if (!ApplySettings()) return;

        var exe = Environment.ProcessPath;
        if (exe is null) { System.Windows.MessageBox.Show("Cannot determine the exe path to relaunch."); return; }

        try
        {
            // Detached relaunch: a helper waits ~1s (for this process to exit and release any
            // single-instance lock / file handles) then starts the fresh exe. We shut down now.
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe",
                $"/c timeout /t 1 /nobreak > nul & start \"\" \"{exe}\" --show")
            { CreateNoWindow = true, UseShellExecute = false });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Saved, but could not relaunch:\n{ex.Message}");
            return;
        }
        System.Windows.Application.Current.Shutdown();
    }

    private void NgDate_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var day = (NgDate.SelectedDate ?? DateTime.Today).ToString("yyyyMMdd");
        if (day == _host.NgRetry.LoadedDay) return;   // already showing this day — don't reload
                                                       // (that would reset the running auto-retry)
        _host.NgRetry.LoadDay(day);
        _dirty = true;
    }

    private void NgIp_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _host.NgRetry.IpMode = (NgIp?.SelectedIndex) switch
        {
            1 => NgIpMode.Primary,
            2 => NgIpMode.Secondary,
            _ => NgIpMode.Auto
        };
    }

    private void NgStart_Click(object sender, RoutedEventArgs e)
    {
        _host.NgRetry.StartAutoRetry();
        _dirty = true;
    }

    private void NgStop_Click(object sender, RoutedEventArgs e)
    {
        _host.NgRetry.StopAutoRetry();
        _dirty = true;
    }

    private void LiveStart_Click(object sender, RoutedEventArgs e) { _host.Engine.Resume(); _dirty = true; }
    private void LiveStop_Click(object sender, RoutedEventArgs e) { _host.Engine.Pause(); _dirty = true; }

    private void DeleteFile_Click(object sender, RoutedEventArgs e)
    {
        if (FindVm<FileRowVm>(sender) is not { } row) return;
        if (MessageBox.Show($"Remove {row.Model.FileName} from job {row.Model.Pid}?",
                "Delete file", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _host.Engine.DeleteFile(row.Model.Pid, row.Model.FileName);
        _dirty = true;
    }

    private void DeleteJob_Click(object sender, RoutedEventArgs e)
    {
        if (FindVm<JobVm>(sender) is not { } job) return;
        if (MessageBox.Show($"Delete the entire job {job.Pid} ({job.Model.Files.Count} files)?",
                "Delete job", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        _host.Engine.DeletePanel(job.Pid);
        _dirty = true;
    }

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _filter = FilterBox.Text.Trim();   // masthead box → Today Jobs list only
        _dirty = true;
    }

    private void NgFilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _ngFilter = NgFilterBox.Text.Trim();   // NG tab's own box → NG list only
        _dirty = true;
    }

    // ---------------- hiding to tray ----------------

    /// <summary>
    /// Minimising sends the window to the tray rather than the taskbar, so closing and
    /// minimising both put the program in the same one place.
    /// </summary>
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized) HideToTray();
    }

    private void HideToTray()
    {
        // restore first, so reopening comes back maximized instead of minimized
        WindowState = WindowState.Maximized;
        Hide();
        // Tell the operator once that closing did not stop the uploads — otherwise the
        // window just vanishes and it looks like the program quit.
        (System.Windows.Application.Current as App)?.NotifyHiddenOnce();
    }

    /// <summary>Closing only hides the window — the upload engine keeps running.</summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!App.ShuttingDown)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }
        _liveTimer.Stop();
        _listTimer.Stop();
        base.OnClosing(e);
    }
}
