Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Public Class ServiceExceptions
    Public Class ServiceInitializationException
        Inherits Exception

        Private g_sExceptionMessage As String = "Unable to initilize service"

        Public Sub New()
        End Sub

        Public Sub New(sMessage As String)
            g_sExceptionMessage = sMessage
        End Sub

        Public Overrides Function ToString() As String
            Return g_sExceptionMessage
        End Function
    End Class

    Public Class ServiceDisconnectedException
        Inherits Exception

        Private g_sExceptionMessage As String = "Lost connection to service"

        Public Sub New()
        End Sub

        Public Sub New(sMessage As String)
            g_sExceptionMessage = sMessage
        End Sub

        Public Overrides Function ToString() As String
            Return g_sExceptionMessage
        End Function
    End Class


    Public Class ServiceVersionException
        Inherits Exception

        Private g_sExceptionMessage As String = "Service protocol version mismatch"

        Public Sub New()
        End Sub

        Public Sub New(sMessage As String)
            g_sExceptionMessage = sMessage
        End Sub

        Public Overrides Function ToString() As String
            Return g_sExceptionMessage
        End Function
    End Class

    Public Class ServiceRequestFailed
        Inherits Exception

        Private g_sExceptionMessage As String = "Request failed"

        Public Sub New()
        End Sub

        Public Sub New(sMessage As String)
            g_sExceptionMessage = sMessage
        End Sub

        Public Overrides Function ToString() As String
            Return g_sExceptionMessage
        End Function
    End Class

    Public Class ServiceDeviceOutOfRangeException
        Inherits Exception

        Private g_sExceptionMessage As String = "Device id out of range"

        Public Sub New()
        End Sub

        Public Sub New(sMessage As String)
            g_sExceptionMessage = sMessage
        End Sub

        Public Overrides Function ToString() As String
            Return g_sExceptionMessage
        End Function
    End Class

    Public Shared Sub ThrowExceptionOnServiceStatus()
        If (Not (PInvoke.PSM_GetIsInitialized() > 0)) Then
            Throw New ServiceInitializationException("Service client not initialized")
        End If

        If (Not (PInvoke.PSM_GetIsConnected() > 0)) Then
            Throw New ServiceDisconnectedException("Not connected to service")
        End If
    End Sub

    Public Shared Function ThrowExceptionServiceRequest(sMessage As String, iResult As Integer) As PSMResult
        Return ThrowExceptionServiceRequest(sMessage, CType(iResult, PSMResult))
    End Function

    Public Shared Function ThrowExceptionServiceRequest(sMessage As String, iResult As PSMResult) As PSMResult
        Select Case (iResult)
            Case PSMResult.PSMResult_Timeout
                Throw New ServiceDisconnectedException(sMessage)

            Case PSMResult.PSMResult_Success
                ' All good

            Case Else
                Throw New ServiceRequestFailed(sMessage)
        End Select

        Return iResult
    End Function

End Class
