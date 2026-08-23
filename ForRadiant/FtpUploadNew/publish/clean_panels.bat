@echo off
REM ==========================================================================
REM  Double-click to remove all synthetic TEST panels (TSN<digits>_<stamp>)
REM  from the queue + ingest backup, and wipe the scratch source tree.
REM  Real production panels are never matched.
REM
REM  From a terminal:
REM    clean_panels.bat            clear test panels + scratch
REM    clean_panels.bat -Full      ALSO wipe jobs/logs/state (all day history)
REM    clean_panels.bat -DryRun    preview only, delete nothing
REM ==========================================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean_panels.ps1" %*
