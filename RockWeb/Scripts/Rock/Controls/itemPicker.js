(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.itemPicker = (function () {
        var ItemPicker = function (options) {
            this.options = options;
        },
            exports;

        ItemPicker.prototype = {
            constructor: ItemPicker,
            initialize: function () {
                var $control = $('#' + this.options.controlId),
                    $tree = $control.find('.treeview'),
                    treeOptions = {
                        multiselect: this.options.allowMultiSelect,
                        restUrl: this.options.restUrl,
                        restParams: this.options.restParams,
                        expandedIds: this.options.expandedIds,
                        id: this.options.startingId
                    },
                    $hfItemIds = $control.find('.js-item-id-value'),
                    $hfExpandedIds = $control.find('.js-initial-item-parent-ids-value');

                if (typeof this.options.mapItems === 'function') {
                    treeOptions.mapping = {
                        mapData: this.options.mapItems
                    };
                }

                // clean up the tree (in case it was initialized already, but we are rebuilding it)
                var rockTree = $tree.data('rockTree');
                if (rockTree) {
                    rockTree.nodes = [];
                }
                $tree.empty();

                $control.find('.scroll-container').tinyscrollbar({ size: 120, sizethumb: 20 });
                // Since some hanlers are "live" events, they need to be bound before tree is initialized
                this.initializeEventHandlers();

                if ($hfItemIds.val() && $hfItemIds !== '0') {
                    treeOptions.selectedIds = $hfItemIds.val().split(',');
                }

                if ($hfExpandedIds.val()) {
                    treeOptions.expandedIds = $hfExpandedIds.val().split(',');
                }

                $tree.rockTree(treeOptions);
                this.updateScrollbar();
            },
            initializeEventHandlers: function () {
                var self = this,
                    $control = $('#' + this.options.controlId),
                    $spanNames = $control.find('.selected-names'),
                    $hfItemIds = $control.find('.js-item-id-value'),
                    $hfItemNames = $control.find('.js-item-name-value');

                // Bind tree events
                $control.find('.treeview')
                    .on('rockTree:selected', function () {
                        // intentionally blank
                    })
                    .on('rockTree:itemClicked', function (e) {
                        if (!self.options.allowMultiSelect) {
                            $control.find('.picker-btn').trigger('click');
                        }
                    })
                    .on('rockTree:expand rockTree:collapse rockTree:dataBound rockTree:rendered', function (evt) {
                        self.updateScrollbar();
                    });

                $control.find('a.picker-label').click(function (e) {
                    e.preventDefault();
                    $control.find('.picker-menu').first().toggle(function () {
                        self.updateScrollbar();
                    });
                });

                $control.find('.picker-cancel').click(function () {
                    $(this).closest('.picker-menu').slideUp(function () {
                        self.updateScrollbar();
                    });
                });

                // have the X appear on hover if something is selected
                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();
                }

                $control.find('.picker-btn').click(function () {

                    var rockTree = $control.find('.treeview').data('rockTree'),
                            selectedNodes = rockTree.selectedNodes,
                            selectedIds = [],
                            selectedNames = [];

                    $.each(selectedNodes, function (index, node) {
                        selectedIds.push(node.id);
                        selectedNames.push(node.name);
                    });

                    $hfItemIds.val(selectedIds.join(','));
                    $hfItemNames.val(selectedNames.join(','));

                    // have the X appear on hover. something is selected
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();

                    $spanNames.text(selectedNames.join(', '));

                    $(this).closest('.picker-menu').slideUp(function () {
                        self.updateScrollbar();
                    });

                    if (!(event.srcElement == this)) {
                        // if this event was called by something other than the button itself, make sure the execute the href (which is probably javascript)
                        if ($(this).attr('href')) {
                            window.location.href = $(this).attr('href');
                        }
                    }
                });

                $control.find('.picker-select-none').click(function (e) {
                    e.stopImmediatePropagation();
                    var rockTree = $control.find('.treeview').data('rockTree');
                    rockTree.clear();
                    $hfItemIds.val('0');
                    $hfItemNames.val('');

                    // don't have the X appear on hover. nothing is selected
                    $control.find('.picker-select-none').removeClass('rollover-item');
                    $control.find('.picker-select-none').hide();

                    $control.siblings('.js-hide-on-select-none').hide();

                    $spanNames.text(self.options.defaultText);
                });
            },
            updateScrollbar: function () {
                // first, update this control's scrollbar, then the modal's
                var $container = $('#' + this.options.controlId).find('.scroll-container')

                if ($container.is(':visible')) {
                    $container.tinyscrollbar_update('relative');
                }

                // update the outer modal  
                Rock.dialogs.updateModalScrollBar(this.options.controlId);
            }
        };

        exports = {
            defaults: {
                id: 0,
                controlId: null,
                restUrl: null,
                restParams: null,
                allowMultiSelect: false,
                defaultText: '',
                selectedIds: null,
                expandedIds: null
            },
            controls: {},
            initialize: function (options) {
                var settings,
                    itemPicker;

                if (!options.controlId) throw 'controlId must be set';
                if (!options.restUrl) throw 'restUrl must be set';

                settings = $.extend({}, exports.defaults, options);

                if (!settings.defaultText) {
                    settings.defaultText = exports.defaults.defaultText;
                }

                itemPicker = new ItemPicker(settings);
                exports.controls[settings.controlId] = itemPicker;
                itemPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));