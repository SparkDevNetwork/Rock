// needs
// - stripe public key
// - account name
// - total amount
//
// returns
// - token
var initPaymentEntry = function () {
  function handleTokenEvent(stripeToken, walletName) {
      $('[id$="hfStripeToken"]').val(stripeToken);
      $('[id$="hfWalletName"]').val(walletName);
      $('[id$="hfPostbackFromModal"]').val("PaymentButtonClicked");
      $(":submit").click();
  }

  var publicKey = $('[id$="hfPublicKey"]').val();
  var paymentButton = document.getElementById('payment-request-button');
  if (paymentButton != null && publicKey != null) {
      var stripe = Stripe(publicKey, { apiVersion: "2016-07-06" });
      var paymentRequest = stripe.paymentRequest({
          country: 'US',
          currency: 'usd',
          total: {
              label: 'Church',
              amount: 0,
          },
          requestPayerName: true,
          requestPayerEmail: true,
      });

      paymentRequest.canMakePayment().then(function (result) {
          let paymentRequestButton = paymentButton.children[0];
          if (result && result.applePay == true)  return paymentRequestButton.classList.add('btn-apple-pay');
          if (result && result.googlePay == true) return paymentRequestButton.classList.add('btn-google-pay');
          paymentButton.style.display = 'none';
      });

      paymentButton.addEventListener('click', () => {
            var organizationName = $('[id$="hfOrganizationName"]').val();
            var amount = $('[id$="hfTotalCost"]').val();
            amount = amount * 100
            paymentRequest.update({
                total: {
                    label: organizationName,
                    amount: amount
                }
            });

          paymentRequest.show();
      });

      paymentRequest.on('token', function (ev) {
          console.log('tokenEvent', ev);
          ev.complete('success');
          handleTokenEvent(ev.token.id, ev.walletName);
      });
    }

};

$(initPaymentEntry);
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initPaymentEntry);
