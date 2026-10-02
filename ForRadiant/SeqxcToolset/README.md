# Seqxc Toolset

A task-plugin desktop tool for `.seqxc` sequence files. Opens a file once, then
each "task" is an independent module that reads/edits a specific slice of it.

WPF, .NET Framework 4.8, SDK-style csproj, zero NuGet dependencies — same
pattern as SeqxcEditor / MultiRemoteTool. Reading .xlsx files is done against the
framework's own `System.IO.Compression` + `System.Xml.Linq` (see **Reading .xlsx**),
so that stays true: no ClosedXML/EPPlus, and no Excel install or COM interop on the
machine either.

## Build

The window opens at 1700x1000 (minimum 900x600), centred on the **primary** monitor.
Centring is done in code against `SystemParameters.WorkArea` rather than with
`WindowStartupLocation="CenterScreen"`, which can pick a secondary monitor; the size is
clamped to that work area first, so on a smaller panel the title bar can't end up off
screen.

Place `app.ico` in the project root (next to `SeqxcToolset.csproj`) — it's
used for the exe icon, title bar, and taskbar icon.

```
build.bat
```
Locates MSBuild via `vswhere.exe` (with VS2022/2019 fallbacks), builds Release.
Output: `bin\Release\net48\Seqxc Toolset.exe`

> **Confirm you're running the fresh build.** The title bar carries the exe's build
> time — `Seqxc Toolset - build 2026-09-30-15-49-00`. If it doesn't match the build you
> just made, you're looking at an older copy. The stamp is the exe's own last-write
> time (`ApplyBuildStampToTitle` in `MainWindow.xaml.cs`): a deterministic .NET build
> can't supply this from the PE header, whose timestamp field holds a content hash
> rather than a time.

## How the file structure maps (learned from X4023-2CB-P1_RSP_POR_DX_MATHON.seqxc)

- `<Items><SequenceItem>` — the ~29 steps in the sequence. Has `<Selected>`,
  `<PatternSetupName>` (a reference by name), `<Analysis xsi:type="...">`.
  It does **not** hold a PatternNumber directly.
- `<PatternSetupList><PatternSetup>` — ~198 named pattern definitions
  (`CalG`, `W16r2`, `g192`, `r216`, ...). Each is either:
  - **terminal**: `<Pattern><Pattern xsi:type="Dove3p0_PG.Dove3p0_Pattern">...<PatternNumber>N</PatternNumber><PatternString>S</PatternString>...`
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

## Reading .xlsx

`Core/XlsxReader.cs` is a small OOXML reader — an .xlsx is a zip of XML parts, so
`ZipFile` plus `XDocument` is the whole mechanism. `System.IO.Compression` and
`System.IO.Compression.FileSystem` are named explicitly in the csproj because .NET
Framework doesn't auto-reference them; they are framework assemblies, not packages.

Worth knowing about it:

- Cells are addressed by their **reference** (`H4`), never by position in the row.
  A workbook stores only non-empty cells, so a row starting at column C would
  otherwise have C read as A. Rows and columns are both sparse.
- A sheet's name is resolved to its part through `workbook.xml` + the workbook
  relationships. The filenames do not reliably follow tab order — `sheet2.xml` can be
  the first tab — so the relationship lookup isn't optional.
