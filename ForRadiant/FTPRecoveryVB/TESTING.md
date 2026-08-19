# Testing FTPRecovery

## One click

```
ResetTestSet.bat
```

~30 seconds. Deletes every generated panel (queue files, dummy images, index/host
files, recovery logs, backups, fail counters) and builds a fresh 500-panel
population covering every case the recovery tool has to handle.

Needs three things, all checked **before** it deletes anything:

1. `MakeTestQueues.ps1` in the same folder as the `.bat`
2. `D:\Program\RVS\UploadQueueTemplate\` holding one real panel's queue files
3. The drives named inside those queue files (`D:` and `E:`) must exist

Settings live at the top of the `.bat`: `COUNT`, `SIZEKB`, the scenario weights,
`SEED`, and `REMOVE_AAA`.

---

## What a template is

One panel's queue `.txt` files, nothing else — 25 of them in the current template.
The generator reads everything it needs from them and creates the dummy images
itself. You never pre-create images.

Each clone gets fresh identifiers so no two panels collide, locally or remotely:

| Template token | Becomes | Appears in |
|---|---|---|
| `AAA` — local file PID | `TSTPIDnnnnnn` | queue filenames, `E:\POCB\...` source folder, session-log folder |
| `A4XN6600PN05BD5` — server PID | `TSNnnnnnnnnnnnn` | every `/data1h1/...` dest path, `.idx` and host filenames |
| `20260819192902` | per-panel | local source folder |
| `20260728000111` | per-panel | dest IMAGE path, host filename |

Timestamps are derived from the **template's own** stamps, not `Get-Date`, so panel
N always gets the same value. Without that, a second pass over a subset invents new
folders and orphans the first pass's.

---

## The six scenarios

Each panel is dealt one of these, so every branch of the recovery logic gets
exercised:

| Scenario | Host/index file | Queue files | What it tests |
|---|---|---|---|
| `FRESH` | none | all 25 | the normal path |
| `PARTIAL` | some clean lines | those deleted | **the real production stall** |
| `DUP` | some clean lines | **still there** | no double-uploading |
| `RETRY` | ` - failed` lines | **still there** | retry, then fix the line in place |
| `ORPHANFAIL` | clean + failed lines | those deleted | placeholder stripping, short manifests |
| `INCOMPLETE` | none | 2–5 deleted | reconstruction; refusal without it |

Separately, `PctMissingSource` panels also lose 1–3 images from disk — that's the
"queue file exists but the image is gone" case.

`-RandomSeed 20260819` makes the whole population reproducible, so a failing case
can be recreated exactly.

---

## Results, verified

Default weights, 500 panels:

```
FRESH       108      DUP          75      ORPHANFAIL   50
PARTIAL     130      RETRY        78      INCOMPLETE   59
missing-source 77
```

A dry run matched the generator exactly:

| Generated | Recovery reported |
|---|---|
| RETRY 78 | 78 panels with `failed-retry > 0` |
| DUP 75 | 75 panels with `done > 0` |
| INCOMPLETE 59 | 59 `CANNOT COMPLETE` |

Turning reconstruction on:

| | Off | On |
|---|---|---|
| Panels that can complete | 438 | **497** |
| `CANNOT COMPLETE` | 59 | **0** |
| Entries rebuilt from disk | — | 206 |
| Rejected as unrecognised | — | 0 |

**The dedupe arithmetic is the bit worth understanding.** A `DUP` panel reports:

```
host now: 4   pending: 25 (4 done, 0 failed-retry, 21 new)
projected: 4 + 21 = 25          <- not 29
```

The 4 already-recorded files are not re-uploaded and add no lines, so the count
can't overshoot `totalFileCount`. That's the duplicate protection, provable before
anything touches the FTP server.

**497 rather than 500** because `PARTIAL` and `ORPHANFAIL` can roll high enough to
delete *every* queue file for a panel. Those become invisible — and genuinely
unrecoverable, since the queue file is the only source of credentials and dest
paths. Production can reach the same state.

A full run with reconstruction on:

```
Panels scanned         : 496
Panels index/host sent : 496
  ...of which SHORT    : 103
