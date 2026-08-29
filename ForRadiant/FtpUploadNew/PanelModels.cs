using System.Text;
using System.Text.RegularExpressions;

namespace FtpUpload;

/// <summary>
/// The parsed contents of a "{PID}_{DateTime}.panel" handoff file written by TrueTest.
/// This is the ENTIRE contract between TrueTest and this program: everything else
/// (which files to upload, their destinations, the manifests) is derived from these
/// fields plus the source folder on disk and the recipe.
///
/// Format is Key=Value, one per line:
///   Model, EQPID, PID, DateTime, UploadIndexPath, UploadHostPath, SourceFolder, ChannelIndex
/// </summary>
public sealed class PanelFile
{
    public string Model { get; init; } = "";
    public string EQPID { get; init; } = "";
    public string Pid { get; init; } = "";
    public string DateTime { get; init; } = "";
    public string UploadIndexPath { get; init; } = "";
    public string UploadHostPath { get; init; } = "";
    public string SourceFolder { get; init; } = "";
    public string ChannelIndex { get; init; } = "";

    /// <summary>The .panel file this was read from (so intake can delete/park it after).</summary>
    public string PanelFilePath { get; init; } = "";

    /// <summary>
    /// The LOCAL file PID — the token that appears in the panel's own filenames (e.g. the "AAA"
    /// in NyPucData_AAA_1st.hex), which is DIFFERENT from the server <see cref="Pid"/>. It is the
    /// source-folder leaf name with the trailing "_{14-digit DateTime}" stripped
    /// ({localPID}_{localDateTime}). Used to resolve @PID@ in the recipe.
    /// </summary>
    public string LocalPid
    {
        get
        {
            var leaf = Path.GetFileName(SourceFolder.TrimEnd('\\', '/'));
            return Regex.Replace(leaf, @"_\d{14}$", "");
        }
    }

    /// <summary>
    /// A panel is only ingestable once TrueTest has written SourceFolder (phase 2). The atomic
    /// rename to *.panel is the real "ready" signal, but this guards against any partial content.
    /// </summary>
    public bool IsReady =>
        !string.IsNullOrWhiteSpace(Pid) &&
        !string.IsNullOrWhiteSpace(DateTime) &&
        !string.IsNullOrWhiteSpace(SourceFolder) &&
        !string.IsNullOrWhiteSpace(UploadIndexPath) &&
        !string.IsNullOrWhiteSpace(UploadHostPath);
}

/// <summary>Reads a .panel file into a PanelFile. Tolerant of blank lines and stray whitespace.</summary>
public static class PanelParser
{
    public static PanelFile? TryParse(string path)
    {
        Dictionary<string, string> kv;
        try
        {
            kv = new(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;
                var eq = line.IndexOf('=');
                if (eq <= 0) continue;
                kv[line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }
        }
        catch
        {
            return null;   // mid-write / locked — try again next poll
        }

        string G(string k) => kv.TryGetValue(k, out var v) ? v : "";

        return new PanelFile
        {
            Model = G("Model"),
            EQPID = G("EQPID"),
            Pid = G("PID"),
            DateTime = G("DateTime"),
            UploadIndexPath = G("UploadIndexPath"),
            UploadHostPath = G("UploadHostPath"),
            SourceFolder = G("SourceFolder"),
            ChannelIndex = G("ChannelIndex"),
            PanelFilePath = path
        };
    }
}

/// <summary>
/// Derives every server-side path from a PanelFile, using exactly the same rules the old
/// TrueTest code used, so the destinations are byte-for-byte identical.
///
///   IMAGE: {ServerRoot}/POCB/IMAGE/{MM}/{DD}/{EQPID}/{Model}/{PID}/{DateTime}/{fileName}  (.tif)
///   HEX  : {ServerRoot}/POCB/HEX/{MM}/{DD}/{EQPID}/{Model}/{PID}/{DateTime}/{fileName}    (all others)
///   INDEX: taken straight from the panel (UploadIndexPath) — already includes the PID hash
///   HOST : taken straight from the panel (UploadHostPath)
///
/// The IMAGE-vs-HEX split matches the old TrueTest routing: .tif images go under POCB/IMAGE,
/// the .hex / .txt data files go under POCB/HEX; everything else in the path is identical.
/// ServerRoot is recovered from the panel's own UploadIndexPath (everything before "POCB/"),
/// so the roots can never drift from the index/host root.
/// </summary>
public static class PathDerivation
{
    /// <summary>Everything in UploadIndexPath before "POCB/", e.g. "data1h1/HN_DATA/".</summary>
    public static string ServerRootOf(PanelFile p)
    {
        var s = p.UploadIndexPath.Replace('\\', '/');
        var i = s.IndexOf("POCB/", StringComparison.OrdinalIgnoreCase);
        return i >= 0 ? s[..i] : "";
    }

