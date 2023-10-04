var initPaymentEntry = function () {
  function calculateFee(amount) {
    if (amount === 0) {
      return 0;
    }

    var fAmount = parseFloat(amount);
    var achRate = parseInt($('[id$="hfAchRate"]').val(), 10) / 100;
    var cardRate = parseFloat($('[id$="hfCardRate"]').val(), 10);
    var capAch = $('[id$="hfCapAch"]').val().toLowerCase() === 'true';
    var offsetAmount = ((fAmount + 0.50) / (1 - cardRate)) - fAmount;

      if (capAch && (offsetAmount > achRate) && isAch()) {
          offsetAmount = achRate;
      }

    return offsetAmount;
    }

    function isAch() {
        var savedAccountId = $('input[id*="rblSavedAccount"]:checked').val();
        var savedAccounts = $('[id$="hfSavedAccounts"]').val();
        var savedAccountsArray = savedAccounts ? JSON.parse(savedAccounts) : [];

        if (savedAccountId && savedAccountId !== '0') {
            var selectedAccount = savedAccountsArray.find(function (account) {
                return account.Id === parseInt(savedAccountId, 10);
            });

            if (selectedAccount != null) {
                return selectedAccount.DataCurrency === 'ACH';
            }
        }

        return $('[id$="hfPaymentTab"]').val() === 'ACH';
    }

  function updateText(amount) {
    var newAmount = amount.toFixed(2);
    console.log("Fee Amount: " + newAmount);
    $('.css-cover-parent > .control-label').text("Cover the $" + newAmount + " in processing fees");
    $('[id$="hfFeeAmount"]').val(newAmount);

    var total = $('[id$="hfTotalCost"]').val();
    var previouslyPaid = $('[id$="hfPreviouslyPaid"]').val();
    var payingAmount = $('.amount-to-pay input').val();

    var previouslyPaidNumeric = 0.0;
    if (previouslyPaid != undefined) {
      previouslyPaidNumeric = parseFloat(previouslyPaid);
    }

    var payingAmountNumeric = 0.0;
    if (payingAmount != undefined) {
      payingAmountNumeric = parseFloat(payingAmount);
    }

    var remainingAmount = (total - (previouslyPaidNumeric + payingAmountNumeric) ).toFixed(2);
    $('[id$="lRemainingDue"]').text( "$" + remainingAmount );
  }

  $('.amount-to-pay input').change(function () {
    var amount = calculateFee( $(this).val() );
    updateText(amount);
  });

  // Launching it once for POSTBACK Values
  var amount = 0;
  if ( $('.amount-to-pay input').length > 0 ) {
    amount = calculateFee($('.amount-to-pay input').val());
  }
  else {
    amount = calculateFee($('[id$="hfTotalCost"]').val());
  }

  updateText(amount);
};

$(initPaymentEntry);
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initPaymentEntry);
