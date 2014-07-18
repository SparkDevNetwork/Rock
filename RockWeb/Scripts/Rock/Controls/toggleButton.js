(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.toggleButton = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                // uses pattern from http://www.bootply.com/92189

                $('#' + options.id + ' .btn-toggle').click(function (e) {

                    e.stopImmediatePropagation();

                    $(this).find('.btn').toggleClass('active');

                    if (options.activeButtonCssClass && $(this).find('.' + options.activeButtonCssClass).size() > 0) {
                        $(this).find('.btn').toggleClass(options.activeButtonCssClass);
                    }
                    
                    if (options.onButtonCssClass) {
                        $(this).find('.js-toggle-on').toggleClass(options.onButtonCssClass);
                    }

                    if (options.offButtonCssClass) {
                        $(this).find('.js-toggle-off').toggleClass(options.offButtonCssClass);
                    }
                    
                    $(this).parent().find('.js-toggle-checked').val($(this).find('.js-toggle-on').hasClass('active'));

                });

            }
        };

        return exports;
    }());
}(jQuery));



