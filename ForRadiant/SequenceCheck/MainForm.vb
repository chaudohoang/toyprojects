Imports System.Data.SqlServerCe
Imports System.IO
Imports System.Runtime.Remoting.Metadata.W3cXsd2001
Imports System.Xml


Public Class MainForm

	Dim conn As SqlCeConnection
	Dim CameraSN As String
	Dim exePath As String = My.Application.Info.DirectoryPath
	Private Sub SetVersionInfo()

		Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
		Dim ver As System.Version = ass.GetName().Version
		Dim startDate As DateTime = New Date(2000, 1, 1)
		Dim diffDays As Integer = ver.Build
		Dim computedDate As DateTime = startDate.AddDays(diffDays)
		Dim lastBuilt As String = computedDate.ToShortDateString()
		'Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision & " (" & lastBuilt & ")")
		Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision)

	End Sub


	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		SetVersionInfo()
		If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
			Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
			txtFile1.Text = latestfile.FullName
			txtFile3.Text = latestfile.FullName
			Dim defaultMasterSequence = latestfile.DirectoryName + "\Master\" + latestfile.Name
			txtFile2.Text = defaultMasterSequence
		End If
	End Sub

	Private Sub txtFile1_DragOver(sender As Object, e As DragEventArgs) Handles txtFile1.DragOver

		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
		End If
	End Sub

	Private Sub txtFile1_DragDrop(sender As Object, e As DragEventArgs) Handles txtFile1.DragDrop
		Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
		txtFile1.Text = files(0)
	End Sub

	Private Sub txtFile2_DragDrop(sender As Object, e As DragEventArgs) Handles txtFile2.DragDrop
		Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
		txtFile2.Text = files(0)
	End Sub

	Private Sub txtFile2_DragOver(sender As Object, e As DragEventArgs) Handles txtFile2.DragOver
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
		End If
	End Sub

	Private Sub btnCompare_Click(sender As Object, e As EventArgs) Handles btnCompare.Click
		btnCompare.Enabled = False

		CheckForMatchingSequenceParameters(txtFile1.Text, txtFile2.Text)
		CommLogUpdateText(vbCrLf)
		CompareCalSettings(txtFile1.Text, txtFile2.Text)
		CommLogUpdateText(vbCrLf)
		btnCompare.Enabled = True
	End Sub

	Private Sub btnBrowse1_Click(sender As Object, e As EventArgs) Handles btnBrowse1.Click
		Using frm = New OpenFileDialog
			If frm.ShowDialog(Me) = DialogResult.OK Then
				txtFile1.Text = frm.FileName
			End If
		End Using
	End Sub

	Private Sub btnBrowse2_Click(sender As Object, e As EventArgs) Handles btnBrowse2.Click
		Using frm = New OpenFileDialog
			If frm.ShowDialog(Me) = DialogResult.OK Then
				txtFile2.Text = frm.FileName
			End If
		End Using
	End Sub

	Public Sub CheckForMatchingSequenceParameters(ByVal file1FullPath As String, ByVal file2FullPath As String)
		Dim equal As Boolean = True
		Dim log As New List(Of String)
		If Not File.Exists(file1FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 1 is not existed !!!")
		ElseIf Not File.Exists(file2FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 2 is not existed !!!")

			Exit Sub
		ElseIf file1FullPath = file2FullPath Then
			equal = False
			CommLogUpdateText("Sequence 1 and Sequence 2 is the same file !!!")
			Exit Sub
		End If

		Dim ignoreList As List(Of String)
		ignoreList = (From s In cbxIgnoreList.Text.Split(CChar(","))
					  Select s).ToList()

		Dim sequence1AnaList As New List(Of String)
		Dim sequence2AnaList As New List(Of String)

		Dim node1 As XmlNode
		Dim nodes1 As XmlNodeList

		Dim node2 As XmlNode
		Dim nodes2 As XmlNodeList

		Dim xmlDoc1 = New XmlDocument()
		Dim xmlDoc2 = New XmlDocument()
		xmlDoc1.Load(file1FullPath)
		xmlDoc2.Load(file2FullPath)

		nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")

		'conpare sequence analysis list
		For i1 = 0 To nodes1.Count - 1
			If nodes1(i1).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence1AnaList.Add(nodes1(i1).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next

		For i2 = 0 To nodes2.Count - 1
			If nodes2(i2).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence2AnaList.Add(nodes2(i2).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next

		If String.Join(",", sequence1AnaList) <> String.Join(",", sequence2AnaList) Then
			equal = False
			CommLogUpdateText("Analysis list does not match !!!")
			CommLogUpdateText("Sequence 1 analyses : " + String.Join(",", sequence1AnaList))
			CommLogUpdateText("Sequence 2 analyses : " + String.Join(",", sequence2AnaList))
			Exit Sub
		End If
		Dim SequenceItemCount As Integer
		If nodes1.Count < nodes2.Count Then
			SequenceItemCount = nodes1.Count
		Else
			SequenceItemCount = nodes2.Count
		End If
		For index = 0 To SequenceItemCount - 1
			node1 = nodes1(index).SelectSingleNode("Analysis")
			node2 = nodes2(index).SelectSingleNode("Analysis")

			'Remove items in ignoreList
			For Each item As String In ignoreList
				For Each childNode1 As XmlNode In node1.ChildNodes
					If childNode1.Name = item Then
						node1.RemoveChild(childNode1)
					End If
				Next
			Next

			For Each item As String In ignoreList
				For Each childNode2 As XmlNode In node2.ChildNodes
					If childNode2.Name = item Then
						node2.RemoveChild(childNode2)
					End If
				Next
			Next

			'Equalize number of elements between 2 node
			For Each childNode1 As XmlNode In node1.ChildNodes
				If node2.SelectSingleNode(childNode1.Name) Is Nothing Then
					Dim importNode As XmlNode = xmlDoc2.ImportNode(childNode1, True)
					node2.AppendChild(importNode)
				End If
			Next

			For Each childNode2 As XmlNode In node2.ChildNodes
				If node1.SelectSingleNode(childNode2.Name) Is Nothing Then
					Dim importNode As XmlNode = xmlDoc1.ImportNode(childNode2, True)
					node1.AppendChild(importNode)
				End If
			Next

			SortElements(node1)
			SortElements(node2)

			For childIndex = 0 To node1.ChildNodes.Count - 1
				If node1.ChildNodes(childIndex).InnerText.ToLower <> node2.ChildNodes(childIndex).InnerText.ToLower Then
					equal = False
					log.Add("Step : " + nodes1(index).SelectSingleNode("PatternSetupName").InnerText + ", Parameter : " + node1.ChildNodes(childIndex).Name + ", Sequence 1 Value : " + node1.ChildNodes(childIndex).InnerText + ", Sequence 2 Value : " + node2.ChildNodes(childIndex).InnerText)
				End If

			Next

		Next

		If equal Then
			CommLogUpdateText("NO PARAMETER DIFFERENCE !!!")
		Else
			CommLogUpdateText("FOUND PARAMETER DIFFERENCE !!!")
		End If

		For i = 0 To log.Count - 1
			CommLogUpdateText(log(i))
		Next

	End Sub

	Public Sub SortElements(ByVal node As XmlNode)
		Dim changed As Boolean = True

		While changed
			changed = False

			For i As Integer = 1 To node.ChildNodes.Count - 1

				If String.Compare(node.ChildNodes(i).Name, node.ChildNodes(i - 1).Name, True) < 0 Then
					node.InsertBefore(node.ChildNodes(i), node.ChildNodes(i - 1))
					changed = True
				End If
			Next
		End While
	End Sub

	Private Delegate Sub UpdateCommLogDelegate(text As String)
	Private Sub CommLogUpdateText(text As String)
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText), New Object() {text}) : Exit Sub
		ListBox1.Items.Add(text)
		ListBox1.TopIndex = ListBox1.Items.Count - 1
	End Sub

	Private Sub btnClearlog_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click
		ListBox1.Items.Clear()
	End Sub

	Private Sub CommLogUpdateText2(text As String)
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText2), New Object() {text}) : Exit Sub
		ListBox2.Items.Add(text)
		ListBox2.TopIndex = ListBox2.Items.Count - 1
	End Sub

	Private Sub btnClearlog2_Click(sender As Object, e As EventArgs) Handles btnClearlog2.Click
		ListBox2.Items.Clear()
	End Sub
	Private Sub CommLogUpdateText3(text As String)
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText3), New Object() {text}) : Exit Sub
		ListBox3.Items.Add(text)
		ListBox3.TopIndex = ListBox3.Items.Count - 1
	End Sub

	Private Sub btnClearlog3_Click(sender As Object, e As EventArgs) Handles btnClearlog3.Click
		ListBox3.Items.Clear()
	End Sub

	Private Sub btnBrowse3_Click(sender As Object, e As EventArgs) Handles btnBrowse3.Click
		Using frm = New OpenFileDialog
			If frm.ShowDialog(Me) = DialogResult.OK Then
				txtFile3.Text = frm.FileName
			End If
		End Using
	End Sub

	Private Sub txtFile3_DragDrop(sender As Object, e As DragEventArgs) Handles txtFile3.DragDrop
		Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
		txtFile3.Text = files(0)
	End Sub

	Private Sub txtFile3_DragOver(sender As Object, e As DragEventArgs) Handles txtFile3.DragOver
		If e.Data.GetDataPresent(DataFormats.FileDrop) Then
			e.Effect = DragDropEffects.Copy
		End If
	End Sub

	Private Sub btnCheck_Click(sender As Object, e As EventArgs) Handles btnCheck.Click
		btnCheck.Enabled = False

		CheckSequence()

		If ListBox2.Items.Count > 0 AndAlso ListBox2.Items(ListBox2.Items.Count - 1).ToString() <> vbCrLf Then
			CommLogUpdateText2(vbCrLf)
		End If
		btnCheck.Enabled = True

	End Sub

	Public Sub CheckSequence()
		Dim subframeMatch As Boolean = True
		Dim CalIsNONE As Boolean = False
		Dim ColorCalNG As Boolean = False
		Dim FlatFieldCalNG As Boolean = False
		Dim ImgScaleCalNG As Boolean = False
		Dim logSubframe As New List(Of String)
		Dim logCalNone As New List(Of String)
		Dim logColorCal As New List(Of String)
		Dim logFlatFieldCal As New List(Of String)
		Dim logImgScaleCal As New List(Of String)
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		If Not Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules") Then
			Directory.CreateDirectory("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules")
		End If
		Dim ColorCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_colorcal.txt"
		Dim ColorCalRuleFilePath As String = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ColorCalRuleFilaName)

		Dim ColorCalRulesDict As New Dictionary(Of String, String)

		If File.Exists(ColorCalRuleFilePath) Then
			Dim CalRuleContent As String() = File.ReadAllLines(ColorCalRuleFilePath)
			For Each line As String In CalRuleContent
				Dim StepName As String = line.Split(",")(0)
				Dim CalID As String = line.Split(",")(1)
				ColorCalRulesDict.Add(StepName, CalID)
			Next

		End If

		Dim FlatFieldCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_flatfieldcal.txt"
		Dim FlatFieldCalRuleFilePath As String = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", FlatFieldCalRuleFilaName)

		Dim FlatFieldCalRulesDict As New Dictionary(Of String, String)

		If File.Exists(FlatFieldCalRuleFilePath) Then
			Dim CalRuleContent As String() = File.ReadAllLines(FlatFieldCalRuleFilePath)
			For Each line As String In CalRuleContent
				Dim StepName As String = line.Split(",")(0)
				Dim CalID As String = line.Split(",")(1)
				FlatFieldCalRulesDict.Add(StepName, CalID)
			Next

		End If

		Dim ImgScaleCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_imgscalecal.txt"
		Dim ImgScaleCalRuleFilePath As String = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ImgScaleCalRuleFilaName)

		Dim ImgScaleCalRulesDict As New Dictionary(Of String, String)

		If File.Exists(ImgScaleCalRuleFilePath) Then
			Dim CalRuleContent As String() = File.ReadAllLines(ImgScaleCalRuleFilePath)
			For Each line As String In CalRuleContent
				Dim StepName As String = line.Split(",")(0)
				Dim CalID As String = line.Split(",")(1)
				ImgScaleCalRulesDict.Add(StepName, CalID)
			Next

		End If

		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If (nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
			Not nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
				demuraStepList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If cbxSubframe.Text <> "" AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				Dim subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				If subframe <> cbxSubframe.Text Then
					subframeMatch = False
					logSubframe.Add("SN : " + SN + " , Step : " + StepName + ", Subframe : " + subframe)
				End If

			End If

		Next

		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If chkCalNone.Checked = True AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				Dim subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText
				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Dim log As String = ""
				If CCID = 0 Then
					CalIsNONE = True
					log += " , ColorCalID : " + CCID
				End If
				If IMCID = 0 Then
					CalIsNONE = True
					log += " , ImageScalingID : " + IMCID
				End If
				If FFID = 0 Then
					CalIsNONE = True
					log += " , FlatFieldID : " + FFID
				End If
				If log <> "" Then
					logCalNone.Add("SN : " + SN + " , Step : " + StepName + log)
				End If
			End If

		Next

		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If chkColorCalSettings.Checked = True AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Dim log As String = ""
				If ColorCalRulesDict.ContainsKey(StepName) AndAlso ColorCalRulesDict(StepName) <> CCID Then
					ColorCalNG = True
					log += " , ColorCalID : " + CCID + " , Correct ColorCalID : " + ColorCalRulesDict(StepName)
				ElseIf Not ColorCalRulesDict.ContainsKey(StepName) Then
					ColorCalNG = True
					log += " , ColorCalID : " + CCID + " , This step has no calibration rule "
				End If

				If log <> "" Then
					logColorCal.Add("SN : " + SN + " , Step : " + StepName + log)
				End If
			End If

		Next

		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If chkFlatFieldCalSettings.Checked = True AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Dim log As String = ""
				If FlatFieldCalRulesDict.ContainsKey(StepName) AndAlso FlatFieldCalRulesDict(StepName) <> FFID Then
					FlatFieldCalNG = True
					log += " , FlatFieldID : " + FFID + " , Correct FlatFieldID : " + FlatFieldCalRulesDict(StepName)
				ElseIf Not FlatFieldCalRulesDict.ContainsKey(StepName) Then
					FlatFieldCalNG = True
					log += " , FlatFieldID : " + FFID + " , This step has no calibration rule "
				End If

				If log <> "" Then
					logFlatFieldCal.Add("SN : " + SN + " , Step : " + StepName + log)
				End If
			End If

		Next

		For index = 0 To nodes3.Count - 1
			'If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If chkImgScaleCalSettings.Checked = True AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim ISCID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Dim log As String = ""
				If ImgScaleCalRulesDict.ContainsKey(StepName) AndAlso ImgScaleCalRulesDict(StepName) <> ISCID Then
					ImgScaleCalNG = True
					log += " , ImageScalingCalibration : " + ISCID + " , Correct ImageScalingCalibration : " + ImgScaleCalRulesDict(StepName)
				ElseIf Not ImgScaleCalRulesDict.ContainsKey(StepName) Then
					ImgScaleCalNG = True
					log += " , ImageScalingCalibration : " + ISCID + " , This step has no calibration rule "
				End If

				If log <> "" Then
					logImgScaleCal.Add("SN : " + SN + " , Step : " + StepName + log)
				End If
			End If

		Next

		If cbxSubframe.Text <> "" AndAlso subframeMatch Then
			CommLogUpdateText2("SUBFRAME MATCH ALL !!!")
		ElseIf cbxSubframe.Text <> "" AndAlso Not subframeMatch Then
			CommLogUpdateText2("SUBFRAME MISMATCH DETECTED !!!")
		Else

		End If

		For i = 0 To logSubframe.Count - 1
			CommLogUpdateText2(logSubframe(i))
		Next

		If chkCalNone.Checked = True AndAlso Not CalIsNONE Then
			CommLogUpdateText2("CALIBRATION ALL SET !!!")
		ElseIf chkCalNone.Checked = True AndAlso CalIsNONE Then
			CommLogUpdateText2("CALIBRATION NONE DETECTED !!!")
		Else
		End If

		For i = 0 To logCalNone.Count - 1
			CommLogUpdateText2(logCalNone(i))
		Next

		If chkColorCalSettings.Checked = True AndAlso Not ColorCalNG Then
			CommLogUpdateText2("COLOR CALIBRATION ALL OK !!!")
		ElseIf chkColorCalSettings.Checked = True AndAlso ColorCalNG Then
			CommLogUpdateText2("NG COLOR CALIBRATION DETECTED !!!")
		Else
		End If

		For i = 0 To logColorCal.Count - 1
			CommLogUpdateText2(logColorCal(i))
		Next

		If chkFlatFieldCalSettings.Checked = True AndAlso Not FlatFieldCalNG Then
			CommLogUpdateText2("FLAT FIELD CALIBRATION ALL OK !!!")
		ElseIf chkFlatFieldCalSettings.Checked = True AndAlso FlatFieldCalNG Then
			CommLogUpdateText2("NG FLAT FIELD CALIBRATION DETECTED !!!")
		Else
		End If

		For i = 0 To logFlatFieldCal.Count - 1
			CommLogUpdateText2(logFlatFieldCal(i))
		Next

		If chkImgScaleCalSettings.Checked = True AndAlso Not ImgScaleCalNG Then
			CommLogUpdateText2("IMAGE SCALING CALIBRATION ALL OK !!!")
		ElseIf chkImgScaleCalSettings.Checked = True AndAlso ImgScaleCalNG Then
			CommLogUpdateText2("NG IMAGE SCALING CALIBRATION DETECTED !!!")
		Else
		End If

		For i = 0 To logImgScaleCal.Count - 1
			CommLogUpdateText2(logImgScaleCal(i))
		Next


	End Sub


	Private Sub btnShowSettings_Click(sender As Object, e As EventArgs) Handles btnShowSettings.Click
		btnShowSettings.Enabled = False
		ShowMeasSettings()
		CommLogUpdateText2(vbCrLf)
		btnShowSettings.Enabled = True
	End Sub

	Public Sub ShowMeasSettings()
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")

		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis") Then
				demuraStepList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		CommLogUpdateText2("ALL SETTINGS :")
		For index = 0 To nodes3.Count - 1
			'If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				Dim FocusDistance = nodes3(index).SelectSingleNode("LensDistance").InnerText
				Dim FNumber = nodes3(index).SelectSingleNode("LensfStop").InnerText
				Select Case FNumber
					Case "2"
						FNumber = "2.0"
					Case "2.5"
						FNumber = "2.3"
					Case "3"
						FNumber = "2.8"
					Case "3.5"
						FNumber = "4.0"
					Case "4"
						FNumber = "3.3"
					Case "4.5"
						FNumber = "4.7"
					Case "5"
						FNumber = "5.6"
					Case "5.5"
						FNumber = "6.7"
					Case "6"
						FNumber = "8.0"
					Case "6.5"
						FNumber = "9.5"
					Case "7"
						FNumber = "11"
					Case "7.5"
						FNumber = "13"
					Case "8"
						FNumber = "16"
					Case "8.5"
						FNumber = "19"
					Case "9"
						FNumber = "22"

					Case Else

				End Select
				Dim CameraRotation = nodes3(index).SelectSingleNode("CameraRotation").InnerText
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				Dim subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText
				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + " , Focus : " + FocusDistance + " , F-number : " + FNumber + " , Rotation : " + CameraRotation + " , Subframe : " + subframe + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
			End If
		Next
	End Sub

	Public Sub ShowColorCalSettings()
		Dim conn As SqlCeConnection
		Dim cmdCalibration As New SqlCeCommand
		Dim daCalibration As New SqlCeDataAdapter
		Dim dsCalibration As New DataSet
		Dim dtCalibration As New DataTable
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis") Then
				demuraStepList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		CommLogUpdateText2("COLOR CALIBRATION SETTINGS :")
		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID)
			End If
		Next
		CommLogUpdateText2("COLOR CALIBRATION REFERENCES :")
		Try
			conn = GetConnect(SN)
			cmdCalibration = conn.CreateCommand
			cmdCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations"

			daCalibration.SelectCommand = cmdCalibration
			daCalibration.Fill(dsCalibration, "ColorCalibrations")
			If dsCalibration.Tables("ColorCalibrations").Rows.Count = 0 Then
				CommLogUpdateText2("SN : " + SN + " : No user created calibrations ")
			Else
				For Each row As DataRow In dsCalibration.Tables("ColorCalibrations").Rows
					CommLogUpdateText2("SN : " + SN + " , ColorCalID : " & row("ColorCalibrationID") & " , Description : " & row("Description"))
				Next
			End If

		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try


	End Sub
	Public Sub ShowFlatFieldCalRefs()
		Dim conn As SqlCeConnection
		Dim cmdCalibration As New SqlCeCommand
		Dim daCalibration As New SqlCeDataAdapter
		Dim dsCalibration As New DataSet
		Dim dtCalibration As New DataTable
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis") Then
				demuraStepList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		CommLogUpdateText2("FLAT FIELD CALIBRATION SETTINGS :")
		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + " , FlatFieldID : " + FFID)
			End If
		Next
		CommLogUpdateText2("FLAT FIELD CALIBRATION REFERENCES :")
		Try
			conn = GetConnect(SN)
			cmdCalibration = conn.CreateCommand
			cmdCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration"

			daCalibration.SelectCommand = cmdCalibration
			daCalibration.Fill(dsCalibration, "FlatFieldCalibration")

			If dsCalibration.Tables("FlatFieldCalibration").Rows.Count = 0 Then
				CommLogUpdateText2("SN : " + SN + " : No user created calibrations ")
			Else
				For Each row As DataRow In dsCalibration.Tables("FlatFieldCalibration").Rows
					CommLogUpdateText2("SN : " + SN + " , FlatFieldID : " & row("CalibrationID") & " , Description : " & row("CalibrationDesc"))
				Next
			End If


		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try


	End Sub
	Public Sub ShowImgScaleCalRefs()
		Dim conn As SqlCeConnection
		Dim cmdCalibration As New SqlCeCommand
		Dim daCalibration As New SqlCeDataAdapter
		Dim dsCalibration As New DataSet
		Dim dtCalibration As New DataTable
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes3(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis") Then
				demuraStepList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		CommLogUpdateText2("IMG SCALING CALIBRATION SETTINGS :")
		For index = 0 To nodes3.Count - 1
			If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
			If sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next

				Dim ISCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + " , ImageScalingCalibrationID : " + ISCID)
			End If
		Next
		CommLogUpdateText2("IMG SCALING CALIBRATION REFERENCES :")
		Try
			conn = GetConnect(SN)
			cmdCalibration = conn.CreateCommand
			cmdCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration"

			daCalibration.SelectCommand = cmdCalibration
			daCalibration.Fill(dsCalibration, "ImageScalingCalibration")

			If dsCalibration.Tables("ImageScalingCalibration").Rows.Count = 0 Then
				CommLogUpdateText2("SN : " + SN + " : No user created calibrations ")
			Else
				For Each row As DataRow In dsCalibration.Tables("ImageScalingCalibration").Rows
					CommLogUpdateText2("SN : " + SN + " , ImageScalingCalibrationID : " & row("ImageScalingCalibrationID") & " , Description : " & row("ImageScalingCalibrationDesc"))

				Next
			End If

		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try


	End Sub

	Private Sub btnUseLastModified1_Click(sender As Object, e As EventArgs) Handles btnUseLastModified1.Click
		If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
			Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
			txtFile1.Text = latestfile.FullName
		End If
	End Sub

	Private Sub btnUseDefaultMaster_Click(sender As Object, e As EventArgs) Handles btnUseDefaultMaster.Click
		If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
			Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
			Dim defaultMasterSequence = latestfile.DirectoryName + "\Master\" + latestfile.Name
			txtFile2.Text = defaultMasterSequence
		End If
	End Sub

	Private Sub btnUseLastModified3_Click(sender As Object, e As EventArgs) Handles btnUseLastModified3.Click
		If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
			Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
			txtFile3.Text = latestfile.FullName
		End If
	End Sub

	Private Sub btnExportAnalysesCompareLog_Click(sender As Object, e As EventArgs) Handles btnExportAnalysesCompareLog.Click
		Dim logPath = Path.Combine(exePath, Now.ToString("yyyyMMddHHmmss") + "_Compare.txt")
		If ListBox1.Items.Count = 0 Then
			MessageBox.Show("Empty Log !!!")
		Else
			Dim savefile As SaveFileDialog = New SaveFileDialog()
			' set a default file name
			savefile.FileName = Path.GetFileName(logPath)
			' set filters - this can be done in properties as well
			savefile.Filter = "Text files (*.txt)|*.txt"
			savefile.InitialDirectory = exePath

			If savefile.ShowDialog() = DialogResult.OK Then
				Using sw As StreamWriter = New StreamWriter(savefile.FileName)
					sw.WriteLine("Sequence 1 : " + txtFile1.Text)
					sw.WriteLine("Sequence 2 : " + txtFile2.Text)
					sw.WriteLine("Ignore List : " + cbxIgnoreList.Text)

					For index = 0 To ListBox1.Items.Count - 1
						sw.WriteLine(ListBox1.Items(index))
					Next
				End Using
			End If
			Process.Start("notepad", savefile.FileName)
		End If
	End Sub

	Private Sub btnExportMeasurementsCompareLog_Click(sender As Object, e As EventArgs) Handles btnExportMeasurementsCompareLog.Click
		Dim logPath = Path.Combine(exePath, Now.ToString("yyyyMMddHHmmss") + "_Measurement.txt")
		If ListBox2.Items.Count = 0 Then
			MessageBox.Show("Empty Log !!!")
		Else
			Dim savefile As SaveFileDialog = New SaveFileDialog()
			' set a default file name
			savefile.FileName = Path.GetFileName(logPath)
			' set filters - this can be done in properties as well
			savefile.Filter = "Text files (*.txt)|*.txt"
			savefile.InitialDirectory = exePath

			If savefile.ShowDialog() = DialogResult.OK Then
				Using sw As StreamWriter = New StreamWriter(savefile.FileName)
					sw.WriteLine("Sequence : " + txtFile3.Text)
					sw.WriteLine("Subframe : " + cbxSubframe.Text)
					sw.WriteLine("Check Calibration NONE : " + If(chkCalNone.Checked, "yes", "no"))

					For index = 0 To ListBox2.Items.Count - 1
						sw.WriteLine(ListBox2.Items(index))
					Next
				End Using
			End If
			Process.Start("notepad", savefile.FileName)
		End If
	End Sub

	Private Sub btnExportOtherCompareLog_Click(sender As Object, e As EventArgs) Handles btnExportOtherCompareLog.Click
		Dim logPath = Path.Combine(exePath, Now.ToString("yyyyMMddHHmmss") + "_Other.txt")
		If ListBox3.Items.Count = 0 Then
			MessageBox.Show("Empty Log !!!")
		Else
			Dim savefile As SaveFileDialog = New SaveFileDialog()
			' set a default file name
			savefile.FileName = Path.GetFileName(logPath)
			' set filters - this can be done in properties as well
			savefile.Filter = "Text files (*.txt)|*.txt"
			savefile.InitialDirectory = exePath

			If savefile.ShowDialog() = DialogResult.OK Then
				Using sw As StreamWriter = New StreamWriter(savefile.FileName)

					For index = 0 To ListBox3.Items.Count - 1
						sw.WriteLine(ListBox3.Items(index))
					Next
				End Using
			End If
			Process.Start("notepad", savefile.FileName)
		End If
	End Sub

	Private Sub btnShowCalSettings_Click(sender As Object, e As EventArgs) Handles btnShowCalSettings.Click
		btnShowCalSettings.Enabled = False
		ShowColorCalSettings()
		ShowFlatFieldCalRefs()
		ShowImgScaleCalRefs()
		CommLogUpdateText2(vbCrLf)
		btnShowCalSettings.Enabled = True

	End Sub

	Private Sub btnEditCalRule_Click(sender As Object, e As EventArgs) Handles btnEditCalRule.Click
		CalRule.Show()
	End Sub

	Public Sub CompareCalSettings(ByVal file1FullPath As String, ByVal file2FullPath As String)

		Dim equal As Boolean = True
		Dim log As New List(Of String)
		If Not File.Exists(file1FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 1 is not existed !!!")
		ElseIf Not File.Exists(file2FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 2 is not existed !!!")

			Exit Sub
		ElseIf file1FullPath = file2FullPath Then
			equal = False
			CommLogUpdateText("Sequence 1 and Sequence 2 is the same file !!!")
			Exit Sub
		End If
		Dim SN1 As String = ""
		Dim SN2 As String = ""

		Dim ColorCalSetting1 As New List(Of String)
		Dim ColorCalSetting2 As New List(Of String)
		Dim ImgScaleSetting1 As New List(Of String)
		Dim ImgScaleSetting2 As New List(Of String)
		Dim FFCSetting1 As New List(Of String)
		Dim FFCSetting2 As New List(Of String)

		Dim sequence1AnaList As New List(Of String)
		Dim sequence2AnaList As New List(Of String)

		Dim demuraStepList1 As New List(Of String)
		Dim demuraStepList2 As New List(Of String)

		Dim node1 As XmlNode
		Dim nodes1 As XmlNodeList

		Dim node2 As XmlNode
		Dim nodes2 As XmlNodeList

		Dim xmlDoc1 = New XmlDocument()
		Dim xmlDoc2 = New XmlDocument()
		xmlDoc1.Load(file1FullPath)
		xmlDoc2.Load(file2FullPath)

		nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")

		For i = 0 To nodes1.Count - 1
			If nodes1(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence1AnaList.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If (nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
			Not nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
				demuraStepList1.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		For index = 0 To nodes1.Count - 1
			If sequence1AnaList.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then
				If demuraStepList1.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then Continue For
				node1 = nodes1(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node1.ChildNodes
					Dim lastChild As XmlNode = node1.LastChild.Clone()
					node1.RemoveAll()
					node1.AppendChild(lastChild)
				Next
				Dim CCID = node1.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node1.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node1.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				SN1 = node1.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				ColorCalSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + CCID)
				ImgScaleSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + IMCID)
				FFCSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + FFID)
				'log.Add("SN : " + SN1 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
			End If
		Next

		For i = 0 To nodes2.Count - 1
			If nodes2(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence2AnaList.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			If (nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
			Not nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
				demuraStepList2.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes2.Count - 1
			If sequence2AnaList.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then
				If demuraStepList2.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then Continue For
				node2 = nodes2(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node2.ChildNodes
					Dim lastChild As XmlNode = node2.LastChild.Clone()
					node2.RemoveAll()
					node2.AppendChild(lastChild)
				Next
				Dim CCID = node2.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node2.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node2.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				SN2 = node2.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				ColorCalSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + CCID)
				ImgScaleSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + IMCID)
				FFCSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + FFID)
				'log.Add("SN : " + SN2 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
			End If
		Next

		If SN1 <> SN2 Then
			CommLogUpdateText("Running sequence and master sequence is from different camera, cannot compare calibration")
			Exit Sub
		Else

		End If

		Dim colorCalRef1 As New Dictionary(Of String, String)
		Dim colorCalRef2 As New Dictionary(Of String, String)

		GetColorCalRef(file1FullPath, colorCalRef1)
		GetColorCalRef(file2FullPath, colorCalRef2)

		Dim imgScaleRef1 As New Dictionary(Of String, String)
		Dim imgScaleRef2 As New Dictionary(Of String, String)

		GetIMGScaleCalRef(file1FullPath, imgScaleRef1)
		GetIMGScaleCalRef(file2FullPath, imgScaleRef2)

		Dim flatFieldRef1 As New Dictionary(Of String, String)
		Dim flatFieldRef2 As New Dictionary(Of String, String)

		GetFFCCalRef(file1FullPath, flatFieldRef1)
		GetFFCCalRef(file2FullPath, flatFieldRef2)

		For index = 0 To ColorCalSetting1.Count - 1
			If ColorCalSetting1(index) <> ColorCalSetting2(index) Then
				equal = False
				log.Add("Step : " + ColorCalSetting1(index).Split(",")(1) + ", Sequence 1 Color Cal : " + colorCalRef1(ColorCalSetting1(index).Split(",")(2)) + ", Sequence 2 Color Cal : " + colorCalRef2(ColorCalSetting2(index).Split(",")(2)))
			End If
		Next

		For index = 0 To ImgScaleSetting1.Count - 1
			If ImgScaleSetting1(index) <> ImgScaleSetting2(index) Then
				equal = False
				log.Add("Step : " + ImgScaleSetting1(index).Split(",")(1) + ", Sequence 1 Img Scale Cal : " + imgScaleRef1(ImgScaleSetting1(index).Split(",")(2)) + ", Sequence 2 Img Scale Cal : " + imgScaleRef2(ImgScaleSetting2(index).Split(",")(2)))
			End If
		Next

		For index = 0 To FFCSetting1.Count - 1
			If FFCSetting1(index) <> FFCSetting2(index) Then
				equal = False
				log.Add("Step : " + FFCSetting1(index).Split(",")(1) + ", Sequence 1 FFC Cal : " + flatFieldRef1(FFCSetting1(index).Split(",")(2)) + ", Sequence 2 FFC Cal : " + flatFieldRef2(FFCSetting2(index).Split(",")(2)))
			End If
		Next

		If equal Then
			CommLogUpdateText("NO CAL DIFFERENCE !!!")
		Else
			CommLogUpdateText("FOUND CAL DIFFERENCE !!!")
		End If

		For i = 0 To log.Count - 1
			CommLogUpdateText(log(i))
		Next
	End Sub

	Public Sub GetColorCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
		Dim conn As SqlCeConnection
		Dim cmdColorCalibration As New SqlCeCommand
		Dim daColorCalibration As New SqlCeDataAdapter
		Dim dsColorCalibration As New DataSet
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim xmlDoc = New XmlDocument()
		Dim node As XmlNode
		Dim nodes As XmlNodeList
		xmlDoc.Load(SequencePath)
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes.Count - 1
			If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes.Count - 1
			If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

				node = nodes(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node.ChildNodes
					Dim lastChild As XmlNode = node.LastChild.Clone()
					node.RemoveAll()
					node.AppendChild(lastChild)
				Next
				SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
			End If
		Next
		Try
			dsColorCalibration.Clear()
			conn = GetConnect(SN)
			cmdColorCalibration = conn.CreateCommand
			cmdColorCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations"
			daColorCalibration.SelectCommand = cmdColorCalibration
			daColorCalibration.Fill(dsColorCalibration, "ColorCalibrations")
			Dim newNoneRow = dsColorCalibration.Tables("ColorCalibrations").NewRow()
			newNoneRow(0) = "0"
			newNoneRow(1) = "(None)"
			dsColorCalibration.Tables("ColorCalibrations").Rows.InsertAt(newNoneRow, 0)
			Dim newFactoryRow = dsColorCalibration.Tables("ColorCalibrations").NewRow()
			newFactoryRow(0) = "-1"
			newFactoryRow(1) = "Factory"
			dsColorCalibration.Tables("ColorCalibrations").Rows.InsertAt(newFactoryRow, 0)
		Catch ex As Exception
			CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
			Exit Sub
		End Try

		For Each Row As DataRow In dsColorCalibration.Tables("ColorCalibrations").Rows
			RefDict.Add(Row(0), Row(1))
		Next
	End Sub

	Public Sub GetIMGScaleCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
		Dim conn As SqlCeConnection
		Dim cmdImgScaleCalibration As New SqlCeCommand
		Dim daImgScaleCalibration As New SqlCeDataAdapter
		Dim dsImgScaleCalibration As New DataSet
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim xmlDoc = New XmlDocument()
		Dim node As XmlNode
		Dim nodes As XmlNodeList
		xmlDoc.Load(SequencePath)
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes.Count - 1
			If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes.Count - 1
			If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

				node = nodes(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node.ChildNodes
					Dim lastChild As XmlNode = node.LastChild.Clone()
					node.RemoveAll()
					node.AppendChild(lastChild)
				Next
				SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
			End If
		Next
		Try
			dsImgScaleCalibration.Clear()
			conn = GetConnect(SN)
			cmdImgScaleCalibration = conn.CreateCommand
			cmdImgScaleCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration"
			daImgScaleCalibration.SelectCommand = cmdImgScaleCalibration
			daImgScaleCalibration.Fill(dsImgScaleCalibration, "ImageScalingCalibration")
			Dim newNoneRow = dsImgScaleCalibration.Tables("ImageScalingCalibration").NewRow()
			newNoneRow(0) = "0"
			newNoneRow(1) = "(None)"
			dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows.InsertAt(newNoneRow, 0)
			Dim newFactoryRow = dsImgScaleCalibration.Tables("ImageScalingCalibration").NewRow()
			newFactoryRow(0) = "-1"
			newFactoryRow(1) = "Factory"
			dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows.InsertAt(newFactoryRow, 0)
		Catch ex As Exception
			CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
			Exit Sub
		End Try

		For Each Row As DataRow In dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows
			RefDict.Add(Row(0), Row(1))
		Next
	End Sub

	Public Sub GetFFCCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
		Dim conn As SqlCeConnection
		Dim cmdFlatFieldCalibration As New SqlCeCommand
		Dim daImgScaleCalibration As New SqlCeDataAdapter
		Dim dsFlatFieldCalibration As New DataSet
		Dim SN As String = ""
		Dim sequenceAnaList As New List(Of String)
		Dim xmlDoc = New XmlDocument()
		Dim node As XmlNode
		Dim nodes As XmlNodeList
		xmlDoc.Load(SequencePath)
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes.Count - 1
			If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes.Count - 1
			If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

				node = nodes(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node.ChildNodes
					Dim lastChild As XmlNode = node.LastChild.Clone()
					node.RemoveAll()
					node.AppendChild(lastChild)
				Next
				SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
			End If
		Next
		Try
			dsFlatFieldCalibration.Clear()
			conn = GetConnect(SN)
			cmdFlatFieldCalibration = conn.CreateCommand
			cmdFlatFieldCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration"
			daImgScaleCalibration.SelectCommand = cmdFlatFieldCalibration
			daImgScaleCalibration.Fill(dsFlatFieldCalibration, "FlatFieldCalibration")
			Dim newNoneRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
			newNoneRow(0) = "0"
			newNoneRow(1) = "(None)"
			dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newNoneRow, 0)
			Dim newFactoryRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
			newFactoryRow(0) = "-1"
			newFactoryRow(1) = "Factory"
			dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newFactoryRow, 0)
		Catch ex As Exception
			CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
			Exit Sub
		End Try

		For Each Row As DataRow In dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows
			RefDict.Add(Row(0), Row(1))
		Next
	End Sub

	Public Function GetConnect(ByVal CameraSN)
		Dim conn As SqlCeConnection
		Dim calFile1 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", CameraSN + "_CalibrationDB.calx")
		Dim calFile2 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", "0" + CameraSN + "_CalibrationDB.calx")
		If File.Exists(calFile1) Then
			conn = New SqlCeConnection("Data Source=" + calFile1 + ";Max Database Size=4091")
		ElseIf File.Exists(calFile2) Then
			conn = New SqlCeConnection("Data Source=" + calFile2 + ";Max Database Size=4091")
		Else
			conn = New SqlCeConnection("Data Source=C:\Radiant Vision Systems Data\Camera Data\Calibration Files\PM Calibration Demo Camera.calx;Max Database Size=4091")
		End If

		Return conn
	End Function

	Private Sub btnCompareAppdata_Click(sender As Object, e As EventArgs) Handles btnCompareAppdata.Click

		Dim AppDataFolder As String = "C:\Radiant Vision Systems Data\TrueTest\AppData"
		Dim MasterAppDataFolder As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
		If Not Directory.Exists(MasterAppDataFolder) Then
			Directory.CreateDirectory(MasterAppDataFolder)
		End If
		Dim files As IEnumerable(Of String) = IO.Directory.EnumerateFiles(MasterAppDataFolder)
		If files.Count = 0 Then
			CommLogUpdateText3("No files in Master AppData folder to compare ! ")
			Exit Sub
		End If
		Dim runningFileMissing As Boolean
		Dim ngFiles As New List(Of String)
		Dim okFiles As New List(Of String)
		For Each masterFile As String In files
			Dim filename = Path.GetFileName(masterFile)
			Dim runningFile As String = Path.Combine(AppDataFolder, filename)
			If Not File.Exists(runningFile) Then
				runningFileMissing = True
				CommLogUpdateText3("NG AppData file : " + runningFile + " is not existed or deleted !")
				Continue For
			End If
			If {"xml", "csv", "txt"}.Contains(Path.GetExtension(masterFile).Remove(0, 1)) Then
				Dim compareProcess As Process = New Process
				Dim startinfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo()
				startinfo.FileName = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "WinMergeU.exe")
				startinfo.Arguments = "-noninteractive -minimize -enableexitcode -cfg Settings/DiffContextV2=0 " + Chr(34) + runningFile + Chr(34) + " " + Chr(34) + masterFile + Chr(34)
				compareProcess = Process.Start(startinfo)

				If compareProcess.WaitForExit(15000) Then
					Dim ExitCode As Integer = compareProcess.ExitCode
					If ExitCode <> 0 Then
						ngFiles.Add(runningFile)
					Else
						okFiles.Add(runningFile)
					End If
				Else
					CommLogUpdateText3("Timed out comparing appdata files")
				End If

			ElseIf {"sdf"}.Contains(Path.GetExtension(masterFile).Remove(0, 1)) Then
				Dim masterInfo As FileInfo = New FileInfo(masterFile)
				Dim runningInfo As FileInfo = New FileInfo(runningFile)
				Dim masterSize As Long = masterInfo.Length
				Dim runningSize As Long = runningInfo.Length
				If masterSize <> runningSize Then
					ngFiles.Add(runningFile)
				Else
					okFiles.Add(runningFile)
				End If
			End If

		Next
		If okFiles.Count > 0 Then
			For Each item As String In okFiles
				CommLogUpdateText3("OK AppData file : " + item)
			Next
		End If
		If ngFiles.Count > 0 Or runningFileMissing Then
			For Each item As String In ngFiles
				CommLogUpdateText3("NG AppData file : " + item)
			Next
			CommLogUpdateText3("Checking Appdata File Finished, NG")
		Else
			CommLogUpdateText3("Checking Appdata File Finished, OK")
		End If

	End Sub
End Class
