Imports System.IO

Public Class CalRef
    Dim exePath As String = My.Application.Info.DirectoryPath
    Private Sub btnAddColorCalRule_Click(sender As Object, e As EventArgs) Handles btnAddColorCalRef.Click
        If txtColorCalID.Text <> "" AndAlso txtColorCalDescription.Text <> "" Then
            Dim text = "ColorCalID" + "," + txtColorCalID.Text + "," + txtColorCalDescription.Text
            If Not CalRefBox.Items.Contains(text) Then
                CalRefBox.Items.Add(text)
            End If
        End If
    End Sub

    Private Sub CalRef_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim calRefFile = Path.Combine(exePath, "calreference.txt")
        Dim calRuleFile = Path.Combine(exePath, "calrule.txt")
        If File.Exists(calRefFile) Then
            For Each line As String In File.ReadLines(calRefFile)
                CalRefBox.Items.Add(line)
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
            Dim text = txtStep.Text + "," + txtStepCalID.Text
            If Not CalRuleBox.Items.Contains(text) Then
                CalRuleBox.Items.Add(text)
            End If
        End If
    End Sub

    Private Sub btnDelCalRef_Click(sender As Object, e As EventArgs) Handles btnDelCalRef.Click
        CalRefBox.Items.Remove(CalRefBox.Items(CalRefBox.SelectedIndex))
    End Sub

    Private Sub btnDelCalRule_Click(sender As Object, e As EventArgs) Handles btnDelCalRule.Click
        CalRuleBox.Items.Remove(CalRuleBox.Items(CalRuleBox.SelectedIndex))
    End Sub

    Private Sub btnDelCalRefAll_Click(sender As Object, e As EventArgs) Handles btnDelCalRefAll.Click
        CalRefBox.Items.Clear()
    End Sub

    Private Sub btnDelCalRuleAll_Click(sender As Object, e As EventArgs) Handles btnDelCalRuleAll.Click
        CalRuleBox.Items.Clear()
    End Sub

    Private Sub btnCalRefSave_Click(sender As Object, e As EventArgs) Handles btnCalRefSave.Click
        Dim calRefFile = Path.Combine(exePath, "calreference.txt")
        Dim calRuleFile = Path.Combine(exePath, "calrule.txt")
        Dim sb1 As New System.Text.StringBuilder()

        For Each o As Object In CalRefBox.Items
            sb1.AppendLine(o)
        Next

        System.IO.File.WriteAllText(calRefFile, sb1.ToString())

        Dim sb2 As New System.Text.StringBuilder()

        For Each o As Object In CalRuleBox.Items
            sb2.AppendLine(o)
        Next

        System.IO.File.WriteAllText(calRuleFile, sb2.ToString())
    End Sub
End Class