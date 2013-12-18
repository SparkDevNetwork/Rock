(function () {
    CKEDITOR.plugins.add('rockimagebrowser', {
        init: function (editor) {
            editor.addCommand('rockimagebrowser', {
                exec: function (editor) {
                    var timestamp = new Date();
                    editor.insertHtml('##TODO Dialog to pick/upload images##');
                }
            });
            editor.ui.addButton && editor.ui.addButton('rockimagebrowser', {
                label: 'Insert rockimagebrowser',
                command: 'rockimagebrowser',
                icon: this.path + 'rockimagebrowser.png'
            });
        }
    });
})()