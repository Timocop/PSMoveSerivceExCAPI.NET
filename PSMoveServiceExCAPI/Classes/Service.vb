Imports System.Runtime.InteropServices
Imports System.Text

Partial Public Class PSMoveServiceExCAPI
    Class Service
        Implements IDisposable

        Private g_sIP As String = ""
        Private g_sPort As String = ""

        Private g_sServerProtocolVersion As String = Nothing

        Public Sub New()
            Me.New(Constants.PSMOVESERVICE_DEFAULT_ADDRESS, Constants.PSMOVESERVICE_DEFAULT_PORT)
        End Sub

        Public Sub New(_IP As String)
            Me.New(_IP, Constants.PSMOVESERVICE_DEFAULT_PORT)
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
            If (IsInitialized()) Then
                Throw New ServiceExceptions.ServiceInitializationException("Service client already initialized")
            End If

            ServiceExceptions.ThrowExceptionServiceRequest("PSM_Initialize failed", PInvoke.PSM_Initialize(g_sIP, g_sPort, iTimeout))

            If (GetClientProtocolVersion() <> GetServerProtocolVersion()) Then
                Throw New ServiceExceptions.ServiceVersionException("Service protocol version mismatch")
            End If
        End Sub

        Public Function GetServerProtocolVersion() As String
            If (g_sServerProtocolVersion IsNot Nothing) Then
                Return g_sServerProtocolVersion
            End If

            Dim sServerVersion As New StringBuilder(Constants.PSMOVESERVICE_MAX_VERSION_STRING_LEN)
            ServiceExceptions.ThrowExceptionServiceRequest("PSM_GetServiceVersionString failed", PInvoke.PSM_GetServiceVersionString(sServerVersion, sServerVersion.Capacity, Constants.PSM_DEFAULT_TIMEOUT))

            g_sServerProtocolVersion = sServerVersion.ToString

            Return sServerVersion.ToString
        End Function

        Public Function GetClientProtocolVersion() As String
            Dim hString As IntPtr = PInvoke.PSM_GetClientVersionString()
            Return Marshal.PtrToStringAnsi(hString)
        End Function

        Public Function IsInitialized() As Boolean
            Return PInvoke.PSM_GetIsInitialized() > 0
        End Function

        Public Function IsConnected() As Boolean
            Return PInvoke.PSM_GetIsConnected() > 0
        End Function

        Public Sub Disconnect()
            If (IsInitialized()) Then
                PInvoke.PSM_Shutdown()
            End If

            g_sServerProtocolVersion = Nothing
        End Sub

        Public Sub Update()
            If (Not IsInitialized()) Then
                Throw New ServiceExceptions.ServiceInitializationException("Service client not initialized")
            End If

            ServiceExceptions.ThrowExceptionServiceRequest("PSM_Update failed", PInvoke.PSM_Update())

            If (Not IsConnected()) Then
                Throw New ServiceExceptions.ServiceDisconnectedException("Not connected to service")
            End If
        End Sub

        Public Sub UpdateNoPollMessages()
            If (Not IsInitialized()) Then
                Throw New ServiceExceptions.ServiceInitializationException("Service client not initialized")
            End If

            ServiceExceptions.ThrowExceptionServiceRequest("PSM_UpdateNoPollMessages failed", PInvoke.PSM_UpdateNoPollMessages())

            If (Not IsConnected()) Then
                Throw New ServiceExceptions.ServiceDisconnectedException("Not connected to service")
            End If
        End Sub

        Public Function HasConnectionStatusChanged() As Boolean
            Return CBool(PInvoke.PSM_HasConnectionStatusChanged())
        End Function

        Public Function HasControllerListChanged() As Boolean
            Return CBool(PInvoke.PSM_HasControllerListChanged())
        End Function

        Public Function HasTrackerListChanged() As Boolean
            Return CBool(PInvoke.PSM_HasTrackerListChanged())
        End Function

        Public Function HasHMDListChanged() As Boolean
            Return CBool(PInvoke.PSM_HasHMDListChanged())
        End Function

        Public Function WasSystemButtonPressed() As Boolean
            Return CBool(PInvoke.PSM_WasSystemButtonPressed())
        End Function

        Public Function HasPlayspaceOffsetChanged() As Boolean
            Return CBool(PInvoke.PSM_HasPlayspaceOffsetChanged())
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