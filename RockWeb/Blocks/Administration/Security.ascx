<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Security.ascx.cs" Inherits="RockWeb.Blocks.Administration.Security" %>
<script type="text/javascript">

    function showAddRole(show) {
        if (show) {
            $('#add-actions').hide('fast');
            $('#add-role').show('fast');
        }
        else {
            $('#add-role').hide('fast');
            $('#add-actions').show('fast');
        }
    }

    function showAddUser(show) {
        if (show) {
            $('#add-actions').hide('fast');
            $('#add-user').show('fast');
        }
        else {
            $('#add-user').hide('fast');
            $('#add-actions').show('fast');
        }
    }

    Sys.Application.add_load(function () {
        $('ol[id$=cblRoleActionList]').hide();
        $('a.show-action-list').click(function () {
            $('ol[id$=cblRoleActionList]').toggle('fast');
            return false;
        });
    });

</script>
<asp:UpdatePanel id="upPanel" runat="server">
<ContentTemplate>
 
    <ul class="tabs">
        <asp:Repeater ID="rptActions" runat="server">
            <ItemTemplate>
                <li class='<%# GetTabClass(Container.DataItem) %>'><asp:LinkButton ID="lbAction" runat="server" Text='<%# Container.DataItem %>' OnClick="lbAction_Click"></asp:LinkButton> </li>
            </ItemTemplate>
        </asp:Repeater>
    </ul>

    <div class="tabContent">

        <p><asp:Literal ID="lActionNote" runat="server"></asp:Literal></p>

        <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" onrowdatabound="rGrid_RowDataBound">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="DisplayName" HeaderText="Name" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                <asp:TemplateField>
                    <HeaderTemplate>Allow or Deny</HeaderTemplate>
                    <HeaderStyle HorizontalAlign="Left" />
                    <ItemStyle Wrap="false" HorizontalAlign="Left" />
                    <ItemTemplate>
                        <asp:RadioButtonList ID="rblAllowDeny" runat="server" RepeatLayout="UnorderedList" 
                            OnSelectedIndexChanged="rblAllowDeny_SelectedIndexChanged" AutoPostBack="true">
                            <asp:ListItem Value="A" Text="Allow"></asp:ListItem>
                            <asp:ListItem Value="D" Text="Deny"></asp:ListItem>
                        </asp:RadioButtonList>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

        <asp:panel id="pnlActions" runat="server" CssClass="actions">
            <asp:LinkButton ID="lbShowRole" runat="server" Text="Add Role" CssClass="btn primary" onclick="lbShowRole_Click"></asp:LinkButton>
            <asp:LinkButton ID="lbShowUser" runat="server" Text="Add User" CssClass="btn primary" onclick="lbShowUser_Click"></asp:LinkButton>
            <asp:LinkButton ID="lbAddAllUsers" runat="server" Text="Add All Users" CssClass="btn primary" onclick="lbAddAllUsers_Click"></asp:LinkButton>
        </asp:panel>

        <asp:Panel ID="pnlAddRole" runat="server" Visible="false">
        
            <fieldset>
                <legend>Add Role</legend>
                <Rock:LabeledDropDownList ID="ddlRoles" runat="server" LabelText="Role" AutoPostBack="true" onselectedindexchanged="ddlRoles_SelectedIndexChanged" />
                <dl>
                    <dt></dt>
                    <dd>
                        <a class="show-action-list" href="#">...</a>
                        <asp:CheckBoxList ID="cblRoleActionList" runat="server" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                    </dd>
                </dl>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbAddRole" runat="server" Text="Add" CssClass="btn primary" onclick="lbAddRole_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddRole" runat="server" Text="Cancel" CssClass="btn secondary" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAddUser" runat="server" Visible="false">
        
            <fieldset>
                <legend>Add User</legend>
                <Rock:LabeledTextBox ID="tbUser" runat="server" LabelText="User" />
                <dl>
                    <dt></dt>
                    <dd>
                        <asp:LinkButton ID="lbUserSearch" runat="server" Text="Search" onclick="lbUserSearch_Click"></asp:LinkButton>
                        <asp:CheckBoxList ID="cbUsers" runat="server"></asp:CheckBoxList>
                    </dd>
                </dl>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbAddUser" runat="server" Text="Add" CssClass="btn primary" OnClientClick="showAddUser(false);" onclick="lbAddUser_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddUser" runat="server" Text="Cancel" CssClass="btn secondary" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

    </div>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

