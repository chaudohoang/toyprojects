# Work log

## 2026-11-21

- **CharlieTheSlave: smaller floating icon that may sit on the taskbar** (same change as Quick
  Launcher / Kit Helper, see toyprojects\WORKLOG.md). 44 px window, 36 px circle, fits the
  Windows 11 taskbar; `KeepOnScreen` clamps to the monitor's full bounds; `TaskbarZ.cs` keeps it
  above the taskbar (1 s timer, only while it overlaps one). Published exe + zip refreshed.

- **CharlieTheSlave: floating icon can't be lost off-screen, plus a tray icon** (ported from
  toyprojects' Quick Launcher, where Chau dragged its icon off-screen and asked for a way back).
  `FloatingIconWindow.KeepOnScreen` clamps into the nearest monitor's work area after a drag, at
  start and on `DisplaySettingsChanged`; new `TrayIcon.cs` (WinForms NotifyIcon, icon drawn in code
  to match the ✦ orb): click opens/hides the prompt, menu Open prompt / Bring icon back here / Exit.
  A second launch now signals `Local\CharlieTheSlave.Show` instead of exiting silently: the running
  copy pulls its icon on screen and opens the prompt (both copies are elevated by the manifest, so
  the event is allowed across them). csproj gained `System.Windows.Forms` / `System.Drawing`
  references, not `UseWindowsForms`, to avoid name clashes with WPF.
  - Tested headlessly (reflection; `UnsafeLoadFrom` because the sandbox refuses `LoadFrom`): orb on the
    left monitor → "Bring icon back here" → bottom-right of the main screen; half off an edge → fully
    on a monitor; tray removed on exit. Not started for real (it needs the UAC prompt).
  - Published: `publish\CharlieTheSlave\CharlieTheSlave.exe` (hash = build) and the zip rebuilt with
    README.txt, which now explains the tray icon.

## 2026-11-19

