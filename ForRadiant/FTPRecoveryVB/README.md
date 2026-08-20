# FTP Recovery — User Guide

*Tiếng Việt: [README.vi.md](README.vi.md)*

This tool finishes uploads that got stuck.

Panels sometimes stop part-way through uploading to the customer's server. The
images are on the machine, some may already be on the server, but the panel never
finishes — so the customer never receives the final list of files, and as far as
they're concerned nothing arrived at all.

This tool finds those panels, uploads whatever is still owed, and sends the final
lists so the panel is properly completed.

---

## When you need it

Any of these:

- Panels sitting in the upload folder for hours or days with nothing happening
- The customer says files are missing for a panel
- The upload folder keeps growing and never empties
- After a network outage or server problem, when uploads stopped part-way

---

## Before you start

**1. Stop FTPUploader.**

This matters. Both programs write to the same files, and if they run at the same
time they will interfere with each other and can corrupt the record of what has
been sent. Stop it and leave it stopped until you're finished.

**2. Check the customer's server is reachable.**

If it isn't, the tool will stop by itself after a few failures rather than waste
time — but it's quicker to check first.

**3. Nothing else is needed.**

You can run the tool as many times as you like. It never uploads the same file
twice, so if you're unsure, run it and look at the results before doing anything.

---

## Using it

Double-click **FTPRecoveryGUI.exe**. The window opens full-screen.

The upload folder is filled in for you. If it's wrong, click **Browse**.

### Step 1 — Look first

Click **Start Scan**.

This only looks. Nothing is uploaded. After a few seconds you get a table of every
stuck panel and what would happen to each one.

### Step 2 — Read the table

Each row is one panel:

| Column | Means |
|---|---|
| **PID** | the panel's ID |
| **Total** | how many files this panel should have |
| **Host now** | how many are recorded as sent so far |
| **Done** | already sent, nothing more needed |
| **Retry** | failed before, will be tried again |
| **New** | not sent yet |
| **Rebuilt** | files found on the machine that had been forgotten about |
| **Projected** | where the count will end up |
| **Verdict** | what will happen |

The colour tells you the state at a glance:

| Colour | Meaning |
|---|---|
| White | will finish normally |
| Blue | files are all sent; only the final lists left to send |
| **Lilac** | will finish, but **only because forgotten files were found on disk** |
| **Pink** | **cannot finish** — see the verdict for why |
| Green | finished successfully |
| Amber | finished, but incomplete — worth checking |

Lilac rows are worth a second look. They're ready, but the destination for the
recovered files was worked out rather than read from an instruction. The Rebuilt
column and the verdict both show how many.

Panels still to do are at the top. Once you've uploaded some, they move to the
bottom with their result, so the table becomes a record of what you did.

### Step 3 — Try one panel first

Click **Upload** on a single row. Watch the log on the right.

If it looks right, carry on. This is always safer than starting with everything.

### Step 4 — Do the rest

Click **Upload ALL panels**.

You can press **Stop** at any time. It finishes the file it's working on and stops
cleanly. Nothing is lost — run it again later and it carries on from where it got to.

---

## What the verdicts mean

### Good

| Verdict | Meaning |
|---|---|
| `READY - 22 to upload` | 22 files to send, then the panel finishes properly |
| `READY - index/host only` | all files already sent, only the final lists to go |
| `INDEX+HOST SENT` | **done correctly** |
| `INDEX+HOST SENT (4 rebuilt)` | done, and 4 forgotten files were found and sent |

### Needs your attention

| Verdict | Meaning | What to do |
|---|---|---|
| `SENT-SHORT (2 missing)` | finished, but 2 files were never sent | see below |
| `SENT-FORCED-SHORT (3 missing)` | you chose to finish it knowing 3 were missing | expected if you ticked Force |

**A "short" panel means the customer receives a list of files that doesn't include
everything.** Usually because the images are no longer on the machine. Worth
checking why before accepting it.

### Cannot finish

| Verdict | Meaning |
|---|---|
| `INCOMPLETE - 3 source file(s) missing` | 3 images are gone from the machine |
| `INCOMPLETE - 4 queue file(s) missing` | 4 upload instructions are gone |
| `INCOMPLETE - 1 source file(s) missing, 2 queue file(s) missing` | both problems |

There's an important difference:

- **Missing images** — the file itself is gone. Nothing can bring it back. Find out
  why it was deleted.
- **Missing instructions** — the file may still be on the machine, just forgotten
  about. **Tick "Reconstruct from disk" and scan again** — this usually fixes it.

---

## The four options

### Reconstruct from disk — usually leave ON

Finds images sitting on the machine that have been forgotten about, works out where
they belong, and sends them.

