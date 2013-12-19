(function () {
    CKEDITOR.plugins.add('rockimagebrowser', {
        init: function (editor) {

            editor.addCommand('rockimagebrowserDialog', new CKEDITOR.dialogCommand('rockimagebrowserDialog'));

            editor.ui.addButton && editor.ui.addButton('rockimagebrowser', {
                label: 'Image Browser',
                command: 'rockimagebrowserDialog',
                icon: this.path + 'rockimagebrowser.png'
            });

            CKEDITOR.dialog.add('rockimagebrowserDialog', this.path + 'dialogs/rockimagebrowser.js');
        }
    });
})()