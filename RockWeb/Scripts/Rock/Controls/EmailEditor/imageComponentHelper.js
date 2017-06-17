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
      },
      setProperties: function ($imageComponent)
      {
        Rock.controls.emailEditor.$currentImageComponent = $imageComponent;
        var imageUrl = $imageComponent.find('img').attr('src');
        var imageCssWidth = $imageComponent.find('img').attr('data-imgcsswidth');
        var imageAlign = $imageComponent.css('text-align');

        $('#componentImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("' + imageUrl + '")');

        if (imageCssWidth == 'full') {
          $('#component-image-imgcsswidth').val(1);
        }
        else {
          $('#component-image-imgcsswidth').val(0);
        }

        $('#component-image-imagealign').val(imageAlign);
      },
      handleImageUpdate: function (e, data)
      {
        if (data != null) {
          Rock.controls.emailEditor.imageComponentHelper.setImageFromData(data);
        }
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
      setImageFromData: function (data)
      {
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id', data.response().result.Id);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename', data.response().result.FileName);

        Rock.controls.emailEditor.imageComponentHelper.setImageSrc();
      },
      setImageSrc: function ()
      {
        var binaryFileId = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id');
        if (!binaryFileId)
        {
          var placeholderUrl = $('#editor-toolbar').find('.component-image').attr('data-content');
          Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', placeholderUrl);
          return;
        }

        var imageUrl = Rock.settings.get('baseUrl')
                + 'GetImage.ashx?'
                + 'isBinaryFile=T'
                + '&id=' + binaryFileId
                + '&fileName=' + Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename');

        var imageWidth = parseInt($('#component-image-imagewidth').val());
        var imageHeight = parseInt($('#component-image-imageheight').val());
        var imageResizeMode = $('#component-image-resizemode').val();
        
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-width', imageWidth);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-height', imageHeight);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-resizemode', imageResizeMode);
        
        if (imageWidth) {
          imageUrl += '&width=' + imageWidth;
        }
        
        if (imageHeight) {
          imageUrl += '&height=' + imageHeight;
        }

        if (imageResizeMode) {
          imageUrl += '&mode=' + imageResizeMode;
        }

        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', imageUrl);
      }
    }

    return exports;

  }());
}(jQuery));

// initialize
Rock.controls.emailEditor.imageComponentHelper.initializeEventHandlers();

