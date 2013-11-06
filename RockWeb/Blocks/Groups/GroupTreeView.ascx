<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupTreeView" %>

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
            <script type="text/javascript">
                $(function () {
                    var $selectedId = $('#hfSelectedGroupId'),
                        $expandedIds = $('#hfInitialGroupParentIds');

                    $('#treeview-content')
                        .on('rockTree:selected', function (e, id) {
                            var groupSearch = '?groupId=' + id;
                            if (window.location.search.indexOf(groupSearch) === -1) {
                                $('#hfSelectedGroupId').val(id); // Todo: Is this necessary, since we're redirecting on the next line?
                                window.location.search = groupSearch;
                            }
                        })
                        .rockTree({
                            restUrl: '<%=ResolveUrl( "~/api/groups/getchildren/" ) %>',
                            restParams: '/' + ($('#hfRootGroupId').val() || 0) + '/' + ($('#hfLimitToSecurityRoleGroups').val() || false) + '/' + ($('#hfGroupTypes').val() || 0),
                            multiSelect: false,
                            selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                            expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                        });
                });
            </script>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
