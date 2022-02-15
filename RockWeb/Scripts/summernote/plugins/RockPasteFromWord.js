var RockPasteFromWord = function (context) {
  var ui = $.summernote.ui;

  // set a maximumImageFileSize to prevent images from getting pasted and triggering a summernote image upload from the clipboard (only seems to happen on Edge)
  context.options.maximumImageFileSize = 1;

  // remove the summernote clipboard keydown handler since this can cause problems on FF and IE, and we are handling it here anyways
  $(context.layoutInfo.note).off('summernote.keydown');

  // from https://github.com/StudioJunkyard/summernote-cleaner
  var cleanText = function (txt) {
    var options = {
        cleaner: {
            action: 'both', // both|button|paste 'button' only cleans via toolbar button, 'paste' only clean when pasting content, both does both options.
            newline: '<br>', // Summernote's default is to use '<p><br></p>'
            icon: '<i class="note-icon"><svg xmlns="http://www.w3.org/2000/svg" id="libre-paintbrush" viewBox="0 0 14 14" width="14" height="14"><path d="m 11.821425,1 q 0.46875,0 0.82031,0.311384 0.35157,0.311384 0.35157,0.780134 0,0.421875 -0.30134,1.01116 -2.22322,4.212054 -3.11384,5.035715 -0.64956,0.609375 -1.45982,0.609375 -0.84375,0 -1.44978,-0.61942 -0.60603,-0.61942 -0.60603,-1.469866 0,-0.857143 0.61608,-1.419643 l 4.27232,-3.877232 Q 11.345985,1 11.821425,1 z m -6.08705,6.924107 q 0.26116,0.508928 0.71317,0.870536 0.45201,0.361607 1.00781,0.508928 l 0.007,0.475447 q 0.0268,1.426339 -0.86719,2.32366 Q 5.700895,13 4.261155,13 q -0.82366,0 -1.45982,-0.311384 -0.63616,-0.311384 -1.0212,-0.853795 -0.38505,-0.54241 -0.57924,-1.225446 -0.1942,-0.683036 -0.1942,-1.473214 0.0469,0.03348 0.27455,0.200893 0.22768,0.16741 0.41518,0.29799 0.1875,0.130581 0.39509,0.24442 0.20759,0.113839 0.30804,0.113839 0.27455,0 0.3683,-0.247767 0.16741,-0.441965 0.38505,-0.753349 0.21763,-0.311383 0.4654,-0.508928 0.24776,-0.197545 0.58928,-0.31808 0.34152,-0.120536 0.68974,-0.170759 0.34821,-0.05022 0.83705,-0.07031 z"/></svg></i>',
            keepHtml: true,
            badTags: ['applet', 'col', 'colgroup', 'embed', 'noframes', 'noscript', 'script', 'style', 'title'], //Remove full tags with contents
            badAttributes: ['bgcolor', 'border', 'height', 'cellpadding', 'cellspacing', 'lang', 'start', 'style', 'valign', 'width'], //Remove attributes from remaining tags
            limitChars: 0, // 0|# 0 disables option
            limitDisplay: 'both', // none|text|html|both
            limitStop: false, // true/false
            imagePlaceholder: 'https://via.placeholder.com/200'
        }
    }
    var stringStripper = /(\n|\r| class=(")?Mso[a-zA-Z]+(")? ^p)/g;
    var output = txt.replace(stringStripper, '');
    var commentSripper = new RegExp('<!--(.*?)-->', 'g');
    var output = output.replace(commentSripper, '');
    var tagStripper = new RegExp('<(/)*(meta|link|span|\\?xml:|st1:|o:|font)(.*?)>', 'gi');
    output = output.replace(/ src="(.*?)"/gi, ' src="' + options.cleaner.imagePlaceholder + '"');
    output = output.replace(/ name="(.*?)"/gi, ' data-title="$1" alt="$1"');
    output = output.replace(tagStripper, '');
    for (var i = 0; i < options.cleaner.badTags.length; i++) {
        tagStripper = new RegExp('<' + options.cleaner.badTags[i] + '.*?' + options.cleaner.badTags[i] + '(.*?)>', 'gi');
        output = output.replace(tagStripper, '');
    }
    for (var i = 0; i < options.cleaner.badAttributes.length; i++) {
        var attributeStripper = new RegExp(options.cleaner.badAttributes[i] + '="(.*?)"', 'gi');
        output = output.replace(attributeStripper, '');
    }
    output = output.replace(/ align="(.*?)"/gi, ' class="text-$1"');
    output = output.replace(/ class="western"/gi, '');
    output = output.replace(/ class=""/gi, '');
    output = output.replace(/<b>(.*?)<\/b>/gi, '<strong>$1</strong>');
    output = output.replace(/<i>(.*?)<\/i>/gi, '<em>$1</em>');
    output = output.replace(/\s{2,}/g, ' ').trim();
    return output;
  };

  var cleanParagraphs = function (txt) {
    var out = txt;

    // remove paragraph tags and use <br/> instead
    var sS = new RegExp('<(/p)>', 'gi');
    var out = txt.replace(sS, '<br />');

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
