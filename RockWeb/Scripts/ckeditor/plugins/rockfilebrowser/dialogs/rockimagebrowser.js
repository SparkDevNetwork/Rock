CKEDITOR.dialog.add('rockimagebrowserDialog', function (editor) {
    var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/rockfilebrowser";
    iframeUrl += "?rootFolder=" + encodeURIComponent(editor.config.rockFileBrowserOptions.imageFolderRoot);
    iframeUrl += "&browserMode=image";
    iframeUrl += "&fileTypeBlackList=" + encodeURIComponent(editor.config.rockFileBrowserOptions.fileTypeBlackList);
    iframeUrl += "&imageFileTypeWhiteList=" + encodeURIComponent(editor.config.rockFileBrowserOptions.imageFileTypeWhiteList);
    iframeUrl += "&theme=" + editor.config.rockTheme;
    return {
        title: 'Select Image',
        minWidth: 1000,
        minHeight: 420,
        editorId: editor.id,
        resizable: CKEDITOR.DIALOG_RESIZE_NONE,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: "<iframe id='iframe-rockimagebrowser_" + editor.id + "' src='" + iframeUrl + "' style='width: 100%; height:420px;' scrolling='no' /> \n"
                    }
                ]
            }
        ],
        onLoad: function (eventParam) {
        },
        onShow: function (eventParam) {
        },
        onOk: function (sender) {
            var fileResult = $('#iframe-rockimagebrowser_' + editor.id).contents().find('.js-filebrowser-result input[type=hidden]').val();
            if (fileResult) {
                // iframe returns the result in the format "imageSrcUrl,imageAltText"
                var resultParts = fileResult.split(',');
                var imageHtml = '<img src="' + Rock.settings.get('baseUrl') + resultParts[0] + '" alt="' + resultParts[1] + '" />';
                editor.insertHtml(imageHtml);
            }
        }
    };
});