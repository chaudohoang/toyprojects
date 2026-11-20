using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CharlieTheSlave;

/// <summary>
/// The user-editable command file (commands.json next to CharlieTheSlave.exe): which phrases trigger which
/// actions, help text, the AI's instructions and TrueTest's control names. The actions themselves are code.
/// It's created from the built-in defaults on first run and never overwritten by a rebuild.
/// </summary>
public sealed class CommandConfig
{
    public static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "commands.json");

    // Where earlier versions kept the file; moved next to the exe on first run.
    static readonly string LegacyFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CharlieTheSlave", "commands.json");

    /// <summary>Instructions for the Ollama model; "{help}" is replaced with the generated command list.</summary>
    public string SystemPrompt { get; set; } = "";
    public string InstallerFolder { get; set; } = "";

    /// <summary>Which points-of-interest set "export poi" uses when the command doesn't name one.</summary>
    public string DefaultPoiSet { get; set; } = "CA410 5 points";

    public TrueTestUiNames TrueTest { get; set; } = new();
    public List<CommandEntry> Commands { get; set; } = new();

    static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Loads the user's file, creating it from the built-in defaults the first time.</summary>
    public static CommandConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            if (File.Exists(LegacyFilePath)) File.Move(LegacyFilePath, FilePath);
            else File.WriteAllText(FilePath, DefaultJson());
        }
        return Parse(File.ReadAllText(FilePath));
    }

    /// <summary>Replaces the user's file with the defaults, keeping a backup. Returns the backup path.</summary>
    public static string ResetToDefaults()
    {
        var backup = Path.ChangeExtension(FilePath, $".{DateTime.Now:yyyyMMdd-HHmmss}.bak.json");
        if (File.Exists(FilePath)) File.Copy(FilePath, backup);
        File.WriteAllText(FilePath, DefaultJson());
        return backup;
    }

    public static CommandConfig Parse(string json)
    {
        var config = JsonSerializer.Deserialize<CommandConfig>(json, Json)
                     ?? throw new InvalidDataException("The file is empty.");
        foreach (var c in config.Commands) c.Compile();
        return config;
    }

    public static string DefaultJson()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CharlieTheSlave.commands.default.json")
                           ?? throw new InvalidOperationException("Built-in commands.default.json is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Lowercases nothing, but trims, collapses spaces and drops trailing ?.! so phrases match naturally.</summary>
    public static string NormalizeInput(string text) =>
        Regex.Replace(text.Trim(), @"\s+", " ").TrimEnd('?', '.', '!', ' ');
}

public sealed class CommandEntry
{
    /// <summary>Built-in action to run (see the "actions" list at the top of commands.json).</summary>
    public string Action { get; set; } = "";

    /// <summary>Phrases in the simple pattern syntax, e.g. "(set|change) step {step:int} {setting} to {value}".</summary>
    public List<string> Patterns { get; set; } = new();

    /// <summary>Raw .NET regular expressions, for anything the simple syntax can't express.</summary>
    public List<string>? Regex { get; set; }

    /// <summary>One line for the help list (and for the AI, so it can tell you what to type).</summary>
    public string? Help { get; set; }

    /// <summary>Ask "yes / no" first. Defaults to true for actions that change something.</summary>
    public bool? Confirm { get; set; }

    /// <summary>For "macro": command lines to run in order. "{name}" inserts a value captured by the pattern.</summary>
    public List<string>? Lines { get; set; }

    /// <summary>For "say": the reply text. For "open": a program, file, folder or web address.</summary>
    public string? Text { get; set; }

    /// <summary>For "open": command-line arguments.</summary>
    public string? Arguments { get; set; }

    [JsonIgnore] public List<Regex> Compiled { get; private set; } = new();