- **YdkeDeckBuilderWpf: read a deck from a screenshot** (`CardRecognition.cs`, new; wired into
  `MainWindow` as "Open image…", Ctrl+V of an image, or dropping an image file). Card names on a
  Master Duel deck screenshot are ~3 px tall, so OCR is out, and there is no free reverse-image
  API for cards. Instead it finds the card grid and matches the middle of each card's art
  (x 21-79 %, y 25-61 % of the card: inside every frame's art box, clear of MD's rarity/limit
  badges) as a 12×12 RGB grid, zero-mean + unit-length, against one print per YGOPRODeck
  `cards_small` image (14,855 incl. alternate arts). Brute force + rescoring the top 40 with the
  crop nudged ±2 px takes ~0.4 s for 55 cards.
  - Grid finding: blob detection failed (MD's horizontal gaps between cards are ~2-9 px and leak).
    Works instead: rows = runs of image rows with pixels brighter than 95 (max channel), cards = runs
    of bright columns inside each row, runs too wide for one card split evenly. A row step >1.35×
    the smallest one starts a new section (Main → Extra → Side, MD's order).
  - On Charlie's Dark Magician screenshot: 55/55 cards found, 49 matched at 0.85-0.98 (runner-up
    ≤0.83), 6 failed at ≤0.58. The 6 are **Master Duel-only alternate artworks** not on
    YGOPRODeck (checked: no Dark Magician image there scores above 0.39). Cutoff `Sure = 0.75`.
    Unrecognized cards are written as `# ? row R, card C: …` comment lines, never as a guessed
    card: an early version put the guesses in, and a wrong Extra Deck guess ("The Fabled
    Kudabbi") got auto-moved and skewed both deck counts.
  - Data: `art-index.bin` (Brotli, ~6 MB) is embedded and also sits next to the exe in
    `publish\wpf\`, with the 14,855 images (392 MB) in `publish\wpf\art\`. Charlie asked for both
    to live next to the exe, not in %LocalAppData% (that is only the fallback when the exe's folder
    isn't writable). Every 30 days, or at most once a day when a card goes unrecognized, the app
    pulls the card list (21 MB JSON) and downloads only the images it lacks, ~12/s.
  - `build.ps1` now skips `art\` when relabelling files Medium (15k `icacls` calls otherwise). Its
    path has `..` in it, so the exclusion compares against the resolved path.
  - Verified in the published exe by pasting the screenshot with Ctrl+V (UI Automation): status,
    list and ydke code as above. The update path was tested by dropping 15 entries and letting
    `UpdateAsync` fetch them back (identical prints).
- **YdkeDeckBuilderWpf: MDPro artwork** (`MdProArt` in `CardRecognition.cs`, "MDPro folder…" button,
  `settings.json` next to the exe). Charlie plays **MDPro4** (fan Master Duel client, `F:\Games\MDPro4`),
  which draws anime/MD alternate arts and frameless full-art cards. That's what the 6 misses were.
  - Its art lives in `Picture\Art\<id>.jpg` (art only, 624² or 712×908 for pendulums) and
    `Picture\OverFrame\<id>.png` (82 whole-card full arts). Most `Art` files are byte-identical to
    YGOPRODeck's `cards_cropped`; the useful ones are MDPro's own alternate-art ids, e.g. Dark Magician
    46986414 → art 46986431, Pharaoh's Servant 88570003 → full art 88570004. `Data\locales\en-US\cards.cdb`
    maps those back through `datas.alias` (580 alias rows). Read with Windows' own `winsqlite3.dll` via
    P/Invoke, so there's no SQLite package.
  - **Trap:** WPF `BitmapDecoder` throws `FileFormatException "Unexpected property type or value"` on ~3/4
    of MDPro's JPEGs (a 6.7 KB EXIF block), with every `BitmapCreateOptions` tried. `BitmapImage` decodes
    them fine, so `Pixels.Decode` uses that everywhere, the screenshot loader included.
  - Art-only images are printed by placing them where the art box sits on a card (x 12-88 %, y from 18.2 %;
    pendulum x 6.7-93.3 %). Index of 15,017 prints, ~26 s to build, saved as `mdpro-index.bin` and rebuilt
    when the Art/OverFrame folder timestamps change. Matches from both indexes are pooled, best score wins.
  - Result on the same screenshot, in the published exe: 55/55, Main 40 / Extra 15, matching Charlie's
    corrections (2 Dark Magician, Dark Magician Girl, Pharaoh's Servant, 3rd Dark Magical Curtain,
    2nd Red-Eyes Dark Dragoon).
  - Follow-up, same day: **auto-detect** the folder from Unity's log,
    `%USERPROFILE%\AppData\LocalLow\YGO\MDPro4\Player.log` ("…at path F:/Games/MDPro4/MDPro4_Data/…";
    MDPro3 is checked too). **Pre-release cards** live in `Expansions\*.ypk`: zips holding `pics\<id>.jpg`
    (whole OCG card scans; `pics\field\` is only field backgrounds) and `test-release.cdb` /
    `test-update.cdb`. These are indexed as full cards, and the cdbs are extracted to %TEMP% for winsqlite.
    The rebuild stamp is the newest of the Art/OverFrame folder times and the .ypk file times (update
    1.0.8 rewrote Picture\Art on 11-09; the packs updated 11-15), so game updates trigger a re-index, also
    while the app is open. When YGOPRODeck doesn't know a name, or only fuzzy-matched it, the deck builder
    takes MDPro's card (id, type from the cdb bitmask, `MdProArt.ToCard`). Checked: all 174 cards in the
    current pre-release pack are already on YGOPRODeck, so that's a safety net. ToCard's Extra Deck call
    agrees with YGOPRODeck on all 14,325 shared cards (the 102 frame differences are tokens).
    Verified in the exe with settings.json deleted: it found F:\Games\MDPro4, re-indexed 15,227 images in
    27 s, read 55/55; the second read took 0.6 s.
  - **Watched an MDPro4 "Update Pre-Release Cards" (+ Rush) run** (FileSystemWatcher + before/after
    snapshot). It only replaces `Expansions\ygopro-super-pre.ypk` and `rush-pre-release.ypk`: it downloads
    into `TempFolder\<guid>\`, checks via `Expansions\.prerelease-check-*.cdb`, and swaps the file in
    (`~RF*.TMP`). `Picture\` was untouched. This run: +2 pre-release cards (Ghostrick Spunkie 100272049,
    Ghostrick Freeze 100272055) and 4 English renames in test-update.cdb (e.g. White Knight Lord → White
    Night Lord, which YGOPRODeck doesn't know).
  - **Trap:** YGOPRODeck has those Ghostricks too, but with its own placeholder ids (100460049/…055).
    Unreleased cards have no passcode, so every site invents one, and a ydke code with YGOPRODeck's id
    is a card MDPro doesn't have. With MDPro installed, `AddMdProCardsAsync` now swaps in MDPro's id
    whenever YGOPRODeck's id isn't in MDPro's databases (and still falls back to MDPro for unknown or
    fuzzy names). Text lists read only MDPro's card list (cached per `MdProArt.Stamp`, which now also
    includes cards.cdb), never the 30 s picture index. Checked in the exe: a typed list built in 1.1 s
    with ids 100272049 100272055 14732294 46986414; then a screenshot of the new cards triggered one
    re-index (15,229 images) and read both Ghostricks.
  - Not handled: the Rush pack keeps its pictures under `Picture/`, not `pics/`. Rush cards don't go
    in ydke decks, so it's left out on purpose.
  - **Id rule (Charlie's):** only one source has the card → its id; both with the same id → that id;
    both with different ids → MDPro's. Written up in `AddMdProCardsAsync`.
  - **Bug found while testing it, in all three apps:** the YGOPRODeck lookup kept results in a map keyed by
    `Norm(name)`, and different cards collide there ("Hero Flash!!" / "H.E.R.O. Flash!", "Raimei" /
    "Rai-Mei"), so one silently got the other's id. Now the exact name (case-insensitive) is tried first,
    then the normalized one: `CardApi.LookupAsync` in `DeckCore.cs` and `lookup()` in
    `web\YdkeDeckBuilder\index.html` (kept in step). MDPro has the same collisions, so there the exact
    spelling wins, then a real passcode over a ≥100000000 placeholder. Tested in the WPF exe: Ancient Fusion
    (skill, YGOPRODeck only) 300302022, White Night Lord 14732294, Dark Magician 46986414, Ghostrick
    Spunkie 100272049, the four clashing names each their own id, `Maxx C` still 23434538. Web page
    checked in the browser pane for the same names. WebView2 app rebuilt with `install.ps1`.
  - `install.ps1` relabelled and listed every file under `publish\`, which now includes `wpf\art\`
    (15k files). It now skips `publish\wpf\`, which belongs to the WPF `build.ps1`.
- **YdkeDeckBuilderWpf: overlapped rows** (`CardFinder.Overlapped`, `FoundCard.Visible`). Charlie's 2nd
  screenshot (Blue-Eyes) read only 48/55: Master Duel squeezes a 15-card Extra Deck into one row where
  each card covers the right ~25 % of the one before, and the dark Xyz frames split the bright columns
  into odd runs. Now every card-height row gets its pitch from the autocorrelation of its vertical edges:
  a grid repeats at ~1.17 card widths (card + gap), the overlapped row at 0.76. Below 0.9 → overlapped:
  n = (span − card width) / pitch + 1, last card whole. Matching then uses only the print columns
  that can be seen (`ArtPrint.VisibleDims` / masked `Score`). Result 55/55; the hidden cards matched at
  0.93-0.97, and the Extra counts agree with MD's header (3 Fusion, 0 Synchro, 5 Xyz, 7 Link).
  - Traps hit on the way: (1) take the highest *local* peak; the raw maximum sits at the low end of
    the lag range, still falling from lag 0. (2) A 1 px pitch error × 14 gaps miscounts by one card, so
    the pitch is refined from the peak 4 pitches out, divided by 4. (3) Header text bands ("Main Deck 40",
    24 px) also have repeating edges and became ~90 tiny "cards" that swung the median card-height filter;
    now only bands within 15 % of the typical tall-band height count as card rows. (4) Thin bright runs
    (scroll bar) must not decide grid vs overlap. Both screenshots verified in the published exe: 55/55 each.
- **YdkeDeckBuilderWpf: video frames** (3rd screenshot: a YouTube frame, Blue-Eyes, 42 + 15). It read
  "71 of 82" with 6× Psychic Tuning. Causes and fixes:
  - A caption banner sits beside the last Main row (2 cards). Its letters repeat, so the overlap
    detector laid ~30 thin "cards" over the text, which matched junk. Now (a) an overlap must leave at
    least half a card visible (MD's was 0.76), and (b) every candidate must have a card's pale text box
    (`CardFinder.HasTextBox`, x 10-90 % / y 72-92 %: share of light, greyish pixels). Real cards
    measured 81-100 % on all three screenshots, banner fakes 0-13 %; cutoff 50 %.
  - A red progress line crosses the 2nd Extra row through the art → 5 cards at 0.52-0.70. Fix:
    `ArtPrint.TrimmedScore` rescoring drops the 2 worst-matching of the 12 print rows. It lifted those
    to 0.89-0.96, left clean cards about the same, and the shortlist grew 40 → 60 because an occluded card's
    right match can rank ~50th untrimmed.
  - Risk checked: trimming also lifts wrong runner-ups (up to 0.87). With MD-only artworks and the
    YGOPRODeck index alone (no right answer), the best wrong guess still scored only 0.56-0.64, with the
    top two within 0.055. Right matches led by ≥ 0.109. So a match now also needs `Lead` = 0.08 over the
    next card.
  - Result: 57/57, Main 42 / Extra 15, matching the header; no Psychic Tuning. Published exe: all three
    screenshots read in full (57, 55, 55).
  - 4th screenshot (another video frame, Blue-Eyes/Primite; the banner hides the "Extra Deck" header
    completely): 55/55 with no code changes. The section split came from the tall gap alone. Weakest match
    0.879, smallest lead 0.165, so nothing passed narrowly.
- **YdkeDeckBuilderWpf: banner touching a card row.** 5th screenshot (video frame, "Blue-Eyes Dark
  Magician", 42 + 15) read Main 40: the caption banner joined the short 5th row into one 112 px band
  (rows are 90), and bands more than 15 % off the row height were dropped whole. On the earlier frame the
  band was 101 px and scraped through. Now a band taller than a row (but under two) is searched as a row at
  its top and at its bottom, and the one with more text-boxed cards wins (`CardFinder.Find`, local `Row`).
  It recovered Majesty of the White Dragons and True Light. All six test screenshots: full counts (57, 55,
  55, 57, 55, 55), weakest match 0.843, smallest lead 0.108.
  - 7th screenshot (video frame, Blue-Eyes/Primite, 45 + 15, banner beside a full 5th row): 60/60 with no
    changes; weakest match 0.885, smallest lead 0.122.
- **YdkeDeckBuilderWpf: dark frames, whitish banners, tokens** (8th screenshot: Ancient Gear, 40 + 15, no
  rarity badges, a mouse cursor, dark Xyz frames, foil cards). It read 54 cards with 5 `# ?`.
  - The 10th Xyz card was lost twice over. Dark frames break the bright-column runs, and `Trim` shrank dark
    cards to ~80 % height, so the size filter dropped them. Now every row is also laid out by the
    autocorrelation pitch (`CardFinder.Pitched`, formerly `Overlapped`; a grid repeats at ~1.17 card widths).
    Slots carry on left and right while they hold a text box, and the layout with more cards wins over the
    bright-run one. The pitch is not re-derived from the bright span (a dark last card ends it early and the
    slots drift). `Trim` never cuts a card below 92 % of the usual *card* height (not band height: a
    banner-stretched band must still trim).
  - Another video (7th) had whitish banner lettering: slices scored 50-63 % light, against real cards'
    69-100 %. Added a brightness-spread test to `HasTextBox`: real text boxes ≤ 66, banner slices 96-116.
    Cutoff < 85.
  - Exceptional Schedule tied with Diagram Token (0.935 / 0.931, same artwork). Tokens can't be in a deck,
    so both indexes leave them out (YGOPRODeck `type == "Token"`, 106 cards / 120 images; MDPro type bit
    0x4000). Index format `YDKART2`, so old saved indexes fail to read and are rebuilt. Built-in index
    regenerated from `publish\wpf\art` (14,735 entries).
  - Left as `# ?` on purpose: 2 cards drawn with MD's shiny foil effect (0.86 with a 0.02 lead, and 0.73).
    Probably the 3rd Mulcharmy Purulia and the 2nd Ancient Gear Ballista, but a guess. The status message
    now names foil as a reason.
  - All eight screenshots in the published exe: 53/55 (the 2 foils flagged), then 60, 57, 55, 55, 57, 55,
    55 in full. Elsewhere unchanged: weakest 0.843, smallest lead 0.108.
- **YdkeDeckBuilderWpf: foil cards via a same-deck second pass** (`ScreenshotReader.DeckSure/DeckLead`).
  Charlie confirmed the 2 Ancient Gear foils are a 3rd Mulcharmy Purulia and a 2nd Ancient Gear Ballista
  (copies of neighbours). Cards the first pass can't place are now rematched against only the artworks of
  cards already identified on the same screenshot. Accepted at ≥ 0.65 with a ≥ 0.15 lead over the next deck
  card: the foils scored 0.80 / 0.73 (leads 0.2+). Risk test, YGOPRODeck index only: MD-only artworks on
  4 screenshots reached at most 0.57 against their deck (and those were often right: Curtain, Dragoon).
  Everything correctly stayed `# ?`, and the highest wrong in-deck runner-up for a sure card was 0.71
  (lead rule). Such cards go in the deck with a `# row R, card C: read as … from the other copies` note,
  and the status line counts them. All nine screenshots now read in full; published exe checked.
- **YdkeDeckBuilderWpf: foil cards with no other copy, colour-free 3rd pass** (`ArtIndex.MatchIgnoringColour`,
  `ScreenshotReader.FoilSure/FoilLead`). 9th screenshot (Blackwing): a single foil Blackwing Full Armor
  Master scored 0.578 in colour, ranked 143rd. Tried on three foils: brightness-only and per-channel
  (`ArtPrint.NormalizePerChannel`: each channel zero-mean / unit on its own, so a tint cancels) both
  ranked the right card 1st every time. Per-channel separates better: Blackwing 0.722 (lead 0.144), Ballista
  0.796 (0.261), Purulia 0.507 (0.054; the deck pass catches it). Six MD-only artworks with the
  YGOPRODeck index alone: at most 0.580, leads ≤ 0.063. Brightness-only gave one of them 0.764, so it was
  rejected. Accepted at ≥ 0.65 with ≥ 0.10 lead, only for cards both earlier passes left; a `# … by shape,
  ignoring colour` note marks them. Per-channel reference vectors are built on first use (~5 s, once per
  session). All ten screenshots read in full; the YGOPRODeck-only risk case still leaves all 6 as `# ?`.
- **YdkeDeckBuilderWpf: look-alike artworks** (10th screenshot: Red Dragon Archfiend, Main 40 + overlapped
  Extra 15 + overlapped Side 15 = 70). 69/70; per-section type counts match all three MD headers
  (32/6/2, 1/13/0/1, 8/1/6). The miss, Extra card 11, is Crimson Blade Dragon (Charlie). It ties with
  Crimson Blader (0.913 vs 0.936, the *wrong* one higher) because the two share nearly the same art and only
  the left 75 % shows. The Lead rule kept it out, which is right. Not fixable from the art; the `# ?` line
  now says "either A or B (their artworks are nearly the same)" when the top two both clear `Sure`. Card 13,
  Red Rising Dragon, came via the colour-free pass (0.977 colour but only 0.072 ahead of King Pyron).
- **YdkeDeckBuilderWpf: the same-deck pass put a wrong card in** (11th screenshot: HERO, 40 + 15 + 15).
  Side cards 7-8 are Spatial Trunade (Charlie), but came out as Terraforming via the deck pass. Cause:
  the first pass had Spatial Trunade 0.960/0.978, tied with its look-alike "Hey, Space Trunade!"
  (0.923/0.930), so unsure. Neither is in the deck, and the deck pass then took the deck's best, Terraforming
  at 0.728 (lead 0.18, cleared both bars). Fix: `DeckGap` = 0.10. The deck card must score within 0.10 of
  the card's best overall match. The real foils were 0.06 and 0.00 below theirs; this was 0.23. The
  colour-free pass already rejects the pair (per-channel lead 0.035/0.045 < 0.10). The cards now read
  "either Spatial Trunade or Hey, Space Trunade!". Spatial Trunade is not on YGOPRODeck (MDPro 2729965),
  so typing it in goes through the MDPro fallback. Type counts match all three headers. All other
  screenshots and the YGOPRODeck-only risk case are unchanged.
- **YdkeDeckBuilderWpf: Skill Cards out of the index too** (12th screenshot: Cyber Dragon, 40 + 15). Power
  Bond came out "either Power Bond or Power Bond (Skill Card)": YGOPRODeck lists 124 Speed Duel Skill Cards,
  some with a real card's name and art. Like tokens, they can't be in a deck, so they're left out
  (`ArtIndex.UpdateAsync` and the built-in index; 14,611 entries now). Format `YDKART3`, so saved indexes
  rebuild. 55/55; all earlier screenshots and the YGOPRODeck-only risk case unchanged.
- **YdkeDeckBuilderWpf: right-click Paste for screenshots.** Only Ctrl+V was intercepted (in PreviewKeyDown).
  The context menu's Paste asks the TextBox's `ApplicationCommands.Paste` CanExecute, which is false
  without text on the clipboard, so it was greyed out. Now `CommandManager.PreviewCanExecute` /
  `PreviewExecuted` on the input box handle Paste for every route (menu, Ctrl+V, Shift+Insert) when the
  clipboard has a picture, or a copied image file (Explorer Ctrl+C), and no text. Text pastes as before.
  Tested in the published exe via UI Automation: menu Paste enabled (Cut/Copy grey) and reads 55/55; a
  copied .webp file plus Ctrl+V reads 57/57; text plus Ctrl+V still inserts the text.

## 2026-10-28

- **YdkeDeckBuilder** (new, `web\YdkeDeckBuilder\index.html`): a single HTML page. Paste a deck list
  and it looks the cards up on the YGOPRODeck API, then outputs a `ydke://` code and a `.ydk` file.
  It runs straight from disk: the API sends `Access-Control-Allow-Origin: *`, so there is no
  server and no build step. It could not work as a claude.ai artifact, because artifact pages
  block fetches to outside hosts.
  - YDKE format: `ydke://` + base64(main) `!` base64(extra) `!` base64(side) `!`. Each section is
    the passcodes as **little-endian uint32**. Checked against the community spec's example:
    Blue-Eyes White Dragon 89631139 encodes as `o6lXBQ==`.
  - Lookup: one batched `?name=A|B|C` request for exact names. Names that aren't found are
    silently dropped, not reported as an error. Each miss then gets an `fname=` search on its
    longest word, and the closest name wins (Levenshtein, 0.85 or better). `fname` fails on
    punctuation (`fname=maxx c` finds nothing for Maxx "C"), which is why it searches one word.
    A request with no match returns HTTP 400, not an empty list.
  - A heading that mentions "alternative" goes to the Side Deck. Extra Deck monsters (fusion,
    synchro, xyz, link) listed under Main move to Extra. It checks deck sizes (40-60 / 15 / 15),
    the 3-copy limit, and the TCG or OCG banlist from `banlist_info`.
  - Tested in Node against Charlie's Blue-Eyes list: Main 60, Extra 15, Side 15, all cards found.
    The typo "Quetzacoatl" was corrected to Quetzalcoatl. TCG banlist warnings: Maxx "C" and
    Crossout Designator are Forbidden, and Droll & Lock Bird is Limited but the list has 2.
  - Charlie confirmed it works in the browser.
- **YdkeDeckBuilder: desktop app** (`c#\YdkeDeckBuilder`): WPF on .NET 8 with WebView2 1.0.2592.51
  showing the same `index.html`. Card links open in the default browser, and "Save .ydk file"
  opens a Save dialog instead of dropping the file into Downloads. Installed with `install.ps1`
  to `%LocalAppData%\Programs\YdkeDeckBuilder`, with Start Menu and Desktop shortcuts. Verified that
  the installed copy starts, its WebView2 processes come up, and it closes cleanly.
  - **Trap, cost ~30 min:** the build output crashed every time in `CreateCoreWebView2ControllerAsync`
    (E_UNEXPECTED, then 0x800700AA "resource in use"). Ruled out one at a time: the profile folder
    (a fresh one also failed), the runtime (155.0.4283.62 works), policies, environment variables,
    and the shell's session (headless Edge and a minimal WebView2 test app both worked from it).
    ~~The cause is the `#` in `E:\Dev\c#\...`~~ **Wrong, see below.**
- **YdkeDeckBuilder: single exe + icon, and the real cause of the crash.** `dotnet publish` now makes
  one `YdkeDeckBuilder.exe` (~1.2 MB, framework-dependent). `index.html` is embedded and served
  through `WebResourceRequested`. `WebView2Loader.dll` is embedded too and written out by the app
  itself (`SetLoaderDllFolderPath`). The icon is `app.ico`. `install.ps1` publishes the single exe to
  `%LocalAppData%\Programs\YdkeDeckBuilder` and adds shortcuts. Verified: the installed exe starts,
  UI Automation finds the page's "Build deck" button, and it closes cleanly.
  - **Real cause: `E:\Dev` has a Low Mandatory Level integrity label (inherited `(OI)(CI)`), so every
    exe stored under it runs at low integrity.** Found with `icacls` after the published exe failed
    in three ways: the .NET host couldn't create its `%TEMP%\.net` extraction folder (exit 0x800080A0,
    which I first misread as the "Unknown Publisher" prompt), then "Access ... is denied" creating a
    folder in `%LocalAppData%`, and WebView2's E_UNEXPECTED. Proven with the minimal WebView2 test
    app: a `#` path at normal integrity works; a no-`#` folder given `/setintegritylevel low` fails
    with the same E_UNEXPECTED. The earlier "works without `#`" test had also moved the exe out of
    `E:\Dev`, which is what fooled me.
  - Ruled out along the way: Sandboxie (`SbieSvc` is running, and `MarkOfTheWebBox=DefaultBox` forces
    files with Mark of the Web into a sandbox, but the exe has no `Zone.Identifier`), and Defender
    Controlled Folder Access (off).
  - Removed the "copy myself out of the `#` folder and relaunch" code built on the wrong theory. It
    couldn't have worked anyway: a low-integrity process can't write a normal-integrity copy. The app
    now detects the case (access denied writing the loader) and says to run `install.ps1` or copy
    the exe out of `E:\Dev`.
  - Left behind: `c#\YdkeDeckBuilder\publish\` holds a stale exe that can't run from there. My tools
    may not delete under `E:\Dev`, so Charlie needs to delete it.
- **YdkeDeckBuilder: exe runs from `publish\` now.** At Charlie's request, gave just
  `c#\YdkeDeckBuilder\publish\` an explicit Medium integrity label, and the exe too, because it had
  already been created with Low. The rest of `E:\Dev` stays Low. Checked: a new file in the folder
  inherits Medium, and the exe started from `publish\` loads the page. Charlie had deleted the folder
  first, so the first `icacls` found nothing and publish recreated it as Low. Label it after it exists.
  Left `publish\_labeltest.txt` behind for Charlie to delete.
- **YdkeDeckBuilder: self-contained build for sharing** in `publish\share\YdkeDeckBuilder.exe`
  (68.8 MB), zipped as `publish\YdkeDeckBuilder-win-x64.zip` (63.6 MB). Verified: the page loads
  from it. Its extraction folder in `%TEMP%\.net` holds WPF's native DLLs but no `coreclr.dll`,
  because self-contained single-file builds link the runtime into the exe itself. Not tested on a
  PC without .NET.
- **YdkeDeckBuilder: AppData install removed.** Deleted `%LocalAppData%\Programs\YdkeDeckBuilder` and
  pointed both shortcuts at `publish\YdkeDeckBuilder.exe`. `install.ps1` now builds everything into
  `publish\` (the small exe, `share\` self-contained, the zip), re-applies the Medium label (also on
  existing files, which keep the label they were created with), and updates the shortcuts. The
  profile folder `%LocalAppData%\YdkeDeckBuilder` (WebView2 data + `loader\`) stays: every copy uses it.
- **CharlieTheSlave: decided to keep the UI in WPF** (no WebView2 hybrid for now). Charlie finds the
  WPF look good enough. The automation must stay in C# regardless, and a hybrid would add a
  JS<->C# message bridge, ~100 MB of Edge processes and slower startup. Revisit only if the UI
  itself becomes the hard part.
- **YdkeDeckBuilderWpf** (new, `c#\YdkeDeckBuilderWpf`): a native WPF version of the deck builder,
  with no WebView2. `DeckCore.cs` is a port of the page's JavaScript (parser, YGOPRODeck lookup with
  fuzzy fallback, deck rules, ydke and .ydk output) with no WPF types, so a console app can test it.
  The UI matches the HTML page's layout and light palette, and reuses `YdkeDeckBuilder\app.ico`. The exe
  is 0.3 MB, framework-dependent, and goes to `YdkeDeckBuilder\publish\wpf\` via `build.ps1`. Putting it
  inside the folder that is already Medium avoids a second exception to `E:\Dev`'s Low label.
  - Verified: on Charlie's Blue-Eyes list the C# logic gives a **byte-identical** ydke code and .ydk
    to the HTML version, with the same four warnings. It also handles the spec's BEWD example, the
    `3x` / `x3` / `(3)` forms, and typo correction. End to end through the real window (UI
    Automation: paste, click Build, read the code box): identical code, plus a screenshot check.
  - Trap: **Windows PowerShell 5 refused to run `build.ps1`** ("not digitally signed"). PowerShell 7
    (`pwsh`) runs it. Use `pwsh -File` or `powershell -ExecutionPolicy Bypass -File`.
- **YdkeDeckBuilder: shortcuts switched to the WPF version** (Charlie's choice: the 0.3 MB native exe
  suits this app better than WebView2). `YdkeDeckBuilderWpf\build.ps1` now owns the Desktop and Start
  Menu "YDKE Deck Builder" shortcuts. `YdkeDeckBuilder\install.ps1` only touches them with
  `-Shortcuts`, so rebuilding the WebView2 version doesn't steal them back.
  - Trap for whoever edits these files with Python: a plain string turns `\b`, `\a`, `\1` into control
    characters. That garbled `install.ps1` and two lines of this log, both fixed. Use raw strings.
- **YdkeDeckBuilderWpf: dark theme.** It follows the Windows app mode (registry
  `...\Themes\Personalize\AppsUseLightTheme`) and switches live on `SystemEvents.UserPreferenceChanged`,
  like the page follows `prefers-color-scheme`. The palettes are in `Themes\Light.xaml` / `Themes\Dark.xaml`
  (the page's values; the only difference is dark text on the dark-mode accent for contrast).
  `Theme.cs` swaps merged dictionary 0, so every palette brush in XAML must be `DynamicResource`, and
  code-behind brushes come from `FindResource` and are re-rendered on `Theme.Changed`. The title bar
  uses `DWMWA_USE_IMMERSIVE_DARK_MODE` (20). WPF on .NET 8 has no dark control templates, so the
  banlist ComboBox became a segmented RadioButton picker, and CheckBox and ScrollBar got their own
  templates. Verified in dark mode (Charlie's setting): build end to end plus a screenshot. Light mode
  was not re-checked visually, to avoid flipping Charlie's Windows theme.

## 2026-10-27

- **CharlieTheSlave** — "TrueTest is still starting up" appearing mid-run on another PC, after the
  measurement list had scrolled and the POI window had opened. The main window was being re-resolved
  from scratch at every step — six times per measurement — through
  `RootElement.FindFirst(TreeScope.Children, ProcessId + AutomationId)`, and any one of those coming
  back empty while TrueTest was busy produced that message. Three changes:
  `TrueTestPoi` now resolves it **once per run** (`_main`, cleared in the `finally`);
  `TrueTestAutomation.MainWindow()` retries for about a second and falls back to the process's own
  `MainWindowHandle`, which also covers a build whose form has a different AutomationId;
  and `Step()` keeps its context on `TrueTestNotReadyException` instead of letting it through bare,
  so the message now names the step it failed at.
  The `MainWindowHandle` fallback skips a window whose title is the POI window's: while that is open
  Windows may well call it the process's main window, and returning it would be worse than failing.

  Then the actual cause, from a screenshot with the context menu still open on screen when the error
  appeared: **while a pop-up menu is open TrueTest's UI thread sits in the menu's modal loop and stops
  answering UI Automation**, so the lookup that runs right then — waiting for the POI window — got
  nothing and reported "still starting up". The process id and main window handle are now taken once
  at the start of a run and kept as plain Win32 values (`_mainPid`, `_mainHwnd`), so finding the POI
  window needs only `EnumWindows` + `GetWindowText`, which keep working through the menu loop. This is
  the same reason the menu itself is read through MSAA rather than UIA.
  **Rule for this file: never make a UI Automation call between opening a menu and it closing.**
  Seen on TrueTest 1.9.226.163; this machine runs .156. The control names and the menu are identical
  between the two, so the version itself is not the problem.

- **CharlieTheSlave** — `export poi` now asks which points to take before it starts: "all", a name
  like `P1`, or a number (1 = the set's first point). The answer arrives through a new one-shot
  `_answering` hook in `HandleAsync`, which takes the next thing typed instead of treating it as a
  command — a general "Charlie asked a question" slot, separate from the existing yes/no `_pending`.
  Every point is still *read* even when one is wanted: the full set is what distinguishes a stale POI
  window from a fresh one, and one point's numbers could repeat between measurements. The choice is
  applied to the output (`PoiPoints.Pick`), a measurement without that point is skipped with its
  actual point names listed, and the chosen point goes into the file name.

- **CharlieTheSlave** — The measurement row is now **left-clicked to select it before the
  right-click**. "Define Points of Interest…" acts on the measurement TrueTest has selected, and a
  right-click does not reliably move the selection — on the other PC the row was plainly unselected
  when the menu came up. Worth more than reliability: without it the POI window can open for whatever
  measurement was selected before, which exports the wrong numbers silently instead of failing. A
  plausible second explanation for duplicated rows, alongside the stale-read fix.

- **CharlieTheSlave** — That machine's POI database uses different set names ("A.CA410 1 point", one
  point rather than five), so `defaultPoiSet` in its `commands.json` needs to match what is actually
  there — the sets live in `C:\Radiant Vision Systems Data\TrueTest\AppData\1.9\PointsOfInterest.poi`
  and are per-machine, not shipped with Charlie.

## 2026-10-26

- **CharlieTheSlave** — Renamed the whole project from DeskAssistant: folder `c#\DeskAssistant` →
  `c#\CharlieTheSlave`, csproj, assembly, root namespace, the embedded `commands.default.json`
  resource name, and the settings / export folders (`%AppData%\CharlieTheSlave`,
  `Documents\CharlieTheSlave\TrueTest`). `AppSettings` still reads the old
  `%AppData%\DeskAssistant\settings.json` when the new one is absent, so the icon position, installer
  folder and component list survive the rename; `commands.json` does not, because it sits next to the
  exe and the exe moved — it is recreated from the defaults. Window title and chat header are now
  "Charlie The Slave" and the greeting is "Hey boss, what do you wanna do?".

