(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentImageComponent = $(false);

  Rock.controls.emailEditor.imageComponentHelper = (function () {
    var exports = {
      initializeEventHandlers: function () {
        // No event handlers are needed for the asset picker

        var self = this;
        $('#component-image-imgcsswidth, #component-image-imagealign').on('change', function (e)
        {
          self.setImageCss();
        });

        $('#component-image-imageheight, #component-image-imagewidth, #component-image-resizemode, #component-image-alt').on('change', function (e)
        {
            self.setImageSrc();
            self.setAssetImageSrc();
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
      setProperties: function ($imageComponent) {
        Rock.controls.emailEditor.$currentImageComponent = $imageComponent;
        var $img = $imageComponent.find('img');
        var imageUrl = $img.attr('src');
        var imageCssWidth = $img.attr('data-imgcsswidth') || 'full';
        var imageAlign = $imageComponent.css('text-align');

        var imageWidth = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-width');
        var imageHeight = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-height');
        var imageResizeMode = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-resizemode');
        var imageAltText = $img.attr('alt');

        var $assetThumbnail = $('.js-asset-thumbnail');
        var $assetThumbnailName = $('.js-asset-thumbnail-name');

        if (Rock.controls.emailEditor.$currentImageComponent.attr('data-image-AssetStorageProviderId') !== undefined) {
          // This indicates the asset manager should be used, so set the thumbnail and show it
          var assetIconPath = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-IconPath');
          if (assetIconPath !== undefined) {
            var fileName = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-name');
            $assetThumbnailName.text(fileName || '');
            $assetThumbnailName.attr('title', fileName || '');
            $assetThumbnailName.removeClass('file-link-default');
            $assetThumbnail.attr('style', 'background-image:url(' + assetIconPath + ')');

            $('.js-image-picker-type-asset').removeClass("btn-default");
            $('.js-image-picker-type-asset').addClass("btn-primary");
            $('.js-image-picker-type-image').removeClass("btn-primary");
            $('.js-image-picker-type-image').addClass("btn-default");
            $('#componentImageUploader').hide();
            $('.js-component-asset-manager').show();
            }

          $('#componentImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("/Assets/Images/image-placeholder.jpg")');
        } else {
          $('#componentImageUploader').find('.imageupload-thumbnail-image').css('background-image', 'url("' + imageUrl + '")');
          if ($assetThumbnailName.length !== 0) {
            $assetThumbnailName.text('');
            $assetThumbnailName.attr('title', '');
            $assetThumbnailName.addClass('file-link-default');

            $('.js-image-picker-type-asset').removeClass("btn-primary");
            $('.js-image-picker-type-asset').addClass("btn-default");
            $('.js-image-picker-type-image').removeClass("btn-default");
            $('.js-image-picker-type-image').addClass("btn-primary");
            $('#componentImageUploader').show();
            $('.js-component-asset-manager').hide();
            }

            if ($assetThumbnail.length !== 0) {
            $assetThumbnail.attr('style', '');
            }
          }
        
        if (imageCssWidth === 'full') {
          $('#component-image-imgcsswidth').val(1);
        }
        else {
          $('#component-image-imgcsswidth').val(0);
        }

        $('#component-image-imagealign').val(imageAlign);

        $('#component-image-imagewidth').val(imageWidth);
        $('#component-image-imageheight').val(imageHeight);
        $('#component-image-resizemode').val(imageResizeMode);
        $('#component-image-alt').val(imageAltText);

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

        // Return this to the previous function caller
        return Rock.controls.emailEditor.$currentImageComponent;
      },
      handleImageUpdate: function (e, data) {
        Rock.controls.emailEditor.imageComponentHelper.setImageFromData(data);
      },
        handleAssetUpdate: function (e, data) {

        if (data !== undefined && typeof (data) === 'string') {
          var parsedData = ''
          if (data.includes('AssetStorageProviderId')) {
            parsedData = JSON.parse(data);
          }

          // maybe check for an access denied error here and display it.
          Rock.controls.emailEditor.imageComponentHelper.setAssetFromData(e, parsedData);
        }
      },
      setImageCss: function () {
        var cssWidth = $('#component-image-imgcsswidth').val();

        var $currentImg = Rock.controls.emailEditor.$currentImageComponent.find('img');

        if (cssWidth === '0') {
          $currentImg.css('width', 'auto');
          $currentImg.attr('data-imgcsswidth', 'image');
        }
        else {
          $currentImg.css('width', '100%');
          $currentImg.attr('data-imgcsswidth', 'full');
        }

        Rock.controls.emailEditor.$currentImageComponent.css('text-align', $('#component-image-imagealign').val());
      },
      setMargins: function () {
        Rock.controls.emailEditor.$currentImageComponent
          .css('margin-top', Rock.controls.util.getValueAsPixels($('#component-image-margin-top').val()))
          .css('margin-left', Rock.controls.util.getValueAsPixels($('#component-image-margin-left').val()))
          .css('margin-right', Rock.controls.util.getValueAsPixels($('#component-image-margin-right').val()))
          .css('margin-bottom', Rock.controls.util.getValueAsPixels($('#component-image-margin-bottom').val()));
      },
      setImageFromData: function (data) {
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-AssetStorageProviderId', undefined);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Key', undefined);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-IconPath', undefined);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Name', undefined);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Url', undefined);
        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', undefined)

        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id', data ? data.response().result.Id : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-guid', data ? data.response().result.Guid : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename', data ? data.response().result.FileName : null);

        Rock.controls.emailEditor.imageComponentHelper.setImageSrc();
      },
      setAssetFromData: function (e, data) {
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id', undefined);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename', undefined);
        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', undefined)

        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-AssetStorageProviderId', data ? data.AssetStorageProviderId : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Key', data ? data.Key : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-IconPath', data ? data.IconPath : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Name', data ? data.Name : null);
        Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Url', data ? data.Url : null);

        Rock.controls.emailEditor.imageComponentHelper.setAssetImageSrc();
      },
      setImageWrapAnchor: function () {
        var $imageLinkInput = $('#component-image-link');
        var imageLinkUrl = $imageLinkInput.val();
        var $img = Rock.controls.emailEditor.$currentImageComponent.find('img');
        if (imageLinkUrl && imageLinkUrl !== '') {
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
      setImageSrc: function () {
        var binaryFileId = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-id');
        var binaryFileGuid = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-guid');
        var imageUrl;
        if (!binaryFileId) {
          imageUrl = $($('.js-emaileditor-iframe').contents().find('#editor-toolbar').find('.component-image').attr('data-content')).prop('src');
        } else {
          imageUrl = Rock.settings.get('baseUrl') + 'GetImage.ashx?isBinaryFile=T';
          imageUrl += '&guid=' + binaryFileGuid;
          imageUrl += '&fileName=' + Rock.controls.emailEditor.$currentImageComponent.attr('data-image-filename');
        }

        var imageWidth = parseInt($('#component-image-imagewidth').val()) || '';
        var imageHeight = parseInt($('#component-image-imageheight').val()) || '';
        var imageResizeMode = $('#component-image-resizemode').val();
        var imageAltText = $('#component-image-alt').val();

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

        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', imageUrl).attr('alt', imageAltText);
      },
      setAssetImageSrc: function () {
        // Also called by property changes, so don't do anything if this is not an asset.
        if (Rock.controls.emailEditor.$currentImageComponent.attr('data-image-AssetStorageProviderId') === undefined) {
            return;
        }

        var imageWidth = parseInt($('#component-image-imagewidth').val()) || '';
        Rock.controls.emailEditor.$currentImageComponent.css('width', imageWidth);

        var imageHeight = parseInt($('#component-image-imageheight').val()) || '';
        Rock.controls.emailEditor.$currentImageComponent.css('height', imageHeight);

        var imageAltText = $('#component-image-alt').val();
        var imageUrl = Rock.controls.emailEditor.$currentImageComponent.attr('data-image-Url');
        Rock.controls.emailEditor.$currentImageComponent.find('img').attr('src', imageUrl).attr('alt', imageAltText);
      }
    }

    return exports;

  }());
}(jQuery));


