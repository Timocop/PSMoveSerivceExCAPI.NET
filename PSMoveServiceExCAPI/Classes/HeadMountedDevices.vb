Imports System.Drawing
Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Partial Public Class PSMoveServiceExCAPI
    Class HeadMountedDevices
        Implements IDisposable

        Private g_mInfo As Info
        Private g_bListening As Boolean = False
        Private g_bDataStream As Boolean = False

        Private g_iDataStreamFlags As PSMStreamFlags = PSMStreamFlags.PSMStreamFlags_defaultStreamOptions

        Public Sub New(_HmdId As Integer)
            Me.New(_HmdId, False, True)
        End Sub

        Public Sub New(_HmdId As Integer, _StartDataStream As Boolean, _NoInitalization As Boolean)
            If (_HmdId < 0 OrElse _HmdId > PSMOVESERVICE_MAX_HMD_COUNT - 1) Then
                Throw New ArgumentOutOfRangeException()
            End If

            g_mInfo = New Info(Me, _HmdId)

            If (_NoInitalization) Then
                g_mInfo.Refresh(Info.RefreshFlags.RefreshType_All)
            Else
                g_mInfo.Refresh(Info.RefreshFlags.RefreshType_All Or Info.RefreshFlags.RefreshType_Init)
            End If

            If (_StartDataStream) Then
                m_Listening = True
                m_DataStreamEnabled = True
            End If
        End Sub

        Protected Sub New(_HmdId As Integer, _Serial As String, _HmdType As PSMHmdType)
            Me.New(_HmdId, False, True)
            m_Info.SetHmdSerial(_Serial)
            m_Info.SetHmdType(_HmdType)

            m_Info.SetHmdInitalized()
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
            Private g_Hmds As HeadMountedDevices

            Private g_PSState As IPSState = Nothing
            Private g_Physics As PSPhysics = Nothing
            Private g_Pose As PSPose = Nothing
            Private g_PSRawSensor As PSRawSensor = Nothing
            Private g_PSCalibratedSensor As PSCalibratedSensor = Nothing
            Private g_PSTracking As PSTracking = Nothing

            Private ReadOnly g_iHmdId As Integer = -1

            Private g_iHmdType As PSMHmdType = PSMHmdType.PSMHmd_None
            Private g_sHmdSerial As String = ""
            Private g_iOutputSequenceNum As Integer
            Private g_iInputSequenceNum As Integer
            Private g_bIsConnected As Boolean
            Private g_iDataFrameLastReceivedTime As ULong
            Private g_iDataFrameAverageFPS As Single
            Private g_iListenerCount As Integer

            Private g_bIsInitalized As Boolean = False

            Enum RefreshFlags
                RefreshType_Init = (1 << 0)
                RefreshType_Basic = (1 << 1)
                RefreshType_State = (1 << 2)
                RefreshType_Pose = (1 << 3)
                RefreshType_Physics = (1 << 4)
                RefreshType_Sensor = (1 << 5)
                RefreshType_Tracker = (1 << 6)
                RefreshType_All =
                    RefreshType_Basic Or RefreshType_State Or RefreshType_Pose Or
                    RefreshType_Physics Or RefreshType_Sensor Or RefreshType_Tracker
            End Enum

            Public Sub New(_Hmd As HeadMountedDevices, _HmdId As Integer)
                g_Hmds = _Hmd
                g_iHmdId = _HmdId
            End Sub

            ReadOnly Property m_HmdId As Integer
                Get
                    Return g_iHmdId
                End Get
            End Property

            ReadOnly Property m_HmdType As PSMHmdType
                Get
                    Return g_iHmdType
                End Get
            End Property

            Protected Friend Sub SetHmdSerial(sSerial As String)
                g_sHmdSerial = sSerial
            End Sub

            Protected Friend Sub SetHmdType(iHmdType As PSMHmdType)
                g_iHmdType = iHmdType
            End Sub

            Protected Friend Sub SetHmdInitalized()
                g_bIsInitalized = True
            End Sub

            ReadOnly Property m_IsInitalized As Boolean
                Get
                    Return g_bIsInitalized
                End Get
            End Property

            ReadOnly Property m_HmdSerial As String
                Get
                    Return g_sHmdSerial
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

            ReadOnly Property m_PSMoveState As PSMorpheusState
                Get
                    Return TryCast(g_PSState, PSMorpheusState)
                End Get
            End Property

            ReadOnly Property m_PSVirtualState As PSVirtualHmdState
                Get
                    Return TryCast(g_PSState, PSVirtualHmdState)
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

            Class PSMorpheusState
                Implements IPSState

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMMorpheus)
                    m_IsTrackingEnabled = CBool(_FromPinvoke.bIsTrackingEnabled)
                    m_IsCurrentlyTracking = CBool(_FromPinvoke.bIsCurrentlyTracking)
                    m_IsOrientationValid = CBool(_FromPinvoke.bIsOrientationValid)
                    m_IsPositionValid = CBool(_FromPinvoke.bIsPositionValid)

                End Sub

                ReadOnly Property m_IsTrackingEnabled As Boolean
                ReadOnly Property m_IsCurrentlyTracking As Boolean
                ReadOnly Property m_IsOrientationValid As Boolean
                ReadOnly Property m_IsPositionValid As Boolean
            End Class

            Class PSVirtualHmdState
                Implements IPSState

                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMVirtualHMD)
                    m_IsTrackingEnabled = CBool(_FromPinvoke.bIsTrackingEnabled)
                    m_IsCurrentlyTracking = CBool(_FromPinvoke.bIsCurrentlyTracking)
                    m_IsPositionValid = CBool(_FromPinvoke.bIsPositionValid)
                End Sub

                ReadOnly Property m_IsTrackingEnabled As Boolean
                ReadOnly Property m_IsCurrentlyTracking As Boolean
                ReadOnly Property m_IsPositionValid As Boolean
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
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMMorpheusRawSensorData)
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
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMMorpheusCalibratedSensorData)
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
                Private ReadOnly g_mTrackingProjection As Object
                Private ReadOnly g_iShape As PSMShape = PSMShape.PSMShape_INVALID_PROJECTION

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
                                i.mPoints(j) =
                                    i.mPoints(j).FromPinvoke(_FromPinvokePointcloud.TrackingProjection.points(j))
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

                ReadOnly Property m_Shape As PSMShape
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
                If ((iRefreshType And RefreshFlags.RefreshType_Init) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMHmdList)))
                    Try
                        If (CType(PInvoke.PSM_GetHmdList(hPtr, PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                            Dim mHmdList = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMHmdList)(hPtr)

                            For i = 0 To mHmdList.count - 1
                                If (mHmdList.hmd_id(i) <> m_HmdId) Then
                                    Continue For
                                End If

                                Dim sSerial As String = String.Format("Invalid_{0}", mHmdList.hmd_id(i))

                                Select Case (mHmdList.hmd_type(i))
                                    Case PSMHmdType.PSMHmd_Morpheus
                                        sSerial = String.Format("MorpheusHMD_{0}", mHmdList.hmd_id(i))
                                    Case PSMHmdType.PSMHmd_Virtual
                                        sSerial = String.Format("VirtualHMD_{0}", mHmdList.hmd_id(i))
                                End Select

                                g_sHmdSerial = sSerial
                                g_iHmdType = mHmdList.hmd_type(i)

                                SetHmdInitalized()
                                Exit For
                            Next
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Basic) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMHeadMountedDisplay)))
                    Try
                        If (PInvoke.PSM_GetHmdEx(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMHeadMountedDisplay)(hPtr)

                            g_iHmdType = mData.HmdType
                            g_iOutputSequenceNum = mData.OutputSequenceNum
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
                    Select Case (g_iHmdType)
                        Case PSMHmdType.PSMHmd_Morpheus
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMMorpheus)))
                            Try
                                If (PInvoke.PSM_GetHmdMorpheusStateEx(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMMorpheus)(hPtr)

                                    g_PSState = New PSMorpheusState(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try

                        Case PSMHmdType.PSMHmd_Virtual
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMVirtualHMD)))
                            Try
                                If (PInvoke.PSM_GetHmdVirtualStateEx(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMVirtualHMD)(hPtr)

                                    g_PSState = New PSVirtualHmdState(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try

                    End Select
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Pose) > 0) Then
                    Dim mPose As New PInvoke.PINVOKE_PSMPosef
                    mPose.Position.x = 0.0F
                    mPose.Position.y = 0.0F
                    mPose.Position.z = 0.0F
                    mPose.Orientation.x = 0.0F
                    mPose.Orientation.y = 0.0F
                    mPose.Orientation.z = 0.0F
                    mPose.Orientation.w = 1.0F

                    Dim bPositionValid As Boolean = False
                    Dim bOrientationValid As Boolean = False

                    If (True) Then
                        Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMVector3f)))
                        Try
                            If (PInvoke.PSM_GetHmdPosition(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMVector3f)(hPtr)

                                mPose.Position = mData
                                bPositionValid = True
                            End If
                        Finally
                            Marshal.FreeHGlobal(hPtr)
                        End Try
                    End If


                    If (True) Then
                        Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMQuatf)))
                        Try
                            If (PInvoke.PSM_GetHmdOrientation(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMQuatf)(hPtr)

                                mPose.Orientation = mData
                                bOrientationValid = True
                            End If
                        Finally
                            Marshal.FreeHGlobal(hPtr)
                        End Try
                    End If

                    If (bPositionValid OrElse bOrientationValid) Then
                        g_Pose = New PSPose(mPose)
                    End If
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Physics) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMPhysicsData)))
                    Try
                        If (PInvoke.PSM_GetHmdPhysicsData(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMPhysicsData)(hPtr)

                            g_Physics = New PSPhysics(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Sensor) > 0) Then
                    Select Case (g_iHmdType)
                        Case PSMHmdType.PSMHmd_Morpheus
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMMorpheusRawSensorData)))
                            Try
                                If (PInvoke.PSM_GetHmdMorpheusRawSensorData(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMMorpheusRawSensorData)(hPtr)

                                    g_PSRawSensor = New PSRawSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                    End Select
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Sensor) > 0) Then
                    Select Case (g_iHmdType)
                        Case PSMHmdType.PSMHmd_Morpheus
                            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMMorpheusCalibratedSensorData)))
                            Try
                                If (PInvoke.PSM_GetHmdMorpheusSensorData(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMMorpheusCalibratedSensorData)(hPtr)

                                    g_PSCalibratedSensor = New PSCalibratedSensor(mData)
                                End If
                            Finally
                                Marshal.FreeHGlobal(hPtr)
                            End Try
                    End Select
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Tracker) > 0) Then
                    Dim iShape As Integer = PSMShape.PSMShape_INVALID_PROJECTION
                    If (PInvoke.PSM_GetHmdRawTrackerShape(m_HmdId, iShape) = PSMResult.PSMResult_Success) Then
                        Select Case (iShape)
                            Case PSMShape.PSMShape_Ellipse
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataEllipse)))
                                Try
                                    If (PInvoke.PSM_GetHmdRawTrackerDataEllipse(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                        Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMRawTrackerDataEllipse)(hPtr)

                                        g_PSTracking = New PSTracking(mData)
                                    End If

                                Finally
                                    Marshal.FreeHGlobal(hPtr)
                                End Try

                            Case PSMShape.PSMShape_LightBar
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataLightbar)))
                                Try
                                    If (PInvoke.PSM_GetHmdRawTrackerDataLightbar(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                                        Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMRawTrackerDataLightbar)(hPtr)

                                        g_PSTracking = New PSTracking(mData)
                                    End If

                                Finally
                                    Marshal.FreeHGlobal(hPtr)
                                End Try

                            Case PSMShape.PSMShape_PointCloud
                                Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMRawTrackerDataPointcloud)))
                                Try
                                    If (PInvoke.PSM_GetHmdRawTrackerDataPointcloud(m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
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
            Set
                If (g_bListening = Value) Then
                    Return
                End If

                g_bListening = Value

                If (g_bListening) Then
                    If (PInvoke.PSM_AllocateHmdListener(g_mInfo.m_HmdId) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_AllocateHmdListener failed")
                    End If
                Else
                    If (PInvoke.PSM_FreeHmdListener(g_mInfo.m_HmdId) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_FreeHmdListener failed")
                    End If
                End If
            End Set
        End Property

        Property m_DataStreamFlags As PSMStreamFlags
            Get
                Return g_iDataStreamFlags
            End Get
            Set
                If (g_iDataStreamFlags = Value) Then
                    Return
                End If

                g_iDataStreamFlags = Value

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
            Set
                If (g_bDataStream = Value) Then
                    Return
                End If

                g_bDataStream = Value

                If (g_bDataStream) Then
                    If (PInvoke.PSM_StartHmdDataStream(g_mInfo.m_HmdId, CUInt(m_DataStreamFlags), PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_StartHmdDataStream failed")
                    End If
                Else
                    If (PInvoke.PSM_StopHmdDataStream(g_mInfo.m_HmdId, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                        Throw New ArgumentException("PSM_StopHmdDataStream failed")
                    End If
                End If
            End Set
        End Property

        Public Sub SetTrackerStream(iTrackerID As Integer)
            If (PInvoke.PSM_SetHmdDataStreamTrackerIndex(m_Info.m_HmdId, iTrackerID, PSM_DEFAULT_TIMEOUT) <> PSMResult.PSMResult_Success) Then
                Throw New ArgumentException("PSM_SetHmdDataStreamTrackerIndex failed")
            End If

            ' Start streams then if not already
            m_DataStreamFlags = (m_DataStreamFlags Or PSMStreamFlags.PSMStreamFlags_includeRawTrackerData)

            m_Listening = True
            m_DataStreamEnabled = True
        End Sub

        Public Function IsTrackerStreamingThisHmd(iTrackerID As Integer) As Boolean
            If (Not m_Info.IsTrackingValid) Then
                Return False
            End If

            If (CInt(m_Info.m_PSTracking.m_ValidTrackerBitmask And (1 << iTrackerID)) = 0) Then
                Return False
            End If

            Return (m_Info.m_PSTracking.m_TrackerID = iTrackerID)
        End Function

        Public Function IsHmdStable() As Boolean
            Dim bStable As Byte = 0
            PInvoke.PSM_GetIsHmdStable(m_Info.m_HmdId, bStable)
            Return CBool(bStable)
        End Function

        Public Function IsHmdTracking() As Boolean
            Dim bTracked As Byte = 0
            PInvoke.PSM_GetIsHmdTracking(m_Info.m_HmdId, bTracked)
            Return CBool(bTracked)
        End Function

        Public Function GetHmdPosition() As PSMVector3f
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMVector3f)))
            Try
                If (PInvoke.PSM_GetHmdPosition(m_Info.m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMVector3f)(hPtr)

                    Return (New PSMVector3f()).FromPinvoke(mData)
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return New PSMVector3f()
        End Function

        Public Function GetHmdOrientation() As PSMQuatf
            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMQuatf)))
            Try
                If (PInvoke.PSM_GetHmdOrientation(m_Info.m_HmdId, hPtr) = PSMResult.PSMResult_Success) Then
                    Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMQuatf)(hPtr)

                    Return (New PSMQuatf()).FromPinvoke(mData)
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return New PSMQuatf()
        End Function

        Public Shared Function GetHmdList() As HeadMountedDevices()
            Dim mHmds As New List(Of HeadMountedDevices)

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMHmdList)))
            Try
                If (CType(PInvoke.PSM_GetHmdList(hPtr, PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                    Dim mHmdList = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMHmdList)(hPtr)

                    For i = 0 To mHmdList.count - 1
                        Dim sSerial As String = String.Format("Invalid_{0}", mHmdList.hmd_id(i))

                        Select Case (mHmdList.hmd_type(i))
                            Case PSMHmdType.PSMHmd_Morpheus
                                sSerial = String.Format("MorpheusHMD_{0}", mHmdList.hmd_id(i))
                            Case PSMHmdType.PSMHmd_Virtual
                                sSerial = String.Format("VirtualHMD_{0}", mHmdList.hmd_id(i))
                        End Select

                        Dim mHmd As New HeadMountedDevices(
                            mHmdList.hmd_id(i),
                            sSerial,
                            mHmdList.hmd_type(i)
                        )

                        mHmds.Add(mHmd)
                    Next
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return mHmds.ToArray
        End Function

        Public Shared Function GetValidHmdCount() As Integer
            Dim iCount = 0

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMHeadMountedDisplay)))
            Try
                For i = 0 To PSMOVESERVICE_MAX_HMD_COUNT - 1
                    If (PInvoke.PSM_GetHmdEx(i, hPtr) = PSMResult.PSMResult_Success) Then
                        Dim hmd = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMHeadMountedDisplay)(hPtr)
                        If (CBool(hmd.bValid)) Then
                            iCount += 1
                        End If
                    End If
                Next
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return iCount
        End Function

        Public Shared Function GetConnectedHmdCount() As Integer
            Dim iCount = 0

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMHeadMountedDisplay)))
            Try
                For i = 0 To PSMOVESERVICE_MAX_HMD_COUNT - 1
                    If (PInvoke.PSM_GetHmdEx(i, hPtr) = PSMResult.PSMResult_Success) Then
                        Dim hmd = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMHeadMountedDisplay)(hPtr)
                        If (CBool(hmd.bValid) AndAlso CBool(hmd.IsConnected)) Then
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
            Try
                m_Listening = False
            Catch ex As Exception
                ' Connection probably already dropped.
            End Try

            Try
                m_DataStreamEnabled = False
                m_DataStreamFlags = PSMStreamFlags.PSMStreamFlags_defaultStreamOptions
            Catch ex As Exception
                ' Connection probably already dropped.
            End Try
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
