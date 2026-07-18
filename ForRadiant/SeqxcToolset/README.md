# Seqxc Toolset

A task-plugin desktop tool for `.seqxc` sequence files. Opens a file once, then
each "task" is an independent module that reads/edits a specific slice of it.

WPF, .NET Framework 4.8, SDK-style csproj, zero NuGet dependencies — same
pattern as SeqxcEditor / MultiRemoteTool.

## Build

Place `app.ico` in the project root (next to `SeqxcToolset.csproj`) — it's
used for the exe icon, title bar, and taskbar icon.

```
build.bat
```
Locates MSBuild via `vswhere.exe` (with VS2022/2019 fallbacks), builds Release.
Output: `bin\Release\net48\Seqxc Toolset.exe`

## How the file structure maps (learned from X4023-2CB-P1_RSP_POR_DX_MATHON.seqxc)

- `<Items><SequenceItem>` — the ~29 steps in the sequence. Has `<Selected>`,
  `<PatternSetupName>` (a reference by name), `<Analysis xsi:type="...">`.
  It does **not** hold a PatternNumber directly.
- `<PatternSetupList><PatternSetup>` — ~198 named pattern definitions
  (`CalG`, `W16r2`, `g192`, `r216`, ...). Each is either:
  - **terminal**: `<Pattern><Pattern xsi:type="Dove3p0_PG.Dove3p0_Pattern">...<PatternNumber>N</PatternNumber>...`
  - **alias**: `<Pattern><PatternSetupName>OtherName</PatternSetupName></Pattern>` —
    points at another PatternSetup instead of owning its own number
    (e.g. `r216`/`g216`/`b216` all resolve through `W216`).

`Core/SequenceDocument.cs` handles this: `ResolveTerminal(name)` follows the
alias chain until it finds the real `<PatternNumber>`. `GetSiblingAliases(name)`
finds every other name that resolves to the same terminal, so the UI can warn
you before a change silently affects them too.

## Saving

`SaveMinimalDiff` does **not** re-serialize the whole XDocument (which would
reformat/reindent a 70k+ line file). It re-reads the original file as text,
finds the specific `<PatternSetup>` block by `<Name>`, and replaces only the
`<PatternNumber>` value inside it — a targeted text patch, same approach used
elsewhere in the TrueTest tooling. If it can't find an exact match it skips
that change and reports a warning rather than risking corruption.

## Task 1: Pattern Numbers

- Lists every `Selected=true` SequenceItem (toggle to show all) with its
  resolved current PatternNumber.
- Typing directly into a row's New # (not via paste) also live-mirrors into
  every other row sharing the exact same `PatternSetupName` (e.g. two `CalG`
  steps) — that sameness is obvious just from the name, so it's safe to
  mirror instantly. This deliberately does NOT extend to alias siblings like
  `r216`/`g216`/`b216` sharing `W216`: those have visibly different names, and
  that side effect is already surfaced explicitly at Save time via the
  "shared by aliasing, continue?" confirmation — silently cascading it live
  here would bypass that warning.

- Paste a 2-column range (`Name`, `To be`) or 3-column range (`Name`, `As is`,
  `To be`) copied straight from Excel — no header row needed. "Parse & Match":
  - **Strict scope**: only PatternSetups tied to a currently-visible row are
    ever eligible — a selected `SequenceItem`, or any item at all if "Show all
    items" is checked. Orphan library patterns with no item reference, and
    steps hidden by the Selected filter, are never matched, auto-applied, or
    offered in the picker — full stop.
  - Resolves each name to its underlying terminal PatternSetup and applies
    the value to **every** row sharing that terminal at once — e.g. two
    SequenceItems both named `CalG` are literally the same pattern, so one
    pasted `CalG` line updates both, instead of leaving one blank.
  - If a name can't be resolved within that scope, or it resolves to a
    terminal that already got a value earlier in the same paste (a genuine
    ambiguity — e.g. several differently-valued lines that can't all be the
    same node), a picker dialog pops up: pick which step this specific line
    should apply to, or Ignore it (or Ignore All Remaining). Candidates are
    ranked by similarity to the pasted name — tokenized on letter/digit runs,
    so `R31` naturally ranks `Step #7: W31_step23_R` at the top (shared "31",
    shared trailing "R" channel suffix) — with the top match pre-selected so
    Enter applies it immediately.
  - Even an exact-name match isn't auto-trusted if a different in-scope step
    fuzzy-matches distinctly better (e.g. a literal `W34` step existing
    alongside `W34_10NIT` when the rest of the paste is clearly `_10NIT`
    values) — that still routes through the picker instead of guessing wrong.

