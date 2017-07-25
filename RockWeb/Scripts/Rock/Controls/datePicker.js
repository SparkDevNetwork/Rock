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

                var $datePickerContainer = $textBox.closest('.js-date-picker-container');
                var $datePickerInputGroup = $textBox.closest('.input-group.js-date-picker');

                // uses https://github.com/eternicode/bootstrap-datepicker
                $datePickerInputGroup.datepicker({
                    format: dateFormat,
                    autoclose: true,
                    todayBtn: true,
                    forceParse: options.forceParse,
                    endDate: options.endDate || new Date(8640000000000000),
                    startView: options.startView,
                    todayHighlight: options.todayHighlight
                });

                // if the guest clicks the addon select all the text in the input
                $datePickerInputGroup.find('.input-group-addon').on('click', function () {
                    $(this).siblings('.form-control').select();
                });

                $datePickerContainer.find('.js-current-date-checkbox').on('click', function (a,b,c) {
                    var $dateOffsetBox = $datePickerContainer.find('.js-current-date-offset');
                    var $dateOffsetlabel = $("label[for='" + $dateOffsetBox.attr('id') + "']")
                    if ($(this).is(':checked')) {
                        $dateOffsetlabel.show();
                        $dateOffsetBox.show();

                        // set textbox val to something instead of empty string so that validation doesn't complain
                        $textBox.val('current');

                        $textBox.prop('disabled', true);
                        $textBox.addClass('aspNetDisabled');

                    } else {
                        $dateOffsetlabel.hide();
                        $dateOffsetBox.hide();
                        $textBox.prop('disabled', false);
                        
                        // set textbox val to empty string so that validation will work again (if it is enabled)
                        $textBox.val('');

                        $textBox.removeClass('aspNetDisabled');
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));