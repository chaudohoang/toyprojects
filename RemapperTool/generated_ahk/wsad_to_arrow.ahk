#NoEnv
#SingleInstance Force
SetWorkingDir %A_ScriptDir%

toggle := true  ; Default: Remap ON when started

; --- Global Toggle ---
~!`::
toggle := !toggle
SoundBeep, % (toggle ? 750 : 400), 150  ; Beep high if ON, low if OFF
return

#If (toggle)  ; Global remap when toggle is ON
w::Up
s::Down
a::Left
d::Right

#If