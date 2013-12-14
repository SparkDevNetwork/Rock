(function () {
    CKEDITOR.plugins.add('rockmergefield', {
        init: function (editor) {

            editor.addCommand('insertMergeField', {
                exec: function (editor) {
                    editor.insertHtml('{{some merge field}}');
                }
            });

            var newbutton = editor.ui.add('rockmergefield', CKEDITOR.UI_BUTTON, {
                label: 'Insert Merge Field',
                command: 'insertMergeField',
                icon: this.path + 'rockmergefield.png',
                text: 'Hello World',
                toolbar: 'mode,10'
            });
        }
    });
})()