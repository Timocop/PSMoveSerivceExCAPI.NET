Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers.Info

Module Tests
    Sub Main()
        Try
            Dim mControllers As Controllers() = Nothing
            Dim iListenController As Integer = -1

            Using mService As New Service()
                mService.Connect()

                ' Get new list of controllers
                mControllers = Controllers.GetControllerList()

                ' Controller list changed, try getting the serial from the controllers.
                ' This is Networked and should only be used once.
                For i = 0 To mControllers.Length - 1
                    mControllers(i).Refresh(RefreshFlags.RefreshType_ProbeSerial)
                Next

                While True
                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("Version: " & mService.GetClientProtocolVersion())
                    Console.WriteLine("Total Controllers: " & mControllers.Length)
                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("[0-9] Controller to listen; [X] to exit")
                    Dim sLine = Console.ReadLine()

                    If (String.IsNullOrEmpty(sLine)) Then
                        Continue While
                    End If

                    If (sLine.ToLowerInvariant = "x") Then
                        Return
                    End If

                    Dim iNum As Integer
                    If (Not Integer.TryParse(sLine, iNum)) Then
                        Continue While
                    End If

                    iListenController = iNum

                    If (iListenController < 0 OrElse iListenController > mControllers.Length - 1) Then
                        Continue While
                    End If

                    Exit While
                End While

                If (mControllers IsNot Nothing) Then
                    Dim mController As Controllers = mControllers(iListenController)

                    ' Setup streaming flags we need.
                    mController.m_DataStreamFlags =
                            PSMStreamFlags.PSMStreamFlags_includeCalibratedSensorData Or
                            PSMStreamFlags.PSMStreamFlags_includePhysicsData Or
                            PSMStreamFlags.PSMStreamFlags_includePositionData Or
                            PSMStreamFlags.PSMStreamFlags_includeRawSensorData Or
                            PSMStreamFlags.PSMStreamFlags_includeRawTrackerData

                    ' Enable and start listening to the stream
                    mController.m_Listening = True
                    mController.m_DataStreamEnabled = True

                    ' Start tracker data stream for this controller
                    ' This is never needed unless you want to get the projection from the camera
                    ' Only one tracker stream per controller is supported
                    mController.SetTrackerStream(0)

                    While True
                        ' Poll changes and refresh controller data
                        ' Use 'RefreshFlags' to optimize what you need to reduce API calls
                        mService.Update()

                        If (mService.HasControllerListChanged) Then
                            Throw New ArgumentException("Controller list has changed")
                        End If

                        mController.Refresh(RefreshFlags.RefreshType_All)

                        Console.WriteLine(" --------------------------------- ")
                        Console.WriteLine("Controller ID: " & mController.m_Info.m_ControllerId)
                        Console.WriteLine("m_ControllerType: " & mController.m_Info.m_ControllerType.ToString)

                        Console.WriteLine("m_ControllerSerial: " & mController.m_Info.m_ControllerSerial)
                        Console.WriteLine("IsControllerStable: " & mController.IsControllerStable())

                        If (mController.m_Info.IsStateValid()) Then
                            ' Get PSMove stuff
                            If (mController.m_Info.m_ControllerType = PSMControllerType.PSMController_Move) Then
                                Dim mPSMoveState = mController.m_Info.GetPSState(Of PSMoveState)

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
                            End If
                        End If

                        If (mController.m_Info.IsPoseValid()) Then
                            Console.WriteLine("m_Position.x: " & mController.m_Info.m_Pose.m_Position.x)
                            Console.WriteLine("m_Position.y: " & mController.m_Info.m_Pose.m_Position.y)
                            Console.WriteLine("m_Position.z: " & mController.m_Info.m_Pose.m_Position.z)
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
                                Dim mProjection = mController.m_Info.m_PSTracking.GetTrackingProjection(Of PSTracking.PSMTrackingProjectionEllipse)

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

        Catch ex As Exception
            Console.WriteLine("ERROR: " & ex.Message)
            Threading.Thread.Sleep(5000)
        End Try
    End Sub

End Module
