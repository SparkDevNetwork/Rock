(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {
        
        var $chosenDropDowns = $('.chosen-select');
        if ($chosenDropDowns.length) {
            // IE fix for 'chosen' causing activeElement to be a plain generic object (not a real node) after postback
            if (document.activeElement && !document.activeElement.nodeType) {
                $('body').focus();
            }

            $chosenDropDowns.chosen({
                width: '100%',
                allow_single_deselect: true,
                placeholder_text_multiple: ' ',
                placeholder_text_single: ' '
            });

            $chosenDropDowns.on('chosen:showing_dropdown chosen:hiding_dropdown', function (evt, params) {
                // update the outer modal 
                Rock.dialogs.updateModalScrollBar(this);
            });

            var $chosenDropDownsAbsolute = $chosenDropDowns.filter('.chosen-select-absolute');
            if ($chosenDropDownsAbsolute.length) {
                $chosenDropDownsAbsolute.on('chosen:showing_dropdown', function (evt, params) {
                    $(this).next('.chosen-container').find('.chosen-drop').css('position', 'relative');
                });
                $chosenDropDownsAbsolute.on('chosen:hiding_dropdown', function (evt, params) {
                    $(this).next('.chosen-container').find('.chosen-drop').css('position', 'absolute');
                });
            }
        }
    });
}(Sys));