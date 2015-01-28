(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.modal = (function () {
        var _showModalPopup = function (sender, popupUrl) {
            var $modalPopup = $('#modal-popup');
            var $modalPopupIFrame = $modalPopup.find('iframe');

            // Use the anchor tag's title attribute as the title of the dialog box
            if (sender.attr('title') != undefined) {
                $('#modal-popup_panel h3').html(sender.attr('title') + ' <small></small>');
            }

            $modalPopupIFrame.one('load', function () {
                $('#modal-popup').fadeTo(0, 1);
                Rock.controls.modal.updateSize();
                
                var newHeight = $(this.contentWindow.document).height();
                if ($(this).height() != newHeight) {
                    $(this).height(newHeight);
                }

                $('#modal-popup').modal('layout');

                $(this.contentWindow).on('resize', function () {
                    var newHeight = $(this.document.body).prop('offsetHeight')
                    var $modalPopup = $('#modal-popup');
                    var $modalPopupIFrame = $modalPopup.find('iframe');
                    if ($modalPopupIFrame.height() != newHeight) {
                        $modalPopupIFrame.height(newHeight);
                    }
                });
            });

            // Use the anchor tag's href attribute as the source for the iframe
            // this will trigger the load event (above) which will show the popup
            $('#modal-popup').fadeTo(0, 0);
            $modalPopupIFrame[0].style.height = 'auto';
            $modalPopupIFrame.attr('src', popupUrl);
            $('#modal-popup').modal({
                show: true,
                backdrop: 'static',
                keyboard: false,
                attentionAnimation: ''
            });
        },

        exports = {
            updateSize: function (controlId) {
                var $control = typeof (controlId) == 'string' ? $('#' + controlId) : $(controlId);
                var $modalPopupIFrame = $(window.parent.document).find('iframe');

                var iframeMode = false;

                if ($modalPopupIFrame.length && $modalPopupIFrame[0].contentWindow == window) {
                    iframeMode = true;
                    if ($modalPopupIFrame[0].style.height != 'auto') {
                        $modalPopupIFrame[0].style.height = 'auto';
                        var contentsHeight = $modalPopupIFrame.contents().height();

                        // shrink the iframe in case the contents got smaller
                        $modalPopupIFrame.height('auto');
                        var iFrameHeight = $modalPopupIFrame.height();
                        if (contentsHeight > iFrameHeight) {
                            // if the contents are larger than the iFrame, grow the iframe to fit
                            $modalPopupIFrame.height(contentsHeight);
                        }
                    }
                }
                
                if ($control && $control.length) {
                    var $modalBody = $control.closest('.modal-body');
                    if ($modalBody.is(':visible')) {
                        var scrollHeight = $modalBody.prop('scrollHeight');
                        if (iframeMode || $modalBody.outerHeight() != scrollHeight) {
                            // if this is an IFrameModal or if modalbody didn't already grow to fit (maybe because of a bootstrap dropdown) make modal-body big enough to fit.  
                            // Intentionally leave it stretched-out even if needed space shrinks
                            $modalBody[0].style.minHeight = scrollHeight + "px";
                        }
                    }
                }
            },
            close: function (msg) {
                // do a setTimeout so this fires after the postback
                $('#modal-popup').hide();
                setTimeout(function () {
                    $('#modal-popup iframe').attr('src', '');
                    $('#modal-popup').modal('hide');

                }, 0);

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
            show: function (sender, popupUrl, detailsId, postbackUrl) {
                _showModalPopup(sender, popupUrl);
            },

        };

        return exports;
    }());
}(jQuery));