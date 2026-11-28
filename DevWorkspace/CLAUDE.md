# Dev root

`<root>` in this file means `toyprojects\DevWorkspace` (it was `E:\Dev` until 2026-11-20, when it
was moved into the toyprojects repo and its own `.git` dropped; older notes below still say
`E:\Dev`, read that as `<root>`). The folder kept its ACLs on the move: the Low label, the `publish\`
Medium exception and the `Everyone:(DENY)(DC)` entry. Build output (`bin`, `obj`, `publish`, `dist`),
`.venv` and `.env` are git-ignored here (`<root>\.gitignore`). It holds Charlie's own projects, sorted by
language (`c#`, `c++`, `java`, `python`, `vb.net`, ...). The projects are unrelated to each other.
Every other path below is written relative to `<root>`.

# Projects

| Shorthand | Folder | What it is |
|---|---|---|
| CharlieTheSlave, Charlie, desk assistant (was DeskAssistant until 2026-10-26) | `c#\CharlieTheSlave` | WPF (.NET 8) floating-icon assistant that automates TrueTest: open / close TrueTest, update it by clicking through the installer, and read or set analysis settings in the TrueTest UI through UI Automation. Unmatched input goes to a local Ollama model. |
| YdkeDeckBuilder, ydke tool | `web\YdkeDeckBuilder\index.html` (the page), `c#\YdkeDeckBuilder` (desktop app) | Paste a Yu-Gi-Oh! deck list, get a `ydke://` code and a `.ydk` file. Looks cards up on the YGOPRODeck API. The page also works on its own from disk. The desktop app is WPF + WebView2 showing that same page; build it with `install.ps1` (output in `publish\`). |
| YdkeDeckBuilderWpf, wpf ydke | `c#\YdkeDeckBuilderWpf` | Native WPF port of YdkeDeckBuilder (no WebView2, ~0.3 MB exe). Deck logic in `DeckCore.cs`, ported from the page's JavaScript; keep the two in step. Build with `build.ps1` (`-Share` adds a self-contained exe), output in `c#\YdkeDeckBuilder\publish\wpf\`. |
| phim-tool, tool phim | `python\phim-tool` | Tkinter app: episode links + downloader (with a queue tab) for phimvn2, tvhay/tvphim, motchill. Used from `D:\PortableApps\phim-tool`, which also holds `ffmpeg\ffmpeg.exe` (git-ignored, 110 MB) and the live `hang_doi.json`; see the deploy table in `toyprojects\CLAUDE.md`. |
| thuyet-minh-phim, thuyết minh phim, longtieng | `python\thuyet-minh-phim` | Tkinter app (package `longtieng`) that OCRs burned-in Vietnamese subtitles and reads them aloud, either live over a playing video or by exporting a dubbed copy of a film file or whole folders. Voices: Edge, Google, Microsoft An, 25 VieNeu. Its own `README.md` is the reference. Used from `D:\PortableApps\ThuyetMinhPhim` (portable build); see the deploy table in `toyprojects\CLAUDE.md`. |

## YdkeDeckBuilder notes

- **`E:\Dev` carries a Low Mandatory Level integrity label (`(OI)(CI)`), inherited by everything
  inside, so every exe built under `<root>` runs at LOW integrity.** Such a process can't write to
  `%LocalAppData%` or `%TEMP%`. It can't start WebView2 either: creating the controller fails with
  `E_UNEXPECTED`, then `ERROR_BUSY` (0x800700AA) on later runs. A single-file .NET app can't even
  extract its bundle ("Failed to create directory ... for extracting bundled files", exit 0x800080A0,
  silent for a GUI app). A low-integrity process can't raise itself, so the exe must sit in a
  folder without the Low label: see the `publish\` exception below. **Build with
  `c#\YdkeDeckBuilder\install.ps1`**, which rebuilds the small exe, the self-contained `share\` exe and
  the zip into `publish\`, re-applies the Medium label, and points the Start Menu and Desktop
  shortcuts at `publish\YdkeDeckBuilder.exe`. Re-run it after changing the app or `index.html`. (Until
  2026-10-28 it installed to `%LocalAppData%\Programs\YdkeDeckBuilder`; that copy was removed.) The
  same trap applies to any exe built here that writes outside `<root>` or uses WebView2/Chromium.
  Check with `icacls <file>` (look for `Mandatory Label\Low`). `E:\Dev` also has
  `Everyone:(DENY)(DC)` set. Both were set deliberately by the Admin account owner, so ask before
  changing either.
- **Exception, set at Charlie's request on 2026-10-28:** `c#\YdkeDeckBuilder\publish\` carries an
  explicit `Medium` label (`icacls ... /setintegritylevel (OI)(CI)medium`). New files there inherit
  Medium, so `dotnet publish -c Release -o publish` makes an exe that runs from that folder. **If the
  folder is deleted, the next publish recreates it as Low.** Re-run that `icacls` command on the folder
  and also on the exe, because the exe was already created with Low. Undo by running it with `low`.
- Shareable build (no .NET needed on the other PC): `dotnet publish -c Release -o publish\share
  -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true`. The result is one exe, ~69 MB
  (zipped as `publish\YdkeDeckBuilder-win-x64.zip`, ~64 MB). `IncludeNativeLibrariesForSelfExtract` is
  required here: without it, WPF's native DLLs (`wpfgfx_cor3`, `PresentationNative_cor3`,
  `vcruntime140_cor3`) end up loose next to the exe. They are extracted to `%TEMP%\.net` at run time,
  which is fine on a normal PC but fails at low integrity. Recipients still need the WebView2 Runtime
  (built into Windows 11) and internet access, and will see a SmartScreen warning because the exe is
  unsigned.
