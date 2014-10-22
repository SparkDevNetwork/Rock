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
                var dateFormat = 'mm/dd/yyyy';
                if (options.format) {
                    dateFormat = options.format;
                }

                var $textBox = $('#' + options.id);

                // uses https://github.com/ianserlin/bootstrap-datepicker/tree/3.x
                $textBox.datepicker({
                    format: dateFormat,
                    autoclose: true,
                    todayBtn: true,
                    startView: options.startView || 'month'
                });

                $('#' + options.id).closest('.form-control-group').find('input:checkbox').click(function () {
                    if ( $(this).is(':checked')) {
                        $textBox.val('');
                        $textBox.prop('disabled', true);
                        $textBox.addClass('aspNetDisabled');
                    } else {
                        $textBox.prop('disabled', false);
                        $textBox.removeClass('aspNetDisabled');
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));