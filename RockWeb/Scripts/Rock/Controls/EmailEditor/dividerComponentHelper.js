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

        $('.js-component-divider-divide-with-line').on('change', function (e) {
            self.setDividerHtml();
        })

      },
      setProperties: function ($dividerComponent)
      {
        Rock.controls.emailEditor.$currentDividerComponent = $dividerComponent.hasClass('component-divider') ? $dividerComponent : $(false);
        var $div = Rock.controls.emailEditor.$currentDividerComponent.find('hr,div');
        var divEl = $div[0];

        var dividerColor = $div.css('background-color');
        var marginTop = parseFloat(divEl.style['margin-top'] || '');
        var marginBottom = parseFloat(divEl.style['margin-bottom'] || '');
        var height = parseFloat(divEl.style['height'] || '');

        var $cbDivideWithLine = $('#component-divider-panel').find('.js-component-divider-divide-with-line');
        if ($div.is('hr')) {
            $cbDivideWithLine.prop('checked', true);
        } else {
            $cbDivideWithLine.prop('checked', false);
        }

        $('#component-divider-color').colorpicker('setValue', dividerColor);
        $('#component-divider-margin-top').val(marginTop);
        $('#component-divider-margin-bottom').val(marginBottom);
        $('#component-divider-height').val(height);

        // Return this to the previous function caller
        return Rock.controls.emailEditor.$currentDividerComponent;
      },
      setDividerColor: function ()
      {
          var $div = Rock.controls.emailEditor.$currentDividerComponent.find('hr,div');
          $div.css('background-color', $('#component-divider-color').colorpicker('getValue'));
      },
      setDividerCss: function ()
      {
          var $div = Rock.controls.emailEditor.$currentDividerComponent.find('hr,div');

          $div.css('margin-top', Rock.controls.util.getValueAsPixels($('#component-divider-margin-top').val()))
          .css('margin-bottom', Rock.controls.util.getValueAsPixels($('#component-divider-margin-bottom').val()))
          .css('height', Rock.controls.util.getValueAsPixels($('#component-divider-height').val()))
      },
      setDividerHtml: function () {
          var $div = Rock.controls.emailEditor.$currentDividerComponent.find('hr,div');
          var $cbDivideWithLine = $('#component-divider-panel').find('.js-component-divider-divide-with-line');
          var tagType = 'hr';
          if (!$cbDivideWithLine.is(":checked")){
            tagType = 'div'
          };

          if (!$div.is(tagType)) {
              $div.replaceWith("<" + tagType + "/>");
              this.setDividerCss();
              this.setDividerColor();
          }
      }
    }

    return exports;

  }());
}(jQuery));
