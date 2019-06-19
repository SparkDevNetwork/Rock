(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.addressControl = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }
            },
            clientValidate: function (validator, args) {
                var $addressControl = $(validator).closest('.js-addressControl');
                var street1 = $addressControl.find('.js-street1').val();
                var city = $addressControl.find('.js-city').val();
                var state = $addressControl.find('.js-state').val();
                
                var required = $addressControl.attr('data-required') == 'true';
                var itemLabelText = $addressControl.attr('data-itemlabel');

                var isValid = true;

                if (required) {
                    // if required, then make sure that the date range has a start and/or end date (can't both be blank)
                    if (street1.length == 0 || city.length == 0 || state.length == 0) {
                        isValid = false;
                        validator.errormessage = itemLabelText + " is required";
                    }
                }

                var control = $addressControl;
                if (isValid) {
                    control.removeClass('has-error');
                }
                else {
                    control.addClass('has-error');
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
