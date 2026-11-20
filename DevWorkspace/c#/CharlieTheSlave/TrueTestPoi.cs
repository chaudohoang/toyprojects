using System.Runtime.InteropServices;
using Accessibility;
using System.Text;
using System.Windows.Automation;

namespace CharlieTheSlave;

public sealed record PoiMeasurement(int Row, string Setup, string Analysis);

public sealed record PoiReading(string Point, string Luminance, string Cx, string Cy);

public sealed record PoiResult(PoiMeasurement Measurement, List<PoiReading> Readings);

/// <summary>Which of a set's points to export: all of them, or one picked by name or position.</summary>
public sealed record PoiPoints(string? Name, int? Number)
{
    public static PoiPoints All { get; } = new(null, null);
    public static PoiPoints Named(string name) => new(name.Trim(), null);
    public static PoiPoints Numbered(int number) => new(null, number);

    public bool IsAll => Name == null && Number == null;

    public string Describe => IsAll ? "all points" : Name is { } n ? $"point \"{n}\"" : $"point {Number}";

    /// <summary>The readings this choice keeps; empty when the measurement hasn't got that point.</summary>
    public List<PoiReading> Pick(List<PoiReading> readings)
    {
        if (IsAll) return readings;
        if (Number is { } index)
            return index >= 1 && index <= readings.Count ? new List<PoiReading> { readings[index - 1] } : new();
        var wanted = Key(Name!);
        return readings.Where(r => Key(r.Point) == wanted).ToList();
    }

    static string Key(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}

/// <summary>
/// The named set isn't in TrueTest's list. Every measurement would fail the same way, so this stops the
/// run instead of being collected as twenty identical skips.
/// </summary>
public sealed class PoiSetMissingException(string message) : Exception(message);

/// <summary>TrueTest went away while the run was going. Same reasoning as above.</summary>
public sealed class TrueTestGoneException(string message) : Exception(message);

public sealed record PoiExport(
    List<PoiResult> Results,
    List<(PoiMeasurement Measurement, string Reason)> Skipped,
    bool Stopped,
    int Total)
{
    /// <summary>Measurements never reached, because the run was stopped part-way.</summary>
    public int NotRead => Math.Max(0, Total - Results.Count - Skipped.Count);
}

/// <summary>
/// Walks TrueTest's Measurement List, opens each measurement's "Define Points of Interest…" window and
/// reads every point's Luminance / Color Cx / Color Cy for one named POI set.
///
/// Unlike <see cref="TrueTestAutomation"/>, this has to drive the real mouse. The measurement list's
/// context menu is a pop-up menu, and Windows tears a pop-up menu down the moment its owner stops being
/// the foreground window, so a posted right-click only makes it flash: it is gone before it can be read.
/// TrueTest is therefore brought to the front and the pointer is moved for real, then put back where it
/// was. Nothing else may use the mouse while this runs.
/// Call everything here from a background thread.
/// </summary>
public static class TrueTestPoi
{
    static readonly TimeSpan MenuTimeout = TimeSpan.FromSeconds(5);
    static readonly TimeSpan WindowTimeout = TimeSpan.FromSeconds(30);
    static readonly TimeSpan PointsTimeout = TimeSpan.FromSeconds(25);

    /// <summary>Consecutive failures that mean TrueTest is wedged rather than one measurement being odd.</summary>
    const int MaxFailuresInARow = 3;

    /// <summary>Pixels of give when deciding whether a row is inside the grid.</summary>
    const double Slack = 2;

    static TrueTestUiNames Names => TrueTestAutomation.Names;

    /// <summary>
    /// TrueTest's main window, found once at the start of a run. It used to be looked up afresh at
    /// every step — six times per measurement — and each of those was a chance for the UI Automation
    /// search to come back empty while TrueTest was busy, which surfaced as the nonsense message
    /// "TrueTest is still starting up" in the middle of a working run.
    /// </summary>
    static AutomationElement? _main;

    /// <summary>
    /// TrueTest's process id and main window handle, taken once at the start of a run. Plain Win32
    /// values on purpose: while a pop-up menu is open TrueTest's UI thread sits in the menu's modal
    /// loop and stops answering UI Automation, so asking it anything then — even for its process id —
    /// comes back empty and used to be reported as "TrueTest is still starting up", in the middle of a
    /// run that was working. EnumWindows and GetWindowThreadProcessId keep working regardless.
    /// </summary>
    static int _mainPid;
    static IntPtr _mainHwnd;

    static AutomationElement MainWindow() => _main ??= TrueTestAutomation.MainWindow();

    static int MainPid => _mainPid != 0 ? _mainPid : MainWindow().Current.ProcessId;

    // ---------- the measurement list ----------

    /// <summary>
    /// The Measurement List's grid. Normally it is found inside the MDI child window of that name, but a
    /// maximized MDI child gives up its own title bar — TrueTest moves the name into the main window's
    /// title — and UI Automation then stops reporting it as a window of its own. So when that lookup
    /// fails, find the grid by a column only it has: the sequence grid also has "Measurement Setup", so
    /// that would match the wrong one.
    /// </summary>
    /// <summary>
    /// The part of the grid that is actually on screen: a child window bigger than the MDI client is
    /// clipped to it, and UI Automation reports the hidden part as if it were visible.
    /// </summary>
    static System.Windows.Rect VisibleGrid(AutomationElement grid)
    {
        var rect = grid.Current.BoundingRectangle;
        var client = MainWindow().FindFirst(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ClassNameProperty, Names.MdiClientClass));
        if (client == null) return rect;
        rect.Intersect(client.Current.BoundingRectangle);
        return rect;
    }

