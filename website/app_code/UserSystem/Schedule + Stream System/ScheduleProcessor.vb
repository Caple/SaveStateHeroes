Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient
Imports UserSystem

Public Class ScheduleProcessor

    Private Shared eventsByID As New Dictionary(Of Integer, ScheduleEvent)
    Private Shared currentEventID As Integer = 0

    Public Shared Function findCurrentEvent() As ScheduleEvent
        Dim thisDate = Now.ToUniversalTime
        For Each nextEvent As ScheduleEvent In eventsByID.Values
            If Not nextEvent.isDeleted Then
                If Date.Compare(thisDate, nextEvent.startTime) > 0 AndAlso Date.Compare(thisDate, nextEvent.endTime) < 0 Then
                    Return nextEvent
                End If
            End If
        Next
        Return Nothing
    End Function

    Public Shared Function findEventsAtRange(starts As Date, ends As Date) As List(Of ScheduleEvent)
        findEventsAtRange = New List(Of ScheduleEvent)
        starts = starts.ToUniversalTime
        ends = ends.ToUniversalTime
        For Each nextEvent As ScheduleEvent In eventsByID.Values
            If Not nextEvent.isDeleted Then
                If Date.Compare(starts, nextEvent.endTime) < 0 AndAlso Date.Compare(ends, nextEvent.startTime) > 0 Then
                    findEventsAtRange.Add(nextEvent)
                End If
            End If
        Next
        Return findEventsAtRange
    End Function

    Public Shared Function getEvents(startTime As Date, endTime As Date) As ScheduleEvent.InnerEventObject()
        Dim eventList As New List(Of ScheduleEvent.InnerEventObject)
        For Each nextEvent As ScheduleEvent In eventsByID.Values
            If Not nextEvent.isDeleted Then
                If Date.Compare(startTime, nextEvent.startTime) < 0 AndAlso Date.Compare(endTime, nextEvent.startTime) > 0 Then
                    eventList.Add(nextEvent.innerEvent)
                End If
            End If
        Next
        Return eventList.ToArray
    End Function

    Public Shared Sub extendEvent(existingEvent As ScheduleEvent, minutes As Integer)
        existingEvent.endTime = existingEvent.endTime.AddMinutes(minutes)
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As MySqlCommand = New MySqlCommand("" &
           "UPDATE custom_stream_schedule " &
           "SET endTime=@ends " &
           "WHERE eventID=@ID", connection)
        command.Parameters.Add("@ends", MySqlDbType.VarChar).Value = existingEvent.endTime.ToFileTimeUtc
        command.Parameters.Add("@ID", MySqlDbType.Int32).Value = existingEvent.eventID
        command.ExecuteNonQuery()
        command.Dispose()
        connection.Close()
        ScheduleHub.getClients.All.editedEventTime(existingEvent.innerEvent.id, existingEvent.innerEvent.start, existingEvent.innerEvent.ends)
    End Sub

    Public Shared Function addEvent(user As OnlineUser, starts As Date, ends As Date, description As String) As Boolean
        If user.privileges.canEditSchedule Then
            For Each nextEvent As ScheduleEvent In findEventsAtRange(starts, ends)
                If nextEvent IsNot Nothing AndAlso Not nextEvent.isDeleted Then Return False
            Next
            currentEventID += 1
            Dim thisID As Integer = currentEventID
            Dim newEvent As New ScheduleEvent(thisID, user, starts, ends, description)
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As MySqlCommand = New MySqlCommand("" &
               "INSERT INTO custom_stream_schedule " &
               "(eventID, addedByIP, addedBy, description, startTime, endTime, isDeleted) VALUES " &
               "(@ID, @ip, @name, @desc, @start, @end, false);", connection)
            command.Parameters.Add("@ID", MySqlDbType.Int32).Value = thisID
            command.Parameters.Add("@ip", MySqlDbType.VarChar).Value = user.ipAddress
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.name
            command.Parameters.Add("@desc", MySqlDbType.VarChar).Value = description
            command.Parameters.Add("@start", MySqlDbType.Int64).Value = starts.ToFileTimeUtc
            command.Parameters.Add("@end", MySqlDbType.Int64).Value = ends.ToFileTimeUtc
            command.ExecuteNonQuery()
            command.Dispose()
            connection.Close()
            eventsByID.Add(thisID, newEvent)
            If StreamProcessor.isLive AndAlso StreamProcessor.streamEvent Is Nothing Then
                Dim timeNow As Date = Now.ToUniversalTime
                If Date.Compare(timeNow, starts) > 0 AndAlso Date.Compare(timeNow, ends) < 0 Then
                    'TODO: StreamProcessor.updateEvent(newEvent)
                End If
            End If

            Return True
        End If
        Return False
    End Function

    Public Shared Function editEvent(user As OnlineUser, eventID As Integer, starts As Date, ends As Date) As Boolean
        If user.privileges.canAdminSchedule OrElse user.privileges.canEditSchedule Then
            For Each nextEvent As ScheduleEvent In findEventsAtRange(starts, ends)
                If nextEvent IsNot Nothing AndAlso Not nextEvent.isDeleted AndAlso nextEvent.eventID <> eventID Then Return False
            Next
            Dim existingEvent As ScheduleEvent = Nothing
            eventsByID.TryGetValue(eventID, existingEvent)
            If existingEvent IsNot Nothing Then
                If user.privileges.canAdminSchedule OrElse user.isMatch(existingEvent.addedBy) Then
                    existingEvent.editedBy = user.name
                    existingEvent.editedByIP = user.ipAddress
                    existingEvent.startTime = starts
                    existingEvent.endTime = ends
                    Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                    Dim command As MySqlCommand = New MySqlCommand("" &
                       "UPDATE custom_stream_schedule " &
                       "SET editedByIP=@ip, editedBy=@name, startTime=@starts, endTime=@ends " &
                       "WHERE eventID=@ID", connection)
                    command.Parameters.Add("@ip", MySqlDbType.VarChar).Value = user.ipAddress
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.name
                    command.Parameters.Add("@starts", MySqlDbType.VarChar).Value = starts.ToFileTimeUtc
                    command.Parameters.Add("@ends", MySqlDbType.VarChar).Value = ends.ToFileTimeUtc
                    command.Parameters.Add("@ID", MySqlDbType.Int32).Value = eventID
                    command.ExecuteNonQuery()
                    command.Dispose()
                    connection.Close()
                    If StreamProcessor.isLive Then
                        If StreamProcessor.streamEvent Is Nothing Then
                            Dim timeNow As Date = Now.ToUniversalTime
                            If Date.Compare(timeNow, starts) > 0 AndAlso Date.Compare(timeNow, ends) < 0 Then
                                'TODO: StreamProcessor.updateEvent(existingEvent)
                            End If
                        ElseIf StreamProcessor.streamEvent Is existingEvent Then
                            Dim timeNow As Date = Now.ToUniversalTime
                            If Not (Date.Compare(timeNow, starts) > 0 AndAlso Date.Compare(timeNow, ends) < 0) Then
                                'TODO: StreamProcessor.updateEvent(Nothing)
                            End If
                        End If
                    End If
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Public Shared Function editEvent(user As OnlineUser, eventID As Integer, description As String) As String
        If user.privileges.canAdminSchedule OrElse user.privileges.canEditSchedule Then
            Dim existingEvent As ScheduleEvent = Nothing
            eventsByID.TryGetValue(eventID, existingEvent)
            If existingEvent IsNot Nothing Then
                If user.privileges.canAdminSchedule OrElse user.isMatch(existingEvent.addedBy) Then
                    existingEvent.editedBy = user.name
                    existingEvent.editedByIP = user.ipAddress
                    existingEvent.description = description
                    Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                    Dim command As MySqlCommand = New MySqlCommand("" &
                       "UPDATE custom_stream_schedule " &
                       "SET editedByIP=@ip, editedBy=@name, description=@description " &
                       "WHERE eventID=@ID", connection)
                    command.Parameters.Add("@ip", MySqlDbType.VarChar).Value = user.ipAddress
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.name
                    command.Parameters.Add("@description", MySqlDbType.VarChar).Value = description
                    command.Parameters.Add("@ID", MySqlDbType.Int32).Value = eventID
                    command.ExecuteNonQuery()
                    command.Dispose()
                    connection.Close()
                    Return existingEvent.innerEvent.title
                End If
            End If
        End If
        Return Nothing
    End Function

    Public Shared Function deleteEvent(user As OnlineUser, eventID As Integer) As Boolean
        If user.privileges.canEditSchedule Then
            Dim existingEvent As ScheduleEvent = Nothing
            eventsByID.TryGetValue(eventID, existingEvent)
            If existingEvent IsNot Nothing Then
                If user.privileges.canAdminSchedule OrElse user.isMatch(existingEvent.addedBy) Then
                    existingEvent.delete(user)
                    Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                    Dim command As MySqlCommand = New MySqlCommand("" &
                       "UPDATE custom_stream_schedule " &
                       "SET isDeleted=true, deletedByIP=@ip, deletedBy=@name " &
                       "WHERE eventID=@ID", connection)
                    command.Parameters.Add("@ip", MySqlDbType.VarChar).Value = user.ipAddress
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.name
                    command.Parameters.Add("@ID", MySqlDbType.Int32).Value = eventID
                    command.ExecuteNonQuery()
                    command.Dispose()
                    connection.Close()
                    If StreamProcessor.isLive AndAlso StreamProcessor.streamEvent Is existingEvent Then
                        'TODO: StreamProcessor.updateEvent(Nothing)
                    End If
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Public Shared Sub init()

        eventsByID.Clear()

        'Get highest message id
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()

        Dim cacheLimitLow As Long = Now.AddDays(-8).ToFileTimeUtc
        Dim cacheLimitHigh As Long = Now.AddDays(8).ToFileTimeUtc
        Dim query As String = "SELECT * FROM custom_stream_schedule"
        Dim command As New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()
        While reader.Read
            Dim nextID As Integer = reader.GetInt32(0)
            Dim newEvent As New ScheduleEvent(nextID, reader.GetString(1), reader.GetString(2))
            If Not reader.IsDBNull(3) Then newEvent.editedByIP = reader.GetString(3)
            If Not reader.IsDBNull(4) Then newEvent.editedBy = reader.GetString(4)
            newEvent.description = reader.GetString(5)
            newEvent.startTime = Date.FromFileTimeUtc(reader.GetInt64(6))
            newEvent.endTime = Date.FromFileTimeUtc(reader.GetInt64(7))
            If reader.GetBoolean(8) Then
                Dim deletedByIP As String = Nothing
                Dim deletedBy As String = Nothing
                If Not reader.IsDBNull(9) Then deletedByIP = reader.GetString(9)
                If Not reader.IsDBNull(10) Then deletedBy = reader.GetString(10)
                newEvent.delete(deletedByIP, deletedBy)
            End If
            newEvent.savedToDB()
            eventsByID.Add(nextID, newEvent)
            If currentEventID < nextID Then currentEventID = nextID
        End While
        reader.Close()
        command.Dispose()
        connection.Close()
    End Sub

End Class
