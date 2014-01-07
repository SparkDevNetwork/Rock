(function () {
    CKEDITOR.plugins.add('rockfilebrowser', {
        init: function (editor) {

            editor.addCommand('rockfilebrowserDialog', new CKEDITOR.dialogCommand('rockfilebrowserDialog'));

            editor.ui.addButton && editor.ui.addButton('rockfilebrowser', {
                label: 'File Browser',
                command: 'rockfilebrowserDialog',
                icon: this.path + 'rockfilebrowser.png'
            });

            CKEDITOR.dialog.add('rockfilebrowserDialog', this.path + 'dialogs/rockfilebrowser.js');
        }
    });
})()