<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfLimitToSecurityRoleGroups" runat="server" ClientIDMode="Static" />
        <div class="treeview-back">
            <h3>
                <asp:Literal ID="ltlTreeViewTitle" runat="server" /></h3>
            <div id="treeviewGroups" class="groupTreeview">
            </div>
        </div>
        <script>
            function onSelect(e) {
                var groupId = this.dataItem(e.node).id;
                showGroupDetails(groupId);
            }

            function showGroupDetails(groupId) {
                var groupSearch = '?groupId=' + groupId;
                if (window.location.search != groupSearch) {
                    window.location.search = groupSearch;
                }
            }

            function findChildItemInTree(treeViewData, groupId, groupParentIds) {
                if (groupParentIds != '') {
                    var groupParentList = groupParentIds.split(",");
                    for (var i = 0; i < groupParentList.length; i++) {
                        var parentGroupId = groupParentList[i];
                        var parentItem = treeViewData.dataSource.get(parentGroupId);
                        var parentNodeItem = treeViewData.findByUid(parentItem.uid);
                        if (!parentItem.expanded && parentItem.hasChildren) {
                            // if not yet expand, expand and return null (which will fetch more data and fire the databound event)
                            treeViewData.expand(parentNodeItem);
                            return null;
                        }
                    }
                }

                var initialGroupItem = treeViewData.dataSource.get(groupId);

                return initialGroupItem;
            }

            function onDataBound(e) {
                // automatically select the first item in the treeview if there isn't one currently selected
                var treeViewData = $('.groupTreeview').data("kendoTreeView");
                var selectedNode = treeViewData.select();
                var nodeData = this.dataItem(selectedNode);
                if (!nodeData) {
                    var initialGroupId = $('#hfInitialGroupId').val();
                    var initialGroupParentIds = $('#hfInitialGroupParentIds').val();
                    var initialGroupItem = findChildItemInTree(treeViewData, initialGroupId, initialGroupParentIds);
                    var firstItem = null;
                    if (initialGroupId) {
                        if (initialGroupItem) {
                            firstItem = treeViewData.findByUid(initialGroupItem.uid);
                        }
                    }
                    else {
                        firstItem = treeViewData.root[0].firstChild;
                    }
                    var firstDataItem = this.dataItem(firstItem);
                    if (firstDataItem) {
                        treeViewData.select(firstItem);
                        showGroupDetails(firstDataItem.id);
                    }
                }
            }

            var restUrl = "<%=ResolveUrl( "~/api/groups/getchildren/" ) %>";
            var rootGroupId = $('#hfRootGroupId').val();
            var limitToSecurityRoleGroups = $('#hfLimitToSecurityRoleGroups').val();

            var groupList = new kendo.data.HierarchicalDataSource({
                transport: {
                    read: {
                        url: function (options) {
                            var requestUrl = restUrl + (options.id || 0) + '/' + (rootGroupId || 0) + '/' + (limitToSecurityRoleGroups || false)
                            return requestUrl;
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

            $('.groupTreeview').kendoTreeView({
                template: "<i class='#= item.GroupTypeIconCssClass #'></i> #= item.Name #",
                dataSource: groupList,
                dataTextField: 'Name',
                dataImageUrlField: 'GroupTypeIconSmallUrl',
                select: onSelect,
                dataBound: onDataBound
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
