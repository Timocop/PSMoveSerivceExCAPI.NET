Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Module Tests
    Dim mControllers As Controllers() = Nothing
    Dim mHmds As HeadMountedDevices() = Nothing
    Dim mTrackers As Trackers() = Nothing

    Dim sHost As String = "127.0.0.1"

    Sub Main()
        Try
            While True
                Dim sLine As String

                Console.WriteLine("Listen to [T]racker, [C]ontroller or [H]head-mounted Device:")
                sLine = Console.ReadLine()

                Select Case (sLine.ToLowerInvariant)
                    Case "t"
                        Console.WriteLine("Tracker to listen [0-7]:")
                        sLine = Console.ReadLine()

                        If (String.IsNullOrEmpty(sLine)) Then
                            Return
                        End If

                        Dim iNum As Integer
                        If (Not Integer.TryParse(sLine, iNum)) Then
                            Return
                        End If

                        Console.WriteLine("Enter remote IP (leave blank for localhost): ")
                        sLine = Console.ReadLine()

                        If (Not String.IsNullOrEmpty(sLine)) Then
                            sHost = sLine
                        End If

                        DoTrackers(iNum)
                    Case "c"
                        Console.WriteLine("Controller to listen [0-9]:")
                        sLine = Console.ReadLine()

                        If (String.IsNullOrEmpty(sLine)) Then
                            Return
                        End If

                        Dim iNum As Integer
                        If (Not Integer.TryParse(sLine, iNum)) Then
                            Return
                        End If

                        Console.WriteLine("Enter remote IP (leave blank for localhost): ")
                        sLine = Console.ReadLine()

                        If (Not String.IsNullOrEmpty(sLine)) Then
                            sHost = sLine
                        End If

                        DoControllers(iNum)

                    Case "h"
                        Console.WriteLine("HMDs to listen [0-9]:")
                        sLine = Console.ReadLine()

                        If (String.IsNullOrEmpty(sLine)) Then
                            Return
                        End If

                        Dim iNum As Integer
                        If (Not Integer.TryParse(sLine, iNum)) Then
                            Return
                        End If

                        Console.WriteLine("Enter remote IP (leave blank for localhost): ")
                        sLine = Console.ReadLine()

                        If (Not String.IsNullOrEmpty(sLine)) Then
                            sHost = sLine
                        End If

                        DoHeadMountedDevices(iNum)
                End Select

                Console.ReadLine()
            End While

        Catch ex As Exception
            Console.WriteLine("ERROR: " & ex.Message)
            Threading.Thread.Sleep(5000)
        End Try
    End Sub

    Private Sub DoControllers(iListenController As Integer)
        Using mService As New Service(sHost)
            mService.Connect()

            ' Get new list of controllers.
            mControllers = Controllers.GetControllerList()

            If (iListenController < 0 OrElse iListenController > mControllers.Length - 1) Then
                Throw New ArgumentException("Controller id out of range")
            End If

            If (mControllers IsNot Nothing) Then
                Dim mController As Controllers = mControllers(iListenController)

                ' Setup streaming flags we need.
                mController.m_DataStreamFlags =
                        PSMStreamFlags.PSMStreamFlags_includeCalibratedSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includePhysicsData Or
                        PSMStreamFlags.PSMStreamFlags_includePositionData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawTrackerData

                ' Enable and start listening to the stream.
                mController.m_Listening = True
                mController.m_DataStreamEnabled = True

                ' Start tracker data stream for this controller.
                ' This is never needed unless you want to get the projection from the camera.
                ' Only one tracker stream per controller is supported.
                mController.SetTrackerStream(0)

                While True
                    ' Poll changes and refresh controller data.
                    ' Use 'RefreshFlags' to optimize what you need to reduce API calls.
                    mService.Update()

                    If (mService.HasControllerListChanged) Then
                        Throw New ArgumentException("Controller list has changed")
                    End If

                    mController.Refresh(Controllers.Info.RefreshFlags.RefreshType_All)

                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("m_ControllerId: " & mController.m_Info.m_ControllerId)
                    Console.WriteLine("m_ControllerType: " & mController.m_Info.m_ControllerType.ToString)
                    Console.WriteLine("m_ControllerHand: " & mController.m_Info.m_ControllerHand.ToString)

                    Console.WriteLine("m_ControllerSerial: " & mController.m_Info.m_ControllerSerial)
                    Console.WriteLine("m_ParentControllerSerial: " & mController.m_Info.m_ParentControllerSerial)
                    Console.WriteLine("IsControllerStable: " & mController.IsControllerStable())

                    If (mController.m_Info.IsStateValid()) Then
                        ' Get PSMove stuff
                        If (mController.m_Info.m_ControllerType = PSMControllerType.PSMController_Move) Then
                            Dim mPSMoveState = mController.m_Info.GetPSState(Of Controllers.Info.PSMoveState)

                            Console.WriteLine("m_TrackingColorType: " & mPSMoveState.m_TrackingColorType.ToString)

                            Console.WriteLine("m_TriangleButton: " & mPSMoveState.m_TriangleButton.ToString)
                            Console.WriteLine("m_CircleButton: " & mPSMoveState.m_CircleButton.ToString)
                            Console.WriteLine("m_CrossButton: " & mPSMoveState.m_CrossButton.ToString)
                            Console.WriteLine("m_SquareButton: " & mPSMoveState.m_SquareButton.ToString)
                            Console.WriteLine("m_SelectButton: " & mPSMoveState.m_SelectButton.ToString)
                            Console.WriteLine("m_StartButton: " & mPSMoveState.m_StartButton.ToString)
                            Console.WriteLine("m_PSButton: " & mPSMoveState.m_PSButton.ToString)
                            Console.WriteLine("m_MoveButton: " & mPSMoveState.m_MoveButton.ToString)
                            Console.WriteLine("m_TriggerButton: " & mPSMoveState.m_TriggerButton.ToString)

                            Console.WriteLine("m_BatteryValue: " & mPSMoveState.m_BatteryValue.ToString)
                            Console.WriteLine("m_TriggerValue: " & mPSMoveState.m_TriggerValue)

                            Console.WriteLine("m_bIsCurrentlyTracking: " & mPSMoveState.m_bIsCurrentlyTracking)
                            Console.WriteLine("m_IsPositionValid: " & mPSMoveState.m_IsPositionValid)
                            Console.WriteLine("m_IsOrientationValid: " & mPSMoveState.m_IsOrientationValid)
                        End If
                    End If

                    If (mController.m_Info.IsPoseValid()) Then
                        Console.WriteLine("m_Position.x: " & mController.m_Info.m_Pose.m_Position.x)
                        Console.WriteLine("m_Position.y: " & mController.m_Info.m_Pose.m_Position.y)
                        Console.WriteLine("m_Position.z: " & mController.m_Info.m_Pose.m_Position.z)

                        Console.WriteLine("m_Orientation.x: " & mController.m_Info.m_Pose.m_Orientation.x)
                        Console.WriteLine("m_Orientation.y: " & mController.m_Info.m_Pose.m_Orientation.y)
                        Console.WriteLine("m_Orientation.z: " & mController.m_Info.m_Pose.m_Orientation.z)
                        Console.WriteLine("m_Orientation.w: " & mController.m_Info.m_Pose.m_Orientation.w)
                    End If

                    If (mController.m_Info.IsSensorValid()) Then
                        Console.WriteLine("m_Gyroscope.x: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.x)
                        Console.WriteLine("m_Gyroscope.y: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.y)
                        Console.WriteLine("m_Gyroscope.z: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.z)

                        Console.WriteLine("m_Magnetometer.x: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.x)
                        Console.WriteLine("m_Magnetometer.y: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.y)
                        Console.WriteLine("m_Magnetometer.z: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.z)

                        Console.WriteLine("m_Accelerometer.x: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.x)
                        Console.WriteLine("m_Accelerometer.y: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.y)
                        Console.WriteLine("m_Accelerometer.z: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.z)
                    End If

                    If (mController.m_Info.IsTrackingValid()) Then
                        If (mController.m_Info.m_PSTracking.m_Shape = PSMShape.PSMShape_Ellipse) Then
                            Dim mProjection = mController.m_Info.m_PSTracking.GetTrackingProjection(Of Controllers.Info.PSTracking.PSMTrackingProjectionEllipse)

                            Console.WriteLine("mCenter.x: " & mProjection.mCenter.x)
                            Console.WriteLine("mCenter.y: " & mProjection.mCenter.y)
                        End If
                    End If

                    Threading.Thread.Sleep(100)

                    'GC.Collect()
                    'GC.WaitForPendingFinalizers()
                End While
            End If
        End Using
    End Sub

    Private Sub DoHeadMountedDevices(iListenHMD As Integer)
        Using mService As New Service(sHost)
            mService.Connect()

            ' Get new list of hmds
            mHmds = HeadMountedDevices.GetHmdList()

            If (iListenHMD < 0 OrElse iListenHMD > mHmds.Length - 1) Then
                Throw New ArgumentException("HMD id out of range")
            End If

            If (mHmds IsNot Nothing) Then
                Dim mHmd As HeadMountedDevices = mHmds(iListenHMD)

                ' Setup streaming flags we need.
                mHmd.m_DataStreamFlags =
                        PSMStreamFlags.PSMStreamFlags_includeCalibratedSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includePhysicsData Or
                        PSMStreamFlags.PSMStreamFlags_includePositionData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawTrackerData

                ' Enable and start listening to the stream.
                mHmd.m_Listening = True
                mHmd.m_DataStreamEnabled = True

                ' Start tracker data stream for this hmd.
                ' This is never needed unless you want to get the projection from the camera.
                ' Only one tracker stream per hmd is supported.
                mHmd.SetTrackerStream(0)

                While True
                    ' Poll changes and refresh hmd data.
                    ' Use 'RefreshFlags' to optimize what you need to reduce API calls.
                    mService.Update()

                    If (mService.HasHMDListChanged) Then
                        Throw New ArgumentException("Hmd list has changed")
                    End If

                    mHmd.Refresh(HeadMountedDevices.Info.RefreshFlags.RefreshType_All)

                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("m_HmdId: " & mHmd.m_Info.m_HmdId)
                    Console.WriteLine("m_HmdType: " & mHmd.m_Info.m_HmdType.ToString)

                    Console.WriteLine("m_HmdSerial: " & mHmd.m_Info.m_HmdSerial)
                    Console.WriteLine("IsHmdStable: " & mHmd.IsHmdStable())

                    If (mHmd.m_Info.IsStateValid()) Then
                        Select Case (mHmd.m_Info.m_HmdType)
                            Case PSMHmdType.PSMHmd_Morpheus
                                Dim mPSMorpheusState = mHmd.m_Info.GetPSState(Of HeadMountedDevices.Info.PSMorpheusState)

                                Console.WriteLine("m_IsCurrentlyTracking: " & mPSMorpheusState.m_IsCurrentlyTracking)
                                Console.WriteLine("m_IsPositionValid: " & mPSMorpheusState.m_IsPositionValid)
                                Console.WriteLine("m_IsOrientationValid: " & mPSMorpheusState.m_IsOrientationValid)
                                Console.WriteLine("m_IsTrackingEnabled: " & mPSMorpheusState.m_IsTrackingEnabled)
                            Case PSMHmdType.PSMHmd_Virtual
                                Dim mPSVirtualHmdState = mHmd.m_Info.GetPSState(Of HeadMountedDevices.Info.PSVirtualHmdState)

                                Console.WriteLine("m_bIsCurrentlyTracking: " & mPSVirtualHmdState.m_IsCurrentlyTracking)
                                Console.WriteLine("m_IsPositionValid: " & mPSVirtualHmdState.m_IsPositionValid)
                                Console.WriteLine("m_IsOrientationValid: " & mPSVirtualHmdState.m_IsTrackingEnabled)
                        End Select
                    End If

                    If (mHmd.m_Info.IsPoseValid()) Then
                        Console.WriteLine("m_Position.x: " & mHmd.m_Info.m_Pose.m_Position.x)
                        Console.WriteLine("m_Position.y: " & mHmd.m_Info.m_Pose.m_Position.y)
                        Console.WriteLine("m_Position.z: " & mHmd.m_Info.m_Pose.m_Position.z)

                        Console.WriteLine("m_Orientation.x: " & mHmd.m_Info.m_Pose.m_Orientation.x)
                        Console.WriteLine("m_Orientation.y: " & mHmd.m_Info.m_Pose.m_Orientation.y)
                        Console.WriteLine("m_Orientation.z: " & mHmd.m_Info.m_Pose.m_Orientation.z)
                        Console.WriteLine("m_Orientation.w: " & mHmd.m_Info.m_Pose.m_Orientation.w)
                    End If

                    If (mHmd.m_Info.IsSensorValid()) Then
                        Console.WriteLine("m_Gyroscope.x: " & mHmd.m_Info.m_PSCalibratedSensor.m_Gyroscope.x)
                        Console.WriteLine("m_Gyroscope.y: " & mHmd.m_Info.m_PSCalibratedSensor.m_Gyroscope.y)
                        Console.WriteLine("m_Gyroscope.z: " & mHmd.m_Info.m_PSCalibratedSensor.m_Gyroscope.z)

                        Console.WriteLine("m_Magnetometer.x: " & mHmd.m_Info.m_PSCalibratedSensor.m_Magnetometer.x)
                        Console.WriteLine("m_Magnetometer.y: " & mHmd.m_Info.m_PSCalibratedSensor.m_Magnetometer.y)
                        Console.WriteLine("m_Magnetometer.z: " & mHmd.m_Info.m_PSCalibratedSensor.m_Magnetometer.z)

                        Console.WriteLine("m_Accelerometer.x: " & mHmd.m_Info.m_PSCalibratedSensor.m_Accelerometer.x)
                        Console.WriteLine("m_Accelerometer.y: " & mHmd.m_Info.m_PSCalibratedSensor.m_Accelerometer.y)
                        Console.WriteLine("m_Accelerometer.z: " & mHmd.m_Info.m_PSCalibratedSensor.m_Accelerometer.z)
                    End If

                    If (mHmd.m_Info.IsTrackingValid()) Then
                        If (mHmd.m_Info.m_PSTracking.m_Shape = PSMShape.PSMShape_Ellipse) Then
                            Dim mProjection = mHmd.m_Info.m_PSTracking.GetTrackingProjection(Of HeadMountedDevices.Info.PSTracking.PSMTrackingProjectionEllipse)

                            Console.WriteLine("mCenter.x: " & mProjection.mCenter.x)
                            Console.WriteLine("mCenter.y: " & mProjection.mCenter.y)
                        End If
                    End If

                    Threading.Thread.Sleep(100)

                    'GC.Collect()
                    'GC.WaitForPendingFinalizers()
                End While
            End If
        End Using
    End Sub

    Private Sub DoTrackers(iListenTracker As Integer)
        Using mService As New Service(sHost)
            mService.Connect()

            ' Get new list of controllers
            mTrackers = Trackers.GetTrackerList()

            If (iListenTracker < 0 OrElse iListenTracker > mTrackers.Length - 1) Then
                Throw New ArgumentException("Controller id out of range")
            End If

            If (mTrackers IsNot Nothing) Then
                Dim mTracker As Trackers = mTrackers(iListenTracker)

                ' Not really needed unless you grab stats from the tracker.
                mTracker.m_Listening = True
                mTracker.m_DataStreamEnabled = True

                mTracker.Refresh(Trackers.Info.RefreshFlags.RefreshType_Init)

                While True
                    ' Poll changes and refresh tracker data.
                    ' Use 'RefreshFlags' to optimize what you need to reduce API calls.
                    mService.Update()

                    If (mService.HasTrackerListChanged) Then
                        Throw New ArgumentException("Tracker list has changed")
                    End If

                    ' Tracker pose does not update with stream.
                    If (mService.HasPlayspaceOffsetChanged) Then
                        mTracker.Refresh(Trackers.Info.RefreshFlags.RefreshType_Init)
                        Console.WriteLine("Playsapce offsets have changed")
                    End If


                    mTracker.Refresh(Trackers.Info.RefreshFlags.RefreshType_Stats)

                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("m_TrackerId: " & mTracker.m_Info.m_TrackerId)
                    Console.WriteLine("m_TrackerType: " & mTracker.m_Info.m_TrackerType.ToString)
                    Console.WriteLine("m_TrackerDrvier: " & mTracker.m_Info.m_TrackerDrvier.ToString)
                    Console.WriteLine("m_DevicePath: " & mTracker.m_Info.m_DevicePath)
                    Console.WriteLine("m_SharedMemoryName: " & mTracker.m_Info.m_SharedMemoryName)

                    Console.WriteLine("m_IsConnected: " & mTracker.m_Info.m_Stats.m_IsConnected)
                    Console.WriteLine("m_ListenerCount: " & mTracker.m_Info.m_Stats.m_ListenerCount)
                    Console.WriteLine("m_SequenceNum: " & mTracker.m_Info.m_Stats.m_SequenceNum)
                    Console.WriteLine("m_DataFrameAverageFps: " & mTracker.m_Info.m_Stats.m_DataFrameAverageFps)

                    Console.WriteLine("m_TrackerExposure: " & mTracker.m_Info.m_Stats.m_TrackerExposure)
                    Console.WriteLine("m_TrackerGain: " & mTracker.m_Info.m_Stats.m_TrackerGain)
                    Console.WriteLine("m_TrackerWidth: " & mTracker.m_Info.m_Stats.m_TrackerWidth)

                    Console.WriteLine("m_DataFrameLastReceivedTime: " & mTracker.m_Info.m_Stats.m_DataFrameLastReceivedTime.ToString)

                    If (mTracker.m_Info.IsPoseValid()) Then
                        Console.WriteLine("m_Position.x: " & mTracker.m_Info.m_Pose.m_Position.x)
                        Console.WriteLine("m_Position.y: " & mTracker.m_Info.m_Pose.m_Position.y)
                        Console.WriteLine("m_Position.z: " & mTracker.m_Info.m_Pose.m_Position.z)

                        Console.WriteLine("m_Orientation.x: " & mTracker.m_Info.m_Pose.m_Orientation.x)
                        Console.WriteLine("m_Orientation.y: " & mTracker.m_Info.m_Pose.m_Orientation.y)
                        Console.WriteLine("m_Orientation.z: " & mTracker.m_Info.m_Pose.m_Orientation.z)
                        Console.WriteLine("m_Orientation.w: " & mTracker.m_Info.m_Pose.m_Orientation.w)
                    End If

                    If (mTracker.m_Info.IsViewValid()) Then
                        Console.WriteLine("tracker_screen_dimensions.x: " & mTracker.m_Info.m_View.tracker_screen_dimensions.x)
                        Console.WriteLine("tracker_screen_dimensions.y: " & mTracker.m_Info.m_View.tracker_screen_dimensions.y)
                    End If

                    Threading.Thread.Sleep(1000)

                    'GC.Collect()
                    'GC.WaitForPendingFinalizers()
                End While
            End If
        End Using
    End Sub
End Module
