(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentButtonComponent = $(false);

  Rock.controls.emailEditor.buttonComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;
        $('#component-button-buttonbackgroundcolor').colorpicker().on('changeColor', function ()
        {
          self.setButtonBackgroundColor();
        });

        $('#component-button-buttonfontcolor').colorpicker().on('changeColor', function ()
        {
          self.setButtonFontColor();
        });

        $('#component-button-buttontext').on('input', function (e)
        {
          self.setButtonText();
        });

        $('#component-button-buttonurl').on('input', function (e)
        {
          self.setButtonUrl();
        });

        $('#component-button-buttonwidth').on('change', function (e)
        {
          self.setButtonWidth();
        });

        $('#component-button-buttonalign').on('change', function (e)
        {
          self.setButtonAlign();
        });

        $('#component-button-buttonfont').on('change', function (e)
        {
          self.setButtonFont();
        });

        $('#component-button-buttonfontweight').on('change', function (e)
        {
          self.setButtonFontWeight()
        });

        $('#component-button-buttonfontsize').on('input', function (e)
        {
          self.setButtonFontSize();
        });

        $('#component-button-buttonpadding').on('input', function (e)
        {
          self.setButtonPadding();
        });
      },
      setProperties: function ($buttonComponent)
      {
        Rock.controls.emailEditor.$currentButtonComponent = $buttonComponent;
        var buttonText = $buttonComponent.find('.button-link').text();
        var buttonUrl = $buttonComponent.find('.button-link').attr('href');
        var buttonBackgroundColor = $buttonComponent.find('.button-shell').css('backgroundColor');
        var buttonFontColor = $buttonComponent.find('.button-link').css('color');
        var buttonWidth = $buttonComponent.find('.button-shell').attr('width');
        var buttonAlign = $buttonComponent.find('.button-innerwrap').attr('align');
        var buttonFont = $buttonComponent.find('.button-link').css("font-family");
        var buttonFontWeight = $buttonComponent.find('.button-link')[0].style['font-weight'];
        var buttonFontSize = $buttonComponent.find('.button-link').css("font-size");
        var buttonPadding = $buttonComponent.find('.button-content')[0].style['padding'];

        $('#component-button-buttontext').val(buttonText);
        $('#component-button-buttonurl').val(buttonUrl);
        $('#component-button-buttonbackgroundcolor').colorpicker('setValue', buttonBackgroundColor);
        $('#component-button-buttonfontcolor').colorpicker('setValue', buttonFontColor);

        if (buttonWidth == '100%') {
          $('#component-button-buttonwidth').val(1);
        }
        else {
          $('#component-button-buttonwidth').val(0);
        }

        $('#component-button-buttonalign').val(buttonAlign);

        $('#component-button-buttonfont').val(buttonFont);
        $('#component-button-buttonfontweight').val(buttonFontWeight);
        $('#component-button-buttonfontsize').val(buttonFontSize);
        $('#component-button-buttonpadding').val(buttonPadding);
      },
      setButtonText: function ()
      {
        var text = $('#component-button-buttontext').val()
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link')
                    .text(text)
                    .attr('title', text);
      },
      setButtonUrl: function ()
      {
        var text = $('#component-button-buttonurl').val()
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').attr('href', text);
      },
      setButtonBackgroundColor: function ()
      {
        var color = $('#component-button-buttonbackgroundcolor').colorpicker('getValue');
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-shell').css('backgroundColor', color);
      },
      setButtonFontColor: function ()
      {
        var color = $('#component-button-buttonfontcolor').colorpicker('getValue');
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('color', color);
      },
      setButtonWidth: function ()
      {
        var selectValue = $('#component-button-buttonwidth').val();

        if (selectValue == 0) {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-shell').removeAttr('width');
        }
        else {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-shell').attr('width', '100%');
        }
      },
      setButtonAlign: function ()
      {
        var selectValue = $('#component-button-buttonalign').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-innerwrap')
                    .attr('align', selectValue)
                    .css('text-align', selectValue);
      },
      setButtonFont: function ()
      {
        var selectValue = $('#component-button-buttonfont').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('font-family', selectValue);
      },
      setButtonFontWeight: function ()
      {
        var selectValue = $('#component-button-buttonfontweight').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('font-weight', selectValue);
      },
      setButtonFontSize: function ()
      {
        var text = $('#component-button-buttonfontsize').val()
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('font-size', text);
      },
      setButtonPadding: function ()
      {
        var text = $('#component-button-buttonpadding').val()
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-content').css('padding', text);
      }

    }

    return exports;

  }());
}(jQuery));



