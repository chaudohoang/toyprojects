using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CharlieTheSlave;

/// <summary>Actions that read and change the running TrueTest's sequence (see <see cref="TrueTestAutomation"/>).</summary>
public partial class PromptWindow
{
    static readonly string ExportFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CharlieTheSlave", "TrueTest");

    // A change waiting for the user to answer yes/no.
    (string Description, Func<Task> Apply)? _pending;

    /// <summary>Handles a yes/no answer to a pending change. Returns true if <paramref name="text"/> was that answer.</summary>
    async Task<bool> HandlePendingAnswerAsync(string text)
    {
        if (_pending is not { } pending) return false;
        _pending = null;
        if (Regex.IsMatch(text, @"^\s*(y|yes|ok|okay|sure|do it|go|go ahead|confirm)\b", RegexOptions.IgnoreCase))
        {
            await pending.Apply();
            return true;
        }
        if (Regex.IsMatch(text, @"^\s*(n|no|nope|cancel|stop|don't)\b", RegexOptions.IgnoreCase))
        {
            Say("OK, I didn't change anything.");
            return true;
        }
        Say("(I've dropped the pending change.)");
        return false; // treat it as a new command
    }

    /// <summary>Runs TrueTest automation off the UI thread, showing the busy ring and turning errors into chat replies.</summary>
    async Task<(bool Ok, T Value)> TrueTestAsync<T>(string working, Func<T> work)
    {
        Say(working);
        BusyChanged?.Invoke(true);
        try
        {
            return (true, await Task.Run(work));
        }
        catch (Exception ex)
        {
            Say("❌ " + ex.Message);
            return (false, default!);
        }
        finally
        {
            BusyChanged?.Invoke(IsBusy);
        }
    }

    async Task ListStepsAsync()
    {
        var (ok, steps) = await TrueTestAsync("Reading the sequence…", TrueTestAutomation.GetSteps);
        if (!ok) return;
        Say(string.Join("\n", steps.Select(s =>
            $"{s.Number}. {(s.Selected ? "☑" : "☐")} {s.Setup}{(s.Analysis.Length > 0 ? " → " + s.Analysis : "")}")));
    }

            async Task SaveSequenceAsync()
    {
        if ((await TrueTestAsync("Saving…", () => { TrueTestAutomation.ClickSaveSequence(); return true; })).Ok)
            Say("Clicked Save Sequence. If TrueTest asks anything, please answer it there.");
    }

                    static string Show(string value) => value.Length == 0 ? "(empty)" : value;

    static string Norm(string s) => new(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
