Imports Microsoft.VisualBasic
Imports Microsoft.AspNet.SignalR.Hubs
Imports MySql.Data.MySqlClient

Public Class StreamerAppsHub
    Inherits Hub

    Private Shared currentID As Integer = 0
    Public Shared applications As New Dictionary(Of Integer, StreamerApp)

    Public Function getClientUsername() As String
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            Return user.name
        Else
            Return Nothing
        End If
    End Function

    Public Function canApply() As Boolean
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            For Each app As StreamerApp In applications.Values
                If app.userID = user.forumID Then Return False
            Next
            Return True
        Else
            Return False
        End If
    End Function

    Public Function hasApplication() As Boolean
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            For Each app As StreamerApp In applications.Values
                If app.userID = user.forumID Then Return True
            Next
            Return False
        Else
            Return False
        End If
    End Function

    Public Function canAdmin() As Boolean
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            Return user.privileges.canAdminApplications
        Else
            Return False
        End If
    End Function

    Public Function queryOpenApplications() As String
        Return queryApplications("open:")
    End Function

    Public Function queryClosedApplications() As String
        Return queryApplications("closed:") + queryApplications("removed:")
    End Function

    Public Function queryTrialApplications() As String
        Return queryApplications("trial")
    End Function

    Public Function queryTestApplications() As String
        Return queryApplications("test")
    End Function

    Public Function queryArchivedApplications() As String
        Return queryApplications("archived:")
    End Function

    Private Function queryApplications(requieredStatus As String) As String
        SyncLock applications
            Dim builder As New StringBuilder
            For Each app As StreamerApp In applications.Values
                If app.status.StartsWith(requieredStatus) Then
                    builder.Append("<div class='appButton' data-appid='")
                    builder.Append(app.appID)
                    builder.Append("'>")
                    builder.Append(app.username)
                    builder.Append("</div>")
                End If
            Next
            Return builder.ToString
        End SyncLock
    End Function

    Public Function queryAppData(id As Integer) As StreamerApp
        Dim tryGet As StreamerApp = Nothing
        applications.TryGetValue(id, tryGet)
        If tryGet IsNot Nothing AndAlso Not canAdmin() Then
            Return tryGet.cloneWithoutSensitiveInformation()
        End If
        Return tryGet
    End Function

    Public Function postNewApplication(rating As Double, clientTimezone As String, clientAge As String, streamProgram As String, essay1 As String, essay2 As String, dxDiag As String, lsUsername As String, skypeName As String) As Integer
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            Dim newApp As New StreamerApp
            newApp.lastUpdatedBy = user.name
            newApp.status = "open:new"
            newApp.userID = user.forumID
            newApp.username = user.name
            newApp.submitDate = Now.ToUniversalTime
            newApp.submitDateString = newApp.submitDate.ToString + " UTC"
            newApp.approvalDate = newApp.submitDate
            newApp.approvalDateString = newApp.approvalDate.ToString + " UTC"
            newApp.trialEndedDate = newApp.submitDate
            newApp.trialEndedDateString = newApp.approvalDate.ToString + " UTC"
            newApp.timezone = clientTimezone
            newApp.age = clientAge
            If clientAge = "21 " OrElse clientAge = "21" Then
                newApp.age = "21+"
            End If
            newApp.program = streamProgram
            newApp.essay1 = essay1
            newApp.essay2 = essay2
            newApp.dxDiag = dxDiag
            newApp.lsUsername = lsUsername
            newApp.skypeName = skypeName
            newApp.connectionRating = Math.Round(rating, 2).ToString
            SyncLock applications
                newApp.appID = currentID
                applications.Add(currentID, newApp)
                currentID += 1
            End SyncLock
            Dim builder As New StringBuilder
            builder.Append("<div class='appButton' data-appid='")
            builder.Append(newApp.appID)
            builder.Append("'>")
            builder.Append(newApp.username)
            builder.Append("</div>")
            Clients.All.postNewApp(builder.ToString)
            updateDBRecord(newApp.appID)
            Return newApp.appID
        Else
            Return -1
        End If
    End Function

    Public Function adminAction(appID As Integer, action As String) As Boolean
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing AndAlso user.privileges.canAdminApplications Then
            Dim tryGet As StreamerApp = Nothing
            applications.TryGetValue(appID, tryGet)
            If tryGet IsNot Nothing Then
                If action = "accept" Then
                    If tryGet.status.StartsWith("open:") Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "test"
                        Clients.All.updateAppStatus(appID, "test")
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.postSystemMessage("Your application has been approved.")
                        End If
                    End If
                ElseIf action = "deny" Then
                    If tryGet.status.StartsWith("open:") Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "closed:badApp"
                        Clients.All.updateAppStatus(appID, "closed")
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.postSystemMessage("The status of your application has changed.")
                        End If
                    End If
                ElseIf action = "fail" Then
                    If tryGet.status = "test" Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "closed:failedTS"
                        Clients.All.updateAppStatus(appID, "closed")
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.postSystemMessage("The status of your application has changed.")
                        End If
                    End If
                ElseIf action = "reopen" Then
                    If (tryGet.status.StartsWith("closed:") OrElse tryGet.status.StartsWith("removed:")) AndAlso tryGet.status <> "archived:fullstreamer" Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "open:reopened"
                        Clients.All.updateAppStatus(appID, "open")
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.postSystemMessage("Your application has been reopened.")
                        End If
                    End If
                ElseIf action = "promote" Then
                    If tryGet.status = "trial" Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "archived:fullstreamer"
                        tryGet.trialEndedDate = Now.ToUniversalTime
                        tryGet.trialEndedDateString = tryGet.trialEndedDate.ToString + " UTC"
                        Clients.All.updateAppStatus(appID, "archived")
                        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                        Dim command As MySqlCommand = New MySqlCommand("INSERT INTO bb_user_group VALUES (16, @userID, 0, 0);", connection)
                        command.Parameters.Add("@userID", MySqlDbType.Int32).Value = tryGet.userID
                        command.ExecuteNonQuery()
                        command.Dispose()
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.privileges.refreshData()
                            newStreamer.postSystemMessage("You have been promoted to full streamer.")
                        End If
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Streamer, tryGet.username + " has been promoted to full streamer.")
                    End If
                ElseIf action = "makeTrial" Then
                    If tryGet.status = "test" Then
                        tryGet.lastUpdatedBy = user.name
                        tryGet.status = "trial"
                        tryGet.approvalDate = Now.ToUniversalTime
                        tryGet.approvalDateString = tryGet.approvalDate.ToString + " UTC"
                        Clients.All.updateAppStatus(appID, "trial")
                        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                        Dim command As MySqlCommand = New MySqlCommand("INSERT INTO bb_user_group VALUES (11, @userID, 0, 0), (12, @userID, 0, 0);", connection)
                        command.Parameters.Add("@userID", MySqlDbType.Int32).Value = tryGet.userID
                        command.ExecuteNonQuery()
                        command.Dispose()
                        connection.Close()
                        updateDBRecord(appID)
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.privileges.refreshData()
                            newStreamer.postSystemMessage("You have been promoted to trial streamer.")
                        End If
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Streamer, tryGet.username + " has been approved to stream.")
                    End If
                ElseIf action = "removed:inactive" OrElse action = "removed:bad" OrElse action = "removed:conduct" Then
                    If tryGet.status = "archived:fullstreamer" OrElse tryGet.status = "trial" Then
                        tryGet.lastUpdatedBy = user.name
                        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                        Dim command As MySqlCommand = New MySqlCommand("DELETE FROM bb_user_group WHERE (group_id=11 OR group_id=12 OR group_id=16) AND (user_id=@userID);", connection)
                        command.Parameters.Add("@userID", MySqlDbType.Int32).Value = tryGet.userID
                        command.ExecuteNonQuery()
                        command.Dispose()
                        connection.Close()
                        Dim newStreamer As UserSystem.FrontPageUser = UserSystem.Connections.matchFirst(UserSystem.Connections.frontPageUsers, tryGet.username)
                        If newStreamer IsNot Nothing Then
                            newStreamer.privileges.refreshData()
                            newStreamer.postSystemMessage("Your streaming rights have been removed.")
                        End If
                        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Streamer, tryGet.username + " is no longer a streamer.")
                        tryGet.status = action
                        Clients.All.updateAppStatus(appID, "removed")
                        updateDBRecord(appID)

                    End If
                ElseIf action = "delete" Then
                    If tryGet.status.StartsWith("closed:") OrElse tryGet.status.StartsWith("removed:") Then
                        tryGet.lastUpdatedBy = user.name
                        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                        Dim command As MySqlCommand = New MySqlCommand("DELETE FROM custom_streamer_apps WHERE appID=@appID;", connection)
                        command.Parameters.Add("@appID", MySqlDbType.Int32).Value = tryGet.appID
                        command.ExecuteNonQuery()
                        command.Dispose()
                        connection.Close()
                        applications.Remove(tryGet.appID)
                        Clients.All.updateAppStatus(appID, "deleted")
                    End If
                End If
            End If
        End If
        Return False
    End Function

    Public Sub updateDBRecord(appID As Integer)
        Dim app As StreamerApp = Nothing
        applications.TryGetValue(appID, app)
        If app Is Nothing Then Return
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As MySqlCommand = New MySqlCommand("" + _
            "INSERT INTO custom_streamer_apps " + _
            "VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17) " + _
            "ON DUPLICATE KEY UPDATE " + _
            "appID=@1, userID=@2, username=@3, lastUpdatedBy=@4, status=@5, " + _
            "submitDate=@6, approvalDate=@7, trialEndedDate=@8, timezone=@9, age=@10, " + _
            "program=@11, essay1=@12, essay2=@13, dxDiag=@14, lsUsername=@15, " &
            "connectionRating=@16, skypeName=@17;", connection)
        command.Parameters.Add("@1", MySqlDbType.Int32).Value = app.appID
        command.Parameters.Add("@2", MySqlDbType.Int32).Value = app.userID
        command.Parameters.Add("@3", MySqlDbType.VarChar).Value = app.username
        command.Parameters.Add("@4", MySqlDbType.VarChar).Value = app.lastUpdatedBy
        command.Parameters.Add("@5", MySqlDbType.VarChar).Value = app.status
        command.Parameters.Add("@6", MySqlDbType.Int64).Value = app.submitDate.ToFileTimeUtc
        command.Parameters.Add("@7", MySqlDbType.Int64).Value = app.approvalDate.ToFileTimeUtc
        command.Parameters.Add("@8", MySqlDbType.Int64).Value = app.trialEndedDate.ToFileTimeUtc
        command.Parameters.Add("@9", MySqlDbType.VarChar).Value = app.timezone
        command.Parameters.Add("@10", MySqlDbType.VarChar).Value = app.age
        command.Parameters.Add("@11", MySqlDbType.Blob).Value = app.program
        command.Parameters.Add("@12", MySqlDbType.Blob).Value = app.essay1
        command.Parameters.Add("@13", MySqlDbType.Blob).Value = app.essay2
        command.Parameters.Add("@14", MySqlDbType.Blob).Value = app.dxDiag
        command.Parameters.Add("@15", MySqlDbType.VarChar).Value = app.lsUsername
        command.Parameters.Add("@16", MySqlDbType.VarChar).Value = app.connectionRating
        command.Parameters.Add("@17", MySqlDbType.VarChar).Value = app.skypeName
        command.ExecuteNonQuery()
        command.Dispose()
        connection.Close()
    End Sub

    Public Shared Sub init()
        SyncLock applications
            applications.Clear()
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As MySqlCommand = New MySqlCommand("SELECT * FROM custom_streamer_apps;", connection)
            Dim reader As MySqlDataReader = command.ExecuteReader()
            While reader.Read()
                Dim newApp As New StreamerApp
                newApp.appID = reader.GetInt32(0)
                newApp.userID = reader.GetInt32(1)
                newApp.username = reader.GetString(2)
                newApp.lastUpdatedBy = reader.GetString(3)
                newApp.status = reader.GetString(4)
                newApp.submitDate = Date.FromFileTimeUtc(reader.GetInt64(5))
                newApp.approvalDate = Date.FromFileTimeUtc(reader.GetInt64(6))
                newApp.trialEndedDate = Date.FromFileTimeUtc(reader.GetInt64(7))
                newApp.timezone = reader.GetString(8)
                newApp.age = reader.GetString(9)
                newApp.program = reader.GetString(10)
                newApp.essay1 = reader.GetString(11)
                newApp.essay2 = reader.GetString(12)
                newApp.dxDiag = reader.GetString(13)
                newApp.lsUsername = reader.GetString(14)
                newApp.connectionRating = reader.GetString(15)
                newApp.skypeName = reader.GetString(16)
                newApp.submitDateString = newApp.submitDate.ToString + " UTC"
                newApp.approvalDateString = newApp.approvalDate.ToString + " UTC"
                newApp.trialEndedDateString = newApp.trialEndedDate.ToString + " UTC"
                applications.Add(newApp.appID, newApp)
                If newApp.appID >= currentID Then currentID = newApp.appID + 1
            End While
            reader.Close()
            command.Dispose()
            connection.Close()
        End SyncLock
    End Sub

    Public Sub serverPing()
        Clients.Caller.clientPing()
    End Sub

    Public Sub serverPingWithData(packet As Object)
    End Sub

    Public Function updateConnectionRating(rating As Double) As Boolean
        Dim user As UserSystem.FrontPageUser = UserSystem.Connections.getSessionUser
        If user IsNot Nothing Then
            Dim newRating As String = Math.Round(rating, 2).ToString
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As MySqlCommand = New MySqlCommand("UPDATE custom_streamer_apps SET connectionRating=@rating WHERE userID=@id;", connection)
            command.Parameters.Add("@rating", MySqlDbType.VarChar).Value = newRating
            command.Parameters.Add("@id", MySqlDbType.Int32).Value = user.name
            command.ExecuteNonQuery()
            command.Dispose()
            connection.Close()
            For Each app As StreamerApp In applications.Values
                If app.userID = user.forumID Then
                    app.connectionRating = newRating
                End If
            Next
            Return True
        End If
        Return False
    End Function

End Class
