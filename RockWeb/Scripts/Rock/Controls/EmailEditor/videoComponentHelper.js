(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};
    Rock.controls.emailEditor = Rock.controls.emailEditor || {};
    Rock.controls.emailEditor.$currentVideoComponent = $(false);

    Rock.controls.emailEditor.videoComponentHelper = (function () {
        var exports = {
            initializeEventHandlers: function () {
                var self = this;

                // Update thumbnail on paste
                $('#component-video-url').on('paste', function () {
                    setTimeout(function () {
                        self.handleVideoUpdate();
                    }, 100);
                });

                // Update thumbnail if someone clicks the arrow
                $('#component-video-addon').on('click', function () {
                    self.handleVideoUpdate();
                });

                // Update url on keyup
                $('#component-video-url').on('keyup', function (e) {
                    self.handleVideoUrlUpdate();
                    //Update thumbnail on enter
                    if (e.keyCode == 13) {
                        self.handleVideoUpdate();
                    }
                });

                // Update target url on paste
                $('#component-target-url').on('paste', function () {
                    setTimeout(function () {
                        self.handleTargetUrlUpdate();
                    }, 100);
                });

                // Update target url on keyup
                $('#component-target-url').on('keyup', function (e) {
                    self.handleTargetUrlUpdate();
                });
            },

            handleTargetUrlUpdate: function () {
                // Set the link href of the video
                var $link = Rock.controls.emailEditor.$currentVideoComponent.find('a');

                $link.attr('href', $('#component-target-url').val());
            },

            handleVideoUrlUpdate: function () {
                // Set the target URL
                $('#component-target-url').val($('#component-video-url').val());

                // Set the link href of the video
                var $link = Rock.controls.emailEditor.$currentVideoComponent.find('a');
                $link.attr('href', $('#component-target-url').val());
            },

            handleVideoUpdate: function () {
                var self = this;

                // Make the button show loading
                $('#component-video-addon').html('<i class="fa fa-sync fa-spin"></i>')

                // Change link
                self.handleVideoUrlUpdate();

                // Hide the error message
                $('#component-video-error').hide();

                // Request thumbnail
                $.post("/GetVideoEmbed.ashx", { video_url: $('#component-video-url').val() })
                    .done(function (data) {
                        self.videoUploadComplete(data);
                    })
                    .fail(function (data) {
                        self.videoUploadComplete('');
                    });
            },

            videoUploadComplete: function (imageUrl) {
                // Change the icon back
                $('#component-video-addon').html('<i class="fa fa-arrow-right"></i>');

                if (imageUrl != '') {
                    //change video image url
                    var $img = Rock.controls.emailEditor.$currentVideoComponent.find('img');
                    $img.attr('src', imageUrl);
                    $('#componentVideoImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("' + imageUrl + '")');
                } else {
                    $('#component-video-error').show();
                }
            },

            handleVideoImageUpdate: function (e, data) {
                //change video image url
                var binaryFileId = data.response().result.Id

                var imageUrl = Rock.settings.get('baseUrl')
                    + 'GetImage.ashx?'
                    + 'isBinaryFile=T'
                    + '&id=' + binaryFileId;

                var $img = Rock.controls.emailEditor.$currentVideoComponent.find('img');
                $img.attr('src', imageUrl);

            },

            setProperties: function ($videoComponent) {
                Rock.controls.emailEditor.$currentVideoComponent = $videoComponent.hasClass('component-video') ? $videoComponent : $(false);

                // Set the value of the url
                var $link = Rock.controls.emailEditor.$currentVideoComponent.find('a');
                $('#component-video-url').val($link.attr('href'))

                // Set the image in the image picker
                var $img = Rock.controls.emailEditor.$currentVideoComponent.find('img');
                $('#componentVideoImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("' + $img.attr('src') + '")');

                // Hide the error message
                $('#component-video-error').hide();

                // Return this to the previous function caller
                return Rock.controls.emailEditor.$currentVideoComponent;
            },

            updateVideoComponent: function (el, contents) {
                var $innerWrapper = Rock.controls.emailEditor.$currentVideoComponent.find('.js-component-video-wrapper');
                if ($innerWrapper.length) {
                    $innerWrapper.html(contents);
                }
            }
        }

        return exports;
    }());
}(jQuery));
