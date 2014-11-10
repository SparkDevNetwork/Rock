(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.mediaPlayer = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var cssFile = Rock.settings.get('baseUrl') + 'Scripts/mediaelementjs/mediaelementplayer.min.css';

                // ensure that css for mediaelementplayers is added to page
                if (!$('#mediaElementCss').length) {
                    $('head').append("<link id='mediaElementCss' href='" + cssFile + "' type='text/css' rel='stylesheet' />");
                }

                $('#' + options.id + '').mediaelementplayer();

            }
        };

        return exports;
    }());
}(jQuery));