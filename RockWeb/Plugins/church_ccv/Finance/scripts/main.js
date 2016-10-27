var autocomplete
var formMapping = {
  locality: 'city',
  administrative_area_level_1: 'state',
  postal_code: 'zip'
}

function initAutocomplete() {
  autocomplete = new google.maps.places.Autocomplete(
    $('.js-street-input')[0],
    {
      types: ['geocode']
    }
  )
  autocomplete.addListener('place_changed', fillInAddress)
}

function fillInAddress() {
  var place = autocomplete.getPlace()
  giveForm.address.street = place.name
  for (var i = 0; i < place.address_components.length; i++) {
    var type = place.address_components[i].types[0]
    if (formMapping[type]) {
      var val = place.address_components[i]['short_name']
      giveForm.address[formMapping[type]] = val
    }
  }
}

function capitalizeEachWord(str) {
  return str.replace(/\w\S*/g, function (txt) {
    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
  })
}

function getInitialData() {
  var today = moment().format('YYYY-MM-DD')
  return {
    amount: '',
    repeating: false,
    firstGift: today,
    schedule: 'biweekly',
    firstName: '',
    lastName: '',
    showSplitNameField: false,
    email: '',
    fund: '',
    card: {
      number: '',
      exp: '',
      cvc: ''
    },
    phone: '',
    address: {
      street: '',
      city: '',
      state: '',
      zip: ''
    },
    givingFunds: givingFunds
  }
}

var giveForm = new Vue({
  el: '#givingForm',
  data: getInitialData,
  computed: {
    fullName: function () {
      return (this.firstName + ' ' + this.lastName).trim()
    },
    todaysDate: function () {
      return moment().format('YYYY-MM-DD')
    },
    tomorrowsDate: function () {
      return moment().add(1, 'days').format('YYYY-MM-DD')
    }
  },
  watch: {
    'fullName': function (val) {
      // Manually trigger update event for card display
      var event = document.createEvent('Event');
      event.initEvent('change', true, true)
      $('#givingForm .js-hf-fullname')[0].dispatchEvent(event)
    },
  },
  methods: {

    splitFullName: function () {
      var name = $('#givingForm .js-input-fullname').val().split(' ').map(capitalizeEachWord)
      this.firstName = name.splice(0, 1)[0]
      this.lastName = name.join(' ')
      this.refreshNameSplit()
    },

    refreshNameSplit: function () {
      this.showSplitNameField = (this.lastName.length > 0)
      $('#givingForm .js-input-fullname').toggle(!this.showSplitNameField);
      $('#givingForm .js-splitname-inputs').toggle(this.showSplitNameField);
    },

    resetData: function () {
      this.$data = getInitialData()
      this.refreshNameSplit()
      this.$nextTick(function () {
        // Trigger card graphic update manually
        $('.js-card-group input').each(function () {
          var event = document.createEvent('Event');
          event.initEvent('keyup', true, true)
          // Need to send backspace to card number to trigger card type reset,
          // other fields only require a 'change' event
          event.keyCode = 8; // Backspace
          this.dispatchEvent(event)
        })
        // Reset the bootstrap-switch
        $('.js-repeating-toggle').bootstrapSwitch('state', this.repeating)
      })
    }
  }
})

Sys.Application.add_load(function () {
  giveForm.refreshNameSplit();
  var isMobile = {
    Windows: function () {
      return /IEMobile/i.test(navigator.userAgent);
    },
    Android: function () {
      return /Android/i.test(navigator.userAgent);
    },
    BlackBerry: function () {
      return /BlackBerry/i.test(navigator.userAgent);
    },
    iOS: function () {
      return /iPhone|iPad|iPod/i.test(navigator.userAgent);
    },
    any: function () {
      return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Windows());
    }
  }

  if (isMobile.iOS()) {
    $('#givingForm .js-amount').attr('pattern', '[0-9]*')
    $('#givingForm .js-amount').inputmask({
      mask: '9{*}.99',
      numericInput: true,
    })
  } else if (isMobile.Android()) {
    $('#givingForm .js-amount').attr('type', 'number')
  } else {
    $('#givingForm .js-amount').inputmask({
      rightAlign: false,
      groupSeparator: ",",
      alias: "numeric",
      placeholder: "0",
      autoGroup: true,
      digits: 2,
      digitsOptional: false,
      clearMaskOnLostFocus: false
    })
  }

  $('.js-phone').inputmask({
    mask: ['(999) 999-9999', '+1 (999) 999-9999'],
    greedy: false
  })

  var cardType

  // cleanup card if is was already initialized (in case we are in an ajax response)
  if ($('form').data('card')) {
    var card = $('form').data('card');
    cardType = card.cardType;
    $('form').data('card', null);
    card = null;
    delete card;
  }

  if ($('#givingForm .js-card-graphic-holder').length) {

    var card = $('form').data('card');
    if (!card) {
      $('form').card({
        container: '#givingForm .js-card-graphic-holder',
        formSelectors: {
          numberInput: '#givingForm .cardinput-number',
          expiryInput: '#givingForm .cardinput-exp',
          cvcInput: '#givingForm .cardinput-cvc',
          nameInput: '#givingForm .js-hf-fullname'
        }
      })


      // if this is an async postback, we need to ensure that the card display is showing the values
      card = $('form').data('card');
      card.cardType = cardType;

      if ($(card.$numberInput).val()) {
        $(card.$numberDisplay).html($(card.$numberInput).val());

        // update the display of the card
        $('#givingForm .jp-card').addClass('jp-card-' + card.cardType + ' jp-card-identified')
      }

      if ($(card.$nameInput).val()) {
        $(card.$nameDisplay).html($(card.$nameInput).val());
      }

      if ($(card.$cvcInput).val()) {
        $(card.$cvcDisplay).html($(card.$cvcInput).val());
      }

      if ($(card.$expiryInput).val()) {
        $(card.$expiryDisplay).html($(card.$expiryInput).val());
      }
    }
  }

  $('#givingForm .js-repeating-toggle').bootstrapSwitch({
    onColor: 'success',
    onSwitchChange: function (event, state) {
      giveForm.repeating = state

      if (state) {
        $('#givingForm .js-repeating-options').slideDown();
      } else {
        $('#givingForm .js-repeating-options').slideUp();
      }
    }
  })

  // manually do .blur on js-lastname and js-input-fullname since some of it uses asp.net runat=server
  $('#givingForm').on('blur', '.js-input-fullname', function () {
    giveForm.splitFullName()
  });

  $('#givingForm').on('blur', '.js-lastname', function () {
    giveForm.refreshNameSplit()
  });

  if (giveForm.repeating) {
    $('#givingForm .js-repeating-options').show();
  }

  if (Modernizr.inputtypes.date == false) {
    $('#givingForm .js-firstgift').datepicker({
      format: 'yyyy-mm-dd',
      startDate: new Date(),
      todayHighlight: true,
      todayBtn: 'linked',
      autoclose: true
    })
  }
})
