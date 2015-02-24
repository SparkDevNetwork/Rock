(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.yearPicker = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var earliestDate = new Date(1000, 1, 1);

                // uses https://github.com/eternicode/bootstrap-datepicker
                $('#' + options.id).datepicker({
                    format: 'yyyy',
                    autoclose: true,
                    startView: 2,
                    minViewMode: 2,
                    startDate: earliestDate
                });

            }
        };

        return exports;
    }());
}(jQuery));