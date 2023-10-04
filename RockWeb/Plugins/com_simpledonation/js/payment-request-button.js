// needs
// - stripe public key
// - account name
// - total amount
//
// returns
// - token
var initPaymentEntry = function () {
  function doPostBack() {
    __doPostBack('hfStripeToken', 'Token_Complete');
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
          console.log('res', result);
          let paymentRequestButton = paymentButton.children[0];

          if (result && result.applePay == true)  return paymentRequestButton.classList.add('btn-apple-pay');
          if (result && result.googlePay == true) return paymentRequestButton.classList.add('btn-google-pay');

          console.log("APPLE IS A NO :(");
          paymentButton.style.display = 'none';
      });

      paymentButton.addEventListener('click', () => {
          var totalCost = 0;
          var organizationName = $('[id$="hfOrganizationName"]').val();

          // Prompt for Registrar / Pre-fill First Registrant / Use First Registrant
          var firstName = $('[id$="tbYourFirstName"]').val();
          var lastName = $('[id$="tbYourLastName"]').val();
          var emailAddress = $('[id$="tbConfirmationEmail"]').val();

          // Use Logged In Person
          if (firstName == undefined) {
              firstName = $('div[id$="lUseLoggedInPersonFirstName"]').text().replace(/(\r\n|\n|\r|\t)/gm, ""); 
          }

          if (lastName == undefined) {
              lastName = $('div[id$="lUseLoggedInPersonLastName"]').text().replace(/(\r\n|\n|\r|\t)/gm, "");
          }

          if (emailAddress == undefined) {
              emailAddress = $('div[id$="lUseLoggedInPersonEmail"]').text().replace(/(\r\n|\n|\r|\t)/gm, "");
              if (emailAddress == undefined) {
                  emailAddress = $('[id$="tbUseLoggedInPersonEmail"]').val();
              }
          }      

          if (firstName.length > 0 && lastName.length > 0 && emailAddress.length > 0) {
              if ($('.amount-to-pay input').length > 0) {
                  amount = $('.amount-to-pay input').val();
              }
              else {
                  amount = $('[id$="hfTotalCost"]').val();
              }

              amount = amount * 100

              if ($('input[id$="CoverFees"]').is(':checked')) {
                  var coverAmount = parseFloat($('[id$="hfFeeAmount"]').val()) * 100;
                  amount = amount + coverAmount
              }

              console.log('totalCost', amount)

              paymentRequest.update({
                  total: {
                      label: organizationName,
                      amount: amount
                  }
              });
              paymentRequest.show();
          }
          else {
              console.log('Missing First Name, Last Name, or Email');
          }


      });

      paymentRequest.on('token', function (ev) {
          console.log('tokenEvent', ev);
          ev.complete('success');
          $('[id$="hfStripeToken"]').val(ev.token.id);
          $('[id$="hfWalletName"]').val(ev.walletName);
          $('[id$="hfPostbackFromModal"]').val("True");
          $('#updateProgress').show();

          doPostBack();
      });
    }

};

$(initPaymentEntry);
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initPaymentEntry);
