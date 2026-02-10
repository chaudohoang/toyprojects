Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Windows.Forms
Imports System.Drawing

Public Class FOIsetCopyForm
    Inherits Form

    ' UI Controls
    Private btnBrowseLeft As Button
    Private btnBrowseRight As Button
    Private chkListFOIsetsLeft As CheckedListBox
    Private chkListFOIsetsRight As CheckedListBox
    Private lblPathLeft As Label
    Private lblPathRight As Label
    Private btnCopyLeftToRight As Button
    Private btnCopyRightToLeft As Button
    Private lblStatusLeft As Label
    Private lblStatusRight As Label
    Private btnSelectAllLeft As Button
    Private btnDeselectAllLeft As Button
    Private btnSelectAllRight As Button
    Private btnDeselectAllRight As Button

    ' Data
    Private sequenceLeft As Sequence
    Private sequenceRight As Sequence
    Private pathLeft As String
    Private pathRight As String

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "FOIset Copy Tool"
        Me.Size = New Size(1200, 700)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Left Panel
        Dim pnlLeft As New Panel With {
            .Location = New Point(10, 10),
            .Size = New Size(520, 620),
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblLeft As New Label With {
            .Text = "Left Sequence",
            .Location = New Point(10, 10),
            .Size = New Size(500, 25),
            .Font = New Font("Arial", 12, FontStyle.Bold)
        }

        btnBrowseLeft = New Button With {
            .Text = "Browse Sequence File...",
            .Location = New Point(10, 40),
            .Size = New Size(500, 35)
        }
        AddHandler btnBrowseLeft.Click, AddressOf BtnBrowseLeft_Click

        lblPathLeft = New Label With {
            .Text = "No file loaded",
            .Location = New Point(10, 80),
            .Size = New Size(500, 40),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.White,
            .AutoEllipsis = True
        }

        lblStatusLeft = New Label With {
            .Text = "FOIsets: 0 | Selected: 0",
            .Location = New Point(10, 125),
            .Size = New Size(500, 20),
            .Font = New Font("Arial", 9, FontStyle.Bold)
        }

        chkListFOIsetsLeft = New CheckedListBox With {
            .Location = New Point(10, 150),
            .Size = New Size(500, 410),
            .CheckOnClick = True
        }
        AddHandler chkListFOIsetsLeft.ItemCheck, AddressOf CheckList_ItemCheck

        ' Selection buttons for left
        btnSelectAllLeft = New Button With {
            .Text = "Select All",
            .Location = New Point(10, 565),
            .Size = New Size(120, 30)
        }
        AddHandler btnSelectAllLeft.Click, Sub() SelectAll(chkListFOIsetsLeft, True)

        btnDeselectAllLeft = New Button With {
            .Text = "Deselect All",
            .Location = New Point(140, 565),
            .Size = New Size(120, 30)
        }
        AddHandler btnDeselectAllLeft.Click, Sub() SelectAll(chkListFOIsetsLeft, False)

        pnlLeft.Controls.AddRange({lblLeft, btnBrowseLeft, lblPathLeft, lblStatusLeft, chkListFOIsetsLeft, btnSelectAllLeft, btnDeselectAllLeft})

        ' Middle Panel - Copy Buttons
        Dim pnlMiddle As New Panel With {
            .Location = New Point(540, 10),
            .Size = New Size(100, 620)
        }

        btnCopyLeftToRight = New Button With {
            .Text = "Copy →" & vbCrLf & "(Preview)",
            .Location = New Point(10, 230),
            .Size = New Size(80, 60),
            .Enabled = False
        }
        AddHandler btnCopyLeftToRight.Click, AddressOf BtnCopyLeftToRight_Click

        btnCopyRightToLeft = New Button With {
            .Text = "← Copy" & vbCrLf & "(Preview)",
            .Location = New Point(10, 300),
            .Size = New Size(80, 60),
            .Enabled = False
        }
        AddHandler btnCopyRightToLeft.Click, AddressOf BtnCopyRightToLeft_Click

        pnlMiddle.Controls.AddRange({btnCopyLeftToRight, btnCopyRightToLeft})

        ' Right Panel
        Dim pnlRight As New Panel With {
            .Location = New Point(650, 10),
            .Size = New Size(520, 620),
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblRight As New Label With {
            .Text = "Right Sequence",
            .Location = New Point(10, 10),
            .Size = New Size(500, 25),
            .Font = New Font("Arial", 12, FontStyle.Bold)
        }

        btnBrowseRight = New Button With {
            .Text = "Browse Sequence File...",
            .Location = New Point(10, 40),
            .Size = New Size(500, 35)
        }
        AddHandler btnBrowseRight.Click, AddressOf BtnBrowseRight_Click

        lblPathRight = New Label With {
            .Text = "No file loaded",
            .Location = New Point(10, 80),
            .Size = New Size(500, 40),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.White,
            .AutoEllipsis = True
        }

        lblStatusRight = New Label With {
            .Text = "FOIsets: 0 | Selected: 0",
            .Location = New Point(10, 125),
            .Size = New Size(500, 20),
            .Font = New Font("Arial", 9, FontStyle.Bold)
        }

        chkListFOIsetsRight = New CheckedListBox With {
            .Location = New Point(10, 150),
            .Size = New Size(500, 410),
            .CheckOnClick = True
        }
        AddHandler chkListFOIsetsRight.ItemCheck, AddressOf CheckList_ItemCheck

        ' Selection buttons for right
        btnSelectAllRight = New Button With {
            .Text = "Select All",
            .Location = New Point(10, 565),
            .Size = New Size(120, 30)
        }
        AddHandler btnSelectAllRight.Click, Sub() SelectAll(chkListFOIsetsRight, True)

        btnDeselectAllRight = New Button With {
            .Text = "Deselect All",
            .Location = New Point(140, 565),
            .Size = New Size(120, 30)
        }
        AddHandler btnDeselectAllRight.Click, Sub() SelectAll(chkListFOIsetsRight, False)

        pnlRight.Controls.AddRange({lblRight, btnBrowseRight, lblPathRight, lblStatusRight, chkListFOIsetsRight, btnSelectAllRight, btnDeselectAllRight})

        ' Status Bar
        Dim lblStatus As New Label With {
            .Text = "Note: Select multiple FOIsets to copy. Preview will show which items will be added or replaced. IDs are automatically generated.",
            .Location = New Point(10, 640),
            .Size = New Size(1160, 20),
            .ForeColor = Color.DarkBlue,
            .Font = New Font("Arial", 8)
        }

        Me.Controls.AddRange({pnlLeft, pnlMiddle, pnlRight, lblStatus})
    End Sub

    Private Sub CheckList_ItemCheck(sender As Object, e As ItemCheckEventArgs)
        ' Use BeginInvoke to update count after the check state changes
        Me.BeginInvoke(Sub() UpdateSelectedCount())
    End Sub

    Private Sub UpdateSelectedCount()
        lblStatusLeft.Text = $"FOIsets: {chkListFOIsetsLeft.Items.Count} | Selected: {chkListFOIsetsLeft.CheckedItems.Count}"
        lblStatusRight.Text = $"FOIsets: {chkListFOIsetsRight.Items.Count} | Selected: {chkListFOIsetsRight.CheckedItems.Count}"
        UpdateCopyButtons()
    End Sub

    Private Sub SelectAll(checkedListBox As CheckedListBox, check As Boolean)
        For i As Integer = 0 To checkedListBox.Items.Count - 1
            checkedListBox.SetItemChecked(i, check)
        Next
        UpdateSelectedCount()
    End Sub

    Private Sub BtnBrowseLeft_Click(sender As Object, e As EventArgs)
        LoadSequence("left")
    End Sub

    Private Sub BtnBrowseRight_Click(sender As Object, e As EventArgs)
        LoadSequence("right")
    End Sub

    Private Sub LoadSequence(side As String)
        Dim ofd As New OpenFileDialog With {
            .Filter = "Sequence Files (*.seqxc)|*.seqxc|All Files (*.*)|*.*",
            .Title = "Select Sequence File"
        }

        If ofd.ShowDialog() = DialogResult.OK Then
            Try
                ' Load using XmlDocument to preserve everything
                Dim xmlDoc As New XmlDocument()
                xmlDoc.Load(ofd.FileName)

                ' Also deserialize for working with objects
                Dim serializer As New XmlSerializer(GetType(Sequence))
                Using reader As New StreamReader(ofd.FileName)
                    Dim sequence As Sequence = CType(serializer.Deserialize(reader), Sequence)

                    If side = "left" Then
                        sequenceLeft = sequence
                        pathLeft = ofd.FileName
                        lblPathLeft.Text = pathLeft
                        PopulateFOIsets(chkListFOIsetsLeft, sequence, lblStatusLeft)
                    Else
                        sequenceRight = sequence
                        pathRight = ofd.FileName
                        lblPathRight.Text = pathRight
                        PopulateFOIsets(chkListFOIsetsRight, sequence, lblStatusRight)
                    End If

                    UpdateCopyButtons()
                End Using
            Catch ex As InvalidOperationException
                ' XML deserialization error - show detailed message
                Dim innerMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, "")
                MessageBox.Show($"Error loading sequence file. The XML structure may not match the expected format.{vbCrLf}{vbCrLf}" &
                               $"Error: {ex.Message}{vbCrLf}{vbCrLf}" &
                               $"Details: {innerMsg}{vbCrLf}{vbCrLf}" &
                               $"File: {ofd.FileName}",
                               "XML Deserialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Catch ex As Exception
                MessageBox.Show($"Error loading sequence: {ex.Message}{vbCrLf}{vbCrLf}File: {ofd.FileName}",
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub PopulateFOIsets(checkedListBox As CheckedListBox, sequence As Sequence, statusLabel As Label)
        checkedListBox.Items.Clear()

        If sequence?.FOImanager?.FOIsets IsNot Nothing Then
            Dim allFOIsets = GetAllFOIsetsFlat(sequence.FOImanager.FOIsets)

            For Each foiSet In allFOIsets.Where(Function(f) Not String.IsNullOrEmpty(f.FOISetDescription))
                checkedListBox.Items.Add(New FOIsetItem(foiSet))
            Next

            statusLabel.Text = $"FOIsets: {checkedListBox.Items.Count} | Selected: 0"
        Else
            statusLabel.Text = "FOIsets: 0 | Selected: 0"
        End If
    End Sub

    Private Function GetAllFOIsetsFlat(foiSets As List(Of FOIset)) As List(Of FOIset)
        Dim result As New List(Of FOIset)

        If foiSets Is Nothing Then Return result

        For Each foiSet In foiSets
            If Not String.IsNullOrEmpty(foiSet.FOISetDescription) Then
                result.Add(foiSet)
            End If

            If foiSet.ChildFOIsets IsNot Nothing Then
                result.AddRange(GetAllFOIsetsFlat(foiSet.ChildFOIsets))
            End If
        Next

        Return result
    End Function

    Private Sub UpdateCopyButtons()
        btnCopyLeftToRight.Enabled = (chkListFOIsetsLeft.CheckedItems.Count > 0 AndAlso sequenceRight IsNot Nothing)
        btnCopyRightToLeft.Enabled = (chkListFOIsetsRight.CheckedItems.Count > 0 AndAlso sequenceLeft IsNot Nothing)
    End Sub

    Private Sub BtnCopyLeftToRight_Click(sender As Object, e As EventArgs)
        CopyFOIsets(chkListFOIsetsLeft, sequenceLeft, sequenceRight, pathRight, chkListFOIsetsRight, lblStatusRight)
    End Sub

    Private Sub BtnCopyRightToLeft_Click(sender As Object, e As EventArgs)
        CopyFOIsets(chkListFOIsetsRight, sequenceRight, sequenceLeft, pathLeft, chkListFOIsetsLeft, lblStatusLeft)
    End Sub

    Private Sub CopyFOIsets(sourceList As CheckedListBox, sourceSeq As Sequence, targetSeq As Sequence, targetPath As String, targetList As CheckedListBox, targetStatus As Label)
        If sourceList.CheckedItems.Count = 0 Then Return

        ' Get selected FOIsets
        Dim selectedFOIsets As New List(Of FOIset)
        For Each item In sourceList.CheckedItems
            selectedFOIsets.Add(CType(item, FOIsetItem).FOIset)
        Next

        ' Analyze what will happen
        Dim targetFOIsets = GetAllFOIsetsFlat(targetSeq.FOImanager.FOIsets)
        Dim toAdd As New List(Of FOIset)
        Dim toReplace As New List(Of FOIset)

        For Each foiSet In selectedFOIsets
            Dim existing = targetFOIsets.FirstOrDefault(Function(f) f.FOISetDescription = foiSet.FOISetDescription)
            If existing IsNot Nothing Then
                toReplace.Add(foiSet)
            Else
                toAdd.Add(foiSet)
            End If
        Next

        ' Show preview dialog
        Dim preview = New CopyPreviewForm(selectedFOIsets, toAdd, toReplace)
        If preview.ShowDialog() <> DialogResult.OK Then
            Return ' User cancelled
        End If

        Try
            ' Load both files as XML Documents to preserve structure
            Dim sourceXml As New XmlDocument()
            sourceXml.PreserveWhitespace = True  ' Preserve original formatting
            sourceXml.Load(If(sourceSeq Is sequenceLeft, pathLeft, pathRight))

            Dim targetXml As New XmlDocument()
            targetXml.PreserveWhitespace = True  ' Preserve original formatting
            targetXml.Load(targetPath)

            Dim copiedCount = 0
            Dim replacedCount = 0

            ' Find the FOIsets node in target
            Dim targetFOIsetsNode = targetXml.SelectSingleNode("//FOImanager/FOIsets")
            If targetFOIsetsNode Is Nothing Then
                Throw New Exception("Could not find FOIsets node in target file")
            End If

            ' Get max ID in target for new FOIsets
            Dim maxID = GetMaxIDFromXml(targetXml)

            ' Process each selected FOIset
            For Each foiSet In selectedFOIsets
                ' Find the FOIset node in source XML by description
                Dim sourceFOIsetNode = FindFOIsetNodeByDescription(sourceXml, foiSet.FOISetDescription)

                If sourceFOIsetNode IsNot Nothing Then
                    ' Check if exists in target
                    Dim existingNode = FindFOIsetNodeByDescription(targetXml, foiSet.FOISetDescription)

                    If existingNode IsNot Nothing Then
                        ' Replace existing - import and replace the node
                        Dim importedNode = targetXml.ImportNode(sourceFOIsetNode, True)
                        existingNode.ParentNode.ReplaceChild(importedNode, existingNode)
                        replacedCount += 1
                    Else
                        ' Add new - import the node and update ID
                        Dim importedNode = targetXml.ImportNode(sourceFOIsetNode, True)

                        ' Update ID to be unique
                        maxID += 1
                        Dim idNode = importedNode.SelectSingleNode("ID")
                        If idNode IsNot Nothing Then
                            idNode.InnerText = maxID.ToString()
                        End If

                        ' Add to target
                        targetFOIsetsNode.AppendChild(importedNode)
                        copiedCount += 1
                    End If
                End If
            Next

            ' Save the modified XML - PreserveWhitespace should keep empty elements as-is
            targetXml.Save(targetPath)

            ' Reload BOTH sequences to refresh the in-memory data
            Try
                Dim serializer As New XmlSerializer(GetType(Sequence))

                ' Reload target sequence
                Using reader As New StreamReader(targetPath)
                    Dim reloadedTarget As Sequence = CType(serializer.Deserialize(reader), Sequence)

                    If targetSeq Is sequenceRight Then
                        sequenceRight = reloadedTarget
                        targetSeq = sequenceRight
                    Else
                        sequenceLeft = reloadedTarget
                        targetSeq = sequenceLeft
                    End If
                End Using

                ' Also reload source sequence to keep everything in sync
                Dim sourcePath = If(sourceSeq Is sequenceLeft, pathLeft, pathRight)
                Using reader As New StreamReader(sourcePath)
                    Dim reloadedSource As Sequence = CType(serializer.Deserialize(reader), Sequence)

                    If sourceSeq Is sequenceLeft Then
                        sequenceLeft = reloadedSource
                        ' Refresh source list too
                        PopulateFOIsets(chkListFOIsetsLeft, sequenceLeft, lblStatusLeft)
                    Else
                        sequenceRight = reloadedSource
                        ' Refresh source list too
                        PopulateFOIsets(chkListFOIsetsRight, sequenceRight, lblStatusRight)
                    End If
                End Using

            Catch ex As Exception
                MessageBox.Show($"Warning: Could not reload sequences: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try

            ' Reload the target list to show the changes
            PopulateFOIsets(targetList, targetSeq, targetStatus)

            ' Show success message
            Dim message = $"Operation completed successfully!{vbCrLf}{vbCrLf}" &
                         $"New FOIsets added: {copiedCount}{vbCrLf}" &
                         $"Existing FOIsets replaced: {replacedCount}{vbCrLf}{vbCrLf}" &
                         $"Total FOIsets copied: {selectedFOIsets.Count}"

            MessageBox.Show(message, "Copy Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Clear selections in source list
            For i As Integer = 0 To sourceList.Items.Count - 1
                sourceList.SetItemChecked(i, False)
            Next

        Catch ex As Exception
            MessageBox.Show($"Error copying FOIsets: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function FindFOIsetNodeByDescription(xmlDoc As XmlDocument, description As String) As XmlNode
        ' Search for FOIset with matching POISetDescription
        Dim nodes = xmlDoc.SelectNodes("//FOIset[POISetDescription='" & description & "']")
        If nodes IsNot Nothing AndAlso nodes.Count > 0 Then
            Return nodes(0)
        End If
        Return Nothing
    End Function

    Private Function GetMaxIDFromXml(xmlDoc As XmlDocument) As Integer
        Dim maxID = 0
        Dim idNodes = xmlDoc.SelectNodes("//FOIset/ID")

        For Each node As XmlNode In idNodes
            Dim id As Integer
            If Integer.TryParse(node.InnerText, id) Then
                If id > maxID Then maxID = id
            End If
        Next

        Return maxID
    End Function

    Private Function GenerateUniqueID(sequence As Sequence) As Integer
        Dim allFOIsets = GetAllFOIsetsFlat(sequence.FOImanager.FOIsets)
        Dim maxID = If(allFOIsets.Any(), allFOIsets.Max(Function(f) f.ID), 0)
        Return maxID + 1
    End Function

    Private Sub SaveSequence(sequence As Sequence, path As String)
        ' This method is no longer used - we use XmlDocument.Save() directly
        ' to preserve original formatting and empty elements
    End Sub
End Class