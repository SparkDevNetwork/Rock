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

                // uses https://github.com/eternicode/bootstrap-datepicker
                $textBox.datepicker({
                    format: dateFormat,
                    autoclose: true,
                    todayBtn: true,
                    startView: options.startView || 'month'
                });

                var $datePickerContainer = $textBox.closest('.js-date-picker-container');
                
                $datePickerContainer.find('.js-current-date-checkbox').on('click', function (a,b,c) {
                    var $dateOffsetBox = $datePickerContainer.find('.js-current-date-offset');
                    var $dateOffsetlabel = $("label[for='" + $dateOffsetBox.attr('id') + "']")
                    if ($(this).is(':checked')) {
                        $dateOffsetlabel.show();
                        $dateOffsetBox.show();
                        $textBox.val('');
                        $textBox.prop('disabled', true);
                        $textBox.addClass('aspNetDisabled');

                    } else {
                        $dateOffsetlabel.hide();
                        $dateOffsetBox.hide();
                        $textBox.prop('disabled', false);
                        $textBox.removeClass('aspNetDisabled');
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));