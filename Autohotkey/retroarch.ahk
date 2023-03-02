#SingleInstance force
#MaxHotkeysPerInterval 500

!`::
Suspend
return

#ifWinActive RetroArch

#If Mode = Xbox
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
2::Shift ;Select
1::Enter ;Start
return

#If Mode = Snes
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
2::Shift ;Select
1::Enter ;Start
return

#If Mode = Nes
e::Up
d::Down
s::Left
f::Right
a::q ;L1
`;::w ;R1
j::z ;B
l::s ;X
i::a ;Y
k::x ;A
2::Shift ;Select
1::Enter ;Start
return

#ifWinActive

!8:
Mode := Xbox
return
!9:
Mode := Snes
return
!0:
Mode := Nes
return
