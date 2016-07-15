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
            var clipboardData = ((typeof (e.originalEvent.clipboardData) != 'undefined' && e.originalEvent.clipboardData) || (typeof (window.clipboardData) != 'undefined' && window.clipboardData));
            if (clipboardData) {
                var types = clipboardData.types;
                if (((types instanceof DOMStringList) && types.contains("text/rtf")) || (types.indexOf && types.indexOf('text/rtf') !== -1)) {
                    e.preventDefault();
                    doPasteFromWord(e, "text/html");
                } else {
                    // sometimes Microsoft Edge will support the clipboardData api, but doesn't tell us about types other than html/plain
                    e.preventDefault();
                    doPasteFromWord(e, "text/plain");
                }

            } else {
                // if the browser doesn't have a Clipboard API, always prompt since we don't know what is on the clipboard and we don't want word data (should only happen on MS Edge)
                // As of 7/15/2016, Microsoft Edge doesn't have ClipboardAPI! see https://developer.microsoft.com/en-us/microsoft-edge/platform/status/clipboardapi
                e.preventDefault();
                context.layoutInfo.toolbar.find('.js-rockpastetext').click();
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
                    '<label>' + 'Paste the word content below, then press the Insert button' + '</label>' +
                    '<div contentEditable=true class="note-editor note-frame js-paste-area" style="height: 300px; overflow:hidden;" />' +
                '</div>';
    var footer = '<button href="#" class="btn btn-primary js-paste-word-btn" >' + 'Insert' + '</button>';

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

    function doPasteFromWord(pasteEvent, mimeType) {
        context.invoke('editor.saveRange');
        mimeType = mimeType || "text/html";

        // make the dialog transparent until we know for sure that we need to prompt and paste
        $dialog.fadeTo(0, 0);

        $dialog.find('.js-paste-area').html('');
        ui.showDialog($dialog);
        $dialog.find('.js-paste-area').focus();

        var clipboardData = pasteEvent.originalEvent && ((typeof (pasteEvent.originalEvent.clipboardData) != 'undefined' && pasteEvent.originalEvent.clipboardData) || (typeof (window.clipboardData) != 'undefined' && window.clipboardData));

        if (clipboardData && clipboardData.types) {
            var types = clipboardData.types;
            if (((types instanceof DOMStringList) && types.contains(mimeType)) || (types.indexOf && types.indexOf(mimeType) !== -1)) {
                var htmlPasteContent = clipboardData.getData(mimeType)
                $dialog.find('.js-paste-area').html(htmlPasteContent);
                $dialog.find('.js-paste-word-btn').click();
                return;
            }
        }

        // if we are not allowed to directly call a Paste, prompt in the dialog
        if (document.execCommand('Paste')) {
            $dialog.find('.js-paste-word-btn').click();
            return;
        };

        // if we weren't able to automatically take care of it, show the dialog
        $dialog.fadeTo(0, 1);
    }

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-file-word-o"/>',
        className: 'js-rockpastefromword',
        tooltip: 'Paste from Word',
        click: function (a) {
            doPasteFromWord(a);
        }
    });

    return button.render();   // return button as jquery object 
}