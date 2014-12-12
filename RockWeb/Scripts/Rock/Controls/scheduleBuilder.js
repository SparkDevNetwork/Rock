(function ($, Sys) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.scheduleBuilder = (function () {
        var exports,
            ScheduleBuilder = function (options) {
                this.id = options.id;
            };

        ScheduleBuilder.prototype.initializeEventHandlers = function () {

            var id = this.id,
                $modal = $('#' + id).closest('.rock-modal');
            

            $modal.find('.schedule-type').click(function () {
                var recurrenceState = $('input[class=schedule-type]:checked').data('schedule-type');

                if (recurrenceState === 'schedule-onetime') {
                    $('#schedule-recurrence-panel_' + id).slideUp();
                } else {
                    $('#schedule-recurrence-panel_' + id).slideDown();
                }
            });

            $modal.find('.recurrence-pattern-radio').click(function () {

                var recurrencePattern = '.' + $('input[class=recurrence-pattern-radio]:checked').data('recurrence-pattern');

                if ($modal.find(recurrencePattern)) {

                    $modal.find('.recurrence-pattern-type').not(recurrencePattern).hide();

                    $(recurrencePattern).fadeIn();
                }
            });

            /** Specific Dates Scripts**/

            // show datepicker, ok, cancel so that new date can be added to the list
            $modal.find('.add-specific-date').click(function () {
                $(this).hide();
                $('#add-specific-date-group_' + id).show(100);
            });

            // add new date to list when ok is clicked
            $modal.find('.add-specific-date-ok').click(function () {

                // get date list from hidden field
                var dateListValues = $modal.find('.recurrence-pattern-type > input:hidden').val().split(',');

                if (!dateListValues[0]) {
                    // if dateList is blank, initialize as a new empty array
                    dateListValues = [];
                }

                // validate

                // set colors back to default just in case previously marked invalid
                var $datepicker = $modal.find('.specific-date input');
                $datepicker.css('color', '');
                var checkDate = Date.parse($datepicker.val());
                if (!checkDate) {
                    // blank, don't color, just return
                    return;
                }

                if (checkDate < 0) {
                    // invalid date entered, color red and return
                    $datepicker.css('color', 'red');
                    return;
                }

                var newDate = $datepicker.val();

                // delete newDate from list in case it is already there
                var index = dateListValues.indexOf(newDate);
                if (index >= 0) {
                    dateListValues.splice(index, 1);
                }

                // add new date to list
                dateListValues.push(newDate);

                // save list back to hidden field
                $modal.find('input[id*="hfSpecificDateListValues"]').val(dateListValues);

                // rebuild the UL
                var dateList = $modal.find(".lstSpecificDates");
                dateList.children().remove();
                $.each(dateListValues, function (i, value) {
                    // add to ul
                    var newLi = "<li><span>" + value + "</span> <a href='#' style='display: none'><i class='fa fa-times'></i></a></li>";
                    dateList.append(newLi);
                });

                $modal.find('#add-specific-date-group_' + id).hide();
                $modal.find('.add-specific-date').show(100);
            });

            // cancel out of adding a new date
            $modal.find('.add-specific-date-cancel').click(function () {
                $modal.find('#add-specific-date-group_' + id).hide();
                $modal.find('.add-specific-date').show(100);
            });

            // fadeIn/fadeOut the X buttons to delete dates
            $modal.find('.lstSpecificDates').hover(
                function () {
                    $(this).find('li a').stop(true, true).show();
                },
                function () {
                    $(this).find('li a').stop(true, true).fadeOut(500);
                }
            );

            // delete specific date from list
            $modal.find('.lstSpecificDates').on('click', 'li a', function () {
                var selectedDate = $(this).siblings().text();

                // get date list from hidden field
                var $hiddenField = $modal.find('input[id*="hfSpecificDateListValues"]');
                var dateList = $hiddenField.val().split(",");

                // delete selectedDate
                var index = dateList.indexOf(selectedDate);
                if (index >= 0) {
                    dateList.splice(index, 1);
                }

                // save list back to hidden field
                $hiddenField.val(dateList);

                // remove date from ul list
                var liItem = $(this).parent();
                liItem.remove();
            });

            /** Exclusion DateRanges Scripts **/

            // show dateRangepicker, ok, cancel so that new dateRange can be added to the list
            $modal.find('.add-exclusion-daterange').click(function () {
                $(this).hide();
                $modal.find('#add-exclusion-daterange-group_' + id).show(100);
            });

            // add new date to list when ok is clicked
            $modal.find('.add-exclusion-daterange-ok').click(function () {

                // get daterange list from hidden field
                var dateRangeListValues = $modal.find('input[id*="hfExclusionDateRangeListValues"]').val().split(",");
                if (!dateRangeListValues[0]) {
                    // if blank, initialize as a new empty array
                    dateRangeListValues = [];
                }

                var $exclusionDateRange = $modal.find('input[id*="dpExclusionDateRange_"]');
                var newDateRange = $exclusionDateRange.first().val() + ' - ' + $exclusionDateRange.last().val();

                // delete newDateRange from list in case it is already there
                var index = dateRangeListValues.indexOf(newDateRange);
                if (index >= 0) {
                    dateRangeListValues.splice(index, 1);
                }

                // add new dateRange to list
                dateRangeListValues.push(newDateRange);

                // save list back to hidden field
                $modal.find('input[id*="hfExclusionDateRangeListValues"]').val(dateRangeListValues);

                // rebuild the UL
                var dateRangeList = $modal.find('.lstExclusionDateRanges');
                dateRangeList.children().remove();
                $.each(dateRangeListValues, function (i, value) {
                    // add to ul
                    var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='fa fa-times'></i></a></li>";
                    dateRangeList.append(newLi);
                });

                $modal.find('#add-exclusion-daterange-group_' + id).hide();
                $modal.find('.add-exclusion-daterange').show(100);
            });

            // cancel out of adding a new dateRange
            $modal.find('.add-exclusion-daterange-cancel').click(function () {
                $modal.find('#add-exclusion-daterange-group' + id).hide();
                $modal.find('.add-exclusion-daterange').show(100);
            });

            // fadeIn/fadeOut the X buttons to delete dateRanges
            $modal.find('.lstExclusionDateRanges').hover(
                function () {
                    $(this).find('li a').stop(true, true).show();
                },
                function () {
                    $(this).find('li a').stop(true, true).fadeOut(500);
                }
            );

            // delete dateRange from list
            $modal.find('.lstExclusionDateRanges').on('click', 'li a', function () {
                var selectedDateRange = $(this).siblings("span").text();

                // get dateRange list from hidden field
                var $hiddenField = $modal.find('input[id*="hfExclusionDateRangeListValues"]');
                var dateRangeList = $hiddenField.val().split(",");

                // delete selectedDateRange
                var index = dateRangeList.indexOf(selectedDateRange);
                if (index >= 0) {
                    dateRangeList.splice(index, 1);
                }

                // save list back to hidden field
                $hiddenField.val(dateRangeList);

                // remove dateRange from ul list
                var liItem = $(this).parent();
                liItem.remove();
            });

            // validate on Save.  Make sure they have at least a StartDate and Time set
            $modal.find('.js-modaldialog-save-link').on('click', function (event) {
                var $datetimepicker = $modal.find('[id*="dpStartDateTime"]').find('input'),
                    startDateValue = Date.parse($datetimepicker.first().val()) || -1,
                    startTimeValue = $datetimepicker.last().val();

                if (startDateValue < 0 || !startTimeValue) {
                    $datetimepicker.parents(".form-group").first().toggleClass("has-error", 1);
                    event.preventDefault();
                    return;
                }
                else {
                    $datetimepicker.parents(".form-group").first().toggleClass("has-error", 0);
                }
            });
        };

        exports = {
            cache: {},
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var sb = new ScheduleBuilder(options);
                exports.cache[options.id] = sb;

                Sys.Application.add_load(function () {
                    sb.initializeEventHandlers();
                });
            }
        };

        return exports;
    }());
}(jQuery, Sys));