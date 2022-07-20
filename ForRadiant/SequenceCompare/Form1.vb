Imports System.Text
Imports System.Reflectioin
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
		If Not File.Exists(file1FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 1 is not existed")
		ElseIf Not File.Exists(file2FullPath) Then
			equal = False
			CommLogUpdateText("Sequence 2 is not existed")

			Exit Sub
		ElseIf file1FullPath = file2FullPath Then
			equal = False
			CommLogUpdateText("Sequence 1 and Sequence 2 is same file")
			Exit Sub
		End If

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
			sequence1AnaList.Add(nodes1(i1).SelectSingleNode("PatternSetupName").InnerText)
		Next

		For i2 = 0 To nodes2.Count - 1
			sequence2AnaList.Add(nodes2(i2).SelectSingleNode("PatternSetupName").InnerText)
		Next

		If String.Join(",", sequence1AnaList) <> String.Join(",", sequence2AnaList) Then
			equal = False
			CommLogUpdateText("Analysis list does not match")
			CommLogUpdateText("Sequence 1 analyses : " + String.Join(",", sequence1AnaList))
			CommLogUpdateText("Sequence 2 analyses : " + String.Join(",", sequence2AnaList))
			Exit Sub
		End If


		For index = 0 To nodes1.Count - 1
			node1 = nodes1(index).SelectSingleNode("Analysis")
			node2 = nodes2(index).SelectSingleNode("Analysis")

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
					CommLogUpdateText("Step : " + nodes1(index).SelectSingleNode("PatternSetupName").InnerText + ", Parameter : " + node1.ChildNodes(childIndex).Name + ", File1 Value : " + node1.ChildNodes(childIndex).InnerText + ", File2 Value : " + node2.ChildNodes(childIndex).InnerText)
				End If

			Next

		Next

		If equal Then
			CommLogUpdateText("Sequence 1 and Sequence 2 have no differences")
		End If

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
		Dim dt As Date = Now
		If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText), New Object() {text}) : Exit Sub
		ListBox1.Items.Add(text)
		ListBox1.TopIndex = ListBox1.Items.Count - 1
	End Sub

	Private Sub btnClearlog_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click
		ListBox1.Items.Clear()
	End Sub
End Class
