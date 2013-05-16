Sys.Application.add_load(function () {

    /** Schedule Panel Show/Hide Scripts **/

    $('.schedule-type').click(function () {
        var reoccurrenceState = $('input[class=schedule-type]:checked').data('schedule-type');

        if (reoccurrenceState == 'schedule-onetime') {
            $('#schedule-reoccurrence-panel').slideUp();
        } else {
            $('#schedule-reoccurrence-panel').slideDown();
        }

    });

    $('.reoccurrence-pattern-radio').click(function () {

        var reoccurrencePattern = '#' + $('input[name=reoccurrence-pattern-radio]:checked').data('reoccurrence-pattern');

        if ($(reoccurrencePattern).css('display') == 'none') {

            $('.reoccurrence-pattern-type').slideUp();
            $(reoccurrencePattern).slideDown();
        }
    });

    $('.modal-control-close').on('click', function () {
        $('#myModal').modal('toggle');
    });

    /** Specific Dates Scripts**/

    // show datepicker, ok, cancel so that new date can be added to the list
    $('#add-specific-date').click(function () {
        $('#add-specific-date').hide();
        $('#add-specific-date-group').show();
    });

    // add new date to list when ok is clicked
    $('#add-specific-date-ok').click(function () {

        // get date list from hidden field
        var dateListValues = $('#hfSpecificDateListValues').val().split(",");
        if (!dateListValues[0]) {
            // if dateList is blank, initialize as a new empty array
            dateListValues = [];
        }

        // validate

        // set colors back to default just in case previously marked invalid
        $('#dpSpecificDate').css('color', '');
        var checkDate = Date.parse($('#dpSpecificDate').val());
        if (!checkDate) {
            // blank, don't color, just return
            return
        }

        if (checkDate < 0) {
            // invalid date entered, color red and return
            $('#dpSpecificDate').css('color', 'red');
            return
        }

        var newDate = $('#dpSpecificDate').val();

        // delete newDate from list in case it is already there
        var index = dateListValues.indexOf(newDate);
        if (index >= 0) {
            dateListValues.splice(index, 1);
        }

        // add new date to list
        dateListValues.push(newDate);

        // save list back to hidden field
        $('#hfSpecificDateListValues').val(dateListValues);

        // rebuild the UL
        var dateList = $("#lstSpecificDates");
        dateList.children().remove();
        $.each(dateListValues, function (index, value) {
            // add to ul
            var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
            dateList.append(newLi);
        });

        $('#add-specific-date-group').hide();
        $('#add-specific-date').show();
    });

    // cancel out of adding a new date
    $('#add-specific-date-cancel').click(function () {
        $('#add-specific-date-group').hide();
        $('#add-specific-date').show();
    });

    // fadeIn/fadeOut the X buttons to delete dates
    $('#lstSpecificDates').hover(
        function () {
            $(this).find('li a').stop(true, true).show();
        },
        function () {
            $(this).find('li a').stop(true, true).fadeOut(500);
        }
    );

    // delete specific date from list
    $('#lstSpecificDates').on('click', 'li a', function (event) {
        var selectedDate = $(this).siblings().text();

        // get date list from hidden field
        var dateList = $('#hfSpecificDateListValues').val().split(",");

        // delete selectedDate
        var index = dateList.indexOf(selectedDate);
        if (index >= 0) {
            dateList.splice(index, 1);
        }

        // save list back to hidden field
        $('#hfSpecificDateListValues').val(dateList);

        // remove date from ul list
        var liItem = $(this).parent();
        liItem.remove();
    });

    /** Exclusion DateRanges Scripts **/

    // show dateRangepicker, ok, cancel so that new dateRange can be added to the list
    $('#add-exclusion-daterange').click(function () {
        $('#add-exclusion-daterange').hide();
        $('#add-exclusion-daterange-group').show();
    });

    // add new date to list when ok is clicked
    $('#add-exclusion-daterange-ok').click(function () {

        // get daterange list from hidden field
        var dateRangeListValues = $('#hfExclusionDateRangeListValues').val().split(",");
        if (!dateRangeListValues[0]) {
            // if blank, initialize as a new empty array
            dateRangeListValues = [];
        }

        // set colors back to default just in case previously marked invalid
        $('#dpExclusionDateStart').css('color', '');
        $('#dpExclusionDateEnd').css('color', '');

        var startDate = Date.parse($('#dpExclusionDateStart').val());
        var endDate = Date.parse($('#dpExclusionDateEnd').val());

        if (!startDate || !endDate) {
            // if either is blank, don't color, just exit
            return;
        }

        if ((startDate < 0) || (endDate < 0)) {

            if (startDate < 0) {
                $('#dpExclusionDateStart').css('color', 'red');
            }
            if (endDate < 0) {
                $('#dpExclusionDateEnd').css('color', 'red');
            }

            // if either is invalid, exit
            return;
        }

        var newDateRange = null;
        if (startDate <= endDate) {
            newDateRange = $('#dpExclusionDateStart').val() + ' - ' + $('#dpExclusionDateEnd').val();
        }
        else {
            // invalid because startdate is after enddate
            $('#dpExclusionDateStart').css('color', 'red');
            $('#dpExclusionDateEnd').css('color', 'red');
            return;
        }

        // delete newDateRange from list in case it is already there
        var index = dateRangeListValues.indexOf(newDateRange);
        if (index >= 0) {
            dateRangeListValues.splice(index, 1);
        }

        // add new dateRange to list
        dateRangeListValues.push(newDateRange);

        // save list back to hidden field
        $('#hfExclusionDateRangeListValues').val(dateRangeListValues);

        // rebuild the UL
        var dateRangeList = $('#lstExclusionDateRanges');
        dateRangeList.children().remove();
        $.each(dateRangeListValues, function (index, value) {
            // add to ul
            var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
            dateRangeList.append(newLi);
        });

        $('#add-exclusion-daterange-group').hide();
        $('#add-exclusion-daterange').show();
    });

    // cancel out of adding a new dateRange
    $('#add-exclusion-daterange-cancel').click(function () {
        $('#add-exclusion-daterange-group').hide();
        $('#add-exclusion-daterange').show();
    });

    // fadeIn/fadeOut the X buttons to delete dateRanges
    $('#lstExclusionDateRanges').hover(
        function () {
            $(this).find('li a').stop(true, true).show();
        },
        function () {
            $(this).find('li a').stop(true, true).fadeOut(500);
        }
    );

    // delete dateRange from list
    $('#lstExclusionDateRanges').on('click', 'li a', function (event) {
        var selectedDateRange = $(this).siblings("span").text();

        // get dateRange list from hidden field
        var dateRangeList = $('#hfExclusionDateRangeListValues').val().split(",");

        // delete selectedDateRange
        var index = dateRangeList.indexOf(selectedDateRange);
        if (index >= 0) {
            dateRangeList.splice(index, 1);
        }

        // save list back to hidden field
        $('#hfExclusionDateRangeListValues').val(dateRangeList);

        // remove dateRange from ul list
        var liItem = $(this).parent();
        liItem.remove();
    });

});