    /// <summary>
    /// The remote destination for one data file. Images (.tif) go under POCB/IMAGE, every other
    /// file under POCB/HEX — the two share the same {MM}/{DD}/{EQPID}/{Model}/{PID}/{DateTime}/ tail.
    /// </summary>
    public static string DestFor(PanelFile p, string fileName)
    {
        var section = fileName.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ? "IMAGE" : "HEX";
        var root = ServerRootOf(p);
        var mm = p.DateTime.Length >= 6 ? p.DateTime.Substring(4, 2) : "";
        var dd = p.DateTime.Length >= 8 ? p.DateTime.Substring(6, 2) : "";
        return $"{root}POCB/{section}/{mm}/{dd}/{p.EQPID}/{p.Model}/{p.Pid}/{p.DateTime}/{fileName}";
    }

    /// <summary>Local index manifest this program creates in the source folder: {PID}.idx</summary>
    public static string IndexSrc(PanelFile p) => Path.Combine(p.SourceFolder, p.Pid + ".idx");

    /// <summary>Local host manifest this program creates: {PID}_{DateTime}.txt</summary>
    public static string HostSrc(PanelFile p) => Path.Combine(p.SourceFolder, p.Pid + "_" + p.DateTime + ".txt");
}

/// <summary>
/// The upload recipe: a list of filename patterns that decide which files in a panel's source
/// folder get uploaded. Used purely as a FILTER — folder ∩ recipe is both the file list and the
/// total count. It is NOT a checklist: a panel simply uploads whatever matching files it has.
///
/// Pattern syntax (one per line, '#' comments):
///   • '@PID@'  is replaced with the panel's PID before matching
///   • '*' matches any run of characters, '?' matches one (case-insensitive)
///   • a bare name with no wildcard matches that exact filename
///   • a line starting with '!' is an EXCLUDE: a file is uploaded only if it matches at least one
///     normal (allow) pattern AND matches no '!' pattern. e.g. "!*_ori*" skips any *_ori* file.
/// </summary>
public sealed class Recipe
{
    private readonly List<string> _allow;
    private readonly List<string> _exclude;

    private Recipe(List<string> allow, List<string> exclude) { _allow = allow; _exclude = exclude; }

    /// <summary>The allow patterns. Empty here = no recipe (treated as misconfigured by the caller).</summary>
    public IReadOnlyList<string> Patterns => _allow;

    public static Recipe Load(string path)
    {
        var allow = new List<string>();
        var exclude = new List<string>();
        try
        {
            if (File.Exists(path))
                foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith('#')) continue;
                    if (line.StartsWith('!'))
                    {
                        var ex = line[1..].Trim();
                        if (ex.Length > 0) exclude.Add(ex);
                    }
                    else allow.Add(line);
                }
        }
        catch { /* missing/locked recipe -> empty; caller decides how to treat that */ }
        return new Recipe(allow, exclude);
    }

    /// <summary>True if fileName matches an ALLOW pattern and NO EXCLUDE ('!') pattern
    /// (with @PID@ resolved to pid).</summary>
    public bool Matches(string fileName, string pid)
    {
        var allowed = false;
        foreach (var pat in _allow)
            if (GlobToRegex(pat.Replace("@PID@", pid)).IsMatch(fileName)) { allowed = true; break; }
        if (!allowed) return false;

        foreach (var pat in _exclude)
            if (GlobToRegex(pat.Replace("@PID@", pid)).IsMatch(fileName)) return false;   // excluded
        return true;
    }

    private static Regex GlobToRegex(string glob)
    {
        var sb = new StringBuilder("^");
        foreach (var c in glob)
            sb.Append(c switch
            {
                '*' => ".*",
                '?' => ".",
                _ => Regex.Escape(c.ToString())
            });
        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.IgnoreCase);
    }
}
