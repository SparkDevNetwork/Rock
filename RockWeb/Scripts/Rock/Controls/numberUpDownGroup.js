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
        var $quantitites = $numberUpDownGroup.find('.numberincrement-value');
        var required = $numberUpDownGroup.hasClass('required');
        
        
        var isValid = true;
        
        //if ( required == true ) {
            // if required, at least one of the values has to be greater than 0
            //isValid = false;

           // ($numberUpDownGroup).find('.numberincrement-value').each(function (i) {
            //    if (i.va)
            //}


            //if ($quantitites).each(
            //    function (i) {
            //        if (text) {
            //            isValid = false;
            //            validator.errormessage = itemLabelText + " is Required";
            //        }
            //    });


        //}

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