(function ($) {
  'use strict';
  window.Rock = window.Rock || {};
  Rock.controls = Rock.controls || {};
  Rock.controls.emailEditor = Rock.controls.emailEditor || {};

  Rock.controls.emailEditor.emailUtil = (function () {
    var exports = {
        inputLockListener: function () {
            $( ".margin-input-lock" ).click(function() {
                var parentContainer = $(this).parent('.margin-input-group');
                parentContainer.toggleClass("locked");
                if ( $(this).parent('.margin-input-group').hasClass("locked") ) {

                    parentContainer.find('.margin-input-padding-top').on('input.linked-field', function (e)
                    {
                        var update = parentContainer.find('.margin-input-padding-top').val();
                        parentContainer.find('.margin-input-padding-bottom').val(update);
                        parentContainer.find('.margin-input-padding-left').val(update);
                        parentContainer.find('.margin-input-padding-right').val(update);
                    });
                    parentContainer.find('.margin-input-padding-bottom').prop('readonly', true);
                    parentContainer.find('.margin-input-padding-left').prop('readonly', true);
                    parentContainer.find('.margin-input-padding-right').prop('readonly', true);

                    $(this).find('i').removeClass("fa-unlock").addClass("fa-lock");
                } else {

                    parentContainer.find('.margin-input-padding-top').off('input.linked-field');
                    parentContainer.find('.margin-input-padding-bottom').prop('readonly', false);
                    parentContainer.find('.margin-input-padding-left').prop('readonly', false);
                    parentContainer.find('.margin-input-padding-right').prop('readonly', false);

                    $(this).find('i').removeClass("fa-lock").addClass("fa-unlock");
                }
            });
        }
    };

    return exports;

}());
}(jQuery));