- **CharlieTheSlave** — Added `export poi`: walks TrueTest's Measurement List, opens each measurement's
  "Define Points of Interest…" window, selects a named POI set (default `CA410 5 points`, from
  `defaultPoiSet` in commands.json) and writes every point's Color Cx / Color Cy / Luminance to an
  `.xlsx` in `Documents\CharlieTheSlave\TrueTest\`. New files: `TrueTestPoi.cs` (automation),
  `PromptWindow.Poi.cs` (command + workbook), `GlobalHotkey.cs`. Columns are
  `Measurement Setup | Point | Color Cx | Color Cy | Luminance`, with the setup cell merged across the
  points of each run of measurements that share a setup name. Took a ClosedXML dependency (0.105.1) for
  real `.xlsx`; the CSV helper stays for the other exports. It runs without asking to confirm (the user
  wanted it immediate) and opens the finished workbook when it's done.

- **CharlieTheSlave** — Deploy package: `publish\CharlieTheSlave\` (self-contained, single file,
  win-x64, compressed — 74 MB exe plus README.txt) and `publish\CharlieTheSlave.zip`. Built with
  `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none`.
  No .NET runtime needed on the target; still needs admin (the manifest is `requireAdministrator`, and
  that is also what makes synthetic input into TrueTest work). The Release single-file build has not
  been run end to end — only the Debug build has.
  For this to work off one machine, `installerFolder` and the "open installers" command in
  `commands.default.json` now say `%USERPROFILE%\Downloads\Programs` instead of a hard-coded
  `C:\Users\Chau\…`, and `PromptWindow.InstallerFolders()` expands environment variables (it used to
  pass the raw string to `Directory.Exists`, so a variable would simply never match).

- **CharlieTheSlave** — Ctrl+Alt+Q is now a system-wide stop key (`GlobalHotkey.cs`, registered in
  `PromptWindow.OnSourceInitialized`). The POI export keeps TrueTest in the foreground and moves the
  pointer, so the app's own window cannot be typed into while it runs. If another program already owns
  the key the app says so at startup rather than failing quietly.

- **CharlieTheSlave** — Fixed "TrueTest has no Measurement List window open" appearing out of nowhere:
  the user had maximized the Measurement List MDI child. A maximized MDI child gives up its own title
  bar (TrueTest moves the name into the main window's title, which still reads "- [Measurement List]")
  and UI Automation stops reporting it as a `Window` with that name, so the lookup by window name
  found nothing even though the grid was right there. `TrueTestPoi.MeasurementGrid` now falls back to
  finding the grid by a column only it has, `Measurement Date / Time` (new `measurementDateColumn` in
  the `trueTest` config block) — the sequence grid also has a `Measurement Setup` column, so that one
  cannot be used to tell them apart.

- **CharlieTheSlave** — "I couldn't scroll to row 1/2" on the two rows hidden behind the column
  headers was a boundary bug, not a scrolling one. Scrolled fully up, a row's top lands on *exactly*
  the header strip's bottom edge (both 217 here), and the test `r.Top >= firstUsable` fails on any
  fractional, display-scaled difference — so the loop wheeled against the end stop until the
  stuck-detector fired. Row 3 at y=241 was clear of the edge and always worked, which is what made it
  look like "scrolling up is broken". Now compared with 2px of slack (`Slack`), and the failure
  message prints the row's y range, where the rows start and where the grid ends.
  Verified beforehand that the mechanism itself was fine: Activate-click then one wheel turn moved
  row 0 from y=169 (hidden) to y=217 (visible).

- **CharlieTheSlave** — Stale point values: the last two measurements of a 20-measurement run came out
  byte-identical across all five points and three columns. A freshly opened POI window shows the set's
  previously computed numbers until TrueTest has re-evaluated it against this measurement's image, and
  the "two identical reads in a row" settle check passes straight through that. It only had something
  to compare against when the POI *set* had to be switched — true for measurement 1 and nothing after,
  since the set then stays selected. `ReadOne` now carries the previous measurement's readings and
  `ReadPoints` waits until the values differ from them; if they never do, the measurement is reported
  as a skip rather than writing the same numbers twice.

- **CharlieTheSlave** — The real cause of "no pop-up window open at all" on later measurements:
  **the Measurement List child window was taller than the MDI area and so clipped**. Measured: MDI
  client 382,74 1538x366 (bottom 440) against a Measurement List of 384,76 700x600 (bottom 676) and a
  grid ending at 668. Everything below y=440 is not drawn — but UI Automation reports those rows at
  ordinary coordinates with `IsOffscreen=false`, so the click went to the Analysis Results panel
  underneath, and the grid never scrolled because as far as it is concerned all 20 rows fit.
  This masqueraded as three different bugs across several rounds (scroll up broken, scroll down
  broken, wrong click coordinates). **Check the MDI client rect against the child's before believing
  any coordinate from this grid.**
  Two changes: `FitMeasurementList()` maximizes the Measurement List for the run and restores its
  previous state afterwards, and `VisibleGrid()` intersects the grid with the MDI client so the
  on-screen area is never over-estimated if maximizing fails (new `mdiClientClass` in the config).

- **CharlieTheSlave** — Scrolling the Measurement List is done with **posted PageUp/PageDown keys**
  (`VK_PRIOR`/`VK_NEXT` to the grid's window), after the mouse wheel turned out to be undependable.
  Measured, in this order, which is worth not repeating:
  - The wheel (posted *or* real) is ignored unless TrueTest is the foreground window; sometimes it
    also needed the grid to have been clicked first, and sometimes it did nothing regardless. Hours
    went into chasing that because the grid's state changed between probes.
  - Posted page keys scroll both ways with no foreground, no activation, no cursor involvement:
    row 19 moved into view with one PageDown, and two PageUps returned to the top. That is the
    primitive to use for this grid.
  - The row *headers* are the only elements whose `IsOffscreen` can be believed; the rows themselves
    always say `false`. A scrolled-away row keeps reporting a position under the column-header strip
    rather than a negative one, so "in view" is tested against that strip's bottom edge, with 2px of
    slack — scrolled fully up, a row's top lands on exactly that edge and an exact `>=` fails on
    display-scaled coordinates, which produced a bogus "I couldn't scroll to row 1".
  - No scroll pattern exists anywhere to use instead: not `ScrollPattern` on the grid, not
    `ScrollItemPattern` on a row header or a cell (cells offer only Invoke, Value, GridItem,
    TableItem), and invoking a row header does not scroll it into view.
  The `Activate()` click and the wheel code are gone with it.

- **CharlieTheSlave** — `OpenContextMenu` right-clicks the middle of the row's **Measurement Setup
  cell**, read fresh on each attempt, and refuses to click a point outside the grid. It used to
  extrapolate "row header's right edge + 120px", which lands on nothing once the Measurement List has
  been moved or resized — the symptom was "no pop-up window open at all" while the reported click
  point sat outside the visible window. It also re-focuses TrueTest, moves the pointer in two steps so
  the arrival is seen, and tries twice; the failure message gives the click point and the cell rect.

- **CharlieTheSlave** — Fixed a cascade where one failed set-switch wrecked the rest of the run
  ("TrueTest wouldn't switch to CA410 5 points", then "no pop-up window open at all" for every
  measurement after). Cause: `SelectSet` typed F4 / arrows / Enter with **global `keybd_event`**, which
  needs the combo to really hold the focus. When `SetForegroundWindow` + `SetFocus` didn't land, those
  keystrokes went into TrueTest's main window and left it unable to raise a context menu at all.
  The arrow keys are now **posted to the combo's own window** — a closed Win32 combo moves its
  selection on Up/Down and notifies its parent, which is all TrueTest needs — so they cannot land
  anywhere else, F4 and Enter are gone, and a failure now costs one measurement instead of the run.
  Lesson for anything similar here: post keys to the control, never type them globally, unless the
  target genuinely owns the focus.

- **CharlieTheSlave** — The run gives up after `MaxFailuresInARow` (3) consecutive failed measurements
  rather than grinding through twenty. A measurement that simply has no POI entry doesn't count.

- **CharlieTheSlave** — Honest reporting when stopped part-way. `OperationCanceledException` was being
  caught by the per-measurement "carry on" handler, so the interrupted measurement was listed as
  "Skipped 1: W16 — The operation was canceled" and the measurements never attempted went unmentioned
  entirely. Cancellation is now rethrown past that handler, and `PoiExport` carries `Total`/`NotRead`
  so the message reads "Stopped with Ctrl+Alt+Q after 10 of 20 measurements; the remaining 10 weren't
  read." The partial workbook is still written and opened.

- **CharlieTheSlave** — Up/Down in the input box walk the commands typed this session, like a console:
  consecutive duplicates aren't stored, and stepping past the newest entry empties the box. In memory
  only, and `clear` leaves it alone (same as `cls` not clearing doskey history).
  They must be caught in **PreviewKeyDown**: a TextBox treats the arrow keys as its own and marks them
  handled, so a bubbling `KeyDown` handler never sees them (the first attempt did nothing at all).

- **CharlieTheSlave** — Third fatal state: `TrueTestNotReadyException`, for TrueTest being up but not
  usable yet — the main window not found while the process exists ("still starting up"), or no
  measurement grid ("Measurement List isn't ready — it may still be loading the measurement
  database"). It flashes as "TRUETEST ISN'T READY" and asks the user to try again, rather than
  reporting a missing control. Like the other two it is rethrown past the per-measurement handler, so
  closing the Measurement List mid-run stops rather than skipping every remaining measurement.

- **CharlieTheSlave** — `AutomationBanner.Flash` shows the "go and fix this" conditions on screen
  as well as in the chat: TrueTest not running (asks to start it *and* prepare the set) and the POI set
  missing. Same position and styling as the running banner, pulsed three times, gone after 8 seconds,
  with the stop-key box hidden since nothing is running. The warning is held in a local and raised
  after the `finally` has closed the run's banner, so the two never stack. `PoiSetMissingException` and
  the new `TrueTestGoneException` are public for this: the command layer catches them by type rather
  than sniffing messages.

- **CharlieTheSlave** — A missing POI set stops the run. It is thrown as its own
  `PoiSetMissingException` so the "carry on after a bad measurement" handling doesn't swallow it:
  without that, a wrong set name opened and closed the POI window once per measurement and produced
  twenty identical warnings. The message names the sets TrueTest does have and tells the user to set
  one up and run `export poi` again. It can only be detected on the first measurement, because the POI
  window is where the set list lives.

- **CharlieTheSlave** — Made a scrolled Measurement List work. The grid lists all its rows in the UI
  Automation tree whether drawn or not (so they are all readable), but it supports **no scrolling
  pattern at all** — no `ScrollPattern` on the grid, and a row header offers only Invoke and Value, no
  `ScrollItemPattern`. `RowRect` therefore scrolls by posting `WM_MOUSEWHEEL` at the grid (works
  unfocused), two notches at a time, until the row sits below the column-header strip and inside the
  grid. It gives up if two turns of the wheel change nothing, rather than spinning.
  Which way to scroll comes from the row header's own rectangle, which stays accurate for rows the grid
  has not drawn (row 19 reported y=642 under a grid ending at 530). Do not judge it from the rows'
  `IsOffscreen`: the row elements claim `false` at any scroll position, and only the row *headers*
  report it honestly. Verified against the live grid — two wheel turns brought row 19 from y=642 to
  y=498.

- **CharlieTheSlave** — A failed measurement no longer destroys the run. Anything other than
  cancellation is recorded as a skip with its reason, the menu and any POI window are closed, and the
  next measurement is tried; Ctrl+Alt+Q now returns what was collected instead of discarding it
  (`PoiExport.Stopped`), and the workbook is still written and opened. Previously one off-screen row
  threw away every measurement already read.

- **CharlieTheSlave** — Trimmed the command set at the user's request: removed `show step`,
  `set step`, `compare step`, `export steps`, `calibration` and the `check step` macro, together with
  the automation behind them (`TrueTestAutomation` is down to `GetSteps`, `ClickSaveSequence` and
  `MainWindow`), their `trueTest` control names, and the CSV writer. `help` no longer lists the
  `edit / reload / reset commands` line (they still work, just undocumented).
  The removed code is **not** recoverable from git (this repo has no commits), so it is preserved in
  full, with the reasoning behind each workaround, in
  `c#\CharlieTheSlave\docs\truetest-analysis-and-calibration.md`.

