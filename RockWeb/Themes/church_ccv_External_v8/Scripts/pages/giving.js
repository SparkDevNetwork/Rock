///
///  Giving Page
/// --------------------------------------------------

// Bind payment success components after postback
function pageLoad()
{

  //
  // Transaction Panel
  //

  // Set style of whichever radio button is checked
  $('#rblAccountType').on('change', function () {
    $('input[type="radio"]:checked').parents('label').addClass('btn-primary');
    $('input[type="radio"]:not(:checked)').parents('label').removeClass('btn-primary');
  });

  // Disable processing submit button after its clicked to prevent duplicate submits
  $('#btnConfirmNext').on('click', function ()
  {
    $('#btnConfirmNext').attr('disabled', 'disabled');
  });

  // Disable progress buttons if they are not in a complete state
  // Not sure why, but disabling at the ASP control level breaks javascript...which is why its here
  if (!$('#btnProgressAmount').hasClass( 'complete' )) {
    $('#btnProgressAmount').attr('disabled', 'disabled');
  }

  if (!$('#btnProgressPerson').hasClass('complete'))
  {
    $('#btnProgressPerson').attr('disabled', 'disabled');
  }

  if (!$('#btnProgressPayment').hasClass('complete'))
  {
    $('#btnProgressPayment').attr('disabled', 'disabled');
  }

  // Validate amount input
  $('#nbAmount').on('input', function ()
  {

    // Remove white space and split at the decimal
    var amount = this.value.replace(/\s/gi, '').split('.');

    // return string
    var formattedAmount = '';

    // format first element of array
    // remove non number characters, keep only 7 numbers, and then format with ,
    formattedAmount = formattedAmount + amount[0].replace(/[^0-9.]/g, "").substring(0, 7).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,").trim();

    // if array more than one element, format the second element with 1 or 2 characters and ignore the rest
    if (amount.length > 1)
    {
      formattedAmount = formattedAmount + '.' + amount[1].replace(/[^0-9]/g, "").substring(0, 2).trim();
    }

    // set the new value
    this.value = '$' + formattedAmount;

    if (!formattedAmount)
    {
      $(this).parents('div.amount-wrapper').addClass('has-error');
    } else
    {
      $(this).parents('div.amount-wrapper').removeClass('has-error');
    }
  });

  // Validate accounts input
  $('#ddlAccounts').on('input', function ()
  {
    var fund = $(this).find(':selected').val();

    if ((!fund || fund === '-1'))
    {
      $(this).parents('div.accounts-wrapper').addClass('has-error');
    } else
    {
      $(this).parents('div.accounts-wrapper').removeClass('has-error');
    }
  });

  // Validate email input
  $('#tbEmail').on('input', function ()
  {
    if (!/^\w([\.-]?\w)*@\w([\.-]?\w)*(\.\w{2,15})+$/.test($(this).val()))
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      $(this).parents('div.form-group').removeClass('has-error');
    }
  });

  // Validate generic text input
  $('.required').on('input', function ()
  {
    if (!this.value)
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      $(this).parents('div.form-group').removeClass('has-error');
    }
  });

  // Validate / Format Credit card input
  $('#nbCreditCard').on('input', function ()
  {
    // Remove spaces from value for validation
    var creditCardNumber = this.value.replace(/[^0-9]/g, '');

    // Test expressions for various credit cards
    var visaRegEx = /^(?:4[0-9]{12}(?:[0-9]{3})?)$/;
    var mastercardRegEx = /^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$/;
    var amexpRegEx = /^(?:3[47][0-9]{13})$/;
    var discovRegEx = /^(?:6(?:011|5[0-9][0-9])[0-9]{12})$/;

    var isValid = false;
    // Test credit card number against each card type
    if (visaRegEx.test(creditCardNumber))
    {
      isValid = true;
    } else if (mastercardRegEx.test(creditCardNumber))
    {
      isValid = true;
    } else if (amexpRegEx.test(creditCardNumber))
    {
      isValid = true;
    } else if (discovRegEx.test(creditCardNumber))
    {
      isValid = true;
    }

    // Reformat number with spaces
    this.value = creditCardNumber.replace(/[^0-9]/g, "").replace(/\W/gi, '').replace(/(.{4})/g, '$1 ').trim();

    // Update display class for valid or invalid input
    if (!isValid)
    {
      $(this).parents('div.creditcard-wrapper').addClass('has-error');
    } else
    {
      $(this).parents('div.creditcard-wrapper').removeClass('has-error');
    }
  });

  // Validate experiation month input
  $('#monthDropDownList').on('input', function ()
  {
    if (!$(this).find(':selected').val())
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      if ($('#yearDropDownList_').find(':selected').val())
      {
        $(this).parents('div.form-group').removeClass('has-error');
      }
    }
  });

  // Validate experation year input
  $('#yearDropDownList_').on('input', function ()
  {
    if (!$(this).find(':selected').val())
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      if ($('#monthDropDownList').find(':selected').val())
      {
        $(this).parents('div.form-group').removeClass('has-error');
      }
    }
  });

  // Validate numbers only fields
  $('.numbers-only').on('input', function ()
  {
    this.value = this.value.replace(/[^0-9]/g, "")

    if (this.value === '')
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      $(this).parents('div.form-group').removeClass('has-error');
    }
  });

  // Validate account type radio button list
  $('#rblAccountType').on('click', function ()
  {
    if ($('#rblAccountType input[type=radio]:checked').val())
    {
      $('#rblAccountType').parents('div.form-group').removeClass('has-error');
    } else
    {
      $('#rblAccountType').parents('div.form-group').addClass('has-error');
    }
  });

  // Validate saved payment account radio button list
  $('#rblSavedPaymentAccounts').on('click', function ()
  {
    if ($('#rblSavedPaymentAccounts input[type=radio]:checked').val())
    {
      $('#rblSavedPaymentAccounts').removeClass('has-error');
    } else
    {
      $('#rblSavedPaymentAccounts').addClass('has-error');
    }
  })


  //
  // Success Panel
  //

  // Save Payment Account Panel
  $('#tbSavePaymentAccountName').on('input', function ()
  {
    // check if input has value
    if (!$(this).val())
    {
      $(this).parents('div.form-group').addClass('has-error');
    } else
    {
      $(this).parents('div.form-group').removeClass('has-error');
    }

    // enable / disable save button
    setInputSaveButtonState();
  });

  // Schedule Recurring Transaction Panel

  // configure the schedule start date picker
  var currentDate = new Date();
  currentDate.setDate(currentDate.getDate() + 1)

  $('#dpScheduleStartDate').datepicker({
    format: 'mm/dd/yyyy',
    startDate: currentDate
  });

  // Validate schedule drop down list
  $('#ddlScheduleFrequency').on('click', function ()
  {
    if ($(this).find(':selected').val() && $(this).find(':selected').val() !== '-1')
    {
      $(this).parents('div.form-group').removeClass('has-error');
    } else
    {
      $(this).parents('div.form-group').addClass('has-error');
    }

    // enable / disable save button
    setInputSaveButtonState();
  });

  // Validate schedule start date picker
  $('#dpScheduleStartDate').on('change', function ()
  {
    if (/^02\/(?:[01]\d|2\d)\/(?:0[048]|[13579][26]|[2468][048])|(?:0[13578]|10|12)\/(?:[0-2]\d|3[01])\/\d{2}|(?:0[469]|11)\/(?:[0-2]\d|30)\/\d{2}|02\/(?:[0-1]\d|2[0-8])\/\d{2}$/.test(this.value))
    {
      $('#dpScheduleStartDate').parents('div.input-group').removeClass('has-error');
    } else
    {
      $('#dpScheduleStartDate').parents('div.input-group').addClass('has-error');
    }

    // enable / disable save button
    setInputSaveButtonState();
  });

  // show/hide save button for success input form
  $('.toggle-input-form').on('click', function ()
  {
    if ($('#tglSavePaymentAccount').is(':checked') || $('#tglScheduleTransaction').is(':checked'))
    {
      $('#btnSaveSuccessInputForm').removeClass('hidden');
    } else
    {
      $('#btnSaveSuccessInputForm').addClass('hidden');
    }

    // enable / disable save button
    setInputSaveButtonState();
  });
};