This is what rescues most stuck panels. Without it they can never finish.

It is careful about what it sends. A file is only picked up if its name is one the
tool recognises — see **Controlling what may be sent** below. Anything unexpected —
a temporary file, a backup copy, a file with an unusual number in the name — is
skipped and listed in the log. It will never invent something to send.

### Force incomplete — leave OFF unless you mean it

Finishes a panel even when files are missing, sending the customer a list that
doesn't include everything.

Only use this when you've looked at a panel, understand what's missing, and have
decided finishing it is better than leaving it stuck.

### Skip missing source — usually leave OFF

Changes what happens when an image has gone missing from the machine.

| | Result |
|---|---|
| **Off** (normal) | the panel finishes without that file — customer gets a short list |
| **On** | the panel stays stuck, nothing is written |

Turn it on when you want to investigate missing images before letting panels finish.

### Retries — leave at 3

How many times to retry a file before giving up. Each attempt makes a fresh
connection, so a brief network glitch usually recovers on its own.

### Using them together

All three can be ticked at once, but in practice only two settings are worth using.
Here is what each combination actually does, measured on a folder of 490 stuck
panels:

| Reconstruct | Skip missing | Force | Panels finished | of those, short lists | Left stuck |
|:---:|:---:|:---:|---:|---:|---:|
| – | – | – | 435 | 55 | 55 |
| **on** | – | – | **490** | 58 | **0** |
| – | on | – | 380 | **0** | 110 |
| – | – | on | 490 | **110** | 0 |
| **on** | **on** | – | 432 | **0** | 58 |
| on | – | on | 490 | 58 | 0 |
| – | on | on | 490 | **110** | 0 |
| on | on | on | 490 | 58 | 0 |

**The two sensible choices:**

| Setting | Use when |
|---|---|
| **Reconstruct only** | you want every panel finished. 490 finish; 58 send a short list because those images are genuinely gone. |
| **Reconstruct + Skip missing** | you must never send a short list. 432 finish cleanly; 58 are held back for you to investigate. |

**Two combinations to avoid:**

- **Force while Reconstruct is on** did nothing in this test, because Reconstruct
  had already unblocked every panel. It is not useless in general — a file missing
  **both** its instruction *and* its image cannot be recovered, and only Force will
  finish that panel. The log tells you which case you are in: if Force was ticked
  but never needed, the summary says so.
- **Skip + Force** gives the same result as Force alone — Force cancels Skip
  completely, so you get the worst outcome, 110 short lists, plus those panels
  reappear on the next scan because Skip left their instructions in place.

**Force on its own is the worst setting** — 110 short lists, twice as many as
necessary, because the missing files were still on the machine and Reconstruct
would have found them.

The program warns you if you pick either of the pointless combinations.

---

## Controlling what may be sent

**This only affects "Reconstruct from disk".** A file that has its own upload
instruction is always sent — TrueTest already decided it belongs. The rules below
apply only to files found lying on the machine with no instruction, where the tool
would otherwise be guessing.

Two text files sit next to `FTPRecoveryGUI.exe` and travel with it:

| File | What it does |
|---|---|
| `allowed_filenames.txt` | names that may be sent |
| `denied_filenames.txt` | names that must never be sent — beats everything else |

Open them in Notepad. One filename per line. Lines starting with `#` are notes and
are ignored.

```
step01_0650NIT_B056_imgY_Crop.tif
step99_0650NIT_UDIRVibMap_imgY_Crop.tif
```

You can use `*` for "anything here":

```
*_gamma.hex               matches d994_gamma.hex, d995_gamma.hex, ...
NyPucData_@PID@_*.hex     matches _1st.hex, _2nd.hex, _3rd.hex, ...
```

`@PID@` stands for the part of the name that changes from panel to panel.

### Learning

By default the tool also **learns** names from the real upload instructions it
finds, and remembers them in `known_filenames.txt`. That file is written by the
tool — **editing it has no effect**, it is rebuilt on every scan.

Learning is normally what you want: the instructions come from TrueTest, so the
names in them are correct by definition, and remembering them means a name still
works after every panel using it has finished.

If you want **only** the names in `allowed_filenames.txt` to be accepted and
nothing learned, put this on a line by itself in that file:

```
!strict
```

Use it when you have the official image list and want nothing outside it sent.

### Checking before you rely on it

Run a scan with Reconstruct ticked and read the log for lines saying
`not a known filename`. Each one is a file the tool refused to send. If the list is
empty, your rules cover everything on the machine. If a legitimate file appears
there, add its name to `allowed_filenames.txt`.

---

## Common situations

**"Lots of pink rows saying queue file(s) missing"**
Tick **Reconstruct from disk** and click **Start Scan** again. Most should turn
white.

