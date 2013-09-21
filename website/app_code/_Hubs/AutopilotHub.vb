Imports Microsoft.VisualBasic
Imports System.Xml
Imports MySql.Data.MySqlClient
Imports UserSystem
Imports Microsoft.AspNet.SignalR
Imports Microsoft.AspNet.SignalR.Hubs

Public Class AutopilotHub
    Inherits Hub

    Private Shared _skippingLocked As Boolean
    Public Shared Property skippingLocked As Boolean
        Get
            Return _skippingLocked
        End Get
        Set(ByVal value As Boolean)
            _skippingLocked = value
            ServerPersistance.setField("skippingLocked", value)
        End Set
    End Property


    Private Shared recentHistory As New List(Of String)
    Private Shared randomClass As New Random
    Private Shared videos As New List(Of AutopilotVideo)
    Private Shared cachedXMLInformation As New Dictionary(Of String, AutopilotVideo)
    Private Shared currentPlayingIndex As Integer = -1
    Private Shared currentVideoTime As Integer
    Private Shared currentVideo As AutopilotVideo
    Private Shared skipTracker As New HashSet(Of String)
    Private Shared shuffleMode As Boolean
    Private Shared recentAPIDs As List(Of String)
    Public Shared videoTimer As New Timers.Timer

    Public Sub toggleShuffleMode()
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            shuffleMode = Not shuffleMode
            ServerPersistance.setField("ap_shuffle", True)
            Clients.All.updateShuffle(shuffleMode)
        End If
    End Sub

    Private Shared Function pickRandomSongIndex() As Integer
        pickRandomSongIndex = currentPlayingIndex
        If videos.Count > recentAPIDs.Count Then
            While recentAPIDs.Contains(videos(pickRandomSongIndex).videoID)
                pickRandomSongIndex = randomClass.Next(0, videos.Count)
            End While
        End If
    End Function

    Public Shared Sub requestUserSkip(ByVal from As FrontPageUser)
        If skippingLocked Then
            from.postSystemMessage("Skipping is currently disabled by an officer.")
            Return
        End If
        If Not StreamProcessor.isLive Then
            If Not skipTracker.Contains(from.ipAddress) Then
                For Each nextIP As String In skipTracker.ToList
                    If Connections.matchFirst(Connections.frontPageUsers, nextIP) Is Nothing Then
                        skipTracker.Remove(nextIP)
                    End If
                Next
                skipTracker.Add(from.ipAddress)
                Dim eligibleVoters As New HashSet(Of String)
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If Not user.isIdle Then
                        If Not eligibleVoters.Contains(user.ipAddress) Then
                            eligibleVoters.Add(user.ipAddress)
                        End If
                    End If
                Next
                Dim requiredVotes As Integer = eligibleVoters.Count / 2
                If (requiredVotes / eligibleVoters.Count) < (1 / 2) Then
                    requiredVotes += 1
                End If
                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, from.displayName & _
                             " voted to skip. " & skipTracker.Count & " / " & requiredVotes)
                If skipTracker.Count >= requiredVotes Then
                    ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, "" & _
                             " AP video skipped by votes.")
                    Dim newIndex As Integer = currentPlayingIndex + 1
                    If shuffleMode Then newIndex = pickRandomSongIndex()
                    If newIndex < videos.Count Then
                        setPlayingVideo(newIndex, True)
                    ElseIf videos.Count > 0 Then
                        setPlayingVideo(0, True)
                    End If
                End If
            Else
                from.postErrorMessage("You can only request one skip per video per IP.")
            End If
        Else
            from.postErrorMessage("The autopilot is not currently playing.")
        End If
    End Sub

    Public Function isAPAdmin() As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing Then
            Return user.privileges.canAdminAutoPilot
        Else
            Return False
        End If
    End Function

    Public Function getAPList() As AutopilotVideo()
        Return videos.ToArray
    End Function

    Public Shared Sub syncAll()
        Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(currentPlayingIndex, currentVideoTime, videos(currentPlayingIndex).length)
    End Sub

    Public Shared Sub freezeTrackBar()
        Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.freezeTrackBar()
    End Sub

    Public Sub forceSync()
        If currentPlayingIndex > -1 AndAlso currentPlayingIndex < videos.Count Then
            Clients.Caller.syncTime(currentPlayingIndex, currentVideoTime, videos(currentPlayingIndex).length)
            Clients.Caller.updateShuffle(shuffleMode)
            If Not videoTimer.Enabled Then
                Clients.Caller.freezeTrackBar()
            End If
        End If
    End Sub

    Public Function cacheVideoInformation(ByVal videoURL As String) As AutopilotVideo
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            Return tryCache(videoURL)
        Else
            Return Nothing
        End If
    End Function

    Public Function addVideo(ByVal videoID As String) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            If Not cachedXMLInformation.ContainsKey(videoID) Then
                tryCache("http://www.youtube.com/watch?v=" & videoID)
            End If
            Return moveXMLInformationToLive(videoID, user)
        Else
            Return False
        End If
    End Function

    Public Shared Function getCachedVideoInfo(ByVal videoID As String) As AutopilotVideo
        If Not cachedXMLInformation.ContainsKey(videoID) Then
            Return tryCache("http://www.youtube.com/watch?v=" & videoID)
        Else
            Return cachedXMLInformation(videoID)
        End If
    End Function

    Public Function removeVideo(ByVal index As Integer) As Boolean
        Return reorderVideos(index, -1)
    End Function

    Public Function reorderVideos(ByVal oldIndex As Integer, ByVal newIndex As Integer) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            SyncLock videos

                Dim item As AutopilotVideo = videos(oldIndex)
                If newIndex > videos.Count - 1 OrElse oldIndex < 0 OrElse oldIndex > videos.Count - 1 Then
                    Return False
                End If

                videos.RemoveAt(oldIndex)
                If newIndex > -1 Then
                    videos.Insert(newIndex, item)
                    Clients.All.sortedItem(oldIndex, newIndex)
                    If currentVideo IsNot Nothing Then
                        currentPlayingIndex = videos.IndexOf(currentVideo)
                        Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(currentPlayingIndex, currentVideoTime, currentVideo.length)
                    End If
                Else
                    Clients.All.removedItem(oldIndex)
                    If oldIndex = currentPlayingIndex Then
                        setPlayingVideo(oldIndex, True)
                    ElseIf currentVideo IsNot Nothing AndAlso oldIndex <> currentPlayingIndex Then
                        currentPlayingIndex = videos.IndexOf(currentVideo)
                        Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(currentPlayingIndex, currentVideoTime, currentVideo.length)
                    End If
                End If

                Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                Dim command As MySqlCommand
                Dim statement As String

                If newIndex < 0 Then
                    statement = String.Format("" &
                        "UPDATE custom_autopilot SET itemOrder={1}, isDeleted=true, " &
                        "deletedOn=@A, deletedByID=@B, deletedByIP=@C, deletedByName=@D " &
                        "WHERE itemOrder={0};" &
                        "UPDATE custom_autopilot SET itemOrder = itemOrder + 1 WHERE itemOrder > 0 AND  itemOrder <  {0}; " &
                        "UPDATE custom_autopilot SET itemOrder = itemOrder - 1 WHERE itemOrder > 0; ", oldIndex, newIndex)
                    command = New MySqlCommand(statement, connection)
                    command.Parameters.Add("@A", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
                    command.Parameters.Add("@B", MySqlDbType.Int32).Value = user.forumID
                    command.Parameters.Add("@C", MySqlDbType.VarChar).Value = user.ipAddress
                    command.Parameters.Add("@D", MySqlDbType.VarChar).Value = user.name
                ElseIf oldIndex < newIndex Then
                    statement = String.Format("" &
                       "UPDATE custom_autopilot SET itemOrder = -7 WHERE itemOrder = {0};" &
                       "UPDATE custom_autopilot SET itemOrder = itemOrder - 1 WHERE itemOrder >  {0} AND  itemOrder <= {1}; " &
                       "UPDATE custom_autopilot SET itemOrder = {1} WHERE itemOrder = -7;", oldIndex, newIndex)
                    command = New MySqlCommand(statement, connection)
                Else
                    statement = String.Format("" &
                       "UPDATE custom_autopilot SET itemOrder = -7 WHERE itemOrder = {0};" &
                       "UPDATE custom_autopilot SET itemOrder = itemOrder + 1 WHERE itemOrder >= {1} AND  itemOrder <  {0}; " &
                       "UPDATE custom_autopilot SET itemOrder = {1} WHERE itemOrder = -7;", oldIndex, newIndex)
                    command = New MySqlCommand(statement, connection)
                End If

                command.ExecuteNonQuery()
                command.Dispose()
                connection.Close()

                Return True
            End SyncLock
        End If
        Return False
    End Function

    Public Shared Function tryCache(ByVal possibleVideoURL As String) As AutopilotVideo
        Dim newVideo As New AutopilotVideo
        Dim YoutubeMatch As Match = Regex.Match(possibleVideoURL, "youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase)
        If YoutubeMatch.Success Then
            Try
                newVideo.videoID = YoutubeMatch.Groups(1).Value
                If cachedXMLInformation.ContainsKey(newVideo.videoID) Then
                    Return cachedXMLInformation(newVideo.videoID)
                End If
                Dim videoXML As New XmlDocument() : videoXML.Load("https://gdata.youtube.com/feeds/api/videos/" + newVideo.videoID + "?v=2")
                newVideo.title = videoXML.GetElementsByTagName("title")(0).InnerText
                newVideo.author = videoXML.GetElementsByTagName("name")(0).InnerText
                newVideo.length = Integer.Parse(videoXML.GetElementsByTagName("yt:duration")(0).Attributes.GetNamedItem("seconds").Value)
                newVideo.lengthFriendly = TimeSpan.FromSeconds(newVideo.length).ToString

                If videoXML.GetElementsByTagName("yt:state").Count > 0 Then
                    newVideo.ytState = videoXML.GetElementsByTagName("yt:state")(0).InnerText
                End If
                For Each node As XmlNode In videoXML.GetElementsByTagName("yt:accessControl")
                    If node.Attributes.GetNamedItem("action").Value = "embed" Then
                        If node.Attributes.GetNamedItem("permission").Value = "denied" Then
                            newVideo.ytState = "Embedding has been disabled by uploader."
                        End If
                    End If
                Next

                cachedXMLInformation.Add(newVideo.videoID, newVideo)
                Return newVideo
            Catch e As Exception
            End Try
        End If
        Return Nothing
    End Function

    Private Function moveXMLInformationToLive(ByVal videoID As String, ByVal apAdmin As FrontPageUser) As Boolean
        SyncLock videos
            Try

                Dim video As AutopilotVideo = cachedXMLInformation(videoID)
                video.addedBy = apAdmin.name
                Dim statement As String = "INSERT INTO custom_autopilot " &
                    "(addedOn, itemOrder, addedByID, addedByIP, addedByName, videoID, videoTitle, videoAuthor, videoLength, isDeleted) " &
                    "VALUES (@A, @B, @C, @D, @E, @F, @G, @H, @I, @J);"
                Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                Dim command As MySqlCommand = New MySqlCommand(statement, connection)

                command.Parameters.Add("@A", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
                command.Parameters.Add("@B", MySqlDbType.Int32).Value = videos.Count
                command.Parameters.Add("@C", MySqlDbType.Int32).Value = apAdmin.forumID
                command.Parameters.Add("@D", MySqlDbType.VarChar).Value = apAdmin.ipAddress
                command.Parameters.Add("@E", MySqlDbType.VarChar).Value = apAdmin.name
                command.Parameters.Add("@F", MySqlDbType.VarChar).Value = video.videoID
                command.Parameters.Add("@G", MySqlDbType.VarChar).Value = video.title
                command.Parameters.Add("@H", MySqlDbType.VarChar).Value = video.author
                command.Parameters.Add("@I", MySqlDbType.Int32).Value = video.length
                command.Parameters.Add("@J", MySqlDbType.Bit).Value = False
                command.ExecuteNonQuery()

                videos.Add(video)
                cachedXMLInformation.Remove(videoID)

                command.Dispose()
                connection.Close()

                Clients.All.addedItem(video)
                Return True

            Catch ex As Exception
                Return False
            End Try
        End SyncLock
    End Function

    Public Shared Sub init()
        videos.Clear()
        Dim query As String = "SELECT * FROM custom_autopilot WHERE isDeleted = false ORDER BY itemOrder;"
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As MySqlCommand = New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()
        While reader.Read
            If Not reader.GetBoolean(9) Then
                Dim newVideo As New AutopilotVideo
                newVideo.addedBy = reader.GetString(4)
                newVideo.videoID = reader.GetString(5)
                newVideo.title = reader.GetString(6)
                newVideo.author = reader.GetString(7)
                newVideo.length = reader.GetInt32(8)
                newVideo.lengthFriendly = TimeSpan.FromSeconds(newVideo.length).ToString
                videos.Add(newVideo)
            End If
        End While
        reader.Close()
        command.Dispose()
        connection.Close()
        videoTimer.Interval = 1000
        AddHandler videoTimer.Elapsed, New Timers.ElapsedEventHandler(AddressOf Handler)
        ServerPersistance.setDefault("ap_shuffle", False)
        ServerPersistance.setDefault("ap_index", 0)
        ServerPersistance.setDefault("ap_time", 0)
        ServerPersistance.setDefault("recentAPIDs", New List(Of String))
        ServerPersistance.setDefault("skippingLocked", False)
        skippingLocked = ServerPersistance.getField("skippingLocked")
        recentAPIDs = ServerPersistance.getField("recentAPIDs")
        shuffleMode = ServerPersistance.getField("ap_shuffle")
        Dim startIndex As Integer = ServerPersistance.getField("ap_index")
        Dim startTime As Integer = ServerPersistance.getField("ap_time")
        If videos.Count > 0 Then
            If startTime > 0 Then
                setPlayingVideo(startIndex, False)
                videoTimer.Stop()
                currentVideoTime = startTime
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(startIndex, startTime, videos(startIndex).length)
                Dim clientTitle = getClientTitle()
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    user.client.syncAPVideo(currentVideo.videoID, startTime, clientTitle)
                Next
                videoTimer.Start()
            Else
                setPlayingVideo(startIndex, True)
            End If
        End If
    End Sub

    Public Sub serverPing()
        Clients.Caller.clientPing()
    End Sub

    Public Function skipTo(ByVal index As Integer, ByVal seconds As Integer) As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            If index <> currentPlayingIndex Then
                If index < videos.Count Then
                    setPlayingVideo(index, (seconds = 0))
                ElseIf videos.Count > 0 Then
                    setPlayingVideo(0, (seconds = 0))
                    index = 0
                End If
            End If
            If seconds > 0 Then
                videoTimer.Stop()
                currentVideoTime = seconds
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(index, seconds, videos(index).length)
                Dim clientTitle = getClientTitle()
                For Each nextUser As FrontPageUser In Connections.frontPageUsers.ToList
                    nextUser.client.syncAPVideo(currentVideo.videoID, seconds, clientTitle)
                Next
                videoTimer.Start()
            End If
        End If
        Return False
    End Function

    Public Function skipToNext() As Boolean
        Dim user As FrontPageUser = Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminAutoPilot Then
            Dim newIndex = currentPlayingIndex + 1
            If shuffleMode Then newIndex = pickRandomSongIndex()
            If newIndex >= videos.Count Then newIndex = 0
            setPlayingVideo(newIndex, True)
        End If
        Return False
    End Function

    Private Shared Sub setPlayingVideo(ByVal index As Integer, ByVal syncClients As Boolean)
        videoTimer.Stop()
        If index < videos.Count Then
            currentVideo = videos(index)
        ElseIf videos.Count > 0 Then
            currentVideo = videos(0)
            index = 0
        Else
            currentVideo = Nothing
            index = -1
        End If
        currentPlayingIndex = index
        currentVideoTime = 0
        If syncClients And index > -1 Then
            Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext(Of AutopilotHub).Clients.All.syncTime(index, 0, currentVideo.length)
            Dim clientTitle = getClientTitle()
            For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                user.client.syncAPVideo(currentVideo.videoID, 0, clientTitle)
            Next
            skipTracker.Clear()
        End If
        ServerPersistance.setField("ap_index", index)
        recentAPIDs.Add(currentVideo.videoID)
        While recentAPIDs.Count > 20
            recentAPIDs.RemoveAt(0)
        End While
        videoTimer.Start()
    End Sub

    Private Shared Sub Handler(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
        currentVideoTime += 1
        ServerPersistance.setField("ap_time", currentVideoTime)
        If currentVideoTime = currentVideo.length Then
            If videos.Count < 1 Then
                currentVideoTime -= 1
                Return
            Else
                Dim newIndex = currentPlayingIndex + 1
                If shuffleMode Then newIndex = pickRandomSongIndex()
                If newIndex >= videos.Count Then newIndex = 0
                setPlayingVideo(newIndex, True)
            End If
        End If
    End Sub

    Public Shared ReadOnly Property videoID As String
        Get
            If currentVideo Is Nothing Then Return ""
            Return currentVideo.videoID
        End Get
    End Property

    Public Shared ReadOnly Property videoTime As Integer
        Get
            If currentVideo Is Nothing Then Return 0
            Return currentVideoTime
        End Get
    End Property

    Public Shared Function getClientTitle() As String
        Dim clientTitle As String = ""
        If currentVideo.author = "SaveStateHeroes" Then
            clientTitle = "<span style='color: #FFF'>Rerun: </span><span style='color: #AAA'>" + currentVideo.title + "</span>"
        End If
        Return clientTitle
    End Function


    Dim a As Integer = 0

End Class
