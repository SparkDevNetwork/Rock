(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.numberUpDown = (function () {

        var exports = {
            adjust: function (btn, adjustment) {

                var $parent = $(btn).closest('.input-group');
                var $min = $parent.find('.js-number-up-down-min').first();
                var $max = $parent.find('.js-number-up-down-max').first();
                var $value = $parent.find('.js-number-up-down-value').first();
                var $lbl = $parent.find('.js-number-up-down-lbl').first();
                var $upBtn = $parent.find('.js-number-up').first();
                var $downBtn = $parent.find('.js-number-down').first();

                // Get the min, max, and new value
                var minValue = parseInt($min.val(), 10);
                var maxValue = parseInt($max.val(), 10);
                var numValue = parseInt($value.val(), 10) + adjustment;

                // If new value is valid, set the hf and lbl
                if (numValue >= minValue && numValue <= maxValue) {
                    $value.val(numValue);
                    $lbl.html(numValue);
                }

                // enable/disable the 'up' button
                if (numValue >= maxValue) {
                    $upBtn.addClass('disabled');
                } else {
                    $upBtn.removeClass('disabled');
                }

                // enable/disable the 'down' button
                if (numValue <= minValue) {
                    $downBtn.addClass('disabled');
                } else {
                    $downBtn.removeClass('disabled');
                }
            }
        };

        return exports;
    }());
}(jQuery));