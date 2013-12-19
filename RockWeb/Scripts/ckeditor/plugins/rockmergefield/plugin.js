(function () {
    CKEDITOR.plugins.add('rockmergefield', {
        init: function (editor) {
            editor.addCommand(
                'showMergeFieldPicker', {
                    exec: function (sender) {

                        // show the merge field picker, which is defined in HtmlEditor.cs
                        var editorId = $(sender.element.$).attr('id')
                        var pickerId = editorId + "_mfPicker";
                        var editorDiv = $('#' + editorId);
                        var pickerMenu = $('#' + pickerId).find('.picker-menu');

                        // center pickerMenu where the mouseclick was
                        pickerMenu.css("left", event.x - pickerMenu.width()/2);

                        pickerMenu.show(0, function (s, e) {
                            pickerMenu.focus();
                        })
                        
                    }
                }
                );

            editor.ui.addButton && editor.ui.addButton('rockmergefield', {
                label: 'Merge Field',
                command: 'showMergeFieldPicker',
                icon: this.path + 'rockmergefield.png'
            });
        }
    });
})()