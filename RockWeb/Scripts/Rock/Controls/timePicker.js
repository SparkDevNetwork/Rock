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

                var $tp = $('#' + options.id);

                // bootstrap-timepicker requires that the parent div have bootstrap-timepicker, input-append classes
                $tp.closest('div').addClass('bootstrap-timepicker').addClass('input-append');

                // uses https://github.com/jdewit/bootstrap-timepicker
                $tp.timepicker({
                    defaultTime: false
                });
                
                $tp.closest('.js-timepicker-input').find('.js-timepicker-clear').click(function () {
                    $tp.timepicker('clear');
                });
            }
        };

        return exports;
    }());
}(jQuery));