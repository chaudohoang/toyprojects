Option Strict On

' =============================================================================
' FTPRecovery - one-shot recovery tool for stalled FTPUploaderVB queues.
'
' PROBLEM
'   FTPUploaderVB fires the index/host upload ONLY from the success path of
'   Upload(), when ReadAllLines(hostFile).Length = totalFileCount. A data file
'   that never reaches maxFailRetry never appends its line, so the count never
'   lands, and index/host are never uploaded. Panels stall indefinitely.
'
' WHAT THIS DOES
'   1. Scans *.txt queue files in the queue root (top level only).
'   2. Groups them by line 13 (sourceHostFile) = one panel.
'   3. Per panel: uploads every pending data file over a single reused WinSCP
'      session, appending "dest@channel" to index + host exactly like the
'      main app does.
'   4. When the host line count reaches totalFileCount, strips " - failed"
'      lines and uploads index + host itself.
'
' SAFETY
'   - Dry-run by default. Nothing is written or uploaded without -go.
'   - Deduplicates against records already present in the host file, so
'     re-running is safe and never overshoots the total.
'   - Backs up every queue file before deleting it.
'   - STOP FTPUploaderVB BEFORE RUNNING. Both processes append to the same
'     host file with no locking.
'
' Build: build.bat        Usage: FTPRecovery.exe -help
' =============================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports WinSCP

