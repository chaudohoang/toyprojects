namespace FtpUpload;

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
                                            string HostName = "", bool IdxSentNow = false, bool HostSentNow = false)
{
    /// <summary>The panel is finalized only when BOTH manifests are on the server.</summary>
    public bool Ok => IdxOk && HostOk;

    /// <summary>
    /// What THIS call actually put on the server, for the log line.
    ///
    /// "index + host manifests sent" was printed regardless, so a call that only had the index left
    /// to send still claimed both — and the host name was blank, because no host went out. Saying
    /// what was really sent makes the blank name self-explaining instead of looking like a defect.
    /// </summary>
    public string SentDescription =>
        IdxSentNow && HostSentNow ? "index + host manifests sent"
        : HostSentNow ? "host manifest sent (index was already there)"
        : IdxSentNow ? "index manifest sent (host was already there)"
        : "nothing sent";
}