//
// Event Methods
// 

// Next button onClick
btnNext_OnClick = function (targetPanel)
{
  if (targetPanel === 'pnlPerson')
  {
    // check if amount panel is completed
    if (validateAmountFormFields())
    {
      // hide Amount Show Person panels
      togglePanel('#pnlAmount', false);
      togglePanel('#pnlPerson', true);

      // update progress bar
      toggleProgressIndicator('#btnProgressPerson', true, false);
      toggleProgressIndicator('#btnProgressAmount', true, true);

      // Enable button for Amount Progress Indicator
      $('#btnProgressAmount').removeAttr('disabled');
    }
  } else if (targetPanel === 'pnlPayment')
  {
    // check if person panel is completed
    if (validatePersonFormFields())
    {
      // hide Person Show Payment panels
      togglePanel('#pnlPerson', false);
      togglePanel('#pnlPayment', true);

      // update progress bar
      toggleProgressIndicator('#btnProgressPayment', true, false);
      toggleProgressIndicator('#btnProgressPerson', true, true);

      // Enable button for Person Progress Indicator
      $('#btnProgressPerson').removeAttr('disabled');
    }
  } else if (targetPanel === 'pnlConfirm')
  {
    // check if payment fields are populated
    if (validatePaymentFormFields())
    {
      populateConfirmFields();

      // hide Payment show Confirm panels
      togglePanel('#pnlPayment', false);
      togglePanel('#pnlConfirm', true);

      // update progress bar
      toggleProgressIndicator('#btnProgressConfirm', true, false);
      toggleProgressIndicator('#btnProgressPayment', true, true);

      // Enable button for Payment Progress Indicator
      $('#btnProgressPayment').removeAttr('disabled');
    }
  }
}

