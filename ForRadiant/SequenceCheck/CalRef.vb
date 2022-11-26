Imports System.IO

Public Class CalRef
    Dim exePath As String = My.Application.Info.DirectoryPath
    Private Sub btnAddColorCalRule_Click(sender As Object, e As EventArgs) Handles btnAddColorCalRef.Click
        If txtColorCalID.Text <> "" AndAlso txtColorCalDescription.Text <> "" Then
            Dim text = "ColorCalID" + "," + txtColorCalID.Text + "," + txtColorCalDescription.Text
            If Not CalReferenceBox.Items.Contains(text) Then
                CalReferenceBox.Items.Add(text)
            End If
        End If
    End Sub

    Private Sub CalRef_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim calRefFile = Path.Combine(exePath, "calreference.txt")
        Dim calRuleFile = Path.Combine(exePath, "calrule.txt")
        If File.Exists(calRefFile) Then
            For Each line As String In File.ReadLines(calRefFile)
                CalReferenceBox.Items.Add(line)
            Next
        End If
        If File.Exists(calRuleFile) Then
            For Each line As String In File.ReadLines(calRuleFile)
                CalRuleBox.Items.Add(line)
            Next
        End If
    End Sub

    Private Sub btnAddColorCalRule_Click_1(sender As Object, e As EventArgs) Handles btnAddColorCalRule.Click
        If txtStep.Text <> "" AndAlso txtStepCalID.Text <> "" Then
            Dim text = "ColorCalID" + "," + txtStep.Text + "," + txtStepCalID.Text
            If Not CalRuleBox.Items.Contains(text) Then
                CalRuleBox.Items.Add(text)
            End If
        End If
    End Sub

    Private Sub btnDelCalRef_Click(sender As Object, e As EventArgs) Handles btnDelCalRef.Click
        CalReferenceBox.Items.Remove(CalReferenceBox.Items(CalReferenceBox.SelectedIndex))
    End Sub
End Class