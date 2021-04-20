var RockPasteFromWord = function (context) {
  var ui = $.summernote.ui;

  // set a maximumImageFileSize to prevent images from getting pasted and triggering a summernote image upload from the clipboard (only seems to happen on Edge)
  context.options.maximumImageFileSize = 1;

  // remove the summernote clipboard keydown handler since this can cause problems on FF and IE, and we are handling it here anyways
  $(context.layoutInfo.note).off('summernote.keydown');

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
    '<div contentEditable=true class="js-paste-area form-control" style="height: 300px; overflow:hidden;" />' +
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
    var $pasteArea = $dialog.find('.js-paste-area');
    var pastedContent = $pasteArea.html();

    // now that we have the pastedContent from the div, remove the html from the dom so that any css doesn't spill over into the doc
    $pasteArea.html('');

    var imgBase64 = /.*src=\"data:image\/([a-zA-Z]*);base64,([^\"]*)\"/g;

    if (imgBase64.test(pastedContent)) {
      // don't allow pasting content with base64 image data
      pastedContent = "";
    }

    // some browsers might just paste the data:image portion of the pasted base64 image
    var imgBase64FF = /.*data:image\/([a-zA-Z]*);base64,([^\"]*)/g;
    if (imgBase64FF.test(pastedContent)) {
      // don't allow pasting content with base64 image data
      pastedContent = "";
    }

    // if copying from Word, the html might be a giant glob of stuff but with a StartFragment/EndFragment section that we can grab
    var isWordDocument = /<meta.*content="Word.Document">/g.test(pastedContent);
    var containsHtmlTags = /<[a-z][\s\S]*>/i.test(pastedContent);
    var wordPastedFragment = /<!--StartFragment-->*[\s\S]*<!--EndFragment-->/g
    var cleanedContent;
    if (isWordDocument && wordPastedFragment.test(pastedContent)) {
      // we know we are getting a StartFragment/EndFragment paste from Word, so lets start with the fragment
      cleanedContent = pastedContent.match(wordPastedFragment)[0];
    }
    else if (containsHtmlTags) {
      // the pastedContent seems to contain HTML tags and probably isn't from a Word Document, so let's start with the raw paste
      cleanedContent = pastedContent;
    }
    else {
      // just plain text, so convert newlines to html breaks to help it look the same
      cleanedContent = pastedContent.replace(/(\n)/g, '<br />');
    }

    cleanedContent = cleanText(cleanedContent).trim();
    cleanedContent = cleanParagraphs(cleanedContent);
    if (cleanedContent && cleanedContent != '') {
      context.invoke('editor.pasteHTML', cleanedContent);
    }
  });

  function doPasteFromWord(pasteEvent, mimeType) {
    context.invoke('editor.saveRange');
    mimeType = mimeType || "text/html";

    // make the dialog transparent until we know for sure that we need to prompt and paste
    $dialog.fadeTo(0, 0);

    $dialog.find('.js-paste-area').html('');
    ui.showDialog($dialog);
    $dialog.find('.js-paste-area').trigger("focus");

    var clipboardData = pasteEvent.originalEvent && ((typeof (pasteEvent.originalEvent.clipboardData) != 'undefined' && pasteEvent.originalEvent.clipboardData) || (typeof (window.clipboardData) != 'undefined' && window.clipboardData));

    if (clipboardData && clipboardData.types) {
      var types = clipboardData.types;
      if (((types instanceof DOMStringList) && types.contains(mimeType)) || (types.indexOf && types.indexOf(mimeType) !== -1)) {
        var htmlPasteContent = clipboardData.getData(mimeType)
        $dialog.find('.js-paste-area').html(htmlPasteContent);
        $dialog.find('.js-paste-word-btn').trigger('click');
        return;
      }
    }

    // if we are not allowed to directly call a Paste, prompt in the dialog
    if (document.execCommand('Paste')) {
      $dialog.find('.js-paste-word-btn').trigger('click');
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
