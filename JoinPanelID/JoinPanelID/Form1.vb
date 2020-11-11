Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Not TextBox1.Text = "" And Not TextBox2.Text = "" Then
            TextBox4.Text = ""

            For Each s As String In Me.TextBox1.Text.Split({Environment.NewLine}, StringSplitOptions.None)

                If Not s = "" Then
                    If s.Length > TextBox2.Text Then
                        s = s.Substring(0, TextBox2.Text)
                        TextBox4.Text += s & TextBox3.Text
                    Else
                        TextBox4.Text += s & TextBox3.Text
                    End If
                End If

            Next

            TextBox4.Text = TextBox4.Text.Substring(0, TextBox4.Text.Length - TextBox3.TextLength)
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Clipboard.Clear()
        Clipboard.SetText(TextBox4.Text)
    End Sub
End Class
