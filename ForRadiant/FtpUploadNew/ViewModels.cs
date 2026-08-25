using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
// WinForms is referenced for the tray icon, so System.Drawing types collide with WPF's.
// These aliases pin every drawing type in this file to the WPF (System.Windows.Media) one.
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace FtpUpload;

public abstract class Notify : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        Raise(name);
        return true;
    }
}

/// <summary>Palette shared with the HTML mockup so the look carries over.</summary>
public static class Palette
{
    public static readonly Brush OkBg = New("#E4F7EA");
    public static readonly Brush OkFg = New("#1F9D55");
    public static readonly Brush BadBg = New("#FDECEB");
    public static readonly Brush BadFg = New("#E0483F");
    public static readonly Brush PendBg = New("#EEF0F4");
    public static readonly Brush PendFg = New("#8891A3");
    public static readonly Brush WarnBg = New("#FFF4E0");
    public static readonly Brush WarnFg = New("#B8860B");
    public static readonly Brush TimeoutBg = New("#F3E8FF");   // "Timed Out" (distinct from red Failed)
    public static readonly Brush TimeoutFg = New("#7C3AED");
    public static readonly Brush Muted = New("#C3C8D2");
    public static readonly Brush Normal = New("#4A5268");
    public static readonly Brush RowHi = New("#E8F1FF");   // current-upload row highlight
    public static readonly Brush Clear = New("#00000000");  // transparent (frozen)

    private static Brush New(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        b.Freeze();     // frozen brushes are cheaper and thread-safe
        return b;
    }
}

/// <summary>One file row in a job card.</summary>
public sealed class FileRowVm : Notify
{
    public JobFile Model { get; }
    private readonly int _maxAttempts;

    public FileRowVm(JobFile model, int maxAttempts)
    {
        Model = model;
        _maxAttempts = maxAttempts;
    }

    public string Pid => Model.Pid;
    public string FileName => Model.FileName;
    public string RemotePath => Model.RemotePath;
    /// <summary>Full destination shown in the UI: ftp://{host}/{path}.</summary>
    public string RemotePathFull => UiConfig.WithHost(Model.RemotePath);

    /// <summary>
    /// A row belongs on the "NG List" tab once it is terminal-but-not-uploaded: it FAILED
    /// (used up every attempt) or was TIMEDOUT (skipped by the panel timeout). Held for MANUAL
    /// retry only; the engine never re-runs them on its own.
    /// </summary>
    public bool IsNg => Model.Status == FileStatus.Failed || Model.Status == FileStatus.TimedOut;

    /// <summary>The NG-list Retry button is live only while the file is failed or timed out.</summary>
    public bool CanRetry => (Model.Status == FileStatus.Failed || Model.Status == FileStatus.TimedOut)
                            && !IsUploading;

    public bool IsUploading { get; private set; }

    /// <summary>Light fill + an outlined box around the row currently being uploaded.</summary>
    public Brush RowBg => IsUploading ? Palette.RowHi : Palette.Clear;
    public System.Windows.Thickness RowBox => IsUploading
        ? new System.Windows.Thickness(2)
        : new System.Windows.Thickness(0);

    public string StatusText => IsUploading ? "Uploading…" : Model.Status switch
    {
        FileStatus.Succeeded => "Succeeded",
        FileStatus.Failed => "Failed",
        FileStatus.TimedOut => "Timed Out",
        _ => "Pending"
    };

    public Brush StatusBg => IsUploading ? Palette.WarnBg : Model.Status switch
    {
        FileStatus.Succeeded => Palette.OkBg,
        FileStatus.Failed => Palette.BadBg,
        FileStatus.TimedOut => Palette.TimeoutBg,
        _ => Palette.PendBg
    };

    public Brush StatusFg => IsUploading ? Palette.WarnFg : Model.Status switch
    {
        FileStatus.Succeeded => Palette.OkFg,
        FileStatus.Failed => Palette.BadFg,
        FileStatus.TimedOut => Palette.TimeoutFg,
        _ => Palette.PendFg
    };

    public string SucceedTime => string.IsNullOrEmpty(Model.SucceedTime) ? "—" : Model.SucceedTime;
    public Brush SucceedBrush => string.IsNullOrEmpty(Model.SucceedTime) ? Palette.Muted : Palette.Normal;

    public string FailTimes => Model.FailTimes.Count == 0 ? "—" : string.Join("\n", Model.FailTimes);
    public Brush FailBrush => Model.FailTimes.Count == 0 ? Palette.Muted : Palette.Normal;

