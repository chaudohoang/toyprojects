# ROM Launcher

A single-file C# WinForms ROM launcher for a folder tree like `F:\Games\Roms\<System>\...`.
Type part of a game name, hit Enter, the emulator starts. No database, no scraper, no LaunchBox-sized install.

## Build

```
build.bat
```

Uses `csc.exe` from .NET Framework 4.x (`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\`).
No Visual Studio, no NuGet, no project file. Drop a `RomLauncher.ico` next to the source and it gets embedded automatically. The bundled icon is a gamepad on an indigo→violet tile; at runtime the app also loads it from the exe so it shows in the title bar and taskbar (not just Explorer).

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
- The **System** dropdown shows a ROM count next to each entry, e.g. `nes  (1,240)`, and `All systems  (37,312)` at the top. Counts refresh automatically after every scan.

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
retroarch.exe     -L "C:\RetroArch\cores\snes9x_libretro.dll" --appendconfig "C:\RetroArch\romlauncher_maximized.cfg" "{rom}"
```

### Emulator presets

For common standalone emulators there's an **Emulator preset** dropdown: pick one, press **Apply…**, browse to its exe, and the emulator path + argument line are filled in for you. The **Window mode** dropdown decides the args — **Window** uses the windowed template, **Maximized** or **Fullscreen** use the fullscreen template (Maximized isn't meaningful for standalone emulators, so it maps to fullscreen). Bundled presets, with args verified against each emulator's current CLI:

| Preset | Fullscreen args |
|---|---|
| DuckStation (PS1) | `-batch -fullscreen "{rom}"` |
| PCSX2 (PS2) | `-batch -fullscreen -- "{rom}"` |
| PPSSPP (PSP) | `--escape-exit --fullscreen "{rom}"` |
| Dolphin (GC / Wii) | `-b --config=Dolphin.Display.Fullscreen=True -e "{rom}"` |
| Flycast (Dreamcast / NAOMI) | `-config window:fullscreen=yes "{rom}"` |
| Yaba Sanshiro (Saturn) | `-i "{rom}" -a -fullscreen` |

`-batch` (DuckStation/PCSX2), `-b` (Dolphin) and `--escape-exit` (PPSSPP) make the emulator close cleanly when you exit the game. You can edit the filled-in args afterward like any other system.

### RetroArch helper

Set the **RetroArch** path once at the top of the dialog. The core dropdown then lists everything in `...\cores\*_libretro.dll`; pick one, choose a **Window mode**, and press **Use core** to fill in the emulator + argument line for that system.

Changing the **RetroArch** path (to a real `retroarch.exe`) re-anchors every RetroArch system to the new folder when you press **OK** — the emulator exe, each `-L "...\cores\<core>.dll"` path, and each `--appendconfig "...\romlauncher_*.cfg"` path are repointed, keeping the core and mode filenames. Standalone emulators (DuckStation, etc.) are left untouched. This mirrors how changing the **ROMs root** re-anchors every system's ROM folder, so moving RetroArch or copying the INI to another PC just needs the two top paths updated.

Once a system already uses a RetroArch core, changing the **Window mode** dropdown rewrites just the window-mode part of its Arguments in place — no need to press **Use core** again — and the dropdown restores to whatever the saved args say the next time you select that system. All of this is written to `RomLauncher.ini` when you press **OK**.

Window modes:
- **Window** — a normal resizable window, via `romlauncher_windowed.cfg`.
- **Maximized** — a real window (title bar + menu) maximized to fill the screen, via `romlauncher_maximized.cfg`. RetroArch can't reliably start maximized, so the launcher opens it as a normal window and maximizes it once the window appears. Slower cores (e.g. PS2) bring their video context up a beat later and can ignore that first maximize — leaving the mouse locked to a stale viewport — so the launcher then does a real restore→maximize "nudge" after the core settles (the same thing as manually un-maximizing and re-maximizing). This is a true maximized window, *not* borderless fullscreen — you keep the title bar and can restore/minimize normally.
- **Fullscreen** — fills the whole screen with no title bar, via `romlauncher_fullscreen.cfg` (`video_fullscreen=true` + `video_windowed_fullscreen=true`, i.e. borderless fullscreen, alt-tab friendly).

Each mode layers only its two lines over your main `retroarch.cfg` with `--appendconfig`; the main config is left untouched.

**Copy emulator + args to all systems** applies the current pair everywhere.

### Alternate emulators (pick at launch)

Each system has one **default** emulator (the Emulator + Arguments fields), plus any number of **alternates**. Click **Alternate emulators…** in the Systems dialog to manage them: **New** adds one, then set its Name, Emulator (Browse), and Arguments — or pick a **Preset** and hit **Fill** to drop in a standalone emulator with its fullscreen args. Alternates are saved per system in `RomLauncher.ini` as `Alt1Name` / `Alt1Emulator` / `Alt1Args`, `Alt2…`, and so on.

At launch time, right-click a game and open **Launch with ▸** to choose: **Default (…)** or any alternate. Double-click / Enter still uses the default. This lets one system (say, PS1) run through RetroArch by default but boot a specific game in DuckStation on demand, without reconfiguring anything.

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
