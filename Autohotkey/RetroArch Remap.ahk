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
k::z ;B
i::s ;X
j::a ;Y
l::x ;A
2::RShift ;Select
1::Enter ;Start
return

#ifWinActive
