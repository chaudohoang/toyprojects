# FTPRecovery — Developer Notes

Technical reference. For operating the tool, see `README.md`.

Two programs, one engine (`FTPRecovery.vb`), differing only in entry point:

| | Build flag |
|---|---|
| `FTPRecoveryGUI.exe` — WPF front end (`FTPRecoveryWpf.vb`) | `/main:WpfProgram` |
| `FTPRecovery.exe` — console | `/main:Program` |

---

## 1. The bug being worked around

`FTPUploaderVB.Upload()` fires the index/host upload from **one place only** — the
success path:

```vb
File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + vbCrLf)
File.AppendAllText(sourceHostFile,  destFile + "@" + channelIndex + vbCrLf)
Dim uploadedCount As Integer = File.ReadAllLines(sourceHostFile).Length
If uploadedCount = Int32.Parse(totalFileCount) Then
    CreateIndexAndHostQueue(InfoFile)
End If
```

A file that fails but has not yet reached `maxFailRetry` appends nothing. Its queue
file remains for a later cycle — but work is selected with

```vb
OrderByDescending(GetCreationTime).Take(maximumUpload)
```

**newest first**, so once the folder exceeds `maximumUpload`, older retrying files
fall out of the window permanently. Fail count freezes, no line is ever written, the
count never reaches `totalFileCount`, manifests never send.

Second hole: when a failure *does* exhaust retries and is the last file to finalize,
the ` - failed` lines are appended inside the `Catch` block, which never re-checks
the count. The queue file is then deleted, so nothing can retrigger it.

### Permanent fixes still owed in FTPUploaderVB

1. Hoist the count check into a helper called from **both** `Try` and `Catch`,
   before `File.Delete(InfoFile)`.
2. `uploadedCount = totalFileCount` → `>=`.
3. `OrderByDescending` → `OrderBy`, so retries get priority instead of starving.
4. Guard `failMessage.IndexOf(".")` returning `-1` in `UpdateSummaryLogFail` — it
   throws from inside a `Catch` block and aborts the rest of that cycle's batch.
5. Only call `UploadIndexAndHost` on a freshly written `Output*InfoFile`, never a
   stale leftover.

---

## 2. Queue file format (17 lines, 0-based)

| Line | Field |
|---|---|
| 0–3 | host, username, password, WinSCP.exe path |
| 4–6 | session log, succeed log, fail log |
| 7–8 | source file → dest path |
| 9–11 | OutputIndexInfoFile, sourceIndexFile, destIndexFile |
| 12–14 | OutputHostInfoFile, sourceHostFile, destHostFile |
| 15–16 | totalFileCount, channelIndex |

Panels are keyed on **line 13** (`sourceHostFile`) — the field the completion logic
uses. `totalFileCount` is read per panel from line 15, so recipe size varies freely.

Host and index lines are `destPath@channelIndex`. **Strip the `@channel` before
taking the filename after the last `/`** — forgetting this caused 1,555 phantom
rebuilds during development.

Two identifiers vary per panel:

| | Appears in |
|---|---|
| **local file PID** (`AAA`) | queue filenames, `E:\POCB\...` source folder, session-log folder |
| **server PID** (`A4XN6600PN05BD5`) | every `/data1h1/...` dest path, `.idx` and host filenames |

`FTPUploaderVB` calls the *server* PID the PID; so does this tool. A panel never
mixes channel indices, so a rebuilt entry may copy the channel from any sibling.

---

## 3. Decision logic

`Classify()` is side-effect free and shared by the grid, the console verdict and
`ProcessPanel`, so all three always agree.

### Per file

| Queue | On disk | Host record | Action | Host line after |
|---|---|---|---|---|
| yes | yes | none | upload | append clean |
| yes | yes | clean | skip, retire queue | unchanged |
| yes | yes | ` - failed` | retry; success → `ReplaceRecord` | placeholder → clean |
| yes | **no** | none | skip upload | append ` - failed` |
| yes | **no** | ` - failed` | leave | unchanged |
| **no** | yes | none | reconstruct, upload | append clean |
| **no** | yes | clean | nothing owed | unchanged |
| **no** | yes | ` - failed` | reconstruct, retry, replace | placeholder → clean |
| **no** | no | none | invisible | — |

