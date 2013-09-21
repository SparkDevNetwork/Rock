(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.itemPicker = (function () {
        // Kendo Treeview does not participate in ViewState, so if checkboxes are enabled
        // need to re-select the checkboxes for each checked node.
        var _registerPostBackHandler = function (itemPicker) {
                var controlId = itemPicker.controlId,
                    $treeview = $('#treeviewItems_' + controlId),
                    onPostBack = function () {
                        // itempicker might not exist if it was on an updatepanel and Visible set to false after postback
                        if (!$('#' + itemPicker.controlId).length) return;

                        if (!itemPicker.isMultiSelect) return;

                        var val = $('#hfItemId_' + controlId).val(),
                            ids = val.split(','),
                            $checkbox,
                            i;

                        // Find each selected node by its model's Id and attempt to check it
                        for (i = 0; i < ids.length; i++) {
                            $checkbox = $treeview.find('input[data-id="' + ids[i] + '"]');
                            $checkbox.prop('checked', true);
                        }
                    };
            
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(onPostBack);
            },
            ItemPicker = function (options) {
                this.controlId = options.controlId;
                this.restUrl = options.restUrl;
                this.allowMultiSelect = options.allowMultiSelect;
                this.defaultText = options.defaultText || '<none>';
            };

        ItemPicker.prototype.updateScrollbar = function (e) {
            var findControl = Rock.controls.itemPicker.findControl,
                controlId = typeof e === 'string' ? e : e.sender.element[0].id,
                control = findControl(controlId),
                $container = $('#treeview-scroll-container_' + control.controlId),
                $dialog = $('#modal-scroll-container');

            if ($container.is(':visible')) {
                $container.tinyscrollbar_update('relative');

                if ($dialog && $dialog.is(':visible')) {
                    var dialogTop = $dialog.offset().top;
                    var pickerTop = $container.offset().top;
                    var amount = pickerTop - dialogTop;
                    if (amount > 160) {
                        $dialog.tinyscrollbar_update('bottom');
                    }
                }
            }
        };

        ItemPicker.prototype.findChildItemInTree = function (treeViewData, itemId, itemParentIds) {
            var itemParentList,
                parentItemId,
                parentItem,
                parentNodeItem,
                initialItem;

            if (itemParentIds) {
                itemParentList = itemParentIds.split(',');

                for (var i = 0; i < itemParentList.length; i++) {
                    parentItemId = itemParentList[i];
                    parentItem = treeViewData.dataSource.get(parentItemId);
                    parentNodeItem = treeViewData.findByUid(parentItem.uid);

                    if (!parentItem.expanded && parentItem.hasChildren) {
                        treeViewData.expand(parentNodeItem);
                        return null;
                    }
                }
            }

            initialItem = treeViewData.dataSource.get(itemId);
            return initialItem;
        };

        ItemPicker.prototype.initializeCheckBoxes = function () {
            var controlId = this.controlId,
                defaultText = this.defaultText,
                $treeView = $('#treeviewItems_' + controlId),
                findCheckedNodes = function (nodes, nodeList) {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].checked) {
                            nodeList.push({
                                id: nodes[i].Id,        // The model's actual `Id` in the database
                                name: nodes[i].Name,    // The model's name
                                uid: nodes[i].uid       // The guid that Kendo assigns to the node on each postback
                            });
                        }

                        if (nodes[i].hasChildren) {
                            findCheckedNodes(nodes[i].children.view(), nodeList);
                        }
                    }
                },
                selectedNodes = [];

            $treeView.data('kendoTreeView').dataSource.bind('change', function () {
                var nodes = $treeView.data('kendoTreeView').dataSource.view(),
                    $selectedItemLabel = $('#selectedItemLabel_' + controlId),
                    $hiddenItemId = $('#hfItemId_' + controlId),
                    $hiddenItemName = $('#hfItemName_' + controlId),
                    ids,
                    names,
                    selectedValues = '0',
                    selectedNames = defaultText;
                
                selectedNodes = [];
                findCheckedNodes(nodes, selectedNodes);

                if (selectedNodes.length) {
                    ids = [];
                    names = [];

                    for (var i = 0; i < selectedNodes.length; i++) {
                        ids.push(selectedNodes[i].id);
                        names.push(selectedNodes[i].name);
                    }

                    selectedValues = ids.join(',');
                    selectedNames = names.join(', ');
                }

                $hiddenItemId.val(selectedValues);
                $hiddenItemName.val(selectedNames);
                $selectedItemLabel.val(selectedValues);
                $selectedItemLabel.text(selectedNames);
            });
        };

        ItemPicker.prototype.onDataBound = function (options) {
            var control = Rock.controls.itemPicker.findControl(options.sender.element[0].id),
                treeViewData = $('#treeviewItems_' + control.controlId).data('kendoTreeView'),
                selectedNode = treeViewData.select(),
                nodeData = this.dataItem(selectedNode),
                initialItemId,
                initialItemParentIds,
                initialItem,
                firstItem,
                firstDataItem;

            if (!nodeData) {
                initialItemId = $('#hfItemId_' + control.controlId).val();
                initialItemParentIds = $('#hfInitialItemParentIds_' + control.controlId).val();
                initialItem = control.findChildItemInTree(treeViewData, initialItemId, initialItemParentIds);

                if (initialItemId && initialItem) {
                    firstItem = treeViewData.findByUid(initialItem.uid);
                    firstDataItem = this.dataItem(firstItem);

                    if (firstDataItem) {
                        treeViewData.select(firstItem);
                    }
                }
            }

            control.updateScrollbar(control.controlId);

            if (control.isMultiSelect) {
                control.initializeCheckBoxes();
            }
        };

        ItemPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                defaultText = this.defaultText,
                isMultiSelect = this.isMultiSelect,
                updateScrollbar = this.updateScrollbar;

            $('#' + controlId + ' a.picker-label').click(function (e) {
                e.preventDefault();
                $('#' + controlId).find('.picker-menu').first().toggle();
                updateScrollbar(controlId);
            });

            $('#' + controlId).hover(
                function () {
                    if ($('#hfItemId_' + controlId).val() !== '0') {
                        $('#btnSelectNone_' + controlId).stop().show();
                    }
                },
                function () {
                    $('#btnSelectNone_' + controlId).fadeOut(500);
                });

            $('#btnCancel_' + controlId).click(function () {
                $(this).closest('.picker-menu').slideUp();
            });

            $('#btnSelectNone_' + controlId).click(function (e) {
                e.stopImmediatePropagation();

                var selectedValue = '0',
                    selectedText = defaultText,
                    $selectedItemLabel = $('#selectedItemLabel_' + controlId),
                    $hiddenItemId = $('#hfItemId_' + controlId),
                    $hiddenItemName = $('#hfItemName_' + controlId);

                $hiddenItemId.val(selectedValue);
                $hiddenItemName.val(selectedText);
                $selectedItemLabel.val(selectedValue);
                $selectedItemLabel.text(selectedText);
                return false;
            });

            $('#btnSelect_' + controlId).click(function () {
                var treeViewData = $('#treeviewItems_' + controlId).data('kendoTreeView'),
                    selectedNode = treeViewData.select(),
                    nodeData = treeViewData.dataItem(selectedNode),
                    selectedValue = '0',
                    selectedText = defaultText,
                    $selectedItemLabel = $('#selectedItemLabel_' + controlId),
                    $hiddenItemId = $('#hfItemId_' + controlId),
                    $hiddenItemName = $('#hfItemName_' + controlId);

                if (!isMultiSelect) {
                    if (nodeData) {
                        selectedValue = nodeData.Id;
                        selectedText = nodeData.Name;
                    }

                    $hiddenItemId.val(selectedValue);
                    $hiddenItemName.val(selectedText);
                    $selectedItemLabel.val(selectedValue);
                    $selectedItemLabel.text(selectedText);
                }

                $(this).closest('.picker-menu').slideUp();
            });
        };

        ItemPicker.prototype.initialize = function () {
            var itemList,
                showCheckboxes,
                controlId = this.controlId,
                restUrl = this.restUrl;

            itemList = new kendo.data.HierarchicalDataSource({
                transport: {
                    read: {
                        url: function (options) {
                            var extraParams = $('#hfItemRestUrlExtraParams_' + controlId).val(),
                                requestUrl = restUrl + (options.Id || 0) + (extraParams || '');
                            return requestUrl;
                        },
                        error: function (xhr, status, error) {
                            console.log(status + ' [' + error + ']: ' + xhr.responseText);
                        }
                    }
                },
                schema: {
                    model: {
                        id: 'Id',
                        hasChildren: 'HasChildren'
                    }
                }
            });

            if (this.allowMultiSelect) {
                showCheckboxes = {
                    checkChildren: true,
                    template: '<input type="checkbox" name="account[#= item.Id #]" data-id="#= item.Id #" />'
                };
                this.isMultiSelect = true;
            } else {
                showCheckboxes = false;
                this.isMultiSelect = false;
            }

            $('#treeviewItems_' + controlId).kendoTreeView({
                template: '<i class="#= item.IconCssClass #"></i> #= item.Name #',
                dataSource: itemList,
                dataTextField: 'Name',
                dataImageUrlField: 'IconSmallUrl',
                checkboxes: showCheckboxes,
                dataBound: this.onDataBound,
                select: this.updateScrollbar
            });

            $('#treeview-scroll-container_' + controlId).tinyscrollbar({ size: 120 });
            this.initializeEventHandlers();
        };

        var exports = {
            itemPickers: {},
            findControl: function (controlId) {
                var index = controlId.indexOf('treeviewItems_'),
                    id = index === -1 ? controlId : controlId.substring(controlId.indexOf('_') + 1);
                return exports.itemPickers[id];
            },
            initialize: function (options) {
                var itemPicker;

                if (!options.controlId) throw '`controlId` is required';
                if (!options.restUrl) throw '`resturl` is required';

                // Check to see if the item picker is already in the cache.
                // If so, this is a postback.
                if (exports.itemPickers[options.controlId]) {
                    itemPicker = exports.itemPickers[options.controlId];
                } else {
                    // If not in a postback, create a new instance of itempicker,
                    // cache it, and register postback handler.
                    itemPicker = new ItemPicker(options);
                    exports.itemPickers[options.controlId] = itemPicker;
                    _registerPostBackHandler(itemPicker);
                }
                
                itemPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));