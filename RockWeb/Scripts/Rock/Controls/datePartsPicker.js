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
        var $month = $datePartsPicker.find('.js-month');
        var $day = $datePartsPicker.find('.js-day');
        var $year = $datePartsPicker.find('.js-year');
        var requireYear = $datePartsPicker.attr('data-requireyear') == 'true';
        var allowFuture = $datePartsPicker.attr('data-allowFuture') == 'true';

        var isValid = true;

        if (!allowFuture) {
          if ($month.val() && $day.val() && $year.val()) {
            var monthIndex = $month.val();
            monthIndex--;
            var bDate = new Date($year.val(), monthIndex, $day.val());
            var now = new Date();
            if (bDate > now) {
              isValid = false;
            }
          }
        }

        if (requireYear) {
          if ($month.val() && $day.val() && $year.val()) {
            // month, day and year are all set, it's OK  
          }
          else if (!$month.val() || !$day.val() || !$year.val()) {
            // at least one of them is set, but some are not, so it is invalid
            isValid = false;
          }
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