<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfLimitToSecurityRoleGroups" runat="server" ClientIDMode="Static" />
        <div class="treeview-back">
            <h3>
                <asp:Literal ID="ltlTreeViewTitle" runat="server" /></h3>
            <div id="treeviewGroups" class="groupTreeview">
            </div>
        </div>
        <div class="console"></div>
        <script>
            function onSelect(e) {
                var groupId = this.dataItem(e.node).id;
                showGroupDetails(groupId);
            }

            function showGroupDetails(groupId) {
                __doPostBack('<%= upGroupType.ClientID %>', 'groupId=' + groupId);
            }

            function onDataBound(e) {
                // automatically select the first item in the treeview if there isn't one currently selected
                var treeViewData = $('.groupTreeview').data("kendoTreeView");
                var selectedNode = treeViewData.select();
                var nodeData = this.dataItem(selectedNode);
                if (!nodeData) {
                    var firstItem = treeViewData.root[0].firstChild;
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
