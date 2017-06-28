(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentCodeComponent = $(false);

  Rock.controls.emailEditor.codeComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;

        $('#component-code-margin-top,#component-code-margin-left,#component-code-margin-right,#component-code-margin-bottom').on('change', function (e)
        {
          // just keep the numeric portion in case they included alpha chars
          $(this).val(parseFloat($(this).val()));

          self.setMargins();
        });
      },
      setProperties: function ($codeComponent)
      {
        Rock.controls.emailEditor.$currentCodeComponent = $codeComponent.hasClass('component-code') ? $currentComponent : $(false);
        var aceEditor = $('.js-component-code-codeEditor .ace_editor').data('aceEditor');
        aceEditor.setValue($codeComponent.html());
        var codeEl = $codeComponent[0];

        $('#component-code-margin-top').val(parseFloat(codeEl.style['margin-top']));
        $('#component-code-margin-left').val(parseFloat(codeEl.style['margin-left']));
        $('#component-code-margin-right').val(parseFloat(codeEl.style['margin-right']));
        $('#component-code-margin-bottom').val(parseFloat(codeEl.style['margin-bottom']));
      },
      setMargins: function ()
      {
        Rock.controls.emailEditor.$currentCodeComponent
                .css('margin-top', parseFloat($('#component-code-margin-top').val()) + 'px')
                .css('margin-left', parseFloat($('#component-code-margin-left').val()) + 'px')
                .css('margin-right', parseFloat($('#component-code-margin-right').val()) + 'px')
                .css('margin-bottom', parseFloat($('#component-code-margin-bottom').val()) + 'px');
      },
      updateCodeComponent: function (el, contents)
      {
        if (Rock.controls.emailEditor.$currentCodeComponent) {
          // just in case they typed in something that caused an exception, for example, a script tag with invalid script, show an error
          try {
            $('#component-code-codeEditor-error').hide();
            Rock.controls.emailEditor.$currentCodeComponent.html(contents);
          }
          catch (ex) {
            $('#component-code-codeEditor-error').html(ex.message);
            $('#component-code-codeEditor-error').show();
          }
        }
      }
    }

    return exports;

  }());
}(jQuery));