    // "Retries" means retries CONSUMED, not attempts made. The first try is not a retry, so a
    // file that succeeds immediately shows 0 / 4 — with MaxAttempts = 5 that is the initial
    // attempt plus 2 retries on the primary IP and 2 more on the secondary.
    private int RetriesUsed => Math.Max(0, Model.Attempts - 1);
    private int MaxRetries => Math.Max(0, _maxAttempts - 1);

    public string RetryText => $"{RetriesUsed} / {MaxRetries}";
    public Brush RetryBg => RetriesUsed >= MaxRetries && RetriesUsed > 0 ? Palette.BadBg
        : RetriesUsed > 0 ? Palette.WarnBg : Palette.PendBg;
    public Brush RetryFg => RetriesUsed >= MaxRetries && RetriesUsed > 0 ? Palette.BadFg
        : RetriesUsed > 0 ? Palette.WarnFg : Palette.PendFg;

    public bool CanForce => Model.Status == FileStatus.Pending && !IsUploading;

    // Status is checked before the attempt count: a file that succeeded on its very last
    // attempt has Attempts == MaxAttempts and must not read "Max retries" as if it had run out.
    public string ForceText => IsUploading ? "Uploading…"
        : Model.Status == FileStatus.Succeeded ? "Uploaded"
        : Model.Status == FileStatus.TimedOut ? "Timed out"
        : Model.Status == FileStatus.Failed ? "Max retries"
        : Model.Attempts >= _maxAttempts ? "Max retries"
        : "Force Upload";

    // Cheap change-detection: a refresh runs several times a second across every row, and
    // raising PropertyChanged unconditionally makes WPF re-render the whole list each time.
    // Only tell the UI about values that genuinely moved.
    private FileStatus _lastStatus = (FileStatus)(-1);
    private bool _lastUploading;
    private string _lastSucceed = "";
    private int _lastFailCount = -1;
    private int _lastAttempts = -1;

    public void Refresh(JobFile? inFlight)
    {
        var uploading = ReferenceEquals(inFlight, Model);

        var statusMoved = uploading != _lastUploading || Model.Status != _lastStatus;
        var succeedMoved = Model.SucceedTime != _lastSucceed;
        var failMoved = Model.FailTimes.Count != _lastFailCount;
        var attemptsMoved = Model.Attempts != _lastAttempts;

        if (!statusMoved && !succeedMoved && !failMoved && !attemptsMoved) return;

        IsUploading = uploading;
        _lastUploading = uploading;
        _lastStatus = Model.Status;
        _lastSucceed = Model.SucceedTime;
        _lastFailCount = Model.FailTimes.Count;
        _lastAttempts = Model.Attempts;

        if (statusMoved)
        {
            Raise(nameof(StatusText)); Raise(nameof(StatusBg)); Raise(nameof(StatusFg));
            Raise(nameof(IsNg)); Raise(nameof(CanRetry)); Raise(nameof(RowBg)); Raise(nameof(RowBox));
        }
        if (succeedMoved)
        {
            Raise(nameof(SucceedTime)); Raise(nameof(SucceedBrush));
        }
        if (failMoved)
        {
            Raise(nameof(FailTimes)); Raise(nameof(FailBrush));
        }
        if (attemptsMoved)
        {
            Raise(nameof(RetryText)); Raise(nameof(RetryBg)); Raise(nameof(RetryFg));
        }
        if (statusMoved || attemptsMoved)
        {
            Raise(nameof(CanForce)); Raise(nameof(ForceText));
        }
    }
}

/// <summary>One panel (PID) card.</summary>
public sealed class JobVm : Notify
{
    public Job Model { get; }
    private readonly int _maxAttempts;

    public JobVm(Job model, int maxAttempts)
    {
        Model = model;
        _maxAttempts = maxAttempts;
        Files = new ObservableCollection<FileRowVm>(model.Files.Select(f => new FileRowVm(f, maxAttempts)));
    }