- "Save Changes..." resolves aliases, detects conflicts (two rows pointing at
  the same terminal with different new values), warns about affected sibling
  aliases, then writes the patched file wherever you choose. After a
  successful save it reloads straight from the saved file and rebuilds the
  grid, so Current # always reflects what's actually on disk rather than
  in-memory assumptions (and FilePath correctly points at the saved copy for
  any further edits).

## Task 2: Exposure Time

- `CaptureFilter`/`ExposureTime` live directly on each `<PatternSetup>` element
  itself (7-slot arrays), unlike `PatternNumber` — they are **not** routed
  through the alias chain, since each named PatternSetup has its own capture
  configuration regardless of pattern-number aliasing.
- Confirmed empirically against the sample file: indices 0/4/5/6 are never
  `true` across all 198 PatternSetups — only indices 1/2/3 are ever used, and
  they match TrueTest's own UI exactly: `Y (Green)`, `X (Red)`, `Z (Blue)`.
  `SequenceDocument.GetChannels(name)` reads those three directly off the
  PatternSetup's own element.
- Grid shows Capture (checkbox, read-only) + Exposure (ms, read-only) for
  each of Y/X/Z per step, each followed by an editable **New** cell: a
  tri-state checkbox for capture (indeterminate = no change, click to cycle
  checked → unchecked → back to indeterminate) and a plain text cell for
  exposure (blank = no change, same rule as Pattern Numbers). A **New Exp
  (all)** column right after Analysis fans a typed value out to all three
  Y/X/Z exposure cells at once (the common case where they match) — it's a
  one-way broadcast, not a bound mirror, so editing an individual channel
  afterward still overrides it independently. Dirty rows highlight the same
  way. Rows sharing the exact same PatternSetupName (e.g. "CalG" used at both
  a RegisterPixelsLGDN and a DemuraLGDNPOCB4p2 step) point at the same
  underlying element, so editing any New cell live-mirrors into every other
  row with that name — not just consistently at save time.
- "Save Changes..." resolves each channel's edits, patches `CaptureFilter`/
  `ExposureTime` with the same minimal-diff approach as Pattern Numbers —
  but since those arrays have several indistinguishable `<boolean>`/`<float>`
  siblings, the patch counts to the Nth occurrence within the right container
  rather than matching on value text (`SequenceDocument.SetChannelValue` /
  `SaveExposureChanges`). Reloads from the saved file afterward, same as
  Task 1.
- No bulk Excel import yet — the column layout for exposure changes isn't
  settled, so for now it's direct in-grid typing only. Follows the same
  Selected/"Show all items" filter as Task 1.

## Task 3: Luminance Scale

- `LuminanceScaleRed`/`Green`/`Blue` live directly on the **SequenceItem's own
  `Analysis` element** — not in `PatternSetup` at all, unlike Tasks 1 and 2.
  Only some Analysis types have them (e.g. `DemuraLGDNPOCB4p2`); rows whose
  type doesn't will just show blank Red/Green/Blue, and their New cells
  (including New (all)) are disabled/grayed out entirely — nothing to apply
  a value to, so editing there wouldn't do anything on Save anyway.
- Since a `SequenceItem` has no unique name to key off (unlike `PatternSetup`,
  which has `<Name>`), saves are addressed by **ordinal position** instead —
  `SequenceDocument.SetSequenceItemField`/`SaveLuminanceScaleChanges` locate
  the target by counting to the Nth `<SequenceItem>` block in the file.
