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

        $('#component-button-buttontext').on('input', function ()
        {
          self.setButtonText();
        });

        $('#component-button-buttonurl').on('input', function ()
        {
          self.setButtonUrl();
        });

        $('#component-button-buttonwidth').on('change', function ()
        {
          self.setButtonWidth();
        });

        $('#component-button-buttonfixedwidth').on('change', function ()
        {
          self.setButtonWidth();
        });

        $('#component-button-buttonalign').on('change', function ()
        {
          self.setButtonAlign();
        });

        $('#component-button-buttonfont').on('change', function ()
        {
          self.setButtonFont();
        });

        $('#component-button-buttonfontweight').on('change', function ()
        {
          self.setButtonFontWeight();
        });

        $('#component-button-buttonfontsize').on('input', function ()
        {
          self.setButtonFontSize();
        });

        $('#component-button-buttonpadding').on('input', function ()
        {
          self.setButtonPadding();
        });

        $('#component-button-buttonradius').on('input', function ()
        {
          self.setButtonRadius();
        });
      },
      setProperties: function ($buttonComponent)
      {
        Rock.controls.emailEditor.$currentButtonComponent = $buttonComponent;
        var isButtonV2 = $buttonComponent.hasClass('v2');
        var buttonRadius;
        var buttonPadding;
        var buttonBackgroundColor;

        var buttonText = $buttonComponent.find('.button-link').text();
        var buttonUrl = $buttonComponent.find('.button-link').attr('href');
        var buttonFontColor = $buttonComponent.find('.button-link').css('color');
        var buttonWidth = $buttonComponent.find('.button-shell').attr('width') || null;
        var buttonAlign = $buttonComponent.find('.button-innerwrap').attr('align');
        var buttonFont = $buttonComponent.find('.button-link').css('font-family');
        var buttonFontWeight = $buttonComponent.find('.button-link')[0].style['font-weight'];
        var buttonFontSize = $buttonComponent.find('.button-link').css('font-size');

        if (isButtonV2)
        {
          buttonRadius = $buttonComponent.find('.button-link').css('border-radius');
          buttonPadding = $buttonComponent.find('.button-link')[0].style.padding;
          buttonBackgroundColor = $buttonComponent.find('.button-content')[0].style['background-color'] || null;
        }
        else
        {
          buttonRadius = $buttonComponent.find('.button-shell')[0].style['border-radius'];
          buttonPadding = $buttonComponent.find('.button-content')[0].style.padding;
          buttonBackgroundColor = $buttonComponent.find('.button-shell').css('backgroundColor');
        }

        $('#component-button-buttontext').val(buttonText);
        $('#component-button-buttonurl').val(buttonUrl);
        $('#component-button-buttonbackgroundcolor').colorpicker('setValue', buttonBackgroundColor);
        $('#component-button-buttonfontcolor').colorpicker('setValue', buttonFontColor);

        var $buttonfixedwidthDiv = $('#component-button-panel').find('.js-buttonfixedwidth');

        if (buttonWidth === null)
        {
          $('#component-button-buttonwidth').val(0);
          $buttonfixedwidthDiv.hide();
          $('#component-button-buttonfixedwidth').val('');
        }
        else if (buttonWidth === '100%')
        {
          $('#component-button-buttonwidth').val(1);
          $buttonfixedwidthDiv.hide();
          $('#component-button-buttonfixedwidth').val('');
        }
        else
        {
          $('#component-button-buttonwidth').val(2);
          $buttonfixedwidthDiv.show();
          $('#component-button-buttonfixedwidth').val(buttonWidth);
        }

        $('#component-button-buttonalign').val(buttonAlign);

        $('#component-button-buttonfont').val(buttonFont);
        $('#component-button-buttonfontweight').val(buttonFontWeight);
        $('#component-button-buttonfontsize').val(buttonFontSize);
        $('#component-button-buttonpadding').val(buttonPadding);
        $('#component-button-buttonradius').val(buttonRadius);

        // Return this to the previous function caller
        return Rock.controls.emailEditor.$currentButtonComponent;
      },
      setButtonText: function ()
      {
        var text = $('#component-button-buttontext').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link')
                    .text(text)
                    .attr('title', text);
      },
      setButtonUrl: function ()
      {
        var text = $('#component-button-buttonurl').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').attr('href', text);
      },
      setButtonBackgroundColor: function ()
      {
        var color = $('#component-button-buttonbackgroundcolor').colorpicker('getValue');
        var currentButton = Rock.controls.emailEditor.$currentButtonComponent;
        var isButtonV2 = currentButton.hasClass('v2');

        if (isButtonV2)
        {
          currentButton.find('.button-content').css('background-color', color);
          currentButton.find('.button-link').css('background-color', color).css('border-color', color);
        }
        else
        {
          currentButton.find('.button-shell').css('backgroundColor', color);
        }
      },
      setButtonFontColor: function ()
      {
        var color = $('#component-button-buttonfontcolor').colorpicker('getValue');
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('color', color);
      },
      setButtonWidth: function ()
      {
        var currentButton = Rock.controls.emailEditor.$currentButtonComponent;
        var isButtonV2 = currentButton.hasClass('v2');
        var selectValue = $('#component-button-buttonwidth').val();
        var fixedValue = $('#component-button-buttonfixedwidth').val();
        var $buttonfixedwidthDiv = $('#component-button-panel').find('.js-buttonfixedwidth');

        if (selectValue == 0)
        {
          currentButton.find('.button-shell').removeAttr('width');
          if (isButtonV2)
          {
            currentButton.find('.button-link').css('width', '').css('width', '');
          }

          $buttonfixedwidthDiv.slideUp();
        }
        else if (selectValue == 1)
        {
          currentButton.find('.button-shell').attr('width', '100%');
          if (isButtonV2)
          {
            currentButton.find('.button-link').css('width', '').css('max-width', '');
          }

          $buttonfixedwidthDiv.slideUp();
        }
        else if (selectValue == 2)
        {
          currentButton.find('.button-shell').attr('width', fixedValue).css('width', fixedValue).css('max-width', '100%');
          if (isButtonV2)
          {
            currentButton.find('.button-link').css('width', fixedValue).css('max-width', '100%');
          }

          $buttonfixedwidthDiv.slideDown();
        }
      },
      setButtonAlign: function ()
      {
        var selectValue = $('#component-button-buttonalign').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-innerwrap').attr('align', selectValue);
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
        var text = $('#component-button-buttonfontsize').val();
        Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('font-size', text);
      },
      setButtonPadding: function ()
      {
        var text = $('#component-button-buttonpadding').val();
        var isButtonV2 = Rock.controls.emailEditor.$currentButtonComponent.hasClass('v2');
        if (isButtonV2)
        {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('padding', text);
        }
        else
        {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-content').css('padding', text);
        }
      },
      setButtonRadius: function ()
      {
        var value = $('#component-button-buttonradius').val();
        var isButtonV2 = Rock.controls.emailEditor.$currentButtonComponent.hasClass('v2');
        if (isButtonV2)
        {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-content').css('border-radius', value);
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-link').css('border-radius', value);
        }
        else
        {
          Rock.controls.emailEditor.$currentButtonComponent.find('.button-shell').css('border-radius', value);
        }
      }

    };

    return exports;
  })();
})(jQuery);

