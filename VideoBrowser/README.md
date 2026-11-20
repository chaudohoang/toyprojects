# Video Browser

Small dark-themed player for culling a folder of videos: the folder's videos are listed
on the right, double-click to play, Del to send the file to the Recycle Bin.

- WinForms, .NET Framework 4.8, x64. Playback via **LibVLC** (bundled from NuGet in
  `bin\Release\net48\libvlc\win-x64`, ~100 MB), so .ts/.m2ts/.mkv/.mp4/.avi/.wmv/.webm/… all
  play without VLC or Windows Media Player installed.
- Build: `build.bat` (or `dotnet build -c Release`). Run: `bin\Release\net48\VideoBrowser.exe [folder]`.
- Settings (last folder, volume, window, options) are saved to `settings.txt` next to the exe.

## Features

- Open folder (Ctrl+O), drag & drop a folder/file onto the window, or pass it on the command line.
- Optional *Include subfolders*; natural sort (`clip 2` before `clip 10`); filter box (Ctrl+F,
  space-separated terms all must match).
- Playlist shows `[001]` index, name, size; the playing file is highlighted.
- Auto-play next video (option). Fullscreen with F / F11 / double-click, Esc to leave; in fullscreen the
  control bar slides over the bottom of the video when the mouse moves and hides after 2.5 s idle.
- **Delete**: Del → Recycle Bin (confirmation can be switched off in Options),
  Shift+Del → permanent (always confirms). Multi-select works. If the playing file is
  deleted, playback stops first (to release the file), then continues with the next one.
  Note: drives without a Recycle Bin (some USB/network drives) delete permanently.

## Keys

| Key | Action |
|---|---|
| Enter / double-click | Play selected |
| Space | Play / pause |
| S | Stop |
| N / P | Next / previous |
| ← / → | Seek 5 s (Shift: 30 s) |
| Ctrl+↑ / ↓, M | Volume, mute |
| F, F11, Alt+Enter | Fullscreen |
| Del / Shift+Del | Recycle Bin / permanent delete |
| Ctrl+F, Ctrl+O, F5 | Filter, open folder, refresh |