- Numbers are rendered as **Excel displays** them, not as stored. A cell showing 10.03
  holds 10.029999999999999 and one showing 0.01415 holds 1.4149999999999999E-2;
  `G15` (fifteen significant digits, Excel's own display precision) reproduces the
  on-screen value, and integers print without a decimal point so an index reads 1008.
- Shared strings concatenate their `<r>` runs, skipping `<rPh>` — those are the
  phonetic-guide runs Excel adds for East Asian text, and including them would append
  a ruby copy of the string to itself.
- Limits: values only, no formatting, so a date comes back as its serial number.
  Formula cells yield the result Excel cached when it saved. `.xlsm` works (same
  format); `.xls`, the old binary format, does not and must be re-saved first.

## Importing from Excel

Every task carries its controls in a **bar along the bottom** rather than a side panel,
so the grid gets the full window width. The bar shows the **full path** of the workbook
currently in use — `Excel: D:\Log\20260909 X402x model New Pattern index rule.xlsx` —
matching how the sequence path appears in the top toolbar, trimmed with the full text as
a tooltip if it runs long. Then two buttons:

- **"Choose file..."** — pick a workbook, then the sheet and columns.
- **"Choose columns"** — skips the file dialog and goes straight to the sheet and
  column picker for the workbook already named in the bar. For when the file is settled
  and it's the columns being changed: pulling a second field out of the same sheet, or
  correcting a mis-picked column, without walking the file dialog again.

The workbook is app-wide, so choosing one in any task updates the label in all four
(`AppSettings.LastXlsxPathChanged`). The import summary fills the middle of the bar,
trimmed with the full text as a tooltip, and this task's **Clear New Values** and
**Save Changes...** sit at the right — distinct from the global **Save All Changes...**
in the window's top toolbar.

Either way the dialog asks for the sheet, the **pattern name** column and that task's
**value** column, with an optional third "as is" column, and previews the matched rows
before anything is applied.

Each task asks only for the columns **it** edits, and that is the whole design rather
than a limitation. The rule sheet this was built for
(`20260909 X402x model New Pattern index rule.xlsx`, Sheet2) carries all four fields
side by side:

| Column | Header | Task |
|---|---|---|
| D | `Pattern` | the name, for every task |
| H | `Pattern String` | Pattern Strings |
| I | `Current` | — |
| J | `L/v Scale` | Luminance Scale |
| K | `Exp time` | Exposure Time |

A task that guessed its own columns out of that could quietly write a neighbouring
task's field. Importing per task, with the columns named explicitly, cannot. It also
means one sheet is simply imported once per task you want to fill, reviewing each
before Save All.

The header row is **detected**, not assumed to be row 1 — Sheet2 has headers on row 3
and Sheet1 on row 2, under a title row. The rule is the first row near the top with at
least three filled cells of which at least two aren't numbers; a box on the dialog
overrides it when the guess is wrong. Name and value columns are then pre-selected by
matching header text, so the file above opens with `D — Pattern` and the right value
column already chosen.

### What the choices are remembered as

The sheet, header row and column choices are remembered **per task** (see
**Settings**), and restored next time that task's dialog opens — provided the workbook
still has that sheet.

Columns are stored by **header text**, never by index or letter. If someone inserts a
column into the rule sheet, a remembered index would silently point at the wrong column
and write wrong values into the sequence; a remembered name either still finds the same
column or fails to match and falls back to auto-detection. The dialog's preview also
shows what a restored choice resolved to before anything is applied.

### Matching differs by task

Rows arrive as Name / As-is / To-be, but what happens next follows each field's own
rules:

- **Pattern Numbers** and **Pattern Strings** resolve the name through the alias chain
  to a terminal PatternSetup and apply to every row sharing it (`ApplyImportRows`),
  with a picker for genuine ambiguities.
- **Exposure Time** matches the **exact** PatternSetupName, because
  `CaptureFilter`/`ExposureTime` live on each named PatternSetup's own element and an
  alias does not share them. The single sheet column is broadcast to Y/X/Z via
  `NewExpAll`. Unmatched names go to the picker.
- **Luminance Scale** matches the name and then only steps that actually carry the
  fields, since `LuminanceScaleRed/Green/Blue` exist only on some Analysis types. The
  single column is broadcast to Red/Green/Blue via `NewAll`. Unmatched names go to the
  picker, which offers only steps that have the fields.

Two consequences of how these sheets are written:

- **A repeated pattern is normal, not a conflict.** A sheet lists one row per *step*, so
  a pattern used at two steps appears twice — `CalG`, `CalR` and `CalB` each appear
  twice in Sheet2, identical but for the luminance column. A repeat carrying the **same**
  value is counted and skipped rather than treated as ambiguous. A repeat carrying a
  **different** value still is ambiguous and is handled per task (picker, or reported).
- **Luminance lines up by itself.** Of the two `CalG` rows, one has a luminance value and
  one is blank, mirroring the two steps that use it — only one of which has the fields.
  The blank row carries no value so it never reaches the matcher, and the valued row
  lands on the one step that can take it.

### The match picker

**All four tasks** open a match picker when a name can't be placed — either no step
carries it, or it repeats one already given a *different* value. Candidates are ranked by
`Tasks/NameMatch.cs` (shared by every task, so the ranking is identical), with the best
pre-selected so Enter applies it, plus **Ignore** and **Ignore All Remaining**.

This matters because a rule sheet may name its patterns on a different scheme than the
sequence does. `20260909 X402x model New Pattern index rule.xlsx` names them `W48`,
`W192`, `WR192`, while `X4023-CB-P1_RSP_POR_DX_MATHON.seqxc` holds `g48`, `r48`, `b48`,
`g192`… — only `CalG`/`CalR`/`CalB` line up, and the rest is a mapping only the person
importing can make. (Note `W48` is **not** simply all three channels: the sheet gives it
one L/v of 10.03 where the sequence holds 4.958 / 2.662 / 4.008 for `g48` / `r48` /
`b48`.)

Two differences between the tasks, both deliberate:

- **Pattern Numbers and Pattern Strings** also decline to auto-trust an exact match when
  a different in-scope step scores distinctly better (`HasBetterScopedMatch`), for the
  `W34` versus `W34_10NIT` case. **Exposure Time and Luminance Scale do not**: `CalG`
  scores 55 against `Cal2G`, over the threshold, so that rule would raise a dialog on
  rows that already match cleanly. There, an exact name match is trusted.
- **Luminance Scale** offers only steps that actually carry luminance fields. Offering
  one that can't take a value would let you pick a step and then watch nothing happen.

## Settings

`%APPDATA%\SeqxcToolset\settings.xml`, written through `Core/AppSettings.cs` — XML via
`XDocument`, so still no JSON serializer and no NuGet dependency. It holds the last
sequence opened, the last workbook opened, and each task's import column choices.

Not stored beside the exe on purpose: `build.bat` deletes `bin\`, so settings there
would be wiped by every rebuild, and a tool folder on a line PC may not be writable.

- **The last sequence reopens on start.** Silent if nothing is remembered or the file has
  since moved or been deleted — a stale path is a normal thing to find, not an error
  worth a dialog. A file that exists but won't parse does report. The path is saved as
  soon as a file loads, not only on exit, so it survives a crash or a kill from Task
  Manager; because every save path reloads the document from the file it just wrote, the
  remembered path follows "Save As" too.
- **The last workbook seeds the import file dialog**, so a second import opens in the
  right folder.
- Every read and write is best-effort. A corrupt, locked or missing file leaves defaults
  in place rather than stopping the app — nothing here is worth failing a launch over.

## Task 1: Pattern Numbers

- Lists every `Selected=true` SequenceItem (toggle to show all) with its
  resolved current PatternNumber.
- Typing directly into a row's New # (not via import) also live-mirrors into
  every other row sharing the exact same `PatternSetupName` (e.g. two `CalG`
  steps) — that sameness is obvious just from the name, so it's safe to
  mirror instantly. This deliberately does NOT extend to alias siblings like
  `r216`/`g216`/`b216` sharing `W216`: those have visibly different names, and
  that side effect is already surfaced explicitly at Save time via the
  "shared by aliasing, continue?" confirmation — silently cascading it live
  here would bypass that warning.

- "Load .xlsx..." imports from the rule sheet (see **Importing from Excel**), and
  matches like this:
  - **Strict scope**: only PatternSetups tied to a currently-visible row are
    ever eligible — a selected `SequenceItem`, or any item at all if "Show all
    items" is checked. Orphan library patterns with no item reference, and
    steps hidden by the Selected filter, are never matched, auto-applied, or
    offered in the picker — full stop.
  - Resolves each name to its underlying terminal PatternSetup and applies
    the value to **every** row sharing that terminal at once — e.g. two
    SequenceItems both named `CalG` are literally the same pattern, so one
    imported `CalG` line updates both, instead of leaving one blank.
  - If a name can't be resolved within that scope, or it resolves to a
    terminal that already got a different value earlier in the same import (a genuine
    ambiguity — e.g. several differently-valued lines that can't all be the
    same node), a picker dialog pops up: pick which step this specific line
    should apply to, or Ignore it (or Ignore All Remaining). Candidates are
    ranked by similarity to the imported name — tokenized on letter/digit runs,
    so `R31` naturally ranks `Step #7: W31_step23_R` at the top (shared "31",
    shared trailing "R" channel suffix) — with the top match pre-selected so
    Enter applies it immediately.
  - Even an exact-name match isn't auto-trusted if a different in-scope step
    fuzzy-matches distinctly better (e.g. a literal `W34` step existing
    alongside `W34_10NIT` when the rest of the sheet is clearly `_10NIT`
    values) — that still routes through the picker instead of guessing wrong.

