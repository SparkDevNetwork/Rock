(function () {
  window.SimpleDonation = window.SimpleDonation || {};
  SimpleDonation.finance = SimpleDonation || {};

  function calculateOffsetAmount(amount) {
    if (amount === 0) {
      return 0;
    }

    var achRate = parseInt($('[id$="hfAchRate"]').val(), 10);
    var cardRate = parseFloat($('[id$="hfCardRate"]').val(), 10);
    var capAch = $('[id$="hfCapAch"]').val().toLowerCase() === 'true';
    var offsetAmount = ((amount + 50) / (1 - cardRate)) - amount;

    if (capAch && (offsetAmount > achRate) && isAch()) {
      offsetAmount = achRate;
    }

    return offsetAmount;
  }

  function sumTotals(accountSelector) {
    var totalAmount = 0;

    $(accountSelector).find('.form-control').each(function () {
      totalAmount += parseInt(this.value * 100, 10);
    });

    return totalAmount;
  }

  function isCoveringFees(paymentSelector) {
    var paymentContainer = $(paymentSelector);
    var coverFeesCheckbox = paymentContainer.find('.cover-fees');

    return coverFeesCheckbox.is(':checked');
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

  function calculateTotal(paymentSelector, accountSelector) {
    var amount = sumTotals(accountSelector);
    var offsetAmount = calculateOffsetAmount(amount);
    var paymentContainer = $(paymentSelector);
    var total = isCoveringFees(paymentSelector) ? amount + offsetAmount : amount;

    total = Math.round(total) / 100;
    total = total.toFixed(2);
    offsetAmount = Math.round(offsetAmount) / 100;
    offsetAmount = offsetAmount.toFixed(2);

    paymentContainer
      .find('.payment-type')
      .text(!isAch() ? 'credit card ' : '')

    paymentContainer
      .find('.total-amount')
      .text('$' + total);

    paymentContainer
      .find('.payment-offset')
      .text('$' + offsetAmount);

    paymentContainer
      .find('[id$="hfFeeAmount"]')
      .val(offsetAmount)
  }

  SimpleDonation.finance.coverFees = {
    init: function (paymentSelector, accountSelector, keypadSelector, organizationName) {
      var coverFeesCheckbox = $(paymentSelector).find('.cover-fees');
      var formControls = $(accountSelector).find('.form-control');

      var tooltip = '&nbsp;<i class="fa fa-info-circle" data-toggle="tooltip" title="" data-original-title="Every donation has a transaction fee deducted - by checking this option you&apos;ll cover that fee and ' + organizationName + ' will get 100% of your original donation amount."></i>';
      coverFeesCheckbox.parent().append(tooltip);
      $('[data-toggle="tooltip"]').tooltip();

      var calculate = function () {
        calculateTotal(paymentSelector, accountSelector);
      };

      $('[data-toggle="pill"]').on('click', function () {
        setTimeout(calculate);
      });

      coverFeesCheckbox.on('change', calculate);
      $('input[id*="rblSavedAccount"]').on('change', calculate);

      formControls
        .on('input', calculate)
        .on('blur', calculate);

      if (keypadSelector != null) {
        $(keypadSelector).on('click', function () {
          setTimeout(calculate);
        });
      }

      calculate();
    }
  };
}());
