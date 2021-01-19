SetTitleMatchMode, 2
#ifWinExist ahk_exe TrueTest.exe
	sleep, 10
    WinActivate ahk_exe TrueTest.exe
	sleep , 10
    Send, ^o
	sleep, 10
	Send, CF-CB-P2
	sleep, 10
	Send, {enter}
#ifWinExist