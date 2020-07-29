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

                function updateListItemValues(e) {
                    var $span = e.closest('span.list-items');
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

                $('a.list-items-add').on('click', function (e) {
                    e.preventDefault();
                    var $ValueList = $(this).closest('.list-items');
                    $ValueList.find('.list-items-rows').append($ValueList.find('.js-list-items-html').val());
                    updateListItemValues($(this));
                    Rock.controls.modal.updateSize($(this));
                });

                Sys.Application.add_load(function () {

                    $(document).on('click', 'a.list-items-remove', function (e) {
                        e.preventDefault();
                        var $rows = $(this).closest('span.list-items-rows');
                        $(this).closest('div.controls-row').remove();
                        updateListItemValues($rows);
                        Rock.controls.modal.updateSize($(this));
                    });

                    $(document).on('focusout', '.js-list-items-input', function (e) {
                        updateListItemValues($(this));
                    });


                });

                $('.list-items .list-items-rows').sortable({
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
