var appsHub = $.connection.streamerAppsHub;
var selectedAppUID = -1;

$(document).ready(function () {

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost") document.title = "Test Site";

    if (!(window.File && window.FileReader && window.FileList && window.Blob)) {
        $("#leftWrapper").hide();
        $("#rightMessageInner").html("Your browser is not supported.");
        return;
    }

    $('#streamProgram').change(function () {
        var value = $(this).attr('value');
        if (value == "Other") {
            $("#hfield_streamProgramOther").fadeIn(function () {
                $("#streamProgramOther").focus();
            });
        } else {
            $("#hfield_streamProgramOther").hide();
        }
    });

    $("#dxDiagTooltipLink").tipTip({
        content: $("#dxDiagTooltip").html(),
        maxWidth: 400,
        delay: 200
    });

    $("#submitButton").button();
    $("#submitButton").click(function () {

        var applicantAge = $("#selectAge").val();
        if ((applicantAge) == "underage") {
            $("#rightMessageInner").html("You are too young to stream here.");
            $("#applyBox").slideUp();
            $("#newApplication").fadeOut(function () {
                $("#rightMessage").slideDown(function () {
                    setTimeout(function () {
                        $("#rightMessage").slideUp();
                    }, 8000)
                });
            });
            return;
        }

        var livestreamName = $("#lsName").val();
        if (!livestreamName) {
            $("#lsNameDesc").effect("highlight", { color: '#f00' }, 2000);
            return;
        }

        var skypeName = $("#skypeName").val();
        if (!skypeName) {
            $("#skypeNameDesc").effect("highlight", { color: '#f00' }, 2000);
            return;
        }

        var programName = $("#streamProgram").val();
        if ((programName) == "Other") {
            programName = $("#streamProgramOther").val();
        }
        if (!programName) {
            $("#programDesc").effect("highlight", { color: '#f00' }, 2000);
            $("#streamProgramOther").effect("highlight", { color: '#f00' }, 2000);
            return;
        }

        var essay1 = $("#essay1").val();
        var essay2 = $("#essay2").val();
        if (!essay1) {
            $("#essay1Desc").effect("highlight", { color: '#f00' }, 2000);
            return;
        }
        if (!essay2) {
            $("#essay2Desc").effect("highlight", { color: '#f00' }, 2000);
            return;
        }

        var files = $("#dxdUpload")[0].files;
        if (files.length < 1) {
            $("#uploadDesc").effect("highlight", { color: '#f00' }, 2000);
            return;
        }

        var targetFile = files[0];
        if (targetFile.size > 10485760) {
            $("#uploadDesc").effect("highlight", { color: '#f00' }, 1000);
            $("#errorMessage").html("Selected file is too large to upload.");
            return;
        }

        $("#newApplication").hide();
        $("#rightMessageInner").html("0% - Reading application data...");
        $("#rightMessage").fadeIn('slow');
        
        var reader = new FileReader();
        reader.onload = function (e) {
            var dxDiagResult = this.result;
            $("#rightMessageInner").html("10% - Rating connection speed...");
            var ping = new Date()
            appsHub.server.serverPingWithData(new Uint8Array(1024 * 78)).done(function () {
                var pong = new Date();
                var rating = (Math.abs(ping - pong)) / 1000;
                $("#rightMessageInner").html("50% - Rating connection speed...");
                ping = new Date();
                appsHub.server.serverPingWithData(new Uint8Array(1024 * 78)).done(function () {
                    pong = new Date();
                    rating = rating + ((Math.abs(ping - pong)) / 1000);
                    rating = 16 / rating;
                    $("#rightMessageInner").html("80% - Uploading application...");
                    appsHub.server.postNewApplication(rating, jstz.determine().name(), applicantAge, programName, essay1, essay2, dxDiagResult, livestreamName, skypeName).done(function (appID) {
                        if (appID > -1) {
                            $("#rightMessage").fadeOut('slow', function () {
                                $("#rightMessageInner").html("100% - Your application has been received.");
                                $("#applyBox").slideUp();
                                $("#newApplication").fadeOut();
                                $("#rightMessage").fadeIn(function () {
                                    setTimeout(function () {
                                        loadApp(appID);
                                        setTimeout(function () {
                                            $("#rightMessage").slideUp('slow');
                                        }, 5000);
                                    }, 500)
                                });
                            });
                        } else {
                            $("#errorMessage").html("Upload failed. Invalid application.");
                            $("#rightMessage").hide();
                            $("#newApplication").fadeIn();
                        }
                    });
                });
            });
          
        };
        reader.onerror = function (e) {
            $("#rightMessage").hide();
            $("#newApplication").fadeIn();
            $("#errorMessage").html("Invalid dxDiag file.");
            $("#uploadDesc").effect("highlight", { color: '#f00' }, 1000);
            console.log(e.getMessage())
        }
        reader.readAsText(files[0]);

    });

    $.connection.hub.start()
    .done(function () {
        appsHub.server.getClientUsername().done(function (result) {

            appsHub.server.queryTestApplications().done(function (appsHTML) { $("#appsATS").append(appsHTML); });
            appsHub.server.queryOpenApplications().done(function (appsHTML) { $("#appsAR").append(appsHTML); });
            appsHub.server.queryTrialApplications().done(function (appsHTML) { $("#appsTrial").append(appsHTML); });
            appsHub.server.queryArchivedApplications().done(function (appsHTML) { $("#appsArchived").append(appsHTML); });
            appsHub.server.queryClosedApplications().done(function (appsHTML) { $("#appsClosed").append(appsHTML); });

            if (result == null) {
                $("#newApplication").html("<div class='pageBoxInner'>You must be logged in on the front page if you wish to start a new application.</div>");
                $("#rightMessage").html("<div class='pageBoxInner'>You must be logged in on the front page if you wish to start a new application.</div>");
                return;
            } else {
                $("#loginInfo").html("Logged in as: " + result);
            }
            appsHub.server.canApply().done(function (boolResult) {
                if (boolResult) {
                    $("#rightMessage").fadeOut(function () {
                        $("#applyBox").fadeIn();
                        $("#newApplication").fadeIn();
                    });
                } else {
                    $("#newApplication").html("<div class='pageBoxInner'>Your account can not apply.</div>");
                    $("#rightMessage").fadeOut();
                }
            });

            appsHub.server.canAdmin().done(function (boolResult) {
                if (boolResult) {
                    $('.appButton[data-appid^="admin"]').hide();
                    $("#adminControvideoWrapper").fadeIn();
                    $("#appsClosed").show();
                }
            });


        });
    })
    .fail(function () {
        window.location.href = "disconnected.aspx";
    });

    $.connection.hub.error(function (e) {
        window.location.href = "disconnected.aspx";
    });

    $(document).on("click", ".appButton", function (event) {
        uid = $(this).attr("data-appid");
        if (uid == "user:Apply") {
            selectedAppUID = -1;
            $("#applicationListing").fadeOut(function () {
                $("#newApplication").fadeIn('slow');
            });
        } else if (uid == "admin:accept") {
            appsHub.server.adminAction(selectedAppUID, "accept");
        } else if (uid == "admin:deny") {
            appsHub.server.adminAction(selectedAppUID, "deny");
        } else if (uid == "admin:reopen") {
            appsHub.server.adminAction(selectedAppUID, "reopen");
        } else if (uid == "admin:delete") {
            appsHub.server.adminAction(selectedAppUID, "delete");
        } else if (uid == "admin:promote") {
            appsHub.server.adminAction(selectedAppUID, "promote");
        } else if (uid == "admin:fail") {
            appsHub.server.adminAction(selectedAppUID, "fail");
        } else if (uid == "admin:makeTrial") {
            appsHub.server.adminAction(selectedAppUID, "makeTrial");
        } else if (uid == "admin:remove-inactive") {
            appsHub.server.adminAction(selectedAppUID, "removed:inactive");
        } else if (uid == "admin:remove-bad") {
            appsHub.server.adminAction(selectedAppUID, "removed:bad");
        } else if (uid == "admin:remove-conduct") {
            appsHub.server.adminAction(selectedAppUID, "removed:conduct");
        } else {
            loadApp(uid);
        }
    });

});

