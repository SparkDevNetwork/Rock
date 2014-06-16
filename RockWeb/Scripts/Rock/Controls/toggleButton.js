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

                $('#' + options.id).click(function () {
                    $(this).find('.btn').toggleClass('active');

                    if ($(this).find('.' + options.activeButtonCssClass).size() > 0) {
                        $(this).find('.btn').toggleClass(options.activeButtonCssClass);
                    }

                    $(this).find('.btn').toggleClass('btn-default');
                    
                    $(this).find('.js-toggle-checked').val($(this).find('.js-toggle-on').hasClass('active'));
                });

            }
        };

        return exports;
    }());
}(jQuery));



