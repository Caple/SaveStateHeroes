<%@ Application Language="VB" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="UserSystem" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="Microsoft.AspNet.SignalR" %>
<script RunAt="server">
    
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        RouteTable.Routes.MapHubs()
        Utils.lastRecycle = Now
        Utils.serverPath = Server.MapPath("/")
        ServerPersistance.reload()
        ServerPersistance.setDefault("announcement", "")
        Infractions.init()
        ChatProcessor.init()
        ScheduleProcessor.init()
        Connections.init()
        AutopilotHub.init()
        StreamerAppsHub.init()
        StreamProcessor.init()
        ServerScheduler.start()
        ServerPersistance.setDefault("updated", False)
        If ServerPersistance.getField("updated") = True Then
            ServerPersistance.setField("updated", False)
            ServerPersistance.save()
            ChatProcessor.postNewMessage(Nothing, Nothing, ChatMessage.MessageType.NotificationRawHTML, "update complete")
        End If
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        StreamProcessor.persist()
        Connections.persist()
        ServerPersistance.save()
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Utils.logError(Server.GetLastError)
        Server.ClearError()
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        If UserSystem.Infractions.isBanned(Utils.getClientIPAddress()) Then
            HttpContext.Current.RewritePath("~/blocked.aspx")
        ElseIf HttpContext.Current.Request.Browser.Browser = "IE" AndAlso HttpContext.Current.Request.Browser.Version < 8 Then
            HttpContext.Current.RewritePath("~/browser.aspx")
            Return
        End If
        Dim domain As String = Request.Url.Host
    End Sub
    
    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

</script>

