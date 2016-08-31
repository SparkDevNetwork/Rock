var RockPasteText = function (context) {
    var ui = $.summernote.ui;

    var body = '<div class="form-group">' +
                    '<label>' + 'Paste the content below, then press the Insert button to insert the content as plain text' + '</label>' +
                    '<div contentEditable=true class="note-editor note-frame js-paste-area" style="height: 300px; overflow:hidden;" />' +
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

    function doPasteText(pasteEvent) {
        context.invoke('editor.saveRange');
        // make the dialog transparent until we know for sure that we need to prompt and paste
        $dialog.fadeTo(0, 0);

        ui.showDialog($dialog);
        $dialog.find('.js-paste-area').html('');
        $dialog.find('.js-paste-area').focus();

        var clipboardData = pasteEvent.originalEvent && ((typeof (pasteEvent.originalEvent.clipboardData) != 'undefined' && pasteEvent.originalEvent.clipboardData) || (typeof (window.clipboardData) != 'undefined' && window.clipboardData));

        if (clipboardData && clipboardData.types) {
            var types = clipboardData.types;
            if (((types instanceof DOMStringList) && types.contains("text/plain")) || (types.indexOf && types.indexOf('text/plain') !== -1)) {
                var textPasteContent = clipboardData.getData('text/plain')
                $dialog.find('.js-paste-area').html(textPasteContent);
                $dialog.find('.js-paste-text-btn').click();
                return;
            }
        }

        // if we are allowed to directly call a Paste, automatically paste and close the dialog
        if (document.execCommand('Paste')) {
            $dialog.find('.js-paste-text-btn').click();
            return;
        }
        
        // if we weren't able to automatically take care of it, show the dialog
        $dialog.fadeTo(0, 1);
    }

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-clipboard"/>',
        className: 'js-rockpastetext',
        tooltip: 'Paste Text',
        click: function (a) {
            doPasteText(a);
        }
    });

    return button.render();   // return button as jquery object 
}