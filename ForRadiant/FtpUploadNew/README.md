# FTP Upload

Standalone background uploader for LGD panel images, **with its manager UI built into the
same program**. Runs completely separately from TrueTest — the two communicate only through
files, so nothing here can affect tact time.

One process does both jobs:

- the **upload engine** runs on background threads from the moment the program starts
- the **manager window** is just a viewer onto that engine, opened from the tray icon

Closing the window hides it to the tray; uploading carries on untouched.

Work arrives as **`.panel` handoff files** that TrueTest drops in the queue folder — one small
file per panel, from which this program derives the file list (via the recipe), the destination
paths, and the index/host manifests it creates and sends itself. It replaces both the old live
uploader (FTPUploaderVB) and the recovery tool (FTPRecovery). See **Interface with TrueTest**.

There is **one list of work** — the in-memory job list, rebuilt on startup from the jobs
file plus today's raw log. "What still needs uploading" is derived from it on the fly (any
file still pending with retries left); there is no second wait-list file. The manager window
shows that one list through two tabs:

- **Today Jobs** — every panel from today, grouped into cards.
- **NG List** — the **NG-retry console**, a manual recovery tool that runs **separately from the
  live line**. A day picker loads any day's NG items (files that ended Failed or Timed Out),
  rebuilt from that day's jobs + raw logs. You choose the IP (Auto / Primary / Secondary) and
  press **Auto Retry** (unlimited retries until you press Stop) or **Retry** on one item. It
  uploads on its own pump — today's live uploading is untouched — and records outcomes to a
  separate per-day **ng-retry log**; once an item succeeds there it drops off the list.

Built with **.NET 8 + WPF**, using **FluentFTP 54.2.0**. WinForms is referenced only for the
tray icon (WPF has none built in). No DLLs to download by hand — `dotnet build` restores
everything, and there is no WebView2 or browser runtime involved.

## Build