- **CharlieTheSlave** — Explorer fallbacks so a fresh machine isn't stuck: `update TrueTest` opens the
  file picker when no installer matches instead of giving up, and `open installers` is now its own
  action (`openInstallers`) that asks for the folder with `OpenFolderDialog` when the configured one
  doesn't exist, remembering the answer in `LastInstallerFolder`.

- **CharlieTheSlave** — Unmatched input no longer reports an Ollama connection error. When Ollama can't
  be reached it answers `I don't know "<what you typed>". Type "help" to see what I can do.`, explaining
  the missing model only the first time. The failure is remembered for a minute
  (`PromptWindow._ollamaOfflineAt`), so later messages answer at once instead of each waiting for the
  connection to fail; a successful reply clears it, so starting Ollama mid-session just works.

- **CharlieTheSlave** — `AutomationBanner` (new window) replaces the easily-missed chat warning during a
  POI run: a red banner across the top of the screen saying the mouse and keyboard are in use, the
  measurement being worked on, and the stop key. It must never take the foreground — that would close
  the pop-up menu mid-read — so it is `ShowActivated="False"` plus `WS_EX_NOACTIVATE`, with
  `WS_EX_TRANSPARENT` so clicks fall through and `WS_EX_TOOLWINDOW` to keep it off the taskbar. Per
  measurement progress now goes to the banner only; the chat gets the start line, warnings (anything
  beginning "⚠") and the result.

