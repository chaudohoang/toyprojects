# FTPRecovery

Repairs stalled `FTPUploaderVB` upload queues: uploads the files that never went,
then sends the index and host manifests so the panel finally completes.

Two programs, same engine:

| | |
|---|---|
| `FTPRecoveryGUI.exe` | double-click. Scan, review a table, upload per panel or all. |
| `FTPRecovery.exe` | command line, for scripting across many machines. |

---

## 1. The problem

`FTPUploaderVB` sends the index/host manifests from **one place only** — the
success path of `Upload()`:

```vb
File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + vbCrLf)
File.AppendAllText(sourceHostFile,  destFile + "@" + channelIndex + vbCrLf)
Dim uploadedCount As Integer = File.ReadAllLines(sourceHostFile).Length
If uploadedCount = Int32.Parse(totalFileCount) Then
    CreateIndexAndHostQueue(InfoFile)      ' <- the only trigger
End If
```

A file that fails but has **not yet used up its retries** writes nothing. Its queue
file stays on disk waiting for another go — but the main loop picks work with

```vb
OrderByDescending(GetCreationTime).Take(maximumUpload)
```

**newest first.** Once the folder holds more than `maximumUpload` files, older
retrying files fall out of the window and are never looked at again. Their fail
count freezes, they never write a line, the count never reaches `totalFileCount`,
and the manifests are never sent. The panel is stuck for good.

Second hole: when a failure *does* use up its retries and happens to be the last
file, the ` - failed` lines are written inside the `Catch` block, which never
re-checks the count. The queue file is then deleted, so nothing can retrigger it.

---

## 2. How a panel is put back together

Panels are grouped by **line 13** (`sourceHostFile`) — the field the completion
logic actually keys on. `totalFileCount` comes from **line 15** of the queue file,
per panel, so a 25-file recipe and a 40-file recipe both work with no changes.

For each file, three questions decide what happens: is there a queue file, is the
image on disk, and what does the host file already say?

| Queue file | Image on disk | Host file says | What happens | Host line after |
|---|---|---|---|---|
| yes | yes | nothing | upload it | new clean line |
| yes | yes | already done | skip, retire the queue file | unchanged |
| yes | yes | ` - failed` | **try again**; if it works, fix the line | ` - failed` → clean |
| yes | **no** | nothing | can't upload, note it | ` - failed` placeholder |
| yes | **no** | ` - failed` | leave alone | unchanged |
| **no** | yes | nothing | **rebuild the entry**, upload | new clean line |
| **no** | yes | already done | nothing owed | unchanged |
| **no** | yes | ` - failed` | **rebuild**, try again, fix the line | ` - failed` → clean |
| **no** | no | nothing | invisible, unrecoverable | — |

Only files *missing* from the host file add a line. Retries reuse their existing
line, duplicates add nothing. That is why a panel can never end up with more lines
than `totalFileCount`.

Then the panel decides whether to send the manifests:

| Situation | Decision | Shown as |
|---|---|---|
| host lines ≥ line 15 | strip placeholders, send index + host | `INDEX+HOST SENT` |
| got there using rebuilt entries | same | `INDEX+HOST SENT (n rebuilt)` |
| got there, but n placeholders stripped | sends — **manifest is short by n** | `SENT-SHORT (n missing)` |
| can't get there | **refuses** | `SKIPPED-INCOMPLETE` |
| can't get there, `-force` given | sends short anyway | `SENT-FORCED-SHORT (n missing)` |
| stopped or aborted mid-panel | does not send | `STOPPED` / `ABORTED` |
| no queue files at all | never discovered | absent |

A short manifest is never reported as a clean success. Filter the report CSV on
`SHORT` after any production run.

---

## 3. The four options

**Reconstruct from disk** — see section 4. Recommended on.

**Force incomplete** — send the manifests even when the count can't be reached,
i.e. deliberately ship a manifest listing fewer files than the panel should have.
Leave off unless you've decided that's what you want for a specific panel.

