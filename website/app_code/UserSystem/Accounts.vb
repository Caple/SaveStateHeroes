Namespace UserSystem

    Public Class Accounts

        Public Shared Function getAllAccounts() As AccountDetails()
            Dim accounts As New List(Of AccountDetails)
            Dim cPhpBB As New PhPBBCode.phpBBCryptoServiceProvider()
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("SELECT user_id, bb_users.username, user_ip FROM bb_users", Connection)
            Dim Reader As MySql.Data.MySqlClient.MySqlDataReader = Command.ExecuteReader()
            While Reader.Read()
                Dim details As New AccountDetails
                details.isValidRecord = True
                details.userID = Reader.GetInt32(0)
                If Not Reader.IsDBNull(1) Then details.username = Reader.GetString(1)
                If Not Reader.IsDBNull(2) Then details.ipAddress = Reader.GetString(2)
                accounts.Add(details)
            End While
            Reader.Close()
            Command.Dispose()
            Connection.Close()
            Return accounts.ToArray
        End Function

        Public Shared Function matchSSHAccounts(match As String) As List(Of AccountDetails)
            Dim detailsList As New List(Of AccountDetails)
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("" &
                "SELECT user_id, username, user_ip FROM bb_users " &
                "WHERE username=@MATCH OR user_ip=@MATCH;", Connection)
            Command.Parameters.Add("@MATCH", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = match
            Dim Reader As MySql.Data.MySqlClient.MySqlDataReader = Command.ExecuteReader()
            While Reader.Read()
                Dim details As New AccountDetails
                details.isValidRecord = True
                details.userID = Reader.GetInt32(0)
                details.username = Reader.GetString(1)
                details.ipAddress = Reader.GetString(2)
                detailsList.Add(details)
            End While
            Reader.Close()
            Command.Dispose()
            Connection.Close()
            Return detailsList
        End Function

        Public Shared Function matchIoGAccounts(match As String) As List(Of AccountDetails)
            Dim detailsList As New List(Of AccountDetails)
            Dim cPhpBB As New PhPBBCode.phpBBCryptoServiceProvider()
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("" &
                "SELECT userid, username, ipaddress FROM iog_website.bb_user " &
                "WHERE username=@MATCH OR ipaddress=@MATCH;", Connection)
            Command.Parameters.Add("@MATCH", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = match
            Dim Reader As MySql.Data.MySqlClient.MySqlDataReader = Command.ExecuteReader()
            While Reader.Read()
                Dim details As New AccountDetails
                details.isValidRecord = False
                details.userID = Reader.GetInt32(0)
                If Not Reader.IsDBNull(1) Then details.username = Reader.GetString(1)
                If Not Reader.IsDBNull(2) Then details.ipAddress = Reader.GetString(2)
                detailsList.Add(details)
            End While
            Reader.Close()
            Command.Dispose()
            Connection.Close()
            Return detailsList
        End Function

        Public Shared Function getAccountByCredentials(userName As String, password As String) As AccountDetails
            Dim details As New AccountDetails
            Dim cPhpBB As New PhPBBCode.phpBBCryptoServiceProvider()
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("" &
                "SELECT user_password, user_id, username, user_ip FROM bb_users " &
                "WHERE username=@NAME;", Connection)
            Command.Parameters.Add("@NAME", MySql.Data.MySqlClient.MySqlDbType.String).Value = userName
            Dim Reader As MySql.Data.MySqlClient.MySqlDataReader = Command.ExecuteReader()
            If Reader.Read() Then
                If cPhpBB.phpbbCheckHash(password, Reader.GetString(0)) Then
                    details.isValidRecord = True
                    details.userID = Reader.GetInt32(1)
                    If Not Reader.IsDBNull(2) Then details.username = Reader.GetString(2)
                    If Not Reader.IsDBNull(3) Then details.ipAddress = Reader.GetString(3)
                End If
            End If
            Reader.Close()
            Command.Dispose()
            Connection.Close()
            Return details
        End Function

        Public Shared Function getAccountBySession(context As HttpContext) As AccountDetails
            Dim details As New AccountDetails
            If context.Request.Cookies("ssHSession") Is Nothing Then Return details
            Dim sessionID As String = context.Request.Cookies("ssHSession").Value()
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("" &
                "SELECT forumID, username FROM custom_sessions " &
                "JOIN bb_users ON user_id=forumID " &
                "WHERE sessionID=@sessionID;", Connection)
            Command.Parameters.Add("@sessionID", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sessionID
            Dim Reader As MySql.Data.MySqlClient.MySqlDataReader = Command.ExecuteReader()
            If Reader.Read() Then
                details.isValidRecord = True
                details.userID = Reader.GetInt32(0)
                details.username = Reader.GetString(1)
                details.ipAddress = Utils.getClientIPAddress
            End If
            Reader.Close()
            Command.Dispose()
            Connection.Close()
            Return details
        End Function

        Public Shared Sub deleteSession(forumID As Integer, context As HttpContext)
            If context.Request.Cookies("ssHSession") Is Nothing Then Return
            Dim sessionID As String = context.Request.Cookies("ssHSession").Value()
            context.Response.Cookies("ssHSession").Expires = Now.AddDays(-1)
            If sessionID IsNot Nothing Then
                Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
                Dim Command As New MySql.Data.MySqlClient.MySqlCommand("" &
                    "DELETE FROM custom_sessions " &
                    "WHERE sessionID=@sessionID OR forumID=@forumID;", Connection)
                Command.Parameters.Add("@sessionID", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sessionID
                Command.Parameters.Add("@forumID", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = forumID
                Command.ExecuteNonQuery()
                Command.Dispose()
                Connection.Close()
            End If
        End Sub

        Public Shared Sub createSession(forumID As Integer, userIP As String, context As HttpContext)
            'Prepare necessary values
            Dim newSessionID As String = Guid.NewGuid.ToString
            context.Response.Cookies("ssHSession").Value = newSessionID
            context.Response.Cookies("ssHSession").Expires = Now.AddDays(7)
            context.Response.Cookies("ssHSession").HttpOnly = True

            'Write session information to DB
            Dim Connection As New MySql.Data.MySqlClient.MySqlConnection(Utils.connectionString) : Connection.Open()
            Dim Command As New MySql.Data.MySqlClient.MySqlCommand("INSERT INTO custom_sessions " &
                "(sessionID, createdAt, forumID, IP) VALUES (@sessionID, @createdAt, @forumID, @IP);", Connection)
            Command.Parameters.Add("@sessionID", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = newSessionID
            Command.Parameters.Add("@createdAt", MySql.Data.MySqlClient.MySqlDbType.Int64).Value = Now.ToFileTimeUtc
            Command.Parameters.Add("@forumID", MySql.Data.MySqlClient.MySqlDbType.Int32).Value = forumID
            Command.Parameters.Add("@IP", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = userIP
            Command.ExecuteNonQuery()
            Command.Dispose()
            Connection.Close()
        End Sub

    End Class

End Namespace

