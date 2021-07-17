#SingleInstance force
#MaxHotkeysPerInterval 500


!`::
Suspend
return

#ifWinActive Age of Empires Expansion


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
