<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuthClientDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.AuthClientDetail" %>

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
                    <asp:ValidationSummary
                        ID="valSummary"
                        runat="server"
                        HeaderText="Please correct the following:"
                        CssClass="alert alert-validation" />
                    <asp:HiddenField ID="hfRestUserId" runat="server" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox
                                ID="tbName"
                                Required="true"
                                Label="Name"
                                runat="server"
                                CssClass="form-group"
                                SourceTypeName="Rock.Model.AuthClient, Rock"
                                PropertyName="Name" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox
                                ID="cbActive"
                                runat="server"
                                Checked="true"
                                Label="Active" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="d-flex align-items-end">
                                <div class="flex-fill">
                                    <Rock:DataTextBox
                                        ID="tbClientId"
                                        Label="Client Id"
                                        Help="The OIDC ClientId. This the public identifier that the client apps will need to use."
                                        runat="server"
                                        CssClass="form-group"
                                        SourceTypeName="Rock.Model.AuthClient, Rock"
                                        PropertyName="ClientId" />
                                </div>
                                <div class="form-group">
                                    <asp:Button
                                        ID="lbGenerateClientId"
                                        runat="server"
                                        CausesValidation="false"
                                        ValidationGroup=""
                                        Text="Generate Id"
                                        CssClass="btn btn-primary ml-2"
                                        OnClick="lbGenerateClientId_Click" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="d-flex align-items-end">
                                <div class="flex-fill">
                                    <Rock:RockTextBox
                                        ID="tbClientSecret"
                                        Required="true"
                                        Help="For security purposes, we don't allow the secret to be displayed once created. Note that you only get one chance to see the Client Secret when you first generate it, so if you lose track of it you’ll need to generate a new one."
                                        Label="Client Secret"
                                        runat="server"
                                        CssClass="form-group" />
                                </div>
                                <div class="form-group">
                                    <asp:Button
                                        ID="lbGenerateClientSecret"
                                        runat="server"
                                        CausesValidation="false"
                                        Text="Generate Secret"
                                        CssClass="btn btn-primary ml-2"
                                        OnClick="lbGenerateClientSecret_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox
                                ID="tbRedirectUri"
                                Label="Redirect Uri"
                                Help="After the person has provided their credentials, they will be redirected back to the client via the path indicated here. This URI is provided by the client system."
                                runat="server"
                                CssClass="form-group"
                                SourceTypeName="Rock.Model.AuthClient, Rock"
                                PropertyName="RedirectUri" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox
                                ID="tbPostLogoutRedirectUri"
                                Label="Logout Redirect Uri"
                                Help="When the person logs out of the client system, they will be redirected to this URI. This is usually a URI on the client system."
                                runat="server"
                                CssClass="form-group"
                                SourceTypeName="Rock.Model.AuthClient, Rock"
                                PropertyName="PostLogoutRedirectUri" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox
                                ID="tbScopeApprovalExpiration"
                                Label="Scope Approval Expiration"
                                Help="The number of days that an individual's scope approval will be valid. After this expiration the individual will be asked to re-approve the scopes."
                                runat="server"
                                CssClass="form-group"
                                SourceTypeName="Rock.Model.AuthClient, Rock"
                                PropertyName="ScopeApprovalExpiration"
                                />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:PanelWidget
                                ID="pwScopes"
                                runat="server"
                                Title="Allowed Scopes and Claims <i class='fa fa-info-circle' data-toggle='tooltip' data-placement='top' title='These are the claims that are allowed to be returned if requested by the client.'></i>" Expanded="true">
                                <asp:Panel ID="litClaims" runat="server" />
                            </Rock:PanelWidget>
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton
                        ID="lbSave"
                        runat="server"
                        data-shortcut-key="s"
                        ToolTip="Alt+s"
                        Text="Save"
                        CssClass="btn btn-primary"
                        OnClick="lbSave_Click" />
                    <asp:LinkButton
                        ID="lbCancel"
                        runat="server"
                        data-shortcut-key="c"
                        ToolTip="Alt+c"
                        Text="Cancel"
                        CssClass="btn btn-link"
                        CausesValidation="false"
                        OnClick="lbCancel_Click" />
                </div>
            </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
