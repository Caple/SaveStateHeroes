Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

Namespace UserSystem

    Public Class UserOptions

        Public Shared defaultOptions As New UserOptions(0)

        Private thisUserID As Integer
        Private userDataDictionary As New Dictionary(Of String, Object)

        Public ReadOnly Property userID As Integer
            Get
                Return thisUserID
            End Get
        End Property

        Public ReadOnly Property chatColorName As String
            Get
                Return getOption("chatColorName")
            End Get
        End Property

        Public ReadOnly Property chatColorText As String
            Get
                Return getOption("chatColorText")
            End Get
        End Property

        Public ReadOnly Property getCommonOptions As UserOptionsCommon
            Get
                Dim data As New UserOptionsCommon
                data.skinNameMain = userDataDictionary("skinnamemain")
                data.skinNameSchedule = userDataDictionary("skinnameschedule")
                data.sizeMode = userDataDictionary("sizemode")
                data.sizeCustomWidth = userDataDictionary("sizecustomwidth")
                data.chatColorName = userDataDictionary("chatcolorname")
                data.chatColorText = userDataDictionary("chatcolortext")
                data.playSoundOnLive = userDataDictionary("playsoundonlive")
                data.showTimestamps = userDataDictionary("showtimestamps")
                data.tangoStyle = userDataDictionary("tangostyle")
                data.disableAllSounds = userDataDictionary("disableallsounds")
                data.playMessageSound = userDataDictionary("playmessagesound")
                data.playerVolume = userDataDictionary("playervolume")
                Return data
            End Get
        End Property

        Sub New(usersID As Integer)
            thisUserID = usersID
            Try
                Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
                If Not readDBData(Connection) Then
                    Dim Command As New MySqlCommand("INSERT INTO custom_user_data SET userID=@userID;", Connection)
                    Command.Parameters.Add("@userID", MySqlDbType.Int32).Value = userID
                    Command.ExecuteNonQuery()
                    Command.Dispose()
                    readDBData(Connection)
                End If
                Connection.Close()
            Catch ex As Exception
            End Try
        End Sub

        Private Function readDBData(Connection As MySqlConnection) As Boolean
            Dim Command As New MySqlCommand("SELECT * FROM custom_user_data WHERE userID=@userID;", Connection)
            Command.Parameters.Add("@userID", MySqlDbType.String).Value = userID
            Dim Reader As MySqlDataReader = Command.ExecuteReader()
            If Reader.Read() Then
                Dim fieldCount As Integer = Reader.FieldCount - 1
                For iteration As Integer = 1 To fieldCount
                    Dim fieldName As String = Reader.GetName(iteration).ToLower
                    Dim fieldValue As Object = Reader.Item(iteration)
                    Dim fieldType As Type = fieldValue.GetType
                    If fieldType = GetType(ULong) Then
                        If fieldValue = 0 Then
                            fieldValue = False
                        ElseIf fieldValue = 1 Then
                            fieldValue = True
                        End If
                    End If
                    userDataDictionary.Add(fieldName, fieldValue)
                Next
                Reader.Close()
                Command.Dispose()
                Return True
            Else
                Reader.Close()
                Command.Dispose()
                Return False
            End If
        End Function


        Public Function getOption(name As String) As Object
            If userDataDictionary.ContainsKey(name.ToLower) Then
                Return userDataDictionary(name.ToLower)
            Else
                Return Nothing
            End If
        End Function

        Public Sub setOption(name As String, value As Object)
            If userDataDictionary.ContainsKey(name.ToLower) Then
                Try
                    If value.GetType = GetType(String) Then
                        If value.ToLower() = "false" Then
                            value = False
                        ElseIf value.ToLower() = "true" Then
                            value = True
                        End If
                    End If
                    If userDataDictionary(name.ToLower) = value Then Return
                    Dim Connection As New MySqlConnection(Utils.connectionString) : Connection.Open()
                    Dim Command As New MySqlCommand("UPDATE custom_user_data SET `" + name + "`=@value WHERE userID=@userID;", Connection)
                    Command.Parameters.AddWithValue("@value", value)
                    Command.Parameters.Add("@userID", MySqlDbType.Int32).Value = userID
                    Command.ExecuteNonQuery()
                    Command.Dispose()
                    Connection.Close()
                    userDataDictionary(name.ToLower) = value
                Catch ex As Exception
                End Try
            End If
        End Sub

    End Class

End Namespace
