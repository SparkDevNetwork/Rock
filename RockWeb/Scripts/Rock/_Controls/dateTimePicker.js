(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.dateTimePicker = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                $('#' + options.id).kendoDateTimePicker();
            }
        };

        return exports;
    }());
}(jQuery));