- "Save Changes..." resolves aliases, detects conflicts (two rows pointing at
  the same terminal with different new values), warns about affected sibling
  aliases, then writes the patched file wherever you choose. After a
  successful save it reloads straight from the saved file and rebuilds the
  grid, so Current # always reflects what's actually on disk rather than
  in-memory assumptions (and FilePath correctly points at the saved copy for
  any further edits).

## Task 2: Pattern Strings

`<PatternString>` is a **newer addition to the format**, sitting immediately after
`<PatternNumber>` inside the very same terminal `<Pattern xsi:type="Dove3p0_PG.Dove3p0_Pattern">`
element. Because the two tags share an element, an alias shares its string for exactly
the same reason it shares its number — so this task reuses the whole of Task 1's
behaviour unchanged: the same alias resolution, the same live mirroring across rows
with an identical `PatternSetupName`, the same "shared by aliasing, continue?"
confirmation at Save time, the same Selected/"Show all items" filter, and the same
minimal-diff text patch.

It is a separate task rather than two more columns on Task 1 — the tags are edited
independently, and keeping Task 1's working match/fuzzy-match logic untouched was worth
more than putting both fields in one grid.

Two things genuinely differ from Pattern Numbers, both because the value is text
rather than an integer:

- **XML escaping.** `SaveMinimalDiff` splices the value straight into the file text, so
  a string containing `&`, `<` or `>` has to be escaped on the way in and matched in
  escaped form on the way out (`SequenceDocument.EscapeXmlText` /
  `TryPatchPatternString`). CR is escaped as well, so a value coming out of Excel can't
  silently alter the file's line endings. A PatternNumber could never contain any of
  these, which is why Task 1 needs none of it.
