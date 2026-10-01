# ForRadiant

~60 standalone utility/tool projects for Radiant Vision Systems workflows (TrueTest
display inspection). Mostly **.NET Framework classic**, no inter-project dependencies —
every project is its own app. Solution: `ForRadiant.sln` (VS2022+).

This folder is Chau's main working area under `E:\Github`. "The repo" or "this project"
means here, not the game ports elsewhere in `E:\Github`.

## Repo conventions (important)

- **Build output is committed on purpose.** `bin/`, `obj/`, exe, pdb and generated
  `.g.cs`/`.baml` are tracked, so a line PC can pull a ready-to-run binary without a
  build step. The 1.3 GB `.git` and the untracked build dirs are known and accepted.
  **Do not propose adding obj/bin rules to `.gitignore`, `git rm -r --cached`, or a
  history rewrite** — that was suggested once and declined.
- When committing, **rebuild first** so the committed exe matches the committed source,
  then include the build artifacts the change produced. Don't stage source only.
- One real exception: `obj/**/sourcelink.json` and `project.assets.json` embed
  machine-specific SDK paths and conflict between machines. Harmless, just noise.

## Building

`build.bat` in each project finds MSBuild via `vswhere` (VS2022/2019 fallbacks). It ends
with `pause`, so it can't be scripted — invoke MSBuild directly instead:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe' `
    <Project>.csproj /restore /p:Configuration=Release /t:Rebuild /nologo /m /v:minimal
```

If the app is running it locks `bin\...\*.exe` and the build fails at the **copy** step
(MSB3027) even though compilation succeeded. Build to `/p:OutputPath=bin\_verify\` to
check compilation without closing it.

## Verification habit

**A green build does not mean the app works.** WPF binding errors and XAML resource
problems are silent at compile time. A change that built cleanly once shipped a
startup-fatal stack overflow here. After any UI change: launch the exe, confirm a real
`MainWindowTitle`, and check the Windows Application event log for new `APPCRASH`
records — a crashing process can linger for seconds while WER writes its dump, so
"still running" is not proof on its own.

## Active work

### SeqxcToolset — task-plugin editor for `.seqxc` TrueTest sequences

Current focus. WPF, .NET Framework 4.8, SDK-style csproj, **zero NuGet dependencies**
(xlsx reading is hand-rolled on `System.IO.Compression` + `System.Xml.Linq`; keep it
that way). Four tasks: Pattern Numbers, Pattern Strings, Exposure Time, Luminance Scale.

**`SeqxcToolset/README.md` is the detailed reference** — file-format mapping, the
minimal-diff save strategy, per-task matching rules, the xlsx reader's limits, settings.
Read it before changing anything there; it also records approaches that were tried and
reverted, with reasons.

Settings live in `%APPDATA%\SeqxcToolset\settings.xml` (last sequence — reopened on
start, last workbook, and each task's import columns stored **by header text**, never by
index).

**Open question, unresolved:** the rule sheet
`D:\Log\20260909 X402x model New Pattern index rule.xlsx` (use **Sheet2**) names patterns
`W48`/`W192`/`WR192`, while `X4023-CB-P1_RSP_POR_DX_MATHON.seqxc` holds
`g48`/`r48`/`b48`/`g192`. Only `CalG`/`CalR`/`CalB` match — 3 of 23 rows. `W48` is *not*
simply all three channels (sheet: one L/v of 10.03; sequence: 4.958 / 2.662 / 4.008).
**Chau resolves this by hand** through the match picker; don't try to infer a rule.

Also unexercised by any automated check: the Excel import dialog, the match picker, and
the remembered-column restore. Logic is tested headlessly (drive the compiled types from
Windows PowerShell 5.1 — `[Reflection.Assembly]::LoadFrom` on the exe; `Add-Type -Path`
rejects `.exe`, and pwsh 7 can't host a net48 assembly).

### FtpUploadNew — .NET 8 + WPF background uploader

Replaces `FTPUploaderVB` (live) and `FTPRecovery`. ~10k LOC, its own README. Not under
active change as of 2026-10-01.

## Archived notes

`MEMORY.md` and `memory/2026-05-*.md` in this folder predate the above (last touched
2026-05-06, before `FtpUploadNew` and `SeqxcToolset` existed). Historical only — don't
treat them as current. `.gemini-knowledge/` is another tool's store; left to go stale in
favour of this file.
