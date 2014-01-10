Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

Public Class Utils

    Public Shared serverPath As String
    Public Shared lastRecycle As Date
    Public Shared lastError As String = "no error logged"

    Private Const connectionStringVars As String = "server=localhost;Database=candycorn;Uid=ssh_asp;Pwd=candycorn;"
    Public Shared connectionString As String = (New MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionStringVars)).ToString()

    Public Shared Function getAvatarPath(User As UserSystem.OfflineUser) As String
        If User Is Nothing Then
            Return "/images/newstream.png"
        End If
        Dim userID As Integer = User.forumID
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As New MySqlCommand("SELECT user_avatar FROM bb_users WHERE user_id=@ID", connection)
        command.Parameters.Add("@ID", MySqlDbType.Int32).Value = userID
        Dim reader As MySqlDataReader = command.ExecuteReader()
        If reader.Read Then
            Dim avatarID As String = reader.GetString(0)
            If avatarID.Length > 0 Then
                getAvatarPath = "/forum/download/file.php?avatar=" & reader.GetString(0)
            Else
                getAvatarPath = "/images/newstream.png"
            End If
        Else
            getAvatarPath = "/images/newstream.png"
        End If
        reader.Close()
        command.Dispose()
        connection.Close()
    End Function

    Public Shared Function getClientIPAddress() As String
        Dim userIPAddress As String = HttpContext.Current.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(userIPAddress) Then
            userIPAddress = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
        End If
        Return userIPAddress
    End Function

    Public Shared Function GetReadableByteSize(size As Long) As String
        Dim sign As String = (If(size < 0, "-", ""))
        Dim readable As Double = (If(size < 0, -size, size))
        Dim suffix As String
        If size >= &H1000000000000000L Then
            ' Exabyte
            suffix = "EB"
            readable = CDbl(size >> 50)
        ElseIf size >= &H4000000000000L Then
            ' Petabyte
            suffix = "PB"
            readable = CDbl(size >> 40)
        ElseIf size >= &H10000000000L Then
            ' Terabyte
            suffix = "TB"
            readable = CDbl(size >> 30)
        ElseIf size >= &H40000000 Then
            ' Gigabyte
            suffix = "GB"
            readable = CDbl(size >> 20)
        ElseIf size >= &H100000 Then
            ' Megabyte
            suffix = "MB"
            readable = CDbl(size >> 10)
        ElseIf size >= &H400 Then
            ' Kilobyte
            suffix = "KB"
            readable = CDbl(size)
        Else
            ' Byte
            Return size.ToString(sign & "0 B")
        End If
        readable = readable / 1024

        Return sign & readable.ToString("0.### ") & suffix
    End Function

    Public Shared Function getUNIXTimestamp(theDate As Date) As Long
        Return (theDate.ToUniversalTime - New DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds
    End Function

    Public Shared Function getReadableTimeElapsed(timeSince As Date, beExact As Boolean) As String
        Dim ticks = New TimeSpan(DateTime.UtcNow.Ticks - timeSince.ToUniversalTime.Ticks)
        Dim seconds As Double = Math.Abs(ticks.TotalSeconds)
        If Not beExact AndAlso seconds < 43200 Then
            'last 12 hours
            Return "today"
        End If
        If seconds < 60 Then
            Return If(ticks.Seconds = 1, "one second ago", Convert.ToString(ticks.Seconds) & " seconds ago")
        End If
        If seconds < 120 Then
            Return "a minute ago"
        End If
        If seconds < 2700 Then
            ' 45 * 60
            Return Convert.ToString(ticks.Minutes) & " minutes ago"
        End If
        If seconds < 5400 Then
            ' 90 * 60
            Return "an hour ago"
        End If
        If seconds < 86400 Then
            ' 24 * 60 * 60
            Return Convert.ToString(ticks.Hours) & " hours ago"
        End If
        If seconds < 172800 Then
            ' 48 * 60 * 60
            Return "yesterday"
        End If
        If seconds < 2592000 Then
            ' 30 * 24 * 60 * 60
            Return Convert.ToString(ticks.Days) & " days ago"
        End If
        If seconds < 31104000 Then
            ' 12 * 30 * 24 * 60 * 60
            Dim months As Integer = Convert.ToInt32(Math.Floor(CDbl(ticks.Days) / 30))
            Return If(months <= 1, "one month ago", months & " months ago")
        End If
        Dim years As Integer = Convert.ToInt32(Math.Floor(CDbl(ticks.Days) / 365))
        Return If(years <= 1, "one year ago", years & " years ago")
    End Function


    Public Shared Function getReadableTimeRemaining(untilTime As Date) As String
        Dim ticks = New TimeSpan(untilTime.ToUniversalTime.Ticks - DateTime.UtcNow.Ticks)
        Dim seconds As Double = Math.Abs(ticks.TotalSeconds)
        If seconds < 60 Then
            Return If(ticks.Seconds = 1, "one second remaining", Convert.ToString(ticks.Seconds) & " seconds remaining")
        End If
        If seconds < 120 Then
            Return "1 minute remaining"
        End If
        If seconds < 2700 Then
            ' 45 * 60
            Return Convert.ToString(ticks.Minutes) & " minutes remaining"
        End If
        If seconds < 5400 Then
            ' 90 * 60
            Return "1 hour remaining"
        End If
        If seconds < 86400 Then
            ' 24 * 60 * 60
            Return Convert.ToString(ticks.Hours) & " hours remaining"
        End If
        If seconds < 172800 Then
            ' 48 * 60 * 60
            Return "1 day remaining"
        End If
        If seconds < 2592000 Then
            ' 30 * 24 * 60 * 60
            Return Convert.ToString(ticks.Days) & " days remaining"
        End If
        If seconds < 31104000 Then
            ' 12 * 30 * 24 * 60 * 60
            Dim months As Integer = Convert.ToInt32(Math.Floor(CDbl(ticks.Days) / 30))
            Return If(months <= 1, "one month remaining", months & " months remaining")
        End If
        Dim years As Integer = Convert.ToInt32(Math.Floor(CDbl(ticks.Days) / 365))
        Return If(years <= 1, "one year remaining", years & " years remaining")
    End Function

    Public Shared Function lookupPollResults(topicID As Integer) As String
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim query As String = "" + _
            "SELECT username, poll_option_text FROM bb_poll_votes " + _
            "JOIN bb_users ON bb_poll_votes.vote_user_id = bb_users.user_id " + _
            "JOIN bb_poll_options ON bb_poll_votes.poll_option_id = bb_poll_options.poll_option_id " + _
            "WHERE bb_poll_votes.topic_id=@ID AND bb_poll_options.topic_id=@ID " + _
            "ORDER BY username;"
        Dim command As New MySqlCommand(query, connection)
        command.Parameters.Add("@ID", MySqlDbType.Int32).Value = topicID
        Dim reader As MySqlDataReader = command.ExecuteReader()
        If reader.HasRows Then
            Dim builder As New StringBuilder()
            While reader.Read
                builder.Append(reader.GetString(0))
                builder.Append(" --> ")
                builder.Append(reader.GetString(1))
                builder.Append("<br />")
            End While
            reader.Close()
            command.Dispose()
            connection.Close()
            Return builder.ToString
        Else
            Return "no results found for this id"
            reader.Close()
            command.Dispose()
            connection.Close()
        End If
    End Function

    Shared Sub logError(ex As Exception)
        Try
            Dim message As String = ""
            Dim innerEx As Exception = ex
            While innerEx IsNot Nothing
                message &=
                    innerEx.GetType().FullName & " -->  " &
                    innerEx.Message & Environment.NewLine &
                    innerEx.StackTrace & Environment.NewLine & Environment.NewLine
                innerEx = innerEx.InnerException
            End While
            IO.File.AppendAllText(Utils.serverPath & "errorlog.txt", message)
            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Mod, "error detected and written to log")
        Finally
        End Try
    End Sub

    'http://en.wikipedia.org/wiki/HSL_and_HSV#Converting_to_RGB
    Shared Function hslToRGBString(hue As Single, saturation As Single, lightness As Single) As String
        'hue ∈ [0°, 360°),  saturation ∈ [0, 1],  lightness ∈ [0, 1]
        Dim hueSection As Single = hue / 60
        Dim chroma As Single = (1 - Math.Abs(2 * lightness - 1)) * saturation
        Dim point As Single = chroma * (1 - Math.Abs(hueSection Mod 2 - 1))
        Dim percentRed, percentGreen, percentBlue As Single
        If 0 <= hueSection And hueSection < 1 Then
            percentRed = chroma
            percentGreen = point
            percentBlue = 0
        ElseIf 1 <= hueSection And hueSection < 2 Then
            percentRed = point
            percentGreen = chroma
            percentBlue = 0
        ElseIf 2 <= hueSection And hueSection < 3 Then
            percentRed = 0
            percentGreen = chroma
            percentBlue = point
        ElseIf 3 <= hueSection And hueSection < 4 Then
            percentRed = 0
            percentGreen = point
            percentBlue = chroma
        ElseIf 4 <= hueSection And hueSection < 5 Then
            percentRed = point
            percentGreen = 0
            percentBlue = chroma
        ElseIf 5 <= hueSection And hueSection < 6 Then
            percentRed = chroma
            percentGreen = 0
            percentBlue = point
        End If
        Dim match As Single = lightness - chroma / 2
        percentRed = percentRed + match
        percentBlue = percentBlue + match
        percentGreen = percentGreen + match
        Dim red As Byte = percentRed * 255
        Dim green As Byte = percentGreen * 255
        Dim blue As Byte = percentBlue * 255
        Return "#" & red.ToString("X2") & green.ToString("X2") & blue.ToString("X2")
    End Function
    '[rainbow]0123456789[/rainbow]
End Class