# Multi Branch Switcher / Cloner

A single-file C# WinForms tool for driving a whole folder of git repositories at once. Two modes:

- **Switch repos in place** — scan a folder for repositories and switch them all to one branch.
- **Clone master folder to a new branch folder** — refresh a prepared *master* folder, copy it to a new
  folder named after the branch, then switch the copy. Repos without that branch stay on master and are
  logged rather than failing the run.

Built for the case where TrueTest work spans a dozen-plus repos (`CommonTargets`, `TrueTestCore`,
`TT-DemuraLGDN`, `WPF`, `General_DI`, ...) and every branch change means repeating the TortoiseGit
*Switch/Checkout* dialog once per repo.

## Files

| File | Purpose |
|---|---|
| `MultiBranchSwitcher.cs` | Entire application (UI built programmatically, no Designer) |
| `build.bat` | Compiles with `csc.exe` from the Windows-bundled .NET Framework 4.x |
| `MultiBranchSwitcher.ini` | Written next to the exe on exit; remembers mode, folders, branch, options |
| `MultiBranchSwitcher.ico` | Application icon, embedded into the exe by `build.bat` |
| `make_icon.bat` | Double-click to regenerate the `.ico`; finds Python, installs Pillow if needed |
| `make_icon.py` | The icon generator itself; only run it to change the design |
| `icon_preview.png` | The icon at every embedded size on light and dark backgrounds |

## Requirements

- Windows with .NET Framework 4.x (already present on Win10/11)
- `git.exe` on `PATH` (Git for Windows). TortoiseGit's bundled git works — add
  `C:\Program Files\TortoiseGit\bin` to `PATH`, or set `GitExe=` in the INI to a full path.
- `robocopy` (ships with Windows) for the clone mode copy.

## Build

```
build.bat
```

Produces `MultiBranchSwitcher.exe` in the same folder. No project file, no NuGet, no dependencies.

`build.bat` passes `/win32icon:MultiBranchSwitcher.ico` when that file is present, so the icon is baked
into the exe — no side-car file needed at runtime. Delete or replace the `.ico` and the build still
succeeds, with a warning. The form picks the same icon back out of the exe at startup, so the title bar,
Alt-Tab and taskbar all match.

The glyph is a branch fork: a near-white master line with two nodes, and an amber branch curving off it —
white for what you keep, amber for what gets created. The 16/24/32px entries are rendered from a separate
master with a tighter badge so the fork still reads at taskbar size. To recolour or redraw, edit the
four colour constants at the top of `make_icon.py`, double-click `make_icon.bat`, then rebuild.

`make_icon.bat` is the reliable way in: it prefers the `py` launcher, falls back to `python` on `PATH`,
installs Pillow if it is missing, and pauses so errors stay readable. Double-clicking `make_icon.py`
directly also works when Python and Pillow are already set up — it writes next to the script rather than
into Explorer's current directory, and waits for a keypress before closing. Neither is needed for a normal
build; the `.ico` is already there.

---

## Mode 1 — Switch repos in place

1. **Root folder** — the parent holding the clones, e.g. `D:\Branch\New folder`.
2. **Depth** — levels below the root to search for a `.git`. `1` if repos sit directly under the root,
   `2` (default) also covers `root\Group\Repo`. Descent stops at the first repo found, so submodules
   inside a repo are left alone.
3. **Scan** — lists every repository and its current branch.
4. **Branch** → **Switch all**.

Per repository: optional fetch, then exact local name → exact remote name (`origin` first) → suffix
match. Local hit gets `git checkout <name>`; remote-only hit gets
`git checkout -b <name> --track <remote>/<name>`, the equivalent of TortoiseGit's *Create New Branch +
Track*. No hit leaves the repo untouched and logs its local branches so a typo or wrong prefix is obvious.

## Mode 2 — Clone master folder to a new branch folder

Keep one folder of repos permanently on master. Each time you need a branch, the tool builds a fresh
working folder from it — no re-cloning over the network, because the `.git` directory is copied too.

