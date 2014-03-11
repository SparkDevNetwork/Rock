(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.datePicker = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                // uses https://github.com/ianserlin/bootstrap-datepicker/tree/3.x
                $('#' + options.id).datepicker({
                    autoclose: true,
                    todayBtn: true
                });

            }
        };

        return exports;
    }());
}(jQuery));