# Work log

Newest first. One section per working day. Keep entries short: what changed, where, and
anything that would be expensive to rediscover. Code detail belongs in git; this is the
"why / what happened" that git messages don't carry.

---

## 2026-11-28

- **New `phim-tool` (Python/Tkinter): episode links + downloader for phimvn2, tvhay/tvphim, motchill.**
  Built earlier in `E:\Github\phim-tool`, now at `DevWorkspace\python\phim-tool` (Python is its main stack).
  Deploy: `D:\PortableApps\phim-tool`.
  - `ffmpeg\ffmpeg.exe` (110 MB) is git-ignored: over GitHub's 100 MB per-file limit. It lives only in the
    deploy folder; `downloader.find_ffmpeg` looks in `ffmpeg\` next to the tool, then PATH.
  - Queue tab (`hang_doi.py`) persists to `hang_doi.json` next to the tool: live data, never overwrite
    the deploy copy from the repo. One movie subfolder per link; the trailing `-1234` id is stripped only
    for phimvn2 (`SLUG_HAS_ID`), tvphim numbers are year/part (`ac-nhan-2025`, `...-phan-3`).
  - Broken sources: segments with 401/403/404/410 raise `SourceBroken` instead of retrying forever
    (Rừng thép ep 3 Thuyết minh had segments 246–676 at 404 on kkphim, stuck at 36% for 1h+).
    `.parts\nguon.txt` records which playlist the parts came from, so versions never mix.
  - tvhay/tvphim/motchill all serve kkphim video, so `sites\kkphim.py` asks phimapi.com for every version
    of the episode: priority Lồng tiếng → Thuyết minh → Vietsub, the rest are fallbacks in that order.
  - Load check: a 570 MB episode in 51 s, Python ~27 % of one core, ffmpeg remux 3.7 s. Fine on old PCs;
    the heavy part is the grid of browser windows (`grid_open.py`), not the downloader.

## 2026-11-26

- **Kit Helper 1.10.0: a kit's OMIT\ / PUC\ / InputIMG\ opened alone is a package; several folders
  dropped at once are copied as one package.** Reported on the X4023 W511BT kit
  (`D:\Log\Kit\X4023-P2-KS151C-650-155 W511BT- …\20261123_X4023  MATLAB + LGD Crop_TT173`): dragging
  `OMIT` or `PUC` showed nothing, because Kit.Suggest only reads a folder's own files and the model
  folder sits one level down.
  - `Package.Is`: a folder NAMED OMIT / PUC / InputIMG now counts as a package root
    (`IsKitContainer`), except under Program Files (TrueTest's own PUC\) and an InputIMG whose parent
    is `DATABASE` (C:\DATABASE\InputIMG, 223 installed models). Scan was already right for it.
  - `Package.Is/Scan/Preview/Run(…, only)`: a pick of subfolder names. Only those are scanned, the
    parent's loose files are not listed, and a picked folder that is itself a package contributes
    its units under its own name. `FloatingIconWindow.OpenDropped`: >1 folder with one parent →
    `MainWindow.OpenFolder(parent, names)`; different parents → first only + toast. While a pick is
    active the single-target buttons are hidden (they'd copy the whole parent); Refresh keeps it.
  - Before/after sweep of `Package.Is` over `D:\Log\Kit` (310 folders, PS 5.1 loading the HEAD exe vs
    the new one): 0 True→False; the ~70 new Trues are all kits' own OMIT/PUC/InputIMG folders;
    C:\DATABASE\InputIMG and the four installs' PUC\ stay False. Pick {OMIT, PUC, R2R} → 3 units;
    dry-run preview 57 new / 7 skipped helpers, nothing written. Real OLE drags onto the K icon from
    a throwaway DoDragDrop form (3 folders, then OMIT alone), panel + confirmation screenshotted
    ("Confirmation of Copy package … (3 folders)"), cancelled. Self-installed to D:\Program\RVS.
  - Not done: Explorer's "Open in Kit Helper" with several folders selected still opens them one by
    one (Explorer launches the verb per item). Run test doesn't list inputs inside model subfolders.
  - Manual: intro 1.10.0 in both languages, plus the stale 1.9.x bits (Close TrueTest checkbox gone,
    tray icon / icon on the taskbar) and the two new features.
- **Kit Helper 1.10.1: OMIT\ / PUC\ opened alone are a kit for ONE button, not a package** (Chau:
  "detected as a package instead of just ScratchDust / PUC"). `Package.SingleTargetModels`: a kit
  container whose units all go to the same install target (not the database) → that target, and the
  model folders as its sources. `Is` returns false for such a folder; `MainWindow` highlights the
  button ("Suggested: PUC (model folder X\ inside)"), `DoCopy` runs `Copier.Run` once per model folder
  (files land directly in PUC\, not PUC\<model>\), and Run test lists the model folder's inputs.
  InputIMG\ alone stays a package (database + possibly POCB4.1Net: two destinations).
  - Checked on every OMIT/PUC/InputIMG under D:\Log\Kit: all 33 OMIT/PUC → single target, InputIMG →
    package, install PUC\ and C:\DATABASE\InputIMG untouched. Real drags of OMIT and PUC onto the icon
    (screenshots: right button highlighted, Omit/PUC input picked), copy of both into a scratch
    install: 18 and 7 files at the folder root, helpers skipped. Not clicked into the real TrueTest.
- **Kit Helper 1.10.2: a folder inside a kit's POCB4.2\ goes to POCB4.2\<its name>.** Before, opening
  `POCB4.2\X4023` alone suggested POCB4.2 and the button dropped its Crop_params.json / PANEL_SHAPE*.tif
  into `<install>\POCB4.2\` (overwriting the root Crop_params.json). Now `Kit.Suggest` says "folder
  inside POCB4.2, goes to POCB4.2\X4023" and `CopyTarget.For(source)` - applied at the top of
  `Copier.Run`, so every path (button, multi-folder pick, package unit) gets it - redirects a
  one-folder, with-subfolders target (POCB4.2 / ScratchDustDetect_NY / LGDVHCustomApp) to
  `<sub>\<folder name>` when the source's parent is named like that target.
  - Before/after preview of every package under D:\Log\Kit (HEAD exe vs new): 3184 destination
    paths, 0 differences. Scratch-install copies: X4023 → POCB4.2\X4023 (5), mainTemplate →
    POCB4.2\mainTemplate (4), mainTest → POCB4.2\mainTest (4), whole POCB4.2 unchanged (24); pick
    X4023 + X4024 → their own subfolders. Real drag of POCB4.2\X4023 onto the icon: button + hint OK.
- **Kit Helper 1.10.3: every copy now has the confirmation (Chau: "use confirmation and result in all
  cases").** Until now only Copy package and Database model had one; the Copy all → X buttons copied on
  click (a 1.5.0 choice) and Kits from old TrueTest only had its folder checkboxes.
  - `DoCopy`: dry run of the same `Copier.Run` per source (model folders included) → CopyResultWindow
    in preview mode, with untick (`Copier.Excluding`) and the vignetting panel when INIs carrying the
    key are copied; the header shows the real destination (`[POCB4.2\X4023]`). The TrueTest-running
    question comes after the confirmation, as for packages.
  - `KitMigration.Run(…, dryRun)`: no folder creation, `CopyOne(dryRun)`; `ShowMigrate` confirms with
    it and honours unticks. No vignetting there (migration keeps the old install's values on purpose).
  - Tested: migration dry run TrueTest 1.8 (POCB4.2 + LGDVHCustomApp) into an empty scratch folder →
    142 items, nothing created; the real run into it → 141 copied, item list identical to the dry run.
    Real drag of POCB4.2\X4023 + click on its button → "Confirmation of Copy all → POCB4.2", 5 Replace
    under TrueTest\POCB4.2\X4023, helpers Not copied; cancelled, no backup folder made. Migrate dialog
    not clicked through in the UI.
- **Kit Helper 1.10.4: results no longer block; they stack as tabs** (Chau: confirmation must block,
  the result must not). New `ResultsWindow` (not modal, `ShowActivated=False`, raised with
  SetWindowPos NOACTIVATE so the panel keeps focus and doesn't auto-hide): one window, one tab per
  result (mood dot, title, time, ✕), newest selected; closing the last tab closes it. The tab body is
  the existing CopyResultWindow's content, moved out with `TakeContent` (window never shown, still
  handles Open backup / Copy as text; its ✕ / Close close the tab). `ShowCopyResult` no longer goes
  through `Modal`. AutoHide ignores clicks into the results window. Placed left of the panel;
  `KeepOnScreen` on SizeChanged (first version, centred, grew off the bottom of the screen).
  - Tested in a PS 5.1 STA harness with the real windows and Theme: 3 results (one with a Failed item)
    → 3 tabs, red one selected, "3 results, 1 with errors", window never active; close one → 2 left;
    close all → window gone. Installed 1.10.4 starts cleanly. Not yet seen after a real copy in the
    real panel (no copy into Program Files was made for the test).
- **Kit Helper 1.10.5: browser-style result tabs** (Chau: fixed small tabs, the extra ones hidden, not
  all shown). WrapPanel → one row in a ScrollViewer (scrollbar hidden), tabs 150 px with a short title
  ("POCB4.2", "Package …", "Kits from …"; full title + time in the tooltip), ‹ › shown only on
  overflow and greyed at either end, mouse wheel scrolls the row, the selected tab is brought into
  view after layout. Harness with 8 results: extent 1232 / viewport 687 px, newest in view, both
  arrows on; scrolled to 0 → ‹ greyed. Installed 1.10.5 starts cleanly.
- **Kit Helper 1.10.6-1.10.7: results window paired with the panel** (Chau: keep it in the panel's frame;
  clicking the taskbar showed only the results, clicking K showed only the panel). `ResultsWindow`:
  `Owner` = panel, centred on it (`ShowWithPanel`, after layout), no taskbar button. `ShowNear` shows
  it with the panel, `HidePanel` hides it, its `Deactivated` calls the panel's `AutoHide`. `AutoHide`
  is now deferred (Input priority) and keeps the pair if either is the active window.
  - 1.10.7 fix: 1.10.6 also skipped hiding when the cursor was over the panel / results, so a window
    taking the focus while the mouse rested there (a program starting) left both up. Only the K-icon
    cursor check remains (its click toggles the panel itself).
  - Tested in the real app with real clicks: Database model copy of D992-CB-MP00_TLSM twice into a
    scratch database typed into the Database box (UIA ValuePattern) → one centred results window, 2
    tabs, panel usable in between; panel ↔ results clicks keep both; click on another window hides
    both (from either); K brings both back. Database box restored to C:\DATABASE and saved; the two
    scratch backup folders removed. Trap: `notepad.exe` on Win11 reuses the open Notepad window (a tab
    was added to Chau's) - use an own throwaway form as the click-away target.
- **Kit Helper 1.10.8: results docked to the panel's left edge** (Chau: "cannot perform another drag
  if the result is kept"). The drop did work (kit folder changed) but the owned, centred, ~925 px
  results window hid the whole panel. Chau picked "beside the panel" over a pane inside it or
  hide-on-next-action. `ResultsWindow.DockTo`: Left = panel.Left - Width + 16 (overlaps the two 10 px
  transparent margins, 4 px gap), same Top / Height (no longer SizeToContent; the tab's list scroller
  loses its 560 px cap and fills), `KeepOnScreen` for narrow screens. Panel `LocationChanged` re-docks;
  either header drags the panel (results follow). Tested in the real app: scratch-database copies →
  results at 216-996 beside the panel at 980-1920; drop of R2R with results open → panel switched to
  it, LGDVHCustomApp suggested; click-away / K pairing unchanged.
  - Test traps: (1) a click at the results' OLD spot hit "Copy all → POCB4.1Net" and opened a real
    confirmation - cancelled, no backup = nothing copied. Recompute click targets after a layout
    change. (2) The `-Command` throwaway form never appeared; the "away" clicks landed on whatever
    was behind (a PowerShell console, then the Claude window). Verify the target window exists before
    clicking it.
- **Kit Helper 1.10.9: docking reverted; opening a folder pops up its confirmation.** Chau didn't like
  the docked results: back to 1.10.7 (centred on the panel, SizeToContent, owned, paired show/hide).
  Instead `OpenFolder` (drop, multi-drop, Explorer's Open in Kit Helper) queues `OfferSuggestedCopy`:
  Copy package when the package button shows (incl. a multi-folder pick), else the suggested Copy all
  button (incl. OMIT\ / PUC\ model sources, Database model). Skipped when nothing is recognised, while
  busy or a dialog is up, and for a dropped .txt (`OpenFile` passes `offerCopy: false`). Browse /
  typing a path don't trigger it.
  - Tested in the real app with real drags, database box on a scratch folder first: model folder →
    "Confirmation of Copy to Database model …" (→ scratch path, OK'd: result tab centred on the panel);
    with results open, OMIT → ScratchDustDetect_NY confirmation on top; OMIT+PUC+R2R → package (3
    folders); kit root → package (4 units); the outer kit folder → nothing. All TrueTest ones
    cancelled, no backups made; database box restored. Harness note: a confirmation owned by the panel
    is a UIA child of the panel window, not a top-level window.
- **Kit Helper 1.10.10: single Copy all buttons hidden for a full package too** (Chau asked why a
  multi-folder pick hid them but a package folder didn't - no reason, 1.2.0 just never hid them). On a
  package root they copy its loose files (.seqxc, .bmp, Kit Helper.exe) into the target, never its
  parts. `CopyButtons` now collapses when `package || picked != null`; the package hint adds "To copy
  one part only: untick the rest in the confirmation, or open that folder alone." Real drags: kit root
  → only Copy package (+ its confirmation, cancelled); OMIT alone → all single buttons back,
  ScratchDustDetect_NY highlighted. No backups made.
- **Kit Helper 1.10.11-1.10.12: the confirmation doesn't block either; the latest one replaces it.**
  Chau: OK → "Copy"; any newer copy action (another Copy all, opening a folder / several / a package)
  replaces a pending confirmation. `MainWindow.Confirm(w)` shows it non-modal (owned by the panel)
  and returns a Task<bool> completed on Closed with `w.Confirmed` (set by Copy); `CancelConfirm()`
  closes the pending one first, and `SetSource` cancels it (it belonged to the old folder). Each copy
  flow captures its source / install / database before awaiting, so a replaced confirmation's flow
  just returns. HidePanel hides it (still pending), ShowNear brings it back on top of the results,
  AutoHide counts it as part of the group. 1.10.12 added its `Deactivated` → AutoHide (1.10.11
  missed it: clicking away from the confirmation hid nothing).
  - Tested in the real app with real drags: OMIT → its confirmation; "Copy all → PUC" clicked on the
    panel meanwhile → replaced; PUC+R2R dropped → replaced by the package one; button text "Copy";
    click away (after giving the confirmation focus) hides panel + results + confirmation, K brings
    all three back still pending; Cancel closes it. Copy through the non-modal confirmation into a
    scratch database: 6 files, result tab. Database box restored.
  - Harness traps: the DragSource form starts DoDragDrop on mouse-down, so clicking it doesn't
    activate it - use a plain form (`clicktarget.ps1`) as the click-away target, and check it exists
    before clicking. After a drag the cursor rests on the K icon, where AutoHide deliberately keeps
    the panel - move the cursor before a focus change is meant to hide it.
  - Two real copies at 12:34:37 (full W511BT package) and 12:34:49 (Kits from TrueTest 1.9 debug
    Mobile) were not from these tests.
- **Kit Helper 1.10.13: group headings show the full destination path, start-trimmed.** Was
  `TrueTest\ScratchDustDetect_NY` (the backup layout). Now the folder of the group's files' `To`
  when they all share one (else the group name: Not copied, a vignetting group across folders).
  `FitPathEnd` cuts the START: longest "…\tail" made of whole trailing folders that fits, else the end
  of the last folder; full path in the tooltip and in Copy as text. Trap: a vertical StackPanel
  arranges a child at max(its width, child's desired width), so the heading row grew to the full path
  and the TextBlock's ActualWidth always "fit" (first try overlapped the counts). Fix: start the
  heading short, measure the row (`header.SizeChanged`) minus its other children. Checked in a harness
  with a dry-run preview: TrueTest 1.9 paths in full; a long fake database path →
  "…\for the trimming test\DATABASE\InputIMG\X4022-CB-C2_POR_DX_SUNBEAM", clear of "6 new".
- **Kit Helper 1.10.14: opening a folder unblocks it before its confirmation.** Chau: since opening
  a folder presses its Copy all / Copy package (1.10.9), it should press Unblock too - CopyFile keeps
  the Zone.Identifier stream, so a downloaded kit arrives blocked in TrueTest. `UnblockThenOffer`
  (from `OpenFolder` when `offerCopy`): only when there is a highlighted action (not a parent full of
  kits), only the opened folder or the picked ones, skipped while busy; then the confirmation if the
  same folder is still open. `UnblockFolders` is shared with the Unblock button (same log wording).
  Tested: scratch copy of the X4023 OMIT with Zone.Identifier on all 21 files, dropped on K → 0
  marked, "Confirmation of Copy all → ScratchDustDetect_NY" (cancelled). First run's drag didn't
  arrive (fired right after the app restart) - check the drag-result file before reading the outcome.
- **Kit Helper 1.10.15: only the detected copy button is shown.** Chau found "Copy all → POCB4.2" on
  an Omit kit wrote its model folder into POCB4.2\X4022-…, and LGDVHCustomApp listed files as
  replaceable - any button "worked" on any folder. Now, with a recognised kit, every other Copy all
  button is collapsed (the detected one stays, to reopen the confirmation after Cancel; packages
  already showed only Copy package). Nothing recognised: all buttons + "Kit type not recognised: pick
  the copy target yourself, or open the folder that holds the kit files." The hint now says
  "Detected:"; section title "Copy kit → TrueTest / Database". Real drags: X4022 Omit → only
  ScratchDustDetect_NY; outer kit folder → all + hint; dated kit → only Copy package. Confirmations
  cancelled, no backups.
- **Kit Helper 1.10.16: "Show all copy targets", and other-type files left out.** Chau: give a way to
  see the hidden buttons, but a wrong button must not copy files of the wrong category. Chose "rule out
  other-type files" over "rule out everything". `Kit.OwnersOf(file)`: name signatures (same as
  Suggest) → target(s) the file belongs to, null = generic; LGD_PO_Compensation / LGD_COMP_INI belong
  to POCB4.1Net AND Database model. `Copier.ByCategory(on, …)` ([ThreadStatic], like Excluding):
  Run / RunRouted list such files as Not copied "<type> file, not for <target>". On only when a
  button other than the detected one copies a recognised kit (`RulesOut`); the detected button,
  packages and unrecognised folders are unchanged. `ChkAllTargets` shows with a detected kit and
  resets per folder; the confirmation header adds "Detected as X: files of other kit types are left
  out." Headless: X4022 Omit model → POCB4.2 with rule: 6 ruled out, PANEL_SHAPE tif New; without:
  all New; → ScratchDustDetect_NY with rule on: all New. Real app: switch → 7 buttons, POCB4.2
  confirmation "Copy 1 file(s)? … 8 not copied", cancelled. No backups.
- **Kit Helper 1.10.17: unblock is an option in the confirmation (ticked), not done on open.**
  Chau: like the Vignetting panel, a checkbox in the confirmation, checked by default. 1.10.14's
  `UnblockThenOffer` removed (opening a folder unblocks nothing again). `CopyResultWindow.OfferUnblock
  (files)`: "Unblock the kit files first" + "N of M file(s) are marked…" (`Unblocker.IsMarked` =
  File.Exists on `\\?\file:Zone.Identifier`), shown for Copy all, Copy package and Database model
  (not Kits from old TrueTest). Copy with it ticked → `UnblockFolders` on the sources (model folders,
  package folder / picked folders) after the TrueTest question, then the copy. `ConfirmCopy` now
  returns the window (Excluded, UnblockFirst). Tested in the real app: scratch InputIMG\D992 model
  with all 6 files marked, database box on a scratch folder → confirmation "[On] 6 of 6 … marked",
  Copy → kit 0 marked, 6 copied files 0 marked. Database box restored.
- **Kit Helper 1.10.18: "Unblock all files"; Unblock + Vignetting in Kits from old TrueTest.** Chau:
  too many words. The option is now just a checkbox "Unblock all files" (what it does and "Now N of M
  file(s) are marked" moved to its tooltip; TxtUnblock removed). `ShowMigrate` builds its own
  confirmation with `OfferVignetting` + `OfferUnblock`: ticked Unblock → `UnblockFolders` on the old
  install's selected folders; a vignetting choice → `Vignetting.ApplyToCopied` after
  `KitMigration.Run` (its items carry From/To, so CopiedInis works). Real app: Kits from old TrueTest
  (1.9 debug Mobile → 1.9, POCB4.1Net ticked) → confirmation with "Vignetting 4 INI file(s) … now 1 in
  3, 1 in 1 DX" and "[On] Unblock all files" (tooltip 0 of 49 marked); cancelled, no backup. The
  vignetting edit after a real migration was not run.
- **Kit Helper 1.10.19: the panel's Vignetting list sees INIs in subfolders.** Chau: D994 1stPUC
  `InputIMG` (2 model folders) was a package and its confirmation offered vignetting, but the panel's
  Vignetting list said "no ini": `RefreshIni` / Set ON-OFF read only the folder's own *.ini (the INIs
  are in `D994-CB-FMM_MATHON\`). Now `KitInis()`: the open (or picked) folders, recursively via
  `Vignetting.IniFilesDeep` (3 levels, ≤200 folders) when a kit or package is recognised (`deepInis`),
  else top level only, so a parent full of kits can't have Set ON reach into all of them. Rows show the
  path relative to the open folder. RefreshIni moved after detection in SetSource. Real app: drop of
  that InputIMG → list "D994-CB-FMM_MATHON\LGD_COMP_INI_D994_Mono_1st/2nd_GIB.ini = 1"; confirmation
  "6 INI file(s) … now 1 in 6" (cancelled).
- **Kit Helper 1.10.20-1.10.22: Run test across a package / several folders.** Chau: Run test only
  found inputs for a single folder. `RunInputFolders`: the folder, OMIT/PUC model sources, and for a
  package (or a pick) every unit's folder plus a mirror unit's subfolders (TrueTest\ScratchDustDetect_NY,
  \PUC, \POCBMobile) → `Kit.RunInputs` on each, shown relative to the open folder. Mode radios:
  a user's switch (`RunMode_Changed`, not while `settingMode` from an input change) calls
  `PickInputForMode`: the first listed input whose `ModeOf` matches; none → the box is emptied (1.10.22,
  so a PUC input can't run in POCBMobile mode). RbOmit got the Checked handler too.
  - **1.10.20 crashed at start** (NullReference in `CheckedMode`): RbOmit's `IsChecked="True"` raises
    Checked during InitializeComponent, before RbPuc / RbPocb exist. Kit Helper was down ~1 min on
    this PC; 1.10.21 guards on the controls being built. Exactly the ForRadiant\CLAUDE.md
    verification habit: after every UI build, confirm the process runs and no APPCRASH first.
  - Real app: X4022 1st PUC package → 2 Omit + 2 PUC inputs, PUC ↔ Omit switches input + working dir;
    W511BT OMIT+PUC pick → 6 Omit + 5 PUC; D997 1st CPP package → 10 POCBMobile + 6 Omit, POCBMobile →
    a template with the POCBMobile dir, PUC → empty, Omit → Omit input. Confirmations cancelled.
- **Kit Helper 1.10.23: the Run test input list shows only the selected mode's inputs.** Chau: Omit
  listed PUC inputs and vice versa, and POCBMobile (none in the kit) still listed everything.
  `allRunInputs` holds every input found; `ShowInputsFor(mode, keep)` fills the box with that kind's
  only (none → empty box); `SelectInput(path)` sets the mode radios (settingMode) then the list;
  SetSource starts with the current input's kind, else the first input's; a mode click
  (`PickInputForMode`) refills the list, keeping a browsed file from elsewhere if it already matches;
  a dropped .txt goes through SelectInput. Real app, W511BT kit: Omit → 6, PUC → 5 (PUC model
  folder as working dir), POCBMobile → 0 / empty, Omit → 6. Started cleanly (checked first).
- **Kit Helper 1.10.24: Run test inputs listed by file name only** (Chau: no OMIT\ / PUC\ prefix).
  `RunInputItem` shows `Name`; `ShowInputsFor` sets `ShowFolder` only when two listed inputs share a
  name ("x.txt  (folder)"). The 1.3.2 reason for relative names (inputTemplate\x vs inputTest\x) no
  longer applies: those file names differ by prefix and the list holds one kind at a time.
  `RunInputPath` still uses the item's full path. Real app, W511BT: Omit 6 / PUC 5 bare names, PUC
  working dir still the model folder. First drag after the restart didn't arrive again (harness).
- **Kit Helper 1.10.25: a package's loose root files go to TrueTest's data folder.** Chau: the .seqxc
  belongs in `C:\Radiant Vision Systems Data\TrueTest\Sequence`, the other loose files (PANEL_SHAPE
  X4023 G.bmp, X4023-subframe-FOV85%.txt) in `AppData\1.8` and `AppData\1.9` - both, he chose that
  over "match the selected install". The station already had them there (Sequence\X4023-…seqxc,
  AppData\1.9\PANEL_SHAPE X4023 G.bmp). `Package.Scan` puts root files into `Layout.DataFiles`
  (`IsDataFile`: not exe / bat / cmd / ps1 / lnk / url / md / README*; those stay "not in a kit
  folder"); `DataDestinations`: Sequence for .seqxc, else the AppData\1.8 / 1.9 that exist. `Execute`
  copies them with `Copier.CopyOne` (confirmation, untick, backup as "TrueTest Data\…", result); Describe
  lists them. Picked folders don't include root files. Dry-run preview of the W511BT kit: seqxc
  Replace in Sequence, bmp Replace in 1.9 / New in 1.8, subframe New in both; data folder 1549 files
  before and after. Real app: package confirmation shows the three data groups; cancelled, no backup.
  - Seen while testing: six real copies into TrueTest 1.9 at 10:56-10:57 (OMIT, OMIT, POCB4.2, PUC, R2R,
    then the 4-folder package of the W511BT kit), not from these tests - Chau using 1.10.2. Backed up
    in `D:\Program\RVS\Kit Helper Backups\20261126_1056*` / `_1057*`.

## 2026-11-25

- **VideoBrowser: clicking the speaker icon didn't mute** (`MainForm.ToggleMute`/`ApplyAudio`).
  It toggled `_mp.Mute = !_mp.Mute`, but libvlc 3 applies mute asynchronously (the getter right
  after the setter still returns the old value, so clicks flipped the wrong way), does nothing
  while no audio output exists, and a new file came up unmuted. Now `_muted` is the source of
  truth; `ApplyAudio` pushes `Volume = 0` + `Mute` and is re-run on every `Playing` event.
  Tested by `BM_CLICK` on the button + reading the app's Windows audio session
  (ISimpleAudioVolume): toggles each click, next file stays vol 0 / muted from its first sample.
  The session *peak meter* kept moving while muted, so it is pre-session-volume — don't use it
  as the "is it silent" signal; endpoint meter was useless because other audio was playing.
  Deployed to `D:\PortableApps\VideoBrowser` (robocopy, `settings.txt` excluded; this also
  delivered the not-yet-deployed seek-bar thumbnail build).

- **MultiRemoteTool: contact list couldn't scroll to the last rows on first launch** (fixed
  until the window was resized). Cause in `ContactListView.UpdateScrollBar`: it computed
  `Maximum = maxFirst + _scroll.LargeChange - 1`, reading `LargeChange` back — but WinForms
  clamps that getter to `Maximum - Minimum + 1`, which is 1 while `Maximum` is still 0, so
  the range collapsed (RustDesk, 11 contacts: thumb filled the track, wheel did nothing).
  Each resize re-ran it from the previous Maximum, so it slowly converged. Fix: use the local
  value, set `Maximum` before `LargeChange`, and clamp the wheel to a stored `_maxFirst`.
  Verified on a copy of the deploy data (scrolls to row 11 at launch); exe copied to
  `D:\PortableApps\MultiRemoteTool` (CLAUDE.md's deploy table says `E:\` — on this PC it's `D:\`).

## 2026-11-21

- **Kit Helper 1.9.3: LGD package roots with inputTemplate\ / mainTemplate\ were not packages.**
  First made on this PC as 1.8.2 (97e43c31, never pushed); the other PC meanwhile shipped
  1.9.0–1.9.2 without it, so this PC was reset to origin and the same `Core.cs` change re-applied.
  Reported on `D:\Log\Kit\D994-GIB FMM Installation kit ...`. That outer folder is (by design) not
  a package: it holds alternative station packages (1stPUC DongA / 2ndPUC Dooone) that must not
  be copied together. The real miss was its `2ndPUC` subfolder: `Kit.Suggest`'s 1.3.2 rule
  "folder contains inputTemplate\ → POCBMobile kit" claimed the whole package root, and
  `Package.Is` rejects any folder Suggest recognises. The `mainTemplate\ → POCB4.2` rule did the
  same to the X4024 kit. Fix: those subfolder rules are skipped when the folder looks like a
  package root (a .seqxc, or OMIT\ / PUC\ / InputIMG\ inside). A package's `mainTemplate\` is now
  its own unit → `<TrueTest>\POCB4.2\mainTemplate` (where all four local TrueTest installs keep it).
  - Swept every folder and subfolder of `D:\Log\Kit` with `Package.Is` (exe loaded in Windows
    PowerShell 5.1): 16 dated package folders went False→True (every D993/D994 2ndPUC, D993-MP,
    X4024), none went True→False. Dry-run Preview of X4024 sends main_*.txt to
    `TrueTest 1.9\POCB4.2\mainTemplate\`. The UI itself was not clicked through.
  - Manual (Claude Doc) was brought up to 1.8.2 today by a side session, including this fix; the
    1.9.x features from the other PC (tray icon, icon on taskbar...) are not in it yet.

- **VideoBrowser: seek-bar hover thumbnails** (MPC-HC / VLC style). New `VideoBrowser\SeekPreview.cs`;
  `SeekBar` gained `HoverMoved` / `HoverEnded` events; the plain time tooltip on the seek bar is gone
  (the popup shows the time under the frame).
  - **How:** a second, hidden `MediaPlayer` (on its own `LibVLC`, see the mute trap below) opens the playing file with
    `:no-audio :no-spu :input-fast-seek :avcodec-hw=none`, decodes into a small RV32 buffer via
    `SetVideoFormatCallbacks` + `SetVideoCallbacks` (frame scaled to fit 224×126, aspect kept), and is
    paused on its first frame. Seeks while paused still emit a frame through the display callback
    (libvlc 3.0.21) — that is what makes this work. One seek in flight at a time (600 ms timeout),
    results cached in 400 buckets along the bar, cleared per file.
  - **Trap (shipped once, fixed same day):** the first version created the hidden player with
    `Mute = true` on the *shared* `LibVLC` → the real video played muted. In libvlc 3 every player
    gets an audio output at creation and on Windows they share the process's audio session, so a
    mute/volume on any player hits all of them. The preview now has its **own** `LibVLC` with
    `--aout=adummy --no-audio`. Verified with a Core Audio session probe (scratch script): playing
    a tone file → session `mute=False`, peak ≈ 0.24.
  - **Trap:** lock and display callbacks run on *different* threads — don't hold a Monitor across
    them (first draft did; `Monitor.Exit` from the other thread fails). Only the copy is locked.
  - **Trap:** the hidden player also holds the file open. `_preview.Close()` must run wherever
    `_mp.Stop()` releases the file (Stop button, delete-the-playing-file path) or Recycle Bin fails.
  - Popup = owned, `WS_EX_NOACTIVATE|TOOLWINDOW|TOPMOST|TRANSPARENT` form + `HTTRANSPARENT`, so it
    never steals focus or the mouse and stays above the video in fullscreen.
  - Tested on a synthetic 2-min AVI (numbered frames, made by a throwaway Python script since there's
    no ffmpeg here): thumbnails at 25/50/75/95 % show the right second, ~150 ms per new position,
    hides on leave, works in fullscreen, file unlocked after Stop. Not tested on big HEVC/4K files.
    Deploy copy `D:\PortableApps\VideoBrowser` **not updated** (it was running).

- **Orb flashed when the taskbar was clicked: Quick Launcher 2.4.1, Kit Helper 1.9.4** (and
  CharlieTheSlave). Built first as Kit Helper 1.9.3, but another session had already pushed a
  different 1.9.3 (`042f2067`, LGD package detection), so this is 1.9.4 and contains both.
  **Before bumping a version, `git fetch` and check origin's csproj**: sessions on the same repo
  pick the same next number. The 1 s re-raise timer left it buried behind the taskbar for up to ~1.4 s.
  `TaskbarZ.Keep` now also hooks `SetWinEventHook(EVENT_SYSTEM_FOREGROUND..EVENT_SYSTEM_CAPTURESTART)`
  (out of context, own process skipped) and re-raises at once; CAPTURESTART is what catches a click
  on a taskbar that was already in front. Timer kept as a backstop; the hook delegate is held in a
  static list so the GC can't collect it. Measured (taskbar brought forward with an Alt tap +
  SetForegroundWindow, z-order sampled every 5 ms, foreground handed back between rounds): old
  531 / 169 / 758 / 400 / 0 ms buried, new 0 in all five, for all three apps.
  - Deploy trap: removing the leftover `Quick Launcher.old.exe` *before* copying made the copy fail
    on 5 PCs ("Cannot find path …Quick Launcher.exe", from the rename fallback). A minute later both
    files were free and a plain copy worked. Copy first; delete the .old only after a good copy.
    vpcca15fg was offline at the time; updated to 2.4.1 once it was back (all 9 PCs hash-checked
    on 2.4.1). Leftover `.old` files removed where free; victus16 still runs its old copy from
    `.old` until Quick Launcher is restarted there.
- **Smaller orb that may sit on the taskbar: Quick Launcher 2.4.0, Kit Helper 1.9.2** (and
  CharlieTheSlave, see DevWorkspace\WORKLOG.md). Window 60 → 44 px, circle 48 → 36 px, glyph
  smaller: fits inside the Windows 11 taskbar (48 px). `KeepOnScreen` now clamps to the monitor's
  `Bounds` instead of its `WorkingArea`, so the orb may stay on the taskbar but never hangs off a
  screen. The taskbar is topmost too and comes to the front when clicked; while the orb overlaps
  a taskbar (outside the work area) a 1 s timer re-asserts `HWND_TOPMOST` (`TaskbarZ.cs`), only
  then, so it doesn't fight other topmost windows elsewhere. Tested headlessly: at y=1034 on the
  1032-1080 taskbar it stays and reports "over taskbar"; at y=1070 it snaps to 1036.
  - Quick Launcher deploy found it **running on 7 of the 8 other PCs**: their exe was renamed to
    `Quick Launcher.old.exe` and the new one copied beside it (new version on next start; the
    next deploy deletes the .old file).
- **Kit Helper 1.9.1: "Close TrueTest before copying" checkbox removed** at Chau's request. The
  popup it gated (`AskCloseTrueTest` → AskWindow: Close TrueTest / Copy without closing / Cancel,
  then `OfferRestart`) now always appears when TrueTest is running; no question when it isn't.
  The `CloseTrueTest` setting is no longer read or written (old values in settings.txt are inert).
  Hint under the copy buttons mentions the question. Self-installed to `D:\Program\RVS`, running,
  no APPCRASH. The popup itself was not exercised (TrueTest was not running).
- **Kit Helper 1.9.0: icon can't be lost off-screen + tray icon** (same as Quick Launcher 2.2.1/2.3.0).
  `FloatingIconWindow.KeepOnScreen` (after a drag, in `OnSourceInitialized`, on
  `DisplaySettingsChanged`, and from `IpcHost` when Kit Helper is started again); new `TrayIcon.cs`
  (click = panel or the open dialog via `FocusModal`; Show panel / Bring icon back here / Exit);
  `MoveToMainScreen` is the old "Default position" made public. Built with MSBuild; started
  `bin\Release` once so it self-installed to `D:\Program\RVS` (1.9.0+20261121104622, running, no
  APPCRASH). Not committed: line PCs get it on the next commit + pull.
  - Test-harness trap: Kit's `MainWindow` constructor NREs when hosted in PowerShell, because
    `Assembly.GetEntryAssembly()` is null there. Fix in the harness, not the app: set
    `AppDomainManager.m_entryAssembly` to the loaded exe and install that manager on the domain.
    Also `LoadFrom` is refused for these exes inside the Bash sandbox (0x80131515); `UnsafeLoadFrom` works.
- **`GameLauncher` → `QuickLauncher`** (v2.0.0): Games tab + new Apps tab. Folder renamed in place
  (it was never committed). Deployed to `D:\PortableApps\QuickLauncher`; desktop shortcut
  `Quick Launcher.lnk`. Orb glyph is now a lightning bolt.
  - **Apps come from the shell's `shell:AppsFolder`** (`StartMenu.cs`), not from walking the Start
    Menu `.lnk` folders: it is Start's own list, includes Store apps, and lists ~170 entries in
    ~0.25 s. Parsing names are `{KnownFolderGUID}\rel\x.exe` (resolved with SHGetKnownFolderPath to a
    real exe → normal file icon, Open folder, run as admin), `Microsoft.AutoGenerated.{…}` (a Start
    `.lnk`), or a Store AUMID. The last two are launched as `shell:AppsFolder\<name>` via
    ShellExecute, which is how Start does it; verified by opening Calculator through `Launch`.
  - Store-app icons need `IShellItemImageFactory`; its HBITMAP is a 32-bit premultiplied DIB section,
    and `CreateBitmapSourceFromHBitmap` drops alpha, so pixels are copied by hand (flipping if
    bottom-up). Shell.Application is apartment-threaded → the scan runs it on its own STA thread.
  - Apps also scan folders (default `D:\PortableApps`, ~125 portable apps). Start-menu entries whose
    exe is already listed from a folder, or lives under a **game** folder, are left out of Apps.
  - Per-tab state: `games.tsv` / `apps.tsv`, settings `GamesRoots` / `AppsRoots`,
    `GamesFoldersOff` / `AppsFoldersOff`. Search, sort, show-hidden and the orb's Recent are shared.
    Drops go to the tab that is open. An empty search shows "N matches in Games (Ctrl+Tab)".
  - **Migration** (`Program.cs`, runs before Settings is touched): first start with no
    `%APPDATA%\QuickLauncher` copies `GameLauncher\library.tsv` → `games.tsv`, prefixes
    `Roots`/`FoldersOff` with `Games`, copies `Shortcuts\` and rewrites paths to it. Old folder
    left in place. Checked on the real data: 5 hidden, 4 manual, 11 play histories carried over.
  - Fix: `jdownloader-portable` found nothing: `JDownloaderPortable.exe` hit the "downloader"
    bad-name rule (meant for COD's `ccsdownloader.exe`). A bad word is now excused when it is part
    of the folder's own name, and only that word (an "excuse the whole exe if named like the
    folder" rule let `harddisksentinelupdate.exe` win Hard Disk Sentinel; rejected). Full
    before/after dump diff: JDownloader was the only change.
  - v2.1.0: version + build time in the panel header (`BuildInfo.cs`, informational version
    `2.1.0+yyyyMMddHHmmss` as in Kit Helper, also readable from file properties).
  - Games default folders are now every fixed drive's `\Games` (not F:/G: hard-coded), so a fresh
    copy works on the desktops (E:) and laptops (D:). Network drives skipped: Z: there is this PC's F:.
  - **Copied to all 8 other PCs** (`\\<pc>\D\PortableApps\QuickLauncher`, hash-checked): i5-2500,
    i5-3470, i5-3570, i5-4670, i3-9100f, tp550ld, vpcca15fg, and **victus16** (the strong laptop;
    not in the F:\Games notes). victus16 has no `Games` folder on any shared drive, so its Games
    tab would start empty: its games are in **E:\GGG**, so its first `settings.txt` was pre-written
    with `GamesRoots=E:\GGG` (`\\victus16\Chau\AppData\Roaming\QuickLauncher`; UTF-8 without BOM,
    a BOM would turn the first key into "\uFEFFGamesRoots"). Not started on the other PCs.
  - Desktop shortcuts `Quick Launcher.lnk` on all 8 other PCs. **Every PC shares its user folder
    as `\\<pc>\Chau`** (shares are `Chau`, `D`, `E`); `\\<pc>\D\Users` is not listable there, and
    `net view` works now (the F:\Games notes say error 1702, no longer true).
  - **v2.2.0, speed** (measured off-screen on the real ~210-app list, old → new):
    panel open 1.85 s → 0.28 s; full repaint 26 → 13 ms; keystroke re-filter 10-50 ms → once per
    90 ms pause. Causes, in order of cost:
    1. Icons read on the UI thread (~1.6 s for all). Now `IconLoader.cs`: one STA worker thread.
    2. A `DropShadowEffect` on the panel's root Border: every repaint re-ran the blur over the whole
       panel (26 ms with it vs 8 without). Moving it to a sibling layer still cost 15 ms wherever a
       repaint overlapped it, so it is gone: three faint rounded Borders fake the shadow for free.
    3. 256 px jumbo icons scaled to 64 with HighQuality on every repaint: now shrunk once to
       64 DIP × screen scale (`IconLoader.Shrink`).
    4. A `ContextMenu` instance per tile (~270): now one shared menu opened from MouseRightButtonUp.
    Measuring trick: RenderTargetBitmap.Render of the window (or one layer) in an `-STA` PowerShell
    that loads the exe by reflection; script kept in the session scratchpad as `perf.ps1`/`perf2.ps1`.
  - Scrollbar 9 → 14 px, thumb lighter on hover and accent-coloured while dragged.
  - v2.2.1: the orb could be dragged off-screen with no way back (its right-click menu's "Default
    position" is unreachable then). `FloatingIconWindow.KeepOnScreen` clamps it fully into the
    work area of the nearest monitor (`Screen.FromHandle`) after every drag, at start, on
    `DisplaySettingsChanged`, and when Quick Launcher is launched again (shortcut = way back).
    This PC has 3 monitors (DISPLAY3 at x -1920, DISPLAY1 0, DISPLAY2 1920); a check against the
    primary work area alone wrongly flagged snaps onto the side monitors as failures.
  - v2.3.0: tray icon (`TrayIcon.cs`, WinForms NotifyIcon) for when the orb's monitor is hard to
    reach: click = show/hide panel; menu Show panel / **Bring icon back here** (orb to its default
    spot on the main screen, saved position forgotten) / Rescan / Exit. Disposed on `app.Exit` so
    it does not linger. A deploy that force-kills the running copy still leaves a ghost tray icon
    until the mouse passes over it.
  - Deploy now renames a running remote exe to `Quick Launcher.old.exe` and copies beside it (a
    running exe can be renamed, not overwritten); none was running this time.
  - "VideoBrowser not found" was not a detection problem: it was in `apps.tsv`, unhidden, and a
    search of the live list found it; it showed after a restart.
  - `NiceName` now also strips platform/build tails (`-windows-x64`, `-net7.0-win7-x64`,
    `-Binaries-Latest`, `CrystalDiskInfo8_11_2`); also tidied `Dolphin-x64`, `duckstation-…`.
- **New tool `GameLauncher`** (WPF, net48): Kit Helper's floating-orb + auto-hiding panel UX and
  its `Theme.xaml` palette (copied, plus `Tile`/`Field` styles), but a grid of game tiles.
  Scans `F:\Games` and `G:\Games` by default (editable in the panel, `;`-separated; dropping a
  folder on the orb adds it). Library + user overrides in `%APPDATA%\GameLauncher\library.tsv`.
  - **Detection model:** each direct subfolder of a root = one game; all exes up to 5 levels deep
    are scored, best wins, the rest stay under right-click → Choose exe. Score = 2·log10(size)
    − 1.5·depth + name-match with the folder + penalties (launcher/server/editor/config).
    Exes named like crash/setup/redist/anticheat/`crs-*`, and dirs like `_crack`,
    `_original files`, `Engine`, `DevTools`, `jre*`, `redist`, are dropped outright.
  - **Unreal rule:** an exe sitting in the folder that contains `<Proj>\Binaries\Win64\*-Shipping.exe`
    gets +5 — that's the store stub (`KOFXV_Steam.exe`, `Expedition33_Steam.exe`, `SB.exe`).
    Earlier "stub name = shipping prefix" rule missed every case where the names differ.
  - Verified with `"Game Launcher.exe" --dump out.tsv` (no UI): 125 games, ~6 s cold scan, picks
    checked by eye. **Git Bash rewrites `/dump` into a path**, hence `--dump` is accepted too.
  - **Trap:** `StartupScan` (async) must be queued with `Dispatcher.BeginInvoke` *before*
    `app.Run()`; called directly, there is no dispatcher SynchronizationContext yet and the
    `await` resumes on a pool thread → cross-thread crash.
  - Folder filter = one pill toggle per root (+ "Added manually"), each on/off independently;
    right-click a pill = show only that folder, again = all. Switched-off set saved as
    `FoldersOff` in settings. (A single-choice dropdown came first; Chau wanted per-folder
    toggles instead.) Pills are rebuilt on every filter pass so counts track hide/rescan.
  - Search box ✕ (shown only while there is text). `Field` style now takes `Padding` via
    TemplateBinding so the search box can reserve room for it.
  - "F:\Games has 92 folders but 86 games": the 6 are correct skips — Heroes III repack setup
    only, `Roms` / `ROM RetroGame` (ROM tools), `TopRoms Collection` (ROMs, no exe),
    `SteamUnlocked` (unfinished download), `VersionSwitcher` (19 KB tool, under the 20 KB cutoff).
  - **Testing trap:** don't drive the panel with SendKeys. It auto-hides on deactivate, and the
    keystrokes then land in whatever window has focus (they went into the Claude chat box once).
    Test by loading the exe in an `-STA` PowerShell with reflection and rendering the window
    off-screen with RenderTargetBitmap, like the MultiRemoteTool `PlatformDialog` check.
  - Deployed to `D:\PortableApps\GameLauncher` (exe + config only); desktop shortcut
    `Game Launcher.lnk` retargeted there. Added to the deploy-folder table in `CLAUDE.md`.
  - Folder pills moved to their own full-width `WrapPanel` row: with them beside the search box,
    a third pill ("Added manually") already squeezed the search field.
  - **Bug: a manually added exe could vanish.** `Rescan` snapshots `games` at the start and
    replaced the whole list at the end, so an exe dropped during the ~5 s startup scan was
    overwritten; `Library.Save` also swallowed every error. Now manual games are taken from the
    live list after the await, `Save` falls back to an in-place write if `File.Replace` fails and
    returns the error (shown in the status line and as a toast), and `Load`/`Save` skip duplicate
    folders. Seen live: library.tsv had 125 rows while the panel showed VersionSwitcher as #126.
  - Drop on the orb now takes desktop shortcuts (`Shortcut.cs`): a `.lnk` to an exe with no
    arguments is added as that exe, named after the shortcut; anything else (`.lnk` with args,
    `.url`) is **copied** to `%APPDATA%\GameLauncher\Shortcuts` and the copy is launched, so the
    desktop original can be deleted. Of the 58 shortcuts on Chau's desktops, 50 resolve to an exe
    and 8 keep a copy (Path of Building, Chrome, Edge, Restart/Shutdown…).
  - Dropping something that is already a game now acts instead of saying "already in the list":
    unhides it, switches its exe if the dropped one is another of its candidates (POE.lnk →
    `PathOfExile_x64.exe` while the scan picked `PathOfExile.exe`), and switches its folder pill on.
  - **Trap: the Bash tool sees a sandboxed copy of `%APPDATA%`** (stale reads, private writes).
    The "VersionSwitcher missing / duplicated" scare earlier today was that copy, not Chau's real
    library. Check real app data from the PowerShell tool. (Handy the other way: drop tests run
    from Bash cannot touch the real library.)
  - Fix: tiles did not re-sort after a launch (the orb's Recent menu did; it is rebuilt on open).
    `Launch` relied on `TxtSearch.Text = ""` to re-filter, which fires only if there was text.
    Now calls `ApplyFilter()` itself.
  - Icons: shell jumbo image list (256 px), falling back to 48 px when the exe only has a small
    icon (detected by the non-transparent width), so tiles never show a tiny icon in a corner.

## 2026-11-20

- **VideoBrowser: control bar in fullscreen.** Fullscreen used to hide `_controls` with no way back.
  Now it's undocked (`Dock=None`, anchored bottom, `BringToFront`) so it floats over the video
  without resizing it. First version (bb7c7052) showed it on any mouse movement with a 2.5 s
  auto-hide; changed on request to MPC-HC behaviour: hidden always, visible only while the cursor is
  over the bar's own area (or a slider is being dragged). The cursor is polled by a 50 ms `_fsTimer`
  that runs only in fullscreen (250 ms felt too slow for hover-reveal), because the LibVLC video
  surface is a native child HWND and its MouseMove events can't be relied on. Trap: on leaving
  fullscreen, `_split.BringToFront()` is needed to restore dock order (Fill must have the lowest
  z-index), otherwise the bottom panel overlaps the video.
- **VideoBrowser: click on video = play/pause** (windowed and fullscreen). Toggles immediately (no
  wait for the double-click timeout, which would add ~500 ms lag); the double-click handler toggles
  back before switching fullscreen, so a double-click leaves pause state unchanged (brief hitch, same
  as MPC). Only acts in Playing/Paused, so a click after Stop/End doesn't restart the file.
- **VideoBrowser icon**: `VideoBrowser\app.ico` (blue tile, play triangle + playlist lines; ≤24 px
  shows only the triangle), generated with System.Drawing from a throwaway PowerShell script, PNG
  entries 16–256. Wired via `<ApplicationIcon>` + `Icon.ExtractAssociatedIcon(Application.ExecutablePath)`,
  same as CredentialManager/EmailBrowser.

- **`E:\Dev` moved into `toyprojects\DevWorkspace`**, its own `.git` dropped (history lost, by choice).
  Compared first with `\\victus16\E\Dev`: 0 files newer there, 1 file only there (a runtime DLL in
  YdkeDeckBuilder `bin`), 541 newer here — so this PC's copy was the one kept. Source only is committed
  (`DevWorkspace\.gitignore`: bin/obj/publish/dist/.vs/.venv/__pycache__/.env): 325 files / 36 MB
  instead of 1.2 GB. The three `.env` files (Telegram bot tokens) stay local; `.env.example` is
  committed. Same-volume move kept the folder's ACLs (Low integrity label, Medium on
  `c#\YdkeDeckBuilder\publish`, `Everyone:(DENY)(DC)`). Repointed the "YDKE Deck Builder" Start Menu
  and Desktop shortcuts. `\\victus16\E\Dev` was left untouched.

