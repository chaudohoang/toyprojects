Imports System.IO
Imports System.Reflection
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Public Class CropForm
    Public Property PatternIdx As Integer
    Public Property ColorIdx As Integer
    Public Property CropIMG As String
    Public Property MatPTN As UShort(,)

    Private Property LGDCropModel As String
    Private SaveFilePath As String = "CropFormLastInput.txt"
    Private SaveFilePathMainForm As String = "MainFormLastInput.txt"

    Private Sub CropForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadLastInput()
        LoadLastInputMainForm()
    End Sub

    Private Sub BtnOK_Click(sender As Object, e As EventArgs) Handles BtnOK.Click
        PatternIdx = Integer.Parse(txtPatternIdx.Text)
        ColorIdx = Integer.Parse(txtColorIdx.Text)
        Dim assemblyPath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        CropIMG = Path.Combine(assemblyPath, "InputIMG" + LGDCropModel, txtCropImage.Text & ".tif")
        Dim ImgArr12(,) As Single = ScaleAndRotateImageArray(CropIMG)
        Dim ImgArr12Ushort(,) As UShort = ConvertDataArrayToJaggedShortArray(ImgArr12, txtCropImage.Text)
        MatPTN = ImgArr12Ushort
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
        lines.Add(txtResolution.Text)
        File.WriteAllLines(SaveFilePath, lines)
    End Sub

    Private Sub LoadLastInput()
        If File.Exists(SaveFilePath) Then
            Dim lines As String() = File.ReadAllLines(SaveFilePath)
            If lines.Length > 1 Then
                txtPatternIdx.Text = lines(0)
                txtColorIdx.Text = lines(1)
                txtCropImage.Text = lines(2)
                txtResolution.Text = lines(3)
            End If
        End If
    End Sub

    Private Sub LoadLastInputMainForm()
        If File.Exists(SaveFilePathMainForm) Then
            Dim lines As String() = File.ReadAllLines(SaveFilePathMainForm)
            If lines.Length = 1 Then
                LGDCropModel = lines(0)
            End If
        End If
    End Sub

    Private Shared Function ConvertDataArrayToJaggedShortArray(DataArr As Single(,), imageFileName As String) As UShort(,)
        ' Remember, the imgArr is column-major
        Dim nbrCols As Integer = DataArr.GetUpperBound(0) + 1
        Dim nbrRows As Integer = DataArr.GetUpperBound(1) + 1

        ' Initialize the UShort(,) array
        Dim pixelData(nbrRows - 1, nbrCols - 1) As UShort

        ' Convert and populate the array
        For col As Integer = 0 To nbrCols - 1
            For row As Integer = 0 To nbrRows - 1
                pixelData(row, col) = ToUShort(DataArr(col, row))
            Next
        Next

        ' Get assembly path and construct output file path
        Dim assemblyPath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        Dim outputFilePath As String = Path.Combine(assemblyPath, "InputIMG", imageFileName & ".csv")

        ' Save array to CSV
        'SaveFloatArrayToCsv(DataArr, outputFilePath)

        Return pixelData
    End Function

    Private Shared Function ToUShort(Value As Single) As UShort
        Return CUShort(Math.Min(Math.Max(0, Value), 65535))
    End Function

    Private Shared Sub SaveFloatArrayToCsv(DataArr As Single(,), filePath As String)
        Dim nbrCols As Integer = DataArr.GetUpperBound(0) + 1
        Dim nbrRows As Integer = DataArr.GetUpperBound(1) + 1

        Using writer As New StreamWriter(filePath)
            ' Write the number of columns and rows as the first line
            writer.WriteLine(nbrCols & "," & nbrRows)

            ' Write the image data
            For row As Integer = 0 To nbrRows - 1
                Dim rowValues As New List(Of String)

                For col As Integer = 0 To nbrCols - 1
                    rowValues.Add(DataArr(col, row).ToString("0.####")) ' Keeps up to 4 decimal places
                Next

                writer.WriteLine(String.Join(",", rowValues))
            Next
        End Using
    End Sub

    Private Function ScaleAndRotateImageArray(bmp As String) As Single(,)
        ' Scale image to designated bit depth (normalize pixel values)
        Dim imgData As Single(,) = Convert16BitTifToSingleArray(bmp)

        ' Rotate image 90 degrees counterclockwise
        'Dim rotatedData As Single(,) = RotateSingleArrayCounterClockwise90(imgData)

        'Return rotatedData

        Return imgData
    End Function

    Private Function RotateSingleArrayCounterClockwise90(input As Single(,)) As Single(,)
        Dim oldWidth As Integer = input.GetLength(0)
        Dim oldHeight As Integer = input.GetLength(1)
        Dim rotated(oldHeight - 1, oldWidth - 1) As Single

        ' Perform rotation (transpose + flip vertically)
        For x As Integer = 0 To oldWidth - 1
            For y As Integer = 0 To oldHeight - 1
                rotated(y, oldWidth - 1 - x) = input(x, y)
            Next
        Next

        Return rotated
    End Function

    Private Function Convert16BitTifToSingleArray(filePath As String) As Single(,)
        ' Load the TIFF image using WPF
        Dim bitmap As BitmapSource = New TiffBitmapDecoder(New Uri(filePath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames(0)

        ' Ensure the image is 16-bit grayscale
        If bitmap.Format <> PixelFormats.Gray16 Then
            Throw New ArgumentException("Only 16-bit grayscale TIFFs are supported.")
        End If

        Dim width As Integer = bitmap.PixelWidth
        Dim height As Integer = bitmap.PixelHeight
        Dim imgArray(width - 1, height - 1) As Single

        ' Buffer for raw pixel data
        Dim stride As Integer = (width * 2) ' 16-bit (2 bytes per pixel)
        Dim buffer As Byte() = New Byte(stride * height - 1) {}

        ' Copy pixel data
        bitmap.CopyPixels(buffer, stride, 0)

        ' Convert buffer to Single(,) array without scaling
        For y As Integer = 0 To height - 1
            For x As Integer = 0 To width - 1
                Dim index As Integer = y * stride + (x * 2) ' 2 bytes per pixel
                Dim lowByte As Byte = buffer(index)
                Dim highByte As Byte = buffer(index + 1)
                Dim pixelValue As UShort = CUShort(highByte) << 8 Or lowByte ' Combine bytes

                ' Directly assign the pixel value to imgArray
                imgArray(x, y) = pixelValue
            Next
        Next

        If txtResolution.Text <> "1" Then
            ' First, get the new width and height based on resolution
            Dim newWidth As Integer = bitmap.PixelWidth * Int32.Parse(txtResolution.Text)
            Dim newHeight As Integer = bitmap.PixelHeight * Int32.Parse(txtResolution.Text)

            ' Interpolate the image to the new size
            Dim scaledImage As Single(,) = InterpolateImage(imgArray, newWidth, newHeight)
            Return scaledImage
        Else
            Return imgArray
        End If

    End Function

    Private Function InterpolateImage(imgArray As Single(,), newWidth As Integer, newHeight As Integer) As Single(,)
        Dim interpolatedArray(newWidth - 1, newHeight - 1) As Single
        Dim originalWidth As Integer = imgArray.GetLength(0)
        Dim originalHeight As Integer = imgArray.GetLength(1)

        For newY As Integer = 0 To newHeight - 1
            For newX As Integer = 0 To newWidth - 1
                ' Find corresponding position in original image
                Dim srcX As Double = newX * (originalWidth - 1) / (newWidth - 1)
                Dim srcY As Double = newY * (originalHeight - 1) / (newHeight - 1)

                ' Perform bilinear interpolation
                Dim x1 As Integer = Math.Floor(srcX)
                Dim x2 As Integer = Math.Min(x1 + 1, originalWidth - 1)
                Dim y1 As Integer = Math.Floor(srcY)
                Dim y2 As Integer = Math.Min(y1 + 1, originalHeight - 1)

                Dim dx As Double = srcX - x1
                Dim dy As Double = srcY - y1

                ' Get the four neighboring pixel values
                Dim topLeft As Single = imgArray(x1, y1)
                Dim topRight As Single = imgArray(x2, y1)
                Dim bottomLeft As Single = imgArray(x1, y2)
                Dim bottomRight As Single = imgArray(x2, y2)

                ' Interpolate in both directions
                Dim topInterp As Single = topLeft + (topRight - topLeft) * dx
                Dim bottomInterp As Single = bottomLeft + (bottomRight - bottomLeft) * dx
                interpolatedArray(newX, newY) = topInterp + (bottomInterp - topInterp) * dy
            Next
        Next

        Return interpolatedArray
    End Function


End Class