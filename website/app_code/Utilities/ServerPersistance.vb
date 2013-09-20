Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient
Imports System.Runtime.Serialization.Formatters.Binary

Public Class ServerPersistance

    Private Shared data As New Dictionary(Of String, Object)
    Private Shared updated As New Dictionary(Of String, Boolean)
    Private Shared brandNew As New Dictionary(Of String, Boolean)

    Public Shared Sub setDefault(key As String, value As Object)
        If Not data.ContainsKey(key) Then
            data(key) = value
            brandNew(key) = True
            updated(key) = True
        End If
    End Sub

    Public Shared Sub setField(key As String, value As Object)
        If data.ContainsKey(key) Then
            data(key) = value
            updated(key) = True
        Else
            data(key) = value
            brandNew(key) = True
            updated(key) = True
        End If
    End Sub

    Public Shared Function getField(key As String) As Object
        If data.ContainsKey(key) Then
            Return data(key)
        Else
            Return Nothing
        End If
    End Function

    Public Shared Sub save()
        Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
        Dim insertCommand As New MySqlCommand("INSERT INTO custom_persistence VALUES (@key,@value);", connection)
        Dim updateCommand As New MySqlCommand("UPDATE custom_persistence SET fieldValue=@value WHERE fieldKey=@key;", connection)
        insertCommand.Prepare()
        updateCommand.Prepare()
        insertCommand.Parameters.Add("@key", MySqlDbType.VarChar)
        insertCommand.Parameters.Add("@value", MySqlDbType.LongBlob)
        updateCommand.Parameters.Add("@key", MySqlDbType.VarChar)
        updateCommand.Parameters.Add("@value", MySqlDbType.LongBlob)
        SyncLock data
            For Each pair As KeyValuePair(Of String, Object) In data.ToList
                If updated(pair.Key) Then
                    If brandNew(pair.Key) Then
                        insertCommand.Parameters("@key").Value = pair.Key
                        insertCommand.Parameters("@value").Value = ObjectToBytes(pair.Value)
                        insertCommand.ExecuteNonQuery()
                    Else
                        updateCommand.Parameters("@key").Value = pair.Key
                        updateCommand.Parameters("@value").Value = ObjectToBytes(pair.Value)
                        updateCommand.ExecuteNonQuery()
                    End If
                End If
            Next
        End SyncLock
        insertCommand.Dispose()
        updateCommand.Dispose()
        connection.Close()
    End Sub

    Public Shared Sub reload()
        data.Clear()
        SyncLock data
            updated.Clear()
            brandNew.Clear()
            Dim connection As New MySqlConnection(Utils.connectionString) : connection.Open()
            Dim command As New MySqlCommand("SELECT * FROM custom_persistence", connection)
            Dim reader As MySqlDataReader = command.ExecuteReader()
            While reader.Read
                Dim key As String = reader.GetString(0)
                Dim value As Object = bytesToObject(reader.GetValue(1))
                data.Add(key, value)
                updated.Add(key, False)
                brandNew.Add(key, False)
            End While
            reader.Close()
            command.Dispose()
            connection.Close()
        End SyncLock
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