1. **Master folder** — the prepared folder, e.g. `D:\Branch\master`.
2. **New folder** — auto-suggested as *(parent of master)* `\` *(last segment of the branch name)*, so
   `feature/LGD-Mobile_Release_20231120` suggests `D:\Branch\LGD-Mobile_Release_20231120`. Edit or
   **Browse** to override; once you override, auto-fill stops.
3. **Branch** → **Clone + switch**.

Per repository, in order:

1. **Refresh master** — `git fetch --all [--prune]`, then either `git reset --hard <upstream>` plus
   `git clean -fd` when **Force** is on, or `git pull --ff-only` when only **Pull** is on.
2. **Copy** — `robocopy master\<Repo> new\<Repo> /E /COPY:DAT /DCOPY:DAT /MT:16` (`/MIR` instead of `/E`
   when overwriting). Exit codes 0–7 are success.
3. **Switch the copy** — same resolution as mode 1, with no second fetch since the refs came across in
   the copy.
4. **No branch found** — the copy stays on master, the row reads `Copied, kept master (branch not found)`,
   and the run continues.

Loose files sitting directly in the master folder root (`.sln`, scripts) are copied once at the end.
The new folder cannot be the master folder, inside it, or a parent of it — the tool refuses and says so.

---

## Options

| Option | In place | Clone mode |
|---|---|---|
| **Fetch first** | `git fetch --all` per repo before resolving | Same, on the master copy |
| **Prune** | Adds `--prune`, dropping stale `origin/...` refs | Same |
| **Create local branch from remote** | Enables `checkout -b --track` for remote-only branches | Same, applied to the copy |
| **Force** | `checkout --force` / `-B` — **discards uncommitted changes** | `reset --hard <upstream>` + `clean -fd` on master before copying — **discards uncommitted master work** |
| **Pull --ff-only** | Fast-forwards after switching | Fast-forwards master before copying |
| **Match name without folder prefix** | Whole-segment suffix match, so `LGD-Mobile_Release_20231120` resolves `feature/LGD-Mobile_Release_20231120`. Exact matches win first | Same |
| **Overwrite existing destination** | — | Mirrors over an existing copy with `/MIR`, deleting files not in master. Off → existing folders are skipped |
| **Enable long paths (core.longpaths)** | `git config core.longpaths true` per repo before checkout | Same, on both master and the copy |
| **Dry run** | Reports what would happen, changes nothing | Refreshes/fetches at most; no reset, pull, or copy |

**Force** always asks for confirmation. **Overwrite** asks only when repository folders actually exist in
the destination, and says how many. If they exist while Overwrite is off, the run logs how many will be
skipped instead of prompting.

The destination folder is created only after every confirmation is accepted, so answering **No** leaves
nothing behind. Before that, the path is validated (syntax and drive) so a typo fails immediately.

Without **Force** in mode 1, any repo with uncommitted changes (`git status --porcelain` non-empty) is
skipped and reported with the change count, rather than half-switched.

## Results column

| Result | Meaning |
|---|---|
| `Switched` / `Copied + switched` | Existing local branch checked out |
| `Created + switched (from origin)` | Local tracking branch created from the remote |
| `Already on <branch>` | No action needed |
| `Copied, kept master (branch not found)` | Clone mode: copy made, branch does not exist, left on master |
| `Destination exists - skipped` | Clone mode: target repo folder is populated and Overwrite is off |
| `Skipped - N local change(s)` | Dirty working tree, Force off |
| `Remote only - creation disabled` | Branch is remote-only and the create option is off |
| `Ambiguous: a, b` | Several branches end with the typed name; type more of the path |
| `Branch not found` | Not present locally or on any remote |
| `Failed - path too long, tree incomplete` | Windows 260-char limit hit mid-checkout; see below |
| `Copy failed: ...` / `Failed: ...` | Robocopy or git error, verbatim |

The option checkboxes sit in auto-sizing flow rows, so labels re-space themselves when the mode changes
the wording (`Pull --ff-only` becomes `Pull master --ff-only`) and nothing clips at higher DPI.

The window opens maximized and remembers its size and maximized state in the INI. The un-maximized size is
stored separately (from `RestoreBounds`), so restoring down always lands on a usable window rather than a
cramped default — note that a maximized window cannot be resized by dragging its edges; restore it first. Columns are always sized to fill the list exactly, so there is no horizontal
scrollbar. Each gets a fixed share of the width — 12 / 26 / 24 / 38 percent for Repository / Branch /
Result / Path, subject to a per-column minimum — and anything too long is ellipsised by the ListView.

Because the shares are fixed rather than measured from the rows, the layout depends only on the window
width: it reflows when the window is resized and never shifts when a run fills the list. Right-click the
list for **Fit columns to window** to reapply it after dragging a column border by hand. To change the
balance, edit the `weight` array in `FitColumns`.

Every git and robocopy command and its output goes to the log pane. Right-click the list to copy rows as
TSV; double-click a row to open that repo — the destination copy in clone mode — in Explorer.

## Long paths (the 260-character limit)

Branch names make long folder names, and a name like
`Azure-13964-CrystalView-InitialFactoryRelease-11-26-2025_TROY-StagingBranch` is 74 characters before any
repo content. Add a deep path such as
`Radiant.MPA.Blobs2.Grid.MirrorDistortionAnalysis\Test\...\....csproj` and the total crosses Windows'
`MAX_PATH` of 260.

Robocopy uses long-path APIs and copies these fine — git is what fails, with
`error: unable to create file ...: Filename too long`. Worse, git may move `HEAD` to the new branch
*before* hitting the limit, so the checkout reports failure while the working tree is left **incomplete**,
missing exactly the files it could not write.

The tool handles this three ways:

- **Enable long paths (core.longpaths)** (on by default) runs `git config core.longpaths true` in each
  repo before any checkout, so git uses the Unicode APIs instead of the ANSI ones.
- Before the run, the destination path length is checked and a warning is logged if it leaves under
  ~170 characters for paths inside each repo.
- A `Filename too long` failure is reported as `Failed - path too long, tree incomplete` rather than a
  raw git message, and the log states plainly that files are missing.

**Recovering a repo that already failed this way**, since it looks switched but is not complete:

```
cd <destination>\<Repo>
git config core.longpaths true
git checkout --force -B <branch> --track origin/<branch>
git status                     ' should report a clean tree
```

Re-running the tool with **Overwrite existing destination** does the same thing wholesale.

`core.longpaths` only fixes git. MSBuild, Visual Studio, and older tools have their own limits, so for
deep solutions also enable long paths system-wide, then reboot:

```
reg add "HKLM\SYSTEM\CurrentControlSet\Control\FileSystem" /v LongPathsEnabled /t REG_DWORD /d 1 /f
```

Even with both, the reliable fix is a shorter destination folder name — `Azure-13964-CV-Staging` instead of
the full branch name. The **New folder** box is free text, so shorten the suggestion before running.

## Notes / limits

- Sequential, one repo at a time, so the log stays readable and credential prompts can't pile up.
- Credential prompting is disabled (`GIT_TERMINAL_PROMPT=0`) — a repo needing fresh credentials fails
  fast instead of hanging. Authenticate once with git/TortoiseGit first.
- **Stop** finishes the current repository, then cancels the rest.
- The only destructive operations are behind **Force** (`checkout --force`, `reset --hard`, `clean -fd`)
  and **Overwrite** (`robocopy /MIR`). Nothing is ever pushed.
- Submodules are not initialised or updated after a copy; the copy carries whatever master had.
- Tags and specific commits are out of scope; this is branch work only.
- Keep the master folder itself short (`D:\Branch2\master`) — every character there is also spent in the
  copy's path budget.
