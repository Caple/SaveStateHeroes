var currentYTID = "";

var flashvars = {
    backgroundColor: 0x000000,
    backgroundAlpha: 1,
    chromeColor: 0x383838
}

var params = {
    wmode: 'direct',
    AllowScriptAccess: 'always',
    allowFullScreen: 'true'
};

var slider;
var tooltip;

var currentVideoMode;
var player;


$(document).ready(function () {

    $("#goLiveWindow").dialog({
        autoOpen: false,
        height: 'auto',
        width: 200,
        modal: false,
        resizable: false,
        draggable: true,
        position: 'center'
    });

    $("#stopStreamButton").button({
        icons: {
            primary: "ui-icon-video"
        }
    });
    $("#startStreamButton").button({
        icons: {
            primary: "ui-icon-video"
        }
    });
    $("#stopStreamButton").click(function () {
        if (initialized) {
            serverHub.server.setVideoMode("youtube", "", "");
        }
    });
    $("#startStreamButton").click(function () {
        $("#goLiveWindow").dialog("open");
        $("#goLiveWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
    });

    $("#channelSelect").change(function () {
        if (this.value == "custom") {
            $("#gameLabel").hide();
            $("#gameName").hide();
            $('#gameName').val("-");
        } else {
            $("#gameLabel").show();
            $("#gameName").show();
            $('#gameName').val("");
        }
    });

    $("#pushStreamButton").button();
    $("#pushStreamButton").click(function () {
        if (initialized) {
            var channelSelected = $("#channelSelect").val();
            var gameNameEntered = $("#gameName").val();
            if (gameNameEntered.length < 1) return;
            $("#goLiveWindow").dialog("close");
            serverHub.server.setVideoMode(channelSelected, gameNameEntered, "");
        }
    });

    slider = $('#slider'),
	tooltip = $('.tooltip');
    tooltip.css('margin-left', 65);
    tooltip.hide();
    slider.slider({
        range: "min",
        min: 0,
        value: 100,
        start: function (event, ui) {
            tooltip.fadeIn('fast');
        },
        slide: function (event, ui) {
            var value = slider.slider('value');
            triggerVolumeSlider(value);
        },
        change: function (event, ui) {
            var value = slider.slider('value');
            triggerVolumeSlider(value);
            if (initialized && !loadingValues) serverHub.server.setUserOption('playerVolume', value);
        },
        stop: function (event, ui) {
            tooltip.fadeOut('fast');
        },
    });
    triggerVolumeSlider(50);

    $('.volume').click(function () {
        triggerVolumeSlider(0);
    });
});

var liveCircleStatus = true;
var isLiveNow = false;
var pageLoadDontPlayLiveSound = true;

serverHub.client.switchVideoMode = function (streamer, mode, gameName) {
    player = null;
    currentVideoMode = mode;
    currentYTID = "";
    if (mode == "livestream") {
        $("#videoWrapper").empty();
        $("#videoWrapper").html('<div id="videoObject"></div>');
        swfobject.embedSWF("http://cdn.livestream.com/chromelessPlayer/v21/playerapi.swf", "videoObject", '100%', '100%', "10.0.0", "expressInstall.swf", flashvars, params);
        $("#volumeControlContainer").fadeIn();
    } else if (mode == "twitch") {
        //<iframe frameborder="0" scrolling="no" id="chat_embed" src="http://twitch.tv/chat/embed?channel=twitch&amp;popout_chat=true" height="500" width="350"></iframe>
        $("#videoWrapper").empty();
        $("#videoWrapper").html('<object id="videoObject" type="application/x-shockwave-flash" height="378" width="620" id="live_embed_player_flash" data="http://www.twitch.tv/widgets/live_embed_player.swf?channel=SaveStateHeroes&auto_play=true&start_volume=100" bgcolor="#000000"><param name="allowFullScreen" value="true" /><param name="allowScriptAccess" value="always" /><param name="allowNetworking" value="all" /><param name="movie" value="http://www.twitch.tv/widgets/live_embed_player.swf&auto_play=true&start_volume=100" /><param name="flashvars" value="hostname=www.twitch.tv&channel=SaveStateHeroes&auto_play=true&start_volume=100" /></object>');
        $("#volumeControlContainer").hide();
    } else if (mode == "ssh rtmp") {
        $("#videoWrapper").empty();
        $("#videoWrapper").html('<div id="videoObject"></div>');
        $f("videoObject", {
            src: "/jslib/flowplayer-3.2.16.swf"
        }, {
            plugins: {
                rtmp: {
                    url: "/jslib/flowplayer.rtmp-3.2.12.swf",
                    netConnectionUrl: "rtmp://localhost/oflaDemo",
                    failOverDelay: 1000
                }
                //controls: {
                //    scrubber: false,
                //    fullscreen: false,
                //    autoHide: false,
                //}
            },
            clip: {
                url: 'livea',
                provider: 'rtmp',
                live: true
            },
        })
        //flowplayer.conf = {
        //    rtmp: "rtmp://s3b78u0kbtx79q.cloudfront.net/cfx/st",
        //    swf: "http://releases.flowplayer.org/5.3.2/flowplayer.swf"
        //};
        //flowplayer("videoObject", "/jslib/flowplayer.swf", {
        //    clip: {
        //        url: 'oflaDemo',
        //        live: true,
        //        provider: 'rtmp'
        //    },
        //    plugins: {
        //        rtmp: {
        //            url: "/jslib/flowplayer.rtmp-3.2.12.swf",
        //            netConnectionUrl: 'rtmp://localhost/'
        //        }
        //    }
        //});
        //$f("live", "/jslib/flowplayer.swf", {
        //    clip: {
        //        url: 'hobbit_vp6.flv',
        //        //live: true,
        //        provider: 'rtmp'
        //    },
        //    plugins: {
        //        rtmp: {
        //            url: "/jslib/flowplayer.rtmp-3.2.12.swf",
        //            netConnectionUrl: 'rtmp://localhost/oflaDemo'
        //        }
        //    }
        //});
    } else if (mode == "youtube") {
        $("#videoWrapper").empty();
        $("#videoWrapper").html('<object id="videoObject" type="application/x-shockwave-flash" id="myytflashplayer" data="http://www.youtube.com/apiplayer?enablejsapi=1&version=3" width="640" height="360" style="display: block !important;"><param name="allowScriptAccess" value="always"><param name="bgcolor" value="#000000"></object>');
    } else if (mode == "custom") {
        $("#videoWrapper").empty();
        $("#videoWrapper").html('<object id="videoObject" type="application/x-shockwave-flash" height="378" width="620" id="live_embed_player_flash" data="http://www.twitch.tv/widgets/live_embed_player.swf?channel=twitch&auto_play=true&start_volume=100" bgcolor="#000000"><param name="allowFullScreen" value="true" /><param name="allowScriptAccess" value="always" /><param name="allowNetworking" value="all" /><param name="movie" value="http://www.twitch.tv/widgets/live_embed_player.swf&auto_play=true&start_volume=100" /><param name="flashvars" value="hostname=www.twitch.tv&channel=twitch&auto_play=true&start_volume=100" /></object>');
        $("#volumeControlContainer").hide();
    }
    if (mode == "livestream" || mode == "twitch" || mode == "ssh rtmp") {
        if (isLiveNow) $('#nowLiveCircle').trigger('resize');
        $("#nowLiveText").html("<span style='color: #FFF'>Live: </span><span style='color: #99D'>" + streamer + "</span> - <span style='color: #9C9'>" + gameName + "</span>");
        $("#nowLive").fadeIn('slow', function () {
            $('#nowLiveCircle').show();
            $('#nowLiveCircle').trigger('resize');
            if (!pageLoadDontPlayLiveSound) playSound("/sounds/newlive.mp3", false);
        });
    } else if (mode == "youtube") {
        serverHub.server.syncVideo();
        $("#volumeControlContainer").fadeIn();
    } else {
        $("#nowLive").hide();
    }
    if (mode == "custom") {
        var customTitle = "E3 Live Feed";
        if (isLiveNow) $('#nowLiveCircle').trigger('resize');
        $("#nowLiveText").html("<span style='color: #FFF'>Now Watching: </span><span style='color: #9C9'>" + customTitle + "</span>");
        $("#nowLive").fadeIn('slow', function () {
            $('#nowLiveCircle').trigger('resize');
            if (!pageLoadDontPlayLiveSound) playSound("/sounds/newlive.mp3", false);
        });
    }
    resizeContent();
}

serverHub.client.updateStreamButtons = function (mode) {
    if (mode == "start") {
        $("#stopStreamButton").hide();
        $("#startStreamButton").show();
    } if (mode == "end") {
        $("#startStreamButton").hide();
        $("#stopStreamButton").show();
    } if (mode == "none") {
        $("#startStreamButton").hide();
        $("#stopStreamButton").hide();
    }
}

function livestreamPlayerCallback(event) {
    if (event == 'ready') {
        player = document.getElementById("videoObject");
        player.setDevKey('0EhmS2Wriw6B5301JBbzJmQbdQdsfXl9uDo_UaJ56W5upT1QNYTN-f-kWMD_sPd4HpcDcbVAUR0GEjW64aSx6zA7pnQrttgvoZqJxEoX666uiZM7JSfO-EfRLfw7GRPofElExiHzFuVdH3lk-WLEtw');
        player.load("savestateheroes");http://www.youtube.com/apiplayer?enablejsapi=1&version=3
        player.startPlayback();
        player.showPlayButton(false);
        player.showPauseButton(false);
        player.showThumbnail(false);
        player.showMuteButton(false);
        player.showFullscreenButton(false);
        resizeContent();
        setVideoVolume(videoVolume);
    }
}

function onYouTubePlayerReady() {
    if (currentVideoMode == "youtube") {
        player = document.getElementById("videoObject");
        player.setVolume(50);
    }
    complementPlayer = document.getElementById("cVideo");
    if (complementPlayer != null) {
        complementPlayer.setVolume(60);
    }
}

function toggleCircleStatus() {
    if (isLiveNow) {
        if (liveCircleStatus) {
            $("#nowLiveCircle").animate({ opacity: 0 }, 1000)
            liveCircleStatus = false;
            setTimeout(function () { toggleCircleStatus() }, 300);
        } else {
            $("#nowLiveCircle").animate({ opacity: 1 }, 1000)
            liveCircleStatus = true;
            setTimeout(function () { toggleCircleStatus() }, 7200);
        }
    }
}

function resetVideoSize() {
    $('#videoWrapper').css("height", defaultHeight);
    $('#videoWrapper').css("width", defaultWidth);
    videoResize();
    chatResize();
}

serverHub.client.syncAPVideo = function (videoID, secondsIn, title) {
    if (noVideoMode) return;
    setAP(videoID, secondsIn, title);
}

serverHub.client.syncYTComplement = function (videoID, secondsIn) {
    if (noVideoMode) return;
    setYTComplement(videoID, secondsIn);
}

function setAP(videoID, secondsIn, title) {
    if (currentVideoMode != "youtube" || player == null) {
        setTimeout(function () { setAP(videoID, secondsIn, title) }, 1000);
    } else {
        if (currentYTID != videoID) {
            player.loadVideoById(videoID, secondsIn, "default");
            currentYTID = videoID;
            player.playVideo();
            if (title) {
                $('#nowLiveCircle').hide();
                if ($("#nowLive").is(":visible")) {
                    $("#nowLive").fadeOut('slow', function () {
                        $("#nowLiveText").html(title);
                        $("#nowLive").fadeIn('slow')
                    });
                } else {
                    $("#nowLiveText").html(title);
                    $("#nowLive").fadeIn('slow');
                }
            } else {
                if ($("#nowLive").is(":visible")) {
                    $("#nowLive").fadeOut();
                }
            }
        } else {
            player.seekTo(secondsIn + 1, true);
        }
    }
}

function setYTComplement(videoID, secondsIn) {
    if (videoID == null) {
        $("#cParent").empty();
        $("#cParent").hide();
    } else if (!$("#cParent").is(":visible")) {
        $("#cParent").empty();
        $("#cParent").show();
        $("#cParent").html('<object id="cVideo" type="application/x-shockwave-flash" width="48" height="20" data="http://www.youtube.com/apiplayer?enablejsapi=1&version=3&theme=dark&controls=1"><param name="allowScriptAccess" value="always"><param name="bgcolor" value="#000000"><param name="controls" value="1"></object>');
        setTimeout(function () { setYTComplement(videoID, secondsIn) }, 500);
    } else if (complementPlayer == null) {
        setTimeout(function () { setYTComplement(videoID, secondsIn) }, 500);
    } else {
        complementPlayer.loadVideoById(videoID, secondsIn, "default");
        complementPlayer.playVideo();
    }
}



function triggerVolumeSlider(value) {
    if (noVideoMode) return;
    volume = $('.volume');
    sliderCTRL = $('#slider');
    if (value != sliderCTRL.slider('value')) {
        sliderCTRL.slider('value', value);
    }
    tooltip.css('margin-left', (value - 38) * .9).text(value);
    if (value <= 5) {
        volume.css('background-position', '0 0');
    }
    else if (value <= 25) {
        volume.css('background-position', '0 -25px');
    }
    else if (value <= 75) {
        volume.css('background-position', '0 -50px');
    }
    else {
        volume.css('background-position', '0 -75px');
    };
    if (value > 99) {
        setVideoVolume(1.00);
    } else if (value < 1) {
        setVideoVolume(0.00);
    } else {
        setVideoVolume(value * 0.01);
    }
}

function setVideoVolume(volume) {
    if (noVideoMode) return;
    if (player == null) return;
    videoVolume = volume;
    if (currentVideoMode == "livestream") {
        player.setVolume(volume);
    } else if (currentVideoMode == "youtube") {
        player.setVolume(100);
        //player.setVolume(volume * 100);
    }
}