(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.imageUploader = (function () {
        var _configure = function (options) {
            options.isBinaryFile = options.isBinaryFile || 'T';

            var wsUrl = Rock.settings.get('baseUrl')
                        + 'ImageUploader.ashx?'
                        + 'isBinaryFile=' + options.isBinaryFile;

            if (options.isBinaryFile == 'T') {
                wsUrl += '&fileId=' + options.fileId
                    + '&fileTypeGuid=' + options.fileTypeGuid;
            }
            else {
                // note rootFolder is encrypted to prevent direct access to filesystem via the URL
                wsUrl += '&rootFolder=' + (encodeURIComponent(options.rootFolder) || '');
            }

            // uses https://github.com/blueimp/jQuery-File-Upload
            $('#' + options.controlId).fileupload({
                url: wsUrl,
                dataType: 'json',
                dropZone: $('#' + options.controlId).closest('.imageupload-dropzone'),
                autoUpload: true,
                submit: options.submitFunction,
                start: function (e, data) {
                    var $el = $('#' + options.controlId).closest('.imageupload-group');
                    $el.find('.imageupload-dropzone').hide();
                    $el.find('.js-upload-progress').show();
                },
                progressall: function (e, data) {
                    var $el = $('#' + options.controlId).closest('.imageupload-group');
                    // implement this to show progress percentage
                },
                stop: function (e) {
                    var $el = $('#' + options.controlId).closest('.imageupload-group');
                    $el.find('.js-upload-progress').hide();
                    $el.find('.imageupload-dropzone').show();
                },
                done: function (e, data) {
                    var $el = $('#' + options.imgThumbnail);
                    $('#' + options.hfFileId).val(data.response().result.Id);
                    var getImageUrl = Rock.settings.get('baseUrl')
                        + 'GetImage.ashx?'
                        + 'isBinaryFile=' + (options.isBinaryFile || 'T')
                        // note rootFolder is encrypted to prevent direct access to filesystem via the URL
                        + '&rootFolder=' + (encodeURIComponent(options.rootFolder) || '')
                        + '&id=' + data.response().result.Id
                        + '&fileName=' + data.response().result.FileName
                        + '&width=50';

                    $el.attr('src', getImageUrl);
                    $('#' + options.aRemove).show();
                    if (options.postbackScript) {
                        eval(options.postbackScript);
                    }

                    if (options.doneFunction) {
                        options.doneFunction(e, data);
                    }
                }
            });

            $('#' + options.aRemove).click(function () {
                $(this).hide();
                var $el = $('#' + options.imgThumbnail);
                $('#' + options.hfFileId).val('0');
                $el.attr('src', Rock.settings.get('baseUrl') + 'Assets/Images/no-picture.svg');
                //$('.imageupload-thumbnail img').css('width', "49px"); // hack for chrome 9/30/2013
                return false;
            });
        },
        exports = {
            initialize: function (options) {
                if (!options.controlId) throw 'Control ID must be set.';
                _configure(options);
            }
        };

        return exports;
    }());
}(jQuery));