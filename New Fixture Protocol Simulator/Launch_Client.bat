@echo off
title Radiant TrueTest Client Simulator

set DEBUG_EXE=Radiant_TrueTest_Client_Simulator\bin\Debug\Radiant_TrueTest_Client_Simulator.exe
set RELEASE_EXE=Radiant_TrueTest_Client_Simulator\bin\Release\Radiant_TrueTest_Client_Simulator.exe

if exist "%DEBUG_EXE%" (
    start "" "%DEBUG_EXE%"
    exit
)

if exist "%RELEASE_EXE%" (
    start "" "%RELEASE_EXE%"
    exit
)

echo.
echo  ERROR: Executable not found.
echo  Please open and build the project in Visual Studio first:
echo    Radiant_TrueTest_Client_Simulator\Radiant_TrueTest_Client_Simulator.vbproj
echo.
pause
