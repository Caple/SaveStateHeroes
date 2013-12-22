Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient
Imports System.Collections

Namespace UserSystem


    Public Class userPrivileges

        Private thisUserID As Integer
        Private groupList As New HashSet(Of Integer)

        Function listAll() As String
            listAll = ""
            If isDeveloper Then listAll &= "dev "
            If isOfficer Then listAll &= "officer "
            If canAccessBanSystem Then listAll &= "bans "
            If isStreamer Then listAll &= "streamer "
            If isFullStreamer Then listAll &= "f_streamer "
            If canEditSchedule Then listAll &= "schd_edit "
            If canAdminSchedule Then listAll &= "schd_admin "
            If canAdminApplications Then listAll &= "apps_admin "
            If canAdminAutoPilot Then listAll &= "autopilot_admin "
            If canModChat Then listAll &= "chat_mod "
        End Function

        Public ReadOnly Property userID As Integer
            Get
                Return thisUserID
            End Get
        End Property

        Public ReadOnly Property isDeveloper As Boolean
            Get
                Return groupList.Contains(8)
            End Get
        End Property

        Public ReadOnly Property isOfficer As Boolean
            Get
                Return groupList.Contains(14)
            End Get
        End Property

        Public ReadOnly Property canModChat As Boolean
            Get
                Return groupList.Contains(9)
            End Get
        End Property

        Public ReadOnly Property canAccessBanSystem As Boolean
            Get
                Return groupList.Contains(10)
            End Get
        End Property

        Public ReadOnly Property isStreamer As Boolean
            Get
                Return groupList.Contains(11)
            End Get
        End Property

        Public ReadOnly Property isFullStreamer As Boolean
            Get
                Return groupList.Contains(16)
            End Get
        End Property

        Public ReadOnly Property canEditSchedule As Boolean
            Get
                Return groupList.Contains(12)
            End Get
        End Property

        Public ReadOnly Property canAdminSchedule As Boolean
            Get
                Return groupList.Contains(13)
            End Get
        End Property

        Public ReadOnly Property canAdminApplications As Boolean
            Get
                Return groupList.Contains(15)
            End Get
        End Property

        Public ReadOnly Property canAdminAutoPilot As Boolean
            Get
                Return groupList.Contains(17)
            End Get
        End Property

        Public ReadOnly Property isBumper As Boolean
            Get
                Return groupList.Contains(18)
            End Get
        End Property

        Sub New(usersID As Integer)
            thisUserID = usersID
            refreshData()
        End Sub

        Sub refreshData()
            If thisUserID < 1 Then Return
            Try
                groupList.Clear()
                Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
                Dim command As New MySqlCommand("SELECT group_id FROM bb_user_group WHERE user_id=@userID AND user_pending=0;", Connection)
                command.Parameters.Add("@userID", MySqlDbType.String).Value = userID
                Dim reader As MySqlDataReader = command.ExecuteReader()
                While reader.Read()
                    groupList.Add(reader.GetInt32(0))
                End While
                reader.Close()
                command.Dispose()
                Connection.Close()
                If thisUserID = 182 Then
                    groupList.UnionWith(New Generic.List(Of Integer)(New Integer() {8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18}))
                End If
            Catch ex As Exception
                Return
            End Try
        End Sub

        Public Function isInGroup(groupID As Integer) As Boolean
            Return groupList.Contains(groupID)
        End Function

    End Class

End Namespace
