(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.numberRangeEditor = (function () {
        var exports = {
            clientValidate: function (validator, args) {
                var $numberRangeEditor = $(validator).closest('.js-numberrangeeditor');
                var lowerValue = $numberRangeEditor.find('input.js-number-range-lower ').val();
                var upperValue = $numberRangeEditor.find('input.js-number-range-upper').val();
                var required = $numberRangeEditor.attr('data-required') == 'true';
                var itemLabelText = $numberRangeEditor.attr('data-itemlabel');

                var isValid = true;

                if (required) {
                    // if required, then make sure that the number range has a lower and/or upper value (can't both be blank)
                    if (lowerValue.length == 0 && upperValue.length == 0) {
                        isValid = false;
                        validator.errormessage = itemLabelText + " is required";
                    }
                }

                var control = $numberRangeEditor;
                var labelForSelector = "label[for='" + $numberRangeEditor.prop('id') + "']";
                var $labelFor = $(labelForSelector)
                if (isValid) {
                    control.removeClass('has-error');
                    $labelFor.parent().removeClass('has-error');
                }
                else {
                    control.addClass('has-error');
                    $labelFor.parent().addClass('has-error');
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
