<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AcmeConfig.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.AcmeCertificate.AcmeConfig" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:ValidationSummary ID="vSummary" runat="server" CssClass="alert alert-danger" />

        <asp:Panel ID="pnlAccount" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-user-secret"></i> Account</h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlAccountDetail" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="ltAccountEmail" runat="server" Label="Account Email"></Rock:RockLiteral>
                        </div>

                        <div class="col-md-6">
                            <Rock:RockLiteral ID="ltTestMode" runat="server" Label="Test Mode" />
                        </div>
                    </div>

                    <hr class="margin-t-none" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="ltOfflineMode" runat="server" Label="Offline Mode" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                        <asp:LinkButton ID="lbRegister" runat="server" Text="Register" CssClass="btn btn-primary" OnClick="lbRegister_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlAccountRegister" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbExistingAccount" runat="server" NotificationBoxType="Warning" Visible="false">
                        You already have an account setup. If you register again you will no longer be able to renew your
                        existing certificates. You should only need to re-register as a last resort if things are not working.
                    </Rock:NotificationBox>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:EmailBox ID="tbAccountEmail" runat="server" Label="Email" Required="true" ValidationGroup="Register" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbTestMode" runat="server" Label="Test Mode" Help="Uses the testing server which generates test certificates. Only used for debugging and testing purposes." ValidationGroup="Register" />
                        </div>
                    </div>

                    <div id="divTOS" runat="server" class="alert alert-info">
                        You must read and agree to the terms of service located at <asp:HyperLink ID="hlTOS" runat="server" />.
                    </div>

                    <Rock:RockCheckBox ID="cbTOSAgree" runat="server" Label="Agree to Terms Of Service" />

                    <div class="actions">
                        <asp:LinkButton ID="lbAccountRegisterSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbAccountRegisterSave_Click" ValidationGroup="Register" />
                        <asp:LinkButton ID="lbAccountRegisterCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbAccountRegisterCancel_Click" CausesValidation="false" ValidationGroup="Register" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlAccountEdit" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbOfflineMode" runat="server" Label="Offline Mode" Help="If your Rock installation does not have access to install certificates then enable this mode and the certificate will be generated and provided to you for manual installation." ValidationGroup="Edit" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbAccountEditSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbAccountEditSave_Click" ValidationGroup="Edit" />
                        <asp:LinkButton ID="lbAccountEditCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbAccountEditCancel_Click" CausesValidation="false" ValidationGroup="Edit" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>