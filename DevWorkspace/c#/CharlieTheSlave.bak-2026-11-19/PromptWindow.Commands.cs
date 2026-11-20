using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace CharlieTheSlave;

/// <summary>
/// Runs the commands defined in commands.json. The file maps phrases to the built-in actions below,
/// so phrasing, help text and shortcuts can change without rebuilding the app.
/// </summary>
public partial class PromptWindow
{
    /// <summary>A built-in action that commands.json can refer to by name.</summary>
    sealed record ActionDef(
        Func<CommandEntry, Match, Task> Run,
        bool AllowedWhileBusy = false,
        bool ConfirmByDefault = false,
        Func<CommandEntry, Match, string>? Describe = null);

    const int MaxMacroDepth = 5;

    readonly Dictionary<string, ActionDef> _actions;
    CommandConfig _config = new();
    FileSystemWatcher? _watcher;
    DispatcherTimer? _reloadTimer;

    Dictionary<string, ActionDef> BuildActions()
    {
        static Task Done(Action a) { a(); return Task.CompletedTask; }

        return new(StringComparer.OrdinalIgnoreCase)
        {
            ["updateTrueTest"] = new((_, m) => UpdateTrueTestAsync(Text(m, "version"), Text(m, "components"))),
            ["showComponents"] = new((_, _) => Done(ShowComponents), AllowedWhileBusy: true),
            ["listSteps"] = new((_, _) => ListStepsAsync()),
            ["openInstallers"] = new((_, _) => Done(OpenInstallerFolder), AllowedWhileBusy: true),
            ["exportPoi"] = new((_, m) => ExportPoiAsync(Text(m, "set"))),
            ["saveSequence"] = new((_, _) => SaveSequenceAsync(), ConfirmByDefault: true,
                Describe: (_, _) => "Save the current sequence in TrueTest (this overwrites the sequence file)"),
            ["macro"] = new(RunMacroAsync,
                Describe: (c, m) => "Run:\n" + string.Join("\n", MacroLines(c, m).Select(l => "• " + l))),
            ["open"] = new((c, m) => Done(() => Open(c, m)),
                Describe: (c, m) => $"Open {Fill(c.Text ?? "", m)}"),
            ["close"] = new((c, m) => CloseProgramAsync(Fill(c.Text ?? "", m)), ConfirmByDefault: true,
                Describe: (c, m) => $"Close {Fill(c.Text ?? "", m)} (it may ask you to save first)"),
            ["say"] = new((c, m) => Done(() => Say(Fill(c.Text ?? "", m))), AllowedWhileBusy: true),
            ["help"] = new((_, _) => Done(() => Say(BuildHelp())), AllowedWhileBusy: true),
            ["stop"] = new((_, _) => Done(Stop), AllowedWhileBusy: true),
            ["clear"] = new((_, _) => Done(ClearChat), AllowedWhileBusy: true),
            ["model"] = new((_, m) => Done(() => SetModel(Text(m, "name"))), AllowedWhileBusy: true),
            ["exit"] = new((_, _) => Done(() => Application.Current.Shutdown()), AllowedWhileBusy: true),
        };
    }

    /// <summary>Runs <paramref name="text"/> if it's a command. Returns false if nothing matched.</summary>
    async Task<bool> RunCommandLineAsync(string text, bool confirmed, int depth)
    {
        var input = CommandConfig.NormalizeInput(text);
        if (depth == 0 && await RunFileCommandAsync(input)) return true;

        foreach (var entry in _config.Commands)
        {
            if (entry.Match(input) is not { } match) continue;
            await RunEntryAsync(entry, match, confirmed, depth);
            return true;
        }
        return false;
    }

