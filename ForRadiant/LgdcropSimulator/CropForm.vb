Imports System.IO

Public Class CropForm
    Public Property PatternIdx As Integer
    Public Property ColorIdx As Integer
    Public Property MatPTN As UShort(,)

    Private SaveFilePath As String = "CropFormLastInput.txt"

    Private Sub CropForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadLastInput()
    End Sub

    Private Sub BtnOK_Click(sender As Object, e As EventArgs) Handles BtnOK.Click
        PatternIdx = Integer.Parse(txtPatternIdx.Text)
        ColorIdx = Integer.Parse(txtColorIdx.Text)
        ' Initialize `MatPTN` as needed
        SaveLastInput()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SaveLastInput()
        Dim lines As New List(Of String)
        lines.Add(txtPatternIdx.Text)
        lines.Add(txtColorIdx.Text)
        lines.Add(txtCropImage.Text)
        File.WriteAllLines(SaveFilePath, lines)
    End Sub

    Private Sub LoadLastInput()
        If File.Exists(SaveFilePath) Then
            Dim lines As String() = File.ReadAllLines(SaveFilePath)
            If lines.Length > 1 Then
                txtPatternIdx.Text = lines(0)
                txtColorIdx.Text = lines(1)
                txtCropImage.Text = lines(2)
            End If
        End If
    End Sub

End Class