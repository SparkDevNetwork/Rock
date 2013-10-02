(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.fileUploader = (function () {
        var _configure = function (options) {
            var wsUrl = Rock.settings.get('baseUrl')
                        + 'FileUploader.ashx?'
                        + 'fileId=' + options.fileId
                        + '&fileTypeGuid=' + options.fileTypeGuid;

            // uses https://github.com/blueimp/jQuery-File-Upload
            $('#' + options.controlId).fileupload({
                url: wsUrl,
                dataType: 'json',
                dropZone: $('#' + options.controlId).closest('.fileupload-drop-zone'),
                autoUpload: true,
                done: function (e, data) {
                    var $el = $('#' + options.aFileName);
                    $('#' + options.hfFileId).val(data.response().result.Id);
                    $el.text(data.response().result.FileName).attr('href', Rock.settings.get('baseUrl') + 'GetFile.ashx?id=' + data.response().result.Id);
                    $('#' + options.aRemove).show();
                    if (options.postbackScript) {
                        eval(options.postbackScript);
                    }
                }
            });

            $('#' + options.aRemove).click(function () {
                $(this).hide();
                var $el = $('#' + options.aFileName);
                $('#' + options.hfFileId).val('0');
                $el.attr('href', '#');

                if (options.postbackScript) {
                    eval(options.postbackScript);
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