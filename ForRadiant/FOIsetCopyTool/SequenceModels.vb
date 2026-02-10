Imports System.Xml.Serialization

<XmlRoot("Sequence")>
Public Class Sequence
    Public Property XmlVersion As String
    Public Property ProductVersion As String
    Public Property ChannelIndex As Integer
    Public Property ChannelActive As Boolean

    <XmlElement("Items")>
    Public Property Items As Object

    <XmlElement("PatternSetupList")>
    Public Property PatternSetupList As Object

    Public Property FOImanager As FOImanager

    Public Property ModelName As String
    Public Property NumUnitsTested As Integer
    Public Property NumUnitsFailed As Integer
    Public Property CameraDistanceM As Double

    <XmlElement("PatternGen")>
    Public Property PatternGen As Object
End Class

Public Class FOImanager
    <XmlArray("FOIsets")>
    <XmlArrayItem("FOIset")>
    Public Property FOIsets As List(Of FOIset)

    <XmlArray("CharacterList")>
    <XmlArrayItem("String")>
    Public Property CharacterList As List(Of String)

    Public Sub New()
        FOIsets = New List(Of FOIset)
        CharacterList = New List(Of String)
    End Sub
End Class

Public Class FOIset
    ' Nested FOIsets
    <XmlElement("FOIset")>
    Public Property ChildFOIsets As List(Of FOIset)

    ' FOI configurations - use Object to avoid type checking
    <XmlElement("FOIs")>
    Public Property FOIs As Object

    Public Property ID As Integer

    ' IMPORTANT: The XML uses POISetDescription not FOISetDescription!
    <XmlElement("POISetDescription")>
    Public Property FOISetDescription As String

    Public Property SpectralResponseType As String
    Public Property PhotometricTermType As String
    Public Property PhotometricUnitType As String
    Public Property GeometryDistanceUnit As String
    Public Property CoordinateSystem As String
    Public Property Notes As String
    Public Property UsePixelSum As Boolean
    Public Property DrawMaxMinLocations As Boolean
    Public Property HideLabels As Boolean
    Public Property PassColor As String
    Public Property FailColor As String

    Public Sub New()
        ChildFOIsets = New List(Of FOIset)
    End Sub
End Class