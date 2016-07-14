var RockPasteFromWord = function (context) {
    var ui = $.summernote.ui;
    

    $(context.layoutInfo.note).on('summernote.paste', function (we, e) {

        // catch the paste event and do either the rockpastetext or rockpastefromword if we can figure out if they are pasting from word
        var ua = window.navigator.userAgent;
        var msie = ua.indexOf("MSIE ");
        if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
            // if they are using IE. Sorry, they'll have to use the pastefromword button since IE doesn't tell us what type of data is getting pasted
            e.preventDefault();
            context.layoutInfo.toolbar.find('.js-rockpastetext').click();
        } else {
            var types = e.originalEvent.clipboardData.types;
            if (((types instanceof DOMStringList) && types.contains("text/rtf")) || (types.indexOf && types.indexOf('text/rtf') !== -1)) {
                e.preventDefault();
                context.layoutInfo.toolbar.find('.js-rockpastefromword').click();
            }
        }
    });


    // from https://github.com/StudioJunkyard/summernote-cleaner
    var cleanText = function (txt) {
        var sS = /(\n|\r| class=(")?Mso[a-zA-Z]+(")?)/g;
        var out = txt.replace(sS, ' ');
        var cS = new RegExp('<!--(.*?)-->', 'gi');
        out = out.replace(cS, '');
        var tS = new RegExp('<(/)*(meta|link|span|\\?xml:|st1:|o:|font)(.*?)>', 'gi');
        out = out.replace(tS, '');

        var nbS = new RegExp('&nbsp;', 'gi');
        out = out.replace(nbS, '');

        var bT = ['style', 'script', 'applet', 'embed', 'noframes', 'noscript'];
        for (var i = 0; i < bT.length; i++) {
            tS = new RegExp('<' + bT[i] + '.*?' + bT[i] + '(.*?)>', 'gi');
            out = out.replace(tS, '');
        }
        var bA = ['style', 'start'];
        for (var ii = 0; ii < bA.length; ii++) {
            var aS = new RegExp(' ' + bA[ii] + '="(.*?)"', 'gi');
            out = out.replace(aS, '');
        }
        return out;
    };

    var cleanParagraphs = function (txt) {
        var out = txt;
        
        // remove paragraph tags and use <br/> instead
        var sS = new RegExp('<(/p)>', 'gi');
        var out = txt.replace(sS, '<br />');

        var pS = new RegExp('<(/)*(p)(.*?)>', 'gi');
        out = out.replace(pS, '');
        
        return out;
    };

    var body = '<div class="form-group">' +
                    '<label>' + 'Paste the word content below, then press the Paste button' + '</label>' +
                    '<div contentEditable=true class="note-editor note-frame js-paste-area" style="height: 600px; overflow:hidden;" />' +
                '</div>';
    var footer = '<button href="#" class="btn btn-primary js-paste-word-btn" >' + 'Paste' + '</button>';

    var $dialog = ui.dialog({
        className: 'rockpastefromword-dialog',
        title: 'Paste from Word',
        body: body,
        footer: footer
    }).render().appendTo($(document.body));

    $dialog.find('.js-paste-word-btn').on('click', { dialog: $dialog }, function (a) {
        var $dialog = a.data.dialog;
        ui.hideDialog($dialog);

        context.invoke('editor.restoreRange');
        var pastedContent = $dialog.find('.js-paste-area').html();

        var cleaned = cleanText(pastedContent).trim();
        var cleaned = cleanParagraphs(cleaned);
        context.invoke('editor.pasteHTML', cleaned);
    });

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-file-word-o"/>',
        className: 'js-rockpastefromword',
        tooltip: 'Paste from Word',
        click: function () {
            context.invoke('editor.saveRange');
            ui.showDialog($dialog);
            $dialog.find('.js-paste-area').html('');
            $dialog.find('.js-paste-area').focus();

            // if we are not allowed to directly call a Paste, prompt in the dialog
            if (document.execCommand('Paste')) {
                $dialog.find('.js-paste-word-btn').click();
            }
        }
    });

    return button.render();   // return button as jquery object 
}