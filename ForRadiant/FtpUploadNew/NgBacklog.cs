using System.Text.Json;

namespace FtpUpload;

/// <summary>
/// Counts NG work sitting on days OUTSIDE the auto-recovery window.
///
/// Why this exists: the NG console only loads today plus <see cref="Config.NgRecoveryDays"/> past
/// days, and its "NG List (n)" count reflects only what is loaded. Once a day ages out of that
/// window its unrecovered files stop being retried AND stop being counted, so the tab reads 0 while
/// thousands of files are still outstanding — the same silent-nothing-is-happening shape as the
/// rollover stall this whole effort started from. This supplies the "· n older" figure so a zero on
/// screen means "nothing outstanding", not "nothing outstanding that I happened to load".
///
/// Cost control: a day outside the window is FROZEN — the live pump only writes today, and the NG
/// pump only works days inside the window — so its count can be computed once and cached forever.
/// The cache is fingerprinted on each log's length + last-write time, so a day that does somehow
/// change (an operator manually retrying an old day) is recomputed rather than served stale.
/// </summary>
internal sealed class NgBacklog
{
    private sealed class Entry
    {
        public long RawLen { get; set; }
        public long RawTicks { get; set; }
        public long NgLen { get; set; }
        public long NgTicks { get; set; }
        public int Outstanding { get; set; }
    }

    private readonly Config cfg;
    private Dictionary<string, Entry> _cache = new();

    public NgBacklog(Config cfg) => this.cfg = cfg;

    /// <summary>Files outstanding on days outside the recovery window, and how many days hold them.</summary>
    public int Outstanding { get; private set; }
    public int Days { get; private set; }

    private string CachePath => Path.Combine(cfg.StateFullPath, "ng_backlog.json");

    /// <summary>
    /// Recount days outside <paramref name="windowDays"/>. Runs off the UI thread and off the watch
    /// loop — it reads every retained day's logs on a cold cache, which must never block a pump.
    /// </summary>
    public void Refresh(IEnumerable<string> windowDays)
    {
        var inWindow = new HashSet<string>(windowDays);
        LoadCache();

        var total = 0;
        var dayCount = 0;
        var seen = new HashSet<string>();
        var dirty = false;

        var logDir = cfg.LogFullPath;
        if (!Directory.Exists(logDir)) { Outstanding = 0; Days = 0; return; }

        // Newest first, so a cap (NgBacklogScanDays) drops the oldest rather than the most relevant.
        var files = Directory.GetFiles(logDir, "*_rawlog.txt")
                             .OrderByDescending(f => f)
                             .ToList();
        var cap = cfg.NgBacklogScanDays;
        var scanned = 0;

        foreach (var rawPath in files)
        {
            var day = Path.GetFileName(rawPath);
            if (day.Length < 8) continue;
            day = day.Substring(0, 8);
            if (inWindow.Contains(day)) continue;          // live; already counted by the console
            if (cap > 0 && scanned >= cap) break;
            scanned++;
            seen.Add(day);

            var rawInfo = new FileInfo(rawPath);
            var ngPath = cfg.NgRetryLogPath(day);
            var ngInfo = File.Exists(ngPath) ? new FileInfo(ngPath) : null;

            var fresh = _cache.TryGetValue(day, out var e)
                        && e.RawLen == rawInfo.Length
                        && e.RawTicks == rawInfo.LastWriteTimeUtc.Ticks
                        && e.NgLen == (ngInfo?.Length ?? 0)
                        && e.NgTicks == (ngInfo?.LastWriteTimeUtc.Ticks ?? 0);

            if (!fresh)
            {
                e = new Entry
                {
                    RawLen = rawInfo.Length,
                    RawTicks = rawInfo.LastWriteTimeUtc.Ticks,
                    NgLen = ngInfo?.Length ?? 0,
                    NgTicks = ngInfo?.LastWriteTimeUtc.Ticks ?? 0,
                    Outstanding = CountDay(rawPath, ngPath)
                };
                _cache[day] = e;
                dirty = true;
            }

            if (e!.Outstanding > 0) { total += e.Outstanding; dayCount++; }
        }

        // Drop cache entries for days that no longer exist (log retention purged them) or that have
        // re-entered the window, so the file can't grow without bound.
        foreach (var stale in _cache.Keys.Where(k => !seen.Contains(k)).ToList())
        {
            _cache.Remove(stale);
            dirty = true;
        }

        Outstanding = total;
        Days = dayCount;
        if (dirty) SaveCache();
    }

    /// <summary>
    /// Outstanding = files whose LAST rawlog status is TIMEDOUT/FAILED and that the ng-retry log
    /// does not show as recovered. Mirrors how BuildItems derives the live list, so the "older"
    /// figure is directly comparable to the on-screen count.
    /// </summary>
    private static int CountDay(string rawPath, string ngPath)
    {
        var final = new Dictionary<string, string>();
        try
        {
            foreach (var line in File.ReadLines(rawPath))
            {
                var p = line.Split('|');
                if (p.Length < 3) continue;
                final[p[0] + "|" + p[1]] = p[2];
            }
        }
        catch { return 0; }

        var recovered = new HashSet<string>();
        if (File.Exists(ngPath))
        {
            try
            {
                foreach (var line in File.ReadLines(ngPath))
                {
                    var p = line.Split('|');
                    if (p.Length >= 3 && p[2] == "SUCCEEDED") recovered.Add(p[0] + "|" + p[1]);
                }
            }
            catch { /* a missing or half-written ng log just means nothing recovered yet */ }
        }

        var n = 0;
        foreach (var kv in final)
            if ((kv.Value == "TIMEDOUT" || kv.Value == "FAILED") && !recovered.Contains(kv.Key)) n++;
        return n;
    }

    private void LoadCache()
    {
        if (_cache.Count > 0) return;
        try
        {
            if (File.Exists(CachePath))
                _cache = JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(CachePath))
                         ?? new Dictionary<string, Entry>();
        }
        catch { _cache = new Dictionary<string, Entry>(); }   // corrupt cache just means a recount
    }

    private void SaveCache()
    {
        try
        {
            Directory.CreateDirectory(cfg.StateFullPath);
            File.WriteAllText(CachePath, JsonSerializer.Serialize(_cache));
        }
        catch { /* the cache is an optimisation; failing to persist it is not an error */ }
    }
}
