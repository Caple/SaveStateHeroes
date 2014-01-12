var apHub = $.connection.autopilotHub;
var isBusy = false;
var addVID;
var intervalID;
var playingIndex = -1;
var isAdmin = false;
var highlightTimeout;

$(document).ready(function () {

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost") document.title = "Test Site";

    //
    // header controls
    //
    $("#trackBarSlider").slider({
        disabled: true,
        min: 0,
        max: 0,
        value: 0,
        slide: function (event, ui) {
            $("#currentTime").html(formatTime(ui.value));
        },
        stop: function (event, ui) {
            apHub.server.skipTo(playingIndex, ui.value);
        }
    });

    $("#shuffleMode").button({
        icons: {
            primary: "ui-icon-shuffle"
        },
        disabled: true
    });
    $("#shuffleMode").click(function () {
        apHub.server.toggleShuffleMode();
    });

    $("#skipButton").button({
        icons: {
            primary: "ui-icon-seek-end"
        },
        disabled: true
    });
    $("#skipButton").click(function () {
        apHub.server.skipToNext();
    });

    $("#addButton").button({
        icons: {
            primary: "ui-icon-circle-plus"
        },
        disabled: true
    });
    $("#addButton").click(function () {
        $("#addVideoDialog").dialog("open");
    });



    //
    // add video dialog
    //
    $("#addVideoDialog").dialog({
        autoOpen: false,
        height: 290,
        width: 500,
        position: 'center',
        draggable: true,
        resizable: false,
        modal: true,
        buttons: {
            "Confirm & Add": function () {
                $("#disableEverything").show();
                apHub.server.addVideo(addVID).done(function (succeeded) {
                    if (succeeded) {
                        $("#addVideoDialog").dialog("close");
                        $("#apList").scrollTo("max", 500, {
                            easing: 'easeOutQuad'
                        });
                    } else {
                        $("#addVideoInfo").html("");
                        $("#addVideoError").html("Action Failed");
                    }
                    $("#disableEverything").hide();
                });
            },
        },
        open: function (event, ui) {
            $('.ui-dialog button').button("disable");
        },
        close: function (event, ui) {
            $("#addVideoURL").val("");
            $("#addVideoInfo").html("");
            $("#addVideoError").html("");
            $("#addVideoLoading").hide();
        }
    });

    $("#addVideoURL").on('input', function () {
        $('.ui-dialog button').button("disable");
        $("#addVideoInfo").html("");
        $("#addVideoError").html("");
        $("#addVideoLoading").hide();
        if ($("#addVideoURL").val().length > 0) {
            $("#addVideoLoading").show();
            $(this).doTimeout('#addVideoURL', 250, function () {
                $("#addVideoError").html("");
                $("#addVideoLoading").show();
                apHub.server.cacheVideoInformation(this.val()).done(function (videoInfo) {
                    $("#addVideoLoading").hide();
                    if (videoInfo == null) {
                        $("#addVideoError").html("Invalid Video URL");
                        $('.ui-dialog button').button("disable");
                    } else {
                        if (videoInfo.ytState != null) {
						    $("#addVideoInfo").html("");
                            $("#addVideoInfo").append("Title: " + videoInfo.title + "<br \>");
                            $("#addVideoInfo").append("Author: " + videoInfo.author + "<br \>");
                            $("#addVideoInfo").append("Length: " + videoInfo.lengthFriendly + "<br \>");
                            addVID = videoInfo.videoID;
                            $('.ui-dialog button').button("enable");
                           // $("#addVideoError").html("Invalid Video: " + videoInfo.ytState);
                          //  $('.ui-dialog button').button("disable");
                        } else {
                            $("#addVideoInfo").html("");
                            $("#addVideoInfo").append("Title: " + videoInfo.title + "<br \>");
                            $("#addVideoInfo").append("Author: " + videoInfo.author + "<br \>");
                            $("#addVideoInfo").append("Length: " + videoInfo.lengthFriendly + "<br \>");
                            addVID = videoInfo.videoID;
                            $('.ui-dialog button').button("enable");
                        }
                    }
                });

            });
        }

    });



    //
    // apList stuff
    //
    $("#apList").sortable({
        disabled: true,
        containment: 'parent',
        distance: 5,
        revert: 100,
        start:  function(event, ui) {
            $(ui.item).data("oldIndex", ui.item.index())
        },
        update: function (event, ui) {
            var oldIndex = ui.item.data("oldIndex");
            var newIndex = ui.item.index();
            $(this).sortable('cancel');
            apHub.server.reorderVideos(oldIndex, newIndex).done(function (succeeded) {
                if (!succeeded) {
                    window.location.href = "disconnected.aspx";
                }
            });
        }
    });

    $(document).on("click", ".controlDelete", function (event) {
        if (!isBusy) {
            var itemIndex = $('#apList li').index($(this).parent().parent());
            isBusy = true
            apHub.server.removeVideo(itemIndex).done(function (succeeded) {
                isBusy = false
            });
        }
    });
    $(document).on("dblclick", ".listItem", function (event) {
        if (!isBusy) {
            var itemIndex = $('#apList li').index($(this));
            apHub.server.skipTo(itemIndex, 0);
        }
    });



    //
    // Connection Init
    //
    $.connection.hub.start()
   .done(function () {

       $.connection.hub.stateChanged(function (change) {
           checkConnection();
       });

       apHub.server.isAPAdmin().done(function (boolResult) {
           if (boolResult) {
               isAdmin = true;
               if (isAdmin) {
                   $("#trackBarSlider").slider("option", "disabled", false);
                   $("#shuffleMode").button("option", "disabled", false);
                   $("#skipButton").button("option", "disabled", false);
                   $("#addButton").button("option", "disabled", false);
                   $("#addButton").button("option", "disabled", false);
                   $("#apList").sortable("option", "disabled", false); 
               }
           }
           apHub.server.getAPList().done(function (items) {
               $.each(items, function () {
                   addItem(this);
               });
               apHub.server.forceSync();
               setTimeout(function () {
                   $("#apList").scrollTo($(".listItemSelected"), 500, {
                       offset: -220,
                       easing: 'easeOutQuad'
                   });
               }, 500)
           });
       });

   })
   .fail(function () {
       window.location.href = "disconnected.aspx";
   });

    $.connection.hub.error(function (e) {
        window.location.href = "disconnected.aspx";
    });

});

