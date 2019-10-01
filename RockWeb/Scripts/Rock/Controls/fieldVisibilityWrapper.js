(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {

        // since some of the edit controls do a postback to show/hide other controls, we want to disable buttons to prevent a double postback
        $('select.js-prevent-double-postback,input.js-prevent-double-postback').on('change', function () {
            var inputOnChange = $(this).attr('onchange');
            if (inputOnChange && inputOnChange.indexOf('__doPostBack') > 0) {
                var $blockButtons = $(this).closest('.js-block-instance').find('a');
                $blockButtons.attr('disabled', 'disabled')
                    .prop('disabled', true)
                    .attr('disabled', 'disabled')
                    .addClass('disabled')
            };
        });
    });
}(Sys));