**Skip missing source** — changes what happens when a queue file points at an image
that's gone. Off (default): mark it ` - failed` so the count still advances and the
panel completes, short by that file. On: leave it alone, panel stays stuck, nothing
written. Turn it on if you want to investigate missing images first.

**Retries** — attempts per file, default 3. Each attempt reopens the FTP session,
so a brief network glitch gets a fresh connection.

Suggested first real run: **Reconstruct on, Force off, Skip missing off, Retries 3.**
That recovers everything recoverable without shipping any short manifests. Then look
at what's left as `SKIPPED-INCOMPLETE` and decide those case by case.

---

## 4. Reconstruction (`-reconstruct` / the checkbox)

**The problem it solves.** An image sitting on disk with no queue file is invisible:
it never uploads, *and* it holds the panel below `totalFileCount` forever. Nothing
else can fix that.

**Why it's possible.** 15 of the 17 queue lines are identical for every file in a
panel. A surviving sibling supplies all of them; only lines 7 and 8 need working out.

| Line | Where it comes from |
|---|---|
| 0–6, 9–16 | any surviving queue file in the same panel — including the channel, since a panel never mixes channels |
| 7 (source) | the file's actual path on disk |
| 8 (dest) | the dest folder for that file type + the same filename |

**Three gates, so nothing junk is ever sent.**

**Gate 1 — the filename must be one we've genuinely seen.** Not a pattern, an exact
match. Your template gives 25 known names:

```
d994_gamma.hex
nypucdata_@pid@_1st.hex          <- @PID@ is the only part allowed to vary
step01_0650nit_b048_imgy_crop.tif
...
```

So `step01_0650NIT_B999_imgY_Crop.tif` and `step99_0650NIT_B048_imgY_Crop.tif` are
**rejected** — an unexpected number is not treated as family. `thumbs.db` and
`*.bak` likewise. Verified by planting all four and confirming they were skipped.

The list is drawn from three places, so it never shrinks as work gets done:

| Source | Why needed |
|---|---|
| live queue files | the obvious one |
| `Log\Recovery\known_filenames.txt` | once every panel with a given filename is finished, no live queue file would remember that name existed |
| the `Backedup*` folders | archived real queue files |

That whitelist file is plain text and safe to review. Delete a line and that name
will never be reconstructed again.

**Gate 2 — dest folder from a sibling of the same file type.** No guessing at all.

**Gate 3 — only if gate 2 can't be met, the folder is worked out from a donor
panel.** There are two folders in play:

```
.hex, .txt  ->  /data1h1/HN_DATA/POCB/HEX  /07/28/.../<serverPID>/<stamp>/
.tif        ->  /data1h1/HN_DATA/POCB/IMAGE/07/28/.../<serverPID>/<stamp>/
                                    ^^^^^ exactly one segment differs
```

If a panel lost all four `.hex`/`.txt` queue files, nothing in it knows where `HEX/`
files go. So another panel is consulted, and **only that one differing segment** is
copied across — this panel's own server PID and timestamp are kept. It refuses
unless the donor pair differs in exactly one segment and all three paths have the
same depth. When this happens the log says so, per panel:

```
dest folder INFERRED from a donor panel for: .hex, .txt
```

Those lines are the only place a dest path is *worked out* rather than *read*. If
anything ever lands in the wrong folder, that's where to look. Everything else came
verbatim from a queue file.

> This assumes one product line per queue folder, which is how the machines are set
> up. Mixing products in one folder would break gate 3.

---

## 5. Interrupting it

**All progress is on disk, not in memory** — which queue files remain, and what the
host file says. Nothing is lost by stopping or crashing; the next scan works it out
again.

**Stop** finishes the file in flight, then stops. An interrupted panel never sends
its manifests, even with Force on, because the missing files are only missing
because we hadn't got to them yet.

**Closing or crashing** mid-run is safe. Worst case a file already on the server is
uploaded again — same path, same bytes.

| Killed here | Next run |
|---|---|
| after upload, before the host line was written | uploads again, then writes the line |
| after the line, before the queue file was retired | sees it's done, retires the queue file |
| after index went up, before host | sends both again |

Two things make that true, and both were bugs first:

