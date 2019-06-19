(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.bootstrapButton = (function () {

        var exports = {

            showLoading: function (btn) {

                if (typeof (Page_ClientValidate) == 'function') {

                    if (Page_IsValid) {
                        // make sure page really is valid
                        Page_ClientValidate();
                    }
                }

                var $btn = $(btn);

                if (Page_IsValid) {
                    setTimeout(function () {
                        $btn.prop('disabled', true);
                        $btn.attr('disabled', 'disabled');
                        $btn.addClass('disabled');
                        $btn.html($btn.attr('data-loading-text'));
                    }, 0)
                }

                return true;
            },

            onCompleted: function (btn) {
                var $btn = $(btn);
                $btn.prop('disabled', true);
                $btn.attr('disabled', 'disabled');
                $btn.addClass('disabled');
                var completedTxt = $btn.attr('data-completed-text');
                if (completedTxt && completedTxt != '') {
                    $btn.html(completedTxt);
                } else {
                    $btn.html($btn.attr('data-init-text'));
                }

                var completedMessage = $btn.attr('data-completed-message');
                if (completedMessage && completedMessage != '') {
                    var id = $btn.attr("id") + "_msg";
                    var $span = $('<span />').attr('id', id).html(completedMessage);
                    $btn.after( $span );
                }

                var timeout = Number($btn.attr('data-timeout-text'));
                if ( timeout && !isNaN(timeout)) {
                    timeout = timeout * 1000;
                    setTimeout(function () {
                        $btn.prop('disabled', false);
                        $btn.removeAttr('disabled');
                        $btn.removeClass('disabled');
                        $btn.html($btn.attr('data-init-text'));
                        var id = $btn.attr("id") + "_msg";
                        var $span = $('#' + id);
                        if ($span){
                            $span.remove();
                        }
                    }, timeout);
                }
            }
        };

        return exports;
    }());
}(jQuery));
