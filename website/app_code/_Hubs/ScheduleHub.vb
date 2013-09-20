Imports Microsoft.VisualBasic
Imports Microsoft.AspNet.SignalR.Hubs
Imports UserSystem

Public Class ScheduleHub
    Inherits Hub

    Public Shared Function getClients() As IHubConnectionContext
        Return Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of ScheduleHub)().Clients
    End Function

    Public Function getUsernameOfSessionUser() As String
        Dim user As FrontPageUser = Connections.getSessionUser
        If user Is Nothing Then
            Return "NO SESSION"
        Else
            Return "SESSION: " & user.name + "<br />IP: " + user.ipAddress

        End If
    End Function

    Public Function initializeConnection() As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing Then
            If user.privileges.canAdminSchedule Then
                Clients.Caller.setScheduleEditable("[all]")
            ElseIf user.privileges.canEditSchedule Then
                Clients.Caller.setScheduleEditable(user.name)
            End If
            Return True
        Else
            Return False
        End If
    End Function

    Public Function queryEvents(startTime As Date, endTime As Date) As ScheduleEvent.InnerEventObject()
        Return ScheduleProcessor.getEvents(startTime, endTime)
    End Function

    Public Function addEvent(start As Date, ends As Date, description As String) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canEditSchedule Then
            Dim result As Boolean = ScheduleProcessor.addEvent(user, start, ends, description)
            If result Then
                Clients.All.refreshEvents()
            End If
            Return result
        End If
        Return False
    End Function

    Public Function updateEventTime(eventID As Integer, startTime As Date, endTime As Date) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canEditSchedule Then
            Dim result As Boolean = ScheduleProcessor.editEvent(user, eventID, startTime, endTime)
            If result Then
                Clients.All.editedEventTime(eventID, Utils.getUNIXTimestamp(startTime), Utils.getUNIXTimestamp(endTime))
            End If
            Return result
        End If
        Return False
    End Function

    Public Function updateEventDescription(eventID As Integer, description As String) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canEditSchedule Then
            Dim result As String = ScheduleProcessor.editEvent(user, eventID, description)
            If result IsNot Nothing Then
                Clients.All.editedEventDescription(eventID, result)
                Return True
            End If
        End If
        Return False
    End Function

    Public Function deleteEvent(eventID As Integer) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canEditSchedule Then
            Dim result As Boolean = ScheduleProcessor.deleteEvent(user, eventID)
            If result Then
                Clients.All.deletedEvent(eventID)
            End If
            Return result
        End If
        Return False
    End Function

    Public Sub serverPing()
        Clients.Caller.clientPing()
    End Sub

End Class
