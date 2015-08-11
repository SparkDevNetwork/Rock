(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.imageUploader = (function () {
        var _configure = function (options) {
            options.isBinaryFile = options.isBinaryFile || 'T';

            // default setImageUrlOnUpload to true if not specified
            if (options.setImageUrlOnUpload == null) {
                options.setImageUrlOnUpload = true;
            }

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
                    $el.find('.js-upload-progress').rockFadeIn();
                },
                progressall: function (e, data) {
                    // var $el = $('#' + options.controlId).closest('.imageupload-group');
                    // implement this to show progress percentage
                },
                stop: function (e) {
                    var $el = $('#' + options.controlId).closest('.imageupload-group');
                    $el.find('.js-upload-progress').hide();
                    $el.find('.imageupload-dropzone').rockFadeIn();
                },
                done: function (e, data) {
                    var $el = $('#' + options.imgThumbnail);
                    $('#' + options.hfFileId).val(data.response().result.Id);
                    var getImageUrl = Rock.settings.get('baseUrl')
                        + 'GetImage.ashx?'
                        + 'isBinaryFile=' + (options.isBinaryFile || 'T')
                        + '&id=' + data.response().result.Id
                        + '&fileName=' + data.response().result.FileName
                        + '&width=500';

                    if (options.rootFolder) {
                        // note rootFolder is encrypted to prevent direct access to filesystem via the URL
                        getImageUrl += '&rootFolder=' + encodeURIComponent(options.rootFolder);
                    }

                    if (options.setImageUrlOnUpload) {
                        if ($el.is('img')) {
                            $el.attr('src', getImageUrl);
                        }
                        else {
                            $el.attr('style', 'background-image:url("' + getImageUrl + '");background-size:cover;background-position:50%');
                        }
                    }

                    $('#' + options.aRemove).show();
                    if (options.postbackScript) {
                        window.location = "javascript:" + options.postbackScript;
                    }

                    if (options.doneFunction) {
                        options.doneFunction(e, data);
                    }
                },
                fail: function (e, data) {
                    var $el = $('#' + options.controlId).closest('.imageupload-group');
                    $el.siblings('.js-rockupload-alert').remove();
                    var $warning = $('<div class="alert alert-warning alert-dismissable js-rockupload-alert"/>');

                    var msg = "unable to upload";
                    if (data.response().jqXHR && data.response().jqXHR.status == 406) {
                        msg = "file type not allowed";
                    } else if (data.response().jqXHR && data.response().jqXHR.responseText) {
                        msg = data.response().jqXHR.responseText;
                    }

                    if (options.maxUploadBytes && data.total) {
                        if (data.total >= options.maxUploadBytes) {
                            msg = "file size is limited to " + (options.maxUploadBytes / 1024 / 1024) + "MB";
                        }
                    }

                    $warning.append('<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>')
                        .append('<strong><i class="fa fa-exclamation-triangle"></i> Warning </strong>')
                        .append(msg);
                    $warning.insertBefore($el);
                }
            });

            $('#' + options.aRemove).click(function () {
                $(this).hide();
                var $el = $('#' + options.imgThumbnail);
                var noPictureUrl = options.noPictureUrl || Rock.settings.get('baseUrl') + 'Assets/Images/no-picture.svg';
                if ($el.is('img')) {
                    $el.attr('src', noPictureUrl);
                }
                else {
                    $el.attr('style', 'background-image:url(' + noPictureUrl + ');background-size:cover;background-position:50%');
                }
                if (options.postbackRemovedScript) {
                    window.location = "javascript:" + options.postbackScript;
                } else {
                    $('#' + options.hfFileId).val('0');
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