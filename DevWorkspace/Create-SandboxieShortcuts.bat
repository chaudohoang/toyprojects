@echo off
setlocal

set "SANDMAN=C:\Program Files\Sandboxie-Plus\SandMan.exe"
set "DESKTOP=%USERPROFILE%\Desktop"

set BOXES=DefaultBox Sandbox1 Sandbox2 Sandbox3 Sandbox4 Sandbox5 Sandbox6 Sandbox7 Sandbox8 Sandbox9

for %%B in (%BOXES%) do (
    powershell -NoProfile -Command ^
        "$ws = New-Object -ComObject WScript.Shell;" ^
        "$s = $ws.CreateShortcut('%DESKTOP%\Browser - %%B.lnk');" ^
        "$s.TargetPath = '%SANDMAN%';" ^
        "$s.Arguments = '/box:%%B default_browser';" ^
        "$s.WorkingDirectory = 'C:\Program Files\Sandboxie-Plus';" ^
        "$s.Description = 'Open default browser in %%B';" ^
        "$s.IconLocation = '%SANDMAN%,0';" ^
        "$s.Save();"
    echo Created: Browser - %%B.lnk
)

echo.
echo Done! All shortcuts created on the Desktop.
pause
