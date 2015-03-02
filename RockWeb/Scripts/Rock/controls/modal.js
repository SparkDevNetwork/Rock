(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.modal = (function () {
        // shows the IFrame #modal-popup modal
        var _showModalPopup = function (sender, popupUrl) {
            var $modalPopup = $('#modal-popup');
            var $modalPopupIFrame = $modalPopup.find('iframe');

            // Use the anchor tag's title attribute as the title of the dialog box
            if (sender.attr('title') != undefined) {
                $('#modal-popup_panel h3').html(sender.attr('title') + ' <small></small>');
            }

            $modalPopupIFrame.height('auto');

            $modalPopupIFrame.one('load', function () {

                // now that the iframe is loaded, show it, set it's initial height and do a modal layout
                $('#modal-popup').fadeTo(0, 1);

                var newHeight = $(this.contentWindow.document).height();
                if ($(this).height() != newHeight) {
                    $(this).height(newHeight);
                }

                $('body').addClass('modal-open');
                $('#modal-popup').modal('layout');
            });

            // Use the anchor tag's href attribute as the source for the iframe
            // this will trigger the load event (above) which will show the popup
            $('#modal-popup').fadeTo(0, 0);
            $modalPopupIFrame.attr('src', popupUrl);
            $('#modal-popup').modal({
                show: true,
                backdrop: 'static',
                keyboard: false,
                attentionAnimation: '',
                modalOverflow: true
            });
        },

        // shows a non-IFrame modal dialog control
        _showModalControl = function ($modalDialog, managerId) {
            $('body').addClass('modal-open');
            $modalDialog.modal({
                show: true,
                manager: managerId,
                backdrop: 'static',
                keyboard: false,
                attentionAnimation: '',
                modalOverflow: true,
                replace: true
            });
        },

        exports = {
            // updates the side of the modal that the control is in.  
            // this function works for both the IFrame modal and ModalDialog control
            updateSize: function (controlId) {
                var $control = typeof (controlId) == 'string' ? $('#' + controlId) : $(controlId);
                if ($control && $control.length) {
                    var $modalBody = $control.closest('.modal-body');
                    if ($modalBody.is(':visible')) {
                        $modalBody[0].style.minHeight = "0px";
                        var scrollHeight = $modalBody.prop('scrollHeight');
                        if ($modalBody.outerHeight() != scrollHeight) {
                            // if modalbody didn't already grow to fit (maybe because of a bootstrap dropdown) make modal-body big enough to fit.
                            $modalBody[0].style.minHeight = scrollHeight + "px";

                            // force the resizeDetector to fire
                            if ($('#dialog').length && $('#dialog')[0].resizedAttached) {
                                $('#dialog')[0].resizedAttached.call();
                            }
                        }
                    }
                }

            },
            // closes the #modal-popup modal (IFrame Modal)
            close: function (msg) {
                // do a setTimeout so this fires after the postback
                $('#modal-popup').hide();
                setTimeout(function () {
                    $('#modal-popup iframe').attr('src', '');
                    $('#modal-popup').modal('hide');

                }, 0);

                $('body').removeClass('modal-open');

                if (msg && msg != '') {

                    if (msg == 'PAGE_UPDATED') {
                        location.reload(true);
                    }
                    else {
                        $('#rock-config-trigger-data').val(msg);
                        $('#rock-config-trigger').click();
                    }
                }
            },
            // closes a ModalDialog control (non-IFrame Modal) 
            closeModalDialog: function ($modalDialog) {
                if ($modalDialog && $modalDialog.length && $modalDialog.modal) {
                    $modalDialog.modal('hide');
                }

                // if all modals are closed, remove the modal-open class from body
                if (!$('.modal').is(':visible')) {
                    {
                        $('body').removeClass('modal-open');
                    }
                }
            },
            // shows the #modal-popup modal (IFrame Modal)
            show: function (sender, popupUrl, detailsId, postbackUrl) {
                _showModalPopup(sender, popupUrl);
            },
            // shows a ModalDialog control (non-IFrame Modal) 
            showModalDialog: function ($modalDialog, managerId) {
                _showModalControl($modalDialog, managerId);
            },
        };

        return exports;
    }());
}(jQuery));