### TrueTest POI automation: the traps, in the order they bit

Each of these cost a run, so they are worth keeping.

1. **The entry point isn't where you'd look.** `Define Points of Interest...` is on the Measurement
   List's row context menu — not on any main menu, not on the Bitmap window. The exact string was read
   out of `TrueTest.exe` (handler `ShowDefinePointsofInterestDialog`) after searching the UI failed.
2. **Pop-up menus need the foreground.** Windows destroys a pop-up menu as soon as its owner stops
   being the foreground window, so the posted-message approach the rest of the app uses only makes the
   menu flash. This automation uses `SetForegroundWindow` + real `mouse_event` and restores the cursor
   afterwards. That is what the stop hotkey is for.
3. **UI Automation cannot read this menu.** It reports only the seven entries the context menu inherits
   from the Measurement List tool strip (`Bitmap` … `CIE Color Chart`) and stops at the first separator,
   so `Define Points of Interest...` is invisible — `FindAll`, `FindAll(TreeScope.Children)`, the control
   view walker and the raw view walker all agree on those seven. The menu is therefore read and invoked
   through **MSAA** (`oleacc`: `AccessibleObjectFromWindow` / `AccessibleChildren` /
   `accDoDefaultAction`), which reports it as drawn. UIA is kept only as a fallback.
   Do not "fix" this back to UIA.