function loadApp(uid) {
    selectedAppUID = uid;
    appsHub.server.queryAppData(uid).done(function (result) {
        if (result != null) {
            $("#newApplication").fadeOut();
            $("#applicationListing").fadeOut(function () {
                var submitDate = new Date(result.submitDateString);
                var creationDate = new Date(result.userCreationDateString);
                var approvalDate = new Date(result.approvalDateString);
                var trialEndDate = new Date(result.trialEndedDateString);
                $("#listingTrialBeganOnField").hide();
                $("#listingTrialEndedOnField").hide();
                $('.appButton[data-appid^="admin"]').slideUp();
                if (result.status == "open:new") {
                    $("#listingStatus").html("Awaiting Review");
                    $("#listingUpdatedBy").empty();
                    $("#adminAccept").slideDown();
                    $("#adminDeny").slideDown();
                } else if (result.status == "open:reopened") {
                    $("#listingStatus").html("Reopened & Awaiting Review");
                    $("#listingUpdatedBy").html("Reopened by: " + result.lastUpdatedBy);
                    $("#adminAccept").slideDown();
                    $("#adminDeny").slideDown();
                } else if (result.status == "test") {
                    $("#listingStatus").html("Awaiting Test-Stream");
                    $("#listingUpdatedBy").html("Approved by: " + result.lastUpdatedBy);
                    $("#adminmakeTrial").slideDown();
                    $("#adminFail").slideDown();
                } else if (result.status == "closed:failedTS") {
                    $("#listingStatus").html("Test-Stream Revealed Inability to Stream");
                    $("#listingUpdatedBy").html("Failed by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                } else if (result.status == "closed:badApp") {
                    $("#listingStatus").html("Invalid, Incomplete or Unsatisfactory");
                    $("#listingUpdatedBy").html("Denied by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                } else if (result.status == "removed:inactive") {
                    $("#listingStatus").html("Purged due to a long peroid of inactivity.");
                    $("#listingUpdatedBy").html("Access removed by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                } else if (result.status == "removed:bad") {
                    $("#listingStatus").html("No longer a streamer (editorial discretion).");
                    $("#listingUpdatedBy").html("Access removed by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                } else if (result.status == "removed:conduct") {
                    $("#listingStatus").html("Streaming rights removed due to a conduct violation.");
                    $("#listingUpdatedBy").html("Access removed by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                } else if (result.status == "trial") {
                    $("#listingStatus").html("Approved To Stream (Trial)");
                    $("#listingUpdatedBy").html("Approved by: " + result.lastUpdatedBy);
                    $("#listingTrialBeganOnField").show();
                    $("#listingTrialBeganOn").html(approvalDate.toLocaleDateString() + " " + approvalDate.toLocaleTimeString());
                    $("#adminPromote").slideDown();
                    $("#adminInactive").slideDown();
                    $("#adminBad").slideDown();
                    $("#adminConduct").slideDown();
                } else if (result.status == "archived:fullstreamer") {
                    $("#listingStatus").html("Archived Application (Full Streamer)");
                    $("#listingUpdatedBy").html("Finalized by: " + result.lastUpdatedBy);
                    $("#listingTrialBeganOnField").show();
                    $("#listingTrialBeganOn").html(approvalDate.toLocaleDateString() + " " + approvalDate.toLocaleTimeString());
                    $("#listingTrialEndedOnField").show();
                    $("#listingTrialEndedOn").html(trialEndDate.toLocaleDateString() + " " + trialEndDate.toLocaleTimeString());
                    $("#adminInactive").slideDown();
                    $("#adminBad").slideDown();
                    $("#adminConduct").slideDown();
                } else if (result.status == "closed:demoted") {
                    $("#listingStatus").html("Trial phase ended. Not a suitable candidate.");
                    $("#listingUpdatedBy").html("Processed by: " + result.lastUpdatedBy);
                    $("#adminReopen").slideDown();
                    $("#adminDelete").slideDown();
                }
                $("#listingTitle").html(result.username + "'s Application");
                $("#listingSubmit").html(submitDate.toLocaleDateString() + " " + submitDate.toLocaleTimeString());
                $("#listingTimezone").html(result.timezone);
                $("#listingAge").html(result.age);
                $("#listingLSUsername").html(result.lsUsername);
                $("#listingProgram").html(result.program);
                $("#listingEssay1").html(result.essay1);
                $("#listingEssay2").html(result.essay2);
                $("#listingDxDiag").val(result.dxDiag);
                $("#listingConnectionRating").html(result.connectionRating);
                $("#listingSkypeName").html(result.skypeName);
                $("#applicationListing").fadeIn();
            });
        }
    });
}

appsHub.client.updateAppStatus = function (appID, category) {
    var selectedElement = $('.appButton[data-appid="' + appID + '"]');

    selectedElement.slideUp('slow', function () {
        if (category == "open") {
            selectedElement.appendTo("#appsAR");
        } else if (category == "test") {
            selectedElement.appendTo("#appsATS");
        } else if (category == "trial") {
            selectedElement.appendTo("#appsTrial");
        } else if (category == "archived") {
            selectedElement.appendTo("#appsArchived");
        } else if (category == "removed") {
            selectedElement.appendTo("#appsClosed");
        } else if (category == "closed") {
            selectedElement.appendTo("#appsClosed");
        }
        if (category == "deleted") {
            selectedElement.remove();
        } else {
            selectedElement.slideDown('slow');
        }
    });
    
    if (category == "deleted") {
        $('.appButton[data-appid^="admin"]').slideUp();
        $("#applicationListing").fadeOut();
    } else if (selectedAppUID == appID) {
        selectedElement.click();
    }
}

appsHub.client.postNewApp = function (appHTML) {
    $("#appsAR").append(appHTML);
}