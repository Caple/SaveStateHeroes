var appsHub = $.connection.streamerAppsHub;

$(document).ready(function () {

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost") document.title = "Test Site";

    $.connection.hub.start()
    .done(function () {
        appsHub.server.getClientUsername().done(function (result) {

            if (result == null) {
                $("#ratingText").html("You are not logged in.");
                return;
            } else {
                appsHub.server.hasApplication().done(function (result) {
                    if (!result) {
                        $("#ratingText").html("You do not have an active application.");
                    } else {
                        var ping = new Date()
                        appsHub.server.serverPingWithData(new Uint8Array(1024 * 78)).done(function () {
                            var pong = new Date();
                            var rating = (Math.abs(ping - pong)) / 1000;
                            ping = new Date();
                            appsHub.server.serverPingWithData(new Uint8Array(1024 * 78)).done(function () {
                                pong = new Date();
                                rating = rating + ((Math.abs(ping - pong)) / 1000);
                                rating = 16 / rating;
                                appsHub.server.updateConnectionRating(rating);
                                $("#ratingText").html("Rating Updated: " + Math.round(rating * 100) / 100);
                                $.connection.hub.stop();
                            });
                        });
                    }
                });
            }
        });
       
    })
    .fail(function () {
        window.location.href = "disconnected.aspx";
    });
    $.connection.hub.error(function (e) {
        window.location.href = "disconnected.aspx";
    });

});