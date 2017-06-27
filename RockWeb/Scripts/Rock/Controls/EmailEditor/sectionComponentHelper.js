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
        $('#component-section-left-column-percent,#component-section-right-column-percent').on('change', function (e)
        {
          // just keep the numeric portion in case they included alpha chars
          $(this).val(parseFloat($(this).val()));
          self.setSectionCss();
        });

      },
      setProperties: function ($sectionComponent)
      {
        Rock.controls.emailEditor.$currentSectionComponent = $sectionComponent.hasClass('component-section') ? $currentComponent : $(false);
        var tableDatas = Rock.controls.emailEditor.$currentSectionComponent.find('td');
        var tableDataLeft = tableDatas[0];
        var tableDataRight = tableDatas[1];

        $('#component-section-left-column-percent').val(parseFloat(tableDataLeft.width));
        $('#component-section-right-column-percent').val(parseFloat(tableDataRight.width));
      },
      setSectionCss: function ()
      {
        var tableDatas = Rock.controls.emailEditor.$currentSectionComponent.find('td');
        var tableDataLeft = tableDatas[0];
        var tableDataRight = tableDatas[1];

        tableDataLeft.width = parseFloat($('#component-section-left-column-percent').val()) + '%';
        tableDataRight.width = parseFloat($('#component-section-right-column-percent').val()) + '%';
      }
    }

    return exports;

  }());
}(jQuery));

// initialize
Rock.controls.emailEditor.sectionComponentHelper.initializeEventHandlers();