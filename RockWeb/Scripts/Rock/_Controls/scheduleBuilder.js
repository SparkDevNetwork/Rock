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
                $modal = $('#schedule-builder-modal_' + id),
                $button = $('#schedule-builder-button_' + id);

            $modal.find('.scroll-container').tinyscrollbar({ size: 150 });

            /** Schedule Panel Show/Hide Scripts **/

            $button.click(function () {
                if ($modal.is(':hidden')) {
                    $modal.modal('show');
                    $modal.find('.scroll-container').tinyscrollbar_update('relative');
                } 
            });

            $modal.on('shown', function () {
                $modal.find('.scroll-container').tinyscrollbar_update('relative');
            });

            $modal.find('.schedule-type').click(function () {
                var recurrenceState = $('input[class=schedule-type]:checked').data('schedule-type');

                if (recurrenceState === 'schedule-onetime') {
                    $('#schedule-recurrence-panel_' + id).slideUp(function () {
                        $modal.find('.scroll-container').tinyscrollbar_update('relative');
                    });
                } else {
                    $('#schedule-recurrence-panel_' + id).slideDown(function () {
                        $modal.find('.scroll-container').tinyscrollbar_update('relative');
                    });
                }
            });

            $modal.find('.recurrence-pattern-radio').click(function () {

                var recurrencePattern = '.' + $('input[class=recurrence-pattern-radio]:checked').data('recurrence-pattern');

                if ($modal.find(recurrencePattern)) {

                    $modal.find('.recurrence-pattern-type').not(recurrencePattern).hide();

                    $(recurrencePattern).fadeIn(function () {
                        $modal.find('.scroll-container').tinyscrollbar_update('relative');
                    });
                }
            });

            $modal.find('.modal-control-cancel').on('click', function () {
                if ($modal.is(':visible')) {
                    $modal.modal('hide');
                }
            });

            /** Specific Dates Scripts**/

            // show datepicker, ok, cancel so that new date can be added to the list
            $modal.find('.add-specific-date').click(function () {
                $(this).hide();
                $('#add-specific-date-group_' + id).show();
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
                var $datepicker = $modal.find('[id$="dpSpecificDate"]');
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
                    var newLi = "<li><span>" + value + "</span> <a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
                    dateList.append(newLi);
                });

                $modal.find('#add-specific-date-group_' + id).hide();
                $modal.find('.add-specific-date').show();

                $modal.find('.scroll-container').tinyscrollbar_update('relative');
            });

            // cancel out of adding a new date
            $modal.find('.add-specific-date-cancel').click(function () {
                $modal.find('#add-specific-date-group_' + id).hide();
                $modal.find('.add-specific-date').show();
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

                $modal.find('.scroll-container').tinyscrollbar_update('relative');
            });

            /** Exclusion DateRanges Scripts **/

            // show dateRangepicker, ok, cancel so that new dateRange can be added to the list
            $modal.find('.add-exclusion-daterange').click(function () {
                $(this).hide();
                $modal.find('#add-exclusion-daterange-group_' + id).show();
            });

            // add new date to list when ok is clicked
            $modal.find('.add-exclusion-daterange-ok').click(function () {

                // get daterange list from hidden field
                var dateRangeListValues = $modal.find('input[id*="hfExclusionDateRangeListValues"]').val().split(",");
                if (!dateRangeListValues[0]) {
                    // if blank, initialize as a new empty array
                    dateRangeListValues = [];
                }

                // set colors back to default just in case previously marked invalid
                var $exclusionStart = $modal.find('input[id*="dpExclusionDateStart"]');
                $exclusionStart.css('color', '');
                var $exclusionEnd = $modal.find('input[id*="dpExclusionDateEnd"]');
                $exclusionEnd.css('color', '');

                var startDate = Date.parse($exclusionStart.val());
                var endDate = Date.parse($exclusionEnd.val());

                if (!startDate || !endDate) {
                    // if either is blank, don't color, just exit
                    return;
                }

                if ((startDate < 0) || (endDate < 0)) {

                    if (startDate < 0) {
                        $exclusionStart.css('color', 'red');
                    }
                    if (endDate < 0) {
                        $exclusionEnd.css('color', 'red');
                    }

                    // if either is invalid, exit
                    return;
                }

                var newDateRange;
                if (startDate <= endDate) {
                    newDateRange = $exclusionStart.val() + ' - ' + $exclusionEnd.val();
                }
                else {
                    // invalid because startdate is after enddate
                    $exclusionStart.css('color', 'red');
                    $exclusionEnd.css('color', 'red');
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
                $modal.find('input[id*="hfExclusionDateRangeListValues"]').val(dateRangeListValues);

                // rebuild the UL
                var dateRangeList = $modal.find('.lstExclusionDateRanges');
                dateRangeList.children().remove();
                $.each(dateRangeListValues, function (i, value) {
                    // add to ul
                    var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
                    dateRangeList.append(newLi);
                });

                $modal.find('#add-exclusion-daterange-group_' + id).hide();
                $modal.find('.add-exclusion-daterange').show();

                $modal.find('.scroll-container').tinyscrollbar_update('relative');
            });

            // cancel out of adding a new dateRange
            $modal.find('.add-exclusion-daterange-cancel').click(function () {
                $modal.find('#add-exclusion-daterange-group' + id).hide();
                $modal.find('.add-exclusion-daterange').show();
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

                $modal.find('.scroll-container').tinyscrollbar_update('relative');
            });

            // validate on Save
            $modal.find('[id*="btnSaveSchedule"]').on('click', function (event) {
                var $datepicker = $modal.find('[id*="dpStartDateTime"]'),
                    startDateValue = Date.parse($datepicker.val()) || -1;
                
                if (startDateValue < 0) {
                    $datepicker.parents(".control-group").first().toggleClass("error", 1);
                    event.preventDefault();
                    return;
                }
                else {
                    $datepicker.parents(".control-group").first().toggleClass("error", 0);
                }

                if ($modal.is(':visible')) {
                    $modal.modal('hide');
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