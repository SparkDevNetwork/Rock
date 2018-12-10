(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};

  Rock.controls.numberUpDownGroup = (function ()
  {
    var exports = {
      clientValidate: function (validator, args)
      {
        var $numberUpDownGroup = $(validator).closest('.number-up-down-group');
        var isValid = true;

        if ($numberUpDownGroup.hasClass('required') == true) {
            isValid = false;

            $numberUpDownGroup.find('.numberincrement-value').each(function (i) {
                if (parseInt(this.outerText, 10) > 0) {
                    isValid = true;
                }
            });
        }

        if (isValid == false) {
            validator.errormessage = $numberUpDownGroup.find('label').text() + " is required";
        }

        var control = $numberUpDownGroup
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
