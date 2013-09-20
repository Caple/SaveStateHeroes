Imports Microsoft.VisualBasic

Public Class StreamerApp

    Public Property appID As Integer
    Public Property userID As Integer
    Public Property username As String

    Public Property status As String
    Public Property lastUpdatedBy As String

    Public Property submitDate As Date
    Public Property submitDateString As String
    Public Property approvalDate As Date
    Public Property approvalDateString As String
    Public Property trialEndedDate As Date
    Public Property trialEndedDateString As String

    Public Property timezone As String
    Public Property age As String
    Public Property program As String
    Public Property lsUsername As String
    Public Property essay1 As String
    Public Property essay2 As String
    Public Property dxDiag As String
    Public Property connectionRating As String
    Public Property skypeName As String

    Public Function cloneWithoutSensitiveInformation() As StreamerApp
        Dim clone As StreamerApp = MemberwiseClone()
        clone.age = "<i>Only an admin can view this field.</i>"
        clone.skypeName = "<i>Only an admin can view this field.</i>"
        clone.dxDiag = "Only an admin can view this field."
        Return clone
    End Function


End Class
