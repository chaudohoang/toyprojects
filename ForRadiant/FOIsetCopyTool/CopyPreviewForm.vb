Imports System.Drawing
Imports System.Windows.Forms

Public Class CopyPreviewForm
    Inherits Form

    Private lstPreview As ListView
    Private btnOK As Button
    Private btnCancel As Button
    Private lblSummary As Label

    Public Sub New(allItems As List(Of FOIset), toAdd As List(Of FOIset), toReplace As List(Of FOIset))
        InitializeComponent()
        PopulatePreview(allItems, toAdd, toReplace)
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "Copy Preview"
        Me.Size = New Size(800, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        Dim lblTitle As New Label With {
            .Text = "The following FOIsets will be copied:",
            .Location = New Point(10, 10),
            .Size = New Size(760, 25),
            .Font = New Font("Arial", 11, FontStyle.Bold)
        }

        lstPreview = New ListView With {
            .Location = New Point(10, 40),
            .Size = New Size(760, 340),
            .View = View.Details,
            .FullRowSelect = True,
            .GridLines = True
        }

        lstPreview.Columns.Add("FOIset Description", 400)
        lstPreview.Columns.Add("Current ID", 80)
        lstPreview.Columns.Add("Action", 120)
        lstPreview.Columns.Add("New ID", 80)

        lblSummary = New Label With {
            .Location = New Point(10, 385),
            .Size = New Size(760, 40),
            .Font = New Font("Arial", 9, FontStyle.Bold),
            .ForeColor = Color.DarkBlue
        }

        btnOK = New Button With {
            .Text = "Proceed with Copy",
            .Location = New Point(520, 430),
            .Size = New Size(130, 30),
            .DialogResult = DialogResult.OK
        }

        btnCancel = New Button With {
            .Text = "Cancel",
            .Location = New Point(660, 430),
            .Size = New Size(110, 30),
            .DialogResult = DialogResult.Cancel
        }

        Me.Controls.AddRange({lblTitle, lstPreview, lblSummary, btnOK, btnCancel})
        Me.AcceptButton = btnOK
        Me.CancelButton = btnCancel
    End Sub

    Private Sub PopulatePreview(allItems As List(Of FOIset), toAdd As List(Of FOIset), toReplace As List(Of FOIset))
        lstPreview.Items.Clear()

        For Each foiSet In allItems
            Dim action As String
            Dim newID As String
            Dim color As Color

            If toReplace.Contains(foiSet) Then
                action = "REPLACE"
                newID = "(keep existing)"
                color = color.Orange
            Else
                action = "ADD NEW"
                newID = "(auto-generate)"
                color = color.Green
            End If

            Dim item As New ListViewItem(foiSet.FOISetDescription)
            item.SubItems.Add(foiSet.ID.ToString())
            item.SubItems.Add(action)
            item.SubItems.Add(newID)
            item.BackColor = color
            item.ForeColor = color.White

            lstPreview.Items.Add(item)
        Next

        lblSummary.Text = $"Summary: {toAdd.Count} new FOIset(s) will be added, {toReplace.Count} existing FOIset(s) will be replaced.{vbCrLf}" &
                         $"Total items to copy: {allItems.Count}"
    End Sub
End Class