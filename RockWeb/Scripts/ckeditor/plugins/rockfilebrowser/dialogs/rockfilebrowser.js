CKEDITOR.dialog.add('rockfilebrowserDialog', function (editor) {
    var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/rockfilebrowser?rootFolder=" + encodeURIComponent(editor.config.rockFileBrowserOptions.imageFolderRoot);
    return {
        title: 'Select File',
        minWidth: 1000,
        minHeight: 400,
        editorId: editor.id,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: "<iframe id='iframe-rockfilebrowser_" + editor.id + "' src='" + iframeUrl + "' style='width: 100%; height:400px;' /> \n"
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
            var fileResult = $('#iframe-rockfilebrowser_' + editor.id).contents().find('.js-filebrowser-result input[type=hidden]').val();
            var resultParts = fileResult.split(',');
            
            var imageHtml = '<img src="' + Rock.settings.get('baseUrl') + resultParts[0] + ' alt="' + resultParts[1] + '" />';
            editor.insertHtml(imageHtml);
        }
    };
});