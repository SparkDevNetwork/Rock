(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.itemPicker = (function () {
        var ItemPicker = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
            this.allowMultiSelect = options.allowMultiSelect;
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
                    $dialog.tinyscrollbar_update('bottom');
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
                $treeView = $('#treeviewItems_' + controlId),
                findCheckedNodes = function (nodes, selectedNodes) {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].checked) {
                            selectedNodes.push({ id: nodes[i].Id, name: nodes[i].Name, uid: nodes[i].uid });
                        }

                        if (nodes[i].hasChildren) {
                            findCheckedNodes(nodes[i].children.view(), selectedNodes);
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
                    selectedNames = '<none>';
                
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

            // Kendo TreeView does not participate in ViewState, so checkboxes must
            // be re-populated after PostBack from cached values.
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                var data = $treeView.data('kendoTreeView'),
                    node;

                for (var i = 0; i < selectedNodes.length; i++) {
                    node = data.findByUid(selectedNodes[i].uid);
                    $(node).find(':checkbox').attr('checked', true);
                }
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
                isMultiSelect = this.isMultiSelect,
                updateScrollbar = this.updateScrollbar;

            $('#' + controlId + ' a.rock-picker').click(function (e) {
                e.preventDefault();
                $(this).parent().siblings('.rock-picker').first().toggle();
                updateScrollbar(controlId);
            });

            $('#' + controlId + ' .rock-picker-select').hover(
                function () {
                    if ($('#hfItemId_' + controlId).val() !== '0') {
                        $('#btnSelectNone_' + controlId).stop().show();
                    }
                },
                function () {
                    $('#btnSelectNone_' + controlId).fadeOut(500);
                });

            $('#btnCancel_' + controlId).click(function () {
                $(this).parent().slideUp();
            });

            $('#btnSelectNone_' + controlId).click(function (e) {
                e.stopImmediatePropagation();

                var selectedValue = '0',
                    selectedText = '<none>',
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
                    selectedText = '<none>',
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

                $(this).parent().slideUp();
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
                            var extraParams = $('#hfItemResturlExtraParams_' + controlId).val(),
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
                if (!options.controlId) throw '`controlId` is required';
                if (!options.restUrl) throw '`resturl` is required';

                var itemPicker = new ItemPicker(options);
                exports.itemPickers[options.controlId] = itemPicker;
                itemPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));