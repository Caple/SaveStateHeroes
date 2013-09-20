var notifyOnLive = true;
var showTimestamps = false;
var loadingValues = false;
var tangoStyle = false;
var disableAllSounds = false;
var disableAllSounds = false;
var playMessageSound = false;

function toggleOptionsDialog() {
    $("#optionsWindow").fadeToggle();
}

$(document).ready(function () {
    $('#optionTabs').tabs();
    $("#optionsWindow").dialog({
        autoOpen: false,
        height: 'auto',
        width: 400,
        modal: false,
        resizable: false,
        draggable: true,
        position: 'center'
    });
    $("#openOptionsWindow").button({
        icons: {
            primary: "ui-icon-gear"
        }
    });
    $("#nameColorPicker").spectrum({
        color: "#fff",
        showInput: true,
        chooseText: "Apply",
        preferredFormat: "hex",
        change: function (color) {
            if (initialized && !loadingValues) serverHub.server.setUserOption('chatColorName', color.toHexString());
        }
    });
    $("#textColorPicker").spectrum({
        color: "#fff",
        showInput: true,
        chooseText: "Apply",
        preferredFormat: "hex",
        change: function (color) {
            if (initialized && !loadingValues) serverHub.server.setUserOption('chatColorText', color.toHexString());
        }
    });

    $("#openOptionsWindow").click(function () {
        $("#optionsWindow").dialog("open");
        $("#optionsWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
    });

    $("#skinSelect").change(function () {
        setSkin(this.value);
    });

    $("#SizeSlider").slider({
        min: 480,
        max: 2200,
        value: 800,
        animate: false,
        slide: function (event, ui) {
            customWidth = $("#SizeSlider").slider("value");
            $("#SizeSliderValue").html(customWidth + " x " + Math.round(customWidth * aspectRatio));
            maximumVideoWidth = customWidth;
            var shouldAutoScroll = autoScrollNeeded();
            resizeContent();
            if (shouldAutoScroll) autoScroll();
        },
        change: function (event, ui) {
            customWidth = $("#SizeSlider").slider("value");
            $("#SizeSliderValue").html(customWidth + " x " + Math.round(customWidth * aspectRatio));
            maximumVideoWidth = customWidth;
            var shouldAutoScroll = autoScrollNeeded();
            resizeContent();
            if (shouldAutoScroll) autoScroll();
        },
        stop: function (event, ui) {
            if (initialized && !loadingValues) serverHub.server.setUserOption('sizeCustomWidth', $("#SizeSlider").slider("value"));
        }
    });
    $("#SizeSlider").slider("option", "disabled", true);

});

function loadUserOptions() {

    loadingValues = true;

    serverHub.server.getCommonUserOptions().done(function (value) {
        var selected;

        selected = $('#skinSelect');
        selected.val(value.skinNameMain);
        selected.trigger("change");

        selected = $('#tangoStyleCheckbox');
        selected.prop('checked', value.tangoStyle);
        selected.trigger("change");

        selected = $('#disableSoundsCheckbox');
        selected.prop('checked', value.disableAllSounds);
        selected.trigger("change");

        selected = $('#playSoundOnMessageCheckbox');
        selected.prop('checked', value.playMessageSound);
        selected.trigger("change");
        

        selected = $('input:radio[name=VideoResizePrefrence]').filter('[value=' + value.sizeMode + ']');
        if ((selected).attr('checked') != "checked") {
            selected.attr('checked', true);
            selected.trigger("change");
        }

        if (value.sizeMode == "custom") {
            $('#SizeSlider').slider("value", value.sizeCustomWidth);
        }

        $("#nameColorPicker").spectrum("set", value.chatColorName);
        $("#textColorPicker").spectrum("set", value.chatColorText);

        selected = $('#showTimestampsCheckbox');
        selected.prop('checked', value.showTimestamps);
        selected.trigger("change");

        loadingValues = false;

    });

}

function setTimeStampVar(checked) {
    if (checked == "true" || checked == true) {
        showTimestamps = true;
    } else {
        showTimestamps = false;
    }
    if (initialized && !loadingValues) serverHub.server.setUserOption('showTimestamps', checked);
    refreshTimestamps();
}

function setSizePrefrence(checked, value) {
    if (!checked) return;
    if (value == "custom") {
        $("#SizeSlider").slider("option", "disabled", false);
        $("#SizeSliderValue").show();
    } else {
        $("#SizeSlider").slider("option", "disabled", true);
        $("#SizeSliderValue").hide();
    }
    if (initialized && !loadingValues) serverHub.server.setUserOption('sizeMode', value);
    if (value == false) value = "default";
    setResizingPrefrence(value);
}

function setResizingPrefrence(value) {
    if (value == "default") {
        maximumVideoWidth = defaultMaximumVideoWidth;
    } else if (value == "expand") {
        maximumVideoWidth = 5000000
    } else if (value == "custom") {
        maximumVideoWidth = customWidth;
    }
    var shouldAutoScroll = autoScrollNeeded();
    resizeContent();
    if (shouldAutoScroll) autoScroll();
}

function setTangoStyle(checked) {
    if (checked == "true" || checked == true) {
        tangoStyle = true;
    } else {
        tangoStyle = false;
    }
    if (initialized && !loadingValues) serverHub.server.setUserOption('tangoStyle', checked);
    var shouldAutoScroll = autoScrollNeeded();
    if (tangoStyle) {
        $('.chatMessage').add('.systemMessage').addClass('tangoStyle');
    } else {
        $('.chatMessage').add('.systemMessage').removeClass('tangoStyle');
    }
    if (shouldAutoScroll) autoScroll();
}

function disableSounds(checked) {
    if (checked == "true" || checked == true) {
        disableAllSounds = true;
    } else {
        disableAllSounds = false;
    }
    if (initialized && !loadingValues) serverHub.server.setUserOption('disableAllSounds', checked);
}

function playSoundOnMessage(checked) {
    if (checked == "true" || checked == true) {
        playMessageSound = true;
    } else {
        playMessageSound = false;
    }
    if (initialized && !loadingValues) serverHub.server.setUserOption('playMessageSound', checked);
}