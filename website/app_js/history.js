var serverHub = $.connection.chatHistoryHub;
var showDeleted = true;

$(document).ready(function () {

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost" || document.domain == "derp.cx" || document.domain == "www.derp.cx") {
        document.title = "History: Test Site";
    }

    $(window).bind('resize', function (event) {
        resizeContent();
    });
    resizeContent();

    $("#dateSelector").datepicker();

    $("#queryRangeButton").button();
    $("#queryRangeButton").click(function () {
        fetchMessages();
    });

    $.connection.hub.start()
        .done(function () {

        })
        .fail(function () {
            
        });

    $.connection.hub.error(function () {
        
    });

    $(".spoilerBox").live('click', function (e) {
        var newHTML = $(this).children('.spoilerHidden').clone();
        $(this).fadeOut('slow', function () {
            newHTML.appendTo($(this).parent()).fadeIn('slow');
        })
    });

});

function fetchMessages() {
    var rangeStart = $("#dateSelector").datepicker('getDate');
    var rangeEnd = new Date(rangeStart.getTime() + (24 * 60 * 60 * 1000));
    $("#loadingMessage").show();
    serverHub.server.fetchMessages(rangeStart, rangeEnd).done(function (message) {
        $('#chatContent').html(message);

        $('#chatContent').find('.chatTimestamp').each(function (index) {
            var postTime = new Date($(this).attr('data-time'));
            $(this).html("[" + postTime.toLocaleTimeString() + "] ");
            $(this).show();
        });

        $('#chatContent').find(".userDateTime").each(function (index) {
            var postTime = new Date($(this).attr('data-time'));
            $(this).html(postTime.toLocaleDateString() + " " + postTime.toLocaleTimeString());
        });

        $('#chatContent').find(".chatMessage").each(function (index) {
            $(this).show();
        });

        $('#chatContent').find('.chatImage').each(function () {
            loadImage('#' + $(this).attr('id'));
        });
        $("#loadingMessage").hide();

    });
}

function loadImage(id) {
    var realsrc = $(id).attr("data-realsrc");
    image = new Image();
    image.onload = function () {
        $(id).attr("src", realsrc);
    }
    image.onerror = function () {
        $(id).attr("src", "images/imageinvalid.jpg");
    }
    image.src = realsrc;
}


function resizeContent() {
    var newHeight = $(window).innerHeight() - 25;
    $('#chatWrapper').height(newHeight);
    $('#chatWindow').height(newHeight);
    $('#chatContent').height(newHeight);
}