4. **A dying pop-up throws `COMException (E_FAIL)`, not `ElementNotAvailableException`.** Both have to
   be caught or the whole run dies on a bare HRESULT (`TrueTestPoi.Transient`).
5. **The POI window is owned by TrueTest's main window**, so UI Automation nests it under its owner and
   it is never a child of the root element. Found with `EnumWindows` + window title, same reason
   `InstallerAutomation` enumerates windows that way.
6. **Switching POI set does not update the table at once.** Reading straight after the switch returns
   the *previous* set's points — the first working run exported the 9-point `Ansi 9 Point LCD Display`
   values while the combo already read `CA410 5 points`. `ReadPoints` now polls until two reads agree
   and the rows differ from the pre-switch snapshot.

Other notes:

- Measurement List is an MDI child window named `Measurement List`; its grid is a `DataGridView` with no
  stable AutomationId (the id is just the hwnd), so it is found as the first `Table` descendant.
- The POI set combo is on the tool strip that also carries `Save Points of Interest Set As`; that button
  is the marker used to tell it from the window's `Luminance` display-channel combo. Items are read with
  `CB_GETCOUNT`/`CB_GETLBTEXT`, which is how a missing set is reported.
- A `DataGridView` only exposes drawn rows to UI Automation, so measurements scrolled out of view are
  invisible. `MeasurementCount()` (GridPattern RowCount) is compared with what was found and the user is
  warned. Untested beyond one screenful — the test DB has 20 rows, all visible.
- On the test database the `Register Pixels` measurements give `Cx=0, Cy=1` for every point; real
  chromaticity only appears on others. Not an automation fault.
