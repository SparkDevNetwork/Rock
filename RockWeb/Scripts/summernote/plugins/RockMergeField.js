var RockMergeField = function (context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '{ }',
        tooltip: 'Merge Field',
        click: function () {
            context.invoke('editor.saveRange');
            var iframeUrl = Rock.settings.get('baseUrl') + "htmleditorplugins/RockMergeField?mergeFields=" + encodeURIComponent(context.options.rockMergeFieldOptions.mergeFields);
            iframeUrl += "&theme=" + context.options.rockTheme;
            iframeUrl += "&modalMode=1";

            Rock.controls.modal.show(context.layoutInfo.editor, iframeUrl);

            $modalPopupIFrame = Rock.controls.modal.getModalPopupIFrame();

            $modalPopupIFrame.load(function () {

                $modalPopupIFrame.contents().off('click');

                $modalPopupIFrame.contents().on('click', '.js-select-mergefield-button', function () {
                    Rock.controls.modal.close();

                    var mergeFields = $('body iframe').contents().find('.js-mergefieldpicker-result input[type=hidden]').val();
                    var url = Rock.settings.get('baseUrl') + 'api/MergeFields/' + encodeURIComponent(mergeFields);
                    $.get(url, function (data) {
                        {
                            context.invoke('editor.restoreRange');
                            context.invoke('editor.pasteHTML', data);
                        }
                    });
                });

                $modalPopupIFrame.contents().on('click', '.js-cancel-mergefield-button', function () {
                    Rock.controls.modal.close();
                });
            });

            
        }
    });
    
    if (context.options.rockMergeFieldOptions.enabled) {
        return button.render();   // return button as jquery object 
    }
    else {
        return null;
    }
}