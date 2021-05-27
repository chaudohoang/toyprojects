#SingleInstance Force
Loop
{
	IfWinExist, Sponsored session
	{
		WinClose
	}

	Sleep, 1000 ; (wait 1 second)
}