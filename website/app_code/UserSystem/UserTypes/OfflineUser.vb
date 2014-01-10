Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

Namespace UserSystem

    'This class represents an offline user.
    Public Class OfflineUser
        Implements IComparable(Of OfflineUser)

        Protected Event boundToAccount()
        Protected Event unboundFromAccount()
        Protected Event objectRenamed()

        Sub New(newIP As String)
            _ipAddress = newIP
            unbindAccount()
        End Sub

        Function CompareTo(other As OfflineUser) As Integer Implements IComparable(Of OfflineUser).CompareTo
            Return String.Compare(_displayName, other.displayName)
        End Function

        Shared Operator =(a As OfflineUser, b As OfflineUser) As Boolean
            If (a Is Nothing And b Is Nothing) Then Return True
            If (a Is Nothing And b IsNot Nothing) OrElse (a IsNot Nothing And b Is Nothing) Then Return False
            Return (String.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase) = 0)
        End Operator

        Shared Operator <>(a As OfflineUser, b As OfflineUser) As Boolean
            If (a Is Nothing And b Is Nothing) Then Return False
            If (a Is Nothing And b IsNot Nothing) OrElse (a IsNot Nothing And b Is Nothing) Then Return True
            Return (String.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase) <> 0)
        End Operator

        Overridable Function isMatch(matchText As String) As Boolean
            Return String.Equals(matchText, _displayName, StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(matchText, _name, StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(matchText, _ipAddress)
        End Function

        Sub renameObject(newname As String)
            _name = newname
            _displayName = newname
            _persistence = New UserPersistence(Me)
            _persistence.setDefault("flags", New HashSet(Of String))
            _persistence.setDefault("blockedUsernames", New HashSet(Of String)(StringComparer.OrdinalIgnoreCase))
            flags = _persistence.getField("flags")
            blockedUsernames = _persistence.getField("blockedUsernames")
            updateDisplayname()
            RaiseEvent boundToAccount()
        End Sub

        Sub bindAccount(details As AccountDetails)
            _name = details.username
            _displayName = details.username
            _forumID = details.userID
            _privileges = New userPrivileges(_forumID)
            _options = New UserOptions(_forumID)
            _persistence = New UserPersistence(Me)
            _persistence.setDefault("flags", New HashSet(Of String))
            _persistence.setDefault("blockedUsernames", New HashSet(Of String)(StringComparer.OrdinalIgnoreCase))
            flags = _persistence.getField("flags")
            blockedUsernames = _persistence.getField("blockedUsernames")
            updateDisplayname()
            RaiseEvent unboundFromAccount()
        End Sub

        Sub unbindAccount()
            _name = "(GUID)_" & Guid.NewGuid.ToString
            _displayName = _name
            _forumID = 0
            _privileges = New userPrivileges(0)
            _options = New UserOptions(0)
            _persistence = Nothing
            displayName = "Snorefax"
        End Sub

        Private Sub updateDisplayname()
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As New MySqlCommand("SELECT displayName FROM custom_nicknames WHERE objectName=@name;", connection)
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = _name
            Dim reader As MySqlDataReader = command.ExecuteReader()
            If reader.Read() Then
                _displayName = reader.GetString(0)
            End If
            command.Dispose()
            connection.Close()
            displayName = "Snorefax"
        End Sub

        ReadOnly Property isBoundToAccount() As Boolean
            Get
                Return forumID <> 0
            End Get
        End Property

        Private _ipAddress As String
        ReadOnly Property ipAddress() As String
            Get
                Return _ipAddress
            End Get
        End Property

        Private _name As String
        ReadOnly Property name() As String
            Get
                Return _name
            End Get
        End Property

        Private _forumID As Integer
        ReadOnly Property forumID() As Integer
            Get
                Return _forumID
            End Get
        End Property

        Private _displayName As String
        Property displayName() As String
            Get
                Return _displayName
            End Get
            Set(value As String)
                _displayName = value
                Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
                Dim command As New MySqlCommand("INSERT INTO custom_nicknames VALUES(@objectName, @displayName) " &
                                                "ON DUPLICATE KEY UPDATE displayName=@displayName;", connection)
                command.Parameters.Add("@objectName", MySqlDbType.VarChar).Value = _name
                command.Parameters.Add("@displayName", MySqlDbType.VarChar).Value = _displayName
                command.ExecuteNonQuery()
                command.Dispose()
                connection.Close()
            End Set
        End Property

        Private _privileges As userPrivileges
        ReadOnly Property privileges() As userPrivileges
            Get
                Return _privileges
            End Get
        End Property

        Private _options As UserOptions
        ReadOnly Property options() As UserOptions
            Get
                Return _options
            End Get
        End Property

        Private _persistence As UserPersistence
        ReadOnly Property persistence() As UserPersistence
            Get
                Return _persistence
            End Get
        End Property

        Private flags As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Sub setFlag(flag As String)
            If Not flags.Contains(flag) Then
                flags.Add(flag)
                _persistence.saveField("flags")
            End If
        End Sub

        Sub clearFlag(flag As String)
            If flags.Contains(flag) Then
                flags.Remove(flag)
                _persistence.saveField("flags")
            End If
        End Sub

        Sub toggleFlag(flag As String)
            If flags.Contains(flag) Then
                flags.Remove(flag)
            Else
                flags.Add(flag)
            End If
            _persistence.saveField("flags")
        End Sub

        Function isFlagSet(flag As String) As Boolean
            Return flags.Contains(flag)
        End Function


        Private blockedUsernames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Public ReadOnly Property isBlocked(username As String) As Boolean
            Get
                Return blockedUsernames.Contains(username)
            End Get
        End Property

        Public Sub startBlocking(username As String)
            If Not isBlocked(username) Then
                blockedUsernames.Add(username)
                persistence.setField("blockedUsernames", blockedUsernames)
                persistence.saveField("blockedUsernames")
            End If
        End Sub

        Public Sub stopBlocking(username As String)
            If isBlocked(username) Then
                blockedUsernames.Remove(username)
                persistence.setField("blockedUsernames", blockedUsernames)
                persistence.saveField("blockedUsernames")
            End If
        End Sub

    End Class

End Namespace

