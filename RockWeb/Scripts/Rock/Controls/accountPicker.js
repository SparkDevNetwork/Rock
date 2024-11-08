(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.accountPicker = (function () {
        var AccountPicker = function (options) {
            this.options = options;
            // set a flag so that the picker only auto-scrolls to a selected item once. This prevents it from scrolling at unwanted times
            this.alreadyScrolledToSelected = false;
            this.iScroll = null;
        },
            exports;

        AccountPicker.prototype = {
            constructor: AccountPicker,
            initialize: function () {
                var $control = $('#' + this.options.controlId),
                    $tree = $control.find('.treeview'),
                    treeOptions = {
                        customDataItems: this.options.customDataItems,
                        enhanceForLongLists: this.options.enhanceForLongLists,
                        multiselect: this.options.allowMultiSelect,
                        categorySelection: this.options.allowCategorySelection,
                        categoryPrefix: this.options.categoryPrefix,
                        restUrl: this.options.restUrl,
                        searchRestUrl: this.options.searchRestUrl,
                        restParams: this.options.restParams,
                        expandedIds: this.options.expandedIds,
                        expandedCategoryIds: this.options.expandedCategoryIds,
                        showSelectChildren: this.options.showSelectChildren,
                        id: this.options.startingId
                    },

                    // used server-side on postback to display the selected nodes
                    $hfItemIds = $control.find('.js-item-id-value'),
                    // used server-side on postback to display the expanded nodes
                    $hfExpandedIds = $control.find('.js-initial-item-parent-ids-value'),
                    $hfExpandedCategoryIds = $control.find('.js-expanded-category-ids');

                // Custom mapping override to add features items
                this.options.mapItems = function (arr, treeView) {

                    return $.map(arr, function (item) {

                        var node = {
                            id: item.Guid || item.Id,
                            name: item.Name || item.Title,
                            iconCssClass: item.IconCssClass,
                            parentId: item.ParentId,
                            hasChildren: item.HasChildren,
                            isActive: item.IsActive,
                            countInfo: item.CountInfo,
                            isCategory: item.IsCategory,
                            path: item.Path,
                            totalCount: item.TotalCount
                        };

                        // Custom node properties passed in from the *Picker.cs using the ItemPicker base CustomDataItems property
                        if (treeView.options.customDataItems && treeView.options.customDataItems.length > 0) {
                            treeView.options.customDataItems.forEach(function (dataItem, idx) {
                                if (!node.hasOwnProperty(dataItem)) {
                                    node['' + dataItem.itemKey + ''] = item['' + dataItem.itemValueKey + ''];
                                }
                            });
                        }

                        if (node.parentId === null) {
                            node.parentId = '0';
                        }

                        if (item.Children && typeof item.Children.length === 'number') {

                            // traverse using the _mapArrayDefault in rockTree.js
                            node.children = _mapArrayDefault(item.Children, treeView);
                        }

                        if (node.isCategory) {
                            node.id = treeView.options.categoryPrefix + node.id;
                        }

                        return node;
                    });
                }

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

                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    treeOptions.selectedIds = $hfItemIds.val().split(',');
                }

                if ($hfExpandedIds.val()) {
                    treeOptions.expandedIds = $hfExpandedIds.val().split(',');
                }

                if ($hfExpandedCategoryIds.val()) {
                    treeOptions.expandedCategoryIds = $hfExpandedCategoryIds.val().split(',');
                }

                // Initialize the rockTree and pass the tree options also makes http fetches
                $tree.rockTree(treeOptions);

                $control.find('.picker-preview').hide();
                $control.find('.picker-treeview').hide();

                if (treeOptions.allowMultiSelect) {
                    $control.find('.picker-preview').remove();
                    $control.find('.picker-treeview').remove();
                }

                this.updateScrollbar();
            },
            initializeEventHandlers: function () {
                var self = this,
                    $control = $('#' + this.options.controlId),
                    $spanNames = $control.find('.selected-names'),
                    $hfItemIds = $control.find('.js-item-id-value'),
                    $hfExpandedIds = $control.find('.js-initial-item-parent-ids-value'),
                    $hfItemNames = $control.find('.js-item-name-value'),
                    $searchValueField = $control.find('.js-existing-search-value');

                // Bind tree events
                $control.find('.treeview')
                    .on('rockTree:selected', function (e) {
                        var rockTree = $control.find('.treeview').data('rockTree');

                        if (rockTree.selectedNodes) {
                            var ids = rockTree.selectedNodes.map(function (v) { return v.id }).join(',');
                            $hfItemIds.val(ids);
                        }

                        self.togglePickerElements();
                    })
                    .on('rockTree:itemClicked', function (e, data) {
                        // make sure it doesn't auto-scroll after something has been manually clicked
                        self.alreadyScrolledToSelected = true;
                        if (!self.options.allowMultiSelect) {
                            $control.find('.picker-btn').trigger('click');
                        }
                    })
                    .on('rockTree:expand rockTree:collapse rockTree:dataBound', function (evt, data) {
                        self.updateScrollbar();
                        // Get any node item so we can read to total count
                        const rockTree = $control.find('.treeview').data('rockTree');
                        const firstNode = rockTree.nodes[0];
                        if (firstNode && firstNode.totalCount > 1000) {
                            $control.find('.js-select-all').hide();
                        }

                        if (self.selectAll) {
                            self.toggleSelectAll(rockTree);
                        }
                    })
                    .on('rockTree:rendered', function (evt, data) {
                        var rockTree = $control.find('.treeview').data('rockTree');
                        self.createSearchControl();
                        self.showActiveMenu();
                        self.scrollToSelectedItem();

                        if ($hfItemIds && $hfItemIds.val().length > 0) {
                            var itemIds = $hfItemIds.val().split(',');

                            rockTree.setSelected(itemIds);
                        }

                        self.togglePickerElements();

                        if (self.getViewMode() === 'selecting') {
                            self.setViewMode('clear');
                            $control.find('.picker-btn').trigger('click', [$hfItemIds.val()]);
                        }

                        if (self.getViewMode() === 'selected') {
                            if ($hfItemIds.val() === '0' && !$control.find('.picker-menu').is(':visible') ) {
                                return;
                            }

                            $control.find('.picker-cancel').trigger('click');
                        }
                    })
                    .on('rockTree:fetchCompleted', function (evt, data) {
                        // intentionally empty
                    })

                $control.find('.picker-label').on('click', function (e) {
                    e.preventDefault();
                    $(this).toggleClass("active");
                    $control.find('.picker-menu').first().toggle(0, function () {
                        self.scrollToSelectedItem();
                        $control.find('.js-search-panel input').trigger('focus');
                    });
                });

                $control.find('.picker-cancel').on('click', function () {
                    $(this).toggleClass("active");
                    $control.find('.picker-menu').hide(0, function () {
                        self.updateScrollbar();
                    });
                    $(this).closest('.picker-label').toggleClass("active");

                    //cleanup
                    self.setViewMode('clear');
                    self.setActiveMenu(false);
                    //doPostBack();
                });

                // Preview Selection link click
                $control.find('.picker-preview').on('click', function () {
                    $control.find('.js-search-panel').hide();
                    $control.find('.picker-preview').hide();
                    $control.find('.picker-treeview').show();
                    $control.find('.js-select-all').hide();

                    self.setViewMode('preview');
                    self.togglePickerElements();

                    var rockTree = $control.find('.treeview').data('rockTree');

                    // Get all of the current rendered items so we can revert if user leaves search
                    self.currentTreeView = $control.find('.treeview').html();

                    var $viewport = $control.find('.viewport');

                    if (rockTree.selectedNodes && rockTree.selectedNodes.length > 0) {

                        var listHtml = '';
                        rockTree.selectedNodes.forEach(function (node) {
                            if (!node.path) {
                                node.path = 'Top-Level';
                            }

                            listHtml +=
                                '<div id="preview-item-' + node.id + '" class="d-flex align-items-center preview-item js-preview-item">' +
                                '         <div class="flex-fill">' +
                                '              <span class="text-color d-block">' + node.name + '</span>' +
                                '              <span class="text-muted text-sm">' + node.path.replaceAll('^', '<i class="fa fa-chevron-right pl-1 pr-1" aria-hidden="true"></i>') + '</span>' +
                                '         </div>' +
                                '         <a id="lnk-remove-preview-' + node.id + '" title="Remove From Preview" class="btn btn-link text-color btn-xs btn-square ml-auto js-remove-preview" data-id="' + node.id + '"><i class="fa fa-times"></i></a>' +
                                '</div>';
                        });

                        // Display preview list
                        var listHtmlView = '<div>' +
                            listHtml +
                            '</div>';

                        $control.find('.scroll-container').addClass('scroll-container-native').html(listHtmlView);
                        // Wire up remove event and remove from dataset

                        $control.find('.js-remove-preview').on('click', function (e) {
                            var nodeId = $(this).attr('data-id');

                            $control.find('#preview-item-' + nodeId).remove();

                            var newSelected = rockTree.selectedNodes.filter(function (fNode) {
                                return fNode.id !== nodeId;
                            });

                            rockTree.selectedNodes = newSelected;

                            // After removing a selection we need to update the hf value
                            var newIds = newSelected.map(function (n) { return n.id });

                            if (newIds && newIds.length > 0) {
                                $hfItemIds.val(newIds.join(','));
                            }
                            else {
                                $hfItemIds.val('0');
                            }

                            if ($control.find('.js-preview-item').length === 0) {
                                $control.find('.picker-treeview').trigger('click');
                            }
                        });
                    }
                });

                // Tree View link click
                $control.find('.picker-treeview').on('click', function () {
                    $searchValueField.val('');
                    self.setActiveMenu(true);
                    self.setViewMode('clear');

                    if ($hfItemIds && $hfItemIds.length > 0) {
                        var restUrl = self.options.getParentIdsUrl
                        var nodeIds = $hfItemIds.val().split(',');
                        var restUrlParams = nodeIds.map(function (id) { return 'ids=' + id }).join('&');

                        restUrl = restUrl + '?' + restUrlParams;

                        // Get the ancestor ids so the tree will expand
                        $.getJSON(restUrl, function (data, status) {
                            if (data && status === 'success') {
                                var selectedIds = [];
                                var expandedIds = [];

                                $.each(data, function (key, value) {
                                    selectedIds.push(key);

                                    value.forEach(function (kval) {
                                        if (!expandedIds.find(function (expVal) {
                                            return expVal === kval
                                        })) {
                                            expandedIds.push(kval);
                                        }
                                    });
                                });

                                if (expandedIds && expandedIds.length > 0) {
                                    $hfExpandedIds.val(expandedIds.join(','));
                                }

                                if (selectedIds && selectedIds.length > 0) {
                                    $hfItemIds.val(selectedIds.join(','));
                                }

                                doPostBack();
                            }
                        });
                    }

                });

                // have the X appear on hover if something is selected
                if ($hfItemIds.val() && $hfItemIds.val() !== '0') {
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();
                }

                // [Select] button click
                $control.find('.picker-btn').on('click', function (el) {

                    if (self.getViewMode() === 'preview' || self.getViewMode() === 'search') {
                        self.setViewMode('selecting');
                        $('#tbSearch_' + self.options.controlId).val('');
                        $control.find('.js-existing-search-value').val('');

                        if ($hfItemIds && $hfItemIds.length > 0) {
                            var restUrl = self.options.getParentIdsUrl
                            var nodeIds = $hfItemIds.val().split(',');
                            var restUrlParams = nodeIds.map(function (id) { return 'ids=' + id }).join('&');

                            restUrl = restUrl + '?' + restUrlParams;

                            // Get the ancestor ids so the tree will expand
                            $.getJSON(restUrl, function (data, status) {
                                if (data && status === 'success') {
                                    var selectedIds = [];
                                    var expandedIds = [];

                                    $.each(data, function (key, value) {
                                        selectedIds.push(key);

                                        value.forEach(function (kval) {
                                            if (!expandedIds.find(function (expVal) {
                                                return expVal === kval
                                            })) {
                                                expandedIds.push(kval);
                                            }
                                        });
                                    });

                                    if (expandedIds && expandedIds.length > 0) {
                                        $hfExpandedIds.val(expandedIds.join(','));
                                    }

                                    if (selectedIds && selectedIds.length > 0) {
                                        $hfItemIds.val(selectedIds.join(','));
                                    }

                                    doPostBack();
                                }
                            });
                        }

                        return;
                    }

                    var rockTree = $control.find('.treeview').data('rockTree'),
                        selectedNodes = rockTree.selectedNodes,
                        selectedIds = [],
                        selectedNames = [];

                    $.each(selectedNodes, function (index, node) {
                        var nodeName = $("<textarea/>").html(node.name).text();
                        selectedNames.push(nodeName);
                        if (!selectedIds.includes(node.id)) {
                            selectedIds.push(node.id);
                        }
                    });

                    // .trigger('change') is used to cause jQuery to fire any "onchange" event handlers for this hidden field.
                    $hfItemIds.val(selectedIds.join(',')).trigger('change');
                    $hfItemNames.val(selectedNames.join(','));

                    // have the X appear on hover. something is selected
                    $control.find('.picker-select-none').addClass('rollover-item');
                    $control.find('.picker-select-none').show();

                    $spanNames.text(selectedNames.join(', '));
                    $spanNames.attr('title', $spanNames.text());

                    $control.find('.picker-label').toggleClass("active");
                    $control.find('.picker-menu').hide(0, function () {
                        self.updateScrollbar();
                    });

                    self.setViewMode('selected');

                    if (!(el && el.originalEvent && el.originalEvent.srcElement === this)) {
                        // if this event was called by something other than the button itself, make sure the execute the href (which is probably javascript)
                        var jsPostback = $(this).attr('href');
                        if (jsPostback) {
                            window.location = jsPostback;
                        }
                    }
                });

                $control.find('.picker-select-none').on("click", function (e) {
                    e.preventDefault();
                    e.stopImmediatePropagation();

                    $hfItemIds.val('0').trigger('change'); // .trigger('change') is used to cause jQuery to fire any "onchange" event handlers for this hidden field.
                    $hfItemNames.val('');

                    var rockTree = $control.find('.treeview').data('rockTree');
                    rockTree.clear();

                    // don't have the X appear on hover. nothing is selected
                    $control.find('.picker-select-none').removeClass('rollover-item').hide();

                    $control.siblings('.js-hide-on-select-none').hide();

                    $spanNames.text(self.options.defaultText);
                    $spanNames.attr('title', $spanNames.text());
                    doPostBack();
                });

                // clicking on the 'select all' btn
                $control.on('click', '.js-select-all', function (e){
                    const $tree = $control.find('.treeview');
                    const rockTree = $tree.data('rockTree');
                    self.selectAll = !self.selectAll;

                    e.preventDefault();
                    e.stopPropagation();

                    let isChildrenLoaded = true;
                    for (const node of rockTree.nodes) {
                        if (node.hasChildren && !node.children) {
                            isChildrenLoaded = false;
                        }
                    }

                    if (!isChildrenLoaded) {
                        rockTree.nodes = [];
                        rockTree.render();
                        let treeOptions = {
                            customDataItems: self.options.customDataItems,
                            enhanceForLongLists: self.options.enhanceForLongLists,
                            multiselect: self.options.allowMultiSelect,
                            categorySelection: self.options.allowCategorySelection,
                            categoryPrefix: self.options.categoryPrefix,
                            restUrl: self.options.restUrl,
                            searchRestUrl: self.options.searchRestUrl,
                            restParams: self.options.restParams + '&loadChildren=true',
                            expandedIds: self.options.expandedIds,
                            expandedCategoryIds: self.options.expandedCategoryIds,
                            showSelectChildren: self.options.showSelectChildren,
                            id: self.options.startingId
                        };
                        $tree.rockTree(treeOptions);
                    } else {
                        self.toggleSelectAll(rockTree);
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
                var $selectedItem = $('#' + this.options.controlId + ' [class^="picker-menu"]').find('.selected').first();
                if ($selectedItem.length && (!this.alreadyScrolledToSelected)) {
                    this.updateScrollbar();
                    this.iScroll.scrollToElement('.selected', '0s');
                    this.alreadyScrolledToSelected = true;
                } else {
                    // initialize/update the scrollbar
                    this.updateScrollbar();
                }
            },
            showActiveMenu: function () {
                var self = this;
                var $control = $('#' + this.options.controlId);

                if (self.isMenuActive()) {
                    $control.find('.picker-label').click();
                }
                self.setActiveMenu(false);
            },
            setActiveMenu: function (value) {
                var $control = $('#' + this.options.controlId);
                if (value === undefined || value === null) {
                    value = true;
                }
                $control.find('.js-picker-showactive-value').val(value.toString());
            },
            isMenuActive: function () {
                var $control = $('#' + this.options.controlId);
                var showPickerActive = $control.find('.js-picker-showactive-value').val();
                var isActive = showPickerActive && showPickerActive === 'true' ? true : false;
                return isActive;
            },
            setViewMode: function (mode) {
                var $control = $('#' + this.options.controlId);
                var $hfViewMode = $control.find('.js-picker-view-mode');

                var clear = mode.toLowerCase() === 'clear';

                if (!clear && clear === false) {
                    $hfViewMode.val(mode);
                }

                if (clear && clear === true) {
                    $hfViewMode.val('');
                }
            },
            getViewMode: function (mode) {
                var $control = $('#' + this.options.controlId);
                var $hfViewMode = $control.find('.js-picker-view-mode');

                return $hfViewMode.val().toLowerCase();
            },
            togglePickerElements: function () {
                var $control = $('#' + this.options.controlId);
                var rockTree = $control.find('.treeview').data('rockTree');

                var hasSelected = rockTree.selectedNodes && rockTree.selectedNodes.length > 0;

                switch (this.getViewMode()) {
                    case 'preview': {
                        $control.find('.picker-treeview').show();
                        $control.find('.picker-preview').hide();
                        $control.find('.js-picker-show-inactive').hide();
                    }
                        break;
                    case 'search': {
                        $control.find('.picker-treeview').show();
                        $control.find('.picker-preview').hide();
                        $control.find('.js-select-all').hide();
                    }
                        break;
                    default: {
                        if (!hasSelected) {
                            $control.find('.picker-preview').hide();
                            $control.find('.picker-treeview').hide();
                        }
                        else {
                            $control.find('.picker-preview').show();
                        }
                    }

                }
            },
            findNodes: function (allNodes, selectNodeIds) {
                if (selectNodeIds) {
                    if ($.isArray(selectNodeIds)) {
                        const filterArray = (nodes, ids) => {
                            const filteredNodes = nodes.filter(node => {
                                return ids.indexOf(node.id) >= 0;
                            });
                            return filteredNodes;
                        };

                        return filterArray(allNodes, selectNodeIds);
                    }
                    else {
                        return allNodes.filter(node => {
                            return selectNodeIds.indexOf(node.id) >= 0;
                        });
                    }
                }
            },
            createSearchControl: function () {
                var self = this;
                var controlId = self.options.controlId;

                var $control = $('#' + controlId);

                var rockTree = $control.find('.treeview').data('rockTree');

                if (self.options.enhanceForLongLists === true) {

                    // A hidden value to store the current search criteria to be read on post-backs
                    var $searchValueField = $control.find('.js-existing-search-value');

                    var $searchControl =
                        $('	<div id="pnlSearch_' + controlId + '" class="input-group input-group-sm js-search-panel mb-2" > ' +
                            '		<input id="tbSearch_' + controlId + '" type="text" placeholder="Quick Find" class="form-control" autocapitalize="off" autocomplete="off" autocorrect="off" spellcheck="false" />' +
                            '		<span class="input-group-btn">' +
                            '			<a id="btnSearch_' + controlId + '" class="btn btn-default btn-sm"><i class="fa fa-search"></i></a>' +
                            '		</span>' +
                            '	</div>');

                    // Get all of the current rendered items so we can revert if user leaves search
                    var $pickerMenu = $control.find('.picker-menu');
                    var $treeView = $control.find('.treeview');
                    var $overview = $control.find('.overview');
                    var $hfItemIds = $control.find('.js-item-id-value');

                    // Added this check to prevent rendering call from duping the element
                    if ($pickerMenu.find('.js-search-panel').length === 0) {
                        // Add the search control after rendering
                        $pickerMenu.prepend($searchControl);
                    }

                    var $searchInputControl = $('#tbSearch_' + controlId);

                    $('#btnSearch_' + controlId).off('click').on('click', function () {

                        var currentSelectedNodeIds = rockTree.selectedNodes.map(function (v) { return v.id; });

                        $hfItemIds = $control.find('.js-item-id-value');

                        var searchKeyword = $searchInputControl.val();

                        if (searchKeyword && searchKeyword.length > 0) {

                            self.setViewMode('search');

                            var searchRestUrl = self.options.searchRestUrl;
                            var restUrlParams = self.options.restParams + '&searchTerm=' + searchKeyword;

                            searchRestUrl += restUrlParams;

                            $.getJSON(searchRestUrl, function (data, status) {

                                if (data && status === 'success') {
                                    $treeView.html('');
                                }
                                else {
                                    $overview.html(treeView);
                                    return;
                                }

                                // Create the search results node object
                                var nodes = [];
                                for (var i = 0; i < data.length; i++) {
                                    var obj = data[i];
                                    var node = {
                                        id: obj.Id,
                                        parentId: obj.ParentId,
                                        glcode: obj.GlCode,
                                        title: obj.Name + (obj.GlCode ? ' (' + obj.GlCode + ')' : ''),
                                        name: obj.Name,
                                        hasChildren: obj.HasChildren,
                                        isActive: obj.IsActive,
                                        path: obj.Path
                                    };

                                    nodes.push(node);
                                }

                                if (nodes) {

                                    var listHtml = '';
                                    nodes.forEach(function (node, idx) {

                                        var disabledCheck = '';
                                        var mutedText = '';
                                        if (!node.isActive || node.isActive === false) {
                                            disabledCheck = ' disabled';
                                            mutedText = ' text-muted';
                                        }

                                        var inputHtml = '<input type="radio" data-id="' + node.id + '" class="checkbox js-opt-search"' + disabledCheck + '>';
                                        var inputType = 'radio';
                                        if (self.options.allowMultiSelect) {
                                            inputHtml = '<input type="checkbox" data-id="' + node.id + '" class="checkbox js-chk-search"' + disabledCheck + '>';
                                            inputType = 'checkbox';
                                        }

                                        if (node.path === '') {
                                            node.path = "Top-Level";
                                        }

                                        listHtml +=

                                            '<div id="divSearchItem-' + node.id + '" class="' + inputType + ' search-item js-search-item">' +
                                            '      <label>' +
                                            inputHtml +
                                            '        <span class="label-text">' +
                                            '              <span class="text-color d-block' + mutedText + '">' + node.title + '</span>' +
                                            '              <span class="text-muted text-sm">' + node.path.replaceAll('^', '<i class="fa fa-chevron-right pl-1 pr-1" aria-hidden="true"></i>') + '</span>' +
                                            '        </span>' +
                                            '     </label>' +
                                            '</div>';
                                    });

                                    // add the results to the panel
                                    $treeView.html(listHtml);

                                    self.updateScrollbar();

                                    $control.find('.js-chk-search').off('change').on('change', function () {
                                        var itemIds = [];
                                        var $allChecked = $control.find('.js-chk-search:checked');
                                        var checkedVals = $allChecked.map(function () {
                                            return $(this).attr('data-id');
                                        }).get();

                                        if (checkedVals && checkedVals.length > 0) {
                                            $control.find('.picker-treeview').show();
                                            var checkedNodes = self.findNodes(nodes, checkedVals);

                                            if (checkedNodes && checkedNodes.length) {
                                                checkedNodes.forEach(function (n) {
                                                    if (n.length === 0) { return; }
                                                    if (!itemIds.find(function (i) { return n.id === i; })) {
                                                        itemIds.push(n.id);
                                                    }

                                                });
                                            }
                                        }

                                        // We need to reselect any selection that were set in the tree but not part of the search results
                                        if (currentSelectedNodeIds && currentSelectedNodeIds.length > 0) {
                                            nodes.forEach(function (node) {
                                                currentSelectedNodeIds = currentSelectedNodeIds.filter(function (current) { return current !== node.id });
                                            });

                                            if (currentSelectedNodeIds && currentSelectedNodeIds.length > 0) {
                                                itemIds.push(currentSelectedNodeIds);
                                            }
                                        }

                                        if (itemIds && itemIds.length) {
                                            // set the selected items on the server control to handle on postback
                                            $hfItemIds.val(itemIds.join(','));
                                        }
                                    });

                                    // Handle multi item check selection
                                    $control.find('.js-search-item').off('click').on('click', function (e) {

                                        var $chkBox = $(this).find('.js-chk-search').first();
                                        if ($chkBox.length > 0) {
                                            if (!$chkBox.prop('checked')) {
                                                $chkBox.prop('checked', true);
                                                $control.find('.js-chk-search').trigger('change');
                                            }
                                            else {
                                                $chkBox.prop('checked', false);
                                            }
                                        }
                                    });


                                    // Handle single item radio selection
                                    $control.find('.js-opt-search').off('change').on('change', function () {
                                        var thisNodeId = $(this).attr('data-id');
                                        var itemIds = [];
                                        //prevent multi select
                                        $control.find('.js-opt-search:not([data-id=' + thisNodeId + '])').prop('checked', false);

                                        var $allChecked = $control.find('.js-opt-search:checked');
                                        var checkedVals = $allChecked.map(function () {
                                            return $(this).attr('data-id');
                                        }).get();

                                        if (checkedVals && checkedVals.length > 0) {
                                            var checkedNodes = self.findNodes(nodes, checkedVals);

                                            if (checkedNodes && checkedNodes.length) {
                                                checkedNodes.forEach(function (n) {
                                                    if (n.length === 0) { return; }
                                                    if (!itemIds.find(function (i) { return n.id === i; })) {
                                                        itemIds.push(n.id);
                                                    }
                                                });
                                            }
                                        }

                                        if (itemIds && itemIds.length) {
                                            // set the selected items on the server control to handle on postback
                                            $hfItemIds.val(itemIds.join(','));
                                        }
                                    });
                                }

                                //select current selected nodes in search
                                if (rockTree.selectedNodes && rockTree.selectedNodes.length) {
                                    rockTree.selectedNodes.forEach(function (selectedNode) {
                                        if (self.options.allowMultiSelect && self.options.allowMultiSelect === true) {
                                            $control.find('[data-id=' + selectedNode.id + '].js-chk-search').prop('checked', true);
                                            var checkedNode = selectedNode;
                                            checkedNode.id = selectedNode.id;
                                            $control.find('[data-id=' + selectedNode.id + '].js-chk-search').trigger('change');
                                        }
                                        else {
                                            $control.find('[data-id=' + selectedNode.id + '].js-opt-search').prop('checked', true);
                                            var enabledNode = selectedNode;
                                            enabledNode.id = selectedNode.id;
                                            $control.find('[data-id=' + selectedNode.id + '].js-opt-search').trigger('change');
                                        }
                                    });
                                }
                            });
                        }

                    });

                    // If we have an existing search value on postback
                    if ($searchValueField.length > 0 && $searchValueField.val().length > 0) {
                        $searchInputControl.val($searchValueField.val());
                        $('#btnSearch_' + controlId).click();
                    }

                    // Handle the input searching
                    $searchInputControl.keyup(function (keyEvent) {
                        keyEvent.preventDefault();

                        var searchKeyword = $searchInputControl.val();

                        if (!searchKeyword || searchKeyword.length === 0) {
                            $control.find('.picker-treeview').trigger('click');
                        }

                        self.togglePickerElements();
                    }).keydown(function (keyEvent) {

                        var searchKeyword = $searchInputControl.val();
                        $searchValueField.val(searchKeyword);

                        if (keyEvent.which === 13) {
                            keyEvent.preventDefault();
                            $('#btnSearch_' + controlId).click();
                        }
                    });
                }
            },
            toggleSelectAll: function (rockTree) {

                const $itemNameNodes = rockTree.$el.find('.rocktree-name');

                const setNodeOpenState = function (node, isOpen) {
                    if (node.hasChildren && node.children) {
                        node.isOpen = isOpen;
                        node.children.forEach(childNode => setNodeOpenState(childNode, isOpen));
                    }
                }

                if (this.selectAll) {
                    // mark them all as unselected (just in case some are selected already), then click them to select them
                    $itemNameNodes.removeClass('selected');
                    $itemNameNodes.trigger('click');
                    rockTree.nodes.forEach(node => setNodeOpenState(node, true));
                } else {
                    // if all were already selected, toggle them to unselected
                    rockTree.setSelected([]);
                    const $control = $('#' + this.options.controlId);
                    const $hfItemIds = $control.find('.js-item-id-value');
                    $hfItemIds.val('0');
                    $itemNameNodes.removeClass('selected');
                    rockTree.nodes.forEach(node => setNodeOpenState(node, false));
                }

                rockTree.render();
            },
            selectAll: false
        }

        // jquery function to ensure HTML is state remains the same each time it is executed
        $.fn.outerHTML = function (s) {
            return (s)
                ? this.before(s).remove()
                : $("<p>").append(this.eq(0).clone()).html();
        }

        exports = {
            defaults: {
                id: 0,
                controlId: null,
                restUrl: null,
                searchRestUrl: null,
                restParams: null,
                allowCategorySelection: false,
                categoryPrefix: '',
                allowMultiSelect: false,
                defaultText: '',
                selectedIds: null,
                expandedIds: null,
                expandedCategoryIds: null,
                showSelectChildren: false,
                enhanceForLongLists: false,
                customDataItems: []
            },
            controls: {},
            initialize: function (options) {
                var settings,
                    accountPicker;

                if (!options.controlId) {
                    throw 'controlId must be set';
                }

                if (!options.restUrl) {
                    throw 'restUrl must be set';
                }

                if (options.enhanceForLongLists === true && !options.searchRestUrl) {
                    throw 'searchRestUrl must be set';
                }

                settings = $.extend({}, exports.defaults, options);

                if (!settings.defaultText) {
                    settings.defaultText = exports.defaults.defaultText;
                }

                accountPicker = new AccountPicker(settings);
                exports.controls[settings.controlId] = accountPicker;
                accountPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));
