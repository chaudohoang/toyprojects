Imports System.IO
Imports System.Xml

Public Class Form1

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

	'Chau added
	'Added ability to drag and drop files to input box
	Private Sub InputTextBox_DragEnter(sender As Object, e As DragEventArgs)

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


		For index = 0 To nodes1.Count - 1
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
			CommLogUpdateText("NO DIFFERENCE !!!")
		Else
			CommLogUpdateText("FOUND DIFFERENCE !!!")
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

	Private Sub btnClearlog2_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click, btnClearlog2.Click
		ListBox2.Items.Clear()
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
		Dim calMatch As Boolean = True
		Dim logSubframe As New List(Of String)
		Dim logCal As New List(Of String)
		Dim sequenceAnaList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

		For index = 0 To nodes3.Count - 1

			If cbxSubframe.Text <> "" AndAlso sequenceAnaList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then
				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				Dim subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				If subframe <> cbxSubframe.Text Then
					subframeMatch = False
					logSubframe.Add("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + ", Subframe : " + subframe)
				End If

			End If

		Next

		For index = 0 To nodes3.Count - 1
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
				Dim SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Dim log As String = ""
				If CCID = 0 Then
					calMatch = False
					log += " , ColorCalID : " + CCID
				End If
				If IMCID = 0 Then
					calMatch = False
					log += " , ImageScalingID : " + IMCID
				End If
				If FFID = 0 Then
					calMatch = False
					log += " , FlatFieldID : " + FFID
				End If
				If log <> "" Then
					logCal.Add("SN : " + SN + " , Step : " + nodes3(index).SelectSingleNode("Name").InnerText + log)
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

		If chkCalNone.Checked = True AndAlso calMatch Then
			CommLogUpdateText2("CALIBRATION ALL SET !!!")
		ElseIf chkCalNone.Checked = True AndAlso Not calMatch Then
			CommLogUpdateText2("CALIBRATION NONE DETECTED !!!")
		Else
		End If

		For i = 0 To logCal.Count - 1
			CommLogUpdateText2(logCal(i))
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
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(txtFile3.Text)
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		For i = 0 To nodes3.Count - 1
			If nodes3(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequenceAnaList.Add(nodes3(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
		Next
		nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		For index = 0 To nodes3.Count - 1
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
End Class
