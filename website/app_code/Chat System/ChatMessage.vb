Imports Microsoft.VisualBasic
Imports System.Xml
Imports UserSystem

Public Class ChatMessage

    Public Shared rand As New Random

    Public Enum MessageType As Short
        Normal = 0
        Whisper = 1
        WhisperEcho = 2
        Emote = 3
        ModAction = 5
        NotificationRawHTML = 10
        Channel_Streamer = 15
        Channel_Mod = 20
        Channel_Officer = 25
        Channel_Bumper = 40
    End Enum

    Private _id As Integer
    Private _type As MessageType
    Private _time As Date
    Private _weight As Integer
    Private _sender As String
    Private _senderIP As String
    Private _DisplayName As String
    Private _recipients As String
    Private _html As String
    Private _originalInput As String
    Private _isDeleted As Boolean
    Private _deletedBy As String
    Private _deletedByIP As String

    Private recipientsLookup As New HashSet(Of String)
    Private visibleToAll As Boolean

    Public ReadOnly Property id As Integer
        Get
            Return _id
        End Get
    End Property

    Public ReadOnly Property type As MessageType
        Get
            Return _type
        End Get
    End Property

    Public ReadOnly Property time As Date
        Get
            Return _time
        End Get
    End Property

    Public ReadOnly Property weight As Integer
        Get
            Return _weight
        End Get
    End Property

    Public ReadOnly Property sender As String
        Get
            Return _sender
        End Get
    End Property

    Public ReadOnly Property senderIP As String
        Get
            Return _senderIP
        End Get
    End Property

    Public ReadOnly Property displayName As String
        Get
            Return _DisplayName
        End Get
    End Property

    Public ReadOnly Property recipients As String
        Get
            Return _recipients
        End Get
    End Property

    Public ReadOnly Property html As String
        Get
            Return _html
        End Get
    End Property

    Public ReadOnly Property originalInput As String
        Get
            Return _originalInput
        End Get
    End Property

    Public ReadOnly Property isDeleted As Boolean
        Get
            Return _isDeleted
        End Get
    End Property

    Public ReadOnly Property deletedBy As String
        Get
            Return _deletedBy
        End Get
    End Property

    Public ReadOnly Property deletedByIP As String
        Get
            Return _deletedByIP
        End Get
    End Property

    Public Function isRecipient(user As OnlineUser) As Boolean
        If visibleToAll Then Return True
        If type = MessageType.Channel_Streamer Then
            Return user.privileges.isStreamer Or user.privileges.canModChat
        ElseIf type = MessageType.Channel_Mod Then
            Return user.privileges.canModChat
        ElseIf type = MessageType.Channel_Officer Then
            Return user.privileges.isOfficer
        ElseIf type = MessageType.Channel_Bumper Then
            Return user.privileges.isBumper Or user.privileges.canModChat
        End If
        If recipientsLookup.Contains(user.name) Then
            Return True
        End If
        Return False
    End Function

    Public Sub deleted(by As OnlineUser)
        _isDeleted = True
        _deletedBy = by.name
        _deletedByIP = by.ipAddress
    End Sub

    'Called when rebuilding message cache with DB data
    Sub New(id As Integer, a As Integer, b As Long, c As Integer, d As String, e As String, f As String, g As String, h As String, i As Boolean, j As String, k As String)
        _id = id
        _type = a
        _time = Date.FromFileTimeUtc(b)
        _weight = c
        _sender = d
        _senderIP = e
        _recipients = f
        _html = g
        _originalInput = h
        _isDeleted = i
        _deletedBy = j
        _deletedByIP = k

        If _recipients = "[all]" Then
            visibleToAll = True
        ElseIf _recipients <> "[m]" AndAlso _recipients <> "[o]" Then
            Dim recipientsArray As String() = recipients.Split(","c)
            For Each element As String In recipientsArray
                If Not recipientsLookup.Contains(element) Then
                    recipientsLookup.Add(element.Trim("'"c))
                End If
            Next
        End If

    End Sub


    'Generally called when user input is availible for immediate posting to the chat 
    Public Sub New(id As Integer, sendingUser As OnlineUser, receivingUsers As OnlineUser(), mType As MessageType, text As String)

        'inital stuff
        _id = id
        _type = mType
        _time = Now
        _sender = sendingUser.name
        _senderIP = sendingUser.ipAddress
        _weight = 10
        _weight += Math.Round(text.Length / 30)
        _originalInput = text

        'builder recipients string and lookup
        If receivingUsers IsNot Nothing Then
            If receivingUsers.Length > 0 Then
                Dim recipientsBuilder As New StringBuilder
                For Each receivingUser As OnlineUser In receivingUsers
                    Dim identifier As String = receivingUser.name
                    recipientsBuilder.Append("'"c)
                    recipientsBuilder.Append(identifier)
                    recipientsBuilder.Append("'"c)
                    recipientsBuilder.Append(","c)
                    If Not recipientsLookup.Contains(identifier) Then
                        recipientsLookup.Add(identifier)
                    End If
                Next
                recipientsBuilder.Remove(recipientsBuilder.Length - 1, 1)
                _recipients = recipientsBuilder.ToString
            End If
        Else
            If type = MessageType.Channel_Streamer Then
                _recipients = "[s]"
            ElseIf type = MessageType.Channel_Mod Then
                _recipients = "[m]"
            ElseIf type = MessageType.Channel_Officer Then
                _recipients = "[o]"
            ElseIf type = MessageType.Channel_Bumper Then
                _recipients = "[b]"
            Else
                _recipients = "[all]"
                visibleToAll = True
            End If

        End If

        'sanitize input
        Dim innerHTML As String
        If mType <> MessageType.NotificationRawHTML AndAlso sendingUser <> ChatProcessor.systemUser Then
            innerHTML = Microsoft.Security.Application.Sanitizer.GetSafeHtml(text)
            innerHTML = Regex.Replace(innerHTML, "<[^>]*(>|$)", String.Empty)
        Else
            innerHTML = text
        End If

        Dim largeContentAdded As Boolean
        Dim urlAdded As Boolean

        Dim frontHTML As New StringBuilder
        Dim backHTML As New StringBuilder

        frontHTML.Append("<div class='chatMessage' style='display: none;' data-messageid='")
        frontHTML.Append(_id)
        frontHTML.Append("' data-sender='")
        frontHTML.Append(_sender)
        frontHTML.Append("'>")
        frontHTML.Append("<span class='chatTimestamp' data-time='")
        frontHTML.Append(Now.ToUniversalTime.ToString)
        frontHTML.Append(" UTC' style='display: none;'></span>")


        Select Case _type
            Case MessageType.Channel_Streamer
                frontHTML.Append("<span style='font-weight: bold; color: #292'>&#60S&#62 </span>")
            Case MessageType.Channel_Mod
                frontHTML.Append("<span style='font-weight: bold; color: #f22'>&#60M&#62 </span>")
            Case MessageType.Channel_Officer
                frontHTML.Append("<span style='font-weight: bold; color: #cc2'>&#60O&#62 </span>")
            Case MessageType.Channel_Bumper
                frontHTML.Append("<span style='font-weight: bold; color: #6f6'>&#60b&#62 </span>")
        End Select

        Select Case _type
            Case MessageType.Normal, MessageType.Channel_Bumper, MessageType.Channel_Streamer, MessageType.Channel_Mod, MessageType.Channel_Officer
                'If StreamProcessor.streamer = sendingUser Then
                '    frontHTML.Append("<img src='images/streamerpost.png' style='margin-right: 3px;'/>")
                'End If
                'If sendingUser.privileges.isOfficer Then
                '    frontHTML.Append("<img src='images/BadgeSprite.png' style='margin-right: 3px;'/>")
                'End IF
                'If sendingUser.name = ("GaryOak") Then
                '   frontHTML.Append("<img src='images/garybadge.png' style='margin-right: 3px;'/>")
                'End IF
                If sendingUser.privileges.isBumper OrElse sendingUser.privileges.isBumper Then
                    frontHTML.Append("<img src=""")
                    frontHTML.Append(Utils.getSnorfaxAvatarPath())
                    'frontHTML.Append(utils.getAvatarPath(sendingUser))
                    frontHTML.Append(""" class=""avatarClass"" />")
                End If
                frontHTML.Append("<span style='font-weight: bold; color: ")
                frontHTML.Append(sendingUser.options.chatColorName)
                frontHTML.Append(";'>")
                frontHTML.Append(sendingUser.displayName)
                frontHTML.Append("</span>: <span style='color: ")
                frontHTML.Append(sendingUser.options.chatColorText)
                frontHTML.Append(";'>")
                backHTML.Append("</span>")
            Case MessageType.Emote
                frontHTML.Append("<span> * ")
                frontHTML.Append(sendingUser.displayName)
                backHTML.Append("</span>")
            Case MessageType.ModAction
                frontHTML.Append("<span style=""color: #F4F; font-family: 'CPMono_v07Plain'; font-size: 78%;"">")
                frontHTML.Append("Global -&#62; ")
                backHTML.Append("</span>")
            Case MessageType.NotificationRawHTML
                frontHTML.Append("Notification :: ")
                frontHTML.Append("<span style=""color: #FFF; font-size: 115%;"">")
                backHTML.Append("</span>")
            Case MessageType.WhisperEcho
                frontHTML.Append("<span style=""font-family: 'LuxiSansOblique';"">")
                backHTML.Append("</span>")
                frontHTML.Append("you whispered ")
                frontHTML.Append("<span style='font-weight: bold; color: ")
                frontHTML.Append(sendingUser.options.chatColorName)
                frontHTML.Append(";'>")
                frontHTML.Append(sendingUser.displayName)
                frontHTML.Append("</span> &#62;&#62;&#62; <span style='color: ")
                frontHTML.Append(receivingUsers(0).options.chatColorText)
                frontHTML.Append(";'>")
                backHTML.Append("</span>")
            Case MessageType.Whisper
                frontHTML.Append("<span style=""font-family: 'LuxiSansOblique';"">")
                backHTML.Append("</span>")
                frontHTML.Append("<span style='font-weight: bold; color: ")
                frontHTML.Append(sendingUser.options.chatColorName)
                frontHTML.Append(";'>")
                frontHTML.Append(sendingUser.displayName)
                frontHTML.Append("</span> whispered you &#60;&#60;&#60; <span style='color: ")
                frontHTML.Append(sendingUser.options.chatColorText)
                frontHTML.Append(";'>")
                backHTML.Append("</span>")
                receivingUsers(0).lastWhisperer = sendingUser
        End Select

        backHTML.Append("<br /></div>")

        'parse all URLS
        If Not mType = MessageType.NotificationRawHTML Then
            Dim URLS As MatchCollection = Regex.Matches(innerHTML, "(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»""‘’]))", RegexOptions.IgnoreCase)
            Dim PreviousMatchValues As New List(Of String)

            For Each URLMatch As Match In URLS
                If PreviousMatchValues.Contains(URLMatch.Value) Then Continue For
                PreviousMatchValues.Add(URLMatch.Value)

                If sendingUser.isFlagSet("nolinks") Then
                    innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, "")
                ElseIf Not largeContentAdded Then

                    If Regex.Match(URLMatch.Value, "(?:([^:/?#]+):)?(?://([^/?#]*))?([^?#]*\.(?:jpg|jpeg|gif|png|tiff))(?:\?([^#]*))?(?:#(.*))?", RegexOptions.IgnoreCase).Success Then 'Check if it's an image
                        'format image links
                        If sendingUser.isFlagSet("pedo") Then
                            innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, "(loli here)")
                        Else
                            innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, _
                            String.Format("<br /><div style=""float: left; vertical-align: top; width: 100%""><a href=""{0}"" target=""_blank""><img id=""image_{1}"" data-realsrc=""{0}"" src=""images/imageloading.jpg"" class=""chatImage"" /></a></div><br />", URLMatch.Value, _id))
                            largeContentAdded = True
                            _weight += 3
                        End If
                    Else
                        'format youtube links
                        Dim YoutubeMatch As Match = Regex.Match(URLMatch.Value, "youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase)
                        If YoutubeMatch.Success Then
                            Try
                                Dim videoID As String = YoutubeMatch.Groups(1).Value
                                Dim thumbnailURL As String = "http://img.youtube.com/vi/" + videoID + "/0.jpg"
                                Dim videoXML As New XmlDocument()
                                videoXML.Load("https://gdata.youtube.com/feeds/api/videos/" + videoID + "?v=2")
                                Dim videoTitle As String = videoXML.GetElementsByTagName("title")(0).InnerText
                                Dim videoAuthor As String = videoXML.GetElementsByTagName("name")(0).InnerText
                                innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, String.Format("<br /><div style=""float: left; width: 100%; margin-bottom:7px""><a href=""{0}"" target=""_blank""><img class=""postedImage"" src=""{1}"" style=""float: left; height: 102px; width: 136px; margin-top: 3px; margin-bottom: 3px; margin-right: 8px; border-style: groove; border-width: 2px;"" /><div style=""width: auto""></a> <b>Youtube Video</b><br /><span style=""font-size: 90%"">{2}</span><br /><span style=""font-size: 75%"">by {3}</span></div></div>", URLMatch.Value, thumbnailURL, videoTitle, videoAuthor))
                                largeContentAdded = True
                                _weight += 3
                            Catch e As Exception
                                ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.Channel_Mod, "Youtube parsing error -> " & e.Message)
                            End Try
                        Else
                            innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, [String].Format("[url={0}]{0}[/url]", URLMatch.Value))
                            urlAdded = True
                        End If
                    End If

                Else
                    innerHTML = replaceFirstInstance(innerHTML, URLMatch.Value, [String].Format("[url={0}]{0}[/url]", URLMatch.Value))
                    urlAdded = True
                End If

            Next

            If sendingUser.isFlagSet("rainbow") Then
                innerHTML = "[rainbow]" & innerHTML & "[/rainbow]"
            End If

            If sendingUser.isFlagSet("spoiler") Then
                innerHTML = "[spoiler] " & innerHTML & " [/spoiler]"
            End If

            If sendingUser.isFlagSet("nofair") Then
                innerHTML &= " [-nofair]"
            End If
			
			If sendingUser.isFlagSet("sasuxnaru") Then
				innerHTML = "[color=C7185]Sasu [color=FFFFFF]x[/color] Naru[/color][color=FF0044] is love[/color] [color=FF000]<3[/color] [color=FF155]^.^[/color] "
			End If
			If sendingUser.isFlagSet("dickbutt") Then
				sendingUser.TryDick(sendingUser)
				innerHTML = " Man, I could really use some dicks in my butt right about now"
			End If
			If sendingUser.isFlagSet("chaosdunk") Then
				sendingUser.TrySlam(sendingUser)
				innerHTML &= " I got dunked"
			End If
			If sendingUser.isFlagSet("fedora") Then
				innerHTML &= " *tips fedora*"
			End If
			If sendingUser.isFlagSet("borat") Then
				innerHTML &= " ...NAAAAAT"
			End If
			
			If sendingUser.isFlagSet("nocaps") Then
				innerHTML = innerHTML.ToLower
			End If
			
			If sendingUser.isFlagSet("allcaps") Then
				innerHTML = innerHTML.ToUpper
			End If
			
			If sendingUser.isFlagSet("memearrow") Then
				innerHTML = ">"&innerHTML
			End If

      '      If rand.NextDouble.CompareTo(0.8D) > 0 Then
       '         innerHTML = "[color=C71585]Sasu [color=FFFFFF]x[/color] Naru[/color] [color=FF0044]is love[/color] [color=FF0000]<3[/color]"
       '     Else
        '        innerHTML = innerHTML & " [color=FF0000]<3[/color] ^.^"
         '   End If


            'Parse in-text codes
            Dim codeStack As New Stack(Of ChatCode)
            Dim lastCharacter As Char = "-"c
            Dim tagStartedAt As Integer = -1
            Dim trackingStartTag As Boolean = False
            Dim trackingEndTag As Boolean = False
            Dim trackingParam As Boolean = False
            Dim currentTagName As String = String.Empty
            Dim currentParamValue As String = String.Empty
            Dim charArray As Char() = innerHTML.ToCharArray
            Dim builder As New StringBuilder()
            For iteration As Integer = 0 To charArray.Length - 1
                Dim character As Char = charArray(iteration)
                If trackingParam Then
                    If character = "]"c Then
                        trackingParam = False
                        trackingStartTag = False
                        Dim newCode = New ChatCode
                        newCode.startsAt = tagStartedAt
                        newCode.TagName = currentTagName.ToLower
                        newCode.paramValue = currentParamValue
                        Dim selfEndingValue As String = processIntextCommandStandalones(currentTagName, currentParamValue, sendingUser)
                        If selfEndingValue Is Nothing Then
                            codeStack.Push(newCode)
                        Else
                            If codeStack.Count > 0 Then
                                codeStack.Peek.innerText &= selfEndingValue
                            Else
                                builder.Append(selfEndingValue)
                            End If
                        End If
                    Else
                        currentParamValue &= character
                    End If
                ElseIf trackingStartTag Then
                    If character = "]"c Then
                        trackingStartTag = False
                        Dim newCode = New ChatCode
                        newCode.startsAt = tagStartedAt
                        newCode.TagName = currentTagName.ToLower
                        Dim selfEndingValue As String = processIntextCommandStandalones(currentTagName, currentParamValue, sendingUser)
                        If selfEndingValue Is Nothing Then
                            codeStack.Push(newCode)
                        Else
                            If codeStack.Count > 0 Then
                                codeStack.Peek.innerText &= selfEndingValue
                            Else
                                builder.Append(selfEndingValue)
                            End If
                        End If
                    ElseIf character = "/"c AndAlso lastCharacter = "["c Then
                        trackingStartTag = False
                        trackingEndTag = True
                    ElseIf character = "="c Then
                        trackingParam = True
                        currentParamValue = String.Empty
                    Else
                        currentTagName &= character
                    End If
                ElseIf trackingEndTag Then
                    If character = "]"c Then
                        currentTagName = currentTagName.ToLower
                        trackingEndTag = False
                        If codeStack.Count > 0 Then
                            Dim applicableCode As ChatCode = codeStack.Pop
                            While codeStack.Count > 0 AndAlso Not String.Equals(applicableCode.TagName, currentTagName)
                                codeStack.Peek.innerText &= applicableCode.innerText
                                applicableCode = codeStack.Pop
                            End While
                            If String.Equals(applicableCode.TagName, currentTagName) Then
                                If codeStack.Count > 0 Then
                                    codeStack.Peek.innerText &= processIntextCommand(applicableCode.TagName, applicableCode.innerText, applicableCode.paramValue, sendingUser)
                                Else
                                    builder.Append(processIntextCommand(applicableCode.TagName, applicableCode.innerText, applicableCode.paramValue, sendingUser))
                                End If
                            Else
                                builder.Append(applicableCode.innerText)
                            End If
                        End If
                    Else
                        currentTagName &= character
                    End If
                Else
                    If character = "["c Then
                        trackingStartTag = True
                        tagStartedAt = iteration
                        currentTagName = String.Empty
                    ElseIf codeStack.Count > 0 Then
                        codeStack.Peek.innerText &= character
                    Else
                        builder.Append(character)
                    End If
                End If
                lastCharacter = character
            Next
            If codeStack.Count > 0 Then
                Dim applicableCode As ChatCode = codeStack.Pop
                While codeStack.Count > 0
                    codeStack.Peek.innerText &= applicableCode.innerText
                    applicableCode = codeStack.Pop
                End While
                builder.Append(applicableCode.innerText)
            End If
            innerHTML = builder.ToString

        End If

        If urlAdded Then
            _weight += 3
        End If

        If sendingUser.isFlagSet("lexd") Then
            innerHTML &= " le xD"
        End If

        If sendingUser.isFlagSet("yolo") Then
            innerHTML &= " #yolo"
        End If

        If sendingUser.isFlagSet("pedo") Then
            innerHTML = "Hello, my name is " & sendingUser.displayName & " and I am obligated to inform you that I am worthless and disgusting.  " & innerHTML
        End If



        _html = frontHTML.ToString() & innerHTML & backHTML.ToString()


    End Sub

    Private Function processIntextCommandStandalones(command As String, param As String, sendingUser As OnlineUser) As String
        Select Case command
            'Case "quote"
            '    Dim castParam As Integer
            '    If Integer.TryParse(param, castParam) Then
            '        Dim quote As String = ChatProcessor.generateQuote(castParam, sendingUser)
            '        _weight += quote.Length / 100
            '        Return quote
            '    End If
            '    Return String.Empty
            Case Else
                If command.StartsWith("-") Then
                    Return "<img src='/images/emoticons/" & command.Substring(1) & ".png'>"
                End If
        End Select
        Return Nothing
    End Function

    Private Function processIntextCommand(command As String, innerText As String, param As String, sendingUser As OnlineUser) As String
        Select Case command
            Case "b"
                Return "<b>" & innerText & "</b>"
            Case "i"
                Return "<i>" & innerText & "</i>"
            Case "u"
                Return "<u>" & innerText & "</u>"
            Case "s"
                Return "<s>" & innerText & "</s>"
            Case "spoiler"
                Return "<span class='spoilerBox'>Spoiler<span class='spoilerHidden'>" & innerText & "</span></span>"
            Case "color"
                param = param.TrimStart("#"c)
                If (param.Length = 6 Or param.Length = 3) AndAlso System.Text.RegularExpressions.Regex.IsMatch(param, "\A\b[0-9a-fA-F]+\b\Z") Then
                    Dim format As String = "<span style='color: #{0}'>{1}</span>"
                    Return String.Format(format, param, innerText)
                End If
            Case "bgcolor"
                param = param.TrimStart("#"c)
                If (param.Length = 6 Or param.Length = 3) AndAlso System.Text.RegularExpressions.Regex.IsMatch(param, "\A\b[0-9a-fA-F]+\b\Z") Then
                    Dim format As String = "<span style='background-color: #{0}'>{1}</span>"
                    Return String.Format(format, param, innerText)
                End If
            Case "rainbow"
                Dim format As String = "<span style='color: {0}'>{1}</span>"
                Dim newText As New StringBuilder
                Dim chars As Char() = innerText.ToCharArray
                Dim totalIterations As Integer = chars.Length - 1
                Dim degreesPerIteration As Single = 355 / totalIterations
                For iteration As Integer = 0 To totalIterations
                    newText.AppendFormat(format, Utils.hslToRGBString(degreesPerIteration * iteration, 1, 0.5), chars(iteration))
                Next
                Return newText.ToString
            Case "size"
                Dim format As String = "<span style='font-size: {0}%'>{1}</span>"
                Select Case param.ToLower
                    Case "tiny", "small"
                        Return String.Format(format, 60, innerText)
                    Case "medium", "normal"
                        Return String.Format(format, 100, innerText)
                    Case "large", "big"
                        Return String.Format(format, 150, innerText)
                    Case Else
                        param = param.TrimEnd("%"c)
                        Dim paramInt As Integer
                        If Integer.TryParse(param, paramInt) Then
                            If paramInt < 60 Then paramInt = 60
                            If paramInt > 150 Then paramInt = 150
                            Return String.Format(format, paramInt & "%", innerText)
                        End If
                End Select
            Case "url"
                If String.IsNullOrEmpty(param) Then
                    Return String.Empty
                Else
                    If param.StartsWith("http://") OrElse param.StartsWith("https://") Then
                        Return "<a href='" & param & "' target='_blank'>" & innerText & "</a>"
                    Else
                        Return "<a href='http://" & param & "' target='_blank'>" & innerText & "</a>"
                    End If
                End If
            Case Else
                Return innerText
        End Select
        Return innerText
    End Function

    Private Function replaceFirstInstance(text As String, search As String, replace As String) As String
        Dim pos As Integer = text.IndexOf(search)
        If pos < 0 Then
            Return text
        End If
        Return text.Substring(0, pos) & replace & text.Substring(pos + search.Length)
    End Function

End Class
