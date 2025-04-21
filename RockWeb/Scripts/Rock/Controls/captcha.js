(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    // Cloudflare Turnstile docs:
    // https://developers.cloudflare.com/turnstile/get-started/client-side-rendering/

    Rock.controls.captcha = (function () {
        // Keep track of the last-rendered Turnstile widget ID for each Rock Captcha
        // control, so we can manage multiple Turnstile widgets per page.
        var turnstileIds = {};

        /**
         * Gets whether the Turnstile module is ready.
         * 
         * @returns Whether the Turnstile module is ready.
         */
        function isTurnstileReady() {
            return typeof turnstile !== 'undefined';
        }

        /**
         * Tries to render a Turnstile widget for a given Rock Captcha control instance.
         * 
         * @param {any} config The configuration info for this Rock Captcha control instance.
         */
        function tryRenderTurnstile(config) {
            if (!isTurnstileReady()) {
                return;
            }

            // Remove any previous widget for this Rock Captcha control to prevent
            // Turnstile from logging warnings/errors to the console.
            tryRemoveTurnstile(config.id);

            // Keep track of the last-rendered Turnstile widget's ID, as we'll
            // need to intstruct the Turnstile library not to track it anymore
            // (and we'll create a new one) for each request. Note that once the
            // challenge has been solved, it's up to the Rock block or parent
            // control to dictate that a Turnstile should no longer be rendered,
            // so our job here is to simply keep rendering a new widget on every
            // request, regardless of whether this is a full or partial postback.
            var turnstileId = turnstile.render('#' + config.id, {
                siteKey: config.key,
                callback: function (token) {
                    var $hfToken = $('#' + config.id + '_hfToken');
                    $hfToken.val(token);

                    if (token) {
                        // Allow time for the success animation to be displayed.
                        setTimeout(function () {
                            if (config.postBackScript) {
                                window.location = "javascript:" + config.postBackScript;
                            }
                        }, 750);
                    }
                },
                'error-callback': function (errorCode) {
                    if (errorCode === '300030') {
                        // This can happen if a Turnstile widget was loaded into
                        // the DOM and is no longer available (i.e. after a
                        // partial postback results in the Rock Captcha
                        // control no longer being visible).
                        tryRemoveTurnstile(config.id);

                        // Return true to prevent Turnstile from repeatedly logging to the console.
                        // https://developers.cloudflare.com/turnstile/troubleshooting/client-side-errors/#error-handling
                        return true;

                        // However, there is an issue where an already-queued
                        // `.reset()` invocation will fail and still log an
                        // error to the console; hopefully this scenario is
                        // not common.
                        // https://community.cloudflare.com/t/window-turnstile-remove-with-retry-set-to-auto-doesnt-cleartimeout/525530
                    }
                }
            });

            turnstileIds[config.id] = turnstileId;
        }

        /**
         * Tries to remove a Turnstile widget for the provided Rock Captcha control ID.
         * 
         * @param {any} rockCaptchaId The Rock Captcha control ID.
         */
        function tryRemoveTurnstile(rockCaptchaId) {
            // Try to find this Rock Captcha control's last-rendered Turnstile widget.
            var turnstileId = turnstileIds[rockCaptchaId];
            if (turnstileId) {
                // Inform the Turnstile library that the old Turnstile widget
                // should no longer be tracked.
                turnstile.remove(turnstileId);
                delete turnstileIds[rockCaptchaId];
            }
        }

        var exports = {
            initialize: function (config) {
                if (!config) {
                    return;
                }

                if (!config.id) {
                    throw 'id is required';
                }

                if (!config.key) {
                    throw 'key is required';
                }

                // Ensure Cloudflare's Turnstile script is added to the page.
                if (!$('#rockCloudflareTurnstile').length && !isTurnstileReady()) {
                    // By default, jQuery adds a cache-busting parameter on dynamically-added scripts.
                    // Set `cache: true` to avoid this, and ensure we only load the script once.
                    $.ajaxSetup({ cache: true });
                    $.getScript('https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit')
                        .done(function () {
                            tryRenderTurnstile(config);
                        })
                        .fail(function () {
                            console.error("Unable to load Captcha library.");
                        });

                    return;
                }

                // The Turnstile script was already added by the C# control;
                // just wait for the DOM to finish loading.
                $(document).ready(function () {
                    tryRenderTurnstile(config);
                });
            },
            clientValidate: function (validator, args) {
                var $validator = $(validator);
                var isRequired = $validator.data('required');

                var rockCaptchaId = $validator.data('captcha-id');
                var $formGroup = $('#' + rockCaptchaId).closest('.form-group');

                if (isRequired && isTurnstileReady()) {
                    // Get this validator control's related Rock Captcha control ID and
                    // try to find the underlying Turnstile widget.
                    var turnstileId = turnstileIds[rockCaptchaId];
                    if (turnstileId) {
                        var turnstileResponse = turnstile.getResponse(turnstileId);
                    }
                }

                var isValid = !!(!isRequired || turnstileResponse);

                if (isValid) {
                    args.IsValid = true;
                    $formGroup.removeClass('has-error');
                }
                else {
                    args.IsValid = false;
                    $formGroup.addClass('has-error');
                }
            }
        };

        return exports;
    }());
}(jQuery));
