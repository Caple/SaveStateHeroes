Imports Microsoft.VisualBasic
Imports Microsoft.AspNet.SignalR.Hubs
Imports UserSystem
Imports Microsoft.AspNet.SignalR

Public Class FrontPageHub
    Inherits Hub

    Public Shared Function getClients() As IHubConnectionContext
        Return Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of FrontPageHub)().Clients
    End Function

    Overrides Function OnConnected() As Threading.Tasks.Task
        Dim user As New FrontPageUser(Utils.getClientIPAddress, Context.ConnectionId, Clients.Caller)
        user.setGuestName(Connections.getGuestName(user.ipAddress))
        Connections.frontPageUsers.Add(user)
        Clients.All.setUserCount(Connections.frontPageUsers.Count)
        Return Nothing
    End Function

    Overrides Function OnReconnected() As Threading.Tasks.Task
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then
            OnConnected()
        Else
            user.rebindClient(Context.ConnectionId, Clients.Caller)
        End If
        connectionInit(False)
        Return Nothing
    End Function

    Overrides Function OnDisconnected() As Threading.Tasks.Task
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user IsNot Nothing Then
            UserSystem.Connections.frontPageUsers.Remove(user)
            Clients.All.setUserCount(Connections.frontPageUsers.Count)
        End If
        Return Nothing
    End Function

    Public Sub connectionInit(ByVal pageLoad As Boolean)
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        If Not user.isBoundToAccount Then
            Dim account As AccountDetails = Accounts.getAccountBySession(HttpContext.Current)
            If account.isValidRecord Then
                bindLoginToUser(user, account, False)
                sendButtons()
            Else
                Clients.Caller.doLogout()
            End If
        Else
            Clients.Caller.doLogout()
        End If
        Clients.Caller.updateModStatus(user.privileges.canModChat)
        If pageLoad Then
            Dim cachedHTML As String = ChatProcessor.getCachedMessages(user)
            If cachedHTML IsNot Nothing Then
                user.postMessage(cachedHTML)
            End If
            'If StreamProcessor.isLive Then
            '    If StreamProcessor.streamer IsNot Nothing Then
            '        If user = StreamProcessor.streamer Then
            '            Clients.Caller.muteVideo()
            '            Clients.Caller.setLiveStatus(True, StreamProcessor.streamer.displayName, False)
            '        Else
            '            Clients.Caller.setLiveStatus(True, StreamProcessor.streamer.displayName, True)
            '        End If
            '    Else
            '        Clients.Caller.setLiveStatus(True, Nothing, True)
            '    End If
            'End If
            Dim announcement As String = ServerPersistance.getField("announcement")
            If announcement.Length > 0 Then Clients.Caller.showPinnedMessage(announcement)
            If user.isBoundToAccount Then
                user.postSystemMessage("Logged in as " & user.name)
            End If
            ChatCommands.syncMusicCommandVideo(user)
        End If
    End Sub

    Public Function checkOnLiveStatus() As Boolean
        Return StreamProcessor.isLive
    End Function

    Public Function getUserOption(ByVal name As String) As Object
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return UserOptions.defaultOptions.getOption(name)
        Return user.options.getOption(name)
    End Function

    Public Function getCommonUserOptions() As UserOptionsCommon
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return UserOptions.defaultOptions.getCommonOptions
        Return user.options.getCommonOptions
    End Function

    Public Function tryLogin(ByVal username As String, ByVal password As String) As Boolean
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return False
        Dim account As AccountDetails = Accounts.getAccountByCredentials(username, password)
        If account.isValidRecord Then
            bindLoginToUser(user, account, True)
            Clients.Caller.updateModStatus(user.privileges.canModChat)
            sendButtons()
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Sub bindLoginToUser(ByVal user As FrontPageUser, ByVal account As AccountDetails, ByVal createSessionCookie As Boolean)
        Dim duplicateLogins As List(Of FrontPageUser) = Connections.matchUsers(Connections.frontPageUsers, account.username)
        For Each duplicate As FrontPageUser In duplicateLogins
            Connections.frontPageUsers.Remove(duplicate)
            duplicate.unbindAccount()
            duplicate.client.jumpToDisconnect()
        Next
        user.bindAccount(account)
        user.client.doLogin()
        If createSessionCookie Then Accounts.createSession(user.forumID, user.ipAddress, HttpContext.Current)
    End Sub

    Public Sub logout()
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        Accounts.deleteSession(user.forumID, HttpContext.Current)
        user.unbindAccount()
        user.setGuestName(Connections.getGuestName(user.ipAddress))
        Clients.Caller.doLogout()
        Clients.Caller.updateModStatus(user.privileges.canModChat)
        sendButtons()
    End Sub

    Public Sub setUserOption(ByVal name As String, ByVal value As String)
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user IsNot Nothing AndAlso user.isBoundToAccount Then
            user.options.setOption(name, value)
        End If
    End Sub

    Public Function queryUserList() As String
        Dim guestCount As Integer
        Dim builder As New StringBuilder()
        Dim sorted As List(Of FrontPageUser) = Connections.frontPageUsers.ToList
        sorted.Sort()
        For Each user As FrontPageUser In sorted
            If user.isBoundToAccount Then
                builder.Append("<br /><span style='color: ")
                builder.Append(user.options.chatColorName)
                If user.isIdle Then builder.Append("; font-family: ""LuxiSansOblique""")
                builder.Append(";'>")
                builder.Append(user.displayName)
                builder.Append("</span>")
            Else
                guestCount += 1
            End If
        Next
        If guestCount > 0 Then
            builder.Append("<br />(+ ")
            builder.Append(guestCount)
            If guestCount = 1 Then
                builder.Append(" Guest)")
            Else
                builder.Append(" Guests)")
            End If
        End If
        Return builder.ToString()
    End Function

    Public Function querySkins() As String()
        Dim BasePath As String = AppDomain.CurrentDomain.BaseDirectory.ToString()
        Dim SkinList As New List(Of String)
        For Each File As String In IO.Directory.GetFiles(BasePath & "\skins\", "*.css")
            SkinList.Add(IO.Path.GetFileNameWithoutExtension(File))
        Next
        Return SkinList.ToArray()
    End Function

    Public Function sendChatMessage(ByVal text As String) As Boolean
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing OrElse text.Length < 1 Then Return False
        If text.Substring(0, 1) = "/" Then
            ChatCommands.processCommand(text, user)
            Return True
        Else
            Dim value As Boolean = ChatProcessor.postNewMessage(user, Nothing, ChatMessage.MessageType.Normal, text)
            If user.isFlagSet("alone") Then
                user.postSystemMessage("No one hears you.")
            End If
            Return value
        End If
    End Function

    Public Sub reportLSLive()
        'StreamProcessor.clientReportedChangeInLiveStatus()
    End Sub

    Public Sub syncVideo()
        Clients.Caller.syncAPVideo(AutopilotHub.videoID, AutopilotHub.videoTime, AutopilotHub.getClientTitle)
    End Sub

    Public Sub serverPing()
        Clients.Caller.clientPing()
    End Sub

    Public Sub updateIdleStatus(ByVal isIdle As Boolean)
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        If StreamProcessor.isLive AndAlso StreamProcessor.streamer IsNot Nothing AndAlso StreamProcessor.streamer = user Then Return
        user.isIdle = isIdle
    End Sub

    Public Sub setVideoMode(ByVal mode As String, ByVal details As String, ByVal socialMessage As String)
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        StreamProcessor.setMode(user, mode, details, socialMessage)
    End Sub

    Public Sub queryVideoMode()
        If (StreamProcessor.streamer IsNot Nothing) Then
            Clients.Caller.switchVideoMode(StreamProcessor.streamer.displayName, StreamProcessor.mode, StreamProcessor.details)
        Else
            Clients.Caller.switchVideoMode("No Streamer", StreamProcessor.mode, StreamProcessor.details)
        End If
    End Sub

    Private Sub sendButtons()
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        If StreamProcessor.isLive Then
            If user.privileges.isOfficer Then
                user.client.updateStreamButtons("end")
            ElseIf user.privileges.isStreamer Then
                If StreamProcessor.streamer IsNot Nothing AndAlso user = StreamProcessor.streamer Then
                    user.client.updateStreamButtons("end")
                Else
                    user.client.updateStreamButtons("none")
                End If
            Else
                user.client.updateStreamButtons("none")
            End If
        Else
            If user.privileges.isStreamer OrElse user.privileges.isOfficer Then
                user.client.updateStreamButtons("start")
            Else
                user.client.updateStreamButtons("none")
            End If
        End If
    End Sub

    Public Sub checkConnectionServer()
        Dim user As FrontPageUser = Connections.matchFirst(Connections.frontPageUsers, Context.ConnectionId)
        If user Is Nothing Then Return
        user.updateLastConnectionResponse()
    End Sub

End Class
