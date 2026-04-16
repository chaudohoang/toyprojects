@echo off
title Fixture Company Server Simulator

set DEBUG_EXE=Fixture_Company_Server_Simulator\bin\Debug\Fixture_Company_Server_Simulator.exe
set RELEASE_EXE=Fixture_Company_Server_Simulator\bin\Release\Fixture_Company_Server_Simulator.exe

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
echo    Fixture_Company_Server_Simulator\Fixture_Company_Server_Simulator.vbproj
echo.
pause
