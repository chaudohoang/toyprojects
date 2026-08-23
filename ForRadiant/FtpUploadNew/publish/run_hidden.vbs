' Launches run_watchdog.bat with no visible console window.
' Task Scheduler runs this instead of the .bat directly, so the operator
' never sees a black window sitting on the desktop.
Dim fso, shell, here
Set fso = CreateObject("Scripting.FileSystemObject")
Set shell = CreateObject("WScript.Shell")
here = fso.GetParentFolderName(WScript.ScriptFullName)
shell.Run """" & here & "\run_watchdog.bat""", 0, False
