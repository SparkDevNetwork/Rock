(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.fileUploader = (function () {
        var _configure = function (options) {
            options.isBinaryFile = options.isBinaryFile || 'T';
            options.uploadUrl = options.uploadUrl || 'FileUploader.ashx';

            var wsUrl = Rock.settings.get('baseUrl')
                        + options.uploadUrl + '?'
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
                dropZone: $('#' + options.controlId).closest('.fileupload-dropzone'),
                autoUpload: true,
                submit: options.submitFunction,
                start: function (e, data) {
                    var $el = $('#' + options.controlId).closest('.fileupload-group');
                    $el.find('.js-upload-progress').rockFadeIn();
                },
                progressall: function (e, data) {
                    try {
                        if (data.total > 0) {
                            var $el = $('#' + options.controlId).closest('.fileupload-group');
                            var $progressPercent = $el.find('.progress-percent');
                            if (!$progressPercent.length) {
                                return;
                            }

                            var percent = (data.loaded * 100 / data.total).toFixed(0);
                            if (percent > 1 && percent < 99) {
                                $progressPercent.text(percent + "%");
                            }
                            else {
                                $progressPercent.text("uploading");
                            }
                        }
                    }
                    catch (ex) {
                        // ignore if any exception occurs
                    }
                },
                stop: function (e) {
                    var $el = $('#' + options.controlId).closest('.fileupload-group');
                    $el.find('.js-upload-progress').hide();
                    $el.find('.fileupload-dropzone').show();
                },
                done: function (e, data) {
                    var $el = $('#' + options.aFileName);

                    if ((options.isBinaryFile || 'T') == 'F') {
                        $('#' + options.hfFileId).val(data.response().result.FileName);
                    }
                    else {
                        $('#' + options.hfFileId).val(data.response().result.Id);
                    }

                    var getFileUrl = Rock.settings.get('baseUrl')
                        + 'GetFile.ashx?'
                        + 'isBinaryFile=' + (options.isBinaryFile || 'T')
                        // note rootFolder is encrypted to prevent direct access to filesystem via the URL
                        + '&rootFolder=' + (encodeURIComponent(options.rootFolder) || '')
                        + '&id=' + data.response().result.Id
                        + '&fileName=' + data.response().result.FileName;

                    $el.text(data.response().result.FileName).attr('href', getFileUrl);
                    $('#' + options.aRemove).show();
                    if (options.postbackScript) {
                        window.location = "javascript:" +  options.postbackScript;
                    }

                    if (options.doneFunction) {
                        options.doneFunction(e, data);
                    }
                },
                fail: function (e, data) {
                    var $el = $('#' + options.controlId).closest('.fileupload-group');
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
                var $el = $('#' + options.aFileName);
                $('#' + options.hfFileId).val('0');
                $el.attr('href', '#');
                $el.text('');
                $el.removeClass('file-exists');

                if (options.postbackScript) {
                    window.location = "javascript:" + options.postbackScript;
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