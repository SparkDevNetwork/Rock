(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.captcha = (function () {

        // #region Constants

        const CaptchaMode = Object.freeze({
            Visible: 0,
            Invisible: 1,
            Disabled: 2
        });

        /**
         * A map of RockCaptcha controls keyed by their control IDs.
         * @type {Object.<string, RockCaptcha>}
         */
        const captchaControls = {};

        // #endregion Constants

        // #region Types

        /**
         * @typedef CaptchaConfigurationBag
         * @property {CaptchaMode} captchaMode
         */

        /**
         * @typedef CaptchaInitializeResultBag
         * @property {(CaptchaProofOfWorkChallengeBag | null | undefined)} pow Might be null
         */

        /**
         * @typedef CaptchaProofOfWorkChallengeBag
         * @property {number} challengeCount
         * @property {number} challengeDifficulty
         * @property {number} challengeSize
         * @property {string} challengeToken
         */

        /**
         * @typedef CaptchaVerifyOptionsBag
         * @property {CaptchaProofOfWorkVerifyOptionsBag} powOptions Might be null
         */

        /**
         * @typedef CaptchaProofOfWorkVerifyOptionsBag
         * @property {string} challengeToken
         * @property {number[]} challengeSolutions
         */

        /**
         * @typedef CaptchaVerifyResultBag
         * @property {boolean} isVerified
         * @property {string} error Can be null
         * @property {number} expires Can be null
         * @property {string} token Can be null 
         */

        /**
         * @typedef InitializeConfig
         * @property {string} id The ID for the CAPTCHA control.
         * @property {string} postBackScript The postback script when a CAPTCHA token has been verified.
         */

        /**
         * The RockCaptcha initialization options.
         * @typedef RockCaptchaInitOptions
         * @property {string} controlId The control ID for the element (without leading #).
         * @property {number} captchaMode The CAPTCHA mode.
         * @property {string} postBackScript The script to run when the CAPTCHA token is resolved.
         */

        /**
         * The Api initialization options.
         * @typedef ApiInitOptions
         * @property {string} baseUrl The base URL for API requests.
         */

        /**
         * @typedef SolveChallengeOptions
         * @property {string[]} solutions
         * @property {string} token
         */

        /**
         * @typedef CapChallengeResult
         * @property {CapChallenge} challenge Might be null
         * @property {string} token Might be null
         */

        /**
         * @typedef CapChallenge
         * @property {number} c
         * @property {number} s
         * @property {number} d
         */

        /**
         * @typedef CapRedeemOptions
         * @property {number[]} solutions
         * @property {string} token
         */

        /**
         * @typedef CapRedeemResult
         * @property {boolean} success
         * @property {string} message Might be null
         * @property {number} expires Might be null
         * @property {string} token Might be null
         */

        /**
         * Creates a new instance of the RockCaptcha class.
         * @param {RockCaptchaInitOptions} options
         */
        function RockCaptcha(options) {
            if (!options.controlId) {
                throw "`controlId` is required.";
            }

            // Set properties.
            /** @type {string} */
            this.controlId = options.controlId;

            /** @type {CaptchaMode} */
            this.captchaMode = options.captchaMode;

            /** @type {string} */
            this.postBackScript = options.postBackScript;

            /** @type {string} */
            this.captchaToken = ""; // initialize with an empty CATCHA token.

            /** @type {HTMLElement | null} Can be null */
            this.widgetEl = null; // initialize the widget element to null.

            /** @type {boolean} */
            this.disposed = false;

            // Set JQuery selectors.
            /** @type {JQuery<HTMLElement>} */
            this.$controlSelector = $("#" + this.controlId);

            /** @type {JQuery<HTMLElement>} */
            this.$hfToken = $("#" + this.controlId + '_hfToken');
        }

        RockCaptcha.prototype.render = async function () {
            const areCustomElementsSupported = typeof window.customElements !== "undefined";

            if (areCustomElementsSupported && this.captchaMode === CaptchaMode.Visible) {
                this.widgetEl = document.createElement("cap-widget");
                this.widgetEl.setAttribute("data-cap-api-endpoint", "/"); // This is a required attribute for the library but the endpoint will be overridden later.

                // Styles
                this.widgetEl.style.setProperty("--cap-background", "var(--color-interface-softest)");
                this.widgetEl.style.setProperty("--cap-border-radius", "var(--rounded-small)");
                this.widgetEl.style.setProperty("--cap-border-color", "var(--color-interface-soft)");
                this.widgetEl.style.setProperty("--cap-widget-padding", "var(--spacing-medium)");
                this.widgetEl.style.setProperty("--cap-color", "var(--color-interface-strong)");
                this.widgetEl.style.setProperty("--cap-widget-width", "200px");
                this.widgetEl.style.setProperty("--cap-checkbox-background", "var(--color-interface-softest)");
                this.widgetEl.style.setProperty("--cap-checkbox-border", "1px solid var(--color-interface-soft)");
                this.widgetEl.style.setProperty("--cap-checkbox-border-radius", "var(--rounded-xsmall)");
                this.widgetEl.style.setProperty("--cap-checkbox-margin", "var(--spacing-large)");
                this.widgetEl.style.setProperty("--cap-spinner-background-color", "var(--color-interface-soft)");
                this.widgetEl.style.setProperty("--cap-spinner-color", "var(--color-primary)");
                this.widgetEl.style.setProperty("--cap-spinner-thickness", "6px");
                this.widgetEl.style.setProperty("--cap-gap", "var(--spacing-medium)");
                this.widgetEl.style.setProperty("--cap-font", "var(--font-family-body)");
                this.widgetEl.style.setProperty("--cap-checkbox-size", "20px");

                this.widgetEl.addEventListener("solve", async (e) => {
                    if (this.disposed) {
                        return;
                    }

                    const token = e && e.detail && e.detail.token ? e.detail.token : undefined;

                    this.handleSolvedToken(token);
                });

                this.widgetEl.addEventListener("error", (_e) => {
                    if (this.disposed) {
                        return;
                    }

                    this.handleError("Captcha challenge failed.");
                });

                const host = this.$controlSelector[0];
                if (host) {
                    host.appendChild(this.widgetEl);

                    setTimeout(() => {
                        if (this.widgetEl && this.widgetEl.shadowRoot) {
                            const attribution = this.widgetEl.shadowRoot.querySelector('[part="attribution"]');
                            if (attribution) {
                                attribution.remove();
                            }
                        }
                    })
                }

                return;
            }

            // Invisible mode fallback (older Safari/Edge without customElements)
            try {
                const cap = new Cap();
                const token = await cap.solve();
                if (!this.disposed) {
                    await this.handleSolvedToken(token.token);
                }
            }
            catch {
                if (!this.disposed) {
                    this.handleError("Captcha challenge failed.");
                }
            }
        };

        /**
         * Handles processing a CAPTCHA token after it is solved.
         * @param {string} token
         */
        RockCaptcha.prototype.handleSolvedToken = async function (token) {
            this.captchaToken = token;
            this.$hfToken.val(token);

            if (this.postBackScript) {
                window.location = "javascript:" + this.postBackScript;
            }
        };

        /**
         * Handles an error.
         * @param {string} msg
         */
        RockCaptcha.prototype.handleError = function (msg) {
            console.error(msg);
            this.captchaToken = "";
        };

        /**
          * Creates a new instance of the Api class.
          * @param {ApiInitOptions} options
          */
        function Api(options) {
            this.baseUrl = options.baseUrl;
        }

        /**
         * Gets the CAPTCHA configuration.
         * @returns {Promise<CaptchaConfigurationBag>}
         */
        Api.prototype.getConfiguration = async function () {
            return await Promise.resolve($.post({
                url: this.baseUrl + 'api/v2/Controls/CaptchaGetConfiguration'
            }));
        };

        /**
         * Initializes the CAPTCHA.
         * @returns {Promise<CaptchaInitializeResultBag>}
         */
        Api.prototype.initialize = async function () {
            return await Promise.resolve($.post({
                url: this.baseUrl + 'api/v2/Controls/CaptchaInitialize'
            }));
        };

        /**
         * Verifies the CAPTCHA.
         * @param {CaptchaVerifyOptionsBag} options
         * @returns {Promise<CaptchaVerifyResultBag>}
         */
        Api.prototype.verify = async function (options) {
            return await Promise.resolve($.post({
                url: this.baseUrl + 'api/v2/Controls/CaptchaVerify',
                data: options
            }));
        };

        // #endregion Types

        /**
         * Gets whether the Cap module is ready.
         * 
         * @returns Whether the Cap module is ready.
         */
        function isCapReady() {
            return typeof Cap !== 'undefined';
        }

        const api = new Api({
            baseUrl: '/'
        });

        async function loadCap() {
            if (typeof window.Cap !== "undefined") {
                return;
            }
            else {
                $.ajaxSetup({ cache: true });
                return await Promise.resolve($.getScript("https://cdn.jsdelivr.net/npm/@cap.js/widget@0.1.30"));
            }
        }

        var exports = {
            /**
             * Initializes a new RockCaptcha instance.
             * @param {InitializeConfig} config
             */
            initialize: async function (config) {
                if (!config) {
                    return;
                }

                if (!config.id) {
                    throw 'id is required';
                }

                function freezeProp(target, propertyName, value) {
                    Object.defineProperty(target, propertyName, {
                        get: () => value,
                        set: undefined,
                        configurable: false,
                        enumerable: true
                    });
                }

                // Override the fetch function used by Cap to route requests via the Rock captcha API.
                if (!window.CAP_CUSTOM_FETCH) {
                    freezeProp(window, "CAP_CUSTOM_FETCH", async function (url, options) {
                        if (typeof url === "string") {
                            if (url.endsWith("/challenge")) {
                                const result = await api.initialize();

                                // Format the challenge as expected by Cap.js

                                /** @type {CapChallengeResult} */
                                const capResult = {
                                    challenge: null,
                                    token: null
                                };

                                if (result && result.pow) {
                                    capResult.token = result.pow.challengeToken;
                                    capResult.challenge = {
                                        c: result.pow.challengeCount,
                                        s: result.pow.challengeSize,
                                        d: result.pow.challengeDifficulty
                                    };
                                }

                                return Response.json(capResult);
                            }
                            else if (url.endsWith("/redeem")) {
                                /** @type {CapRedeemOptions} */
                                const capRedeemOptions = options && options.body ? JSON.parse(options.body.toString()) : null;

                                /** @type {CaptchaVerifyOptionsBag} */
                                const verifyOptions = {
                                    powOptions: null
                                };

                                if (capRedeemOptions) {
                                    verifyOptions.powOptions = {
                                        challengeSolutions: capRedeemOptions.solutions,
                                        challengeToken: capRedeemOptions.token
                                    };
                                }

                                const result = await api.verify(verifyOptions);

                                /** @type {CapRedeemResult} */
                                const capResult = {
                                    success: false,
                                    expires: null,
                                    message: "An error occurred",
                                    token: null
                                };

                                if (result) {
                                    capResult.success = result.isVerified;
                                    capResult.message = result.error;
                                    capResult.expires = result.expires;
                                    capResult.token = result.token;
                                };

                                return Response.json(capResult);
                            }
                        }

                        return fetch(url, options);
                    });
                }

                let rockCaptcha = captchaControls[config.id];
                if (!rockCaptcha) {
                    await loadCap();
                    const captchaConfig = await api.getConfiguration();

                    rockCaptcha = new RockCaptcha({
                        api: api,
                        captchaMode: captchaConfig.captchaMode,
                        controlId: config.id,
                        postBackScript: config.postBackScript
                    });

                    // Store a ref to the new control so it can be managed later.
                    captchaControls[config.id] = rockCaptcha;
                }

                $(document).ready(async function () {
                    await captchaControls[config.id].render();
                });
            },
            clientValidate: function (validator, args) {
                var $validator = $(validator);
                var isRequired = $validator.data('required');

                var rockCaptchaId = $validator.data('captcha-id');
                var $formGroup = $('#' + rockCaptchaId).closest('.form-group');

                if (isRequired && isCapReady()) {
                    // Get this validator control's related Rock Captcha control ID and
                    // try to find the underlying Turnstile widget.
                    var turnstileId = captchaIds[rockCaptchaId];
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