apHub.client.updateShuffle = function (value) {
    $('#shuffleMode').prop('checked', value);
    $('#shuffleMode').button('refresh');
}

apHub.client.addedItem = function (item) {
    addItem(item);
}

apHub.client.removedItem = function (index) {
    isBusy = true;
    var targetItem = $('#apList li').eq(index);
    var clickedID = targetItem.attr('data-videoID');
    targetItem.hide('drop', 500, function () {
        targetItem.remove();
    });
    isBusy = false;
}

apHub.client.sortedItem = function (oldIndex, newIndex) {
    busy = true;
    var itemToMove = $("#apList li").eq(oldIndex);
    if (newIndex == 0) {
        $("#apList").prepend(itemToMove);
    } else {
        var precedingItem
        if (oldIndex > newIndex) {
            precedingItem = $("#apList li").eq(newIndex - 1);
        } else if (oldIndex < newIndex) {
            precedingItem = $("#apList li").eq(newIndex);
        }
        precedingItem.after(itemToMove);
    }
    busy = false;
}

apHub.client.syncTime = function (index, seconds, total) {
    $("#apList li").removeClass("listItemSelected");
    $("#apList li").eq(index).addClass("listItemSelected");
    if (highlightTimeout != null) {
        clearTimeout(highlightTimeout);
        highlightTimeout = null;
    }
    highlightTimeout = setTimeout(function () {
        $("#apList li").removeClass("listItemSelected");
        $("#apList li").eq(index).addClass("listItemSelected");
    }, 1000);
    if (playingIndex != index) {
        $("#apList").scrollTo($(".listItemSelected"), 500, {
            offset: -220,
            easing: 'easeOutQuad'
        });
    }
    $("#trackBarSlider").slider("option", "max", total);
    $("#trackBarSlider").slider("value", seconds);
    $("#currentTime").html(formatTime(seconds));
    $("#totalTime").html(formatTime(total));
    if (intervalID != null) clearInterval(intervalID);
    intervalID = setInterval(function () {
        var newValue = $("#trackBarSlider").slider("value") + 1;
        $("#trackBarSlider").slider("value", newValue);
        $("#currentTime").html(formatTime($("#trackBarSlider").slider("value")));
    }, 1000)
    playingIndex = index;
}

apHub.client.freezeTrackBar = function () {
    if (intervalID != null) clearInterval(intervalID);
}

function addItem(item) {
    isBusy = true;
    var pageURL = 'http://www.youtube.com/watch?v=' + item.videoID;
    var thumbnailURL = 'http://img.youtube.com/vi/' + item.videoID + '/0.jpg';
    var itemHTML = $('#template_listItem').clone().removeAttr('id');
    itemHTML.children('.itemThumbnail').find('a').attr('href', pageURL);
    itemHTML.find('.videoImage').attr('src', thumbnailURL);
    itemHTML.find('.videoTitle').html(item.title + '<span class="videoAuthor"> by ' + item.author + '</span>');
    itemHTML.find('.videoLength').html('Length: ' + item.lengthFriendly);
    itemHTML.find('.videoAddedBy').html('Added By: ' + item.addedBy);
    if (isAdmin) {
        itemHTML.find('.controlDelete').button({
            icons: {
                primary: "ui-icon-circle-close"
            }
        });
    }
    itemHTML.appendTo('#apList').fadeIn('slow');
    isBusy = false;
}

function formatTime(totalSeconds) {
    var remaining = totalSeconds;
    var hours = Math.floor(totalSeconds / 3600); 
    var remaining = remaining % 3600;
    var minutes = Math.floor(remaining / 60);
    var remaining = remaining % 60;
    var seconds = remaining;
    var finalString = '';
    if (hours > 9) {
        finalString += hours + ':';
    } else if (hours > 0) {
        finalString += '0' + hours + ':';
    }
    if (minutes > 9) {
        finalString += minutes + ':';
    } else if (minutes > 0) {
        finalString += '0' + minutes + ':';
    } else {
        finalString += '00:'
    }
    if (seconds > 9) {
        finalString += seconds;
    } else if (seconds > 0) {
        finalString += '0' + seconds;
    } else {
        finalString += '00'
    }
    return finalString;
}

var receivingMessages;
var sendingMessages;
function checkConnection() {
    receivingMessages = false;
    sendingMessages = false;
    setTimeout(function () {
        if (!receivingMessages) {
            setTimeout(function () { location.reload(true) }, 3000);
        } else if (!sendingMessages) {
            setTimeout(function () { location.reload(true) }, 3000);
        }
    }, 20000);
    setTimeout(function () {
        apHub.server.serverPing().done(function () {
            sendingMessages = true;
        });
    }, 5000);
    setTimeout(function () {
        apHub.server.serverPing().done(function () {
            sendingMessages = true;
        });
    }, 15000);
}

apHub.client.clientPing = function () {
    receivingMessages = true;
}