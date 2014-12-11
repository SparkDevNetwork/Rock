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

            var $modalPopup = $('#modal-popup');
            var $modalPopupIFrame = $modalPopup.find('iframe');

            // Use the anchor tag's title attribute as the title of the dialog box
            if (sender.attr('title') != undefined) {
                $('#modal-popup_panel h3').html(sender.attr('title') + ' <small></small>');
            }

            $modalPopupIFrame.one('load', function () {
                $('#modal-popup').fadeTo(0, 1);
                var newHeight = $(this.contentWindow.document).height();
                $(this).height(newHeight);

                $('#modal-popup').modal('layout');

                $(this.contentWindow).on('resize', function () {
                    var newHeight = $(this.document.body).prop('scrollHeight')
                    var $modalPopup = $('#modal-popup');
                    var $modalPopupIFrame = $modalPopup.find('iframe');
                    $modalPopupIFrame.height(newHeight);
                    $modalPopup.modal('layout');
                });

            });

            // Use the anchor tag's href attribute as the source for the iframe
            // this will trigger the load event (above) which will show the popup
            $('#modal-popup').fadeTo(0, 0);
            $modalPopupIFrame.attr('src', popupUrl);
            $('#modal-popup').modal('show');

        },

        exports = {
            updateSize: function (controlId) {
                var $dialog = $('#dialog');
                if ($dialog.length) {
                    var innerWindow = window;
                    var modalBodyScrollHeight = $('#dialog .modal-body').prop('scrollHeight') + 'px';
                    $('#dialog .modal-body').innerHeight(modalBodyScrollHeight);

                    var newHeight = $dialog.prop('scrollHeight') + 'px';
                    $(innerWindow).height(newHeight);

                    $(innerWindow).on('resize', function () {
                        var newHeight = $dialog.prop('scrollHeight') + 'px';
                        $(innerWindow).height(newHeight);
                    });

                    var $modalPopupIFrame = $(innerWindow.parent.document).find('iframe');
                    $modalPopupIFrame.height(newHeight);
                }
                else {
                    
                    var $modalBody = $('#' + controlId).closest('.modal-body');
                    if ($modalBody.length) {
                        $modalBody.height("auto");
                        var modalBodyScrollHeight = $modalBody.prop('scrollHeight') + 'px';
                        $modalBody.innerHeight(modalBodyScrollHeight);
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