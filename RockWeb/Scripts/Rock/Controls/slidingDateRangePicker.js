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

                $select.on('change', function () {
                    showHideControls($picker, $select);
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

            $picker.siblings('.js-slidingdaterange-info').toggle(isLast || isPrevious || isCurrent);
        }

        return exports;
    }());
}(jQuery));