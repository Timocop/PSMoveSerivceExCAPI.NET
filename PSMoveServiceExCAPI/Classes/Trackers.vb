Imports System.Runtime.InteropServices
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Partial Public Class PSMoveServiceExCAPI
    Class Trackers
        Implements IDisposable

        Private g_mInfo As Info
        Private g_bListening As Boolean = False
        Private g_bDataStream As Boolean = False

        Public Sub New(_TrackerId As Integer)
            Me.New(_TrackerId, False, True)
        End Sub

        Public Sub New(_TrackerId As Integer, _StartDataStream As Boolean, _NoInitalization As Boolean)
            If (_TrackerId < 0 OrElse _TrackerId > PSMOVESERVICE_MAX_TRACKER_COUNT - 1) Then
                Throw New ServiceExceptions.ServiceDeviceOutOfRangeException()
            End If

            g_mInfo = New Info(Me, _TrackerId)

            If (_NoInitalization) Then
                g_mInfo.Refresh(Info.RefreshFlags.RefreshType_Stats)
            Else
                g_mInfo.Refresh(Info.RefreshFlags.RefreshType_Stats Or Info.RefreshFlags.RefreshType_Init)
            End If

            If (_StartDataStream) Then
                m_Listening = True
                m_DataStreamEnabled = True
            End If
        End Sub

        Protected Friend Sub New(_TrackerId As Integer, _PSMClientTrackerInfo As PInvoke.PINVOKE_PSMClientTrackerInfo)
            Me.New(_TrackerId, False, True)

            m_Info.SetTrackerType(_PSMClientTrackerInfo.tracker_type)
            m_Info.SetTrackerDriver(_PSMClientTrackerInfo.tracker_Driver)
            m_Info.SetTrackerDevicePath(_PSMClientTrackerInfo.device_path)
            m_Info.SetTrackerSharedMemoryName(_PSMClientTrackerInfo.shared_memory_name)
            m_Info.SetTrackerView(_PSMClientTrackerInfo)
            m_Info.SetTrackerPose(_PSMClientTrackerInfo)

            m_Info.SetTrackerInitalized()
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
            Private g_Trackers As Trackers

            Private g_Pose As PSPose = Nothing
            Private g_View As PSView = Nothing
            Private g_Stats As PSStats = Nothing

            Private ReadOnly g_iTrackerId As Integer = -1

            Private g_iTrackerType As PSMTrackerType = PSMTrackerType.PSMTracker_None
            Private g_iTrackerDrvier As PSMTrackerDriver = PSMTrackerDriver.PSMDriver_LIBUSB
            Private g_sDevicePath As String = ""
            Private g_sSharedMemoryName As String = ""

            Private g_bIsInitalized As Boolean = False

            Enum RefreshFlags
                RefreshType_Init = (1 << 0)
                RefreshType_Stats = (1 << 1)
            End Enum

            Public Sub New(_Trackers As Trackers, _TrackerId As Integer)
                g_Trackers = _Trackers
                g_iTrackerId = _TrackerId
            End Sub

            Protected Friend Sub SetTrackerType(iTrackerType As PSMTrackerType)
                g_iTrackerType = iTrackerType
            End Sub

            Protected Friend Sub SetTrackerDriver(iTrackerDrvier As PSMTrackerDriver)
                g_iTrackerDrvier = iTrackerDrvier
            End Sub

            Protected Friend Sub SetTrackerDevicePath(sDevicePath As String)
                g_sDevicePath = sDevicePath
            End Sub

            Protected Friend Sub SetTrackerSharedMemoryName(sSharedMemoryName As String)
                g_sSharedMemoryName = sSharedMemoryName
            End Sub

            Protected Friend Sub SetTrackerView(mView As PInvoke.PINVOKE_PSMClientTrackerInfo)
                g_View = New PSView(mView)
            End Sub

            Protected Friend Sub SetTrackerPose(mPose As PInvoke.PINVOKE_PSMClientTrackerInfo)
                g_Pose = New PSPose(mPose.tracker_pose)
            End Sub

            Protected Friend Sub SetTrackerInitalized()
                g_bIsInitalized = True
            End Sub

            ReadOnly Property m_TrackerId As Integer
                Get
                    Return g_iTrackerId
                End Get
            End Property

            ReadOnly Property m_TrackerType As PSMTrackerType
                Get
                    Return g_iTrackerType
                End Get
            End Property

            ReadOnly Property m_TrackerDrvier As PSMTrackerDriver
                Get
                    Return g_iTrackerDrvier
                End Get
            End Property

            ReadOnly Property m_DevicePath As String
                Get
                    Return g_sDevicePath
                End Get
            End Property

            ReadOnly Property m_SharedMemoryName As String
                Get
                    Return g_sSharedMemoryName
                End Get
            End Property

            ReadOnly Property m_IsInitalized As Boolean
                Get
                    Return g_bIsInitalized
                End Get
            End Property

            ReadOnly Property m_Pose As PSPose
                Get
                    Return g_Pose
                End Get
            End Property

            ReadOnly Property m_View As PSView
                Get
                    Return g_View
                End Get
            End Property

            ReadOnly Property m_Stats As PSStats
                Get
                    Return g_Stats
                End Get
            End Property

            Class PSView
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMClientTrackerInfo)
                    tracker_focal_lengths = tracker_focal_lengths.FromPinvoke(_FromPinvoke.tracker_focal_lengths)
                    tracker_principal_point = tracker_principal_point.FromPinvoke(_FromPinvoke.tracker_principal_point)
                    tracker_screen_dimensions = tracker_screen_dimensions.FromPinvoke(_FromPinvoke.tracker_screen_dimensions)
                    tracker_hfov = tracker_hfov
                    tracker_vfov = tracker_vfov
                    tracker_znear = tracker_znear
                    tracker_zfar = tracker_zfar
                    tracker_k1 = tracker_k1
                    tracker_k2 = tracker_k2
                    tracker_k3 = tracker_k3
                    tracker_p1 = tracker_p1
                    tracker_p2 = tracker_p2
                End Sub

                ReadOnly Property tracker_focal_lengths As PSMVector2f
                ReadOnly Property tracker_principal_point As PSMVector2f
                ReadOnly Property tracker_screen_dimensions As PSMVector2f
                ReadOnly Property tracker_hfov As Single
                ReadOnly Property tracker_vfov As Single
                ReadOnly Property tracker_znear As Single
                ReadOnly Property tracker_zfar As Single
                ReadOnly Property tracker_k1 As Single
                ReadOnly Property tracker_k2 As Single
                ReadOnly Property tracker_k3 As Single
                ReadOnly Property tracker_p1 As Single
                ReadOnly Property tracker_p2 As Single

                Function GetRaw() As PInvoke.PINVOKE_PSMClientTrackerInfo
                    Dim mInfo As New PInvoke.PINVOKE_PSMClientTrackerInfo

                    mInfo.tracker_focal_lengths = tracker_focal_lengths.ToPinvoke
                    mInfo.tracker_principal_point = tracker_principal_point.ToPinvoke
                    mInfo.tracker_screen_dimensions = tracker_screen_dimensions.ToPinvoke
                    mInfo.tracker_hfov = tracker_hfov
                    mInfo.tracker_vfov = tracker_vfov
                    mInfo.tracker_znear = tracker_znear
                    mInfo.tracker_zfar = tracker_zfar
                    mInfo.tracker_k1 = tracker_k1
                    mInfo.tracker_k2 = tracker_k2
                    mInfo.tracker_k3 = tracker_k3
                    mInfo.tracker_p1 = tracker_p1
                    mInfo.tracker_p2 = tracker_p2

                    Return mInfo
                End Function
            End Class

            Class PSPose
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMPosef)
                    m_Position = m_Position.FromPinvoke(_FromPinvoke.Position)
                    m_Orientation = m_Orientation.FromPinvoke(_FromPinvoke.Orientation)
                End Sub

                ReadOnly Property m_Position As PSMVector3f
                ReadOnly Property m_Orientation As PSMQuatf
            End Class

            Class PSStats
                Sub New(_FromPinvoke As PInvoke.PINVOKE_PSMTracker)
                    m_ListenerCount = _FromPinvoke.listener_count
                    m_IsConnected = CBool(_FromPinvoke.is_connected)
                    m_SequenceNum = _FromPinvoke.sequence_num
                    m_DataFrameLastReceivedTime = TimeSpan.FromMilliseconds(_FromPinvoke.data_frame_last_received_time)
                    m_DataFrameAverageFps = _FromPinvoke.data_frame_average_fps
                    m_TrackerExposure = _FromPinvoke.tracker_exposure
                    m_TrackerGain = _FromPinvoke.tracker_gain
                    m_TrackerWidth = _FromPinvoke.tracker_width
                End Sub

                ReadOnly Property m_ListenerCount As Integer
                ReadOnly Property m_IsConnected As Boolean
                ReadOnly Property m_SequenceNum As Integer
                ReadOnly Property m_DataFrameLastReceivedTime As TimeSpan
                ReadOnly Property m_DataFrameAverageFps As Single

                ReadOnly Property m_TrackerExposure As Integer
                ReadOnly Property m_TrackerGain As Integer
                ReadOnly Property m_TrackerWidth As Integer
            End Class

            Public Function IsViewValid() As Boolean
                Return (m_View IsNot Nothing)
            End Function

            Public Function IsPoseValid() As Boolean
                Return (m_Pose IsNot Nothing)
            End Function

            Public Sub Refresh(iRefreshType As RefreshFlags)
                If ((iRefreshType And RefreshFlags.RefreshType_Init) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMTrackerList)))
                    Try
                        If (CType(PInvoke.PSM_GetTrackerList(hPtr, PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMTrackerList)(hPtr)

                            For i = 0 To mData.count - 1
                                If (mData.trackers(i).tracker_id = m_TrackerId) Then
                                    SetTrackerType(mData.trackers(i).tracker_type)
                                    SetTrackerDriver(mData.trackers(i).tracker_Driver)
                                    SetTrackerDevicePath(mData.trackers(i).device_path)
                                    SetTrackerSharedMemoryName(mData.trackers(i).shared_memory_name)
                                    SetTrackerView(mData.trackers(i))
                                    SetTrackerPose(mData.trackers(i))

                                    SetTrackerInitalized()
                                    Exit For
                                End If
                            Next

                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If

                If ((iRefreshType And RefreshFlags.RefreshType_Stats) > 0) Then
                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMTracker)))
                    Try
                        If (CType(PInvoke.PSM_GetTrackerEx(m_TrackerId, hPtr), PSMResult) = PSMResult.PSMResult_Success) Then
                            Dim mData = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMTracker)(hPtr)

                            g_Stats = New PSStats(mData)
                        End If
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                End If
            End Sub
        End Class

        Public Shared Function GetTrackerList() As Trackers()
            Dim mTrackers As New List(Of Trackers)

            Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMTrackerList)))
            Try
                If (CType(PInvoke.PSM_GetTrackerList(hPtr, PSM_DEFAULT_TIMEOUT), PSMResult) = PSMResult.PSMResult_Success) Then
                    Dim mTrackerList = Marshal.PtrToStructure(Of PInvoke.PINVOKE_PSMTrackerList)(hPtr)

                    For i = 0 To mTrackerList.count - 1
                        Dim mTracker As New Trackers(
                            mTrackerList.trackers(i).tracker_id,
                            mTrackerList.trackers(i)
                        )

                        mTrackers.Add(mTracker)
                    Next
                End If
            Finally
                Marshal.FreeHGlobal(hPtr)
            End Try

            Return mTrackers.ToArray
        End Function

        Property m_Listening As Boolean
            Get
                Return g_bListening
            End Get
            Set
                If (g_bListening = Value) Then
                    Return
                End If

                g_bListening = Value

                ServiceExceptions.ThrowExceptionOnServiceStatus()

                If (g_bListening) Then
                    Dim mTrackerInfo = m_Info.m_View.GetRaw()

                    Dim hPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(PInvoke.PINVOKE_PSMClientTrackerInfo)))
                    Marshal.StructureToPtr(m_Info.m_View.GetRaw(), hPtr, True)
                    Try
                        ServiceExceptions.ThrowExceptionServiceRequest("PSM_AllocateTrackerListener failed", PInvoke.PSM_AllocateTrackerListener(g_mInfo.m_TrackerId, hPtr))
                    Finally
                        Marshal.FreeHGlobal(hPtr)
                    End Try
                Else
                    ServiceExceptions.ThrowExceptionServiceRequest("PSM_FreeTrackerListener failed", PInvoke.PSM_FreeTrackerListener(g_mInfo.m_TrackerId))
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

                ServiceExceptions.ThrowExceptionOnServiceStatus()

                If (g_bDataStream) Then
                    ServiceExceptions.ThrowExceptionServiceRequest("PSM_StartTrackerDataStream failed", PInvoke.PSM_StartTrackerDataStream(g_mInfo.m_TrackerId, PSM_DEFAULT_TIMEOUT))
                Else
                    ServiceExceptions.ThrowExceptionServiceRequest("PSM_StopTrackerDataStream failed", PInvoke.PSM_StopTrackerDataStream(g_mInfo.m_TrackerId, PSM_DEFAULT_TIMEOUT))
                End If
            End Set
        End Property

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then

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