- **It is NOT the `#` in `c#\`.** That was the first diagnosis on 2026-10-28 and it was wrong. The
  "works without `#`" test had also moved the exe out of `E:\Dev`. With the two separated, a `#` path
  at normal integrity works, and a no-`#` path at low integrity fails.
- Single exe: `dotnet publish` makes one `YdkeDeckBuilder.exe` (~1.2 MB, framework-dependent).
  `index.html` is embedded and served from the virtual host `https://ydke.example/` through
  `WebResourceRequested`, not `file://`, which gives it a real origin for fetch, the clipboard and
  localStorage. There is still only one copy of the page (`web\YdkeDeckBuilder\`). `WebView2Loader.dll`
  is embedded as a plain resource. `App.PrepareWebView2Loader` writes it to
  `%LocalAppData%\YdkeDeckBuilder\loader` and calls `SetLoaderDllFolderPath`, so the .NET host never
  extracts anything.
- Icon: `c#\YdkeDeckBuilder\app.ico` (a card stack with a gold-framed "Y"), sizes 16-256. It was
  drawn with Pillow; the script isn't kept, so re-draw rather than hunt for it.
- Starting the exe through the shell (`Start-Process`, Explorer) can show an "Unknown Publisher"
  prompt on Charlie's screen. In tests, use `Process.Start` with `UseShellExecute = false`.
- Killing the app (e.g. with `timeout`) leaves its `msedgewebview2.exe` processes running, and they
  keep the profile in `%LocalAppData%\YdkeDeckBuilder` locked. Close it with `CloseMainWindow` in tests.

## CharlieTheSlave notes

- The commands are defined in `commands.json`, which sits next to the exe and reloads automatically
  when saved. It is seeded from the embedded `commands.default.json`, so edits to the default
  only reach a machine after it runs "reset commands" or deletes its `commands.json`.
- User settings (icon position, installer folder, TrueTest components, Ollama URL/model) are in
  `%AppData%\CharlieTheSlave\settings.json`.
- TrueTest's internal control names are in the `trueTest` block of `commands.json`. If an
  automation step stops finding a control after a TrueTest update, check there first.
- `open TrueTest` runs a hard-coded path, `C:\Program Files\Radiant Vision Systems\TrueTest 1.9\TrueTest.exe`.
  A different TrueTest version needs that entry in `commands.json` changed.

## CharlieTheSlave: how the TrueTest automation works (and its traps)

Two automation layers, both driven through UI Automation (UIA) plus Win32 messages:

**Installer (`InstallerAutomation.cs`, `InstallerAutomation.Components.cs`)**: "update TrueTest"
picks an installer from the installer folder, launches it and clicks the "forward" buttons (Next,
I Agree, Install, Finish) until every process it started has exited.
- Clicks are posted as `BM_CLICK`, not UIA `Invoke()`. `Invoke()` blocks until the click handler
  returns, which is forever if the click opens a message box, and it stalls every other UIA call
  into that process meanwhile.
- A page is only acted on once it looks the same on two polls in a row. Otherwise it could click
  "Finish" before the "reboot later" option it must select first has been drawn.
- It never answers "Restart Windows now?" questions for the user.
- Windows are found with `EnumWindows`, not the UIA tree. UIA nests owned dialogs (e.g. "Uninstall
  the previous version?") under their owner, so they never appear as top-level windows there.
  NSIS uninstallers re-launch themselves from `%TEMP%`, so their dialogs are also matched by title.
- NSIS "Choose Components" check boxes are custom tree-view state images that UIA can't read.
  Their state is read with `TVM_GETITEMSTATE` (1 unticked, 2 ticked, 3 partly ticked; 4-6 are the
  read-only versions) and toggled with Space, one item per poll. Group boxes are left alone, since
  they only reflect their children. Components to keep come from `TrueTestComponents` in
  settings.json (default: "RVS SQL CE DB Converter") or from "update TrueTest with A, B".
- When an installer needs admin rights, the app restarts itself elevated with `--install <path>`.
  The single-instance mutex waits 5 s in that case so the new copy can take over.

**TrueTest sequence and measurement database (`TrueTestAutomation.cs`, `TrueTestPoi.cs`, commands in
`PromptWindow.TrueTest.cs` and `PromptWindow.Poi.cs`)**: `TrueTestAutomation` only lists the sequence
steps and clicks Save Sequence now. The analysis-settings commands (`show step`, `set step`,
`compare step`, `export steps`, `calibration`, `check step`) were removed on 2026-10-26 as unwanted.
Nothing is committed in this repo, so that code is not in git either: it is written up, verbatim and
with its traps, in `c#\CharlieTheSlave\docs\truetest-analysis-and-calibration.md`. Start from that
document for any job touching the Analysis Setup window, a setting's value, or the
measurement-setup / calibration panels - do not work it out again.

`TrueTestPoi` reads points of interest out of the measurement database, and its traps are the
expensive part — see the 2026-10-26 entry in `WORKLOG.md` before touching it. The short version:

- `Define Points of Interest...` lives only on the Measurement List's row context menu.
- Pop-up menus die the moment their owner loses the foreground, so this is the one place that uses
  `SetForegroundWindow` plus real `mouse_event`/`keybd_event` rather than posted messages.
- UI Automation cannot read that menu (it shows only the seven entries inherited from the tool strip
  and stops at the first separator). It is read and invoked through **MSAA** (`oleacc`). Do not
  "fix" this back to UIA.
- A dying pop-up throws `COMException (E_FAIL)`, not `ElementNotAvailableException`.
- The POI window is owned by the main window, so UIA nests it under its owner and `EnumWindows` is
  the only way to find it — the same reason `InstallerAutomation` enumerates windows that way.
- A POI set must be chosen with the keyboard (F4, arrows, Enter). UIA's `ValuePattern.SetValue` only
  rewrites the combo's text: TrueTest never gets the selection-changed notification and keeps
  evaluating the old set, while the combo reads back as if it worked. `CB_GETCURSEL` is the honest
  check, and the points table is then polled until it settles.
- `AutomationBanner` warns on screen while a run owns the mouse; Ctrl+Alt+Q stops it
  (`GlobalHotkey.cs`).

- Exports go to `Documents\CharlieTheSlave\TrueTest\`: points of interest as `.xlsx` (ClosedXML).

# Work log

Keep a running log of each day's work in `<root>\WORKLOG.md`, newest day first, one section
per working day (`## YYYY-MM-DD`). Begin each item with the project name in bold, because the
projects here are unrelated.

Append to it without being asked, whenever a piece of work concludes: a feature added, a bug
diagnosed, a fix made, something ruled out. Record what changed and where (file paths, commit
hashes once there are commits) plus anything expensive to rediscover: why something was done
that way, what was tried and eliminated, traps for next time. Code detail belongs in the code
and git; this is the reasoning they don't carry. A few lines per item. If a day produced nothing
substantive, say so rather than leaving the day out.
