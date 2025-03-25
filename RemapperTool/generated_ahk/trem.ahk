#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

toggle := true  ; Default: Remap ON when started

; --- Toggle works ONLY inside the game ---
#IfWinActive ahk_exe empiresx.exe
~!`::
toggle := !toggle
SoundBeep, % (toggle ? 750 : 400), 150  ; Beep high if ON, low if OFF
return

#If (toggle && WinActive("ahk_exe empiresx.exe"))
q::b
b::k
z::a
x::l
r::p
n::n
t::y
m::m
c::c
g::g
u::w
i::t
o::o
v::s
w::Up
s::Down
a::Left
d::Right

#If