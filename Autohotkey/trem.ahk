#SingleInstance force
#MaxHotkeysPerInterval 500

#ifWinActive ahk_exe empiresx.exe

!`::
Suspend
return

w::Up
s::Down
a::Left
d::Right
q::b
z::a
x::l
v::h
`::s

WheelUp::
WheelDown::
{
	Send {Enter}
	Sleep 5
	Send Cho ti nhe !
	Sleep 5
	Send {Enter}
	Send {F3}{LWin}
}

#ifWinActive
