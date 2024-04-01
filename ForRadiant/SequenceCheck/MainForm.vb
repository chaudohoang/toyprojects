Imports System.Data.SqlServerCe
Imports System.IO
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
		getCurrentCameraSN()
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
		Dim timeLogString As New List(Of String)
		Dim tempString As String = ""
		Dim sw As New Stopwatch
		sw.Start()
		Dim equal As Boolean = True
		Dim log As New List(Of String)

		Dim sw2 As New Stopwatch
		sw2.Start()

		If Not File.Exists(file1FullPath) Then
			equal = False
			CommLogUpdateText("Parameters Check : Sequence 1 is not existed !!!")
			Exit Sub
		ElseIf Not File.Exists(file2FullPath) Then
			equal = False
			CommLogUpdateText("Parameters Check : Sequence 2 is not existed !!!")

			Exit Sub

		ElseIf file1FullPath = file2FullPath Then
			equal = False
			CommLogUpdateText("Parameters Check : Sequence 1 and Sequence 2 is the same file !!!")
			Exit Sub
		End If
		sw2.Stop()
		timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		Dim ignoreList As List(Of String)
		ignoreList = (From s In cbxIgnoreList.Text.Split(CChar(","))
					  Select s).ToList()

		sw2.Stop()
		timeLogString.Add("Get ignore parameters : " + sw2.ElapsedMilliseconds.ToString + "ms")
		If cbxIgnoreList.Text <> "" Then timeLogString.Add(cbxIgnoreList.Text)

		Dim sequence1AnaList As New List(Of String)
		Dim sequence2AnaList As New List(Of String)

		Dim node1 As XmlNode
		Dim nodes1 As XmlNodeList

		Dim node2 As XmlNode
		Dim nodes2 As XmlNodeList

		Dim xmlDoc1 = New XmlDocument()
		Dim xmlDoc2 = New XmlDocument()
		Dim fileloaded As Boolean
		While Not fileloaded
			Try
				xmlDoc1.Load(file1FullPath)
				xmlDoc2.Load(file2FullPath)
				fileloaded = True
			Catch ex As Exception
				fileloaded = False
			End Try
		End While

		nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
		nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")

		sw2.Restart()
		'conpare sequence analysis list
		For i1 = 0 To nodes1.Count - 1
			If nodes1(i1).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence1AnaList.Add(nodes1(i1).SelectSingleNode("PatternSetupName").InnerText + "_" + (nodes1(i1).SelectSingleNode("Analysis/UserName").InnerText))
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Get sequence 1 analysis list : " + sw2.ElapsedMilliseconds.ToString + "ms")
		timeLogString.Add("Sequence 1 analysis : " + String.Join(",", sequence1AnaList))

		sw2.Restart()
		For i2 = 0 To nodes2.Count - 1
			If nodes2(i2).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence2AnaList.Add(nodes2(i2).SelectSingleNode("PatternSetupName").InnerText + "_" + (nodes2(i2).SelectSingleNode("Analysis/UserName").InnerText))
			End If
		Next

		sw2.Stop()
		timeLogString.Add("Get sequence 2 analysis list : " + sw2.ElapsedMilliseconds.ToString + "ms")
		timeLogString.Add("Sequence 2 analysis : " + String.Join(",", sequence2AnaList))


		sw2.Restart()

		If String.Join(",", sequence1AnaList) <> String.Join(",", sequence2AnaList) Then
			equal = False
			CommLogUpdateText("Parameters Check : Analysis list does not match !!!")
			CommLogUpdateText("Parameters Check : Sequence 1 analyses : " + String.Join(",", sequence1AnaList))
			CommLogUpdateText("Parameters Check : Sequence 2 analyses : " + String.Join(",", sequence2AnaList))
			Exit Sub
		End If
		sw2.Stop()
		timeLogString.Add("Check if 2 sequences having same number of analysis : " + sw2.ElapsedMilliseconds.ToString + "ms" + Environment.NewLine)

		Dim SequenceItemCount As Integer
		If nodes1.Count < nodes2.Count Then
			SequenceItemCount = nodes1.Count
		Else
			SequenceItemCount = nodes2.Count
		End If
		For index = 0 To SequenceItemCount - 1
			If Not nodes1(index).SelectSingleNode("Selected").InnerText.ToLower = "true" Or Not nodes2(index).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				Continue For
			End If
			node1 = nodes1(index).SelectSingleNode("Analysis")
			node2 = nodes2(index).SelectSingleNode("Analysis")

			Dim seq1AnalysisName = nodes1(index).SelectSingleNode("PatternSetupName").InnerText + "_" + nodes1(index).SelectSingleNode("Analysis/UserName").InnerText
			Dim seq2AnalysisName = nodes2(index).SelectSingleNode("PatternSetupName").InnerText + "_" + nodes2(index).SelectSingleNode("Analysis/UserName").InnerText
			timeLogString.Add("Checking step : " + seq1AnalysisName)
			sw2.Restart()
			'Remove items in ignoreList

			For Each item As String In ignoreList
				For Each childNode1 As XmlNode In node1.ChildNodes
					If childNode1.Name.ToLower = item.ToLower Then
						node1.RemoveChild(childNode1)
						tempString = tempString + childNode1.Name + ","
					End If
				Next
			Next
			sw2.Stop()
			timeLogString.Add("Sequence 1 remove ignored parameters in Analysis " + seq1AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
			tempString = ""
			sw2.Restart()
			For Each item As String In ignoreList
				For Each childNode2 As XmlNode In node2.ChildNodes
					If childNode2.Name.ToLower = item.ToLower Then
						node2.RemoveChild(childNode2)
						tempString = tempString + childNode2.Name + ","
					End If
				Next
			Next
			sw2.Stop()
			timeLogString.Add("Sequence 2 remove ignored parameters in Analysis " + seq2AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
			tempString = ""
			sw2.Restart()
			For childindex = node1.ChildNodes.Count - 1 To 0 Step -1
				If node2.SelectSingleNode(node1.ChildNodes(childindex).Name) Is Nothing Then
					tempString = tempString + node1.ChildNodes(childindex).Name + ","
					node1.RemoveChild(node1.ChildNodes(childindex))
				End If
			Next
			sw2.Stop()
			Dim seq2NeedSorting As Boolean
			seq2NeedSorting = If(tempString.Length = 0, False, True)
			timeLogString.Add("Sequence 1 removing extra element in Analysis " + seq1AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
			tempString = ""

			sw2.Restart()
			For childIndex = node2.ChildNodes.Count - 1 To 0 Step -1
				If node1.SelectSingleNode(node2.ChildNodes(childIndex).Name) Is Nothing Then
					tempString = tempString + node2.ChildNodes(childIndex).Name + ","
					node2.RemoveChild(node2.ChildNodes(childIndex))
				End If
			Next
			sw2.Stop()
			Dim seq1NeedSorting As Boolean
			seq1NeedSorting = If(tempString.Length = 0, False, True)
			timeLogString.Add("Sequence 2 removing extra element in Analysis " + seq2AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
			tempString = ""

			sw2.Restart()

			For childIndex = 0 To node1.ChildNodes.Count - 1
				If node1.ChildNodes(childIndex).Name.Contains("FilterList") AndAlso
					node1.ChildNodes(childIndex).SelectSingleNode("FilterList") IsNot Nothing AndAlso
					node2.ChildNodes(childIndex).SelectSingleNode("FilterList") IsNot Nothing Then

					Dim FLXmlNode1 As XmlNode = node1.ChildNodes(childIndex).SelectSingleNode("FilterList")
					Dim FLXmlNode2 As XmlNode = node2.ChildNodes(childIndex).SelectSingleNode("FilterList")

					For FLChildIndex = FLXmlNode1.ChildNodes.Count - 1 To 0 Step -1
						If FLXmlNode1.ChildNodes(FLChildIndex).SelectSingleNode("Selected").InnerText.ToLower = "false" Then Continue For
						For FLGrandChildIndex = FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes.Count - 1 To 0 Step -1
							If FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).Name = "ElapsedMilliseconds" Then
								FLXmlNode1.ChildNodes(FLChildIndex).RemoveChild(FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex))
							End If
							If FLXmlNode2.ChildNodes(FLChildIndex).SelectSingleNode(FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).Name) Is Nothing Then
								FLXmlNode1.ChildNodes(FLChildIndex).RemoveChild(FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex))
							End If
						Next
					Next

					For FLChildIndex = FLXmlNode2.ChildNodes.Count - 1 To 0 Step -1
						If FLXmlNode2.ChildNodes(FLChildIndex).SelectSingleNode("Selected").InnerText.ToLower = "false" Then Continue For
						For FLGrandChildIndex = FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes.Count - 1 To 0 Step -1
							If FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).Name = "ElapsedMilliseconds" Then
								FLXmlNode2.ChildNodes(FLChildIndex).RemoveChild(FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex))
							End If
							If FLXmlNode1.ChildNodes(FLChildIndex).SelectSingleNode(FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).Name) Is Nothing Then
								FLXmlNode2.ChildNodes(FLChildIndex).RemoveChild(FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex))
							End If
						Next
					Next

					For FLChildIndex = 0 To FLXmlNode1.ChildNodes.Count - 1
						If FLXmlNode1.ChildNodes(FLChildIndex).SelectSingleNode("Selected").InnerText.ToLower = "false" Then Continue For
						For FLGrandChildIndex = FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes.Count - 1 To 0 Step -1
							If FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).InnerText.ToLower <> FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).InnerText.ToLower Then
								equal = False
								log.Add("Step : " + seq1AnalysisName + ", FilterList Parameter : " + FLXmlNode1.ChildNodes(FLChildIndex).Name + "/" + FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).Name + ", Sequence 1 Value : " + FLXmlNode1.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).InnerText + ", Sequence 2 Value : " + FLXmlNode2.ChildNodes(FLChildIndex).ChildNodes(FLGrandChildIndex).InnerText)
							End If
						Next
					Next
				Else
					If node1.ChildNodes(childIndex).InnerText.ToLower <> node2.ChildNodes(childIndex).InnerText.ToLower Then
						equal = False
						log.Add("Step : " + seq1AnalysisName + ", Parameter : " + node1.ChildNodes(childIndex).Name + ", Sequence 1 Value : " + node1.ChildNodes(childIndex).InnerText + ", Sequence 2 Value : " + node2.ChildNodes(childIndex).InnerText)
					End If
				End If

				tempString = tempString + node1.ChildNodes(childIndex).Name + ","
			Next
			sw2.Stop()

			timeLogString.Add("Comparing done for analysis step : " + seq1AnalysisName + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
			timeLogString.Add("Compared " + node1.ChildNodes.Count.ToString + " parameters : " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + Environment.NewLine)
			tempString = ""
		Next

		If equal Then
			CommLogUpdateText("NO PARAMETER DIFFERENCE !!!")
		Else
			CommLogUpdateText("FOUND PARAMETER DIFFERENCE !!!")
		End If

		sw.Stop()
		timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString + "ms")

		For i = 0 To log.Count - 1
			CommLogUpdateText(log(i))
		Next

		CommLogUpdateText("Compare Parameters Time : " + (sw.ElapsedMilliseconds / 1000).ToString + "s")
		'File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"), timeLogString)
		'Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"))

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
		If text <> vbCrLf Then
			ListBox1.Items.Add(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text)
		Else
			ListBox1.Items.Add(text)
		End If
		ListBox1.TopIndex = ListBox1.Items.Count - 1
	End Sub

	Private Sub btnClearlog_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click
		ListBox1.Items.Clear()
	End Sub

	Private Sub CommLogUpdateText2(text As String)
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText2), New Object() {text}) : Exit Sub
		If text <> vbCrLf Then
			ListBox2.Items.Add(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text)
		Else
			ListBox2.Items.Add(text)
		End If
		ListBox2.TopIndex = ListBox2.Items.Count - 1
	End Sub

	Private Sub btnClearlog2_Click(sender As Object, e As EventArgs) Handles btnClearlog2.Click
		ListBox2.Items.Clear()
	End Sub
	Private Sub CommLogUpdateText3(text As String)
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText3), New Object() {text}) : Exit Sub
		If text <> vbCrLf Then
			ListBox3.Items.Add(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text)
		Else
			ListBox3.Items.Add(text)
		End If
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
		getCurrentCameraSN2()
		CheckSequence(txtFile3.Text)
		If Not String.IsNullOrEmpty(txtAdditionalSequence.Text) Then
			Try
				Dim additionalTargets As String() = txtAdditionalSequence.Text.Split(New Char() {ChrW(13), ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
				For Each item As String In additionalTargets
					CommLogUpdateText2(vbCrLf)
					CheckSequence(item)
				Next
			Catch ex As Exception
			End Try
		End If

		If ListBox2.Items.Count > 0 AndAlso ListBox2.Items(ListBox2.Items.Count - 1).ToString() <> vbCrLf Then
			CommLogUpdateText2(vbCrLf)
		End If
		btnCheck.Enabled = True

	End Sub

	Public Sub CheckSequence(InputSequence As String)
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
		Dim SN As String = ""
		If Not Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules") Then
			Directory.CreateDirectory("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules")
		End If
		Dim ColorCalRuleFilaName As String = Path.GetFileNameWithoutExtension(InputSequence) + "_colorcal.txt"
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

		Dim FlatFieldCalRuleFilaName As String = Path.GetFileNameWithoutExtension(InputSequence) + "_flatfieldcal.txt"
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

		Dim ImgScaleCalRuleFilaName As String = Path.GetFileNameWithoutExtension(InputSequence) + "_imgscalecal.txt"
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

		xmlDoc3.Load(InputSequence)
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
				If cbCameraSNStyle2.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node3.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node3.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node3.ChildNodes
						Dim lastChild As XmlNode = node3.LastChild.Clone()
						node3.RemoveAll()
						node3.AppendChild(lastChild)
					Next
				End If
				Try
					SN = ""
					SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try
				If SN = "" Then
					Exit For
				End If

				Dim subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
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

				If cbCameraSNStyle2.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node3.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node3.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node3.ChildNodes
						Dim lastChild As XmlNode = node3.LastChild.Clone()
						node3.RemoveAll()
						node3.AppendChild(lastChild)
					Next
				End If
				Try
					SN = ""
					SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try

				If SN = "" Then
					Exit For
				End If

				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
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
				If cbCameraSNStyle2.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node3.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node3.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node3.ChildNodes
						Dim lastChild As XmlNode = node3.LastChild.Clone()
						node3.RemoveAll()
						node3.AppendChild(lastChild)
					Next
				End If
				Try
					SN = ""
					SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try

				If SN = "" Then
					Exit For
				End If

				Dim CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
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

				If cbCameraSNStyle2.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node3.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node3.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node3.ChildNodes
						Dim lastChild As XmlNode = node3.LastChild.Clone()
						node3.RemoveAll()
						node3.AppendChild(lastChild)
					Next
				End If
				Try
					SN = ""
					SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try
				If SN = "" Then
					Exit For
				End If

				Dim FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText
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
				If cbCameraSNStyle2.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node3.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node3.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node3.ChildNodes
						Dim lastChild As XmlNode = node3.LastChild.Clone()
						node3.RemoveAll()
						node3.AppendChild(lastChild)
					Next
				End If
				Try
					SN = ""
					SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try

				If SN = "" Then
					Exit For
				End If

				Dim ISCID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				Dim StepName = nodes3(index).SelectSingleNode("Name").InnerText

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

		If cbxSubframe.Text <> "" Or chkCalNone.Checked = True Or chkColorCalSettings.Checked = True Or chkFlatFieldCalSettings.Checked = True Or chkImgScaleCalSettings.Checked = True Then
			CommLogUpdateText2("Local Camera SN : " + CameraSN)
			CommLogUpdateText2("Sequence File : " + InputSequence)
			CommLogUpdateText2("Sequence Camera SN : " + SN)
			If SN = "" Then
				CommLogUpdateText2("Sequence is not set with local Camera, cannot check with local Camera !")
				Exit Sub
			End If
		End If

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
		ShowMeasSettings(txtFile3.Text)
		CommLogUpdateText2(vbCrLf)
		btnShowSettings.Enabled = True
	End Sub

	Public Sub ShowMeasSettings(InputSequence As String)
		Dim sequenceAnaList As New List(Of String)
		Dim demuraStepList As New List(Of String)
		Dim node3 As XmlNode
		Dim nodes3 As XmlNodeList
		Dim xmlDoc3 = New XmlDocument()
		xmlDoc3.Load(InputSequence)
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
		CommLogUpdateText2("Sequence File : " + InputSequence)
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

	Public Sub ShowColorCalSettings(InputSequence As String)
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
		xmlDoc3.Load(InputSequence)
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
		CommLogUpdateText2("Sequence File : " + InputSequence)
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
	Public Sub ShowFlatFieldCalRefs(InputSequence As String)
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
		xmlDoc3.Load(InputSequence)
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
		CommLogUpdateText2("Sequence File : " + InputSequence)
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
	Public Sub ShowImgScaleCalRefs(InputSequence As String)
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
		xmlDoc3.Load(InputSequence)
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
		CommLogUpdateText2("Sequence File : " + InputSequence)
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
				Process.Start("notepad", savefile.FileName)
			End If

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
				Process.Start("notepad", savefile.FileName)
			End If

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
				Process.Start("notepad", savefile.FileName)
			End If

		End If
	End Sub

	Private Sub btnShowCalSettings_Click(sender As Object, e As EventArgs) Handles btnShowCalSettings.Click
		btnShowCalSettings.Enabled = False
		ShowColorCalSettings(txtFile3.Text)
		ShowFlatFieldCalRefs(txtFile3.Text)
		ShowImgScaleCalRefs(txtFile3.Text)
		CommLogUpdateText2(vbCrLf)
		btnShowCalSettings.Enabled = True

	End Sub

	Private Sub btnEditCalRule_Click(sender As Object, e As EventArgs) Handles btnEditCalRule.Click
		CalRule.Show()
	End Sub

	Private Sub getCurrentCameraSN()

		If File.Exists("C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt") Then
			Dim fileloaded As Boolean
			While Not fileloaded
				Try
					CameraSN = File.ReadAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt")
					fileloaded = True
				Catch ex As Exception
					fileloaded = False
				End Try
			End While

		End If
		'CommLogUpdateText("Local Camera SN : " + CameraSN)

	End Sub

	Private Sub getCurrentCameraSN2()

		If File.Exists("C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt") Then
			Dim fileloaded As Boolean
			While Not fileloaded
				Try
					CameraSN = File.ReadAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt")
					fileloaded = True
				Catch ex As Exception
					fileloaded = False
				End Try
			End While

		End If
		'CommLogUpdateText2("Local Camera SN : " + CameraSN)

	End Sub

	Public Sub CompareCalSettings(ByVal file1FullPath As String, ByVal file2FullPath As String)
		If Not chkCompareCAL.Checked Then
			CommLogUpdateText("SKIPPED CAL COMPARISION !!!")
			Exit Sub
		End If
		Dim timeLogString As New List(Of String)
		Dim tempString As String = ""
		Dim sw As New Stopwatch
		sw.Start()
		Dim equal As Boolean = True
		Dim log As New List(Of String)
		Dim sw2 As New Stopwatch
		sw2.Start()

		CommLogUpdateText("Local Camera SN : " + CameraSN)

		If Not File.Exists(file1FullPath) Then
			equal = False
			CommLogUpdateText("Calibrations Check : Sequence 1 is not existed !!!")
			Exit Sub
		ElseIf Not File.Exists(file2FullPath) Then
			equal = False
			CommLogUpdateText("Calibrations Check : Sequence 2 is not existed !!!")

			Exit Sub
		ElseIf file1FullPath = file2FullPath Then
			equal = False
			CommLogUpdateText("Calibrations Check : Sequence 1 and Sequence 2 is the same file !!!")
			Exit Sub
		End If
		sw2.Stop()
		timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString + "ms")
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
		sw2.Restart()

		For i = 0 To nodes1.Count - 1
			If nodes1(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence1AnaList.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			'If (nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
			'Not nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
			'	demuraStepList1.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
			'End If
		Next
		sequence1AnaList = sequence1AnaList.Distinct.ToList()
		sw2.Stop()
		timeLogString.Add("Get sequence 1 analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		For index = 0 To nodes1.Count - 1

			If sequence1AnaList.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then
				'If demuraStepList1.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then Continue For
				node1 = nodes1(index).SelectSingleNode("CameraSettingsList")
				If cbCameraSNStyle.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node1.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node1.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node1.ChildNodes
						Dim lastChild As XmlNode = node1.LastChild.Clone()
						node1.RemoveAll()
						node1.AppendChild(lastChild)
					Next
				End If
				Try
					SN1 = ""
					SN1 = node1.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try
				If SN1 = "" Then
					Exit For
				End If

				Dim CCID = node1.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node1.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node1.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				ColorCalSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + CCID)
				ImgScaleSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + IMCID)
				FFCSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + FFID)
				'log.Add("SN : " + SN1 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Get sequence 1 Serial Number : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()

		For i = 0 To nodes2.Count - 1
			If nodes2(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
				sequence2AnaList.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
			End If
			'If (nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
			'Not nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
			'	demuraStepList2.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
			'End If
		Next
		sequence2AnaList = sequence2AnaList.Distinct.ToList()
		sw2.Stop()
		timeLogString.Add("Get sequence 2 analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
		For index = 0 To nodes2.Count - 1
			If sequence2AnaList.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then
				'If demuraStepList2.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then Continue For
				node2 = nodes2(index).SelectSingleNode("CameraSettingsList")
				If cbCameraSNStyle.SelectedIndex = 0 Then
					For Each childNode As XmlNode In node2.ChildNodes
						If childNode.SelectSingleNode("SerialNumber").InnerText <> CameraSN Then
							node2.RemoveChild(childNode)
						End If
					Next
				Else
					For Each childNode As XmlNode In node2.ChildNodes
						Dim lastChild As XmlNode = node2.LastChild.Clone()
						node2.RemoveAll()
						node2.AppendChild(lastChild)
					Next
				End If
				Try
					SN2 = ""
					SN2 = node2.SelectSingleNode("CameraSettings/SerialNumber").InnerText
				Catch ex As Exception
				End Try
				If SN2 = "" Then
					Exit For
				End If
				Dim CCID = node2.SelectSingleNode("CameraSettings/ColorCalID").InnerText
				Dim IMCID = node2.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
				Dim FFID = node2.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
				ColorCalSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + CCID)
				ImgScaleSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + IMCID)
				FFCSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + FFID)
				'log.Add("SN : " + SN2 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Get sequence 2 Serial Number : " + sw2.ElapsedMilliseconds.ToString + "ms")

		If SN1 = "" Then
			CommLogUpdateText("Sequence 1 Camera SN : " + SN1)
			CommLogUpdateText("Sequence 2 Camera SN : " + SN2)
			CommLogUpdateText("Running Sequence is copied but not set calibraion !")
			Exit Sub
		End If
		If SN2 = "" Then
			CommLogUpdateText("Sequence 1 Camera SN : " + SN1)
			CommLogUpdateText("Sequence 2 Camera SN : " + SN2)
			CommLogUpdateText("Calibration Sequence is copied but not set calibraion !")
			Exit Sub
		End If

		If SN1 <> "" AndAlso SN2 <> "" AndAlso SN1 <> SN2 Then
			CommLogUpdateText("Sequence 1 Camera SN : " + SN1)
			CommLogUpdateText("Sequence 2 Camera SN : " + SN2)
			CommLogUpdateText("2 sequences are set from different cameras !")
			Exit Sub
		End If

		Dim colorCalRef1 As New Dictionary(Of String, String)
		Dim colorCalRef2 As New Dictionary(Of String, String)

		sw2.Restart()
		GetColorCalRef(file1FullPath, colorCalRef1)
		GetColorCalRef(file2FullPath, colorCalRef2)
		sw2.Stop()
		timeLogString.Add("Get color calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

		Dim imgScaleRef1 As New Dictionary(Of String, String)
		Dim imgScaleRef2 As New Dictionary(Of String, String)

		sw2.Restart()
		GetIMGScaleCalRef(file1FullPath, imgScaleRef1)
		GetIMGScaleCalRef(file2FullPath, imgScaleRef2)
		sw2.Stop()
		timeLogString.Add("Get image scaling calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

		Dim flatFieldRef1 As New Dictionary(Of String, String)
		Dim flatFieldRef2 As New Dictionary(Of String, String)

		sw2.Restart()
		GetFFCCalRef(file1FullPath, flatFieldRef1)
		GetFFCCalRef(file2FullPath, flatFieldRef2)
		sw2.Stop()
		timeLogString.Add("Get flat field calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		For index = 0 To ColorCalSetting1.Count - 1
			If ColorCalSetting1(index).Split(",").Reverse()(0) = "0" Then
				equal = False
				log.Add("Step : " + ColorCalSetting1(index).Split(",")(1) + ", Sequence 1 Color Cal is (None)")
			ElseIf ColorCalSetting1(index) <> ColorCalSetting2(index) Then
				equal = False
				log.Add("Step : " + ColorCalSetting1(index).Split(",")(1) + ", Sequence 1 Color Cal : " + colorCalRef1(ColorCalSetting1(index).Split(",")(2)) + ", Sequence 2 Color Cal : " + colorCalRef2(ColorCalSetting2(index).Split(",")(2)))
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Done checking color calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		For index = 0 To ImgScaleSetting1.Count - 1
			If ImgScaleSetting1(index).Split(",").Reverse()(0) = "0" Then
				equal = False
				log.Add("Step : " + ImgScaleSetting1(index).Split(",")(1) + ", Sequence 1 Img Scale Cal is (None)")
			ElseIf ImgScaleSetting1(index) <> ImgScaleSetting2(index) Then
				equal = False
				log.Add("Step : " + ImgScaleSetting1(index).Split(",")(1) + ", Sequence 1 Img Scale Cal : " + imgScaleRef1(ImgScaleSetting1(index).Split(",")(2)) + ", Sequence 2 Img Scale Cal : " + imgScaleRef2(ImgScaleSetting2(index).Split(",")(2)))
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Done checking image scaling calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

		sw2.Restart()
		For index = 0 To FFCSetting1.Count - 1
			If FFCSetting1(index).Split(",").Reverse()(0) = "0" Then
				equal = False
				log.Add("Step : " + FFCSetting1(index).Split(",")(1) + ", Sequence 1 FFC Cal is (None)")
			ElseIf FFCSetting1(index) <> FFCSetting2(index) Then
				equal = False
				log.Add("Step : " + FFCSetting1(index).Split(",")(1) + ", Sequence 1 FFC Cal : " + flatFieldRef1(FFCSetting1(index).Split(",")(2)) + ", Sequence 2 FFC Cal : " + flatFieldRef2(FFCSetting2(index).Split(",")(2)))
			End If
		Next
		sw2.Stop()
		timeLogString.Add("Done checking flat field calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

		CommLogUpdateText("Sequence 1 Camera SN : " + SN1)
		CommLogUpdateText("Sequence 2 Camera SN : " + SN2)

		If equal Then
			CommLogUpdateText("NO CAL DIFFERENCE !!!")
		Else
			CommLogUpdateText("FOUND CAL DIFFERENCE !!!")
		End If

		sw.Stop()
		timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString + "ms")

		For i = 0 To log.Count - 1
			CommLogUpdateText(log(i))
		Next

		CommLogUpdateText("Compare Calibrations Time : " + (sw.ElapsedMilliseconds / 1000).ToString + "s")
		'File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"), timeLogString)
		'Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"))

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
	Private Sub CompareAppDataFiles()
		Dim sw As New Stopwatch
		sw.Start()
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
		If Not File.Exists(System.IO.Path.Combine(My.Application.Info.DirectoryPath, "WinMergeU.exe")) Then
			MessageBox.Show(System.IO.Path.Combine(My.Application.Info.DirectoryPath, "WinMergeU.exe") + " not existed, copy WinMergeU.exe to same folder and try again !")
			Exit Sub
		End If
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
		sw.Stop()
		CommLogUpdateText3("Elapsed time : " + (sw.ElapsedMilliseconds / 1000).ToString + " seconds.")
	End Sub

	Private Sub btnCompareAppdata_Click(sender As Object, e As EventArgs) Handles btnCompareAppdata.Click
		CompareAppDataFiles()
	End Sub

	Private Sub btnBrowseAdditional_Click(sender As Object, e As EventArgs) Handles btnBrowseAdditional.Click
		Using dialog As New OpenFileDialog()
			dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\Sequence"
			dialog.Multiselect = True

			If dialog.ShowDialog(Me) = DialogResult.OK Then
				Dim files As String() = dialog.FileNames

				For Each file As String In files
					txtAdditionalSequence.Text += file & vbCrLf
				Next
			End If
		End Using
	End Sub

	Private Sub txtAdditionalSequence_DragDrop(sender As Object, e As DragEventArgs) Handles txtAdditionalSequence.DragDrop
		e.Effect = DragDropEffects.Copy
		Dim files As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())

		If files IsNot Nothing AndAlso files.Length <> 0 Then
			For Each file As String In files
				txtAdditionalSequence.Text += file & vbCrLf
			Next
		End If
	End Sub

	Private Sub txtAdditionalSequence_DragEnter(sender As Object, e As DragEventArgs) Handles txtAdditionalSequence.DragEnter
		e.Effect = DragDropEffects.Copy
	End Sub

	Private Sub btnClearAdditional_Click(sender As Object, e As EventArgs) Handles btnClearAdditional.Click
		txtAdditionalSequence.Text = ""
	End Sub
End Class
