Imports System.Drawing
Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Partial Public Class PSMoveServiceExCAPI
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
            Private g_PSRawSensor As PSRawSensor = Nothing
            Private g_PSCalibratedSensor As PSCalibratedSensor = Nothing
            Private g_PSTracking As PSTracking = Nothing

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

            ReadOnly Property m_PSRawSensor As PSRawSensor
                Get
                    Return g_PSRawSensor
                End Get
            End Property

            ReadOnly Property m_PSCalibratedSensor As PSCalibratedSensor
                Get
                    Return g_PSCalibratedSensor
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

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMDualShock4)
                    m_bHasValidHardwareCalibration = CBool(_FromPinvoke.bHasValidHardwareCalibration)
                    m_bIsTrackingEnabled = CBool(_FromPinvoke.bIsTrackingEnabled)
                    m_bIsCurrentlyTracking = CBool(_FromPinvoke.bIsCurrentlyTracking)
                    m_bIsOrientationValid = CBool(_FromPinvoke.bIsOrientationValid)
                    m_bIsPositionValid = CBool(_FromPinvoke.bIsPositionValid)
                    m_bHasUnpublishedState = CBool(_FromPinvoke.bHasUnpublishedState)

                    m_DevicePath = _FromPinvoke.DevicePath
                    m_DeviceSerial = _FromPinvoke.DeviceSerial
                    m_AssignedHostSerial = _FromPinvoke.AssignedHostSerial
                    m_PairedToHost = CBool(_FromPinvoke.PairedToHost)
                    m_ConnectionType = _FromPinvoke.ConnectionType
                    m_TrackingColorType = _FromPinvoke.TrackingColorType


                    m_DPadUpButton = _FromPinvoke.DPadUpButton
                    m_DPadDownButton = _FromPinvoke.DPadDownButton
                    m_DPadLeftButton = _FromPinvoke.DPadLeftButton
                    m_DPadRightButton = _FromPinvoke.DPadRightButton

                    m_SquareButton = _FromPinvoke.SquareButton
                    m_CrossButton = _FromPinvoke.CrossButton
                    m_CircleButton = _FromPinvoke.CircleButton
                    m_TriangleButton = _FromPinvoke.TriangleButton

                    m_L1Button = _FromPinvoke.L1Button
                    m_R1Button = _FromPinvoke.R1Button
                    m_L2Button = _FromPinvoke.L2Button
                    m_R2Button = _FromPinvoke.R2Button
                    m_L3Button = _FromPinvoke.L3Button
                    m_R3Button = _FromPinvoke.R3Button

                    m_ShareButton = _FromPinvoke.ShareButton
                    m_OptionsButton = _FromPinvoke.OptionsButton

                    m_PSButton = _FromPinvoke.PSButton
                    m_TrackPadButton = _FromPinvoke.TrackPadButton


                    m_LeftAnalogX = _FromPinvoke.LeftAnalogX
                    m_LeftAnalogY = _FromPinvoke.LeftAnalogY
                    m_RightAnalogX = _FromPinvoke.RightAnalogX
                    m_RightAnalogY = _FromPinvoke.RightAnalogY
                    m_LeftTriggerValue = _FromPinvoke.LeftTriggerValue
                    m_RightTriggerValue = _FromPinvoke.RightTriggerValue

                    m_BigRumble = CBool(_FromPinvoke.BigRumble)
                    m_SmallRumble = CBool(_FromPinvoke.SmallRumble)
                    m_LED_r = CBool(_FromPinvoke.LED_r)
                    m_LED_g = CBool(_FromPinvoke.LED_g)
                    m_LED_b = CBool(_FromPinvoke.LED_b)

                    m_ResetPoseButtonPressTime = _FromPinvoke.ResetPoseButtonPressTime
                    m_bResetPoseRequestSent = CBool(_FromPinvoke.bResetPoseRequestSent)
                    m_bPoseResetButtonEnabled = CBool(_FromPinvoke.bPoseResetButtonEnabled)
                End Sub

                ReadOnly Property m_bHasValidHardwareCalibration As Boolean
                ReadOnly Property m_bIsTrackingEnabled As Boolean
                ReadOnly Property m_bIsCurrentlyTracking As Boolean
                ReadOnly Property m_bIsOrientationValid As Boolean
                ReadOnly Property m_bIsPositionValid As Boolean
                ReadOnly Property m_bHasUnpublishedState As Boolean

                ReadOnly Property m_DevicePath As String
                ReadOnly Property m_DeviceSerial As String
                ReadOnly Property m_AssignedHostSerial As String
                ReadOnly Property m_PairedToHost As Boolean
                ReadOnly Property m_ConnectionType As PSMConnectionType
                ReadOnly Property m_TrackingColorType As PSMTrackingColorType


                ReadOnly Property m_DPadUpButton As PSMButtonState
                ReadOnly Property m_DPadDownButton As PSMButtonState
                ReadOnly Property m_DPadLeftButton As PSMButtonState
                ReadOnly Property m_DPadRightButton As PSMButtonState

                ReadOnly Property m_SquareButton As PSMButtonState
                ReadOnly Property m_CrossButton As PSMButtonState
                ReadOnly Property m_CircleButton As PSMButtonState
                ReadOnly Property m_TriangleButton As PSMButtonState

                ReadOnly Property m_L1Button As PSMButtonState
                ReadOnly Property m_R1Button As PSMButtonState
                ReadOnly Property m_L2Button As PSMButtonState
                ReadOnly Property m_R2Button As PSMButtonState
                ReadOnly Property m_L3Button As PSMButtonState
                ReadOnly Property m_R3Button As PSMButtonState

                ReadOnly Property m_ShareButton As PSMButtonState
                ReadOnly Property m_OptionsButton As PSMButtonState

                ReadOnly Property m_PSButton As PSMButtonState
                ReadOnly Property m_TrackPadButton As PSMButtonState


                ReadOnly Property m_LeftAnalogX As Single
                ReadOnly Property m_LeftAnalogY As Single
                ReadOnly Property m_RightAnalogX As Single
                ReadOnly Property m_RightAnalogY As Single
                ReadOnly Property m_LeftTriggerValue As Single
                ReadOnly Property m_RightTriggerValue As Single

                ReadOnly Property m_BigRumble As Boolean
                ReadOnly Property m_SmallRumble As Boolean
                ReadOnly Property m_LED_r As Boolean
                ReadOnly Property m_LED_g As Boolean
                ReadOnly Property m_LED_b As Boolean

                ReadOnly Property m_ResetPoseButtonPressTime As ULong
                ReadOnly Property m_bResetPoseRequestSent As Boolean
                ReadOnly Property m_bPoseResetButtonEnabled As Boolean
            End Class

            Class PSNaviState
                Implements IPSState

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSNavi)
                    m_L1Button = _FromPinvoke.L1Button
                    m_L2Button = _FromPinvoke.L2Button
                    m_L3Button = _FromPinvoke.L3Button
                    m_CircleButton = _FromPinvoke.CircleButton
                    m_CrossButton = _FromPinvoke.CrossButton
                    m_PSButton = _FromPinvoke.PSButton
                    m_TriggerButton = _FromPinvoke.TriggerButton
                    m_DPadUpButton = _FromPinvoke.DPadUpButton
                    m_DPadRightButton = _FromPinvoke.DPadRightButton
                    m_DPadDownButton = _FromPinvoke.DPadDownButton
                    m_DPadLeftButton = _FromPinvoke.DPadLeftButton

                    m_TriggerValue = CBool(_FromPinvoke.TriggerValue)
                    m_StickXAxis = CBool(_FromPinvoke.StickXAxis)
                    m_StickYAxis = CBool(_FromPinvoke.StickYAxis)
                End Sub

                ReadOnly Property m_L1Button As PSMButtonState
                ReadOnly Property m_L2Button As PSMButtonState
                ReadOnly Property m_L3Button As PSMButtonState
                ReadOnly Property m_CircleButton As PSMButtonState
                ReadOnly Property m_CrossButton As PSMButtonState
                ReadOnly Property m_PSButton As PSMButtonState
                ReadOnly Property m_TriggerButton As PSMButtonState
                ReadOnly Property m_DPadUpButton As PSMButtonState
                ReadOnly Property m_DPadRightButton As PSMButtonState
                ReadOnly Property m_DPadDownButton As PSMButtonState
                ReadOnly Property m_DPadLeftButton As PSMButtonState

                ReadOnly Property m_TriggerValue As Boolean
                ReadOnly Property m_StickXAxis As Boolean
                ReadOnly Property m_StickYAxis As Boolean
            End Class

            Class PSVirtualState
                Implements IPSState

                Private g_bAxisStates(PSM_MAX_VIRTUAL_CONTROLLER_AXES) As Byte
                Private g_iButtonStates(PSM_MAX_VIRTUAL_CONTROLLER_BUTTONS) As Integer

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMVirtualController)
                    m_bIsTrackingEnabled = CBool(_FromPinvoke.bIsTrackingEnabled)
                    m_bIsCurrentlyTracking = CBool(_FromPinvoke.bIsCurrentlyTracking)
                    m_bIsPositionValid = CBool(_FromPinvoke.bIsPositionValid)

                    m_DevicePath = _FromPinvoke.DevicePath

                    m_VendorID = _FromPinvoke.vendorID
                    m_ProductID = _FromPinvoke.productID

                    m_NumAxes = _FromPinvoke.numAxes
                    m_NumButtons = _FromPinvoke.numButtons

                    g_bAxisStates = _FromPinvoke.axisStates
                    g_iButtonStates = _FromPinvoke.buttonStates

                    m_TrackingColorType = _FromPinvoke.TrackingColorType
                End Sub

                ReadOnly Property m_bIsTrackingEnabled As Boolean
                ReadOnly Property m_bIsCurrentlyTracking As Boolean
                ReadOnly Property m_bIsPositionValid As Boolean

                ReadOnly Property m_DevicePath As String

                ReadOnly Property m_VendorID As Integer
                ReadOnly Property m_ProductID As Integer

                ReadOnly Property m_NumAxes As Integer
                ReadOnly Property m_NumButtons As Integer

                ReadOnly Property m_AxisStates(i As Integer) As Boolean
                    Get
                        Return CBool(g_bAxisStates(i))
                    End Get
                End Property
                ReadOnly Property m_ButtonStates(i As Integer) As Integer
                    Get
                        Return g_iButtonStates(i)
                    End Get
                End Property

                ReadOnly Property m_TrackingColorType As PSMTrackingColorType
            End Class

            Class PSPose
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPosef)
                    m_Position = m_Position.FromPinvoke(_FromPinvoke.Position)
                    m_Orientation = m_Orientation.FromPinvoke(_FromPinvoke.Orientation)
                End Sub

                ReadOnly Property m_Position As PSMVector3f
                ReadOnly Property m_Orientation As PSMQuatf
            End Class

            Class PSRawSensor
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSMoveRawSensorData)
                    m_Accelerometer = Nothing
                    m_Gyroscope = Nothing

                    m_LinearVelocityCmPerSec = m_LinearVelocityCmPerSec.FromPinvoke(_FromPinvoke.LinearVelocityCmPerSec)
                    m_LinearAccelerationCmPerSecSqr = m_LinearAccelerationCmPerSecSqr.FromPinvoke(_FromPinvoke.LinearAccelerationCmPerSecSqr)
                    m_AngularVelocityRadPerSec = m_AngularVelocityRadPerSec.FromPinvoke(_FromPinvoke.AngularVelocityRadPerSec)
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMDS4RawSensorData)
                    m_Accelerometer = m_Accelerometer.FromPinvoke(_FromPinvoke.Accelerometer)
                    m_Gyroscope = m_Gyroscope.FromPinvoke(_FromPinvoke.Gyroscope)

                    m_LinearVelocityCmPerSec = Nothing
                    m_LinearAccelerationCmPerSecSqr = Nothing
                    m_AngularVelocityRadPerSec = Nothing
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                ReadOnly Property m_Accelerometer As PSMVector3i
                ReadOnly Property m_Gyroscope As PSMVector3i

                ReadOnly Property m_LinearVelocityCmPerSec As PSMVector3i
                ReadOnly Property m_LinearAccelerationCmPerSecSqr As PSMVector3i
                ReadOnly Property m_AngularVelocityRadPerSec As PSMVector3i
                ReadOnly Property m_TimeInSeconds As Double
            End Class

            Class PSCalibratedSensor
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)
                    m_Magnetometer = m_Magnetometer.FromPinvoke(_FromPinvoke.Magnetometer)
                    m_Accelerometer = m_Accelerometer.FromPinvoke(_FromPinvoke.Accelerometer)
                    m_Gyroscope = m_Gyroscope.FromPinvoke(_FromPinvoke.Gyroscope)
                    m_TimeInSeconds = _FromPinvoke.TimeInSeconds
                End Sub

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMDS4CalibratedSensorData)
                    m_Magnetometer = Nothing
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
                Return (g_PSRawSensor IsNot Nothing AndAlso g_PSCalibratedSensor IsNot Nothing)
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
                    Select Case (g_iControllerType)
                        Case PSMControllerType.PSMController_Move
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMove)))
                            Try
                                If (PInvoke.PSM_GetControllerPSMoveStateEx(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMove)(hPtr)

                                    g_PSState = New PSMoveState(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try

                        Case PSMControllerType.PSMController_DualShock4
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMDualShock4)))
                            Try
                                If (PInvoke.PSM_GetControllerDualShock4StateEx(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMDualShock4)(hPtr)

                                    g_PSState = New PSDualShock4State(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                        Case PSMControllerType.PSMController_Navi
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSNavi)))
                            Try
                                If (PInvoke.PSM_GetControllerPSNaviState(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSNavi)(hPtr)

                                    g_PSState = New PSNaviState(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                        Case PSMControllerType.PSMController_Virtual
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMVirtualController)))
                            Try
                                If (PInvoke.PSM_GetControllerVirtualControllerStateEx(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMVirtualController)(hPtr)

                                    g_PSState = New PSVirtualState(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try

                    End Select
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
                    Select Case (g_iControllerType)
                        Case PSMControllerType.PSMController_Move
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMoveRawSensorData)))
                            Try
                                If (PInvoke.PSM_GetControllerPSMoveRawSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMoveRawSensorData)(hPtr)

                                    g_PSRawSensor = New PSRawSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                        Case PSMControllerType.PSMController_DualShock4
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMDS4RawSensorData)))
                            Try
                                If (PInvoke.PSM_GetControllerPSMoveRawSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMDS4RawSensorData)(hPtr)

                                    g_PSRawSensor = New PSRawSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                    End Select
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Sensor) > 0) Then
                    Select Case (g_iControllerType)
                        Case PSMControllerType.PSMController_Move
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)))
                            Try
                                If (PInvoke.PSM_GetControllerPSMoveSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPSMoveCalibratedSensorData)(hPtr)

                                    g_PSCalibratedSensor = New PSCalibratedSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                        Case PSMControllerType.PSMController_DualShock4
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMDS4CalibratedSensorData)))
                            Try
                                If (PInvoke.PSM_GetControllerPSMoveSensorData(m_ControllerId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMDS4CalibratedSensorData)(hPtr)

                                    g_PSCalibratedSensor = New PSCalibratedSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                    End Select
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
End Class
