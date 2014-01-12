Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

'This class represents a user connected to the website's front page.
Namespace UserSystem

    Public Class FrontPageUser
        Inherits OnlineUser

        Sub New(newIP As String, hubConnectionID As String, hubClient As Object)
            MyBase.New(newIP, hubConnectionID, hubClient)
        End Sub

        Private guestShortHand As String
        Overrides Function isMatch(matchText As String) As Boolean
            Return MyBase.isMatch(matchText) OrElse (Not isBoundToAccount AndAlso String.Equals(matchText, guestShortHand, StringComparison.OrdinalIgnoreCase))
        End Function

        Private Sub bound() Handles Me.boundToAccount
            guestShortHand = Nothing
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As New MySqlCommand("UPDATE bb_users SET user_ip=@ip WHERE username=@name;", connection)
            command.Parameters.Add("@ip", MySqlDbType.VarChar).Value = ipAddress
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = name
            command.ExecuteNonQuery()
            command.Dispose()
            connection.Close()
        End Sub

        Sub setGuestName(fullName As String)
            renameObject(fullName)
            Dim prefixLength As Integer = "(Guest ".Length
            guestShortHand = name.Substring(prefixLength, name.IndexOf(")") - prefixLength)
        End Sub

       ' Public recentChatHistory As New List(Of ChatMessage)
       ' Overridable ReadOnly Property isFloodingChat() As Boolean
         '   Get
          '      If recentChatHistory.Count > 0 Then
              '      Dim totalWeight As Integer
                '    For Each message As ChatMessage In recentChatHistory.ToList
                 '       If message.weight > 0 Then
                    '        Dim thisWeight As Integer = message.weight - (Now - message.time).TotalSeconds
                   '         If thisWeight > 0 Then totalWeight += thisWeight
                     '   End If
                   ' Next
                 '   Return totalWeight > 18
              '  Else
              '      Return False
              '  End If
          '  End Get
       ' End Property

        Sub refreshBrowser()
            client.initiateRefresh(10)
        End Sub

        Private isInNoVideoMode As Boolean
        Sub enableNoVideoMode()
            If Not isInNoVideoMode Then
                isInNoVideoMode = True
                client.noVideoMode()
            End If
        End Sub

        Overrides Sub postMessage(message As String)
            client.postMessage(message)
        End Sub

        Overrides Sub postSystemMessage(message As String)
            client.postSystemMessage(message)
        End Sub

        Overrides Sub postErrorMessage(message As String)
            client.postSystemMessage("<span style=""color:#C00"">" + message + "</span>")
        End Sub

        Sub abandonConnection()
            client.abandonConnection("disconnected")
        End Sub

        Private _isIdle As Boolean = False
        Property isIdle() As Boolean
            Get
                Return _isIdle
            End Get
            Set(value As Boolean)
                _isIdle = value
            End Set
        End Property

        Private lastResponse As Date = Now
        Sub updateLastConnectionResponse()
            lastResponse = Now
        End Sub

        Function connectionCheckFailed() As Boolean
            Return (Date.Compare(lastResponse.AddMinutes(7), Now) < 0)
        End Function

    End Class

End Namespace

