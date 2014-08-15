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
                    defaultTime: false,
                    appendWidgetTo: '.bootstrap-timepicker'
                });
                
                $tp.on('show.timepicker', function (e) {
                    var $scrollcontainer = $tp.closest('.scroll-container');
                    if ($scrollcontainer.length) {
                        $scrollcontainer.tinyscrollbar_update('relative');
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));