Files uploaded         : 9248
Files failed           : 0
Files source missing   : 107
Failed->clean retries  : 197
Rebuilt from disk      : 206
```

The 103 short manifests line up with the 107 missing images — deliberately deleted
by the generator. On production those would be something to investigate, not accept.

---

## Testing the filename gate

Reconstruction only rebuilds a file whose exact name came from a real queue file.
Confirmed by deleting one genuine queue file and planting four impostors in the
same folder:

| File | Result |
|---|---|
| `step01_0650NIT_B048_imgY_Crop.tif` (real, queue deleted) | **rebuilt** |
| `step01_0650NIT_B999_imgY_Crop.tif` | rejected |
| `step99_0650NIT_B048_imgY_Crop.tif` | rejected |
| `thumbs.db` | rejected |
| `step01_0650NIT_B048_imgY_Crop.tif.bak` | rejected |

```
skipped 4 unrecognised file(s) in the source folder:
   step01_0650NIT_B999_imgY_Crop.tif (not a known filename for this product)
   ...
```

Note the test set only ever contains legitimate files, so `0 skipped` on a normal
run proves nothing about junk tolerance. **On real machines, read the
`not a known filename` lines** — that list tells you whether the gate is tight
enough for what's actually in those folders.

---

## Testing resume

The queue folder after an interrupted run is itself a good test — partial host
files, some queue files consumed, some not. Scan and upload again: already-recorded
files are skipped, nothing is duplicated, and the counts still land on
`totalFileCount`.

To test the abort path, stop your FTP server and upload anything. Expect 25
consecutive failures then a clean abort, with no manifests sent.

---

## Generator reference

```powershell
.\MakeTestQueues.ps1 -Template <dir> -Root <dir> -OldPid AAA [options]
```

| Option | Meaning |
|---|---|
| `-Count <n>` | panels to create |
| `-StartIndex <n>` | first panel number |
| `-FileSizeKB <n>` | dummy image size |
| `-Random` | random scenario mix |
| `-RandomSeed <n>` | reproducibility |
| `-Pct*` | scenario weights |
| `-SeedRecorded <n>` / `-SeedFailed <n>` | fixed seeding instead of random |
| `-HostOverride <ip>` | rewrite line 0 — **always use this**; don't aim tests at production |
| `-WinScpOverride <path>` | rewrite line 3 for a different machine |
| `-FixTotal` | rewrite line 15 to the real queue-file count |
| `-Clean` | delete everything matching the PID prefix |
| `-Go` | actually write; dry run without it |

---

## Copying to another machine

Copy the whole project folder (the `.bat` needs `MakeTestQueues.ps1` beside it),
plus the template:

```
robocopy "D:\Program\RVS\UploadQueueTemplate" "<target>\Program\RVS\UploadQueueTemplate" /e
```

The exes are prebuilt, so the other machine doesn't need `vbc`.

**Watch the absolute paths inside the template.** Every clone inherits them:
`E:\POCB\...` for images, `D:\Program\RVS\...` for logs, and the WinSCP path on
line 3. The generator creates missing *folders* but not missing *drives*. Use
`-WinScpOverride` and `-HostOverride` for a machine that differs.

---

## Traps found while building this

- A PID shorter than 4 characters risks matching unintended text; the script warns.
- Host lines are `destPath@channel` — strip the `@channel` before taking a filename,
  or matching silently fails.
- `List.Count` shadows LINQ's `Count(predicate)` in VB; use `.Where(...).Count()`.
- A parameter named `path` shadows `System.IO.Path`.
- A field named `grid` shadows WPF's `Grid`, so `Grid.SetRow` resolves to the field.
- Comments are illegal inside a VB object initializer.
- `CheckBox.IsChecked` is `Boolean?` — needs `.GetValueOrDefault()`.

The last five were only caught because the build uses `/optionstrict+`.
