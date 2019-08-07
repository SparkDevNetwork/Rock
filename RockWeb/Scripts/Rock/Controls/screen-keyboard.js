(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.screenKeyboard = (function () {
        //
        // Handle toggling the Shift key states.
        //
        var toggleShift = function ($keyboard) {
            $keyboard.find('[data-command="shift"]').toggleClass('active');

            setAltMode($keyboard, $keyboard.find('[data-command="shift"].active').length > 0);
        };

        //
        // Handle setting digits to either normal value or alternate value.
        //
        var setAltMode = function ($keyboard, altMode) {
            $keyboard.find('.digit').each(function () {
                $(this).text(altMode === true ? $(this).data('alt-value') : $(this).data('value'));
            });
        };

        //
        // Process normal digit presses.
        //
        var digitClick = function () {
            var $keyboard = $(this).closest('.screen-keyboard');
            var $target = $('#' + $keyboard.data('target'));

            $target.val($target.val() + $(this).text());
            $target.trigger('input');

            if ($keyboard.find('[data-command="shift"].active').length > 0) {
                toggleShift($keyboard);
            }
        };

        //
        // Process special command keys.
        //
        var commandClick = function (options) {
            var $keyboard = $(this).closest('.screen-keyboard');
            var $target = $('#' + $keyboard.data('target'));
            var command = $(this).data('command');

            if (command === 'backspace') {
                var val = $target.val();
                if (val.length > 0) {
                    val = val.substr(0, val.length - 1);
                }
                $target.val(val);
                $target.trigger('input');
            }
            else if (command === 'clear') {
                $target.val('');
                $target.trigger('input');
            }
            else if (command === 'shift') {
                $keyboard.find('[data-command="caps"]').removeClass('active');
                toggleShift($keyboard);
            }
            else if (command === 'caps') {
                $keyboard.find('[data-command="shift"]').removeClass('active');
                $(this).toggleClass('active');

                setAltMode($keyboard, $(this).hasClass('active'));
            }
            else if (command === 'tab') {
                /* TODO: What do we do with this action? */
            }
            else if (command === 'enter') {
                window.location = "javascript:__doPostBack('" + options.postback + "', 'enter')";
            }
        };

        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var $keyboard = $('#' + options.id);

                $keyboard.find('.digit').on('click', digitClick);
                $keyboard.find('.command').on('click', function () { commandClick.apply(this, [options]); });
            }
        };

        return exports;
    }());
}(jQuery));
