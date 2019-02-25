(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};

  Rock.controls.datePartsPicker = (function ()
  {
    var exports = {
      initialize: function (options)
      {
        if (!options.id) {
          throw 'id is required';
        }

      },
      clientValidate: function (validator, args)
      {
        var $datePartsPicker = $(validator).closest('.js-datepartspicker');
        var monthNumber = Number($datePartsPicker.find('.js-month').val());
        var dayNumber = Number($datePartsPicker.find('.js-day').val());
        var yearNumber = Number($datePartsPicker.find('.js-year').val());
        var required = $datePartsPicker.attr('data-required') == 'true';
        var requireYear = $datePartsPicker.attr('data-requireyear') == 'true';
        var allowFuture = $datePartsPicker.attr('data-allowFuture') == 'true';
        var itemLabelText = $datePartsPicker.attr('data-itemlabel');
        
        var isValid = true;

        if (!allowFuture) {
          if (monthNumber && dayNumber && yearNumber) {
            // NOTE: Javascript Date Contructor's month parameter is zero based!
            // see http://stackoverflow.com/questions/2552483/why-does-the-month-argument-range-from-0-to-11-in-javascripts-date-constructor
            var bDate = new Date(yearNumber, monthNumber-1, dayNumber);
            var now = new Date();
            if (bDate > now) {
              isValid = false;
              validator.errormessage = itemLabelText + ' cannot be a future date.'
            }
          }
        }

        if (monthNumber && dayNumber && (yearNumber || !requireYear)) {
          // month, day and (conditionally) year are all set, it's OK
        }
        else if (monthNumber || dayNumber || yearNumber) {
          // at least one of them is set, but some are not, so it is invalid
          isValid = false;
          validator.errormessage = itemLabelText + ' must be a valid value.';
        }
        else if (required) {
          // nothing is set but it is a required field.
          isValid = false;
          validator.errormessage = itemLabelText + ' is required.';
        }

        var control = $datePartsPicker
        if (isValid) {
          control.removeClass('has-error');
        } else {
          control.addClass('has-error');
        }

        args.IsValid = isValid;
      }
    };

    return exports;
  }());
}(jQuery));
