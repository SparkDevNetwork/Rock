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
                var dateFormat = 'mm/dd/yyyy';
                if (options.format) {
                    dateFormat = options.format;
                }

                var $dp = $('#' + options.id + " .js-datetime-date");

                var $dateTimePickerContainer = $dp.closest('.js-datetime-picker-container');
                var $dateTimePickerInputGroup = $dp.closest('.input-group.date');

                // uses https://github.com/uxsolutions/bootstrap-datepicker
                $dateTimePickerInputGroup.datepicker({
                    format: dateFormat,
                    assumeNearbyYear: 10,
                    autoclose: true,
                    todayBtn: "linked",
                    startView: options.startView || 'month',
                    todayHighlight: options.todayHighlight || true,
                    zIndexOffset: 1050
                });

                // if the guest clicks the addon select all the text in the input
                $dateTimePickerInputGroup.find('.input-group-addon').on('click', function ()
                {
                  $(this).siblings('.form-control').select();
                });

                var $tp = $('#' + options.id + " .js-datetime-time");
                if ($tp) {
                    var $tpid = $tp.attr('id');
                    if ($tpid) {
                        Rock.controls.timePicker.initialize({
                            id: $tpid
                        });
                    }
                }

                $dateTimePickerContainer.find('.js-current-datetime-checkbox').on('click', function (a, b, c) {
                    var $dateTimeOffsetBox = $dateTimePickerContainer.find('.js-current-datetime-offset');
                    var $dateOffsetlabel = $("label[for='" + $dateTimeOffsetBox.attr('id') + "']")
                    if ($(this).is(':checked')) {
                        $dateOffsetlabel.removeClass('aspNetDisabled').show();
                        $dateTimeOffsetBox.show();
                        $dateTimeOffsetBox.prop('disabled', false).removeClass('aspNetDisabled').val( $dateTimeOffsetBox.data('last-value') );
                        $dp.data( "last-value", $dp.val()).val('').prop('disabled', true).addClass('aspNetDisabled').prop('placeholder', 'Current');
                        $tp.data( "last-value", $tp.val()).val('').prop('disabled', true).addClass('aspNetDisabled');
                    } else {
                        $dateOffsetlabel.addClass('aspNetDisabled').hide();
                        $dateTimeOffsetBox.data( "last-value", $dateTimeOffsetBox.val()).hide();
                        $dateTimeOffsetBox.val('').prop('disabled', true).addClass('aspNetDisabled');
                        $dp.prop('disabled', false).removeClass('aspNetDisabled').prop('placeholder', '').val($dp.data('last-value'));
                        $tp.prop('disabled', false).removeClass('aspNetDisabled').val($tp.data('last-value'));
                    }
                });
            }
        };

        return exports;
    }());
}(jQuery));
