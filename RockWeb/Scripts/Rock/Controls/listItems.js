(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.listItems = (function () {
        var exports = {

            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                if (!options.clientId) {
                    throw 'clientId is required';
                }

                const $listItemsControl = $('#' + options.clientId);

                function updateListItemValues($e) {
                    var $span = $e.closest('span.list-items');
                    var keyValuePairs = [];
                    $span.children('span.list-items-rows').first().children('div.controls-row').each(function (index) {
                        keyValuePairs.push({
                            'Key': $(this).children('.input-group').find('.js-list-items-input').first().data('id'),
                            'Value': $(this).children('.input-group').find('.js-list-items-input').first().val()
                        });
                    });
                    $span.children('input').first().val(JSON.stringify(keyValuePairs));
                    if (options.valueChangedScript) {
                        window.location = "javascript:" + options.valueChangedScript;
                    }

                }

                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                $listItemsControl.find('a.list-items-add').on('click', function (e) {
                    e.preventDefault();
                    $listItemsControl.find('.list-items-rows').append($listItemsControl.find('.js-list-items-html').val());
                    updateListItemValues($(this));
                    Rock.controls.modal.updateSize($(this));
                });

                // Use add_load to fire with page load and postbacks
                Sys.Application.add_load(function () {
                    function onRemoveClick(event) {
                        event.preventDefault();
                        var $rows = $(this).closest('span.list-items-rows');
                        $(this).closest('div.controls-row').remove();
                        updateListItemValues($rows);
                        Rock.controls.modal.updateSize($(this));
                    }

                    function onFocus(event) {
                        var element = event.target;
                        var $element = $(element);
                        var valueOnFocus = element.value;

                        function onBlur() {
                            var valueOnBlur = element.value;

                            if (valueOnFocus != valueOnBlur) {
                                updateListItemValues($element);
                            }
                        }

                        // Only handle the focus out event once and remove the handler.
                        $element.one('blur', onBlur);
                    }

                    $listItemsControl.find('a.list-items-remove').off('click', onRemoveClick).on('click', onRemoveClick);
                    $listItemsControl.find('.js-list-items-input').off('focus', onFocus).on('focus', onFocus);
                });

                $listItemsControl.find('.list-items-rows').sortable({
                    helper: fixHelper,
                    handle: '.fa-bars',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        updateListItemValues($(this));
                    }

                }).disableSelection();

            }
        };

        return exports;
    }());
}(jQuery));
