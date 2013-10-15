Imports Microsoft.VisualBasic

'This class represents a connected in user.
Namespace UserSystem

    Public MustInherit Class OnlineUser
        Inherits OfflineUser

        Sub New(newIP As String, hubConnectionID As String, hubClient As Object)
            MyBase.New(newIP)
            _connectionID = hubConnectionID
            _client = hubClient
        End Sub

        Sub rebindClient(hubConnectionID As String, hubClient As Object)
            _connectionID = hubConnectionID
            _client = hubClient
        End Sub

        Overloads Shared Operator =(a As OnlineUser, b As OnlineUser) As Boolean
            If String.Compare(a.connectionID, b.connectionID) = 0 Then
                Return True
            Else
                Return (String.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase) = 0)
            End If
        End Operator

        Overloads Shared Operator <>(a As OnlineUser, b As OnlineUser) As Boolean
            If String.Compare(a.connectionID, b.connectionID) = 0 Then
                Return False
            Else
                Return (String.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase) <> 0)
            End If
        End Operator

        Overrides Function isMatch(matchText As String) As Boolean
            Return String.Equals(matchText, _connectionID) OrElse MyBase.isMatch(matchText)
        End Function

        Private _connectionID As String
        ReadOnly Property connectionID() As String
            Get
                Return _connectionID
            End Get
        End Property

        Private _client As Object
        ReadOnly Property client() As Object
            Get
                Return _client
            End Get
        End Property

        Property lastOutgoingPage() As Date

        Private lastPage As Date = Now.AddHours(-1)
        Function TryPage(caller As OnlineUser) As Boolean
            If Infractions.isBanned(caller) Then
                caller.postSystemMessage("You can not send pages while banned.")
            ElseIf Infractions.isMuted(caller) Then
                caller.postSystemMessage("You can not send pages while muted.")
            ElseIf StreamProcessor.streamer = Me Then
                caller.postSystemMessage("You can not page a user who is streaming.")
           ' ElseIf Date.Compare(lastPage.AddMinutes(15), Now) > 0 Then
           '     caller.postSystemMessage("This user has already been paged recently. Try again later.")
           ' ElseIf Not privileges.isOfficer AndAlso Date.Compare(caller.lastOutgoingPage.AddMinutes(10), Now) > 0 Then
            '    caller.postSystemMessage("You are excessively paging users. Page suppressed.")
            Else
                lastPage = Now
                caller.lastOutgoingPage = Now
                caller.postSystemMessage(displayName & " has been paged.")
                postSystemMessage("You have been paged by " & caller.displayName & ".")
                client.audioPage(caller.displayName)
                Return True
            End If
            Return False
        End Function
		Function TryShake(caller as OnlineUser) As Boolean
			If Infractions.isBanned(caller) Then
				caller.postSystemMessage("you can not shake while banned.")
			ElseIf Infractions.isMuted(caller) Then
				caller.postSystemMessage("You can not shake while muted.")
			Else
				client.DoShake(caller.displayName)
				postSystemMessage("DO THE HARLEM SHAKE!")
				Return True
			End If
			Return False
		End Function

		        Function TrySXN(caller As OnlineUser) As Boolean
            If Infractions.isBanned(caller) Then
                caller.postSystemMessage("You can not do this while banned.")
            ElseIf Infractions.isMuted(caller) Then
                caller.postSystemMessage("You can not do this while muted.")
            ElseIf StreamProcessor.streamer = Me Then
                caller.postSystemMessage("You can not do this to a user who is streaming.")
            Else
                caller.postSystemMessage(displayName & " has been smitten.")
                postSystemMessage("You have been smitten by " & caller.displayName & ".")
                client.audioSXN(caller.displayName)
                Return True
            End If
            Return False
        End Function
		
		Function TryJason(caller as OnlineUser) As Boolean
			If Infractions.isBanned(caller) Then
				caller.postSystemMessage("you can not jason while banned.")
			ElseIf Infractions.isMuted(caller) Then
				caller.postSystemMessage("You can not jason while muted.")
			Else
				client.audioJason(caller.displayName)
				Return True
			End If
			Return False
		End Function
		
		Function TrySlam(caller as OnlineUser) As Boolean
			If Infractions.isBanned(caller) Then
				caller.postSystemMessage("you can not slam while banned.")
			ElseIf Infractions.isMuted(caller) Then
				caller.postSystemMessage("You can not slam while muted.")
			Else
				client.audioSlam(caller.displayName)
				Return True
			End If
			Return False
		End Function
		
				Function TryDick(caller as OnlineUser) As Boolean
			If Infractions.isBanned(caller) Then
				caller.postSystemMessage("you can not dickbutt while banned.")
			ElseIf Infractions.isMuted(caller) Then
				caller.postSystemMessage("You can not dickbutt while muted.")
			Else
				client.dickFlag(caller.displayName)
				Return True
			End If
			Return False
		End Function
		
				Function TryAustin(caller as OnlineUser) As Boolean
			If Infractions.isBanned(caller) Then
				caller.postSystemMessage("you can not austin while banned.")
			ElseIf Infractions.isMuted(caller) Then
				caller.postSystemMessage("You can not austin while muted.")
			Else
				client.audioAustin(caller.displayName)
				Return True
			End If
			Return False
		End Function
		
		
        Private _lastWhsiperer As FrontPageUser = Nothing
        Property lastWhisperer() As FrontPageUser
            Get
                Return _lastWhsiperer
            End Get
            Set(value As FrontPageUser)
                _lastWhsiperer = value
            End Set
        End Property

        MustOverride Sub postMessage(message As String)
        MustOverride Sub postSystemMessage(message As String)
        MustOverride Sub postErrorMessage(message As String)

    End Class

End Namespace