(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.NMI = Rock.NMI || {}
    Rock.NMI.controls = Rock.NMI.controls || {};

    /** JS helper for the gatewayCollectJS */
    Rock.NMI.controls.gatewayCollectJS = (function () {
        var exports = {
            initialize: function (controlId) {
                var self = this;
                var $control = $('#' + controlId);

                if ($control.length == 0) {
                    // control hasn't been rendered so skip
                    return;
                }

                self.$creditCardInput = $('.js-credit-card-input', $control);
                self.$creditCardExpInput = $('.js-credit-card-exp-input', $control);
                self.$creditCardCVVInput = $('.js-credit-card-cvv-input', $control);
                self.$checkAccountNumberInput = $('.js-check-account-number-input', $control);
                self.$checkRoutingNumberInput = $('.js-check-routing-number-input', $control);
                self.$checkFullNameInput = $('.js-check-fullname-input', $control);

                self.$paymentInputs = $('.js-nmi-payment-inputs', $control);
                self.$selectedPaymentType = $('.js-selected-payment-type', $control);

                // collectJs needs a payment button to work, so it exists but isn't shown, and we don't do anything with it
                self.$paymentButton = $('.js-payment-button', $control);

                self.$paymentValidation = $('.js-payment-input-validation', $control);

                self.$responseToken = $('.js-response-token', $control)
                self.$rawResponseToken = $('.js-tokenizer-raw-response', $control)

                self.$submitPaymentmentInfo = $('.js-submit-hostedpaymentinfo');

                // remove the display style that was rendered so that hide/show works
                self.$paymentValidation.css({ display: '' });

                self.$paymentValidation.hide();
                self.validationFieldStatus = {
                    ccnumber: {},
                    ccexp: {},
                    cvv: {},
                    checkaccount: {},
                    checkaba: {},
                    checkname: {},
                    inputsValidated: function () {
                        var field = self.validationFieldStatus;
                        if (self.$selectedPaymentType.val() == 'ach') {
                            return (field.checkaccount.status && field.checkaba.status && field.checkname.status);
                        } else {
                            return (field.ccnumber.status && field.ccexp.status && field.cvv.status);
                        }
                    }
                }

                self.tokenizerPostbackScript = $control.attr('data-tokenizer-postback-script');
                self.currencyChangePostbackScript = $control.attr('data-currencychange-postback-script');
                var enabledPaymentTypes = JSON.parse($('.js-enabled-payment-types', $control).val());

                self.$creditCardContainer = $('.js-gateway-creditcard-container', $control);
                self.$achContainer = $('.js-gateway-ach-container', $control);

                self.$paymentTypeSelector = $('.js-gateway-paymenttype-selector', $control);
                self.$paymentButtonCreditCard = $('.js-payment-creditcard', $control);
                self.$paymentButtonACH = $('.js-payment-ach', $control);

                // tokenResponseSent helps track if we sent a token response back to Rock. Since a validation event could happen 'after' submitting the request, we might have sent the token response back to rock
                // the timeoutCallback could also sent a token Response back to rock, so we'll check tokenResponseSent to avoid a double postback
                self.tokenResponseSent = function (sent) {
                    if ((typeof sent) === 'boolean') {
                        $control.data["tokenResponseSent"] = sent;
                    }

                    return $control.data["tokenResponseSent"];
                }

                var inputStyles = function (style) {
                    return $('.js-input-style-hook').css(style)
                };

                var inputInvalidStyles = function (style) {
                    return $('.js-input-invalid-style-hook').css(style)
                };

                self.collectJSSettings = {
                    paymentSelector: '#' + controlId + ' .js-payment-button',
                    // There is a paymentType option, but it only affects which fields get created when in Lightbox mode, so it doesn't help us since we are in inline mode
                    // paymentType: ck,cc...

                    // we are using inline mode (lightbox mode pops up a dialog to prompt for payment info)
                    variant: "inline",

                    /* field configuration */
                    fields: {
                        ccnumber: {
                            selector: '#' + controlId + ' .js-credit-card-input',
                            title: "Card Number",
                            placeholder: "0000 0000 0000 0000"
                        },
                        ccexp: {
                            selector: '#' + controlId + ' .js-credit-card-exp-input',
                            title: "Card Expiration",
                            placeholder: "MM / YY"
                        },
                        cvv: {
                            display: "show",
                            selector: '#' + controlId + ' .js-credit-card-cvv-input',
                            title: "CVV Code",
                            placeholder: "CVV"
                        },
                        checkaccount: {
                            selector: '#' + controlId + ' .js-check-account-number-input',
                            title: "Account Number",
                            placeholder: "Account Number"
                        },
                        checkaba: {
                            selector: '#' + controlId + ' .js-check-routing-number-input',
                            title: "Routing Number",
                            placeholder: "Routing Number"
                        },
                        checkname: {
                            selector: '#' + controlId + ' .js-check-fullname-input',
                            title: "Name on Checking Account",
                            placeholder: "Name on Account"
                        }
                    },

                    styleSniffer: false, // we probably want to disable this. A fake input is created and gatewayCollect will steal css from that (and will disables all the css options)

                    /* Available CSS options. It can only be 1 level deep */
                    /* Only a limited number of styles are supported. see https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#cjs_integration_inline2 */
                    customCss: {
                        // applied to all input fields
                        'color': inputStyles('color'),
                        'border-bottom-color': inputStyles('border-bottom-color'),
                        'border-bottom-left-radius': inputStyles('border-bottom-left-radius'),
                        'border-bottom-right-radius': inputStyles('border-bottom-right-radius'),
                        'border-bottom-style': inputStyles('border-bottom-style'),
                        'border-bottom-width': inputStyles('border-bottom-width'),
                        'border-left-color': inputStyles('border-left-color'),
                        'border-left-style': inputStyles('border-left-style'),
                        'border-left-width': inputStyles('border-left-width'),
                        'border-right-color': inputStyles('border-right-color'),
                        'border-right-style': inputStyles('border-right-style'),
                        'border-right-width': inputStyles('border-right-width'),
                        'border-top-color': inputStyles('border-top-color'),
                        'border-top-left-radius': inputStyles('border-top-left-radius'),
                        'border-top-right-radius': inputStyles('border-top-right-radius'),
                        'border-top-style': inputStyles('border-top-style'),
                        'border-top-width': inputStyles('border-top-width'),
                        'border-width': inputStyles('border-width'),
                        'border-style': inputStyles('border-style'),
                        'border-radius': inputStyles('border-radius'),
                        'border-color': inputStyles('border-color'),
                        'background-color': inputStyles('background-color'),
                        'box-shadow': inputStyles('box-shadow'),
                        'margin-bottom': '5px',
                        'margin-top': '0',
                        'padding': inputStyles('padding'),
                        'font-size': inputStyles('font-size'),
                        'height': inputStyles('height'),
                        'font-family': inputStyles('font-family'),
                    },
                    focusCss: {
                        'border-color': getComputedStyle(document.documentElement).getPropertyValue('--focus-state-border-color'),
                        'outline-style': 'none'
                    },
                    invalidCss: {
                        "border-color": inputInvalidStyles('border-color'),
                    },
                    placeholderCss: {
                        'color': getComputedStyle(document.documentElement).getPropertyValue('--input-placeholder')
                    },

                    /* Callback options*/

                    // CollectJS uses timeoutDuration and timeoutCallback to handle either connectivity issues, or invalid input
                    // In other words, if input is invalid, CollectJS just times out (it doesn't tell us)
                    timeoutDuration: 10000,
                    timeoutCallback: function () {

                        // if got a validation callback aftering submitting a token request, we probably already sent the token response( with a validation message).
                        // so, if we already sent a token response back (thru a postback), we don't have to deal with timeouts
                        var tokenResponseSent = self.tokenResponseSent();
                        if (tokenResponseSent) {
                            return false;
                        }

                        // a timeout callback will fire due to a timeout or incomplete input fields (CollectJS doesn't tell us why)
                        console.log("The tokenization didn't respond in the expected timeframe. This could be due to an invalid or incomplete field or poor connectivity - " + Date());

                        var tokenResponse;

                        // Since we don't know exactly what happened, lets see if it might be invalid inputs by checking them all manually
                        self.validateInputs(function (v) {
                            if (!v.IsValid) {
                                // timeout was probably due to invalid inputs so construct a token response with a ValidationMessage
                                tokenResponse = {
                                    token: '',
                                    validationMessage: v.ValidationMessage
                                };
                            }
                            else {
                                // inputs seem to be valid, so show a message to let them know what seems to be happeninng
                                console.log("Timeout happened for unknown reason, probably poor connectivity since we already validated inputs.");

                                tokenResponse = {
                                    token: '',
                                    errorMessage: 'Response from gateway timed out. This could be do to poor connectivity or invalid payment values.'
                                }

                                // we'll be doing a postback which will display the error, but just in case, show a validation message here
                                var $validationMessage = self.$paymentValidation.find('.js-validation-message')
                                $validationMessage.text(tokenResponse.ErrorMessage);
                            }
                        });

                        self.handleTokenResponse(tokenResponse);
                    },

                    // Collect JS will validate inputs when blurring out of fields (and it might take a second for it to fire this callback)
                    validationCallback: function (field, status, message) {
                        // if there is a validation error, keep the message and field that has the error. Then we'll check it before doing the submitPaymentInfo

                        if (message == 'Field is empty') {
                            message = field + ' is empty';
                        }

                        self.validationFieldStatus[field] = {
                            field: field,
                            status: status,
                            message: message
                        };

                        var fieldVisible = $(CollectJS.config.fields[field].selector).is(":visible");
                        if (!status && CollectJS.inSubmission && fieldVisible) {
                            // failed validation event while waiting for submission response

                            var tokenResponse = {
                                token: '',
                                validationMessage: message
                            };

                            // tokenResponseSent helps track if we sent a token response back to Rock. Since multiple validation events could happen 'after' submitting the request, we might have sent the token response back to rock
                            var tokenResponseSent = self.tokenResponseSent();
                            if (tokenResponseSent) {
                                // if we already sent a tokenResponse, there is no need to
                                return false;
                            }
                            else {
                                self.handleTokenResponse(tokenResponse);
                            }
                        }
                    },

                    // After we call CollectJS.configure, we have to wait for CollectJS to create all the iframes for the input fields.
                    // Note that it adds the inputs to the DOM one at a time, and then fires this callback when all of them have been created.
                    fieldsAvailableCallback: function (a, b, c) {
                        // if we would need to do anything after the fields were all created, this is where we could do it
                    },

                    // this is the callback when the token response comes back. This callback will only happen if all the inputs are valid. To deal with an invalid input response, we have to use timeoutDuraction, timeoutCallback to find that out.
                    callback: function (tokenResponse) {
                        self.handleTokenResponse(tokenResponse);
                    }
                };

                if (enabledPaymentTypes.includes('card')) {
                    self.$achContainer.hide();
                }
                else {
                    self.$creditCardContainer.hide();
                }

                try {
                    CollectJS.configure(self.collectJSSettings);
                }
                catch (err) {
                    // we'll get a CollectJS doesn't exist, which could happen if the tokenization key is missing or incorrect
                    var $validationMessage = self.$paymentValidation.find('.js-validation-message')
                    $validationMessage.text('Error configuring hosted gateway. This could be due to an invalid or missing Tokenization Key. Please verify that Tokenization Key is configured correctly in gateway settings.');
                    self.$paymentValidation.show();
                    return;
                }

                /* Payment Selector Stuff*/
                //// Credit Card

                if (enabledPaymentTypes.includes('card')) {
                    self.$paymentButtonCreditCard.off().on('click', function () {
                        $(this).removeClass('btn-default').addClass('btn-primary active').siblings().addClass('btn-default').removeClass('btn-primary active');

                        // have CollectJS clear all the input fields when the PaymentType (ach vs cc) changes. This will prevent us sending both ACH and CC payment info at the same time
                        // CollectJS determines to use ACH vs CC by seeing which inputs have data in it. There isn't a explicit option to indicate which to use.
                        CollectJS.clearInputs();

                        self.$selectedPaymentType.val('card');
                        self.$creditCardContainer.show();
                        self.$achContainer.hide();
                        if (self.currencyChangePostbackScript) {
                            window.location = self.currencyChangePostbackScript;
                        }
                    });
                };

                //// ACH
                if (enabledPaymentTypes.includes('ach')) {

                    self.$paymentButtonACH.off().on('click', function () {
                        $(this).removeClass('btn-default').addClass('btn-primary active').siblings().addClass('btn-default').removeClass('btn-primary active');

                        // have CollectJS clear all the input fields when the PaymentType (ach vs cc) changes. This will prevent us sending both ACH and CC payment info at the same time
                        // CollectJS determines to use ACH vs CC by seeing which inputs have data in it. There isn't a explicit option to indicate which to use.
                        CollectJS.clearInputs();
                        self.$selectedPaymentType.val('ach');
                        self.$creditCardContainer.hide();
                        self.$achContainer.show();
                        if (self.currencyChangePostbackScript) {
                            window.location = self.currencyChangePostbackScript;
                        }
                    });
                };

                if ((enabledPaymentTypes.includes('card') == false) && self.$creditCardContainer) {
                    // if the $creditCardContainer was created, but CreditCard isn't enabled, remove it from the DOM
                    self.$creditCardContainer.remove();
                }

                if ((enabledPaymentTypes.includes('ach') == false) && self.$achContainer) {
                    // if the $achContainer was created, but ACH isn't enabled, remove it from the DOM
                    self.$achContainer.remove();
                }

                if (self.$paymentTypeSelector) {
                    if (enabledPaymentTypes.length > 1) {

                        // only show the payment type selector (tabs) if there if both ACH and CC are enabled.
                        self.$paymentTypeSelector.show();
                    }
                    else {
                        self.$paymentTypeSelector.hide();
                    }
                }

                var selectedPaymentTypeVal = self.$selectedPaymentType.val();
                if (selectedPaymentTypeVal == '') {
                    selectedPaymentTypeVal = 'card';
                }

                if (selectedPaymentTypeVal == 'card' && enabledPaymentTypes.includes('card')) {
                    self.$paymentButtonACH.removeClass('active btn-primary').addClass('btn-default');
                    self.$paymentButtonCreditCard.removeClass('btn-default').addClass('btn-primary active');
                    self.$selectedPaymentType.val('card');
                    self.$creditCardContainer.show();
                    self.$achContainer.hide();
                }
                else {
                    self.$paymentButtonCreditCard.removeClass('active btn-primary').addClass('btn-default');
                    self.$paymentButtonACH.removeClass('btn-default').addClass('btn-primary active')
                    self.$selectedPaymentType.val('ach');
                    self.$creditCardContainer.hide();
                    self.$achContainer.show();
                }
            },

            handleTokenResponse: function (tokenResponse) {
                var self = this;

                self.$responseToken.val(tokenResponse.token);
                self.$rawResponseToken.val(JSON.stringify(tokenResponse, null, 2));

                if (self.tokenizerPostbackScript) {
                    self.tokenResponseSent(true);
                    window.location = self.tokenizerPostbackScript;
                }
            },

            // NMIHostedPaymentControl will call this when the 'Next' button is clicked
            // use javascript setTimeout to give validation a little bit of time to check stuff. If that doesn't stop invalid input, CollectJS will do the 'timeoutCallback' event of CollectJS if it finds invalid input after submitting the payment info.
            submitPaymentInfo: function (controlId) {
                var self = this
                setTimeout(function () {
                    self.startSubmitPaymentInfo(self, controlId);
                }, 0);
            },

            validateInputs: function (validationCallback) {

                var self = this;
                // according to https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#cjs_integration_inline3, there will be things with 'CollectJSInvalid' classes if there are any validation errors
                for (var iframeKey in CollectJS.iframes) {
                    var $frameEl = $(CollectJS.iframes[iframeKey]);
                    if ($frameEl.hasClass('CollectJSInvalid') == true) {
                        // if a field has CollectJSInValid, is should be indicated with a red outline, or something similar
                        // NOTE: we might not need this
                    }
                }

                var validationMessage = '';

                // check for both .CollectJSInvalid and also if the fields weren't validated (they are probably blank)
                var hasInvalidFields = $('.CollectJSInvalid').length > 0;

                for (var validationFieldKey in self.validationFieldStatus) {
                    var validationField = self.validationFieldStatus[validationFieldKey];
                    // first check visibility. If this is an ACH field, but we are in CC mode (and vice versa), don't validate
                    var fieldVisible = $(CollectJS.config.fields[validationFieldKey].selector).is(':visible');
                    if (fieldVisible && !validationField.status) {
                        hasInvalidFields = true;
                        var validationFieldTitle = CollectJS.config.fields[validationFieldKey].title;
                        var isBlank = !validationField.message || validationField.message == 'Field is empty'
                        if (isBlank) {
                            validationMessage = validationFieldTitle + ' cannot be blank';

                        }
                        else {
                            validationMessage = validationField.message || 'unknown validation error';
                        }

                        break;
                    }
                }

                if (hasInvalidFields) {
                    var $validationMessage = self.$paymentValidation.find('.js-validation-message')
                    $validationMessage.text(validationMessage);
                    self.$paymentValidation.show();
                    validationCallback({
                        IsValid: false,
                        ValidationMessage: validationMessage
                    });

                    return false;
                }
                else {
                    validationCallback({
                        IsValid: true,
                        ValidationMessage: ''
                    });

                    return true;
                }
            },

            // Tells the gatewayTokenizer to submit the entered info so that we can get a token (or error, etc) in the response
            startSubmitPaymentInfo: function (self, controlId) {
                self.tokenResponseSent(false);
                CollectJS.startPaymentRequest();
            }
        }

        return exports;
    }());
}(jQuery));
