(function () {
    CKEDITOR.plugins.add('rockdocumentbrowser', {
        init: function (editor) {
            editor.addCommand('rockdocumentbrowser', {
                exec: function (editor) {
                    var timestamp = new Date();
                    editor.insertHtml('##TODO: dialog to pick/upload documents##');
                }
            });
            editor.ui.addButton && editor.ui.addButton('rockdocumentbrowser', {
                label: 'Insert rockdocumentbrowser',
                command: 'rockdocumentbrowser',
                icon: this.path + 'rockdocumentbrowser.png'
            });
        }
    });
})()