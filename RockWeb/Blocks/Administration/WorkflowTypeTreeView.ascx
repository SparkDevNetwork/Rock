<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeTreeView.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowTypeTreeView" %>

<asp:UpdatePanel ID="upWorkflowType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialEntityTypeName" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialItemId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" ClientIDMode="Static" />
        <div class="treeview-back">
            <h3>
                <asp:Literal ID="ltlTreeViewTitle" runat="server" /></h3>
            <div id="treeviewWorkflowType" class="tree-view tree-view-workflowtype">
            </div>
        </div>
        <script>
            function onSelect(e) {
                var dataItem = this.dataItem(e.node);
                showItemDetails(dataItem);
            }

            function showItemDetails(dataItem) {
                var itemSearch = '?' + dataItem.EntityTypeName + 'Id=' + dataItem.Id
                if (window.location.search != itemSearch) {
                    window.location.search = itemSearch;
                }
            }

            function findItemInData(data, entityTypeName, itemId) {
                for (var i = 0; i < data.length; i++) {
                    dataItem = data[i];
                    if (dataItem.id == itemId && dataItem.EntityTypeName.toLowerCase() == entityTypeName.toLowerCase()) {
                        return dataItem;
                    }
                    if (dataItem.hasChildren) {
                        var childrenData = dataItem.children.data();
                        var childItemData = findItemInData(childrenData, entityTypeName, itemId);
                        if (childItemData) {
                            return childItemData;
                        }
                    }
                }
            }

            function findChildItemInTree(treeViewData, entityTypeName, itemId, itemParentIds) {

                if (itemParentIds != '') {
                    var itemParentList = itemParentIds.split(",");
                    for (var i = 0; i < itemParentList.length; i++) {
                        var parentItemId = itemParentList[i];
                        var parentItem = treeViewData.dataSource.get(parentItemId);
                        var parentNodeItem = treeViewData.findByUid(parentItem.uid);
                        if (!parentItem.expanded && parentItem.hasChildren) {
                            // if not yet expand, expand and return null (which will fetch more data and fire the databound event)
                            treeViewData.expand(parentNodeItem);
                            return null;
                        }
                    }
                }

                if (entityTypeName == '') {
                    return null;
                }

                var data = treeViewData.dataSource.data();
                var dataItem = findItemInData(data, entityTypeName, itemId);
                return dataItem;

                return null;
            }


            function onDataBound(e) {
                // select the item specified in the page param in the treeview if there isn't one currently selected
                var treeViewData = $('#treeviewWorkflowType').data("kendoTreeView");
                var selectedNode = treeViewData.select();
                var nodeData = this.dataItem(selectedNode);
                if (!nodeData) {
                    var initialEntityTypeName = $('#hfInitialEntityTypeName').val();
                    var initialItemId = $('#hfInitialItemId').val();
                    var initialCategoryParentIds = $('#hfInitialCategoryParentIds').val();
                    var initialItem = findChildItemInTree(treeViewData, initialEntityTypeName, initialItemId, initialCategoryParentIds);
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
                            var requestUrl = restUrl + (options.id || 0) + '/' + 'WorkflowType'
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
                        entityTypeName: 'EntityTypeName'
                    }
                }
            });

            $('#treeviewWorkflowType').kendoTreeView({
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
