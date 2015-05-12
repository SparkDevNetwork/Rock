(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.bootstrapButton = (function () {

        var exports = {
            showLoading: function (btn) {

                if (typeof (Page_ClientValidate) == 'function') {

                    if (Page_IsValid) {
                        // make sure page really is valid
                        Page_ClientValidate();
                    }
                }

                if (Page_IsValid) {
                    $(btn).button('loading');
                }

                return true;
            }
        };

        return exports;
    }());
}(jQuery));