var RockImageLink = function (context)
{
  var ui = $.summernote.ui;

  var body = '<div class="form-group">' +
                  '<label>' + 'Link' + '</label>' +
                  '<input type="text" class="js-image-link form-control" />' +
              '</div>';
  var footer = '<button href="#" class="btn btn-primary js-set-image-link-btn" >' + 'OK' + '</button>';

  var $dialog = ui.dialog({
    className: 'rockimagelink-dialog',
    title: 'Set Image Link',
    body: body,
    footer: footer
  }).render().appendTo($(document.body));

  $dialog.find('.js-set-image-link-btn').on('click', { dialog: $dialog }, function (a)
  {
    var $dialog = a.data.dialog;
    ui.hideDialog($dialog);

    var $imageLinkInput = $dialog.find('.js-image-link');
    var imageLinkUrl = $imageLinkInput.val();
    var $img = $(context.layoutInfo.editable.data('target'));
    if (imageLinkUrl && imageLinkUrl != '') {
      if ($img.parent().is('a')) {
        $img.parent().attr('href', imageLinkUrl);
      }
      else {
        var linkTag = "<a href='" + imageLinkUrl + "'></a>"
        $img.wrap(linkTag);
      }
    }
    else {
      if ($img.parent().is('a')) {
        $img.unwrap();
      }
    }
  });

  // create button
  var button = ui.button({
    contents: '<i class="fa fa-link"/>',
    className: 'js-rockimagelink',
    tooltip: 'Set Image Link',
    click: function (a)
    {
      var $img = $(context.layoutInfo.editable.data('target'));
      var $imageLinkInput = $dialog.find('.js-image-link');
      if ($img.parent().is('a')) {
        $imageLinkInput.val($img.parent().attr('href'));
      }
      else {
        $imageLinkInput.val('');
      }
      ui.showDialog($dialog);
    }
  });

  return button.render();   // return button as jquery object 
}