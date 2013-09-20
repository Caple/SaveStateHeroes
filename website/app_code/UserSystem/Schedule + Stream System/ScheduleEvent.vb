Imports Microsoft.VisualBasic
Imports UserSystem

Public Class ScheduleEvent

    Public Class InnerEventObject
        Public id As Integer
        Public creator As String
        Public title As String
        Public start As String
        Public ends As String
        Public allDay As Boolean = False
    End Class

    Public Sub New(id As Integer, user As OfflineUser, starts As Date, ends As Date, desc As String)
        this_eventID = id
        this_innerEvent.id = id
        this_addedByIP = user.ipAddress
        this_addedBy = user.name
        this_innerEvent.creator = user.name
        startTime = starts
        endTime = ends
        description = desc
    End Sub

    Public Sub New(a As Integer, b As String, c As String)
        this_eventID = a
        this_innerEvent.id = a
        this_addedByIP = b
        this_addedBy = c
        this_innerEvent.creator = c
        this_hasBeenEdited = True
    End Sub

    Private this_hasBeenEdited As Boolean
    Private this_isDeleted As Boolean
    Private this_eventID As Integer
    Private this_addedByIP As String
    Private this_addedBy As String
    Private this_editedByIP As String
    Private this_editedBy As String
    Private this_deletedByIP As String
    Private this_deletedBy As String
    Private this_description As String
    Private this_startTime As Date
    Private this_endTime As Date
    Private this_innerEvent As New InnerEventObject

    Public ReadOnly Property eventID As Integer
        Get
            Return this_eventID
        End Get
    End Property

    Public ReadOnly Property isDeleted As Boolean
        Get
            Return this_isDeleted
        End Get
    End Property

    Public ReadOnly Property deletedByIP As String
        Get
            Return this_deletedByIP
        End Get
    End Property

    Public ReadOnly Property deletedBy As String
        Get
            Return this_deletedBy
        End Get
    End Property

    Public ReadOnly Property addedByIP As String
        Get
            Return this_addedByIP
        End Get
    End Property

    Public ReadOnly Property addedBy As String
        Get
            Return this_addedBy
        End Get
    End Property

    Public ReadOnly Property innerEvent As InnerEventObject
        Get
            Return this_innerEvent
        End Get
    End Property

    Public ReadOnly Property hasBeenEdited As Boolean
        Get
            Return this_hasBeenEdited
        End Get
    End Property

    Public Property description As String
        Get
            Return this_description
        End Get
        Set(value As String)
            this_hasBeenEdited = True
            this_description = value
            this_innerEvent.title = this_addedBy + " - " + this_description
        End Set
    End Property

    Public Property startTime As Date
        Get
            Return this_startTime
        End Get
        Set(value As Date)
            this_hasBeenEdited = True
            this_startTime = value
            this_innerEvent.start = Utils.getUNIXTimestamp(value)
        End Set
    End Property

    Public Property endTime As Date
        Get
            Return this_endTime
        End Get
        Set(value As Date)
            this_hasBeenEdited = True
            this_endTime = value
            this_innerEvent.ends = Utils.getUNIXTimestamp(value)
        End Set
    End Property

    Public Property editedByIP As String
        Get
            Return this_editedByIP
        End Get
        Set(value As String)
            this_hasBeenEdited = True
            this_editedByIP = value
        End Set
    End Property

    Public Property editedBy As String
        Get
            Return this_editedBy
        End Get
        Set(value As String)
            this_hasBeenEdited = False
            this_editedBy = value
        End Set
    End Property

    Public Sub savedToDB()
        this_hasBeenEdited = False
    End Sub

    Public Sub delete(byUser As OfflineUser)
        this_deletedByIP = byUser.ipAddress
        this_deletedBy = byUser.name
        this_isDeleted = True
    End Sub

    Public Sub delete(a As String, b As String)
        this_deletedByIP = a
        this_deletedBy = b
        this_isDeleted = True
    End Sub

End Class
