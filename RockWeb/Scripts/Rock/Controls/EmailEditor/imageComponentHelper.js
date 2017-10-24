(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentImageComponent = $(false);

  Rock.controls.emailEditor.imageComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;
        $('#component-image-imgcsswidth, #component-image-imagealign').on('change', function (e)
        {
          self.setImageCss();
        });

        $('#component-image-imageheight, #component-image-imagewidth, #component-image-resizemode').on('change', function (e)
        {
          self.setImageSrc();
        });

        $('#component-image-margin-top,#component-image-margin-left,#component-image-margin-right,#component-image-margin-bottom').on('change', function (e)
        {
          $(this).val(parseFloat($(this).val()) || '');
          self.setMargins();
        });

        $('#component-image-link').on('blur', function ()
        {
          self.setImageWrapAnchor();
        });
      },
      setProperties: function ($imageComponent)
      {
        Rock.controls.emailEditor.$currentImageComponent = $imageComponent;
        var $img = $imageComponent.find('img');
        var imageUrl = $img.attr('src');
        var imageCssWidth = $img.attr('data-imgcsswidth') || 'full';
        var imageAlign = $imageComponent.css('text-align');

        var imageWidth = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-width');
        var imageHeight = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-height');
        var imageResizeMode = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-resizemode');

        $('#componentImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("' + imageUrl + '")');

        if (imageCssWidth == 'full') {
          $('#component-image-imgcsswidth').val(1);
        }
        else {
          $('#component-image-imgcsswidth').val(0);
        }

        $('#component-image-imagealign').val(imageAlign);

        $('#component-image-imagewidth').val(imageWidth);
        $('#component-image-imageheight').val(imageHeight);
        $('#component-image-resizemode').val(imageResizeMode);
        
        var imageEl = $imageComponent[0];

        $('#component-image-margin-top').val(parseFloat(imageEl.style['margin-top']) || '');
        $('#component-image-margin-left').val(parseFloat(imageEl.style['margin-left']) || '');
        $('#component-image-margin-right').val(parseFloat(imageEl.style['margin-right']) || '');
        $('#component-image-margin-bottom').val(parseFloat(imageEl.style['margin-bottom']) || '');
        
        var $imageLinkInput = $('#component-image-link');
        if ($img.parent().is('a')) {
          $imageLinkInput.val($img.parent().attr('href'));
        }
        else {
          $imageLinkInput.val('');
        }
      },
      handleImageUpdate: function (e, data)
      {
        Rock.controls.emailEditor.imageComponentHelper.setImageFromData(data);
      },
      setImageCss: function ()
      {
        var cssWidth = $('#component-image-imgcsswidth').val();

        var $currentImg = Rock.controls.emailEditor.$currentImageComponent.find('img');

        if (cssWidth == 0) {
          $currentImg.css('width', 'auto');
          $currentImg.attr('data-imgcsswidth', 'image');
        }
        else {
          $currentImg.css('width', '100%');
          $currentImg.attr('data-imgcsswidth', 'full');
        }

        Rock.controls.emailEditor.$currentImageComponent.css('text-align', $('#component-image-imagealign').val());
      },
      setMargins: function ()
      {
        Rock.controls.emailEditor.$currentImageComponent
                .css('margin-top', Rock.controls.util.getValueAsPixels($('#component-image-margin-top').val()))
                .css('margin-left', Rock.controls.util.getValueAsPixels($('#component-image-margin-left').val()))
                .css('margin-right', Rock.controls.util.getValueAsPixels($('#component-image-margin-right').val()))
                .css('margin-bottom', Rock.controls.util.getValueAsPixels($('#component-image-margin-bottom').val()));
      },
      setImageFromData: function (data)
      {
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id', data ? data.response().result.Id : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename', data ? data.response().result.FileName : null);

        Rock.controls.emailEditor.imageComponentHelper.setImageSrc();
      },
      setImageWrapAnchor: function ()
      {
        var $imageLinkInput = $('#component-image-link');
        var imageLinkUrl = $imageLinkInput.val();
        var $img = Rock.controls.emailEditor.$currentImageComponent.find('img');
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
      },
      setImageSrc: function ()
      {
        var binaryFileId = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id');
        var imageUrl;
        if (!binaryFileId) {
          imageUrl = $($('.js-emaileditor-iframe').contents().find('#editor-toolbar').find('.component-image').attr('data-content')).prop('src');
        } else {
          imageUrl = Rock.settings.get('baseUrl')
                  + 'GetImage.ashx?'
                  + 'isBinaryFile=T'
                  + '&id=' + binaryFileId
                  + '&fileName=' + Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename');
        }

        var imageWidth = parseInt($('#component-image-imagewidth').val());
        var imageHeight = parseInt($('#component-image-imageheight').val());
        var imageResizeMode = $('#component-image-resizemode').val();

        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-width', imageWidth);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-height', imageHeight);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-resizemode', imageResizeMode);

        if (binaryFileId && imageWidth) {
          imageUrl += '&width=' + imageWidth;
        }

        if (binaryFileId && imageHeight) {
          imageUrl += '&height=' + imageHeight;
        }

        if (binaryFileId && imageResizeMode) {
          imageUrl += '&mode=' + imageResizeMode;
        }

        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', imageUrl);
      }
    }

    return exports;

  }());
}(jQuery));


