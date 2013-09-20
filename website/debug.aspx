<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en">
    <head runat="server">

        <title>Debug Page</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <link rel="image_src" href='images/social.png' />
        <meta name="description" content="not another video game streaming site" />
        <meta name="viewport" content="width=device-width" />

        <!-- 3rd party resources -->
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.signalR-1.0.0-rc1.min.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>
        <style>
            html, body {
                background-color: #333;
            }
            #output {
                position:absolute;
                top: 50px;
                left: 50px;
                right: 50px;
                bottom: 50px;
                padding: 20px;
                border: 1px solid #111;
                background-color: #222;
                color: #fff;
                font-size: 2em;
            }
        </style>

    </head>
    <body>

        <div id="output"></div>

        <script type="text/javascript">
            $(document).ready(function () {

                var scheduleHub = $.connection.scheduleHub;

                $.connection.hub.start().done(function () {
                    scheduleHub.server.getUsernameOfSessionUser().done(function (result) {
                        $("#output").html(result);
                    });
                })
                .fail(function () {
                    $("#output").html("connection failed");
                });

                $.connection.hub.error(function () {
                    $("#output").html("hub error");
                });

            });
        </script>

    </body>
</html>
