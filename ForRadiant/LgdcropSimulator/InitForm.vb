Imports System.IO
Imports System.Reflection

Public Class InitForm
    Public Property Month As Integer
    Public Property Day As Integer
    Public Property CompMode As String
    Public Property DefaultFolder As String
    Public Property Model As String
    Public Property PID As String
    Public Property INIFile As String

    Private SaveFilePath As String = "InitFormLastInput.txt"
    Private Sub InitForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadLastInput()
    End Sub

    Private Sub BtnOK_Click(sender As Object, e As EventArgs) Handles BtnOK.Click
        Integer.TryParse(txtMonth.Text, Month)
        Integer.TryParse(txtDay.Text, Day)
        CompMode = txtCompMode.Text
        DefaultFolder = txtDefaultFolder.Text
        Model = txtModel.Text
        PID = txtPID.Text
        Dim assemblyPath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        INIFile = Path.Combine(assemblyPath, "CropDLL", txtINIFile.Text)
        SaveLastInput()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SaveLastInput()
        Dim lines As String() = {
            txtMonth.Text,
            txtDay.Text,
            txtCompMode.Text,
            txtDefaultFolder.Text,
            txtModel.Text,
            txtPID.Text,
            txtINIFile.Text
        }
        File.WriteAllLines(SaveFilePath, lines)
    End Sub

    Private Sub LoadLastInput()
        If File.Exists(SaveFilePath) Then
            Dim lines As String() = File.ReadAllLines(SaveFilePath)
            If lines.Length >= 7 Then
                txtMonth.Text = lines(0)
                txtDay.Text = lines(1)
                txtCompMode.Text = lines(2)
                txtDefaultFolder.Text = lines(3)
                txtModel.Text = lines(4)
                txtPID.Text = lines(5)
                txtINIFile.Text = lines(6)
            End If
        End If
    End Sub

End Class
