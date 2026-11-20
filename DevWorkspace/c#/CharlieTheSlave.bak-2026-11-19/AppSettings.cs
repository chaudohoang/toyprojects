using System.IO;
using System.Text.Json;

namespace CharlieTheSlave;

public sealed class AppSettings
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CharlieTheSlave", "settings.json");

    // Where the settings lived when the app was called DeskAssistant; read once so a rename keeps them.
    static readonly string LegacyFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskAssistant", "settings.json");

    public static AppSettings Current { get; } = Load();

    public double? IconLeft { get; set; }
    public double? IconTop { get; set; }
    public string? LastInstallerFolder { get; set; }

    /// <summary>Components to keep ticked when updating TrueTest; null keeps the installer's defaults.</summary>
    public List<string>? TrueTestComponents { get; set; } = new() { "RVS SQL CE DB Converter" };

    /// <summary>Local Ollama server used for anything that isn't a built-in command.</summary>
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "gemma2:2b";

    static AppSettings Load()
    {
        try
        {
            var path = File.Exists(FilePath) ? FilePath : File.Exists(LegacyFilePath) ? LegacyFilePath : null;
            if (path != null)
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new();
        }
        catch { /* corrupt settings: start fresh */ }
        return new();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* not worth crashing over */ }
    }
}
