Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Partial Public Class PSMoveServiceExCAPI
    Class Service
        Implements IDisposable

        Private g_sIP As String = ""
        Private g_sPort As String = ""

        Private g_sServerProtocolVersion As String = Nothing

        Private g_bDidUpdate As Boolean = False

        Public Sub New()
            Me.New(PSMOVESERVICE_DEFAULT_ADDRESS, PSMOVESERVICE_DEFAULT_PORT)
        End Sub

        Public Sub New(_IP As String)
            Me.New(_IP, PSMOVESERVICE_DEFAULT_PORT)
        End Sub

        Public Sub New(_IP As String, _Port As String)
            g_sIP = _IP
            g_sPort = _Port
        End Sub

        ReadOnly Property m_IP As String
            Get
                Return g_sIP
            End Get
        End Property

        ReadOnly Property m_Port As String
            Get
                Return g_sPort
            End Get
        End Property

        Public Sub Connect(Optional iTimeout As Integer = 5000)
            If (PInvoke.PSM_Initialize(g_sIP, g_sPort, iTimeout) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_Initialize failed")
            End If

            If (GetClientProtocolVersion() <> GetServerProtocolVersion()) Then
                Throw New ArgumentException("Version mismatch")
            End If
        End Sub

        Public Function GetServerProtocolVersion() As String
            If (Not IsInitialized()) Then
                Throw New ArgumentException("Not initialized")
            End If

            If (g_sServerProtocolVersion IsNot Nothing) Then
                Return g_sServerProtocolVersion
            End If

            Dim sServerVersion As New System.Text.StringBuilder(PSMOVESERVICE_MAX_VERSION_STRING_LEN)
            If (PInvoke.PSM_GetServiceVersionString(sServerVersion, sServerVersion.Capacity, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_GetServiceVersionString failed")
            End If

            g_sServerProtocolVersion = sServerVersion.ToString

            Return sServerVersion.ToString
        End Function

        Public Function GetClientProtocolVersion() As String
            Dim hString As IntPtr = PInvoke.PSM_GetClientVersionString()
            Return Marshal.PtrToStringAnsi(hString)
        End Function

        Public Function IsInitialized() As Boolean
            Return PInvoke.PSM_GetIsInitialized()
        End Function

        Public Function IsConnected() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_GetIsConnected()
        End Function

        Public Sub Disconnect()
            If (IsInitialized()) Then
                PInvoke.PSM_Shutdown()
            End If

            g_sServerProtocolVersion = Nothing
        End Sub

        Public Sub Update()
            If (Not IsInitialized()) Then
                Throw New ArgumentException("Not initialized")
            End If

            If (PInvoke.PSM_Update() <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_Update failed")
            End If

            g_bDidUpdate = True
        End Sub

        Public Sub UpdateNoPollMessages()
            If (Not IsInitialized()) Then
                Throw New ArgumentException("Not initialized")
            End If

            If (PInvoke.PSM_UpdateNoPollMessages() <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_Update failed")
            End If
        End Sub

        Public Function HasConnectionStatusChanged() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_HasConnectionStatusChanged()
        End Function

        Public Function HasControllerListChanged() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_HasControllerListChanged()
        End Function

        Public Function HasTrackerListChanged() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_HasTrackerListChanged()
        End Function

        Public Function HasHMDListChanged() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_HasHMDListChanged()
        End Function

        Public Function WasSystemButtonPressed() As Boolean
            If (Not g_bDidUpdate) Then
                Update()
            End If

            Return PInvoke.PSM_WasSystemButtonPressed()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    Disconnect()
                End If
            End If
            disposedValue = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub
#End Region

    End Class
End Class