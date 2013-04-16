(function ($) {
    'use strict';
    window.Rock = window.Rock || {},
    Rock.controls = {};

    Rock.controls.itemPicker = (function () {
        var _controlId,
            _restUrl,
            _updateScrollbar = function (controlId) {
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
            },
            _initializeEventHandlers = function (controlId) {
                $('#' + _controlId + ' a.rock-picker').click(function (e) {
                    e.preventDefault();
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

                $('#btnCancel_' + _controlId).click(function () {
                    $(this).parent().slideUp();
                })

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

                $('#btnSelect_' + _controlId).click(function () {
                    var treeViewData = $('#treeviewItems_' + _controlId).data('kendoTreeView'),
                        selectedNode = treeViewData.select(),
                        nodeData = treeViewData.dataItem(selectedNode),
                        selectedValue = '0',
                        selectedText = '<none>',
                        $selectedItemLabel = $('#selectedItemLabel_' + _controlId),
                        $hiddenItemId = $('#hfItemId_' + _controlId),
                        $hiddenItemName = $('#hfItemName_' + _controlId);

                    if (nodeData) {
                        selectedValue = nodeData.Id;
                        selectedText = nodeData.Name;
                    }

                    $hiddenItemId.val(selectedValue);
                    $hiddenItemName.val(selectedText);
                    $selectedItemLabel.val(selectedValue);
                    $selectedItemLabel.text(selectedText);
                    $(this).parent().slideUp();
                });
            };

        return {
            initialize: function (options) {
                var itemList;

                if (!options.controlId) throw 'ClientID is a required field.';
                if (!options.restUrl) throw 'RestUrl is a required field.';

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

                $('#treeviewItems_' + _controlId).kendoTreeView({
                    template: '<i class="#= item.IconCssClass #"></i> #= item.Name #',
                    dataSource: itemList,
                    dataTextField: 'Name',
                    dataImageUrlField: 'IconSmallUrl',
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
                            "label": "OK", "class": "btn-primary", "callback": function () {
                                var postbackJs = e.target.href;
                                if (postbackJs == null) {
                                    postbackJs = e.target.parentElement.href;
                                }
                                // need to do unescape because firefox might put %20 instead of spaces
                                postbackJs = unescape(postbackJs);
                                eval(postbackJs)
                            }
                        },
                        {
                            "label": "Cancel", "class": "btn-secondary"
                        }
                    ]);
            }
        };
    }());

    Rock.controls.personPicker = (function () {
        var _controlId,
            _restUrl,
            _initializeEventHandlers = function () {
                $('#personPicker_' + _controlId).autocomplete({
                    source: function (request, response) {
                        $.ajax({
                            url: _restUrl + request.term,
                            dataType: 'json',
                            success: function (data, status, xhr) {
                                $('#personPickerItems_' + _controlId)[0].innerHTML = '';
                                response($.map(data, function (item) {
                                    return item;
                                }))
                            },
                            error: function (xhr, status, error) {
                                console.log(status + ' [' + error + ']: ' + xhr.responseText);
                            }
                        });
                    },
                    minLength: 3,
                    html: true,
                    appendTo: 'personPickerItems_' + _controlId,
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
                            currentItem = $el.parent().attr('data-person-id');

                        if (currentItem != $selectedItem) {
                            $el.slideUp();
                        }
                    });

                    $(this).find('.rock-picker-select-item-details:hidden').slideDown();
                });

                $('#btnCancel_' + _controlId).click(function (e) {
                    $(this).parent().slideUp();
                });

                $('#btnSelect_' + _controlId).click(function (e) {
                    var radInput = $('#' + _controlId).find('input:checked');

                    var selectedValue = radInput.val();
                    var selectedText = radInput.parent().text();

                    var selectedPersonLabel = $('#selectedPersonLabel_' + _controlId);

                    var hiddenPersonId = $('#hfPersonId_' + _controlId);
                    var hiddenPersonName = $('#hfPersonName_' + _controlId);

                    hiddenPersonId.val(selectedValue);
                    hiddenPersonName.val(selectedText);

                    selectedPersonLabel.val(selectedValue);
                    selectedPersonLabel.text(selectedText);

                    $(this).parent().slideUp();
                });

            };

        return {
            initialize: function (options) {
                if (!options.controlId) throw 'ControlId is required.';
                if (!options.restUrl) throw 'RestUrl is required.';
                _controlId = options.controlId;
                _restUrl = options.restUrl;

                $.extend($.ui.autocomplete.prototype, {
                    _renderItem: function (ul, item) {

                        if (this.options.html) {

                            // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                            // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                            var listItem = document.createElement("li");
                            listItem.className = "rock-picker-select-item";
                            if (!item.IsActive) {
                                listItem.className += " inactive";
                            }
                            listItem.setAttribute('data-person-id', item.Id);
                            listItem.innerHTML = '<label><input type="radio" id="' + item.Id + '" name="person-id" value="' + item.Id + '">' + item.Name + '</label>'
                                    + item.PickerItemDetailsHtml;
                            var myResultSection = $('#' + this.options.appendTo);
                            return myResultSection.append(listItem);
                        }
                        else {
                            return $("<li></li>")
                                    .data("item.autocomplete", item)
                                    .append($("<a></a>")["text"](item.label))
                                    .appendTo(ul);
                        }
                    }
                });
                console.log('initializing person picker');
                _initializeEventHandlers();
            }
        }
    }());
}(jQuery));