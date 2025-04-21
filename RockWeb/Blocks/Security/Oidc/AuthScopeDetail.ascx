<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuthScopeDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.AuthScopeDetail" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-openid"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <div id="pnlEditDetails" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:HiddenField ID="hfRestUserId" runat="server" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" Required="true" Label="Name" runat="server" MaxLength="50" CssClass="form-group" />
                            <asp:RegularExpressionValidator ID="regValidator" ControlToValidate="tbName" runat="server" ValidationExpression="^[a-zA-Z0-9_]*$" Display="None" ErrorMessage="Only alphanumeric and underscore characters can be used." />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" Checked="true" Label="Active" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbPublicName" Label="Public Name" MaxLength="100" runat="server" CssClass="form-group" />
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>