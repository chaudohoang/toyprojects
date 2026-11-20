# Reading and writing TrueTest's analysis settings and calibration

Removed from the app on 2026-10-26 because the commands weren't wanted. Kept here because the
behaviour was expensive to work out, and because the next job against the Analysis Setup window or
the measurement-setup navigator should start from this rather than from scratch.

Nothing in the app references any of this. To bring a piece back, paste it into
`TrueTestAutomation.cs` / `PromptWindow.TrueTest.cs`, re-add the control names to `TrueTestUiNames`
and `commands.default.json`, and register the action in `PromptWindow.BuildActions`.

This was written against **TrueTest 1.9.226.156**.

---

## 1. The traps, before the code

These are the parts that are not obvious from reading the code, and that cost real time:

- **Never use UIA `Invoke()` on anything that opens a modal window.** `Invoke()` blocks until the
  click handler returns, which is forever if a message box appears — and while it blocks, every
  other UIA call into TrueTest stalls too. Post `BM_CLICK`, or post `WM_COMMAND`/`BN_CLICKED` to the
  button's parent, and poll for the window.

- **Opening a step's editor needs three posted messages, in order.** Click the row header, press
  `End` (moves to the Edit column), press `Space` (a DataGridView button cell treats Space as a
  click). Clicking the Edit button cell directly is unreliable with posted messages and sometimes
  opened the editor twice. The step must already be scrolled into view — a DataGridView doesn't put
  off-screen rows in the UIA tree at all.

- **Closing the Analysis Setup window:** posting `WM_COMMAND` with `BN_CLICKED` to the button's
  *parent* works whether or not TrueTest has focus. `BM_CLICK` and UIA `Invoke()` are both ignored
  unless the dialog is the active window. The first click can be swallowed while the settings grid
  finishes an edit, so retry — three attempts was enough.

- **A closed dialog can linger, hidden, until TrueTest disposes it.** Only count windows for which
  `IsWindowVisible` is true, or you will "find" a dialog that isn't there.

- **`ValuePattern.SetValue` only types into the grid's in-place editor.** `Enter` has to be posted
  afterwards to commit it, exactly as a person would. And `SetValue` can block forever on TrueTest's
  own "invalid value" message box, so run it on a separate task with a timeout (5 s was fine).
  After saving, reopen the step and re-read the value to prove it actually stuck.
  (The same class of lie bites the POI set combo — see `TrueTestPoi.SelectSet`.)

- **A changed setting lives only in TrueTest's memory** until the Save Sequence button is clicked.

- **The Calibration Settings category's expand arrow is not exposed to UIA.** The workaround is to
  post a click 12 px in from the right-hand end of its tool-strip bar.

- **Measurement setup names can repeat**, so the setup navigator can't be driven by name. Read the
  `of {N}` label, press "Move first", then press "Move next" at most N times, comparing as you go.

- **Setting names are matched in three passes:** exact name, then category + name, then partial. If
  more than one survives, fail and ask for the category rather than guessing — several TrueTest
  analyses reuse names like `Maximum Image Pixels` across categories.

---

## 2. Control names that have to go back into `TrueTestUiNames` and `commands.json`

```csharp
public string AnalysisWindow { get; set; } = "AnalysisSetupForm";
public string AnalysisSaveButton { get; set; } = "SaveBtn";
public string AnalysisCancelButton { get; set; } = "CancelBtn";
public string SetupSelector { get; set; } = "PatternComboBox";
public string SetupFirstButton { get; set; } = "Move first";
public string SetupNextButton { get; set; } = "Move next";
public string CalibrationPanel { get; set; } = "CalibrationsPanel";
public string CalibrationBar { get; set; } = "CalibrationSettingsToolStrip";
```

```json
"analysisWindow": "AnalysisSetupForm",
"analysisSaveButton": "SaveBtn",
"analysisCancelButton": "CancelBtn",
"setupSelector": "PatternComboBox",
"setupFirstButton": "Move first",
"setupNextButton": "Move next",
"calibrationPanel": "CalibrationsPanel",
"calibrationBar": "CalibrationSettingsToolStrip",
```

