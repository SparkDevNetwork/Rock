(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};

  Rock.controls.dateRangePicker = (function ()
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
        var $dateRangePicker = $(validator).closest('.js-daterangepicker');
        var lowerValue = $dateRangePicker.find('.js-lower').data('datepicker');
        var upperValue = $dateRangePicker.find('.js-upper').data('datepicker');
        var required = $dateRangePicker.attr('data-required') == 'true';
        var itemLabelText = $dateRangePicker.attr('data-itemlabel');

        var isValid = true;

        if (required) {
            // if required, then make sure that the date range has a start and/or end date (can't both be blank)
            if (lowerValue.dates.length == 0 && upperValue.dates.length == 0) {
                isValid = false;
                validator.errormessage = itemLabelText + " is Required";
            }
        }

        var control = $dateRangePicker
        if (isValid)
        {
          control.removeClass('has-error');
        }
        else
        {
          control.addClass('has-error');
        }

        args.IsValid = isValid;
      }
    };

    return exports;
  }());
}(jQuery));