- **Kit Helper 1.8.1 (rebuilt, version kept on request): `BlockFiles.exe` is a helper tool**
  (`Kit.IsHelper`), like UnblockFiles. LGD's POCB4.1Net / InputIMG folders ship it, and it was being
  copied into TrueTest and the Database. Checked on the D997 kit preview: both copies now Not copied.

- **Kit Helper 1.8.1: state colours on the popups** (`CopyResultWindow.SetMood`, also used by
  `AskWindow`). The window outline, a tinted title band, a badge and the title text take one
  colour: confirmation = Warn/amber "CONFIRM", result = Ok/green "DONE" or Err/red "ERRORS" (any
  Failed item), the TrueTest question = amber "QUESTION". Checked by rendering all four.

- **Kit Helper 1.8.0: untick files in the confirmation; report titled "Result of …".**
  - Confirmation (Copy package, Database model): a checkbox per file that would be written,
    ticked by default; a folder checkbox ticks / unticks its group and shows a "–" when only some
    are ticked (new mixed-state look in Theme.xaml's CheckBox). Select all / Select none in the
    footer. Live counts ("… · 3 unticked"); OK is disabled when nothing is ticked.
  - Unticked = destination paths (`CopyResultWindow.Excluded`), passed with
    `Copier.Excluding(set, () => copy)`. That is a [ThreadStatic] set read by `CopyOne`, the one
    place every copy path writes through, so no signature changed. The report lists them as Not
    copied, "unticked in the confirmation, left as it was". Keyed by destination, so one INI can be
    unticked for POCB4.1Net only. Vignetting only touches written files, so unticked INIs keep their
    value.
  - Tested on the D997 kit in a sandbox: 11 unticked (one INI's POCB4.1Net copy + all of
    LGDVHCustomApp) were left alone; that INI's POCB4.1 and Database copies were written; 57 = 68 − 11
    copied; the set is cleared after the copy. Window: 68 checkboxes, mixed folder state, Excluded=3,
    "3 unticked", Select none disables OK.
  - Harness gotchas: PowerShell variables ignore case (`$W` type vs `$w` window collided), and a
    HashSet passed to `MethodInfo.Invoke` must be `.psobject.BaseObject`.

- **Kit Helper 1.7.0: InputIMG folders always also go to the Database; vignetting option in the
  package confirmation.**
  - `Package.Scan`: an `InputIMG\<model>` folder whose own target is not the Database (D997's
    `D997-CB-CGDIM_TLSM`, "Copy all to POCB4.1Net" script) gets a second unit,
    `Database\InputIMG\<model>`. `00` model folders are unchanged (Database only). The station
    really keeps it there: the preview against C:\DATABASE showed all 10 files as Replace.
  - The confirmation shows a Vignetting panel when INIs with STEP_3_VIGNETTING_CORRECTION are being
    copied (current values listed): Keep as is (default) / Set ON (1) / Set OFF (0) + Include DX
    ini. After the copy, `Vignetting.ApplyToCopied` edits the copied files only (not the kit); the
    report lists them in a "Vignetting set to N" group as Set ("was 0") or Kept (DX).
  - `CopyItem` now carries From/To; `Result.Written` = New/Replaced items with a destination.
  - Tested on a sandbox copy of the D997 kit with an extra `_DX` INI: 2 units for CGDIM_TLSM, 21
    INIs offered (7 × POCB4.1Net / POCB4.1 / Database). With ON and no DX, 18 were set 0→1 and 3
    DX kept, at all three destinations; the kit INIs were unchanged.

- **Kit Helper 1.6.0: Copy package understands LGD's kit layout too** (`Package.Scan`).
  - LGD kits are category folders, some with a model folder inside: `OMIT\<model>\`,
    `PUC\<model>\`, `InputIMG\<model>\`, plus `POCB4.2\`, `R2R\`, `inputTemplateGIB…\`, and a .seqxc
    at the top. Each folder goes through `Kit.Suggest` (the same rule as opening it alone). A
    recognised folder is one unit for that target. Only `OMIT\ PUC\ InputIMG\` are searched one
    level deeper; any other unrecognised folder, and loose files (.seqxc, .bmp, README), are
    listed as Not copied. The TrueTest\ / DATABASE\ mirror layout still works and can be mixed in.
  - `InputIMG\<model>` goes to the Database unless its own "Copy all to X" script says otherwise:
    D997's `InputIMG\D997-CB-CGDIM_TLSM` has "Copy all to POCB4.1Net folder.exe" → POCB4.1Net.
  - Package root rule (so `D:\Log`, `D:\Log\Kit` and drop folders aren't packages): not itself a
    single-target kit, AND (TrueTest\/DATABASE\, or a .seqxc, or ≥2 known category folders), AND
    ≥1 unit. The first version without this treated `D:\Log` as a package with 25 scattered units.
  - The confirmation lists one line per unit ("OMIT\D997-CB-GIB_TLSM\ → ScratchDustDetect_NY"). It
    asks only for the install / database the units need, and closes TrueTest only when
    something goes into the install.
  - `Copier.Run(..., dryRun)` / `CopyOne(dryRun)`: the preview runs the real copy code without
    touching anything, so the confirmation can't disagree with the copy. The Database model
    preview uses it too.
  - Tested (PS 5.1, built exe) on the 11/11 BT kit, 11/04 MATLAB kit, D997 CGDIM kit and our
    package: detection as above, preview touched nothing, the real copy into an empty sandbox gave
    Items identical to the preview (77/98/67/43), files on disk = copied count, no helper tools.
    Also checked that `D:\Log`, `D:\Log\Kit`, the X402x drop folder, and a lone POCB4.2 or
    OMIT\<model> kit are not packages. `Is(D:\Log)` takes 1 ms.
  - Open question: `BlockFiles.exe` (D997 POCB4.1Net folder) is copied into TrueTest. It is not in
    `Kit.IsHelper` (UnblockFiles is), and the old POCB4.1Net copy exe copied it too.

- **Kit Helper 1.5.1:** the copy confirmation is titled "Confirmation of <action>", its buttons
  are Cancel / OK, and it has no "Copy as text" (that stays on the report only).

- **Kit Helper 1.5.0: no more Windows MessageBoxes.**
  - Copy confirmations (Copy package, Database model) are `CopyResultWindow` in preview mode
    (`Package.Preview`, or the model folder's files): every file grouped by destination as
    Replace (exists, backed up first) / New / Not copied, with Cancel / Copy (`ShowDialog()==true`).
    It's the same look as the report afterwards.
  - "TrueTest is running" and "Restart TrueTest?" use `AskWindow` (same chrome; `Ask` returns the
    button index, -1 when closed). The first button is primary and drawn on the right. The question
    no longer embeds the path: `Running:` / `Copy to:` lines under it. The first render cut the
    message off after "copying to" when the path was inside it.
  - Single-target copies still have no confirmation (unchanged); they show only the report.
  - Gotcha: a `\n` inside a C# string sent through a bash heredoc to Python lost its backslash,
    so the replace matched nothing. Use the Edit tool for such lines.

- **Kit Helper 1.4.1: the K icon works while a dialog is open.** WPF's `ShowDialog` disables every
  window of the app, the floating icon included, so with the copy report (or any Kit Helper
  dialog) open, clicking K did nothing, even when the dialog was behind other apps.
  `MainWindow.Modal` now re-enables the icon once the dialog is up (a BeginInvoke queued before
  the dialog runs inside its message loop, after the disabling; MessageBox pumps the dispatcher
  too). While `InModal`, an icon click, the menu's Show/Hide and Kits from old TrueTest, a drop on
  the icon, and Explorer's "Open in Kit Helper" all call `FocusModal`
  (`GetLastActivePopup` + `SetForegroundWindow`), which brings the panel and dialog back without
  toggling the panel or changing the kit folder under the dialog. A drop or open shows a toast.
  Verified with the real icon + panel and a dialog through `Modal`: 1.4.0 icon disabled during the
  dialog, 1.4.1 enabled. The foreground jump itself needs a real click and wasn't automated. The
  first harness hung (ContentRendered + BeginInvoke from a PowerShell scriptblock never closed the
  dialog); a DispatcherTimer started before ShowDialog, plus a kill timer, works.

- **Kit Helper 1.4.0: copy result popup** (`CopyResultWindow`). After every copy action (single
  target, Database model, Copy package, Kits from old TrueTest), a window lists each file grouped
  by the folder it went to (`TrueTest\POCB4.2\mainTemplate`, `DATABASE\InputIMG\<model>`…) with
  New / Replaced / Failed (+ reason) / Not copied (+ reason, grouped last). It also shows colored
  counts, the backup folder with "Open backup", and "Copy as text". Folders with a failure are
  listed first. Data: `Copier.Result.Items` (`CopyItem`), filled in `CopyOne` (New vs Replaced =
  whether the destination existed) and in every skip/failure branch. The log keeps its lines.
  Checked by running a real package copy into a sandbox (4 existing files, 1 read-only) and
  rendering the window: 37 new, 4 replaced, 1 failed with the access-denied reason.

- **Kit Helper 1.3.2: Run test finds POCBMobile inputs in inputTemplate\ / inputTest\.**
  The mode switch (Omit / PUC / POCBMobile) was never broken: it follows the picked Input .txt,
  and `Kit.RunInputs` only scanned the opened folder's top level. A package's `TrueTest\POCBMobile`
  keeps its inputs one level down, so nothing was picked and the mode stayed Omit. RunInputs
  now also reads `inputTemplate\` and `inputTest\`, and the list shows them as
  `inputTemplate\x.txt` / `inputTest\x.txt`. The package's ScratchDustDetect_NY had no
  `LGD_OMIT_INI_*.txt` at all (LGD's drop lacked them); they were added to the package, not to
  Kit Helper. Checked by opening each package folder in the real window offscreen:
  ScratchDustDetect_NY → 6 inputs, Omit; POCBMobile → 8, POCBMobile; POCBMobile\inputTest → 4,
  POCBMobile; PUC → 5, PUC.

- **Kit Helper 1.3.1: Database row at the top; log no longer cut off.**
  - The database box (`CboPocbDb`) moved from Run test, where it only showed in POCBMobile mode,
    to a "Database" row under TrueTest install. Copy package, Database model and the POCBMobile
    run all use it. Default: the saved pick, else the first of `C/D/E:\DATABASE` that exists. The
    line under it shows the model count and the other DATABASE folders on the PC; it is now
    saved in every mode.
  - Why the log was cut: the window is a fixed height (NoResize) and the tools row was Auto, so the
    extra 1.2/1.3 buttons pushed the `*` log row off the bottom. Now the tools row is `*` inside a
    ScrollViewer and the log row is a fixed 160 px. "Other" moved to the right column with
    its two buttons side by side (explanations as tooltips). The window is 990 px tall
    (ShowNear still clamps to the work area).
  - Labels: "Database", "Copy package → TrueTest install + Database", POCBMobile button
    `CopyTarget.Label` = "inputTemplate + inputTest", and the package line wraps.
  - Checked by rendering the real window offscreen (PS 5.1 STA, RenderTargetBitmap) in Omit and
    POCBMobile modes. Gotcha: Kit Helper calls `Assembly.GetEntryAssembly()`, which is null
    under PowerShell; set it through `AppDomainManager.m_entryAssembly` /
    `AppDomain._domainManager`. Never Close/Hide the window in a harness: HidePanel saves settings.

- **Kit Helper 1.3.0: every copy is backed up; one POCBMobile target.**
  - `Backup` (Core.cs): every copy action (single target, Database model, Copy package, Kits from
    old TrueTest) first saves each file it overwrites to
    `D:\Program\RVS\Kit Helper Backups\<yyyyMMdd_HHmmss> <action>\`, laid out as `TrueTest\...`
    and `DATABASE\...` plus a `backup.txt` saying what each part maps back to. If a file's backup
    fails, it is not overwritten. No folder is created when nothing was overwritten. Backups sit
    beside the installed copy, not in the kit: a later POCB4.2/ScratchDustDetect_NY copy (which
    includes subfolders) would otherwise carry an old backup into TrueTest. This replaces 1.2.0's
    `<package>\Backup_*`. "Open backups folder" button added. Not covered: the Run test POCBMobile
    prep (copying model files into POCBMobile), which is not a copy action.
  - The `POCBMobile inputTemplate` and `POCBMobile inputTest` targets are merged into
    **POCBMobile** (`CopyTarget.Route`). In a flat kit, inputTest*.txt go to `inputTest`, everything
    else to `inputTemplate`; a kit named `inputTest…` goes entirely to `inputTest`; `inputTemplate\`
    / `inputTest\` subfolders map to themselves. A half the kit doesn't have is skipped (logged,
    folder not created). Migration logs a missing install folder as Info, not Warn.
  - The X402x package no longer ships Install.bat; its README points to Kit Helper only.
  - Tested in a sandbox (27 checks, Windows PowerShell 5.1 loading the built exe, `Backup.BaseDir`
    redirected): all POCBMobile kit shapes, backups per action, no-overwrite case, and backup
    failure blocking the overwrite. The UI itself was not clicked through.

- **Kit Helper 1.2.0: combined packages** (`Core.cs` `Package`, `MainWindow` `Package_Click`).
  A folder with `TrueTest\` and/or `DATABASE\` inside is a package. "Copy package" copies
  `TrueTest\<sub>\...` into the picked install and `DATABASE\...` into the database picked in
  Run test → POCBMobile, each file at its own relative path. Every file it overwrites is backed up
  to `<package>\Backup_<yyyyMMdd_HHmmss>\`. If the backup of a file fails, that file is not
  overwritten (counted as failed). Files at the package root (README, Install.bat) are ignored.
  First package: `D:\Log\X402x_NewFlowSetup_2nd\ReplacePackage_X402x_20261120` (36 files).
  - New targets: `POCBMobile inputTest`, and `Database model` (→ `<db>\InputIMG\<folder name>`,
    hidden from the migrate window since it isn't an install folder).
  - Fixed: `Kit.Suggest` on a model folder (`InputIMG\X4023-CB-MP00_MATHON`, holding
    `LGD_PO_Compensation.dll`) suggested **POCB4.1Net**. That would have copied an X4023 PUC
    dll into TrueTest's POCB4.1Net/POCB4.1. A folder whose parent is `InputIMG` is now checked first.
  - Tested by loading the built exe in Windows PowerShell 5.1 against sandbox copies: Suggest for
    every package subfolder, Plan (36 files), Run (backup = exactly the overwritten files,
    unrelated files untouched, read-only target reported as 1 failure). The UI itself (button
    visibility, dialogs) was not clicked through.
  - The manual intro still says 1.1.0; it isn't in this repo.

- **New tool `VideoBrowser`** (WinForms net48 x64): folder → video-only playlist on the right,
  play, Del → Recycle Bin / Shift+Del → permanent. Player is LibVLCSharp 3.9.0 +
  VideoLAN.LibVLC.Windows 3.0.21 because this PC has neither `wmp.dll` (WMP legacy feature absent,
  so the AxHost/WPF MediaElement route is dead) nor VLC/mpv/ffmpeg; libvlc adds ~100 MB to `bin`
  (x86 copy disabled via `VlcWindowsX86Enabled=false`).
  Traps hit: (1) VLC `EndReached` → BeginInvoke runs inside a MessageBox's modal loop, so
  auto-next changed the selection under the delete confirmation — now deferred while `_inDialog`.
  (2) Shrinking a virtual ListView's `VirtualListSize` asks `RetrieveVirtualItem` for a stale
  index → ArgumentOutOfRange; guarded with a blank item. (3) `Process.MainWindowHandle` was 0
  right after start; enumerate windows when automating it.
  Tested by SendKeys + screenshots: .mkv with Vietnamese name, .mp4, .webm play; natural sort;
  .txt excluded; deleting the playing file releases it and recycles it. Not tested: real .ts
  files, recursive mode, fullscreen, huge folders.

## 2026-11-19

- **Kit Helper 1.1.0: exe icon** (`ForRadiant/QuickTools/KitHelper/KitHelper.ico`, `<ApplicationIcon>`):
  same gradient "K" circle as the floating icon (#7C5CFF→#22B8CF), so Explorer, taskbar,
  Alt+Tab and the right-click menu entry stop showing the blank default. Sizes 16–128 are
  stored as classic 32-bit DIBs and only 256 as PNG: System.Drawing.Icon (net48) throws on
  PNG frames below 256. WPF windows without an `Icon` take it from the exe. First version
  bump under the "bump on every change" rule; manual intro updated to 1.1.0.

- **CharlieTheSlave → net48** (side session, `E:\Dev\c#\CharlieTheSlave`, not in this repo):
  single 3.6 MB exe instead of 74 MB self-contained net8; backup `CharlieTheSlave.bak-2026-11-19`.
  Untested: UAC admin launch, TrueTest steps/POI, installer flow, clean factory PC.
- **Kit Helper: drop onto the K icon works when run as administrator** (`AdminDrop.cs`,
  `FloatingIconWindow.OnSourceInitialized`). Cause: UIPI blocks OLE drag from medium-integrity
  Explorer into an elevated window, so WPF's `Drop` never fired ("no entry" cursor).
  ChangeWindowMessageFilterEx cannot open OLE drag-drop; the only way through is the legacy
  path: `RevokeDragDrop` (with an OLE target registered OLE never falls back), `DragAcceptFiles`,
  and allow `WM_DROPFILES` + `WM_COPYDATA` + `WM_COPYGLOBALDATA (0x49)`. Re-applied at
  ApplicationIdle after Loaded in case WPF re-registers its target. Only when elevated
  (`IsInRole(Administrator)` is false for a filtered UAC token); normal windows keep OLE.
  Tested: elevated instance, real OLE drag from a medium-integrity process — folder and .txt
  both opened. UI Automation can't read an elevated window from medium, so checked by screenshot.
  Still version 1.0.0 (build 20261119131826), deployed to `D:\Log\Script` and `D:\Program\RVS`.
- **Kit Helper: new copy target `POCBMobile\inputTemplate`** (`CopyTarget.All` + `Kit.Suggest`
  in `Core.cs`). Kits now ship POCBMobile input templates in a folder like
  `…\20261105 D997 1st CPP + LGD Crop(Gooil)_TT959\inputTemplateGIBD992_D997` (10 ×
  `inputTemplateY25GIB*.txt`, YAML-ish `key: value`, no subfolders). TrueTest's folder is spelled
  `POCBMobile\inputTemplate` (lower-case t). Suggested when the folder holds `inputTemplate*.txt`
  or its name starts with `inputTemplate`; checked before the other content rules. Also appears in
  "Kits from old TrueTest…" (whole folder, recursive). Panel 920 → 960 px tall: the extra button
  pushed the log box below the window edge. Tested: Suggest on the real kit folder (parent folder
  suggests nothing), `Copier.Run` into a scratch install → 10/10 files, screenshot of the panel.
  Manual (Claude Doc) updated in both languages.
- **Kit Helper: Run test POCBMobile mode** (`RunTest.IsPocbMobileInput` / `PreparePocbMobile`,
  new `PocbTemplate` in `Core.cs`; `RbPocb` in `MainWindow`). Inputs starting with `debugMode:`
  are listed and pick the mode; Run writes the content to `<install>\POCBMobile\input.txt` and
  starts `POCBMobile.exe` there (no args). Templates are NOT runnable as shipped: placeholders
  `ExeDebugMode nMonth nDay DLLDefFolder mPUCDLLModel PID INIFile` (worked out by diffing
  `POCBMobile\inputTemplate\X.txt` against TrueTest's filled `POCBMobile\inputTest\X.txt`). Values
  come from the `.seqxc` Analysis step whose `<PUCTempplateArgFile>` names the template (sibling
  `<ExeDebugMode>` false→0, `<DLLDefFolder>`, `<PUCDLLModel>`, `<INIFile>`); in the D997 kit all 113
  steps share them. nMonth/nDay = today; PID = panel folder under `InputIMG\<model>` holding the
  first image, else TEMP (inputTest uses TEMP). No .seqxc → model unknown → Run refuses.
  POCBMobile.exe ends with "Press Enter to exit..." → stdin redirected and closed, or the run hangs.
  Tested: fill of `inputTemplateY25GIBFWMono2nd.txt` from the D997 kit, real POCBMobile.exe in a
  scratch copy of TT 1.8's folder → exits, code 1 "Select Comp. Mode Error" (no D997 images on this
  PC, TT 1.8 DLL); not run against a real install with images. UI screenshot: mode auto-picked.
- **Same day, revised at Chau's request:** DLLDefFolder / mPUCDLLModel / PID are now picked in the
  panel instead of guessed (POCBMobile-only rows `CboPocbDb`, `CboPocbModel`, `TxtPocbPid`):
  Database = C:\ / D:\ / E:\DATABASE or typed, remembered as `PocbDatabase`; Model = folders of
  `<db>\InputIMG`, those with "00" in the name (= PUC model) bold + accent, the kit `.seqxc`'s
  `<PUCDLLModel>` preselected if present; Panel ID = TEMP, editable. The `.seqxc` now only gives
  ExeDebugMode / INIFile. The auto PID (panel folder holding the first image) is gone. Tested:
  model pick via UI Automation (221 models in C:\DATABASE\InputIMG here), fill with
  D997-2CB-GIB00_TLSM / TEMP, real POCBMobile.exe in a scratch copy → still "1_Select Comp. Mode
  Error" with images present, so it's POCBMobile's DLL/data on this PC (TT 1.8), not the fill.
  Model list now re-reads `<db>\InputIMG` on every DropDownOpened (Chau: new folders didn't
  appear). Note: UI Automation's ExpandCollapse.Expand did NOT show the refreshed items in the test;
  a real mouse click did - verify such lists with a click + screenshot.
  Then: Run copies the files (not subfolders) of `<db>\InputIMG\<model>` into POCBMobile first -
  it loads LGD_PO_Compensation.dll / PANEL_SHAPE / LUT / csv from its own folder (Chau). An
  inputTest file uses its own `defaultFolder:` / `model:` lines. Effect measured in the scratch copy:
  "1_Select Comp. Mode Error" (TT 1.8's own DLL) → now "4_Initail Parameter Setting Error" with the
  D997 DLL, which on this PC has no `TEMP` image folder for D997-CB-CGDIM00_TLSM.

## 2026-11-18

- **New tool `ForRadiant/QuickTools/ScriptToolbox/`** — one WinForms (net48, SDK-style) UI
  replacing the 8 single-purpose tools in `D:\Log\Script`: the 3 "Copy all to X folder"
  `.bat`s, `Copy all to POCB4.1Net folder.exe`, `VignetingOn/Off.exe`, `UnblockFiles.exe`,
  `RunTestOmit.exe`. Logic rewritten in `Core.cs` (not launching the old exes), UI in
  `MainForm.cs` built in code. Deployed as `D:\Log\Script\ScriptToolbox.exe`; old files left
  in place.
  - **All old tools work on "the folder they sit in"**, so the toolbox has a *package folder*
    (default: its own folder, or a command-line arg / dropped folder). Its own exe is never
    copied.
  - **TrueTest installs are discovered** (`C:\Program Files\Radiant Vision Systems\TrueTest*`)
    instead of hardcoded. The old lists did not agree with each other or with this PC: they
    name `TrueTest 1.9 debug`, the installed one is `TrueTest 1.9 debug Mobile`. The install
    TrueTest is running from is preselected.
  - Behaviour kept from the old tools: POCB copies to `POCB4.1Net` AND `POCB4.1`, skipping
    `*.tif`, `*.csv`, `LGD_VNTT.ini`; only LGDVHCustomApp copies subfolders (xcopy /E, empty
    folders too); 3x retry on IOException; vignetting regex anchored at line start, DX ini
    skipped unless ticked.
  - Deliberate changes: "close TrueTest first" now applies to every copy target and asks
    before killing (the POCB exe killed silently; the .bats never closed it). Unblock is
    recursive. Vignetting writes only on change and keeps the file's encoding/BOM. The
    hidden "0 + HHmm password" per-file vignetting menu was dropped (Chau's call).
  - **Latent RunTestOmit bug fixed:** a folder argument with spaces AND a trailing `\` was
    quoted as `"C:\a b\"`, where `\"` escapes the closing quote. Trailing `\` is now doubled.
  - **WinForms trap:** an `AutoSize` GroupBox with a *docked* child collapses to zero width
    (titles rendered one letter per line). Children must be undocked with a set Location.
  - Not elevated by manifest (no UAC on every launch); a "Run as admin" button restarts it
    elevated, and access-denied copy failures point at it.
  - Verified: logic headless against a scratch package + fake `TrueTest 9.9` install;
    Include DX + Set ON clicked in the live exe (PostMessage BM_CLICK), files and log checked,
    no APPCRASH. **Not exercised live:** the copy buttons against real Program Files, the
    TrueTest close/restart prompts, and Run test with a real input file.
- **ScriptToolbox, round 2: replace the per-folder script copies entirely.** Chau keeps a
  copy of the relevant scripts INSIDE each package folder (`D:\Log\PUC\<model>`,
  `D:\Log\Kit\<model>\<kit>\InputIMG\<model>`, `...\Omit\<model>`, `...\R2R`). Surveying those
  turned up gaps:
  - **`RunTestPUC.exe` was never in `D:\Log\Script`** but lives in the PUC folders. It is NOT
    RunTestOmit: no working-dir prompt (runs in the folder it sits in), every folder arg gets
    `Test\` appended, and a bare `*puc*.hex/.txt` arg (not `dummy*`) is placed in that Test
    folder. Now a PUC mode in Run test, auto-picked when line 1's exe contains "PUC".
  - **Relative exe names relied on the launcher's own location.** The Omit input's line 1 is
    `ScratchDustDetect_NY_10.20.100.exe`, found only because RunTestOmit.exe sat beside it
    (CreateProcess searches the parent exe's folder). A central toolbox resolves relative
    exe/folder names against the input .txt's folder, then the working dir.
  - **The helper tools were being copied into TrueTest.** The old scripts excluded only
    themselves, so `UnblockFiles.exe`, `VignetingOn.exe`, `RunTestOmit.exe`... landed in
    Program Files on every copy. `Package.IsHelper` now skips them for every target.
  - Added: recent-folders drop-down, run inputs listed from the folder (any .txt whose line 1
    is an .exe), the matching copy target highlighted (a leftover "Copy all to X" script
    wins, else payload patterns), and a button that adds "Open in Script Toolbox" to the
    Explorer folder right-click menu (HKCU `Directory\shell` + `Directory\Background\shell`,
    no admin; it notices when it points at a different copy of the exe).
  - Layout traps: a root TableLayoutPanel with no ColumnStyle sizes to content, so a long
    path in the folder combo widened the whole window; and a DropDown ComboBox re-selects its
    old matching item when its handle is created, overwriting a Text set in the ctor (PUC
    working dir showed the TrueTest path) - clear Items first.
  - Verified on the real folders read-only: all 4 get the right target, PUC/Omit inputs and
    mode detected, Omit parse resolves the exe in the package folder. PUC parse checked on a
    copy with `D:\POCB\HEX\` redirected to scratch (Parse creates folders). **Not tested:**
    the Explorer menu button (writes HKCU; left for Chau to click), and a real run.
  - Chau then registered the right-click menu himself (HKCU command points at
    `D:\Log\Script\ScriptToolbox.exe`).
- **ScriptToolbox: single instance.** A second launch finds another process with the same
  exe name and a window, sends its folder by `WM_COPYDATA` (tagged `dwData` "STBO"), restores
  + foregrounds that window itself (the launched process holds foreground rights; the
  receiver could not take focus on its own), and exits. Receiver replies first and loads
  via `BeginInvoke` so the sender is not held inside SendMessage. No arg = just come forward.
  - **UIPI:** an elevated window silently drops WM_COPYDATA from a non-elevated sender, which
    is exactly the Explorer→"Run as admin" window case; the form calls
    `ChangeWindowMessageFilterEx(WM_COPYDATA, MSGFLT_ALLOW)`. Not tested (needs a UAC click).
  - **"Run as admin" passes `/new`**: the old window is still closing when the elevated
    process starts, so without it the new one forwards to the dying window and exits.
  - Verified: minimized first window, second launch exits in ~110 ms, first restored with the
    new folder and "Opened …" logged, 1 process; no-arg relaunch leaves 1; `/new` gives 2.
- **ScriptToolbox 1.1.0: one installed copy at `D:\Program\RVS\ScriptToolbox.exe`.**
  Engineers keep copies everywhere, so the newest build wins (`InstalledCopy.cs`):
  - Every build stamps its time into the product version (`1.1.0+20261118111621`, via
    `InformationalVersion` in the csproj, `IncludeSourceRevisionInInformationalVersion=false`
    so no git hash is appended). Another copy's stamp is read with `FileVersionInfo`, no load.
    Order = `<Version>` then build time; a 1.0.0 exe has no stamp and sorts oldest. **Bump
    `<Version>` for real releases**: the build time alone only orders builds of one version.
  - Add AND Remove first copy this exe to the static folder unless the copy there is the same
    or newer; the right-click entry always runs the static copy. An entry pointing elsewhere
    (set up before 1.1.0) shows as "Move right-click entry to D:\Program\RVS".
  - Any copy started while the static one is newer re-launches the static one with the same
    args and exits. A copy clicking Add/Remove after the static one became newer starts it
    with `/new /menu:add|remove` and closes; the static copy performs it on `Shown`.
  - A newer copy started while an OLDER window is open sends a quit token (WM_COPYDATA) and
    takes over instead of forwarding to it; equal builds forward as before. If the old window
    has not exited in 5 s it is used anyway.
  - **Name clash:** the class was first called `Install`, which MainForm's `Install` property
    (selected TrueTest folder) shadowed - hence `InstalledCopy`.
  - `D:\Program\RVS` already existed with other tools' data folders (AutoBackupDataLog, EZAE,
    UploadQueue...); only `ScriptToolbox.exe` is added there.
  - Verified with two real builds 33 s apart (old in scratch, new in D:\Program\RVS): old
    launch → static window (twice, folder forwarded, 1 process); static launch closes an old
    `/new` window; Add from the newer copy upgraded the static exe and pointed the menu at it;
    Add from an older copy handed over and the static copy registered it. HKCU menu keys were
    exported first and re-imported afterwards, so Chau's entry still points at
    `D:\Log\Script` until he clicks "Move right-click entry".
- **ScriptToolbox 1.2.0: install on every launch, always run the installed copy** (Chau's
  simplification of the above). Any copy outside `D:\Program\RVS` copies itself there if it
  is newer (or none is installed), then starts the installed copy with the same args and
  exits (`InstalledCopy.RunInstalled`). The open window is therefore always the latest build
  and Add/Remove just register it; the `/menu:` hand-over and `InstalledIsNewer` were removed.
  - **An older installed window locks the exe**, so it is sent the quit token first
    (`SingleInstance.CloseOlder`) and the copy retries 10x200 ms while the handle is released.
    If install or start fails (no D:, no rights, still locked) the copy runs itself.
  - **No folder arg used to mean "the folder the exe sits in"** (the drop-exe-in-package-folder
    habit). Running from the static folder would turn that into `D:\Program\RVS`, so the copy
    passes its own folder along; the installed copy started bare opens the most recent folder.
  - Known gap: a non-elevated copy cannot read an ELEVATED window's exe path, so it cannot tell
    that window is older, does not close it, and the copy over its locked exe fails → the new
    copy runs from where it is. Rare (needs an old admin window open during an upgrade).
  - Verified (1.1.0 installed + open, 1.2.0 copy in a scratch "package" folder): double-click
    upgrades and replaces the open window and opens the copy's folder; old 1.1.0 copy with a
    folder → installed 1.2.0 opens it; bare installed launch → most recent folder; fresh
    install with nothing in D:\Program\RVS; same build relaunch does not rewrite the exe.
    Right-click entry not touched this round (still `D:\Log\Script`, which now redirects).
- **ScriptToolbox 1.2.1: "package" → "kit" everywhere** - the team's word for these folders.
  UI labels, log messages, comments, and the `Package` class (now `Kit`). Deployed by the
  normal path (new exe into `D:\Log\Script`, double-click), which upgraded `D:\Program\RVS`
  to 1.2.1 by itself. Chau had meanwhile moved his right-click entry to `D:\Program\RVS`.
- **Renamed ScriptToolbox → Kit Helper (1.3.0).** Folder `ForRadiant/QuickTools/KitHelper/`,
  `KitHelper.csproj`, namespace `KitHelper`, exe **`Kit Helper.exe`** (AssemblyName with a
  space, like "Seqxc Toolset"), installed as `D:\Program\RVS\Kit Helper.exe`, settings in
  `%APPDATA%\KitHelper\`, HKCU keys `Directory\[Background\]shell\KitHelper`.
  - **Migration, in the app** (every PC that ran Script Toolbox): `Settings.Load` falls back to
    `%APPDATA%\ScriptToolbox\settings.txt` (left in place); the installed Kit Helper, on
    `Shown`, deletes the `ScriptToolbox` right-click keys and registers its own if none exist,
    and deletes `D:\Program\RVS\ScriptToolbox.exe` (retried next start if locked).
  - **Old copies can't be fixed from here:** a ScriptToolbox.exe 1.1–1.2.1 still on someone's
    PC installs itself as `ScriptToolbox.exe` again when double-clicked, and its process name
    differs so the two do not see each other. Kit Helper cleans that up on its next start;
    the real fix is replacing old copies. `Kit.IsHelper` skips both exe names when copying.
  - Done on this PC via the normal path (D:\Log\Script\ScriptToolbox.exe renamed to
    Kit Helper.exe and replaced, double-clicked on R2R): log showed both migration lines.
    Backups of the old keys and installed exe are in the session scratchpad only.
- **Kit Helper 1.4.0: WPF rewrite with a floating icon**, styled after
  `E:\Dev\c#\CharlieTheSlave` (its `FloatingIconWindow`, `#1E1F26` card, `#7C5CFF→#22B8CF`).
  Stays **net48** (Charlie is net8) so line PCs need nothing installed. `Core.cs` and
  `InstalledCopy.cs` unchanged; `MainForm.cs` replaced by `MainWindow.xaml`, plus
  `FloatingIconWindow.xaml`, `Theme.xaml` (dark templates for button/combo/check/radio/
  ListView/scrollbar), `Settings.cs` (now shared, `Settings.Current`).
  - Icon: click toggles the panel, double-click installs + adds "Open in Kit Helper" (or says
    it is already there, in a note above the icon), drag moves (saved as IconLeft/IconTop),
    folder drop opens it, right-click menu has show/hide, add/remove, open folder, Exit.
    **Single click waits `DoubleClickTime`** before toggling, else a double-click would
    flash the panel open and shut first. Panel ✕/Esc only hides; Exit is on the icon menu.
  - Started for a folder → panel shown; started bare → icon only.
  - **IPC moved to a message-only window** (`HwndSource`, parent `HWND_MESSAGE`, name
    `KitHelper.Ipc`), found with `FindWindowEx(HWND_MESSAGE, …)`: it exists whether or not
    the panel is shown. The sender calls `AllowSetForegroundWindow(pid)` so the panel can come
    forward. Legacy 1.3.0 WinForms windows are still found via `MainWindowHandle` and get the
    quit token (verified: a 1.3.0 window is replaced by 1.4.0).
  - **Startup race found in testing:** two launches ~3 s apart both ran (the first had not
    created its IPC window yet). Fixed with mutex `Local\KitHelper.Running`, taken before the
    slow part of start-up; a launch that sees it but no IPC window polls up to 10 s.
    Verified with launches 300 ms apart → 1 process, second kit shown. Real start ≈ 0.4 s.
  - Trap: PowerShell passes `$null` to a `string` P/Invoke parameter as `""`, so a
    `FindWindowEx(..., $null, name)` probe from PS never matches - use a C# helper.
  - Layered windows (AllowsTransparency) come out blank/odd with PrintWindow; screenshots
    were taken with `CopyFromScreen` over the window bounds instead.
  - Verified with real mouse input (SetCursorPos + mouse_event): click hide/show/hide,
    double-click shows the note and does not toggle; upgrade over a running 1.4.0 (quit
    token through the new IPC window); bare relaunch shows a hidden panel. **Not tested:**
    Exit from the icon menu, folder drop on the icon, copy/run actions in the WPF panel
    (logic unchanged from 1.3.0), an elevated instance.
- **Kit Helper 1.4.1: instant icon click + click-away hides the panel.**
  - The "delay" Chau noticed was not work: a single click waited `DoubleClickTime` (500 ms)
    to rule out a double-click. Now a click toggles at once (measured 12-15 ms); a
    double-click's first click therefore toggles too, its second only adds the right-click
    entry. Accepted trade-off: double-click is a one-time setup action.
  - Panel hides on `Deactivated`, except (a) when the cursor is over the floating icon -
    clicking the icon deactivates the panel first, and hiding there would make the icon's
    own toggle re-open it - checked with `GetCursorPos` + `PointFromScreen`; (b) while one
    of its dialogs is open: every MessageBox / FolderBrowser / OpenFileDialog goes through
    `Modal()`, which suspends auto-hide (opening a dialog deactivates the owner).
  - Verified with real input: icon click hide/show, click on another window hides at once,
    Browse dialog opened (screenshot) with the panel kept visible and still visible after Esc.
    Test trap: a new window opening also steals focus, so it legitimately hides the panel -
    open the click target BEFORE showing the panel. Each PowerShell tool call is a fresh
    session, so Add-Type helpers must be re-loaded (dot-source a helper .ps1).
- **Kit Helper 1.4.2: right-click entry added on every start, unless removed.** Chau's
  original "double-click → add Open with Kit Helper" meant double-clicking the EXE; I had
  read it as the floating icon, so starting the app never added it. Now
  `EnsureRightClickEntry` runs at start-up: adds the entry (or re-points it at
  `D:\Program\RVS`) and shows the note on the icon. Remove sets `MenuRemoved=1` in settings
  and the start-up check then leaves it off; Add (button, icon menu, icon double-click)
  clears it.
  - **Settings bug found on the way, present since 1.4.0:** in `Settings.cs`, the static
    `Current = Load()` was declared above `static readonly FilePath`. Static initialisers
    run in text order, so Load() ran with FilePath null, `File.Exists(null)` was false, and
    it fell back to the OLD `%APPDATA%\ScriptToolbox\settings.txt` on every start - while
    Save() wrote the new file. Effect: nothing saved since the WPF rewrite (recent folders,
    TrueTest choice, MenuRemoved) survived a restart. Found because the Remove flag was
    written and verified in the file yet ignored; confirmed by reading `Settings.Current`
    through reflection. Fix: FilePath moved above Current, with a comment saying why.
  - Verified with real button clicks via UI Automation (powershell.exe 5.1,
    InvokePattern): fresh start adds; Remove → restart stays removed; Add → restart stays.
- **Kit Helper 1.4.3: Remove is no longer remembered** (Chau: he wanted "every start", not
  "unless removed" - my question offered both and the answer combined them). Remove only
  deletes the entry; the next start re-adds it. To be rid of it: remove it and stop starting
  the app. `MenuRemoved` dropped from code and from his settings file. Verified: Remove →
  entry gone → restart → entry back.
  - Start-up only writes when needed: it checks the `Directory\shell` command; if that exists
    and points at the installed exe it does nothing. It does NOT check the
    `Directory\Background` half, so a hand-deleted Background key is not repaired.
- **Kit Helper 1.4.4: clearer unblock result.** Chau read "Done: 0 unblocked of 10" on the
  PUC kit as a failure; the files simply had no `Zone.Identifier` stream (checked with
  `Get-Item -Stream *`: no alternate streams at all). Unblock itself was verified on marked
  scratch files (2 of 3, subfolder included). Now: "Nothing to unblock: none of the N file(s)
  is marked…", "Done: unblocked X of N file(s); the rest were not marked.", or the failure
  count in red. Both wordings checked by clicking Unblock through UI Automation.
- **Kit Helper 1.5.0: new copy target POCB4.2** (second button). Kit example:
  `D:\Log\Kit\X4023-P2-KS151C- test kit V1.0.1(...)\20261104_X4023  MATLAB + LGD Crop_TT163\POCB4.2`
  - Copies **with subfolders**: the kit has `X4023\` (Crop_params.json, PANEL_SHAPE*.tif) and
    `mainTemplate\` (main_*.txt), the same layout as the installed `TrueTest *\POCB4.2`
    (which every install here has). No .tif/.csv exclusions, unlike POCB4.1Net: the .tif files
    are needed under X4023\.
  - Suggested when the folder is NAMED `POCB4.2`, or holds `Crop_params.json` / a
    `mainTemplate` subfolder (or an old "Copy all to POCB4.2 folder" script). The parent kit
    folder (holds OMIT/POCB4.2/PUC/R2R) correctly gets no suggestion.
  - **Subfolder copies used to include helper tools**: the kit has `X4023\UnblockFiles.exe`
    and `X4023\Thumbs.db`, and the never-copy filter only looked at top-level files. Now
    `Kit.NeverCopy` (helpers + Thumbs.db + desktop.ini) applies at every depth, for
    LGDVHCustomApp too. Panel file count uses the same rule.
  - Verified headless from the built exe: 28 of 31 kit files copied to a scratch fake install,
    byte-identical, subfolders recreated, the 3 excluded are exactly the two UnblockFiles.exe
    and Thumbs.db; suggestions for the four earlier kits unchanged.
  - Fifth button pushed the log off the card again: panel 860 → 920 px tall. Still fits a
    1080p work area; ShowNear shrinks it to the work area on smaller screens.
- **Kit Helper 1.5.1: ScratchDustDetect_NY copies subfolders too** (Chau: Omit kits sometimes
  have them). None of the ~35 Omit kits under `D:\Log` has one today, nor does TrueTest's own
  ScratchDustDetect_NY, so tested on a copy of a real Omit kit with an added nested folder:
  9/9 top-level files + nested files + empty folder copied, the subfolder's UnblockFiles.exe
  and Thumbs.db skipped. Only POCB4.1Net and PUC remain top-level-only.
- **Kit Helper 1.5.2: Run test working dir follows the TrueTest install.** Chau saw install
  = 1.8 but working dir = 1.9 debug Mobile. The two had independent rules: install = running
  TrueTest > last used > last in list; working dir = current text > saved `RunWorkDir` >
  install. Now in `LoadWorkDirs` (runs at start and on every install change) any working
  dir under `C:\Program Files\Radiant Vision Systems\` is replaced by
  `<selected install>\ScratchDustDetect_NY`; the custom `D:\Batch\ScratchNY_Custom\Exe Files`
  or a typed path is kept. Verified through UI Automation (ExpandCollapse + SelectionItem on
  the combos, ValuePattern to read): start matched despite a saved 1.9-debug value, follows
  1.9 debug Mobile → 1.9 → 1.8 debug Mobile → 1.8, custom folder survives an install change.
- **Kit Helper 1.5.3: two holes in that matching**, found when Chau opened an Omit kit and
  got install 1.8 + working dir = the custom batch folder.
  1. `LoadInstalls` refills the install combo: `Items.Clear()` fires SelectionChanged with
     NO install selected, `LoadWorkDirs` then fell back to `RunTest.CustomWorkDir`, and when
     1.8 was re-selected that custom folder counted as a deliberate choice and stuck. Every
     kit open calls LoadInstalls. Fix: `refillingInstalls` guard (match once at the end) and
     never substitute the custom folder for a TrueTest path when no install is known.
  2. PUC → Omit mode switch emptied the box and refilled it from the SAVED `RunWorkDir` of a
     previous run (my UIA test had left the custom folder there). Now leaving Omit stores the
     box in `omitWorkDir` and returning restores that, re-matched; saved value only at start.
  - Test trap: launching a new build while the old one runs leaves TWO processes for a few
    seconds (old one exiting); a UIA script picking "the first Kit Helper process" then drove
    the OLD build and reported the bug as unfixed. Wait for a single process of the new
    version before testing.
  - Verified: Omit kit opened 4x → 1.8 each time; PUC → Omit → 1.8; install → 1.9 debug
    Mobile follows and survives a reopen; picked custom survives a reopen.
- **Kit Helper 1.5.4: log fixes + run-input argument fix.**
  - **"Done" appeared above the last "copied" lines.** Worker-thread lines were posted to the
    UI thread (BeginInvoke) while UI-thread lines were added immediately, so the "Done"
    written after `await` overtook still-queued lines. Now every line goes through one
    `ConcurrentQueue` (timestamped at the call); a UI-thread call drains the queue first.
    Verified: 300-file unblock through the real button, log read back via its Copy button
    (the ListBox virtualises, UIA only sees visible rows) → Done is line 302 of 302.
  - **Log scrolled sideways during a PUC run** (Chau's screenshot). Auto-scroll used
    `ScrollIntoView(last)`; now `ScrollViewer.ScrollToBottom()`, which never touches the
    horizontal offset. **Not reproduced** on either build (short log, long scrolling log with
    a long last line held for 6 s via `waitfor`, clicking a long row) - so the change is
    believed, not proven, to be the fix. Ask for a screenshot if it recurs.
  - **A run-input argument containing < > | aborted the run** ("Illegal characters in
    path"): `Path.Combine` / `Path.GetFileName` throw for those on .NET Framework, used to
    test "is this argument a folder" and the PUC "bare file name" rule. Found because the
    first scroll test used `>nul`. Now `IsExistingDir` catches it and the bare-name test is
    "no \ or /". Both modes verified passing `>nul a|b <x` through unchanged.
  - Testing aid: `LogLine.ToString()` returns the text, so UI Automation reads real rows.
- **Kit Helper 1.6.0: "Kits from old TrueTest…"** (top-row button next to Rescan, and on the
  icon's menu). The engineers' TrueTest update routine: rename `TrueTest 1.8` →
  `1.8.5756.961`, install a new `TrueTest 1.8`, copy the deployed kit folders back. Popup
  (`MigrateWindow`): To / From combos (From excludes To), one checkbox per copy target
  (POCB4.1Net+POCB4.1, POCB4.2, PUC, ScratchDustDetect_NY, LGDVHCustomApp) ticked when the
  source has it and greyed when not, and "Skip EDT_Mobile.csv" under LGDVHCustomApp (ticked).
  - **Installs are found by `TrueTest.exe`, not by name** (`TrueTest.AllInstalls`): a renamed
    folder needn't start with "TrueTest". `RVSConverter 1.9` has no TrueTest.exe → excluded.
    The panel's own install list still uses `TrueTest*`, so renamed old installs do not
    appear as copy targets there.
  - **Default From = most recently renamed.** A rename does not touch a folder's created /
    modified dates, but it does update NTFS's ChangeTime, read with
    `GetFileInformationByHandleEx(FileBasicInfo)` (CreateFile needs
    FILE_FLAG_BACKUP_SEMANTICS for a directory). Order: renamed-looking first (name not
    `^TrueTest \d+\.\d+( .*)?$`), then ChangeTime. On a PC with no renamed install it falls
    back to the most recently changed other install and says "Not a renamed install".
  - `KitMigration.Run`: whole folders incl. subfolders and empty folders, overwrite same-name
    files, keep files only in the new install, skip Thumbs.db; EDT_Mobile.csv skipped only
    in LGDVHCustomApp. Helper exes are NOT filtered here (they may be deployed on purpose).
  - The close-TrueTest prompt / result line / restart offer were pulled out of DoCopy
    (`AskCloseTrueTest`, `LogCopyResult`, `OfferRestart`) and shared.
  - Verified: core on a fake RVS folder (renamed 1.8.5756.961 picked over an older renamed
    1.7.1.1 and a more recently changed but standard 1.9; EDT kept NEW when skipped, OLD when
    not; new-only file kept; subfolder + empty folder copied; Thumbs.db not). Dialog opened
    via UI Automation on the real installs: defaults and checkboxes as above, Cancel copies
    nothing. **Not run for real into Program Files** (needs a real rename + new install).
  - Test trap: PowerShell variables are case-insensitive - `$r` (result) silently replaced
    `$R` (root folder) in the test script.
- **Kit Helper 1.6.1: popup starts with nothing ticked**, plus Select all / Unselect all
  beside "Folders to copy" (Chau's call). Both act only on folders the old install has and
  leave "Skip EDT_Mobile.csv" alone; Copy stays disabled until something is ticked.
  Changing From keeps the ticks of folders the new source also has. Verified via UI
  Automation: open [-----x] Copy off → Select all [xxxxxx] on → Unselect all [-----x] off.
- **Kit Helper released as 1.0.0** (Chau is shipping it). Everything above 1.0.0 (1.1–1.6.1)
  was pre-release on this PC only. **Trap:** copies compare (Version, build time), so a
  1.0.0 build counts as OLDER than an installed pre-release and would never replace it; on
  this PC `D:\Program\RVS\Kit Helper.exe` (1.6.1) was overwritten by hand. Any other PC that
  got a 1.1–1.6.1 build needs the same once. Next release: 1.0.1 / 1.1.0 from here.
  - Pinning to the taskbar was asked for and NOT done: Windows 10 1809+ has no supported way
    for an app to pin itself (only the user, or an admin taskbar-layout policy). Offered
    instead: Startup-folder shortcut and/or Desktop + Start-menu shortcuts - not decided.
- **1.0.0 rebuilt (version kept) after Chau's review:**
  - Double-click on the K icon no longer adds the right-click entry (start-up already does,
    and the panel / icon menu have Add/Remove). The second click of a double-click is now
    ignored, so a double-click toggles once.
  - **Dropping onto the panel removed: it could never work.** Starting a drag in Explorer is
    a click outside the panel, which auto-hides it. The K icon (always on top, never hides)
    is the drop target: a folder opens it; a file opens its folder, and a .txt is also
    selected as the run-test input (`MainWindow.OpenFile`) - that .txt drop used to exist
    only on the panel.
  - Verified with a REAL OLE drag: a throwaway WinForms window calling `DoDragDrop` with a
    FileDrop on mouse-down, the mouse driven by SetCursorPos/mouse_event onto the icon.
    Folder → PUC kit opened; `second_run.txt` (2nd of 2 inputs) → its folder opened with
    that file selected. Drag result Copy|Link both times.
  - User manual (Claude doc "Kit Helper – Hướng dẫn sử dụng / User Manual",
    https://claude.ai/artifact/TWrCgMZX3tcRCYCP3XLciy), VI then EN, 8 sections each; updated
    for the two points above.
  - K icon's first-start position is now the **top centre** (4 px below the work-area top),
    not the bottom right; a dragged position is still saved (IconLeft/IconTop) and wins.
    Not top RIGHT: the always-on-top icon would cover maximized windows' close buttons.
    ShowNear already opens the panel BELOW the icon when there is no room above. Verified:
    icon 930,4 with the panel below it; dragged to 1470,304 → kept across a restart; saved
    keys removed again so this PC keeps the default.
  - **Panel now docks to the right edge of the screen** (Chau), not hanging off the icon:
    Left = work-area right - width; Top = just below the icon if the icon's centre is in the
    top half, else just above it; clamped to the work area. Work area = the icon's monitor
    (`Screen.FromHandle` + `TransformFromDevice` to DIPs), not `SystemParameters.WorkArea`,
    which is the primary monitor only. Verified: icon top centre → panel 980..1920 × 64..984;
    icon bottom left → 980..1920 × 10..930.
  - **Mistake made and fixed:** a test assumed the icon was at the default, but Chau had
    dragged it to 264,565, and the test's "reset to default" step deleted his saved
    position. Restored (IconLeft=264, IconTop=565). Read the icon's real position before
    driving it, and never reset a user's saved settings as test cleanup.
  - **Icon menu: "Default position"** moves the icon back to the first-start spot (top centre,
    `MoveToDefault`) and removes IconLeft/IconTop (`Settings.Remove`); an open panel re-docks.
    Verified via the real context menu (right-click + UIA Invoke): 911,3 → 930,4, keys gone.
  - **Same mistake again, caught by the test's own "before" line:** Chau had meanwhile moved
    the icon to 911,3, and the cleanup restored the OLD remembered 264,565. Fixed to 911,3.
    Restore the value the test itself read just before it changed anything - never one
    remembered from earlier in the session.

## 2026-11-11

- **New tool `ArpgNameRandomizer/`** (renamed `NameRandomizer/` later the same day) (WinForms, net472, SDK-style csproj, built with `dotnet build`;
  output in `ArpgNameRandomizer/bin/Debug/net472`). Game picker (PoE1, PoE2, Diablo IV, Last Epoch)
  → class → flavour themes in `NameData.cs`; generator in `NameGenerator.cs`. Click to copy, favourites.
  - Name quality comes from the filters in `NameGenerator.Pronounceable`, not the word lists: without
    them about 1 in 10 made-up names was junk (`Brugrigorn`, `Uraraji`, `Sererira`, `Veelle`). Rejecting
    a repeated letter pair, doubled vowels and 3+ consonant runs (th/sh/ch count as one) fixed most of it.
    Compounds reject joins that double a sound (`Steel`+`lash`, `Ash`+`shroud`).
  - No underscores in any name, even though PoE allows them: Chau doesn't want `Thuga_Ashfang`-style
    names, so name + title is always joined CamelCase (`ThugaAshfang`). PoE allows 3-23 characters. The D4 (12) and Last Epoch (20) limits are
    from memory and **not checked in game**.
  - `favorites.txt` / `settings.txt` are written next to the exe, so they land in `bin/`; the tool's
    own `.gitignore` excludes just those two files, not the build output.
  - **No repeats (v1.1.0)**, like the online name generators. The generator no longer rolls dice:
    `NamePool` lists every name the settings can make and hands out unseen ones shuffled, so the
    "N not shown yet" count is exact. When a pool runs out, the cycle starts over.
    Name + title is too big to list, so it is counted from per-first-name title lists and sampled.
    History is **in memory only**. A `seen.txt` that remembered names across restarts was built and
    then dropped at Chau's request; bigger pools do the job instead. Compound word lists were roughly
    doubled. At max 14, PoE2 Witch has 1,848 made-up, 639 compound and 190k title names; PoE
    "Any class" has about 1.44M. Building a pool takes about 150 ms for "Any class"; it is cached per settings.
  - **v1.2.0: one name at a time**, big and centred (Chau's request), with no list, count or
    "N left" label. The font shrinks for long names so 23-letter PoE names stay on one line.
    Trap: `ActiveControl = button` inside the constructor throws "Invisible or disabled control
    cannot be activated" and the app dies silently at startup; focus it in `Shown` instead.
  - **v1.3.0: Category picker.** "ARPG character" (the per-game picker was folded into one class list,
    e.g. "Witch / Necromancer / Acolyte", with max length covering game limits) and "Group chat
    nickname" (`ChatNames.cs`: vibes x patterns like `Sleepy Potato`, `Queen of Naps`, `Certified
    Overthinker`, about 7.6k for "Any vibe"). Both pools implement `INamePool`. Window title is now
    "Name Randomizer"; the folder and exe keep the old name so the desktop shortcut still works.
    Chat traps: "___ of X" only after real ranks (no "Mr. of Memes"); no "of the Night" (reads as
    "Lady of the Night"); no Bro/Sis suffixes ("Taco Sis").
  - **v1.4.0:** chat nicknames are joined without spaces, each word capitalised (`QueenOfNaps`,
    `PartTimeNapper`; "Mr." loses its dot). One shared max length (3-30) for both categories, as
    Chau asked; at 14, "Any vibe" still has about 6.3k chat names.
  - **v1.5.0: much bigger pools.** At max length 14: ARPG "Any class" went from 1.6M to 17.8M
    (made-up 12k to 80k, compound 5k to 13k); group chat "Any vibe" went from 6.3k to 73k (about 500
    to about 4k per vibe). Done by roughly doubling every syllable and word list, adding chat titles
    (Lil, Mega, Baroness...) and a noun + noun pattern (`TacoGoblin`), which supplies most of the
    chat growth.
    - Vowel join rule changed: when two different vowels meet and the pair is not a smooth one
      (ae ai au ea ei ia ie io ua), drop the LEFT vowel (`Fidmi`+`ard` -> `Fidmard`). The old rule
      dropped the right one, which ate into endings (`Jacdint`, `Fidmird`).
    - Speed: counting name + title combinations by walking every title took 1.7-3.4 s for
      "Any class" after the growth. Now each title list keeps counts by length, and everything
      that does not depend on max length is cached per class / gender / prefix: first build is
      about 0.3 s, then a max-length step takes about 15 ms. Generate picks random unseen names
      instead of shuffling whole lists.
  - **v1.6.0: chat lists x3 and an "Add a number" option.** About 90 adjectives and 90 nouns per vibe
    (was ~35): "Any vibe" went from 73k to 446k at max 14, a single vibe from ~4k to 17-24k. The checkbox
    appends two digits (00-99) for both categories (`NumberedPool` wraps the real pool, built 2 shorter):
    chat Any vibe ~30M, ARPG Any class ~178M at max 14. Noun + noun is now about half of all chat names
    (`LasagnaCheese`, `EarmuffFuton`), which is the most random-sounding pattern; if quality complaints
    come, restrict that pattern before shrinking lists. `ChatPool` is cached per vibe / gender / prefix
    like `NamePool` (first "Any vibe" build ~0.1 s). Numbered names are picked at random and checked
    against `seen`; 20k numbered names took 41 ms with no repeats.
  - **v1.7.0: 8 more vibes** (Spooky, Space, Music, Sports, Nature, Fantasy, Office / school, Tech),
    about 70 adjectives and 70 nouns each. "Any vibe" went from 446k to 1.52M at max 14 (101M with a
    number). Still about 10x below ARPG: chat names must be real words (~1,000 adjectives, ~1,000 nouns),
    while ARPG made-up first names come from syllables (~80k) and are multiplied by ~13k titles.
    - `ChatPool` no longer lists names: millions of strings would be hundreds of MB. Each pattern keeps
      its word lists plus a count of Y words by length, so the size is counted and names are built when
      picked (cached word lists ~0.7 MB; max-length step ~0.1 ms). With a "starts with" prefix the
      pool is small and is listed outright. Total slightly overcounts names that two patterns can both
      make (Food at max 8: 1,138 counted, 1,128 real); harmless, `seen` still prevents repeats.
    - Noun + noun is half the pool but sounds the most random (`PetalQuark`), so it is picked with
      bias 0.35: every name is still reachable, it just comes up less often.
  - **Renamed to `NameRandomizer/`** (v1.7.1): folder, `NameRandomizer.csproj`, exe, namespace and
    AssemblyTitle, since it is no longer ARPG-only. Entries above that say `ArpgNameRandomizer/` mean
    this folder. Old `bin`/`obj` output was deleted and rebuilt so no `ArpgNameRandomizer.*` files are
    left. The desktop shortcut was replaced by "Name Randomizer.lnk" pointing at the new exe.
  - **v1.8.0: "Starts with" became "Add in front"**, a tag glued on as typed, any characters (`FR_`,
    `_`), as Chau asked. Tag, name and number together stay within max length (the name gets
    max - tag - 2). The pools' letter-prefix filter code is still there, called with "" now.
    Also fixed: after switching category the Style box could show above its label (hiding the label
    and box one by one in the options flow); they now sit in one panel that is hidden as a whole.
    Name + title first names must be 3+ letters (`PiSkysoul` slipped through).
  - The PC's PowerShell execution policy blocks `.ps1` files; use `powershell -ExecutionPolicy Bypass -File`.

## 2026-11-01

- **Windows reinstalled on the main PC.** Claude's own memory (`C:\Users\Chau\.claude`) was
  wiped; this file and `CLAUDE.md` survived because they live in the repo on E:. Nothing tracked
  was uncommitted (checked by hashing the working tree against `.git/index` before git was
  reinstalled), and one commit from the other PC (`e443fc5`) was pulled.
  - **After a reinstall git refuses every repo** ("dubious ownership"): the files are owned by
    the OLD account's SID. `E:\Github\add_git_safe_dirs.ps1` fixes all of them in one go; it
    calls bare `git`, so run it from a terminal opened after Git was installed.
  - `core.autocrlf=true` (Git installer default) is required here: the index holds LF, the
    working tree CRLF. Without it ~3000 text files show as modified.
- **Committed three groups that had never been added to git**, each one broken on a fresh clone:
  - `ForRadiant/SeqxcToolset/DataGridEditHelper.cs` (written 2026-10-01). Three tracked views
    call `DataGridEditHelper.Attach` and the README documents it, so the clone did not build.
    The committed Release exe (2026-10-02) already contained it — only the source was missing.
  - `ForRadiant/QuickTools/FtpUploadQeueModify/` — listed in the tracked `ForRadiant.sln` while
    its project files were absent. `.vs/` left out.
  - `RomHunter/` (2026-10-07) — WPF / net10.0 scaffold, demo data only, no real search or
    download yet. Committed with its build output per repo convention.

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
  - **Follow-up 7: checkbox captions were BLACK on another PC (i3-9100f).** The theme relies on
    `SetWindowTheme(..., "DarkMode_Explorer" / "DarkMode_CFD" / "DarkMode_ItemsView")`, which are
    **undocumented and only exist on recent Windows builds**. This dev box is Win11 26300 so it
    all worked here; on an older build the call is a no-op and any control left for the NATIVE
    side to draw reverts to system colours. Checkboxes were on `FlatStyle.System`, so the native
    control drew the caption — black on dark.
    Fixed by taking the drawing back: `FlatStyle.Flat` means WinForms paints glyph AND text, so
    `ForeColor` is honoured on every OS build. `FlatAppearance.CheckedBackColor` needs an OPAQUE
    `BackColor` (taken from the parent) to apply — that was the trap that originally pushed this
    to FlatStyle.System, and it is why the box is given the parent's colour rather than
    `Color.Transparent`.
  - **Rule:** anything still drawn natively is only dark where those undocumented themes exist.
    Verified as portable: the ListView header (owner-drawn), rows (owner-drawn), and now
    checkboxes. Still native and therefore build-dependent: **ComboBox drop-down arrows** and
    scrollbars. Test theme changes on an older machine, not just this one.

  - **Follow-up 6: the `WM_ERASEBKGND` handler had to come back out.** With the flash gone it
    showed its cost: the list appeared to "keep refreshing" whenever the mouse moved over it.
    The class comment already warns this control depends on native `LVS_EX_DOUBLEBUFFER` rather
    than managed double-buffering, and intercepting the erase bypasses exactly that, so every
    hot-track repaint became visible. Removed, with a comment so it is not re-added. The flash
    fix that actually worked is the `Theme.Init` reveal ordering, not this.
  - **Pattern worth remembering:** belt-and-braces fixes added while chasing something else are
    the first suspects when a NEW symptom appears right after. Two changes shipped together here
    and the redundant one was the one that broke.

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

- **operation.csv column naming (folded back into 1.0.0.10).** `run` held a STAMP there while `Run` in
  summary.csv held an ORDINAL - same word, two meanings, in two reports read side by side.
  Renamed to `run start` so both files mean the same thing by the same header.
  A `run` = `1 of 2` ordinal was added alongside it and then **reverted**: "N of M" is a
  fact about the panel's whole DAY, not the event, so a row written at 08:00 reads `1 of 1`
  until a 15:00 retest makes it `1 of 2` - the same physical event renders differently in a
  report rebuilt later. operation.csv is a timeline that gets grepped and diffed, so a cell
  that changes retroactively does not belong in it. The ordinal stays in summary.csv, the
  per-panel rollup, where a whole-day fact belongs.
  Shipped as **1.0.0.10**, not .11: the first .10 build never left `D:\Log`, so rather than
  leave two near-identical versions around, the rename was folded back and the .10 folder's
  exe swapped in place. `D:\Log\FtpUpload 1.0.0.11` is gone. Note git history still holds a
  commit stamped 1.0.0.11, so the version is not a unique key for a binary in this range —
  `run` vs `run start` in operation.csv is the reliable tell.
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