- **Missing element.** A sequence written by an older TrueTest has no `<PatternString>`
  at all. That is kept distinct from a present-but-empty `<PatternString></PatternString>`
  (which is what every entry in a freshly upgraded file looks like): `PatternStringRaw`
  is `null` in the first case and `""` in the second. Rows with no element show
  `(absent)`, are disabled in the grid, and are reported rather than patched on Save —
  inserting a tag into a file whose schema may not expect it is not this tool's call to
  make.

Blank "New String" means **no change**, the same rule as every other task. Since
PatternString starts out empty everywhere, the consequence is that a value can be set
but not cleared back to empty from here; clearing is deliberately out of scope rather
than giving an empty cell a second meaning.

Verified against `X4023-CB-P1_RSP_POR_DX_MATHON.seqxc` (36 items, 171 PatternSetups —
170 terminals each carrying an empty `<PatternString>`, 1 alias): setting three values,
one of them `R&D <test>`, changed exactly 3 lines out of 59,787, left every
`PatternNumber` untouched, round-tripped each value unescaped, and produced valid XML.

## Task 3: Exposure Time

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
- Bulk import from Excel is available — see **Importing from Excel**. The single
  exposure column is broadcast to Y/X/Z. (This used to say the column layout wasn't
  settled; picking the columns explicitly in the dialog is what settled it.) Follows
  the same Selected/"Show all items" filter as Task 1.

## Task 4: Luminance Scale

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
- Bulk import from Excel is available — see **Importing from Excel**. The single
  luminance column is broadcast to Red/Green/Blue, and steps whose Analysis type has
  no luminance fields are skipped.

## Saving across multiple tasks in one session

Each task's own Save button still works exactly as before (its own file
dialog, its own confirmation). On top of that, the toolbar has two buttons
for editing several tasks in one sitting:

- **Save All Changes...** — shows one file dialog, then walks every task
  that has pending edits, in order (Pattern Numbers → Pattern Strings →
  Exposure Time → Luminance Scale), calling each one's internal save (same validation/
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
each task individually, in any order, still always
patches correctly on top of whatever the previous one just wrote. Since the
four tasks edit entirely distinct XML tags (`PatternNumber` vs `PatternString` vs
`CaptureFilter`/`ExposureTime` vs `LuminanceScaleRed/Green/Blue`), there's
nothing for one task's save to make another task's *displayed* Current
values wrong either — Pattern Numbers and Pattern Strings share an *element*, but
not a tag, so their patches never collide.

