Imports Microsoft.VisualBasic
Imports System.Xml
Imports MySql.Data.MySqlClient
Imports UserSystem

Public Class ChatProcessor

    Public Shared systemUser As New SystemUser

    Private Const maxCacheRequest As Integer = 150
    Private Const maxCacheStore As Integer = 400 'Should be at least double maxCacheRequest
    Private Const maxCharactersPerMessage As Integer = 400
    Private Const maxCharactersInBuiltMessage As Integer = 7000

    Private Shared currentID As Integer
    Private Shared cachedMessages As New List(Of ChatMessage)

    Public Shared Function postNewMessage(sender As FrontPageUser, recipients As OnlineUser(), type As ChatMessage.MessageType, initialText As String) As Boolean
        If sender Is Nothing Then sender = systemUser
        If sender <> systemUser Then
            If sender.isFloodingChat Then
                sender.postSystemMessage("message suppressed; flooding")
                Return False
            End If
            If Infractions.isMuted(sender) Then
                sender.postSystemMessage(String.Format("muted ({0})", Utils.getReadableTimeRemaining(Infractions.muteEndsAt(sender))))
                Return False
            End If
            If Infractions.isBanned(sender) Then
                Return False
            End If
            If initialText.Length > maxCharactersPerMessage Then
                sender.postSystemMessage("message suppressed; character limit (" & maxCharactersPerMessage & ")")
                Return False
            End If
        End If

        If type <> ChatMessage.MessageType.Whisper AndAlso type <> ChatMessage.MessageType.WhisperEcho AndAlso Infractions.isSilentBanned(sender) Then
            recipients = {sender}
        End If

        currentID += 1
        Dim message As New ChatMessage(currentID, sender, recipients, type, initialText)

        If message.html.Length > maxCharactersInBuiltMessage Then
            sender.postErrorMessage("message suppressed; too large")
            Return True
        End If

        If sender.recentChatHistory.Count > 0 Then
            If String.Equals(sender.recentChatHistory(sender.recentChatHistory.Count - 1).originalInput, message.originalInput) Then
                Return True
            End If
        End If
        sender.recentChatHistory.Add(message)
        If sender.recentChatHistory.Count > 9 Then
            sender.recentChatHistory.RemoveAt(0)
        End If

        cachedMessages.Add(message)
        If cachedMessages.Count >= maxCacheStore Then
            cachedMessages.RemoveRange(0, cachedMessages.Count - maxCacheRequest)
        End If

        If type <> ChatMessage.MessageType.Whisper AndAlso type <> ChatMessage.MessageType.WhisperEcho AndAlso Infractions.isSilentBanned(sender) Then
            sender.postMessage(message.html)
        ElseIf recipients IsNot Nothing Then
            For Each recipient As OnlineUser In recipients
                If Not recipient.isBlocked(sender.name) Then
                    recipient.postMessage(message.html)
                    If type = ChatMessage.MessageType.Whisper Then
                        recipient.client.audioWhisperIn()
                    End If
                End If
            Next
        Else
            If type = ChatMessage.MessageType.Channel_Streamer Then
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If user.privileges.isStreamer OrElse user.privileges.canModChat Then
                        If Not user.isBlocked(sender.name) Then
                            user.postMessage(message.html)
                        End If
                    End If
                Next
            ElseIf type = ChatMessage.MessageType.Channel_Bumper Then
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If user.privileges.isBumper OrElse user.privileges.canModChat Then
                        If Not user.isBlocked(sender.name) Then
                            user.postMessage(message.html)
                        End If
                    End If
                Next
            ElseIf type = ChatMessage.MessageType.Channel_Mod Then
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If user.privileges.canModChat Then
                        If Not user.isBlocked(sender.name) Then
                            user.postMessage(message.html)
                        End If
                    End If
                Next
            ElseIf type = ChatMessage.MessageType.Channel_Officer Then
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If user.privileges.isOfficer Then
                        user.postMessage(message.html)
                    End If
                Next
            Else
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    If Not user.isBlocked(sender.name) Then
                        user.postMessage(message.html)
                    End If
                Next
            End If
        End If

        Try
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As MySqlCommand = New MySqlCommand("" &
            "INSERT INTO custom_chat_history " &
            "(id, type, time, weight, sender, senderIP, recipients, html, originalInput, isDeleted) VALUES " &
            "(@id, @type, @time, @weight, @sender, @senderIP, @recipients, @html, @originalInput, false);", connection)
            command.Parameters.Add("@id", MySqlDbType.Int32).Value = message.id
            command.Parameters.Add("@type", MySqlDbType.Int32).Value = message.type
            command.Parameters.Add("@time", MySqlDbType.Int64).Value = message.time.ToFileTimeUtc
            command.Parameters.Add("@weight", MySqlDbType.Int64).Value = message.weight
            command.Parameters.Add("@sender", MySqlDbType.VarChar).Value = message.sender
            command.Parameters.Add("@senderIP", MySqlDbType.VarChar).Value = message.senderIP
            command.Parameters.Add("@recipients", MySqlDbType.VarChar).Value = message.recipients
            command.Parameters.Add("@html", MySqlDbType.Blob).Value = message.html
            command.Parameters.Add("@originalInput", MySqlDbType.Blob).Value = message.originalInput
            command.ExecuteNonQuery()
            command.Dispose()
            connection.Close()
        Catch ex As Exception
            Utils.logError(ex)
        End Try

        Return True
    End Function

    Public Shared Function generateQuote(id As Integer, requestedBy As OnlineUser) As String
        Dim message As ChatMessage = getStoredMessage(id)
        If message Is Nothing OrElse Not message.isRecipient(requestedBy) Then Return ""
        Dim formattedMessage As String = Regex.Replace(message.html, "(?<=chatMessage'|chatMessage"")(.*?)(</span)(?=><)", String.Empty)
        Dim finalText As String = ""
        finalText &= "<div class='userQuote'>" &
                     "      <div class='quoteTitle'>Quote :: " &
                     "<span class='userDateTime' data-time='" & Now.ToUniversalTime.ToString & " UTC'></span></div>" &
                     "      <div class='quoteBox'>" & formattedMessage & "</div>" &
                     "</div>"
        Return finalText
    End Function

    Public Shared Sub delete(chatMod As OnlineUser, id As Integer)
        If id < 0 Or id > currentID Then
            chatMod.postErrorMessage("message with given id not found")
            Return
        End If
        Dim messageInDB As Boolean = True
        For Each message As ChatMessage In cachedMessages.ToList
            If message.id = id Then
                If Not chatMod.privileges.isOfficer AndAlso message.type = ChatMessage.MessageType.ModAction Then
                    chatMod.postErrorMessage("you must be an officer to delete a global broadcast")
                    Return
                End If
                message.deleted(chatMod)
                Exit For
            End If
        Next
        Dim success As Boolean = False
        If messageInDB Then
            Try
                Dim update As String = "UPDATE custom_chat_history SET isDeleted=@VALUE, deletedBy=@BY, deletedByIP=@BYIP WHERE id=@ID;"
                Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                Dim command As MySqlCommand = New MySqlCommand(update, connection)
                command.Parameters.Add("@VALUE", MySqlDbType.Bit).Value = True
                command.Parameters.Add("@BY", MySqlDbType.VarChar).Value = chatMod.name
                command.Parameters.Add("@BYIP", MySqlDbType.VarChar).Value = chatMod.ipAddress
                command.Parameters.Add("@ID", MySqlDbType.Int32).Value = id
                command.ExecuteNonQuery()
                command.Dispose()
                connection.Close()
                success = True
            Catch ex As Exception
                Utils.logError(ex)
            End Try
        End If
        If success Or Not messageInDB Then
            For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                user.client.deleteMessage(id)
            Next
            chatMod.postSystemMessage("deleted post id::" & id)
        Else
            chatMod.postErrorMessage("message could not be deleted")
        End If
    End Sub

    Public Shared Sub deleteAll(caller As OnlineUser, match As String)
        If match = "[system]" And Not caller.privileges.isDeveloper Then
            caller.postErrorMessage("delete all is not allowed on system posts")
            Return
        End If
        Dim actualObjectName As String = Connections.getObjectName(match)
        If actualObjectName IsNot Nothing Then match = actualObjectName

        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Try
            Dim senders As New List(Of String)
            Dim command As MySqlCommand = New MySqlCommand("SELECT sender FROM custom_chat_history WHERE (sender=@match OR senderIP=@match) AND (isDeleted=false) GROUP BY sender;", connection)
            command.Parameters.Add("@match", MySqlDbType.VarChar).Value = match
            Dim reader As MySqlDataReader = command.ExecuteReader()
            While reader.Read
                senders.Add(reader.GetString(0))
            End While
            reader.Close()
            command.Dispose()

            command = New MySqlCommand("UPDATE custom_chat_history SET isDeleted=@VALUE, deletedBy=@BY, deletedByIP=@BYIP WHERE sender=@sender;", connection)
            command.Prepare()
            command.Parameters.Add("@VALUE", MySqlDbType.Bit)
            command.Parameters.Add("@BY", MySqlDbType.VarChar)
            command.Parameters.Add("@BYIP", MySqlDbType.VarChar)
            command.Parameters.Add("@sender", MySqlDbType.VarChar)
            For Each sender As String In senders
                command.Parameters("@VALUE").Value = True
                command.Parameters("@BY").Value = caller.name
                command.Parameters("@BYIP").Value = caller.ipAddress
                command.Parameters("@sender").Value = sender
                command.ExecuteNonQuery()
                For Each message As ChatMessage In cachedMessages.ToList
                    If message.sender = sender Then
                        message.deleted(caller)
                    End If
                Next
            Next
            Dim deleteAllNotification As String = caller.displayName & " deleted all messages by "
            For Each sender As String In senders
                For Each user As FrontPageUser In Connections.frontPageUsers.ToList
                    user.client.deleteMessagesBy(sender)
                Next
                deleteAllNotification &= sender & ", "
            Next
            deleteAllNotification = deleteAllNotification.Substring(0, deleteAllNotification.Length - 2)
            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.ModAction, deleteAllNotification)
        Catch ex As Exception
            Utils.logError(ex)
            caller.postErrorMessage("messages not deleted; db error")
            Return
        End Try
        connection.Close()
    End Sub

    Public Shared Sub listCachedDeletes(caller As OnlineUser)
        Dim foundMessage As Boolean
        Dim builder As New StringBuilder
        builder.Append("listing cached deletions...<br />")
        For Each message As ChatMessage In cachedMessages.ToList
            If message.isDeleted Then
                builder.Append("id::")
                builder.Append(message.id)
                builder.Append(" poster::'")
                builder.Append(message.sender)
                builder.Append("' deleter->'")
                builder.Append(message.deletedBy)
                builder.Append("'")
                builder.Append("<br />")
                foundMessage = True
            End If
        Next
        If foundMessage Then
            caller.postSystemMessage(builder.ToString)
        Else
            caller.postSystemMessage("no cached deletions exist")
        End If
    End Sub

    Public Shared Function getCachedMessages(forUser As OnlineUser) As String
        Dim builder As New StringBuilder
        Dim messagesToRetreive As Integer = cachedMessages.Count
        If messagesToRetreive > maxCacheRequest Then messagesToRetreive = maxCacheRequest
        If messagesToRetreive < 1 Then Return Nothing
        For iteration = cachedMessages.Count - messagesToRetreive To cachedMessages.Count - 1
            Dim message As ChatMessage = cachedMessages.Item(iteration)
            If Not message.isDeleted AndAlso message.isRecipient(forUser) AndAlso Not forUser.isBlocked(message.sender) Then
                builder.Append(message.html)
            End If
        Next
        'IO.File.WriteAllText("E:\history.html", "<html><body style='background-color: #000;'>" & builder.Replace("display: none", "").ToString & "</body></html>")
        Return builder.ToString()
    End Function

    Public Shared Sub init()
        cachedMessages.Clear()

        'Get highest message id
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As New MySqlCommand("SELECT MAX(id) FROM custom_chat_history", connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()
        If reader.Read Then
            If Not reader.IsDBNull(0) Then
                currentID = reader.GetInt32(0) + 1
            Else
                currentID = 0
            End If
        End If
        reader.Close()
        command.Dispose()

        Dim minId As Integer = currentID - maxCacheRequest + 1
        If minId < 0 Then minId = 0
        Dim query As String = "SELECT id, type, time, weight, sender, senderIP, recipients, " + _
            "html, originalInput, isDeleted, deletedBy, deletedByIP FROM custom_chat_history WHERE " + _
            "`id` BETWEEN " + minId.ToString + " AND " + currentID.ToString + ";"
        command = New MySqlCommand(query, connection)
        reader = command.ExecuteReader()
        While reader.Read
            Dim deletedBy As String = Nothing
            Dim deletedByIP As String = Nothing
            If Not reader.IsDBNull(9) Then deletedBy = reader.GetString(9)
            If Not reader.IsDBNull(10) Then deletedByIP = reader.GetString(10)
            Dim message As New ChatMessage( _
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt64(2), reader.GetInt32(3), _
                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8), reader.GetBoolean(9), deletedBy, deletedByIP)
            cachedMessages.Add(message)
        End While
        reader.Close()
        command.Dispose()
        connection.Close()
    End Sub

    Public Shared Function getStoredMessage(databaseID As Integer) As ChatMessage
        getStoredMessage = Nothing
        Dim query As String = "SELECT id, type, time, weight, sender, senderIP, recipients, " + _
            "html, originalInput, isDeleted, deletedBy, deletedByIP FROM custom_chat_history WHERE " + _
            "`id`=@id;"
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim Command As MySqlCommand = New MySqlCommand(query, connection)
        Command.Parameters.Add("@id", MySqlDbType.Int32).Value = databaseID
        Dim reader As MySqlDataReader = Command.ExecuteReader()
        If reader.Read Then
            Dim deletedBy As String = Nothing
            Dim deletedByIP As String = Nothing
            If Not reader.IsDBNull(9) Then deletedBy = reader.GetString(9)
            If Not reader.IsDBNull(10) Then deletedByIP = reader.GetString(10)
            Dim message As New ChatMessage( _
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt64(2), reader.GetInt32(3), _
                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8), reader.GetBoolean(9), deletedBy, deletedByIP)
            getStoredMessage = message
        End If
        reader.Close()
        Command.Dispose()
        connection.Close()
    End Function

End Class
