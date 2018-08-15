"use strict";
(function ($) {

    var campuses = [
        [67713, "Akron Campus", "AKR"],
        [53103, "Canton Campus", "CAN"],
        [62004, "Coshocton Campus", "COS"],
        [51774, "Millersburg Campus", "MIL"],
        [67714, "Wooster Campus", "WST"],
        [51773, "Dover Campus", "DOV"],
        ["ALL", "ALL", "ALL"]
    ];

    function getCampusById(cmp) {
        return campuses.filter(function (c) { return c[0] == cmp; })[0] || campuses[campuses.length - 1];
    }
    function getCampusByShortCode(cmp) {
        return campuses.filter(function (c) { return c[2] == cmp; })[0] || campuses[campuses.length - 1];
    }
    function getCampusByName(cmp) {
        return campuses.filter(function (c) { return c[1] == cmp; })[0] || campuses[campuses.length - 1];
    }

    var categories = [
        [13399, "event-important", "cm"],
        [13405, "event-info", "sm"],
        [21205, "event-warning", "ya"],
        [11111, "event-success", "g"],
        ["00000", "event-inverse", "ae"],
        ["-1", "", "all"]
    ];
    
    function getCategoryById(cat) {
        return categories.filter(function (c) { return c[0] == cat; })[0] || categories[categories.length-1];
    }
    function getCategoryByShortCode(cat) {
        return categories.filter(function (c) { return c[2] == cat; })[0] || categories[categories.length-1];
    }
    function getCategoryByName(cat) {
        return categories.filter(function (c) { return c[1] == cat; })[0] || categories[categories.length-1];
    }

    var jsonData = [];
    var searchData, selectedCampus, selectedCategory;

    var dropdownlist = $("[id*=ddlCampusDropdown]");
    
    setSelectedCampus($("input[name *= hdnCampus]").val());
    setSelectedCategory($("input[name *= hdnCategory]").val());

    function filterCampus(d) {
        if (selectedCategory[1] == "" && selectedCampus[2] == "ALL") {
            return d
        } else if (selectedCategory[1] == "" && selectedCampus[2] != "ALL") {
            return d.departmentname.split('|').map(function (s) { return s.trim(); }).indexOf(selectedCampus[1]) > -1;
        } else if (selectedCategory[1] != "" && selectedCampus[2] == "ALL") {
            return d.class == selectedCategory[1];
        } else {
            return d.departmentname.split('|').map(function (s) { return s.trim(); }).indexOf(selectedCampus[1]) > -1 && d.class == selectedCategory[1];
        }
    }

    $.getJSON('/assets/calendar.json', { _: new Date().getTime() }).done(bindCalendar);


    function bindCalendar(json) {
        jsonData = json;

        searchData = $.grep(json, filterCampus);

        var options = {
            events_source: searchData, //'/assets/calendar.json',
            view: 'month',
            tmpl_path: 'Plugins/org_newpointe/ServiceUEvents/Scripts/tmpls/',
            tmpl_cache: false,
            format12: true,
            day: 'now',
            display_week_number: false,
            weekbox: false,
            onAfterEventsLoad: function (events) {
                if (!events) {
                    return;
                }

                var list = $('#eventlist');
                list.html('');

                $.each(events, function (key, val) {
                    $(document.createElement('li'))
                        .html('<a href="' + val.url + '">' + val.title + '</a>')
                        .appendTo(list);
                });
            },
            onAfterViewLoad: function (view) {
                $('.page-header h3').text(this.getTitle());
                $('.btn-group button').removeClass('active');
                $('button[data-calendar-view="' + view + '"]').addClass('active');
                //ads on click event to calendar date. 
                $(".cal-cell").click(function () {
                    getEventsBy(parseCalDate($(this).find("div.cal-month-day span").data("cal-date")));
                });
                //void event links 
                $("div.events-list a").attr("href", "javascript:void(0);");
                var event = $("input[name *= hdnEventId ]");
                if (event.val() != '') {
                    getEventsBy('', event.val());
                    event.val('');
                } else {
                    getEventsBy(new Date());
                }
                $.each($(".cal-row-head div.cal-cell1"), function () {
                    $(this).html($(this).html().slice(0, 3));
                });
            },
            classes: {
                months: {
                    general: 'label'
                }
            }
        };


        var calendar = $('#calendar').calendar(options);


        $('.btn-group button[data-calendar-nav]').each(function () {
            var $this = $(this);
            $this.click(function (e) {
                e.preventDefault();
                calendar.navigate($this.data('calendar-nav'));
            });
        });

        $('.btn-group button[data-calendar-view]').each(function () {
            var $this = $(this);
            $this.click(function () {
                calendar.view($this.data('calendar-view'));
            });

        });

        $('#first_day').change(function () {
            var value = $(this).val();
            value = value.length ? parseInt(value) : null;
            calendar.setOptions({ first_day: value });
            calendar.view();
        });
    }

    function parseCalDate(date) {
        var a = date.split('-');
        return new Date(a[0], a[1]-1, a[2]);
    }

    function getEventsBy(date, id) {
        var search = "";
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        if (date != '') {
            date = new Date(date);
            $("#event-date").html(monthNames[date.getMonth()] + ' ' + date.getDate());
            search = "/Plugins/org_newpointe/Calendar.ashx?date=" + (date.getUTCMonth() + 1) + "/" + date.getUTCDate() + "/" + date.getUTCFullYear();
        } else if (id != undefined) {
            search = "/Plugins/org_newpointe/Calendar.ashx?id=" + id;
        } else {
            return;
        }
        var eventList = $("#events-list");

        eventList.empty();
        $(".event-description").hide();
        $("#event-description").empty();

        $.getJSON(search, function (data) {
            //filter out based on campus if this isn't a drop down list saerch
            if (id == undefined) {
                searchData = $.grep(data, filterCampus);
            } else {
                searchData = data;
                if (data.length > 0) {
                    date = new Date(data[0].start);
                    $("#event-date").html(monthNames[date.getMonth()] + ' ' + date.getDate());
                }
            }
            if ($(window).width() <= 992) {
                ///scroll to Evetns
                $('html, body').animate({
                    scrollTop: $("#event-date").offset().top
                }, 800);
            }
            var start, end;
            $.each(searchData, function (key, val) {
                start = new Date(searchData[key].start);
                end = new Date(searchData[key].end);
                eventList.append('<div class="col-sm-12 events nopadding"  data-id="' + searchData[key].id + '" ><i class="fa fa-plus-square"></i><i class="fa fa-info-circle" style="display:none;"></i><strong>' + searchData[key].startTime + '-' + searchData[key].endtime + '</strong> ' + searchData[key].title + '</div><div class="col-sm-12 event-description  parent-' + searchData[key].id + '" style="display:none;">' + searchData[key].description + '</div>');


            });
            if (searchData.length == 0) {
                $("#event-description").html("<div class='text-center'><strong>NO EVENTS TO DISPLAY</strong></div>");
            }
            eventList.find("div.events").click(function () {
                $(".event-description").hide();
                $("i.fa-info-circle").hide();
                $("i.fa-plus-square").show();
                $(this).find("i.fa-info-circle").show();
                $(this).find("i.fa-plus-square").hide();
                $(".parent-" + $(this).data("id")).slideDown();
            });
            if (searchData.length == 1) {
                $(".parent-" + searchData[0].id).slideDown();
            }

        });
    }

    function setSelectedCategory(catCode) {
        selectedCategory = getCategoryByShortCode(catCode || "all");

        $("[id*=ddlCategoryDropdown]").val(selectedCategory[2]);

        $("#CategoryFilterButtons a").removeClass("active");
        $(".category-" + selectedCategory[2] + "-hover").addClass("active");
    }

    function setSelectedCampus(campusCode) {
        selectedCampus = getCampusByShortCode(campusCode || "ALL");

        $("#CampusFilterDropdown select").val(selectedCampus[2]);
        
        $("#CampusFilterButtons .btn").removeClass("active");
        $("#CampusFilterButtons .btn.campus-" + selectedCampus[2].toLowerCase() + "-hover").addClass("active");
    }

    $(document).ready(function () {

        $("#CategoryFilterButtons .btn").click(function (e) {
            e.preventDefault();
            setSelectedCategory($(this).attr("data-categorycode"));
            bindCalendar(jsonData);
        })

        $("#CampusFilterButtons .btn").click(function (e) {
            e.preventDefault();
            setSelectedCampus($(this).attr("data-campuscode"));
            bindCalendar(jsonData);
        });

        $("[id*='ddlCampusDropdown']").change(function () {
            setSelectedCampus($(this).val());
            bindCalendar(jsonData);
        });

        $("[id*='ddlCategoryDropdown']").change(function () {
            setSelectedCategory($(this).val());
            bindCalendar(jsonData);
        });
        $("#collapse-Button").click(function () {
            console.log('dafsd');
            var cfilter = $("#collapseFilter");
            if (!cfilter.is(':hidden')) {
                cfilter.slideUp();
            } else {
                cfilter.slideDown();

            }
        });


        $("#calendar-search").autocomplete({
            serviceUrl: "../../Plugins/org_newpointe/Calendar.ashx",
            onSelect: function (suggestion) {
                //Logs the EventID from the selected event.
                getEventsBy('', suggestion.data);
                $("#calendar-search").val('');
            }
        }).on('focus', function () {
            $(this).val('');
        });

    });

}(jQuery));
