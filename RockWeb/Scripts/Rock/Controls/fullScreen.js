(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.fullScreen = (function () {
        var exports = {
            initialize: function (el) {
                $('.js-fullscreen-trigger').on('click', function (e) {
                    e.preventDefault();
                    var elem = $(this).closest('.block-instance')[0] || document.documentElement
                    if (el) {
                        // If Requesting Fullscreen page, verify layout is fullscreen capable.
                        if (el === 'body' && $(this).parents('.page-fullscreen-capable').length < 1) {
                            elem = $(this).closest('.block-instance')[0]
                        } else {
                            elem = $(el)[0];
                        }
                    }
                    Rock.controls.fullScreen.toggleFullscreen(elem);
                });
            },
            toggleFullscreen: function (elem) {
                elem = elem || document.documentElement;
                if (!document.fullscreenElement && !document.mozFullScreenElement &&
                    !document.webkitFullscreenElement && !document.msFullscreenElement) {
                    if (elem.requestFullscreen) {
                    elem.requestFullscreen();
                    document.addEventListener('fullscreenchange', this.exitHandler, false);
                    } else if (elem.msRequestFullscreen) {
                    elem.msRequestFullscreen();
                    document.addEventListener('MSFullscreenChange', this.exitHandler, false);
                    } else if (elem.mozRequestFullScreen) {
                    elem.mozRequestFullScreen();
                    document.addEventListener('mozfullscreenchange', this.exitHandler, false);
                    } else if (elem.webkitRequestFullscreen) {
                    elem.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
                    document.addEventListener('webkitfullscreenchange', this.exitHandler, false);
                    }
                    $(elem).addClass('is-fullscreen');
                } else {
                    if (document.exitFullscreen) {
                    document.exitFullscreen();
                    } else if (document.msExitFullscreen) {
                    document.msExitFullscreen();
                    } else if (document.mozCancelFullScreen) {
                    document.mozCancelFullScreen();
                    } else if (document.webkitExitFullscreen) {
                    document.webkitExitFullscreen();
                    }
                    $(elem).removeClass('is-fullscreen');
                }
            },
            exitHandler: function (elem) {
                if (!document.webkitIsFullScreen && !document.mozFullScreen && !document.msFullscreenElement) {
                    $(elem.target).removeClass('is-fullscreen');
                    document.dispatchEvent(new Event('RockExitFullscreen'));
                }
            }
        };

        return exports;
    }());
}(jQuery));
