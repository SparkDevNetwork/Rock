(function () {
    CKEDITOR.plugins.add('rockmergefield', {
        init: function (editor) {
            editor.addCommand('rockmergefieldDialog', new CKEDITOR.dialogCommand('rockmergefieldDialog'));

            editor.ui.addButton && editor.ui.addButton('rockmergefield', {
                label: 'Merge Field',
                command: 'rockmergefieldDialog',
                icon: this.path + 'rockmergefield.png'
            });

            CKEDITOR.dialog.add('rockmergefieldDialog', this.path + 'dialogs/rockmergefield.js');
        }
    });
})()