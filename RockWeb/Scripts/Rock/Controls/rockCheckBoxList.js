(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.rockCheckBoxList = (function () {

        var exports = {
            clientValidate: function (validator, args) {
                var $checkBoxList = $(validator).closest('.js-rockcheckboxlist');

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


