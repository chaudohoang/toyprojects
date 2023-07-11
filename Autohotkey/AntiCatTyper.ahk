RunAsAdmin()

LWin & z::
BlockInput On
;Msgbox,,Anti-Cat Typer,Keyboard & mouse disabled.`nUse Win+X to re-enable.
ToolTip, Keyboard & mouse disabled. Use Win+X to re-enable.
;settimer, remove, 10000	; as a safety feature while testing
SetTimer, RemoveToolTip, -3000
return

RemoveToolTip:
ToolTip
return

RWin & x::
LWin & x::
blockinput off
ToolTip, Keyboard & mouse enabled.
SetTimer, RemoveToolTip, -3000
return

;remove:
;blockinput off

RunAsAdmin() {
  Loop, %0%  ; For each parameter:
    {
      param := %A_Index%  ; Fetch the contents of the variable whose name is contained in A_Index.
      params .= A_Space . param
    }
  ShellExecute := A_IsUnicode ? "shell32\ShellExecute":"shell32\ShellExecuteA"

  if not A_IsAdmin
  {
      If A_IsCompiled
         DllCall(ShellExecute, uint, 0, str, "RunAs", str, A_ScriptFullPath, str, params , str, A_WorkingDir, int, 1)
      Else
         DllCall(ShellExecute, uint, 0, str, "RunAs", str, A_AhkPath, str, """" . A_ScriptFullPath . """" . A_Space . params, str, A_WorkingDir, int, 1)
      ExitApp
  }
}