// Back button onClick
btnBack_OnClick = function (targetPanel)
{
  clearErrorFormatting();

  if (targetPanel === 'pnlAmount')
  {
    // hide Person show Amount panels
    togglePanel('#pnlPerson', false);
    togglePanel('#pnlAmount', true);

    // update progress bar
    toggleProgressIndicator('#btnProgressPerson', false, false);
    toggleProgressIndicator('#btnProgressAmount', true, false);

    // Disable amount progress indicator button
    $('#btnProgressAmount').attr('disabled', 'disabled');
  } else if (targetPanel === 'pnlPerson')
  {
    // hide Payment show Person panels
    togglePanel('#pnlPayment', false);
    togglePanel('#pnlPerson', true);

    // update progress bar
    toggleProgressIndicator('#btnProgressPayment', false, false);
    toggleProgressIndicator('#btnProgressPerson', true, false);

    // Disable person progress indicator button
    $('#btnProgressPerson').attr('disabled', 'disabled');
  } else if (targetPanel === 'pnlPayment')
  {
    // hide Confirm show Payment panels
    togglePanel('#pnlConfirm', false);
    togglePanel('#pnlPayment', true);

    // update progress bar
    toggleProgressIndicator('#btnProgressConfirm', false, false);
    toggleProgressIndicator('#btnProgressPayment', true, false);

    // Disable payment progress indicator button
    $('#btnProgressPayment').attr('disabled', 'disabled');
  }
}

