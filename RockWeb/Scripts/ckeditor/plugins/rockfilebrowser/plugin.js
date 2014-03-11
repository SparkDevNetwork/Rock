(function () {
    CKEDITOR.plugins.add('rockfilebrowser', {
        init: function (editor) {
            editor.addCommand('rockimagebrowserDialog', new CKEDITOR.dialogCommand('rockimagebrowserDialog'));
            editor.addCommand('rockdocbrowserDialog', new CKEDITOR.dialogCommand('rockdocbrowserDialog'));

            editor.ui.addButton && editor.ui.addButton('rockimagebrowser', {
                label: 'Image Browser',
                command: 'rockimagebrowserDialog',
                icon: this.path + 'rockimagebrowser.png'
            });

            editor.ui.addButton && editor.ui.addButton('rockdocumentbrowser', {
                label: 'Document Browser',
                command: 'rockdocbrowserDialog',
                icon: this.path + 'rockdocbrowser.png'
            });

            CKEDITOR.dialog.add('rockimagebrowserDialog', this.path + 'dialogs/rockimagebrowser.js');
            CKEDITOR.dialog.add('rockdocbrowserDialog', this.path + 'dialogs/rockdocbrowser.js');
        }
    });
})()