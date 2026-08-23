# ROM Launcher

A single-file C# WinForms ROM launcher for a folder tree like `F:\Games\Roms\<System>\...`.
Type part of a game name, hit Enter, the emulator starts. No database, no scraper, no LaunchBox-sized install.

## Build

```
build.bat
```

Uses `csc.exe` from .NET Framework 4.x (`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\`).
No Visual Studio, no NuGet, no project file. Drop a `RomLauncher.ico` next to the source and it gets embedded automatically.

Output: `RomLauncher.exe` — fully portable, keeps all its state next to itself:

| File | Purpose |
|---|---|
| `RomLauncher.ini` | settings + per-system emulator config |
| `RomLauncher.cache.tsv` | ROM index, so startup is instant |
| `RomLauncher.stats.tsv` | play counts, last played, favorites |

## First run

1. It reads `F:\Games\Roms` (change it in **Systems...**) and treats every first-level subfolder as a *system*.
2. It scans recursively and builds the index.
3. Open **Systems...** and point each system at its emulator.

Nothing launches until a system has an emulator, except `.exe` / `.bat` / `.lnk` / `.cmd` entries, which are shell-executed directly (handy for a PC games folder).

## Searching

- Type in the big box — filtering is live, tokens are AND-ed.
- Multi-word: `snes mario` — a token may match either the title or the system name.
- Fuzzy subsequence fallback: `sm64` finds `Super Mario 64 (USA)`.
- Region/version tags are stripped for matching, so `chrono trigger` beats `Chrono Trigger (USA) [!]` into the top slot.
- Ranking: exact > prefix > word-start substring > substring > subsequence, with small bonuses for favorites and play count, and a penalty for longer titles.

## Keyboard

| Key | Action |
|---|---|
| type | search |
| Up / Down / PgUp / PgDn | move selection without leaving the search box |
| Enter | launch |
| Esc | clear search |
| F2 | toggle favorite |
| F5 | rescan |
| Ctrl+F | focus search |

Right-click a row for launch / favorite / open containing folder / copy path.

## Emulator configuration

Per system, in **Systems...**:

- **Filter box** (above the system list) — type any substring to narrow the list, e.g. `ne` shows `nes`, `neogeo`, `snes`. Up/Down moves the selection without leaving the box, Esc clears it. The plain ListBox type-ahead only matched single characters, which is useless with 100+ systems.
- **Emulator exe** — full path.
- **Arguments** — template with placeholders:
  - `{rom}` full ROM path
  - `{romdir}` its folder
  - `{romname}` file name without extension
  - `{system}` system name
- **Extensions** — optional comma-separated whitelist (`sfc,smc,zip`). Empty means auto-detect.
- **Enabled** — uncheck to skip a folder entirely during scans.

Examples:

```
snes9x.exe        "{rom}"
pcsx2.exe         "{rom}" -fullscreen -nogui
duckstation.exe   -fullscreen "{rom}"
retroarch.exe     -L "C:\RetroArch\cores\snes9x_libretro.dll" -f "{rom}"
```

### RetroArch helper

Set the **RetroArch** path once at the top of the dialog. The core dropdown then lists everything in `...\cores\*_libretro.dll`; pick one, press **Use core**, and the emulator + argument line are filled in for that system. **Copy emulator + args to all systems** applies the current pair everywhere (useful for an all-RetroArch setup, then override the odd system afterwards).

## Scan rules

Ignored extensions: saves, states, patches, artwork, metadata, `.bin`/`.img`/`.sub`/`.ccd` disc parts, etc.
Ignored folders: `media`, `images`, `boxart`, `snap`, `manuals`, `saves`, `states`, `bios`, `videos`, `gamelists`, dot-folders, and similar.

With **Collapse multi-disc / duplicate-format sets** on (default):

- A folder containing an `.m3u` shows only the `.m3u` — one entry per multi-disc game.
- Same base name in several disc formats collapses to one, priority `m3u > chd > cue > gdi > cdi > iso > pbp > nrg`.
- Non-disc duplicates (e.g. `.sfc` alongside `.zip`) are left alone — both stay visible.

Access-denied folders are skipped silently rather than aborting the scan.

## Notes / limits

- The index is cached, so the app opens instantly and only rescans on F5. Newly added ROMs need a rescan.
- No archive introspection: `.zip`/`.7z` are passed to the emulator as-is, which is what most cores want. MAME sets therefore show as short filenames (`sf2ce`) rather than full titles — a `.dat` name-map could be added later.
- Columns are strictly proportional (40/15/7/12/26%) and reflow on resize; there is never a horizontal scrollbar.
- Search is O(n) per keystroke with a 120 ms debounce; ~100k entries stay comfortably interactive.

## Possible next steps

- Global hotkey to summon the window
- Box art panel (read from `<System>\media\boxart\<name>.png`)
- MAME/No-Intro DAT title mapping
- "Recently played" tab and per-system last-played memory
