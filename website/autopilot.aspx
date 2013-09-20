<%@ Page Language="VB" %>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>AP</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <link rel="stylesheet" type="text/css" href="/jslib/tipTip.css"/>
        <link rel="stylesheet" type="text/css" href="/css/fonts.css"/>
        <link rel="stylesheet" type="text/css" href="/css/autopilot.css?v=3"/>
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.json.js"></script>
        <script type="text/javascript" src="/Scripts/jquery.signalR-1.1.3.min.js"></script>
        <script type="text/javascript" src='<%= ResolveClientUrl("~/signalr/hubs") %>'></script>
        <script type="text/javascript" src="/jslib/jquery.easing.1.3.js"></script>
        <script type="text/javascript" src="/jslib/jquery.scrollTo-1.4.3.1-min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.ba-dotimeout.min.js"></script>
        <script type="text/javascript" src="/app_js/autopilot.js?t=<%=DateTime.UtcNow.ToFileTimeUtc%>"></script>
		

    </head>
    <body>

        <div id="disableEverything"></div>

        <div id="topControls">
            <div id="trackBarSlider"></div>
            <div id="underTrackBar">
                <div id="videoTimeInformation">
                    <span id="currentTime">0</span>
                     / 
                    <span id="totalTime">0</span>
                </div>
                <input type="checkbox" id="shuffleMode" /><label for="shuffleMode">Shuffle</label>
                <div id="skipButton">Skip</div>
                <div id="addButton">Add Video</div>
            </div>
        </div>

        <ul id="apList">
        </ul>

        <div id="dialogsAndTemplates">

            <div id="addVideoDialog" title="Add Video">
                Video URL
                <div>
                    <input type="text" id="addVideoURL" />
                </div>
                <br />
                <div id="addVideoLoading">Loading Video...</div>
                <div id="addVideoError"></div>
                <div id="addVideoInfo"></div>
            </div>
            
            <ul>
                <li id="template_listItem" class='listItem'>
                    <div class='itemThumbnail'>
                        <a href='/' target='_blank'>
                            <img class='videoImage' src='/'/>
                        </a>
                    </div>
                    <div class='listItemControls'>
                        <div class='controlDelete'></div>
                    </div>
                    <div class='itemDescription'>
                        <div class='videoTitle'></div>
                        <div class='videoLength'></div>
                        <div class='videoAddedBy'></div>
                    </div>
                </li>
            </ul>

        </div>

    </body>
</html>
