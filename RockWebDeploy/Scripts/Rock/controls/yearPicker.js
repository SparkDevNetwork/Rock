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

                // uses https://github.com/ianserlin/bootstrap-datepicker/tree/3.x
                $('#' + options.id).datepicker({
                    format: 'yyyy',
                    autoclose: true,
                    startView: 2,
                    minViewMode: 2
                });

            }
        };

        return exports;
    }());
}(jQuery));