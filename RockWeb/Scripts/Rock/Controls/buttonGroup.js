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

                /*
                 * 2020-03-18 - JPH
                 *
                 * ASP.NET renders a ButtonGroup's child ListItem controls as follows:
                 * <label class="js-buttongroup-item ...">
                 *    <input id="..." type="radio" name="..." value="...">
                 *    <span class="label-text">...</span>
                 * </label>
                 *
                 * When client-side validation is performed on this ListItem control, the validation script
                 * looks for a label element containing a [for] attribute. Since the label element is rendered
                 * WITHOUT a [for] attribute, a client-side null reference error is encountered. The following
                 * adds the appropriate [for] attribute and value to each ListItem control that is rendered
                 * as part of this ButtonGroup control. This will allow the out-of-the-box ASP.NET validation
                 * script to successfully select the label element, preventing the null reference exception.
                 *
                 * Reason: Issue #4043
                 * https://github.com/SparkDevNetwork/Rock/issues/4043
                 */
                $buttonGroupItems.each(function () {
                    var $input = $(this).find('input[id]').first();
                    if ($input) {
                        $(this).attr('for', $input.attr('id'));
                    }
                });

                $buttonGroupItems.on('click', function () {
                    var $selectedItem = $(this);
                    var $unselectedItems = $buttonGroupItems.not($selectedItem);
                    $unselectedItems.removeClass(selectedItemClass).addClass(unselectedItemClass);
                    $selectedItem.removeClass(unselectedItemClass).addClass(selectedItemClass);

                    // make sure the input elements onclick get executed so that the postback gets fired (if there is one)
                    var $input = $selectedItem.find('input');
                    var clickFunction = $input.prop('onclick');
                    if (clickFunction) {
                        clickFunction();
                    }
                });

            }
        };

        return exports;
    }());
}(jQuery));
