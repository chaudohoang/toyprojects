Imports System.Data.SqlServerCe
Imports System.IO
Imports System.Net.Http
Imports System.Windows.Forms.LinkLabel
Imports System.Xml

Public Class CalRule
	Public conn As SqlCeConnection
	Public cmdColorCalibration As New SqlCeCommand
	Public daColorCalibration As New SqlCeDataAdapter
	Public dsColorCalibration As New DataSet
	Public dtColorCalibration As New DataTable
	Public cmdFlatFieldCalibration As New SqlCeCommand
	Public daFlatFieldCalibration As New SqlCeDataAdapter
	Public dsFlatFieldCalibration As New DataSet
	Public dtFlatFieldCalibration As New DataTable
	Public cmdImgScaleCalibration As New SqlCeCommand
	Public daImgScaleCalibration As New SqlCeDataAdapter
	Public dsImgScaleCalibration As New DataSet
	Public dtImgScaleCalibration As New DataTable
	Public SN As String = ""
	Public sequenceAnaList As New List(Of String)
	Public node3 As XmlNode
	Public nodes3 As XmlNodeList
	Public xmlDoc3 = New XmlDocument()
	Public exePath As String = My.Application.Info.DirectoryPath
	Public Function GetConnect(ByVal CameraSN)
		Dim calFile1 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", CameraSN + "_CalibrationDB.calx")
		Dim calFile2 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", "0" + CameraSN + "_CalibrationDB.calx")
		If File.Exists(calFile1) Then
			conn = New SqlCeConnection("Data Source=" + calFile1)
		ElseIf File.Exists(calFile2) Then
			conn = New SqlCeConnection("Data Source=" + calFile2)
		Else
			conn = New SqlCeConnection("Data Source=C:\Radiant Vision Systems Data\Camera Data\Calibration Files\PM Calibration Demo Camera.calx")
		End If

		Return conn
	End Function
	Private Sub btnUseLastModified3_Click(sender As Object, e As EventArgs) Handles btnUseLastModified3.Click
		If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
			Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
			txtFile3.Text = latestfile.FullName
		End If
	End Sub

	Private Sub btnBrowse3_Click(sender As Object, e As EventArgs) Handles btnBrowse3.Click
		Using frm = New OpenFileDialog
			If frm.ShowDialog(Me) = DialogResult.OK Then
				txtFile3.Text = frm.FileName
			End If
		End Using
	End Sub

	Private Sub ShowColorCalRefs()

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

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
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
			ColorCalDataGridView1.DataSource = dsColorCalibration
			ColorCalDataGridView1.DataMember = "ColorCalibrations"
			ColorCalDataGridView1.ReadOnly = True
			ColorCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			ColorCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
			ColorCalDataGridView1.AllowUserToResizeColumns = True

		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try
	End Sub
	Private Sub ShowFlatFieldCalRefs()

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

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
			End If
		Next
		Try
			dsFlatFieldCalibration.Clear()
			conn = GetConnect(SN)
			cmdFlatFieldCalibration = conn.CreateCommand
			cmdFlatFieldCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration"

			daFlatFieldCalibration.SelectCommand = cmdFlatFieldCalibration
			daFlatFieldCalibration.Fill(dsFlatFieldCalibration, "FlatFieldCalibration")
			Dim newNoneRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
			newNoneRow(0) = "0"
			newNoneRow(1) = "(None)"
			dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newNoneRow, 0)
			Dim newFactoryRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
			newFactoryRow(0) = "-1"
			newFactoryRow(1) = "Factory"
			dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newFactoryRow, 0)
			FlatFieldCalDataGridView1.DataSource = dsFlatFieldCalibration
			FlatFieldCalDataGridView1.DataMember = "FlatFieldCalibration"
			FlatFieldCalDataGridView1.ReadOnly = True
			FlatFieldCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			FlatFieldCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
			FlatFieldCalDataGridView1.AllowUserToResizeColumns = True

		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try
	End Sub
	Private Sub ShowImgScaleCalRefs()

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

				node3 = nodes3(index).SelectSingleNode("CameraSettingsList")
				For Each childNode As XmlNode In node3.ChildNodes
					Dim lastChild As XmlNode = node3.LastChild.Clone()
					node3.RemoveAll()
					node3.AppendChild(lastChild)
				Next
				SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText
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
			ImgScaleCalDataGridView1.DataSource = dsImgScaleCalibration
			ImgScaleCalDataGridView1.DataMember = "ImageScalingCalibration"
			ImgScaleCalDataGridView1.ReadOnly = True
			ImgScaleCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			ImgScaleCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
			ImgScaleCalDataGridView1.AllowUserToResizeColumns = True

		Catch ex As Exception
			MsgBox("Error: " & ex.Source & ": " & ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!")
		End Try
	End Sub

	Private Sub CalRule_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		txtFile3.Text = MainForm.txtFile3.Text
	End Sub

	Private Sub txtFile3_TextChanged(sender As Object, e As EventArgs) Handles txtFile3.TextChanged
		ReloadColorCalRule()
		ShowColorCalRefs()
		ReloadFlatFieldCalRule()
		ShowFlatFieldCalRefs()
		ReloadImgScaleCalRule()
		ShowImgScaleCalRefs()
	End Sub
	Private Sub ReloadColorCalRule()

		Dim ColorCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_colorcal.txt"
		Dim ColorCalRuleFilePath As String = Path.Combine(exePath, ColorCalRuleFilaName)

		Dim Fields As String()
		Fields = "Step,ColorCalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		If File.Exists(ColorCalRuleFilePath) Then
			Dim Lines As String() = File.ReadAllLines(ColorCalRuleFilePath)
			Dim dr As DataRow
			For index = 0 To Lines.GetLength(0) - 1
				Fields = Lines(index).Split(",")
				dr = dt.NewRow
				For f = 0 To Cols - 1
					dr(f) = Fields(f)

				Next
				dt.Rows.Add(dr)
			Next

		End If

		ColorCalDataGridView2.DataSource = dt
		ColorCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
		ColorCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
		ColorCalDataGridView2.AllowUserToResizeColumns = True
	End Sub
	Private Sub ReloadFlatFieldCalRule()

		Dim FlatFieldCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_flatfieldcal.txt"
		Dim FlatFieldCalRuleFilePath As String = Path.Combine(exePath, FlatFieldCalRuleFilaName)

		Dim Fields As String()
		Fields = "Step,CalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		If File.Exists(FlatFieldCalRuleFilePath) Then
			Dim Lines As String() = File.ReadAllLines(FlatFieldCalRuleFilePath)
			Dim dr As DataRow
			For index = 0 To Lines.GetLength(0) - 1
				Fields = Lines(index).Split(",")
				dr = dt.NewRow
				For f = 0 To Cols - 1
					dr(f) = Fields(f)

				Next
				dt.Rows.Add(dr)
			Next

		End If

		FlatFieldCalDataGridView2.DataSource = dt
		FlatFieldCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
		FlatFieldCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
		FlatFieldCalDataGridView2.AllowUserToResizeColumns = True
	End Sub
	Private Sub ReloadImgScaleCalRule()

		Dim ImgScaleCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_imgscalecal.txt"
		Dim ImgScaleCalRuleFilePath As String = Path.Combine(exePath, ImgScaleCalRuleFilaName)

		Dim Fields As String()
		Fields = "Step,ImageScalingCalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		If File.Exists(ImgScaleCalRuleFilePath) Then
			Dim Lines As String() = File.ReadAllLines(ImgScaleCalRuleFilePath)
			Dim dr As DataRow
			For index = 0 To Lines.GetLength(0) - 1
				Fields = Lines(index).Split(",")
				dr = dt.NewRow
				For f = 0 To Cols - 1
					dr(f) = Fields(f)

				Next
				dt.Rows.Add(dr)
			Next

		End If

		ImgScaleCalDataGridView2.DataSource = dt
		ImgScaleCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
		ImgScaleCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
		ImgScaleCalDataGridView2.AllowUserToResizeColumns = True
	End Sub

	Private Sub btnColorCalSaveCalRules_Click(sender As Object, e As EventArgs) Handles btnColorCalSaveCalRules.Click
		Dim ColorCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_colorcal.txt"
		Dim ColorCalRuleFilePath As String = Path.Combine(exePath, ColorCalRuleFilaName)
		Dim rows = From row As DataGridViewRow In ColorCalDataGridView2.Rows.Cast(Of DataGridViewRow)()
				   Where Not row.IsNewRow
				   Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))

		Dim rowsData As New List(Of String)
		Dim stepData As New List(Of String)
		Dim duplicateRows As New List(Of String)
		For Each r In rows
			Dim rowdata = String.Join(",", r)
			Dim rowstep As String = r(0)
			If stepData.Contains(rowstep) Then
				duplicateRows.Add(rowstep)
			End If
			stepData.Add(rowstep)
			rowsData.Add(rowdata)
		Next
		Dim saveContent As String = ""
		Dim duplicateContent As String = ""
		If rowsData.Count > 0 Then
			saveContent = String.Join(vbCrLf, rowsData)
		End If
		If duplicateRows.Count > 0 Then
			duplicateContent = String.Join(",", duplicateRows)
		End If

		Dim result As DialogResult = MessageBox.Show("Confirm saving ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			If duplicateContent <> "" Then
				MessageBox.Show("Check duplicate rules : " + duplicateContent)

			ElseIf saveContent = "" Then
				Dim result2 As DialogResult = MessageBox.Show("Save empty rules ?",
													  "Confirm saving",
													  MessageBoxButtons.YesNo)
				If result2 = DialogResult.Yes Then
					File.WriteAllText(ColorCalRuleFilePath, saveContent)
				End If

			Else
				File.WriteAllText(ColorCalRuleFilePath, saveContent)
			End If
		End If


	End Sub

	Private Sub btnColorCalUseSampleRuleMono_Click(sender As Object, e As EventArgs) Handles btnColorCalUseSampleRuleMono.Click
		Dim Fields As String()
		Fields = "Step,ColorCalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "1")
		dt.Rows.Add("R255", "1")
		dt.Rows.Add("B255", "1")
		dt.Rows.Add("g36", "1")
		dt.Rows.Add("g32", "1")
		dt.Rows.Add("g216", "1")
		dt.Rows.Add("g192", "1")
		dt.Rows.Add("r36", "1")
		dt.Rows.Add("r32", "1")
		dt.Rows.Add("r216", "1")
		dt.Rows.Add("r192", "1")
		dt.Rows.Add("b36", "1")
		dt.Rows.Add("b32", "1")
		dt.Rows.Add("b216", "1")
		dt.Rows.Add("b192", "1")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			ColorCalDataGridView2.DataSource = dt
		End If

	End Sub

	Private Sub bntColorCalUseSampleRuleColor_Click(sender As Object, e As EventArgs) Handles bntColorCalUseSampleRuleColor.Click
		Dim Fields As String()
		Fields = "Step,ColorCalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "1")
		dt.Rows.Add("R255", "1")
		dt.Rows.Add("B255", "1")
		dt.Rows.Add("W10", "1")
		dt.Rows.Add("W12", "1")
		dt.Rows.Add("W216", "1")
		dt.Rows.Add("W8", "2")
		dt.Rows.Add("W23", "3")
		dt.Rows.Add("W31", "3")
		dt.Rows.Add("W42", "3")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			ColorCalDataGridView2.DataSource = dt
		End If
	End Sub

	Private Sub btnFlatFieldCalUseSampleRuleMono_Click(sender As Object, e As EventArgs) Handles btnFlatFieldCalUseSampleRuleMono.Click
		Dim Fields As String()
		Fields = "Step,CalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "-1")
		dt.Rows.Add("R255", "-1")
		dt.Rows.Add("B255", "-1")
		dt.Rows.Add("g36", "-1")
		dt.Rows.Add("g32", "-1")
		dt.Rows.Add("g216", "-1")
		dt.Rows.Add("g192", "-1")
		dt.Rows.Add("r36", "-1")
		dt.Rows.Add("r32", "-1")
		dt.Rows.Add("r216", "-1")
		dt.Rows.Add("r192", "-1")
		dt.Rows.Add("b36", "-1")
		dt.Rows.Add("b32", "-1")
		dt.Rows.Add("b216", "-1")
		dt.Rows.Add("b192", "-1")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			FlatFieldCalDataGridView2.DataSource = dt
		End If
	End Sub

	Private Sub btnImgScaleCalUseSampleRuleMono_Click(sender As Object, e As EventArgs) Handles btnImgScaleCalUseSampleRuleMono.Click
		Dim Fields As String()
		Fields = "Step,ImageScalingCaibrationlID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "-1")
		dt.Rows.Add("R255", "-1")
		dt.Rows.Add("B255", "-1")
		dt.Rows.Add("g36", "-1")
		dt.Rows.Add("g32", "-1")
		dt.Rows.Add("g216", "-1")
		dt.Rows.Add("g192", "-1")
		dt.Rows.Add("r36", "-1")
		dt.Rows.Add("r32", "-1")
		dt.Rows.Add("r216", "-1")
		dt.Rows.Add("r192", "-1")
		dt.Rows.Add("b36", "-1")
		dt.Rows.Add("b32", "-1")
		dt.Rows.Add("b216", "-1")
		dt.Rows.Add("b192", "-1")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			ImgScaleCalDataGridView2.DataSource = dt
		End If
	End Sub

	Private Sub btnFlatFieldCalSaveCalRules_Click(sender As Object, e As EventArgs) Handles btnFlatFieldCalSaveCalRules.Click
		Dim FlatFieldCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_flatfieldcal.txt"
		Dim FlatFieldCalRuleFilePath As String = Path.Combine(exePath, FlatFieldCalRuleFilaName)
		Dim rows = From row As DataGridViewRow In FlatFieldCalDataGridView2.Rows.Cast(Of DataGridViewRow)()
				   Where Not row.IsNewRow
				   Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))

		Dim rowsData As New List(Of String)
		Dim stepData As New List(Of String)
		Dim duplicateRows As New List(Of String)
		For Each r In rows
			Dim rowdata = String.Join(",", r)
			Dim rowstep As String = r(0)
			If stepData.Contains(rowstep) Then
				duplicateRows.Add(rowstep)
			End If
			stepData.Add(rowstep)
			rowsData.Add(rowdata)
		Next
		Dim saveContent As String = ""
		Dim duplicateContent As String = ""
		If rowsData.Count > 0 Then
			saveContent = String.Join(vbCrLf, rowsData)
		End If
		If duplicateRows.Count > 0 Then
			duplicateContent = String.Join(",", duplicateRows)
		End If

		Dim result As DialogResult = MessageBox.Show("Confirm saving ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			If duplicateContent <> "" Then
				MessageBox.Show("Check duplicate rules : " + duplicateContent)

			ElseIf saveContent = "" Then
				Dim result2 As DialogResult = MessageBox.Show("Save empty rules ?",
													  "Confirm saving",
													  MessageBoxButtons.YesNo)
				If result2 = DialogResult.Yes Then
					File.WriteAllText(FlatFieldCalRuleFilePath, saveContent)
				End If

			Else
				File.WriteAllText(FlatFieldCalRuleFilePath, saveContent)
			End If
		End If
	End Sub

	Private Sub btnImgScaleCalSaveCalRules_Click(sender As Object, e As EventArgs) Handles btnImgScaleCalSaveCalRules.Click
		Dim ImgScaleCalRuleFilaName As String = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_imgscalecal.txt"
		Dim ImgScaleCalRuleFilePath As String = Path.Combine(exePath, ImgScaleCalRuleFilaName)
		Dim rows = From row As DataGridViewRow In ImgScaleCalDataGridView2.Rows.Cast(Of DataGridViewRow)()
				   Where Not row.IsNewRow
				   Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))

		Dim rowsData As New List(Of String)
		Dim stepData As New List(Of String)
		Dim duplicateRows As New List(Of String)
		For Each r In rows
			Dim rowdata = String.Join(",", r)
			Dim rowstep As String = r(0)
			If stepData.Contains(rowstep) Then
				duplicateRows.Add(rowstep)
			End If
			stepData.Add(rowstep)
			rowsData.Add(rowdata)
		Next
		Dim saveContent As String = ""
		Dim duplicateContent As String = ""
		If rowsData.Count > 0 Then
			saveContent = String.Join(vbCrLf, rowsData)
		End If
		If duplicateRows.Count > 0 Then
			duplicateContent = String.Join(",", duplicateRows)
		End If

		Dim result As DialogResult = MessageBox.Show("Confirm saving ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			If duplicateContent <> "" Then
				MessageBox.Show("Check duplicate rules : " + duplicateContent)

			ElseIf saveContent = "" Then
				Dim result2 As DialogResult = MessageBox.Show("Save empty rules ?",
													  "Confirm saving",
													  MessageBoxButtons.YesNo)
				If result2 = DialogResult.Yes Then
					File.WriteAllText(ImgScaleCalRuleFilePath, saveContent)
				End If

			Else
				File.WriteAllText(ImgScaleCalRuleFilePath, saveContent)
			End If
		End If
	End Sub

	Private Sub btnColorCalReloadCalRules_Click(sender As Object, e As EventArgs) Handles btnColorCalReloadCalRules.Click
		ReloadColorCalRule()
	End Sub

	Private Sub btnFlatFieldCalReloadCalRules_Click(sender As Object, e As EventArgs) Handles btnFlatFieldCalReloadCalRules.Click
		ReloadFlatFieldCalRule()
	End Sub

	Private Sub btnImgScaleCalReloadCalRules_Click(sender As Object, e As EventArgs) Handles btnImgScaleCalReloadCalRules.Click
		ReloadImgScaleCalRule()
	End Sub

	Private Sub bntImgScaleCalUseSampleRuleColor_Click(sender As Object, e As EventArgs) Handles bntImgScaleCalUseSampleRuleColor.Click
		Dim Fields As String()
		Fields = "Step,ImageScalingCalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "-1")
		dt.Rows.Add("R255", "-1")
		dt.Rows.Add("B255", "-1")
		dt.Rows.Add("W10", "-1")
		dt.Rows.Add("W12", "-1")
		dt.Rows.Add("W216", "-1")
		dt.Rows.Add("W8", "-1")
		dt.Rows.Add("W23", "-1")
		dt.Rows.Add("W31", "-1")
		dt.Rows.Add("W42", "-1")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			ImgScaleCalDataGridView2.DataSource = dt
		End If
	End Sub

	Private Sub bntFlatFieldCalUseSampleRuleColor_Click(sender As Object, e As EventArgs) Handles bntFlatFieldCalUseSampleRuleColor.Click
		Dim Fields As String()
		Fields = "Step,CalibrationID".Split(",")
		Dim Cols As Integer = Fields.GetLength(0)
		Dim dt As DataTable = New DataTable
		For index = 0 To Cols - 1
			dt.Columns.Add(Fields(index))
		Next
		dt.Rows.Add("G255", "-1")
		dt.Rows.Add("R255", "-1")
		dt.Rows.Add("B255", "-1")
		dt.Rows.Add("W10", "-1")
		dt.Rows.Add("W12", "-1")
		dt.Rows.Add("W216", "-1")
		dt.Rows.Add("W8", "-1")
		dt.Rows.Add("W23", "-1")
		dt.Rows.Add("W31", "-1")
		dt.Rows.Add("W42", "-1")

		Dim result As DialogResult = MessageBox.Show("This will clear current rules ?",
													  "Confirmation",
													  MessageBoxButtons.YesNo)
		If result = DialogResult.Yes Then
			FlatFieldCalDataGridView2.DataSource = dt
		End If
	End Sub
End Class