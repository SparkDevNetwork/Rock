function initializeTokenizer(controlId) {

    var $control = $('#' + controlId);

    if ($control.length == 0) {
        // control hasn't been rendered so skip
        return;
    }
    var postbackScript = $control.attr('data-postback-script');

    var enabledPaymentTypes = JSON.parse($('.js-enabled-payment-types', $control).val());;

    var $creditCardContainer = $('.js-gateway-creditcard-iframe-container', $control);
    var $achContainer = $('.js-gateway-ach-iframe-container', $control);

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

            if (postbackScript) {
                window.location = postbackScript;
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
                'input': {
                    'color': inputStyles('color'),
                    'border-radius': inputStyles('border-radius'),
                    'background-color': inputStyles('background-color'),
                    'border': inputStyles('border')
                },
                '.payment .cvv input': {
                    'border': inputStyles('border'),
                    'padding-left': inputStyles('padding-left')
                }
            }
        }
    };

    // NOTE: the PI Tokenizer supports doing both ACH and CC in the same tokenizer, but we want to have two tokenizers for each so that we can take care of the toggling between them in the UI


    //// Credit Card
    if (enabledPaymentTypes.includes('card')) {
        // create PI Gateway Tokenizer object for CreditCard (from example on https://sandbox.gotnpgateway.com/docs/tokenizer/)
        var tokenizerCreditCardSettings = $.extend(true, {}, tokenizerBaseSettings);
        tokenizerCreditCardSettings.container = $creditCardContainer[0];
        tokenizerCreditCardSettings.settings.payment.types = ['card'];
        var creditCardGatewayTokenizer = new Tokenizer(tokenizerCreditCardSettings);

        // Initiate creation on container element
        creditCardGatewayTokenizer.create();
        $creditCardContainer.data('gatewayTokenizer', creditCardGatewayTokenizer);

        var $paymentButtonCreditCard = $control.find('.js-payment-creditcard');

        $paymentButtonCreditCard.off().on('click', function () {
            $creditCardContainer.show();
            $achContainer.hide();
        });
    };


    //// ACH
    if (enabledPaymentTypes.includes('ach')) {
        // create PI Gateway Tokenizer object for ACH (from example on https://sandbox.gotnpgateway.com/docs/tokenizer/)
        var tokenizerACHSettings = $.extend(true, {}, tokenizerBaseSettings);
        tokenizerACHSettings.container = $achContainer[0];
        tokenizerACHSettings.settings.payment.types = ['ach'];
        var achGatewayTokenizer = new Tokenizer(tokenizerACHSettings);

        // Initiate creation on container element
        achGatewayTokenizer.create();
        $achContainer.data('gatewayTokenizer', achGatewayTokenizer);

        var $paymentButtonACH = $control.find('.js-payment-ach');
        $paymentButtonACH.off().on('click', function () {
            $creditCardContainer.hide();
            $achContainer.show();
        });
    };

    var $paymentTypeSelector = $control.find('.js-gateway-paymenttype-selector');
    if (enabledPaymentTypes.length > 1) {

        $paymentTypeSelector.show();
    }
    else {
        $paymentTypeSelector.hide();
    }

    // show default payment type
    if (enabledPaymentTypes.includes('card')) {
        $creditCardContainer.show();
        $achContainer.hide();
    }
    else {
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
