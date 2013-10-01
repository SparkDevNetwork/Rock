(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.fileUploader = (function () {
        var _updateScrollbar = function () {
            // TODO: Update this selector to be relative to the current instance of the control and use '.scroll-container' class rather than id.
            var $scrollContainer = $('#modal-scroll-ontainer');
            if ($scrollContainer.length) {
                $scrollContainer.tinyscrollbar_update('relative');
            }
        },
            _configure = function (options) {
                var isImage = options.fileType === 'image',
                    wsUrl = isImage ? Rock.settings.get('baseUrl') + 'ImageUploader.ashx?' : Rock.settings.get('baseUrl') + 'FileUploader.ashx?';

                if (options.fileId) {
                    wsUrl += 'fileId=' + options.fileId;
                }

                if (options.fileTypeGuid) {
                    if (/fileId/.test(wsUrl)) {
                        wsUrl += '&';
                    }

                    wsUrl += 'fileTypeGuid=' + options.fileTypeGuid;
                }

                // uses https://github.com/blueimp/jQuery-File-Upload
                $('#' + options.controlId).fileupload({
                    url: wsUrl,
                    dataType: 'json',
                    dropZone: isImage ? $('#' + options.controlId).closest('.imageupload-dropzone') : $('#' + options.controlId).closest('.fileupload-drop-zone'),
                    autoUpload: true,
                    done: function (e, data) {
                        var $el = isImage ? $('#' + options.imgThumbnail) : $('#' + options.aFileName);
                        $('#' + options.hfFileId).val(data.response().result.Id);

                        if (isImage) {
                            $el.attr('src', Rock.settings.get('baseUrl') + 'GetImage.ashx?id=' + data.response().result.Id + '&width=50');
                        } else {
                            $el.text(data.response().result.FileName).attr('href', Rock.settings.get('baseUrl') + 'GetFile.ashx?id=' + data.response().result.Id);
                        }

                        $('#' + options.aRemove).show();

                        if (options.postbackScript) {
                            eval(options.postbackScript);
                        }
                    }
                });

                $('#' + options.aRemove).click(function () {
                    $(this).hide();
                    var $el = isImage ? $('#' + options.imgThumbnail) : $('#' + options.aFileName);
                    $('#' + options.hfFileId).val('0');

                    if (isImage) {
                        $el.attr('src', Rock.settings.get('baseUrl') + 'Assets/Images/no-picture.svg');
                        //$('.imageupload-thumbnail img').css('width', "49px"); // hack for chrome 9/30/2013

                    } else {
                        $el.attr('href', '#');
                    }

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