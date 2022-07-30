Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Windows.Forms
Imports System.Xml

Namespace SetSequenceVB
	Partial Public Class MainForm
		Inherits Form

		Private subframeregion As String = ""
		Private lensdistance As String = ""
		Private fnumber As String = ""
		Private colorcalID As String = ""
		Private imagescalingID As String = ""
		Private flatfieldID As String = ""
		Private cameraRotation As String = ""
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub SetVersionInfo()
			Dim versionInfo As Version = Assembly.GetExecutingAssembly().GetName().Version
			Dim startDate As Date = New DateTime(2000, 1, 1)
			Dim diffDays = versionInfo.Build
			Dim computedDate = startDate.AddDays(diffDays)
			Dim lastBuilt As String = computedDate.ToShortDateString()
			'this.Text = string.Format("{0} - {1} ({2})",
			'            this.Text, versionInfo.ToString(), lastBuilt);
			Text = String.Format("{0} - {1}", Text, versionInfo.ToString())
		End Sub



		Private Sub txtAdditionalSequence_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs) Handles txtAdditionalSequence.DragEnter
			e.Effect = DragDropEffects.Copy
		End Sub

		Private Sub lblAbout_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles lblAbout.DoubleClick
			Dim AboutForm As Form = New About()
			AboutForm.ShowDialog()
		End Sub

		Private Sub cmdUseLastModifiedSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdUseLastModifiedSequence.LinkClicked
			If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
				Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles("*.*", SearchOption.AllDirectories).OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
				lblSequencePath.Text = latestfile.FullName
			End If
		End Sub
		Private Sub cmdBrowseSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdBrowseSequence.LinkClicked
			Using dialog As OpenFileDialog = New OpenFileDialog()
				dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\Sequence"
				If dialog.ShowDialog(Me) = DialogResult.OK Then
					lblSequencePath.Text = dialog.FileName
				End If
			End Using
		End Sub

		Private Sub btnBrowseAdditional_Click(sender As Object, e As EventArgs) Handles btnBrowseAdditional.Click
			Using dialog As OpenFileDialog = New OpenFileDialog()
				dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\Sequence"
				dialog.Multiselect = True
				If dialog.ShowDialog(Me) = DialogResult.OK Then
					Dim files = dialog.FileNames
					For Each file In files
						txtAdditionalSequence.Text += file & Microsoft.VisualBasic.Constants.vbCrLf
					Next
				End If
			End Using
		End Sub

		Private Sub btnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click
			btnApply.Enabled = False

			GetValues(lblSequencePath.Text, cbSubframe.Text, cbCalBox.Text, cbFocusDistance.Text, cbFNumber.Text, cbCameraRotation.Text)
			SetValues(lblSequencePath.Text)

			Try
				Dim additionalTargets As String()
				additionalTargets = txtAdditionalSequence.Text.Split(New Char() {Microsoft.VisualBasic.Strings.ChrW(13), Microsoft.VisualBasic.Strings.ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
				For Each item In additionalTargets
					SetValues(item)
				Next
			Catch ex As Exception

			End Try

			Dim m_Rnd As Random = New Random()
			Dim tempcolor As Color
			tempcolor = label1.ForeColor
			While label1.ForeColor = tempcolor
				label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			End While
			label1.Text = "Done!"
			btnApply.Enabled = True

		End Sub

		Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
			SetVersionInfo()
			If Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Sequence") Then
				Dim latestfile = New DirectoryInfo("C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(Function(o) o.LastWriteTime).FirstOrDefault()
				lblSequencePath.Text = latestfile.FullName
			End If
		End Sub

		Private Sub txtAdditionalSequence_DragDrop(sender As Object, e As DragEventArgs) Handles txtAdditionalSequence.DragDrop
			e.Effect = DragDropEffects.Copy
			Dim files = CType(e.Data.GetData(DataFormats.FileDrop), String())
			If files IsNot Nothing AndAlso files.Length <> 0 Then
				For Each file In files
					txtAdditionalSequence.Text += file & Microsoft.VisualBasic.Constants.vbCrLf
				Next
			End If
		End Sub

		Private Sub GetValues(xmlFile As String, subframeSetting As String, calSetting As String, focusSetting As String, fNumberSetting As String, rotationSetting As String)
			Dim node As XmlNode
			Dim nodes As XmlNodeList

			Dim xmlDoc = New XmlDocument()
			If File.Exists(xmlFile) Then
				xmlDoc.Load(xmlFile)

				If Equals(subframeSetting, "Copy from first step") Then
					node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName")
					Dim firstPatternName = node.InnerText
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
					For index = 0 To nodes.Count - 1
						If Equals(nodes(index).SelectSingleNode("Name").InnerText, firstPatternName) Then
							subframeregion = nodes(index).SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion").InnerText
						End If
					Next
				Else
					subframeregion = subframeSetting
				End If

				If Equals(calSetting, "Copy from first step") Then
					node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName")
					Dim firstPatternName = node.InnerText
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
					For index = 0 To nodes.Count - 1
						If Equals(nodes(index).SelectSingleNode("Name").InnerText, firstPatternName) Then
							colorcalID = nodes(index).SelectSingleNode("CameraSettingsList/CameraSettings/ColorCalID").InnerText
							imagescalingID = nodes(index).SelectSingleNode("CameraSettingsList/CameraSettings/ImageScalingCalibrationID").InnerText
							flatfieldID = nodes(index).SelectSingleNode("CameraSettingsList/CameraSettings/FlatFieldID").InnerText
						End If
					Next
				Else
					Dim values = calSetting.Split(","c)
					colorcalID = values(1)
					imagescalingID = values(2)
					flatfieldID = values(3)
				End If

				If Equals(focusSetting, "Copy from first step") Then
					node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName")
					Dim firstPatternName = node.InnerText
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
					For index = 0 To nodes.Count - 1
						If Equals(nodes(index).SelectSingleNode("Name").InnerText, firstPatternName) Then
							lensdistance = nodes(index).SelectSingleNode("LensDistance").InnerText
						End If
					Next
				Else
					lensdistance = focusSetting
				End If

				If Equals(fNumberSetting, "Copy from first step") Then
					node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName")
					Dim firstPatternName = node.InnerText
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
					For index = 0 To nodes.Count - 1
						If Equals(nodes(index).SelectSingleNode("Name").InnerText, firstPatternName) Then
							fnumber = nodes(index).SelectSingleNode("LensfStop").InnerText
						End If
					Next
				Else
					Select Case fNumberSetting
						Case "8.0"
							fnumber = "6"
						Case Else
							fnumber = fNumberSetting
					End Select

				End If

				If Equals(rotationSetting, "Copy from first step") Then
					node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName")
					Dim firstPatternName = node.InnerText
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
					For index = 0 To nodes.Count - 1
						If Equals(nodes(index).SelectSingleNode("Name").InnerText, firstPatternName) Then
							cameraRotation = nodes(index).SelectSingleNode("CameraRotation").InnerText
						End If
					Next
				Else
					cameraRotation = rotationSetting
				End If
			End If
		End Sub

		Private Sub SetValues(xmlFile As String)

			Dim nodes As XmlNodeList

			Dim xmlDoc = New XmlDocument()
			If File.Exists(xmlFile) Then
				xmlDoc.Load(xmlFile)
				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensDistance")
				For index = 0 To nodes.Count - 1
					If Not Equals(lensdistance, "") Then
						nodes(index).InnerText = lensdistance
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensfStop")
				For index = 0 To nodes.Count - 1
					If Not Equals(fnumber, "") Then
						nodes(index).InnerText = fnumber
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion")
				For index = 0 To nodes.Count - 1
					If Not Equals(subframeregion, "") Then
						nodes(index).InnerText = subframeregion
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ColorCalID")
				For index = 0 To nodes.Count - 1
					If Not Equals(colorcalID, "") Then
						nodes(index).InnerText = colorcalID
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ImageScalingCalibrationID")
				For index = 0 To nodes.Count - 1
					If Not Equals(imagescalingID, "") Then
						nodes(index).InnerText = imagescalingID
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/FlatFieldID")
				For index = 0 To nodes.Count - 1
					If Not Equals(flatfieldID, "") Then
						nodes(index).InnerText = flatfieldID
					End If
				Next

				nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraRotation")
				For index = 0 To nodes.Count - 1
					If Not Equals(cameraRotation, "") Then
						nodes(index).InnerText = cameraRotation
					End If
				Next

				Dim settings As XmlWriterSettings = New XmlWriterSettings With {
					.Indent = True
				}
				Dim writer = XmlWriter.Create(xmlFile, settings)
				xmlDoc.Save(writer)
				If writer IsNot Nothing Then writer.Close()

			End If
		End Sub

	End Class
End Namespace
