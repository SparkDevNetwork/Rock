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

    Rock.controls.mediaplayer = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw "id is required";
                }

                if (!options.playerId) {
                    throw "playerId is required";
                }

                if (!options.progressId) {
                    throw "progressId is required";
                }

                var $playerElement = $("#" + options.id);
                var playerOptions = $playerElement.data("player-options");

                var player = new Rock.UI.MediaPlayer("#" + options.playerId, playerOptions);

                player.on("ready", function () {
                    $("#" + options.progressId).val(player.percentWatched);
                });

                player.on("progress", function () {
                    console.log("progress", player.percentWatched, options.progressId);
                    $("#" + options.progressId).val(player.percentWatched);
                });

                $playerElement
                    .data("required", options.required || false)
                    .data("progress-id", options.progressId)
                    .data("required-percentage", options.requiredPercentage || 0)
                    .data("required-error-message", options.requiredErrorMessage || "You must watch the video.");
            },

            clientValidate: function (validator, args) {
                var $playerElement = $(validator).closest(".js-media-player");
                var $formGroup = $playerElement.parent().hasClass("control-wrapper") ? $playerElement.closest(".form-group") : null;
                var progressId = $playerElement.data("progress-id");
                var required = $playerElement.data('required');
                var requiredProgress = $playerElement.data("required-percentage");
                var progress = parseFloat($("#" + progressId).val() || "0");

                var isValid = !required || progress >= requiredProgress;

                if (isValid) {
                    if ($formGroup) {
                        $formGroup.removeClass("has-error");
                    }

                    args.IsValid = true;
                }
                else {
                    if ($formGroup) {
                        $formGroup.addClass("has-error");
                    }

                    args.IsValid = false;
                    validator.errormessage = $playerElement.data('required-error-message');
                }
            }
        };

        return exports;
    }());
}(jQuery));
