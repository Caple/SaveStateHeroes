var defaultMaximumVideoWidth = 800;
var maximumVideoWidth = 800;
var minimumVideoWidth = 300;
var customWidth;
var aspectRatio = 0.5625;

function setSkin(skinName) {
    if (initialized && !loadingValues) serverHub.server.setUserOption('skinNameMain', skinName);
    //$("#dynamicCSS").attr('href', '/skins/' + skinName + '.css?t=' + new Date().getTime());
    $("#dynamicCSS").attr('href', '/skins/feels.css?t=' + new Date().getTime());
}

function resizeContent() {
    if (refrence_underPlayer == null) return;
    if (!noVideoMode) {
        // Calculate video resolution based on window size, aspect ratio, and user prefrence
        var newVideoWidth = $(window).innerWidth() - refrence_chatWrapper.outerWidth() - 50;
        if (newVideoWidth < minimumVideoWidth) newVideoWidth = minimumVideoWidth;
        if (newVideoWidth > maximumVideoWidth) newVideoWidth = maximumVideoWidth;
        newVideoWidth = Math.round(newVideoWidth)

        var newVideoHeight = Math.round(newVideoWidth * aspectRatio);
        var headerFooterHeight = refrence_pageTop.height() + refrence_pageBottom.height() + 25;
        if (headerFooterHeight + newVideoHeight > $(window).innerHeight()) {
            newVideoHeight = $(window).innerHeight() - headerFooterHeight;
            newVideoWidth = Math.round(newVideoHeight / aspectRatio);
            if (newVideoWidth < minimumVideoWidth) {
                newVideoWidth = minimumVideoWidth;
                newVideoHeight = Math.round(newVideoWidth * aspectRatio);
            }
        }

        //Reize Video
        refrence_videoWrapper.width(newVideoWidth);
        refrence_videoWrapper.height(newVideoHeight);
        $("#videoObject").width(newVideoWidth);
        $("#videoObject").height(newVideoHeight);
        //$('#youtubePlayer > iframe').height(newVideoHeight - 1);
        //$('#youtubePlayer > iframe').width(newVideoWidth - 1);

        //Resize Chat
        refrence_chatWrapper.height(newVideoHeight);
        $('#chatWindow').height(newVideoHeight);
        refrence_chatContent.height(newVideoHeight - 28);
        refrence_chatWrapper.css("margin-left", newVideoWidth + 4 + "px");

        //Resize segment wrappers
        var newContainerWidth = $('#videoWrapper').width() + $('#chatWrapper').width();
        refrence_pageTop.width(newContainerWidth);
        refrence_pageMiddle.width(newContainerWidth);
        refrence_pageBottom.width(newContainerWidth);
        refrence_underPlayer.width(newVideoWidth);

    } else {

        var headerFooterHeight = $("#rightButtons").height() + $("#pageBottom").height() + $("#nowLive").height() - 20;
        var chatHeight = $(window).innerHeight() - headerFooterHeight;
        if (chatHeight < 100) chatHeight = 100;

        $('#chatWrapper').height(chatHeight);
        $('#chatWindow').height(chatHeight);
        $("#chatContent").height(chatHeight - 28);

        var newContainerWidth = $('#chatWrapper').width();
        $('#pageTop').width(newContainerWidth);
        $('#pageMiddle').width(newContainerWidth);
        $('#pageBottom').width(newContainerWidth);;
    }
}