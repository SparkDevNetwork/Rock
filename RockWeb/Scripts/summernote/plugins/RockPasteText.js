var RockPasteText = function (context) {
    var ui = $.summernote.ui;

    var body = '<div class="form-group">' +
                    '<label>' + 'Paste the content below, then press the Insert button to insert the content as plain text' + '</label>' +
                    '<div contentEditable=true class="note-editor note-frame js-paste-area" style="height: 600px; overflow:hidden;" />' +
                '</div>';
    var footer = '<button href="#" class="btn btn-primary js-paste-text-btn" >' + 'Insert' + '</button>';

    var $dialog = ui.dialog({
        className: 'rockpastetext-dialog',
        title: 'Paste as Plain Text',
        body: body,
        footer: footer
    }).render().appendTo($(document.body));

    $dialog.find('.js-paste-text-btn').on('click', { dialog: $dialog }, function (a) {
        var $dialog = a.data.dialog;
        ui.hideDialog($dialog);

        context.invoke('editor.restoreRange');
        var text = $dialog.find('.js-paste-area').text();

        context.invoke('editor.insertText', text);
    });

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-clipboard"/>',
        className: 'js-rockpastetext',
        tooltip: 'Paste Text',
        click: function (a,b,c) {
            context.invoke('editor.saveRange');
            ui.showDialog($dialog);
            $dialog.find('.js-paste-area').html('');
            $dialog.find('.js-paste-area').focus();

            // if we are not allowed to directly call a Paste, prompt in the dialog
            if (document.execCommand('Paste')) {
                $dialog.find('.js-paste-text-btn').click();
            }
        }
    });

    return button.render();   // return button as jquery object 
}