(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.modal = (function () {
        var _showModalPopup = function (sender, popupUrl) {
            var $primaryBtn = $('#modal-popup_panel div.modal-footer a.btn.primary');
            if (sender.attr('primary-button') !== undefined) {
                $primaryBtn.text(sender.attr('primary-button'));
            } else {
                $primaryBtn.text('Save');
            }
            if ($primaryBtn.text() !== '') {
                $primaryBtn.show();
            } else {
                $primaryBtn.hide();
            }

            var $secondaryBtn = $('#modal-popup_panel div.modal-footer a.btn.secondary');
            if (sender.attr('secondary-button') !== undefined) {
                $secondaryBtn.text(sender.attr('secondary-button'));
            } else {
                $secondaryBtn.text('Cancel');
            }
            if ($secondaryBtn.text() !== '') {
                $secondaryBtn.show();
            } else {
                $secondaryBtn.hide();
            }

            // If the anchor tag specifies a modal height, set the dialog's height
            if (sender.attr('height') != undefined) {
                $('#modal-popup_panel div.modal-body').css('height', sender.attr('height'));
                $('#modal-popup_contentPanel.iframe').css('height', sender.attr('height'));
            }
            else {
                $('#modal-popup_panel div.modal-body').css('height', '');
                $('#modal-popup_contentPanel.iframe').css('height', '500px');
            }

            // Use the anchor tag's title attribute as the title of the dialog box
            if (sender.attr('title') != undefined) {
                $('#modal-popup_panel h3').html(sender.attr('title') + ' <small></small>');
            }

            // to avoid flicker, first hide the contents, then load the iframe.  After loading, then show the contents
            $('#modal-popup_contentPanel').hide(0, function () {

                $('#modal-popup_iframe').on('load', function () {

                    console.log('load event');

                    // set opacity to 1% (instead of invisible) so that ModalIFrameDialog can position correctly
                    $('#modal-popup_contentPanel').fadeTo(0, 1);
                    $('#modal-popup_iframe').off('load');
                    // popup the dialog box
                    $find('modal-popup').show();
                    $('#modal-popup_contentPanel').show();
                });

                // Use the anchor tag's href attribute as the source for the iframe
                // this will trigger the load event (above) which will show the popup
                $('#modal-popup_iframe').attr('src', popupUrl);
            });

        },

        exports = {
            close: function (msg) {
                $('#modal-popup_iframe').attr('src', '');
                $find('modal-popup').hide();

                if (msg && msg != '') {
                    $('#rock-config-trigger-data').val(msg);
                    $('#rock-config-trigger').click();
                }
            },
            show: function (sender, popupUrl, detailsId, postbackUrl ) {
                _showModalPopup(sender, popupUrl);
            },

        };

        return exports;
    }());
}(jQuery));