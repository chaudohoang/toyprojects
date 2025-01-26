Imports System.IO

Public Class MapForm
    Public Property MatPTN As UShort(,)
    Public Property ColorIdx As Integer

    Private SaveFilePath As String = "MapFormLastInput.txt"

    Private Sub MapForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadLastInput()
    End Sub

    Private Sub BtnOK_Click(sender As Object, e As EventArgs) Handles BtnOK.Click
        ColorIdx = Integer.Parse(txtColorIdx.Text)
        ' Initialize `_matPTN` as needed
        SaveLastInput()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SaveLastInput()
        Dim lines As New List(Of String)
        lines.Add(txtColorIdx.Text)
        lines.Add(txtCalImage.Text)
        File.WriteAllLines(SaveFilePath, lines)
    End Sub

    Private Sub LoadLastInput()
        If File.Exists(SaveFilePath) Then
            Dim lines As String() = File.ReadAllLines(SaveFilePath)
            If lines.Length > 0 Then
                txtColorIdx.Text = lines(0)
                txtCalImage.Text = lines(1)
            End If
        End If
    End Sub

End Class