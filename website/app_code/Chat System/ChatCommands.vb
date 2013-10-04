Imports Microsoft.VisualBasic
Imports System.Management
Imports UserSystem
Imports MySql.Data.MySqlClient
Imports System.Diagnostics

Public Class ChatCommands

    Public Shared musicCommandVideo As AutopilotVideo
    Public Shared musicCommandVideoStarted As Date

    Public Shared Sub syncMusicCommandVideo(user As FrontPageUser)
        If (musicCommandVideo Is Nothing) Then Return
        Dim span As TimeSpan = Now - musicCommandVideoStarted
        If (span.TotalSeconds >= musicCommandVideo.length) Then
            musicCommandVideo = Nothing
            Return
        End If
        user.client.syncYTComplement(musicCommandVideo.videoID, span.TotalSeconds)
    End Sub

    Public Shared Sub processCommand(message As String, caller As FrontPageUser)

        Dim CommandParts As MatchCollection = Regex.Matches(message, "[^\s""']+|""([^""]*)""|'([^']*)'")
        Dim command As String = CommandParts(0).Value.TrimStart("/"c).ToLower
        Dim arguments As New List(Of String)
        Dim messageWithQuotes As String = message.Substring(CommandParts(0).Value.Length).TrimStart(" ")
        For iteration As Integer = 1 To CommandParts.Count - 1
            arguments.Add(CommandParts.Item(iteration).Value.Trim(""""c, "'"c))
        Next

        Dim hasOnDemandModAccess As Boolean = (StreamProcessor.streamer IsNot Nothing AndAlso StreamProcessor.streamer = caller AndAlso caller.privileges.isStreamer)
        Dim availableCommands As New StringBuilder(" help")

        If caller.privileges.canModChat OrElse hasOnDemandModAccess Then
            If command = "poll" Then
                Dim pollOptions As String() = messageWithQuotes.Split(","c)
                If pollOptions.Count > 1 Then
                    Dim current As Poll = Poll.currentPoll
                    If current Is Nothing OrElse Not current.isOngoing Then
                        Poll.currentPoll = New Poll(caller, pollOptions)
                    Else
                        caller.postErrorMessage("there is already an ongoing poll, wait for it to expire")
                    End If
                Else
                    caller.postSystemMessage("usage: /poll 2 or more, options, seperated by commas")
                End If
                Return
            End If

            availableCommands.Append(" poll")

        End If

        If caller.privileges.canModChat Or hasOnDemandModAccess Then

            If command = "mute" Then
                If arguments.Count > 1 Then
                    Dim minutes As Integer
                    Integer.TryParse(arguments(0), minutes)
                    If minutes > 0 Then
                        If minutes < 61 Then
                            Dim users As List(Of OfflineUser) = matchAllOfflineUsers(reconstructArgs(arguments, 1))
                            If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                            For Each user As OfflineUser In users
                                Infractions.add("mute", user.ipAddress, minutes * 60, "", caller)
                                Infractions.add("mute", user.name, minutes * 60, "", caller)
                                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "" &
                                    caller.displayName & " Muted " & user.displayName & " For " & minutes & " minute(s).")
                                For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                                    If Not String.Equals(fpVersion.ipAddress, user.ipAddress) Then
                                        Infractions.add("mute", fpVersion.ipAddress, minutes * 60, "", caller)
                                    End If
                                Next
                            Next
                        Else
                            caller.postSystemMessage("60 minutes maximum")
                        End If
                    Else
                        caller.postSystemMessage("usage: /mute minutes identifier")
                    End If
                Else
                    caller.postSystemMessage("usage: /mute minutes identifier")
                End If
                Return
            End If

            If command = "unmute" Then
                If arguments.Count > 0 Then
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(reconstructArgs(arguments, 0))
                    If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                    For Each user As OfflineUser In users
                        Infractions.clear("mute", user.ipAddress, caller)
                        Infractions.clear("mute", user.name, caller)
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "" &
                            caller.displayName & " unmuted " & user.displayName & ".")
                    Next
                Else
                    caller.postSystemMessage("usage: /unmute identifier")
                End If
                Return
            End If

            If command = "clearmutes" Then
                Infractions.clearAll("mute", caller)
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "" &
                    caller.displayName & " cleared all outstanding mutes.")
                Return
            End If

            If command = "delete" Then
                If arguments.Count = 1 Then
                    Dim indexToRemove As Integer
                    Integer.TryParse(arguments(0), indexToRemove)
                    If indexToRemove > 0 Then
                        ChatProcessor.delete(caller, indexToRemove)
                    End If
                Else
                    caller.postSystemMessage("usage: /delete messageUID")
                End If
                Return
            End If

            If command = "music" Then
                If arguments.Count > 0 Then
                    Dim YTVideoURL As String = reconstructArgs(arguments, 0)
                    If (YTVideoURL = "off" OrElse YTVideoURL = "stop") Then
                        If musicCommandVideo Is Nothing Then
                            musicCommandVideo = Nothing
                            caller.postErrorMessage("No BG music is currently playing.")
                            Return
                        End If
                        Dim timeSinceStart As TimeSpan = Now - musicCommandVideoStarted
                        If (timeSinceStart.TotalSeconds >= musicCommandVideo.length) Then
                            musicCommandVideo = Nothing
                            caller.postErrorMessage("No BG music is currently playing.")
                            Return
                        End If
                        musicCommandVideo = Nothing
                        FrontPageHub.getClients().All.syncYTComplement(Nothing, 0)
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, caller.displayName & " stopped the BG music.")
                        Return
                    End If
                    caller.postSystemMessage("caching video information")
                    Dim video As AutopilotVideo = AutopilotHub.getCachedVideoInfo(YTVideoURL)
                    If video Is Nothing Then
                        caller.postErrorMessage("Invalid YouTube URL")
                    ElseIf video.ytState IsNot Nothing Then
                        caller.postErrorMessage("Invalid Video: " + video.ytState)
                    Else
                        musicCommandVideo = video
                        musicCommandVideoStarted = Now
                        FrontPageHub.getClients().All.syncYTComplement(video.videoID, 0)
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, caller.displayName & " started new BG music.")
                    End If
                Else
                    caller.postSystemMessage("usage: /music YouTubeUrl")
                End If
                Return
            End If

            availableCommands.Append(" vote")
            availableCommands.Append(" mute")
            availableCommands.Append(" unmute")
            availableCommands.Append(" clearmutes")
            availableCommands.Append(" delete")

        End If

        If caller.privileges.canModChat Then

            If command = "flag" Then
                If arguments.Count > 1 Then
                    Dim flag As String = arguments(0)
                    Dim matches As List(Of FrontPageUser) = Connections.matchUsers(Connections.frontPageUsers, reconstructArgs(arguments, 1))
                    If matches.Count > 0 Then
                        For Each user As FrontPageUser In matches
                            user.toggleFlag(flag)
                            If user.isFlagSet(flag) Then
                                caller.postSystemMessage("enabled flag (" & flag & ") on " & user.displayName)
                            Else
                                caller.postSystemMessage("cleared flag (" & flag & ") on " & user.displayName)
                            End If
                        Next
                    Else
                        caller.postErrorMessage("identifier matched 0 users")
                    End If
                Else
                    caller.postSystemMessage("usage: /flag flagName identifier")
                End If
                Return
            End If
            availableCommands.Append(" flag")

            If command = "page" Then
                If arguments.Count > 0 Then
                    Dim matchText As String = reconstructArgs(arguments, 0)
                    Dim targetFound As Boolean
                    For Each target As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, matchText)
                        target.TryPage(caller)
                        targetFound = True
                    Next
                    If Not targetFound Then
                        caller.postErrorMessage("identifier matched 0 users")
                    End If
                Else
                    caller.postSystemMessage("usage: /page identifier")
                End If
                Return
            End If
            availableCommands.Append(" page")




            If command = "addbumper" Then
                If arguments.Count > 0 Then
                    Dim match As String = reconstructArgs(arguments, 0)
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(match)
                    If users.Count = 0 Then
                        caller.postErrorMessage("identifier matched  0 users")
                        Return
                    End If
                    For Each user As OfflineUser In users
                        '  If (user.privileges.isOfficer OrElse user.privileges.canModChat) Then
                        '  caller.postErrorMessage("You can not target an officer or mod with this command.")
                        '  Return
                        '   Else
                        If (user.privileges.isBumper) Then
                            caller.postErrorMessage("This user is already a bumper.")
                            Return
                        ElseIf (Not user.isBoundToAccount) Then
                            caller.postErrorMessage("This user is not registered.")
                            Return
                        End If
                        Try
                            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
                            Dim statement As New MySqlCommand("INSERT IGNORE bb_user_group (group_id, user_id, group_leader, user_pending) VALUES (@1, @2, @3, @4);", Connection)
                            statement.Parameters.Add("@1", MySqlDbType.Int32).Value = 18
                            statement.Parameters.Add("@2", MySqlDbType.Int32).Value = user.forumID
                            statement.Parameters.Add("@3", MySqlDbType.Bit).Value = 0
                            statement.Parameters.Add("@4", MySqlDbType.Bit).Value = 0
                            statement.ExecuteNonQuery()
                            statement.Dispose()
                            Connection.Close()
                        Catch ex As Exception
                            Return
                        End Try
                        For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                            fpVersion.privileges.refreshData()
                        Next
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Bumper, caller.displayName &
                             " added " & user.displayName & " to the bumper user group.")
                    Next

                Else
                    caller.postSystemMessage("usage: /addbumper identifier")
                End If
                Return
            End If

            If command = "removebumper" Then
                If arguments.Count > 0 Then
                    Dim match As String = reconstructArgs(arguments, 0)
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(match)
                    If users.Count = 0 Then
                        caller.postErrorMessage("identifier matched  0 users")
                        Return
                    End If
                    For Each user As OfflineUser In users
                        If (user.privileges.isOfficer OrElse user.privileges.canModChat) Then
                            caller.postErrorMessage("You can not target an officer with this command.")
                            Return
                        ElseIf (Not user.privileges.isBumper) Then
                            caller.postErrorMessage("This user is not a bumper.")
                            Return
                        ElseIf (Not user.isBoundToAccount) Then
                            caller.postErrorMessage("This user is not registered.")
                            Return
                        End If
                        Try
                            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
                            Dim statement As New MySqlCommand("DELETE FROM bb_user_group WHERE group_id=@1 AND user_id=@2;", Connection)
                            statement.Parameters.Add("@1", MySqlDbType.Int32).Value = 18
                            statement.Parameters.Add("@2", MySqlDbType.Int32).Value = user.forumID
                            statement.ExecuteNonQuery()
                            statement.Dispose()
                            Connection.Close()
                        Catch ex As Exception
                            Return
                        End Try
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Bumper, caller.displayName &
                            " removed " & user.displayName & " from the the bumper user group.")
                        For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                            fpVersion.privileges.refreshData()
                        Next
                    Next

                Else
                    caller.postSystemMessage("usage: /addbumper identifier")
                End If
                Return
            End If

            If command = "deleteall" Then
                If arguments.Count > 0 Then
                    ChatProcessor.deleteAll(caller, reconstructArgs(arguments, 0))
                Else
                    caller.postSystemMessage("usage: /deleteall identifier")
                End If
                Return
            End If

            If command = "listdeletes" Then
                ChatProcessor.listCachedDeletes(caller)
                Return
            End If

            If command = "infractions" Then
                If arguments.Count > 0 Then
                    Dim match As String = reconstructArgs(arguments, 0)
                    Dim builder As New StringBuilder("<br /><br />:: Outstanding Infractions ::")
                    builder.Append("<br />----------------------------------------")
                    builder.Append("<br />Matching: """ & match & """")
                    Dim infractionsList As List(Of InfractionDetails) = Infractions.queryDetails(match)
                    If infractionsList.Count = 0 Then
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />No outstanding infractions for this identifier.")
                    End If
                    For Each details As InfractionDetails In infractionsList
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />Type: " & details.type)
                        builder.Append("<br />Created By: " & details.createdByName & " [" & details.createdByIP & "]"c)
                        builder.Append("<br />Created At: " & details.createdAt.ToString & " UTC")
                        builder.Append("<br />Duration: " & details.duration.ToString)
                        builder.Append("<br />Time Left: " & Utils.getReadableTimeRemaining(details.endsAt))
                        builder.Append("<br />Notes: " & details.notes)
                    Next
                    caller.postSystemMessage(builder.ToString)
                Else
                    caller.postSystemMessage("usage: /infractions identifier")
                End If
                Return
            End If

            If command = "setnick" Then
                If arguments.Count = 2 Then
                    Dim matchAlreadyExists As Boolean =
                        Connections.matchFirst(Connections.frontPageUsers, arguments(1)) IsNot Nothing OrElse
                        Connections.matchGuest(arguments(1)) IsNot Nothing OrElse
                        Accounts.matchSSHAccounts(arguments(1)).Count > 0 OrElse
                        Connections.getObjectName(arguments(1)) IsNot Nothing
                    If Not matchAlreadyExists Then
                        Dim users As List(Of OfflineUser) = matchAllOfflineUsers(arguments(0))
                        If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                        For Each user As OfflineUser In users
                            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, caller.displayName &
                                                         " nicknamed " & user.displayName & " as ''" & arguments(1) & "'.")
                            user.displayName = arguments(1)
                            For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                                fpVersion.displayName = arguments(1)
                            Next
                        Next
                    Else
                        caller.postErrorMessage("new name already exists")
                    End If
                Else
                    caller.postSystemMessage("usage: /rename identifier newname")
                End If
                Return
            End If

            If command = "clearnick" Then
                If arguments.Count > 0 Then
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(reconstructArgs(arguments, 0))
                    If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                    For Each user As OfflineUser In users
                        If user.displayName <> user.name Then
                            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, caller.displayName &
                                " cleared the nickname of " & user.displayName & ". They are once again known as " & user.name)
                            user.displayName = user.name & "."c
                            For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                                fpVersion.displayName = fpVersion.name
                            Next
                        End If
                    Next
                Else
                    caller.postSystemMessage("usage: /clearnick identifier")
                End If
                Return
            End If

            availableCommands.Append(" setnick")
            availableCommands.Append(" clearnick")
            availableCommands.Append(" deleteall")
            availableCommands.Append(" listdeletes")
            availableCommands.Append(" infractions")
            availableCommands.Append(" listguests")
        End If

        If caller.privileges.canAccessBanSystem Then

            If command = "ban" Then
                If arguments.Count > 2 Then
                    Dim hours As Integer
                    Integer.TryParse(arguments(0), hours)
                    If hours > 0 Then
                    
                            Dim reason As String = reconstructArgs(arguments, 2)
                            Dim users As List(Of OfflineUser) = matchAllOfflineUsers(arguments(1))
                            If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                            For Each user As OfflineUser In users
                                Infractions.add("ban", user.ipAddress, hours * 60 * 60, reason, caller)
                                Infractions.add("ban", user.name, hours * 60 * 60, reason, caller)
                                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "" &
                                    caller.displayName & " banned " & user.displayName & " for " & hours & " hour(s).")
                                For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.ipAddress)
                                    fpVersion.refreshBrowser()
                                Next
                                For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                                    If Not String.Equals(fpVersion.ipAddress, user.ipAddress) Then
                                        Infractions.add("ban", fpVersion.ipAddress, hours * 60 * 60, reason, caller)
                                    End If
                                    fpVersion.refreshBrowser()
                                Next
                            Next
                        
                    Else
                        caller.postSystemMessage("usage: /ban hours identifier reason")
                    End If
                Else
                    caller.postSystemMessage("usage: /ban hours identifier reason")
                End If
                Return
            End If

            If command = "unban" Then
                If arguments.Count > 0 Then
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(reconstructArgs(arguments, 0))
                    If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                    For Each user As OfflineUser In users
                        Infractions.clear("ban", user.ipAddress, caller)
                        Infractions.clear("ban", user.name, caller)
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "" &
                            caller.displayName & " cleared ban on " & user.displayName & ".")
                    Next
                Else
                    caller.postSystemMessage("usage: /unban identifier")
                End If
                Return
            End If

            If command = "silentban" Then
                If arguments.Count > 2 Then
                    Dim days As Integer
                    Integer.TryParse(arguments(0), days)
                    If days > 0 Then
                        If days < 8 Then
                            Dim reason As String = reconstructArgs(arguments, 2)
                            Dim users As List(Of OfflineUser) = matchAllOfflineUsers(arguments(1))
                            If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                            For Each user As OfflineUser In users
                                Infractions.add("silentban", user.ipAddress, days * 60 * 60 * 24, reason, caller)
                                Infractions.add("silentban", user.name, days * 60 * 60 * 24, reason, caller)
                                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Officer, "" &
                                    caller.displayName & " silently banned " & user.displayName & " for " & days & " day(s).")
                                For Each fpVersion As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, user.name)
                                    If Not String.Equals(fpVersion.ipAddress, user.ipAddress) Then
                                        Infractions.add("silentban", fpVersion.ipAddress, days * 60 * 60 * 24, reason, caller)
                                    End If
                                Next
                            Next
                        Else
                            caller.postErrorMessage("7 days maximum")
                        End If
                    Else
                        caller.postSystemMessage("usage: /silentban dats identifier reason")
                    End If
                Else
                    caller.postSystemMessage("usage: /silentban days identifier reason")
                End If
                Return
            End If

            If command = "unsilentban" Then
                If arguments.Count > 0 Then
                    Dim users As List(Of OfflineUser) = matchAllOfflineUsers(reconstructArgs(arguments, 0))
                    If users.Count = 0 Then caller.postErrorMessage("identifier matched  0 users")
                    For Each user As OfflineUser In users
                        Infractions.clear("silentban", user.ipAddress, caller)
                        Infractions.clear("silentban", user.name, caller)
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Officer, "" &
                                caller.displayName & " cleared the silent ban on " & user.displayName & ".")
                    Next
                Else
                    caller.postSystemMessage("usage: /unban identifier")
                End If
                Return
            End If

            availableCommands.Append(" ban")
            availableCommands.Append(" unban")
            availableCommands.Append(" silentban")
            availableCommands.Append(" unsilentban")
        End If

        If caller.privileges.isDeveloper Then

            If command = "postas" Then
                If arguments.Count > 1 Then
                    Dim newMessage As String = reconstructArgs(arguments, 1)
                    Dim matches As List(Of FrontPageUser) = Connections.matchUsers(Connections.frontPageUsers, arguments(0))
                    If matches.Count > 0 Then
                        If matches(0).name = "GaryOak" Then
                            caller.postErrorMessage("Error. You are a fag.")
                            Return
                        End If
                        If newMessage.Substring(0, 1) = "/" Then
                            processCommand(newMessage, matches(0))
                        Else
                            ChatProcessor.postNewMessage(matches(0), Nothing, ChatMessage.MessageType.Normal, newMessage)
                        End If
                    Else
                        caller.postErrorMessage("identifier matched 0 users")
                    End If
                Else
                    caller.postSystemMessage("usage: /postas identifier message")
                End If
                Return
            End If

            If command = "pushmaintenance" Then
                Try
                    ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "server going down for maintenance")
                    For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                        user.client.initiateRefresh(10000)
                    Next
                Catch ex As Exception
                End Try
                Threading.Thread.Sleep(2000)
                IO.File.Copy(Utils.serverPath & "app_maintenance", Utils.serverPath & "app_offline.htm")
                Return
            End If

            If command = "update" Then
                ServerPersistance.setField("updated", True)
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    user.client.initiateRefresh(10)
                Next
                Dim Update As New Process
                Update.StartInfo.UseShellExecute = True
                Update.StartInfo.WorkingDirectory = HttpContext.Current.Server.MapPath("~")
                Update.StartInfo.FileName = "update.bat"
                Update.StartInfo.WindowStyle = ProcessWindowStyle.Normal
                Update.Start()
                Return
            End If

            If command = "refreshbrowsers" Then
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, "system-wide client refresh")
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    user.client.initiateRefresh(100)
                Next
                Return
            End If

            If command = "status" Or command = "stats" Then
                Dim statusMessage As String = "<br />Server Status ::"
                Dim query As New SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'")
                Dim searcher As New ManagementObjectSearcher(query)
                For Each mo As ManagementObject In searcher.Get()
                    Dim dtBootTime As DateTime = ManagementDateTimeConverter.ToDateTime(mo.Properties("LastBootUpTime").Value.ToString())
                    statusMessage &= "<br />last boot: " & Utils.getReadableTimeElapsed(dtBootTime, True)
                Next
                statusMessage &= "<br />last recycle: " & Utils.getReadableTimeElapsed(Utils.lastRecycle, True)
                statusMessage &= "<br />app memory used: " & Utils.GetReadableByteSize(GC.GetTotalMemory(False))
                caller.postSystemMessage(statusMessage)
                Return
            End If

            If command = "setskin" Then
                If arguments.Count > 0 Then
                    Dim skinName As String = reconstructArgs(arguments, 0)
                    Dim skinFiles As String() = IO.Directory.GetFiles(Utils.serverPath & "\skins\", skinName & ".*")
                    If skinFiles.Length > 0 Then
                        Dim properName As String = IO.Path.GetFileNameWithoutExtension(skinFiles(0))
                        caller.postSystemMessage("switched users to specified skin")
                        For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                            user.client.setUserSkin(properName)
                            user.client.postSystemMessage("skin switched by " & caller.displayName)
                        Next
                    Else
                        caller.postErrorMessage("no such skin exists")
                    End If
                Else
                    caller.postSystemMessage("usage: /setskin skinName")
                End If
                Return
            End If

            availableCommands.Append(" postas")
            availableCommands.Append(" pushmaintenance")
            availableCommands.Append(" update")
            availableCommands.Append(" refreshbrowsers")
            availableCommands.Append(" status")
            availableCommands.Append(" setskin")
        End If

        If caller.privileges.isOfficer Then

            If command = "reloadinfractions" Then
                Infractions.init()
                caller.postSystemMessage("Infractions refreshed to current DB data.")
                Return
            End If
            availableCommands.Append(" reloadinfractions")

            If command = "lockskip" Then
                AutopilotHub.skippingLocked = True
                caller.postSystemMessage("AP skipping locked")
                Return
            End If

            If command = "unlockskip" Then
                AutopilotHub.skippingLocked = False
                caller.postSystemMessage("AP skipping unlocked")
                Return
            End If

            availableCommands.Append(" lockskip")
            availableCommands.Append(" unlockskip")

            If command = "pin" Then
                If arguments.Count > 0 Then
                    ServerPersistance.setField("announcement", messageWithQuotes)
                    ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Officer, caller.displayName & " pinned a new announcement.")
                    For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                        user.client.showPinnedMessage(messageWithQuotes)
                    Next
                Else
                    caller.postSystemMessage("usage: /pin message")
                End If
                Return
            End If

            If command = "unpin" Then
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Officer, caller.displayName & " removed the current pinned announcement.")
                ServerPersistance.setField("announcement", "")
                Return
            End If

            availableCommands.Append(" pin")
            availableCommands.Append(" unpin")


            If command = "listguests" Then
                Dim guests As New List(Of OnlineUser)
                Dim listedCount As New Dictionary(Of String, Integer)
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If Not user.isBoundToAccount Then
                        If listedCount.ContainsKey(user.name) Then
                            listedCount(user.name) += 1
                        Else
                            listedCount(user.name) = 1
                            guests.Add(user)
                        End If
                    End If
                Next
                If guests.Count > 0 Then
                    guests.Sort()
                    Dim builder As New StringBuilder("<br /><br />:: Online Guests ::")
                    builder.Append("<br />----------------------------------------")
                    For Each user As FrontPageUser In guests
                        Dim numberOf As Integer = listedCount(user.name)
                        builder.Append("<br />" & user.ipAddress)
                        If numberOf > 1 Then
                            builder.Append(" (x" & numberOf & ")")
                        End If
                        builder.Append(" -> ")
                        If user.name <> user.displayName Then
                            builder.Append(user.name & " (" & user.displayName & ")")
                        Else
                            builder.Append(user.name)
                        End If
                    Next
                    caller.postSystemMessage(builder.ToString)
                Else
                    caller.postSystemMessage("No guests online.")
                End If

                Return
            End If

            If command = "whois" Or command = "who" Then
                If arguments.Count > 0 Then
                    Dim match As String = reconstructArgs(arguments, 0)
                    Dim builder As New StringBuilder("<br /><br />:: WHOIS Lookup ::")
                    builder.Append("<br />----------------------------------------")
                    builder.Append("<br />Matching: """ & match & """")
                    Dim actualObjectName As String = Connections.getObjectName(match)
                    If actualObjectName IsNot Nothing Then
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />Nickname Belongs To: " & actualObjectName)
                        If Infractions.hasOutstanding(actualObjectName, "ban") Then
                            builder.Append("<br />----------------------------------------")
                            builder.Append("<br /><b>Outstanding Ban</b>")
                        End If
                        If Infractions.hasOutstanding(actualObjectName, "mute") Then
                            builder.Append("<br />----------------------------------------")
                            builder.Append("<br />Outstanding Mute")
                        End If
                    Else
                        If Infractions.hasOutstanding(match, "ban") Then
                            builder.Append("<br />----------------------------------------")
                            builder.Append("<br /><b>Outstanding Ban</b>")
                        End If
                        If Infractions.hasOutstanding(match, "mute") Then
                            builder.Append("<br />----------------------------------------")
                            builder.Append("<br />Outstanding Mute")
                        End If
                    End If
                    For Each user As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, match)
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />[Online Chat User] : ")
                        builder.Append("<br />Object Name: " & user.name)
                        builder.Append("<br />Display Name: " & user.displayName)
                        builder.Append("<br />IP Address: " & user.ipAddress)
                        builder.Append("<br />permissions: " & user.privileges.listAll)
                    Next
                    For Each account As AccountDetails In Accounts.matchSSHAccounts(match)
                        Dim user As OfflineUser = New OfflineUser(account.ipAddress)
                        user.bindAccount(account)
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />[Forum Account] : ")
                        builder.Append("<br />Account Name: " & user.name)
                        builder.Append("<br />IP Address: " & user.ipAddress)
                        builder.Append("<br />permissions: " & user.privileges.listAll)
                    Next
                    Dim guestUser As OfflineUser = Connections.matchGuest(match)
                    If guestUser IsNot Nothing Then
                        builder.Append("<br />----------------------------------------")
                        builder.Append("<br />[Guest Record] : ")
                        builder.Append("<br />Object Name: " & guestUser.name)
                        builder.Append("<br />Display Name: " & guestUser.displayName)
                        builder.Append("<br />IP Address: " & guestUser.ipAddress)
                    End If
                    'may need to re-use this code at some point in the future maybe.
                    '  For Each account As AccountDetails In Accounts.matchIoGAccounts(match)
                    '     builder.Append("<br />----------------------------------------")
                    '     builder.Append("<br />[IoG DB Record] : " & account.username)
                    '     builder.Append("<br />Name: " & account.username)
                    '     builder.Append("<br />IP Address: " & account.ipAddress)
                    ' Next
                    caller.postSystemMessage(builder.ToString)
                Else
                    caller.postSystemMessage("usage: /" + command + " identifier")
                End If
                Return
            End If

            availableCommands.Append(" listguests")
            availableCommands.Append(" whois")

        End If

        If caller.privileges.isStreamer Or caller.privileges.canModChat Then
            If command = "streamer" Or command = "s" Then
                If arguments.Count > 0 Then
                    ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Channel_Streamer, messageWithQuotes)
                Else
                    caller.postSystemMessage("usage: /" & command & " message to streamers only")
                End If
                Return
            End If
            availableCommands.Append(" streamer s")
        End If

        If caller.privileges.isBumper Or caller.privileges.canModChat Then
            If command = "bumper" Or command = "bump" Or command = "b" Then
                If arguments.Count > 0 Then
                    ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Channel_Bumper, messageWithQuotes)
                Else
                    caller.postSystemMessage("usage: /" & command & " message to bumper channel")
                End If
                Return
            End If
            availableCommands.Append(" bump b")
        End If

        If caller.privileges.canModChat Then
            If command = "mod" Or command = "m" Then
                If arguments.Count > 0 Then
                    ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Channel_Mod, messageWithQuotes)
                Else
                    caller.postSystemMessage("usage: /" & command & " message to mods only")
                End If
                Return
            End If
            availableCommands.Append(" mod m")
        End If

        If caller.privileges.isOfficer Then
            If command = "officer" Or command = "o" Then
                If arguments.Count > 0 Then
                    ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Channel_Officer, messageWithQuotes)
                Else
                    caller.postSystemMessage("usage: /" & command & " message to officers only")
                End If
                Return
            End If
            availableCommands.Append(" officer o")
        End If


        ' Command availible to all users

        If command = "vote" Then
            If arguments.Count = 1 Then
                Dim index As Integer
                If Not Integer.TryParse(arguments(0), index) Then
                    caller.postSystemMessage("usage: /vote number")
                    Return
                End If
                index = index - 1
                Dim current As Poll = Poll.currentPoll
                If current IsNot Nothing AndAlso current.isOngoing Then
                    current.vote(caller, index)
                Else
                    caller.postErrorMessage("There is no ongoing poll.")
                End If
            Else
                caller.postSystemMessage("usage: /vote number")
            End If
            Return
        End If
        availableCommands.Append(" vote")

        If command = "me" Then
            If arguments.Count > 0 Then
                ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Emote, messageWithQuotes)
            Else
                caller.postSystemMessage("usage: /me is emoting")
            End If
            Return
        End If
        availableCommands.Append(" me")

        If command = "whisper" OrElse command = "w" Then
            If arguments.Count > 1 Then
                Dim matches As List(Of FrontPageUser) = Connections.matchUsers(Connections.frontPageUsers, arguments(0))
                If matches.Count > 0 Then
                    Dim finalMessage As String = messageWithQuotes.Substring(CommandParts(1).Value.Length).TrimStart(" ")
                    For Each target As FrontPageUser In matches
                        If caller = target OrElse Not Infractions.isSilentBanned(caller) Then
                            ChatProcessor.postNewMessage(caller, {target}, ChatMessage.MessageType.Whisper, finalMessage)
                        End If
                        ChatProcessor.postNewMessage(target, {caller}, ChatMessage.MessageType.WhisperEcho, finalMessage)
                    Next
                Else
                    caller.postErrorMessage("identifier matched 0 users")
                End If
            Else
                caller.postSystemMessage("usage: /" & command & " identifier message")
            End If
            Return
        End If
        availableCommands.Append(" whisper w")

        If command = "reply" OrElse command = "r" Then
            If arguments.Count > 0 Then
                If caller.lastWhisperer IsNot Nothing Then
                    If caller = caller.lastWhisperer OrElse Not Infractions.isSilentBanned(caller) Then
                        ChatProcessor.postNewMessage(caller, {caller.lastWhisperer}, ChatMessage.MessageType.Whisper, messageWithQuotes)
                    End If
                    ChatProcessor.postNewMessage(caller.lastWhisperer, {caller}, ChatMessage.MessageType.WhisperEcho, messageWithQuotes)
                Else
                    caller.postSystemMessage("No one to reply to.")
                End If
            Else
                caller.postSystemMessage("usage: /" & command & " message")
            End If
            Return
        End If
        availableCommands.Append(" reply r")

        If command = "skip" Then
            AutopilotHub.requestUserSkip(caller)
            Return
        End If
        availableCommands.Append(" skip")

        If command = "jason" Then
            If arguments.Count > 0 Then
                caller.postSystemMessage("usage: use /jason")
            Else
                caller.postSystemMessage("JASON!")
                caller.TryJason(caller)
            End If
            Return
        End If
        availableCommands.Append(" jason")

        If command = "dog" Then
            caller.client.redirect("http://www.youtube.com/embed/x8WF5EMhd_A?autoplay=1")
            Return
        End If
        availableCommands.Append(" dog")
        
        If command = "duane" Then
            caller.client.redirect("http://www.youtube.com/embed/ItQKcKnfkIg?autoplay=1")
            Return
        End if
        availableCommands.Append(" duane")

        If command = "slam" Then
            If arguments.Count > 0 Then
                caller.postSystemMessage("usage: use /slam")
            Else
                caller.postSystemMessage("Hey You Whatcha Gonna Do!")
                caller.TrySlam(caller)
            End If
            Return
        End If
        availableCommands.Append(" slam")

        If command = "austin" Then
            If arguments.Count > 0 Then
                caller.postSystemMessage("usage: use /slam")
            Else
                caller.postSystemMessage("IT'S ME AUSTIN!")
                caller.TryAustin(caller)
            End If
            Return
        End If
        availableCommands.Append(" austin")


        If command = "clear" Then
            caller.client.clearChat()
            Return
        End If
        availableCommands.Append(" clear")

        If command = "ping" Then
            caller.client.calculatePing()
            Return
        End If
        availableCommands.Append(" ping")

        If command = "novideo" Then
            'If caller.privileges.canModChat Or caller.privileges.isStreamer Or caller.name = ("garhor") Then
                caller.enableNoVideoMode()
           ' Else
          '      caller.postErrorMessage("Must be mod or streaming to use /novideo.")
           ' End If
           ' Return
        End If
        availableCommands.Append(" novideo")


        If command = "block" Then
            If arguments.Count > 0 Then
                Dim matchText As String = reconstructArgs(arguments, 0)
                Dim targetFound As Boolean
                For Each target As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, matchText)
                    If caller.isBlocked(target.name) Then
                        caller.postErrorMessage("You are already blocking " & target.displayName & ".")
                    Else
                        caller.startBlocking(target.name)
                        caller.postSystemMessage("Now blocking messages from " & target.displayName & ".")
                    End If
                    targetFound = True
                Next
                If Not targetFound Then
                    caller.postErrorMessage("identifier matched 0 users")
                End If
            Else
                caller.postSystemMessage("usage: /block identifier")
            End If
            Return
        End If
        availableCommands.Append(" block")

        If command = "unblock" Then
            If arguments.Count > 0 Then
                Dim matchText As String = reconstructArgs(arguments, 0)
                Dim targetFound As Boolean
                For Each target As FrontPageUser In Connections.matchUsers(Connections.frontPageUsers, matchText)
                    If caller.isBlocked(target.name) Then
                        caller.stopBlocking(target.name)
                        caller.postSystemMessage("No longer blocking messages from " & target.displayName & ".")
                    Else
                        caller.postErrorMessage("You are not currently blocking " & target.displayName & ".")
                    End If
                    targetFound = True
                Next
                If Not targetFound Then
                    caller.postErrorMessage("identifier matched 0 users")
                End If
            Else
                caller.postSystemMessage("usage: /unblock identifer")
            End If
            Return
        End If
        availableCommands.Append(" unblock")

        If command = "help" OrElse command = "halp" Then
            caller.postSystemMessage("commands: " + availableCommands.ToString())
            Return
        End If

        If command = "boo" Then
            If StreamProcessor.isLive Then
                ChatProcessor.postNewMessage(caller, Nothing, ChatMessage.MessageType.Emote, "[b]boos the streamer![/b] [-2cool]")
            Else
                caller.postSystemMessage("who are you booing?")
            End If
            Return
        End If


        'easter-egg commands ::
        If command = "jesth" Then
            caller.postSystemMessage("IQ lowering complete.")
            Return
        End If
        'command = "debug" OrElse command = "scg" OrElse
        If command = "banme" Then
            Infractions.add("ban", caller.ipAddress, 140, "self ban joke command", caller)
            Infractions.add("ban", caller.name, 140, "self ban joke command", caller)
            caller.refreshBrowser()
            Return
        End If

        caller.postErrorMessage("unknown command or access denied")

    End Sub

    Private Shared Function reconstructArgs(stringArray As List(Of String), startingAt As Integer) As String
        Dim builder As New StringBuilder
        For iteration = startingAt To stringArray.Count - 1
            builder.Append(stringArray.Item(iteration))
            builder.Append(" "c)
        Next
        If builder.Length > 0 Then builder.Remove(builder.Length - 1, 1)
        Return builder.ToString
    End Function

End Class