    async Task RunEntryAsync(CommandEntry entry, Match match, bool confirmed, int depth)
    {
        if (!_actions.TryGetValue(entry.Action, out var action))
        {
            Say($"⚠ commands.json uses an unknown action \"{entry.Action}\". Type \"help\" to see the list at the top of the file.");
            return;
        }
        if (IsBusy && !action.AllowedWhileBusy)
        {
            Say("I'm still busy with the installer. Type \"stop\" to stop auto-clicking.");
            return;
        }

        try
        {
            bool needsConfirm = entry.Confirm ?? (action.ConfirmByDefault || (entry.Action.Equals("macro", StringComparison.OrdinalIgnoreCase) &&
                                                                              MacroLines(entry, match).Any(LineNeedsConfirm)));
            if (needsConfirm && !confirmed)
            {
                var description = action.Describe?.Invoke(entry, match) ?? $"Run \"{match.Value.Trim()}\"";
                _pending = (description, () => RunEntryAsync(entry, match, confirmed: true, depth));
                Say($"{description}? (yes / no)");
                return;
            }

            _macroDepth = depth;
            await action.Run(entry, match);
        }
        catch (CommandArgumentException ex)
        {
            Say($"⚠ {ex.Message}");
        }
    }

    int _macroDepth;

    async Task RunMacroAsync(CommandEntry entry, Match match)
    {
        int depth = _macroDepth + 1;
        if (depth > MaxMacroDepth)
        {
            Say("⚠ That macro calls itself too many times; stopping.");
            return;
        }
        foreach (var line in MacroLines(entry, match))
        {
            Say($"▶ {line}");
            // The macro as a whole was confirmed (if needed), so its lines run without asking again.
            if (!await RunCommandLineAsync(line, confirmed: true, depth))
                Say($"⚠ \"{line}\" isn't a command, so I skipped it.");
        }
    }

    static IEnumerable<string> MacroLines(CommandEntry entry, Match match) =>
        (entry.Lines ?? new()).Select(l => Fill(l, match));

    bool LineNeedsConfirm(string line)
    {
        var input = CommandConfig.NormalizeInput(line);
        var entry = _config.Commands.FirstOrDefault(c => c.Match(input) != null);
        return entry != null && (entry.Confirm ?? (_actions.TryGetValue(entry.Action, out var a) && a.ConfirmByDefault));
    }

    void Open(CommandEntry entry, Match match)
    {
        var target = Environment.ExpandEnvironmentVariables(Fill(entry.Text ?? "", match));
        if (target.Length == 0) throw new CommandArgumentException("This \"open\" command has no \"text\" to open.");
        try
        {
            Process.Start(new ProcessStartInfo(target)
            {
                UseShellExecute = true,
                Arguments = Fill(entry.Arguments ?? "", match),
            });
            Say($"Opened {target}");
        }
        catch (Exception ex)
        {
            Say($"❌ Couldn't open {target}: {ex.Message}");
        }
    }

    /// <summary>Asks a program to close as if its ✕ was clicked, so it can still ask about unsaved work.</summary>
    async Task CloseProgramAsync(string name)
    {
        name = Path.GetFileNameWithoutExtension(name.Trim());
        if (name.Length == 0) throw new CommandArgumentException("This \"close\" command has no \"text\" (program name) to close.");

        var processes = Process.GetProcessesByName(name);
        if (processes.Length == 0)
        {
            Say($"{name} isn't running.");
            return;
        }

        Say($"Closing {name}…");
        foreach (var p in processes) p.CloseMainWindow();
        bool closed = await Task.Run(() => processes.All(p => p.WaitForExit(15_000)));
        Say(closed
            ? $"✅ {name} is closed."
            : $"{name} is still open. It may be asking you something (for example to save); please check it.");
    }

    // ---------- commands that manage commands.json itself (always available) ----------

