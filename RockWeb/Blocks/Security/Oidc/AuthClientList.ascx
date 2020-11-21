<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuthClientList.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.AuthClientList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-12">
                <asp:LinkButton ID="btnOpenIdScopes" runat="server" OnClick="btnOpenIdScopes_Click" CssClass="btn btn-default btn-sm margin-b-md pull-right">
                    OpenID Connect Scopes
                </asp:LinkButton>
            </div>
        </div>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-openid"></i>OpenID Connect Clients</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name"></Rock:RockTextBox>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gAuthClients" runat="server"
                        AllowSorting="true"
                        RowItemText="Authentication Client"
                        OnRowDataBound="gAuthClients_RowDataBound"
                        ShowConfirmDeleteDialog="true"
                        OnRowSelected="gAuthClients_RowSelected">
                        <Columns>
                            <asp:BoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                            <asp:BoundField HeaderText="Client Id" DataField="ClientId" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField ID="dfDeleteScope" OnClick="gAuthClients_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>