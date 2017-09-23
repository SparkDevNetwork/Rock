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

        $('#component-text-margin-top,#component-text-margin-left,#component-text-margin-right,#component-text-margin-bottom').on('change', function (e)
        {
          // just keep the numeric portion in case they included alpha chars
          $(this).val(parseFloat($(this).val()) || '');

          self.setMargins();
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
        $('.js-component-text-htmlEditor').summernote('code', $textComponent.html());
        var textEl = $textComponent[0];

        $('#component-text-backgroundcolor').colorpicker('setValue', $textComponent.css('backgroundColor'));
        $('#component-text-margin-top').val(parseFloat(textEl.style['margin-top']) || '');
        $('#component-text-margin-left').val(parseFloat(textEl.style['margin-left']) || '');
        $('#component-text-margin-right').val(parseFloat(textEl.style['margin-right']) || '');
        $('#component-text-margin-bottom').val(parseFloat(textEl.style['margin-bottom']) || '');

        $('.js-component-text-lineheight').val(textEl.style['line-height']);
      },
      setBackgroundColor: function ()
      {
        var color = $('#component-text-backgroundcolor').colorpicker('getValue');
        Rock.controls.emailEditor.$currentTextComponent.css('backgroundColor', color);
      },
      setMargins: function ()
      {
        Rock.controls.emailEditor.$currentTextComponent
                .css('margin-top', Rock.controls.util.getValueAsPixels($('#component-text-margin-top').val()))
                .css('margin-left', Rock.controls.util.getValueAsPixels($('#component-text-margin-left').val()))
                .css('margin-right', Rock.controls.util.getValueAsPixels($('#component-text-margin-right').val()))
                .css('margin-bottom', Rock.controls.util.getValueAsPixels($('#component-text-margin-bottom').val()));
      },
      updateTextComponent: function (el, contents)
      {
        if (Rock.controls.emailEditor.$currentTextComponent) {
          Rock.controls.emailEditor.$currentTextComponent.html(contents);
        }
      }
    }

    return exports;

  }());
}(jQuery));
