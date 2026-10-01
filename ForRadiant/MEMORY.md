<!-- AI INSTRUCTIONS
When reading this file:
  - Also read all files in the `memory/` folder for detailed session logs.
When updating this file:
  - Also create or update `memory/YYYY-MM-DD.md` for today's session log.
  - MEMORY.md = high-level summary (keep concise).
  - memory/YYYY-MM-DD.md = detailed work log for that day.
-->

# ForRadiant Solution Memory

> **Last updated:** 2026-05-06
> **Workspace:** `e:\Github\toyprojects\ForRadiant`
> **Solution:** ForRadiant.sln (Visual Studio 2022, v17)
> **Configurations:** Debug / Release × (Any CPU, x64, x86)

---

## What Is This

60+ independent utility/tool projects for **Radiant Vision Systems** workflows (TrueTest display inspection platform). All **.NET Framework classic** (no .NET 5/6+). **No inter-project dependencies** — every project is a standalone app.

---

## Solution Snapshot

| Category | Count | Examples |
|---|---|---|
| C# WinForms | ~26 | AOITrace, EZAE, PanelFFC, TCPClient/Server, TTLogViewer |
| C# Console | ~7 | BackupCurrentTT, KillProcess, StartProcess |
| VB.NET WinForms | ~22 | AutoBackupData, TrueTestWatcher, SequenceCheck, PanelFFC variants |
| C++ | 2 | DynamicDLLWrapper, MatlabWrapperGenerator |
| QuickTools (solution folder) | 22 | Small utilities, mostly C# console |

---

## Complete Project Catalog

### C# WinForms (~26)
AOITrace, AutoClickOnLog, AutoClose, CopySequenceStep, CSVPrefixchange, CustomPopup, EZAE, FFCDBGenerate, FileHide, FolderLock, FTPIndexPathGenerator, IPListBuilder, MakeSequenceFOI, OneTimeRunner, PanelFFC, ProjectCloner, RemoteTools, RVSWorklog, SequenceCheckCS, SequenceClearer, SetSequence, SetZone3, TCPClient, TCPServer, TextToImage, TTLogViewer

### C# Console (~7)
BackupCurrentTT, CppProjectCloner, KillProcess, MatchingSummaryLogForAOIDetect, StartCopyWhizFixtureToCamera, StartProcess, StartTightVNC

### VB.NET WinForms (~22)
AutoBackupData, AutoCopyData, AutoDeleteData, CopyMaster, CSVAverage, CSVNormalization, DoubleResolution, Emu2p1SequenceConvert, FOIsetCopyTool, FTPUploaderVB, JoinPanelID, LGDCustomAppLauncher, LgdcropSimulator_Cpp, LgdcropSimulator_Net, MotherGrayGenerator, PanelFFC D854 Color, PanelFFC GRAY, PanelFFC Illunis CF/DJ/EP, PanelFFC Radiant CF, PanelFFC RGB, PanelFFC X1080 1/3/5 panels, RestartTTDove2p0, SequenceCheck, SetSequenceVB, TrueTestWatcher, UploadFTPVB

### C++ (2)
DynamicDLLWrapper (main.cpp + parse_header), MatlabWrapperGenerator (Main.cpp)

### QuickTools Solution Folder (22 sub-projects)
BinXMLChangeIntercept, BlockFiles, Block_Unblock_Files, ClearVignettingINI, Copy all to POCB4.1Net folder, Distribute POCB4.1Net Copier, Distribute files, EnableDisable saving synthetic, FileLister, FtpUploadQeueModify, POCBMobileWin7CompatibilityCheck, RemoveEZAEAddons, RemoveFFSPOT, RenameFiles, RenameVNTT, RunTestOmit (VB), RunTestPUC (VB), SequenceMeasurementSetupCleaner, SetMaxNewBrightDefects, UnblockFiles, VignetingOff, VignetingOn

---

## Standard Project Structure

```
ProjectName/
├── App.config                    # .NET config
├── ProjectName.csproj/.vbproj    # Project file
├── Program.cs/.vb                # Entry point
├── MainForm.cs/.vb               # Main form (WinForms)
├── MainForm.Designer.cs/.vb      # Designer code
├── MainForm.resx                 # Resources
├── NativeMethods.cs/.vb          # P/Invoke (some projects)
├── *.ico                         # App icon
├── Properties/ or My Project/    # C# vs VB.NET metadata
├── bin/                          # Build output
└── obj/                          # Build intermediates
```

---

## Key Patterns & Details

### Dual-Language Variants
Same tool implemented in both languages:
- SequenceCheck (VB) ↔ SequenceCheckCS (C#)
- SetSequence (C#) ↔ SetSequenceVB (VB)

### 10 PanelFFC Variants
Camera-specific calibration tools, code-cloned:
PanelFFC, PanelFFC D854 Color, PanelFFC GRAY, PanelFFC Illunis CF, PanelFFC Illunis DJ, PanelFFC Illunis EP, PanelFFC Radiant CF, PanelFFC RGB, PanelFFC X1080 1 panel, PanelFFC X1080 3 panels, PanelFFC X1080 5 panels

### Security Pattern (Password Forms)
FileHide, FolderLock, CopyMaster, TextToImage

### Admin Manifests (Require Elevation)
EZAE, BackupCurrentTT, KillProcess, CopyMaster, FolderLock, TrueTestWatcher

### NativeMethods / P/Invoke (~8 projects)
Projects that use Win32 API interop via NativeMethods classes.

### Largest Files
| File | Size |
|---|---|
| TrueTestWatcher MainForm.vb | 151 KB |
| AutoDeleteData MainForm.vb | 115 KB |
| SequenceCheckCS MainForm.cs | 105 KB |

### Shared Directories
- `bin/` — shared build output
- `dll/` — shared DLLs
- `packages/` — NuGet package cache

### Notable Exceptions
- **FOIsetCopyTool** — the only SDK-style VB project (newer csproj format)
- **GenericWrapper** — has its own `.sln`, not part of the main ForRadiant solution

---

## Session Log

| Date | Summary |
|------|---------|
| 2026-05-06 | Began `FTPUploaderVB` modernization by creating `FTPUploaderCS`, a C# WinForms .NET Framework 4.8 conversion with code-only UI and extracted upload services. Verified `dotnet build FTPUploaderCS\FTPUploaderCS.csproj` succeeds. See `memory/2026-05-06.md` |
| 2026-05-05 | Initial solution study. Cataloged all 60+ projects, identified patterns. See `memory/2026-05-05.md` |
