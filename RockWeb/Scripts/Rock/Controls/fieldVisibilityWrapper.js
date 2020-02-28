(function (Sys) {
    'use strict';
    Sys.Application.add_load(function () {

        var preventDoublePostback = function () {
            var inputOnChange = $(this).attr('onchange');
            var inputOnClick = $(this).attr('onclick');
            if (inputOnChange && inputOnChange.indexOf('__doPostBack') > 0 || inputOnClick && inputOnClick.indexOf('__doPostBack') > 0) {
                var $blockButtons = $(this).closest('.js-block-instance').find('a');
                $blockButtons.attr('disabled', 'disabled')
                    .prop('disabled', true)
                    .attr('disabled', 'disabled')
                    .addClass('disabled');
            }
        };

        // since some of the edit controls do a postback to show/hide other controls, we want to disable buttons to prevent a double postback
        $('select.js-prevent-double-postback,input.js-prevent-double-postback').on('change', preventDoublePostback);
        $('.js-prevent-double-postback input[type="radio"]').on('click', preventDoublePostback);
    });
}(Sys));
