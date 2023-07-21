(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.mediaSelector = (function () {

        var exports = {
            clientValidate: function (validator, args) {
                var $checkBoxList = $(validator).closest('.js-mediaselector');

                var checkboxes = $checkBoxList.find('input');
                var isValid = false;
                for (var i = 0; i < checkboxes.length; i++) {
                    if (checkboxes[i].checked) {
                        isValid = true;
                        break;
                    }
                }

                if (isValid) {
                    $checkBoxList.removeClass('has-error');
                } else {
                    $checkBoxList.addClass('has-error');
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));