---

## 3. `TrueTestAutomation` — the removed members

The record the settings came back in:

```csharp
public sealed record TrueTestSetting(string Category, string Name, string Value)
{
    public string Path => Category.Length > 0 ? $"{Category} › {Name}" : Name;
}
```

### Reading one step's analysis settings

```csharp
public static (TrueTestStep Step, List<TrueTestSetting> Settings) ReadStepSettings(int stepNumber)
{
    var step = AnalysedStep(stepNumber);
    var dialog = OpenStepEditor(stepNumber);
    try
    {
        return (step, ReadSettings(dialog).Select(s => s.Setting).ToList());
    }
    finally
    {
        CloseDialog(dialog, Names.AnalysisCancelButton);
    }
}
```

### Matching a setting by name

```csharp
/// <summary>Finds settings matching <paramref name="query"/>: exact name first, then "category name", then partial.</summary>
public static List<TrueTestSetting> Match(IEnumerable<TrueTestSetting> settings, string query)
{
    var q = Norm(query);
    var list = settings.ToList();
    var exact = list.Where(s => Norm(s.Name) == q).ToList();
    if (exact.Count > 0) return exact;
    var withCategory = list.Where(s => Norm(s.Category + s.Name) == q || Norm(s.Name + s.Category) == q ||
                                       (q.Contains(Norm(s.Name)) && q.Contains(Norm(s.Category)))).ToList();
    if (withCategory.Count > 0) return withCategory;
    return list.Where(s => Norm(s.Name).Contains(q)).ToList();
}
```

### Changing a setting and saving it

```csharp
/// <summary>Changes one analysis setting of a step and presses Save in the Analysis Setup window.</summary>
public static (TrueTestStep Step, TrueTestSetting Before, TrueTestSetting After) SetStepSetting(
    int stepNumber, string query, string newValue)
{
    var step = AnalysedStep(stepNumber);
    var dialog = OpenStepEditor(stepNumber);
    bool saved = false;
    TrueTestSetting before;
    try
    {
        var all = ReadSettings(dialog);
        var matches = Match(all.Select(a => a.Setting), query);
        if (matches.Count == 0)
            throw new InvalidOperationException($"Step {stepNumber} has no setting called \"{query}\".");
        if (matches.Count > 1)
            throw new InvalidOperationException(
                $"\"{query}\" matches several settings: {string.Join("; ", matches.Select(m => m.Path))}. " +
                "Include the category, e.g. \"Dark Particles Maximum Image Pixels\".");

        AutomationElement element;
        (before, element) = all.First(a => a.Setting == matches[0]);
        if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var vp) || ((ValuePattern)vp).Current.IsReadOnly)
            throw new InvalidOperationException($"{before.Path} can't be typed into (it may need its own editor window).");

        // SetValue can pop up TrueTest's own "invalid value" message box, which would block the call.
        var set = Task.Run(() => ((ValuePattern)vp).SetValue(newValue));
        bool finished;
        try { finished = set.Wait(TimeSpan.FromSeconds(5)); }
        catch (AggregateException)
        {
            throw new InvalidOperationException($"TrueTest didn't accept \"{newValue}\" for {before.Path} (it's {before.Value}). Nothing was changed.");
        }
        if (!finished)
            throw new InvalidOperationException(
                $"TrueTest didn't accept \"{newValue}\" for {before.Path} and is showing a message. Please check TrueTest.");

        // SetValue only types into the grid's in-place editor; Enter commits it, as it would for a person.
        var editor = dialog.FindFirst(TreeScope.Descendants, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
            new PropertyCondition(AutomationElement.NameProperty, element.Current.Name)));
        if (editor != null && editor.Current.NativeWindowHandle != 0)
        {
            PostKey(editor, VK_RETURN);
            Thread.Sleep(400);
        }
        if (FindMessageBox(dialog) is { } box)
            throw new InvalidOperationException(
                $"TrueTest rejected \"{newValue}\" for {before.Path}: {box}. Please close its message and the Analysis Setup window.");

        CloseDialog(dialog, Names.AnalysisSaveButton);
        saved = true;
    }
    finally
    {
        if (!saved) CloseDialog(dialog, Names.AnalysisCancelButton);
    }

    // Reopen the step to prove the new value really stuck.
    var after = ReadStepSettings(stepNumber).Settings.FirstOrDefault(s => s.Category == before.Category && s.Name == before.Name)
                ?? throw new InvalidOperationException($"Couldn't re-read {before.Path} after saving.");
    if (Norm(after.Value) != Norm(newValue))
        throw new InvalidOperationException(
            $"TrueTest kept {before.Path} at {after.Value} (it was {before.Value}); \"{newValue}\" wasn't accepted.");
    return (step, before, after);
}
```

