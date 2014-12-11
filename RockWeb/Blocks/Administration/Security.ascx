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
    <div class="nav navbar nav-pagelist">
        <ul class="nav nav-pills">
            <asp:Repeater ID="rptActions" runat="server">
                <ItemTemplate>
                    <li class='<%# GetTabClass( Eval( "Key" ) ) %>'><asp:LinkButton ID="lbAction" runat="server" Text='<%# Eval( "Key" ) %>' OnClick="lbAction_Click"></asp:LinkButton> </li>
                </ItemTemplate>
            </asp:Repeater>
            <li class="pull-right pill-help"><a data-toggle="collapse" href="#security-details" class=""><i class="fa fa-question-circle"></i></a></li>
        </ul>
    </div>

    <div class="tab-content">

        <asp:PlaceHolder ID="phList" runat="server">
            <div id="security-details" class="security-action-description alert alert-info collapse"><asp:Literal ID="lActionDescription" runat="server" /></div>
            <div class="security-rights">
                <h4>Item Permissions</h4>
                
                <div class="grid">
                    <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" RowItemText="role/user">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="DisplayName" HeaderText="Role / User" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                            <Rock:RockTemplateField>
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
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <asp:panel id="pnlActions" runat="server" CssClass="actions">
                <asp:LinkButton ID="lbShowRole" runat="server" Text="Add Role" CssClass="btn btn-primary" onclick="lbShowRole_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbShowUser" runat="server" Text="Add User" CssClass="btn btn-primary" onclick="lbShowUser_Click"></asp:LinkButton>
            </asp:panel>

            <div class="security-inherited">
                <h4>Inherited Permissions</h4>
                
                <div class="grid">
                    <Rock:Grid ID="rGridParentRules" runat="server" AllowPaging="false" RowItemText="Inherited Security Rule">
                        <Columns>
                            <Rock:RockBoundField DataField="DisplayName" HeaderText="Role / User" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                            <Rock:RockTemplateField>
                                <HeaderTemplate>Allow or Deny</HeaderTemplate>
                                <HeaderStyle HorizontalAlign="Left" />
                                <ItemStyle Wrap="false" HorizontalAlign="Left" />
                                <ItemTemplate>
                                    <%# Eval("AllowOrDeny").ToString() == "A" ? "Allow" : "Deny" %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="EntityTitle" HeaderText="From" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:PlaceHolder>

        <asp:Panel ID="pnlAddRole" runat="server" Visible="false" CssClass="add-role">
        
            <fieldset>
                <legend>Select Role to Add</legend>
                <Rock:RockDropDownList ID="ddlRoles" runat="server" Label="Role" AutoPostBack="true" onselectedindexchanged="ddlRoles_SelectedIndexChanged" />
                <dl>
                    <dt></dt>
                    <dd>
                        <asp:CheckBoxList ID="cblRoleActionList" runat="server" RepeatDirection="Horizontal" CssClass="inputs-list" ></asp:CheckBoxList>
                    </dd>
                </dl>
            </fieldset>

            <div class="actions margin-t-md">
                <asp:LinkButton ID="lbAddRole" runat="server" Text="Add" CssClass="btn btn-primary" onclick="lbAddRole_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddRole" runat="server" Text="Cancel" CssClass="btn btn-link" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAddUser" runat="server" Visible="false" CssClass="add-user">
        
            <fieldset>
                <legend>Select User to Add</legend>
                <Rock:PersonPicker ID="ppUser" runat="server" Label="User" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="lbAddUser" runat="server" Text="Add" CssClass="btn btn-primary" OnClientClick="showAddUser(false);" onclick="lbAddUser_Click"></asp:LinkButton>
                <asp:LinkButton ID="lbCancelAddUser" runat="server" Text="Cancel" CssClass="btn btn-link" onclick="lbCancelAdd_Click"></asp:LinkButton>
            </div>

        </asp:Panel>

    </div>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

