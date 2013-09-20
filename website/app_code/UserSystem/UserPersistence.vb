Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient
Imports System.Runtime.Serialization.Formatters.Binary

Public Class UserPersistence

    Private userName As String = ""
    Private data As New Dictionary(Of String, Object)

    Sub New(user As UserSystem.OfflineUser)
        userName = user.name
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As New MySqlCommand("SELECT fieldKey, fieldValue FROM custom_user_persistence WHERE userName=@name", connection)
        command.Parameters.Add("@name", MySqlDbType.VarChar).Value = userName
        Dim reader As MySqlDataReader = command.ExecuteReader()
        While reader.Read
            data.Add(reader.GetString(0), bytesToObject(reader.GetValue(1)))
        End While
        reader.Close()
        command.Dispose()
        connection.Close()
    End Sub

    Sub setDefault(key As String, value As Object)
        If Not data.ContainsKey(key) Then
            data(key) = value
            saveField(key)
        End If
    End Sub

    Sub setField(key As String, value As Object)
        data(key) = value
        saveField(key)
    End Sub

    Function getField(key As String) As Object
        If data.ContainsKey(key) Then
            Return data(key)
        Else
            Return Nothing
        End If
    End Function

    Sub saveField(key As String)
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim command As New MySqlCommand("INSERT INTO custom_user_persistence VALUES (@name, @key, @value) " &
                                        "ON DUPLICATE KEY UPDATE fieldValue=@value;", connection)
        command.Parameters.Add("@name", MySqlDbType.VarChar).Value = userName
        command.Parameters.Add("@key", MySqlDbType.VarChar).Value = key
        command.Parameters.Add("@value", MySqlDbType.Blob).Value = ObjectToBytes(data(key))
        command.ExecuteNonQuery()
        command.Dispose()
        connection.Close()
    End Sub

    Private Shared Function bytesToObject(bytes As Byte()) As Object
        Dim memStream As New IO.MemoryStream(bytes)
        Dim formatter As New BinaryFormatter()
        bytesToObject = formatter.Deserialize(memStream)
        memStream.Close()
    End Function

    Private Shared Function ObjectToBytes(obj As Object) As Byte()
        Dim memStream As New IO.MemoryStream()
        Dim formatter As New BinaryFormatter()
        formatter.Serialize(memStream, obj)
        ObjectToBytes = memStream.ToArray()
        memStream.Close()
    End Function

End Class
