using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace CharlieTheSlave;

public sealed record TrueTestStep(int Number, bool Selected, string Setup, string Analysis);

/// <summary>
/// TrueTest is there but not usable yet — still starting up, or the window we need isn't open. Worth
/// its own type so the command layer can tell the user to wait and try again rather than reporting a
/// missing control.
/// </summary>
public sealed class TrueTestNotReadyException(string message) : Exception(message);

/// <summary>
/// Reads the running TrueTest's sequence through UI Automation, using the control names TrueTest
/// exposes (SequenceDataGridView and friends). Call everything here from a background thread.
/// <see cref="TrueTestPoi"/> builds on <see cref="MainWindow"/> and <see cref="Names"/> for the
/// measurement database and its points of interest.
/// </summary>
public static class TrueTestAutomation
{
    /// <summary>TrueTest's control names, from commands.json ("trueTest" section).</summary>
    public static TrueTestUiNames Names { get; set; } = new();

    // ---------- Sequence steps ----------

    public static List<TrueTestStep> GetSteps()
    {
        var grid = ById(MainWindow(), Names.SequenceGrid);
        var steps = new List<TrueTestStep>();
        foreach (AutomationElement row in grid.FindAll(TreeScope.Children, Condition.TrueCondition))
        {
            var rowName = row.Current.Name;
            if (!rowName.StartsWith("Row ") || !int.TryParse(rowName.Substring(4), out int index)) continue;

            var cells = row.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>().ToList();
            AutomationElement? Cell(string column) => cells.FirstOrDefault(c => c.Current.Name == $"{column} {rowName}");

            bool selected = Cell("Select") is { } sel && sel.TryGetCurrentPattern(TogglePattern.Pattern, out var tp) &&
                            ((TogglePattern)tp).Current.ToggleState == ToggleState.On;
            steps.Add(new(index + 1, selected, Clean(ValueOf(Cell("Measurement Setup"))), Clean(ValueOf(Cell("Analysis")))));
        }
        return steps;
    }

    public static void ClickSaveSequence()
    {
        var button = MainWindow().FindFirst(TreeScope.Descendants, new AndCondition(
                         new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                         new PropertyCondition(AutomationElement.NameProperty, Names.SaveSequenceButton)))
                     ?? throw new InvalidOperationException("Couldn't find TrueTest's Save Sequence button.");
        // A tool-strip button has no window of its own; invoke it without waiting in case it asks something.
        FireAndForget(() => ((InvokePattern)button.GetCurrentPattern(InvokePattern.Pattern)).Invoke());
    }

    // ---------- Plumbing ----------

    /// <summary>Whether TrueTest is running at all — cheaper than looking for its window, and a clearer
    /// thing to tell the user about than a missing control.</summary>
    public static bool IsRunning => Process.GetProcessesByName(Names.ProcessName).Length > 0;

    /// <summary>
    /// TrueTest's main window. Looked up by AutomationId first, then — because that search goes through
    /// the UI Automation tree and can come back empty for a moment while TrueTest is busy, or because a
    /// different build may name the form differently — by the process's own main window handle. Tried a
    /// few times over a second before concluding TrueTest isn't ready.
    /// </summary>
    public static AutomationElement MainWindow()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            foreach (var p in Process.GetProcessesByName(Names.ProcessName))
            {
                try
                {
                    var byId = AutomationElement.RootElement.FindFirst(TreeScope.Children, new AndCondition(
                        new PropertyCondition(AutomationElement.ProcessIdProperty, p.Id),
                        new PropertyCondition(AutomationElement.AutomationIdProperty, Names.MainWindow)));
                    if (byId != null) return byId;

                    // MainWindowHandle is whichever of the process's windows Windows considers main,
                    // which can be the Define Points of Interest window while that is open — so don't
                    // accept that one as TrueTest's main window.
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        var byHandle = AutomationElement.FromHandle(p.MainWindowHandle);
                        var name = byHandle.Current.Name ?? "";
                        if (!name.StartsWith(Names.PoiWindowTitle, StringComparison.OrdinalIgnoreCase))
                            return byHandle;
                    }
                }
                catch (Exception ex) when (ex is ElementNotAvailableException or COMException or InvalidOperationException)
                {
                    // that process is going away or busy; try the next one, then round again
                }
            }
            if (!IsRunning) break;
            Thread.Sleep(200);
        }

        throw new TrueTestNotReadyException(IsRunning
            ? "TrueTest is still starting up, or it's too busy to answer. Please wait for it to settle and try again."
            : "TrueTest isn't running. Please start it and try again.");
    }

    static AutomationElement ById(AutomationElement root, string id) =>
        root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, id))
        ?? throw new InvalidOperationException($"Couldn't find \"{id}\" in TrueTest.");

    static string ValueOf(AutomationElement? e)
    {
        if (e == null) return "";
        try
        {
            return e.TryGetCurrentPattern(ValuePattern.Pattern, out var p) ? ((ValuePattern)p).Current.Value ?? "" : "";
        }
        catch (ElementNotAvailableException) { return ""; }
    }

    static string Clean(string value) => value is "(null)" or "(None)" ? "" : value.Trim();

    static void FireAndForget(Action action) => _ = Task.Run(() =>
    {
        try { action(); } catch { /* the window changed under us; nothing to do */ }
    });
}