// Progress Indicator Button Click
btnProgress_OnClick = function (targetPanel)
{
  if (targetPanel === 'pnlAmount')
  {
    // show Amount panel / hide others
    togglePanel('#pnlConfirm', false);
    togglePanel('#pnlPayment', false);
    togglePanel('#pnlPerson', false);
    togglePanel('#pnlAmount', true);

    // set progress indicators
    toggleProgressIndicator('#btnProgressConfirm', false, false);
    toggleProgressIndicator('#btnProgressPayment', false, false);
    toggleProgressIndicator('#btnProgressPerson', false, false);
    toggleProgressIndicator('#btnProgressAmount', true, false);

    // enable / disable progress buttons
    $('#btnProgressAmount').attr('disabled', 'disabled');
    $('#btnProgressPerson').attr('disabled', 'disabled');
    $('#btnProgressPayment').attr('disabled', 'disabled');
  } else if (targetPanel === 'pnlPerson')
  {
    // show Person panel / hide others
    togglePanel('#pnlAmount', false);
    togglePanel('#pnlConfirm', false);
    togglePanel('#pnlPayment', false);
    togglePanel('#pnlPerson', true);

    // set progress indicators
    toggleProgressIndicator('#btnProgressAmount', true, true);
    toggleProgressIndicator('#btnProgressConfirm', false, false);
    toggleProgressIndicator('#btnProgressPayment', false, false);
    toggleProgressIndicator('#btnProgressPerson', true, false);

    // enable / disable progress buttons
    $('#btnProgressAmount').removeAttr('disabled');;
    $('#btnProgressPerson').attr('disabled', 'disabled');
    $('#btnProgressPayment').attr('disabled', 'disabled');
  } else if (targetPanel === 'pnlPayment')
  {
    // show Payment panel / hide others
    togglePanel('#pnlPerson', false);
    togglePanel('#pnlAmount', false);
    togglePanel('#pnlConfirm', false);
    togglePanel('#pnlPayment', true);

    // set progress indicators
    toggleProgressIndicator('#btnProgressPerson', true, true);
    toggleProgressIndicator('#btnProgressAmount', true, true);
    toggleProgressIndicator('#btnProgressConfirm', false, false);
    toggleProgressIndicator('#btnProgressPayment', true, false);

    // enable / disable progress buttons
    $('#btnProgressAmount').removeAttr('disabled');;
    $('#btnProgressPerson').removeAttr('disabled');;
    $('#btnProgressPayment').attr('disabled', 'disabled');
  }
}

// Credit Card payment type on onClick
btnCreditCard_OnClick = function ()
{
  clearErrorFormatting();

  // update payment type hidden field
  $('#hfPaymentType').val('CC');

  // hide Bank Account or Saved Payment panel show Credit Card panel and update button selected state
  togglePanel('#pnlBankAccount', false);
  togglePanel('#pnlSavedPayment', false);
  togglePanel('#pnlCreditCard', true);

  toggleButtonSelectedState('#btnBankAccount', false);
  toggleButtonSelectedState('#btnSavedPayment', false);
  toggleButtonSelectedState('#btnCreditCard', true);
}

// ACH payment type onClick
btnBankAccount_OnClick = function ()
{
  clearErrorFormatting();

  // update payment type hidden field
  $('#hfPaymentType').val('ACH');

  // hide Credit Card or Saved Payment panel show Bank Account panel and update button class
  togglePanel('#pnlCreditCard', false);
  togglePanel('#pnlSavedPayment', false);
  togglePanel('#pnlBankAccount', true);

  toggleButtonSelectedState('#btnCreditCard', false);
  toggleButtonSelectedState('#btnSavedPayment', false);
  toggleButtonSelectedState('#btnBankAccount', true);
}

// Saved Payment type onClick
btnSavedPayment_OnClick = function ()
{
  clearErrorFormatting();

  // update payment type hidden field
  $('#hfPaymentType').val('REF');

  // hide Credit Card or Bank Account panel show Saved Payment panel and update button class
  togglePanel('#pnlCreditCard', false);
  togglePanel('#pnlBankAccount', false);
  togglePanel('#pnlSavedPayment', true);

  toggleButtonSelectedState('#btnCreditCard', false);
  toggleButtonSelectedState('#btnBankAccount', false);
  toggleButtonSelectedState('#btnSavedPayment', true);
}


