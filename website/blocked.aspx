<%@ Page Language="VB" %>

<%@ Import Namespace="UserSystem" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Access Denied</title>

        <style type="text/css">
        
            html {
                height: 100%;
            }
        
            body {
                height: 100%;
                margin: 0;
                background-repeat: no-repeat;
                background-attachment: fixed;
                background: #FFF;
                background: -moz-radial-gradient(center, ellipse cover,  rgba(255,181,253,1) 0%, rgba(0,0,0,1) 100%);
                background: -webkit-gradient(radial, center center, 0px, center center, 100%, color-stop(0%,rgba(255,181,253,1)), color-stop(100%,rgba(0,0,0,1)));
                background: -webkit-radial-gradient(center, ellipse cover,  rgba(255,181,253,1) 0%,rgba(0,0,0,1) 100%);
                background: -o-radial-gradient(center, ellipse cover,  rgba(255,181,253,1) 0%,rgba(0,0,0,1) 100%);
                background: -ms-radial-gradient(center, ellipse cover,  rgba(255,181,253,1) 0%,rgba(0,0,0,1) 100%);
                background: radial-gradient(ellipse at center,  rgba(255,181,253,1) 0%,rgba(0,0,0,1) 100%);
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#ffb5fd', endColorstr='#000000',GradientType=1 );
                color: #FFF;
            }
        
            #message {
                width: 400px;
                height: 120px;
                position: absolute;
                left: 50%;
                top: 50%; 
                margin-left: -200px;
                margin-top: -60px;
                padding-top: 20px;
                border-style:solid;
                border-width:medium;
                font-size: 120%;
                text-align: center;
                background-color:#222;
                font-size: 90%;
                font-family: Verdana;
            }
        
        </style>

    </head>
    <body>
        <form id="runatserver" runat="server" style="width: 100%; height: 100%">
            <div id="message">
                <p>You have been banned.
                <br /><br/>Time Remaining:
                <br /><b><% 
                             Dim ipAddress As String = Utils.getClientIPAddress
                             If Infractions.isBanned(ipAddress) Then
                                 Response.Write(Utils.getReadableTimeRemaining(Infractions.banEndsAt(ipAddress)))
                             Else
                                 Response.Write("ban has expired")
                             End If
                         %>
                    </b>
                </p>
            </div>
        </form>
    </body>
</html>
