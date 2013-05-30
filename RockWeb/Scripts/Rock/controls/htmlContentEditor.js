(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.htmlContentEditor = (function () {
        var HtmlEditor = function (options) {
            this.blockId = options.blockId;
            this.behaviorId = options.behaviorId;
            this.hasBeenModified = options.hasBeenModified || false;
            this.versionId = options.versionId;
            this.startDateId = options.startDateId;
            this.expireDateId = options.expireDateId;
            this.cdEditorId = options.ckEditorId;
            this.approvalId = options.approvalId;
        };

        HtmlEditor.prototype.initializeEventHandlers = function () {
            var blockId = this.blockId,
                behaviorId = this.behaviorId,
                hasBeenModified = this.hasBeenModified,
                versionId = this.versionId,
                startDateId = this.startDateId,
                expireDateId = this.expireDateId,
                ckEditorId = this.ckEditorId,
                approvalId = this.approvalId;

            $('#html-content-edit-' + blockId + ' .date-picker').kendoDatePicker({
                open: function () {
                    setTimeout(function () {
                        $('.k-calendar-container').parent('.k-animation-container').css({ zindex: 200000 });
                    }, 1);
                }
            });

            $('#html-content-version-' + blockId).click(function () {
                $('#html-content-versions-' + blockId).show();
                $(this).hide();
                $('#html-content-edit-' + blockId).hide();
                $find(behaviorId)._layout();
                return false;
            });

            $('#html-content-versions-cancel-' + blockId).click(function () {
                $('#html-content-edit-' + blockId).show();
                $('#html-content-version-' + blockId).show();
                $('#html-content-versions-' + blockId).hide();
                $find(behaviorId)._layout();
                return false;
            });

            $('a.html-content-show-version-' + blockId).click(function () {
                var confirmMessage = 'Loading a previous version will cause any changes you\'ve made to the existing text to be lost. Are you sure you want to continue?',
                    request;

                if (hasBeenModified  || confirm(confirmMessage)) {

                    // TODO: Update this endpoint URL. It doesn't appear to work anymore.
                    request = $.ajax({
                        type: 'GET',
                        contentType: 'application/json',
                        dataType: 'json',
                        url: '/REST/Cms/HtmlContent/' + $(this).attr('data-html-id')
                    });

                    request.done(function (data) {
                        var ckInstance = CKEDITOR.instances[ckEditorId];

                        $('#html-content-version-' + blockId).text('Version ' + data.Version);
                        $('#' + versionId).val(data.Version);
                        $('#' + startDateId).val(data.StartDateTime);
                        $('#' + expireDateId).val(data.ExpireDateTime);
                        $('#' + approvalId).attr('checked', data.Approved);

                        ckInstance.setData(data.Content, function() {
                            ckInstance.resetDirty();
                            $('#html-content-edit-' + blockId).show();
                            $('#html-content-version-' + blockId).show();
                            $('#html-content-versions-' + blockId).hide();
                            $find(behaviorId)._layout();
                        });

                    });

                    request.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);
                    });
                }
            });
        };

        HtmlEditor.prototype.initialize = function () {
            this.initializeEventHandlers();
        };

        var exports = {
            htmlEditors: {},
            initialize: function (options) {
                var htmlEditor = new HtmlEditor(options);
                exports.htmlEditors[options.blockId] = htmlEditor;
                htmlEditor.initialize();
            },
            saveHtmlContent: function (blockId) {
                $('#html-content-edit-' + blockId)
                    .parent()
                    .find('.save-button')
                    .click();
            }
        };

        return exports;
    }());
}(jQuery));