populateConfirmFields = function ()
{
  // populate confirm fields
  $('#confirmGiftAmount').html($('#nbAmount').val());

  if ($('#pnlCreditCard').hasClass('hidden') && $('#pnlSavedPayment').hasClass('hidden'))
  {
    // populate bank account info
    $('#accountType').html('Bank Account');
    $('#personName').html($('#tbFirstName').val() + ' ' + $('#tbLastName').val());
    $('#confirmAccountNumber').html($('#nbAccountNumber').val());
    $('#savedAccountName').html($('#rblAccountType input[type=radio]:checked').val());

  } else if ($('#pnlCreditCard').hasClass('hidden') && $('#pnlBankAccount').hasClass('hidden'))
  {
    // create name and account number
    var savedAccount = $('#ddlSavedPaymentAccounts').find(':selected').text().replace('Use ', '').split('(');
    var savedAccountNumber = savedAccount[1].replace(')', '');

    // populate saved account info
    $('#accountType').html('Saved Account');
    $('#personName').html($('#tbFirstName').val() + ' ' + $('#tbLastName').val());
    $('#confirmAccountNumber').html('**** **** **** ' + savedAccountNumber.substr(savedAccountNumber.length - 4));
    $('#savedAccountName').html(savedAccount[0]);
  } else
  {
    // populate credit card account info
    // Populate Credit Card type
    var visaRegEx = /^(?:4[0-9]{12}(?:[0-9]{3})?)$/;
    var mastercardRegEx = /^(?:5[1-5][0-9]{14})$/;
    var amexpRegEx = /^(?:3[47][0-9]{13})$/;
    var discovRegEx = /^(?:6(?:011|5[0-9][0-9])[0-9]{12})$/;
    if (visaRegEx.test($('#nbCreditCard').val().replace(/[^0-9]/g, '')))
    {
      $('#accountType').html('Visa');
    } else if (mastercardRegEx.test($('#nbCreditCard').val().replace(/[^0-9]/g, '')))
    {
      $('#accountType').html('Master Card');
    } else if (amexpRegEx.test($('#nbCreditCard').val().replace(/[^0-9]/g, '')))
    {
      $('#accountType').html('American Express');
    } else if (discovRegEx.test($('#nbCreditCard').val().replace(/[^0-9]/g, '')))
    {
      $('#accountType').html('Discover');
    }

    // populate name
    $('#personName').html($('#tbName').val());

    // populate credit card and mask number
    $('#confirmAccountNumber').html('**** **** **** ' + $('#nbCreditCard').val().substr($('#nbCreditCard').length - 5));
  }
}

//
// Validation Methods
//

// Verify Amount Fields are populated
validateAmountFormFields = function ()
{
  var amount = $('#nbAmount').val();
  var fund = $('#ddlAccounts').find(':selected').val();

  if ((amount && amount !== '$') && (fund && fund !== '-1') && $('.has-error').length === 0)
  {
    $('#nbHTMLMessage').addClass('hidden');
    return true;
  } else
  {
    // highlight fields not ready
    if ((!amount || amount === '$'))
    {
      $('#nbAmount').parents('div.amount-wrapper').addClass('has-error');
    }

    if ((!fund || fund === '-1'))
    {
      $('#ddlAccounts').parents('div.accounts-wrapper').addClass('has-error');
    }

    displayMessage('Please correct errors and try again.', 'danger');
    return false;
  }

  return false;
}

// Validate if person fields are populated
validatePersonFormFields = function ()
{
  var firstName = $('#tbFirstName').val();
  var lastName = $('#tbLastName').val();
  var email = $('#tbEmail').val();

  if (firstName && lastName && email && $('.has-error').length === 0)
  {
    $('#nbHTMLMessage').addClass('hidden');
    return true;
  } else
  {
    // highlight missing fields
    if (!firstName)
    {
      $('#tbFirstName').parents('div.form-group').addClass('has-error');
    }
    if (!lastName)
    {
      $('#tbLastName').parents('div.form-group').addClass('has-error');
    }
    if (!email)
    {
      $('#tbEmail').parents('div.form-group').addClass('has-error');
    }

    displayMessage('Please correct errors and try again.', 'danger');
    return false;
  }

  return false;
}

