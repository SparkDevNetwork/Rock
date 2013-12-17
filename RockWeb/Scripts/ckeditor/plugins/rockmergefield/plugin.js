(function () {
    CKEDITOR.plugins.add('rockmergefield', {
        init: function (editor) {
            editor.addCommand(
                'showMergeFieldPicker', {
                    exec: function (sender) {

                        // show the merge field picker, which is defined in HtmlEditor.cs
                        var editorId = $(sender.element.$).attr('id')
                        var pickerId = editorId + "_mfPicker";
                        $('#' + pickerId).find('.picker-menu').show(0, function () {
                            $('#' + pickerId).find('.picker-menu').focus();
                        })
                        
                    }
                }
                );

            editor.ui.add('rockmergefield', CKEDITOR.UI_BUTTON, {
                label: 'Merge Field',
                command: 'showMergeFieldPicker',
                icon: this.path + 'rockmergefield.png'
            });
        }
    });
})()