**"Still pink after reconstruct"**
Those images are genuinely gone from the machine. Look at the log to see which
files, and find out why they were deleted. Only use Force once you've decided a
short list is acceptable.

**"It stopped by itself"**
The customer's server stopped responding, so it aborted rather than mark hundreds
of files as failed for no reason. Fix the connection and run it again — it picks up
where it left off.

**"Nothing happens when I click Upload"**
Check the row's verdict. Pink rows can't finish, so nothing is sent.

**"The window looks frozen"**
Check the log on the right and the counter at the bottom. If they're moving, it's
working. A file that's failing takes about a minute before it moves on, so pauses
are normal.

**"A file on the machine was not sent"**
Look in the log for `not a known filename`. If it's there, add the name to
`allowed_filenames.txt` — see **Controlling what may be sent**.

**"I closed it by accident"**
No harm done. Open it again, Scan, and carry on. Nothing is lost.

**"I ran it twice by mistake"**
No harm done. It knows what has already been sent and won't send anything twice.

---

## Where the records are

Everything the program writes is in a **Log\Recovery** folder next to
`FTPRecoveryGUI.exe` — not in the upload folder. The upload folder is only an input.

| File | What it's for |
|---|---|
| `..._recovery_report.csv` | **one line per panel — open this first**, in Excel |
| `..._recovery.log` | the full detail of everything that happened |
| `..._winscp.log` | the raw connection log, for when a transfer misbehaves |

The panel on the right of the window shows the same detail as it happens, but it
trims itself on long runs. **The log file is the complete record.**

Open the CSV in Excel and sort by the last column to see how the run went at a
glance, and which panels ended up short.

### The files that travel with the program

Next to `FTPRecoveryGUI.exe`:

| File | |
|---|---|
| `FTPRecoveryGUI.exe` | the program |
| `WinSCPnet.dll` | required, don't delete |
| `allowed_filenames.txt` | **you edit** — names that may be sent |
| `denied_filenames.txt` | **you edit** — names that must never be sent |
| `known_filenames.txt` | written by the program, editing it does nothing |
| `Log\Recovery\` | all logs and reports |

Copy those to any machine and it's ready to run. The upload folder is only an
input — nothing needs to be placed there, and nothing is written there.

---

## If the network drops

The program is built to be left running unattended, so it does **not** stop when the
server becomes unreachable. It keeps going and protects your data:

- **Nothing is marked failed.** A file that could not be sent because the server was
  down is left exactly as it was, with its upload instruction intact.
- **No lists are sent** for those panels. They report `SERVER-OFFLINE`. Sending a
  list during an outage would tell the customer those files are never coming, when
  in fact they were never attempted.
- **It does not sit there for hours.** Once the server stops answering, the rest of
  the files are skipped in a fraction of a second each, so the run finishes quickly.
- **It recovers by itself.** Every 30 seconds it re-checks the server. If it comes
  back mid-run you'll see `server is reachable again - resuming normally` and it
  carries on at full speed.

In the log this looks like:

```
[conn] server unreachable, queue file kept: step01_0650NIT_B192_imgY_Crop.tif
```

and at the end:

```
Left for a later run : 4182  (server unreachable - queue files kept, nothing marked failed)
```

**Just run it again once the connection is back.** It carries on from where it got
to, with nothing lost and nothing sent twice.

There is an important difference between two things that look the same:

| | Upload failed | Server unreachable |
|---|---|---|
| What happened | the connection worked, but **this file** would not transfer | the server could not be reached at all |
| Likely cause | permission denied, remote disk full, file locked | server stopped, network down, wrong password |
| Instruction file | removed | **kept** |
| Result | panel finishes, list is short by that file | panel untouched, nothing sent |

If a transfer is taking a while, the log says so rather than going silent:

```
... waiting 5s on step01_0650NIT_B048_imgY_Crop.tif  (attempt 1/3)
attempt 1/3 failed after 14s: <reason>
```

---

## Words you'll see

| Word | Meaning |
|---|---|
| **Panel** | one display and all the files belonging to it |
| **PID** | the panel's ID, as the customer's system knows it |
| **Queue file** | a small instruction file: "send this image to this place" |
| **Host / index** | the final lists sent at the end, telling the customer what arrived |
| **Short** | finished, but the list doesn't include everything |
| **Reconstruct** | working out an instruction that went missing, from the file on disk |

---

## If you get stuck

Send whoever supports this tool:

1. The `..._recovery_report.csv` for the run
2. The `..._recovery.log` for the run
3. Which options were ticked

That's enough to see exactly what happened.

---

*For installation, testing, and how the tool works internally, see `DEVELOPER.md`.*
