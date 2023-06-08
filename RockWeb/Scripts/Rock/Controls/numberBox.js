(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.numberBox = (function () {
        var exports = {
            clientValidate: function (validator, args) {
                var validationControl = $(validator);
                var numberType = validationControl.attr('type');

                // Only validate numeric types - currency and other formats require server validation.
                if (!(numberType === 'double' || numberType === 'integer')) {
                    return;
                }

                var $numberBox = $(validator).closest('.js-number-box');
                var isValid = true;
                var validationMessage = '';

                var label = $numberBox.find('label').text();
                if (!label) {
                    label = "Value";
                }

                var value = args.Value;

                // Check for a valid number - allow a signed or unsigned decimal with or without digit separators.
                var floatRegex = /(?=.*?\d)^[-,+]?(([1-9]\d{0,2}(,\d{3})*)|\d+)?(\.\d{1,9})?$/s;
                validationMessage = label + " must be a valid number.";
                isValid = floatRegex.test(value);

                // Get the numeric value.
                value = parseFloat(value);

                // Check for integer.
                if (isValid) {
                    if (numberType === "integer") {
                        validationMessage = label + " must be an integer value.";
                        isValid = Number.isInteger(value);
                    }
                }

                // Check range.
                if (isValid) {
                    var max = parseFloat(validationControl.attr("max"));
                    var min = parseFloat(validationControl.attr("min"));
                    var checkMaxValue = !Number.isNaN( max );
                    var checkMinValue = !Number.isNaN( min );

                    if ( checkMinValue && checkMaxValue) {
                        validationMessage = label + ' must have a value between ' + min + ' and ' + max + '.';
                        isValid = (value >= min && value <= max);
                    }
                    else if (checkMinValue) {
                        validationMessage = label + ' must have a value of ' + min + ' or more.';
                        isValid = (value >= min);
                    }
                    else if (checkMaxValue) {
                        validationMessage = label + ' must have a value of ' + max + ' or less.';
                        isValid = (value <= max);
                    }
                }

                args.IsValid = isValid;

                // Set the visual feedback.
                if (isValid) {
                    $numberBox.removeClass('has-error');
                    $numberBox.attr('title', '');
                }
                else {
                    $numberBox.addClass('has-error');
                    $numberBox.attr('title', validationMessage);

                    validator.errormessage = validationMessage;
                    validator.isvalid = false;
                }
            }
        };

        return exports;
    }());
}(jQuery));
