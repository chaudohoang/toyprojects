Public Class FOIsetItem
    Public Property FOIset As FOIset
    
    Public Sub New(foiSet As FOIset)
        Me.FOIset = foiSet
    End Sub
    
    Public Overrides Function ToString() As String
        Return $"{FOIset.FOISetDescription} (ID: {FOIset.ID})"
    End Function
End Class