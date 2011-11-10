<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Security.ascx.cs" Inherits="RockWeb.Blocks.Cms.Security" %>
<script type="text/javascript">

    Sys.Application.add_load(function () {
        $('ol[id$=cblRoleActionList]').hide();
        $('a.show-action-list').click(function () {
            $('ol[id$=cblRoleActionList]').toggle('fast');
            return false;
        });
    });

</script>
<asp:UpdatePanel id="upPanel" runat="server" class="admin-dialog">
<ContentTemplate>
    
    <div class='dialog-title'><asp:Literal ID="lTitle" runat="server"/></div>
    <asp:Label ID="lblAction" runat="server" Text="Action" AssociatedControlID="ddlAction"></asp:Label> 
    <asp:DropDownList ID="ddlAction" runat="server" AutoPostBack="true" 
        onselectedindexchanged="ddlAction_SelectedIndexChanged"></asp:DropDownList>

    <div class="dialog-note"><asp:Literal ID="lActionNote" runat="server"/></div>

    <Rock:Grid ID="rGrid" runat="server" onrowdatabound="rGrid_RowDataBound">
        <Columns>
            <Rock:ReorderField />
            <asp:TemplateField>
                <HeaderTemplate>Allow/Deny</HeaderTemplate>
                <HeaderStyle HorizontalAlign="Left" />
                <ItemStyle Wrap="false" HorizontalAlign="Left" />
                <ItemTemplate>
                    <asp:RadioButtonList ID="rblAllowDeny" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" 
                        OnSelectedIndexChanged="rblAllowDeny_SelectedIndexChanged" AutoPostBack="true">
                        <asp:ListItem Value="A" Text="Allow"></asp:ListItem>
                        <asp:ListItem Value="D" Text="Deny"></asp:ListItem>
                    </asp:RadioButtonList>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="DisplayName" HeaderText="Name" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
            <Rock:DeleteField OnClick="rGrid_Delete" />
        </Columns>
    </Rock:Grid>

    <asp:LinkButton ID="lbShowRole" runat="server" Text="Add Role" 
        onclick="lbShowRole_Click"></asp:LinkButton>
    <asp:LinkButton ID="lbShowUser" runat="server" Text="Add User" 
        onclick="lbShowUser_Click"></asp:LinkButton>
    <asp:LinkButton ID="lbAddAllUsers" runat="server" Text="Add All Users" 
        onclick="lbAddAllUsers_Click"></asp:LinkButton>

    <asp:Panel ID="pnlAddRole" runat="server" Visible="false">
        <asp:Label ID="lblRoles" runat="server" Text="Role" AssociatedControlID="ddlRoles"></asp:Label> 
        <asp:DropDownList ID="ddlRoles" runat="server" AutoPostBack="true" onselectedindexchanged="ddlRoles_SelectedIndexChanged"></asp:DropDownList>
        <a class="show-action-list" href="#">...</a>
        <asp:CheckBoxList ID="cblRoleActionList" runat="server" RepeatLayout="OrderedList"></asp:CheckBoxList>
        <asp:LinkButton ID="lbAddRole" runat="server" Text="Add" onclick="lbAddRole_Click"></asp:LinkButton>
    </asp:Panel>

    <asp:Panel ID="pnlAddUser" runat="server" Visible="false">
        <asp:Label ID="lblUser" runat="server" Text="User" AssociatedControlID="tbUser"></asp:Label> 
        <asp:TextBox ID="tbUser" runat="server"></asp:TextBox>
        <asp:LinkButton ID="lbUserSearch" runat="server" Text="Search" onclick="lbUserSearch_Click"></asp:LinkButton>
        <asp:CheckBoxList ID="cbUsers" runat="server"></asp:CheckBoxList>
        <asp:LinkButton ID="lbAddUser" runat="server" Text="Add" onclick="lbAddUser_Click"></asp:LinkButton>
    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

