(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.electronicSignatureControl = (function () {
        var exports = {
            /**
             * 
             * @param {any} options
             */
            initialize: function (options) {
                if (!options.controlId) {
                    throw 'id is required';
                }

                var self = this;

                self.ImageMimeType = options.imageMimeType || "image/png";

                var $control = $('#' + options.controlId);

                if ($control.length == 0) {
                    return;
                }

                self.$signatureControl = $control;
                self.$signatureTyped = $('.js-signature-typed', $control);

                self.$signatureEntryDrawn = $('.js-signature-entry-drawn', $control);
                self.$signaturePadCanvas = $('.js-signature-pad-canvas', $control);
                self.$signatureImageDataURL = $('.js-signature-data', $control);
                
                self.$clearSignature = $('.js-clear-signature', $control);
                self.$saveSignature = $('.js-save-signature', $control);

                self.initializeSignaturePad();

                self.$saveSignature.click(function () {
                    if (!self.signatureIsValid(self.$signatureControl)) {
                        return false;
                    }

                    if (self.$signaturePadCanvas.length) {
                        var signaturePad = self.$signaturePadCanvas.data('signatureComponent');

                        if (signaturePad) {
                            var signatureImageDataUrl = signaturePad.toDataURL(self.ImageMimeType);
                            self.$signatureImageDataURL.val(signatureImageDataUrl);
                        }
                    }
                });
            },

            /** */
            initializeSignaturePad: function () {
                var self = this;
                if (!self.$signaturePadCanvas.length) {
                    return
                }

                var signaturePadOptions = {};
                if (self.ImageMimeType == "image/jpeg") {
                    // NOTE That if we use jpeg, we'll have to deal with this https://github.com/szimek/signature_pad/issues/584
                    signaturePadOptions = {
                        penColor: 'black',
                        backgroundColor: 'white'
                    }
                };

                var signaturePad = new SignaturePad(self.$signaturePadCanvas[ 0 ], signaturePadOptions);

                self.$signaturePadCanvas.data('signatureComponent', signaturePad);

                self.$clearSignature.click(function () {
                    signaturePad.clear();
                })

                window.addEventListener("resize", function () {
                    self.resizeSignatureCanvas(self)
                });

                self.resizeSignatureCanvas(self);
            },

            /** */
            resizeSignatureCanvas: function (signatureControl) {
                if (!signatureControl.$signaturePadCanvas.length) {
                    return;
                }

                // If the window is resized, that'll affect the drawing canvas
                // also, if there is an existing signature, it'll get messed up, so clear it and
                // make them sign it again. See additional details why 
                // https://github.com/szimek/signature_pad
                var signaturePadCanvas = signatureControl.$signaturePadCanvas[ 0 ];
                var containerWidth = signatureControl.$signatureEntryDrawn.width();
                if (!containerWidth || containerWidth == 0) {
                    containerWidth = 400;
                }

                // Note the suggestion  https://github.com/szimek/signature_pad#handling-high-dpi-screens
                // to re-calculate the ratio based on window.devicePixelRatio isn't needed. 
                // We can just use the width() of the container and use fixed height of 100.
                const ratio = 1;
                signaturePadCanvas.width = containerWidth * ratio;
                signaturePadCanvas.height = 100 * ratio;
                signaturePadCanvas.getContext("2d").scale(ratio, ratio);

                var signaturePad = signatureControl.$signaturePadCanvas.data('signatureComponent');
                signaturePad.clear();
            },

            signatureIsValid: function ($signatureControl) {
                var $signatureCanvas = $signatureControl.find('.js-signature-pad-canvas')
                var $signatureTyped = $signatureControl.find('.js-signature-typed');

                if ($signatureCanvas.length == 0 && $signatureTyped.length == 0) {
                    return;
                }

                var isValid = true;

                if ($signatureCanvas.length) {
                    var signatureCanvas = $signatureCanvas.data('signatureComponent');

                    if (signatureCanvas.isEmpty()) {
                        isValid = false;
                    }
                }
                else {
                    if ($signatureTyped.val().trim() == '') {
                        isValid = false;
                    }
                }

                return isValid;
            },

            /** */
            clientValidate: function (validator, args) {
                var $signatureControl = $(validator).closest('.js-electronic-signature-control');

                var isValid = this.signatureIsValid($signatureControl);
                
                if (!isValid) {
                    validator.errormessage = "Signature is required.";
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
