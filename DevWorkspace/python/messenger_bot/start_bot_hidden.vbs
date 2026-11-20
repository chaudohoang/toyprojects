Set fso = CreateObject("Scripting.FileSystemObject")
currentDir = fso.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = currentDir
' Chạy lệnh kích hoạt venv và khởi động bot dưới dạng ẩn (0)
WshShell.Run "cmd /c .\.venv\Scripts\activate.bat && python bot.py", 0, False

' Hiển thị thông báo popup cho người dùng biết
MsgBox "OK bot da duoc chay ngam thanh cong! Sep co the chat voi bot tren Telegram.", 64, "Tro ly Pika"