    public void Compile()
    {
        try
        {
            Compiled = Patterns.Select(CommandPattern.ToRegex)
                .Concat((Regex ?? new()).Select(r => new Regex(r, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Command \"{Action}\" has a bad pattern: {ex.Message}", ex);
        }
        if (Compiled.Count == 0) throw new InvalidDataException($"Command \"{Action}\" has no patterns.");
    }

    public Match? Match(string input) => Compiled.Select(r => r.Match(input)).FirstOrDefault(m => m.Success);
}

/// <summary>TrueTest's internal control names, in case a TrueTest update renames something.</summary>
public sealed class TrueTestUiNames
{
    public string ProcessName { get; set; } = "TrueTest";
    public string MainWindow { get; set; } = "TrueTestMDIForm";
    public string SequenceGrid { get; set; } = "SequenceDataGridView";
    public string SaveSequenceButton { get; set; } = "Save Sequence";

    // The measurement database and its points of interest (see TrueTestPoi).
    public string MeasurementListWindow { get; set; } = "Measurement List";

    /// <summary>The MDI area; a child window bigger than this is clipped to it.</summary>
    public string MdiClientClass { get; set; } = "WindowsForms10.MDICLIENT.app.0.13965fa_r6_ad1";

    /// <summary>A column only the measurement list has, so its grid can be told from the sequence grid.</summary>
    public string MeasurementDateColumn { get; set; } = "Measurement Date / Time";

    /// <summary>The column whose cell is right-clicked to raise a measurement's menu.</summary>
    public string MeasurementSetupColumn { get; set; } = "Measurement Setup";
    public string PoiMenuItem { get; set; } = "Define Points of Interest...";
    public string PoiWindowTitle { get; set; } = "Define Points of Interest";

    /// <summary>A button that only the POI set tool strip has, so the set list can be told from the other combos.</summary>
    public string PoiSetMarkerButton { get; set; } = "Save Points of Interest Set As";

    public string PoiPointColumn { get; set; } = "Point Description";
    public string PoiLuminanceColumn { get; set; } = "Luminance";
    public string PoiCxColumn { get; set; } = "Color Cx";
    public string PoiCyColumn { get; set; } = "Color Cy";
}

/// <summary>
/// Turns the friendly phrase syntax into a regular expression:
///   word           a word (any case)               "true test" also matches "truetest"
///   (a|b c)        one of the alternatives
///   [a|b]          optional (alternatives allowed)
///   {name}         captures some text   {name:int} a number   {name:version} 158, 1.9.226.158 or latest
/// The whole message must match; spaces are flexible and trailing ?.! are ignored.
/// </summary>
public static class CommandPattern
{
    public static Regex ToRegex(string pattern)
    {
        int pos = 0;
        var body = ParseAlternatives(pattern, ref pos, endChar: null);
        if (pos < pattern.Length) throw new FormatException($"unexpected '{pattern[pos]}' at position {pos + 1}");
        return new Regex($@"^\s*{body}\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    static string ParseAlternatives(string p, ref int pos, char? endChar)
    {
        var alternatives = new List<string> { ParseSequence(p, ref pos, endChar) };
        while (pos < p.Length && p[pos] == '|')
        {
            pos++;
            alternatives.Add(ParseSequence(p, ref pos, endChar));
        }
        return alternatives.Count == 1 ? alternatives[0] : "(?:" + string.Join("|", alternatives) + ")";
    }

    static string ParseSequence(string p, ref int pos, char? endChar)
    {
        // Plain words are kept apart from other items so that word boundaries can be added only where a word
        // meets a capture or the edge of the phrase: "true test" then also matches "truetest", while "to"
        // still can't match inside "Photo".
        var items = new List<(string Regex, bool Word)>();
        while (pos < p.Length)
        {
            char c = p[pos];
            if (c == '|' || c == endChar) break;
            if (char.IsWhiteSpace(c)) { pos++; continue; }

            if (c == '(' || c == '[')
            {
                char close = c == '(' ? ')' : ']';
                pos++;
                var inner = ParseAlternatives(p, ref pos, close);
                if (pos >= p.Length || p[pos] != close) throw new FormatException($"missing '{close}'");
                pos++;
                items.Add((c == '(' ? $"(?:{inner})" : $"(?:{inner})?", false));
            }
            else if (c == '{')
            {
                int end = p.IndexOf('}', pos);
                if (end < 0) throw new FormatException("missing '}'");
                var spec = p.Substring(pos + 1, end - pos - 1).Split(':');
                pos = end + 1;
                var name = spec[0].Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9]*$"))
                    throw new FormatException($"bad capture name \"{name}\"");
                items.Add(((spec.Length > 1 ? spec[1].Trim().ToLowerInvariant() : "text") switch
                {
                    "int" => $@"(?<{name}>\d+)",
                    "version" => $@"(?<{name}>latest|newest|\d+(?:\.\d+){{0,3}})",
                    "text" => $@"(?<{name}>.+?)",
                    var t => throw new FormatException($"unknown capture type \"{t}\" (use int, version or text)"),
                }, false));
            }
            else
            {
                var word = new StringBuilder();
                while (pos < p.Length && !char.IsWhiteSpace(p[pos]) && "()[]{}|".IndexOf(p[pos]) < 0)
                    word.Append(p[pos++]);
                items.Add((word.ToString(), true));
            }
        }

        var sb = new StringBuilder();
        for (int i = 0; i < items.Count; i++)
        {
            var (regex, isWord) = items[i];
            if (i > 0) sb.Append(@"\s*"); // flexible spacing; optional items simply match nothing
            if (!isWord)
            {
                sb.Append(regex);
                continue;
            }
            bool wordBefore = i > 0 && items[i - 1].Word;
            bool wordAfter = i < items.Count - 1 && items[i + 1].Word;
            if (!wordBefore && char.IsLetterOrDigit(regex[0])) sb.Append(@"\b");
            sb.Append(System.Text.RegularExpressions.Regex.Escape(regex));
            if (!wordAfter && char.IsLetterOrDigit(regex[regex.Length - 1])) sb.Append(@"\b");
        }
        return sb.ToString();
    }
}
