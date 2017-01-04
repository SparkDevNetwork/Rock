(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.mediaPlayer = (function () {
        var exports = {
            initialize: function () {

                Sys.Application.add_load(function () {
                    var cssFile = Rock.settings.get('baseUrl') + 'Scripts/mediaelementjs/mediaelementplayer.min.css';
                    var jsFile = Rock.settings.get('baseUrl') + 'Scripts/mediaelementjs/mediaelement-and-player.js';

                    // ensure that css for mediaelementplayers is added to page
                    if (!$('#mediaElementCss').length) {
                        $('head').append("<link id='mediaElementCss' href='" + cssFile + "' type='text/css' rel='stylesheet' />");
                    }

                    // ensure that js for mediaelementplayers is added to page
                    if (!$('#mediaElementJs').length) {
                        // by default, jquery adds a cache-busting parameter on dynamically added script tags. set the ajaxSetup cache:true to prevent this
                        $.ajaxSetup({ cache: true });
                        $('head').prepend("<script id='mediaElementJs' src='" + jsFile + "' />");
                    }
                    
                    // ensure that mediaelementplayer is applied to all the the rock audio/video that was generated from a Rock Video/Audio FieldType (js-media-audio,js-media-video)
                    $('audio.js-media-audio,video.js-media-video').mediaelementplayer({ enableAutosize: true });
                });

            }
        };

        return exports;
    }());
}(jQuery));