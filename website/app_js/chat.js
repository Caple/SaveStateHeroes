var initialized = false;
var selectedMessageID;
var selectedUser;
var showModMenu = false;

var messageCache = new Array();
var upRequests = 0;
var processingMessageSendRequest = false;

$.contextMenu.theme = 'xp';

$(document).ready(function () {

    $(".bbButton").button();

    $('#bbBold').click(function () { addBBCode("b", true); });
    $('#bbItallic').click(function () { addBBCode("i", true); });
    $('#bbUnderline').click(function () { addBBCode("u", true); });
    $('#bbStrikethrough').click(function () { addBBCode("s", true); });
    $('#bbSpoiler').click(function () { addBBCode("spoiler", true); });
    $('#bbRainbow').click(function () { addBBCode("rainbow", true); });
    $('#bbDog').click(function () { window.location = "http://www.youtube.com/embed/x8WF5EMhd_A?autoplay=1"; });

    $("#userListWindow").dialog({
        autoOpen: false,
        width: 400,
        height: 'auto',
        modal: false,
        resizable: false,
        draggable: true,
        position: 'right'
    });

    $(function () {
        $("#chatContent").contextMenu(
                [
                    {
                        'USER NAME HERE': {
                            className: 'menuTitle',
                            disabled: true
                        }
                    },
                    $.contextMenu.separator,
                    {
                        'Delete post':
                        function (menuItemClicked, menuObject) {
                            if (initialized) serverHub.server.sendChatMessage("/delete " + selectedMessageID);
                        }
                    },
                    {
                        'Mute poster (10 minutes)':
                        function (menuItemClicked, menuObject) {
                            if (initialized) serverHub.server.sendChatMessage("/mute 10 '" + selectedUser + "'");
                        }
                    },
                ],
                {
                    showTransition: 'fadeIn',
                    hideTransition: 'fadeOut',
                    showSpeed: 'fast',
                    hideSpeed: 'fast',
                    beforeShow: function () {
                        return (selectedMessageID != null && showModMenu);
                    },
                    showCallback: function () {
                    },
                    hideCallback: function () {
                        selectedMessageID = null;
                        selectedUser = null;
                    }
                });
    });

    $(".chatMessage").live('mousedown', function (e) {
        if (e.which == 3) {
            selectedMessageID = $(this).attr("data-messageid");
            selectedUser = $(this).attr("data-sender");
            $(".menuTitle").children().first().html(selectedUser);
        }
    });

    $(".spoilerBox").live('click', function (e) {
        var shouldAutoScroll = autoScrollNeeded();
        var newHTML = $(this).children('.spoilerHidden').clone();
        $(this).fadeOut('slow', function () {
            $(this).replaceWith(newHTML);
            newHTML.fadeIn('slow', function () {
                if (shouldAutoScroll) autoScroll();
            });
            if (shouldAutoScroll) autoScroll();
        })
    });

    $('#chatTextEntry').keydown(function (event) {
        if (event.keyCode == 13) {
            if (initialized) {
                var message = $('#chatTextEntry').val();
                if (message.length > 0) {
                    if (!processingMessageSendRequest) {
                        processingMessageSendRequest = true;
                        serverHub.server.sendChatMessage(message).done(function (success) {
                            if (success) {
                                $('#chatTextEntry').val("");
                                upRequests = 0;
                                messageCache.push(message);
                                if (messageCache.length > 15) messageCache.shift();
                            }
                        });
                        setTimeout(function () {
                            processingMessageSendRequest = false;
                        }, 500);
                    }
                }
            }
            event.preventDefault();
        } else if (event.keyCode == 38) {
            var value = $('#chatTextEntry').val();
            if (value.length == 0 || value === messageCache[messageCache.length - upRequests]) {
                var newValue = messageCache[messageCache.length - upRequests - 1];
                if (newValue != undefined) {
                    $('#chatTextEntry').val(newValue);
                    upRequests = upRequests + 1;
                }
            }
            event.preventDefault();
        } else if (event.keyCode == 40) {
            var value = $('#chatTextEntry').val();
            if (value.length == 0 || value === messageCache[messageCache.length - upRequests]) {
                var newValue = messageCache[messageCache.length - upRequests + 1];
                if (newValue != undefined) {
                    $('#chatTextEntry').val(newValue);
                    upRequests = upRequests - 1;
                }
            }
            event.preventDefault();
        } else {
            upRequests = 0;
        }
    });

    $('#usersOnlineCount').hover(function () { $(this).toggleClass('userCountShaded'); });
    $('#usersOnlineCount').click(function () {
        if (initialized) {
            $("#guiUserList").html("Fetching information...");
            $("#userListWindow").dialog("open");
            $("#userListWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
            serverHub.server.queryUserList()
                    .done(function (retreivedHTML) {
                        $("#guiUserList").html(retreivedHTML);
                        $("#guiUserList").columnize({ columns: 2 });
                        $("#userListWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
                    })
                    .fail(function (error) {
                        $("#guiUserList").html("Failed. Server error.");
                        $("#userListWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
                    });
        } else {
            $("#guiUserList").html("Not Yet Connected");
        }
    });

    refreshTimestamps();
});

function addBBCode(bbCode, requiresClosingTag) {
    var txtarea = $('#chatTextEntry')[0]
    if (txtarea.selectionEnd > 0) {
        if (requiresClosingTag) {
            $(txtarea).val(
                  $(txtarea).val().substring(0, txtarea.selectionStart) +
                  "[" + bbCode + "]" +
                  $(txtarea).val().substring(txtarea.selectionStart, txtarea.selectionEnd) +
                  "[/" + bbCode + "]" +
                  $(txtarea).val().substring(txtarea.selectionEnd)
             );
        } else {
            $(txtarea).val(
                  $(txtarea).val().substring(0, txtarea.selectionStart) +
                  "[" + bbCode + "]" +
                  $(txtarea).val().substring(txtarea.selectionEnd)
             );
        }
    } else {
        if (requiresClosingTag) {
            $(txtarea).val(
                $(txtarea).val() + "[" + bbCode + "]" + "[/" + bbCode + "]"
            );
        } else {
            $(txtarea).val(
                $(txtarea).val() + "[" + bbCode + "]"
            );
        }
    }


}

function refreshTimestamps() {
    var shouldAutoScroll = autoScrollNeeded();
    if (showTimestamps) {
        $('.chatTimestamp').show();
    } else {
        $('.chatTimestamp').hide();
    }
    if (shouldAutoScroll) autoScroll();
}

function autoScrollNeeded() {
    return ($('#chatContent')[0].offsetHeight + $('#chatContent')[0].scrollTop + 5 >= $('#chatContent')[0].scrollHeight);
}

function autoScroll() {
    var chatContentObject = $("#chatContent")[0];
    chatContentObject.scrollTop = chatContentObject.scrollHeight
    setTimeout(function () { chatContentObject.scrollTop = chatContentObject.scrollHeight; }, 75);
}

function loadImage(id) {
    var shouldAutoScroll = autoScrollNeeded();
    var realsrc = $(id).attr("data-realsrc");
    image = new Image();
    image.onload = function () {
        $(id).attr("src", realsrc);
        if (shouldAutoScroll) autoScroll();
    }
    image.onerror = function () {
        $(id).attr("src", "images/imageinvalid.jpg");
        if (shouldAutoScroll) autoScroll();
    }
    image.src = realsrc;
}

function postLocalSystemMessage(message) {
    var shouldAutoScroll = autoScrollNeeded();
    var postTime = new Date();
    var sysMessage = $("<div class='systemMessage'>")
    if (tangoStyle) sysMessage.addClass('tangoStyle');
    if (!showTimestamps) {
        sysMessage.append("<span class='chatTimestamp' style='display: none'>[" + postTime.toLocaleTimeString() + "]</span>")
    } else {
        sysMessage.append("<span class='chatTimestamp'>[" + postTime.toLocaleTimeString() + "]</span>")
    }
    sysMessage.append(" System -&#62; " + message + "<br /></div>");
    sysMessage.appendTo('#chatContent').fadeIn('slow', function () {
        if (shouldAutoScroll) autoScroll();
    });
    if (shouldAutoScroll) autoScroll();
}

serverHub.client.postMessage = function (message) {
    var shouldAutoScroll = autoScrollNeeded();
    var newHTML = $(message);

    newHTML.children('.chatTimestamp').each(function (index) {
        var postTime = new Date($(this).attr('data-time'));
        $(this).html("[" + postTime.toLocaleTimeString() + "] ");
        if (showTimestamps) $(this).show();
    });

    newHTML.find(".userDateTime").each(function (index) {
        var postTime = new Date($(this).attr('data-time'));
        $(this).html(postTime.toLocaleDateString() + " " + postTime.toLocaleTimeString());
    });

    if (tangoStyle) {
        newHTML.addClass('tangoStyle');
    }

    newHTML.appendTo('#chatContent').fadeIn('slow', function () {
        if (shouldAutoScroll) autoScroll();
    });

    if (playMessageSound) playSound("/sounds/message.mp3", true);

    newHTML.find('.chatImage').each(function () {
        loadImage('#' + $(this).attr('id'));
    });

    if (shouldAutoScroll) autoScroll();

}

serverHub.client.clearChat = function () {
    $('#chatContent').empty();
    postLocalSystemMessage("Local chat cleared.");
}

serverHub.client.postSystemMessage = function (message) {
    postLocalSystemMessage(message);
}

serverHub.client.deleteMessage = function (id) {
    $(".chatMessage[data-messageid='" + id + "']").remove();
}
serverHub.client.deleteMessagesBy = function (user) {
    $(".chatMessage[data-sender='" + user + "']").remove();
}

serverHub.client.setUserCount = function (count) {
    $('#userCount').html(count);
    if (count == 1) {
        $('#userCountPlural').hide();
    } else {
        $('#userCountPlural').show();
    }
}

serverHub.client.showPinnedMessage = function (message) {
    var shouldAutoScroll = autoScrollNeeded();
    var newHTML = $('#template_announcement').clone().removeAttr('id');
    newHTML.children('.announcementMessage').html(message);
    newHTML.appendTo('#chatContent').fadeIn('slow', function () {
        if (shouldAutoScroll) autoScroll();
    });
}

