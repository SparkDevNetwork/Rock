var RockImageBrowser = function (context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-picture-o"/>',
        tooltip: 'Image Browser',
        click: function () {
            var contextInfo = context;
            var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/rockfilebrowser";
            iframeUrl += "?rootFolder=" + encodeURIComponent(context.options.rockFileBrowserOptions.imageFolderRoot);
            iframeUrl += "&browserMode=image";
            iframeUrl += "&fileTypeBlackList=" + encodeURIComponent(context.options.rockFileBrowserOptions.fileTypeBlackList);
            iframeUrl += "&imageFileTypeWhiteList=" + encodeURIComponent(context.options.rockFileBrowserOptions.imageFileTypeWhiteList);
            iframeUrl += "&theme=" + context.options.rockTheme;

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $('body iframe').contents().on('click', '.js-select-file-button', function () {
                Rock.controls.modal.close();
                var fileResult = $('body iframe').contents().find('.js-filebrowser-result input[type=hidden]').val();
                if (fileResult) {
                    // iframe returns the result in the format "imageSrcUrl|imageAltText"
                    var resultParts = fileResult.split('|');
                    var imageElement = document.createElement('img');
                    var url = Rock.settings.get('baseUrl') + resultParts[0];
                    var altText = resultParts[1];

                    // insert the image at 25% to get them started
                    context.invoke('editor.insertImage', url, function ($image) {
                        $image.css('width', '25%');
                        $image.attr('alt', altText);
                    });
                }
            });

            $('body iframe').contents().on('click', '.js-cancel-file-button', function () {
                Rock.controls.modal.close();
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