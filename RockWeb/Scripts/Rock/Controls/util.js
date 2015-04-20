(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.util = (function () {
        var exports = {
            googleMapsIsLoaded: function () {
                // trigger googleMapsIsLoaded so that controls (like the geoPicker) know about it
                $(window).trigger('googleMapsIsLoaded');
            },
            loadGoogleMapsApi: function (apiSrc) {
                // ensure that js for googleMapsApi is added to page
                if (!$('#googleMapsApi').length) {
                    // by default, jquery adds a cache-busting parameter on dynamically added script tags. set the ajaxSetup cache:true to prevent this
                    $.ajaxSetup({ cache: true });
                    var src = apiSrc + '&callback=Rock.controls.util.googleMapsIsLoaded';
                    $('head').prepend("<script id='googleMapsApi' src='" + src + "' />");
                }
            }
        };

        return exports;

    }());
}(jQuery));
