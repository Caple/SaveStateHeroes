<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en">
    <head runat="server">

        <title>Chat History</title>

        <!-- 3rd party resources -->
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/Scripts/jquery.signalR-1.1.3.min.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>
        
        <!-- my stuff -->
        <link rel="stylesheet" type="text/css" href="/css/fonts.css?v=100"/>
        <link rel="stylesheet" type="text/css" href="/css/history.css?v=101"/>
        <script type="text/javascript" src="/app_js/history.js?v=102"></script>

    </head>
    <body>

        <div id="rangeSelection">
            <div id="dateSelector"></div>
            <div id="queryRangeButton">Query History</div>
            <div id="loadingMessage">Loading...</div>
        </div>

        <div id="chatWrapper" class="ui-widget-content">
            <div id="chatControlContainer">
                <div id="chatContent"></div>
            </div>
        </div>

    </body>
</html>
