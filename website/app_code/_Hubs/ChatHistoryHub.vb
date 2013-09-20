Imports Microsoft.VisualBasic
Imports Microsoft.AspNet.SignalR.Hubs
Imports MySql.Data.MySqlClient
Imports UserSystem
Imports Microsoft.AspNet.SignalR

Public Class ChatHistoryHub
    Inherits Hub

    Public Function fetchMessages(ByVal rangeStart As Date, ByVal rangeEnd As Date) As String
        Dim user As FrontPageUser = Connections.getSessionUser
        If user Is Nothing Then Return "<div>This page can only be accessed by logged in officers.</div>"
        If Not user.privileges.isOfficer Then Return "<div>This page can only be accessed by officers.</div>"

        Dim builder As New StringBuilder
        Dim minFileTime = rangeStart.ToFileTimeUtc
        Dim maxFileTime = rangeEnd.ToFileTimeUtc

        Dim query As String = "SELECT isDeleted, html FROM custom_chat_history WHERE " + _
        "`time` BETWEEN " & minFileTime & " AND " & maxFileTime & ";"
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()
        While reader.Read
            If reader.GetBoolean(0) Then

            End If
            builder.Append(reader.GetString(1))
        End While
        reader.Close()
        command.Dispose()
        connection.Close()

        Return builder.ToString

    End Function

End Class