Only records absent from the host file add a line, so retries and duplicates cannot
push a panel past `totalFileCount`.

`MissingSrc` is counted during classification and affects the projection: with
`SkipMissingSource` those files write nothing, so they cannot advance the count and
the panel genuinely cannot complete. Without it they become placeholders that do
advance the count, but the manifest ends short.

### Per panel

| Condition | Decision | Report note |
|---|---|---|
| host ≥ line 15 | strip placeholders, send | `INDEX+HOST SENT` |
| reached via rebuilds | same | `INDEX+HOST SENT (n rebuilt)` |
| reached, n placeholders stripped | sends short | `SENT-SHORT (n missing)` |
| below total | refuse | `SKIPPED-INCOMPLETE` |
| below total, `-force` | sends short | `SENT-FORCED-SHORT (n missing)` |
| server was unreachable during the panel | **never sends**, even with `-force` | `SERVER-OFFLINE` |
| interrupted | does not send | `STOPPED` / `ABORTED` |

---

## 4. Reconstruction

Rebuilds a queue entry for an image on disk with no queue file — otherwise invisible
*and* blocking the panel below `totalFileCount`.

15 of the 17 lines are identical across a panel, so a surviving sibling supplies
everything except lines 7 and 8.

**Gate 1 — the filename must be permitted.** `IsAllowedName()` is the single
decision point, and it is called from **exactly one place**: `AddReconstructed`.
A file that has its own queue file is uploaded unconditionally — TrueTest decided it
belongs, and the tool does not second-guess it. These rules govern only the case
where there is no instruction and intent would otherwise be inferred from disk.

Names are canonicalised first: the panel's local PID (derived from the source folder
name `<localPID>_<yyyyMMddHHmmss>`) is replaced with `@PID@`, and the result is
lowercased. Everything else must match exactly. Digit-generalised patterns were
tried first and rejected as too loose — `^step\d+_\d+NIT_B\d+_imgY_Crop\.tif$`
accepts `step99_` and `_B999_`, which must not be sent. Note `step99_...UDIRVibMap`
turned out to be a legitimate file, which is why guessing at numbering conventions
was abandoned in favour of an explicit list.

Inputs, in precedence order:

| Source | File | Note |
|---|---|---|
| deny wildcards | `denied_filenames.txt` | checked first; also strips matching learned names |
| deny exact names | `denied_filenames.txt` | beats every other source |
| allow exact + wildcards | `allowed_filenames.txt` | hand-maintained; `!strict` disables all learning |
| learned | live queue files | correct by construction in production |
| remembered | `known_filenames.txt` | regenerated every scan; edits do not survive |
| archived | `Backedup*` folders | keeps names alive after the live queue drains |

Learning exists because the live queue alone shrinks: once every panel holding a
name has completed, nothing live remembers it was ever legitimate, and genuine files
start being rejected. That failure occurred in testing — `d994_gamma.hex` vanished
from the vocabulary after ~440 panels completed.

Wildcards (`*`, `?`) work in both hand-maintained files, converted to anchored
case-insensitive regexes by `GlobToRegex`. **Patterns only ever come from those two
files** — nothing learned becomes a pattern, so the vocabulary cannot silently widen.

`RuleFile()` locates all three: beside the exe first (they ship with the tool), then
the queue folder (per-folder override), then `<queue>\Log\Recovery` (legacy).
`known_filenames.txt` is written beside the exe, falling back to the queue folder if
that path is not writable. All three are excluded by name in `ScanQueueFiles`, so
they are never mistaken for queue files when the exe sits in the queue folder.

**Gate 2 — dest folder from a same-extension sibling.** No inference.

**Gate 3 — positional inference from a donor panel**, only when gate 2 can't be met
(e.g. a panel lost all four `.hex`/`.txt` queue files). Donor pairs must differ in
**exactly one** path segment (`.../POCB/IMAGE/...` vs `.../POCB/HEX/...`) with equal
depth in all three paths; otherwise it refuses. Only that segment is copied; the
panel's own server PID and timestamp are preserved. Donors also come from backups,
for the same shrinkage reason.

