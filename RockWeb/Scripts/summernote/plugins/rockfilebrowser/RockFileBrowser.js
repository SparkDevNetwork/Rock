var RockFileBrowser = function (context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-file-text-o"/>',
        tooltip: 'File Browser',
        click: function () {
            var contextInfo = context;
            var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/rockfilebrowser";
            iframeUrl += "?rootFolder=" + encodeURIComponent(context.options.rockFileBrowserOptions.documentFolderRoot);
            iframeUrl += "&browserMode=doc";
            iframeUrl += "&fileTypeBlackList=" + encodeURIComponent(context.options.rockFileBrowserOptions.fileTypeBlackList);
            iframeUrl += "&theme=" + context.options.rockTheme;

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $('body iframe').contents().on('click', '.js-select-file-button', function () {
                Rock.controls.modal.close();
                var fileResult = $('body iframe').contents().find('.js-filebrowser-result input[type=hidden]').val();
                if (fileResult) {

                    // iframe returns the result in the format "href|text"
                    var resultParts = fileResult.split('|');

                    context.invoke('editor.createLink', {
                        text: resultParts[1],
                        url: Rock.settings.get('baseUrl') + resultParts[0],
                        newWindow: false
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