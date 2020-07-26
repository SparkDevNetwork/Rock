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
                var $addressFieldControls = $addressControl.find('.js-address-field.required');
                var addressControlLabelText = $addressControl.attr('data-itemlabel');
                var isValid = true;
                var invalidFieldList = [];

                $addressFieldControls.each(function (index, fieldControl) {
                    var $fieldControl = $(fieldControl);
                    if (typeof fieldControl == 'undefined' || $fieldControl.val().length > 0) {
                        $fieldControl.parent('div.form-group').removeClass('has-error');
                    }
                    else {
                        $fieldControl.parent('div.form-group').addClass('has-error');
                        invalidFieldList.push($fieldControl.attr('field-name'));
                        isValid = false;
                    }
                });

                if (!isValid) {
                    validator.errormessage = addressControlLabelText + " field is required: " + invalidFieldList.join(", ");
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
