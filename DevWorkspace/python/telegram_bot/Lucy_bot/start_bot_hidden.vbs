Set fso = CreateObject("Scripting.FileSystemObject")
currentDir = fso.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = currentDir

' Kiểm tra xem bot đã chạy chưa
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

If isRunning Then
    MsgBox "Hien tai Thu ky Lucy dang chay ngam roi sep oi! Khong can bat lai dau a.", 48, "Thu ky Lucy"
    WScript.Quit
End If

' Chạy lệnh kích hoạt venv và khởi động bot dưới dạng ẩn (0)
WshShell.Run "cmd /c .\.venv\Scripts\activate.bat && python bot.py", 0, False

' Hiển thị thông báo popup cho người dùng biết
MsgBox "OK bot da duoc chay ngam thanh cong! Sep co the chat voi bot tren Telegram.", 64, "Thu ky Lucy"
