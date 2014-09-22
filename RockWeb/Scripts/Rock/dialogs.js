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
                    var $control = $('#' + controlId);
                    var $controlContainer = $control.closest('.scroll-container');
                    var $pickerMenu = $control.find('.picker-menu');
                    var $dialogScrollContainer = $controlContainer.first('div.rock-modal > div.modal-body > div.scroll-container')

                    if ($controlContainer.is(':visible') && $controlContainer.data('tsb')) {

                        // update the picker's scrollbar
                        $controlContainer.tinyscrollbar_update('relative');

                        if ($dialogScrollContainer.length > 0 && $dialogScrollContainer.is(':visible')) {

                            var dialogBodyTop = $dialogScrollContainer.offset().top;
                            var dialogBodyHeight = $dialogScrollContainer.outerHeight(true);
                            var dialogBodyBottom = dialogBodyTop + dialogBodyHeight;

                            var pickerBottom = 0;
                            if ($pickerMenu.length > 0) {
                                var pickerTop = $pickerMenu.offset().top;
                                var pickerHeight = $pickerMenu.outerHeight(true);
                                pickerBottom = pickerTop + pickerHeight;
                            }
                            else
                            {
                                // shouldn't happen, but just in case the picker menu can't be found
                                pickerBottom = 0;
                            }

                            // update the dialog's scrollbar, scrolling to the bottom if the control overflows
                            if (pickerBottom >= dialogBodyBottom) {

                                // set scrollposition to current position + the overflow amount
                                var currentScrollPosition = -parseInt($dialogScrollContainer.find('.overview').css('top'));
                                var scrollAmount = pickerBottom - dialogBodyBottom;
                                $dialogScrollContainer.tinyscrollbar_update(currentScrollPosition + scrollAmount);
                            }
                            else {
                                $dialogScrollContainer.tinyscrollbar_update('relative');
                            }
                        }
                    }
                }
            }

        return exports;
    }());
}(jQuery));