<%@ Page Language="VB" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Maintenance</title>

        <style type="text/css">
        
            html {
                height: 100%;
            }
        
            body {
                height: 100%;
                margin: 0;
                background: #888
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
                color: #FFF;
            }
        
        </style>

    </head>
    <body>
        <form id="runatserver" runat="server" style="width: 100%; height: 100%">
            <div id="message">
                <b><p>You are using an old or incompatible browser.</p>
                Please get a <a href="https://www.google.com/intl/en/chrome/browser/">better one</a> to view this site.</b>
            </div>
        </form>
    </body>
</html>
