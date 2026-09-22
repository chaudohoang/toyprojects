# Mid-fail: when the index + host manifests are sent early

"Mid-fail" is the early manifest send. When a file fails, the host is told what the panel
HAS managed to upload, instead of waiting for a panel that may never complete.

This note lists every trigger and every reason a send is declined, because for most of a
day's development the two were indistinguishable from outside: a legitimate skip looked
exactly like the feature not running.

Setting: `MidFailHostUpload` (bool). When false, nothing below happens.


## Triggers - where a send is attempted

### Live pump (UploadEngine)

| # | Where | Situation |
|---|-------|-----------|
| 1 | `UploadOne`, default branch | A file used up every attempt on its own merits -> `FAILED` |
| 2 | `UploadOne`, default branch | The panel timed out while this file still had attempts left -> `TIMEDOUT` |
| 3 | `UploadOne`, `Preempted` | The panel timeout cut the file off mid-attempt -> `TIMEDOUT` |
| 4 | `UploadOne`, `LocalMissing` | The source file no longer exists -> `FAILED`. Fires AFTER `DropLine`, so the manifest no longer lists a file that can never arrive |
| 5 | `RunAsync` catch block | The transfer THREW (e.g. "Session was aborted") and the file ran out of attempts -> `FAILED`. The most common failure on site: 28 of 50 failing panels on 2026-09-10 reached `FAILED` this way |
| 6 | `CheckPanelTimeouts` | A panel exceeded `PanelTimeoutSeconds`. Queued, not sent inline - that sweep runs on the watch loop, where an FTP send starved intake and the rollover. The finalize pump drains the queue |

### NG pump (NgRetryEngine)

| # | Where | Situation |
|---|-------|-----------|
| 7 | `Attempt`, failure branch | An NG retry failed |
| 8 | `Attempt`, `LocalMissing` | The source vanished while the file sat in NG. After `DropLine` |

Triggers 7 and 8 are what make repeat sends work. Once a panel is in NG, every later
failure IS an NG failure, so without them the host would hear about the panel once and
never again - even as NG recovered more of its files.

### Deliberately NOT triggers

- **Midnight rollover.** Every pending file across every open panel is marked `TIMEDOUT`
  and handed to NG. Sending there means a burst of manifest sends at the moment the app is
  trying to settle a rollover, and it is redundant: NG picks the files up immediately and
  the finalize sweep sends the real manifests as they recover.
- **A manifest itself failing** (`FinalizeReadyPanels`). These ARE the index and host
  manifests failing to send. Re-sending a partial version of what just failed is the
  finalize retry's job, and it would fight the marker guard.


## Guards - why a send is declined

Every decline is logged to the day's operation log as
`MIDFAIL <PID>: NOT sent - <reason>`, so a skip can always be told from a fault.

| Reason | Meaning |
|--------|---------|
| `no manifest paths on the job` | The job has no `IndexSrc`/`HostSrc` - not a panel job |
| `no remote manifest paths` | The `.panel` carried no `UploadIndexPath`/`UploadHostPath` |
| `host manifest file does not exist` | Nothing to read; usually the source folder is gone |
| `nothing has landed yet` | Every line is still ` -pending`. There is nothing to tell the host |
| `no new files since the last send (now N, last M)` | The count has not increased. See below |
| `both manifests already sent in full` | `.idxsent` and `.hostsent` both exist. Only the MISSING one is ever sent, so a panel with one marker still gets a partial for the other |
| `a finalize holds the lock` | A real finalize owns `<PID>.idx.sending`; it is about to send the complete manifests |
| `the upload itself failed` | The send was attempted and the transfer failed |

### The re-send rule

Only when the count of clean (non-pending) lines has INCREASED since the last send.

- Same count -> skip. The host would learn nothing.
- Lower count -> skip. `DropLine` can shrink the set when a source is gone for good; that
  shrink is recorded locally but never pushed, so the server is never moved from a longer
  list to a shorter one.
- Higher count -> send.

The previous count is read from the panel's own `<PID>.midfail` record, which logs one
block per send: timestamp, file count, and the file names.

A signature (hash) was considered instead. It only differs from the count in one case -
one file dropped while another lands, leaving the count equal but the set different - which
requires a deletion and an upload between two sends. Never observed on site.


## What a send leaves behind

| Artifact | Purpose |
|----------|---------|
| `<PID>.midfail` | One block per send: time, count, file names. Also the source of the count for the re-send rule |
| `<PID>.idxpartial` | An early, INCOMPLETE index manifest is on the server |
| `<PID>.hostpartial` | An early, INCOMPLETE host manifest is on the server |
| oplog line | `MIDFAIL <PID>: early index+host manifests sent -> <host>, N file(s)` |

Deliberately NOT written: `.idxsent` / `.hostsent`, and no row in the transfer log. Those
mean "the COMPLETE manifest is up there", and the finalize guard keys on them - claiming
them would stop the real manifest ever being sent. A transfer-log row would also make the
restart dedup skip the real manifest later.

The summary CSV reads the `partial` markers and shows the manifest column as a triangle
(U+25B2) instead of `X`: an early, short manifest is on the server, which is a different
state from nothing at all. It does NOT count as success.

Note the markers are files on the machine that uploaded, so the triangle only appears when
the summary is built there. A log set copied elsewhere shows `X`.


## Ordering

Index first, host last, and only the clean lines - the same content as the final manifest,
just shorter. The lock is claimed before sending so an early send and a real finalize can
never write the same remote path from two sessions; a 140 ms race was measured before it.


## Test status (2026-09-11)

| Trigger | Verified firing |
|---------|-----------------|
| 1 attempts exhausted (`FAILED`) | yes |
| 4 source gone (live) | yes |
| 6 panel-timeout sweep | yes |
| 7 NG retry failure | yes |
| 2, 3 timeout variants | by inspection only |
| 5 pump error / session abort | NOT REPRODUCED - see below |
| 8 NG source gone | by inspection only |

Trigger 5 could not be reproduced locally in eight attempts: killing the FTP server,
stopping it gracefully, a TCP proxy severing sessions mid-transfer, both transfer engines,
and the failure injector. A session abort is an exception from the transport, and nothing
available here raises one on demand. The call site is two lines inside the catch block that
produced all 484 `PUMP ERROR` lines on site that day, guarded by the same status field that
wrote their `FAILED` rows - but it has not been watched working.

The `NOT sent - <reason>` logging is what makes that acceptable to ship: the first real
abort burst will say either that it sent, or precisely which guard declined.
