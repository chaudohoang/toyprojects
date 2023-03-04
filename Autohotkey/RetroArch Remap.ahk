#SingleInstance force
#MaxHotkeysPerInterval 500

#ifWinActive ahk_exe retroarch.exe

`::
Suspend
return

~*LControl::
	Suspend,On
Return
~*LControl Up::
	Suspend,Off
Return

e::Up
d::Down
s::Left
f::Right
a::q ;L1
`;::w ;R1
l::z ;B
j::s ;X
i::a ;Y
k::x ;A
2::RShift ;Select
1::Enter ;Start
return

#ifWinActive