### Spotting TrueTest's own message box

```csharp
/// <summary>Text of a message box TrueTest has open over <paramref name="owner"/>, if any.</summary>
static string? FindMessageBox(AutomationElement owner)
{
    var box = owner.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "#32770"));
    if (box == null) return null;
    var text = box.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text))
        .Cast<AutomationElement>().Select(t => t.Current.Name).Where(t => t.Length > 0);
    return string.Join(" ", text);
}
```

### Calibration settings for a measurement setup

```csharp
public static (string Setup, List<TrueTestSetting> Calibrations) ReadCalibration(string? setupName)
{
    var main = MainWindow();
    if (setupName != null) SelectSetup(main, setupName);

    var panel = ById(main, Names.CalibrationPanel);
    var combos = CalibrationCombos(panel);
    if (combos.Count == 0)
    {
        // Category is collapsed; its expand arrow isn't exposed to UIA, so click the arrow at the bar's right end.
        var bar = ById(main, Names.CalibrationBar);
        var r = bar.Current.BoundingRectangle;
        PostClick(bar, r.Right - 12, r.Top + r.Height / 2);
        combos = WaitFor(() => CalibrationCombos(panel) is { Count: > 0 } c ? c : null, TimeSpan.FromSeconds(5))
                 ?? throw new InvalidOperationException("Couldn't open the Calibration Settings category.");
    }

    var setup = ValueOf(ById(main, Names.SetupSelector));
    var list = combos.Select(c => new TrueTestSetting("Calibration Settings",
        c.Current.Name.Trim().TrimEnd(':'), Clean(ValueOf(c)))).ToList();
    return (setup, list);
}

static List<AutomationElement> CalibrationCombos(AutomationElement panel) =>
    panel.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox))
        .Cast<AutomationElement>().Where(c => !c.Current.IsOffscreen).ToList();
```

### Walking the measurement-setup navigator

```csharp
/// <summary>Steps the measurement-setup navigator until "Select by Description" shows <paramref name="name"/>.</summary>
static void SelectSetup(AutomationElement main, string name)
{
    var combo = ById(main, Names.SetupSelector);
    if (Norm(ValueOf(combo)) == Norm(name)) return;

    AutomationElement NavButton(string label) =>
        main.FindFirst(TreeScope.Descendants, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
            new PropertyCondition(AutomationElement.NameProperty, label)))
        ?? throw new InvalidOperationException($"Couldn't find TrueTest's \"{label}\" button.");

    // The navigator shows "of {N}"; names can repeat, so walk exactly N setups.
    var countText = main.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text))
        .Cast<AutomationElement>().Select(t => t.Current.Name).FirstOrDefault(n => n.StartsWith("of {"));
    int count = countText != null && int.TryParse(countText.Trim('o', 'f', ' ', '{', '}'), out var n) ? n : 200;

    var seen = new List<string>();
    Invoke(NavButton(Names.SetupFirstButton));
    for (int i = 0; i < count; i++)
    {
        Thread.Sleep(150);
        var current = ValueOf(combo);
        if (Norm(current) == Norm(name)) return;
        seen.Add(current);
        Invoke(NavButton(Names.SetupNextButton));
    }
    throw new InvalidOperationException(
        $"There's no measurement setup called \"{name}\". Setups: {string.Join(", ", seen.Distinct())}.");
}
```

