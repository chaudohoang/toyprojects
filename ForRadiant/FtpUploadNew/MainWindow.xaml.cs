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

        // Must be set BEFORE InitializeComponent: the XAML marks "Auto" IsSelected, which raises
        // SelectionChanged during construction. Without this guard that phantom event overwrites a
        // saved Primary/Secondary with Auto on every single launch, before we ever restore it.
        InitializeComponent();

        // Proper app version in the title bar, read from the assembly (set by <Version> in the
        // .csproj - bump it when shipping a new build).
        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        // Include Revision when it is non-zero. Major.Minor.Build alone renders 1.0.0.1 as "v1.0.0",
        // which makes a shipped patch build indistinguishable from the one it replaced - exactly the
        // thing this title is meant to let you check at a glance.
        var vstr = ver is null ? ""
                 : ver.Revision > 0 ? $"v{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}"
                 : $"v{ver.Major}.{ver.Minor}.{ver.Build}";

        // Build date alongside the version.
        //
        // Two builds a day apart both call themselves v1.0.0.4, and that cost real confusion: a
        // summary CSV was read through a day-old deployed copy while the fix sat in a newer build,
        // with nothing on screen to tell them apart. The date makes the running binary identifiable.
        var built = "";
        try
        {
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe) && System.IO.File.Exists(exe))
                built = System.IO.File.GetLastWriteTime(exe).ToString("yyyy.MMdd.HHmmss");
        }
        catch { }

        var stamp = vstr.Length > 0
            ? (built.Length > 0 ? $"{vstr} \u2014 {built}" : vstr)
            : built;

        Title = stamp.Length > 0 ? $"FTP Upload Job Manager - {stamp}" : "FTP Upload Job Manager";
        BuildStamp.Text = stamp;

        // Client IP first: a site runs many of these across 10.119.x / 10.121.x, and "which PC is
        // this?" should be answerable from the window without opening a console.
        SubTitle.Text = $"{NetInfo.LocalIp(host.Cfg.FirstHost)} -> {host.Cfg.FirstHost}  -  " +
                        $"{host.Cfg.TimeoutSeconds}s timeout  -  {host.Cfg.MaxAttempts} attempts";

        // Rows/strips show the destination as ftp://{host}/{path}. Use the host the first attempt
        // actually targets - cfg.PrimaryHost names the wrong server whenever InitialHost=Secondary,
        // so every row displayed a destination the file was never sent to.
        UiConfig.FtpHost = host.Cfg.FirstHost;

        // Show which single IP everything uses. Sourced from Settings, not a separate NG control.
        NgIpLabel.Text = host.Cfg.InitialHost.Equals("Secondary", StringComparison.OrdinalIgnoreCase)
            ? $"Secondary ({host.Cfg.SecondaryHost})" : $"Primary ({host.Cfg.PrimaryHost})";

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
        // Hidden is the normal state in production - the operator closes the window and it
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
    private static readonly SolidColorBrush DotDayOk = Freeze(0x2E, 0xA0, 0x62);   // green = day settled, pump free
    private static readonly SolidColorBrush DotDayRolling = Freeze(0xE0, 0x8A, 0x1E);  // amber = rollover settling
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
        // (out of the per-session cap, or - when unlimited).
        var cap = _host.Cfg.MaxFilesPerSession;
        var capText = cap > 0 ? cap.ToString() : "\u221E";   // -
        StatSession.Text = f is not null
            ? $"-  Session #{_host.Engine.SessionNumber} - {_host.Engine.FilesThisSession}/{capText}"
            : "";

        var paused = _host.Engine.Paused;
        LiveStartBtn.IsEnabled = paused;
        LiveStopBtn.IsEnabled = !paused;
        // Badge = armed (not paused). Dot = a file is actually transferring right now.
        LiveDot.Fill = (f is not null) ? DotBusy : DotIdle;
        LiveRunningBadge.Visibility = paused ? Visibility.Collapsed : Visibility.Visible;
        if (f is null)
            LiveIdle.Text = paused
                ? "Paused - uploads held (jobs still queued)."
                : _host.Engine.RolloverPending
                    ? "Day rollover settling - uploads are held until it completes."
                    : "Idle - no uploads in progress right now.";

        UpdateDayBadge();
        UpdateAutoScroll(f);
        UpdateNgStatus();
        UpdateNgAutoScroll();
    }

    /// <summary>
    /// Header "Day" badge: which day the engine is filing work under, and when it last rolled over.
    /// Amber while a rollover is settling - in that state the live pump is HELD, so jobs can pile up
    /// in the list without uploading, and this is what tells the operator why.
    /// </summary>
    private void UpdateDayBadge()
    {
        var pending = _host.Engine.RolloverPending;
        DayText.Text = Clock.Today.ToString("yyyyMMdd");
        DayDot.Fill = pending ? DotDayRolling : DotDayOk;

        if (pending)
        {
            DayRolled.Text = "- rolling over...";
            return;
        }

        var at = _host.Engine.LastRolloverAt;
        DayRolled.Text = at is null
            ? ""    // no rollover yet this run - the app started on this day
            : $"- rolled {at:HH:mm:ss} from {_host.Engine.LastRolloverFromDay} " +
              $"({_host.Engine.LastRolloverAbandoned} - NG)";
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
        // Mini version of the NG report: same states, same denominator, same colours as the live
        // strip. ng.Items holds only what is still OUTSTANDING — BuildItems drops anything already
        // recovered — so counting it alone gave "14 items, 100% recovered" against the report's
        // "36 items, 83.3%". The total has to include the recoveries, which RecoveredKeys carries.
        var ngItems = ng.Items;
        var ngRec = ng.RecoveredKeys.Count;
        var ngFail = ngItems.Count(i => !i.DisplayOnly && i.State != NgItemState.Succeeded);
        var ngPend = ngItems.Count(i => i.DisplayOnly && i.State != NgItemState.Succeeded);
        var ngTotal = ngRec + ngFail + ngPend;

        // Panels touched by NG = distinct PIDs across the recovered keys and what is outstanding.
        // A panel counts as recovered when nothing of its is still failing or pending.
        var ngPids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var k in ng.RecoveredKeys) { var i = k.IndexOf('|'); if (i > 0) ngPids.Add(k[..i]); }
        foreach (var it in ngItems) ngPids.Add(it.Pid);
        var ngOpenPids = new HashSet<string>(
            ngItems.Where(i => i.State != NgItemState.Succeeded).Select(i => i.Pid),
            StringComparer.OrdinalIgnoreCase);
        var ngPanelsOk = ngPids.Count(p => !ngOpenPids.Contains(p));

        NgStatPanels.Text = ngPids.Count > 0
            ? $"{ngPids.Count} ({ngPanelsOk * 100.0 / ngPids.Count:0.#}%)"
            : "0";
        NgStatFiles.Text = ngTotal.ToString();
        NgStatOk.Text = ngTotal > 0 ? $"{ngRec} ({ngRec * 100.0 / ngTotal:0.#}%)" : "0";
        NgStatPending.Text = ngTotal > 0 ? $"{ngPend} ({ngPend * 100.0 / ngTotal:0.#}%)" : "0";
        NgStatFailed.Text = ngTotal > 0 ? $"{ngFail} ({ngFail * 100.0 / ngTotal:0.#}%)" : "0";
        NgCount.Text = running && ng.QueueLength > 0 ? $"-  {ng.QueueLength} queued" : "";

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
                : "cooldown...";
        }
        else
        {
            NgRow.Visibility = Visibility.Collapsed;
            NgIdle.Visibility = Visibility.Visible;
            var remaining = ng.QueueLength;
            NgIdle.Text = running
                ? (remaining > 0
                    ? $"Recovering - {remaining} item(s) still failing, retrying between sweeps..."
                    : "Monitoring - all recovered; watching for new failures.")
                : $"Stopped - day {ng.LoadedDay}, {_ng.Sum(g => g.Items.Count)} item(s). Press Auto Retry to recover them.";
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

            // VisualTreeHelper.GetParent THROWS on a ContentElement - and a click that lands on
            // inline text reports its source as a Run, which is exactly that. Unhandled on the UI
            // thread, so it killed the whole process: clicking a row's text closed the app and the
            // pumps with it. Step out to the logical tree for those nodes instead.
            d = (d is System.Windows.Media.Visual || d is System.Windows.Media.Media3D.Visual3D)
                ? System.Windows.Media.VisualTreeHelper.GetParent(d)
                : LogicalTreeHelper.GetParent(d);
        }
        return false;
    }

    /// <summary>A file is "terminal" when nothing more will be done with it: it succeeded, it
    /// failed / timed out, or it's out of attempts. (An in-flight file is Pending with attempts
    /// left, so it is NOT terminal - its panel stays visible.)</summary>
    private bool FileTerminal(JobFile f) =>
        f.Status == FileStatus.Succeeded || f.Status == FileStatus.Failed || f.Status == FileStatus.TimedOut
        || (f.Status == FileStatus.Pending && f.Attempts >= _host.Cfg.MaxAttempts);

    /// <summary>A live panel is hidden once EVERY file is terminal (all retries consumed, whether
    /// they succeeded or not). Failures still live on in the NG list.</summary>
    private bool PanelDone(Job j) => j.Files.Count > 0 && j.Files.All(FileTerminal);

    private void RefreshList()
    {
        var inFlight = _host.Engine.InFlight;

        // Everything matching the PID filter - used for the day-total stats strip.
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

        // Reconcile with NG before counting, so the strip is a mini version of the day report.
        // The live engine never learns about NG recoveries, so a file NG rescued stayed "Failed"
        // here forever — 297 Failed in the strip against 0 in the report for the same run.
        var recovered = _host.NgRetry.RecoveredKeys;
        var ok = matched.Sum(j => j.Files.Count(f =>
            f.Status == FileStatus.Succeeded || recovered.Contains(f.Key)));
        // Failed and TimedOut counted SEPARATELY, matching the HTML report's cards. They were
        // summed into one "Failed" number here, so a run whose files were cut off by the panel
        // timeout showed them as outright failures and the strip had no equivalent of the
        // report's "TIMED OUT" card at all.
        var bad = matched.Sum(j => j.Files.Count(f =>
            f.Status == FileStatus.Failed && !recovered.Contains(f.Key)));
        var timedOut = matched.Sum(j => j.Files.Count(f =>
            f.Status == FileStatus.TimedOut && !recovered.Contains(f.Key)));
        var pending = files - ok - bad - timedOut;

        // Panels fully landed — the strip's equivalent of the report's "Panels succeeded" card.
        var panelsOk = matched.Count(j => j.Files.Count > 0 && j.Files.All(f =>
            f.Status == FileStatus.Succeeded || recovered.Contains(f.Key)));

        StatJobs.Text = matched.Count > 0
            ? $"{matched.Count} ({panelsOk * 100.0 / matched.Count:0.#}%)"
            : "0";
        StatFiles.Text = files.ToString();
        StatOk.Text = files > 0 ? $"{ok} ({ok * 100.0 / files:0.#}%)" : "0";
        StatPending.Text = files > 0 ? $"{pending} ({pending * 100.0 / files:0.#}%)" : "0";
        StatFailed.Text = files > 0 ? $"{bad} ({bad * 100.0 / files:0.#}%)" : "0";
        StatTimedOut.Text = files > 0 ? $"{timedOut} ({timedOut * 100.0 / files:0.#}%)" : "0";

        var mbps = _host.Engine.RollingMBps;
        StatSpeed.Text = mbps > 0 ? $"-  {mbps:0.0} MB/s avg" : "";

        SyncNg();

        // Tab headers show the ACTIVE/current count, so they rise as work arrives and fall as it
        // clears: live = panels still in the pump (not yet all-terminal); NG = items still needing
        // attention (Waiting / Uploading / Failed), i.e. recovered/gone ones drop off.
        AllTab.Header = $"Today Jobs ({visible.Count})";
        var ngActive = _host.NgRetry.Items.Count(i =>
            i.State is NgItemState.Waiting or NgItemState.Uploading or NgItemState.Failed);
        // Show out-of-window work alongside the loaded count. Without it a bare "NG List (0)" reads
        // as "nothing outstanding" when it only means "nothing outstanding in the days I loaded" -
        // days that have aged out are neither retried nor counted.
        var older = _host.NgRetry.BacklogOutstanding;
        NgTab.Header = older > 0 ? $"NG List ({ngActive})  -  {older:N0} older" : $"NG List ({ngActive})";
        NgEmpty.Visibility = _ng.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        NgEmpty.Text = older > 0
            ? $"No NG items in the loaded day(s). {older:N0} file(s) on {_host.NgRetry.BacklogDays} older day(s) are outside the {_host.Cfg.NgRecoveryDays}-day recovery window - pick that day above to retry them."
            : "No NG items for this day.";
    }

    /// <summary>
    /// Reconciles the NG tab with the NG-retry engine's item list. Rows are keyed on the item's
    /// stable identity string (Day|PID|File), so a Retry just refreshes the row in place and a
    /// day reload REBINDS existing rows rather than clearing and rebuilding - the list never
    /// blanks out. Items that succeed stay visible (green) until the day is reloaded.
    /// </summary>
    private void SyncNg()
    {
        // The NG filter is view-only - the pump still works the full loaded list; this just
        // controls which rows are shown, using the NG tab's own PID box (separate from Today Jobs).
        var items = _host.NgRetry.Items
            .Where(it => _ngFilter.Length == 0 ||
                         it.Pid.Contains(_ngFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Group by day+PID, preserving first-seen order so cards don't jump around. The list can
        // span several days (today + the recovery window), and the same panel can legitimately
        // appear on two of them - grouping on PID alone would merge those into one card.
        var order = new List<string>();
        var byPid = new Dictionary<string, List<NgItem>>();
        foreach (var it in items)
        {
            var key = it.Day + "|" + it.Pid;
            if (!byPid.TryGetValue(key, out var list))
            {
                list = new List<NgItem>();
                byPid[key] = list;
                order.Add(key);
            }
            list.Add(it);
        }

        // Hide fully-recovered panels: keep a card only while at least one item still needs
        // attention (Waiting / Uploading / Failed). All Succeeded (or Gone) - drop the card.
        static bool NeedsAttention(NgItem it) =>
            it.State is NgItemState.Waiting or NgItemState.Uploading or NgItemState.Failed;
        order = order.Where(k => byPid[k].Any(NeedsAttention)).ToList();

        // Reconcile group cards by day+PID - existing cards are refreshed in place (keeping their
        // expand/collapse state and row objects), new ones added, vanished ones removed.
        var existing = new Dictionary<string, NgGroupVm>();
        foreach (var g in _ng) existing[g.GroupKey] = g;

        // Within a panel, show rows in the same order as the main rawlog / jobs file: data files
        // first (original order), then the index manifest, then the host manifest (host last).
        static int NgRank(NgItem it) => !it.IsManifest ? 0 : (it.RemotePath == it.UploadIndexPath ? 1 : 2);

        foreach (var key in order)
        {
            if (!existing.TryGetValue(key, out var group))
            {
                var first = byPid[key][0];
                group = new NgGroupVm(first.Day, first.Pid);
                _ng.Add(group);
                existing[key] = group;
            }
            var rows = byPid[key]
                .Select((it, i) => (it, i))
                .OrderBy(x => NgRank(x.it))
                .ThenBy(x => x.i)
                .Select(x => x.it)
                .ToList();
            group.Refresh(rows);
        }

        var wantedPids = new HashSet<string>(order);
        for (var i = _ng.Count - 1; i >= 0; i--)
            if (!wantedPids.Contains(_ng[i].GroupKey))
                _ng.RemoveAt(i);
    }

    private string _lastLogText = "";

    private void RefreshLog()
    {
        // newest first, capped - the full history lives in the raw log on disk.
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
        ok &= Mark(SetRetryCount, IsNonNegInt(SetRetryCount.Text));
        ok &= Mark(SetPanelTimeout, IsNonNegInt(SetPanelTimeout.Text));
        ok &= Mark(SetPollInterval, IsNonNegInt(SetPollInterval.Text) && ParseInt(SetPollInterval.Text, 0) > 0);
        ok &= Mark(SetLogRetention, IsNonNegInt(SetLogRetention.Text));
        ok &= Mark(SetNgRecoveryDays, IsNonNegInt(SetNgRecoveryDays.Text));
        ok &= Mark(SetHtmlRefresh, IsNonNegInt(SetHtmlRefresh.Text));
        // Max files/session is a combo (Unlimited/100/300/500) - always valid, nothing to check.
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
            ["Retry count"] = c.RetryCount.ToString(),
            ["Upload to IP"] = c.InitialHost,
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

    // The Settings route preview was removed: with a single destination host and one retry count,
    // the two fields say everything the panel used to explain. These handlers stay as no-ops so the
    // XAML bindings on those fields remain valid.
    private void RoutePreview_Changed(object sender, TextChangedEventArgs e) { }

    private void RoutePreview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

    private void LoadSettings()
    {
        var c = _host.Cfg;
        SetPrimaryHost.Text = c.PrimaryHost;
        SetSecondaryHost.Text = c.SecondaryHost;
        SetPort.Text = c.Port.ToString();
        SetUser.Text = c.User;
        SetPassword.Text = c.Password;
        SelectCombo(SetFtpSecure, c.FtpSecure);
        SelectCombo(SetEngine, string.Equals(c.Engine, "WinSCP", StringComparison.OrdinalIgnoreCase) ? "WinSCP" : "FluentFTP");
        SelectCombo(SetFtpMode, string.Equals(c.FtpMode, "Active", StringComparison.OrdinalIgnoreCase) ? "Active" : "Passive");
        SelectCombo(SetInitialHost, string.Equals(c.InitialHost, "Secondary", StringComparison.OrdinalIgnoreCase) ? "Secondary" : "Primary");
        // Engine log always on; .part rename and preserve-timestamp always off. No longer in the UI.

        SetQueueFolder.Text = c.QueueFolder;
        SetRecipePath.Text = c.RecipePath;
        SetBackupFolder.Text = c.PanelBackupFolder;
        SetJobsFolder.Text = c.JobsFolder;
        SetLogFolder.Text = c.LogFolder;
        SetStateFolder.Text = c.StateFolder;

        SetTimeout.Text = c.TimeoutSecondsOverride.ToString();
        SetRetryCount.Text = c.RetryCount.ToString();
        SetPanelTimeout.Text = c.PanelTimeoutSeconds.ToString();
        SetPollInterval.Text = c.PollIntervalMs.ToString();
        SetLogRetention.Text = c.LogRetentionDays.ToString();
        SetNgRecoveryDays.Text = c.NgRecoveryDays.ToString();
        SetHtmlRefresh.Text = c.HtmlLogRefreshSeconds.ToString();
        SetMidFailHost.IsChecked = c.MidFailHostUpload;
        SetStampManifestName.IsChecked = c.StampManifestNameAtUpload;
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

    /// <summary>
    /// The settings that affect behaviour, as name -> value. Taken BEFORE the form is read into the
    /// live config and again after, so the oplog can name what actually changed.
    ///
    /// A snapshot is necessary rather than comparing two Config objects: ApplySettings mutates the
    /// live config in place, so "old" and "new" would be the same instance.
    /// </summary>
    private static Dictionary<string, string> SettingsSnapshot(Config c)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        void S(string name, string? v) => d[name] = string.IsNullOrEmpty(v) ? "(empty)" : v!;

        S("PrimaryHost", c.PrimaryHost);
        S("SecondaryHost", c.SecondaryHost);
        S("InitialHost", c.InitialHost.ToString());
        S("User", c.User);
        // Never the password itself — only whether it is set, so a change is visible without leaking it.
        S("Password", string.IsNullOrEmpty(c.Password) ? "(unset)" : "(set:" + c.Password.Length + ")");
        S("RetryCount", c.RetryCount.ToString());
        S("TimeoutSecondsOverride", c.TimeoutSecondsOverride.ToString());
        S("PanelTimeoutSeconds", c.PanelTimeoutSeconds.ToString());
        S("PollIntervalMs", c.PollIntervalMs.ToString());
        S("LogRetentionDays", c.LogRetentionDays.ToString());
        S("NgRecoveryDays", c.NgRecoveryDays.ToString());
        S("HtmlLogRefreshSeconds", c.HtmlLogRefreshSeconds.ToString());
        S("MidFailHostUpload", c.MidFailHostUpload.ToString());
        S("StampManifestNameAtUpload", c.StampManifestNameAtUpload.ToString());
        S("MaxFilesPerSession", c.MaxFilesPerSession.ToString());
        S("AutoStartUploading", c.AutoStartUploading.ToString());
        S("AutoStartRetrying", c.AutoStartRetrying.ToString());
        S("QueueFolder", c.QueueFolder);
        S("JobsFolder", c.JobsFolder);
        S("LogFolder", c.LogFolder);
        S("StateFolder", c.StateFolder);
        S("RecipePath", c.RecipePath);
        return d;
    }

    /// <summary>"key: old -> new" for each setting that differs. Unchanged values are omitted.</summary>
    private static List<string> DiffSettings(Dictionary<string, string> before, Dictionary<string, string> after)
    {
        var list = new List<string>();
        foreach (var kv in after)
            if (!before.TryGetValue(kv.Key, out var old) || !string.Equals(old, kv.Value, StringComparison.Ordinal))
                list.Add($"{kv.Key}: {(old ?? "(new)")} -> {kv.Value}");
        return list;
    }

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
            var beforeSave = SettingsSnapshot(c);   // before the form overwrites it
            c.PrimaryHost = SetPrimaryHost.Text.Trim();
            c.SecondaryHost = SetSecondaryHost.Text.Trim();
            c.Port = ParseInt(SetPort.Text, c.Port);
            c.User = SetUser.Text.Trim();
            c.Password = SetPassword.Text;
            c.FtpSecure = (SetFtpSecure.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? c.FtpSecure;
            c.Engine = (SetEngine.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? c.Engine;
            c.FtpMode = (SetFtpMode.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? c.FtpMode;
            c.InitialHost = (SetInitialHost.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? c.InitialHost;

            c.QueueFolder = SetQueueFolder.Text.Trim();
            c.RecipePath = SetRecipePath.Text.Trim();
            c.PanelBackupFolder = SetBackupFolder.Text.Trim();
            c.JobsFolder = SetJobsFolder.Text.Trim();
            c.LogFolder = SetLogFolder.Text.Trim();
            c.StateFolder = SetStateFolder.Text.Trim();

            c.TimeoutSecondsOverride = ParseInt(SetTimeout.Text, c.TimeoutSecondsOverride);
            c.RetryCount = ParseInt(SetRetryCount.Text, c.RetryCount);
            c.PanelTimeoutSeconds = ParseInt(SetPanelTimeout.Text, c.PanelTimeoutSeconds);
            c.PollIntervalMs = ParseInt(SetPollInterval.Text, c.PollIntervalMs);
            c.LogRetentionDays = ParseInt(SetLogRetention.Text, c.LogRetentionDays);
            var ngDaysChanged = ParseInt(SetNgRecoveryDays.Text, c.NgRecoveryDays) != c.NgRecoveryDays;
            c.NgRecoveryDays = ParseInt(SetNgRecoveryDays.Text, c.NgRecoveryDays);
            c.HtmlLogRefreshSeconds = ParseInt(SetHtmlRefresh.Text, c.HtmlLogRefreshSeconds);
            c.MidFailHostUpload = SetMidFailHost.IsChecked == true;
        c.StampManifestNameAtUpload = SetStampManifestName.IsChecked == true;
            // Combo: "Unlimited" -> 0, otherwise the numeric preset.
            var sessSel = (SetMaxFilesPerSession.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Unlimited";
            c.MaxFilesPerSession = sessSel.Equals("Unlimited", StringComparison.OrdinalIgnoreCase) ? 0 : ParseInt(sessSel, 0);

            c.AutoStartUploading = SetAutoUpload.IsChecked == true;
            c.AutoStartRetrying = SetAutoRetry.IsChecked == true;

            // What actually changed, captured BEFORE the save so the oplog can name it.
            //
            // "Who changed the destination IP, and when?" is the question that started this whole
            // investigation, and until now the oplog could not answer it: it recorded startups,
            // shutdowns, rollovers and crashes, but not a settings change. A restart line saying
            // "settings saved" is no use either — the values are the point.
            var changes = DiffSettings(beforeSave, SettingsSnapshot(c));

            c.Save(_host.ConfigPath);

            if (changes.Count > 0)
                _host.LogEvent("SETTINGS SAVED - " + string.Join("; ", changes));
            else
                _host.LogEvent("SETTINGS SAVED - no values changed");

            // Create any missing folders (resolved against the exe for relative paths).
            try { c.EnsureFolders(); }
            catch (Exception ex) { SettingsHint.Text = "Saved; some folders could not be created: " + ex.Message; }

            // NG past-days applies live: rebuild the recovery window now rather than waiting for the
            // next midnight or a restart. Only when it actually changed and the console is showing
            // the window - if the operator has picked a single day to review, leave them there.
            if (ngDaysChanged && _host.NgRetry.WindowMode)
            {
                var wasRunning = _host.NgRetry.AutoRunning;
                _host.NgRetry.LoadWindow();               // LoadWindow stops auto-retry...
                if (wasRunning) _host.NgRetry.StartAutoRetry();   // ...so put it back if it was on
                _dirty = true;
            }

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

    // Single-instance non-modal calendar popups (day + NG). Re-shown/refreshed instead of duplicated.
    private LogCalendarWindow? _dayLogCal;
    private LogCalendarWindow? _ngLogCal;
    private LogCalendarWindow? _summaryCal;
    private LogCalendarWindow? _appLogCal;




    private TracePanelWindow? _traceWin;

    /// <summary>
    /// Open the Trace Panel window, pre-filled from whichever PID filter box has text so you do not
    /// retype a panel you are already looking at.
    /// </summary>
    private void TracePanel_Click(object sender, RoutedEventArgs e)
    {
        if (_traceWin != null) { _traceWin.Activate(); return; }
        var seed = FilterBox.Text.Trim();
        if (seed.Length == 0) seed = NgFilterBox.Text.Trim();
        _traceWin = new TracePanelWindow(_host.Cfg, seed) { Owner = this };
        _traceWin.Closed += (_, _) => _traceWin = null;
        _traceWin.Show();
    }

    private LogCalendarWindow? _activityCal;



    private LogViewerWindow? _logViewer;

    /// <summary>
    /// Open the one log window: a tab per log, a day list in each.
    ///
    /// Replaces five buttons. Single instance, so pressing it again brings the existing window
    /// forward instead of stacking copies.
    /// </summary>
    private void ViewLog_Click(object sender, RoutedEventArgs e)
    {
        if (_logViewer != null) { _logViewer.Activate(); return; }
        _logViewer = new LogViewerWindow(_host.Cfg) { Owner = this };
        _logViewer.Closed += (_, _) => _logViewer = null;
        _logViewer.Show();
    }

    private static DateTime ParseDayOrToday(string day)
        => DateTime.TryParseExact(day, "yyyyMMdd", null,
               System.Globalization.DateTimeStyles.None, out var d) ? d : DateTime.Today;






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
            Scan(_host.Cfg.LogFullPath, "_totallog.txt");
            Scan(_host.Cfg.LogFullPath, "_rawlog.txt");        // days from before the rename
            Scan(_host.Cfg.JobsFullPath, "_jobs.txt");
        }
        else
        {
            Scan(_host.Cfg.LogFullPath, "_ngretrytotallog.txt");
            Scan(_host.Cfg.LogFullPath, "_ngretrylog.txt");     // ditto
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
            LogSnapshot.OpenCopy(path);   // a copy: an open viewer would deny the writer
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
        if (string.IsNullOrWhiteSpace(host)) { dot.Fill = PingIdle; text.Text = "-"; return; }
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
        if (day == _host.NgRetry.LoadedDay) return;   // already showing this day - don't reload
                                                       // (that would reset the running auto-retry)
        _host.NgRetry.LoadDay(day);
        _dirty = true;
    }

    private void NgIp_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // NG no longer has its own IP selector; the host comes from Settings. Kept as a no-op so
        // any stale XAML binding cannot crash the tab.
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
        _filter = FilterBox.Text.Trim();   // masthead box - Today Jobs list only
        // Hidden, not Collapsed: Hidden keeps the button's layout slot so the box never changes width.
        if (FilterClear is not null)
            FilterClear.Visibility = FilterBox.Text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
        _dirty = true;
    }

    private void FilterClear_Click(object sender, RoutedEventArgs e)
    {
        FilterBox.Text = "";       // TextChanged does the rest (filter + hide the button)
        FilterBox.Focus();
    }

    private void NgFilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _ngFilter = NgFilterBox.Text.Trim();   // NG tab's own box - NG list only
        if (NgFilterClear is not null)
            NgFilterClear.Visibility = NgFilterBox.Text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
        _dirty = true;
    }

    private void NgFilterClear_Click(object sender, RoutedEventArgs e)
    {
        NgFilterBox.Text = "";
        NgFilterBox.Focus();
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
        // Tell the operator once that closing did not stop the uploads - otherwise the
        // window just vanishes and it looks like the program quit.
        (System.Windows.Application.Current as App)?.NotifyHiddenOnce();
    }

    /// <summary>Closing only hides the window - the upload engine keeps running.</summary>
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
