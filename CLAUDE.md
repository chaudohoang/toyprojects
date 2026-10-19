# Github root

This file is tracked in `toyprojects`, so it travels to every PC with a `git pull`.

`<root>` below means **the folder `toyprojects` is cloned into** — `E:\Github` on Chau's
main PC. Everything outside `toyprojects` is described relative to `<root>`, so a PC that
clones somewhere else needs no edit here.

Two paths are genuinely machine-specific and ARE worth checking on a new PC:

- the deploy folder in the table below (`E:\PortableApps\MultiRemoteTool`)
- `C:\Program Files\...` paths inside the tool INIs, which depend on what is installed

Note this file only loads automatically for a session started inside `toyprojects`. Working
at `<root>` itself — on the upstream clones — won't pick it up.

# What lives under `<root>`

| Folder | Remote | What it is |
|---|---|---|
| `toyprojects` | `chaudohoang/toyprojects` | Chau's own repo. Dozens of small standalone tools, one folder each. |
| `YgoMaster` | `pixeltris/YgoMaster` | Upstream clone, not Chau's. |
| `Yu-Gi-Oh-Forbidden-Memories-Recompiled` | `Unchiga/…` | Upstream clone, not Chau's. |
| `YuGiOhForbiddenMemoriesRecomp` | `Unchiga/…` | Upstream clone, not Chau's. |

Only `toyprojects` is Chau's to commit and push to. The other three are upstream clones —
read them, build them, but don't push.

Unqualified "the repo" means `toyprojects\ForRadiant`. That subtree has **its own
`CLAUDE.md`, which is canonical for anything inside it** — read it before working there;
nothing in this file overrides it.

# Repo conventions

- **Build output is committed on purpose**, in `ForRadiant` and elsewhere in
  `toyprojects` (e.g. `MultiRemoteTool.exe` and the bundled `tvnviewer.exe` /
  `vncviewer.exe` are tracked). A line PC pulls a ready-to-run binary with no build step.
  **Do not propose `.gitignore` rules for `bin`/`obj`, `git rm -r --cached`, or a history
  rewrite** — that was suggested once and declined.
- When committing a code change, **rebuild first** so the committed exe matches the
  committed source, and stage both.
- `toyprojects` keeps a single `main` branch. Don't create feature branches; commit to
  `main`. (A stale `ftprecovery-no-duplicate-manifests` was deleted 2026-10-09.)

# Tools with a separate deploy folder

Some tools are *used* from a folder outside the repo, which holds its own live data. A
change isn't finished until both sides are updated, and the deploy copy's data files must
not be overwritten with the repo's.

| Tool | Deploy folder | Diverges from the repo copy by |
|---|---|---|
| `toyprojects\MultiRemoteTool` | `E:\PortableApps\MultiRemoteTool` | Has a `Radmin` platform the repo lacks; its own contact CSVs (real hosts); its own `settings.txt` ordering. |

Sync the exe, the INIs and bundled viewers; leave `contacts_*.csv` alone on each side.

**`MultiRemoteTool` gotcha:** running the app rewrites `settings.txt` (it saves the
selected platform as the first entry in `LoadedPlatforms`). After launching it to test,
check `git status` and `git checkout --` that file if the change wasn't intentional.

# Work log

Keep a running log in `WORKLOG.md`, next to this file at the root of `toyprojects`, newest
day first, one section per working day (`## YYYY-MM-DD`).

Append to it without being asked, whenever a piece of work concludes — a feature shipped,
a bug diagnosed, something ruled out. Record what changed and where (commit hashes, file
paths) plus anything expensive to rediscover: why something is built the way it is, what
was investigated and eliminated, measurements taken, traps for next time. Skip what git
already says well — this is the reasoning that commit messages don't carry. A few lines
per item. If a day produced nothing substantive, say so rather than leaving the day out.

`WORKLOG.md` is tracked and pushed, so the other PC sees the latest work on `git pull`.
**Commit it with the work it describes**, not as a separate housekeeping commit — then the
log and the code it refers to arrive together.
