Imports Microsoft.VisualBasic
Imports System.Threading
Imports UserSystem

Public Class Poll

    Public Shared currentPoll As Poll

    Private ongoing As Boolean
    Private callback As TimerCallback
    Private timer As Timer
    Private ReadOnly workAroundCallback As TimerCallback
    Private ReadOnly workAroundTimer As Timer
    Private ReadOnly votesByIP As New Dictionary(Of String, Integer)
    Private ReadOnly voteCounts As New List(Of Integer)
    Private ReadOnly options As New List(Of String)

    Sub New(user As FrontPageUser, ByRef arguments As String())
        For Each arg As String In arguments
            options.Add(arg.Trim())
        Next
        For iteration As Integer = 0 To options.Count - 1
            voteCounts.Add(0)
        Next
        Dim builder As New StringBuilder()
        builder.Append(user.displayName)
        builder.Append(" started a poll.")
        builder.Append("<div style='margin-top: 10px' font-size: 1em>")
        builder.Append("<span style='font-weight: bold; color: #7d0; padding-left: 5px;'>New Poll</span>")
        builder.Append("<ol style='margin-top: 2px; color: #ddd'>")
        For Each nextOption As String In options
            builder.Append("<li>")
            builder.Append(nextOption)
            builder.Append("</li>")
        Next
        builder.Append("</ol><div style='font-size: 0.85em; margin-left: 5px'>Use /vote # to participate. Vote closes in 30 seconds.</div></div>")
        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, builder.ToString)
        ongoing = True
        workAroundCallback = New TimerCallback(AddressOf workAround)
        workAroundTimer = New Timer(workAroundCallback, Nothing, 100, Timeout.Infinite)
    End Sub

    Public Sub vote(user As FrontPageUser, index As Integer)
        If Not ongoing Then Return
        If index < 0 OrElse index >= options.Count Then
            user.postErrorMessage("Invalid option #")
            Return
        End If
        If votesByIP.ContainsKey(user.ipAddress) Then
            user.postErrorMessage("You have already voted.")
            Return
        End If
        votesByIP.Add(user.ipAddress, index)
        voteCounts(index) = voteCounts(index) + 1
        ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, "" & user.displayName & " voted #" & index + 1)
    End Sub

    Public Function isOngoing() As Boolean
        Return ongoing
    End Function

    Private Sub workAround(state As Object)
        workAroundTimer.Dispose()
        callback = New TimerCallback(AddressOf endPoll)
        timer = New Timer(callback, Nothing, 30000, Timeout.Infinite)
    End Sub

    Private Sub endPoll(state As Object)
        ongoing = False
        timer.Dispose()
        Dim winners = New List(Of Integer)
        Dim highestCount As Integer = 0
        Dim highestIndex As Integer = -1
        For index As Integer = 0 To voteCounts.Count - 1
            Dim count As Integer = voteCounts(index)
            If count > highestCount Then
                highestCount = count
                highestIndex = index
            End If
        Next
        For index As Integer = 0 To voteCounts.Count - 1
            Dim count As Integer = voteCounts(index)
            If count = highestCount Then
                winners.Add(index)
            End If
        Next
        If highestCount < 1 Then
            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, "Poll closed. No one voted.")
        Else
            Dim winner As Integer = -1
            If winners.Count = 1 Then
                winner = winners(0)
            ElseIf winners.Count > 1 Then
                Dim random As New Random
                Dim randomIndex = random.Next(0, winners.Count)
                winner = winners(randomIndex)
            End If


            Dim builder As New StringBuilder()
            builder.Append("Poll closed.")
            builder.Append("<div style='margin-top: 10px; font-size: 1em'>Winner: ")
            builder.Append("<span style='color: #7d0; font-size: 1.2em;'>")
            builder.Append(winner + 1)
            builder.Append(". ")
            builder.Append(options(winner))
            builder.Append("</span></div>")
            builder.Append("</ol>")

            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Normal, builder.ToString)
        End If


    End Sub

End Class
