(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.slidingDateRangePicker = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var $picker = $('#' + options.id);

                // the dropdown with current, last, previous, daterange in it
                var $select = $picker.find('.js-slidingdaterange-select');

                showHideControls($picker, $select);
                updateDateRangeInfo($picker);

                $select.on('change', function () {
                    showHideControls($picker, $select);
                    updateDateRangeInfo($picker);
                });

                $('.js-number, .js-time-units-singular, .js-time-units-plural, .js-slidingdaterange-select', $picker).on('change', function () {
                    updateDateRangeInfo($picker);
                });

                $('.js-number', $picker).on('keyup', function () {
                    updateDateRangeInfo($picker);
                });
            },
        },
        showHideControls = function ($picker, $select) {
            var isLast = $select.val() == '0';
            var isCurrent = $select.val() == '1';
            var isDateRange = $select.val() == '2';
            var isPrevious = $select.val() == '4';
            $picker.find('.js-number').toggle(isLast || isPrevious);
            $picker.find('.js-time-units-singular').toggle(isCurrent);
            $picker.find('.js-time-units-plural').toggle(isLast || isPrevious);
            $picker.find('.js-time-units-date-range').toggle(isDateRange);
            var $pickerContainer = $picker.closest('.js-slidingdaterange-container');
            $pickerContainer.find('.js-slidingdaterange-info').toggle(isLast || isPrevious || isCurrent);
        },
        updateDateRangeInfo = function ($picker) {
            var $select = $picker.find('.js-slidingdaterange-select');
            var isLast = $select.val() == '0';
            var isCurrent = $select.val() == '1';
            var isDateRange = $select.val() == '2';
            var isPrevious = $select.val() == '4';
            var timeUnitType = 0;
            if ($picker.find('.js-time-units-singular').is(':visible')) {
                timeUnitType = $picker.find('.js-time-units-singular').val();
            } else {
                timeUnitType = $picker.find('.js-time-units-plural').val();
            }

            var numberOf = $picker.find('.js-number').val();

            var $pickerContainer = $picker.closest('.js-slidingdaterange-container');

            var getDateRangeUrl = Rock.settings.get('baseUrl') + 'api/Utility/CalculateSlidingDateRange?slidingDateRangeType=' + $select.val() + '&timeUnitType=' + timeUnitType + '&number=' + numberOf;
            $.get(getDateRangeUrl, function (r) {
                $pickerContainer.find('.js-slidingdaterange-info').text(r);
            });

            var getTextValueUrl = Rock.settings.get('baseUrl') + 'api/Utility/GetSlidingDateRangeTextValue?slidingDateRangeType=' + $select.val() + '&timeUnitType=' + timeUnitType + '&number=' + numberOf;
            $.get(getTextValueUrl, function (r) {
                $pickerContainer.find('.js-slidingdaterange-text-value').val(r);
            });
        }

        return exports;
    }());
}(jQuery));