Logged per panel as `dest folder INFERRED from a donor panel for: .hex, .txt`. These
are the only dest paths that are *derived* rather than *read*.

> Assumes one product line per queue folder. Mixing products would break gate 3.

`EffectiveEntries()` filters rebuilt entries out when the option is off, so toggling
it either way takes effect immediately without a re-scan. `p.ReconstructApplied`
guards against a panel being processed twice.

---

## 5. Crash, interruption and outage safety

All state is on disk: which queue files remain, and the host file contents. Nothing
in memory is authoritative.

| Killed here | Next run |
|---|---|
| after upload, before host line | re-uploads (overwrite), then writes the line |
| after line, before queue retired | sees clean record, retires queue |
| after index up, before host | host still at total, sends both again |

Two mechanisms make that true, both of which were bugs first:

- **`AppendRecord` is idempotent per file.** Index and host are separate writes; a
  crash between them left index with the line and host without, and since dedupe
  reads only the host file, the next run appended a *second* index line and shipped
  a duplicate. Each file is now checked before appending.
- **Placeholder stripping uploads a temp copy** (`CleanCopyFor`). Stripping the
  original in place meant a crash before the upload left it permanently below
  `totalFileCount` — panel stranded with no queue files to recover from.

`CancelRequested` is checked at the top of the per-file loop, so the file in flight
completes. An interrupted panel never finalizes, even with `-force`.

### Connection failure vs upload failure

`TryUpload` returns `connError` separately from `False`, and the distinction decides
whether a queue file is consumed:

| | Upload failure | Connection failure |
|---|---|---|
| Detected by | session opened, `PutFiles`/`Check` threw | session never opened, or post-failure reachability check fails |
| Queue file | retired to `Backedup...\Failed` | **kept** |
| Host record | ` - failed` placeholder | nothing written |
| Panel outcome | may fire, short | `SERVER-OFFLINE`, never fires |

Without this split, one unattended run against a dead server would mark every file
failed and ship hundreds of near-empty manifests — permanently, from a fault that
had nothing to do with the files.

A network drop **mid-transfer** initially looks like an upload failure, since the
session had opened. After the retries are exhausted the code re-opens a session to
ask whether the server is still there; if not, the failure is reclassified as a
connection problem. That closes the one-file-per-outage misclassification.

### Not stalling, and self-healing

`TimeoutInMilliseconds` only bounds waiting for a response on an **established**
connection — it does not cap a connect to a dead port, which falls through to the OS
TCP timeout and blocks for minutes with no output. Two things fix that:

- **`ServerReachable()`** — a raw `TcpClient` connect with a 3 s timeout before
  WinSCP is involved at all. A dead server costs 3 s per file, not minutes.
- **A heartbeat timer** logs `... waiting Ns on <file> (attempt n/m)` every 5 s, so a
  slow transfer is visible rather than silent.

Once `ServerDown` is set, subsequent files fail instantly without retrying;
every `PROBE_INTERVAL_SECONDS` (30) one file is allowed a single attempt as a probe.
A success clears `ServerDown` and logs `server is reachable again`. The run is never
aborted — unattended operation is the design goal.

`AbortAfterConsecutiveFailures` remains for **file-level** failures only and
defaults to **0 (off)**.

> Single instance only, and `FTPUploaderVB` must be stopped. Two writers on the same
> host file with no locking defeats every guarantee above.

---

## 6. Command line

```
FTPRecovery.exe [root] [options]
```

| Option | Meaning |
|---|---|
| `root` / `-root <path>` | queue folder |
| `-go` | execute; **dry run without it** |
| `-reconstruct` | rebuild entries for images with no queue file |
| `-force` | send manifests even when short |
| `-retry <n>` | attempts per file (default 3) |
| `-pid <text>` | only panels whose PID contains this |
| `-skipmissing` | leave queue files whose image is gone |

Default root: the exe's own folder if it contains `.txt` files, else
`D:\Program\RVS\UploadQueue`, else the exe folder.