### Opening, finding and closing the Analysis Setup window

```csharp
static TrueTestStep AnalysedStep(int stepNumber)
{
    var steps = GetSteps();
    var step = steps.FirstOrDefault(s => s.Number == stepNumber)
               ?? throw new InvalidOperationException($"There's no step {stepNumber}; the sequence has {steps.Count} steps.");
    if (step.Analysis.Length == 0)
        throw new InvalidOperationException($"Step {stepNumber} ({step.Setup}) has no analysis, so there are no analysis settings.");
    return step;
}

static AutomationElement OpenStepEditor(int stepNumber)
{
    var main = MainWindow();
    if (FindAnalysisDialog(main) != null)
        throw new InvalidOperationException("An Analysis Setup window is already open in TrueTest. Please close it first.");
    if (!main.Current.IsEnabled)
        throw new InvalidOperationException("TrueTest is busy or has another window open. Please finish that first.");

    var grid = ById(main, Names.SequenceGrid);
    var header = grid.FindFirst(TreeScope.Descendants, new AndCondition(
                     new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Header),
                     new PropertyCondition(AutomationElement.NameProperty, $"Row {stepNumber - 1}")))
                 ?? throw new InvalidOperationException($"Couldn't find step {stepNumber} in TrueTest's sequence.");
    var r = header.Current.BoundingRectangle;
    if (r.IsEmpty || header.Current.IsOffscreen)
        throw new InvalidOperationException($"Step {stepNumber} is scrolled out of view in TrueTest. Please scroll to it.");

    // Click the row header (it can only select the row), End to move to the Edit column, then Space,
    // which a DataGridView button cell treats as a click. Clicking the Edit cell itself is unreliable
    // with posted messages and could open the editor twice.
    PostClick(grid, r.Left + r.Width / 2, r.Top + r.Height / 2);
    Thread.Sleep(200);
    PostKey(grid, VK_END);
    Thread.Sleep(100);
    PostKey(grid, VK_SPACE);
    return WaitFor(() => FindAnalysisDialog(main), WindowTimeout)
           ?? throw new InvalidOperationException($"The Analysis Setup window for step {stepNumber} didn't open.");
}

static AutomationElement? FindAnalysisDialog(AutomationElement main)
{
    // A closed dialog can linger hidden until TrueTest disposes it, so only count visible ones.
    var id = new PropertyCondition(AutomationElement.AutomationIdProperty, Names.AnalysisWindow);
    var candidates = main.FindAll(TreeScope.Children, id).Cast<AutomationElement>().Concat(
        AutomationElement.RootElement.FindAll(TreeScope.Children, new AndCondition(
            new PropertyCondition(AutomationElement.ProcessIdProperty, main.Current.ProcessId), id)).Cast<AutomationElement>());
    return candidates.FirstOrDefault(IsVisible);
}

static bool IsVisible(AutomationElement window)
{
    try
    {
        var hwnd = new IntPtr(window.Current.NativeWindowHandle);
        return hwnd != IntPtr.Zero ? IsWindowVisible(hwnd) : !window.Current.IsOffscreen;
    }
    catch (ElementNotAvailableException) { return false; }
}

static void CloseDialog(AutomationElement dialog, string buttonId)
{
    try
    {
        var button = ById(dialog, buttonId);
        var main = MainWindow();
        // BM_CLICK and UIA Invoke (which sends BM_CLICK too) are ignored unless the dialog is the active
        // window. A WinForms button really fires on the BN_CLICKED notification its parent receives, so post
        // that: it works whether or not TrueTest has focus. The first click can be swallowed while the
        // settings grid finishes an edit, so retry a few times.
        var hwnd = new IntPtr(button.Current.NativeWindowHandle);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            if (hwnd != IntPtr.Zero)
                PostMessage(GetParent(hwnd), WM_COMMAND, (IntPtr)(GetDlgCtrlID(hwnd) & 0xFFFF), hwnd); // BN_CLICKED = 0
            else
                FireAndForget(() => Invoke(button));
            if (WaitFor(() => FindAnalysisDialog(main) == null ? "" : null, TimeSpan.FromSeconds(3)) != null) return;
            if (FindMessageBox(dialog) != null) return; // leave TrueTest's question for the user
        }
    }
    catch (ElementNotAvailableException) { /* already closed */ }
}
```

