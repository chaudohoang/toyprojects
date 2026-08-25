namespace FtpUpload;

/// <summary>
/// Watches the jobs folder for work written by TrueTest.
///
/// Job file: YYYYMMDD_jobs.txt, append-only, one line per file to upload:
///     PID|FileName|LocalPath|RemotePath
/// RemotePath may be omitted, in which case it is derived as
///     {RemoteBaseFolder}/{PID}/{FileName}
///
/// TrueTest only ever appends; this program never rewrites the job file, so the two
/// processes cannot clobber each other.
/// </summary>
public sealed class JobIntake(Config cfg, UploadEngine engine)
{
    private long _offsetLines;
    private string _currentFile = "";

    public event Action<string>? Logged;

    public void Poll()
    {
        var path = cfg.JobsPath(Clock.Now);
        if (path != _currentFile) { _currentFile = path; _offsetLines = 0; }
        if (!File.Exists(path)) return;

        var lines = SafeFile.ReadLines(path);
        if (lines.Length <= _offsetLines) return;

        var fresh = lines.Skip((int)_offsetLines).ToList();
        _offsetLines = lines.Length;

        var parsed = new List<JobFile>();
        var panelPids = new HashSet<string>();
        foreach (var line in fresh)
        {
            var jl = JobsLine.Parse(line);
            if (jl is null) continue;

            var remote = !string.IsNullOrWhiteSpace(jl.RemotePath)
                ? jl.RemotePath
                : $"{cfg.RemoteBaseFolder.TrimEnd('/')}/{jl.Pid}/{jl.FileName}";

            // Panel line: restore the panel's manifest metadata onto its Job (idempotent — on a
            // fresh run PanelIntake already did this; on RESTART this is the sole restore path).
            if (jl.IsPanel)
            {
                engine.RegisterPanel(jl.Pid, Clock.Today, sourceFolder: Path.GetDirectoryName(jl.LocalPath) ?? "",
                    channelIndex: jl.ChannelIndex, uploadIndexPath: jl.UploadIndexPath, uploadHostPath: jl.UploadHostPath,
                    indexSrc: jl.IndexSrc, hostSrc: jl.HostSrc, totalFileCount: 0);
                panelPids.Add(jl.Pid);
            }

            parsed.Add(new JobFile { Pid = jl.Pid, FileName = jl.FileName, LocalPath = jl.LocalPath,
                                     RemotePath = remote, IsManifest = jl.IsManifest });
        }

        if (parsed.Count > 0)
        {
            engine.AddFiles(parsed);
            foreach (var pid in panelPids) engine.SeedPanelManifest(pid);   // create-or-resume (idempotent)
            Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] intake: +{parsed.Count} file(s)");
        }
    }
}

/// <summary>
/// Commands from TrueTest, via an append-only commands.txt that this program drains.
/// Keeping it a file (rather than a pipe) means a command issued while the program is
/// restarting is not lost.
///
///   RESULT|PID                 -> write snapshot log + preempt in favour of PID
///   FORCE|PID|FileName         -> jump that file to the front of the queue
///   DELETE|PID                 -> drop the whole job
///   STOP                       -> intentional shutdown (watchdog stands down)
///
/// The UI does NOT use this channel — it calls the engine directly, in-process.
/// </summary>
public sealed class CommandChannel(Config cfg, UploadEngine engine)
{
    /// <summary>Set by a STOP command — an intentional shutdown that the watchdog must not undo.</summary>
    public bool StopRequested { get; private set; }

    public event Action<string>? Logged;

    public void Poll()
    {
        foreach (var line in SafeFile.DrainLines(cfg.CommandPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split('|');
            var verb = p[0].Trim().ToUpperInvariant();

            switch (verb)
            {
                case "RESULT" when p.Length >= 2:
                    Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] RESULT timing for {p[1]} -> snapshot + preempt");
                    engine.OnResultTiming(p[1].Trim());
                    break;

                case "FORCE" when p.Length >= 3:
                    engine.ForceUpload(p[1].Trim(), p[2].Trim());
                    break;

                case "DELETE" when p.Length >= 2:
                    engine.DeletePanel(p[1].Trim());
                    break;

                case "STOP":
                    Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] STOP requested — shutting down, watchdog will not restart");
                    StopRequested = true;
                    break;

                default:
                    Logged?.Invoke($"[{DateTime.Now:HH:mm:ss}] ignored command: {line}");
                    break;
            }
        }
    }
}
