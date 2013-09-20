Imports Microsoft.VisualBasic
Imports System.Diagnostics
Imports MySql.Data.MySqlClient

Namespace UserSystem

    Public Class Infractions

        Private Shared cachedBans As New Dictionary(Of String, Date)(System.StringComparer.OrdinalIgnoreCase)
        Private Shared cachedSilentBans As New Dictionary(Of String, Date)(System.StringComparer.OrdinalIgnoreCase)
        Private Shared cachedMutes As New Dictionary(Of String, Date)(System.StringComparer.OrdinalIgnoreCase)

        Shared Sub init()
            cachedBans.Clear()
            cachedSilentBans.Clear()
            cachedMutes.Clear()
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim command As New MySqlCommand("SELECT target, endsAt, type FROM custom_infractions " &
                                            "WHERE isDeleted=false AND (type='ban' OR type='mute' OR type='silentban') AND endsAt>@endsAt;", Connection)
            command.Parameters.Add("@endsAt", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            Dim reader As MySqlDataReader = command.ExecuteReader()
            While reader.Read()
                Dim target As String = reader.GetString(0)
                Dim endsAt As Date = Date.FromFileTimeUtc(reader.GetInt64(1)).ToLocalTime
                Dim type As String = reader.GetString(2)
                If type = "ban" Then
                    SyncLock cachedBans
                        If Not cachedBans.ContainsKey(target) Then
                            cachedBans(target) = endsAt
                        ElseIf Date.Compare(endsAt, cachedBans(target)) > 0 Then
                            cachedBans(target) = endsAt
                        End If
                    End SyncLock
                End If
                If type = "silentban" Then
                    SyncLock cachedSilentBans
                        If Not cachedSilentBans.ContainsKey(target) Then
                            cachedSilentBans(target) = endsAt
                        ElseIf Date.Compare(endsAt, cachedSilentBans(target)) > 0 Then
                            cachedSilentBans(target) = endsAt
                        End If
                    End SyncLock
                End If
                If type = "mute" Then
                    SyncLock cachedMutes
                        If Not cachedMutes.ContainsKey(target) Then
                            cachedMutes(target) = endsAt
                        ElseIf Date.Compare(endsAt, cachedMutes(target)) > 0 Then
                            cachedMutes(target) = endsAt
                        End If
                    End SyncLock
                End If
            End While
            reader.Close()
            command.Dispose()
            Connection.Close()
        End Sub

        Shared Function isBanned(user As OfflineUser) As Boolean
            Return isBanned(user.name) OrElse isBanned(user.ipAddress)
        End Function

        Shared Function isSilentBanned(user As OfflineUser) As Boolean
            Return isSilentBanned(user.name) OrElse isSilentBanned(user.ipAddress)
        End Function

        Shared Function isMuted(user As OfflineUser) As Boolean
            Return isMuted(user.name) OrElse isMuted(user.ipAddress)
        End Function

        Shared Function isBanned(match As String) As Boolean
            If cachedBans.ContainsKey(match) Then
                If Date.Compare(cachedBans(match), Now) > 0 Then
                    Return True
                Else
                    clear("ban", match, New SystemUser, True)
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Shared Function isSilentBanned(match As String) As Boolean
            If cachedSilentBans.ContainsKey(match) Then
                If Date.Compare(cachedSilentBans(match), Now) > 0 Then
                    Return True
                Else
                    clear("silentban", match, New SystemUser, True)
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Shared Function isMuted(match As String) As Boolean
            If cachedMutes.ContainsKey(match) Then
                If Date.Compare(cachedMutes(match), Now) > 0 Then
                    Return True
                Else
                    clear("mute", match, New SystemUser, True)
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Shared Function banEndsAt(user As OfflineUser) As Date
            If Not isBanned(user) Then Return Now.AddMonths(-1)
            If cachedBans.ContainsKey(user.ipAddress) Then
                If cachedBans.ContainsKey(user.name) Then
                    Dim date1 As Date = cachedBans(user.ipAddress)
                    Dim date2 As Date = cachedBans(user.name)
                    If Date.Compare(date1, date2) = 0 Then
                        Return date1
                    ElseIf Date.Compare(date1, date2) > 0 Then
                        Return date1
                    Else
                        Return date2
                    End If
                Else
                    Return cachedBans(user.ipAddress)
                End If
            ElseIf cachedBans.ContainsKey(user.name) Then
                Return cachedBans(user.name)
            Else
                Return Now.AddMonths(-1)
            End If
        End Function

        Shared Function muteEndsAt(user As OfflineUser) As Date
            If Not isMuted(user) Then Return Now.AddMonths(-1)
            If cachedMutes.ContainsKey(user.ipAddress) Then
                If cachedMutes.ContainsKey(user.name) Then
                    Dim date1 As Date = cachedMutes(user.ipAddress)
                    Dim date2 As Date = cachedMutes(user.name)
                    If Date.Compare(date1, date2) = 0 Then
                        Return date1
                    ElseIf Date.Compare(date1, date2) > 0 Then
                        Return date1
                    Else
                        Return date2
                    End If
                Else
                    Return cachedMutes(user.ipAddress)
                End If
            ElseIf cachedMutes.ContainsKey(user.name) Then
                Return cachedMutes(user.name)
            Else
                Return Now.AddMonths(-1)
            End If
        End Function

        Shared Function banEndsAt(match As String) As Date
            If cachedBans.ContainsKey(match) Then
                Return cachedBans(match)
            Else
                Return Now.AddMonths(-1)
            End If
        End Function

        Shared Function muteEndsAt(match As String) As Date
            If cachedMutes.ContainsKey(match) Then
                Return cachedMutes(match)
            Else
                Return Now.AddMonths(-1)
            End If
        End Function

        Shared Function hasOutstanding(match As String, infractionType As String) As Boolean
            hasOutstanding = False
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim command As New MySqlCommand("SELECT COUNT(*) FROM custom_infractions " &
                                            "WHERE isDeleted=false AND type=@type AND endsAt>@endsAt AND target=@match", Connection)
            command.Parameters.Add("@type", MySqlDbType.VarChar).Value = infractionType
            command.Parameters.Add("@endsAt", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            command.Parameters.Add("@match", MySqlDbType.VarChar).Value = match
            Dim reader As MySqlDataReader = command.ExecuteReader()
            If reader.Read() Then
                hasOutstanding = reader.GetInt32(0)
            Else
                hasOutstanding = 0
            End If
            reader.Close()
            command.Dispose()
            Connection.Close()
        End Function

        Shared Function queryDetails(match As String) As List(Of InfractionDetails)
            Dim infractionList As New List(Of InfractionDetails)
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim command As New MySqlCommand("SELECT * FROM custom_infractions WHERE " &
                                            "isDeleted=false AND endsAt>@endsAt AND target=@match", Connection)
            command.Parameters.Add("@endsAt", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            command.Parameters.Add("@match", MySqlDbType.VarChar).Value = match
            Dim reader As MySqlDataReader = command.ExecuteReader()
            While reader.Read()
                Dim details As New InfractionDetails
                details.type = reader.GetString(1)
                details.createdAt = Date.FromFileTimeUtc(reader.GetInt64(2))
                details.createdByIP = reader.GetString(3)
                details.createdByName = reader.GetString(4)
                details.target = reader.GetString(5)
                details.endsAt = Date.FromFileTimeUtc(reader.GetInt64(6))
                details.duration = details.endsAt - details.createdAt
                details.notes = reader.GetString(7)
                details.isDeleted = reader.GetBoolean(8)
                If Not reader.IsDBNull(9) Then details.deletedByIP = reader.GetString(9)
                If Not reader.IsDBNull(10) Then details.deletedByName = reader.GetString(10)
                infractionList.Add(details)
            End While
            reader.Close()
            command.Dispose()
            Connection.Close()
            Return infractionList
        End Function

        Shared Sub add(type As String, match As String, duration As Long, notes As String, caller As UserSystem.OnlineUser)
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As MySqlCommand
            Dim timeNow As Date = Now
            Dim endsAt As Date = timeNow.AddSeconds(duration)
            Command = New MySqlCommand("INSERT INTO custom_infractions " &
                                       "(type, createdAt, createdByIP, createdByName, target, endsAt, notes, isDeleted) " &
                                       "VALUES(@A, @B, @C, @D, @E, @F, @G, @H);", Connection)
            Command.Parameters.Add("@A", MySqlDbType.VarChar).Value = type
            Command.Parameters.Add("@B", MySqlDbType.Int64).Value = timeNow.ToFileTimeUtc
            Command.Parameters.Add("@C", MySqlDbType.VarChar).Value = caller.ipAddress
            Command.Parameters.Add("@D", MySqlDbType.VarChar).Value = caller.name
            Command.Parameters.Add("@E", MySqlDbType.VarChar).Value = match
            Command.Parameters.Add("@F", MySqlDbType.Int64).Value = endsAt.ToFileTimeUtc
            Command.Parameters.Add("@G", MySqlDbType.Text).Value = notes
            Command.Parameters.Add("@H", MySqlDbType.Bit).Value = False
            Command.ExecuteNonQuery()
            Command.Dispose()
            Connection.Dispose()
            If type = "ban" Then
                SyncLock cachedBans
                    If Not cachedBans.ContainsKey(match) Then
                        cachedBans(match) = endsAt
                    ElseIf Date.Compare(endsAt, cachedBans(match)) > 0 Then
                        cachedBans(match) = endsAt
                    End If
                End SyncLock
            End If
            If type = "silentban" Then
                SyncLock cachedSilentBans
                    If Not cachedSilentBans.ContainsKey(match) Then
                        cachedSilentBans(match) = endsAt
                    ElseIf Date.Compare(endsAt, cachedSilentBans(match)) > 0 Then
                        cachedSilentBans(match) = endsAt
                    End If
                End SyncLock
            End If
            If type = "mute" Then
                SyncLock cachedMutes
                    If Not cachedMutes.ContainsKey(match) Then
                        cachedMutes(match) = endsAt
                    ElseIf Date.Compare(endsAt, cachedMutes(match)) > 0 Then
                        cachedMutes(match) = endsAt
                    End If
                End SyncLock
            End If
        End Sub

        Shared Sub clear(type As String, match As String, caller As OnlineUser, Optional clearOld As Boolean = False)
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As MySqlCommand
            If clearOld Then
                Command = New MySqlCommand("UPDATE custom_infractions " &
                           "SET isDeleted=true, deletedByIP=@callerIP, deletedByName=@callerName " &
                           "WHERE type=@type AND isDeleted=false AND target=@match AND endsAt>@endsAt;", Connection)
            Else
                Command = New MySqlCommand("UPDATE custom_infractions " &
                               "SET isDeleted=true, deletedByIP=@callerIP, deletedByName=@callerName " &
                               "WHERE type=@type AND isDeleted=false AND target=@match;", Connection)
            End If
            Command.Parameters.Add("@callerIP", MySqlDbType.VarChar).Value = caller.ipAddress
            Command.Parameters.Add("@callerName", MySqlDbType.VarChar).Value = caller.name
            Command.Parameters.Add("@type", MySqlDbType.VarChar).Value = type
            Command.Parameters.Add("@match", MySqlDbType.VarChar).Value = match
            Command.Parameters.Add("@endsAt", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            Command.ExecuteNonQuery()
            Command.Dispose()
            Connection.Dispose()
            If type = "ban" AndAlso cachedBans.ContainsKey(match) Then
                SyncLock cachedBans
                    cachedBans.Remove(match)
                End SyncLock
            End If
            If type = "silentban" AndAlso cachedSilentBans.ContainsKey(match) Then
                SyncLock cachedSilentBans
                    cachedSilentBans.Remove(match)
                End SyncLock
            End If
            If type = "mute" AndAlso cachedMutes.ContainsKey(match) Then
                SyncLock cachedMutes
                    cachedMutes.Remove(match)
                End SyncLock
            End If
        End Sub

        Shared Sub clearAll(type As String, caller As UserSystem.OnlineUser)
            Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As MySqlCommand
            Command = New MySqlCommand("UPDATE custom_infractions " &
                                       "SET isDeleted=true, deletedByIP=@callerIP, deletedByName=@callerName " &
                                       "WHERE type=@type AND isDeleted=false AND endsAt>@endsAt;", Connection)
            Command.Parameters.Add("@type", MySqlDbType.VarChar).Value = type
            Command.Parameters.Add("@callerIP", MySqlDbType.VarChar).Value = caller.ipAddress
            Command.Parameters.Add("@callerName", MySqlDbType.VarChar).Value = caller.name
            Command.Parameters.Add("@endsAt", MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            Command.ExecuteNonQuery()
            Command.Dispose()
            Connection.Dispose()
            SyncLock cachedMutes
                cachedMutes.Clear()
            End SyncLock
        End Sub

    End Class

End Namespace
