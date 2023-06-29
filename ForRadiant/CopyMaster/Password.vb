Imports System
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports CopyMasterAppData.CopyMasterAppData
Imports Microsoft.VisualBasic

Public Class Password
    Public Property password As String = ""

    Private Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click
        Dim passwordEntered As Boolean
        password = TextBox1.Text
        If password = "" Then
            MessageBox.Show("Password cannot be blank")
        Else
            passwordEntered = True
        End If
        If passwordEntered Then
            Me.Close()
        End If

    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If (e.KeyCode = Keys.Enter) Then

            Button1_Click(Me, New EventArgs())
        End If

        If (e.KeyCode = Keys.Escape) Then
            Me.Close()
        End If

    End Sub

End Class