<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.Login" %>

<asp:Panel ID="mdLoginShell" runat="server">
    <div ID="login-main-panel" style="box-sizing: border-box; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); transition: all 450ms cubic-bezier(0.23, 1, 0.32, 1) 0ms; position: relative; width: 75%; max-width: 768px; margin: 0px auto; z-index: 1500; opacity: 1; transform: translate(0px, 64px);">
        <div style="color: rgba(0, 0, 0, 0.87); background-color: rgb(255, 255, 255); transition: all 450ms cubic-bezier(0.23, 1, 0.32, 1) 0ms; box-sizing: border-box; font-family: Roboto, sans-serif; -webkit-tap-highlight-color: rgba(0, 0, 0, 0); box-shadow: rgba(0, 0, 0, 0.25) 0px 14px 45px, rgba(0, 0, 0, 0.22) 0px 10px 18px; border-radius: 2px;">
            <div style="font-size: 16px; color: rgba(0, 0, 0, 0.6); padding: 24px; box-sizing: border-box; overflow-y: hidden; border-top: none; border-bottom: none; background-color: rgb(255, 255, 255);">
                <div class="row">
                    <div id="divOrgLogin" runat="server" class="col-sm-12">
                        <div>
                            <h1 class="login-form-title text-center">Log In</h1>
                            <p style="margin-top: -.75em; margin-bottom: 3em;" class="small-paragraph-bold">Fill out the form below to securely access your account.</p>
                        </div>

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

                        <div class="col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-12">
                            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>
                                
                            <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:RockTextBox>

                            <div style="margin: 25px 0 25px 0;"></div>

                            <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" autocomplete="off" Required="true" DisplayRequiredIndicator="false" ValidateRequestMode="Disabled" TextMode="Password" ></Rock:RockTextBox>
                            <Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Remember Me" />

                            <div class="row v-center" style="margin: 25px 0 25px 0;">
                                <div class="col-md-4 col-sm-4 col-xs-6" style="padding: 0;">
                                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="login-form-button btn btn-primary" OnClick="btnLogin_Click" />
                                </div>

                                <div class="col-md-8 col-sm-4 col-xs-6">
                                    <asp:Button ID="btnHelp" runat="server" Text="Forgot username or password?" CssClass="small-paragraph login-form-forgot" OnClick="btnHelp_Click" CausesValidation="false" />
                                </div>
                            </div>
                                
                            <asp:Button ID="btnNewAccount" runat="server" Text="Create Account" CssClass="login-form-register btn btn-action" OnClick="btnNewAccount_Click" CausesValidation="false" />

                            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message margin-t-md"/>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Panel>

<asp:Panel ID="pnlTempLoginTrigger" runat="server">

    <div class="alert alert-danger">
        <asp:Button ID="ButtonTempLoginTrigger" runat="server" Text="Login" CssClass="btn btn-primary" OnClientClick="return displayLoginPanel();" />
    </div>

</asp:Panel>

<script type="text/javascript">
    function displayLoginPanel() {

        var loginPanel = $("#login-main-panel");
        loginPanel.css("visibility", "visible");
        loginPanel.css("height", "");

        return false;
    }
</script>