var serverHub = $.connection.frontPageHub;
var scheduleHub = $.connection.scheduleHub;

var refrence_skinSelect;
var refrence_videoWrapper;
var refrence_livestreamPlayer;
var refrence_youtubePlayer;
var refrence_chatWrapper;
var refrence_chatWindow;
var refrence_chatContent;
var refrence_pageTop;
var refrence_pageMiddle;
var refrence_pageBottom;
var refrence_underPlayer;

$(document).ready(function () {
    refrence_skinSelect = $('#skinSelect');
    refrence_videoWrapper = $('#videoWrapper');
    refrence_chatWrapper = $('#chatWrapper');
    refrence_chatWindow = $('#chatWindow');
    refrence_chatContent = $('#chatContent');
    refrence_pageTop = $('#pageTop');
    refrence_pageMiddle = $('#pageMiddle');
    refrence_pageBottom = $('#pageBottom');
    refrence_underPlayer = $('#underPlayer');

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost" || document.domain == "derp.cx" || document.domain == "www.derp.cx") {
        document.title = "Test Site";
    }

    $(window).bind('resize', function (event) {
        resizeContent();
    });
    resizeContent();

    //UUID.generate();

    $("#nowLive").jEffects([
        {
            event: "resize",
            type: "bomb",
            duration: 800,
            colors: ['#fff'],
            particles: 20,
            radius: 360,
        },
        {
            event: "resize",
            type: "bomb",
            duration: 1200,
            colors: ['#c00'],
            particles: 20,
            radius: 360,
        },
    ]);

    postLocalSystemMessage("connecting to chat...");

    $.connection.hub.start()
        .done(function () {

            serverHub.server.queryVideoMode();

            $(document).bind("idle.idleTimer", function () {
                serverHub.server.updateIdleStatus(true);
            });

            $(document).bind("active.idleTimer", function () {
                serverHub.server.updateIdleStatus(false);
            });

            $.idleTimer(600000);

            $.connection.hub.stateChanged(function (change) {
                checkConnection();
            });

            serverHub.server.querySkins().done(function (skinList) {
                $.each(skinList, function () {
                    refrence_skinSelect.append("<option value='" + this + "'>" + this + "</option>");
                })
                serverHub.server.connectionInit(true).done(function () {
                    initialized = true;
                    postLocalSystemMessage("connected successfully");
                    $("#openScheduleWindow").show();
                    $("#chatTextEntry").val("");
                    $("#chatTextEntry").prop("disabled", false);
                });
            });

            setTimeout(function () {
                pageLoadDontPlayLiveSound = false;
            }, 10000);

        })
        .fail(function () {
            postLocalSystemMessage("server error; could not connect");
        });

    $.connection.hub.error(function () {
        postLocalSystemMessage("connection error occurred");
    });

});

function setMusic(location) {
    if (location != null) {
        $('#music').html('<div id="musicjplayer"></div>');
        $("#musicjplayer").jPlayer({
            ready: function (event) {
                $(this).jPlayer("setMedia", { mp3: location });
                $(this).jPlayer("play");
            },
            swfPath: "../jslib/",
            supplied: "mp3",
            wmode: "direct"
        });
    } else {
        $('#music').empty();
    }
}

function playSound(filename, forced) {
    if (disableAllSounds && !forced) return;
    var soundID = 'sound' + (new Date().getTime());
    $('#sounds').append('<div id="' + soundID + '"></div>');
    $('#' + soundID).jPlayer({
        supplied: 'mp3',
        swfPath: '/jslib/',
        preload: 'auto',
        volume: 0.3,
        ready: function (event) {
            $(this).jPlayer("setMedia", { mp3: filename });
            $(this).jPlayer("play");
        },
    });
}

function stopAllSounds() {
    $("#sounds").empty();
    return false;
}

function GetQueryValue(name) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == name) {
            return pair[1];
        }
    }
    return null
}


var receivingMessages;
var sendingMessages;
function checkConnection() {
    receivingMessages = false;
    sendingMessages = false;
    setTimeout(function () {
        if (!receivingMessages) {
            postLocalSystemMessage("disconnected (not receiving); refreshing");
            setTimeout(function () { location.reload(true) }, 3000);
        } else if (!sendingMessages) {
            postLocalSystemMessage("disconnected (not sending); refreshing");
            setTimeout(function () { location.reload(true) }, 3000);
        }
    }, 20000);
    setTimeout(function () {
        serverHub.server.serverPing().done(function () {
            sendingMessages = true;
        });
    }, 5000);
    setTimeout(function () {
        serverHub.server.serverPing().done(function () {
            sendingMessages = true;
        });
    }, 15000);
}

serverHub.client.clientPing = function () {
    receivingMessages = true;
}