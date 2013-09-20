var scheduleHub = $.connection.scheduleHub;
var selectedStart;
var selectedEnd;
var selectedEvent;
var editableValue;
var first = true;
var timelineVisible = false;
var timelineInterval;
var defaultEventColor = "#404040"
var selectedEventColor = "#955"
var checkDialogs = true;
var scheduleIntialized = false;
var currentZoomLevel = 2;

$(document).ready(function () {

    checkDialogs = false;

    if (!window.console) console = {};
    console.log = console.log || function () { };
    console.warn = console.warn || function () { };
    console.error = console.error || function () { };
    console.info = console.info || function () { };

    if (document.domain == "localhost") document.title = "Test Site";

    $.connection.hub.start()
    .done(function () {
        scheduleHub.server.initializeConnection().done(function (result) {
            checkServerResponse();
            $('#scheduleWindow').show();
            $('#jqCalendar').fullCalendar('today');
            $('#jqCalendar').fullCalendar('changeView', 'agendaWeek');
            $('#jqCalendar').fullCalendar('option', 'height', $(window).innerHeight() - 50);
            setTimeline();
        });
    })
    .fail(function () {
        window.location.href = "disconnected.aspx";
    });

    $.connection.hub.error(function () {
        window.location.href = "disconnected.aspx";
    });

    $("#confirmDeleteWinow").dialog({
        autoOpen: false,
        resizable: false,
        height: 180,
        width: 300,
        modal: true,
        buttons: {
            "Confirm Delete": function () {
                scheduleHub.server.deleteEvent(selectedEvent.id).done();
                selectEvent(null);
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
    });

    $("#newEventWindow").dialog({
        autoOpen: false,
        height: 150,
        width: 300,
        modal: true,
        draggable: true,
        position: 'right',
        resizable: false
    });

    $("#addEventOK").button({
        icons: {
            primary: "ui-icon-circle-check"
        }
    });
    $("#addEventCancel").button({
        icons: {
            primary: "ui-icon-circle-close"
        }
    });


    $("#addEventDescription").keypress(function (event) {
        if (event.which == 13) {
            tryAddEdit();
            return false;
        }
    });

    $("#radioButtons").buttonset();
    $("input[name='radioButtons']").change(function () {
        var selectedRadio = $('input[name=radioButtons]:checked', '#radioButtons').attr('id');
        if (selectedRadio == "week60Button") {
            initalizeSchedule(60, 'agendaWeek');
        } else if (selectedRadio == "week30Button") {
            initalizeSchedule(30, 'agendaWeek');
        } else if (selectedRadio == "day60Button") {
            initalizeSchedule(60, 'agendaDay');
        } else if (selectedRadio == "day30Button") {
            initalizeSchedule(30, 'agendaDay');
            $(".timeline").css("left", "60px");
        } else if (selectedRadio == "day5Button") {
            initalizeSchedule(5, 'agendaDay');
            $(".timeline").css("left", "81px");
        }
    });

    $("#addEventOK").click(function () {
        tryAddEdit();
    });

    function tryAddEdit() {

        var description = $("#addEventDescription").val()
        if (description.length < 0) {
            return;
        }
        if (selectedEvent == null) {
            scheduleHub.server.addEvent(selectedStart, selectedEnd, description).done(function (result) {
                if (result) {
                    $("#newEventWindow").dialog("close");
                    $('#addEventDescription').val("");
                    $('#jqCalendar').fullCalendar('unselect');
                }
            });
        } else {
            scheduleHub.server.updateEventDescription(selectedEvent.id, description).done(function (result) {
                if (result) {
                    selectEvent(null);
                    $("#newEventWindow").dialog("close");
                    $('#addEventDescription').val("");
                    $('#jqCalendar').fullCalendar('unselect');
                }
            });
        }
    }

    $("#addEventCancel").click(function () {
        $("#newEventWindow").dialog("close");
        $('#addEventDescription').val("");
        $('#jqCalendar').fullCalendar('unselect');
    });

    $(document).click(function () {
        selectEvent(null);
    });

    $('html').keyup(function (e) {
        if (e.keyCode == 46) {
            if (editableValue != null) {
                if (!checkDialogs || $("#scheduleWindow").dialog("isOpen") === true) {
                    if ($("#newEventWindow").dialog("isOpen") === false) {
                        if (selectedEvent != null) {
                            var event = $('#jqCalendar').fullCalendar('clientEvents', selectedEvent.id);
                            if (isEditable(event[0])) {
                                $("#confirmDeleteWinow").dialog("open");
                                $("#confirmDeleteWinow").dialog().parent().position({ my: 'center', at: 'center', of: '#scheduleWindow' });
                            }
                        } else {
                            //TODO: double click zoom week view
                        }
                    }
                }
            }
        }
    })
    scheduleHub.client.refreshEvents = function (message) {
        if (!checkDialogs || $("#scheduleWindow").dialog("isOpen") === true) {
            $('#jqCalendar').fullCalendar('refetchEvents');
        }
    }

    scheduleHub.client.editedEventTime = function (eventID, newStart, newEnd) {
        var events = $('#jqCalendar').fullCalendar('clientEvents', eventID);
        if (events.length > 0) {
            $.each(events, function () {
                this.start = newStart;
                this.end = newEnd;
                $('#jqCalendar').fullCalendar('updateEvent', this);
            });
        }
    }

    scheduleHub.client.editedEventDescription = function (eventID, newDescription) {
        var events = $('#jqCalendar').fullCalendar('clientEvents', eventID);
        if (events.length > 0) {
            $.each(events, function () {
                this.title = newDescription
                $('#jqCalendar').fullCalendar('updateEvent', this);
            });
        }
    }

    scheduleHub.client.deletedEvent = function (eventID) {
        $('#jqCalendar').fullCalendar('removeEvents', eventID);
    }

    scheduleHub.client.setScheduleEditable = function (value) {
        setEditable(value);
    }

    $(window).bind('resize', function (event) {
        if ($(event.target).prop("tagName") == "DIV") { return; }
        $('#jqCalendar').fullCalendar('option', 'height', $(window).innerHeight() - 30);
    });

    initalizeSchedule(30);
});

function isEditable(event) {
    if (editableValue != null && editableValue == "[all]") {
        return true;
    } else if (editableValue != null) {
        return (event.creator == editableValue);
    } else {
        return false;
    }
}

function setEditable(value) {
    editableValue = value;
    var events = $('#jqCalendar').fullCalendar('clientEvents');
    if (events.length > 0) {
        $.each(events, function () {
            this.editable = isEditable(this);
            $('#jqCalendar').fullCalendar('updateEvent', this);
        });
    }
}

function setTimeline() {
    var parentDiv = jQuery(".fc-agenda-slots:visible").parent();
    var timeline = parentDiv.children(".timeline");
    if (timeline.length == 0) { //if timeline isn't there, add it
        timeline = jQuery("<hr>").addClass("timeline");
        parentDiv.prepend(timeline);
    }

    var curTime = new Date();
    //curTime.setHours(curTime.getHours() + 10)

    var curCalView = jQuery("#jqCalendar").fullCalendar('getView');
    if (curCalView.visStart < curTime && curCalView.visEnd > curTime) {
        timeline.show();
        timelineVisible = true;
    } else {
        timeline.hide();
        timelineVisible = false;
        return;
    }

    var curSeconds = ((curTime.getHours() - curCalView.opt("minTime")) * 60 * 60) + (curTime.getMinutes() * 60) + curTime.getSeconds();
    var percentOfDay = curSeconds / ((curCalView.opt("maxTime") - curCalView.opt("minTime")) * 3600); // 60 * 60 = 3600, # of seconds in a hour
    var topLoc = Math.floor(parentDiv.height() * percentOfDay);

    timeline.css("top", topLoc + "px");

    if (curCalView.name == "agendaWeek") { //week view, don't want the timeline to go the whole way across
        var dayCol = jQuery(".fc-today:visible");
        var left = dayCol.position().left + 1;
        var width = dayCol.width() - 2;
        timeline.css({
            left: left + "px",
            width: width + "px"
        });
    }

}

function selectEvent(event) {
    if (event != selectedEvent) {
        if (selectedEvent != null) {
            selectedEvent.color = defaultEventColor;
            $('#jqCalendar').fullCalendar('updateEvent', selectedEvent);
        }
        selectedEvent = event;
        if (event != null) {
            event.color = selectedEventColor;
            $('#jqCalendar').fullCalendar('updateEvent', event);
        }
    }
}

function initalizeSchedule(accuracy, newView) {
    if (scheduleIntialized) {
        $('#jqCalendar').fullCalendar('destroy');
    }
    $('#jqCalendar').fullCalendar({
        theme: true,
        defaultView: newView,
        allDaySlot: false,
        lazyFetching: false,
        editable: true,
        selectable: true,
        selectHelper: true,
        eventColor: defaultEventColor,
        unselectAuto: false,
        header: {
            left: 'prev,next,today',
            center: 'title',
            right: '',
        },
        slotMinutes: accuracy,
        events: function (start, end, callback) {
            scheduleHub.server.queryEvents(start, end).done(function (events) {
                $.each(events, function () {
                    this.end = this.ends;
                    this.editable = isEditable(this);
                });
                callback(events);
            });
        },
        select: function (startDate, endDate, allDay, jsEvent, view) {
            if (!allDay && editableValue != null) {
                selectedStart = startDate;
                selectedEnd = endDate;
                selectEvent(null);
                $("#newEventWindow").dialog("open");
                $("#newEventWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#scheduleWindow' });
            }
        },
        eventDrop: function (event, dayDelta, minuteDelta, allDay, revertFunc, jsEvent, ui, view) {
            scheduleHub.server.updateEventTime(event.id, event.start, event.end).done(function (result) {
                if (!result) revertFunc();
            });
        },
        eventResize: function (event, dayDelta, minuteDelta, revertFunc, jsEvent, ui, view) {
            scheduleHub.server.updateEventTime(event.id, event.start, event.end).done(function (result) {
                if (!result) revertFunc();
            });
        },
        eventRender: function (event, element) {
            element.dblclick(function () {
                if (isEditable(event)) {
                    selectEvent(event);
                    $("#addEventDescription").val(event.title.substr(event.title.indexOf('- ') + 2, event.title.length - event.title.indexOf('- ') - 2));
                    $("#newEventWindow").dialog("open");
                    $("#newEventWindow").dialog().parent().position({ my: 'center', at: 'center', of: '#scheduleWindow' });
                }
            });
        },
        eventClick: function (event, jsEvent, view) {
            if (editableValue != null) selectEvent(event);
            jsEvent.stopPropagation();
        },
        viewDisplay: function (view) {
            if (first) {
                first = false;
            } else {
                window.clearInterval(timelineInterval);
            }
            timelineInterval = window.setInterval(setTimeline, 300000);
            try {
                setTimeline();
                var viewName = view.name;
                if (timelineVisible) {
                    if (viewName === "agendaDay") {
                        $(".calendarScrollPane").eq(0).scrollTo($(".timeline").eq(0), 1000, {
                            offset: -220,
                            easing: 'easeOutQuad'
                        });
                    } else if (viewName === "agendaWeek") {
                        $(".calendarScrollPane").eq(1).scrollTo($(".timeline").eq(1), 1000, {
                            offset: -220,
                            easing: 'easeOutQuad'
                        });
                    }
                }
            } catch (err) { }
        },
    });
    $('#jqCalendar').fullCalendar('option', 'height', $(window).innerHeight() - 30);
    setTimeline();
    scheduleIntialized = true;
}

var test1;
var test2;

function checkServerResponse() {
    test1 = false;
    test2 = false;
    setTimeout(function () {
        checkIfBadConnection();
    }, 15000);
    setTimeout(function () {
        scheduleHub.server.serverPing().done(function () {
            test1 = true
        });
    }, 5000);
    setTimeout(function () {
        scheduleHub.server.serverPing().done(function () {
            test2 = true
        });
    }, 10000);
}

function checkIfBadConnection() {
    if (!test1 && !test2) {
        window.location.href = "disconnected.aspx";
    } else {
        setTimeout(function () {
            checkServerResponse();
        }, 60000);
    }
}