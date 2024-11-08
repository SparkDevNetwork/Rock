<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ChangePassword.ascx.cs" Inherits="RockWeb.Blocks.Security.ChangePassword" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> Change Password</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox id="nbMessage" runat="server" Visible="false" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <asp:Panel ID="pnlChangePassword" runat="server" Visible="true">

                    <fieldset>
                        <legend>Change Password</legend>

                        <Rock:RockTextBox ID="tbOldPassword" runat="server" Label="Old Password" Required="true" TextMode="Password" ValidateRequestMode="Disabled"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="New Password" Required="true" TextMode="Password" ValidateRequestMode="Disabled"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm Password" Required="true" ValidateRequestMode="Disabled" TextMode="Password"></Rock:RockTextBox>
                        <asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="New Password and Confirm Password do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
                    </fieldset>

                    <div class="actions">
                        <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnChange_Click" Visible="false" />
                        <div id="pnlCaptcha" runat="server" class="form-group">
                            <Rock:Captcha ID="cpCaptcha" runat="server" OnTokenReceived="cpCaptcha_TokenReceived" />
                        </div>
                    </div>

                </asp:Panel>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
