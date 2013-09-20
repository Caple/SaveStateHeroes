Imports MySql.Data.MySqlClient

Namespace UserSystem

    Public Module Connections

        Public frontPageUsers As New List(Of FrontPageUser)

        Function matchFirst(Of T As OfflineUser)(collection As List(Of T), matchText As String) As T
            For Each nextUser As T In collection.ToList 'tolist copies refrences to new collection to avoid concurrent modification
                If nextUser.isMatch(matchText) Then
                    Return nextUser
                End If
            Next
            Return Nothing
        End Function

        Function matchUsers(Of T As OfflineUser)(collection As List(Of T), matchText As String) As List(Of T)
            Dim matches As New List(Of T)
            For Each nextUser As T In collection.ToList
                If nextUser.isMatch(matchText) Then
                    matches.Add(nextUser)
                End If
            Next
            Return matches
        End Function

        Function getSessionUser() As FrontPageUser
            Dim account As AccountDetails = Accounts.getAccountBySession(HttpContext.Current)
            If account.isValidRecord Then
                Return matchFirst(frontPageUsers, account.username)
            Else
                Return Nothing
            End If
        End Function

        Public Sub init()
            ServerPersistance.setDefault("guestNames", New Dictionary(Of String, String))
            guestNamesByIP = ServerPersistance.getField("guestNames")
        End Sub

        Public Sub persist()
            ServerPersistance.setField("guestNames", guestNamesByIP)
        End Sub

        Private randoms As New Random
        Private guestNamesByIP As New Dictionary(Of String, String)
        Function getGuestName(userIPAddress As String) As String
            If guestNamesByIP.ContainsKey(userIPAddress) Then
                Return guestNamesByIP(userIPAddress)
            End If
            Dim isUnique As Boolean
            Dim guestName As String = Nothing
            While Not isUnique
                isUnique = True
                Dim letters As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                guestName = "(Guest " &
                    letters.Substring(randoms.Next(0, 25), 1) &
                    letters.Substring(randoms.Next(0, 25), 1) &
                    letters.Substring(randoms.Next(0, 25), 1) & "-"c &
                    randoms.Next(0, 10) & randoms.Next(0, 10) & ")"
                isUnique = Not guestNamesByIP.ContainsValue(guestName)
            End While
            guestNamesByIP.Add(userIPAddress, guestName)
            Return guestName
        End Function

        Function matchGuest(match As String) As OfflineUser
            Dim actualObjectName As String = getObjectName(Match)
            If actualObjectName IsNot Nothing Then Match = actualObjectName
            Dim onUser As OnlineUser = matchFirst(frontPageUsers, match)
            If onUser IsNot Nothing AndAlso Not onUser.isBoundToAccount Then
                Return onUser
            Else
                For Each pair As KeyValuePair(Of String, String) In guestNamesByIP.ToList
                    If String.Equals(pair.Value, match, StringComparison.OrdinalIgnoreCase) OrElse
                       String.Equals(pair.Key, match, StringComparison.OrdinalIgnoreCase) Then
                        Dim user As New OfflineUser(pair.Key)
                        user.renameObject(pair.Value)
                        Return user
                    End If
                Next
            End If
            Return Nothing
        End Function

        Function matchAllOfflineUsers(match As String) As List(Of OfflineUser)
            Dim actualObjectName As String = getObjectName(match)
            If actualObjectName IsNot Nothing Then match = actualObjectName
            matchAllOfflineUsers = New List(Of OfflineUser)
            Dim guestUser As OfflineUser = Connections.matchGuest(match)
            If guestUser IsNot Nothing Then matchAllOfflineUsers.Add(guestUser)
            For Each account As AccountDetails In Accounts.matchSSHAccounts(match)
                Dim user As New OfflineUser(account.ipAddress)
                user.bindAccount(account)
                matchAllOfflineUsers.Add(user)
            Next
        End Function

        Function getObjectName(displayName As String) As String
            getObjectName = Nothing
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As New MySqlCommand("SELECT objectName FROM custom_nicknames WHERE displayName=@name;", connection)
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = displayName
            Dim reader As MySqlDataReader = command.ExecuteReader()
            If reader.Read() Then
                getObjectName = reader.GetString(0)
                If String.Equals(getObjectName, displayName, StringComparison.OrdinalIgnoreCase) Then getObjectName = Nothing
            End If
            command.Dispose()
            connection.Close()
        End Function

        Function getDisplayName(objectName As String) As String
            getDisplayName = Nothing
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As New MySqlCommand("SELECT displayName FROM custom_nicknames WHERE objectName=@name;", connection)
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = objectName
            Dim reader As MySqlDataReader = command.ExecuteReader()
            If reader.Read() Then
                getDisplayName = reader.GetString(0)
                If String.Equals(getDisplayName, objectName, StringComparison.OrdinalIgnoreCase) Then getDisplayName = Nothing
            End If
            command.Dispose()
            connection.Close()
        End Function

    End Module

End Namespace