```bat
REM stop FTPUploaderVB first
FTPRecovery.exe
FTPRecovery.exe -reconstruct
FTPRecovery.exe -pid <onePID> -reconstruct -go
FTPRecovery.exe -reconstruct -go
FTPRecovery.exe -reconstruct -force -go
```

---

## 7. Output

Everything the tool writes lives beside the **exe**, not in the queue folder, so one
folder holds the program, its rules and its logs. `RecoveryLogDir()` falls back to
`<queue>\Log\Recovery` only if the exe's folder is not writable.

`<exe>\Log\Recovery\`:

| File | |
|---|---|
| `<stamp>_recovery.log` | full per-file trace |
| `<stamp>_recovery_report.csv` | one row per panel |
| `<stamp>_winscp.log` | raw WinSCP session log |
| `gui_error.log` | unhandled GUI exceptions |

Rule files, also beside the exe:

| File | |
|---|---|
| `allowed_filenames.txt` | hand-maintained; supports `*`/`?` and `!strict` |
| `denied_filenames.txt` | hand-maintained; beats every other source |
| `known_filenames.txt` | learned cache, rewritten every scan |

`RuleFile()` looks beside the exe first, then the queue folder (per-folder
override), then `<queue>\Log\Recovery` (legacy). All three are excluded by name in
`ScanQueueFiles`, `HostsInQueue` and `DefaultQueueRoot` — they are `.txt` in what is
often the queue folder, and treating one as a queue file caused two real bugs.

A shippable bundle is: `FTPRecoveryGUI.exe`, `WinSCPnet.dll`,
`allowed_filenames.txt`, `denied_filenames.txt`. `build.bat` seeds the two rule
files into `bin\` from the `.sample.txt` masters, but only when absent, so local
edits survive a rebuild.

```
PID,Total,HostBefore,Pending,Uploaded,Failed,MissingSource,AlreadyRecorded,HostAfter,Result
```

Successes and failures also append to each panel's own succeed/fail logs (lines 5
and 6), prefixed `FTPRecovery`, keeping the existing audit trail continuous.

Retired queue files are archived under `Backedup Recovery Queue\` in `Succeeded` /
`AlreadyRecorded` / `MissingSource` / `Failed` / `StaleIndexHost`, under their
original filenames — so one can be put back into the queue folder by copying it
straight back. They feed reconstruction (vocabulary and donors), so keep at least
one panel's worth or folder inference stops working once the live queue drains.
`MissingSource` doubles as a worklist:

```powershell
Get-ChildItem '...\Backedup Recovery Queue\MissingSource' |
  ForEach-Object { (Get-Content $_.FullName)[7] } | Sort-Object -Unique
```

---

## 8. Building

```
copy ..\FTPUploaderVB\lib\WinSCPnet.dll lib\
build.bat
```

Deploy the chosen exe plus `WinSCPnet.dll`. `WinSCP.exe` is located via line 3 of
each queue file.

- WPF is written in code, no XAML, so plain `vbc` builds it. Its assemblies are in
  `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\WPF\` — hence `/libpath`.
- `System.Windows.Forms.dll` is referenced only for `FolderBrowserDialog`.
- Icon via `/win32icon:`, skipped automatically if `FTPRecovery.ico` is absent.
- `/optionstrict+` is deliberate. It caught: `List.Count` shadowing LINQ's
  `Count(predicate)`; parameters named `path` shadowing `System.IO.Path`; a field
  named `grid` shadowing WPF's `Grid`; comments inside an object initializer;
  `CheckBox.IsChecked` being `Boolean?`.

### GUI threading

Scanning and uploading run on background tasks. The engine's log output is buffered
and flushed by a `DispatcherTimer` every 250 ms — marshalling each line individually
floods the dispatcher queue and starves input, which looked exactly like a freeze.
The post-upload re-scan also runs off the UI thread for the same reason. `SetBusy`
deliberately sets no wait cursor.

---

## 9. Testing

`ResetTestSet.bat` builds a 500-panel population covering every case in ~30 s.
See `TESTING.md`.