    static AutomationElement MeasurementGrid()
    {
        var main = MainWindow();

        var list = main.FindFirst(TreeScope.Descendants, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
            new PropertyCondition(AutomationElement.NameProperty, Names.MeasurementListWindow)));
        if (list?.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table)) is { } inWindow)
            return inWindow;

        foreach (var table in ByType(main, ControlType.Table))
            if (ByType(table, ControlType.Header, depth: 3).Any(h => SafeName(h) == Names.MeasurementDateColumn))
                return table;

        throw new TrueTestNotReadyException(
            $"TrueTest's \"{Names.MeasurementListWindow}\" isn't ready — it may still be loading the " +
            "measurement database. Open it from the View menu if it's closed, then ask me to export poi again.");
    }

    /// <summary>How many rows the grid says it has, which can exceed the rows it has drawn.</summary>
    public static int MeasurementCount()
    {
        var grid = MeasurementGrid();
        return grid.TryGetCurrentPattern(GridPattern.Pattern, out var p) ? ((GridPattern)p).Current.RowCount : 0;
    }

    /// <summary>
    /// The measurements the grid has drawn. A DataGridView only puts visible rows in the UI Automation
    /// tree, so a list longer than the window holds comes back short; compare with <see cref="MeasurementCount"/>.
    /// </summary>
    public static List<PoiMeasurement> GetMeasurements()
    {
        var grid = MeasurementGrid();
        var rows = new List<PoiMeasurement>();
        foreach (AutomationElement row in grid.FindAll(TreeScope.Children, Condition.TrueCondition))
        {
            var name = row.Current.Name;
            if (!name.StartsWith("Row ") || !int.TryParse(name.Substring(4), out int index)) continue;
            var cells = row.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>().ToList();
            string Cell(string column) => ValueOf(cells.FirstOrDefault(c => c.Current.Name.StartsWith($"{column} {name}")));
            rows.Add(new PoiMeasurement(index, Cell("Measurement Setup"), Cell("Analysis")));
        }
        return rows.OrderBy(r => r.Row).ToList();
    }

    // ---------- the whole run ----------

    public static PoiExport ExportAll(string setName, PoiPoints points, IProgress<string> progress,
        CancellationToken ct)
    {
        _main = TrueTestAutomation.MainWindow();
        _mainPid = _main.Current.ProcessId;
        _mainHwnd = new IntPtr(_main.Current.NativeWindowHandle);
        var measurements = GetMeasurements();
        if (measurements.Count == 0)
            throw new InvalidOperationException("TrueTest's Measurement List is empty, so there's nothing to read.");

        int total = MeasurementCount();
        if (total > measurements.Count)
            progress.Report($"⚠ The Measurement List has {total} measurements but only {measurements.Count} are drawn; " +
                            "TrueTest only lets me see the rows on screen. Make the window taller to cover the rest.");

        var mainHwnd = _mainHwnd;
        GetCursorPos(out var restoreTo);

        var results = new List<PoiResult>();
        var skipped = new List<(PoiMeasurement, string)>();
        bool stopped = false;
        int failures = 0;
        var listWindow = FitMeasurementList();
        List<PoiReading>? previous = null; // the last measurement's points, to spot a stale read
        try
        {
            foreach (var m in measurements)
            {
                ct.ThrowIfCancellationRequested();
                // Outside the per-measurement catch below: if TrueTest has gone, every remaining
                // measurement would fail the same way.
                if (!TrueTestAutomation.IsRunning)
                    throw new TrueTestGoneException(
                        "TrueTest closed while I was working. Please start it and ask me to export poi again.");
                progress.Report($"Measurement {m.Row + 1} of {measurements.Count}: {m.Setup}…");
                try
                {
                    // Always read every point: the whole set is what tells a stale window from a fresh
                    // one, and one point's numbers alone could easily repeat between measurements.
                    var readings = ReadOne(mainHwnd, m, setName, previous, ct);
                    previous = readings;

                    var chosen = points.Pick(readings);
                    if (chosen.Count == 0)
                        throw new PoiUnavailableException(
                            $"it has no {points.Describe} (its points are: {string.Join(", ", readings.Select(r => r.Point))})");
                    results.Add(new PoiResult(m, chosen));
                }
                catch (PoiUnavailableException ex)
                {
                    skipped.Add((m, ex.Message));
                }
                catch (Exception ex) when (ex is PoiSetMissingException or TrueTestNotReadyException
                                           or OperationCanceledException)
                {
                    throw; // the next measurement would fail identically
                }
                catch (Exception ex)
                {
                    // One bad measurement mustn't throw away the ones already read: note it and carry on.
                    skipped.Add((m, ex.Message));
                    progress.Report($"⚠ {m.Setup}: {ex.Message}");
                    CloseMenu();
                    ClosePoiWindow(CancellationToken.None);

                    // But a run where nothing works any more is worse than one that stops: something in
                    // TrueTest is wedged and the remaining measurements will fail the same way.
                    if (++failures >= MaxFailuresInARow)
                        throw new InvalidOperationException(
                            $"Giving up — {failures} measurements in a row failed. The last said: {ex.Message}");
                    continue;
                }
                failures = 0;
            }
        }
        catch (OperationCanceledException)
        {
            stopped = true;
        }
        finally
        {
            RestoreMeasurementList(listWindow);
            SetCursorPos(restoreTo.X, restoreTo.Y);
            _main = null;
            _mainPid = 0;
            _mainHwnd = IntPtr.Zero;
        }
        return new PoiExport(results, skipped, stopped, measurements.Count);
    }

    /// <summary>
    /// Makes the Measurement List fill the MDI area for the duration of the run, and remembers how to
    /// put it back. A child window taller than the MDI client is simply clipped — TrueTest draws only
    /// the part that fits — while UI Automation still reports the rows below the cut at ordinary
    /// coordinates with IsOffscreen=false. Right-clicking one of those lands on whatever panel is
    /// underneath, and no amount of scrolling helps, because as far as the grid is concerned every row
    /// already fits inside it. Seen as a 600px-tall list inside a 366px-tall MDI area.
    /// </summary>
    static (AutomationElement Window, WindowVisualState State)? FitMeasurementList()
    {
        var main = MainWindow();
        var list = main.FindFirst(TreeScope.Descendants, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
            new PropertyCondition(AutomationElement.NameProperty, Names.MeasurementListWindow)));
        // Not found means it is already maximized: a maximized MDI child gives up its own title.
        if (list == null || !list.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern)) return null;

        var window = (WindowPattern)pattern;
        WindowVisualState was;
        try
        {
            was = window.Current.WindowVisualState;
            if (was == WindowVisualState.Maximized || !window.Current.CanMaximize) return null;
            window.SetWindowVisualState(WindowVisualState.Maximized);
        }
        catch (Exception ex) when (Transient(ex) || ex is InvalidOperationException) { return null; }

        Thread.Sleep(500);
        return (list, was);
    }

    static void RestoreMeasurementList((AutomationElement Window, WindowVisualState State)? saved)
    {
        if (saved is not { } s) return;
        try
        {
            if (s.Window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern))
                ((WindowPattern)pattern).SetWindowVisualState(s.State);
        }
        catch (Exception ex) when (Transient(ex) || ex is InvalidOperationException) { }
    }

    /// <summary>A measurement that simply offers no POI; the run carries on with the next one.</summary>
    sealed class PoiUnavailableException(string message) : Exception(message);



    /// <summary>
    /// UI Automation reports an element that has gone away either as ElementNotAvailableException or as a
    /// bare COMException (E_FAIL) — a closing pop-up menu usually gives the second. Both mean "look again".
    /// </summary>
    static bool Transient(Exception ex) => ex is ElementNotAvailableException or COMException;

    /// <summary>Runs a step, labelling anything that escapes so the chat message says where it broke.</summary>
    static T Step<T>(string what, Func<T> work)
    {
        try
        {
            return work();
        }
        catch (TrueTestNotReadyException ex)
        {
            throw new TrueTestNotReadyException($"{what}: {ex.Message}");
        }
        catch (Exception ex) when (ex is not (InvalidOperationException or PoiUnavailableException
                                      or PoiSetMissingException or TrueTestGoneException
                                      or OperationCanceledException))
        {
            throw new InvalidOperationException($"{what}: {ex.Message}", ex);
        }
    }

    static List<PoiReading> ReadOne(IntPtr mainHwnd, PoiMeasurement m, string setName,
        List<PoiReading>? previous, CancellationToken ct)
    {
        Step("bringing TrueTest to the front", () => { Focus(mainHwnd); return 0; });
        Step($"finding row {m.Row + 1} ({m.Setup})", () => RowRect(m.Row));
        var names = Step($"opening the right-click menu for {m.Setup}", () => OpenContextMenu(m.Row, mainHwnd, ct));

        var wanted = Norm(Names.PoiMenuItem);
        var item = names.FirstOrDefault(i => Norm(i.Name).StartsWith(wanted));
        if (item == null)
        {
            CloseMenu();
            throw new PoiUnavailableException(
                $"its right-click menu has no \"{Names.PoiMenuItem}\" " +
                $"(it offers: {string.Join(", ", names.Select(i => i.Name))})");
        }

        // Choosing a menu entry that opens a window can block until that window closes, so don't wait on it.
        FireAndForget(item.Choose);

        var poi = WaitFor(FindPoiWindow, WindowTimeout, ct)
                  ?? throw new InvalidOperationException(
                      $"The \"{Names.PoiWindowTitle}\" window didn't open for measurement {m.Row + 1} ({m.Setup}).");
        try
        {
            var before = ReadPointsOnce(poi);
            bool switched = Step($"choosing the \"{setName}\" set", () => SelectSet(poi, setName, ct));
            // What the points must *not* still be: the set that was showing if we just switched, and
            // otherwise the previous measurement's values — a freshly opened window shows those until
            // TrueTest has re-evaluated the set against this measurement's image.
            var stale = switched ? before : previous;
            return Step("reading the points", () => ReadPoints(poi, stale, ct));
        }
        finally
        {
            ClosePoiWindow(ct);
        }
    }

    // ---------- the context menu ----------

    /// <summary>
    /// The measurement row's rectangle on screen, scrolling the grid until the row is actually in view.
    ///
    /// The grid lists every row in the UI Automation tree whether it is drawn or not, but it offers no way
    /// to scroll: neither ScrollPattern on the grid nor ScrollItemPattern on a row. So the wheel is used,
    /// posted straight at the grid — which works without the window being focused — a couple of notches at
    /// a time, until the row is inside the grid below its column headers.
    /// </summary>
    static System.Windows.Rect RowRect(int rowIndex)
    {
        var grid = MeasurementGrid();
        var hwnd = new IntPtr(grid.Current.NativeWindowHandle);

        AutomationElement? Header(int index) => grid.FindFirst(TreeScope.Descendants, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Header),
            new PropertyCondition(AutomationElement.NameProperty, $"Row {index}")));

        if (Header(rowIndex) == null)
            throw new InvalidOperationException($"TrueTest's Measurement List has no row {rowIndex + 1}.");

        double lastTop = double.NaN;
        int stuck = 0;
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var gridRect = VisibleGrid(grid);
            // The column header strip is drawn over the top of the grid, and a scrolled-away row keeps
            // reporting a position underneath it — so "in view" means below the strip's bottom edge.
            var topRow = Descendants(grid, depth: 1).FirstOrDefault(e => SafeName(e) == "Top Row");
            double firstUsable = topRow?.Current.BoundingRectangle.Bottom ?? gridRect.Top + 40;

            var header = Header(rowIndex);
            if (header == null) throw new InvalidOperationException($"Row {rowIndex + 1} vanished from TrueTest's Measurement List.");
            var r = header.Current.BoundingRectangle;
            // A row scrolled to the very top lands exactly on the header strip's bottom edge, so compare
            // with a pixel or two of slack: an exact >= fails on fractional, display-scaled coordinates
            // and the loop then scrolls forever against the end stop and reports "couldn't scroll".
            if (!header.Current.IsOffscreen && !r.IsEmpty &&
                r.Top >= firstUsable - Slack && r.Bottom <= gridRect.Bottom + Slack)
                return r;

            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException(
                    $"Row {rowIndex + 1} is scrolled out of view and the grid has no window to scroll. " +
                    "Please scroll to it yourself and try again.");

            // Page keys posted straight at the grid, which is the only scrolling that proved dependable:
            // the mouse wheel — posted or real — is ignored unless TrueTest is the foreground window and
            // the grid has been activated, and silently does nothing otherwise. Posted keys need neither.
            // Which way to go comes from the row's own rectangle, which the grid keeps accurate even for
            // rows it hasn't drawn; the rows themselves claim IsOffscreen=false at any scroll position,
            // so only the row *headers* can be believed.
            PostKey(hwnd, r.IsEmpty || r.Top >= firstUsable ? VK_NEXT : VK_PRIOR);
            Thread.Sleep(200);

            // Give up only once a page key has changed nothing twice running — and the loop always
            // re-tests above before that counts, because the last one may well have arrived.
            double top = Header(rowIndex)?.Current.BoundingRectangle.Top ?? double.NaN;
            stuck = top.Equals(lastTop) ? stuck + 1 : 0;
            lastTop = top;
            if (stuck >= 2)
                throw new InvalidOperationException(
                    $"I couldn't bring row {rowIndex + 1} of TrueTest's Measurement List into view " +
                    $"(it sits at y={(int)r.Top}-{(int)r.Bottom}; the visible rows run from y={(int)firstUsable} " +
                    $"to y={(int)gridRect.Bottom}). The Measurement List is probably taller than the space " +
                    "TrueTest has for it — maximize it, or make it small enough to fit.");
        }
        throw new InvalidOperationException($"TrueTest's Measurement List wouldn't scroll to row {rowIndex + 1}.");
    }

    /// <summary>One entry of a pop-up menu, with the name it had when the menu was read.</summary>
    sealed record MenuEntry(string Name, Action Choose);

    /// <summary>
    /// Right-clicks the row and returns the menu's entries, names and all, read the moment the menu
    /// appears (a pop-up can die under us, and then even reading a name throws).
    /// </summary>
    /// <summary>
    /// The point to right-click for a row: the middle of one of its cells, read fresh. Never a fixed
    /// offset from the row header — the window gets moved and resized while this runs, and an
    /// extrapolated point lands outside the grid, which right-clicks nothing at all.
    /// </summary>
    static (System.Windows.Point Point, System.Windows.Rect Row)? ClickTarget(int rowIndex)
    {
        var grid = MeasurementGrid();
        var gridRect = VisibleGrid(grid);

        var row = Descendants(grid, depth: 1).FirstOrDefault(e => SafeName(e) == $"Row {rowIndex}");
        if (row == null) return null;

        var cell = Descendants(row, depth: 1)
            .FirstOrDefault(c => SafeName(c).StartsWith($"{Names.MeasurementSetupColumn} Row {rowIndex}"))
            ?? Descendants(row, depth: 1).FirstOrDefault(c => SafeName(c).StartsWith("Serial Number Row "));
        if (cell == null) return null;

        var r = cell.Current.BoundingRectangle;
        if (r.IsEmpty) return null;
        var point = new System.Windows.Point(r.Left + r.Width / 2, r.Top + r.Height / 2);

        // The cell must really be inside the grid; if the layout moved under us this catches it.
        if (!gridRect.Contains(point)) return null;
        return (point, r);
    }

    static List<MenuEntry> OpenContextMenu(int rowIndex, IntPtr mainHwnd, CancellationToken ct)
    {
        int pid = MainPid;
        System.Windows.Rect lastRow = default;
        var lastPoint = new System.Windows.Point(double.NaN, double.NaN);

        // Twice: the first right-click after the grid has just scrolled sometimes produces nothing, and
        // the coordinates are worked out again in between in case the window moved.
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            Focus(mainHwnd);

            if (ClickTarget(rowIndex) is not { } target)
            {
                RowRect(rowIndex); // bring it back into view and try once more
                if (ClickTarget(rowIndex) is not { } retry)
                    throw new InvalidOperationException(
                        $"I couldn't work out where row {rowIndex + 1} is on screen. " +
                        "Please leave TrueTest's Measurement List alone while I work.");
                target = retry;
            }
            lastPoint = target.Point;
            lastRow = target.Row;

            SetCursorPos((int)target.Point.X, (int)target.Point.Y - 1);
            Thread.Sleep(60);
            SetCursorPos((int)target.Point.X, (int)target.Point.Y); // a real move, so the pointer is seen to arrive
            Thread.Sleep(150);

            // Select the row first. "Define Points of Interest…" works on the measurement TrueTest has
            // selected, and a right-click does not always move the selection — without this the window
            // can open for whichever measurement happened to be selected before, which would quietly
            // export the wrong rows rather than fail.
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(60);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(250);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(80);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);

            var items = WaitFor(() => Popups(pid, mainHwnd).Select(ReadMenu)
                .OrderByDescending(found => found.Count).FirstOrDefault(found => found.Count > 0), MenuTimeout, ct);
            if (items != null) return items;

            CloseMenu();
            Thread.Sleep(250);
        }

        var windows = Popups(pid, mainHwnd).Select(DescribeWindow).ToList();
        CloseMenu();
        throw new InvalidOperationException(
            $"TrueTest didn't show a right-click menu I could read (I right-clicked at " +
            $"{(int)lastPoint.X},{(int)lastPoint.Y}, inside the cell at " +
            $"{(int)lastRow.Left},{(int)lastRow.Top}-{(int)lastRow.Right},{(int)lastRow.Bottom}). " +
            (windows.Count > 0
                ? $"Its other windows held: {string.Join(" | ", windows)}"
                : "TrueTest had no pop-up window open at all."));
    }

    /// <summary>
    /// A pop-up menu's entries. Read through MSAA (oleacc), not UI Automation: UI Automation shows only
    /// the entries TrueTest's measurement menu inherits from the Measurement List tool strip and stops at
    /// the first separator, hiding "Define Points of Interest…" and everything else below it. MSAA reports
    /// the menu as drawn. UI Automation is still the fallback, in case a menu has no MSAA object.
    /// </summary>
    static List<MenuEntry> ReadMenu(IntPtr hwnd)
    {
        var msaa = MsaaMenu(hwnd);
        if (msaa.Count > 0) return msaa;

        AutomationElement element;
        try { element = AutomationElement.FromHandle(hwnd); }
        catch (Exception ex) when (Transient(ex)) { return new(); }
        return Descendants(element)
            .Select(e => (Name: SafeName(e).Trim(), Element: e))
            .Where(e => e.Name.Length > 0 && CanInvoke(e.Element))
            .Select(e => new MenuEntry(e.Name,
                () => ((InvokePattern)e.Element.GetCurrentPattern(InvokePattern.Pattern)).Invoke()))
            .ToList();
    }

    static List<MenuEntry> MsaaMenu(IntPtr hwnd)
    {
        var entries = new List<MenuEntry>();
        var iid = IID_IAccessible;
        try
        {
            if (AccessibleObjectFromWindow(hwnd, OBJID_CLIENT, ref iid, out object raw) != 0 ||
                raw is not IAccessible root) return entries;
            CollectMsaa(root, entries, depth: 3);
        }
        catch (COMException) { }
        return entries;
    }

    static void CollectMsaa(IAccessible parent, List<MenuEntry> into, int depth)
    {
        if (depth == 0) return;
        int count;
        try { count = parent.accChildCount; }
        catch (COMException) { return; }
        if (count <= 0) return;

        var children = new object[count];
        try
        {
            if (AccessibleChildren(parent, 0, count, children, out count) != 0) return;
        }
        catch (COMException) { return; }

        for (int i = 0; i < count; i++)
        {
            switch (children[i])
            {
                case IAccessible child:
                    AddMsaa(child, CHILDID_SELF, into);
                    CollectMsaa(child, into, depth - 1);
                    break;
                case int childId:
                    AddMsaa(parent, childId, into);
                    break;
            }
        }
    }

    static void AddMsaa(IAccessible accessible, int childId, List<MenuEntry> into)
    {
        try
        {
            // Separators have no name, which is also how they are left out.
            var name = (accessible.get_accName(childId) ?? "").Trim();
            if (name.Length == 0) return;
            if (accessible.get_accRole(childId) is int role && role is not (ROLE_MENUITEM or ROLE_PUSHBUTTON)) return;
            into.Add(new MenuEntry(name, () => accessible.accDoDefaultAction(childId)));
        }
        catch (COMException) { }
    }

    static string DescribeWindow(IntPtr hwnd)
    {
        var cls = new StringBuilder(256);
        GetClassName(hwnd, cls, cls.Capacity);
        var names = ReadMenu(hwnd).Select(e => e.Name).ToList();
        return $"[{cls}] → {(names.Count > 0 ? string.Join(", ", names) : "(nothing in it)")}";
    }

    /// <summary>
    /// TrueTest's visible top-level windows apart from its main one — a pop-up menu is one of these.
    /// They are not filtered by window class: WinForms names a ToolStripDropDown's class after the window
    /// styles it was created with, so the name differs between menus and can't be matched on.
    /// </summary>
    static List<IntPtr> Popups(int pid, IntPtr mainHwnd)
    {
        var found = new List<IntPtr>();
        EnumWindows((hwnd, _) =>
        {
            if (hwnd == mainHwnd || !IsWindowVisible(hwnd)) return true;
            GetWindowThreadProcessId(hwnd, out int owner);
            if (owner == pid) found.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return found;
    }

    /// <summary>
    /// Everything clickable in a pop-up menu, whatever UI Automation calls it. TrueTest's measurement menu
    /// doesn't report its entries as MenuItem, so the control type isn't matched on: anything with a name
    /// that can be invoked counts, which leaves out the separators (they are invokable but unnamed).
    /// </summary>
    /// <summary>
    /// Elements of one control type, asking the provider for the subtree first and walking it step by step
    /// if that is refused (see <see cref="Descendants"/>).
    /// </summary>
    static List<AutomationElement> ByType(AutomationElement root, ControlType type, int depth = 8)
    {
        try
        {
            return root.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, type)).Cast<AutomationElement>().ToList();
        }
        catch (Exception ex) when (Transient(ex)) { }

        return Descendants(root, depth).Where(e =>
        {
            try { return Equals(e.Current.ControlType, type); }
            catch (Exception ex) when (Transient(ex)) { return false; }
        }).ToList();
    }

    static bool CanInvoke(AutomationElement element)
    {
        try { return element.TryGetCurrentPattern(InvokePattern.Pattern, out _); }
        catch (Exception ex) when (Transient(ex)) { return false; }
    }

    /// <summary>
    /// Walks a subtree one step at a time. FindAll(TreeScope.Descendants) asks the provider for the whole
    /// subtree in one call, which TrueTest's pop-up menu answers with E_FAIL; walking child by child works.
    /// </summary>
    static List<AutomationElement> Descendants(AutomationElement root, int depth = 3)
    {
        var all = new List<AutomationElement>();

        void Visit(AutomationElement node, int left)
        {
            if (left == 0) return;
            foreach (var child in ChildrenOf(node))
            {
                all.Add(child);
                Visit(child, left - 1);
            }
        }

        Visit(root, depth);
        return all;
    }

    /// <summary>
    /// One node's children. Asking for them in a single call is tried first; sibling-by-sibling walking is
    /// only the fallback, because the control-view walk of TrueTest's measurement menu gives up at the first
    /// separator and so reports just the top handful of entries. The raw view is used for the same reason.
    /// </summary>
    static List<AutomationElement> ChildrenOf(AutomationElement node)
    {
        try
        {
            var batch = node.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>().ToList();
            if (batch.Count > 0) return batch;
        }
        catch (Exception ex) when (Transient(ex)) { }

        var list = new List<AutomationElement>();
        var walker = TreeWalker.RawViewWalker;
        AutomationElement? child;
        try { child = walker.GetFirstChild(node); }
        catch (Exception ex) when (Transient(ex)) { return list; }
        while (child != null)
        {
            list.Add(child);
            try { child = walker.GetNextSibling(child); }
            catch (Exception ex) when (Transient(ex)) { break; } // keep what was reached
        }
        return list;
    }

    static string SafeName(AutomationElement element)
    {
        try { return element.Current.Name ?? ""; }
        catch (Exception ex) when (Transient(ex)) { return ""; }
    }

    static void CloseMenu()
    {
        keybd_event(VK_ESCAPE, 0, 0, IntPtr.Zero);
        keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        Thread.Sleep(250);
    }

    // ---------- the Define Points of Interest window ----------

    /// <summary>
    /// The POI window, found with EnumWindows rather than through the UI Automation tree: the window is
    /// owned by TrueTest's main window, and UI Automation nests an owned window under its owner instead of
    /// listing it as a top-level one, so it is never a child of the root element. (InstallerAutomation hits
    /// the same thing with the installer's dialogs.)
    /// </summary>
    static AutomationElement? FindPoiWindow()
    {
        var hwnd = FindPoiHandle();
        if (hwnd == IntPtr.Zero) return null;
        try { return AutomationElement.FromHandle(hwnd); }
        catch (Exception ex) when (Transient(ex)) { return null; }
    }

    static IntPtr FindPoiHandle()
    {
        int pid = MainPid;
        var found = IntPtr.Zero;
        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd)) return true;
            GetWindowThreadProcessId(hwnd, out int owner);
            if (owner != pid) return true;
            var title = new StringBuilder(512);
            GetWindowText(hwnd, title, title.Capacity);
            if (!title.ToString().StartsWith(Names.PoiWindowTitle, StringComparison.OrdinalIgnoreCase)) return true;
            found = hwnd;
            return false;
        }, IntPtr.Zero);
        return found;
    }

    /// <summary>The POI-set combo box, found through the tool strip that holds the POI set buttons.</summary>
    static AutomationElement SetCombo(AutomationElement poi)
    {
        foreach (var bar in ByType(poi, ControlType.ToolBar))
        {
            var children = Descendants(bar, depth: 1);
            if (!children.Any(c => SafeName(c) == Names.PoiSetMarkerButton)) continue;
            var combo = children.FirstOrDefault(c =>
            {
                try { return Equals(c.Current.ControlType, ControlType.ComboBox); }
                catch (Exception ex) when (Transient(ex)) { return false; }
            });
            if (combo != null) return combo;
        }
        throw new InvalidOperationException(
            $"Couldn't find the POI set list in the \"{Names.PoiWindowTitle}\" window " +
            $"(I look on the tool strip that has the \"{Names.PoiSetMarkerButton}\" button).");
    }

    static List<string> ComboItems(AutomationElement combo)
    {
        var hwnd = new IntPtr(combo.Current.NativeWindowHandle);
        if (hwnd == IntPtr.Zero) return new();
        int count = (int)SendMessage(hwnd, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);
        var items = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var text = new StringBuilder(512);
            SendMessage(hwnd, CB_GETLBTEXT, (IntPtr)i, text);
            items.Add(text.ToString());
        }
        return items;
    }

    /// <summary>
    /// Picks a points-of-interest set, the way a person does: focus the combo, drop it open, walk to the
    /// entry with the arrow keys and press Enter. Returns true when the selection really changed.
    ///
    /// UI Automation's ValuePattern.SetValue is no good here. It only rewrites the text the combo shows:
    /// TrueTest never gets a selection-changed notification, so it keeps evaluating the set that was
    /// loaded before, while the combo — and anything that reads the combo back to check — says otherwise.
    /// CB_GETCURSEL is the honest answer, so that is what is compared.
    /// </summary>
    static bool SelectSet(AutomationElement poi, string setName, CancellationToken ct)
    {
        var combo = SetCombo(poi);
        var hwnd = new IntPtr(combo.Current.NativeWindowHandle);
        if (hwnd == IntPtr.Zero)
            throw new InvalidOperationException("TrueTest's point-of-interest set list has no window to work with.");

        var items = ComboItems(combo);
        int target = items.FindIndex(i => Norm(i) == Norm(setName));
        if (target < 0)
            throw new PoiSetMissingException(
                $"Stopped — TrueTest has no \"{setName}\" point-of-interest set. " +
                "Please set it up in TrueTest, then ask me to export poi again.");

        if (Selected(hwnd) == target) return false;

        // Arrow keys are POSTED to the combo, never typed globally. A closed Win32 combo moves its
        // selection on Up/Down and tells its parent, which is the notification TrueTest acts on — and
        // posting means the keys cannot land anywhere else. Typing them globally (keybd_event) needs the
        // combo to really hold the focus, and when it didn't, F4/arrows/Enter went into TrueTest's main
        // window instead and left it unable to raise a context menu for the rest of the run.
        for (int guard = 0; guard <= items.Count * 3; guard++)
        {
            ct.ThrowIfCancellationRequested();
            int now = Selected(hwnd);
            if (now == target) break;
            PostKey(hwnd, now >= 0 && now > target ? VK_UP : VK_DOWN);
            Thread.Sleep(150);
        }

        if (Selected(hwnd) != target)
        {
            int now = Selected(hwnd);
            throw new InvalidOperationException(
                $"TrueTest wouldn't switch to the \"{setName}\" point-of-interest set " +
                $"(its list is still on \"{(now >= 0 && now < items.Count ? items[now] : "?")}\").");
        }
        Thread.Sleep(200);
        return true;
    }

    static void PostKey(IntPtr hwnd, byte key)
    {
        PostMessage(hwnd, WM_KEYDOWN, (IntPtr)key, (IntPtr)1);
        PostMessage(hwnd, WM_KEYUP, (IntPtr)key, unchecked((IntPtr)(int)0xC0000001));
    }

    static int Selected(IntPtr combo) => (int)SendMessage(combo, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);

    static List<PoiReading> ReadPointsOnce(AutomationElement poi)
    {
        AutomationElement? grid = null;
        foreach (var table in ByType(poi, ControlType.Table))
        {
            if (ByType(table, ControlType.Header, depth: 3).Any(h => SafeName(h) == Names.PoiCxColumn))
            {
                grid = table;
                break;
            }
        }
        if (grid == null)
            throw new InvalidOperationException(
                $"Couldn't find the points table (no \"{Names.PoiCxColumn}\" column) in the \"{Names.PoiWindowTitle}\" window.");

        var readings = new List<PoiReading>();
        foreach (var row in Descendants(grid, depth: 1))
        {
            var name = SafeName(row);
            if (!name.StartsWith("Row ")) continue;
            var cells = Descendants(row, depth: 1);
            string Cell(string column) => ValueOf(cells.FirstOrDefault(c => SafeName(c) == $"{column} {name}"));
            readings.Add(new PoiReading(Cell(Names.PoiPointColumn), Cell(Names.PoiLuminanceColumn),
                Cell(Names.PoiCxColumn), Cell(Names.PoiCyColumn)));
        }
        return readings;
    }

    /// <summary>
    /// The points as the window settles on them. Switching sets makes TrueTest rebuild the table and
    /// re-evaluate every point against the image, so the rows are read until two reads running agree —
    /// and, when the set was just switched, until they stop being the previous set's rows. Reading once
    /// straight after the switch returns the old set's points, which is exactly the bug this avoids.
    /// </summary>
    static List<PoiReading> ReadPoints(AutomationElement poi, List<PoiReading>? previous, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + PointsTimeout;
        List<PoiReading>? last = null;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            var now = ReadPointsOnce(poi);
            if (now.Count > 0 && Same(now, last) && !Same(now, previous)) return now;
            last = now;
            Thread.Sleep(250);
        }

        if (last is { Count: > 0 } && !Same(last, previous)) return last;
        throw new InvalidOperationException(last is { Count: > 0 }
            ? $"its points were still the previous ones {PointsTimeout.TotalSeconds:0} seconds on, so I'd have " +
              "written the same numbers twice"
            : $"the \"{Names.PoiWindowTitle}\" window showed no points.");
    }

    static bool Same(List<PoiReading> a, List<PoiReading>? b) =>
        b != null && a.Count == b.Count && a.Zip(b, (x, y) => x == y).All(same => same);

    static void ClosePoiWindow(CancellationToken ct)
    {
        var hwnd = FindPoiHandle();
        if (hwnd == IntPtr.Zero) return;

        PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        if (WaitFor(() => FindPoiHandle() == IntPtr.Zero ? "" : null, TimeSpan.FromSeconds(10), ct) != null) return;

        throw new InvalidOperationException(
            $"The \"{Names.PoiWindowTitle}\" window wouldn't close — it may be asking you something. Please check TrueTest.");
    }

    // ---------- plumbing ----------

    static void Focus(IntPtr mainHwnd)
    {
        if (GetForegroundWindow() == mainHwnd) return;
        SetForegroundWindow(mainHwnd);
        Thread.Sleep(400);
        if (GetForegroundWindow() != mainHwnd)
            throw new InvalidOperationException(
                "Windows wouldn't let me bring TrueTest to the front. Please click TrueTest once and try again.");
    }

    static string ValueOf(AutomationElement? e)
    {
        if (e == null) return "";
        try
        {
            return e.TryGetCurrentPattern(ValuePattern.Pattern, out var p) ? ((ValuePattern)p).Current.Value ?? "" : "";
        }
        catch (Exception ex) when (Transient(ex)) { return ""; }
    }

    static string Norm(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    static void FireAndForget(Action action) => _ = Task.Run(() =>
    {
        try { action(); } catch { /* the window changed under us; the wait that follows decides what happened */ }
    });

    static T? WaitFor<T>(Func<T?> probe, TimeSpan timeout, CancellationToken ct) where T : class
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (probe() is { } found) return found;
            }
            catch (Exception ex) when (Transient(ex)) { }
            Thread.Sleep(150);
        }
        return null;
    }

    const uint MOUSEEVENTF_LEFTDOWN = 0x0002, MOUSEEVENTF_LEFTUP = 0x0004;
    const uint MOUSEEVENTF_RIGHTDOWN = 0x0008, MOUSEEVENTF_RIGHTUP = 0x0010;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const byte VK_ESCAPE = 0x1B, VK_UP = 0x26, VK_DOWN = 0x28, VK_PRIOR = 0x21, VK_NEXT = 0x22;
    const uint WM_CLOSE = 0x0010, WM_COMMAND = 0x0111;
    const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101;
    const uint CB_GETCOUNT = 0x0146, CB_GETCURSEL = 0x0147, CB_GETLBTEXT = 0x0148;

    static Guid IID_IAccessible = new("618736e0-3c3d-11cf-810c-00aa00389b71");
    const uint OBJID_CLIENT = 0xFFFFFFFC;
    const int CHILDID_SELF = 0, ROLE_MENUITEM = 0x0C, ROLE_PUSHBUTTON = 0x2B;

    [DllImport("oleacc.dll")]
    static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint objectId, ref Guid iid,
        [MarshalAs(UnmanagedType.Interface)] out object accessible);

    [DllImport("oleacc.dll")]
    static extern int AccessibleChildren(IAccessible container, int start, int count,
        [Out] object[] children, out int obtained);

    delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr param);

    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int X, Y; }

    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc callback, IntPtr param);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll")] static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT point);
    [DllImport("user32.dll")] static extern void mouse_event(uint flags, uint dx, uint dy, uint data, IntPtr extra);
    [DllImport("user32.dll")] static extern void keybd_event(byte key, byte scan, uint flags, IntPtr extra);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, StringBuilder lParam);
}
