(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.timePicker = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                // bootstrap-timepicker requires that the parent div have bootstrap-timepicker, input-append classes
                $('#' + options.id).closest('div').addClass('bootstrap-timepicker').addClass('input-append');


                // uses https://github.com/jdewit/bootstrap-timepicker
                $('#' + options.id).timepicker();
            }
        };

        return exports;
    }());
}(jQuery));