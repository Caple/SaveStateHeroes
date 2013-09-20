<%@ Page Language="VB" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Rerate Connection</title>

        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/Scripts/jquery.signalR-1.1.3.min.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>
        <script type="text/javascript" src="/app_js/rerate.js?t=<%=DateTime.UtcNow.ToFileTimeUtc%>"></script>

        <style type="text/css">
        
            html {
                height: 100%;
            }
        
            body {
                height: 100%;
                margin: 0;
                color: #FFF;
                background: rgb(181,189,200); /* Old browsers */
                background: -moz-linear-gradient(top,  rgba(181,189,200,1) 0%, rgba(130,140,149,1) 36%, rgba(40,52,59,1) 100%); /* FF3.6+ */
                background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,rgba(181,189,200,1)), color-stop(36%,rgba(130,140,149,1)), color-stop(100%,rgba(40,52,59,1))); /* Chrome,Safari4+ */
                background: -webkit-linear-gradient(top,  rgba(181,189,200,1) 0%,rgba(130,140,149,1) 36%,rgba(40,52,59,1) 100%); /* Chrome10+,Safari5.1+ */
                background: -o-linear-gradient(top,  rgba(181,189,200,1) 0%,rgba(130,140,149,1) 36%,rgba(40,52,59,1) 100%); /* Opera 11.10+ */
                background: -ms-linear-gradient(top,  rgba(181,189,200,1) 0%,rgba(130,140,149,1) 36%,rgba(40,52,59,1) 100%); /* IE10+ */
                background: linear-gradient(to bottom,  rgba(181,189,200,1) 0%,rgba(130,140,149,1) 36%,rgba(40,52,59,1) 100%); /* W3C */
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#b5bdc8', endColorstr='#28343b',GradientType=0 ); /* IE6-9 */
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

            #ratingText {
                margin-top: 40px;
                font-size: 1.2em;
            }
        </style>

    </head>
    <body>
        <form id="runatserver" runat="server" style="width: 100%; height: 100%">
            <div id="message">
                <div id="ratingText">Please wait...</div>
            </div>
        </form>
    </body>
</html>
