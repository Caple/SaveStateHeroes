<%@ Page Language="VB" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Voting Details</title>

        <style type="text/css">
        
            html {
                height: 100%;
            }
        
            body {
                height: 100%;
                margin: 0;
                color: #FFF;
                background: #ffffff; /* Old browsers */
                background: url(data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/Pgo8c3ZnIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDEgMSIgcHJlc2VydmVBc3BlY3RSYXRpbz0ibm9uZSI+CiAgPHJhZGlhbEdyYWRpZW50IGlkPSJncmFkLXVjZ2ctZ2VuZXJhdGVkIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgY3g9IjUwJSIgY3k9IjUwJSIgcj0iNzUlIj4KICAgIDxzdG9wIG9mZnNldD0iMCUiIHN0b3AtY29sb3I9IiNmZmZmZmYiIHN0b3Atb3BhY2l0eT0iMSIvPgogICAgPHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjMDAwMDAwIiBzdG9wLW9wYWNpdHk9IjEiLz4KICA8L3JhZGlhbEdyYWRpZW50PgogIDxyZWN0IHg9Ii01MCIgeT0iLTUwIiB3aWR0aD0iMTAxIiBoZWlnaHQ9IjEwMSIgZmlsbD0idXJsKCNncmFkLXVjZ2ctZ2VuZXJhdGVkKSIgLz4KPC9zdmc+);
                background: -moz-radial-gradient(center, ellipse cover,  #ffffff 0%, #000000 100%); /* FF3.6+ */
                background: -webkit-gradient(radial, center center, 0px, center center, 100%, color-stop(0%,#ffffff), color-stop(100%,#000000)); /* Chrome,Safari4+ */
                background: -webkit-radial-gradient(center, ellipse cover,  #ffffff 0%,#000000 100%); /* Chrome10+,Safari5.1+ */
                background: -o-radial-gradient(center, ellipse cover,  #ffffff 0%,#000000 100%); /* Opera 12+ */
                background: -ms-radial-gradient(center, ellipse cover,  #ffffff 0%,#000000 100%); /* IE10+ */
                background: radial-gradient(ellipse at center,  #ffffff 0%,#000000 100%); /* W3C */
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#ffffff', endColorstr='#000000',GradientType=1 ); /* IE6-8 fallback on horizontal gradient */

            }
        
            #message {
                width: 800px;
                height: auto;
                position: absolute;
                left: 10%;
                top: 10%; 
                margin: 20px;
                padding: 20px;
                border-style:solid;
                border-width:medium;
                font-size: 120%;
                background-color:#222;
                font-size: 90%;
                font-family: Verdana;
            }
        
        </style>

    </head>
    <body>
        <form id="runatserver" runat="server" style="width: 100%; height: 100%">
            <div id="message">
                <%
                    Dim topicIDString As String = Page.Request.QueryString("id")
                    Dim topicID As Integer
                    If topicIDString IsNot Nothing Then
                        Integer.TryParse(topicIDString, topicID)
                        If topicID > 0 Then
                            Response.Write(Utils.lookupPollResults(topicID))
                        Else
                            Response.Write("invalid topic id")
                        End If
                    Else
                        Response.Write("please include ?id=0 at the end of the URL<br />(where 0 is the is the topic id you wish to query)")
                    End If
                %>
            </div>
        </form>
    </body>
</html>
