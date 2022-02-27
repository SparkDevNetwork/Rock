<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="login-block">

            <fieldset>
                <legend>Login</legend>

                <div class="row">
                    <Rock:NotificationBox ID="nbAdminRedirectPrompt" runat="server" NotificationBoxType="Danger" Visible="false" />
                    <asp:Panel ID="pnlRemoteAuthLogins" runat="server" CssClass="col-md-6 margin-b-lg remote-logins">
                        <p>
                            <asp:Literal ID="lRemoteAuthLoginsHeadingText" runat="server" Text="Login with social account" />
                        </p>
                        <asp:PlaceHolder ID="phExternalLogins" runat="server"></asp:PlaceHolder>
                    </asp:Panel>
                    <asp:Panel ID="pnlInternalAuthLogin" runat="server">

                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />



                        <asp:Literal ID="lPromptMessage" runat="server" />
                        <asp:Literal ID="lInvalidPersonTokenText" runat="server" />

                        <div class="col-xs-12">
                            <Rock:RockTextBox ID="tbUserName" runat="server" Label="email" Required="true" DisplayRequiredIndicator="false"></Rock:RockTextBox>
                        </div>

                        <div class="col-xs-12">
                            <Rock:RockTextBox ID="tbPassword" runat="server" Label="password" autocomplete="off" Required="true" DisplayRequiredIndicator="false" ValidateRequestMode="Disabled" TextMode="Password"></Rock:RockTextBox>
                            <ul class="list-unstyled list-inline align-middle">
                                <li>
                                    <Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Keep me logged in" />
                                </li>
                                <li class="pull-right clearfix" style="padding-top: 8px;">
                                    <a href="/newaccount">create account</a>
                                </li><br />
                                <li class="pull-right clearfix form-group">
                                    <a href="/forgotpassword" class="text-gray">forgot password?</a>
                                </li>
                                
                            </ul>
                            
                        </div>

                        <div class="col-xs-12 col-sm-4 clearfix">

                            <Rock:BootstrapButton ID="btnLogin" runat="server" Text="log in" CssClass="btn btn-sm btn-primary btn-block" OnClick="btnLogin_Click" DataLoadingText="Logging In..." />
                            <br />
                        </div>

                        <div class="col-xs-12 form-group">
                            
                        </div>

                        <div class="col-xs-12 hidden">
                            <p style="color: #666; padding: 1px 7px 2px; margin-top: 30px;">
                                don't have an account? 
                                <asp:Button ID="btnNewAccount" runat="server" Text="Register" CssClass="btn-link text-primary" OnClick="btnNewAccount_Click" CausesValidation="false" />
                            </p>
                        </div>

                        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message margin-t-md" />

                    </asp:Panel>
                </div>
            </fieldset>
        </asp:Panel>


        <asp:Panel ID="pnlLockedOut" runat="server" Visible="false">

            <div class="alert alert-danger">
                <asp:Literal ID="lLockedOutCaption" runat="server" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">

            <div class="alert alert-warning">
                <asp:Literal ID="lConfirmCaption" runat="server" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


