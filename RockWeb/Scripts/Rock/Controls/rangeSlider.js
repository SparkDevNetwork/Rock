(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.rangeSlider = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.controlId) {
                    throw 'controlId is required';
                }

                Sys.Application.add_load(function () {
                    var cssFile = Rock.settings.get('baseUrl') + 'Scripts/ion.rangeSlider/css/ion.rangeSlider.Rock.css';
                    var jsFile = Rock.settings.get('baseUrl') + 'Scripts/ion.rangeSlider/js/ion-rangeSlider/ion.rangeSlider.min.js';

                    // ensure that css for rangeSlider is added to page
                    if (!$('#rangeSliderCss').length) {
                        $('head').append("<link id='rangeSliderCss' href='" + cssFile + "' type='text/css' rel='stylesheet' />");
                    }
                });

                // use https://github.com/IonDen/ion.rangeSlider/ to make a slider
                // see https://github.com/IonDen/ion.rangeSlider/#settings for settings
                $('#' + options.controlId).ionRangeSlider({
                    type: options.type || 'single', // Choose slider type, could be 'single' for one handle, or 'double' for two handles
                    min: options.min || 0,
                    max: options.max || 100,
                    from: options.from || null, // for 'single' the position of the slider. for 'double' the lower position of the selected range
                    to: options.to || null // if 'double' the upper position of the selected range
                });

            }
        };

        return exports;
    }());
}(jQuery));