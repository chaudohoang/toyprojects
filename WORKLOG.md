# Work log

Newest first. One section per working day. Keep entries short: what changed, where, and
anything that would be expensive to rediscover. Code detail belongs in git; this is the
"why / what happened" that git messages don't carry.

---

## 2026-10-24

- **`RomLauncher`: resolve No-Intro style folder names to short system ids.** New collection at
  `F:\Games\TopRoms Collection` names folders `Manufacturer - Full System Name`; the old root
  `F:\Games\Roms` (303 systems) uses EmulationStation short names (`nes`, `psx`).
  - **The search was not merely weak on long names, it ranked the wrong system first.** Measured
    against `Fuzzy.Match`: `nes` scored **566 on "sega - mega drive - geNESis"** (literal
    substring) versus **383** on "nintendo - nintendo entertainment system" (subsequence fallback
    only, plus a length penalty). `snes` likewise lost to Genesis 396 vs 387. `psx` matched
    **nothing** — there is no p→s→x subsequence in "sony - playstation". The dropdown type-ahead
    (prefix then substring) failed the same way.
  - Fix is at **discovery**, not in the matcher: `SystemNames.Canonical()` maps the folder to the
    short id before `cfg.Find()`, so the id *is* the search term and scores an exact 1000. After:
    `nes` → nes 1000, nes-hacks 794, snes 599, Genesis absent.
  - **Merging came free.** `SystemConfig.ExtraFolders` ("same system in other roots") already
    existed, so resolving to `nes` makes the new collection join the configured `nes` system
    instead of appearing beside it. No merge logic was written.
  - All **62 folders map, 0 unmapped**, and every base id already existed in the old root with an
    emulator configured — so the collection inherits emulator setup. Variant suffixes stay
    separate on purpose: `nes-hacks`, `megadrive-sports`, `mame-chd`, `xbox-dlc`.
  - New ini keys: `DisplayName=` per system (drives the `nes — Nintendo Entertainment System`
    dropdown label) and a `[FolderMap]` section for `folder name=system id` overrides, for
    collections the built-in table does not cover.
  - **`CACHE_VER` 2 → 3.** The cache is keyed on `RootsKey()` (the roots string), which does NOT
    change when system *naming* changes — so the stale cache, full of entries filed under the
    long folder names, would have been accepted and the fix would not have shown until a manual
    rescan. Bumping the version is the built-in mechanism for exactly this and forces the rebuild
    everywhere. Remember it on any future change to how entries are named or filed.
  - **Deployed to `F:\Games\Romlauncher`** (the live copy; the repo folder is not the one he
    runs). Chau had already added the collection as `ExtraRoot2` that morning with the OLD exe,
    which auto-created **62 placeholder `[System:<long folder name>]` sections**. Discovery never
    removes stale systems, so those would have lingered as ghost dropdown rows showing (0).
    Verified all 62 carried no emulator/alts/extensions, backed the ini up to
    `RomLauncher.ini.before-shortnames`, and stripped them — 313 blocks kept, roots untouched.
  - **Window 1000x620 -> 1600x900, columns rebalanced** so Path is widest: Name 32%, System 10%,
    Type 6%, Played 8%, Path the remaining ~44%. It was Name 40% / Path 26%, which was backwards
    once paths carry a root prefix plus a long folder name. At 1600 that is Path 701px against a
    601px worst-case path, Name 511 against 310.
  - The `id - Long Name` dropdown labels then overflowed the 200px combo ("snes - Super Nintendo
    Entertainment System (12,345)" measures 303px); combo widened to 340 and the Sort/Favorites
    controls shifted right to match. Verified by screenshotting the real window.
  - **Screenshot trick:** `CopyFromScreen` grabs whatever is on top, so it captured the terminal
    instead. `PrintWindow(hwnd, hdc, 2)` (PW_RENDERFULLCONTENT) renders the target window itself
    and works while occluded - use that for WinForms captures.
  - **Deliberately did NOT add auto-pruning** of folder-less systems: if F: is ever disconnected
    at launch, that would wipe the emulator config of all 300+ systems. A one-time data clean is
    correct here, since the new exe never creates the long names again.
  - Expect ~10 new systems with no emulator configured (`*-hacks`, `*-homebrew`, `mame-chd`,
    `bios-retroarch`, …). The two BIOS folders are not games and are probably worth `Enabled=0`.
- **`RomLauncher`: dark theme**, on by default, toggled by "Dark theme" in Systems & emulators
  (`DarkTheme=1` in `[General]`). The app had only 10 hardcoded colours and otherwise rode system
  defaults, so a `Theme.Apply(Form)` walker that sets colours per control type covers nearly
  everything.
  - **WinForms has no dark mode; the native controls need the OS asked separately.**
    `SetWindowTheme(hwnd, "DarkMode_Explorer")` on the ListView, and crucially the HEADER is a
    SEPARATE HWND reached via `LVM_GETHEADER` (0x1000+31) which needs `"DarkMode_ItemsView"` —
    without it the column header strip stays white over a black list. Title bar is
    `DwmSetWindowAttribute` attr 20 (and 19 for 1809..1903).
  - **Follow-up 5: flash became INTERMITTENT ("most of the time it is clean").** Intermittent =
    a race, and the culprit was the safety timer: it fired unconditionally at 400ms, so on a
    slower start where the queue had not gone idle yet it revealed a half-painted window. Fixed
    by making the reveal path itself safe rather than tuning the race — `show()` now calls
    `f.Refresh()` (invalidates control AND children, paints synchronously) immediately before
    setting Opacity, so BOTH the Idle path and the timer path reveal a finished window. Timer
    could then stay short (600ms) instead of being pushed out to 2.5s, which would have risked
    a visibly late window.
  - **Harness caveat:** `Application.Idle` does NOT fire under a PowerShell `DoEvents` loop, so
    reflection harnesses show `Opacity` stuck at 0 and look like a hang. Only `Application.Run`
    raises it. Verify this path against the real exe (handle appears ~405ms), not a harness.

  - **Follow-up 4: still flashing, "just faster".** That symptom is the tell — the remaining
    flash is a LATER paint than the one being waited on. The form painting says nothing about
    its CHILDREN, and a native ListView erases itself light before its first row draw. Two
    changes: reveal on **`Application.Idle`** (fires when the message queue is EMPTY, so after
    every pending child paint) instead of the form's own `Paint`, with a 400ms timer so a missed
    Idle can never strand the window invisible; and `FastListView` now handles `WM_ERASEBKGND`
    itself, filling with `BackColor`, which removes the white at source rather than hiding it.
  - **Lesson for this class of bug:** "it still flashes but faster" means the chosen wait point
    is too early, not that the approach is wrong. Each fix moved the reveal later in the paint
    sequence — Shown → form Paint → queue empty.

  - **Follow-up 3: the flash survived the Opacity trick, because `Shown` is too early.**
    `Form.Shown` fires when the window is DISPLAYED, before its first `WM_PAINT` completes, so
    restoring opacity there reveals an unpainted — i.e. white — window. That IS the flash. Now:
    `Refresh()` to paint synchronously while still invisible, then reveal from the form's first
    `Paint` AND from a `BeginInvoke`, whichever lands first (both idempotent). Two paths on
    purpose: relying on `BeginInvoke` alone leaves the window invisible forever if that message
    is never pumped, which is a far worse failure than a flash.
  - **Screen-sampling to detect the flash mostly failed and cost real time.** A dark window over
    a dark desktop is indistinguishable by average brightness (both ~30-50), so "no change" told
    me nothing about whether the window had even appeared. What did work: assert on
    `f.Opacity`/`f.Visible` directly after pumping messages, and only use brightness to catch
    values >140 (white), which the dark theme can never produce. Startup also takes ~12s loading
    the cache, so any sampler must run well past that.

  - **Follow-up 2: the selected row kept DARK text.** `DrawDefault` on the sub-items let the
    native highlight paint a light-blue band and keep the item's own (dark-on-light) text on it.
    Rows are now drawn in `OnDrawSubItem` when dark — background `Theme.Selection` (a deep blue)
    or the list background, text in the item's own `ForeColor` so favourites stay amber. ~200
    `DrawText` calls per paint at 40 visible rows, which is nothing; light mode keeps
    `DrawDefault` throughout.
  - **Follow-up 2: the flash was NOT fixed by setting colours in the ctor.** Windows fills the
    window from its class brush before the first `WM_PAINT`, and the dark title-bar attribute
    only lands on the frame AFTER it is set, so there is a gap no amount of early colouring
    closes. `Theme.Init` now holds the form at `Opacity = 0` and restores it on `Shown`, which
    fires after that first paint. Applies to every form, so the dialogs stop flashing too.

  - **Follow-up: white flash on window open.** `Theme.Apply` ran on `Shown`, i.e. after the form
    had already painted in system colours. Split into `Theme.Init(f)` — called at the END of each
    ctor — which sets managed colours immediately (before the window is ever shown) and defers
    only the handle-dependent native calls to `HandleCreated`, which still fires before the
    window becomes visible. `Theme.Apply` remains for re-theming on toggle.
  - **Follow-up: theme toggle button** in the top bar, labelled with the mode it switches TO
    ("☀ Light" while dark). Writes `DarkTheme` to the ini immediately rather than at exit, so
    the choice survives a kill.
  - **Removed the "Dark theme" checkbox from Systems & emulators** that had been added an hour
    earlier: its handler mutated `cfg` directly, so toggling it and then pressing **Cancel** left
    the theme changed anyway. One control instead of two that can disagree.
  - Driving the toggle from a script needs **client** coordinates through `ClientToScreen`, not
    window-relative ones — the first attempt clicked into the title bar and silently did nothing.

  - **Follow-up: `DarkMode_ItemsView` darkens the header BACKGROUND but leaves its text black**,
    which is unreadable on it. No theme string fixes that, so the header is owner-drawn:
    `OwnerDraw = true` with `OnDrawColumnHeader` overridden in `FastListView`, while
    `OnDrawItem`/`OnDrawSubItem` just set `DrawDefault = true` so rows keep the native fast path
    (important with 55k virtual rows). Put in the subclass rather than event handlers so that
    re-applying the theme cannot attach them twice. Light mode sets `DrawDefault` on the header
    too, so it still renders natively.
  - Theming must run on **`Shown`, not in the constructor**: both of those APIs need a created
    handle and silently do nothing before that.
  - **ComboBox: must stay `FlatStyle.Standard`.** `FlatStyle.Flat` makes WinForms paint the
    drop-down arrow itself in system colours, so it stays light no matter what; Standard hands
    painting back to the native control where `DarkMode_CFD` darkens the arrow too.
  - **CheckBox: `FlatStyle.System` + `DarkMode_Explorer`.** Tried `FlatStyle.Flat` with
    `FlatAppearance.CheckedBackColor` first — WinForms ignores it while `BackColor` is
    transparent, so a TICKED box rendered identically to an empty one. Caught only by checking a
    render against the ini (`MinimizeOnLaunch=1` showing as unchecked); the build was clean and
    the colours all "looked" right.
  - Light mode verified too, by flipping `Theme.Dark` through reflection before building the
    form — the FlatStyle changes above are unconditional, so they had to be checked both ways.
  - Favourite rows and the search clear-button hover now read `Theme.Fav` / `Theme.Danger`
    instead of literal RGB.
  - Unrelated, noticed while verifying: the library is now **55,271 ROMs**, up from 51,563,
    because a fourth root `G:\Phone roms` was added. Not a regression.

- **`RomLauncher`: widened the two settings dialogs so the grey hint lines stop clipping.**
  SystemsForm 860 -> 940, AltEmusForm 720 -> 800. The hints are `AutoSize` labels, so they were
  not being clipped by a label box but running past the FORM edge - measuring text width against
  `ClientSize` is how to find these, not inspecting the label.
  - Overflow was 30px (SystemsForm "Standalone emulators: ... else fullscreen.") and 39px
    (AltEmusForm "Or make the alternate RetroArch itself ..."). The Extensions hint measured as
    fitting with 5px to spare yet still clipped on Chau's screen - DPI scaling eats that, so
    treat anything under ~10px of slack as broken.
  - One of the two was self-inflicted: the Extensions hint got longer when it was reworded to
    "empty uses the built-in list for this system" the day before.
  - Widened the right-hand column with the form rather than leaving a gap: fields +80px
    (`txtEmu`, `txtArgs`, `cboCore`, `cboPreset`, `txtExt` and the AltEmus equivalents) and the
    right-edge buttons shifted +80 (`rx + 483` -> `rx + 563`, etc). OK/Cancel/Close already sit
    at `ClientSize.Width - N` and followed on their own.
  - No `CACHE_VER` bump: layout only, nothing about what a scan collects changed.
  - Verified by rendering both dialogs through reflection and `PrintWindow`. AltEmusForm needs a
    `SystemConfig` built by hand with Name/Emulator/Args set plus a non-null RetroArch path; it
    throws on a bare one from the config list.

- **`RomLauncher`: extension whitelists moved from the ini into the code as built-in defaults.**
  `Scanner.DefaultExt` (83 systems) is consulted when `Extensions=` is empty; an explicit value
  in the ini still wins, and a system absent from the table keeps the old `IgnoreExt` blacklist
  behaviour — guessing a whitelist for an unrecognised system hides files silently. All 83 ini
  values were cleared afterwards so there is one source of truth, and the defaults now travel to
  the other PC and survive a regenerated ini.
  - **`ps2`'s hand-set `chd bin iso` was cleared too**, at Chau's request, so it takes the
    curated `chd,cso,gz,iso,m3u` + archives. It loses 2 `.bin` files — for PS2 a loose `.bin` is
    a disc track, not something you launch.
  - `CACHE_VER` 3 → 4, since the admitted extensions changed what a scan collects.
  - Verified end to end: **51,563 ROMs / 84 systems**, exactly 51,780 − 215 (nested folder
    deleted, below) − 2 (ps2 `.bin`). genesis keeps its +395 `.bin`, megadrive its +20 `.md`.

- **Deleted `F:\Games\Roms\TopRoms Collection`** — a real directory (NOT a link), 246 files /
  6.33 GB, a partial copy of the collection nested inside the old root. Checked every file
  against `F:\Games\TopRoms Collection` first: 246/246 present at identical sizes, 0 unique.
  Its orphaned `[System:TopRoms Collection]` ini section was removed too.
  - **`sufami` and `sg1000` are `SYMLINKD`** to `sfc` and `sg-1000` (confirmed via `dir /al`) —
    deliberate, leave them. Only the TopRoms one was a real duplicate. Check link status before
    touching anything that looks duplicated here.

- **`RomLauncher`: per-system `Extensions=` whitelists derived from the actual library.** 83 of
  326 systems now have one; the 243 with no ROMs stay on auto (a whitelist guessed with no
  evidence would silently hide a format). Derived from `RomLauncher.cache.tsv` rather than by
  walking the disk - the cache already holds every scanned path, so this is instant.
  - Each list = observed extensions (minus junk) ∪ a curated per-platform baseline ∪
    `zip,7z,rar`. The baseline matters: dedup means a `.cue` next to a `.chd` never reaches the
    cache, so a purely observed list would omit `.cue` and hide games that only have one.
  - **Net +355 ROMs, not fewer.** A whitelist BYPASSES `Scanner.IgnoreExt`, which was hiding real
    ROMs: **genesis +395 `.bin`** and **megadrive +19 `.md`**. `.md` is in IgnoreExt as *markdown*
    and had been silently eating every Mega Drive ROM using that extension — a latent bug the
    whitelist now works around. Left IgnoreExt alone: dropping `.md` from it globally would let
    real markdown through everywhere.
  - Junk correctly removed: singe −22 `.ttf`, ports −9 `.libretro`, saturn −5 `.s`/`.ic8`,
    psx −4 (`chdman.exe`, `.bat`), and scattered −1s of `.old`. `jaguarcd` fell to 0 and vanished
    from the list — `F:\Games\Roms\jaguarcd` is empty and its only entries were `.bigpimg`.
  - **`ps2` was left alone** — it already had a hand-set `chd bin iso`. The script never
    overwrites a non-empty value, which is also why `.bin` appeared under ps2 in the before-data.
  - **Skipped as not-game-systems:** `bios`, `bios-standalone` (drops yield nonsense lists like
    `1024,prefs_backup,sha`) and `TopRoms Collection`. First attempt wrote them anyway — the edit
    that added the skip list produced two loops and the second ignored it; caught by inspecting
    the written ini, cleared afterwards. **Verify the file, not the script's success message.**
  - Backups: `RomLauncher.ini.before-extensions`, and `.before-shortnames` from the earlier pass.
  - **Data hygiene found, NOT fixed (Chau's call):** `F:\Games\Roms\TopRoms Collection` is a
    nested copy inside the old root; `sfc`/`sufami` and `sg-1000`/`sg1000` are duplicate pairs
    with byte-identical extension counts, i.e. the same content scanned twice.
  - The live ini at `F:\Games\Romlauncher` is NOT in the repo; the tracked
    `Romlauncher/RomLauncher.ini` is a separate, staler config and did not get these lists.

## 2026-10-22

- **FtpUploadNew: a panel tested twice in a day lost its retest entirely.** `AddFiles`
  deduped on FILE NAME alone (`UploadEngine.cs`) against a `_jobs` table keyed by PID, and
  a finished panel's Job stays in memory until the day rollover. A retest produces the same
  13 names from a different folder to a different remote path, so all 13 were silently
  dropped — not queued, not logged. Found via `20261012`: 11 of 95 panels, every one of
  them a retest, 100% of retests affected. The visible symptom was "all files O, Index O,
  **Host X**" in the summary, because the host manifest `{PID}_{DateTime}.txt` is the only
  artifact whose NAME carries the run, so it was the only cell that could tell the two runs
  apart — `SummaryLog` kept the LAST name per PID (last-write-wins) and asked whether the
  never-sent second one had landed.
- **Fix: identity is now panel + RUN.** Run id = the `.panel` handoff's `DateTime`
  (`PathDerivation.RunIdOf`) — already unique per run, already the `{DateTime}` segment of
  every remote path and the host manifest stamp. **Trap:** the SOURCE FOLDER carries a
  different stamp (capture-folder creation, e.g. `_20261012083338` vs run `20261012083021`);
  keying off it would not match what the server sees.
  `_jobs` → `pid|runId`; `JobFile.Key` → `Pid|RunId|FileName`; `NgItem.Key`,
  NG card `GroupKey` likewise. `AddFiles` now also WARNS when a name clashes with a
  different destination instead of dropping it in silence.
- **All logs carry the run**, every field APPENDED so old rows still parse: `jobs.txt` 11th
  field, rawlog + ng-retry log 12th. `summary.csv` gained `Run` (`1 of 2`) + `Run start`
  and is one row per RUN, counting `Total runs` — filter `Run <> "1 of 1"` to find every
  retested panel. `operation.csv` gained a `run` column. HTML day + NG reports emit one
  card per run, labelled only when a panel ran more than once.
  **Index trap:** on a JOBS line the run is `p[10]`; on a RAWLOG line it is `p[11]`, because
  `LogRow.Fields` strips a leading timestamp only when one is present. Mixing them silently
  splits or merges runs.
  Panel-event lines now name the subject `{PID}#{RunId}` — the run is NOT recoverable from
  the text otherwise (the index manifest is `{PID}.idx` on every run).
- **Nearly shipped a regression:** `NgRetryLog.ReadState` keyed by `pid|filename`, so the
  new 3-part keys would never have matched `prior` — already-recovered NG items would have
  been retried forever. The ng-retry log needed the run too.
- **Tested against a live FTP server on 127.0.0.1**, not just a green build: 40 panels/12
  retests, 60 panels/20 retests with the retests landing 6s in (mid-flight, 55% simulated
  failure), and a kill-mid-upload + restart to exercise the jobs.txt resume path. 52/52,
  80/80, 35 runs with 0 files re-uploaded after the restart. Audited afterwards: 0 rows
  missing a run in any log, html cards == runs exactly.
  Stress trees live under `D:\FtpuploadTest\<run>\` (NOT at `D:\` root).
  `FtpUpload.exe --reports <yyyyMMdd>` rebuilds a day's reports without starting the engine —
  the fastest way to see report changes.
- **Separate, unresolved (`20261005`, "no img in MAS"):** upload side is provably complete
  (5024 + 1658 = 6682 = 514 x 13, 100%). With `StampManifestNameAtUpload` ON — a setting
  **LGD asked for** — an NG-recovered panel's host manifest arrives as TWO differently-named
  files each listing only its own portion (267 of 514 panels that day; 29 of them have ZERO
  images in the first). Anything consuming only one manifest sees a partial panel. The INDEX
  is unaffected: fixed name, rewritten complete. Also: that uploader covers CH 1+5 only
  (514 of 2063 panels on the EQP), so a CH 3 panel missing from its logs is normal.
- Shipped **1.0.0.10**; deployed to `D:\Log\FtpUpload 1.0.0.10`.
- **Still open:** the app has not been clicked through in the UI (per-run NG cards unseen);
  `ForRadiant/CLAUDE.md` still says FtpUploadNew is "not under active change as of 2026-10-01".

- **1.0.0.11: operation.csv column naming.** `run` held a STAMP there while `Run` in
  summary.csv held an ORDINAL - same word, two meanings, in two reports read side by side.
  Renamed to `run start` so both files mean the same thing by the same header.
  A `run` = `1 of 2` ordinal was added alongside it and then **reverted**: "N of M" is a
  fact about the panel's whole DAY, not the event, so a row written at 08:00 reads `1 of 1`
  until a 15:00 retest makes it `1 of 2` - the same physical event renders differently in a
  report rebuilt later. operation.csv is a timeline that gets grepped and diffed, so a cell
  that changes retroactively does not belong in it. The ordinal stays in summary.csv, the
  per-panel rollup, where a whole-day fact belongs.
- **Deploy recipe learned the hard way:** copy the PREVIOUS `D:\Log\FtpUpload <ver>` folder
  and swap only `FtpUpload.exe` — 18 entries. Copying the repo's `publish\` folder shipped
  49: the test harness, `.pdb`, five config variants, stale September data in
  `jobs\`/`logs\`/`state\`, no `WinSCP.ini`, and — worst — overwrote the production
  `config.json` (demo host, `SimulateFailurePercent: 40`, autostart off,
  `StampManifestNameAtUpload: false`) and `allowed_filenames.txt` with the repo's loose dev
  recipe, whose `step*.tif` also matches `*_Raw.tif`/`*_omit.tif` (~3 GB/panel).
  Those three files belong to the SITE, not the build.
- `publish\FtpUpload.exe` is ~147 MB and deliberately untracked (over GitHub's 100 MB
  limit). Commit with `git add -u`; `git add <project-folder>` sweeps it in and the push is
  rejected by the pre-receive hook.

---

## 2026-10-19

- **Moved `CLAUDE.md` and `WORKLOG.md` from `E:\Github` into `toyprojects`** so both travel
  with a `git pull` instead of living only on the main PC. `<root>` now means "wherever
  `toyprojects` is cloned", so a PC that clones elsewhere needs no edit.
  - **Trade-off accepted:** this file only auto-loads for a session started *inside*
    `toyprojects`. Working at `<root>` on the upstream clones (YgoMaster, the Yu-Gi-Oh
    recomps) no longer picks it up. Worth it, since that is where almost no work happens.
  - Two things stay machine-specific and need checking on a new PC: the deploy folder path
    (`E:\PortableApps\MultiRemoteTool`) and the `C:\Program Files\...` paths inside the
    tool INIs, which depend on what is installed there.

## 2026-10-17

- **`MultiRemoteTool`: new `RustDesk-IP` platform — book holds PC names, address is found
  at connect time** (`8b09629`). New `{ip}` placeholder in the command template; a
  platform using it resolves the contact's Id to an address before launching, so a machine
  moving between the LAN and the tailnet needs no book edit.
  - `HostResolver` gathers candidates from three sources **run concurrently**: DNS,
    `tailscale ip -4 <name>`, and the `direct <lan-ip>:41641` field of `tailscale status`.
    Serially an unknown name cost 5.5s before the error dialog; concurrently 2.8s, and a
    live host resolves in ~40-50ms.
  - **The `tailscale status` scrape is the important one.** Once MagicDNS is answering,
    `Dns.GetHostAddresses` returns *only* the 100.x address, so that status line is the
    only way to learn a peer's LAN IP. It is present only while that peer has a live
    direct connection — if Tailscale is relaying through DERP, there is no LAN candidate
    at all and the tailnet address is used. Expect the same host to resolve LAN one minute
    and Tailscale the next; that is correct behaviour, not a bug. (`i5-3470` did exactly
    this during testing.)
  - **Selection is by rank, not by who answers first.** Candidates sort LAN
    (10/8, 172.16/12, 192.168/16) before Tailscale (100.64/10) before anything else, then
    all are TCP-probed on `ProbePort` in parallel; the best-ranked address that *answered*
    wins. Parallelism only caps the wait at one timeout — it does not decide the winner.
    To invert the preference, swap the two return values in `HostResolver.Rank()`.
  - **Port 21118 is confirmed, not guessed.** RustDesk `src/client.rs:441` routes a
    direct-IP peer through `check_port(peer, RELAY_PORT + 1)` and `RELAY_PORT` is 21117
    (`hbb_common/src/config.rs:121`). `is_ipv4_str("1.2.3.4:90")` is true in their test
    suite, so the explicit `host:port` form is valid. Probing the tailnet confirmed 21118
    open on the live peers, i.e. Direct IP Access is already enabled on Chau's machines.
  - `ProbePort` is a new optional INI key, editable in the platform dialog and carried
    through platform edit and export. Platforms without `{ip}` parse to `ProbePort=0` and
    skip resolution entirely — all five pre-existing platforms verified unaffected.
  - **Not tested end-to-end:** no live RustDesk session was opened. Resolution, INI
    parsing, the final argument string and GUI startup are verified; RustDesk's own
    handling of those args is not. Chau said he would test it himself.

- **`MultiRemoteTool`: sidebar no longer truncates long platform names** (`6b39548`).
  "RustDesk-IP" needs 81px at Segoe UI 9.5 Bold; a *selected* row offered 71, so it drew
  as "RustDes...". Two causes: the contact-count badge reserved a flat 40px regardless of
  content (a 1-2 digit badge is 19-25px), and unselected rows reserved nothing at all, so
  a long enough name could run under the badge. Now both reserve `BadgeWidth()`, the same
  helper the badge draws with. Sidebar also went 240 -> 264px, leaving 104px for the name
  with the four action buttons showing.

- **`MultiRemoteTool`: README** (`6b5d536`). Platform model, every INI key and placeholder, the
  executable search order, the UI, and a long `{ip}` section. Copied to the deploy folder too, so
  it now exists in two places and can drift — update both.

- **`MultiRemoteTool`: `{ip}` is now visible in the UI** (`8f99afe`). It was the only placeholder
  with a side effect yet looked like the equal of `{pw}`, so a resolving platform was
  indistinguishable from a plain one without opening the edit dialog and already knowing what
  `{ip}` meant. Header subtitle now reads `rustdesk.exe ✓ · name → IP` in amber with the full
  behaviour in its tooltip; status bar says the Remote ID is a machine name; the dialog gives
  `{ip}` its own amber line.
  - **The header subtitle has very little room** — it shares its line with the search box, and at
    the 820px minimum window width `titleWidth` collapses to **60px** (186px at 1020). Even
    `rustdesk.exe ✓` is 86px, so it already clipped at minimum size before this change. Anything
    added there must be short; detail belongs in a tooltip.
  - Found while testing: the **Probe Port hint had been clipping its last word** since it was
    added (288px of text in a 268px label). Reworded.
  - Verified by rendering `PlatformDialog` directly through reflection in an `-STA` pwsh process
    and screenshotting it — much faster than driving the real UI, and it catches wrap/overlap
    that measurement alone misses.

- **Trap, cost ~10 min:** launching `MultiRemoteTool.exe` to smoke-test **rewrites
  `settings.txt`** — it saves the selected platform as the first entry of
  `LoadedPlatforms`. This silently dirtied the repo copy during testing. Check
  `git status` after any GUI test run.

## 2026-10-16

- **`MultiRemoteTool`: `VNC` platform split into `TightVNC` + `UltraVNC`** (`7d8ae3a`),
  after Chau installed UltraVNC 1.8.3. Both set `BookFileName=contacts_VNC.csv` so they
  share one address book — that INI key is the mechanism for pointing two platforms at the
  same CSV.
  - UltraVNC arg translation from the TightVNC line: `-autoscaling` for `-scale=auto`, and
    **no fullscreen flag at all** — UltraVNC's `-fullscreen` is a bare flag with no `=no`
    form, unlike TightVNC's. Added `-quickoption 1` and `-disablesponsor`.
  - **No `-password`:** `contacts_VNC.csv` carries no passwords, and an empty `{pw}` would
    make `-password` swallow the next argument. Needs a third CSV column first.
  - **UltraVNC viewer runs standalone** — its PE import table is Windows system DLLs only,
    and it launched from a folder containing nothing but the exe. The companion files in
    its install dir belong to the *server* or are optional (`SecureVNCPlugin64.dsm` loads
    only with `-dsmplugin`). So `vncviewer.exe` is bundled in the repo and the deploy
    folder, referenced by bare name.
  - **Bare name is deliberate:** `ResolveExe` scans only one directory level under Program
    Files, and UltraVNC installs two deep (`Program Files\uvnc\UltraVNC\`), so a bare name
    cannot accidentally resolve to the installed copy — it falls through to the app folder.
  - UltraVNC writes `options.vnc` next to the exe rather than only to `HKCU`, so the
    bundled copy is genuinely portable. Its command-line options **merge with** saved
    defaults rather than replacing them — a setting saved in the GUI sticks regardless of
    args.
  - `LoadedPlatforms` in `settings.txt` is a strict whitelist: a new `.ini` on disk stays
    invisible until it is listed there, and the first entry is the auto-selected platform.

## 2026-10-09

- **`toyprojects`: deleted the stale `ftprecovery-no-duplicate-manifests` branch**, leaving
  `main` as the only branch locally and on GitHub. It pointed at `89f580a`, the same commit
  as `main`, so nothing was lost.
