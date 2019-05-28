(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.captcha = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }
                if (!options.key) {
                    throw 'key is required';
                }

                $('#' + options.id).data('key', options.key);

                // For some reason the reCaptcha library cannot call the onloadInitialize function
                // directly, I suspect because it is in a class. Put a chain function in the global
                // namespace to help us out.
                if (!window.Rock_controls_captcha_onloadInitialize) {
                    window.Rock_controls_captcha_onloadInitialize = function () {
                        Rock.controls.captcha.onloadInitialize();
                    };
                }

                if (Rock.controls.captcha._onloadInitialized !== true) {
                    Sys.Application.add_load(function () {
                        Rock.controls.captcha.onloadInitialize();
                    });
                    Rock.controls.captcha._onloadInitialized = true;
                }
            },
            onloadInitialize: function () {
                // Perform final initialization on all captchas.
                if (typeof (grecaptcha) != 'undefined') {
                    $('.js-captcha').each(function () {
                        var $captcha = $(this);
                        var $validator = $captcha.closest('.form-group').find('.js-captcha-validator');

                        if ($captcha.data('captcha-id') == undefined) {
                            var verifyCallback = function (response) {
                                ValidatorValidate(window[$validator.prop('id')]);
                            };
                            var expiredCallback = function (response) {
                                ValidatorValidate(window[$validator.prop('id')]);
                            };

                            var id = grecaptcha.render($captcha.prop('id'), {
                                'sitekey': $captcha.data('key'),
                                'callback': verifyCallback,
                                'expired-callback': expiredCallback
                            });

                            $captcha.data('captcha-id', id);
                        }
                    });
                }
            },
            clientValidate: function (validator, args) {
                var $captcha = $(validator).closest('.form-group').find('.js-captcha');
                var required = $captcha.data('required') == true;
                var captchaId = $captcha.data('captcha-id');

                var isValid = !required || grecaptcha.getResponse(captchaId) !== '';

                if (isValid) {
                    $captcha.closest('.form-group').removeClass('has-error');
                    args.IsValid = true;
                }
                else {
                    $captcha.closest('.form-group').addClass('has-error');
                    args.IsValid = false;
                    validator.errormessage = $captcha.data('required-error-message');
                }
            }
        };

        return exports;
    }());
}(jQuery));
