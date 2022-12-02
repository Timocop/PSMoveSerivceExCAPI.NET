﻿Imports System.Runtime.InteropServices
Imports System.Text

Partial Public Class PSMoveServiceExCAPI
    Public Class PInvoke
        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector2f
            Public x As Single
            Public y As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector3f
            Public x As Single
            Public y As Single
            Public z As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMVector3i
            Public x As Integer
            Public y As Integer
            Public z As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMQuatf
            Public w As Single
            Public x As Single
            Public y As Single
            Public z As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMPosef
            Public Position As PINVOKE_PSMVector3f
            Public Orientation As PINVOKE_PSMQuatf
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMPhysicsData
            Public LinearVelocityCmPerSec As PINVOKE_PSMVector3f
            Public LinearAccelerationCmPerSecSqr As PINVOKE_PSMVector3f
            Public AngularVelocityRadPerSec As PINVOKE_PSMVector3f
            Public AngularAccelerationRadPerSecSqr As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMoveRawSensorData
            Public LinearVelocityCmPerSec As PINVOKE_PSMVector3i
            Public LinearAccelerationCmPerSecSqr As PINVOKE_PSMVector3i
            Public AngularVelocityRadPerSec As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMoveCalibratedSensorData
            Public Magnetometer As PINVOKE_PSMVector3f
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionEllipse
            Public center As PINVOKE_PSMVector2f
            Public half_x_extent As Single
            Public half_y_extent As Single
            Public angle As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionLightbar
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := 3)> Public triangle As PINVOKE_PSMVector2f()
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := 3)> Public quad As PINVOKE_PSMVector2f()
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingProjectionPointcloud
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := 7)> Public points As PINVOKE_PSMVector2f()
            Public point_count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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


        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMPSMove
            Public bHasValidHardwareCalibration As Byte
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
            Public bHasUnpublishedState As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 256)> Public DevicePath As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 128)> Public DeviceSerial As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 128)> Public AssignedHostSerial As String

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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMDS4RawSensorData
            Public Accelerometer As PINVOKE_PSMVector3i
            Public Gyroscope As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMDS4CalibratedSensorData
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMDualShock4
            Public bHasValidHardwareCalibration As Byte
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
            Public bHasUnpublishedState As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 256)> Public DevicePath As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 128)> Public DeviceSerial As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 128)> Public AssignedHostSerial As String
            Public PairedToHost As Byte
            Public ConnectionType As Constants.PSMConnectionType
            Public TrackingColorType As Constants.PSMTrackingColorType


            Public DPadUpButton As Constants.PSMButtonState
            Public DPadDownButton As Constants.PSMButtonState
            Public DPadLeftButton As Constants.PSMButtonState
            Public DPadRightButton As Constants.PSMButtonState

            Public SquareButton As Constants.PSMButtonState
            Public CrossButton As Constants.PSMButtonState
            Public CircleButton As Constants.PSMButtonState
            Public TriangleButton As Constants.PSMButtonState

            Public L1Button As Constants.PSMButtonState
            Public R1Button As Constants.PSMButtonState
            Public L2Button As Constants.PSMButtonState
            Public R2Button As Constants.PSMButtonState
            Public L3Button As Constants.PSMButtonState
            Public R3Button As Constants.PSMButtonState

            Public ShareButton As Constants.PSMButtonState
            Public OptionsButton As Constants.PSMButtonState

            Public PSButton As Constants.PSMButtonState
            Public TrackPadButton As Constants.PSMButtonState


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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMVirtualController
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsPositionValid As Byte

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 256)> Public DevicePath As String

            Public vendorID As Integer
            Public productID As Integer

            Public numAxes As Integer
            Public numButtons As Integer

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSM_MAX_VIRTUAL_CONTROLLER_AXES)> Public _
                axisStates As Byte()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSM_MAX_VIRTUAL_CONTROLLER_BUTTONS)> Public _
                buttonStates As Integer()

            Public TrackingColorType As Constants.PSMTrackingColorType
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMController
            Public ControllerID As Integer
            Public ControllerType As Constants.PSMControllerType
            Public ControllerHand As Constants.PSMControllerHand

            Public bValid As Byte
            Public OutputSequenceNum As Integer
            Public InputSequenceNum As Integer
            Public IsConnected As Byte
            Public DataFrameLastReceivedTime As ULong
            Public DataFrameAverageFPS As Single
            Public ListenerCount As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMClientTrackerInfo
            Public tracker_id As Integer

            Public tracker_type As Constants.PSMTrackerType
            Public tracker_Driver As Constants.PSMTrackerDriver
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 128)> Public device_path As String
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := 64)> Public shared_memory_name As String

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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTracker
            Public tracker_info As PINVOKE_PSMClientTrackerInfo

            Public listener_count As Integer
            Public is_connected As Byte
            Public sequence_num As Integer
            Public data_frame_last_received_time As ULong
            Public data_frame_average_fps As Single

            Public opaque_shared_memory_accesor As IntPtr
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheusRawSensorData
            Public Accelerometer As PINVOKE_PSMVector3i
            Public Gyroscope As PINVOKE_PSMVector3i
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheusCalibratedSensorData
            Public Accelerometer As PINVOKE_PSMVector3f
            Public Gyroscope As PINVOKE_PSMVector3f
            Public TimeInSeconds As Double
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMMorpheus
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsOrientationValid As Byte
            Public bIsPositionValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMVirtualHMD
            Public bIsTrackingEnabled As Byte
            Public bIsCurrentlyTracking As Byte
            Public bIsPositionValid As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
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

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMServiceVersion
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := Constants.PSMOVESERVICE_MAX_VERSION_STRING_LEN)> Public _
                version_string As String
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMController_Serial
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst := Constants.PSMOVESERVICE_CONTROLLER_SERIAL_LEN)> Public _
                [get] As String
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMControllerList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_CONTROLLER_COUNT)> Public _
                controller_id As Integer()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_CONTROLLER_COUNT)> Public _
                controller_type As Constants.PSMControllerType()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_CONTROLLER_COUNT)> Public _
                controller_hand As Constants.PSMControllerHand()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_CONTROLLER_COUNT)> Public _
                controller_serial As PINVOKE_PSMController_Serial()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_CONTROLLER_COUNT)> Public _
                parent_controller_serial As PINVOKE_PSMController_Serial()

            Public count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackerList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_TRACKER_COUNT)> Public _
                trackers As PINVOKE_PSMClientTrackerInfo()

            Public count As Integer
            Public global_forward_degrees As Single
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMHmdList
            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_HMD_COUNT)> Public _
                hmd_id As PINVOKE_PSMClientTrackerInfo()

            <MarshalAs(UnmanagedType.ByValArray, SizeConst := Constants.PSMOVESERVICE_MAX_HMD_COUNT)> Public _
                hmd_type As Constants.PSMHmdType

            Public count As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Ansi)>
        Public Structure PINVOKE_PSMTrackingSpace
            Public global_forward_degrees As Single
        End Structure

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_Initialize(<MarshalAs(UnmanagedType.LPStr)> host As String,
                                              <MarshalAs(UnmanagedType.LPStr)> port As String, timeout_ms As Integer) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_Shutdown() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_Update() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_UpdateNoPollMessages() As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetClientVersionString() As IntPtr
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetIsInitialized() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetIsConnected() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_HasConnectionStatusChanged() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_HasControllerListChanged() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_HasTrackerListChanged() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_HasHMDListChanged() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_WasSystemButtonPressed() As Byte
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetServiceVersionString(out_version_string As StringBuilder,
                                                           max_version_string As Integer, timeout_ms As Integer) _
            As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetServiceVersionStringAsync(PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_PollNextMessage(PSMMessage *out_message, size_t message_size);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_SendOpaqueRequest(PSMRequestHandle request_handle, PSMRequestID *out_request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_RegisterCallback(PSMRequestID request_id, PSMResponseCallback callback, void *callback_userdata);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_CancelCallback(PSMRequestID request_id);
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_EatResponse(PSMRequestID request_id);
        'PSM_PUBLIC_FUNCTION(PSMController *) PSM_GetController(PSMControllerID controller_id);

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerEx(controller_id As Integer, controller_out As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveState(controller_id As Integer, controller_out As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveStateEx(controller_id As Integer, controller_out As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSNaviState(controller_id As Integer, controller_out As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerDualShock4StateEx(controller_id As Integer, controller_out As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerVirtualControllerStateEx(controller_id As Integer,
                                                                         controller_out As IntPtr) As Integer
        End Function

        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4State(PSMControllerID controller_id, PSMDualShock4 *controller_out); 
        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerVirtualControllerState(PSMControllerID controller_id, PSMVirtualController *controller_out);

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_AllocateControllerListener(controller_id As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_FreeControllerListener(controller_id As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerList(out_controller_list As IntPtr, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_StartControllerDataStream(controller_id As Integer, data_stream_flags As UInteger,
                                                             timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_StopControllerDataStream(controller_id As Integer, timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_SetControllerLEDTrackingColor(controller_id As Integer, tracking_color As Integer,
                                                                 timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_ResetControllerOrientation(controller_id As Integer, q_pose As IntPtr,
                                                              timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_SetControllerDataStreamTrackerIndex(controller_id As Integer, tracker_id As Integer,
                                                                       timeout_ms As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_SetControllerHand(controller_id As Integer, hand As Integer, timeout_ms As Integer) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerOrientation(controller_id As Integer, out_orientation As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPosition(controller_id As Integer, out_position As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPose(controller_id As Integer, out_pose As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPhysicsData(controller_id As Integer, out_physics As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveRawSensorData(controller_id As Integer, out_data As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerDualShock4RawSensorData(controller_id As Integer, out_data As IntPtr) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerPSMoveSensorData(controller_id As Integer, out_data As IntPtr) _
            As Integer
        End Function


        'PSM_PUBLIC_FUNCTION(PSMResult) PSM_GetControllerDualShock4SensorData(PSMControllerID controller_id, PSMDS4CalibratedSensorData *out_data);

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRumble(controller_id As Integer, channel As Integer,
                                                       <Out> ByRef out_rumble_fraction As Single) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetIsControllerStable(controller_id As Integer, <Out> ByRef out_is_stable As Byte) _
            As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetIsControllerTracking(controller_id As Integer, <Out> ByRef out_is_tracking As Byte) _
            As Integer
        End Function

        '<DllImport("PSMoveClient_CAPI.dll", CharSet:=CharSet.Ansi)>
        'Public Shared Function PSM_GetControllerRawTrackerData(controller_id As Integer, out_tracker_data As IntPtr) As Integer
        'End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerShape(controller_id As Integer,
                                                                <Out> ByRef out_shape_type As Integer) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataEllipse(controller_id As Integer,
                                                                      out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataLightbar(controller_id As Integer,
                                                                       out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_GetControllerRawTrackerDataPointcloud(controller_id As Integer,
                                                                         out_tracker_data As IntPtr) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_SetControllerLEDOverrideColor(controller_id As Integer, r As Byte, g As Byte,
                                                                 b As Byte) As Integer
        End Function

        <DllImport("PSMoveClient_CAPI.dll", CharSet := CharSet.Ansi)>
        Public Shared Function PSM_SetControllerRumble(controller_id As Integer, channel As Integer,
                                                       rumble_fraction As Single) As Integer
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
