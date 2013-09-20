$(document).ready(function () {

    $("#loginWindow").dialog({
        autoOpen: false,
        width: 250,
        height: 'auto',
        modal: false,
        resizable: false,
        draggable: true,
        position: 'right'
    });

    $("#openLoginWindow").button({
        icons: {
            primary: "ui-icon-key"
        }
    });

    $("#logoutOfChat").button({
        icons: {
            primary: "ui-icon-key"
        }
    });

    $("#login_button").button();

    $("#openLoginWindow").click(function () {
        $("#invalidLoginText").hide();
        $("#loginWindow").dialog("open");
        $("#loginWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#chatControlContainer' });
    });

    $("#logoutOfChat").click(function () {
        if (initialized) {
            $("#logoutOfChat").hide();
            $("#openLoginWindow").show();
            serverHub.server.logout();
        }
    });

    $("#login_button").click(function () {
        logIn($("#login_user").val(), $("#login_password").val());
    });

    $("#login_password").keypress(function (event) {
        if (event.which == 13) {
            logIn($("#login_user").val(), $("#login_password").val());
            return false;
        }
    });

});


function logIn(username, password) {
    $("#invalidLoginText").hide();
    $("#loginWindow").dialog("option", "disabled", true);
    serverHub.server.tryLogin(username, password)
        .done(function (success) {
            if (success) {
                $("#openLoginWindow").hide();
                $("#logoutOfChat").show();
                $("#loginWindow").dialog("close");
                $('#login_user').val("");
                $('#login_password').val("");
            } else {
                $("#invalidLoginText").html("Incorrect login information");
                $("#invalidLoginText").show();
            }
            $("#loginWindow").dialog("option", "disabled", false);
            $("#chatTextEntry").focus();
        })
        .fail(function (error) {
            $("#invalidLoginText").html("Server error.");
            $("#invalidLoginText").show();
            $("#loginWindow").dialog("option", "disabled", false);
        });
}

serverHub.client.updateModStatus = function (value) {
    showModMenu = value;
}

serverHub.client.doLogin = function () {
    scheduleHub.server.initializeConnection().done(function () {
        $("#openLoginWindow").hide();
        $("#logoutOfChat").show();
        $("#openOptionsWindow").show();
        loadUserOptions();
    });

}

serverHub.client.doLogout = function () {
    showModMenu = false;
    $("#openLoginWindow").show();
    $("#logoutOfChat").hide();
    $("#openOptionsWindow").hide();
    loadUserOptions();
}

serverHub.client.abandonConnection = function (message) {
    $.connection.hub.stopp;
    postLocalSystemMessage(message);
   window.location.href = "disconnected.aspx";
}