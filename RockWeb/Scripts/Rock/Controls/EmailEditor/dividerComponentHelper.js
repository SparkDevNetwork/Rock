(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentDividerComponent = $(false);

  Rock.controls.emailEditor.dividerComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;
        $('#component-divider-color').colorpicker().on('changeColor', function ()
        {
          self.setDividerColor();
        });

        $('#component-divider-margin-top,#component-divider-margin-bottom,#component-divider-height').on('change', function (e)
        {
          // just keep the numeric portion in case they included alpha chars
          $(this).val(parseFloat($(this).val()) || '');
          self.setDividerCss();
        });

      },
      setProperties: function ($dividerComponent)
      {
        Rock.controls.emailEditor.$currentDividerComponent = $dividerComponent.hasClass('component-divider') ? $currentComponent : $(false);
        var $hr = Rock.controls.emailEditor.$currentDividerComponent.find('hr');
        var hrEl = $hr[0];

        var dividerColor = $hr.css('background-color');
        var marginTop = parseFloat(hrEl.style['margin-top'] || '');
        var marginBottom = parseFloat(hrEl.style['margin-bottom'] || '');
        var height = parseFloat(hrEl.style['height'] || '');

        $('#component-divider-color').colorpicker('setValue', dividerColor);
        $('#component-divider-margin-top').val(marginTop);
        $('#component-divider-margin-bottom').val(marginBottom);
        $('#component-divider-height').val(height);
      },
      setDividerColor: function ()
      {
        var $hr = Rock.controls.emailEditor.$currentDividerComponent.find('hr');

        $hr.css('background-color', $('#component-divider-color').colorpicker('getValue'));
      },
      setDividerCss: function ()
      {
        var $hr = Rock.controls.emailEditor.$currentDividerComponent.find('hr');

        $hr.css('margin-top', Rock.controls.util.getValueAsPixels($('#component-divider-margin-top').val()))
          .css('margin-bottom', Rock.controls.util.getValueAsPixels($('#component-divider-margin-bottom').val()))
          .css('height', Rock.controls.util.getValueAsPixels($('#component-divider-height').val()))
      }
    }

    return exports;

  }());
}(jQuery));
