Set fso = CreateObject("Scripting.FileSystemObject")
currentDir = fso.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = currentDir

Set objWMIService = GetObject("winmgmts:\\.\root\cimv2")
Set colProcesses = objWMIService.ExecQuery("Select * from Win32_Process Where Name = 'python.exe'")
isRunning = False
For Each objProcess in colProcesses
    If Not IsNull(objProcess.ExecutablePath) And Not IsNull(objProcess.CommandLine) Then
        If InStr(objProcess.ExecutablePath, "Lucy_bot") > 0 And InStr(objProcess.CommandLine, "bot.py") > 0 Then
            isRunning = True
            Exit For
        End If
    End If
Next

If Not isRunning Then
    WshShell.Run "cmd /c .\.venv\Scripts\activate.bat && python bot.py", 0, False
End If
