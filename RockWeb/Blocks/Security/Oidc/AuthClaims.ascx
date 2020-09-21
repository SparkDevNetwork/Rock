<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuthClaims.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.AuthClaims" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="maGridWarning" runat="server" />
            <Rock:ModalDialog ID="dlgClaimDetails" runat="server" Title="Claim Detail">
                <Content>
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:HiddenField ID="hfAuthClaimId" runat="server" />
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbClaimName" Required="true" Label="Name" runat="server" MaxLength="50" CssClass="form-group" />
                            <asp:RegularExpressionValidator ID="regValidator" ControlToValidate="tbClaimName" runat="server" ValidationExpression="^[a-zA-Z0-9_]*$" Display="None" ErrorMessage="Only alphanumeric and underscore characters can be used." />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbClaimActive" runat="server" Checked="true" Label="Active" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbClaimPublicName" MaxLength="100" runat="server" Label="Public Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CodeEditor EditorMode="Lava" ID="tbClaimValue" runat="server" Label="Value" Help="Claim value that should be returned when this claim is requested.<span class='tip tip-lava'></span>." />
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-openid"></i>OpenID Connect Claims</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

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

                    <Rock:Grid ID="gAuthClaims" runat="server"
                        AllowSorting="true"
                        RowItemText="Claim"
                        OnRowDataBound="gAuthClaims_RowDataBound"
                        ShowConfirmDeleteDialog="true"
                        OnRowSelected="gAuthClaims_RowSelected">
                        <Columns>
                            <asp:BoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                            <asp:BoundField HeaderText="Public Name" DataField="PublicName" SortExpression="PublicName" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField ID="dfDeleteScope" OnClick="gAuthClaims_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
