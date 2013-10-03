(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.imageUploader = (function () {
        var _configure = function (options) {
            var wsUrl = Rock.settings.get('baseUrl')
                        + 'ImageUploader.ashx?'
                        + 'fileId=' + options.fileId
                        + '&fileTypeGuid=' + options.fileTypeGuid;

            // uses https://github.com/blueimp/jQuery-File-Upload
            $('#' + options.controlId).fileupload({
                url: wsUrl,
                dataType: 'json',
                dropZone: $('#' + options.controlId).closest('.imageupload-dropzone'),
                autoUpload: true,
                done: function (e, data) {
                    var $el = $('#' + options.imgThumbnail);
                    $('#' + options.hfFileId).val(data.response().result.Id);
                    $el.attr('src', Rock.settings.get('baseUrl') + 'GetImage.ashx?id=' + data.response().result.Id + '&width=50');
                    $('#' + options.aRemove).show();
                    if (options.postbackScript) {
                        eval(options.postbackScript);
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