// Validate if payment fields are populated
validatePaymentFormFields = function ()
{
  if ($('#pnlCreditCard').hasClass('hidden') && $('#pnlSavedPayment').hasClass('hidden'))
  {
    return validateBankAccountFields();
  } else if ($('#pnlBankAccount').hasClass('hidden') && $('#pnlSavedPayment').hasClass('hidden'))
  {
    return validateCreditCardFields();
  } else
  {
    return validateSavedPaymentFields();
  }
  // default false if checks fail to process
  return false;
}

// validate bank account fields
validateBankAccountFields = function ()
{
  // Check Bank Account Fields
  // build array of elements
  var elementArray = [];

  var routingNumber = $('#nbRoutingNumber');
  var accountNumber = $('#nbAccountNumber');
  elementArray.push(routingNumber);
  elementArray.push(accountNumber);

  // check each element
  elementArray.forEach(checkForValue);

  // check if account type has value
  if ($('#rblAccountType input[type=radio]:checked').val())
  {
    $('#rblAccountType').parents('div.form-group').removeClass('has-error');
  } else
  {
    $('#rblAccountType').parents('div.form-group').addClass('has-error');
  }

  // check page for any errors
  if ($('.has-error').length === 0)
  {
    // no errorrs
    $('#nbHTMLMessage').addClass('hidden');
    return true;
  } else
  {
    // page has errors, return false
    displayMessage('Please correct errors and try again.', 'danger');
    return false;
  }
}

// validate credit card fields
validateCreditCardFields = function ()
{
  // Check Credit Card Account Fields
  var name = $('#tbName').val();
  var creditCard = $('#nbCreditCard').val();
  var expDateMonth = $('#monthDropDownList').find(':selected').val();
  var expDateYear = $('#yearDropDownList_').find(':selected').val();
  var CVV = $('#nbCVV').val();
  var street = $('#tbStreet').val();
  var city = $('#tbCity').val();
  var state = $('#ddlState').find(':selected').val();
  var country = $('#ddlCountry').find(':selected').val();
  var postalCode = $('#nbPostalCode').val();

  if (name && creditCard && expDateMonth && expDateYear && CVV && street && city && state && country && postalCode && $('.has-error').length === 0)
  {
    $('#nbHTMLMessage').addClass('hidden');
    return true;
  } else
  {
    // highlight fields not ready
    if (!name)
    {
      $('#tbName').parents('div.form-group').addClass('has-error');
    }

    if (!creditCard)
    {
      $('#nbCreditCard').parents('div.creditcard-wrapper').addClass('has-error');
    }

    if (!expDateMonth)
    {
      $('#monthDropDownList').parents('div.form-group').addClass('has-error');
    }

    if (!expDateYear)
    {
      $('#yearDropDownList_').parents('div.form-group').addClass('has-error');
    }

    if (!CVV)
    {
      $('#nbCVV').parents('div.form-group').addClass('has-error');
    }

    if (!street)
    {
      $('#tbStreet').parents('div.form-group').addClass('has-error');
    }

    if (!city)
    {
      $('#tbCity').parents('div.form-group').addClass('has-error');
    }

    if (!state)
    {
      $('#ddlState').parents('div.form-group').addClass('has-error');
    }

    if (!country)
    {
      $('#ddlCountry').parents('div.form-group').addClass('has-error');
    }

    if (!postalCode)
    {
      $('#nbPostalCode').parents('div.form-group').addClass('has-error');
    }

    displayMessage('Please correct errors and try again.', 'danger');
    return false;
  }
}

// validate saved payment fields
validateSavedPaymentFields = function ()
{
  // check if payment account has selected value
  if ($('#ddlSavedPaymentAccounts').find(':selected').val())
  {
    $('#ddlSavedPaymentAccounts').removeClass('has-error');
  } else
  {
    $('#ddlSavedPaymentAccounts').addClass('has-error');
  }

  // check page for any errors
  if ($('.has-error').length === 0)
  {
    // no errorrs
    $('#nbHTMLMessage').addClass('hidden');
    return true;
  } else
  {
    // page has errors, return false
    displayMessage('Please correct errors and try again.', 'danger');
    return false;
  }
}

