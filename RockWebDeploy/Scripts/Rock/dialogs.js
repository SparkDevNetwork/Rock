(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.dialogs = (function () {
        var _dialogs = {},
            exports = {
                // Presents a bootstrap style alert box with the specified message 
                // then executes the callback function(result)
                confirm: function (msg, callback) {
                    bootbox.dialog({
                        message: msg,
                        buttons: {
                            ok: {
                                label: 'OK',
                                className: 'btn-primary',
                                callback: function () {
                                    callback(true);
                                }
                            },
                            cancel: {
                                label: 'Cancel',
                                className: 'btn-secondary',
                                callback: function () {
                                    callback(false);
                                }
                            }
                        }
                    });
                },

                // Presents a bootstrap style alert box with a 'Are you sure you want to delete this ...' message 
                // Returns true if the user selects OK
                confirmDelete: function (e, nameText, additionalMsg) {
                    // make sure the element that triggered this event isn't disabled
                    if (e.currentTarget && e.currentTarget.disabled) {
                        return false;
                    }

                    e.preventDefault();
                    var msg = 'Are you sure you want to delete this ' + nameText + '?';
                    if (additionalMsg) {
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
                },

                // Updates the closest (outer) scroll-container scrollbar (if the control is with a scroll-container)
                updateModalScrollBar: function (controlId) {
                    var $container = $('#' + controlId).closest('.scroll-container');
                    var $dialog = $('div.rock-modal > div.modal-body > div.scroll-container'),
                    dialogTop,
                    pickerTop,
                    amount;

                    if ($container.is(':visible') && $container.data('tsb')) {
                        $container.tinyscrollbar_update('relative');

                        if ($dialog.length > 0 && $dialog.is(':visible')) {
                            dialogTop = $dialog.offset().top;
                            pickerTop = $container.offset().top;
                            amount = pickerTop - dialogTop;

                            if (amount > 160) {
                                $dialog.tinyscrollbar_update('bottom');
                            }
                        }
                    }
                }
            }

        return exports;
    }());
}(jQuery));