Module Program

    ' Settings - Public so the GUI front-end can drive the same engine.
    Public QueueRoot As String = ""
    Public DoExecute As Boolean = False
    Public ForceIncomplete As Boolean = False
    Public SkipMissingSource As Boolean = False
    Public Reconstruct As Boolean = False
    Public MaxRetry As Integer = 3
    ' Circuit breaker for FILE-level failures (server reachable, transfers refused).
    ' 0 disables. Connection outages are handled separately - see ServerDown below.
    Public AbortAfterConsecutiveFailures As Integer = 0
    Public ConsecutiveFailures As Integer = 0
    Public Aborted As Boolean = False

    ' Outage handling, designed for unattended runs: the tool never stops, never
    ' consumes a queue file it could not send, and never marks a good file failed
    ' just because the network was down. Once the server looks unreachable it stops
    ' paying the full retry cost on every file, and re-probes periodically so a
    ' server that comes back mid-run is picked up automatically.
    Private ServerDown As Boolean = False
    Private LastProbe As DateTime = DateTime.MinValue
    Private Const PROBE_INTERVAL_SECONDS As Integer = 30
    Private nSkippedOffline As Integer = 0
    Public OnlyPid As String = ""

    ' Optional extra log destination (the GUI hooks its textbox in here).
    Public LogSink As Action(Of String) = Nothing
    ' Cooperative cancellation for the GUI's Stop button.
    Public CancelRequested As Boolean = False

    Private LogWriter As StreamWriter = Nothing
    Private ReportWriter As StreamWriter = Nothing
    Private RunStamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")

    Private Const FAILED_SUFFIX As String = " - failed"
    Private Const MIN_LINES As Integer = 17

    ' ---- running totals -----------------------------------------------------
    Private nPanels As Integer = 0
    Private nPanelsFired As Integer = 0
    Private nPanelsIncomplete As Integer = 0
    Private nUploaded As Integer = 0
    Private nFailed As Integer = 0
    Private nMissing As Integer = 0
    Private nAlready As Integer = 0
    Private nRetried As Integer = 0
    Private nRebuilt As Integer = 0
    Private nPanelsShort As Integer = 0
    Private nPanelsForced As Integer = 0

    ' =========================================================================
    ' Data model
    ' =========================================================================

    Public Class QueueEntry
        Public FilePath As String = ""
        Public Raw As String() = New String() {}
        Public Host As String = ""
        Public User As String = ""
        Public Pass As String = ""
        Public ExePath As String = ""
        Public SessionLog As String = ""
        Public SucceedLog As String = ""
        Public FailLog As String = ""
        Public SourceFile As String = ""
        Public DestFile As String = ""
        Public OutIndexInfo As String = ""
        Public IndexSrc As String = ""
        Public IndexDst As String = ""
        Public OutHostInfo As String = ""
        Public HostSrc As String = ""
        Public HostDst As String = ""
        Public Total As Integer = 0
        Public Channel As String = ""
        Public Created As DateTime = DateTime.MinValue
        Public IsReconstructed As Boolean = False

        Public ReadOnly Property Record As String
            Get
                Return DestFile & "@" & Channel
            End Get
        End Property

        ' True when this queue file is an index/host queue rather than a data queue.
        Public ReadOnly Property IsIndexOrHostQueue As Boolean
            Get
                Return String.Equals(SourceFile, IndexSrc, StringComparison.OrdinalIgnoreCase) _
                    OrElse String.Equals(SourceFile, HostSrc, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property
    End Class

    ' Entries that count for THIS run. Rebuilt entries stay attached to the panel
    ' once created, so that switching Reconstruct off does not require a re-scan:
    ' they are simply filtered out here. Without this, unticking the option would
    ' have no effect until the next scan.
    Private Function EffectiveEntries(p As Panel) As List(Of QueueEntry)
        If Reconstruct Then Return p.Entries
        Return p.Entries.Where(Function(e) Not e.IsReconstructed).ToList()
    End Function

    Public Class PanelStatus
        Public HostNow As Integer = 0
        Public Pending As Integer = 0
        Public DoneCount As Integer = 0
        Public RetryCount As Integer = 0
        Public NewCount As Integer = 0
        Public Projected As Integer = 0
        Public CanComplete As Boolean = False
        Public Rebuilt As Integer = 0
        Public MissingSrc As Integer = 0
        Public ShortBySource As Integer = 0
        Public ShortByQueue As Integer = 0
        ' True when the panel cannot reach the total but Force is on, so it WILL be
        ' sent - short. The table must predict that rather than showing INCOMPLETE.
        Public WillForce As Boolean = False
        Public Verdict As String = ""
    End Class

    ' Read-only classification of a panel - no uploads, no writes. Used by the
    ' GUI grid and by ProcessPanel so both agree on the numbers.
    Public Function Classify(p As Panel) As PanelStatus
        Dim s As New PanelStatus()
        Dim recorded = ReadRecordStates(p.HostSrc)
        Dim eff = EffectiveEntries(p)
        s.HostNow = CountLines(p.HostSrc)
        s.Pending = eff.Count
        s.Rebuilt = eff.Where(Function(e) e.IsReconstructed).Count()
        s.DoneCount = eff.Where(Function(e) recorded.ContainsKey(Normalize(e.Record)) AndAlso Not recorded(Normalize(e.Record))).Count()
        s.RetryCount = eff.Where(Function(e) recorded.ContainsKey(Normalize(e.Record)) AndAlso recorded(Normalize(e.Record))).Count()
        s.NewCount = s.Pending - s.DoneCount - s.RetryCount

        ' How many of the outstanding files have lost their image on disk. With
        ' "skip missing source" on, those write nothing at all, so they cannot move
        ' the count towards totalFileCount - the panel genuinely cannot complete.
        ' With it off they get a " - failed" placeholder, which does advance the
        ' count, but the manifest ends up short by that many.
        s.MissingSrc = eff.Where(Function(e) _
            (Not recorded.ContainsKey(Normalize(e.Record)) OrElse recorded(Normalize(e.Record))) _
            AndAlso Not File.Exists(e.SourceFile)).Count()

        If SkipMissingSource Then s.NewCount -= s.MissingSrc
        If s.NewCount < 0 Then s.NewCount = 0

        s.Projected = s.HostNow + s.NewCount
        s.CanComplete = (s.Projected >= p.Total)

        If s.CanComplete Then
            ' Flag panels that only reach the total because entries were rebuilt -
            ' they are ready, but for a different reason than a clean panel, and
            ' the dest paths of the rebuilt files were derived rather than read.
            Dim rebuiltNote = If(s.Rebuilt > 0, " (" & s.Rebuilt.ToString() & " rebuilt)", "")
            If s.MissingSrc > 0 Then
                ' Completes, but n files can never be sent - manifest will be short.
                s.Verdict = "READY - " & (s.NewCount + s.RetryCount).ToString() &
                            " to upload, " & s.MissingSrc.ToString() &
                            " source file(s) missing -> SHORT" & rebuiltNote
            ElseIf s.NewCount = 0 AndAlso s.RetryCount = 0 Then
                s.Verdict = "READY - index/host only" & rebuiltNote
            Else
                s.Verdict = "READY - " & (s.NewCount + s.RetryCount).ToString() &
                            " to upload" & rebuiltNote
            End If
        Else
            ' Split the shortfall into its two causes so the reason is explicit.
            Dim shortfall = p.Total - s.Projected
            s.ShortBySource = If(SkipMissingSource, Math.Min(s.MissingSrc, shortfall), 0)
            s.ShortByQueue = shortfall - s.ShortBySource
            Dim parts As New List(Of String)()
            If s.ShortBySource > 0 Then parts.Add(s.ShortBySource.ToString() & " source file(s) missing")
            If s.ShortByQueue > 0 Then parts.Add(s.ShortByQueue.ToString() & " queue file(s) missing")
            If parts.Count = 0 Then parts.Add(shortfall.ToString() & " file(s) short")

            If ForceIncomplete Then
                ' Force is on, so this panel WILL be sent despite the gap. Say so,
                ' with the cost, rather than showing it as blocked.
                s.WillForce = True
                s.Verdict = "FORCED - will send, manifest short by " & shortfall.ToString() &
                            " of " & p.Total.ToString() & " (" & String.Join(", ", parts) & ")"
            Else
                s.Verdict = "INCOMPLETE - " & String.Join(", ", parts)
            End If
        End If
        Return s
    End Function

    ' Outcome of a panel that has been processed. Kept after the queue files are
    ' consumed so the UI can still show what happened - otherwise a completed
    ' panel simply vanishes from the next scan with no visible record.
    Public Class PanelOutcome
        Public PID As String = ""
        Public Total As Integer = 0
        Public Uploaded As Integer = 0
        Public Failed As Integer = 0
        Public Missing As Integer = 0
        Public Rebuilt As Integer = 0
        Public HostAfter As Integer = 0
        Public Result As String = ""
        Public Stamp As DateTime = DateTime.Now
    End Class

    ' Keyed by panel key (the host file path). Survives ResetRun so results
    ' accumulate across successive uploads; cleared by ClearOutcomes on a fresh scan.
    Public Outcomes As New Dictionary(Of String, PanelOutcome)(StringComparer.OrdinalIgnoreCase)

    Public Sub ClearOutcomes()
        Outcomes.Clear()
    End Sub

    Public Class Panel
        Public Key As String = ""
        Public PID As String = ""
        Public Entries As New List(Of QueueEntry)()
        Public Leftovers As New List(Of QueueEntry)()   ' stale index/host queues
        Public Total As Integer = 0
        Public HostSrc As String = ""
        Public HostDst As String = ""
        Public IndexSrc As String = ""
        Public IndexDst As String = ""
        Public TotalMismatch As Boolean = False
        Public RebuiltCount As Integer = 0
        Public SkippedJunk As New List(Of String)()
        Public InferredExts As New List(Of String)()
        Public ReconstructApplied As Boolean = False
    End Class

    ' =========================================================================
    ' Entry point
    ' =========================================================================

    ' The standard queue location on the TrueTest machines.
    Public Const STANDARD_QUEUE As String = "D:\Program\RVS\UploadQueue"

    ' The rule files are .txt and sit beside the exe, which may also be a queue
    ' folder. They must never be mistaken for queue files.
    Public Function IsRuleFileName(name As String) As Boolean
        Return String.Equals(name, ALLOW_FILE, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(name, DENY_FILE, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(name, KNOWN_FILE, StringComparison.OrdinalIgnoreCase)
    End Function

    ' Where to look when no path is given. The exe's own folder wins only if it
    ' actually contains QUEUE files - the rule files shipped alongside the exe are
    ' .txt too, so their presence must not make bin\ look like a queue folder.
    Public Function DefaultQueueRoot() As String
        Dim here = AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\"c)
        Try
            For Each f In Directory.EnumerateFiles(here, "*.txt", SearchOption.TopDirectoryOnly)
                If IsRuleFileName(Path.GetFileName(f)) Then Continue For
                If ParseQueueFile(f) IsNot Nothing Then Return here
            Next
        Catch
        End Try
        If Directory.Exists(STANDARD_QUEUE) Then Return STANDARD_QUEUE
        Return here
    End Function

    ' Distinct FTP hosts named by the queue files in a folder (line 0), so the UI
    ' can monitor the servers this queue is actually configured for rather than a
    ' hardcoded list. Reads a bounded number of files - one host per machine is
    ' the norm, so there is no need to crawl thousands.
    Public Function HostsInQueue(root As String, Optional maxFiles As Integer = 300) As List(Of String)
        Dim found As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If Not Directory.Exists(root) Then Return found
        Dim n As Integer = 0
        Try
            For Each f In Directory.EnumerateFiles(root, "*.txt", SearchOption.TopDirectoryOnly)
                n += 1
                If n > maxFiles Then Exit For
                If IsRuleFileName(Path.GetFileName(f)) Then Continue For
                Try
                    ' Parse properly rather than trusting line 0 - a rule file's
                    ' comment banner would otherwise be read as a hostname.
                    Dim e = ParseQueueFile(f)
                    If e Is Nothing Then Continue For
                    If e.Host <> "" AndAlso seen.Add(e.Host) Then found.Add(e.Host)
                Catch
                End Try
            Next
        Catch
        End Try
        found.Sort(StringComparer.OrdinalIgnoreCase)
        Return found
    End Function

    ' The three options are independent and can be combined, but they are applied
    ' in a fixed order and one pairing is self-contradictory. Say so plainly.
    '
    '   1. Reconstruct  - decides WHAT can be uploaded (adds rebuilt entries)
    '   2. Skip missing - decides whether a file with no image still counts
    '   3. Force        - decides whether to send a manifest that is still short
    Public Sub LogOptionNotes()
        If SkipMissingSource AndAlso ForceIncomplete Then
            Log("NOTE: 'Skip missing source' and 'Force incomplete' work against each other.")
            Log("      Skip leaves those queue files in place, so the panel will reappear on")
            Log("      the next scan even though Force has already sent its manifest.")
            Log("      Untick Skip if you want forced panels to finish cleanly.")
        End If
        If ForceIncomplete AndAlso Not Reconstruct Then
            Log("NOTE: 'Force incomplete' without 'Reconstruct from disk' may send short")
            Log("      manifests for files that are still on disk. Try Reconstruct first.")
        End If
    End Sub

    Sub Main(args As String())
        If Not ParseArgs(args) Then
            PrintUsage()
            Environment.ExitCode = 1
            Exit Sub
        End If

        If QueueRoot = "" Then
            QueueRoot = DefaultQueueRoot()
        End If
        QueueRoot = QueueRoot.TrimEnd("\"c)

        If Not Directory.Exists(QueueRoot) Then
            Console.WriteLine("Queue root not found: " & QueueRoot)
            Environment.ExitCode = 2
            Exit Sub
        End If

        OpenLogs()

        Log("FTPRecovery  run " & RunStamp)
        Log("Queue root  : " & QueueRoot)
        Log("Mode        : " & If(DoExecute, "EXECUTE", "DRY RUN (add -go to execute)"))
        Log("Max retry   : " & MaxRetry.ToString())
        Log("Force       : " & ForceIncomplete.ToString())
        If OnlyPid <> "" Then Log("Filter PID  : " & OnlyPid)
        LogOptionNotes()
        Log("")

        Try
            Dim entries = ScanQueueFiles()
            Log("Parsed " & entries.Count.ToString() & " queue file(s).")

            Dim panels = BuildPanels(entries)
            Log("Grouped into " & panels.Count.ToString() & " panel(s).")
            Log("")

            For Each p In panels
                ProcessPanel(p)
            Next
        Catch ex As Exception
            Log("FATAL: " & ex.ToString())
            Environment.ExitCode = 3
        Finally
            CloseSession()
        End Try

        Log("")
        Log("================ SUMMARY ================")
        Log("Panels scanned        : " & nPanels.ToString())
        Log("Panels index/host sent: " & nPanelsFired.ToString())
        Log("  ...of which SHORT   : " & nPanelsShort.ToString())
        If ForceIncomplete Then
            If nPanelsForced = 0 Then
                Log("  ...Force was ON but was not needed - nothing was blocked.")
            Else
                Log("  ...of which FORCED  : " & nPanelsForced.ToString())
            End If
        End If
        Log("Panels still short    : " & nPanelsIncomplete.ToString())
        Log("Files uploaded        : " & nUploaded.ToString())
        Log("Files failed          : " & nFailed.ToString())
        If nSkippedOffline > 0 Then
            Log("Left for a later run  : " & nSkippedOffline.ToString() &
                "  (server unreachable - queue files kept, nothing marked failed)")
        End If
        Log("Files source missing  : " & nMissing.ToString())
        Log("Files already recorded: " & nAlready.ToString())
        Log("Failed->clean retries : " & nRetried.ToString())
        Log("Rebuilt from disk     : " & nRebuilt.ToString())
        If Not DoExecute Then
            Log("")
            Log("DRY RUN - nothing was changed. Re-run with -go to execute.")
        End If

        CloseLogs()
    End Sub

    ' =========================================================================
    ' Arguments
    ' =========================================================================

    Private Function ParseArgs(args As String()) As Boolean
        Dim i As Integer = 0
        While i < args.Length
            Dim a = args(i).ToLowerInvariant()
            Select Case a
                Case "-go", "/go", "-execute"
                    DoExecute = True
                Case "-force", "/force"
                    ForceIncomplete = True
                Case "-skipmissing", "/skipmissing"
                    SkipMissingSource = True
                Case "-reconstruct", "/reconstruct"
                    Reconstruct = True
                Case "-help", "/?", "-?", "/help"
                    Return False
                Case "-root", "/root"
                    i += 1
                    If i >= args.Length Then Return False
                    QueueRoot = args(i)
                Case "-retry", "/retry"
                    i += 1
                    If i >= args.Length Then Return False
                    If Not Integer.TryParse(args(i), MaxRetry) Then Return False
                    If MaxRetry < 1 Then MaxRetry = 1
                Case "-pid", "/pid"
                    i += 1
                    If i >= args.Length Then Return False
                    OnlyPid = args(i)
                Case Else
                    If Not a.StartsWith("-") AndAlso Not a.StartsWith("/") AndAlso QueueRoot = "" Then
                        QueueRoot = args(i)
                    Else
                        Console.WriteLine("Unknown option: " & args(i))
                        Return False
                    End If
            End Select
            i += 1
        End While
        Return True
    End Function

    Private Sub PrintUsage()
        Console.WriteLine("FTPRecovery - drain stalled FTPUploaderVB queues and upload index/host")
        Console.WriteLine()
        Console.WriteLine("  FTPRecovery.exe [root] [options]")
        Console.WriteLine()
        Console.WriteLine("  root           Queue folder. Default: the exe's folder if it holds queue")
        Console.WriteLine("                 files, else " & STANDARD_QUEUE)
        Console.WriteLine("  -root <path>   Same as above, explicit.")
        Console.WriteLine("  -go            Actually upload and modify files. Without this it is a DRY RUN.")
        Console.WriteLine("  -force         Also fire index/host for panels that can never reach")
        Console.WriteLine("                 totalFileCount (queue files missing). Use with care.")
        Console.WriteLine("  -retry <n>     Upload attempts per file. Default 3.")
        Console.WriteLine("  -pid <text>    Only process panels whose PID contains this text.")
        Console.WriteLine("  -skipmissing   Leave queue files whose source image is gone from disk.")
        Console.WriteLine("                 Default is to mark them ' - failed' so the panel can finish.")
        Console.WriteLine()
        Console.WriteLine("  -reconstruct   Rebuild entries for files on disk that have no queue file,")
        Console.WriteLine("                 using a surviving sibling from the same panel. Only files")
        Console.WriteLine("                 matching a sibling's filename pattern are accepted.")
        Console.WriteLine()
        Console.WriteLine("  STOP FTPUploaderVB BEFORE RUNNING WITH -go.")
    End Sub

    ' =========================================================================
    ' Scan and parse
    ' =========================================================================

    ' =========================================================================
    ' Reconstruction - rebuild queue entries for files that exist on disk but
    ' have no queue file. Without this, such a file is invisible: it never
    ' uploads AND it holds the panel below totalFileCount forever.
    '
    ' 15 of the 17 queue lines are identical across every file in a panel, so a
    ' surviving sibling supplies everything except lines 7 and 8.
    '
    ' SAFETY: a candidate must match a filename pattern derived from a surviving
    ' sibling (digit runs generalised) AND have a known extension->dest folder.
    ' Anything else is skipped and flagged - never guessed, so stray thumbnails,
    ' temp files or backups can't be pushed to the customer.
    ' =========================================================================

    ' "step01_0650NIT_B048_imgY_Crop.tif" -> "^step\d+_\d+NIT_B\d+_imgY_Crop\.tif$"
    ' The source folder is named "<localPID>_<yyyyMMddHHmmss>", so the local file
    ' PID is everything before that trailing 14-digit stamp. It is the only part
    ' of a filename that legitimately differs between panels.
    Private Function LocalToken(sourcePath As String) As String
        Try
            Dim dir = Path.GetFileName(Path.GetDirectoryName(sourcePath))
            If dir Is Nothing Then Return ""
            Dim m = Regex.Match(dir, "^(.+)_\d{14}$")
            If m.Success Then Return m.Groups(1).Value
            Return dir
        Catch
            Return ""
        End Try
    End Function

    ' Canonical filename: the panel's own local PID replaced by a placeholder so
    ' names can be compared across panels. Everything else must match EXACTLY -
    ' no digit wildcards, so step99_... or ..._B999_... are rejected rather than
    ' treated as valid members of the step01_..._B048_... family.
    Private Function CanonicalName(fileName As String, token As String) As String
        If token <> "" AndAlso token.Length >= 3 Then
            Return fileName.Replace(token, "@PID@").ToLowerInvariant()
        End If
        Return fileName.ToLowerInvariant()
    End Function

    Private GlobalNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    ' Only the names genuinely LEARNED from queue files. Kept separate from
    ' GlobalNames because the cache must not absorb rule-file entries: if it did,
    ' deleting a name from allowed_filenames.txt would have no effect, since the
    ' cache would keep permitting it.
    Private LearnedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    ' Wildcard rules from the hand-maintained files. Only ever come from those two
    ' files - nothing learned is ever turned into a pattern.
    Private AllowPatterns As New List(Of Regex)()
    Private DenyPatterns As New List(Of Regex)()

    Private Function IsGlob(s As String) As Boolean
        Return s.Contains("*") OrElse s.Contains("?")
    End Function

    ' "*_gamma.hex" -> ^.*_gamma\.hex$      "step0?_*.tif" -> ^step0._.*\.tif$
    Private Function GlobToRegex(glob As String) As Regex
        Dim sb As New StringBuilder("^")
        For Each ch In glob
            Select Case ch
                Case "*"c : sb.Append(".*")
                Case "?"c : sb.Append(".")
                Case Else : sb.Append(Regex.Escape(ch.ToString()))
            End Select
        Next
        sb.Append("$")
        Return New Regex(sb.ToString(), RegexOptions.IgnoreCase Or RegexOptions.Compiled)
    End Function

    ' A candidate filename is acceptable when it is named exactly, or matches an
    ' allow wildcard - and is not excluded by name or by a deny wildcard.
    Private Function IsAllowedName(canonical As String) As Boolean
        For Each rx In DenyPatterns
            If rx.IsMatch(canonical) Then Return False
        Next
        If GlobalNames.Contains(canonical) Then Return True
        For Each rx In AllowPatterns
            If rx.IsMatch(canonical) Then Return True
        Next
        Return False
    End Function
    ' Per-panel ext->dest folder maps from the whole scan, used as donors when a
    ' panel has lost every queue file of a given extension.
    Private DonorMaps As New List(Of Dictionary(Of String, String))()
    ' Cached from the last scan so reconstruction can be applied later, per panel,
    ' without re-scanning.
    Private LastEntries As List(Of QueueEntry) = Nothing
    Private ReconstructContextReady As Boolean = False

    ' A panel may know where .tif goes but have no surviving .hex entry. Donor
    ' panels show that the two folders differ by exactly ONE path segment
    ' (.../POCB/IMAGE/... vs .../POCB/HEX/...), so apply that same positional
    ' substitution to this panel's known folder. Refuses unless the donor pair
    ' differs in exactly one segment and all three paths have equal depth, so a
    ' wrong inference is impossible rather than merely unlikely.
    Private Function InferFolder(ourKnown As String, refExt As String, wantExt As String) As String
        For Each donor In DonorMaps
            If Not donor.ContainsKey(refExt) OrElse Not donor.ContainsKey(wantExt) Then Continue For
            Dim dRef = donor(refExt).Split("/"c)
            Dim dWant = donor(wantExt).Split("/"c)
            Dim ours = ourKnown.Split("/"c)
            If dRef.Length <> dWant.Length OrElse ours.Length <> dRef.Length Then Continue For
            Dim diffAt As Integer = -1
            Dim diffs As Integer = 0
            For k = 0 To dRef.Length - 1
                If Not String.Equals(dRef(k), dWant(k), StringComparison.Ordinal) Then
                    diffs += 1
                    diffAt = k
                End If
            Next
            If diffs <> 1 Then Continue For
            ours(diffAt) = dWant(diffAt)
            Return String.Join("/", ours)
        Next
        Return ""
    End Function

    ' Learn canonical filenames from queue files under a folder. Used for the
    ' backup folders, which archive real queue files that recovery (or the main
    ' app) already retired - so their names stay in the vocabulary even after the
    ' live queue files are gone.
    Private Function LearnNamesFrom(folder As String, ByRef scanned As Integer) As Integer
        Dim added As Integer = 0
        scanned = 0
        If Not Directory.Exists(folder) Then Return 0
        Try
            For Each f In Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories)
                scanned += 1
                If scanned > 20000 Then Exit For      ' don't crawl forever
                Try
                    Dim l = File.ReadAllLines(f)
                    If l.Length < MIN_LINES Then Continue For
                    Dim src = l(7).Trim()
                    If src = "" Then Continue For
                    ' skip the index/host queue files themselves
                    If src = l(10).Trim() OrElse src = l(13).Trim() Then Continue For
                    Dim cn = CanonicalName(Path.GetFileName(src), LocalToken(src))
                    LearnedNames.Add(cn)
                    If GlobalNames.Add(cn) Then added += 1
                Catch
                End Try
            Next
        Catch
        End Try
        Return added
    End Function

    ' The vocabulary must not shrink as panels are completed. Once every queue
    ' file of a given name has been consumed, nothing in a later scan would know
    ' that name was ever legitimate - and reconstruction would start rejecting
    ' real files. So it is remembered on disk between runs.
    ' Rule files ship WITH the exe: copy the exe and these three files to a machine
    ' and it is ready to run against any queue folder. The queue folder is only an
    ' input, so nothing needs to be placed there.
    '
    ' Search order:
    '   1. beside the exe                   <- normal place, ships with the tool
    '   2. the queue folder                 <- per-folder override, if you want one
    '   3. <queue>\Log\Recovery             <- where older versions kept them
    ' known_filenames.txt is written beside the exe, falling back to the queue
    ' folder if that location is not writable (e.g. a read-only share).
    Public Const ALLOW_FILE As String = "allowed_filenames.txt"
    Public Const DENY_FILE As String = "denied_filenames.txt"
    Public Const KNOWN_FILE As String = "known_filenames.txt"

    Private Function ExeDir() As String
        Return AppDomain.CurrentDomain.BaseDirectory.TrimEnd("\"c)
    End Function

    Private Function RuleFile(name As String) As String
        Dim beside = Path.Combine(ExeDir(), name)
        If File.Exists(beside) Then Return beside
        Dim inQueue = Path.Combine(QueueRoot, name)
        If File.Exists(inQueue) Then Return inQueue
        Dim legacy = Path.Combine(LegacyRuleDir(), name)
        If File.Exists(legacy) Then Return legacy
        Return beside                       ' default location for writing
    End Function

    Private Function VocabPath() As String
        Return RuleFile(KNOWN_FILE)
    End Function

    ' A hand-maintained rule file. By default its names are ADDED to whatever is
    ' learned from real queue files, so a name that no longer appears anywhere is
    ' still allowed. Put "!strict" on a line by itself to use ONLY this file and
    ' learn nothing - useful when the learned cache has been polluted.
    Private Function AllowListPath() As String
        Return RuleFile(ALLOW_FILE)
    End Function

    ' Names here are removed after everything else, whatever their source. This is
    ' how a wrong name that got learned is permanently excluded.
    Private Function DenyListPath() As String
        Return RuleFile(DENY_FILE)
    End Function

    Private AllowListStrict As Boolean = False

    Private Function LoadAllowList() As Integer
        AllowListStrict = False
        Dim p = AllowListPath()
        If Not File.Exists(p) Then Return 0
        Dim n As Integer = 0
        Try
            For Each l In File.ReadAllLines(p)
                Dim s = l.Trim()
                If s = "" OrElse s.StartsWith("#") Then Continue For
                If String.Equals(s, "!strict", StringComparison.OrdinalIgnoreCase) Then
                    AllowListStrict = True
                    Continue For
                End If
                If IsGlob(s) Then
                    AllowPatterns.Add(GlobToRegex(s))
                Else
                    GlobalNames.Add(s)
                End If
                n += 1
            Next
        Catch ex As Exception
            Log("  ! could not read allowed_filenames.txt: " & ex.Message)
        End Try
        Return n
    End Function

    Private Function ApplyDenyList() As Integer
        Dim p = DenyListPath()
        If Not File.Exists(p) Then Return 0
        Dim n As Integer = 0
        Try
            For Each l In File.ReadAllLines(p)
                Dim s = l.Trim()
                If s = "" OrElse s.StartsWith("#") Then Continue For
                If IsGlob(s) Then
                    ' Applied at match time, and also strips anything already
                    ' learned that the pattern covers - including from the cache,
                    ' so a denied name does not reappear on the next scan.
                    Dim rx = GlobToRegex(s)
                    DenyPatterns.Add(rx)
                    Dim hit = GlobalNames.Where(Function(k) rx.IsMatch(k)).ToList()
                    For Each k In hit
                        GlobalNames.Remove(k)
                        LearnedNames.Remove(k)
                    Next
                    n += hit.Count
                ElseIf GlobalNames.Remove(s) Then
                    LearnedNames.Remove(s)
                    n += 1
                Else
                    LearnedNames.Remove(s)
                End If
            Next
        Catch ex As Exception
            Log("  ! could not read denied_filenames.txt: " & ex.Message)
        End Try
        Return n
    End Function

    Private Sub LoadVocab()
        Try
            Dim p = VocabPath()
            If Not File.Exists(p) Then Exit Sub
            For Each l In File.ReadAllLines(p)
                Dim s = l.Trim()
                If s <> "" AndAlso Not s.StartsWith("#") Then
                    LearnedNames.Add(s)
                    GlobalNames.Add(s)
                End If
            Next
        Catch
        End Try
    End Sub

    Private Sub SaveVocab()
        Dim lines As New List(Of String)()
        lines.Add("# Canonical source filenames LEARNED from real FTPUploaderVB queue files.")
        lines.Add("# @PID@ stands for the panel's local file PID.")
        lines.Add("#")
        lines.Add("# THIS FILE IS REGENERATED ON EVERY SCAN - edits here do not survive.")
        lines.Add("# It holds only what was learned; names from allowed_filenames.txt are NOT")
        lines.Add("# copied here, so removing one from that file really does remove it.")
        lines.AddRange(LearnedNames.OrderBy(Function(s) s))

        ' Beside the exe by default; fall back to the queue folder if that is not
        ' writable, so a read-only or shared install still works.
        Dim primary = VocabPath()
        Try
            File.WriteAllLines(primary, lines)
            Return
        Catch
        End Try
        Try
            File.WriteAllLines(Path.Combine(QueueRoot, KNOWN_FILE), lines)
        Catch ex As Exception
            Log("  ! could not save filename cache: " & ex.Message)
        End Try
    End Sub

    ' Harvest ext->dest-folder pairs from archived queue files, grouped by the
    ' panel they belonged to, so folder inference keeps working after the live
    ' queue no longer contains a panel holding both extensions.
    Private Sub AddDonorsFromBackups()
        Dim byPanel As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)
        Dim seen As Integer = 0
        For Each bdir In New String() {"Backedup Recovery Queue", "Backedup Succeed Queue",
                                       "Backedup Failed Queue"}
            Dim root = Path.Combine(QueueRoot, bdir)
            If Not Directory.Exists(root) Then Continue For
            Try
                For Each f In Directory.EnumerateFiles(root, "*.txt", SearchOption.AllDirectories)
                    seen += 1
                    If seen > 20000 Then Exit For
                    Try
                        Dim l = File.ReadAllLines(f)
                        If l.Length < MIN_LINES Then Continue For
                        Dim src = l(7).Trim()
                        Dim dst = l(8).Trim()
                        If src = "" OrElse dst = "" Then Continue For
                        If src = l(10).Trim() OrElse src = l(13).Trim() Then Continue For
                        Dim key = l(13).Trim().ToLowerInvariant()      ' host file = panel identity
                        Dim ex = Path.GetExtension(src)
                        If ex = "" Then Continue For
                        Dim c = dst.LastIndexOf("/"c)
                        If c <= 0 Then Continue For
                        If Not byPanel.ContainsKey(key) Then
                            byPanel(key) = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                        End If
                        If Not byPanel(key).ContainsKey(ex) Then byPanel(key)(ex) = dst.Substring(0, c)
                    Catch
                    End Try
                Next
            Catch
            End Try
        Next
        For Each kv In byPanel
            If kv.Value.Count > 1 Then DonorMaps.Add(kv.Value)
        Next
    End Sub

    Private Sub AddReconstructed(p As Panel)
        p.ReconstructApplied = True                ' guard: never run twice on one panel
        If p.Entries.Count = 0 Then Exit Sub   ' nothing to learn from

        Dim sib = p.Entries(0)
        Dim folder As String
        Try
            folder = Path.GetDirectoryName(sib.SourceFile)
        Catch
            Exit Sub
        End Try
        If folder Is Nothing OrElse folder = "" OrElse Not Directory.Exists(folder) Then Exit Sub
        Dim ourToken = LocalToken(sib.SourceFile)

        ' What is already accounted for: live queue files, plus anything the host
        ' file already records (clean or placeholder).
        Dim known As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each e In p.Entries
            known.Add(Path.GetFileName(e.SourceFile))
        Next
        For Each rec In ReadRecordStates(p.HostSrc).Keys
            ' Records are "destPath@channel" - drop the channel before taking the
            ' filename, or nothing will ever match.
            Dim bare = rec
            Dim at = bare.LastIndexOf("@"c)
            If at > 0 Then bare = bare.Substring(0, at)
            Dim cut = bare.LastIndexOf("/"c)
            If cut >= 0 AndAlso cut < bare.Length - 1 Then
                known.Add(bare.Substring(cut + 1))
            End If
        Next
        ' Never treat the index or host file itself as uploadable data.
        known.Add(Path.GetFileName(p.IndexSrc))
        known.Add(Path.GetFileName(p.HostSrc))

        ' Allowed filename shapes come from the GLOBAL vocabulary (every real queue
        ' file seen in this scan), because a panel's own survivors may not cover a
        ' uniquely-named file such as d994_gamma.hex or puc_otp_read.txt. Every
        ' pattern still originates from a genuine queue file - nothing is invented.
        ' The extension->dest folder map must stay panel-local, since the folder
        ' embeds this panel's own server PID and timestamp.
        Dim extFolder As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each e In p.Entries
            Dim ext = Path.GetExtension(e.SourceFile)
            If ext <> "" AndAlso Not extFolder.ContainsKey(ext) Then
                Dim cut = e.DestFile.LastIndexOf("/"c)
                If cut > 0 Then extFolder(ext) = e.DestFile.Substring(0, cut)
            End If
        Next

        For Each f In Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
            Dim fname = Path.GetFileName(f)
            If known.Contains(fname) Then Continue For

            Dim ext = Path.GetExtension(fname)
            Dim matched = IsAllowedName(CanonicalName(fname, ourToken))

            If Not matched Then
                p.SkippedJunk.Add(fname & " (not a known filename for this product)")
                Continue For
            End If
            If Not extFolder.ContainsKey(ext) Then
                ' No surviving entry of this extension - try to infer the folder
                ' from a donor panel that has both.
                Dim inferred As String = ""
                For Each kv In extFolder
                    inferred = InferFolder(kv.Value, kv.Key, ext)
                    If inferred <> "" Then Exit For
                Next
                If inferred = "" Then
                    p.SkippedJunk.Add(fname & " (cannot determine dest folder for " & ext & ")")
                    Continue For
                End If
                extFolder(ext) = inferred
                ' Recorded rather than logged per-extension: at ~2 per panel this
                ' was flooding the log with a thousand near-identical lines.
                If Not p.InferredExts.Contains(ext) Then p.InferredExts.Add(ext)
            End If

            Dim r As New QueueEntry()
            r.FilePath = ""                      ' no queue file on disk
            r.IsReconstructed = True
            r.Host = sib.Host : r.User = sib.User : r.Pass = sib.Pass
            r.ExePath = sib.ExePath : r.SessionLog = sib.SessionLog
            r.SucceedLog = sib.SucceedLog : r.FailLog = sib.FailLog
            r.OutIndexInfo = sib.OutIndexInfo
            r.IndexSrc = sib.IndexSrc : r.IndexDst = sib.IndexDst
            r.OutHostInfo = sib.OutHostInfo
            r.HostSrc = sib.HostSrc : r.HostDst = sib.HostDst
            r.Total = sib.Total : r.Channel = sib.Channel
            r.SourceFile = f
            r.DestFile = extFolder(ext) & "/" & fname
            Try
                r.Created = File.GetCreationTime(f)
            Catch
                r.Created = DateTime.Now
            End Try

            p.Entries.Add(r)
            p.RebuiltCount += 1
        Next

        If p.RebuiltCount > 0 Then
            p.Entries = p.Entries.OrderBy(Function(x) x.Created).ToList()
        End If
    End Sub

    Public Function ScanQueueFiles() As List(Of QueueEntry)
        Dim result As New List(Of QueueEntry)()
        Dim files = Directory.EnumerateFiles(QueueRoot, "*.txt", SearchOption.TopDirectoryOnly)
        Dim bad As Integer = 0

        For Each f In files
            ' The rule files sit beside the exe, which is often the queue folder
            ' itself. They are .txt, so exclude them by name.
            If IsRuleFileName(Path.GetFileName(f)) Then Continue For

            Dim e = ParseQueueFile(f)
            If e Is Nothing Then
                bad += 1
            Else
                result.Add(e)
            End If
        Next

        If bad > 0 Then
            Log("Skipped " & bad.ToString() & " .txt file(s) that are not queue files (< " &
                MIN_LINES.ToString() & " lines or unreadable).")
        End If
        Return result
    End Function

    Private Function ParseQueueFile(path As String) As QueueEntry
        Try
            Dim lines = File.ReadAllLines(path)
            If lines.Length < MIN_LINES Then Return Nothing

            Dim e As New QueueEntry()
            e.FilePath = path
            e.Raw = lines
            e.Host = lines(0).Trim()
            e.User = lines(1).Trim()
            e.Pass = lines(2)
            e.ExePath = lines(3).Trim()
            e.SessionLog = lines(4).Trim()
            e.SucceedLog = lines(5).Trim()
            e.FailLog = lines(6).Trim()
            e.SourceFile = lines(7).Trim()
            e.DestFile = lines(8).Trim()
            e.OutIndexInfo = lines(9).Trim()
            e.IndexSrc = lines(10).Trim()
            e.IndexDst = lines(11).Trim()
            e.OutHostInfo = lines(12).Trim()
            e.HostSrc = lines(13).Trim()
            e.HostDst = lines(14).Trim()
            Dim t As Integer = 0
            Integer.TryParse(lines(15).Trim(), t)
            e.Total = t
            e.Channel = lines(16).Trim()

            If e.Host = "" OrElse e.HostSrc = "" OrElse e.Total <= 0 Then Return Nothing

            Try
                e.Created = File.GetCreationTime(path)
            Catch
                e.Created = DateTime.Now
            End Try

            Return e
        Catch
            Return Nothing
        End Try
    End Function

    Public Function BuildPanels(entries As List(Of QueueEntry)) As List(Of Panel)
        Dim map As New Dictionary(Of String, Panel)(StringComparer.OrdinalIgnoreCase)

        For Each e In entries
            Dim key = e.HostSrc.ToLowerInvariant()
            Dim p As Panel = Nothing
            If Not map.TryGetValue(key, p) Then
                p = New Panel()
                p.Key = key
                p.PID = Path.GetFileNameWithoutExtension(e.IndexSrc)
                If p.PID = "" Then p.PID = Path.GetFileNameWithoutExtension(e.HostSrc)
                p.Total = e.Total
                p.HostSrc = e.HostSrc
                p.HostDst = e.HostDst
                p.IndexSrc = e.IndexSrc
                p.IndexDst = e.IndexDst
                map(key) = p
            End If

            If p.Total <> e.Total Then p.TotalMismatch = True

            If e.IsIndexOrHostQueue Then
                p.Leftovers.Add(e)
            Else
                p.Entries.Add(e)
            End If
        Next

        Dim list = map.Values.ToList()
        If OnlyPid <> "" Then
            list = list.Where(Function(p) p.PID.IndexOf(OnlyPid, StringComparison.OrdinalIgnoreCase) >= 0).ToList()
        End If

        ' FIFO within a panel - oldest queue file first, the opposite of the
        ' main app's OrderByDescending, which is what starved these in the first place.
        For Each p In list
            p.Entries = p.Entries.OrderBy(Function(x) x.Created).ToList()
        Next

        ' Rebuild entries for files sitting on disk with no queue file.
        BuildPanelsTail(list, entries)
        Return list.OrderBy(Function(p) p.PID).ToList()
    End Function

    ' Vocabulary + donor maps shared by every panel. Split out so it can be built
    ' either during a scan or on demand at upload time.
    Private Sub BuildReconstructContext(entries As List(Of QueueEntry))
        If True Then
            ' Vocabulary of EXACT canonical filenames from real queue files. Exact
            ' rather than digit-generalised, so an unexpected number (step99_...,
            ' ..._B999_...) is rejected instead of accepted as family member.
            '
            ' Three sources, because the live queue alone is not enough: once every
            ' panel holding a given filename has been completed, that name would
            ' vanish from the vocabulary and reconstruction would start rejecting
            ' genuine files.
            GlobalNames = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            LearnedNames = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            AllowPatterns.Clear()
            DenyPatterns.Clear()

            ' Rule file first, so "!strict" is known before deciding whether to learn.
            Dim fromRules = LoadAllowList()
            If fromRules > 0 Then Log("Reconstruction: rules from " & AllowListPath())
            If File.Exists(DenyListPath()) Then Log("Reconstruction: denials from " & DenyListPath())

            Dim fromLive As Integer = 0
            Dim fromMemory As Integer = 0
            Dim fromBackup As Integer = 0

            If Not AllowListStrict Then
                For Each e In entries
                    If e.IsIndexOrHostQueue Then Continue For
                    Dim cn = CanonicalName(Path.GetFileName(e.SourceFile), LocalToken(e.SourceFile))
                    LearnedNames.Add(cn)
                    If GlobalNames.Add(cn) Then fromLive += 1
                Next

                Dim beforeRemembered = GlobalNames.Count
                LoadVocab()
                fromMemory = GlobalNames.Count - beforeRemembered

                For Each bdir In New String() {"Backedup Recovery Queue", "Backedup Succeed Queue",
                                               "Backedup Failed Queue"}
                    Dim n As Integer = 0
                    fromBackup += LearnNamesFrom(Path.Combine(QueueRoot, bdir), n)
                Next
            End If

            ' Denials win over every source, including the rule file. Applied BEFORE
            ' the cache is written, or a denied name would be saved and come back.
            Dim denied = ApplyDenyList()
            If Not AllowListStrict Then SaveVocab()

            If AllowListStrict Then
                Log("Reconstruction: STRICT - allowed_filenames.txt only, " &
                    fromRules.ToString() & " name(s). Nothing learned.")
            Else
                Log("Reconstruction: vocabulary = " & GlobalNames.Count.ToString() &
                    " filename(s) + " & AllowPatterns.Count.ToString() & " wildcard(s)  (" &
                    fromRules.ToString() & " from rules, " &
                    fromLive.ToString() & " from live queue, " &
                    fromMemory.ToString() & " remembered, " & fromBackup.ToString() &
                    " from backups)")
            End If
            If denied > 0 Then
                Log("Reconstruction: " & denied.ToString() & " name(s) removed by denied_filenames.txt")
            End If

            ' Donor ext->folder maps, so a panel that lost every queue file of one
            ' extension can still place it. Only the ext->ext RELATIONSHIP is taken
            ' from a donor (IMAGE vs HEX), never the panel-specific part, so a
            ' donor from the backup archive is as good as a live one - and is
            ' needed once the live queue no longer holds a panel with both.
            DonorMaps.Clear()
            Dim byHost As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)
            For Each e In entries
                If e.IsIndexOrHostQueue Then Continue For
                Dim k = e.HostSrc.ToLowerInvariant()
                Dim ex = Path.GetExtension(e.SourceFile)
                If ex = "" Then Continue For
                Dim c = e.DestFile.LastIndexOf("/"c)
                If c <= 0 Then Continue For
                If Not byHost.ContainsKey(k) Then
                    byHost(k) = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                End If
                If Not byHost(k).ContainsKey(ex) Then byHost(k)(ex) = e.DestFile.Substring(0, c)
            Next
            For Each kv In byHost
                If kv.Value.Count > 1 Then DonorMaps.Add(kv.Value)
            Next
            Dim liveDonors = DonorMaps.Count
            AddDonorsFromBackups()
            Log("Reconstruction: " & DonorMaps.Count.ToString() & " donor panel(s) for folder inference (" &
                liveDonors.ToString() & " live, " & (DonorMaps.Count - liveDonors).ToString() & " from backups)")

            ReconstructContextReady = True
        End If
    End Sub

    ' Runs the per-panel reconstruction pass when the scan is made with the option
    ' already enabled.
    Private Sub BuildPanelsTail(list As List(Of Panel), entries As List(Of QueueEntry))
        LastEntries = entries
        ReconstructContextReady = False
        If Not Reconstruct Then Exit Sub

        BuildReconstructContext(entries)
        Dim rebuilt As Integer = 0
        Dim junk As Integer = 0
        Dim inferredPanels As Integer = 0
        For Each p In list
            AddReconstructed(p)
            rebuilt += p.RebuiltCount
            junk += p.SkippedJunk.Count
            If p.InferredExts.Count > 0 Then inferredPanels += 1
        Next
        Log("Reconstruction: rebuilt " & rebuilt.ToString() & " entr(ies) from disk, " &
            junk.ToString() & " candidate(s) skipped as unrecognised.")
        If inferredPanels > 0 Then
            Log("Reconstruction: dest folder inferred from a donor panel for " &
                inferredPanels.ToString() & " panel(s) - see per-panel detail on upload.")
        End If
    End Sub

    ' Apply reconstruction to a single panel, building the shared context first if
    ' the scan did not. Lets the option be switched on after a scan and take effect
    ' on just the panel(s) being uploaded.
    Public Sub EnsureReconstructed(p As Panel)
        If p.ReconstructApplied Then Exit Sub
        If Not ReconstructContextReady Then
            If LastEntries Is Nothing Then Exit Sub
            BuildReconstructContext(LastEntries)
        End If
        AddReconstructed(p)
    End Sub

    ' =========================================================================
    ' Per-panel processing
    ' =========================================================================

    Public Sub ProcessPanel(p As Panel)
        nPanels += 1

        ' If the option was switched on after the scan, apply it now for just this
        ' panel rather than forcing the user to re-scan.
        If Reconstruct AndAlso Not p.ReconstructApplied Then
            EnsureReconstructed(p)
            If p.RebuiltCount > 0 Then
                Log("  (reconstruction applied at upload time)")
            End If
        End If

        Dim st = Classify(p)
        Dim recorded = ReadRecordStates(p.HostSrc)
        Dim hostBefore = st.HostNow
        Dim pending = st.Pending
        Dim dupCount = st.DoneCount
        Dim retryCount = st.RetryCount
        Dim newCount = st.NewCount
        Dim realPending = newCount + retryCount
        Dim projected = st.Projected

        Log("---------------------------------------------------------------")
        Log("PID " & p.PID)
        Log("  host file   : " & p.HostSrc)
        Log("  total needed: " & p.Total.ToString() & "   host now: " & hostBefore.ToString() &
            "   pending: " & pending.ToString() & " (" & dupCount.ToString() & " done, " &
            retryCount.ToString() & " failed-retry, " & newCount.ToString() & " new)")
        Log("  projected   : " & projected.ToString() & " / " & p.Total.ToString())
        If st.MissingSrc > 0 Then
            Log("  " & st.MissingSrc.ToString() & " source image(s) missing from disk - " &
                If(SkipMissingSource, "left in place, so the panel cannot complete.",
                                      "will be marked failed, so the manifest will be short."))
        End If
        If p.TotalMismatch Then
            Log("  WARNING: queue files disagree on totalFileCount. Using " & p.Total.ToString() & ".")
        End If
        If p.Leftovers.Count > 0 Then
            Log("  note: " & p.Leftovers.Count.ToString() & " stale index/host queue file(s) present.")
        End If
        If p.RebuiltCount > 0 AndAlso Reconstruct Then
            Log("  RECONSTRUCTED " & st.Rebuilt.ToString() & " entr(ies) from disk:")
            If p.InferredExts.Count > 0 Then
                Log("     dest folder INFERRED from a donor panel for: " & String.Join(", ", p.InferredExts))
            End If
            For Each e In p.Entries.Where(Function(x) x.IsReconstructed)
                Log("     " & Path.GetFileName(e.SourceFile) & "  ->  " & e.DestFile)
            Next
        ElseIf p.RebuiltCount > 0 Then
            Log("  note: " & p.RebuiltCount.ToString() &
                " rebuilt entr(ies) from an earlier scan are IGNORED (Reconstruct is off).")
        End If
        If p.SkippedJunk.Count > 0 Then
            Log("  skipped " & p.SkippedJunk.Count.ToString() & " unrecognised file(s) in the source folder:")
            For Each j In p.SkippedJunk
                Log("     " & j)
            Next
        End If

        Dim canComplete = (projected >= p.Total)
        If Not canComplete AndAlso Not ForceIncomplete Then
            ' Show every option that applies, best first, so the choice is the
            ' operator's. Force alone would ship a short manifest for files that
            ' are often still sitting on disk, so it is never the only suggestion.
            Dim gap = p.Total - projected
            Dim opts As New List(Of String)()

            If st.ShortByQueue > 0 AndAlso Not Reconstruct Then
                opts.Add("'Reconstruct from disk' (-reconstruct) - rebuilds the missing entries " &
                         "if the images are still on disk. Try this first.")
            End If
            If st.ShortBySource > 0 AndAlso SkipMissingSource Then
                opts.Add("Untick 'Skip missing source' - lets the panel finish without those " &
                         st.ShortBySource.ToString() & " file(s).")
            End If
            If st.ShortByQueue > 0 AndAlso Reconstruct Then
                opts.Add("Nothing left to recover - the images are not on disk either.")
            End If
            opts.Add("'Force incomplete' (-force) - sends now; the customer gets a manifest " &
                     "short by " & gap.ToString() & " of " & p.Total.ToString() & ".")

            Log("  VERDICT: " & st.Verdict & ". Skipped.")
            Log("  Options:")
            For i = 0 To opts.Count - 1
                Log("    " & (i + 1).ToString() & ") " & opts(i))
            Next
            nPanelsIncomplete += 1
            ReportRow(p, hostBefore, pending, 0, 0, 0, dupCount, hostBefore, "SKIPPED-INCOMPLETE")
            Return
        End If

        If Not DoExecute Then
            Log("  VERDICT: " & st.Verdict)
            nPanelsFired += 1
            ReportRow(p, hostBefore, pending, realPending, 0, 0, dupCount, projected, "DRYRUN-WOULD-FIRE")
            Return
        End If

        ' ---- drain the data files -------------------------------------------
        Dim up As Integer = 0
        Dim fl As Integer = 0
        Dim ms As Integer = 0

        For Each e In EffectiveEntries(p)
            If CancelRequested Then
                Log("    [stop] cancelled by user - remaining queue files untouched.")
                Exit For
            End If
            Dim rec = Normalize(e.Record)
            Dim isRetry As Boolean = False

            If recorded.ContainsKey(rec) Then
                If Not recorded(rec) Then
                    ' Clean record already present - genuinely done.
                    Log("    [dup ] already recorded, dropping queue: " & Path.GetFileName(e.FilePath))
                    BackupAndDelete(e.FilePath, "Backedup Recovery Queue\AlreadyRecorded")
                    nAlready += 1
                    Continue For
                End If
                ' Only a " - failed" placeholder is present and a live queue file
                ' still exists, so this one deserves another attempt. On success we
                ' REPLACE the placeholder rather than appending, or the panel would
                ' end up with more lines than totalFileCount.
                isRetry = True
                Log("    [retry] failed placeholder found, retrying: " & Path.GetFileName(e.SourceFile))
            End If

            If Not File.Exists(e.SourceFile) Then
                If SkipMissingSource Then
                    Log("    [miss] source gone, LEFT IN PLACE: " & e.SourceFile)
                    ms += 1 : nMissing += 1
                    Continue For
                End If
                Log("    [miss] source gone, marking failed: " & e.SourceFile)
                If Not isRetry Then AppendRecord(e, True)   ' placeholder already there on a retry
                recorded(rec) = True
                AppendLog(e.FailLog, "Recovery: source file missing on disk: " & e.SourceFile)
                BackupAndDelete(e.FilePath, "Backedup Recovery Queue\MissingSource")
                ms += 1 : nMissing += 1
                Continue For
            End If

            Dim err As String = ""
            Dim connErr As Boolean = False
            If TryUpload(e, e.SourceFile, e.DestFile, err, connErr) Then
                Log("    [ ok ] " & Path.GetFileName(e.SourceFile) & " -> " & e.DestFile)
                AppendLog(e.SucceedLog, "Recovery: upload succeeded " & e.SourceFile &
                          " to: ftp://" & e.Host & e.DestFile)
                If isRetry Then
                    ' Placeholder becomes a clean record - replace, do not append.
                    ReplaceRecord(e)
                    nRetried += 1
                Else
                    AppendRecord(e, False)
                End If
                ConsecutiveFailures = 0
                recorded(rec) = False
                BackupAndDelete(e.FilePath, "Backedup Recovery Queue\Succeeded")
                up += 1 : nUploaded += 1
                If e.IsReconstructed Then nRebuilt += 1
            Else
                If connErr Then
                    ' Server unreachable - nothing wrong with this file. Leave the
                    ' queue file alone and write no placeholder, so a re-run after
                    ' the connection is fixed picks it up exactly as it was.
                    ' Deliberately does NOT stop the run: this must work unattended.
                    ' Every skip is listed: during an outage this is the record of
                    ' exactly which files still need sending, and it is cheap now
                    ' that the socket pre-check makes each one instant.
                    Log("    [conn] server unreachable, queue file kept: " &
                        Path.GetFileName(e.SourceFile))
                    AppendLog(e.FailLog, "Recovery: server unreachable, queue file kept: " & err)
                    nSkippedOffline += 1
                Else
                    Log("    [FAIL] " & Path.GetFileName(e.SourceFile) & " : " & err)
                    AppendLog(e.FailLog, "Recovery: upload failed after " & MaxRetry.ToString() &
                              " attempt(s): " & err & " " & e.SourceFile &
                              " to: ftp://" & e.Host & e.DestFile)
                    If Not isRetry Then AppendRecord(e, True)   ' placeholder already present on a retry
                    recorded(rec) = True
                    BackupAndDelete(e.FilePath, "Backedup Recovery Queue\Failed")
                    fl += 1 : nFailed += 1
                End If

                ' Only file-level failures count towards the breaker, and it is off
                ' by default - an unattended run should keep going and mark what it
                ' can, not stop and wait for someone to notice.
                If Not connErr Then
                    ConsecutiveFailures += 1
                    If AbortAfterConsecutiveFailures > 0 AndAlso
                       ConsecutiveFailures >= AbortAfterConsecutiveFailures Then
                        Aborted = True
                        CancelRequested = True
                        Log("")
                        Log("  *** ABORTING: " & ConsecutiveFailures.ToString() &
                            " uploads failed back-to-back with the server reachable.")
                        Log("  *** Remaining panels left untouched.")
                        Exit For
                    End If
                End If
            End If
        Next

        ' ---- fire index + host ----------------------------------------------
        Dim hostAfter = CountLines(p.HostSrc)
        Dim note As String

        ' Count how many lines are placeholders - those get stripped, so the
        ' manifest LGD receives will be short by exactly that many. A short fire
        ' must never look like a clean one in the report.
        Dim placeholders As Integer = 0
        If File.Exists(p.HostSrc) Then
            Try
                placeholders = File.ReadAllLines(p.HostSrc).
                    Count(Function(l) l.Contains(FAILED_SUFFIX))
            Catch
            End Try
        End If
        Dim shortBy = Math.Max(0, p.Total - (hostAfter - placeholders))

        If Aborted OrElse CancelRequested Then
            ' Never finalize a panel that was interrupted part-way: with -force
            ' that would ship a manifest short of the files we simply had not
            ' reached yet. Left alone, the next run resumes cleanly.
            Log("  index/host NOT sent - run " & If(Aborted, "aborted", "stopped") &
                " before this panel finished.")
            nPanelsIncomplete += 1
            ReportRow(p, hostBefore, pending, up, fl, ms, dupCount, hostAfter,
                      If(Aborted, "ABORTED", "STOPPED"))
            Return
        End If

        If ServerDown Then
            ' Files were skipped because the server was unreachable, not because
            ' they are unavailable. Sending the manifest now - even with Force -
            ' would tell the customer those files are never coming.
            Log("  index/host NOT sent - the server was unreachable during this panel.")
            Log("  Nothing was consumed; run Upload again once the connection is back.")
            nPanelsIncomplete += 1
            ReportRow(p, hostBefore, pending, up, fl, ms, dupCount, hostAfter, "SERVER-OFFLINE")
            Return
        End If

        If hostAfter >= p.Total OrElse ForceIncomplete Then
            Dim forced = (hostAfter < p.Total)
            If forced Then
                Log("  FORCED: host has " & hostAfter.ToString() & " / " & p.Total.ToString() &
                    " - sending index/host anyway (-force).")
            End If
            If shortBy > 0 Then
                ' Spell out both causes. "1 source image missing" alone reads as
                ' though the manifest should be short by 1, when files that never
                ' had a queue file are usually the bigger part of the gap.
                Dim causes As New List(Of String)()
                If placeholders > 0 Then
                    causes.Add(placeholders.ToString() & " image(s) missing from disk")
                End If
                Dim noQueue = shortBy - placeholders
                If noQueue > 0 Then
                    causes.Add(noQueue.ToString() & " file(s) with no queue file" &
                               If(Reconstruct, " and not recoverable from disk", " (try Reconstruct)"))
                End If
                Log("  WARNING: manifest will be SHORT by " & shortBy.ToString() &
                    " of " & p.Total.ToString() & " - " & String.Join(", ", causes))
            End If
            If FinalizePanel(p) Then
                nPanelsFired += 1
                If forced Then nPanelsForced += 1
                If shortBy > 0 Then
                    nPanelsShort += 1
                    note = If(forced, "SENT-FORCED-SHORT (" & shortBy.ToString() & " missing)",
                                      "SENT-SHORT (" & shortBy.ToString() & " missing)")
                ElseIf p.RebuiltCount > 0 Then
                    note = "INDEX+HOST SENT (" & p.RebuiltCount.ToString() & " rebuilt)"
                Else
                    note = "INDEX+HOST SENT"
                End If
            Else
                nPanelsIncomplete += 1
                note = "INDEX/HOST UPLOAD FAILED"
            End If
        Else
            Log("  host still " & hostAfter.ToString() & " / " & p.Total.ToString() &
                " - index/host NOT sent.")
            nPanelsIncomplete += 1
            note = "STILL SHORT"
        End If

        ReportRow(p, hostBefore, pending, up, fl, ms, dupCount, hostAfter, note)
    End Sub

    ' Strip " - failed" placeholder lines, then upload index and host.
    Private Function FinalizePanel(p As Panel) As Boolean
        Dim ref = p.Entries.FirstOrDefault()
        If ref Is Nothing Then ref = p.Leftovers.FirstOrDefault()
        If ref Is Nothing Then
            Log("  cannot finalize: no queue file left to read credentials from.")
            Return False
        End If

        ' Upload placeholder-free copies; the originals stay as they are so an
        ' interruption here cannot strand the panel below totalFileCount.
        Dim idxTemp As Boolean = False
        Dim hstTemp As Boolean = False
        Dim idxUpload = CleanCopyFor(p.IndexSrc, idxTemp)
        Dim hstUpload = CleanCopyFor(p.HostSrc, hstTemp)

        Dim ok As Boolean = True
        Dim err As String = ""

        If File.Exists(idxUpload) Then
            If TryUpload(ref, idxUpload, p.IndexDst, err) Then
                Log("  [ ok ] INDEX -> " & p.IndexDst)
                AppendLog(ref.SucceedLog, "Recovery: index uploaded " & p.IndexSrc &
                          " to: ftp://" & ref.Host & p.IndexDst)
            Else
                Log("  [FAIL] INDEX : " & err)
                AppendLog(ref.FailLog, "Recovery: index upload failed: " & err)
                ok = False
            End If
        Else
            Log("  [FAIL] index file does not exist: " & p.IndexSrc)
            ok = False
        End If

        If File.Exists(hstUpload) Then
            If TryUpload(ref, hstUpload, p.HostDst, err) Then
                Log("  [ ok ] HOST  -> " & p.HostDst)
                AppendLog(ref.SucceedLog, "Recovery: host uploaded " & p.HostSrc &
                          " to: ftp://" & ref.Host & p.HostDst)
            Else
                Log("  [FAIL] HOST  : " & err)
                AppendLog(ref.FailLog, "Recovery: host upload failed: " & err)
                ok = False
            End If
        Else
            Log("  [FAIL] host file does not exist: " & p.HostSrc)
            ok = False
        End If

        ' Discard the temp copies regardless of outcome.
        If idxTemp Then
            Try
                File.Delete(idxUpload)
            Catch
            End Try
        End If
        If hstTemp Then
            Try
                File.Delete(hstUpload)
            Catch
            End Try
        End If

        ' Clear stale index/host queue files so the main app cannot re-send them later.
        If ok Then
            For Each lo In p.Leftovers
                Log("  clearing stale index/host queue: " & Path.GetFileName(lo.FilePath))
                BackupAndDelete(lo.FilePath, "Backedup Recovery Queue\StaleIndexHost")
            Next
            For Each pth In New String() {p.Entries.Select(Function(x) x.OutIndexInfo).FirstOrDefault(),
                                          p.Entries.Select(Function(x) x.OutHostInfo).FirstOrDefault()}
                If pth IsNot Nothing AndAlso pth <> "" AndAlso File.Exists(pth) Then
                    BackupAndDelete(pth, "Backedup Recovery Queue\StaleIndexHost")
                End If
            Next
        End If

        Return ok
    End Function

    ' =========================================================================
    ' WinSCP session - one connection reused across files and panels
    ' =========================================================================

    Private CurSession As Session = Nothing
    Private CurHost As String = ""
    Private CurUser As String = ""

    Private Function GetSession(e As QueueEntry) As Session
        If CurSession IsNot Nothing AndAlso CurSession.Opened _
           AndAlso String.Equals(CurHost, e.Host, StringComparison.OrdinalIgnoreCase) _
           AndAlso String.Equals(CurUser, e.User, StringComparison.OrdinalIgnoreCase) Then
            Return CurSession
        End If

        CloseSession()

        Dim opts As New SessionOptions()
        opts.Protocol = Protocol.Ftp
        opts.HostName = e.Host
        opts.UserName = e.User
        opts.Password = e.Pass
        opts.TimeoutInMilliseconds = 20000

        Dim s As New Session()
        If e.ExePath <> "" AndAlso File.Exists(e.ExePath) Then
            s.ExecutablePath = e.ExePath
        End If
        Try
            Dim dir = RecoveryLogDir()
            s.SessionLogPath = Path.Combine(dir, RunStamp & "_winscp.log")
        Catch
        End Try

        s.Open(opts)
        CurSession = s
        CurHost = e.Host
        CurUser = e.User
        Return s
    End Function

    Private Sub CloseSession()
        If CurSession IsNot Nothing Then
            Try
                CurSession.Dispose()
            Catch
            End Try
            CurSession = Nothing
            CurHost = ""
            CurUser = ""
        End If
    End Sub

    ' connError comes back True when the session itself could not be established -
    ' i.e. the server is unreachable rather than this particular file being bad.
    ' That distinction matters: a file that failed because of an outage must keep
    ' its queue file, or a five-minute network problem would permanently retire
    ' hundreds of perfectly good files.
    ' A plain TCP connect with a short timeout. WinSCP's TimeoutInMilliseconds only
    ' governs waiting for a response on an established connection - it does not cap
    ' a connect to a dead port, which falls through to the OS TCP timeout and can
    ' block for minutes. Checking the socket first is what makes an outage fast
    ' and visible instead of a silent stall.
    Private Function ServerReachable(host As String, Optional timeoutMs As Integer = 3000) As Boolean
        If host = "" Then Return False
        Dim h = host
        Dim port As Integer = 21
        Dim colon = h.LastIndexOf(":"c)
        If colon > 0 Then
            Dim pp As Integer
            If Integer.TryParse(h.Substring(colon + 1), pp) Then
                port = pp
                h = h.Substring(0, colon)
            End If
        End If
        Try
            Using c As New Net.Sockets.TcpClient()
                Dim ar = c.BeginConnect(h, port, Nothing, Nothing)
                If Not ar.AsyncWaitHandle.WaitOne(timeoutMs) Then Return False
                c.EndConnect(ar)
                Return c.Connected
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function TryUpload(e As QueueEntry, src As String, dst As String,
                               ByRef err As String, Optional ByRef connError As Boolean = False) As Boolean
        err = ""
        connError = False

        ' While the server is known to be down, don't pay 3 x timeout on every
        ' file. Try once every PROBE_INTERVAL_SECONDS; the rest fail instantly.
        Dim probing As Boolean = False
        If ServerDown Then
            If DateTime.Now.Subtract(LastProbe).TotalSeconds < PROBE_INTERVAL_SECONDS Then
                connError = True
                err = "server offline (skipped without retrying)"
                Return False
            End If
            probing = True
            LastProbe = DateTime.Now
            Log("      probing the server again ...")
        End If

        ' Cheap socket check before handing over to WinSCP, so a dead server costs
        ' 3 seconds rather than an OS-level TCP timeout.
        If Not ServerReachable(e.Host) Then
            connError = True
            err = "cannot connect to " & e.Host & " (no answer within 3s)"
            ' Must set ServerDown here too. This branch returns early, so without
            ' it the flag was never raised and every file paid the full 3s socket
            ' timeout instead of being skipped instantly.
            If Not ServerDown Then
                ServerDown = True
                LastProbe = DateTime.Now
                Log("      server appears to be down - remaining files will be left for a")
                Log("      later run, with a re-check every " & PROBE_INTERVAL_SECONDS.ToString() & "s.")
            ElseIf probing Then
                Log("      still no answer from " & e.Host & ".")
                LastProbe = DateTime.Now
            End If
            Return False
        End If

        Dim attempts = If(probing, 1, MaxRetry)      ' a probe is a single attempt
        For attempt = 1 To attempts
            Dim thisAttempt = attempt          ' captured by the heartbeat lambda
            Dim opening As Boolean = True
            Dim sw = Diagnostics.Stopwatch.StartNew()
            Dim finished As Boolean = False
            ' PutFiles blocks, and a dropped connection can sit at the TCP level for
            ' far longer than the WinSCP timeout. Without a heartbeat the tool looks
            ' hung, so tick every 5s to show it is still waiting and for how long.
            Dim beat As Threading.Timer = Nothing
            Try
                Dim cb As New Threading.TimerCallback(
                    Sub(o)
                        If Not finished Then
                            Log("      ... waiting " & CInt(sw.Elapsed.TotalSeconds).ToString() &
                                "s on " & Path.GetFileName(src) &
                                "  (attempt " & thisAttempt.ToString() & "/" & attempts.ToString() & ")")
                        End If
                    End Sub)
                beat = New Threading.Timer(cb, Nothing, 5000, 5000)

                Dim s = GetSession(e)
                opening = False
                Dim topts As New TransferOptions()
                topts.TransferMode = TransferMode.Binary
                Dim r = s.PutFiles(src, dst, False, topts)
                r.Check()
                finished = True
                If ServerDown Then
                    Log("      server is reachable again - resuming normally.")
                    ServerDown = False
                End If
                connError = False
                Return True
            Catch ex As Exception
                finished = True
                err = ex.Message
                connError = opening          ' failed before the transfer started
                Log("      attempt " & attempt.ToString() & "/" & attempts.ToString() &
                    " failed after " & CInt(sw.Elapsed.TotalSeconds).ToString() & "s" &
                    If(opening, " (cannot reach server)", "") & ": " & err)
                CloseSession()   ' force a fresh connection on the next attempt
                If attempt < attempts Then Threading.Thread.Sleep(1000)
            Finally
                finished = True
                If beat IsNot Nothing Then beat.Dispose()
            End Try
        Next

        If Not connError Then
            ' The session opened, so this looks like a problem with the file. But a
            ' network drop DURING a transfer looks identical at this point, and
            ' marking a good file failed for that reason would be wrong. Settle it
            ' by asking whether the server is still there at all.
            Try
                CloseSession()
                GetSession(e)
            Catch
                connError = True
                Log("      ...server is no longer reachable - treating as a connection")
                Log("         problem, not a bad file. Queue file kept.")
            End Try
        End If

        If connError AndAlso Not ServerDown Then
            ServerDown = True
            LastProbe = DateTime.Now
            Log("      server appears to be down - remaining files will be left for a")
            Log("      later run, with a re-check every " & PROBE_INTERVAL_SECONDS.ToString() & "s.")
        End If
        Return False
    End Function

    ' =========================================================================
    ' File helpers
    ' =========================================================================

    Private Function Normalize(record As String) As String
        Dim r = record.Trim()
        If r.EndsWith(FAILED_SUFFIX, StringComparison.OrdinalIgnoreCase) Then
            r = r.Substring(0, r.Length - FAILED_SUFFIX.Length).Trim()
        End If
        Return r.ToLowerInvariant()
    End Function

    Private Function CountLines(path As String) As Integer
        If Not File.Exists(path) Then Return 0
        Try
            Return File.ReadAllLines(path).Count(Function(l) l.Trim() <> "")
        Catch
            Return 0
        End Try
    End Function

    ' Map normalized record -> True when it exists ONLY as a " - failed"
    ' placeholder, False when a clean record is present. A clean line always wins
    ' over a placeholder for the same record.
    Private Function ReadRecordStates(path As String) As Dictionary(Of String, Boolean)
        Dim map As New Dictionary(Of String, Boolean)(StringComparer.OrdinalIgnoreCase)
        If Not File.Exists(path) Then Return map
        Try
            For Each l In File.ReadAllLines(path)
                If l.Trim() = "" Then Continue For
                Dim key = Normalize(l)
                Dim failed = l.Trim().EndsWith(FAILED_SUFFIX, StringComparison.OrdinalIgnoreCase)
                If map.ContainsKey(key) Then
                    If Not failed Then map(key) = False
                Else
                    map(key) = failed
                End If
            Next
        Catch
        End Try
        Return map
    End Function

    ' Turn an existing " - failed" placeholder into a clean record, in place, in
    ' both index and host. Appending instead would push the panel past totalFileCount.
    Private Sub ReplaceRecord(e As QueueEntry)
        For Each fpath In New String() {e.IndexSrc, e.HostSrc}
            If Not File.Exists(fpath) Then Continue For
            Try
                Dim lines = File.ReadAllLines(fpath)
                Dim key = Normalize(e.Record)
                Dim hit As Boolean = False
                For i = 0 To lines.Length - 1
                    If lines(i).Trim() = "" Then Continue For
                    If Normalize(lines(i)) = key AndAlso
                       lines(i).Trim().EndsWith(FAILED_SUFFIX, StringComparison.OrdinalIgnoreCase) Then
                        lines(i) = e.Record
                        hit = True
                        Exit For
                    End If
                Next
                If hit Then
                    File.WriteAllLines(fpath, lines)
                Else
                    ' Placeholder vanished between the scan and now - fall back to append.
                    File.AppendAllText(fpath, e.Record & Environment.NewLine)
                End If
            Catch ex As Exception
                Log("      ! could not replace record in " & Path.GetFileName(fpath) & ": " & ex.Message)
            End Try
        Next
    End Sub

    ' Append "dest@channel" (optionally with the " - failed" placeholder) to
    ' BOTH index and host, in the same order the main app uses.
    '
    ' Each file is checked individually before appending. The two appends are not
    ' atomic, so a crash between them would otherwise leave index with the line
    ' and host without - and since dedupe reads only the host file, the next run
    ' would append to index a second time and ship a duplicate to the customer.
    Private Sub AppendRecord(e As QueueEntry, failed As Boolean)
        Dim line = e.Record & If(failed, FAILED_SUFFIX, "")
        AppendRecordTo(e.IndexSrc, e.Record, line, "index")
        AppendRecordTo(e.HostSrc, e.Record, line, "host")
    End Sub

    Private Sub AppendRecordTo(fpath As String, bareRecord As String, line As String, label As String)
        Try
            EnsureDirFor(fpath)
            If File.Exists(fpath) Then
                Dim key = Normalize(bareRecord)
                For Each l In File.ReadAllLines(fpath)
                    If l.Trim() <> "" AndAlso Normalize(l) = key Then
                        ' Already present (possibly from an interrupted run) - do
                        ' not add a second line for the same file.
                        Return
                    End If
                Next
            End If
            File.AppendAllText(fpath, line & Environment.NewLine)
        Catch ex As Exception
            Log("      ! could not append to " & label & ": " & ex.Message)
        End Try
    End Sub

    ' Build a placeholder-free copy in the temp folder and return its path, or
    ' return the original when there is nothing to strip.
    '
    ' The original is deliberately left untouched. Stripping it in place before
    ' the upload used to mean a crash mid-finalize left the host file permanently
    ' below totalFileCount, stranding the panel with no queue files to recover
    ' from. Uploading a copy keeps the on-disk count consistent whatever happens.
    Private Function CleanCopyFor(fpath As String, ByRef isTemp As Boolean) As String
        isTemp = False
        If Not File.Exists(fpath) Then Return fpath
        Try
            Dim all = File.ReadAllLines(fpath)
            Dim clean = all.Where(Function(l) Not l.Contains(FAILED_SUFFIX)).ToArray()
            If clean.Length = all.Length Then Return fpath

            Dim tmp = Path.Combine(Path.GetTempPath(),
                "ftprec_" & Guid.NewGuid().ToString("N") & "_" & Path.GetFileName(fpath))
            File.WriteAllLines(tmp, clean)
            isTemp = True
            Log("  stripped " & (all.Length - clean.Length).ToString() &
                " placeholder line(s) for upload of " & Path.GetFileName(fpath) &
                " (original left intact)")
            Return tmp
        Catch ex As Exception
            Log("  ! could not build clean copy of " & fpath & " : " & ex.Message)
            Return fpath
        End Try
    End Function

    Private Sub RemoveFailedLines(fpath As String)
        If Not File.Exists(fpath) Then Exit Sub
        Try
            Dim all = File.ReadAllLines(fpath)
            Dim clean = all.Where(Function(l) Not l.Contains(FAILED_SUFFIX)).ToArray()
            If clean.Length <> all.Length Then
                File.WriteAllLines(fpath, clean)
                Log("  stripped " & (all.Length - clean.Length).ToString() &
                    " placeholder line(s) from " & Path.GetFileName(fpath))
            End If
        Catch ex As Exception
            Log("  ! RemoveFailedLines failed on " & fpath & " : " & ex.Message)
        End Try
    End Sub

    Private Sub BackupAndDelete(fpath As String, subFolder As String)
        If Not File.Exists(fpath) Then Exit Sub
        Try
            Dim dir = Path.Combine(QueueRoot, subFolder)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
            ' Plain copy under the original name - queue filenames are already
            ' unique per panel, and keeping the name means a file can be restored
            ' to the queue folder by copying it straight back.
            File.Copy(fpath, Path.Combine(dir, Path.GetFileName(fpath)), True)
        Catch ex As Exception
            Log("      ! backup failed: " & ex.Message)
        End Try
        Try
            File.Delete(fpath)
        Catch ex As Exception
            Log("      ! delete failed: " & ex.Message)
        End Try
    End Sub

    Private Sub EnsureDirFor(filePath As String)
        Dim d = Path.GetDirectoryName(filePath)
        If d IsNot Nothing AndAlso d <> "" AndAlso Not Directory.Exists(d) Then
            Directory.CreateDirectory(d)
        End If
    End Sub

    Private Sub AppendLog(logPath As String, text As String)
        If logPath = "" Then Exit Sub
        Try
            EnsureDirFor(logPath)
            File.AppendAllText(logPath,
                "FTPRecovery" & vbTab & DateTime.Now.ToString("HH:mm:ss.fff") & vbTab &
                text & Environment.NewLine)
        Catch
        End Try
    End Sub

    ' =========================================================================
    ' Logging / report
    ' =========================================================================

    ' Logs live with the exe, alongside the rule files, so everything the tool owns
    ' is in one folder and the queue folder stays purely an input. Falls back to the
    ' queue folder if the exe's folder is not writable (read-only share, etc).
    Private Function RecoveryLogDir() As String
        Dim d = Path.Combine(ExeDir(), "Log\Recovery")
        Try
            If Not Directory.Exists(d) Then Directory.CreateDirectory(d)
            Return d
        Catch
        End Try
        Dim fallback = Path.Combine(QueueRoot, "Log\Recovery")
        Try
            If Not Directory.Exists(fallback) Then Directory.CreateDirectory(fallback)
        Catch
        End Try
        Return fallback
    End Function

    ' Where earlier versions kept the rule files - still honoured when reading.
    Private Function LegacyRuleDir() As String
        Return Path.Combine(QueueRoot, "Log\Recovery")
    End Function

    Private Sub OpenLogs()
        Try
            Dim d = RecoveryLogDir()
            LogWriter = New StreamWriter(Path.Combine(d, RunStamp & "_recovery.log"), True, Encoding.UTF8)
            LogWriter.AutoFlush = True
            ReportWriter = New StreamWriter(Path.Combine(d, RunStamp & "_recovery_report.csv"), False, Encoding.UTF8)
            ReportWriter.AutoFlush = True
            ReportWriter.WriteLine("PID,Total,HostBefore,Pending,Uploaded,Failed,MissingSource,AlreadyRecorded,HostAfter,Result")
        Catch ex As Exception
            Console.WriteLine("Warning: could not open log files: " & ex.Message)
        End Try
    End Sub

    Private Sub CloseLogs()
        Try
            If LogWriter IsNot Nothing Then LogWriter.Dispose()
            If ReportWriter IsNot Nothing Then ReportWriter.Dispose()
        Catch
        End Try
    End Sub

    Private Sub Log(text As String)
        Console.WriteLine(text)
        If LogSink IsNot Nothing Then
            Try
                LogSink(text)
            Catch
            End Try
        End If
        If LogWriter IsNot Nothing Then
            Try
                LogWriter.WriteLine(text)
            Catch
            End Try
        End If
    End Sub

    ' Reset per-run state so the GUI can scan/upload repeatedly in one session.
    Public Sub ResetRun()
        nPanels = 0 : nPanelsFired = 0 : nPanelsIncomplete = 0
        nUploaded = 0 : nFailed = 0 : nMissing = 0 : nAlready = 0 : nRetried = 0
        nRebuilt = 0 : nPanelsShort = 0 : nPanelsForced = 0
        ConsecutiveFailures = 0 : Aborted = False
        ServerDown = False : LastProbe = DateTime.MinValue : nSkippedOffline = 0
        CancelRequested = False
        RunStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
    End Sub

    Public Sub OpenLogsPublic()
        OpenLogs()
    End Sub

    Public Sub CloseLogsPublic()
        CloseLogs()
    End Sub

    Public Sub CloseSessionPublic()
        CloseSession()
    End Sub

    Public Function SummaryText() As String
        Return String.Join(Environment.NewLine, New String() {
            "Panels scanned        : " & nPanels.ToString(),
            "Panels index/host sent: " & nPanelsFired.ToString(),
            "  ...of which SHORT   : " & nPanelsShort.ToString(),
            "  ...of which FORCED  : " & nPanelsForced.ToString(),
            "Panels still short    : " & nPanelsIncomplete.ToString(),
            "Files uploaded        : " & nUploaded.ToString(),
            "Files failed          : " & nFailed.ToString(),
            "Files source missing  : " & nMissing.ToString(),
            "Files already recorded: " & nAlready.ToString(),
            "Failed->clean retries : " & nRetried.ToString(),
            "Rebuilt from disk     : " & nRebuilt.ToString()})
    End Function

    Private Sub ReportRow(p As Panel, hostBefore As Integer, pending As Integer,
                          up As Integer, fl As Integer, ms As Integer,
                          dup As Integer, hostAfter As Integer, note As String)

        ' Remember the outcome even after the queue files are gone, so the UI can
        ' keep showing the row with its result instead of dropping it.
        Outcomes(p.Key) = New PanelOutcome() With {
            .PID = p.PID, .Total = p.Total, .Uploaded = up, .Failed = fl,
            .Missing = ms, .Rebuilt = If(Reconstruct, p.RebuiltCount, 0), .HostAfter = hostAfter,
            .Result = note, .Stamp = DateTime.Now}

        If ReportWriter Is Nothing Then Exit Sub
        Try
            ReportWriter.WriteLine(String.Join(",", New String() {
                Csv(p.PID), p.Total.ToString(), hostBefore.ToString(), pending.ToString(),
                up.ToString(), fl.ToString(), ms.ToString(), dup.ToString(),
                hostAfter.ToString(), Csv(note)}))
        Catch
        End Try
    End Sub

    Private Function Csv(s As String) As String
        If s Is Nothing Then Return ""
        If s.Contains(",") OrElse s.Contains("""") Then
            Return """" & s.Replace("""", """""") & """"
        End If
        Return s
    End Function

End Module