// Validate success panel input form
validateSuccessInputForm = function ()
{
  var ready = false;

  // check if save payment account is toggled
  if ($('#tglSavePaymentAccount').is(':checked'))
  {
    // check if its fields have values
    if ($('#tbSavePaymentAccountName').val())
    {
      ready = true;
    } else
    {
      return false;
    }
  }

  // check if schedule input is toggled
  if ($('#tglScheduleTransaction').is(':checked'))
  {
    // check if its fields have values
    if ($('#ddlScheduleFrequency').find(':selected').val() && $('#ddlScheduleFrequency').find(':selected').val() !== '-1' && /^02\/(?:[01]\d|2\d)\/(?:0[048]|[13579][26]|[2468][048])|(?:0[13578]|10|12)\/(?:[0-2]\d|3[01])\/\d{2}|(?:0[469]|11)\/(?:[0-2]\d|30)\/\d{2}|02\/(?:[0-1]\d|2[0-8])\/\d{2}$/.test($('#dpScheduleStartDate').val()))
    {
      // ready for save
      ready = true;
    } else
    {
      return false;
    }
  }

  return ready;
}

//
//  Helper Methods
//

// Display message in notification box
displayMessage = function (message, alert)
{
  // get notification box
  var nb = $('#nbHTMLMessage');

  // set message
  nb.html(message);

  // show notification box and set alert type
  nb.addClass(getAlertClass(alert));
  nb.removeClass('hidden');
}

// clear any errors and hide notifiation box if shown
clearErrorFormatting = function ()
{
  $('.has-error').removeClass('has-error');
  $('#nbHTMLMessage').addClass('hidden');
}

// return CSS class for alert type
getAlertClass = function (alert)
{
  switch (alert)
  {
    case 'primary': {
      return 'alert-primary';
    };
    case 'secondary': {
      return 'alert-secondary';
    }
    case 'success': {
      return 'alert-success';
    }
    case 'warning': {
      return 'alert-warning';
    }
    case 'info': {
      return 'alert-info';
    }
    case 'light': {
      return 'alert-light';
    }
    case 'dark': {
      return 'alert-dark';
    }
    default: {
      return 'alert-danger';
    }
  }
}

// toggle panel visibility
togglePanel = function (name, show)
{
  if (show === true)
  {
    $(name).removeClass('hidden');
  } else
  {
    $(name).addClass('hidden');
  }
}

// toggle progress indicator
toggleProgressIndicator = function (name, active, complete)
{
  // set active state
  if (active === true)
  {
    $(name).addClass('active');
  } else
  {
    $(name).removeClass('active');
  }

  // set complete state
  if (complete === true)
  {
    $(name).addClass('complete');
  } else
  {
    $(name).removeClass('complete');
  }
}

// toggle button selected state
toggleButtonSelectedState = function (name, selected)
{
  if (selected === true)
  {
    $(name).addClass('btn-primary');
  } else
  {
    $(name).removeClass('btn-primary');
  }
}

// check if element has value and set error state
checkForValue = function (item, index)
{
  if (!item.val())
  {
    $(item).parents('div.form-group').addClass('has-error');
  } else
  {
    $(item).parents('div.form-group').removeClass('has-error');
  }
}

// evaluate enable/disable save button on success input form
setInputSaveButtonState = function ()
{
  // enable / disable save button
  if (validateSuccessInputForm())
  {
    // enable save button
    $('#btnSaveSuccessInputForm').removeAttr('disabled');
    $('#btnSaveSuccessInputForm').removeClass('aspNetDisabled');
  } else
  {
    // disable save button
    $('#btnSaveSuccessInputForm').attr('disabled', 'disabled');
    $('#btnSaveSuccessInputForm').addClass('aspNetDisabled');
  }
}