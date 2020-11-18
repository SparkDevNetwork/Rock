(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.emailConfirmControl = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }
            },
            clientValidate: function (validator, args) {
                var $emailConfirmControl = $(validator).closest('.js-emailConfirmControl');
                var isValid = true;

                var primaryEmail = $emailConfirmControl.find('.js-primary input').val().trim().toLowerCase();
                var confirmEmail = $emailConfirmControl.find('.js-confirm input').val().trim().toLowerCase();

                if (primaryEmail.length > 0 && primaryEmail !== confirmEmail) {
                    isValid = false;
                    validator.errormessage = "Email and confirmation do not match.";
                    $emailConfirmControl.find('.js-confirm').closest('.form-group').addClass('has-error');
                }
                else {
                    $emailConfirmControl.find('.js-confirm').closest('.form-group').removeClass('has-error');
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
