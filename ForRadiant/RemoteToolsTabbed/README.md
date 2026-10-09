# RemoteToolsTabbed

Same job as `RemoteTools` — pick a machine from a CSV and open a TightVNC session — but
the machine list is a **permanent sidebar** and every session is docked as a **tab** in
the pane beside it, instead of a separate `tvnviewer` window per machine. Several CSVs
can be loaded at once; see [Several machine lists](#several-machine-lists).

```
+-------------------+--------------------------------------+
| COMPUTERS (426) [Hide]| [ loopback x ] [ 301#4 x ]       |
| search            +--------------------------------------+
| PC          IP    |                                      |
| v All   421       |        the selected session,         |
|   ...             |        filling the pane              |
| > LineA   3    x  |                                      |
| (ping output)     |                                      |
| [Remote Ctrl]     |                                      |
| [+Machine][+List] |                                      |
| [Expand][Collapse]|                                      |
+-------------------+--------------------------------------+
| status                                    2 sessions open |
+-----------------------------------------------------------+
```

The sidebar stays put while you work, so you can open the next machine without leaving the
session you are on. `Ctrl+B` (or **Hide** in its header) folds it away to a thin strip
when a session needs the whole window; the open sessions re-fit themselves when it does.

The list has four states:

| state | how | what it does |
|---|---|---|
| **Docked** | default, `Ctrl+B` | a column of the main window, resizable by the splitter down to 260 px |
| **Collapsed** | `Ctrl+B` | a thin strip; the session takes the rest (1239px -> 1550px at 1600px wide) |
| **Flyout** | click the strip | slides out *over* the session and tucks away when it loses focus - Visual Studio's auto-hide. The session never moves |

The flyout is a **real top-level window**, not an overlay inside the main one. It has to be:
a docked session is a child HWND belonging to tvnviewer, and a window handle always paints
over WPF content, so an in-window overlay would vanish behind the session the moment one is
connected.

Two details that are easy to get wrong if this is ever rewritten:

* A flyout that opens while the app is **not** the foreground app gets a `Deactivated` the
  instant it appears, and auto-hides again before it is ever seen. `SidebarWindow` ignores
  deactivation for the first 500 ms for exactly that reason.
* A message box raised from the list (Save Lists, File Share) also deactivates the flyout.
  Hiding then would strand the dialog, so a `#32770` of this process in the foreground does
  not count as losing focus.
* The sidebar's minimum width belongs on the **`ColumnDefinition`**, not on the `Border`
  inside it. The splitter resizes the column, so a minimum on the Border only lets the
  column shrink underneath and clip the buttons and the IP column. It is applied while
  docked and lifted to 0 while collapsed, otherwise the 34 px strip could not exist.
* Auto-hide **hides** the window and defers it to `DispatcherPriority.Background`; it must
  not `Close()` it from inside `Deactivated`. Destroying the active window in the middle of
  an activation change drops focus out of the application altogether. The window is created
  once and reused.
* **The title, the machine count and the gear live in `ComputersView`, not in the window
  around it.** They used to sit in the main window's sidebar, above the host that the list
  is plugged into - so sliding the list out left them behind, and the flyout had no count
  and no viewer-options button at all. Only **Hide** is still the host's business, and the
  view hides it (`ShowCollapseButton`) while the list is slid out, where **Dock** does that
  job - the two are labelled rather than glyphs so they read as the pair they are.
* **The collapsed strip is one click target, and it acts on the press.** It used to carry a
  chevron *button* as well, a second target doing the same job - and doing it differently.
  A button fires on the release, and the flyout's deferred auto-hide gets its turn in the
  gap between press and release, so the click only ever saw a list that had already tucked
  itself away and opened it straight back up: the strip toggled, the arrow always opened.
  The chevron is now a glyph, and the whole strip is handled on `MouseLeftButtonDown`,
  which beats the auto-hide to the decision.
* The `#32770` check above has to be made **in the deferred callback, not in the handler**.
  A message box deactivates the flyout *before* it becomes the foreground window, so asking
  any earlier only ever sees the window that is going away - the flyout tucked itself away
  underneath its own dialog. On top of that, `AutoHideSuspended` holds the flyout open for
  the length of the viewer-options run, which puts up a dialog *and* then hands the focus
  to tvnviewer. That path only became reachable when the gear moved into the list.

WPF, .NET Framework 4.8, SDK-style csproj, no NuGet dependencies.

## How the tabbing works

There is no VNC code here. `tvnviewer.exe` does the VNC work, launched as
`tvnviewer.exe <address> -warnfullscr=no`; once its window appears it is
re-parented into a tab:

1. `NativeMethods.FindMainWindows("tvnviewer")` snapshots the viewer's top-level windows
   before launching, then polls (150 ms, up to 60 s) for a window that wasn't there before.
   Dialogs are skipped by class — TightVNC uses `#32770` for its connection, authentication
   and error boxes, so only the real viewer window is picked up. A set of already-claimed
   windows is shared between sessions so two connections started at once can't grab the same one.
2. `ExternalWindowHost` (an `HwndHost`) creates a plain child "holder" window and re-parents
   the viewer into it: strip `WS_POPUP | WS_CAPTION | WS_THICKFRAME | WS_*BOX`, add `WS_CHILD`.
   The holder exists from the moment the tab is created, which is what lets the
   "Connecting ..." state live in the same tab.
3. Keyboard focus is handed over with `AttachThreadInput` + `SetFocus` — the viewer runs on
   another thread, so a plain `SetFocus` would be ignored.

Closing a tab un-parents the window, restores the original styles and posts `WM_CLOSE`; the
process is only killed if no window ever appeared. One `tvnviewer` process can own several
windows, so killing it would take other sessions down with it.

A 1 s watchdog drops tabs whose window disappeared on its own (disconnect, viewer quit,
crash). *Pop out to own window* on the tab's right-click menu gives a session back its
frame and leaves it running as an ordinary window.

### Two WPF-specific traps, both handled

* **`TabControl` destroys the content of unselected tabs.** The stock control swaps a single
  `ContentPresenter` between tabs, so leaving a tab would unload the host and drop the
  session. `TabControlEx` instead keeps one presenter per tab in a `PART_ItemsHolder` grid
  and only toggles `Visibility`. This is the reason the control exists — don't "simplify" it
  back to a plain `TabControl`.
* **`HwndHost` only re-places its window when the element's visibility changes**, not on an
  ordinary layout pass, so a window resize would leave the session at its old size until the
  next tab switch. `ExternalWindowHost` drives `UpdateWindowPos()` from `SizeChanged` /
  `IsVisibleChanged` itself.

`TabControlEx` also publishes its tab content through a custom `AutomationPeer`: the stock
`TabControlAutomationPeer` only exposes content it finds in a template part named
`PART_SelectedContentHost`, which this template does not have, and without the peer the
entire contents of every tab are invisible to UI automation and assistive tech.

## Several machine lists

`All.csv` next to the exe is the **default list** and is always loaded. **+ Add list**
loads another CSV beside it, and every loaded list is **merged into the one grid**, each
under **its own header band** — the file name, how many machines it is showing, and an
`x` that unloads it. One search still covers all of them, and it matches the list name
too, so typing `LineA` narrows the grid to that list.

The band of the list you are scrolling through **stays pinned** to the top of the grid
once its own band has gone past, so the file name is on screen whatever you have scrolled
to. The pinned copy is the band: clicking it folds the list, and its `x` unloads the list.

Click a band to fold that list away; **Expand all** / **Collapse all** do the lot. A
collapsed list stays collapsed while you work, but it is **opened again as soon as a
filter matches something inside it** — a search must never leave a match hidden behind a
closed header. Lists with nothing to show drop out of the view entirely, so the bands you
can see are the ones that still have matches.

The grid is **always grouped**, including when `All.csv` is the only list loaded: one band
reading `All  421` answers "which file is this row from" the same way three bands do, and
it keeps **Expand all** / **Collapse all** meaning the same thing however many lists are
open. `All.csv` is the default list, so its band never gets an `x` - there is nothing to
unload.

An earlier version put a `List` column in the grid instead. The bands replaced it: the
column repeated the same word down every row of a list and still left you counting rows
to see where one file ended and the next began.

| | |
|---|---|
| **lists.txt** | next to the exe, one full CSV path per line, `;` or `#` comments. Rewritten whenever a list is added or removed. |
| **All.csv** | never written to `lists.txt`. It is always first and always loaded, so the default list cannot be lost by deleting or mangling `lists.txt`. |
| **A list whose file is gone** | dropped at start-up, `lists.txt` is rewritten without it, and the sidebar says so. It does not come back. |
| **the `x` on a band** | unloads that list and forgets it. **The CSV file itself is not deleted**, so re-adding it is one click — which is why this does not ask first. |

### Adding a machine

**+ Add machine** puts an empty row at the end of **the list being worked in** and opens it
for typing. That is the list of the last row selected; with nothing selected yet it is the
default list. **Save Lists** then writes it to that list's own CSV. A row that was never
filled in is dropped on save rather than written as a blank line.

Which list that is, is **remembered in a field, not read from the grid's selection**, and
this is not a detail that can be simplified away. Adding a row leaves the grid with no
selection, no current item and no current cell at all - and the row being typed stays
editable regardless, because editing follows the current cell rather than the selection,
which hides the problem. Reading `SelectedItem` at the moment of the click therefore
worked for the first row and returned nothing for every row after it, so the second and
later rows silently went to `All.csv` however deep in another list you were. Adding also
must **not** call `_view.Refresh()`: an `ObservableCollection` add already drops the row
into the right group, while a refresh resets the view and is itself one of the things that
drops the selection.

The grid's own built-in new-item row is switched off (`CanUserAddRows="False"`). With the
lists grouped it rendered **once, at the very bottom, under every group** - not one per
list - and whatever was typed there joined `All.csv` whichever list you were working in.
Both are inherent to how WPF places `NewItemPlaceholder` in a grouped view, which is why
the row is gone and a button took its place.

### Three traps in the grouped grid, all handled

* **A `ToggleButton` placed in a `GroupItem` template never sees the mouse.** The first
  version of the band was a templated `ToggleButton` filling the header, bound two-way to
  the list's expanded state. It rendered correctly and did nothing at all — no click, not
  even a hover highlight — while the `x` button sitting beside it in the same `Border`
  worked. The band is now a plain `Border` with a `MouseLeftButtonUp` handler and a
  chevron `TextBlock`, and the expanded state is flipped in code. **Don't "tidy" it back
  into a ToggleButton or an Expander.**
* **Grouping switches row virtualization off.** A `DataGrid` only keeps virtualizing when
  grouped if `VirtualizingPanel.IsVirtualizingWhenGrouping` says so, and realising all 421
  rows of `All.csv` at once is slow enough to time out anything reading the automation
  tree. It is set to `True` on the grid.
* **What the pinned band names is hit-tested, and the band has to be taken out of the hit
  test first.** Working it out from the containers' own positions does not survive
  virtualization: a `GroupItem` reports an estimated height for the rows it has not built,
  and a recycled row keeps the position it had before it was reused, so a row scrolled well
  off the top came back as the topmost one and named the list above the right one. Two
  things about the hit test itself:
  * It must be **`InputHitTest`**, not `VisualTreeHelper.HitTest`. Only the input one obeys
    `IsHitTestVisible`, so the visual one cheerfully returned the pinned band itself - which
    then answered for the row it was covering and held itself up on the wrong list.
  * A second probe one band lower asks whether the **next** list's band has already arrived.
    If it has, the pinned copy stands down and lets the real one take the top, instead of
    stacking two bands and labelling a couple of rows it is covering.

  Its own position comes from the column headers at run time, and every reading is taken on
  a dispatcher callback, never in the scroll handler - scrolling realises rows that have not
  been arranged yet.
* **A double-click anywhere in the grid used to open a session.** The handler only tested
  that *something* was selected, and a row is always kept selected, so a double-click on a
  band — easy while folding a list away — connected to whatever happened to be selected.
  It now requires the click to land on a `DataGridRow`, found by walking up from the
  **hit-tested point**: a double-click puts the cell into edit mode first, which pulls
  `e.OriginalSource` out of the visual tree before the handler runs, and
  `ItemsControl.ContainerFromElement` does not reach through the group containers either.

The expanded/collapsed flag lives on `MachineList`, not on the header. A
`CollectionViewGroup` is rebuilt on every refresh, so anything remembered on the group
itself would be lost on the next keystroke.

One thing is **not** handled: neither the band nor its `x` reaches the UI automation tree.
Stock WPF exposes a group as a named `Group` element but drops the header's own content,
and there is no supported hook to replace a `GroupItem`'s automation peer. The band is a
`Border`, so it is mouse-only — **Expand all** and **Collapse all** are the keyboard route
to the same thing, and there is no keyboard equivalent of the `x`.

**Save Lists** writes *every* loaded list back to its own CSV. A row added with **+ Add machine**
goes back to the list it was added to. Rows cannot be moved between lists from the grid;
edit the CSVs for that.

An unreadable CSV costs that one list, not the others: the rest still load and the sidebar
reports which one failed. A read-only install folder means `lists.txt` cannot be written —
the lists still work for that run, they just are not remembered.

### Viewer options, and why they used to reset

Three things about how TightVNC Viewer keeps its settings, all checked against this build:

1. **A switch on the command line beats anything stored.** So passing `-scale=auto` forced
   the scale on every connect, whatever the user had chosen.
2. **Options are stored per host**, under `HKCU\Software\TightVNC\Viewer\History\<host>`,
   with the defaults in the `History` key itself. The key is the exact string connected to,
   so settings saved against a *name* do not apply when this tool connects by IP.
3. **Only the options set before connecting are written to that store.** The same dialog
   reached from a live session's toolbar applies to that session.

So a session's settings were never guaranteed to come back. The fix is a profile:

* **`viewer-profile.vnc`** next to the exe holds the options. One ships with the tool,
  tuned for speed (see below); every session starts from it, for every machine.
* **The gear in the sidebar header** opens tvnviewer on its own. Set its options through
  *Options…*, press OK, close the viewer, and `ViewerProfile.CaptureFromViewerDefaults()`
  **overwrites** `viewer-profile.vnc` with what the viewer stored — comments and all the
  tuning below included. Use it when you want the viewer's current settings to win.

### Per-machine overrides: removed, do not re-add

There used to be a second layer - `viewer-profile.<host>.vnc` next to the exe, laid **over**
the default profile, so an old machine could get cheaper settings (256 colours, Hextile)
than a modern one. Four shipped with the tool, named after CPUs: `i5-2500`, `i5-3470`,
`i5-3570`, `i5-4670`.

It was removed, and the reason is worth keeping: `<host>` was the address **actually
connected to**. `All.csv` holds IPs, so a file named after a machine - or after its CPU -
never matched anything, and an override that matches nothing is ignored in silence with
nothing in the UI to say so. All four shipped files were dead on arrival.

If per-machine options are ever wanted again, key them off the `All.csv` row rather than
the address, and make the sidebar say when one was applied.

### What the shipped profile is tuned for, and why

| setting | value | why |
|---|---|---|
| `preferred_encoding` | 7 (Tight) | Raw (0) sends **uncompressed** pixels — a full 1920×1080 repaint is ~8 MB. Tight + JPEG sends a fraction of it. This is the first thing to check when a session feels laggy. |
| `quality` | 5 | JPEG quality 0 (poor, fastest) … 9 (best). The viewer's own default is 6. |
| `copyrect` | 1 | moved blocks are re-used instead of resent |
| `fitwindow`, `scale_*` | 0, 1/1 | **no scaling.** Scaling resamples every frame on the CPU, continuously, for each docked session. Collapse the sidebar (`Ctrl+B`) when a session needs room instead. |
| `8bit` | 0 | full colour. **Set to 1** if it is still slow — 256 colours is a big bandwidth cut. |
| `cursorshape` | 1 | track the remote cursor locally, so the pointer does not lag behind the mouse |

Values that are not obvious, all checked against this build: encodings are
`Raw=0, RRE=2, Hextile=5, Tight=7, ZRLE=10`, and local cursor shape is
`do not show=0, normal dot=1, arrow=2, small dot=3`. Note that **Raw cannot be saved** —
choosing it stores 0, which the viewer reads back as "unset" and shows as Tight, so Raw only
ever affects the session it was set in.

Other things that cost speed, in rough order: a Wi-Fi link (latency and jitter, not
bandwidth, are what VNC suffers from), several tabs open at once (every one is a live
connection still pulling updates), and the remote machine's own load. The viewer's
*Connection Information* item shows the encoding actually in use for a session.

`-optionsfile=FILE` is how the viewer reads it. That switch restores the whole connection
**including the host**, and ignores `-host=` and a bare host argument (all three forms
tested), so each session gets a copy of the profile in `%TEMP%` with its own host written
in; it is deleted as soon as the viewer has read it, and stale ones are swept at start-up.
The copy never contains a `password=` line — a stored VNC password is recoverable, and this
file would otherwise sit in the temp folder.

**`-warnfullscr=no` is the only switch ever passed**, with a profile or without one. It
suppresses a popup; it is not a stored setting, so it overrides nothing, and nothing else
belongs beside it.

`-scale=auto` used to be passed whenever there was *no* profile. Read against point 1
above that is self-defeating: no profile is exactly the state you are in after deleting
one to go back to the viewer's own per-host settings, and the forced scale quietly undid
them - so the one documented way of using the viewer's own options was the one case that
could not work. Options set through the gear then looked as though they were ignored,
which is what this folder-without-a-profile looks like from the outside.

An unknown switch makes tvnviewer stop with *"Error in format of command line"* and never
open a window, so the tab would just sit at *Connecting…* until it times out. Check any
addition against `tvnviewer -help` first.

## Using it

| | |
|---|---|
| Open a session | double-click a row, `Enter`, or **Remote Control** |
| Several at once | select several rows, then **Remote Control** |
| Ad-hoc address | type it in the search box and press `Enter` |
| Close a session | the `✕` on the tab, middle-click the tab, `Ctrl+W`, or the tab menu |
| Jump to the search box | `Ctrl+F` (re-opens the sidebar if it is folded away) |
| Show / hide the sidebar | `Ctrl+B`, or **Hide** in its header |
| Slide the list out over a session | click the collapsed strip; click away to hide it |
| Put it back in the window | *Dock* in the flyout, or `Ctrl+B` |
| Viewer options for every session | the gear in the sidebar header |
| Add a machine | **+ Add machine** - it joins the selected row's list |
| Load another CSV | **+ Add list** (several can be picked at once) |
| Fold one list away | click its header band |
| Fold / unfold every list | **Collapse all** / **Expand all** |
| Unload one | the `x` on its header band |
| Save every loaded list | **Save Lists** |

A row is always kept selected (the top match after filtering), so typing a filter and
pressing `Enter` opens the machine you just searched for. Selecting a machine that is
already open switches to its tab instead of starting a second viewer.

## Differences from `RemoteTools`

* **Ping** uses `System.Net.NetworkInformation.Ping` instead of shelling out to `ping.exe`.
  The old version killed *every* `PING` and `conhost` process on the machine to stop; this
  one just cancels. Replies are green, failures red (the old random per-line colours are gone).
* **Save Lists** writes the whole collection, not the grid. Saving while the list was
  filtered used to drop every machine that was filtered out, and it wrote a `pc,ip,` header
  row that came back as a fake machine on the next load. It also trims stray trailing spaces.
  With several CSVs loaded, each row goes back to the file it came from.
* **More than one CSV** can be loaded at a time and is remembered between runs.
* The CSV loader tolerates blank lines, short rows and a leftover header row.
* The search box matches the address as well as the name.
* **Horizontal scrolling is off.** The two columns are star-sized, so they are defined to
  fit and there is nothing to scroll to. Left on, the grid kept the pixel widths it last
  measured and narrowing the sidebar left them overflowing - a scrollbar under rows that
  all fit comfortably.
* `All.csv` and `tvnviewer.exe` are read from the exe's folder, not the working directory.

## Notes

* If a host is unreachable, TightVNC puts up its own error box (a `#32770` dialog, so it
  stays a floating window) and the tab sits at *Connecting…* until it is cancelled or the
  60 s timeout passes.
* An embedded window is a real HWND, so it always paints on top of WPF content (airspace).
  Nothing is drawn over a live session except the connecting panel, which is only up while
  the host window is empty.
* The ping panel in the sidebar takes no room until something is pinged.
* The app runs system-DPI aware. On a scaled display the embedded viewer is scaled by
  Windows like any other DPI-unaware window.
* `All.csv` here is a copy; the two tools do not share a list.
* `lists.txt` is written at run time and is not part of the build — a fresh copy of the
  tool starts with `All.csv` alone.
* The bundled `tvnviewer.exe` is TightVNC **2.8.90 x64** (the installed build). The folder
  started out with 2.8.27 32-bit, inherited from `RemoteTools`; that copy is still there if
  it is ever needed.
* Delete `viewer-profile.vnc` to go back to the viewer's own per-host settings; the tool
  then passes nothing that could override them. Do not put a password in it — the file is
  committed with the build output. It is the **only** `.vnc` file the tool reads.

## Build

`build.bat`, or:

```
MSBuild.exe RemoteToolsTabbed.csproj /restore /p:Configuration=Release /t:Rebuild
```

Output: `..\bin\Release\RemoteToolsTabbed\` with `All.csv` and `tvnviewer.exe` copied next
to the exe. Any extra CSVs are referenced by absolute path from `lists.txt`, so they can
live anywhere — they are not copied into the build.