    async Task<bool> RunFileCommandAsync(string input)
    {
        if (Regex.IsMatch(input, @"^(edit|open)\s+commands?(\s+file)?$", RegexOptions.IgnoreCase))
        {
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{CommandConfig.FilePath}\"") { UseShellExecute = true });
            Say($"Opened {CommandConfig.FilePath}. Save it and I'll pick up the changes automatically.");
            return true;
        }
        if (Regex.IsMatch(input, @"^reload\s+commands?$", RegexOptions.IgnoreCase))
        {
            LoadCommands(announce: true);
            return true;
        }
        if (Regex.IsMatch(input, @"^reset\s+commands?$", RegexOptions.IgnoreCase))
        {
            _pending = ("reset commands", () =>
            {
                var backup = CommandConfig.ResetToDefaults();
                Say($"Restored the default commands. Your old file is saved as {backup}.");
                LoadCommands(announce: true);
                return Task.CompletedTask;
            });
            Say("Replace commands.json with the defaults? Your current file will be kept as a backup. (yes / no)");
            return true;
        }
        await Task.CompletedTask;
        return false;
    }

    void LoadCommands(bool announce)
    {
        try
        {
            _config = CommandConfig.Load();
            if (announce) Say($"✅ Loaded {_config.Commands.Count} commands from commands.json.");
        }
        catch (Exception ex)
        {
            var problem = ex is System.Text.Json.JsonException j
                ? $"line {j.LineNumber + 1}: {j.Message.Split(" Path:")[0]}"
                : ex.Message;
            if (_config.Commands.Count == 0)
            {
                _config = CommandConfig.Parse(CommandConfig.DefaultJson());
                Say($"⚠ commands.json has a problem ({problem}). Using the built-in defaults until it's fixed.");
            }
            else
            {
                Say($"⚠ commands.json has a problem ({problem}). Keeping the previous commands until it's fixed.");
            }
        }
        TrueTestAutomation.Names = _config.TrueTest;
        _ollama.SystemPrompt = _config.SystemPrompt.Replace("{help}", BuildHelp());
    }

    void WatchCommandsFile()
    {
        // Editors often save in several steps, so wait for things to settle before reloading.
        _reloadTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _reloadTimer.Tick += (_, _) =>
        {
            _reloadTimer.Stop();
            LoadCommands(announce: true);
        };

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(CommandConfig.FilePath)!, Path.GetFileName(CommandConfig.FilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };
        void Changed(object? s, FileSystemEventArgs e) => Dispatcher.BeginInvoke(() =>
        {
            _reloadTimer.Stop();
            _reloadTimer.Start();
        });
        _watcher.Changed += Changed;
        _watcher.Created += Changed;
        _watcher.Renamed += (s, e) => Changed(s, e);
    }

    string BuildHelp() =>
        "Things I can do:\n" +
        string.Join("\n", _config.Commands.Where(c => !string.IsNullOrWhiteSpace(c.Help)).Select(c => "• " + c.Help)) +
        "\n• edit commands / reload commands / reset commands: change what I can do\n" +
        (_ollamaOfflineAt == null
            ? "Anything else goes to your local Ollama model.\n"
            : $"Anything else would go to Ollama, but I can't reach it at {_ollama.BaseUrl}.\n") +
        "Esc hides this window. Drag the icon to move it.";

    // ---------- reading pattern captures ----------

    static string? Text(Match m, string name) =>
        m.Groups[name] is { Success: true } g && g.Value.Trim().Length > 0 ? g.Value.Trim() : null;

    static string Required(Match m, string name) =>
        Text(m, name) ?? throw new CommandArgumentException($"This command's pattern needs a {{{name}}} part.");

    static int Int(Match m, string name) =>
        int.TryParse(Required(m, name), out var n) ? n : throw new CommandArgumentException($"\"{Text(m, name)}\" isn't a number.");

    /// <summary>Replaces "{name}" in <paramref name="template"/> with what the pattern captured.</summary>
    static string Fill(string template, Match m) =>
        Regex.Replace(template, @"\{(\w+)\}", x => Text(m, x.Groups[1].Value) ?? x.Value);

    sealed class CommandArgumentException(string message) : Exception(message);
}
