(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.numberBox = (function () {
        var exports = {
            clientValidate: function (validator, args) {
                var validationControl = $(validator);
                var max = parseFloat(validationControl.attr("max"));
                var min = parseFloat(validationControl.attr("min"));

                var checkMaxValue = max || max == 0;
                var checkMinValue = min || min == 0;

                var value = parseFloat(args.Value);

                args.IsValid = (value || value == 0)
                    && (!checkMaxValue || args.Value <= max)
                    && (!checkMinValue || args.Value >= min)

                if (!args.isValid) {
                    setTimeout(function () {
                        $("#" + validationControl[0].id).text(validationControl.attr("errormessage"));
                    }, 50);
                }
            }
        };

        return exports;
    }());
}(jQuery));
