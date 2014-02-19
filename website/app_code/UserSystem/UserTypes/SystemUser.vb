Imports Microsoft.VisualBasic
Imports UserSystem

Public Class SystemUser
    Inherits FrontPageUser

    Sub New()
        MyBase.New("0.0.0.0", Nothing, Nothing)
        Dim account As New AccountDetails
        account.ipAddress = "0.0.0.0"
        account.username = "Gary Oak"
        account.userID = 0
        bindAccount(account)
    End Sub

    Public Overrides Sub postMessage(message As String)

    End Sub

    Public Overrides Sub postSystemMessage(message As String)

    End Sub

    Public Overrides Sub postErrorMessage(message As String)

    End Sub

    Overrides ReadOnly Property isFloodingChat() As Boolean
        Get
            Return False
        End Get
    End Property

End Class
