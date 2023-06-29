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

*e::Up
*d::Down
*s::Left
*f::Right
*a::q ;L1
*;::w ;R1
*k::z ;B
*i::s ;X
*j::a ;Y
*l::x ;A
*2::RShift ;Select
*1::Enter ;Start
return

#ifWinActive

#ifWinActive ahk_exe emulationstation.exe

`::
Suspend
return

*e::Up
*d::Down
*s::Left
*f::Right
*a::PgUp ;L1
*`;::PgDn ;R1
*k::z ;B
*i::q ;X
*j::s ;Y
*l::x ;A
*2::Backspace ;Select
*1::Enter ;Start
return

#ifWinActive
