Imports System.Threading
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Module Module1
    Sub Main()
        While True
            Try
                Using mService As New PSMoveServiceExCAPI.PSMoveServiceExCAPI.Service()
                    mService.Connect()

                    Dim mController As New PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers(0) _
                            With {
                            .m_DataStreamFlags =
                            PSMStreamFlags.PSMStreamFlags_includeCalibratedSensorData Or
                            PSMStreamFlags.PSMStreamFlags_includePhysicsData Or
                            PSMStreamFlags.PSMStreamFlags_includePositionData Or
                            PSMStreamFlags.PSMStreamFlags_includeRawSensorData Or
                            PSMStreamFlags.PSMStreamFlags_includeRawTrackerData,
                            .m_Listening = True,
                            .m_DataStreamEnabled = True
                            }

                    mController.SetTrackerStream(0)

                    While True
                        mService.Update()
                        mController.Refresh(PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers.Info.RefreshFlags.RefreshType_All)

                        Console.WriteLine(" --------------------------------- ")
                        Console.WriteLine("Version: " & mService.GetClientProtocolVersion())
                        Console.WriteLine("GetValidControllerCount: " & PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers.GetValidControllerCount())
                        Console.WriteLine("m_ControllerType: " & mController.m_Info.m_ControllerType)

                        Console.WriteLine("m_StartButton: " & If(mController.m_Info.m_PSMoveState?.m_StartButton, PSMButtonState.PSMButtonState_DOWN))
                        Console.WriteLine("m_bIsCurrentlyTracking: " & If(mController.m_Info.m_PSMoveState?.m_bIsCurrentlyTracking, False))
                        Console.WriteLine("m_ControllerSerial: " & mController.m_Info.m_ControllerSerial)

                        Console.WriteLine("IsControllerStable: " & mController.IsControllerStable())

                        If (mController.m_Info.IsPoseValid) Then
                            Console.WriteLine("m_Position.x: " & mController.m_Info.m_Pose.m_Position.x)
                            Console.WriteLine("m_Position.y: " & mController.m_Info.m_Pose.m_Position.y)
                            Console.WriteLine("m_Position.z: " & mController.m_Info.m_Pose.m_Position.z)
                        End If

                        If (mController.m_Info.IsSensorValid) Then
                            Console.WriteLine("m_Gyroscope.x: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.x)
                            Console.WriteLine("m_Gyroscope.y: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.y)
                            Console.WriteLine("m_Gyroscope.z: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.z)
                        End If

                        If (mController.m_Info.IsSensorValid) Then
                            Console.WriteLine("m_Accelerometer.x: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.x)
                            Console.WriteLine("m_Accelerometer.y: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.y)
                            Console.WriteLine("m_Accelerometer.z: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.z)
                        End If

                        If (mController.m_Info.IsTrackingValid() AndAlso
                            mController.m_Info.m_PSTracking.m_Shape = PSMShape.PSMShape_Ellipse) Then
                            Console.WriteLine("mCenter.x: " & mController.m_Info.m_PSTracking.GetTrackingProjection _
                                    (Of PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers.Info.PSTracking.PSMTrackingProjectionEllipse).mCenter.x)
                            Console.WriteLine("mCenter.y: " & mController.m_Info.m_PSTracking.GetTrackingProjection _
                                    (Of PSMoveServiceExCAPI.PSMoveServiceExCAPI.Controllers.Info.PSTracking.PSMTrackingProjectionEllipse).mCenter.y)
                        End If

                        Thread.Sleep(1000)

                        'GC.Collect()
                        'GC.WaitForPendingFinalizers()
                    End While
                End Using

            Catch ex As Exception
                Console.WriteLine("ERROR: " & ex.Message)
                Thread.Sleep(5000)
            End Try
        End While
    End Sub
End Module
