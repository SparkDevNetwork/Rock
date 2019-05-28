(function ($) {
    window.Rock = window.Rock || {};

    Rock.dialogs = (function () {
        var _dialogs = {},
            exports = {
                // Presents a bootstrap style alert box with the specified message
                // then executes the callback function(result)
                alert: function (msg) {
                    bootbox.dialog({
                        message: msg,
                        buttons: {
                            ok: {
                                label: 'OK',
                                className: 'btn-primary'
                            }
                        }
                    });
                },

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
                                className: 'btn-default',
                                callback: function () {
                                    callback(false);
                                }
                            }
                        }
                    });
                },

                // Presents a bootstrap style alert box which prevents on "Cancel" and continues on "Ok"
                confirmPreventOnCancel: function (e, msg) {
                    // make sure the element that triggered this event isn't disabled
                    if (e.currentTarget && e.currentTarget.disabled) {
                        return false;
                    }

                    e.preventDefault();
                    bootbox.dialog({
                        message: msg,
                        buttons: {
                            ok: {
                                label: 'OK',
                                className: 'btn-primary',
                                callback: function () {
                                    var postbackJs = e.target.href ? e.target.href : e.target.parentElement.href;
                                    window.location = postbackJs;
                                }
                            },
                            cancel: {
                                label: 'Cancel',
                                className: 'btn-default'
                            }
                        }
                    });
                },

                // Presents a bootstrap style alert box with a 'Are you sure you want to delete this ...' message
                // Returns true if the user selects OK
                confirmDelete: function (e, nameText, additionalMsg)
                {
                    var msg = 'Are you sure you want to delete this ' + nameText + '?';
                    if (additionalMsg)
                    {
                        msg += ' ' + additionalMsg;
                    }
                    this.confirmPreventOnCancel(e, msg);
                },

                // Updates the modal so that scrolling works
                updateModalScrollBar: function (controlId) {
                    Rock.controls.modal.updateSize(controlId);
                }
            }

        return exports;
    }());
}(jQuery));
