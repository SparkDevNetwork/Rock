<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryTreeView.ascx.cs" Inherits="RockWeb.Blocks.Utility.CategoryTreeView" %>

<asp:UpdatePanel ID="upCategoryTree" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialEntityIsCategory" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialItemId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedCategoryId" runat="server" ClientIDMode="Static" />
        <span id="add-buttons" class="pull-right" style="display: none">
            <asp:LinkButton ID="lbAddCategory" runat="server" CssClass="add btn" ToolTip="Add Category" CausesValidation="false" OnClick="lbAddCategory_Click"><i class="icon-plus-sign"></i></asp:LinkButton>
            <asp:LinkButton ID="lbAddItem" runat="server" CssClass="add btn" ToolTip="Add Group" CausesValidation="false" OnClick="lbAddItem_Click"><i class="icon-plus-sign"></i></asp:LinkButton>
        </span>
        <div id="treeviewCategories" class="tree-view tree-view-categories"></div>
        <script>
            function onSelect(e) {
                var dataItem = this.dataItem(e.node);
                showItemDetails(dataItem);
            }

            function showItemDetails(dataItem) {
                var itemSearch = '?' + (dataItem.IsCategory ? 'CategoryId' : '<%=PageParameterName%>') + '=' + dataItem.Id

                if (dataItem.IsCategory) {
                    $('#hfSelectedCategoryId').val(dataItem.Id);
                    $('#add-buttons').show();
                }
                else {
                    $('#hfSelectedCategoryId').val('');
                    $('#add-buttons').hide();
                }

                if (window.location.search != itemSearch) {
                    window.location.search = itemSearch;
                }
            }

            function findItemInData(data, isCategory, itemId) {
                for (var i = 0; i < data.length; i++) {
                    dataItem = data[i];
                    if (dataItem.id == itemId && (dataItem.IsCategory == isCategory)) {
                        return dataItem;
                    }
                    if (dataItem.hasChildren) {
                        var childrenData = dataItem.children.data();
                        var childItemData = findItemInData(childrenData, isCategory, itemId);
                        if (childItemData) {
                            return childItemData;
                        }
                    }
                }
            }

            function findChildItemInTree(treeViewData, isCategory, itemId, itemParentIds) {

                if (itemParentIds != '') {
                    var itemParentList = itemParentIds.split(",");
                    for (var i = 0; i < itemParentList.length; i++) {
                        var parentItemId = itemParentList[i];
                        var parentItem = findItemInData(treeViewData.dataSource.data(), true, parentItemId);
                        var parentNodeItem = treeViewData.findByUid(parentItem.uid);
                        if (!parentItem.expanded && parentItem.hasChildren) {
                            // if not yet expand, expand and return null (which will fetch more data and fire the databound event)
                            treeViewData.expand(parentNodeItem);
                            return null;
                        }
                    }
                }

                var data = treeViewData.dataSource.data();
                var dataItem = findItemInData(data, isCategory, itemId);
                return dataItem;

                return null;
            }


            function onDataBound(e) {
                // select the item specified in the page param in the treeview if there isn't one currently selected
                var treeViewData = $('#treeviewCategories').data("kendoTreeView");
                var selectedNode = treeViewData.select();
                var nodeData = this.dataItem(selectedNode);
                if (!nodeData) {
                    var initialEntityIsCategory = ($('#hfInitialEntityIsCategory').val() === 'True');
                    var initialItemId = $('#hfInitialItemId').val();
                    var initialCategoryParentIds = $('#hfInitialCategoryParentIds').val();
                    var initialItem = findChildItemInTree(treeViewData, initialEntityIsCategory, initialItemId, initialCategoryParentIds);
                    if (initialItemId) {
                        if (initialItem) {
                            var firstItem = treeViewData.findByUid(initialItem.uid);
                            var firstDataItem = this.dataItem(firstItem);
                            if (firstDataItem) {
                                treeViewData.select(firstItem);
                                showItemDetails(firstDataItem);
                            }
                        }
                    }
                }
            }

            var restUrl = "<%=ResolveUrl( "~/api/categories/getchildren/" ) %>";

            var dataList = new kendo.data.HierarchicalDataSource({
                transport: {
                    read: {
                        url: function (options) {
                            var requestUrl = restUrl + (options.id || 0) + '/' + '<%=EntityTypeName%>' + '/true';
                            return requestUrl;
                        },
                        error: function (xhr, status, error) {
                            {
                                alert(status + ' [' + error + ']: ' + xhr.responseText);
                            }
                        }
                    }
                },
                schema: {
                    model: {
                        id: 'Id',
                        hasChildren: 'HasChildren',
                        isCategory: 'IsCategory'
                    }
                }
            });

                $('#treeviewCategories').kendoTreeView({
                    template: "<i class='#= item.IconCssClass #'></i> #= item.Name #",
                    dataSource: dataList,
                    dataTextField: 'Name',
                    dataImageUrlField: 'IconSmallUrl',
                    select: onSelect,
                    dataBound: onDataBound
                });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