    public string Pid => Model.Pid;
    public ObservableCollection<FileRowVm> Files { get; }

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { if (Set(ref _isExpanded, value)) Raise(nameof(ExpandGlyph)); }
    }

    /// <summary>Disclosure triangle in the header: down when open, right when collapsed.</summary>
    public string ExpandGlyph => _isExpanded ? "\uE96E" : "\uE970";  // Segoe MDL2 ChevronDown / ChevronRight

    public string Tally =>
        $"{Model.Files.Count(f => f.Status == FileStatus.Succeeded)}/{Model.Files.Count} succeeded  ·  " +
        $"{Model.Files.Count(f => f.Status == FileStatus.Failed || f.Status == FileStatus.TimedOut)} failed";

    public string OverallText => Model.TimedOut ? "Timed Out"
        : Model.Files.Count == 0 ? "Empty"
        : Model.AnyPending ? "In Progress"
        : Model.AllSucceeded ? "Success" : "Failed";

    public Brush OverallBg => Model.TimedOut ? Palette.TimeoutBg
        : Model.Files.Count == 0 ? Palette.PendBg
        : Model.AnyPending ? Palette.PendBg
        : Model.AllSucceeded ? Palette.OkBg : Palette.BadBg;

    public Brush OverallFg => Model.TimedOut ? Palette.TimeoutFg
        : Model.Files.Count == 0 ? Palette.PendFg
        : Model.AnyPending ? Palette.PendFg
        : Model.AllSucceeded ? Palette.OkFg : Palette.BadFg;

    private string _lastTally = "";
    private string _lastOverall = "";

    public void Refresh(JobFile? inFlight)
    {
        // add rows for files that appeared since the last refresh
        foreach (var f in Model.Files)
            if (Files.All(v => v.FileName != f.FileName))
                Files.Add(new FileRowVm(f, _maxAttempts));

        // drop rows for files that were deleted
        foreach (var stale in Files.Where(v => Model.Files.All(f => f.FileName != v.FileName)).ToList())
            Files.Remove(stale);

        foreach (var v in Files) v.Refresh(inFlight);

        var tally = Tally;
        if (tally != _lastTally) { _lastTally = tally; Raise(nameof(Tally)); }

        var overall = OverallText;
        if (overall != _lastOverall)
        {
            _lastOverall = overall;
            Raise(nameof(OverallText)); Raise(nameof(OverallBg)); Raise(nameof(OverallFg));
        }
    }
}

/// <summary>Row in the NG-retry console. Wraps an NgItem (a value record from files), refreshed
/// in place as the NG-retry pump updates its state/retry count.</summary>
public sealed class NgItemVm : Notify
{
    public NgItem Model { get; private set; }
    public NgItemVm(NgItem m) { Model = m; }

    /// <summary>Point this row at a fresh NgItem with the same identity (used when a day reloads),
    /// so the row object stays put and the list doesn't flicker/empty.</summary>
    public void Rebind(NgItem m) { Model = m; Refresh(); }

    public string Day => Model.Day;
    public string Pid => Model.Pid;
    public string FileName => Model.FileName;
    public string RemotePath => Model.RemotePath;
    /// <summary>Full destination shown in the UI: ftp://{host}/{path}.</summary>
    public string RemotePathFull => UiConfig.WithHost(Model.RemotePath);

    public bool IsUploading => Model.State == NgItemState.Uploading;
    // The blue box tracks the item the pump is actively working — set through its whole
    // attempt+cooldown cycle — so it stays visible instead of flashing on instant retries.
    public bool IsCurrent => Model.IsCurrent;
    public Brush RowBg => IsCurrent ? Palette.RowHi : Palette.Clear;
    public System.Windows.Thickness RowBox => IsCurrent
        ? new System.Windows.Thickness(2)
        : new System.Windows.Thickness(0);

    public string OrigText => Model.OrigStatus switch
    {
        "TIMEDOUT" => "Timed Out",
        "PENDING" => "Pending",
        _ => "Failed"
    };
    public Brush OrigBg => Model.OrigStatus switch
    {
        "TIMEDOUT" => Palette.TimeoutBg,
        "PENDING" => Palette.PendBg,
        _ => Palette.BadBg
    };
    public Brush OrigFg => Model.OrigStatus switch
    {
        "TIMEDOUT" => Palette.TimeoutFg,
        "PENDING" => Palette.PendFg,
        _ => Palette.BadFg
    };

    public string StateText => Model.State switch
    {
        NgItemState.Uploading => "Uploading…",
        NgItemState.Succeeded => "Succeeded",
        NgItemState.Failed => "Failed",
        NgItemState.Gone => "File gone",
        _ => "Waiting"
    };
    public Brush StateBg => Model.State switch
    {
        NgItemState.Uploading => Palette.WarnBg,
        NgItemState.Succeeded => Palette.OkBg,
        NgItemState.Failed => Palette.BadBg,
        NgItemState.Gone => Palette.PendBg,
        _ => Palette.PendBg
    };
    public Brush StateFg => Model.State switch
    {
        NgItemState.Uploading => Palette.WarnFg,
        NgItemState.Succeeded => Palette.OkFg,
        NgItemState.Failed => Palette.BadFg,
        NgItemState.Gone => Palette.PendFg,
        _ => Palette.PendFg
    };

    public string RetriesText => Model.TotalRetries.ToString();
    public string LastHost => Model.LastHost;

