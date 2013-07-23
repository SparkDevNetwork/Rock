<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfGroupTypes" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfLimitToSecurityRoleGroups" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedGroupId" runat="server" ClientIDMode="Static" />

        <div class="treeview treeview-group">
            <div class="treeview-actions pull-right">
                <asp:LinkButton ID="lbAddGroup" runat="server" CssClass="add btn btn-mini" ToolTip="Add Group" CausesValidation="false" OnClick="lbAddGroup_Click">
                        <i class="icon-plus-sign"></i> Add
                </asp:LinkButton>
            </div>

            <div id="treeview-content">
            </div>
            <script>
                function onSelect(e) {
                    var groupId = this.dataItem(e.node).id;
                    showGroupDetails(groupId);
                }

                function showGroupDetails(groupId) {
                    var groupSearch = '?groupId=' + groupId;
                    if (window.location.search.indexOf(groupSearch) === -1) {
                        $('#hfSelectedGroupId').val(groupId);
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
                    // select the item specified in the page param in the treeview if there isn't one currently selected
                    var treeViewData = $('#treeview-content').data("kendoTreeView");
                    var selectedNode = treeViewData.select();
                    var nodeData = this.dataItem(selectedNode);
                    if (!nodeData) {
                        var initialGroupId = $('#hfInitialGroupId').val();
                        var initialGroupParentIds = $('#hfInitialGroupParentIds').val();
                        var initialGroupItem = findChildItemInTree(treeViewData, initialGroupId, initialGroupParentIds);
                        if (initialGroupId) {
                            if (initialGroupItem) {
                                var firstItem = treeViewData.findByUid(initialGroupItem.uid);
                                var firstDataItem = this.dataItem(firstItem);
                                if (firstDataItem) {
                                    treeViewData.select(firstItem);
                                    showGroupDetails(firstDataItem.id);
                                }
                            }
                        }
                    }
                }

                var restUrl = "<%=ResolveUrl( "~/api/groups/getchildren/" ) %>";
                var rootGroupId = $('#hfRootGroupId').val();
                var limitToSecurityRoleGroups = $('#hfLimitToSecurityRoleGroups').val();
                var groupTypes = $('#hfGroupTypes').val();

                var groupList = new kendo.data.HierarchicalDataSource({
                    transport: {
                        read: {
                            url: function (options) {
                                var requestUrl = restUrl + (options.id || 0) + '/' + (rootGroupId || 0) + '/' + (limitToSecurityRoleGroups || false) + '/' + (groupTypes || '0');
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

                $('#treeview-content').kendoTreeView({
                    template: "<i class='#= item.IconCssClass #'></i> #= item.Name #",
                    dataSource: groupList,
                    dataTextField: 'Name',
                    dataImageUrlField: 'IconSmallUrl',
                    select: onSelect,
                    dataBound: onDataBound
                });
            </script>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