### Reading the settings grid (categories expand on demand)

```csharp
static List<(TrueTestSetting Setting, AutomationElement Element)> ReadSettings(AutomationElement dialog)
{
    var table = dialog.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table))
                ?? throw new InvalidOperationException("Couldn't find the settings list in the Analysis Setup window.");
    var dataItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem);

    var result = new List<(TrueTestSetting, AutomationElement)>();
    foreach (AutomationElement top in table.FindAll(TreeScope.Children, dataItem))
    {
        if (top.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var ep) && !top.TryGetCurrentPattern(ValuePattern.Pattern, out _) &&
            ((ExpandCollapsePattern)ep).Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
            ((ExpandCollapsePattern)ep).Expand();

        var children = top.FindAll(TreeScope.Children, dataItem).Cast<AutomationElement>().ToList();
        if (children.Count == 0) // alphabetical view: properties are top-level
            result.Add((new TrueTestSetting("", top.Current.Name.Trim(), ValueOf(top)), top));
        foreach (var child in children)
            result.Add((new TrueTestSetting(top.Current.Name.Trim(), child.Current.Name.Trim(), ValueOf(child)), child));
    }
    return result;
}
```

### Plumbing these needed

```csharp
static readonly TimeSpan WindowTimeout = TimeSpan.FromSeconds(15);

static string Norm(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

static void Invoke(AutomationElement e) => ((InvokePattern)e.GetCurrentPattern(InvokePattern.Pattern)).Invoke();

static T? WaitFor<T>(Func<T?> probe, TimeSpan timeout) where T : class
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        try
        {
            if (probe() is { } found) return found;
        }
        catch (ElementNotAvailableException) { }
        Thread.Sleep(250);
    }
    return null;
}

/// <summary>Clicks a point (screen pixels) inside a control by posting mouse messages to it; doesn't move the real mouse.</summary>
static void PostClick(AutomationElement control, double screenX, double screenY)
{
    var hwnd = new IntPtr(control.Current.NativeWindowHandle);
    if (hwnd == IntPtr.Zero) throw new InvalidOperationException("That TrueTest control has no window to click.");
    var pt = new POINT { X = (int)screenX, Y = (int)screenY };
    ScreenToClient(hwnd, ref pt);
    var lParam = (IntPtr)((pt.Y << 16) | (pt.X & 0xFFFF));
    PostMessage(hwnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
    PostMessage(hwnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, lParam);
    PostMessage(hwnd, WM_LBUTTONUP, IntPtr.Zero, lParam);
}

static void PostKey(AutomationElement control, int virtualKey)
{
    var hwnd = new IntPtr(control.Current.NativeWindowHandle);
    PostMessage(hwnd, WM_KEYDOWN, (IntPtr)virtualKey, (IntPtr)1);
    PostMessage(hwnd, WM_KEYUP, (IntPtr)virtualKey, unchecked((IntPtr)(int)0xC0000001));
}

const uint WM_MOUSEMOVE = 0x0200, WM_LBUTTONDOWN = 0x0201, WM_LBUTTONUP = 0x0202;
const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_COMMAND = 0x0111;
const int MK_LBUTTON = 0x0001, VK_SPACE = 0x20, VK_RETURN = 0x0D, VK_END = 0x23;

[StructLayout(LayoutKind.Sequential)]
struct POINT { public int X, Y; }

[DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
[DllImport("user32.dll")] static extern IntPtr GetParent(IntPtr hWnd);
[DllImport("user32.dll")] static extern int GetDlgCtrlID(IntPtr hWnd);
[DllImport("user32.dll")] static extern bool ScreenToClient(IntPtr hWnd, ref POINT point);
[DllImport("user32.dll")] static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
```

---

## 4. The command handlers that drove them