    public bool CanRetry => !Model.DisplayOnly
        && Model.State != NgItemState.Uploading && Model.State != NgItemState.Succeeded;

    public void Refresh()
    {
        Raise(nameof(StateText)); Raise(nameof(StateBg)); Raise(nameof(StateFg));
        Raise(nameof(RetriesText)); Raise(nameof(LastHost)); Raise(nameof(CanRetry));
        Raise(nameof(IsUploading)); Raise(nameof(IsCurrent)); Raise(nameof(RowBg)); Raise(nameof(RowBox));
        Raise(nameof(RemotePath)); Raise(nameof(RemotePathFull)); Raise(nameof(OrigText)); Raise(nameof(OrigBg)); Raise(nameof(OrigFg));
    }
}


/// <summary>
/// One panel (PID) card on the NG-retry console — the NG-tab equivalent of <see cref="JobVm"/>.
/// Groups that panel's NG rows (<see cref="NgItemVm"/>) under a collapsible header showing
/// "X failed / Y recovered". Rows are reconciled in place (by NgItem.Key) so retries and day
/// reloads never blank the card.
/// </summary>
public sealed class NgGroupVm : Notify
{
    public string Pid { get; }
    public ObservableCollection<NgItemVm> Items { get; } = new();

    public NgGroupVm(string pid) { Pid = pid; }

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { if (Set(ref _isExpanded, value)) Raise(nameof(ExpandGlyph)); }
    }

    /// <summary>Disclosure triangle shown in the header: down when open, right when collapsed.</summary>
    public string ExpandGlyph => _isExpanded ? "\uE96E" : "\uE970";  // Segoe MDL2 ChevronDown / ChevronRight

    private int Recovered => Items.Count(i => i.Model.State == NgItemState.Succeeded);
    private int Failed => Items.Count - Recovered;

    public string Tally => $"{Failed} failed  ·  {Recovered} recovered";

    /// <summary>A status badge mirroring the live job card, so a fully-recovered panel stands out:
    /// "Recovered" (green) when every item succeeded, "Retrying" (amber) while any is in flight,
    /// otherwise "Failing" (red).</summary>
    public string HeaderText =>
        Items.Count == 0 ? "Empty"
        : Failed == 0 ? "Recovered"
        : Items.Any(i => i.Model.State == NgItemState.Uploading) ? "Retrying"
        : "Failing";

    public Brush HeaderBg =>
        Items.Count == 0 ? Palette.PendBg
        : Failed == 0 ? Palette.OkBg
        : Items.Any(i => i.Model.State == NgItemState.Uploading) ? Palette.WarnBg
        : Palette.BadBg;

    public Brush HeaderFg =>
        Items.Count == 0 ? Palette.PendFg
        : Failed == 0 ? Palette.OkFg
        : Items.Any(i => i.Model.State == NgItemState.Uploading) ? Palette.WarnFg
        : Palette.BadFg;

    private string _lastTally = "";
    private string _lastHeader = "";

    /// <summary>Reconcile this card's rows against the NG items for this PID (already filtered).</summary>
    public void Refresh(List<NgItem> itemsForPid)
    {
        var byKey = new Dictionary<string, NgItemVm>();
        foreach (var vm in Items) byKey[vm.Model.Key] = vm;

        var wanted = new HashSet<string>();
        foreach (var it in itemsForPid)
        {
            wanted.Add(it.Key);
            if (byKey.TryGetValue(it.Key, out var vm))
            {
                if (!ReferenceEquals(vm.Model, it)) vm.Rebind(it); else vm.Refresh();
            }
            else
            {
                Items.Add(new NgItemVm(it));
            }
        }
        for (var i = Items.Count - 1; i >= 0; i--)
            if (!wanted.Contains(Items[i].Model.Key))
                Items.RemoveAt(i);

        var tally = Tally;
        if (tally != _lastTally) { _lastTally = tally; Raise(nameof(Tally)); }

        var header = HeaderText;
        if (header != _lastHeader)
        {
            _lastHeader = header;
            Raise(nameof(HeaderText)); Raise(nameof(HeaderBg)); Raise(nameof(HeaderFg));
        }
    }
}


/// <summary>
/// App-wide display settings for the view-models. Set once at startup so every row can render a
/// full "ftp://{host}/{path}" destination without threading the host through each constructor.
/// </summary>
public static class UiConfig
{
    public static string FtpHost = "";

    /// <summary>Prefix a stored remote path with "ftp://{host}/" for display. Falls back to the
    /// bare path if no host is set yet.</summary>
    public static string WithHost(string remotePath) =>
        FtpHost.Length == 0 ? remotePath : $"ftp://{FtpHost}/{remotePath.TrimStart('/')}";
}
