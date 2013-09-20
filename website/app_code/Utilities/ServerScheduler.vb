Imports Microsoft.VisualBasic

Public Class ServerScheduler

    Private Shared started As Boolean
    Private Shared timer1 As Threading.Timer
    Private Shared timer2 As Threading.Timer
    Private Shared callback1 As Threading.TimerCallback
    Private Shared callback2 As Threading.TimerCallback

    Public Shared Sub start()
        If Not started Then
            started = True
            Dim nextFive = Now.Minute - (Now.Minute Mod 5) + 5
            Dim closestFiveMinutes = New DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Now.Hour, 0, 0)
            If nextFive = 60 Then
                closestFiveMinutes = closestFiveMinutes.AddHours(1)
            Else
                closestFiveMinutes = closestFiveMinutes.AddMinutes(nextFive)
            End If
            callback1 = New System.Threading.TimerCallback(AddressOf firedEveryFiveMinutes)
            timer1 = New Threading.Timer(callback1, Nothing, closestFiveMinutes - Now, TimeSpan.FromMinutes(5))
            callback2 = New System.Threading.TimerCallback(AddressOf firedEvery30Seconds)
            timer2 = New Threading.Timer(callback2, Nothing, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30))
        End If
    End Sub

    Public Shared Sub firedEvery30Seconds()

    End Sub

    Public Shared Sub firedEveryFiveMinutes(stateInfo As Object)
        'If there is an event associated with this stream, then see if it expired (if so extend it)
        If StreamProcessor.isLive Then
            If StreamProcessor.streamEvent IsNot Nothing AndAlso Not StreamProcessor.streamEvent.isDeleted Then
                Dim eventToExtend As ScheduleEvent = StreamProcessor.streamEvent
                If Date.Compare(Now.ToUniversalTime.AddMinutes(5), StreamProcessor.streamEvent.endTime) > 0 Then
                    ScheduleProcessor.extendEvent(eventToExtend, 5)
                End If
            ElseIf StreamProcessor.streamEvent Is Nothing Then
                Dim currentEvent As ScheduleEvent = ScheduleProcessor.findCurrentEvent()
                If currentEvent IsNot Nothing Then
                    'TODO: StreamProcessor.updateEvent(currentEvent)
                End If
            End If
        End If
        FrontPageHub.getClients.All.checkConnection()
        Dim userCountUpdated As Boolean = False
        For Each user As UserSystem.FrontPageUser In UserSystem.frontPageUsers.ToList
            If user.connectionCheckFailed() Then
                UserSystem.Connections.frontPageUsers.Remove(user)
                userCountUpdated = True
                Try
                    user.client.jumpToDisconnect()
                Catch ex As Exception
                End Try
            End If
        Next
        If userCountUpdated Then
            FrontPageHub.getClients.All.setUserCount(UserSystem.Connections.frontPageUsers.Count)
        End If
    End Sub
    Dim a As Integer = 0

End Class