- **Appends are idempotent per file.** Index and host are separate writes; a crash
  between them used to leave index with the line and host without, and because
  dedupe only reads the host file, the next run added a *second* index line and
  shipped a duplicate. Each file is now checked before appending.
- **Placeholder stripping doesn't touch the original.** A cleaned **copy** is
  uploaded. Stripping the real file first meant a crash before the upload left it
  permanently below `totalFileCount` — panel stranded, no queue files left to
  recover from.

**If the server is down**, 25 consecutive failures aborts the run rather than
burning every panel's retries on timeouts:

```
*** ABORTING: 25 uploads failed back-to-back. The server looks unreachable.
*** Remaining panels left untouched. Fix the connection and re-run;
*** already-uploaded files are recorded and will not be sent twice.
```

One success resets the counter, so isolated failures don't trip it.

> **Stop FTPUploaderVB before running.** Both write to the same host file with no
> locking, and that defeats every guarantee above.

---

## 6. The GUI

Drop `FTPRecoveryGUI.exe` + `WinSCPnet.dll` anywhere and run it. Opens maximized.

The queue path is filled in for you: the exe's own folder if that folder holds queue
files, otherwise `D:\Program\RVS\UploadQueue`. Browse to change it.

**Start Scan** builds the table. Nothing is uploaded.

| PID | Total | Host now | Done | Retry | New | Rebuilt | Projected | Verdict | |
|---|---|---|---|---|---|---|---|---|---|
| TSN…001 | 25 | 0 | 0 | 0 | 25 | 0 | 25 / 25 | READY - 25 to upload | `Upload` |

