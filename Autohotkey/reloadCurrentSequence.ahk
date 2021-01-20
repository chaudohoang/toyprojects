SetTitleMatchMode, 2

FileRead, xmldata, C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\app.settings
	doc := ComObjCreate("MSXML2.DOMDocument.6.0")
	doc.async := false
	doc.loadXML(xmldata)
	DocNode := doc.selectSingleNode("//Settings/PatternGenerator/PatternGeneratorTypeXml")
	deviceInterface := DocNode.text
	
FileRead, xmldata, C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\Dove2p0_PG.Dove2p0_PG.xml
	doc := ComObjCreate("MSXML2.DOMDocument.6.0")
	doc.async := false
	doc.loadXML(xmldata)
	DocNode := doc.selectSingleNode("//Dove2p0_PG/Model")
	doooneModel := DocNode.text
	
FileRead, xmldata, C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\Emu2p0_PG.Emu2p0_PG.xml
	doc := ComObjCreate("MSXML2.DOMDocument.6.0")
	doc.async := false
	doc.loadXML(xmldata)
	DocNode := doc.selectSingleNode("//Emu2p0_PG/Model")
	gooilModel := "NY_"DocNode.text
	
Model := ""

if (deviceInterface = "Dove2p0_PG.Dove2p0_PG" )
{
    Model := doooneModel
}
else if (deviceInterface = "Emu2p0_PG.Emu2p0_PG")
{
    Model := gooilModel
}

#ifWinExist ahk_exe TrueTest.exe
	sleep, 10
    WinActivate ahk_exe TrueTest.exe
	sleep , 10
    Send, ^o
	sleep, 10
	Send, %Model%
	sleep, 100
	Send, {enter}
#ifWinExist