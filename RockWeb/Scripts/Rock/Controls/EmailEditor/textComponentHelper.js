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
        Rock.controls.emailEditor.$currentTextComponent = $textComponent.hasClass('component-text') ? $textComponent : $(false);

        // replace the component div with a table that has special inner td that we can put border, padding, etc on
        var $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
        if (!$innerWrapper.length) {
          var componentCss = $textComponent.attr('class');
          var componentDataState = $textComponent.attr('data-state');
          Rock.controls.emailEditor.$currentTextComponent.wrapInner("<table><tr><td class='js-component-text-wrapper'></td></tr></table>");
          $innerWrapper = Rock.controls.emailEditor.$currentTextComponent.find('.js-component-text-wrapper');
          var $tableWrapper = $innerWrapper.closest("table");
          $tableWrapper.attr('class', componentCss);
          $tableWrapper.attr('data-state', componentDataState);
          $tableWrapper.attr('style', 'border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;');
          Rock.controls.emailEditor.$currentTextComponent.contents().unwrap();

          // DV 25-JAN-2022: This must be returned to the caller as the compenent DOM has been
          // manipulated at this point. Without a return the delete doesn't work as expected. #4862
          Rock.controls.emailEditor.$currentTextComponent = $tableWrapper;
        }

        $('.js-component-text-htmlEditor').summernote('code', $innerWrapper.html());
        $('.js-component-text-htmlEditor').summernote('commit');

        var textEl = $innerWrapper[0];

        $('#component-text-backgroundcolor').colorpicker('setValue', Rock.controls.emailEditor.$currentTextComponent.css('backgroundColor'));

        $('#component-text-border-color').colorpicker('setValue', String($innerWrapper[0].style['border-color'] || 'rgba(0,0,0,0)'));
        $('#component-text-border-width').val(parseFloat($innerWrapper[0].style['border-width']) || '');

        $('#component-text-padding-top').val(parseFloat(textEl.style['padding-top']) || '');
        $('#component-text-padding-left').val(parseFloat(textEl.style['padding-left']) || '');
        $('#component-text-padding-right').val(parseFloat(textEl.style['padding-right']) || '');
        $('#component-text-padding-bottom').val(parseFloat(textEl.style['padding-bottom']) || '');

        $('.js-component-text-lineheight').val(textEl.style['line-height']);

        // Return this to the previous function caller
        return Rock.controls.emailEditor.$currentTextComponent;
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
