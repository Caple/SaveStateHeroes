Imports Microsoft.VisualBasic
Imports UserSystem
Imports System.Xml

Public Class StreamProcessor

    Public Shared Sub init()
        ServerPersistance.setDefault("modeChangedOn", Now)
        ServerPersistance.setDefault("mode", "youtube")
        ServerPersistance.setDefault("details", "Unknown Game")
        ServerPersistance.setDefault("streamerName", String.Empty)
        _modeChangedOn = ServerPersistance.getField("modeChangedOn")
        _mode = ServerPersistance.getField("mode")
        _details = ServerPersistance.getField("details")
        If mode <> "youtube" Then
            _isLive = True
        ElseIf mode = "youtube" Then
            _isLive = False
        End If
        Dim streamerName As String = ServerPersistance.getField("streamerName")
        If Not String.IsNullOrEmpty(streamerName) Then
            For Each account As AccountDetails In Accounts.matchSSHAccounts(streamerName)
                Dim user As New OfflineUser(account.ipAddress)
                user.bindAccount(account)
                _streamer = user
            Next
        Else
            _streamer = Nothing
        End If
        _streamEvent = ScheduleProcessor.findCurrentEvent()
    End Sub

    Public Shared Sub persist()
        ServerPersistance.setField("modeChangedOn", _modeChangedOn)
        ServerPersistance.setField("mode", _mode)
        ServerPersistance.setField("details", _details)
        If (streamer IsNot Nothing) Then
            ServerPersistance.setField("streamerName", _streamer.name)
        Else
            ServerPersistance.setField("streamerName", String.Empty)
        End If
    End Sub

    Public Shared Function setMode(user As OnlineUser, newMode As String, newDetails As String, socialMessage As String) As Boolean
        If Not user.privileges.isStreamer AndAlso Not user.privileges.isOfficer Then Return False

        Dim matchingAccounts As List(Of UserSystem.AccountDetails) = Accounts.matchSSHAccounts(user.name)
        If matchingAccounts.Count < 1 Then Return False
        Dim matchingAccount = matchingAccounts(0)
        Dim offlineUser As New OfflineUser(matchingAccount.ipAddress)
        offlineUser.bindAccount(matchingAccount)

        If mode <> "youtube" Then
            If (newMode <> "youtube") Then Return False
            If (Not user.privileges.isOfficer AndAlso user <> _streamer) Then Return False
            _isLive = False
        ElseIf mode = "youtube" Then
            If (newMode = "youtube") Then Return False
            _isLive = True
        Else
            Return False
        End If

        _modeChangedOn = Now
        _mode = newMode
        _details = newDetails
        FrontPageHub.getClients().All.switchVideoMode(user.displayName, mode, details)

        If mode = "custom" Then
            _streamer = offlineUser
            AutopilotHub.videoTimer.Stop()
            AutopilotHub.freezeTrackBar()

            Dim newEvent = ScheduleProcessor.findCurrentEvent()
            If newEvent Is Nothing OrElse newEvent.isDeleted Then
                Dim newStartDate = New Date(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute - (Now.Minute Mod 5), 0).ToUniversalTime
                Dim newEndDate = newStartDate.AddMinutes(5)
                ScheduleProcessor.addEvent(user, newStartDate, newEndDate, "Special Live Event")
                newEvent = ScheduleProcessor.findCurrentEvent()
            End If
            _streamEvent = newEvent

            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.NotificationRawHTML, "A special live event has started.")
            Dim onlineStreamer As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, user.name)
            If onlineStreamer IsNot Nothing Then
                onlineStreamer.client.updateModStatus(True)
            End If
            For Each nextUser As FrontPageUser In Connections.frontPageUsers.ToList
                If nextUser.privileges.isStreamer AndAlso Not nextUser.privileges.isOfficer Then
                    If (nextUser <> _streamer) Then
                        nextUser.client.updateStreamButtons("none")
                    Else
                        nextUser.client.updateStreamButtons("end")
                    End If
                ElseIf (nextUser.privileges.isOfficer) Then
                    nextUser.client.updateStreamButtons("end")
                End If
            Next
        ElseIf mode <> "youtube" Then
            _streamer = offlineUser
            AutopilotHub.videoTimer.Stop()
            AutopilotHub.freezeTrackBar()

            Dim newEvent = ScheduleProcessor.findCurrentEvent()
            If newEvent Is Nothing OrElse newEvent.isDeleted Then
                Dim newStartDate = New Date(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute - (Now.Minute Mod 5), 0).ToUniversalTime
                Dim newEndDate = newStartDate.AddMinutes(5)
                ScheduleProcessor.addEvent(user, newStartDate, newEndDate, details)
                newEvent = ScheduleProcessor.findCurrentEvent()
            End If
            _streamEvent = newEvent

            'TODO: clean up notification code
            Dim notificationHTML As New StringBuilder()
            notificationHTML.Append("<div style=""float: left; width: 100%; margin-bottom:7px""><br />")
            notificationHTML.Append("<img class=""postedImage"" src=""")
            notificationHTML.Append(Utils.getAvatarPath(user))
            notificationHTML.Append(""" style=""height: 80px; width: 80px; margin-right: 2px; border-style: none; float: left"" />")
            notificationHTML.Append("Now live: ")
            notificationHTML.Append(user.displayName)
            notificationHTML.Append("<br /><span style='font-size: 90%; margin-top: 4px; color: #ccc;'>")
            notificationHTML.Append("Playing " & _details)
            notificationHTML.Append("</span></div>")
            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.NotificationRawHTML, notificationHTML.ToString)
            Dim onlineStreamer As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, user.name)
            If onlineStreamer IsNot Nothing Then
                onlineStreamer.client.updateModStatus(True)
            End If
            For Each nextUser As FrontPageUser In Connections.frontPageUsers.ToList
                If nextUser.privileges.isStreamer AndAlso Not nextUser.privileges.isOfficer Then
                    If (nextUser <> _streamer) Then
                        nextUser.client.updateStreamButtons("none")
                    Else
                        nextUser.client.updateStreamButtons("end")
                    End If
                ElseIf (nextUser.privileges.isOfficer) Then
                    nextUser.client.updateStreamButtons("end")
                End If
            Next
        Else
            AutopilotHub.videoTimer.Start()
            AutopilotHub.syncAll()
            If (_streamer.isMatch(user.name)) Then
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.NotificationRawHTML, "The stream has ended.")
            Else
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.NotificationRawHTML, user.displayName & " stopped the stream.")
            End If
            Dim onlineStreamer As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, _streamer.name)
            If onlineStreamer IsNot Nothing Then
                onlineStreamer.client.updateModStatus(onlineStreamer.privileges.canModChat)
            End If
            _streamer = Nothing
            For Each nextUser As FrontPageUser In Connections.frontPageUsers.ToList
                If (nextUser.privileges.isStreamer OrElse nextUser.privileges.isOfficer) Then
                    nextUser.client.updateStreamButtons("start")
                End If
            Next
        End If

        'TODO: post social message to twitter
        Return True
    End Function

    Private Shared _isLive As Boolean
    Public Shared ReadOnly Property isLive As Boolean
        Get
            Return _isLive
        End Get
    End Property

    Private Shared _streamEvent As ScheduleEvent
    Public Shared ReadOnly Property streamEvent As ScheduleEvent
        Get
            Return _streamEvent
        End Get
    End Property

    Private Shared _streamer As OfflineUser
    Shared ReadOnly Property streamer As OfflineUser
        Get
            Return _streamer
        End Get
    End Property

    Private Shared _mode As String
    Shared ReadOnly Property mode As String
        Get
            Return _mode
        End Get
    End Property

    Private Shared _details As String
    Shared ReadOnly Property details As String
        Get
            Return _details
        End Get
    End Property

    Private Shared _modeChangedOn As Date
    Shared ReadOnly Property modeChangedOn As Date
        Get
            Return _modeChangedOn
        End Get
    End Property

End Class
