CKEDITOR.dialog.add('rockmergefieldDialog', function (editor) {
    var iframeUrl = Rock.settings.get('baseUrl') + "ckeditorplugins/RockMergeField?mergeFields=" + encodeURIComponent(editor.config.rockMergeFieldOptions.mergeFields);
    iframeUrl += "&theme=" + editor.config.rockTheme;
    return {
        title: 'Select Merge Field',
        minWidth: 400,
        minHeight: 350,
        resizable: CKEDITOR.DIALOG_RESIZE_NONE,
        editorId: editor.id,
        contents: [
            {
                id: 'tab0',
                label: '',
                title: '',
                elements: [
                    {
                        type: 'html',
                        html: "<iframe id='iframe-rockmergefield_" + editor.id + "' src='" + iframeUrl + "' style='width: 100%; height:350px;' scrolling='no' /> \n"
                    }
                ]
            }
        ],
        onLoad: function (eventParam) {
        },
        onShow: function (eventParam) {
        },
        onOk: function (sender) {
            var mergeFields = $('#iframe-rockmergefield_' + editor.id).contents().find('.js-mergefieldpicker-result input[type=hidden]').val();
            var url = Rock.settings.get('baseUrl') + 'api/MergeFields/' + encodeURIComponent(mergeFields);
            $.get(url, function (data) {
                {
                    editor.insertHtml(data);
                }
            });
        }
    };
});