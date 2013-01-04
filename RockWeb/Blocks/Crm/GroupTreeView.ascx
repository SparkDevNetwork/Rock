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

                $('.console').text("Selected: " + this.text(e.node) + ", " + groupId);

                __doPostBack('<%= upGroupType.ClientID %>', 'groupId=' + groupId);
            }

            function onNavigate(e) {
                $('.console').text("Navigate " + this.text(e.node));
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
                dataSource: groupList,
                dataTextField: 'Name',
                select: onSelect,
                navigate: onNavigate
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
