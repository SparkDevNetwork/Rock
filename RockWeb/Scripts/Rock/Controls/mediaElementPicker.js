(function($) {
    window.Rock = window.Rock || {};
    window.Rock.controls = window.Rock.controls || {};

    window.Rock.controls.mediaElementPicker = (function () {
        var exports = {
            initialize: function(options) {
                var $picker = $('#' + options.controlId);

                $picker.data('required', options.required === true);

                // Setup click handlers for any refresh buttons.
                $picker.find('.js-media-element-picker-refresh').on('click', function () {
                    if ($(this).hasClass('disabled')) {
                        return false;
                    }

                    $picker.find('.js-media-element-picker-refresh').addClass('disabled');
                    $(this).find('i.fa').addClass('fa-spin');

                    window.location = 'javascript:' + options.refreshScript;

                    return false;
                });

                // Re-validate after they change the selection of the media.
                $picker.find('.js-media-element-value select').on('change', function () {
                    ValidatorValidate(window[$picker.find('.js-media-element-validator').prop('id')]);
                });
            },

            clientValidate: function (validator, args) {
                // After a postback, the validator is not actually in the DOM
                // anymore. So find the fake ancestor's Id and then lookup the
                // real media element picker.
                var mediaElementPickerId = $(validator).closest('.js-media-element-picker').attr('id');
                var $mediaElementPicker = $('#' + mediaElementPickerId);

                var required = $mediaElementPicker.data('required') === true;
                var value = $mediaElementPicker.find('.js-media-element-value select').val() || '';
                var isValid = !required || (value !== '' && value !== '0');

                if (isValid) {
                    $mediaElementPicker.closest('.form-group').removeClass('has-error');
                    args.IsValid = true;
                }
                else {
                    $mediaElementPicker.closest('.form-group').addClass('has-error');
                    args.IsValid = false;
                }
            }
        };

        return exports;
    })();
})(jQuery);
