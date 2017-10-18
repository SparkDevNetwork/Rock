(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentTextComponent = $(false);

  Rock.controls.emailEditor.textComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;
        $('#component-text-backgroundcolor').colorpicker().on('changeColor', function ()
        {
          self.setBackgroundColor();
        });

        $('#component-text-border-color').colorpicker().on('changeColor', function ()
        {
          self.setBorderColor();
        });

        $('#component-text-border-width').on('change', function ()
        {
          self.setBorderWidth();
        });

        $('#component-text-padding-top,#component-text-padding-left,#component-text-padding-right,#component-text-padding-bottom').on('change', function (e)
        {
          // just keep the numeric portion in case they included alpha chars
          $(this).val(parseFloat($(this).val()) || '');

          self.setPadding();
        });

        $('.js-component-text-lineheight').on('change', function ()
        {

          var lineHeight = $(this).val();
          Rock.controls.emailEditor.$currentTextComponent.css('line-height', lineHeight);
          Rock.controls.emailEditor.$currentTextComponent.find('p').css('line-height', lineHeight);
        });
      },
      setProperties: function ($textComponent)
      {
        Rock.controls.emailEditor.$currentTextComponent = $textComponent.hasClass('component-text') ? $currentComponent : $(false);

        // create a special inner div (if there isn't one already) that we can put border, padding, etc on
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        if (!$innerWrapper.length) {
          Rock.controls.emailEditor.$currentTextComponent.wrapInner("<div class='js-component-text-wrapper'></div>")
          $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        }

        $('.js-component-text-htmlEditor').summernote('code', $innerWrapper.html());
        var textEl = $innerWrapper[0];

        $('#component-text-backgroundcolor').colorpicker('setValue', $textComponent.css('backgroundColor'));
        
        $('#component-text-border-color').colorpicker('setValue', $innerWrapper[0].style['border-color']);
        $('#component-text-border-width').val(parseFloat($innerWrapper[0].style['border-width']) || '');

        $('#component-text-padding-top').val(parseFloat(textEl.style['padding-top']) || '');
        $('#component-text-padding-left').val(parseFloat(textEl.style['padding-left']) || '');
        $('#component-text-padding-right').val(parseFloat(textEl.style['padding-right']) || '');
        $('#component-text-padding-bottom').val(parseFloat(textEl.style['padding-bottom']) || '');

        $('.js-component-text-lineheight').val(textEl.style['line-height']);
      },
      setBackgroundColor: function ()
      {
        var color = $('#component-text-backgroundcolor').colorpicker('getValue');
        Rock.controls.emailEditor.$currentTextComponent.css('backgroundColor', color);
      },
      setBorderColor: function ()
      {
        var color = $('#component-text-border-color').colorpicker('getValue');
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        $innerWrapper.css('border-color', color);
      },
      setBorderWidth: function ()
      {
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        
        var borderWidth = Rock.controls.util.getValueAsPixels($('#component-text-border-width').val());
        if (borderWidth) {
          $innerWrapper.css('border-style', 'solid');
        }
        else {
          $innerWrapper.css('border-style', '');
        }

        $innerWrapper.css('border-width', borderWidth);
      },
      setPadding: function ()
      {
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        $innerWrapper
                .css('padding-top', Rock.controls.util.getValueAsPixels($('#component-text-padding-top').val()))
                .css('padding-left', Rock.controls.util.getValueAsPixels($('#component-text-padding-left').val()))
                .css('padding-right', Rock.controls.util.getValueAsPixels($('#component-text-padding-right').val()))
                .css('padding-bottom', Rock.controls.util.getValueAsPixels($('#component-text-padding-bottom').val()));
      },
      updateTextComponent: function (el, contents)
      {
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        if ($innerWrapper.length) {
          $innerWrapper.html(contents);
        }
      }
    }

    return exports;

  }());
}(jQuery));
