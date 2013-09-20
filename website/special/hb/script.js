$(document).ready(function () {

    var toggledOn = false;
    setInterval(function () {
        toggledOn = !toggledOn;
        if (toggledOn) {
            $('#text').animate({ color: "#f80" }, 1200);
        } else {
            $('#text').animate({ color: "#fff" }, 1200);
        }
    }, 1300);

    $('#music').jPlayer({
        supplied: 'mp3',
        swfPath: '../../jslib/',
        preload: 'auto',
        volume: 0.5,
        ready: function (event) {
            $(this).jPlayer("setMedia", { mp3: "music.mp3" });
            $(this).jPlayer("play");
        },
    });

    $(document).snowfall({
        round: true,
        minSize: 5,
        maxSize: 8,
        collection: '#box',
    });

});