(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.grid = (function () {
        return {
            confirmDelete: function (e, nameText, additionalMsg) {
                e.preventDefault();
                var msg = 'Are you sure you want to delete this ' + nameText + '?';
                if (additionalMsg)
                {
                    msg += ' ' + additionalMsg;
                }
                
                bootbox.dialog({
                    message: msg,
                    buttons: {
                        ok: {
                            label: 'OK',
                            className: 'btn-primary',
                            callback: function () {
                                var postbackJs = e.target.href ? e.target.href : e.target.parentElement.href;

                                // need to do unescape because firefox might put %20 instead of spaces
                                postbackJs = unescape(postbackJs);

                                // Careful!
                                eval(postbackJs);
                            }
                        },
                        cancel: {
                            label: 'Cancel',
                            className: 'btn-secondary'
                        }
                    }
                });
            }
        };
    }());
}(jQuery));