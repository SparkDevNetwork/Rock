/*
 * BJW  7-7-2020
 * Previous Issues and Fixes
 *
 * April 15, 2020
 * Problem:
 *      Open a modal dialog and close it. Next, try to open a modal alert. An invisible backdrop is added to the
 *      page such that the page cannot be interacted with. The alert does not appear.
 * Solution:
 *      Every postback causes the C# modal control code to register JS that checks a hidden field to determine
 *      if a modal should be open or closed. If many modals exist, then they are all checked and all call
 *      the JS close dialog method, which removed backdrops. If each modal removes any backdrop without
 *      discretion, then they interfere with each other. Therefore, only remove backdrops that belong to the modal
 *      in current context being closed.
 *      https://github.com/SparkDevNetwork/Rock/commit/8a1c5653c534c61548de34d0ca1a4483304437a1
 *
 * June 1, 2020
 * Problem:
 *      When a modal is opened, sometimes the body is set to .modal-open and sometimes a manager (usually the
 *      update panel of the block) is set to .modal-open. In the event of the manager, cleanup was not occurring
 *      and the class remained even after the modal was gone.
 * Solution:
 *      A param was added to the close dialog JS method that allowed proper cleanup of classes (.modal-open) added
 *      to the manager.
 *      https://github.com/SparkDevNetwork/Rock/commit/26cf101ae8e8923008ae42f3d833cab5a35df8cb
 *
 * July 1, 2020
 * Problem:
 *      Open a dialog. Close the dialog with a postback that calls C# Hide with no manager param. Open the dialog again
 *      and there is no backdrop.
 * Solution:
 *      This seems to be an issue with bootstrap (seems unlikely) or the combination of bootstrap with the postback
 *      UI re-rendering. The modal is removed from the DOM before our JS close method is reached. Therefore proper
 *      cleanup of the first modal open is never done. This seems to impact the second opening and bootstrap fails
 *      to add the backdrop. Thus our code adds a backdrop if one is not visible.
 *      https://github.com/SparkDevNetwork/Rock/commit/f08bb6fc1e17b79d0772152707acd4a3876a68d6
 *
 *  July 7, 2020
 *  Problem:
 *      Open a dialog. Close the dialog with a postback that calls C# Hide with no manager param. Open the dialog again
 *      and there is a backdrop but no modal.
 *  Solution:
 *      This is because the postback calls the C# Hide method, which does not provide the manager to the JS close modal
 *      method. Because of this, proper cleanup doesn't occur. We can calculate the manger as we did when we opened the
 *      modal, which fixes the issue.
 *      Also, there was a bug in hiding the backdrop where if the owner no longer existed
 *      in the DOM, then the backdrop was not removed.
 */ 

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

                $('body').addClass('modal-open').css('padding-right', Rock.controls.util.getScrollbarWidth());
                $('#modal-popup').modal('layout');
            });

            // Use the anchor tag's href attribute as the source for the iframe
            // this will trigger the load event (above) which will show the popup
            $('#modal-popup').fadeTo(0, 0);

            // Indicate to Rock that this content is being served in an iFrame modal.
            var separator = popupUrl.indexOf("?") === -1 ? "?" : "&";
            popupUrl = popupUrl + separator + "IsIFrameModal=true";

            $modalPopupIFrame.attr('src', popupUrl);
            $('#modal-popup').modal({
                show: true,
                backdrop: 'static',
                keyboard: false,
                attentionAnimation: '',
                modalOverflow: true
            });
        }

        // shows a non-IFrame modal dialog control
        var _showModalControl = function ($modalDialog, managerId) {
            $('body').addClass('modal-open').css('padding-right', Rock.controls.util.getScrollbarWidth());
            $modalDialog.modal({
                show: true,
                manager: managerId,
                backdrop: 'static',
                keyboard: false,
                attentionAnimation: '',
                modalOverflow: true,
                replace: true
            });

            if ($('.modal-backdrop').filter(':visible').length === 0) {
                // ensure that there is a modal-backdrop and include its owner as an attribute so that we can remove it when this modal is closed
                $('<div class="modal-backdrop" data-modal-id="' + $modalDialog.prop('id') + '" />').appendTo('body');
            }
        }

        var exports = {
            // updates the side of the modal that the control is in.
            // this function works for both the IFrame modal and ModalDialog control
            updateSize: function (controlId) {
                var $control = typeof (controlId) == 'string' ? $('#' + controlId) : $(controlId);
                if ($control && $control.length) {
                    var $modalBody = $control.closest('.modal-body');
                    if ($modalBody.is(':visible')) {
                        $modalBody[0].style.minHeight = "0";
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

                $('body').removeClass('modal-open').css('padding-right', '');

                if (msg && msg != '') {

                    if (msg == 'PAGE_UPDATED') {
                        location.reload(true);
                    }
                    else {
                        $('#rock-config-trigger-data').val(msg);
                        $('#rock-config-trigger').trigger('click');
                    }
                }
            },
            // closes a ModalDialog control (non-IFrame Modal)
            closeModalDialog: function ($modalDialog, $manager) {
                if ($modalDialog && $modalDialog.length && $modalDialog.modal) {
                    $modalDialog.modal('hide');
                }

                // remove the modal-open class from this modal's manager
                if ($manager && $manager.length) {
                    $manager.each(function () {
                        $(this).removeClass('modal-open');
                    });
                }

                // if all modals are closed, remove the modal-open class from the body
                if ($('.modal:visible').length === 0) {
                    $('body').removeClass('modal-open').css('padding-right', '');
                }

                // Ensure any modalBackdrops are removed if its owner is no longer visible. Note that some
                // backdrops do not have an owner
                $('.modal-backdrop').each(function () {
                    var $modalBackdrop = $(this);
                    var ownerId = $modalBackdrop.data('modalId');
                    var $owner = $('#' + ownerId);
                    var isOwnerInDom = $owner.length > 0;

                    if (ownerId && (!isOwnerInDom || !$owner.is(':visible'))) {
                        $modalBackdrop.remove();
                    }
                });
            },
            // shows the #modal-popup modal (IFrame Modal)
            show: function (sender, popupUrl, detailsId, postbackUrl) {
                _showModalPopup(sender, popupUrl);
            },
            // shows a ModalDialog control (non-IFrame Modal)
            showModalDialog: function ($modalDialog, managerId) {
                _showModalControl($modalDialog, managerId);
            },

            // gets the IFrame element of the global the Modal Popup (for the IFrame Modal)
            getModalPopupIFrame: function () {
                var $modalPopup = $('#modal-popup');
                var $modalPopupIFrame = $modalPopup.find('iframe');

                return $modalPopupIFrame;
            }
        };

        return exports;
    }());
}(jQuery));
