using System.Collections.Concurrent;

namespace FtpUpload;

/// <summary>
/// Watches QueueFolder for "*.panel" handoff files from TrueTest and turns each into a panel of
/// jobs for the engine. One .panel = one panel (PID); TrueTest writes it via temp-then-rename so a
/// visible .panel is always complete.
///
/// Per panel: parse -> confirm ready + source folder present -> list SourceFolder ∩ recipe
/// (excluding the manifest files themselves) -> derive each HEX destination -> register the panel
/// metadata -> add the files -> delete or archive the .panel.
///
/// Runs alongside the legacy JobIntake (jobs.txt) so the demo/test path keeps working during cutover.
/// </summary>
public sealed class PanelIntake(Config cfg, UploadEngine engine)
{
    // PIDs we've already ingested this run, so a .panel that lingers (archive mode) isn't re-added.
    private readonly ConcurrentDictionary<string, byte> _seen = new();

    public event Action<string>? Logged;
    private void Log(string msg) => Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

    public void Poll()
    {
        string[] panels;
        try
        {
            if (!Directory.Exists(cfg.QueueFullPath)) return;
            panels = Directory.GetFiles(cfg.QueueFullPath, "*.panel");
        }
        catch { return; }

        foreach (var path in panels)
        {
            try { Ingest(path); }
            catch (Exception ex) { Log($"panel intake error on {Path.GetFileName(path)}: {ex.Message}"); }
        }
    }

    private void Ingest(string path)
    {
        var panel = PanelParser.TryParse(path);
        if (panel is null) return;                       // locked / mid-write — retry next poll

        if (!panel.IsReady)
        {
            // Phase-1 panel (no SourceFolder yet) or incomplete — leave it; phase 2 completes it.
            return;
        }

        var key = panel.Pid + "|" + panel.DateTime;
        // Already ingested this session: this is a redundant re-drop. The original is already in the
        // backup folder, so just remove the duplicate .panel. Guarded so a locked file can't break us.
        if (_seen.ContainsKey(key))
        {
            try { File.Delete(path); } catch (Exception ex) { Log($"could not delete duplicate panel {Path.GetFileName(path)}: {ex.Message}"); }
            return;
        }

        if (!Directory.Exists(panel.SourceFolder))
        {
            Log($"panel {panel.Pid}: source folder missing, will retry: {panel.SourceFolder}");
            return;
        }

        var recipe = Recipe.Load(cfg.RecipeFullPath);
        if (recipe.Patterns.Count == 0)
        {
            // No recipe loaded at all — almost always a misconfigured/missing RecipePath, NOT a
            // genuinely empty panel. Do NOT consume the panel over a config problem: leave it in
            // place (retried next poll) so fixing the recipe lets it ingest.
            Log($"panel {panel.Pid}: recipe is EMPTY ({cfg.RecipeFullPath}) — leaving panel in place; fix the recipe and it will ingest");
            return;
        }

        // The manifest files live in the same folder — never treat them as data files.
        var indexName = Path.GetFileName(PathDerivation.IndexSrc(panel));
        var hostName = Path.GetFileName(PathDerivation.HostSrc(panel));

        var dataFiles = new List<JobFile>();
        foreach (var full in Directory.GetFiles(panel.SourceFolder))
        {
            var name = Path.GetFileName(full);
            if (name.Equals(indexName, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(hostName, StringComparison.OrdinalIgnoreCase)) continue;
            if (!recipe.Matches(name, panel.LocalPid)) continue;

            dataFiles.Add(new JobFile
            {
                Pid = panel.Pid,
                FileName = name,
                LocalPath = full,
                RemotePath = PathDerivation.DestFor(panel, name)
            });
        }

        if (dataFiles.Count == 0)
        {
            // Recipe loaded fine but nothing in the folder matched it. This is unusual (a real
            // panel always has uploadable files), so treat it as suspect: move the panel to a
            // "rejected" subfolder for inspection rather than deleting it, and warn loudly.
            _seen[key] = 1;
            var rejected = Path.Combine(cfg.QueueFullPath, "rejected");
            Log($"panel {panel.Pid}: WARNING no files match the recipe in {panel.SourceFolder} — moved to {rejected} (check the recipe / source folder)");
            MoveTo(path, rejected);
            return;
        }

        // Register the panel metadata first (so the Job carries manifest info), then add its files.
        engine.RegisterPanel(
            pid: panel.Pid,
            day: Clock.Today,
            sourceFolder: panel.SourceFolder,
            channelIndex: panel.ChannelIndex,
            uploadIndexPath: panel.UploadIndexPath,
            uploadHostPath: panel.UploadHostPath,
            indexSrc: PathDerivation.IndexSrc(panel),
            hostSrc: PathDerivation.HostSrc(panel),
            totalFileCount: dataFiles.Count);

        engine.AddFiles(dataFiles);
        engine.SeedPanelManifest(panel.Pid);   // create-or-resume {PID}.idx / {PID}_{DateTime}.txt

        // Persist to the day's jobs file so a restart re-loads the panel (with its manifest
        // metadata) and the NG-retry console can reconstruct these files if any end up FAILED.
        var jobsPath = cfg.JobsPath(Clock.Now);
        foreach (var f in dataFiles)
        {
            var jl = new JobsLine
            {
                IsPanel = true,
                Pid = f.Pid, FileName = f.FileName, LocalPath = f.LocalPath, RemotePath = f.RemotePath,
                IndexSrc = PathDerivation.IndexSrc(panel), HostSrc = PathDerivation.HostSrc(panel),
                UploadIndexPath = panel.UploadIndexPath, UploadHostPath = panel.UploadHostPath,
                ChannelIndex = panel.ChannelIndex
            };
            SafeFile.Append(jobsPath, jl.ToLine());
        }

        _seen[key] = 1;
        Log($"panel {panel.Pid}: ingested {dataFiles.Count} file(s) from {panel.SourceFolder} (ch {panel.ChannelIndex})");
        BackupOrArchive(path);
    }

    /// <summary>
    /// Keep a successfully ingested .panel as a backup under a per-day subfolder of the backup
    /// folder (by default "Backup Jobs" alongside the incoming panels).
    /// </summary>
    private void BackupOrArchive(string path)
    {
        var dated = Path.Combine(cfg.PanelBackupFullPath, Clock.Now.ToString("yyyyMMdd"));
        Log($"panel backup -> {Path.Combine(dated, Path.GetFileName(path))}");
        MoveTo(path, dated);
    }

    /// <summary>Move a .panel into a folder, creating it and overwriting any same-named file.</summary>
    private void MoveTo(string path, string destFolder)
    {
        try
        {
            Directory.CreateDirectory(destFolder);
            var dest = Path.Combine(destFolder, Path.GetFileName(path));
            if (File.Exists(dest)) File.Delete(dest);
            File.Move(path, dest);
        }
        catch (Exception ex)
        {
            Log($"could not move panel file {Path.GetFileName(path)} to {destFolder}: {ex.Message}");
        }
    }
}
