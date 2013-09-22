<!DOCTYPE html>
<html lang="en">
    <head runat="server">

        <title>Save State Heroes</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <link rel="image_src" href='images/social_logo.png' />
        <meta name="description" content="Your new video game community." />
        <meta name="viewport" content="width=device-width" />
        <meta http-equiv="X-UA-Compatible" content="IE=edge" />

        <!-- 3rd party resources -->
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <link rel="stylesheet" type="text/css" href="/jslib/jquery.contextmenu.css" />
        <link rel="stylesheet" type="text/css" href="/jslib/spectrum.css" />
        <link rel="stylesheet" type="text/css" href="/jslib/fullcalendar.css" />
        <link rel="stylesheet" type="text/css" href="/jslib/skin/minimalist.css" />
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.easing.1.3.js"></script>
        <script type="text/javascript" src="/jslib/jquery.scrollTo-1.4.3.1-min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.contextMenu.js"></script>
        <script type="text/javascript" src="/jslib/jquery.spectrum.js"></script>
        <script type="text/javascript" src="/jslib/jquery.fullcalendar-custom.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.jeffects.js"></script>
        <script type="text/javascript" src="/jslib/jquery.jplayer.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.idle-timer.js"></script>
        <script type="text/javascript" src="/jslib/jquery.cookie.js"></script>
        <script type="text/javascript" src="/jslib/jquery.columnizer.js"></script>
        <script type="text/javascript" src="/jslib/jquery.signalR-1.1.3.min.js"></script>
        <script type="text/javascript" src="/jslib/flowplayer-3.2.12.min.js" ></script>
        <script type="text/javascript" src="/jslib/swfobject.js"></script>
        <script type="text/javascript" src="/jslib/uuid.core.js"></script>
        <script type="text/javascript" src="/jslib/evercookie-0.4/evercookie.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>
        
        
        <!-- my styles -->
        <link rel="stylesheet" type="text/css" href="/css/default.css?v=106"/>
        <link rel="stylesheet" type="text/css" href="/css/fonts.css?v=100"/>
        <link rel="stylesheet" type="text/css" href="/skins/I%20can%20have%20a%20darkside%20too.css" />
        <link rel="stylesheet" type="text/css" id="dynamicCSS" />

        <!-- my javascript -->
        <script type="text/javascript" src="/app_js/default.js?v=106"></script>
        <script type="text/javascript" src="/app_js/chat.js?v=107"></script>
        <script type="text/javascript" src="/app_js/commands.misc.js?v=105"></script>
        <script type="text/javascript" src="/app_js/login.js?v=103"></script>
        <script type="text/javascript" src="/app_js/options.js?v=101"></script>
        <script type="text/javascript" src="/app_js/styles.resizing.js?v=105"></script>
        <script type="text/javascript" src="/app_js/video.js?v=122"></script>

    </head>
    <body>

        <div id="bgWrapper"></div>

        <!-- above main content -->
        <div id="pageTop" class="content-wrapper">
            <div id="topArea">
                <div id="logo"></div>
                <div id="siteNav">
                    <table border="0" style="vertical-align: top;">
                        <tr>
                            <td>
                                <ul>
                                    <li><a href="/wiki/" target="_blank">Wiki</a></li>
                                    <li><a href="/forum/" target="_blank">Forum</a></li> 
                                    <li><a href="schedule.aspx" target="_blank">Schedule</a></li>
                                </ul>
                            </td>
                            <td>
                                <ul>
                                    <li><a href="apply.aspx" target="_blank">Become a Streamer</a></li>
                                    <li><a href="autopilot.aspx" target="_blank">Autopilot Control</a></li> 
                                </ul> 
                            </td>
                        </tr>
                    </table> 
                </div>
                <div id="socialNav">
                    <div style="float:right; margin-left: 8px; margin-right: 0;">
                        	

   		 	

    			<form action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">
    			<input type="hidden" name="cmd" value="_s-xclick">
    			<input type="hidden" name="hosted_button_id" value="VJY6LLZMKNRDQ">
    			<input type="image" src="http://i.imgur.com/2sYn3Xz.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">
    			<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">
    		</form>
                    </div>
                     <div style="float:right; margin-left: 8px;">
                        <a href="http://steamcommunity.com/groups/savestateheroes" target="_blank">
                             <img src="/images/social/steam.png" />
                        </a>
                     </div>
                     <div style="float:right; margin-left: 8px;">
                        <a href="http://www.youtube.com/user/SaveStateHeroes" target="_blank">
                             <img src="/images/social/youtube.png" />
                        </a>
                     </div>
                     <!--<div style="float:right; margin-left: 8px;">
                        <a href="https://www.facebook.com/SaveStateHeroes" target="_blank">
                             <img src="/images/social/facebook.png" />
                        </a>
                     </div>-->
                     <!--<div style="float:right; margin-left: 8px">
                        <a href="http://omencraft.com/pages/about/" target="_blank">
                             <img src="/images/social/mc.png" />
                        </a>
                     </div>-->
                </div>
            </div>
            <div id="nowLive" style="float: left; display: none">
                <img id="nowLiveCircle" src="images/live.png" alt=""/>
                <span id="nowLiveText" style="font-family: 'LuxiSansRegular'; color: #fff; margin-left: 3px"></span>
            </div>

            <div id="rightButtons" style="display: inline-block; float: right; margin: 0; height: 26px">
                <div id="stopStreamButton" style="font-size: 75%; display: none;">End Stream</div>
                <div id="startStreamButton" style="font-size: 75%; display: none;">Go Live</div>
                <div id="openOptionsWindow" style="font-size: 75%; display: none;">Options...</div>
                <div id="openLoginWindow" style="font-size: 75%; display: none;">Login / Register</div>
                <div id="logoutOfChat" style="font-size: 75%; display: none;">Logout</div>
            </div>
            <br />
        </div>
            

        <!-- main content -->
        <div id="pageMiddle" class="content-wrapper">
                      
            <div id="videoWrapper" class="ui-widget-content" style="height: 450px; width: 800px; float: left; background: #000">
            
            </div>
    
            <div id="chatWrapper" class="ui-widget-content" style="height: 450px; width: 400px; margin-left: 804px;">
                <div id="chatControlContainer">
                    <div id="usersOnlineCount" class=""><span id="userCount">?</span> user<span id="userCountPlural">s</span> online</div>
                    <div id="chatContent"></div>
                    <div id="chatStyles">
                        <div class="bbButton" id="bbBold"><b>B</b></div>
                        <div class="bbButton" id="bbItallic"><i>I</i></div>
                        <div class="bbButton" id="bbUnderline"><u>U</u></div>
                        <div class="bbButton" id="bbStrikethrough"><s>abc</s></div>
                        <div class="bbButton" id="bbSpoiler" style="color: #AAA;">Spoiler</div>
                        <div class="bbButton" id="bbRainbow">
                            <span style="color: #FF0000">-</span><span style="color: #FFFB00">-</span><span style="color: #07FF00">-</span><span style="color: #00FFF4">-</span><span style="color: #000EFF">-</span><span style="color: #ED00FF">-</span><span style="color: #FF0015">-</span>
                        </div>
                    </div>
                    <input type="text" id="chatTextEntry" value="connecting to server..." disabled="disabled" x-webkit-speech />
                </div>
            </div>

        </div>


        <!-- below main content -->
        <div id="pageBottom" class="content-wrapper" style="height: 50px">

            <div id="underPlayer" style="float: left; width: 300px; display: inline">
                <div style="float: left;">
                    <div id="volumeControlContainer">	
	                    <div id="slider" class="ui-slider ui-slider-horizontal ui-widget ui-widget-content ui-corner-all"><div class="ui-slider-range ui-widget-header ui-slider-range-min" style="width: 0%;"></div><a class="ui-slider-handle ui-state-default ui-corner-all" href="#" style="left: 0%;"></a></div>
	                    <div class="volume" style="background-position: 0px 0px;"></div>
                        <div id="tooltipContainer">
                            <div class="tooltip" style="margin-left: -34.2px; display: none;">0</div>
	                    </div>
                    </div>
                </div>
                <div style="float:right;">
                    <div id="cParent" style="border: 1px solid #555; display:none">

                    </div>
                </div>
            </div>

            <div id="underChat" style="float: right; margin-top: 8px; width: 400px; display: inline">
                <div style="display: inline-block; float: left;"></div>
                <div style="display: inline-block; float: right"></div>
            </div>

        </div>

        <!-- footer; dynamic elements -->
        <div id="music" style="position: absolute; top: 0px;"></div>
        <div id="sounds" style="position: absolute; top: 0px;"></div>


        <!-- jquery dialogs and templates -->
        <div style="display: none">

            <div id="template_announcement" style="clear: both; margin-top: 7px; margin-bottom: 7px; padding-top:7px; padding-bottom:7px; border-top: 1px solid #333; border-bottom: 1px solid #333">
                <div style="font-family: 'LuxiSansOblique'; color: #CCC; font-weight: bold">Pinned Announcement ::</div>
                <div class="announcementMessage" style="font-family: 'LuxiSansRegular'; color:#A0A0A0; font-size: 95%; margin-top: 2px;"></div>
            </div>

             <div id="goLiveWindow" title="Go Live" style="font-size: 85%">
                Channel
                <br /><select id="channelSelect" style="width: 170px; margin: 0px; margin-bottom: 10px">
                    <option value="livestream">Livestream</option>
                    <option value="twitch">Twitch.tv</option>
                    <option value="custom">Special: E3</option>
                </select><br />
                <span id="gameLabel">Video Game</span><br /><input type="text" id="gameName"/><br />
                <div id="pushStreamButton" style="font-size: 75%; margin-top: 13px;">Broadcast</div>
            </div>

            <div id="loginWindow" title="Login">
                <div id="invalidLoginText"></div>
                Username<input type="text" id="login_user"/>
                Password<input type="password" id="login_password"/>
                <div id="login_button" style="font-size: 75%; margin-top: 5px;">login</div>
                <div style="margin-top: 12px; font-size: 80%">Need an account?
                    <a href="/forum/ucp.php?mode=register" target="_blank">
                        Register Here
                    </a>
                </div>
            </div>

            <div id="userListWindow" title="Users Online">
                <div id="userListBG"><div id="guiUserList"></div></div>
            </div>

            <div id="scheduleWindow" title="Stream Schedule" style="overflow: hidden">
                <div id="zoomOutButton" style="position: absolute; right: 22px; top:10px;  width: 30px; height: 30px;"></div>
                <div id="zoomInButton" style="position: absolute;  right: 58px; top:10px;  width: 30px; height: 30px;"></div>
                <div id="jqCalendar" style="margin: 5px"></div>
            </div>

            <div id="newEventWindow" title="Add/Edit Stream" style="overflow: hidden">
                Stream Description<input type="text" id="addEventDescription" style="width: 265px; margin-top: 4px;"/>
                <div style="margin-right: 0; margin-top: 10px">
                    <div id="addEventCancel" style="font-size: 85%; float: right; margin-left: 3px">Cancel</div>
                    <div id="addEventOK" style="font-size: 85%; float: right;">Save</div>
                </div>
            </div>

            <div id="confirmDeleteWinow" title="Delete">
                <p><span class="ui-icon ui-icon-alert" style="float: left; margin:  0 7px 15px 0;"></span>Delete selected stream?</p>
            </div>

            <div id="optionsWindow" title="Options..." style="width: 300px">
                <div id="optionTabs">
                    <ul>
                        <li><a href='#tab1'>General</a></li>
                        <li><a href='#tab2'>Style</a></li>
                        <li><a href='#tab3'>Size</a></li>
                    </ul>
                    <div style="background-color: #000; color: #FFF; padding-top: 10px; font-family: Verdana">
                        <div id='tab1'>
                            <div style="padding-top: 3px;"><input id="showTimestampsCheckbox" type="checkbox" onchange="setTimeStampVar(this.checked);return false" /> Show timestamps on chat messages.</div>
                            <div style="padding-top: 3px;"><input id="tangoStyleCheckbox" type="checkbox" onchange="setTangoStyle(this.checked);return false" /> Use wide message spacing.</div>
                            <div style="padding-top: 3px;"><input id="playSoundOnMessageCheckbox" type="checkbox" onchange="playSoundOnMessage(this.checked);return false" /> Play sound for all messages.</div>
                            <div style="padding-top: 3px; margin-bottom: 13px;"><input id="disableSoundsCheckbox" type="checkbox" onchange="disableSounds(this.checked);return false" /> Disable additive sound effects.</div>
                            <span style="font-size: 85%">Front Page Skin:</span><select id="skinSelect" style="width: 310px; margin: 0px;"></select> 
                        </div>
                        <div id='tab2'>
                            <div style="height: 40px">
                                <span style="font-size: 85%; float: left;">Name color <input type='text' id="nameColorPicker" /></span>
                                <span style="font-size: 85%; float: right;">Text color <input type='text' id="textColorPicker" /></span>
                            </div>
                        </div>
                        <div id='tab3'>
                            <span style="font-size: 85%">Prefered Video Size</span>
                            <div style="padding-top: 6px"><input class="VRP" type="radio" name="VideoResizePrefrence" value="default" onchange="setSizePrefrence(this.checked, this.value);return false" checked="checked"/> Default</div>
                            <div style="padding-top: 6px"><input class="VRP" type="radio" name="VideoResizePrefrence" value="expand"  onchange="setSizePrefrence(this.checked, this.value);return false"/> Fill Page</div>
                            <div style="padding-top: 6px"><input class="VRP" type="radio" name="VideoResizePrefrence" value="custom"  onchange="setSizePrefrence(this.checked, this.value);return false"/> Custom Size</div>
	                        <div style="margin-top: 10px; width: 280px; font-size: 100%;" id="SizeSlider"></div><div id="SizeSliderValue" style="display: inline; margin-left: 50px; font-size: 80%;">800 x 450</div>
                        </div>
                    </div>
                </div>
            </div>

        </div>

    </body>
</html>
