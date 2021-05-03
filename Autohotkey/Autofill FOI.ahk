SetTitleMatchMode, 2
#SingleInstance, force

#ifWinExist ahk_exe TrueTest.exe
	sleep, 10
	WinActivate ahk_exe TrueTest.exe
	sleep , 10
	FileRead, OutputVar, C:\POCB\INPUT\points.csv
	Loop, parse, OutputVar, `n, `r ; VERY important that you omit the `r characters or else the folders won't be created
	{
	   If A_LoopField= ; if a field is blank, don't worry about it
		  continue
	   If A_Index=1 ; because you have a "header" in Row 1, I figure you don't want a folder and MsgBox for this
		  continue
		StringSplit, Points, A_LoopField, `, ; splits the A_LoopField into Array Apple based on commas
		SendInput, %Points1%
		Sleep , 300
		Send {tab}
		Sleep , 300
		Send  {right}
		Sleep , 300
		SendInput,  %Points2%
		Sleep , 300
		Send {tab}
		Sleep , 300
		Send {right}
		Sleep , 300
		SendInput,  %Points3%
		Sleep , 300
		Send {tab}
		Sleep , 300
		Send {left}
		Sleep , 300
		Send {left}
		Sleep , 300
		Send {left}
		Sleep , 300
		Send {down}
	   
	}
Escape::
ExitApp
Return
#ifWinExist
