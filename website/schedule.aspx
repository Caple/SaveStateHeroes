<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en">
    <head runat="server">

        <title>Schedule</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <link rel="image_src" href='images/social.png' />
        <meta name="description" content="not another video game streaming site" />
        <meta name="viewport" content="width=device-width" />

        <!-- 3rd party resources -->
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <link rel="stylesheet" type="text/css" href="/jslib/fullcalendar.css" />
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.easing.1.3.js"></script>
        <script type="text/javascript" src="/jslib/jquery.scrollTo-1.4.3.1-min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.fullcalendar-custom.min.js"></script>
        <script type="text/javascript" src="/Scripts/jquery.signalR-1.1.3.min.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>

        <!-- my files -->
        <link rel="stylesheet" type="text/css" href="/css/fonts.css?v=100"/>
        <link rel="stylesheet" type="text/css" href="/css/schedule.css?v=102"/> 
        <script type="text/javascript" src="/app_js/schedule.js?v=102"></script>
        
    </head>
    <body>

        <div id="scheduleWindow" style="display: none">
            <div id="radioButtons">
                <input type="radio" id="week60Button" name="radioButtons" /><label for="week60Button">Week:hour</label>
                <input type="radio" id="week30Button" name="radioButtons" checked="checked" /><label for="week30Button">Week:half</label>
                <input type="radio" id="day60Button" name="radioButtons" /><label for="day60Button">Day:hour</label>
                <input type="radio" id="day30Button" name="radioButtons" /><label for="day30Button">Day:half</label>
                <input type="radio" id="day5Button" name="radioButtons" /><label for="day5Button">Day:5min</label>
            </div>
            <div id="jqCalendar"></div>
        </div>

            

        <!-- jquery dialogs -->
        <div style="display: none">

              <div id="newEventWindow" title="Add/Edit Stream">
                Stream Description<input type="text" id="addEventDescription" />
                <div style="margin-right: 0; margin-top: 10px">
                    <div id="addEventCancel">Cancel</div>
                    <div id="addEventOK">Save</div>
                </div>
            </div>

            <div id="confirmDeleteWinow" title="Delete">
                <p>
                    <span id="deleteIcon" class="ui-icon ui-icon-alert"></span>
                    Delete selected stream?
                </p>
            </div>

        </div>

    </body>
</html>
