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

serverHub.client.redirect = function (url) {
    $.connection.hub.stop();
    postLocalSystemMessage("disconnected");
    setTimeout(function () {
        window.location = url;
    }, 50);
}

serverHub.client.audioPage = function () {
    playSound("/sounds/page.mp3", true);
}

serverHub.client.audioSXN = function () {
    playSound("/sounds/SXN.mp3", true);
	document.getElementById("dynamicCSS").href = "/skins_special/sxn.css";
}

serverHub.client.DoShake = function () {
(function(){function c(){var e=document.createElement("link");e.setAttribute("type","text/css");e.setAttribute("rel","stylesheet");e.setAttribute("href",f);e.setAttribute("class",l);document.body.appendChild(e)}function h(){var e=document.getElementsByClassName(l);for(var t=0;t<e.length;t++){document.body.removeChild(e[t])}}function p(){var e=document.createElement("div");e.setAttribute("class",a);document.body.appendChild(e);setTimeout(function(){document.body.removeChild(e)},100)}function d(e){return{height:e.offsetHeight,width:e.offsetWidth}}function v(i){var s=d(i);return s.height>e&&s.height<n&&s.width>t&&s.width<r}function m(e){var t=e;var n=0;while(!!t){n+=t.offsetTop;t=t.offsetParent}return n}function g(){var e=document.documentElement;if(!!window.innerWidth){return window.innerHeight}else if(e&&!isNaN(e.clientHeight)){return e.clientHeight}return 0}function y(){if(window.pageYOffset){return window.pageYOffset}return Math.max(document.documentElement.scrollTop,document.body.scrollTop)}function E(e){var t=m(e);return t>=w&&t<=b+w}function S(){var e=document.createElement("audio");e.setAttribute("class",l);e.src=i;e.loop=false;e.addEventListener("canplay",function(){setTimeout(function(){x(k)},500);setTimeout(function(){N();p();for(var e=0;e<O.length;e++){T(O[e])}},15500)},true);e.addEventListener("ended",function(){N();h()},true);e.innerHTML=" <p>If you are reading this, it is because your browser does not support the audio element. We recommend that you get a new browser.</p> <p>";document.body.appendChild(e);e.play()}function x(e){e.className+=" "+s+" "+o}function T(e){e.className+=" "+s+" "+u[Math.floor(Math.random()*u.length)]}function N(){var e=document.getElementsByClassName(s);var t=new RegExp("\\b"+s+"\\b");for(var n=0;n<e.length;){e[n].className=e[n].className.replace(t,"")}}var e=30;var t=30;var n=350;var r=350;var i="//s3.amazonaws.com/moovweb-marketing/playground/harlem-shake.mp3";var s="mw-harlem_shake_me";var o="im_first";var u=["im_drunk","im_baked","im_trippin","im_blown"];var a="mw-strobe_light";var f="//s3.amazonaws.com/moovweb-marketing/playground/harlem-shake-style.css";var l="mw_added_css";var b=g();var w=y();var C=document.getElementsByTagName("*");var k=null;for(var L=0;L<C.length;L++){var A=C[L];if(v(A)){if(E(A)){k=A;break}}}if(A===null){console.warn("Could not find a node of the right size. Please try a different page.");return}c();S();var O=[];for(var L=0;L<C.length;L++){var A=C[L];if(v(A)){O.push(A)}}})()
playSound("/harlemshake.mp3", true);
}

serverHub.client.audioWhisperIn = function () {
    playSound("/sounds/whisper.mp3", false);
}

serverHub.client.audioJason = function () {
    playSound("/sounds/Jason.mp3", false);
}

serverHub.client.audioSlam = function () {
    playSound("/sounds/slam.mp3", false);
	document.getElementById("dynamicCSS").href = "/skins/slam.css";
}

serverHub.client.dickFlag = function () {
	document.getElementById("dynamicCSS").href = "/skins_special/dickbutt.css";
}

serverHub.client.audioAustin = function () {
    playSound("/sounds/Austin.mp3", false);
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