Easiest — just run **`build.bat`** (double-click, or from a terminal). It restores, builds
Release, and publishes the deployable package to `publish\`. Options:

```
build.bat          self-contained single file -> .\publish   (default)
build.bat run      ...then launch the manager window
build.bat fdd      framework-dependent build (needs .NET 8 Desktop Runtime on the target PC)
build.bat clean    delete bin\ obj\ publish\ and exit
```

It stops the worker (and the scheduled tasks), waits for the lock to release, and **deletes the
old `FtpUpload.exe` before publishing** — overwriting a single-file exe in place is what causes
the "rebuilt exe won't start" corruption, so the fresh write avoids it.

> **Confirm you're running the fresh build.** The window header shows the exe's build time
> (e.g. `… · 5 attempts · build Aug 17 11:58`). If it doesn't match the build you just ran,
> an old `FtpUpload.exe` was still running and locked — close it (tray → exit, or
> `taskkill /f /im FtpUpload.exe`) and build again. Config-only changes (like the fail rate)
> take effect without a rebuild; UI changes need the exe rebuilt and published to the folder
> the test scripts launch from.

Or run the same commands by hand:

```
dotnet build
dotnet publish -c Release -p:PublishSingleFile=true -o publish
```

Build on Windows with the **.NET 8 SDK (Windows Desktop workload)** — WPF/WinForms will not
build on a plain SDK or on Linux. `dotnet build` restores FluentFTP for you; no DLLs to fetch
by hand, and there is no WebView2 or browser runtime involved.

The publish folder is the deployable package: `FtpUpload.exe` (self-contained, .NET runtime
bundled), a few native WPF DLLs, and the scripts below.

### Startup timing, measured

| Build style | First ever run | Later runs |
|---|---|---|
| Self-contained single file (~150 MB) | 10.8 s | 0.7 s |
| Self-contained, loose files (244 files) | 5.5 s | 0.67 s |
| Framework-dependent (148 KB exe) | 1.5 s | 0.71 s |

The first-run delay scales with the size of the binary, i.e. it is antivirus scanning a newly
seen executable — it happens once per machine per version, then the file is cached. The
default build trades that one-time delay for needing nothing installed on the target PC. If
that delay is unwanted, publish with `--self-contained false` and install the
**.NET 8 Desktop Runtime** on the inspection PCs instead.

## Install on the inspection PC

1. Copy the publish folder to e.g. `C:\FtpUpload\`.
2. Run `FtpUpload.exe` once to generate `config.json`, then edit it (see below).
3. Run `install_task.bat` **once**. It registers two scheduled tasks:
   - **FTP Upload Worker** — at log on
   - **FTP Upload Worker Keepalive** — every 5 minutes, revives it even if the watchdog itself died

No runtime prerequisites: WPF ships inside the self-contained build, so nothing needs
installing on the inspection PC.

## Running / stopping

| Action | How |
|---|---|
| Start now, no reboot | `schtasks /run /tn "FTP Upload Worker"` |
| Open the manager | double-click the tray icon, or run `FtpUpload.exe --show` |
| Stop deliberately | `stop_worker.bat` (watchdog stands down; keep-alive revives it in 5 min unless you also run `uninstall_task.bat`) |
| Remove the tasks | `uninstall_task.bat` |

Exit codes tell the watchdog what to do: **2** = another instance already running,
**3** = STOP requested (both mean "do not restart"); anything else = restart in 5 s.
A global mutex guarantees two copies never upload at once.

## Configuration (`config.json`)

| Key | Meaning |
|---|---|
| `PrimaryHost` / `SecondaryHost` | CNS dual IP. With the defaults, the first 3 attempts use the primary and the last 2 fail over to the secondary (see `PrimaryRetries` / `SecondaryRetries`). |
| `Port`, `User`, `Password` | FTP credentials. |
| `FtpSecure` | `None`, `Explicit` or `Implicit` (FTPS). |
| `RemoteBaseFolder` | Used when a job line omits the remote path. |
| `TimeoutSecondsOverride` | Per-file FTP operation timeout in seconds (connect + transfer), set directly. Default **20**, with a 5 s floor. |
| `PrimaryRetries` | Retries on the **primary** IP after the initial attempt. Default 2 (initial + 2 = 3 attempts on the primary). |
| `SecondaryRetries` | Retries on the **secondary** IP after failing over. Default 2. Set to 0 to disable failover. |
| `PanelTimeoutSeconds` | Per-panel deadline, measured from when the panel's first file starts uploading. If it still has unfinished files after this long, the remaining (and in-flight) files are skipped to the NG list as **Timed Out**. 0 disables it. Example: 120. |
| `NgRetryCooldownSeconds` | Delay between attempts in the NG-retry pump (unlimited retries). Default 5. |
| `JobsFolder`, `LogFolder`, `StateFolder` | Working folders. Use **UNC paths**, not mapped drives. |
| `QueueFolder` | Where TrueTest drops `{PID}_{DateTime}.panel` handoff files (and where the old WinSCP queue lived). The app watches it for `*.panel`. Default `D:\Program\RVS\UploadQueue`. |
| `RecipePath` | The upload recipe (`allowed_filenames.txt`): filename patterns deciding which files in a panel's source folder are uploadable. A **relative** path resolves against the **exe folder**; absolute is used as-is. |
| `LogRetentionDays` | Auto-delete date-stamped logs/reports (in the Log/Jobs folders) and old per-day panel-backup subfolders older than this many days, at startup and each day rollover. `0` = keep everything forever. A panel that matches **no** recipe files is moved to a `rejected` subfolder instead of being lost. |
| `SimulateFailurePercent` | **Testing only.** Per-attempt chance (0–100) of a deliberately injected failure. **Leave at `0` in production.** |
| `SimulateUploadMs` | **Testing only.** Artificial per-attempt transfer time in ms, so the demo isn't instant (elapsed counters tick, panel timeouts trigger under load, the NG highlight is readable). `0` = instant. **Leave at `0` in production.** |
| `SimulateFastDaySeconds` | **Testing only.** If > 0, advances a simulated calendar day every N seconds so the **day-rollover** can be exercised without waiting for real midnight. `0` = real clock. **Leave at `0` in production.** |

## Testing & stress-testing the `.panel` intake

`gen_panels.ps1` is the harness for the upload path: it builds sample `.panel` handoffs plus
fake source files across a mix of scenarios and drops them into the config's real `QueueFolder`.
It reads `config.json` (never writes it), seeds `config.json`/`allowed_filenames.txt` from
`config.default.json` if they're missing, and wipes only its own scratch tree
(`D:\FtpUploadDemo\src`). Fake files are **1 KB** each by default.

Double-click wrappers:

```
gen_500.bat        clean? no  - stage 500 panels and launch the app
gen_5000.bat       clean? no  - stage 5000 panels (staged; start the app yourself)
stress_500.bat     clean? YES - full clean (no prompt) + stage 500 + launch   (repeatable)
stress_5000.bat    clean? YES - full clean (no prompt) + stage 5000 (staged)  (repeatable)
```

Use the **`stress_*`** scripts for repeatable runs: the generator uses deterministic `TSN` PIDs,
so without a full clean first the same-day dedup marks a re-run "already resolved" and it looks
instantly done. `stress_*` clears jobs/logs/state history + test panels + scratch each time, so
every run uploads fresh. Append args to override, e.g. `stress_5000.bat -FileKB 4`.

Run the generator directly for custom mixes:

```
gen_panels.bat -Panels 250                       # custom count, default mix
gen_panels.bat -Panels 5000 -PctFull 100         # pure throughput (few failures/skips)
gen_panels.bat -NoLaunch                          # build the set without starting the app
```

To also exercise the **NG / recovery** path under load, set `SimulateFailurePercent` > 0 in
`config.json` before a run (it's a test-only knob — leave it at `0` in production). `_progress.ps1`
prints a live tally (succeeded / failed / pending, files on the server, stray `.part` count) a
few seconds into a run.

Weighted scenarios, adapted to the design where this tool makes the manifests:

| Scenario | What it checks |
|---|---|
| **FULL** | all recipe files present → ingest, upload, finalize |
| **MISSING** | a few recipe files absent → count = `folder ∩ recipe`, not a fixed number |
| **JUNK** | extra non-recipe files present → filtered out, not counted |
| **RESUME** | a partial `{PID}.idx`/host already on disk → create-or-resume |
| **NOTREADY** | a `.panel` with no `SourceFolder` → intake skips it (phase-1) |

## Interface with TrueTest

### `.panel` handoff (the current path)

TrueTest's whole job is to write **one small `.panel` file per panel** into `QueueFolder` and
drop the panel's images in a source folder — nothing else. No per-file queue generation, no
connection settings, no manifests. The file is `Key=Value`, named `{PID}_{DateTime}.panel`:

```
Model=...
EQPID=...
PID=...
DateTime=...
UploadIndexPath=...       server path for the panel's index manifest
UploadHostPath=...        server path for the panel's host manifest
SourceFolder=...          the folder holding this panel's files
ChannelIndex=...
```

TrueTest writes it **temp-then-rename** (`.panel.tmp` → `.panel`), so a visible `.panel` is
always complete. On the TrueTest side this is an `FTP Upload Method` switch (**New** = write the
panel file, **Old** = the original in-app WinSCP queue), so the changeover is reversible per line.

From that, this program derives everything itself:

- **which files to upload** — the source folder filtered by the **recipe** (`RecipePath`), which
  is also the total file count (`folder ∩ recipe`). The recipe is a list of filename patterns
  (`*` / `?` wildcards, `@PID@` = the PID), editable as a plain text file beside the exe.
- **destination paths** — the HEX path per file is rebuilt with the exact rule the old TrueTest
  code used (`{ServerRoot}/POCB/HEX/{MM}/{DD}/{EQPID}/{Model}/{PID}/{DateTime}/{file}`); the index
  and host destinations come straight from the `.panel` file. `ServerRoot` is recovered from the
  panel's own `UploadIndexPath`, so the HEX root can't drift from the index/host root.
- **connection, retry, logging** — all from `config.json` and the existing engine.

A `.panel` that isn't ready yet (no `SourceFolder`) is left alone; once ingested it is deleted or
moved to `ProcessedPanelFolder`. If the recipe is missing/empty the panel is **left in place**
(so fixing the recipe lets it ingest); if the recipe loads but matches nothing it is moved to a
`rejected` subfolder rather than lost.

### Index + host manifests (panel completion)

The index (`{PID}.idx`) and host (`{PID}_{DateTime}.txt`) manifests are the customer-facing
"panel complete" signal. **This program creates and sends them** — TrueTest no longer does. On
ingest, every data file gets a line `{destPath}@{channelIndex} -pending` written to both
manifests (in the source folder). As each file uploads the ` -pending` is stripped to a clean
line; a file whose source is genuinely gone has its line dropped. When **no `-pending` remains**,
the panel is finalized: the clean index and host are uploaded to `UploadIndexPath` /
`UploadHostPath`. A file that keeps failing simply leaves its line `-pending`, so a panel **waits**
to finalize rather than ever shipping a short manifest. Finalizing is guarded by a `.sent`
sentinel so the live engine and the NG pump can never both send the same panel, and so a completed
panel is never re-sent after a restart.

### Jobs file (internal record)

Ingested panels are also written to `{JobsFolder}\YYYYMMDD_jobs.txt`, one line per file, so a
restart re-loads the panel and the NG console can reconstruct its files:

```
PID|FileName|LocalPath|RemotePath|IndexSrc|HostSrc|UploadIndexPath|UploadHostPath|ChannelIndex
```

The first four fields are the legacy format (`PID|FileName|LocalPath[|RemotePath]`); the extra
fields carry the panel's manifest metadata and are ignored by any legacy reader. The demo tools
still write the legacy 3–4-field form.

### Commands

Appended to `{StateFolder}\commands.txt`:

```
RESULT|PID              write the snapshot log for PID, then preempt in its favour
FORCE|PID|FileName      jump that file to the front of the queue
DELETE|PID              drop the whole job
STOP                    intentional shutdown
```

`RESULT` is the one requiring a TrueTest-side change: it must be written at the exact moment
the Result value is sent to the fixture, since both the snapshot log and the preemption rule
key off that instant.

The UI does **not** use this channel — being in the same process, its buttons call the engine
directly.

## Outputs

**`{LogFolder}\YYYYMMDD_rawlog.txt`** — Log 1, strictly append-only, one line per attempt:

```
PID|FileName|Status|SucceedTime|FailCount|FailTimes|Attempts|MaxRetries|Host|PanelStatus
```

Each line is a full snapshot of that file's state at that moment, so a reader reduces the file
to current state by taking the **last** line per `PID|FileName`. `Status` is `SUCCEEDED`,
`FAILED`, `TIMEDOUT` (skipped by the panel timeout), or `PENDING`.

The `Host` field records which CNS IP each attempt used — the only way to confirm afterwards
that failover to the secondary happened. `PanelStatus` (last field) is that file's panel status
at the moment the line was written: `INPROGRESS` / `SUCCESS` / `FAILED` / `TIMEDOUT`. Both are
appended last, so any reader expecting the original 8 fields keeps working. A failed file reads
like this, with the switch visible at attempt 4:

```
attempt 1/5  retries 0/4  PENDING   host 10.99.99.1
attempt 2/5  retries 1/4  PENDING   host 10.99.99.1
attempt 3/5  retries 2/4  PENDING   host 10.99.99.1
attempt 4/5  retries 3/4  PENDING   host 10.99.99.2
attempt 5/5  retries 4/4  FAILED    host 10.99.99.2
```

**`{LogFolder}\YYYYMMDD_snapshot.txt`** — Log 2, written at Result timing:

```
2026-08-15 05:56:46|PNL-001|X|File1:O|File2:X
```

Overall is `O` only when 100 % of the panel's files are uploaded at that instant.

There is **no** `ng_waitlist.txt` any more. Outstanding work is not persisted to its own file:
the raw log above already records the final state of every file, so on startup the engine
replays it to recover what is done (SUCCEEDED) and what is dead (FAILED), and everything else
is simply re-read from the jobs file and treated as still-pending. The NG List tab is a live
view of the FAILED files from that same derivation.

**`{LogFolder}\YYYYMMDD_ngretrylog.txt`** — the NG-retry log, one file per original day, written
only by the NG-retry console (separate from the live raw log). Same pipe-delimited shape, with the
running retry count in the attempts fields and `NGRETRY` as the last field. When an item succeeds
here it is removed from that day's NG list.

**NG-retry HTML report** — run `_nghtmllog.bat` (double-click for today, or `_nghtmllog.bat 20260816`)
to turn a day's `ngretrylog.txt` into a report: one row per NG item with its original reason
(Failed / Timed Out), whether it was **Recovered** or is still failing, how many retries it took,
and each retry attempt's IP + outcome.

**HTML report** — run `_htmllog.bat` (double-click for today, or `_htmllog.bat 20260816` for a
day) to turn that day's `rawlog.txt` into a colour-coded `YYYYMMDD_htmllog.html` and open it.
It's laid out like the app: **one card per panel** with the PID, an `X/Y succeeded &middot; Z
failed` tally and an overall status badge (In Progress / Success / Failed), and inside each card
a row per file with final status, succeed / fail times, retries used, and every attempt's IP
shown as Primary or Secondary with its outcome — so you can read the failover per file. Files
that have not been attempted yet still appear (as Pending), because it reads the jobs file for
the full list and overlays the rawlog outcomes — so the panel tally and status match the UI. It
also summarises per-IP success/fail totals and the result-timing snapshots, and reads
`config.json` (next to the exe) to find the log and jobs folders and label the host IPs.

## Behaviour notes

- **One file in flight at a time**, so the timeout and preemption always apply to a single
  unambiguous transfer.
- **Temp-name-then-rename** — uploads go to `<remote>.part`, renamed only on success, so an
  aborted transfer never leaves a partial file under its real name on CNS.
- **One list, no wait-list file** — all work is derived from the single job list, not kept
  in a second file. Fewer moving parts and no chance of the two lists disagreeing.
- **Preemption is not a failed attempt** — a skipped file keeps its remaining retries and
  stays pending; idle catch-up picks it up once the line is free.
- **A failed file goes to the NG List for manual retry** — once a file uses up `MaxAttempts`
  real attempts it is marked FAILED and is **never retried automatically** (not by idle
  catch-up, not on restart). It appears on the **NG List** tab, where the operator retries it
  with **Retry** (or clears the whole list with **Retry All**); either resets its attempt
  budget and re-queues it for a fresh run. Its failure history (`FailTimes`) is kept as a record.
- **Panel timeout** — when `PanelTimeoutSeconds` > 0, each panel gets that long to finish,
  measured from when its **first file starts uploading** (not from when it was received) — so a
  panel still waiting its turn isn't on the clock, and each panel's window starts fresh when its
  turn comes. If a panel can't finish in time, its remaining files, and any in-flight file of
  that panel, are skipped and marked **Timed Out** (a distinct terminal state from Failed, shown
  with its own badge and logged as `TIMEDOUT`). They go to the NG list; retrying one from there
  re-arms the panel's clock. The panel's status (`In Progress` / `Success` / `Failed` /
  `Timed Out`) is written on every raw-log line.
- **Panel completion (index + host)** — a panel is "complete" only when every one of its files
  is uploaded (or its source is gone). The tool tracks this with ` -pending` markers in the
  index/host manifests it writes in the source folder, and sends the finalized manifests only
  when none remain — so a transient server outage makes a panel **wait**, never ship short. The
  NG pump updates the same manifests when it recovers a file, and can finalize a **past-day**
  panel itself (which has no live job); a `.sent` sentinel stops the live engine and NG ever
  double-sending.
- **Day rollover** — when the calendar day changes while the app is running, a single check in
  the watch loop fires once: it lets the in-flight upload finish, then marks every still-pending
  file from the old day **Timed Out** and logs it to the **old day's** raw log (so it lands in
  that day's NG list), clears the live job list for the fresh day, and re-points the NG console to
  the new day. New work flows in from the new day's jobs file automatically. If the app is
  restarted each shift instead of running across midnight, this simply never triggers.
- **The window opens maximized** but is a normal resizable window, not kiosk/fullscreen.
- **The Today Jobs list follows the upload** — the file being uploaded is framed with a blue
  highlight box, and the list auto-scrolls to keep it in view, moving to each next file as it
  goes. Scrolling by hand (wheel, scrollbar, or Page/Home/End keys) pauses this; it resumes
  5 seconds after the last manual gesture. It only drives the Today Jobs tab, and does nothing
  while the window is hidden.
- **The window is built on first open and kept**, so reopening from the tray is instant.
- **No UI work happens while the window is hidden** — every refresh timer checks `IsVisible`
  first. Measured with 40 jobs / 160 file rows and uploads churning continuously:

  | State | CPU | Memory |
  |---|---|---|
  | Hidden in tray (normal production state) | 1 % of one core | 19 MB |
  | Window open, 160 rows | 12.7 % | 153 MB |

- The job list is a virtualized `ListBox`, so only the cards actually on screen are built.
- View models raise `PropertyChanged` **only for values that actually changed**; raising them
  unconditionally made WPF re-render the whole list several times a second.
- `timeout /t` is deliberately **not** used in `run_watchdog.bat`; it fails instantly when
  stdout is redirected (which Task Scheduler does) and the loop would spin at full CPU.

## Not done yet

- Protocol assumed **FTP/FTPS**. If CNS is actually SFTP, `FtpTransfer.cs` must switch from
  FluentFTP to SSH.NET — confirm against `SessionOptions` in the existing FTPUploaderVB.
- `ValidateAnyCertificate = true` is set for FTPS; tighten if CNS presents a proper cert.
- No day-picker in the UI yet — it shows the jobs currently loaded in memory. The calendar
  view from the mockup can be added once multi-day job files are being read back on startup.
- Config is edited as JSON; a **Settings screen in the UI** (connection, folders, and a viewer/
  editor for the recipe) is still to do. The recipe is editable now as the plain-text
  `allowed_filenames.txt`.
- The tray icon uses the stock Windows application icon; a proper .ico can be dropped in.
- The old TrueTest **`GenerateWinSCPUploadInfo`** queue generation is left in place behind the
  `FTP Upload Method = Old` switch, for reversibility during cutover; it can be removed in a later
  pass once the `.panel` path is proven on the line.
