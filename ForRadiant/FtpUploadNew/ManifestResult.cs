namespace FtpUpload;

/// <summary>
/// One manifest upload — which of the two it was, the name it went out as, and whether it landed.
///
/// The early send used to be logged as a single "index+host manifests sent" line, which could name
/// only one remote file and claimed both had gone even when one failed. Reporting each manifest
/// separately lets the log say exactly what reached the server and under what name.
/// </summary>
public readonly record struct ManifestSend(string Kind, string RemoteName, bool Ok, int Files, string Host,
                                          string Error = "");

/// <summary>
/// Outcome of a finalize attempt, reported PER MANIFEST.
///
/// The index and host manifests are separate uploads and can land separately, so their statuses
/// must be recorded separately. Writing one shared status for the pair stamped a manifest that had
/// succeeded with its partner's failure: on LGD's 2026-09-02 log, 97 of 124 host manifests that
/// WinSCP proves were uploaded still read FAILED in the rawlog, and index files showed
/// "SUCCEEDED" followed by "FAILED" on an attempt where they were never re-sent.
/// <para>
/// <see cref="Uploaded"/> separates "I sent them just now" from "they were already there". Without
/// it, the caller logs a manifest send on every call — including the early exit that does no work —
/// and one panel's 3 real transfers produced 13 rawlog rows and 12 ng-retry rows.
/// </para>
/// </summary>
public readonly record struct FinalizeResult(bool IdxOk, bool HostOk, string Host, bool Uploaded = false,
                                            string HostName = "", bool IdxSentNow = false, bool HostSentNow = false,
                                            int IdxFiles = 0, int HostFiles = 0)
{
    /// <summary>The panel is finalized only when BOTH manifests are on the server.</summary>
    public bool Ok => IdxOk && HostOk;

    /// <summary>
    /// What THIS call actually put on the server, for the log line.
    ///
    /// "index + host sent" was printed regardless, so a call that only had the index left
    /// to send still claimed both — and the host name was blank, because no host went out. Saying
    /// what was really sent makes the blank name self-explaining instead of looking like a defect.
    /// </summary>
    public string SentDescription =>
        IdxSentNow && HostSentNow ? "index + host sent"
        : HostSentNow ? "host sent (index was already there)"
        : IdxSentNow ? "index sent (host was already there)"
        : "nothing sent";

    /// <summary>
    /// One line per manifest this call actually sent, each naming its own remote file.
    ///
    /// The index name is fixed and the host name varies with stamping, so a single combined line
    /// could only ever name one of the two. Splitting keeps every file on the server traceable to
    /// a log line of its own.
    /// </summary>
    public IEnumerable<string> SentLines(string idxRemoteName)
    {
        // The count matters as much as the name: it says how much of the panel that file describes.
        // The index always carries the whole checklist; the host carries its increment when
        // per-upload naming is on.
        if (IdxSentNow) yield return $"final index sent -> {Host} as {idxRemoteName}, {IdxFiles} file{(IdxFiles == 1 ? "" : "s")}";
        if (HostSentNow) yield return $"final host sent -> {Host} as {HostName}, {HostFiles} file{(HostFiles == 1 ? "" : "s")}";
    }
}
