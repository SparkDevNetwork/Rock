(function ($)
{
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};
  Rock.controls.emailEditor.$currentSectionComponent = $(false);

  Rock.controls.emailEditor.sectionComponentHelper = (function ()
  {
    var exports = {
      initializeEventHandlers: function ()
      {
        var self = this;

        $('#component-section-backgroundcolor-1,#component-section-backgroundcolor-2,#component-section-backgroundcolor-3').colorpicker().on('changeColor', function (ev)
        {
          self.setBackgroundColor(this);
        });


        $('.js-component-section-padding-input').on('input', function (e)
        {
          self.setPadding();
        });

      },
      setProperties: function ($sectionComponent)
      {
        Rock.controls.emailEditor.$currentSectionComponent = $sectionComponent.hasClass('component-section') ? $currentComponent : $(false);

        $('#component-section-column2').hide();
        $('#component-section-column3').hide();

        var $dropzones = Rock.controls.emailEditor.$currentSectionComponent.find('.dropzone');
        if ($dropzones.length > 1)
        {
          $('#component-section-column2').show();
        }
        if ($dropzones.length > 2) {
          $('#component-section-column3').show();
        }

        $dropzones.each(function (index)
        {
          var $columnTd = $($dropzones[index]).closest('td');
          var columnEl = $columnTd[0];
          var columnNumber = index + 1;
          $('#component-section-backgroundcolor-' + columnNumber).colorpicker('setValue', $columnTd.css('backgroundColor'));

          var padTop = parseFloat(columnEl.style['padding-top']) || '';
          var padLeft = parseFloat(columnEl.style['padding-left']) || '';
          var padRight = parseFloat(columnEl.style['padding-right']) || '';
          var padBottom = parseFloat(columnEl.style['padding-bottom']) || '';

          if(padTop !== '' && padTop == padBottom && padLeft == padRight && padTop == padLeft ) {
            var inputGroup = $('#component-section-padding-top-1').closest('.margin-input-group');
            inputGroup.addClass('locked');
            Rock.controls.emailEditor.handleMarginLock();
          }

          $('#component-section-padding-top-' + columnNumber).val(padTop);
          $('#component-section-padding-left-' + columnNumber).val(padLeft);
          $('#component-section-padding-right-' + columnNumber).val(padRight);
          $('#component-section-padding-bottom-' + columnNumber).val(padBottom);
        });

      },
      setPadding: function ()
      {
        var $dropzones = Rock.controls.emailEditor.$currentSectionComponent.find('.dropzone');
        $dropzones.each(function (index)
        {
          var $columnTd = $($dropzones[index]).closest('td');
          var columnNumber = index + 1;
          $columnTd.css('padding-top', Rock.controls.util.getValueAsPixels($('#component-section-padding-top-' + columnNumber).val()))
                  .css('padding-left', Rock.controls.util.getValueAsPixels($('#component-section-padding-left-' + columnNumber).val()))
                  .css('padding-right', Rock.controls.util.getValueAsPixels($('#component-section-padding-right-' + columnNumber).val()))
                  .css('padding-bottom', Rock.controls.util.getValueAsPixels($('#component-section-padding-bottom-' + columnNumber).val()));
        });
      },
      setBackgroundColor: function (inputEl)
      {
        var $dropzones = Rock.controls.emailEditor.$currentSectionComponent.find('.dropzone');
        $dropzones.each(function (index)
        {
          var columnNumber = index + 1;
          var colorPickerInputId = 'component-section-backgroundcolor-' + columnNumber;
          if (inputEl.id == colorPickerInputId)
          {
            var $columnTd = $($dropzones[index]).closest('td');
            var colorAlpha = $('#' + colorPickerInputId).data('colorpicker').color.value.a;
            var color = $('#' + colorPickerInputId).colorpicker('getValue');
            $columnTd.css('backgroundColor', color);
          }
        });
      },
    }

    return exports;

  }());
}(jQuery));
