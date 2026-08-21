# What changed — 2026-08-21

Built from the analysis of the LGD production report
(`20260820_190832_recovery_report.csv`, 6,568 panels).

---

## 1. Empty manifests are never sent  ← most important

**What the report showed.** 1,453 panels reported `SENT-SHORT (12 missing)` or
`(16 missing)`, and every one of them read:

```
Total 12   Pending 12   Uploaded 0   MissingSource 12   HostAfter 12
```

Every image was already gone from disk, so all 12 got ` - failed` placeholders, the
count reached 12/12, and the placeholder-stripping step then removed all of them.
**The manifest uploaded to LGD listed zero files.**

1,453 panels were declared complete to the customer with nothing in them.

**Fix.** A manifest with no real entries is never uploaded. This overrides Force —
an empty manifest is never the right answer. Those panels now report:

```
SKIPPED-NOTHING-TO-SEND (all 12 images gone)
```

and the scan shows it before anything is uploaded:

```
NOTHING TO SEND - all 12 images gone from disk
```

**Action needed on the 1,453 already sent.** Their PIDs are in the old report:

```powershell
Import-Csv '<report>.csv' |
  Where-Object { $_.Uploaded -eq 0 -and $_.Result -like 'SENT-SHORT*' } |
  Select-Object PID,Total | Export-Csv 'empty_manifests.csv' -NoTypeInformation
```

LGD may need those panels reset — an empty manifest could otherwise mark them
permanently delivered.

---

## 2. Files can no longer be stranded on the FTP server

**The problem.** Each queue file is deleted as soon as its file uploads. So by the
time the manifest is attempted, every queue file is gone. If the manifest then
failed, the panel became **invisible** to every later scan — the uploaded files sat
on LGD's server forever, unusable by MAS and taking space, with nothing left to
retry from.

The report has 1 such panel (`INDEX/HOST UPLOAD FAILED`).

**Fix.** When a manifest upload fails, the tool writes an index/host queue file back
into the queue folder, the same way `FTPUploaderVB` does. The next scan finds the
panel, sees nothing pending, reports `READY - index/host only`, and sends just the
manifest.

The one panel from the earlier run is still orphaned — it predates this fix.

---

## 3. "Reconstruct from disk" is ON by default

The report had 334 `SKIPPED-INCOMPLETE`, of which **322** read:

```
Total 17   Pending 1   HostBefore 0   MissingSource 0
```

Instructions lost, but **images still on disk** — exactly what Reconstruct recovers.
It was off for that run, so those panels were reported as unfixable.

Force and Skip missing source remain off, which measurement showed is the right
combination.

---

## 4. Send to: — destination server override

New dropdown next to Retries:

```
Auto (from queue)      default, unchanged behaviour
10.119.211.173
10.119.211.174
```

Console: `-host 10.119.211.174`

Only the server changes. Credentials and destination paths still come from the queue
file. Selecting an override is logged loudly, and named per panel:

```
sending to  : 10.119.211.174   (OVERRIDE - queue says 10.119.211.173)
```

> Check with LGD that both servers accept the same credentials — the override keeps
> the queue file's username and password.

---

## 5. Every log line has a timestamp

Previously only the filename carried a time, so there was no way to say when an
outage started or to line the run up against the FTP server's own logs.

```
14:32:07  [ ok ] step01_0650NIT_B048_imgY_Crop.tif -> ftp://10.119.211.173/data1h1/...
```

The report CSV also gains a `Time` column, one per panel.

---

## 6. Full destination path, including the server

Upload lines previously showed the path but not which server it went to — so
"uploading to the wrong host" was invisible in the log. Now every line shows both,
and each panel states its destination once:

```
PID A4XD65000R02AB3
  sending to  : 10.119.211.173
```

Paste an `ftp://...` line into Explorer's address bar to open it directly.

---

## 7. Outage handling — no longer stalls, never loses work

Found while testing with the FTP server stopped mid-run.

- **A dead server used to stall silently for minutes.** WinSCP's timeout does not
  cap a connect to a dead port. A socket pre-check with a 3-second timeout now runs
  first, and a heartbeat reports any wait over 5 seconds.
- **A bug made it ~100× slower than intended**: the flag marking the server as down
  was never set, so every file paid the full timeout. 125 files took 6 minutes;
  they now take 3.7 seconds.
- **Nothing is destroyed by an outage.** Queue files are kept, no ` - failed`
  placeholders are written, and no manifests are sent (`SERVER-OFFLINE`). Just run
  again when the connection is back.
- **It self-heals**: re-probes every 30 seconds and resumes mid-run if the server
  returns.
- **It never stops** — designed for unattended running.

A connection failure and an upload failure are now distinguished, because they mean
opposite things:

| | Upload failed | Server unreachable |
|---|---|---|
| Cause | this file (permissions, disk full, locked) | the connection |
| Queue file | removed | **kept** |
| Marked failed | yes | **no** |

---

## 8. Progress reporting on slow disks

The backlog is on HDD machines, where reading ~100,000 queue files takes minutes
rather than seconds. The scan used to print nothing until it finished.

```
... read 2000 queue file(s)  (14s)
... checked 500 / 6568 panel(s) for forgotten files  (22s)
```

Related: this is also *why* the HDD machines fell behind. `FTPUploaderVB` opens a
new session per file and picks work newest-first, so once a slow machine falls
behind, older files drop out of the window and never get retried. On SSD it keeps
up; on HDD the backlog compounds and never drains.

---

## 9. Ping strip tests FTP, not ping

Was ICMP; now a TCP connect to port 21, labelled `no FTP` when it fails. A network
can block ping while allowing FTP, or answer ping with the port closed — the old
light did not mean "an upload would work".

---

## 10. Verified at scale

| | |
|---|---|
| Queue files | 96,820 |
| Panels | 4,990 |
| Scan | 15.9 s with reconstruction |
| Rebuilt from disk | 1,882 |
| Junk accepted | 0 |

Scaling is linear. `ResetTestSet5000.bat` reproduces this population.

---

## Reading the report

| Result | Delivered? | |
|---|---|---|
| `INDEX+HOST SENT` | **yes** | complete |
| `SENT-SHORT (n missing)` | partly | manifest sent, n files not in it |
| `SKIPPED-NOTHING-TO-SEND` | no | **new** — all images gone, nothing sendable |
| `SKIPPED-INCOMPLETE` | no | try Reconstruct — usually fixable |
| `SERVER-OFFLINE` | no | untouched, just re-run |
| `INDEX/HOST UPLOAD FAILED` | no | files on server, manifest missing — **re-run** |

```powershell
Import-Csv '<report>.csv' | Group-Object Result | Sort-Object Count -Descending
```

---

## To deploy

Copy from `bin\`:

```
FTPRecoveryGUI.exe
WinSCPnet.dll
allowed_filenames.txt
denied_filenames.txt
```

Runs from any folder; the queue path is an input. Logs and rules stay with the exe.
