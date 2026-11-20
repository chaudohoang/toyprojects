@echo off
echo ========================================
echo          TAT TRO LY PIKA
echo ========================================
echo.
echo Dang tim tien trinh bot.py chay ngam de tat...

powershell -Command "Get-CimInstance Win32_Process -Filter \"name='python.exe'\" | Where-Object { $_.ExecutablePath -match 'Pika_bot' -and $_.CommandLine -match 'bot.py' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force }"

echo.
echo Da tat bot thanh cong! Sep co the dong cua so nay.
pause
