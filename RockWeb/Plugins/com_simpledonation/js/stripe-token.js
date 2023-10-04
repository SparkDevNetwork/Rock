(function () {
  window.SimpleDonation = window.SimpleDonation || {};
  SimpleDonation.finance = SimpleDonation || {};

  var swipeInfo = null;

  var createSwipeInfo = function () {
    return {
      token: null,
      firstName: null,
      lastName: null
    };
  };

  var handleStripeResponse = function (status, response) {
    swipeInfo.token = response.id;
    $('#hfSwipe').val(JSON.stringify(swipeInfo));
    __doPostBack('hfSwipe', 'Swipe_Complete');
  };

  var handleSwipeSuccess = function (swipeData) {
    var firstName = swipeData.firstName;
    var lastName = swipeData.lastName;

    firstName = firstName[0].toUpperCase() + firstName.substring(1).toLowerCase();
    lastName = lastName[0].toUpperCase() + lastName.substring(1).toLowerCase();

    swipeInfo.firstName = firstName;
    swipeInfo.lastName = lastName;

    var payload = {
      name: swipeData.firstName + ' ' + swipeData.lastName,
      number: swipeData.account,
      exp_month: swipeData.expMonth,
      exp_year: swipeData.expYear,
    };

    Stripe.card.createToken(payload, handleStripeResponse);
  };

  SimpleDonation.finance.stripeToken = {
    init: function (key) {
      Stripe.setPublishableKey(key);
      swipeInfo = createSwipeInfo();

      $.cardswipe({
        firstLineOnly: false,
        success: handleSwipeSuccess,
        parsers: ["visa", "mastercard", "amex", "discover", "generic"],
        error: function () {}
      });
    }
  };
}());

