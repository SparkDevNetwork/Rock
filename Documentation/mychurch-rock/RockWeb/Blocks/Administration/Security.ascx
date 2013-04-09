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
        $('ul[id$=cblRoleActionList]').hide();
        $('a.show-action-list').click(function () {
            $('ul[id$=cblRoleActionList]').toggle('fast');
            return false;
        });
    });

</script>
<asp:UpdatePanel id="upPanel" runat="server">
<ContentTemplate>
 
    <ul class="nav nav-pills">
        <asp:Repeater ID="rptActions" runat="server">
            <ItemTemplate>
                <li class='<%# GetTabClass(Container.DataItem) %>'><asp:LinkButton ID="lbAction" runat="server" Text='<%# Container.DataItem %>' OnClick="lbAction_Click"></asp:LinkButton> </li>
            </ItemTemplate>
        </asp:Repeater>
    </ul>

    <div class="tab-content">

        <asp:PlaceHolder ID="phList" runat="server">
            <div class="security-rights">
                <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" RowItemText="role/user">
                    <Columns>
                        <Rock:ReorderField />
                        <asp:BoundField DataField="DisplayName" HeaderText="Name" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                        <asp:TemplateField>
                            <HeaderTemplate>Allow or Deny</HeaderTemplate>
                            <HeaderStyle HorizontalAlign="Left" />
                            <ItemStyle Wrap="false" HorizontalAlign="Left" />
                            <ItemTemplate>
                                <asp:RadioButtonList ID="rblAllowDeny" runat="server" RepeatDirection="Horizontal" CssClass="inputs-list"
                                    OnSelectedIndexChanged="rblAllowDeny_SelectedIndexChanged" AutoPostBack="true">
                                    <asp:ListItem Value="A" Text="Allow"></asp:ListItem>
                                    <asp:ListItem Value="D" Text="Deny"></asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <Rock:DeleteField OnClick="rGrid_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

            <asp:panel id="pnlActions" runat="server" CssClass="actions">
                <asp:LinkButton ID="lbShowRole" runat="server" Text="Add Role" CssClass="btn btn-primary" onclick="lbShowRole_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbShowUser" runat="server" Text="Add User" CssClass="btn btn-primary" onclick="lbShowUser_Click"></asp:LinkButton>
            </asp:panel>

            <div class="security-inherited">
                <Rock:Grid ID="rGridParentRules" runat="server" AllowPaging="false" RowItemText="Inherited Security Rule">
                    <Columns>
                        <asp:BoundField DataField="DisplayName" HeaderText="Inherited Security" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                        <asp:TemplateField>
                            <HeaderTemplate>Allow or Deny</HeaderTemplate>
                            <HeaderStyle HorizontalAlign="Left" />
                            <ItemStyle Wrap="false" HorizontalAlign="Left" />
                            <ItemTemplate>
                                <%# Eval("AllowOrDeny").ToString() == "A" ? "Allow" : "Deny" %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="EntityTitle" HeaderText="From" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:PlaceHolder>

        <asp:Panel ID="pnlAddRole" runat="server" Visible="false" CssClass="add-role">
        
            <fieldset>
                <legend>Select Role to Add</legend>
                <Rock:LabeledDropDownList ID="ddlRoles" runat="server" LabelText="Role" AutoPostBack="true" onselectedindexchanged="ddlRoles_SelectedIndexChanged" />
                <dl>
                    <dt></dt>
                    <dd>
                        <a class="show-action-list" href="#">...</a>
                        <asp:CheckBoxList ID="cblRoleActionList" runat="server" RepeatDirection="Horizontal" CssClass="inputs-list" ></asp:CheckBoxList>
                    </dd>
                </dl>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbAddRole" runat="server" Text="Add" CssClass="btn btn-primary" onclick="lbAddRole_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddRole" runat="server" Text="Cancel" CssClass="btn" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAddUser" runat="server" Visible="false" CssClass="add-user">
        
            <fieldset>
                <legend>Select User to Add</legend>
                <Rock:LabeledTextBox ID="tbUser" runat="server" LabelText="User" />
                <dl>
                    <dt></dt>
                    <dd>
                        <asp:LinkButton ID="lbUserSearch" runat="server" Text="Search" onclick="lbUserSearch_Click" CssClass="btn x-small"></asp:LinkButton>
                        <asp:CheckBoxList ID="cbUsers" runat="server" CssClass="inputs-list" RepeatDirection="Vertical"></asp:CheckBoxList>
                    </dd>
                </dl>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbAddUser" runat="server" Text="Add" CssClass="btn btn-primary" OnClientClick="showAddUser(false);" onclick="lbAddUser_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddUser" runat="server" Text="Cancel" CssClass="btn" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

    </div>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

