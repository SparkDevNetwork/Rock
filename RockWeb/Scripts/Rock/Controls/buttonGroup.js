(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.buttonGroup = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var $buttonGroup = $('#' + options.id);
                var $buttonGroupItems = $buttonGroup.find('.js-buttongroup-item');
                var selectedItemClass = $buttonGroup.attr('data-selecteditemclass');
                var unselectedItemClass = $buttonGroup.attr('data-unselecteditemclass');

                $buttonGroupItems.on('click', function () {
                    var $selectedItem = $(this);
                    var $unselectedItems = $buttonGroupItems.not($selectedItem);
                    $unselectedItems.removeClass(selectedItemClass).addClass(unselectedItemClass);
                    $selectedItem.removeClass(unselectedItemClass).addClass(selectedItemClass);
                });

            }
        };

        return exports;
    }());
}(jQuery));



