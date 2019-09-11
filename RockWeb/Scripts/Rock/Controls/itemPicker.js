(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.itemPicker = (function () {
        var ItemPicker = function (options) {
            this.options = options;

            // set a flag so that the picker only auto-scrolls to a selected item once. This prevents it from scrolling at unwanted times
            this.alreadyScrolledToSelected = false;
            this.iScroll = null;
        },
            exports;

        ItemPicker.prototype = {
            constructor: ItemPicker,
            initialize: function () {
                var $control = $('#' + this.options.controlId),
                    $tree = $control.find('.treeview'),
                    treeOptions = {
                        multiselect: this.options.allowMultiSelect,
                        categorySelection: this.options.allowCategorySelection,
                        restUrl: this.options.restUrl,
                        restParams: this.options.restParams,
                        expandedIds: this.options.expandedIds,
                        showSelectChildren: this.options.showSelectChildren,
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

                var $scrollContainer = $control.find('.scroll-container .viewport');
                var $scrollIndicator = $control.find('.track');
                this.iScroll = new IScroll($scrollContainer[0], {
                    mouseWheel: true,
                    indicators: {
                        el: $scrollIndicator[0],
                        interactive: true,
                        resize: false,
                        listenY: true,
                        listenX: false,
                    },
                    click: false,
                    preventDefaultException: { tagName: /.*/ }
                });

                // Since some handlers are "live" events, they need to be bound before tree is initialized
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
                        // make sure it doesn't autoscroll after something has been manually clicked
                        self.alreadyScrolledToSelected = true;
                        if (!self.options.allowMultiSelect) {
                            $control.find('.picker-btn').trigger('click');
                        }
                    })
                    .on('rockTree:expand rockTree:collapse rockTree:dataBound', function (evt) {
                        self.updateScrollbar();
                    })
                    .on('rockTree:rendered', function (evt) {
                        self.scrollToSelectedItem();
                    });

                $control.find('a.picker-label').click(function (e) {
                    e.preventDefault();
                    $(this).toggleClass("active");
                    $control.find('.picker-menu').first().toggle(0, function () {
                        self.scrollToSelectedItem();
                    });
                });

                $control.find('.picker-cancel').click(function () {
                    $(this).toggleClass("active");
                    $(this).closest('.picker-menu').toggle(0, function () {
                        self.updateScrollbar();
                    });
                    $(this).closest('a.picker-label').toggleClass("active");
                });

                // have the X appear on hover if something is selected
                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();
                }

                $control.find('.picker-btn').click(function (el) {

                    var rockTree = $control.find('.treeview').data('rockTree'),
                            selectedNodes = rockTree.selectedNodes,
                            selectedIds = [],
                            selectedNames = [];

                  $.each(selectedNodes, function (index, node) {
                    var nodeName = $("<textarea/>").html(node.name).text();
                    selectedNames.push(nodeName);
                    selectedIds.push(node.id);
                    });

                    $hfItemIds.val(selectedIds.join(','));
                    $hfItemNames.val(selectedNames.join(','));

                    // have the X appear on hover. something is selected
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();

                    $spanNames.text(selectedNames.join(', '));
                    $spanNames.attr('title', $spanNames.text());

                    $(this).closest('a.picker-label').toggleClass("active");
                    $(this).closest('.picker-menu').toggle(0, function () {
                        self.updateScrollbar();
                    });

                    if (!(el && el.originalEvent && el.originalEvent.srcElement == this)) {
                        // if this event was called by something other than the button itself, make sure the execute the href (which is probably javascript)
                        var jsPostback = $(this).attr('href');
                        if (jsPostback) {
                            window.location = jsPostback;
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
                    $spanNames.attr('title', $spanNames.text());
                });

                // clicking on the 'select all' btn
                $control.on('click', '.js-select-all', function (e)
                {
                  var rockTree = $control.find('.treeview').data('rockTree');

                  e.preventDefault();
                  e.stopPropagation();

                  var $itemNameNodes = rockTree.$el.find('.rocktree-name');

                  var allItemNodesAlreadySelected = true;
                  $itemNameNodes.each(function (a)
                  {
                    if (!$(this).hasClass('selected')) {
                      allItemNodesAlreadySelected = false;
                    }
                  });

                  if (!allItemNodesAlreadySelected) {
                    // mark them all as unselected (just in case some are selected already), then click them to select them
                    $itemNameNodes.removeClass('selected');
                    $itemNameNodes.click();
                  } else {
                    // if all were already selected, toggle them to unselected
                    rockTree.setSelected([]);
                    $itemNameNodes.removeClass('selected');
                  }
                });
            },
            updateScrollbar: function (sPosition) {
                var self = this;
                // first, update this control's scrollbar, then the modal's
                var $container = $('#' + this.options.controlId).find('.scroll-container');

                if ($container.is(':visible')) {
                    if (!sPosition) {
                        sPosition = 'relative'
                    }
                    if (self.iScroll) {
                        self.iScroll.refresh();
                    }
                }

                // update the outer modal
                Rock.dialogs.updateModalScrollBar(this.options.controlId);
            },
            scrollToSelectedItem: function () {
                var $selectedItem = $('#' + this.options.controlId).find('.picker-menu').find('.selected').first();
                if ($selectedItem.length && (!this.alreadyScrolledToSelected)) {
                    this.updateScrollbar();
                    this.iScroll.scrollToElement('.selected', '0s');
                    this.alreadyScrolledToSelected = true;
                } else {
                    // initialize/update the scrollbar
                    this.updateScrollbar();
                }
            }
        };

        exports = {
            defaults: {
                id: 0,
                controlId: null,
                restUrl: null,
                restParams: null,
                allowCategorySelection: false,
                allowMultiSelect: false,
                defaultText: '',
                selectedIds: null,
                expandedIds: null,
                showSelectChildren: false
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
