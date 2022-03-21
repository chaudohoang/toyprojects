Imports System
Imports System.IO

Namespace Program
    Friend Class Program
        Public Shared Sub Main(ByVal args As String())
            Dim rootDir = Directory.GetCurrentDirectory()
            Dim fileNames = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories)

            For Each path As String In fileNames
                Dim dir = IO.Path.GetDirectoryName(path)
                Dim fileName = IO.Path.GetFileNameWithoutExtension(path).Remove(IO.Path.GetFileNameWithoutExtension(path).Length - 2)
                Dim ext = IO.Path.GetExtension(path)
                Dim newPath = IO.Path.Combine(dir, fileName & ext)
                File.Move(path, newPath)
            Next
        End Sub
    End Class
End Namespace
