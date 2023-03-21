#SingleInstance force
#MaxHotkeysPerInterval 500
SetTitleMatchMode, 2

#ifWinActive ahk_exe TrueTest.exe

~*^`::
{
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

#ifWinActive


