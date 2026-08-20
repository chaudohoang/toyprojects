@echo off
REM ===========================================================================
REM ResetTestSet5000.bat - large-scale test population.
REM
REM Builds 5000 panels (~125,000 queue files, 1 KB dummy images) to check the
REM tool behaves at the size of a real backlog. Takes about 7 minutes.
REM
REM For everyday testing use ResetTestSet.bat instead - 500 panels, ~30 seconds.
REM ===========================================================================
call "%~dp0ResetTestSet.bat" 5000 1