An earlier version of this wired a shared "Reloaded" event so every task's
grid refreshed the instant *any* task saved individually — reverted, because
a naive full-rebuild refresh wipes out any not-yet-saved "New" edits sitting
in a tab you aren't currently looking at, which is worse than the cosmetic
staleness it was solving. The "Save All Changes" button above is the correct
place for a unified refresh, since by definition nothing is left pending to
lose at that point.

## Selected vs unselected steps

Every task's grid lists only the sequence items with `<Selected>true` until **"Show all
items (incl. unselected)"** is ticked. With it on, the unselected steps are shown
**dimmed (55% opacity) and italic**, so a grid mixing the two never reads as one
uniform list.

Two cues rather than one on purpose: colour alone would be lost on a dim panel or to a
colour-blind reader, and italic survives both. Opacity rather than a muted foreground,
so the Capture checkboxes in Exposure Time dim along with the text; applied per cell
(`DataCellStyle` in `App.xaml`), which leaves a dirty row's yellow highlight at full
strength underneath. Luminance Scale's `NewCellStyle` is `BasedOn` it, or its New cells
would stay bright on an otherwise dimmed row.

Note the dimming marks a step that is **switched off in the sequence**, not one that
can't be edited — Luminance Scale greys the *cell background* separately for steps whose
Analysis type has no luminance fields. The two can appear together.

## Editing in the grid

`DataGridEditHelper.cs` gives all four grids the same cell-editing behaviour. Every
path through it writes only into bound properties whose name starts with `New`, so a
rectangular selection spanning a read-only "Current" column can never overwrite it.

| Key | On a selected cell | Inside a cell editor |
|---|---|---|
| **Backspace** | deletes **one character**, leaving the editor open | normal Backspace |
| **Delete** | clears every selected New cell **immediately**, no Enter needed | forward delete |
| **Ctrl+V** | pastes a range (below) | normal paste |
| **Ctrl+Z** | undoes the last paste or Delete | the editor's own undo |

**Backspace** needs the help because the DataGrid opens an editor with the whole value
*selected*, so an untouched Backspace wipes the entire cell and leaves the edit pending
a commit. The helper opens the edit, drops the selection, puts the caret at the end and
removes exactly one character — deferred to `Input` priority, since the editing TextBox
does not exist until the DataGrid has prepared the cell.

**Delete** writes straight to the row instead of opening an editor, which is why it
needs no Enter, and clears the whole selection as one undo step. The grids set
`CanUserDeleteRows="False"`: at its default of true the DataGrid would also try to
delete the row, dropping a step out of the view.

**Ctrl+Z** covers the bulk edits — paste and Delete — up to 25 steps, newest first, and
reverts a batch through the same property setters so the dirty highlight recalculates
normally. A single typed cell is deliberately *not* on the stack: Esc already abandons
an edit in progress, and the editor has its own undo while open.

### Copy/paste of a range

This is the **grid's own** Ctrl+C/Ctrl+V between cells, unrelated to importing a sheet —
the paste-a-range-into-a-text-box import that Tasks 1 and 2 used to have is gone,
replaced by "Load .xlsx...":

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
  `NewBlue`/`NewAll` (string), and Pattern Strings' `NewPatternString` (string),
  without per-task-specific paste code.

## Shared pieces under `Tasks/`

- `ExcelImportWindow` — the sheet/column picker, used by every task.
- `ResolveMatchWindow` — the match picker. It started inside `PatternNumberTask` and
  moved up here once all four tasks used it.
- `NameMatch` — the candidate ranking. Was a private copy in the two pattern tasks;
  shared rather than let four copies of a scoring rule drift apart.
- `DataGridEditHelper` — the grids' cell editing: paste, Backspace/Delete, undo.
  (Lives at the project root, not under `Tasks/`.)
- `SaveResult`, `ITaskModule` — the task contract.

## Adding a new task

1. New folder under `Tasks/YourTask/` with a `UserControl` (view) + view-model.
2. Implement `ITaskModule` (`TaskName`, `View`, `OnDocumentLoaded(SequenceDocument)`).
3. Register it in `MainWindow.RegisterTasks()`.

That's it — it shows up in the sidebar automatically and receives the shared
`SequenceDocument` whenever a file is opened.
