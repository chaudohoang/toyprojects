Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class MainForm
    Inherits Form

    Private lblMessage As Label
    Private txtName    As TextBox
    Private btnHello   As Button

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text            = "Hello WinForms"
        Me.Size            = New Size(360, 220)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.StartPosition   = FormStartPosition.CenterScreen

        lblMessage = New Label With {
            .Text     = "Enter your name:",
            .Location = New Point(20, 20),
            .Size     = New Size(300, 24),
            .Font     = New Font("Segoe UI", 10)
        }

        txtName = New TextBox With {
            .Location = New Point(20, 50),
            .Size     = New Size(300, 26),
            .Font     = New Font("Segoe UI", 10)
        }

        btnHello = New Button With {
            .Text     = "Say Hello",
            .Location = New Point(20, 90),
            .Size     = New Size(100, 32),
            .Font     = New Font("Segoe UI", 10)
        }
        AddHandler btnHello.Click, AddressOf btnHello_Click

        Me.Controls.Add(lblMessage)
        Me.Controls.Add(txtName)
        Me.Controls.Add(btnHello)
    End Sub

    Private Sub btnHello_Click(sender As Object, e As EventArgs)
        Dim name As String = txtName.Text.Trim()
        If String.IsNullOrEmpty(name) Then
            lblMessage.Text = "Hello, World!"
        Else
            lblMessage.Text = "Hello, " & name & "!"
        End If
    End Sub

End Class