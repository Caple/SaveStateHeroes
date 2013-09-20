serverHub.client.calculatePing = function (count) {
    var ping = new Date()
    serverHub.server.serverPing().done(function () {
        var pong = new Date();
        postLocalSystemMessage("round-trip time was " + Math.abs(ping - pong) + " ms");
    });
}

var noVideoMode = false;
serverHub.client.noVideoMode = function () {
    noVideoMode = true;
    postLocalSystemMessage("no video mode activated");
    $("#volumeControlContainer").add($('#chatWrapper')).add($("#rightButtons")).add($("#videoWrapper")).add($("#underPlayer")).add($("#topArea")).add($("#nowLive")).fadeOut('slow', function () {
        $("#volumeControlContainer").add($("#videoWrapper")).add($("#underPlayer")).add($("#topArea")).add($("#nowLive")).remove();
        $("#rightButtons").css('margin-top', '10px');
        $('#chatWrapper').css("margin-left", 0);
        resizeContent();
        $('#chatWrapper').add($("#rightButtons")).fadeIn('slow');
    });
}


serverHub.client.setClientCookie = function (name, value) {
    if (value == null || value == "") {
        $.removeCookie(name);
    } else {
        $.cookie(name, value);
    }
}

serverHub.client.setUserSkin = function (value) {
    selected = $('#skinSelect');
    selected.val(value);
    $("#dynamicCSS").attr('href', "/skins/" + value + '.css?t=' + new Date().getTime());
    debugger;
}


serverHub.client.setUserSkinSpecial = function (value) {
    $("#dynamicCSS").attr('href', "/skins_special/" + value + '.css?t=' + new Date().getTime());
}

serverHub.client.initiateRefresh = function (timeOut) {
    $.connection.hub.stop();
    postLocalSystemMessage("disconnected");
    setTimeout(function () {
        setTimeout(function () { location.reload(true) }, 500);
    }, timeOut);
}

serverHub.client.audioPage = function () {
    playSound("/sounds/page.mp3", true);
}

serverHub.client.audioWhisperIn = function () {
    playSound("/sounds/whisper.mp3", false);
}

serverHub.client.muteVideo = function () {
    triggerVolumeSlider(0);
}

serverHub.client.jumpToDisconnect = function () {
    window.location = "/disconnected.aspx";
}

serverHub.client.checkConnection = function () {
    serverHub.server.checkConnectionServer()
}