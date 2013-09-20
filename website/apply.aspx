<%@ Page Language="VB" %>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Streamer Applications</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <link rel="stylesheet" type="text/css" href="/jslib/themes/dark-hive/jquery-ui-1.9.1.min.css"/>
        <link rel="stylesheet" type="text/css" href="/jslib/tipTip.css"/>
        <link rel="stylesheet" type="text/css" href="/css/fonts.css"/>
        <link rel="stylesheet" type="text/css" href="/css/apply.css?v=9"/>
        <script type="text/javascript" src="/jslib/jquery-1.8.2.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery-ui-1.9.1.min.js"></script>
        <script type="text/javascript" src="/jslib/jquery.signalR-1.0.0-rc1.min.js"></script>
        <script type="text/javascript" src="/signalr/hubs"></script>
        <script type="text/javascript" src="/jslib/jquery.tipTip.min.js"></script>
        <script type="text/javascript" src="/jslib/timezone.min.js"></script>
        <script type="text/javascript" src="/app_js/apply.js?t=<%=DateTime.UtcNow.ToFileTimeUtc%>"></script>

    </head>
    <body>

        <div id="leftWrapper">
            <div class="pageBox" id="applyBox">
                <div class="appButton" data-appid="user:Apply">New Application</div>
            </div>
            <div class="pageBox" id="appsAR">
                <div class="boxHeader">Awaiting Review</div>
            </div>
            <div class="pageBox" id="appsATS">
                <div class="boxHeader">Awaiting Test-Stream</div>
            </div>
            <div class="pageBox" id="appsTrial">
                <div class="boxHeader">Trial Period</div>
            </div>
            <div class="pageBox" id="appsArchived">
                <div class="boxHeader">Accepted & Archived</div>
            </div>
            <div class="pageBox" id="appsClosed" style="display: none">
                <div class="boxHeader">Ineligible</div>
            </div>
        </div>

        <div id="rightWrapper">

            <div class="pageBox" id="rightMessage">
                <div class="pageBoxInner" id="rightMessageInner">
                    Loading... Please wait...
                </div>
            </div>

            <div class="pageBox" id="applicationListing">
                 <div class="pageBoxInner">
                    <div id="listingUpdatedBy"></div>
                    <div id="listingTitle">
                    </div>
                    <div id="statusText">
                        Status: <span id="listingStatus"></span>
                    </div>
                    <hr />
                    <div class="field">
                        <div class="listingDesc">
                            Submited On
                        </div>
                        <div class="listing" id="listingSubmit"></div>
                    </div>
                    <div class="field" id="listingTrialBeganOnField">
                        <div class="listingDesc">
                            Trial Period Started On
                        </div>
                        <div class="listing" id="listingTrialBeganOn"></div>
                    </div>
                    <div class="field" id="listingTrialEndedOnField">
                        <div class="listingDesc">
                            Trial Period Ended On
                        </div>
                        <div class="listing" id="listingTrialEndedOn"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Timezone
                        </div>
                        <div class="listing" id="listingTimezone"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Age
                        </div>
                        <div class="listing" id="listingAge"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Connection
                        </div>
                        <div class="listing" id="listingConnectionRating"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Program
                        </div>
                        <div class="listing" id="listingProgram"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            LS Name
                        </div>
                        <div class="listing" id="listingLSUsername"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Skype
                        </div>
                        <div class="listing" id="listingSkypeName"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Experience
                        </div>
                        <div class="listing" id="listingEssay1"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            Bio
                        </div>
                        <div class="listing" id="listingEssay2"></div>
                    </div>
                    <div class="field">
                        <div class="listingDesc">
                            DxDiag
                        </div>
                        <textarea rows="20" cols="57" id="listingDxDiag" disabled="disabled"></textarea>
                    </div>
                    <br />
                </div>
            </div>

            <div class="pageBox" id="newApplication">
                
                <div class="pageBoxInner">
                    <div id="newHeading">
                        New Application
                    </div>
                    <div id="loginInfo">

                    </div>
                    <hr />
                    <div class="field">
                        <div class="fieldDesc">
                            Age
                        </div>
                        <select id="selectAge">
                          <option value="underage">Under 18</option>
                          <option value="18-20">18-20</option>
                          <option value="21+">21+</option>
                        </select>
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="programDesc">
                            What program do you use to stream?
                        </div>
                        <select id="streamProgram">
                          <option value="Unsure">I'm not sure...</option>
                          <option value="Procaster">Procaster</option>
                          <option value="XSplit">XSplit</option>
                          <option value="Other">Other</option>
                        </select>
                        <div class="hiddenField" id="hfield_streamProgramOther">
                            <input id="streamProgramOther" type="text" value="" />
                        </div>
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="lsNameDesc">
                            Livestream Username
                        </div>
                        <input id="lsName" type="text" value="" />
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="skypeNameDesc">
                            Skype Username
                        </div>
                        <input id="skypeName" type="text" value="" />
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="essay1Desc">
                            What internet streaming experience do you have?
                        </div>
                        <textarea rows="4" cols="57" id="essay1"></textarea>
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="essay2Desc">
                            Tell us about yourself. Why do you want to stream here?
                        </div>
                        <textarea rows="7" cols="57" id="essay2"></textarea>
                    </div>
                    <div class="field">
                        <div class="fieldDesc" id="uploadDesc">
                            Upload DxDiag
                            <span id="dxDiagTooltipLink"><img src="/images/helptooltip.png" /></span>
                        </div>
                        <input type="file" id="dxdUpload" />
                    </div>
                    <br /><hr />
                    <div id="submitButton">Submit Application</div>
                    <div id="errorMessage">

                    </div>
                </div>
            </div>

        </div>

        <div id="adminControvideoWrapper">
            <div class="pageBox">
                <div class="boxHeader">Admin Controls</div>
                <div class="appButton" data-appid="admin:accept" id="adminAccept">Accept</div>
                <div class="appButton" data-appid="admin:deny" id="adminDeny">Deny</div>
                <div class="appButton" data-appid="admin:reopen" id="adminReopen">Reopen</div>
                <div class="appButton" data-appid="admin:delete" id="adminDelete">Permanently Delete</div>
                <div class="appButton" data-appid="admin:promote" id="adminPromote">Promote to Full Streamer</div>
                <div class="appButton" data-appid="admin:makeTrial" id="adminmakeTrial">Make Trial Streamer</div>
                <div class="appButton" data-appid="admin:fail" id="adminFail">Close (Failed Test Stream)</div>
                <div class="appButton" data-appid="admin:remove-inactive" id="adminInactive">Remove (Inactive)</div>
                <div class="appButton" data-appid="admin:remove-bad" id="adminBad">Remove (Editorial Discretion)</div>
                <div class="appButton" data-appid="admin:remove-conduct" id="adminConduct">Remove (Conduct Violation)</div>
            </div>
        </div>

        <div id="dxDiagTooltip">
            <h3>How to generate a DxDiag file:</h3>
            <ol>
                <li>Press the Windows Key + R.</li>
                <li>Type in dxdiag and click OK.</li>
                <li>Click on the Save All Information button.</li>
                <li>Save this file somewhere you can find it.</li>
            </ol>
        </div>

    </body>
</html>
