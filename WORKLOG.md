# Work log

Newest first. One section per working day. Keep entries short: what changed, where, and
anything that would be expensive to rediscover. Code detail belongs in git; this is the
"why / what happened" that git messages don't carry.

---

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
