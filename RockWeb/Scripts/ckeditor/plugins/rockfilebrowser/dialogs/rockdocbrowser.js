CKEDITOR.dialog.add('rockdocbrowserDialog', function (editor) {
    var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/rockfilebrowser";
    iframeUrl += "?rootFolder=" + encodeURIComponent(editor.config.rockFileBrowserOptions.documentFolderRoot);
    iframeUrl += "&browserMode=doc";
    return {
        title: 'Select File',
        minWidth: 1000,
        minHeight: 400,
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
                        html: "<iframe id='iframe-rockdocbrowser_" + editor.id + "' src='" + iframeUrl + "' style='width: 100%; height:400px;' scrolling='no' /> \n"
                    }
                ]
            }
        ],
        onLoad: function (eventParam) {
        },
        onShow: function (eventParam) {
        },
        onOk: function (sender) {
            debugger
            var fileResult = $('#iframe-rockdocbrowser_' + editor.id).contents().find('.js-filebrowser-result input[type=hidden]').val();
            if (fileResult) {
                // iframe returns the result in the format "href,text"
                var resultParts = fileResult.split(',');
                var imageHtml = '<a href="' + Rock.settings.get('baseUrl') + resultParts[0] + '" >' + resultParts[1] + '</a>';
                editor.insertHtml(imageHtml);
            }
        }
    };
});