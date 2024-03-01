var RockImageBrowser = function (context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-picture-o"/>',
        tooltip: 'Image Browser',
        click: function () {
            // If we have a target image, store it before the selection is modified when the modal dialog receives focus.
            var imgTarget = $( context.layoutInfo.editable.data( 'target' ) )[0];

            context.invoke('editor.saveRange');
            var iframeUrl = Rock.settings.get('baseUrl') + "htmleditorplugins/rockfilebrowser";
            iframeUrl += "?RootFolder=" + encodeURIComponent(context.options.rockFileBrowserOptions.imageFolderRoot);
            iframeUrl += "&BrowserMode=image";
            iframeUrl += "&EditorTheme=" + context.options.rockTheme;
            iframeUrl += "&ModalMode=1";
            iframeUrl += "&Title=Select%20Image";

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $modalPopupIFrame = Rock.controls.modal.getModalPopupIFrame();

            $modalPopupIFrame.load(function () {

                $modalPopupIFrame.contents().off('click').on('click', '.js-select-file-button', function (e) {
                    // In some cases, such as the email editor, this can run too soon. Pause 1ms here to ensure correct funcitonality.
                    setTimeout(function () { }, 1);

                    Rock.controls.modal.close();
                    var fileResult = $(e.target).closest('body').find('.js-filebrowser-result input[type=hidden]').val();
                    if (fileResult) {
                        // iframe returns the result in the format "imageSrcUrl|imageAltText"
                        var resultParts = fileResult.split('|');
                        var imageElement = document.createElement('img');

                        // Ensure the string is not double-encoded.
                        var url = encodeURI(decodeURI(Rock.settings.get('baseUrl') + resultParts[0]));
                        var altText = resultParts[1];

                        // if they already have an img selected, just change the src of the image
                        if (imgTarget) {
                            imgTarget.src = url;
                            imgTarget.alt = altText;
                        }
                        else {
                            // insert the image at 25% to get them started
                            context.invoke('editor.restoreRange');
                            context.invoke('editor.insertImage', url, function ($image) {
                                $image.css('width', '25%');
                                $image.attr('alt', altText);
                            });
                        }

                        // Invoke the change event and ensure the updated content is correctly
                        // passed to other event subscribers.
                        var html = context.code();
                        context.invoke('triggerEvent', 'change', html);
                    }
                });

                $modalPopupIFrame.contents().on('click', '.js-cancel-file-button', function () {
                    Rock.controls.modal.close();
                });
            });
        }
    });

    if (context.options.rockFileBrowserOptions.enabled) {
        return button.render();   // return button as jquery object 
    }
    else {
        return null;
    }
}
