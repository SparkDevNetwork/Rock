(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = {};

    Rock.controls.itemPicker = (function () {
        var _controlId,
            _restUrl,
            _isMultiSelect = false,
            _updateScrollbar = function () {
                var $container = $('#treeview-scroll-container_' + _controlId),
                    $dialog = $('#modal-scroll-container');

                if ($container.is(':visible')) {
                    $container.tinyscrollbar_update('relative');

                    if ($dialog && $dialog.is(':visible')) {
                        $dialog.tinyscrollbar_update('bottom');
                    }
                }
            },
            _findChildItemInTree = function (treeViewData, itemId, itemParentIds) {
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
            },
            _initializeCheckboxes = function () {
                var $treeView = $('#treeviewItems_' + _controlId),
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
                        $selectedItemLabel = $('#selectedItemLabel_' + _controlId),
                        $hiddenItemId = $('#hfItemId_' + _controlId),
                        $hiddenItemName = $('#hfItemName_' + _controlId),
                        ids,
                        names,
                        selectedValues = '0',
                        selectedNames = '<none>';;
                    
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
            },
            _onDataBound = function () {
                var treeViewData = $('#treeviewItems_' + _controlId).data('kendoTreeView'),
                    selectedNode = treeViewData.select(),
                    nodeData = this.dataItem(selectedNode),
                    initialItemId,
                    initialItemParentIds,
                    initialItem,
                    firstItem,
                    firstDataItem;

                if (!nodeData) {
                    initialItemId = $('#hfItemId_' + _controlId).val();
                    initialItemParentIds = $('#hfInitialItemParentIds_' + _controlId).val();
                    initialItem = _findChildItemInTree(treeViewData, initialItemId, initialItemParentIds);

                    if (initialItemId && initialItem) {
                        firstItem = treeViewData.findByUid(initialItem.uid);
                        firstDataItem = this.dataItem(firstItem);

                        if (firstDataItem) {
                            treeViewData.select(firstItem);
                        }
                    }
                }

                _updateScrollbar(_controlId);

                if (_isMultiSelect) {
                    _initializeCheckboxes();
                }
            },
            _initializeEventHandlers = function () {
                $('#' + _controlId + ' a.rock-picker').click(function (e) {
                    $(this).parent().siblings('.rock-picker').first().toggle();
                    _updateScrollbar(_controlId);
                });

                $('#' + _controlId + ' .rock-picker-select').hover(
                    function () {
                        if ($('#hfItemId_' + _controlId).val() !== '0') {
                            $('#btnSelectNone_' + _controlId).stop().show();
                        }
                    },
                    function () {
                        $('#btnSelectNone_' + _controlId).fadeOut(500);
                    });

                $('#btnCancel_' + _controlId).click(function (e) {
                    $(this).parent().slideUp();
                });

                $('#btnSelectNone_' + _controlId).click(function (e) {
                    e.stopImmediatePropagation();

                    var selectedValue = '0',
                        selectedText = '<none>',
                        $selectedItemLabel = $('#selectedItemLabel_' + _controlId),
                        $hiddenItemId = $('#hfItemId_' + _controlId),
                        $hiddenItemName = $('#hfItemName_' + _controlId);

                    $hiddenItemId.val(selectedValue);
                    $hiddenItemName.val(selectedText);
                    $selectedItemLabel.val(selectedValue);
                    $selectedItemLabel.text(selectedText);
                    return false;
                });

                $('#btnSelect_' + _controlId).click(function (e) {
                    var treeViewData = $('#treeviewItems_' + _controlId).data('kendoTreeView'),
                        selectedNode = treeViewData.select(),
                        nodeData = treeViewData.dataItem(selectedNode),
                        selectedValue = '0',
                        selectedText = '<none>',
                        $selectedItemLabel = $('#selectedItemLabel_' + _controlId),
                        $hiddenItemId = $('#hfItemId_' + _controlId),
                        $hiddenItemName = $('#hfItemName_' + _controlId);

                    if (!_isMultiSelect) {
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

        return {
            initialize: function (options) {
                var itemList,
                    showCheckboxes;

                if (!options.controlId) throw '`controlId` is a required field.';
                if (!options.restUrl) throw '`restUrl` is a required field.';

                _controlId = options.controlId;
                _restUrl = options.restUrl;
                itemList = new kendo.data.HierarchicalDataSource({
                    transport: {
                        read: {
                            url: function (options) {
                                var extraParams = $('#hfItemResturlExtraParams_' + _controlId).val(),
                                    requestUrl = _restUrl + (options.Id || 0) + (extraParams || '');
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

                if (options.allowMultiSelect) {
                    showCheckboxes = {
                        checkChildren: true,
                        template: '<input type="checkbox" name="account[#= item.Id #]" data-id="#= item.Id #" />'
                    };
                    _isMultiSelect = true;
                } else {
                    showCheckboxes = false;
                }

                $('#treeviewItems_' + _controlId).kendoTreeView({
                    template: '<i class="#= item.IconCssClass #"></i> #= item.Name #',
                    dataSource: itemList,
                    dataTextField: 'Name',
                    dataImageUrlField: 'IconSmallUrl',
                    checkboxes: showCheckboxes,
                    dataBound: _onDataBound,
                    select: _updateScrollbar
                });

                $('#treeview-scroll-container_' + _controlId).tinyscrollbar({ size: 120 });
                _initializeEventHandlers(_controlId);
            }
        };
    }());

    Rock.controls.grid = (function () {
        return {
            confirmDelete: function (e, nameText) {
                e.preventDefault();
                bootbox.dialog('Are you sure you want to delete this ' + nameText + '?',
                    [
                        {
                            label: 'OK', 
                            'class': 'btn-primary', 
                            callback: function () {
                                var postbackJs = e.target.href ? e.target.href : e.target.parentElement.href;

                                // need to do unescape because firefox might put %20 instead of spaces
                                postbackJs = unescape(postbackJs);
                                
                                // Careful!
                                eval(postbackJs);
                            }
                        },
                        {
                            label: 'Cancel',
                            'class': 'btn-secondary'
                        }
                    ]);
            }
        };
    }());

    Rock.controls.personPicker = (function () {
        var _controlId,
            _restUrl,
            _initializeEventHandlers = function () {
                // TODO: Can we use TypeHead here (already integrated into BootStrap) instead of jQueryUI?
                // Might be a good opportunity to break the dependency on jQueryUI.
                $('#personPicker_' + _controlId).autocomplete({
                    source: function (request, response) {
                        var promise = $.ajax({
                            url: _restUrl + request.term,
                            dataType: 'json'
                        });

                        promise.done(function (data, status, xhr) {
                            $('#personPickerItems_' + _controlId).first().html('');
                            response($.map(data, function (item) {
                                return item;
                            }));
                        });

                        // Is this needed? If an error is thrown on the server, we should see an exception in the log now...
                        promise.fail(function (xhr, status, error) {
                            console.log(status + ' [' + error + ']: ' + xhr.responseText);

                            // TODO: Display some feedback to the user that something went wrong?
                        });
                    },
                    minLength: 3,
                    html: true,
                    appendTo: '#personPickerItems_' + _controlId,
                    messages: {
                        noResults: function () {},
                        results: function () {}
                    }
                });

                $('a.rock-picker').click(function (e) {
                    e.preventDefault();
                    $(this).next('.rock-picker').toggle();
                });

                $('.rock-picker-select').on('click', '.rock-picker-select-item', function (e) {
                    var $selectedItem = $(this).attr('data-person-id');

                    // hide other open details
                    $('.rock-picker-select-item-details').each(function (index) {
                        var $el = $(this),
                            $currentItem = $el.parent().attr('data-person-id');

                        if ($currentItem != $selectedItem) {
                            $el.slideUp();
                        }
                    });

                    $(this).find('.rock-picker-select-item-details:hidden').slideDown();
                });

                $('#btnCancel_' + _controlId).click(function (e) {
                    $(this).parent().slideUp();
                });

                $('#btnSelect_' + _controlId).click(function (e) {
                    var radInput = $('#' + _controlId).find('input:checked'),

                        selectedValue = radInput.val(),
                        selectedText = radInput.parent().text(),

                        selectedPersonLabel = $('#selectedPersonLabel_' + _controlId),

                        hiddenPersonId = $('#hfPersonId_' + _controlId),
                        hiddenPersonName = $('#hfPersonName_' + _controlId);

                    console.log(radInput);       // []
                    console.log(selectedValue);  // undefined
                    console.log(selectedText);   // ''

                    hiddenPersonId.val(selectedValue);
                    hiddenPersonName.val(selectedText);

                    selectedPersonLabel.val(selectedValue);
                    selectedPersonLabel.text(selectedText);

                    $(this).parent().slideUp();
                });
            };

        return {
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                if (!options.restUrl) throw '`restUrl` is required.';
                _controlId = options.controlId;
                _restUrl = options.restUrl;

                $.extend($.ui.autocomplete.prototype, {
                    _renderItem: function ($ul, item) {
                        if (this.options.html) {
                            // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                            // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                            var $label = $('<label/>').text(item.Name),

                                $radio = $('<input type="radio" name="person-id" />')
                                    .attr('id', item.Id)
                                    .attr('value', item.Id)
                                    .prependTo($label),

                                $li = $('<li/>')
                                    .addClass('rock-picker-select-item')
                                    .attr('data-person-id', item.Id)
                                    .html($label),

                                $resultSection = $(this.options.appendTo);

                            $(item.PickerItemDetailsHtml).appendTo($li);

                            if (!item.IsActive) {
                                $li.addClass('inactive');
                            }

                            return $resultSection.append($li);
                        }
                        else {
                            return $('<li></li>')
                                .data('item.autocomplete', item)
                                .append($('<a></a>').text(item.label))
                                .appendTo($ul);
                        }
                    }
                });
                
                _initializeEventHandlers();
            }
        }
    }());
}(jQuery));