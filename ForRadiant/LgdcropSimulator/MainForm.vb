Imports System.Reflection
Imports System.IO

Public Class MainForm
    Private LgdLoadedAssembly As Assembly
    Private LgdInstance As Object
    Private LGD_COMP_CLEAR_NET_MethodInfo As MethodInfo
    Private LGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo As MethodInfo
    Private LGD_COMP_INIT_NET_MethodInfo As MethodInfo
    Private LGD_COMP_MAPPING_NET_MethodInfo As MethodInfo

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDLLAndInitialize()
    End Sub

    Private Sub LoadDLLAndInitialize()
        Try
            Dim assemblyPath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            Dim dllPath As String = Path.Combine(assemblyPath, "CropDLL", "LGD_PO_Compensation_Net.dll")
            LgdLoadedAssembly = Assembly.LoadFrom(dllPath)

            Dim typeName As String = "LGDPOCompensationNet.LGD_PO_Compensation_Net"
            Dim loadedType As Type = LgdLoadedAssembly.GetType(typeName)

            If loadedType IsNot Nothing Then
                If LgdInstance Is Nothing Then LgdInstance = Activator.CreateInstance(loadedType)

                If LGD_COMP_CLEAR_NET_MethodInfo Is Nothing Then LGD_COMP_CLEAR_NET_MethodInfo = loadedType.GetMethod("LGD_COMP_CLEAR_NET", {})
                If LGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo Is Nothing Then LGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo = loadedType.GetMethod("LGD_COMP_CROP_TO_COMP_DATA_NET", {GetType(Integer), GetType(Integer), GetType(UShort(,)), GetType(List(Of String))})
                If LGD_COMP_INIT_NET_MethodInfo Is Nothing Then LGD_COMP_INIT_NET_MethodInfo = loadedType.GetMethod("LGD_COMP_INIT_NET", {GetType(Integer).MakeByRefType, GetType(Integer).MakeByRefType, GetType(String), GetType(String), GetType(String), GetType(String), GetType(String)})
                If LGD_COMP_MAPPING_NET_MethodInfo Is Nothing Then LGD_COMP_MAPPING_NET_MethodInfo = loadedType.GetMethod("LGD_COMP_MAPPING_NET", {GetType(UShort(,)), GetType(Integer), GetType(List(Of String))})
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading DLL: " & ex.Message)
        End Try
    End Sub

    Public Function UseLGD_COMP_MAPPING_NET_MethodInfo(_matDotPTN As UShort(,), _nColorIdx As Integer, _mtfLogData As List(Of String)) As Integer
        Try
            If LgdInstance IsNot Nothing AndAlso LGD_COMP_MAPPING_NET_MethodInfo IsNot Nothing Then
                Return CType(LGD_COMP_MAPPING_NET_MethodInfo.Invoke(LgdInstance, {_matDotPTN, _nColorIdx, _mtfLogData}), Integer)
            End If
        Catch ex As Exception
            MessageBox.Show("Error Code: " & ex.Message)
            Return -1
        End Try
        Return -2
    End Function

    Public Function UseLGD_COMP_CLEAR_NET_MethodInfo() As Integer
        Try
            If LgdInstance IsNot Nothing AndAlso LGD_COMP_CLEAR_NET_MethodInfo IsNot Nothing Then
                Return CType(LGD_COMP_CLEAR_NET_MethodInfo.Invoke(LgdInstance, Nothing), Integer)
            End If
        Catch ex As Exception
            MessageBox.Show("Error Code: " & ex.Message)
            Return -1
        End Try
        Return -2
    End Function

    Public Function UseLGD_COMP_INIT_NET_MethodInfo(ByRef _nMonth As Integer, ByRef _nDay As Integer, _strCompMode As String, _strDefaultFolder As String, _strModel As String, _strPID As String, _strINIFile As String) As Integer
        Try
            If LgdInstance IsNot Nothing AndAlso LGD_COMP_INIT_NET_MethodInfo IsNot Nothing Then
                Return CType(LGD_COMP_INIT_NET_MethodInfo.Invoke(LgdInstance, {_nMonth, _nDay, _strCompMode, _strDefaultFolder, _strModel, _strPID, _strINIFile}), Integer)
            End If
        Catch ex As Exception
            MessageBox.Show("Error Code: " & ex.Message)
            Return -1
        End Try
        Return -2
    End Function

    Public Function UseLGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo(_nPTN As Integer, _nColor As Integer, _matPTN As UShort(,), _ccdLogData As List(Of String)) As Integer
        Try
            If LgdInstance IsNot Nothing AndAlso LGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo IsNot Nothing Then
                Return CType(LGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo.Invoke(LgdInstance, {_nPTN, _nColor, _matPTN, _ccdLogData}), Integer)
            End If
        Catch ex As Exception
            MessageBox.Show("Error Code: " & ex.Message)
            Return -1
        End Try
        Return -2
    End Function

    Private Sub BtnClear_Click(sender As Object, e As EventArgs) Handles BtnClear.Click
        Dim result As Integer = UseLGD_COMP_CLEAR_NET_MethodInfo()
        MessageBox.Show("LGD_COMP_CLEAR_NET executed. Error Code: " & result)
    End Sub

    Private Sub BtnInit_Click(sender As Object, e As EventArgs) Handles BtnInit.Click
        Dim inputForm As New InitForm()
        If inputForm.ShowDialog() = DialogResult.OK Then
            Dim result As Integer = UseLGD_COMP_INIT_NET_MethodInfo(inputForm.Month, inputForm.Day, inputForm.CompMode, inputForm.DefaultFolder, inputForm.Model, inputForm.PID, inputForm.INIFile)
            MessageBox.Show("LGD_COMP_INIT_NET executed. Error Code: " & result)
        End If
    End Sub

    Private Sub BtnCrop_Click(sender As Object, e As EventArgs) Handles BtnCrop.Click
        Dim inputForm As New CropForm()
        Dim CCDLogData As New List(Of String)
        If inputForm.ShowDialog() = DialogResult.OK Then
            Dim result As Integer = UseLGD_COMP_CROP_TO_COMP_DATA_NET_MethodInfo(inputForm.PatternIdx, inputForm.ColorIdx, inputForm.MatPTN, CCDLogData)
            MessageBox.Show("LGD_COMP_CROP_TO_COMP_DATA_NET executed. Error Code: " & result)
        End If
    End Sub

    Private Sub BtnMap_Click(sender As Object, e As EventArgs) Handles BtnMap.Click
        Dim inputForm As New MapForm()
        Dim MTFLogData As New List(Of String)
        If inputForm.ShowDialog() = DialogResult.OK Then
            Dim result As Integer = UseLGD_COMP_MAPPING_NET_MethodInfo(inputForm.MatPTN, inputForm.ColorIdx, MTFLogData)
            MessageBox.Show("LGD_COMP_MAPPING_NET executed. Error Code: " & result)
        End If
    End Sub
End Class
