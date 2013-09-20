Imports Microsoft.VisualBasic

Public Class InfractionDetails
    Public type As String
    Public createdAt As Date
    Public endsAt As Date
    Public duration As TimeSpan
    Public createdByName As String
    Public createdByIP As String
    Public target As String
    Public notes As String
    Public isDeleted As Boolean
    Public deletedByIP As String
    Public deletedByName As String
End Class
