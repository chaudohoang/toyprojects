#SingleInstance force
#MaxHotkeysPerInterval 500
SetTitleMatchMode, 2
!`::
Suspend
return

if (NumGet(lParam+8) & 0x80) { ; key up
      if (count = 0 && NumGet(lParam+4) = 0x24) {        ; 'j'
        count = 1
      } else if (count = 1 && NumGet(lParam+4) = 0x25) { ; 'k'
        count = 2
      } else if (count = 2 && NumGet(lParam+4) = 0x26) { ; 'l'
        count = 3
      } else if (count = 3 && NumGet(lParam+4) = 0x17) { ; 'i'
        count = 0
        {
            IfWinExist, ahk_exe TrueTest.exe
			{
				WinActivate  
				FileRead, xmldata, C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\app.settings
				doc := ComObjCreate("MSXML2.DOMDocument.6.0")
				doc.async := false
				doc.loadXML(xmldata)
				DocNode := doc.selectSingleNode("//Settings/Password")
				password := DocNode.text
				SendInput, {F2}
				Sleep, 100
				Send, %password%
				Sleep, 100
				SendInput, {Enter}
				return
			}
        }
      } else {
        count = 0
      }
    }