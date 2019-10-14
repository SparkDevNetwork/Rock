var RockFileBrowser = function (context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-file-text-o"/>',
        tooltip: 'File Browser',
        click: function () {
            context.invoke('editor.saveRange');
            var iframeUrl = Rock.settings.get('baseUrl') + "htmleditorplugins/rockfilebrowser";
            iframeUrl += "?rootFolder=" + encodeURIComponent(context.options.rockFileBrowserOptions.documentFolderRoot);
            iframeUrl += "&browserMode=doc";
            iframeUrl += "&fileTypeBlackList=" + encodeURIComponent(context.options.rockFileBrowserOptions.fileTypeBlackList);
            iframeUrl += "&fileTypeWhiteList=" + encodeURIComponent(context.options.rockFileBrowserOptions.fileTypeWhiteList);
            iframeUrl += "&theme=" + context.options.rockTheme;
            iframeUrl += "&modalMode=1";
            iframeUrl += "&title=Select%20File";

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $modalPopupIFrame = Rock.controls.modal.getModalPopupIFrame();

            $modalPopupIFrame.load(function () {

                $modalPopupIFrame.contents().off('click');

                $modalPopupIFrame.contents().on('click', '.js-select-file-button', function (e) {
                    Rock.controls.modal.close();
                    var fileResult = $(e.target).closest('body').find('.js-filebrowser-result input[type=hidden]').val();
                    if (fileResult) {

                        // iframe returns the result in the format "href|text"
                        var resultParts = fileResult.split('|');

                        context.invoke('editor.restoreRange');
                        context.invoke('editor.createLink', {
                            text: resultParts[1],
                            url: Rock.settings.get('baseUrl') + resultParts[0],
                            newWindow: false
                        });

                        context.invoke('triggerEvent', 'change');
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
