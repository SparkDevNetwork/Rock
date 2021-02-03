function initializeTokenizer(controlId) {
    var $control = $('#' + controlId);

    if ($control.length == 0) {
        // control hasn't been rendered so skip
        return;
    }
    var tokenizerPostbackScript = $control.attr('data-tokenizer-postback-script');
    var currencyChangePostbackScript = $control.attr('data-currencychange-postback-script');

    var enabledPaymentTypes = JSON.parse($('.js-enabled-payment-types', $control).val());;

    var $creditCardContainer = $('.js-gateway-creditcard-iframe-container', $control);
    var $achContainer = $('.js-gateway-ach-iframe-container', $control);
    var $selectedPaymentType = $('.js-selected-payment-type', $control);
    var $paymentButtonACH = $control.find('.js-payment-ach');
    var $paymentButtonCreditCard = $control.find('.js-payment-creditcard');

    var containerStyles = function (style) {
        return $creditCardContainer.css(style);
    };
    var inputStyles = function (style) {
        return $('.js-input-style-hook').css(style)
    };

    var pubApiKey = $('.js-public-api-key', $control).val();
    var gatewayUrl = $('.js-gateway-url', $control).val();

    var tokenizerBaseSettings = {
        apikey: pubApiKey,
        url: gatewayUrl, // ensures that it uses the specified url
        container: $creditCardContainer[0],
        submission: (resp) => {
            $('.js-response-token', $control).val(resp.token);
            $('.js-tokenizer-raw-response', $control).val(JSON.stringify(resp, null, 2));

            if (tokenizerPostbackScript) {
                window.location = tokenizerPostbackScript;
            }
        },
        settings: {
            payment: {
                types: enabledPaymentTypes,
                ach: {
                    sec_code: 'web' // Default web - web, ccd, ppd, tel
                }
            },
            // Styles object will get converted into a css style sheet.
            // Inspect elements to see structured html elements
            // and style them the same way you would in css.
            styles: {
                'body': {
                    'color': containerStyles('color')
                },
                '#app': {
                    'padding': '5px 15px'
                },
                'input,select': {
                    'color': inputStyles('color'),
                    'border-radius': inputStyles('border-radius'),
                    'background-color': inputStyles('background-color'),
                    'border': inputStyles('border'),
                    'box-shadow': inputStyles('box-shadow'),
                    'padding': inputStyles('padding'),
                    'font-size': inputStyles('font-size'),
                    'height': inputStyles('height'),
                    'font-family': inputStyles('font-family'),
                },
                'input:focus,select:focus': {
                    'border': getComputedStyle(document.documentElement).getPropertyValue('--focus-state-border'),
                    'box-shadow': getComputedStyle(document.documentElement).getPropertyValue('--focus-state-shadow')
                },
                'select': {
                    'padding': '6px 4px'
                },
                '.fieldsetrow': {
                    'margin-left': '-2.5px',
                    'margin-right': '-2.5px'
                },
                '.card > .fieldset': {
                    'padding': '0 !important',
                    'margin': '0 2.5px 5px !important'
                },
                'input[type=number]::-webkit-inner-spin-button,input[type=number]::-webkit-outer-spin-button': {
                    '-webkit-appearance': 'none',
                    'margin': '0'
                }
            }
        }
    };

    // NOTE: the MyWell Tokenizer supports doing both ACH and CC in the same tokenizer, but we want to have two tokenizers for each so that we can take care of the toggling between them in the UI


    //// Credit Card
    if (enabledPaymentTypes.includes('card')) {
        // create MyWell Gateway Tokenizer object for CreditCard (from example on https://sandbox.gotnpgateway.com/docs/tokenizer/)
        var tokenizerCreditCardSettings = $.extend(true, {}, tokenizerBaseSettings);
        tokenizerCreditCardSettings.container = $creditCardContainer[0];
        tokenizerCreditCardSettings.settings.payment.types = ['card'];
        var creditCardGatewayTokenizer = new Tokenizer(tokenizerCreditCardSettings);

        // Initiate creation on container element
        creditCardGatewayTokenizer.create();
        $creditCardContainer.data('gatewayTokenizer', creditCardGatewayTokenizer);

        $paymentButtonCreditCard.off().on('click', function () {
            $(this).removeClass('btn-default').addClass('btn-primary active').siblings().addClass('btn-default').removeClass('btn-primary active');
            $selectedPaymentType.val('card');
            $creditCardContainer.show();
            $achContainer.hide();
            if (currencyChangePostbackScript) {
                window.location = currencyChangePostbackScript;
            }
        });
    };


    //// ACH
    if (enabledPaymentTypes.includes('ach')) {
        // create MyWell Gateway Tokenizer object for ACH (from example on https://sandbox.gotnpgateway.com/docs/tokenizer/)
        var tokenizerACHSettings = $.extend(true, {}, tokenizerBaseSettings);
        tokenizerACHSettings.container = $achContainer[0];
        tokenizerACHSettings.settings.payment.types = ['ach'];
        var achGatewayTokenizer = new Tokenizer(tokenizerACHSettings);

        // Initiate creation on container element
        achGatewayTokenizer.create();
        $achContainer.data('gatewayTokenizer', achGatewayTokenizer);

        $paymentButtonACH.off().on('click', function () {
            $(this).removeClass('btn-default').addClass('btn-primary active').siblings().addClass('btn-default').removeClass('btn-primary active');
            $selectedPaymentType.val('ach');
            $creditCardContainer.hide();
            $achContainer.show();
            if (currencyChangePostbackScript) {
                window.location = currencyChangePostbackScript;
            }
        });
    };
    
    var $paymentTypeSelector = $control.find('.js-gateway-paymenttype-selector');
    if (enabledPaymentTypes.length > 1) {
        $paymentTypeSelector.show();
    }
    else {
        $paymentTypeSelector.hide();
    }

    var selectedPaymentTypeVal = $selectedPaymentType.val();
    if (selectedPaymentTypeVal == '') {
        selectedPaymentTypeVal = 'card';
    }

    if (selectedPaymentTypeVal == 'card' && enabledPaymentTypes.includes('card')) {
        $paymentButtonACH.removeClass('active btn-primary').addClass('btn-default');
        $paymentButtonCreditCard.removeClass('btn-default').addClass('btn-primary active');
        $selectedPaymentType.val('card');
        $creditCardContainer.show();
        $achContainer.hide();
    }
    else {
        $paymentButtonCreditCard.removeClass('active btn-primary').addClass('btn-default');
        $paymentButtonACH.removeClass('btn-default').addClass('btn-primary active')

        $selectedPaymentType.val('ach');
        $creditCardContainer.hide();
        $achContainer.show();
    }

}

// Tells the gatewayTokenizer to submit the entered info so that we can get a token (or error, etc) in the response
function submitTokenizer(controlId) {
    var $control = $('#' + controlId)

    var $creditCardContainer = $('.js-gateway-creditcard-iframe-container', $control);
    var $achContainer = $('.js-gateway-ach-iframe-container', $control);

    var gatewayTokenizer;
    if ($achContainer.is(':visible')) {
        gatewayTokenizer = $achContainer.data('gatewayTokenizer');
    }
    else {
        gatewayTokenizer = $creditCardContainer.data('gatewayTokenizer');
    }

    gatewayTokenizer.submit() // Use submission callback to deal with response
}