- Same New-column + **New (all)** broadcast pattern as Exposure Time (blank
  = no change; typing in "New (all)" fans out to Red/Green/Blue, since they
  usually match; editing an individual channel afterward still overrides it).
- No bulk Excel import yet, same as Exposure Time.

## Saving across multiple tasks in one session

Each task's own Save button still works exactly as before (its own file
dialog, its own confirmation). On top of that, the toolbar has two buttons
for editing several tasks in one sitting:

- **Save All Changes...** — shows one file dialog, then walks every task
  that has pending edits, in order (Pattern Numbers → Exposure Time →
  Luminance Scale), calling each one's internal save (same validation/
  conflict logic as its own Save button, including the alias-sharing
  confirmation — declining that just skips that one task rather than
  aborting the whole batch). After each task writes, `_document` reloads
  from the file before the next task runs, so every task's save reads the
  version that includes everyone before it. Once the whole batch is done,
  every task's grid refreshes together, and one combined summary is shown
  (total changes, which tasks contributed, anything skipped or warned).
- **Clear All Changes** — clears pending "New" edits in every task at once,
  no confirmation (mirrors what each task's own "Clear New Values" already
  does, just for all three in one click).

All three tasks share a single `SequenceDocument` instance, but each only
refreshes its own grid after its own (individual) save — there's
deliberately no *automatic* cross-task refresh outside of the explicit
"Save All" flow above. That's not a gap: each task's Save reads the file
**fresh from disk** at save time (not from some in-memory snapshot) and
`_document.FilePath` is updated the moment any task reloads, so saving in
Task 1 then Task 2 then Task 3 individually, in any order, still always
patches correctly on top of whatever the previous one just wrote. Since the
three tasks edit entirely distinct XML tags (`PatternNumber` vs
`CaptureFilter`/`ExposureTime` vs `LuminanceScaleRed/Green/Blue`), there's
nothing for one task's save to make another task's *displayed* Current
values wrong either.

An earlier version of this wired a shared "Reloaded" event so every task's
grid refreshed the instant *any* task saved individually — reverted, because
a naive full-rebuild refresh wipes out any not-yet-saved "New" edits sitting
in a tab you aren't currently looking at, which is worse than the cosmetic
staleness it was solving. The "Save All Changes" button above is the correct
place for a unified refresh, since by definition nothing is left pending to
lose at that point.

## Copy/paste in the grid

All three grids support Excel-style cell-range copy/paste for their "New"
columns (`DataGridPasteHelper.cs`):

- **Copy** needs no extra code — WPF's `DataGrid` already exports a selected
  cell range as tab/newline-delimited text on Ctrl+C.
- **Paste** (Ctrl+V) reads the clipboard starting at the top-left of whatever
  cells are currently selected, spreading values down rows and across
  columns as far as they fit — so copying a single New-column value down 13
  rows and pasting it onto a different block of rows below works, and so
  does pasting a full rectangular block across multiple New columns at once.
- Grids use `SelectionUnit="Cell"` so individual cells (not just whole rows)
  can be selected/dragged into a range, matching normal spreadsheet behavior.
- Safety net: paste only ever writes into a bound property whose name starts
  with `"New"` — even if a selection rectangle happens to span a read-only
  "Current" column, that column can never be overwritten.
- Works generically across row types via reflection on the target column's
  binding path, so the same helper serves Pattern Numbers' `NewPatternNumber`
  (string), Exposure Time's `NewYCapture`/etc. (`bool?`) and
  `NewYExposure`/etc. (string), and Luminance Scale's `NewRed`/`NewGreen`/
  `NewBlue`/`NewAll` (string) without per-task-specific paste code.

## Adding a new task

1. New folder under `Tasks/YourTask/` with a `UserControl` (view) + view-model.
2. Implement `ITaskModule` (`TaskName`, `View`, `OnDocumentLoaded(SequenceDocument)`).
3. Register it in `MainWindow.RegisterTasks()`.

That's it — it shows up in the sidebar automatically and receives the shared
`SequenceDocument` whenever a file is opened.