From `PromptWindow.TrueTest.cs`. `TrueTestAsync`, `Say`, `ExportFolder` and `_pending` are all still
in the app; `Show`, `Norm` and `WriteCsv` went with these.

```csharp
async Task ShowStepAsync(int step, string? query)
{
    var (ok, (info, settings)) = await TrueTestAsync($"Opening step {step}'s settings…", () => TrueTestAutomation.ReadStepSettings(step));
    if (!ok) return;

    if (string.IsNullOrWhiteSpace(query))
    {
        var categories = settings.GroupBy(s => s.Category).Select(g => $"• {g.Key} ({g.Count()})");
        Say($"Step {step}: {info.Setup} → {info.Analysis}, {settings.Count} settings:\n{string.Join("\n", categories)}\n" +
            $"Ask e.g. \"show step {step} dark particles\" or \"export step {step}\".");
        return;
    }

    // A category name shows the whole category; otherwise match setting names.
    var byCategory = settings.Where(s => Norm(s.Category).Contains(Norm(query))).ToList();
    var found = byCategory.Count > 0 ? byCategory : TrueTestAutomation.Match(settings, query);
    Say(found.Count == 0
        ? $"Step {step} has no setting or category matching \"{query}\"."
        : string.Join("\n", found.Take(40).Select(s => $"{s.Path} = {Show(s.Value)}")) +
          (found.Count > 40 ? $"\n…and {found.Count - 40} more. Try \"export step {step}\"." : ""));
}

async Task SetSettingAsync(int step, string query, string value)
{
    value = value.Trim().Trim('"', '\'');
    var (ok, (info, before, after)) = await TrueTestAsync($"Changing step {step}…",
        () => TrueTestAutomation.SetStepSetting(step, query, value));
    if (ok)
        Say($"✅ Step {step} ({info.Analysis}): {before.Path} {Show(before.Value)} → {Show(after.Value)}.\n" +
            "This is only in TrueTest so far. Type \"save sequence\" to save it to the file.");
}

async Task CompareStepsAsync(int a, int b)
{
    var (ok, (sa, sb)) = await TrueTestAsync($"Reading steps {a} and {b}…", () =>
        (TrueTestAutomation.ReadStepSettings(a), TrueTestAutomation.ReadStepSettings(b)));
    if (!ok) return;

    var left = sa.Settings.ToDictionary(s => s.Path, s => s.Value);
    var right = sb.Settings.ToDictionary(s => s.Path, s => s.Value);
    var lines = left.Keys.Union(right.Keys)
        .Select(k => (k, l: left.GetValueOrDefault(k), r: right.GetValueOrDefault(k)))
        .Where(x => x.l != x.r)
        .Select(x => $"{x.k}: {Show(x.l ?? "(none)")} ↔ {Show(x.r ?? "(none)")}")
        .ToList();

    var header = $"Step {a} ({sa.Step.Analysis}) vs step {b} ({sb.Step.Analysis}): ";
    if (lines.Count == 0) { Say(header + "all settings are the same."); return; }

    var file = WriteCsv($"compare step {a} vs {b}", new[] { "Setting", $"Step {a}", $"Step {b}" },
        left.Keys.Union(right.Keys).Select(k => new[] { k, left.GetValueOrDefault(k) ?? "", right.GetValueOrDefault(k) ?? "" }));
    Say(header + $"{lines.Count} differ:\n" + string.Join("\n", lines.Take(30)) +
        (lines.Count > 30 ? $"\n…and {lines.Count - 30} more." : "") + $"\nFull comparison: {file}");
}

async Task ExportStepsAsync(int? only)
{
    var (ok, rows) = await TrueTestAsync(only is { } n ? $"Reading step {n}…" : "Reading every step with an analysis…", () =>
    {
        var steps = only is { } one
            ? new List<int> { one }
            : TrueTestAutomation.GetSteps().Where(s => s.Analysis.Length > 0).Select(s => s.Number).ToList();
        if (steps.Count == 0) throw new InvalidOperationException("No step in this sequence has an analysis.");
        return steps.SelectMany(n =>
        {
            var (info, settings) = TrueTestAutomation.ReadStepSettings(n);
            return settings.Select(s => new[] { n.ToString(), info.Setup, info.Analysis, s.Category, s.Name, s.Value });
        }).ToList();
    });
    if (!ok) return;

    var file = WriteCsv(only is { } m ? $"step {m}" : "all steps",
        new[] { "Step", "Measurement Setup", "Analysis", "Category", "Setting", "Value" }, rows);
    Say($"✅ Exported {rows.Count} settings to {file}");
}

async Task ShowCalibrationAsync(string? setup)
{
    var (ok, (name, cals)) = await TrueTestAsync(setup != null ? $"Switching to the {setup} setup…" : "Reading calibration settings…",
        () => TrueTestAutomation.ReadCalibration(setup));
    if (!ok) return;
    Say($"Calibration settings for measurement setup \"{name}\":\n" +
        string.Join("\n", cals.Select(c => $"• {c.Name}: {Show(c.Value)}")));
}

static string WriteCsv(string what, string[] header, IEnumerable<string[]> rows)
{
    Directory.CreateDirectory(ExportFolder);
    var path = Path.Combine(ExportFolder, $"TrueTest {what} {DateTime.Now:yyyy-MM-dd HHmmss}.csv");
    static string Esc(string v) => v.IndexOfAny(new[] { ',', '"', '\n' }) >= 0 ? $"\"{v.Replace("\"", "\"\"")}\"" : v;
    var sb = new StringBuilder();
    foreach (var row in rows.Prepend(header)) sb.AppendLine(string.Join(",", row.Select(Esc)));
    File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true)); // BOM so Excel reads it as UTF-8
    return path;
}

static string Show(string value) => value.Length == 0 ? "(empty)" : value;

static string Norm(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
```

