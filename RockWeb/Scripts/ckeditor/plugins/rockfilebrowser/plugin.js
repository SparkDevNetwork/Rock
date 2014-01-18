(function () {
    CKEDITOR.plugins.add('rockfilebrowser', {
        init: function (editor) {
            editor.addCommand('rockfilebrowserDialog', new CKEDITOR.dialogCommand('rockfilebrowserDialog'));

            editor.ui.addButton && editor.ui.addButton('rockimagebrowser', {
                label: 'Image Browser',
                command: 'rockfilebrowserDialog',
                //rockfilebrowserMode: 'images',
                icon: this.path + 'rockfilebrowser.png'
            });

            editor.ui.addButton && editor.ui.addButton('rockdocumentbrowser', {
                label: 'Document Browser',
                command: 'rockfilebrowserDialog',
                //rockfilebrowserMode: 'documents',
                icon: this.path + 'rockfilebrowser.png'
            });

            CKEDITOR.dialog.add('rockfilebrowserDialog', this.path + 'dialogs/rockfilebrowser.js');
        }
    });
})()