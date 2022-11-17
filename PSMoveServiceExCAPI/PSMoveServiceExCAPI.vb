Imports System.Drawing
Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Public Class PSMoveServiceExCAPI
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

    Class Controllers
        Implements IDisposable

        Private g_mInfo As Info
        Private g_bListening As Boolean = False
        Private g_bDataStream As Boolean = False
        Private g_iDataStreamFlags As PSMStreamFlags = PSMStreamFlags.PSMStreamFlags_defaultStreamOptions

        Public Sub New(_ControllerId As Integer)
            Me.New(_ControllerId, False)
        End Sub

        Public Sub New(_ControllerId As Integer, _StartDataStream As Boolean)
            If (_ControllerId < 0 OrElse _ControllerId > PSMOVESERVICE_MAX_CONTROLLER_COUNT - 1) Then
                Throw New ArgumentOutOfRangeException()
            End If

            g_mInfo = New Info(Me, _ControllerId)

            g_mInfo.Refresh(Info.RefreshFlags.RefreshType_All)

            m_Listening = True
            m_DataStreamEnabled = True
        End Sub

        Public Sub Refresh(iRefreshType As Info.RefreshFlags)
            g_mInfo.Refresh(iRefreshType)
        End Sub

        ReadOnly Property m_Info As Info
            Get
                Return g_mInfo
            End Get
        End Property

        Class Info
            Private g_Parent As Controllers

            Private g_PSState As IPSState = Nothing
            Private g_Physics As PSPhysics = Nothing
            Private g_Pose As PSPose = Nothing
            Private g_PSMoveRawSensor As PSMoveRawSensor = Nothing
            Private g_PSMoveCalibratedSensor As PSMoveCalibratedSensor = Nothing
            Private g_PSTracking As PSTracking

            Private g_iControllerId As Integer = -1

            Private g_iControllerType As PSMControllerType = PSMControllerType.PSMController_None
            Private g_iControllerHand As PSMControllerHand = PSMControllerHand.PSMControllerHand_Any
            Private g_sControllerSerial As String = ""
            Private g_sParentControllerSerial As String = ""
            Private g_iOutputSequenceNum As Integer
            Private g_iInputSequenceNum As Integer
            Private g_bIsConnected As Boolean
            Private g_iDataFrameLastReceivedTime As ULong
            Private g_iDataFrameAverageFPS As Single
            Private g_iListenerCount As Integer

            Enum RefreshFlags
                RefreshType_ProbeSerial = (1 << 0)
                RefreshType_Basic = (1 << 1)
                RefreshType_State = (1 << 2)
                RefreshType_Pose = (1 << 3)
                RefreshType_Physics = (1 << 4)
                RefreshType_Sensor = (1 << 5)
                RefreshType_Tracker = (1 << 6)
                RefreshType_All = (RefreshType_Basic Or RefreshType_State Or RefreshType_Pose Or RefreshType_Physics Or RefreshType_Sensor Or RefreshType_Tracker)
            End Enum

            Public Sub New(_Parent As Controllers, _ControllerID As Integer)
                g_Parent = _Parent
                g_iControllerId = _ControllerID
            End Sub

            ReadOnly Property m_ControllerId As Integer
                Get
                    Return g_iControllerId
                End Get
            End Property

            ReadOnly Property m_ControllerType As PSMControllerType
                Get
                    Return g_iControllerType
                End Get
            End Property

            Property m_ControllerHand As PSMControllerHand
                Get
                    Return g_iControllerHand
                End Get
                Set(value As PSMControllerHand)
                    If (g_iControllerHand = value) Then
                        Return
                    End If

                    If (PInvoke.PSM_SetControllerHand(m_ControllerId, value, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_SetControllerHand failed")
                    End If

                    g_iControllerHand = value
                End Set
            End Property

            ReadOnly Property m_ControllerSerial As String
                Get
                    Return g_sControllerSerial
                End Get
            End Property

            ReadOnly Property m_ParentControllerSerial As String
                Get
                    Return g_sParentControllerSerial
                End Get
            End Property

            ReadOnly Property m_OutputSequenceNum As Integer
                Get
                    Return g_iOutputSequenceNum
                End Get
            End Property

            ReadOnly Property m_InputSequenceNum As Integer
                Get
                    Return g_iInputSequenceNum
                End Get
            End Property

            ReadOnly Property m_IsConnected As Boolean
                Get
                    Return g_bIsConnected
                End Get
            End Property

            ReadOnly Property m_DataFrameLastReceivedTime As ULong
                Get
                    Return g_iDataFrameLastReceivedTime
                End Get
            End Property

            ReadOnly Property m_DataFrameAverageFPS As Single
                Get
                    Return g_iDataFrameAverageFPS
                End Get
            End Property

            ReadOnly Property m_ListenerCount As Integer
                Get
                    Return g_iListenerCount
                End Get
            End Property

            ReadOnly Property m_PSMoveState As PSMoveState
                Get
                    Return TryCast(g_PSState, PSMoveState)
                End Get
            End Property

            ReadOnly Property m_PSDualShock4State As PSDualShock4State
                Get
                    Return TryCast(g_PSState, PSDualShock4State)
                End Get
            End Property

            ReadOnly Property m_PSNaviState As PSNaviState
                Get
                    Return TryCast(g_PSState, PSNaviState)
                End Get
            End Property

            ReadOnly Property m_PSVirtualState As PSVirtualState
                Get
                    Return TryCast(g_PSState, PSVirtualState)
                End Get
            End Property

            ReadOnly Property m_Physics As PSPhysics
                Get
                    Return g_Physics
                End Get
            End Property

            ReadOnly Property m_Pose As PSPose
                Get
                    Return g_Pose
                End Get
            End Property

            ReadOnly Property m_PSMoveRawSensor As PSMoveRawSensor
                Get
                    Return g_PSMoveRawSensor
                End Get
            End Property

            ReadOnly Property m_PSMoveCalibratedSensor As PSMoveCalibratedSensor
                Get
                    Return g_PSMoveCalibratedSensor
                End Get
            End Property

            ReadOnly Property m_PSTracking As PSTracking
                Get
                    Return g_PSTracking
                End Get
            End Property

            Public Interface IPSState
            End Interface

            Class PSMoveState
                Implements IPSState

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSMove)
                    m_HasValidHardwareCalibration = CBool(_FromPinvoke.bHasValidHardwareCalibration)
                    m_IsTrackingEnabled = CBool(_FromPinvoke.bIsTrackingEnabled)
                    m_bIsCurrentlyTracking = CBool(_FromPinvoke.bIsCurrentlyTracking)
                    m_IsOrientationValid = CBool(_FromPinvoke.bIsOrientationValid)
                    m_IsPositionValid = CBool(_FromPinvoke.bIsPositionValid)
                    m_bHasUnpublishedState = CBool(_FromPinvoke.bHasUnpublishedState)

                    m_DevicePath = _FromPinvoke.DevicePath
                    m_DeviceSerial = _FromPinvoke.DeviceSerial
                    m_AssignedHostSerial = _FromPinvoke.AssignedHostSerial
                    m_PairedToHost = CBool(_FromPinvoke.PairedToHost)
                    m_ConnectionType = _FromPinvoke.ConnectionType
                    m_TrackingColorType = _FromPinvoke.TrackingColorType

                    m_TriangleButton = CType(_FromPinvoke.TriangleButton, PSMButtonState)
                    m_CircleButton = CType(_FromPinvoke.CircleButton, PSMButtonState)
                    m_CrossButton = CType(_FromPinvoke.CrossButton, PSMButtonState)
                    m_SquareButton = CType(_FromPinvoke.SquareButton, PSMButtonState)
                    m_SelectButton = CType(_FromPinvoke.SelectButton, PSMButtonState)
                    m_StartButton = CType(_FromPinvoke.StartButton, PSMButtonState)
                    m_PSButton = CType(_FromPinvoke.PSButton, PSMButtonState)
                    m_MoveButton = CType(_FromPinvoke.MoveButton, PSMButtonState)
                    m_TriggerButton = CType(_FromPinvoke.TriggerButton, PSMButtonState)
                    m_BatteryValue = CType(_FromPinvoke.BatteryValue, PSMBatteryState)
                    m_TriggerValue = _FromPinvoke.TriggerValue
                    m_Rumble = _FromPinvoke.Rumble
                    m_LED_r = _FromPinvoke.LED_r
                    m_LED_g = _FromPinvoke.LED_g
                    m_LED_b = _FromPinvoke.LED_b
                    m_ResetPoseButtonPressTime = _FromPinvoke.ResetPoseButtonPressTime
                    m_ResetPoseRequestSent = CBool(_FromPinvoke.bResetPoseRequestSent)
                    m_PoseResetButtonEnabled = CBool(_FromPinvoke.bPoseResetButtonEnabled)
                End Sub

                ReadOnly Property m_HasValidHardwareCalibration As Boolean
                ReadOnly Property m_IsTrackingEnabled As Boolean
                ReadOnly Property m_bIsCurrentlyTracking As Boolean
                ReadOnly Property m_IsOrientationValid As Boolean
                ReadOnly Property m_IsPositionValid As Boolean
                ReadOnly Property m_bHasUnpublishedState As Boolean

                ReadOnly Property m_DevicePath As String
                ReadOnly Property m_DeviceSerial As String
                ReadOnly Property m_AssignedHostSerial As String

                ReadOnly Property m_PairedToHost As Boolean
                ReadOnly Property m_ConnectionType As PSMConnectionType
                ReadOnly Property m_TrackingColorType As PSMTrackingColorType

                ReadOnly Property m_TriangleButton As PSMButtonState
                ReadOnly Property m_CircleButton As PSMButtonState
                ReadOnly Property m_CrossButton As PSMButtonState
                ReadOnly Property m_SquareButton As PSMButtonState
                ReadOnly Property m_SelectButton As PSMButtonState
                ReadOnly Property m_StartButton As PSMButtonState
                ReadOnly Property m_PSButton As PSMButtonState
                ReadOnly Property m_MoveButton As PSMButtonState
                ReadOnly Property m_TriggerButton As PSMButtonState
                ReadOnly Property m_BatteryValue As PSMBatteryState
                ReadOnly Property m_TriggerValue As Byte
                ReadOnly Property m_Rumble As Byte
                ReadOnly Property m_LED_r As Byte
                ReadOnly Property m_LED_g As Byte
                ReadOnly Property m_LED_b As Byte

                ReadOnly Property m_ResetPoseButtonPressTime As ULong
                ReadOnly Property m_ResetPoseRequestSent As Boolean
                ReadOnly Property m_PoseResetButtonEnabled As Boolean

                ReadOnly Property m_HasLEDOverwriteColor As Boolean
                    Get
                        Return (m_LED_r <> 0 OrElse m_LED_g <> 0 OrElse m_LED_b <> 0)
                    End Get
                End Property
            End Class

            Class PSDualShock4State
                Implements IPSState

            End Class

            Class PSNaviState
                Implements IPSState

            End Class

            Class PSVirtualState
                Implements IPSState

            End Class

            Class PSPose
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPosef)
                    m_Position = m_Position.FromPinvoke(_FromPinvoke.Position)
                    m_Orientation = m_Orientation.FromPinvoke(_FromPinvoke.Orientation)
                End Sub

                ReadOnly Property m_Position As PSMVector3f
                ReadOnly Property m_Orientation As PSMQuatf
            End Class

            Class PSMoveRawSensor
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSMoveRawSensorData)
                    m_LinearVelocityCmPerSec = m_LinearVelocityCmPerSec.FromPinvoke(_FromPinvoke.LinearVelocityCmPerSec)
                    m_LinearAccelerationCmPerSecSqr = m_LinearAccelerationCmPerSecSqr.FromPinvoke(_FromPinvoke.LinearAccelerationCmPerSecSqr)
                    m_AngularVelocityRadPerSec = m_AngularVelocityRadPerSec.FromPinvoke(_FromPinvoke.AngularVelocityRadPerSec)
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                ReadOnly Property m_LinearVelocityCmPerSec As PSMVector3i
                ReadOnly Property m_LinearAccelerationCmPerSecSqr As PSMVector3i
                ReadOnly Property m_AngularVelocityRadPerSec As PSMVector3i
                ReadOnly Property m_TimeInSeconds As Double
            End Class

            Class PSMoveCalibratedSensor
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)
                    m_Magnetometer = m_Magnetometer.FromPinvoke(_FromPinvoke.Magnetometer)
                    m_Accelerometer = m_Accelerometer.FromPinvoke(_FromPinvoke.Accelerometer)
                    m_Gyroscope = m_Gyroscope.FromPinvoke(_FromPinvoke.Gyroscope)
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                ReadOnly Property m_Magnetometer As PSMVector3f
                ReadOnly Property m_Accelerometer As PSMVector3f
                ReadOnly Property m_Gyroscope As PSMVector3f
                ReadOnly Property m_TimeInSeconds As Double
            End Class

            Class PSPhysics
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPhysicsData)
                    m_LinearVelocityCmPerSec = m_LinearVelocityCmPerSec.FromPinvoke(_FromPinvoke.LinearVelocityCmPerSec)
                    m_LinearAccelerationCmPerSecSqr = m_LinearAccelerationCmPerSecSqr.FromPinvoke(_FromPinvoke.LinearAccelerationCmPerSecSqr)
                    m_AngularVelocityRadPerSec = m_AngularVelocityRadPerSec.FromPinvoke(_FromPinvoke.AngularVelocityRadPerSec)
                    m_AngularAccelerationRadPerSecSqr = m_AngularAccelerationRadPerSecSqr.FromPinvoke(_FromPinvoke.AngularAccelerationRadPerSecSqr)
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                ReadOnly Property m_LinearVelocityCmPerSec As PSMVector3f
                ReadOnly Property m_LinearAccelerationCmPerSecSqr As PSMVector3f
                ReadOnly Property m_AngularVelocityRadPerSec As PSMVector3f
                ReadOnly Property m_AngularAccelerationRadPerSecSqr As PSMVector3f
                ReadOnly Property m_TimeInSeconds As Double
            End Class

            Class PSTracking
                Private g_mTrackingProjection As Object
                Private g_iShape As Constants.PSMShape = PSMShape.PSMShape_INVALID_PROJECTION

                Sub New(_FromPinvoke As Object)
                    Select Case (True)
                        Case (TypeOf _FromPinvoke Is PInvoke.PINVOKE_PSMRawTrackerDataEllipse)
                            Dim _FromPinvokeEllipse = DirectCast(_FromPinvoke, PInvoke.PINVOKE_PSMRawTrackerDataEllipse)

                            g_iShape = PSMShape.PSMShape_Ellipse

                            m_TrackerID = _FromPinvokeEllipse.TrackerID
                            m_ScreenLocation = m_ScreenLocation.FromPinvoke(_FromPinvokeEllipse.ScreenLocation)
                            m_RelativePositionCm = m_RelativePositionCm.FromPinvoke(_FromPinvokeEllipse.RelativePositionCm)
                            m_RelativeOrientation = m_RelativeOrientation.FromPinvoke(_FromPinvokeEllipse.RelativeOrientation)
                            m_ValidTrackerBitmask = _FromPinvokeEllipse.ValidTrackerBitmask
                            m_MulticamPositionCm = m_MulticamPositionCm.FromPinvoke(_FromPinvokeEllipse.MulticamPositionCm)
                            m_MulticamOrientation = m_MulticamOrientation.FromPinvoke(_FromPinvokeEllipse.MulticamOrientation)
                            m_bMulticamPositionValid = CBool(_FromPinvokeEllipse.bMulticamPositionValid)
                            m_bMulticamOrientationValid = CBool(_FromPinvokeEllipse.bMulticamOrientationValid)

                            Dim i As New PSMTrackingProjectionEllipse()
                            i.mCenter = i.mCenter.FromPinvoke(_FromPinvokeEllipse.TrackingProjection.center)
                            i.fHalf_X_Extent = i.fHalf_X_Extent
                            i.fHalf_Y_Extent = i.fHalf_Y_Extent
                            i.fAngle = i.fAngle
                            g_mTrackingProjection = i

                        Case (TypeOf _FromPinvoke Is PInvoke.PINVOKE_PSMRawTrackerDataLightbar)
                            Dim _FromPinvokeLightbar = DirectCast(_FromPinvoke, PInvoke.PINVOKE_PSMRawTrackerDataLightbar)

                            g_iShape = PSMShape.PSMShape_LightBar

                            m_TrackerID = _FromPinvokeLightbar.TrackerID
                            m_ScreenLocation = m_ScreenLocation.FromPinvoke(_FromPinvokeLightbar.ScreenLocation)
                            m_RelativePositionCm = m_RelativePositionCm.FromPinvoke(_FromPinvokeLightbar.RelativePositionCm)
                            m_RelativeOrientation = m_RelativeOrientation.FromPinvoke(_FromPinvokeLightbar.RelativeOrientation)
                            m_ValidTrackerBitmask = _FromPinvokeLightbar.ValidTrackerBitmask
                            m_MulticamPositionCm = m_MulticamPositionCm.FromPinvoke(_FromPinvokeLightbar.MulticamPositionCm)
                            m_MulticamOrientation = m_MulticamOrientation.FromPinvoke(_FromPinvokeLightbar.MulticamOrientation)
                            m_bMulticamPositionValid = CBool(_FromPinvokeLightbar.bMulticamPositionValid)
                            m_bMulticamOrientationValid = CBool(_FromPinvokeLightbar.bMulticamOrientationValid)

                            Dim i As New PSMTrackingProjectionLightbar()
                            For j = 0 To _FromPinvokeLightbar.TrackingProjection.triangle.Length - 1
                                i.mTriangle(j) = i.mTriangle(j).FromPinvoke(_FromPinvokeLightbar.TrackingProjection.triangle(j))
                            Next
                            For j = 0 To _FromPinvokeLightbar.TrackingProjection.quad.Length - 1
                                i.mQuad(j) = i.mQuad(j).FromPinvoke(_FromPinvokeLightbar.TrackingProjection.quad(j))
                            Next
                            g_mTrackingProjection = i

                        Case (TypeOf _FromPinvoke Is PInvoke.PINVOKE_PSMRawTrackerDataPointcloud)
                            Dim _FromPinvokePointcloud = DirectCast(_FromPinvoke, PInvoke.PINVOKE_PSMRawTrackerDataPointcloud)

                            g_iShape = PSMShape.PSMShape_PointCloud

                            m_TrackerID = _FromPinvokePointcloud.TrackerID
                            m_ScreenLocation = m_ScreenLocation.FromPinvoke(_FromPinvokePointcloud.ScreenLocation)
                            m_RelativePositionCm = m_RelativePositionCm.FromPinvoke(_FromPinvokePointcloud.RelativePositionCm)
                            m_RelativeOrientation = m_RelativeOrientation.FromPinvoke(_FromPinvokePointcloud.RelativeOrientation)
                            m_ValidTrackerBitmask = _FromPinvokePointcloud.ValidTrackerBitmask
                            m_MulticamPositionCm = m_MulticamPositionCm.FromPinvoke(_FromPinvokePointcloud.MulticamPositionCm)
                            m_MulticamOrientation = m_MulticamOrientation.FromPinvoke(_FromPinvokePointcloud.MulticamOrientation)
                            m_bMulticamPositionValid = CBool(_FromPinvokePointcloud.bMulticamPositionValid)
                            m_bMulticamOrientationValid = CBool(_FromPinvokePointcloud.bMulticamOrientationValid)

                            Dim i As New PSMTrackingProjectionPointcloud()
                            For j = 0 To _FromPinvokePointcloud.TrackingProjection.points.Length - 1
                                i.mPoints(j) = i.mPoints(j).FromPinvoke(_FromPinvokePointcloud.TrackingProjection.points(j))
                            Next
                            i.iPointCount = _FromPinvokePointcloud.TrackingProjection.point_count
                            g_mTrackingProjection = i

                        Case Else
                            g_iShape = PSMShape.PSMShape_INVALID_PROJECTION
                    End Select

                End Sub

                Public Interface IPSMTrackingProjectiomShape
                End Interface

                Public Class PSMTrackingProjectionEllipse
                    Implements IPSMTrackingProjectiomShape

                    Public mCenter As PSMVector2f
                    Public fHalf_X_Extent As Single
                    Public fHalf_Y_Extent As Single
                    Public fAngle As Single
                End Class

                Public Class PSMTrackingProjectionLightbar
                    Implements IPSMTrackingProjectiomShape

                    Public mTriangle(3) As PSMVector2f
                    Public mQuad(3) As PSMVector2f
                End Class

                Public Class PSMTrackingProjectionPointcloud
                    Implements IPSMTrackingProjectiomShape

                    Public mPoints(7) As PSMVector2f
                    Public iPointCount As Integer
                End Class

                ReadOnly Property m_TrackerID As Integer
                ReadOnly Property m_ScreenLocation As PSMVector2f
                ReadOnly Property m_RelativePositionCm As PSMVector3f
                ReadOnly Property m_RelativeOrientation As PSMQuatf
                ReadOnly Property m_ValidTrackerBitmask As UInteger
                ReadOnly Property m_MulticamPositionCm As PSMVector3f
                ReadOnly Property m_MulticamOrientation As PSMQuatf
                ReadOnly Property m_bMulticamPositionValid As Boolean
                ReadOnly Property m_bMulticamOrientationValid As Boolean

                Public Function GetTrackingProjection(Of IPSMTrackingProjectiomShape)() As IPSMTrackingProjectiomShape
                    Return DirectCast(g_mTrackingProjection, IPSMTrackingProjectiomShape)
                End Function

                ReadOnly Property m_Shape() As PSMShape
                    Get
                        Return g_iShape
                    End Get
                End Property
            End Class

            Public Function GetPSState(Of IPSState)() As IPSState
                Return DirectCast(g_PSState, IPSState)
            End Function

            Public Function IsPoseValid() As Boolean
                Return (m_Pose IsNot Nothing)
            End Function

            Public Function IsPhysicsValid() As Boolean
                Return (m_Physics IsNot Nothing)
            End Function

            Public Function IsSensorValid() As Boolean
                Select Case (m_ControllerType)
                    Case PSMControllerType.PSMController_Move
                        Return (g_PSMoveRawSensor IsNot Nothing AndAlso g_PSMoveCalibratedSensor IsNot Nothing)
                    Case PSMControllerType.PSMController_DualShock4
                        ' ###TODO
                    Case PSMControllerType.PSMController_Navi
                        ' ###TODO
                    Case PSMControllerType.PSMController_Virtual
                        ' ###TODO
                End Select

                Return False
            End Function

            Public Function IsStateValid() As Boolean
                Return (g_PSState IsNot Nothing)
            End Function

            Public Function IsTrackingValid() As Boolean
                Return (g_PSTracking IsNot Nothing)
            End Function

            Public Sub Refresh(iRefreshType As RefreshFlags)
                If ((iRefreshType And RefreshFlags.RefreshType_ProbeSerial) > 0) Then
                    Dim iSize = Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMControllerList))
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(iSize)
                    Try
                        If (CType(PInvoke.PSM_GetControllerList(hPtr, Constants.PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                            Dim mControllerList = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMControllerList)(hPtr)

                            For i = 0 To mControllerList.count - 1
                                If (mControllerList.controller_id(i) <> m_ControllerId) Then
                                    Continue For
                                End If

                                g_sControllerSerial = mControllerList.controller_serial(i).get
                                g_sParentControllerSerial = mControllerList.parent_controller_serial(i).get
                                Exit For
                            Next
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Basic) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMController)))
                    Try
                        If (PInvoke.PSM_GetControllerEx(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMController)(hPtr)

                            g_iControllerType = mData.ControllerType
                            g_iOutputSequenceNum = mData.OutputSequenceNum
                            g_iInputSequenceNum = mData.InputSequenceNum
                            g_bIsConnected = CBool(mData.IsConnected)
                            g_iDataFrameLastReceivedTime = mData.DataFrameLastReceivedTime
                            g_iDataFrameAverageFPS = mData.DataFrameAverageFPS
                            g_iListenerCount = mData.ListenerCount
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_State) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMove)))
                    Try
                        Select Case (g_iControllerType)
                            Case PSMControllerType.PSMController_Move
                                If (PInvoke.PSM_GetControllerPSMoveStateEx(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMove)(hPtr)

                                    g_PSState = New PSMoveState(mData)
                                End If

                            Case PSMControllerType.PSMController_DualShock4
                                ' ###TODO
                            Case PSMControllerType.PSMController_Navi
                                ' ###TODO
                            Case PSMControllerType.PSMController_Virtual
                                ' ###TODO

                        End Select
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Pose) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPosef)))
                    Try
                        If (PInvoke.PSM_GetControllerPose(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPosef)(hPtr)


                            g_Pose = New PSPose(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Physics) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPhysicsData)))
                    Try
                        If (PInvoke.PSM_GetControllerPhysicsData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPhysicsData)(hPtr)

                            g_Physics = New PSPhysics(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Sensor) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMoveRawSensorData)))
                    Try
                        If (PInvoke.PSM_GetControllerPSMoveRawSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMoveRawSensorData)(hPtr)

                            g_PSMoveRawSensor = New PSMoveRawSensor(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Sensor) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)))
                    Try
                        If (PInvoke.PSM_GetControllerPSMoveSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)(hPtr)

                            g_PSMoveCalibratedSensor = New PSMoveCalibratedSensor(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Tracker) > 0) Then
                    Dim iShape As Integer = PSMShape.PSMShape_INVALID_PROJECTION
                    If (PInvoke.PSM_GetControllerRawTrackerShape(m_ControllerId, iShape) = PSMResult.PSMResult_Success) Then
                        Select Case (iShape)
                            Case PSMShape.PSMShape_Ellipse
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataEllipse)))
                                Try
                                    If (PInvoke.PSM_GetControllerRawTrackerDataEllipse(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                        Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMRawTrackerDataEllipse)(hPtr)

                                        g_PSTracking = New PSTracking(mData)
                                    End If

                                Finally
                                    Marshal.FreeHGlobal(hPtr)
                                End Try

                            Case PSMShape.PSMShape_LightBar
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataLightbar)))
                                Try
                                    If (PInvoke.PSM_GetControllerRawTrackerDataLightbar(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                        Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMRawTrackerDataLightbar)(hPtr)

                                        g_PSTracking = New PSTracking(mData)
                                    End If

                                Finally
                                    Marshal.FreeHGlobal(hPtr)
                                End Try

                            Case PSMShape.PSMShape_PointCloud
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataPointcloud)))
                                Try
                                    If (PInvoke.PSM_GetControllerRawTrackerDataPointcloud(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                        Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMRawTrackerDataPointcloud)(hPtr)

                                        g_PSTracking = New PSTracking(mData)
                                    End If

                                Finally
                                    Marshal.FreeHGlobal(hPtr)
                                End Try

                        End Select

                    End If

                End If

            End Sub
        End Class

        Property m_Listening As Boolean
            Get
                Return g_bListening
            End Get
            Set(value As Boolean)
                If (g_bListening = value) Then
                    Return
                End If

                g_bListening = value

                If (g_bListening) Then
                    If (PInvoke.PSM_AllocateControllerListener(g_mInfo.m_ControllerId) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_AllocateControllerListener failed")
                    End If
                Else
                    If (PInvoke.PSM_FreeControllerListener(g_mInfo.m_ControllerId) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_FreeControllerListener failed")
                    End If
                End If
            End Set
        End Property

        Property m_DataStreamFlags As PSMStreamFlags
            Get
                Return g_iDataStreamFlags
            End Get
            Set(value As PSMStreamFlags)
                If (g_iDataStreamFlags = value) Then
                    Return
                End If

                g_iDataStreamFlags = value

                'Refresh data streams with new flags
                If (m_DataStreamEnabled) Then
                    m_DataStreamEnabled = False
                    m_DataStreamEnabled = True
                End If
            End Set
        End Property

        Property m_DataStreamEnabled As Boolean
            Get
                Return g_bDataStream
            End Get
            Set(value As Boolean)
                If (g_bDataStream = value) Then
                    Return
                End If

                g_bDataStream = value

                If (g_bDataStream) Then
                    If (PInvoke.PSM_StartControllerDataStream(g_mInfo.m_ControllerId, CUInt(m_DataStreamFlags), PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_AllocateControllerListener failed")
                    End If
                Else
                    If (PInvoke.PSM_StopControllerDataStream(g_mInfo.m_ControllerId, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_FreeControllerListener failed")
                    End If
                End If
            End Set
        End Property

        Public Sub SetTrackerStream(iTrackerID As Integer)
            If (PInvoke.PSM_SetControllerDataStreamTrackerIndex(m_Info.m_ControllerId, iTrackerID, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_SetControllerDataStreamTrackerIndex failed")
            End If

            ' Start streams then if not already
            m_Listening = True
            m_DataStreamEnabled = True
            m_DataStreamFlags = (m_DataStreamFlags Or PSMStreamFlags.PSMStreamFlags_includeRawTrackerData)
        End Sub

        Public Function IsTrackerStreamingThisController(iTrackerID As Integer) As Boolean
            If (Not m_Info.IsTrackingValid) Then
                Return False
            End If

            If (CInt(m_Info.m_PSTracking.m_ValidTrackerBitmask And (1 << iTrackerID)) = 0) Then
                Return False
            End If

            Return (m_Info.m_PSTracking.m_TrackerID = iTrackerID)
        End Function

        Public Sub SetControllerLEDTrackingColor(iColor As PSMTrackingColorType)
            If (PInvoke.PSM_SetControllerLEDTrackingColor(m_Info.m_ControllerId, iColor, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_SetControllerLEDTrackingColor failed")
            End If
        End Sub

        Public Sub SetControllerLEDOverrideColor(r As Byte, g As Byte, b As Byte)
            SetControllerLEDOverrideColor(Color.FromArgb(r, g, b))
        End Sub

        Public Sub SetControllerLEDOverrideColor(mColor As Color)
            If (PInvoke.PSM_SetControllerLEDOverrideColor(m_Info.m_ControllerId, mColor.R, mColor.G, mColor.B) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_SetControllerLEDTrackingColor failed")
            End If
        End Sub

        Public Sub ResetControlerOrientation(mOrientation As PSMQuatf)
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMQuatf)))
            Marshal.StructureToPtr(mOrientation.ToPinvoke(), hPtr, True)
            Try
                If (PInvoke.PSM_ResetControllerOrientation(m_Info.m_ControllerId, hPtr, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                    Throw New ArgumentException("PSM_ResetControllerOrientation failed")
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try
        End Sub

        Public Function GetControllerRumble(iChannel As PSMControllerRumbleChannel) As Single
            Dim fRumbleOut As Single = -1.0
            PInvoke.PSM_GetControllerRumble(m_Info.m_ControllerId, iChannel, fRumbleOut)
            Return fRumbleOut
        End Function

        Public Sub SetControllerRumble(iChannel As PSMControllerRumbleChannel, fRumble As Single)
            If (PInvoke.PSM_SetControllerRumble(m_Info.m_ControllerId, iChannel, fRumble) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_SetControllerRumble failed")
            End If
        End Sub

        Public Function IsControllerStable() As Boolean
            Dim bStable As Byte = 0
            PInvoke.PSM_GetIsControllerStable(m_Info.m_ControllerId, bStable)
            Return CBool(bStable)
        End Function

        Public Function IsControllerTracking() As Boolean
            Dim bTracked As Byte = 0
            PInvoke.PSM_GetIsControllerTracking(m_Info.m_ControllerId, bTracked)
            Return CBool(bTracked)
        End Function

        Public Function GetControllerPosition() As PSMVector3f
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMVector3f)))
            Try
                If (PInvoke.PSM_GetControllerPosition(m_Info.m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMVector3f)(hPtr)

                    Return (New PSMVector3f()).FromPinvoke(mData)
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return New PSMVector3f()
        End Function

        Public Function GetControllerOrientation() As PSMQuatf
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMQuatf)))
            Try
                If (PInvoke.PSM_GetControllerOrientation(m_Info.m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMQuatf)(hPtr)

                    Return (New PSMQuatf()).FromPinvoke(mData)
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return New PSMQuatf()
        End Function

        Public Shared Function GetControllerList() As Controllers()
            Dim mControllers As New List(Of Controllers)

            Dim iSize = Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMControllerList))
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(iSize)
            Try
                If (CType(PInvoke.PSM_GetControllerList(hPtr, Constants.PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                    Dim mControllerList = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMControllerList)(hPtr)

                    For i = 0 To mControllerList.count - 1
                        mControllers.Add(New Controllers(mControllerList.controller_id(i)))
                    Next
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return mControllers.ToArray
        End Function

        Public Shared Function GetValidControllerCount() As Integer
            Dim iCount As Integer = 0

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMController)))
            Try
                For i = 0 To PSMOVESERVICE_MAX_CONTROLLER_COUNT - 1
                    If (PInvoke.PSM_GetControllerEx(i, hPtr) = PSMResult.PSMResult_Success) Then
                        Dim controller = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMController)(hPtr)
                        If (CBool(controller.bValid)) Then
                            iCount += 1
                        End If
                    End If
                Next
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return iCount
        End Function

        Public Shared Function GetConnectedControllerCount() As Integer
            Dim iCount As Integer = 0

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMController)))
            Try
                For i = 0 To PSMOVESERVICE_MAX_CONTROLLER_COUNT - 1
                    If (PInvoke.PSM_GetControllerEx(i, hPtr) = PSMResult.PSMResult_Success) Then
                        Dim controller = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMController)(hPtr)
                        If (CBool(controller.bValid) AndAlso CBool(controller.IsConnected)) Then
                            iCount += 1
                        End If
                    End If
                Next
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return iCount
        End Function

        Public Sub Disconnect()
            m_DataStreamEnabled = False
            m_Listening = False
            m_DataStreamFlags = PSMStreamFlags.PSMStreamFlags_defaultStreamOptions
        End Sub

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

    Class HeadMountDevices

    End Class

    Class Trackers

    End Class

    Public Class Constants
        Public Const PSM_DEFAULT_TIMEOUT = 1000

        Public Const PSMOVESERVICE_CONTROLLER_SERIAL_LEN = 18
        Public Const PSMOVESERVICE_MAX_VERSION_STRING_LEN = 32

        Public Const PSM_METERS_TO_CENTIMETERS = 100.0
        Public Const PSM_CENTIMETERS_TO_METERS = 0.01


        Public Const PSMOVESERVICE_DEFAULT_ADDRESS = "127.0.0.1" '"localhost"
        Public Const PSMOVESERVICE_DEFAULT_PORT = "9512"

        Public Const MAX_OUTPUT_DATA_FRAME_MESSAGE_SIZE = 500
        Public Const MAX_INPUT_DATA_FRAME_MESSAGE_SIZE = 64

        Public Const PSMOVESERVICE_MAX_CONTROLLER_COUNT = 10
        Public Const PSMOVESERVICE_MAX_TRACKER_COUNT = 8
        Public Const PSMOVESERVICE_MAX_HMD_COUNT = 4

        Public Const PSM_MAX_VIRTUAL_CONTROLLER_AXES = 32
        Public Const PSM_MAX_VIRTUAL_CONTROLLER_BUTTONS = 32



        Enum PSMShape
            PSMShape_INVALID_PROJECTION = -1
            PSMShape_Ellipse
            PSMShape_LightBar
            PSMShape_PointCloud
        End Enum

        Enum PSMResult
            PSMResult_Error = -1
            PSMResult_Success = 0
            PSMResult_Timeout = 1
            PSMResult_RequestSent = 2
            PSMResult_Canceled = 3
            PSMResult_NoData = 4
        End Enum

        Enum PSMConnectionType
            PSMConnectionType_BLUETOOTH
            PSMConnectionType_USB
        End Enum

        Enum PSMButtonState
            PSMButtonState_UP = &H0
            PSMButtonState_PRESSED = &H1
            PSMButtonState_DOWN = &H3
            PSMButtonState_RELEASED = &H2
        End Enum

        Enum PSMTrackingColorType
            PSMTrackingColorType_Magenta
            PSMTrackingColorType_Cyan
            PSMTrackingColorType_Yellow
            PSMTrackingColorType_Red
            PSMTrackingColorType_Green
            PSMTrackingColorType_Blue

            PSMTrackingColorType_Custom0
            PSMTrackingColorType_Custom1
            PSMTrackingColorType_Custom2
            PSMTrackingColorType_Custom3
            PSMTrackingColorType_Custom4
            PSMTrackingColorType_Custom5
            PSMTrackingColorType_Custom6
            PSMTrackingColorType_Custom7
            PSMTrackingColorType_Custom8
            PSMTrackingColorType_Custom9

            PSMTrackingColorType_MaxColorTypes
        End Enum

        Enum PSMBatteryState
            PSMBattery_0 = 0
            PSMBattery_20 = 1
            PSMBattery_40 = 2
            PSMBattery_60 = 3
            PSMBattery_80 = 4
            PSMBattery_100 = 5
            PSMBattery_Charging = &HEE
            PSMBattery_Charged = &HEF
        End Enum

        Enum PSMStreamFlags
            PSMStreamFlags_defaultStreamOptions = &H0
            PSMStreamFlags_includePositionData = &H1
            PSMStreamFlags_includePhysicsData = &H2
            PSMStreamFlags_includeRawSensorData = &H4
            PSMStreamFlags_includeCalibratedSensorData = &H8
            PSMStreamFlags_includeRawTrackerData = &H10
            PSMStreamFlags_disableROI = &H20
        End Enum

        Enum PSMControllerRumbleChannel
            PSMControllerRumbleChannel_All
            PSMControllerRumbleChannel_Left
            PSMControllerRumbleChannel_Right
        End Enum

        Enum PSMControllerType
            PSMController_None = -1
            PSMController_Move
            PSMController_Navi
            PSMController_DualShock4
            PSMController_Virtual
        End Enum

        Enum PSMControllerHand
            PSMControllerHand_Any = 0
            PSMControllerHand_Left = 1
            PSMControllerHand_Right = 2
        End Enum

        Enum PSMTrackerType
            PSMTracker_None = -1
            PSMTracker_PS3Eye
        End Enum

        Enum PSMHmdType
            PSMHmd_None = -1
            PSMHmd_Morpheus = 0
            PSMHmd_Virtual = 1
        End Enum

        Enum PSMTrackerDriver
            PSMDriver_LIBUSB
            PSMDriver_CL_EYE
            PSMDriver_CL_EYE_MULTICAM
            PSMDriver_GENERIC_WEBCAM
        End Enum

        Public Structure PSMVector2f
            Public x As Single
            Public y As Single

            Public Function ToPinvoke() As PInvoke.PINVOKE_PSMVector2f
                Dim i As New PInvoke.PINVOKE_PSMVector2f()
                i.x = x
                i.y = y
                Return i
            End Function

            Public Function FromPinvoke(i As PInvoke.PINVOKE_PSMVector2f) As PSMVector2f
                Dim j As New PSMVector2f()
                j.x = i.x
                j.y = i.y
                Return j
            End Function
        End Structure

        Public Structure PSMVector3f
            Public x As Single
            Public y As Single
            Public z As Single

            Public Function ToPinvoke() As PInvoke.PINVOKE_PSMVector3f
                Dim i As New PInvoke.PINVOKE_PSMVector3f()
                i.x = x
                i.y = y
                i.z = z
                Return i
            End Function

            Public Function FromPinvoke(i As PInvoke.PINVOKE_PSMVector3f) As PSMVector3f
                Dim j As New PSMVector3f()
                j.x = i.x
                j.y = i.y
                j.z = i.z
                Return j
            End Function
        End Structure

        Public Structure PSMVector3i
            Public x As Integer
            Public y As Integer
            Public z As Integer

            Public Function ToPinvoke() As PInvoke.PINVOKE_PSMVector3i
                Dim i As New PInvoke.PINVOKE_PSMVector3i()
                i.x = x
                i.y = y
                i.z = z
                Return i
            End Function

            Public Function FromPinvoke(i As PInvoke.PINVOKE_PSMVector3i) As PSMVector3i
                Dim j As New PSMVector3i()
                j.x = i.x
                j.y = i.y
                j.z = i.z
                Return j
            End Function
        End Structure

        Public Structure PSMQuatf
            Public w As Single
            Public x As Single
            Public y As Single
            Public z As Single

            Public Function ToPinvoke() As PInvoke.PINVOKE_PSMQuatf
                Dim i As New PInvoke.PINVOKE_PSMQuatf()
                i.w = w
                i.x = x
                i.y = y
                i.z = z
                Return i
            End Function

            Public Function FromPinvoke(i As PInvoke.PINVOKE_PSMQuatf) As PSMQuatf
                Dim j As New PSMQuatf()
                j.w = i.w
                j.x = i.x
                j.y = i.y
                j.z = i.z
                Return j
            End Function
        End Structure

        Public Structure PSMPosef
            Public Position As PSMVector3f
            Public Orientation As PSMQuatf

            Public Function ToPinvoke() As PInvoke.PINVOKE_PSMPosef
                Dim i As New PInvoke.PINVOKE_PSMPosef()
                i.Position.x = Position.x
                i.Position.y = Position.y
                i.Position.z = Position.z
                i.Orientation.w = Orientation.w
                i.Orientation.x = Orientation.x
                i.Orientation.y = Orientation.y
                i.Orientation.z = Orientation.z
                Return i
            End Function

            Public Function FromPinvoke(i As PInvoke.PINVOKE_PSMPosef) As PSMPosef
                Dim a As New PSMVector3f
                Dim b As New PSMQuatf
                a.x = i.Position.x
                a.y = i.Position.y
                a.z = i.Position.z
                b.w = i.Orientation.w
                b.x = i.Orientation.x
                b.y = i.Orientation.y
                b.z = i.Orientation.z

                Dim j As New PSMPosef()
                j.Position = a
                j.Orientation = b
                Return j
            End Function
        End Structure
    End Class

    Public Class PInvoke
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector2f
            Public x As Single
            Public y As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector3f
            Public x As Single
            Public y As Single
            Public z As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector3i
            Public x As Integer
            Public y As Integer
            Public z As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMQuatf
            Public w As Single
            Public x As Single
            Public y As Single
            Public z As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPosef
            Public Position As PINVOKE_PSMVector3f
            Public Orientation As PINVOKE_PSMQuatf
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPhysicsData
            Public LinearVelocityCmPerSec As PINVOKE_PSMVector3f
            Public LinearAccelerationCmPerSecSqr As PINVOKE_PSMVector3f
            Public AngularVelocityRadPerSec As PINVOKE_PSMVector3f
            Public AngularAccelerationRadPerSecSqr As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMoveRawSensorData
            Public LinearVelocityCmPerSec As PINVOKE_PSMVector3i
            Public LinearAccelerationCmPerSecSqr As PINVOKE_PSMVector3i
            Public AngularVelocityRadPerSec As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMoveCalibratedSensorData
            Public Magnetometer As PINVOKE_PSMVector3f
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionEllipse
            Public center As PINVOKE_PSMVector2f
            Public half_x_extent As Single
            Public half_y_extent As Single
            Public angle As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionLightbar
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
            Public triangle As PINVOKE_PSMVector2f()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
            Public quad As PINVOKE_PSMVector2f()
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionPointcloud
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=7)>
            Public points As PINVOKE_PSMVector2f()
            Public point_count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMRawTrackerData
            Public TrackerID As Integer
            Public ScreenLocation As PINVOKE_PSMVector2f
            Public RelativePositionCm As PINVOKE_PSMVector3f
            Public RelativeOrientation As PINVOKE_PSMQuatf
            Public ValidTrackerBitmask As UInteger
            Public MulticamPositionCm As PINVOKE_PSMVector3f
            Public MulticamOrientation As PINVOKE_PSMQuatf
            Public bMulticamPositionValid As Byte
            Public bMulticamOrientationValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMRawTrackerDataEllipse
            Public TrackerID As Integer
            Public ScreenLocation As PINVOKE_PSMVector2f
            Public RelativePositionCm As PINVOKE_PSMVector3f
            Public RelativeOrientation As PINVOKE_PSMQuatf
            Public TrackingProjection As PINVOKE_PSMTrackingProjectionEllipse
            Public ValidTrackerBitmask As UInteger
            Public MulticamPositionCm As PINVOKE_PSMVector3f
            Public MulticamOrientation As PINVOKE_PSMQuatf
            Public bMulticamPositionValid As Byte
            Public bMulticamOrientationValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMRawTrackerDataLightbar
            Public TrackerID As Integer
            Public ScreenLocation As PINVOKE_PSMVector2f
            Public RelativePositionCm As PINVOKE_PSMVector3f
            Public RelativeOrientation As PINVOKE_PSMQuatf
            Public TrackingProjection As PINVOKE_PSMTrackingProjectionLightbar
            Public ValidTrackerBitmask As UInteger
            Public MulticamPositionCm As PINVOKE_PSMVector3f
            Public MulticamOrientation As PINVOKE_PSMQuatf
            Public bMulticamPositionValid As Byte
            Public bMulticamOrientationValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMRawTrackerDataPointcloud
            Public TrackerID As Integer
            Public ScreenLocation As PINVOKE_PSMVector2f
            Public RelativePositionCm As PINVOKE_PSMVector3f
            Public RelativeOrientation As PINVOKE_PSMQuatf
            Public TrackingProjection As PINVOKE_PSMTrackingProjectionPointcloud
            Public ValidTrackerBitmask As UInteger
            Public MulticamPositionCm As PINVOKE_PSMVector3f
            Public MulticamOrientation As PINVOKE_PSMQuatf
            Public bMulticamPositionValid As Byte
            Public bMulticamOrientationValid As Byte
        End Structure


        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMove
            Public bHasValidHardwareCalibration As Byte
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
            Public bHasUnpublishedState As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)>
            Public DevicePath As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public DeviceSerial As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public AssignedHostSerial As String

            Public PairedToHost As Byte
            Public ConnectionType As Constants.PSMConnectionType
            Public TrackingColorType As Constants.PSMTrackingColorType

            Public TriangleButton As Integer
            Public CircleButton As Integer
            Public CrossButton As Integer
            Public SquareButton As Integer
            Public SelectButton As Integer
            Public StartButton As Integer
            Public PSButton As Integer
            Public MoveButton As Integer
            Public TriggerButton As Integer
            Public BatteryValue As Integer
            Public TriggerValue As Byte
            Public Rumble As Byte
            Public LED_r As Byte
            Public LED_g As Byte
            Public LED_b As Byte

            Public ResetPoseButtonPressTime As ULong
            Public bResetPoseRequestSent As Byte
            Public bPoseResetButtonEnabled As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSNavi
            Public L1Button As Constants.PSMButtonState
            Public L2Button As Constants.PSMButtonState
            Public L3Button As Constants.PSMButtonState
            Public CircleButton As Constants.PSMButtonState
            Public CrossButton As Constants.PSMButtonState
            Public PSButton As Constants.PSMButtonState
            Public TriggerButton As Constants.PSMButtonState
            Public DPadUpButton As Constants.PSMButtonState
            Public DPadRightButton As Constants.PSMButtonState
            Public DPadDownButton As Constants.PSMButtonState
            Public DPadLeftButton As Constants.PSMButtonState

            Public TriggerValue As Byte
            Public StickXAxis As Byte
            Public StickYAxis As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMDS4RawSensorData
            Public Accelerometer As PINVOKE_PSMVector3i
            Public Gyroscope As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMDS4CalibratedSensorData
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMDualShock4
            Public bHasValidHardwareCalibration As Byte
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
            Public bHasUnpublishedState As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)>
            Public DevicePath As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public DeviceSerial As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public AssignedHostSerial As String
            Public PairedToHost As Byte
            Public ConnectionType As PSMConnectionType
            Public TrackingColorType As PSMTrackingColorType


            Public DPadUpButton As PSMButtonState
            Public DPadDownButton As PSMButtonState
            Public DPadLeftButton As PSMButtonState
            Public DPadRightButton As PSMButtonState

            Public SquareButton As PSMButtonState
            Public CrossButton As PSMButtonState
            Public CircleButton As PSMButtonState
            Public TriangleButton As PSMButtonState

            Public L1Button As PSMButtonState
            Public R1Button As PSMButtonState
            Public L2Button As PSMButtonState
            Public R2Button As PSMButtonState
            Public L3Button As PSMButtonState
            Public R3Button As PSMButtonState

            Public ShareButton As PSMButtonState
            Public OptionsButton As PSMButtonState

            Public PSButton As PSMButtonState
            Public TrackPadButton As PSMButtonState


            Public LeftAnalogX As Single
            Public LeftAnalogY As Single
            Public RightAnalogX As Single
            Public RightAnalogY As Single
            Public LeftTriggerValue As Single
            Public RightTriggerValue As Single

            Public BigRumble As Byte
            Public SmallRumble As Byte
            Public LED_r As Byte
            Public LED_g As Byte
            Public LED_b As Byte

            Public ResetPoseButtonPressTime As ULong
            Public bResetPoseRequestSent As Byte
            Public bPoseResetButtonEnabled As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMVirtualController
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsPositionValid As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)>
            Public DevicePath As String

            Public vendorID As Integer
            Public productID As Integer

            Public numAxes As Integer
            Public numButtons As Integer

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSM_MAX_VIRTUAL_CONTROLLER_AXES)>
            Public axisStates As Byte()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSM_MAX_VIRTUAL_CONTROLLER_BUTTONS)>
            Public buttonStates As Integer()

            Public TrackingColorType As PSMTrackingColorType
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMController
            Public ControllerID As Integer
            Public ControllerType As PSMControllerType
            Public ControllerHand As PSMControllerHand

            Public bValid As Byte
            Public OutputSequenceNum As Integer
            Public InputSequenceNum As Integer
            Public IsConnected As Byte
            Public DataFrameLastReceivedTime As ULong
            Public DataFrameAverageFPS As Single
            Public ListenerCount As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMClientTrackerInfo
            Public tracker_id As Integer

            Public tracker_type As PSMTrackerType
            Public tracker_Driver As PSMTrackerDriver
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
            Public device_path As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=64)>
            Public shared_memory_name As String

            Public tracker_focal_lengths As PINVOKE_PSMVector2f
            Public tracker_principal_point As PINVOKE_PSMVector2f
            Public tracker_screen_dimensions As PINVOKE_PSMVector2f
            Public tracker_hfov As Single
            Public tracker_vfov As Single
            Public tracker_znear As Single
            Public tracker_zfar As Single
            Public tracker_k1 As Single
            Public tracker_k2 As Single
            Public tracker_k3 As Single
            Public tracker_p1 As Single
            Public tracker_p2 As Single

            Public tracker_pose As PINVOKE_PSMPosef
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTracker
            Public tracker_info As PINVOKE_PSMClientTrackerInfo

            Public listener_count As Integer
            Public is_connected As Byte
            Public sequence_num As Integer
            Public data_frame_last_received_time As ULong
            Public data_frame_average_fps As Single

            Public opaque_shared_memory_accesor As IntPtr
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheusRawSensorData
            Public Accelerometer As PINVOKE_PSMVector3i
            Public Gyroscope As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheusCalibratedSensorData
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheus
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMVirtualHMD
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsPositionValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMHeadMountedDisplay
            Public HmdID As Integer
            Public HmdType As Constants.PSMHmdType

            Public bValid As Byte
            Public OutputSequenceNum As Integer
            Public IsConnected As Byte
            Public DataFrameLastReceivedTime As ULong
            Public DataFrameAverageFPS As Single
            Public ListenerCount As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMServiceVersion
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=Constants.PSMOVESERVICE_MAX_VERSION_STRING_LEN)>
            Public version_string As String
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMController_Serial
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=PSMOVESERVICE_CONTROLLER_SERIAL_LEN)>
            Public [get] As String
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMControllerList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_CONTROLLER_COUNT)>
            Public controller_id As Integer()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_CONTROLLER_COUNT)>
            Public controller_type As PSMControllerType()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_CONTROLLER_COUNT)>
            Public controller_hand As PSMControllerHand()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_CONTROLLER_COUNT)>
            Public controller_serial As PINVOKE_PSMController_Serial()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_CONTROLLER_COUNT)>
            Public parent_controller_serial As PINVOKE_PSMController_Serial()

            Public count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackerList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_TRACKER_COUNT)>
            Public trackers As PINVOKE_PSMClientTrackerInfo()

            Public count As Integer
            Public global_forward_degrees As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMHmdList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_HMD_COUNT)>
            Public hmd_id As PINVOKE_PSMClientTrackerInfo()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=PSMOVESERVICE_MAX_HMD_COUNT)>
            Public hmd_type As PSMHmdType

            Public count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingSpace
            Public global_forward_degrees As Single
        End Structure

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_Initialize(<MarshalAs(UnmanagedType.LPStr)> host As String, <MarshalAs(UnmanagedType.LPStr)> port As String, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_Shutdown() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_Update() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_UpdateNoPollMessages() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetClientVersionString() As IntPtr
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetIsInitialized() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetIsConnected() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_HasConnectionStatusChanged() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_HasControllerListChanged() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_HasTrackerListChanged() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_HasHMDListChanged() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_WasSystemButtonPressed() As Boolean
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetServiceVersionString(out_version_string As System.Text.StringBuilder, max_version_string As Integer, timeout_ms As Integer) As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetServiceVersionStringAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_PollNextMessage(PSMMessage *out_message, size_t message_size);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SendOpaqueRequest(PSMRequestHandle request_handle, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_RegisterCallback(PSMRequestID request_id, PSMResponseCallback callback, void *callback_userdata);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_CancelCallback(PSMRequestID request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_EatResponse(PSMRequestID request_id);
        'PSM_PUBLIC_FUNCTION(PSMController *) PSM_GetController(PSMControllerID controller_id);

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerEx(controller_id As Integer, controller_out As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveState(controller_id As Integer, controller_out As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveStateEx(controller_id As Integer, controller_out As IntPtr) As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerPSNaviState(PSMControllerID controller_id, PSMPSNavi *controller_out);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4State(PSMControllerID controller_id, PSMDualShock4 *controller_out);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4StateEx(PSMControllerID controller_id, PSMDualShock4Ex *controller_out);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerVirtualControllerState(PSMControllerID controller_id, PSMVirtualController *controller_out);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerVirtualControllerStateEx(PSMControllerID controller_id, PSMVirtualControllerEx *controller_out);

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_AllocateControllerListener(controller_id As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_FreeControllerListener(controller_id As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerList(out_controller_list As IntPtr, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_StartControllerDataStream(controller_id As Integer, data_stream_flags As UInteger, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_StopControllerDataStream(controller_id As Integer, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_SetControllerLEDTrackingColor(controller_id As Integer, tracking_color As Integer, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_ResetControllerOrientation(controller_id As Integer, q_pose As IntPtr, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_SetControllerDataStreamTrackerIndex(controller_id As Integer, tracker_id As Integer, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_SetControllerHand(controller_id As Integer, hand As Integer, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerOrientation(controller_id As Integer, out_orientation As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPosition(controller_id As Integer, out_position As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPose(controller_id As Integer, out_pose As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPhysicsData(controller_id As Integer, out_physics As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveRawSensorData(controller_id As Integer, out_data As IntPtr) As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4RawSensorData(PSMControllerID controller_id, PSMDS4RawSensorData *out_data);

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveSensorData(controller_id As Integer, out_data As IntPtr) As Integer
        End Function


        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4SensorData(PSMControllerID controller_id, PSMDS4CalibratedSensorData *out_data);

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRumble(controller_id As Integer, channel As Integer, <Out> ByRef out_rumble_fraction As Single) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetIsControllerStable(controller_id As Integer, <Out> ByRef out_is_stable As Byte) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetIsControllerTracking(controller_id As Integer, <Out> ByRef out_is_tracking As Byte) As Integer
        End Function

        '<DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        'Public Shared Function PSM_GetControllerRawTrackerData(controller_id As Integer, out_tracker_data As IntPtr) As Integer
        'End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerShape(controller_id As Integer, <Out> ByRef out_shape_type As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataEllipse(controller_id As Integer, out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataLightbar(controller_id As Integer, out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataPointcloud(controller_id As Integer, out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_SetControllerLEDOverrideColor(controller_id As Integer, r As Byte, g As Byte, b As Byte) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        Public Shared Function PSM_SetControllerRumble(controller_id As Integer, channel As Integer, rumble_fraction As Single) As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerPixelLocationOnTracker(PSMControllerID controller_id, PSMTrackerID *out_tracker_id, PSMVector2f *out_location);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerPositionOnTracker(PSMControllerID controller_id, PSMTrackerID *out_tracker_id, PSMVector3f *outPosition);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerOrientationOnTracker(PSMControllerID controller_id, PSMTrackerID *out_tracker_id, PSMQuatf *outOrientation);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerProjectionOnTracker(PSMControllerID controller_id, PSMTrackerID *out_tracker_id, PSMTrackingProjection *out_projection); 
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerListAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StartControllerDataStreamAsync(PSMControllerID controller_id, unsigned int data_stream_flags, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StopControllerDataStreamAsync(PSMControllerID controller_id, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SetControllerLEDColorAsync(PSMControllerID controller_id, PSMTrackingColorType tracking_color, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_ResetControllerOrientationAsync(PSMControllerID controller_id, const PSMQuatf *q_pose, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SetControllerDataStreamTrackerIndexAsync(PSMControllerID controller_id, PSMTrackerID tracker_id, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SetControllerHandAsync(PSMControllerID controller_id, PSMControllerHand hand, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMTracker *) PSM_GetTracker(PSMTrackerID tracker_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_AllocateTrackerListener(PSMTrackerID tracker_id, const PSMClientTrackerInfo *tracker_info);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_FreeTrackerListener(PSMTrackerID controller_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackerIntrinsicMatrix(PSMTrackerID tracker_id, PSMMatrix3f *out_matrix);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackerList(PSMTrackerList *out_tracker_list, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StartTrackerDataStream(PSMTrackerID tracker_id, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StopTrackerDataStream(PSMTrackerID tracker_id, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackingSpaceSettings(PSMTrackingSpace *out_tracking_space, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_OpenTrackerVideoStream(PSMTrackerID tracker_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_PollTrackerVideoStream(PSMTrackerID tracker_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_CloseTrackerVideoStream(PSMTrackerID tracker_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackerVideoFrameBuffer(PSMTrackerID tracker_id, const unsigned char **out_buffer); 
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackerFrustum(PSMTrackerID tracker_id, PSMFrustum *out_frustum);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackerListAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StartTrackerDataStreamAsync(PSMTrackerID tracker_id, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StopTrackerDataStreamAsync(PSMTrackerID tracker_id, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetTrackingSpaceSettingsAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMHeadMountedDisplay *) PSM_GetHmd(PSMHmdID hmd_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdEx(PSMHmdID hmd_id, PSMHeadMountedDisplayEx *out_hmd);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdMorpheusState(PSMHmdID hmd_id, PSMMorpheus *hmd_state);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdVirtualState(PSMHmdID hmd_id, PSMVirtualHMD *hmd_state);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdPhysicsData(PSMHmdID hmd_id, PSMPhysicsData *out_physics);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdMorpheusRawSensorData(PSMHmdID hmd_id, PSMMorpheusRawSensorData *out_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdMorpheusSensorData(PSMHmdID hmd_id, PSMMorpheusCalibratedSensorData *out_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_AllocateHmdListener(PSMHmdID hmd_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_FreeHmdListener(PSMHmdID hmd_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdOrientation(PSMHmdID hmd_id, PSMQuatf *out_orientation);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdPosition(PSMHmdID hmd_id, PSMVector3f *out_position);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdPose(PSMHmdID hmd_id, PSMPosef *out_pose);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetIsHmdStable(PSMHmdID hmd_id, bool *out_is_stable);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdRawTrackerData(PSMHmdID hmd_id, PSMRawTrackerData *out_tracker_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdRawTrackerShape(PSMHmdID hmd_id, PSMTrackingProjection:eShapeType *out_shape_type);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdRawTrackerDataEllipse(PSMHmdID hmd_id, PSMRawTrackerDataEllipse *out_tracker_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdRawTrackerDataLightbar(PSMHmdID hmd_id, PSMRawTrackerDataLightbat *out_tracker_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdRawTrackerDataPointcloud(PSMHmdID hmd_id, PSMRawTrackerDataPointcloud *out_tracker_data);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetIsHmdTracking(PSMHmdID hmd_id, bool *out_is_tracking);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdPixelLocationOnTracker(PSMHmdID hmd_id, PSMTrackerID *out_tracker_id, PSMVector2f *out_location);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdPositionOnTracker(PSMHmdID hmd_id, PSMTrackerID *out_tracker_id, PSMVector3f *out_position);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdOrientationOnTracker(PSMHmdID hmd_id, PSMTrackerID *out_tracker_id, PSMQuatf *out_orientation);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdProjectionOnTracker(PSMHmdID hmd_id, PSMTrackerID *out_tracker_id, PSMTrackingProjection *out_projection);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdList(PSMHmdList *out_hmd_list, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StartHmdDataStream(PSMHmdID hmd_id, unsigned int data_stream_flags, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StopHmdDataStream(PSMHmdID hmd_id, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SetHmdDataStreamTrackerIndex(PSMHmdID hmd_id, PSMTrackerID tracker_id, int timeout_ms);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetHmdListAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StartHmdDataStreamAsync(PSMHmdID hmd_id, unsigned int data_stream_flags, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_StopHmdDataStreamAsync(PSMHmdID hmd_id, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SetHmdDataStreamTrackerIndexAsync(PSMHmdID hmd_id, PSMTrackerID tracker_id, PSMRequestID *out_request_id);

    End Class

End Class
