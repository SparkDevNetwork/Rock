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

                if (!options.format) {
                    $('#' + options.id).kendoDatePicker();
                }
                else {
                    $('#' + options.id).kendoDatePicker({
                        format: "" + options.format + "",
                        depth: "" + options.depth + "",
                        start: "" + options.start + ""
                    });
                }
            }
        };

        return exports;
    }());
}(jQuery));