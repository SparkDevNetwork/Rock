(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.buttonDropDownList = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.controlId) {
                    throw 'id is required';
                }

                var $control = $('#' + options.controlId);
                var $selectBtn = $control.find('.js-buttondropdown-btn-select');
                var $selectedId = $control.find('.js-buttondropdown-selected-id');
                var checkmarksEnabled = $control.attr('data-checkmarks-enabled') == 1;

                $('.dropdown-menu a', $control).on('click', function (e) {
                    var $el = $(this);
                    var text = $el.html();
                    var textHtml = $el.html() + " <span class='ti ti-caret-down-filled' ></span >";
                    var idValue = $el.attr('data-id');
                    var postbackScript = $el.attr('data-postback-script');

                    if (checkmarksEnabled) {
                        $el.closest('.dropdown-menu').find('.js-selectionicon').removeClass('ti-check');
                        $el.find('.js-selectionicon').addClass('ti-check');
                    }
                    else {
                        $selectBtn.html(textHtml);
                    }

                    $selectedId.val(idValue);

                    if (postbackScript) {
                        window.location = postbackScript;
                    }
                });

            }
        };

        return exports;
    }());
}(jQuery));



