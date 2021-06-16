<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuthScopeList.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.AuthScopeList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="maGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-openid"></i>OpenID Connect Scopes</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPublicName" runat="server" Label="Public Name"></Rock:RockTextBox>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gAuthScopes" runat="server"
                        AllowSorting="true"
                        RowItemText="Scope"
                        OnRowDataBound="gAuthScopes_RowDataBound"
                        ShowConfirmDeleteDialog="true"
                        OnRowSelected="gAuthScopes_RowSelected">
                        <Columns>
                            <asp:BoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                            <asp:BoundField HeaderText="Public Name" DataField="PublicName" SortExpression="PublicName" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField ID="dfDeleteScope" OnClick="gAuthScopes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>