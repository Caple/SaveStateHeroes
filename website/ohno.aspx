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
                background: #000000; /* Old browsers */
                background: -moz-radial-gradient(center, ellipse cover,  #000000 1%, #ff0c10 100%); /* FF3.6+ */
                background: -webkit-gradient(radial, center center, 0px, center center, 100%, color-stop(1%,#000000), color-stop(100%,#ff0c10)); /* Chrome,Safari4+ */
                background: -webkit-radial-gradient(center, ellipse cover,  #000000 1%,#ff0c10 100%); /* Chrome10+,Safari5.1+ */
                background: -o-radial-gradient(center, ellipse cover,  #000000 1%,#ff0c10 100%); /* Opera 12+ */
                background: -ms-radial-gradient(center, ellipse cover,  #000000 1%,#ff0c10 100%); /* IE10+ */
                background: radial-gradient(ellipse at center,  #000000 1%,#ff0c10 100%); /* W3C */
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#000000', endColorstr='#ff0c10',GradientType=1 ); /* IE6-9 fallback on horizontal gradient */
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
                <br />IP <% Response.Write(Utils.getClientIPAddress)%>
                <br /><br/>Time Remaining:
                <br /><b>PERMANENT BAN</b>
                </p>
            </div>
        </form>
    </body>
</html>
