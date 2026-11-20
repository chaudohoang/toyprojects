namespace TrayChat;

/// <summary>Plain-text log in a Log folder next to TrayChat.exe, for diagnosing startup and tray problems.</summary>
static class Log
{
    public static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "Log", "log.txt");

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{Environment.ProcessId}] {message}{Environment.NewLine}");
        }
        catch (Exception) { }
    }
}