---

## 5. The command entries

```json
{
  "action": "showStep",
  "patterns": [
    "(what is|what's|what are) [the] {setting} (in|of|for) step {step:int}",
    "(show|read|get) step {step:int} [settings] [{setting}]"
  ],
  "help": "show step 3 [dark particles] / what is DarkThreshold in step 3: read analysis settings"
},
{
  "action": "setSetting",
  "patterns": [ "(set|change) step {step:int} {setting} to {value}" ],
  "help": "set step 3 DarkThreshold to 6: change an analysis setting"
},
{
  "action": "compareSteps",
  "patterns": [ "compare step {a:int} (with|and|to|vs) [step] {b:int}" ],
  "help": "compare step 3 with step 5: settings that differ"
},
{
  "action": "exportSteps",
  "patterns": [ "export [all] steps", "export step {step:int}" ],
  "help": "export step 3 / export steps: save settings to CSV"
},
{
  "action": "calibration",
  "patterns": [ "[show] (calibration|calibrations) [settings] [(for|of) {setup}]" ],
  "help": "calibration [for Black]: a measurement setup's calibration settings"
},
{
  "action": "macro",
  "patterns": [ "check step {step:int}" ],
  "lines": [ "what is DarkThreshold in step {step}", "what is BrightThreshold in step {step}", "what is MaxNumberOfDefects in step {step}" ],
  "help": "check step 3: show its key particle thresholds"
}
```

And the registrations in `PromptWindow.BuildActions`:

```csharp
["showStep"] = new((_, m) => ShowStepAsync(Int(m, "step"), Text(m, "setting"))),
["setSetting"] = new((_, m) => SetSettingAsync(Int(m, "step"), Required(m, "setting"), Required(m, "value")),
    ConfirmByDefault: true,
    Describe: (_, m) => $"Change step {Int(m, "step")}'s \"{Required(m, "setting")}\" to {Required(m, "value")}"),
["compareSteps"] = new((_, m) => CompareStepsAsync(Int(m, "a"), Int(m, "b"))),
["exportSteps"] = new((_, m) => ExportStepsAsync(Text(m, "step") is { } s ? int.Parse(s) : null)),
["calibration"] = new((_, m) => ShowCalibrationAsync(Text(m, "setup"))),
```
