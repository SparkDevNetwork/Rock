(function () {
  window.SimpleDonation = window.SimpleDonation || {};
  SimpleDonation.finance = SimpleDonation || {};

  var selectors = {
    cardNumber: '.js-credit-card-number',
    cvv: '.js-cvv',
    expMonth: '.js-exp-date',
    expYear: '.js-exp-date',
    nextButton: '.js-submit-button',
    firstName: '.js-card-first-name',
    lastName: '.js-card-last-name',
    gatewayErrors: '.js-gateway-errors',
    paymentInfoToken: '.js-response-token',
    routing: '[id$="txtRoutingNumber"]',
    bankAccount: '[id$="txtAccountNumber"]',
    giveAs: '[id$="tglGiveAsOption_hfChecked"]',
    zipCode: '[id$="tbPostalCode"]',
    bankAccountContainer: '[id$="divACHPaymentInfo"]',
    creditCardContainer: '[id$="divCCPaymentInfo"]'
  };

  function processCard() {
    var stripeCardData = {
      name: getFullName(),
      number: $(selectors.cardNumber).val(),
      cvc: $(selectors.cvv).val(),
      exp_month: $(selectors.expMonth).val(),
      exp_year: $(selectors.expYear).val(),
      address_zip: $(selectors.zipCode).val()
    };

    Stripe.card.createToken(stripeCardData, handleStripeResponse);
  }

  function processAch() {
    var stripeBankData = {
      account_number: $(selectors.bankAccount).val(),
      routing_number: $(selectors.routing).val(),
      country: 'US',
      currency: 'USD',
      account_holder_name: getAccountHolderName(),
      account_holder_type: getGiverType()
    };

    Stripe.bankAccount.createToken(stripeBankData, handleStripeResponse);
  }

  function handleStripeResponse(status, response) {
    if (response.error) {
      $(selectors.gatewayErrors).html(
        '<div class="alert alert-danger">' +
        '<a class="close" data-dismiss="alert" href="#">&times;</a>' +
          '<ul><li>' + response.error.message + '</li></ul>' +
        '</div>'
      );
    } else {
      $(selectors.paymentInfoToken).val(response.id);
      doPostBack();
    }
  }

  function doPostBack() {
    __doPostBack('hfStripeToken', 'Token_Complete');
  }

  function getFullName() {
    return $(selectors.firstName).val() + ' ' + $(selectors.lastName).val()
  }

  function getAccountHolderName() {
    // Is this a person or business? True if person
    if ($(selectors.giveAs).val() == 'True') {
      var currentName = $('[id$="txtCurrentName"]').text().trim();

      // Have they already entered a name?
      if (currentName.length === 0) {
        return getFullName();

      }

      return currentName;
    }

    return $('[id$="txtBusinessName"]').val();
  }

  function getGiverType() {
    if ($(selectors.giveAs).val() === 'True') {
      return 'individual';
    }

    return 'company';
  }

  function handleNextClick(e) {
    e.preventDefault();

    if ($(selectors.creditCardContainer).is(':visible')) {// && $('[id$="hfPaymentTab"]').val() == 'CreditCard') {
      if (isCardValid()) {
        processCard();
      }
    } else if ($(selectors.bankAccountContainer).is(':visible')) {
      if (isBankAccountValid()) {
        processAch();
      }
    } else {
      doPostBack();
    }
  }

  function initializeValidation() {
    $(selectors.cardNumber).payment('formatCardNumber');
    $(selectors.cvv).payment('formatCardCVC');

    $(selectors.bankAccount).payment('restrictNumeric');
    $(selectors.routing).payment('restrictNumeric');

    $(selectors.zipCode).payment('restrictNumeric');
  }

  function isCardValid() {
    var cardNumber = $(selectors.cardNumber).val();
    var isValidCardNumber = $.payment.validateCardNumber(cardNumber);
    var cardType = $.payment.cardType(cardNumber);
    var isValidExpiration = $.payment.validateCardExpiry($(selectors.expMonth).val(), $(selectors.expYear).val())
    var isValidCvv = $.payment.validateCardCVC($(selectors.cvv).val(), cardType);

    var errorString = "";
    if (!isValidCardNumber) {
        errorString += "<li>Please enter a valid card number</li>";
    }
    if (!isValidExpiration) {
        errorString += "<li>Please enter a valid expiration date</li>";
    }
    if (!isValidCvv) {
        errorString += "<li>Please enter a valid CVV</li>";
    }

    if (errorString != "") {
        $('#gateway_errors').html(
            '<div class="alert alert-danger">' +
            '<a class="close" data-dismiss="alert" href="#">&times;</a><ul>'
            + errorString +
            '</ul></div>');
    }

    return isValidCardNumber && isValidExpiration && isValidCvv;
  }

  function isBankAccountValid() {
    var isValidAccountNumber = $(selectors.bankAccount).val().length > 0;
    var isValidRoutingNumber = $(selectors.routing).val().length === 9;

    var errorString = "";
    if (!isValidAccountNumber) {
        errorString += "<li>Please enter a valid account number</li>";
    }
    if (!isValidRoutingNumber) {
        errorString += "<li>Please enter a valid routing number</li>";
    }

    if (errorString != "") {
        $('#gateway_errors').html(
            '<div class="alert alert-danger">' +
            '<a class="close" data-dismiss="alert" href="#">&times;</a><ul>'
            + errorString +
            '</ul></div>');
    }

    return isValidAccountNumber && isValidRoutingNumber;
  }

  SimpleDonation.finance.stripeTokenTransactionEntry = {
    init: function (key) {
      Stripe.setPublishableKey(key);

      $(selectors.nextButton).on('click', handleNextClick);
      initializeValidation();
    }
  };
}());
