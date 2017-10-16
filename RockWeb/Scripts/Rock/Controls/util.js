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
                } else if (typeof google == 'object' && typeof google.maps == 'object') {
                    this.googleMapsIsLoaded();
                }
            },
          // parses the value as a float and adds a 'px' suffix if successful. If the value can't be parsed, this will return an empty string
          // note: this uses javascript's parseFloat which will convert the the numeric portion value to a float as long as it starts with a numeric value
            getValueAsPixels: function (a)
            {
              var floatValue = parseFloat(a)
              if (floatValue) {
                return floatValue + 'px'
              }
              else {
                return '';
              }
            }
        };

        return exports;

    }());
}(jQuery));
