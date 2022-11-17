Imports System.Drawing
Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Partial Public Class PSMoveServiceExCAPI
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
End Class
