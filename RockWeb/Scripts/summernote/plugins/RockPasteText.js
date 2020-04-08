var RockPasteText = function (context) {
  var ui = $.summernote.ui;

  var body = '<div class="form-group">' +
    '<label>' + 'Paste the content below, then press the Insert button to insert the content as plain text' + '</label>' +
    '<textarea class="js-paste-area" cols="40" rows="10" style="width:100%" ></textarea>' +
    '</div>';
  var footer = '<button href="#" class="btn btn-primary js-paste-text-btn" >' + 'Insert' + '</button>';

  // handle all paste events from summernote with this..
  $(context.layoutInfo.note).on('summernote.paste', function (we, e) {
    // catch the paste event and do click the paste-text-btn
    // derived from https://stackoverflow.com/a/31019586/1755417
    var clipboardData = ((e.originalEvent || e).clipboardData || window.clipboardData);
    if (clipboardData) {
      var bufferText = clipboardData.getData('Text');
      e.preventDefault();

      https://stackoverflow.com/a/34876744/1755417 (IE Fix)
      if (document.queryCommandSupported('insertText')) {
        document.execCommand('insertText', false, bufferText);
      } else {
        document.execCommand('paste', false, bufferText);
      }
    }
  });

  var $dialog = ui.dialog({
    className: 'rockpastetext-dialog',
    title: 'Paste as Plain Text',
    body: body,
    footer: footer
  }).render().appendTo($(document.body));

    $dialog.find('.js-paste-text-btn').on('click', { dialog: $dialog }, function (a)
    {
    var $dialog = a.data.dialog;
    ui.hideDialog($dialog);

    context.invoke('editor.restoreRange');

    var $pasteArea = $dialog.find('.js-paste-area');
    var pastedContentAsText = $pasteArea.val();

    // in case they include html tags, just get the text
    var containsHtmlTags = /<[a-z][\s\S]*>/i.test(pastedContentAsText);
    if (containsHtmlTags) {
      pastedContentAsText = $("<div/>").html(pastedContentAsText).text();
    }

    // now that we have the pastedContent from the div, remove the html from the dom
    $pasteArea.val('');

    if (pastedContentAsText && pastedContentAsText != "") {
      // convert newlines to html breaks to help it look the same
      pastedContentAsText = pastedContentAsText.replace(/(\n)/g, '<br />');
      context.invoke('editor.pasteHTML', pastedContentAsText);
    }
  });

  function doPasteText(pasteEvent) {
    context.invoke('editor.saveRange');
    // make the dialog transparent until we know for sure that we need to prompt and paste
    $dialog.fadeTo(0, 0);

    ui.showDialog($dialog);
    $dialog.find('.js-paste-area').html('');
    $dialog.find('.js-paste-area').trigger("focus");

    var clipboardData = pasteEvent.originalEvent && ((typeof (pasteEvent.originalEvent.clipboardData) != 'undefined' && pasteEvent.originalEvent.clipboardData) || (typeof (window.clipboardData) != 'undefined' && window.clipboardData));

    if (clipboardData && clipboardData.types) {
      var types = clipboardData.types;
      if (((types instanceof DOMStringList) && types.contains("text/plain")) || (types.indexOf && types.indexOf('text/plain') !== -1)) {
        var textPasteContent = clipboardData.getData('text/plain')
        $dialog.find('.js-paste-area').html(textPasteContent);
        $dialog.find('.js-paste-text-btn').trigger('click');
        return;
      }
    }

    // if we are allowed to directly call a Paste, automatically paste and close the dialog
    if (document.execCommand('Paste')) {
      $dialog.find('.js-paste-text-btn').trigger('click');
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