`Host now` is how many lines the host file already has — 0 means nothing recorded
yet (the file doesn't exist until the first upload). `Projected` is where the count
will land.

Row colours:

| Colour | Meaning |
|---|---|
| white | pending, will complete |
| blue | pending, only the manifests left to send |
| pink | pending, **can't** complete |
| green | finished — `INDEX+HOST SENT` |
| amber | finished, but short / skipped / stopped |

Pending rows sort to the top; finished ones collect below with their result, so the
table becomes a record of the session. Their `Upload` buttons are disabled. On
finished rows the numbers mean the end state: `Host now` is the final count and
`Done` is how many files *that run* sent.

Clicking **Start Scan** again clears the finished rows and starts fresh.

**Ticking an option does nothing on its own** — options are read when you click
Upload, so ticking Reconstruct and pressing Upload applies it to just those panels.
No re-scan needed either way.

**Stop** works throughout. The window stays responsive; the progress bar, the
`461 / 496` counter and the streaming log are the signs of life. (There's
deliberately no spinning cursor — it reads as "frozen" when it isn't.)

The log pane trims itself on long runs. **The log file is the complete record.**

---

## 7. The command line

```
FTPRecovery.exe [root] [options]
```

| Option | Meaning |
|---|---|
| `root` / `-root <path>` | queue folder; defaults as in section 6 |
| `-go` | actually do it. **Without this it's a dry run.** |
| `-reconstruct` | rebuild entries for images with no queue file |
| `-force` | send manifests even when short |
| `-retry <n>` | attempts per file (default 3) |
| `-pid <text>` | only panels whose PID contains this |
| `-skipmissing` | leave queue files whose image is gone |

```bat
REM 1. stop FTPUploaderVB
FTPRecovery.exe                                  REM look first
FTPRecovery.exe -reconstruct                     REM look, including rebuilds
FTPRecovery.exe -pid <onePID> -reconstruct -go    REM prove it on one
FTPRecovery.exe -reconstruct -go                  REM the rest
FTPRecovery.exe -reconstruct -force -go           REM last resort for leftovers
```

---

## 8. What it writes

In `<queue>\Log\Recovery\`:

| File | Use |
|---|---|
| `<stamp>_recovery.log` | full per-file trace — the complete record |
| `<stamp>_recovery_report.csv` | **one row per panel — start here** |
| `<stamp>_winscp.log` | raw FTP protocol log |
| `known_filenames.txt` | the reconstruction whitelist (section 4) |

```
PID,Total,HostBefore,Pending,Uploaded,Failed,MissingSource,AlreadyRecorded,HostAfter,Result
```

```powershell
# how did the run go?
Import-Csv '<stamp>_recovery_report.csv' | Group-Object Result | Sort-Object Count -Descending

# which panels shipped short manifests?
Import-Csv '<stamp>_recovery_report.csv' | Where-Object Result -match 'SHORT'
```

Successes and failures are also appended to each panel's own succeed/fail logs
(lines 5 and 6 of its queue file), prefixed `FTPRecovery`, so the existing audit
trail stays continuous.

Retired queue files are archived under `Backedup Recovery Queue\` in
`Succeeded` / `AlreadyRecorded` / `MissingSource` / `Failed` / `StaleIndexHost`,
timestamp-prefixed. These are useful beyond an audit trail:

- **`MissingSource` is a worklist** — images the customer will never receive:
  ```powershell
  Get-ChildItem '...\Backedup Recovery Queue\MissingSource' |
    ForEach-Object { (Get-Content $_.FullName)[7] } | Sort-Object -Unique
  ```
- **They feed reconstruction** — filename whitelist and donor panels both fall back
  to them. Keep at least one panel's worth, or folder inference stops working once
  the live queue is drained.
- Prune old ones freely; keep `MissingSource` and `Failed` longer than `Succeeded`.

---

## 9. Building

```
copy ..\FTPUploaderVB\lib\WinSCPnet.dll lib\
build.bat
```

Produces both exes in `bin\`. Deploy the one you want plus `WinSCPnet.dll` —
`WinSCP.exe` is found via line 3 of each queue file, so it doesn't need copying.

Notes for anyone touching the build:

- One engine file, two entry points (`/main:Program` vs `/main:WpfProgram`), so the
  logic exists once.
- WPF is written in code, no XAML, so plain `vbc` builds it. Its assemblies live in
  `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\WPF\`, hence the `/libpath`.
- `System.Windows.Forms.dll` is referenced only for the folder-picker dialog.
- `/optionstrict+` is on deliberately — it caught `List.Count` shadowing LINQ's
  `Count(predicate)` and parameters named `path` shadowing `System.IO.Path`.
- The icon is optional; the build skips it if `FTPRecovery.ico` is missing.

---

## 10. Queue file format (17 lines, 0-based)

| Line | Field |
|---|---|
| 0–3 | host, username, password, WinSCP.exe path |
| 4–6 | session log, succeed log, fail log |
| 7–8 | source file → dest path |
| 9–11 | OutputIndexInfoFile, sourceIndexFile, destIndexFile |
| 12–14 | OutputHostInfoFile, sourceHostFile, destHostFile |
| 15–16 | totalFileCount, channelIndex |

Index and host lines look like `destPath@channelIndex`, the channel coming from
line 16. **When parsing, strip the `@channel` before taking the filename after the
last `/`** — forgetting this caused 1,555 phantom rebuilds during development.

Two identifiers vary per panel:

| | Appears in |
|---|---|
| **local file PID** (`AAA`) | queue filenames, the `E:\POCB\...` source folder, session-log folder |
| **server PID** (`A4XN6600PN05BD5`) | every `/data1h1/...` dest path, the `.idx` and host filenames |

`FTPUploaderVB` calls the *server* PID the PID, and so does this tool.

---

## 11. Testing

`ResetTestSet.bat` — one click, ~30 s, builds a 500-panel population covering every
case. See `TESTING.md`.

---

## 12. Still owed in FTPUploaderVB itself

This tool is a mitigation. The real fixes:

1. Move the count check into a helper called from **both** the `Try` and the `Catch`
   paths, before `File.Delete(InfoFile)`.
2. `uploadedCount = totalFileCount` → `>=`.
3. `OrderByDescending` → `OrderBy`, so retries get priority instead of starving.
4. Guard `failMessage.IndexOf(".")` returning `-1` in `UpdateSummaryLogFail` — it
   throws from inside a `Catch` block and kills the rest of that cycle's batch.
5. Only call `UploadIndexAndHost` on a freshly written `Output*InfoFile`, never a
   stale leftover.
