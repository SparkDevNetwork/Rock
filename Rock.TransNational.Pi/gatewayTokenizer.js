function initializeTokenizer(controlId) {

    var $control = $('#' + controlId);

    if ($control.length == 0) {
        // control hasn't been rendered so skip
        return;
    }
    var postbackScript = $control.attr('data-postback-script');

    var enabledPaymentTypes = JSON.parse($('.js-enabled-payment-types', $control).val());;

    var $container = $('.js-gateway-iframe-container', $control);

    var containerStyles = function (style) {
        return $container.css(style);
    };
    var inputStyles = function (style) {
        return $('.js-input-style-hook').css(style)
    };

    var pubApiKey = $('.js-public-api-key', $control).val();
    var gatewayUrl = $('.js-gateway-url', $control).val();

    // create PI Gateway Tokenizer object (from example on https://sandbox.gotnpgateway.com/docs/tokenizer/)
    gatewayTokenizer = new Tokenizer({
        apikey: pubApiKey,
        url: gatewayUrl, // ensures that it uses the specified url
        container: $container[0],
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
    })

    // Initiate creation on container element
    gatewayTokenizer.create();

    $control.data('gatewayTokenizer', gatewayTokenizer);
}

// Tells the gatewayTokenizer to submit the entered info so that we can get a token (or error, etc) in the response
function submitTokenizer(controlId) {
    var $control = $('#' + controlId)
    var gatewayTokenizer = $control.data('gatewayTokenizer');
    gatewayTokenizer.submit() // Use submission callback to deal with response
}
