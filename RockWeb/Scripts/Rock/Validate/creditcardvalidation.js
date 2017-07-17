// Detect credit card type
Sys.Application.add_load(function () {{
  $('.js-numeric').payment('restrictNumeric');
  $('.cc-number').payment('formatCardNumber');
  $('.cc-cvc').payment('formatCardCVC');

  $.fn.toggleInputError = function(erred) {
      this.closest('.form-group').toggleClass('has-error', erred);
      return this;
  };

  var cardType = false,
      cardNum = $('.cc-number').val().replace(/\s/g,'');

  // TODO Create accurate method for determining form is in validation state.
  if (cardNum.length == 16){
    $('.cc-number').toggleInputError(!$.payment.validateCardNumber(cardNum));
    var cardType = $.payment.cardType(cardNum);
    
    if (cardType != null) {
      $('.cc-cvc').toggleInputError(!$.payment.validateCardCVC($('.cc-cvc').val(), cardType));
      $('.card-logos').addClass('is-' + cardType);
    }

    if (!$.payment.validateCardExpiry($('.cc-exp .js-month').val(), $('.cc-exp .js-year').val())) {
      $('.cc-exp').addClass('has-error');
    } else {
      $('.cc-exp').removeClass('has-error');
    }
  }

  $('.cc-number').bind('change keyup input paste', function() {
      var cardNum = $('.cc-number').val().replace(/\s/g,'');
      var cardType = $.payment.cardType(cardNum);
      if (cardType != null) {
          $('.card-logos').removeClass('is-visa is-mastercard is-amex is-discover');
          $('.card-logos').addClass('is-' + cardType);
      } else {
          $('.card-logos').removeClass('is-visa is-mastercard is-amex is-discover');
      }
      if (cardNum.length == 16){
        $('.cc-number').toggleInputError(!$.payment.validateCardNumber(cardNum));
      }
  });

  $('form').submit(function(e) {
      //e.preventDefault();
      var cardType = $.payment.cardType(cardNum);
      $('.cc-number').toggleInputError(!$.payment.validateCardNumber(cardNum));
      $('.cc-cvc').toggleInputError(!$.payment.validateCardCVC($('.cc-cvc').val(), cardType));

      if (!$.payment.validateCardExpiry($('.cc-exp .js-month').val(), $('.cc-exp .js-year').val())) {
        $('.cc-exp').addClass('has-error');
      } else {
        $('.cc-exp').removeClass('has-error');
      }
  });

}});
