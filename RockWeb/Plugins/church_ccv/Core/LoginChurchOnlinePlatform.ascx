<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginChurchOnlinePlatform.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.Login" %>

    <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin">
        <div class="sso-login-wrapper">
            <div id="sso-login-panel-title">
                <img src="themes/church_ccv_external_v8/assets/ccv_logo-hi-res.png" style="width: 125px;"/>
            
                <h1 class="lm-form-title text-center">Log In</h1>
                <p class="small-paragraph-bold">Fill out the form below to securely access your account.</p>
            </div>

            <div id="login-message">
                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message margin-t-md"/>
            </div>

            <div id="loginform" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>

                <asp:Literal ID="lPromptMessage" runat="server" />
                <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:RockTextBox>
                <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" autocomplete="off" Required="true" DisplayRequiredIndicator="false" ValidateRequestMode="Disabled" TextMode="Password" ></Rock:RockTextBox>
                <Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Remember me" />   
            </div>     
                    
            <div id="login-buttons">
                <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary btn-sso-login" OnClick="btnLogin_Click" />
                <asp:Button ID="btnHelp" runat="server" Text="Forgot username or password?" CssClass="small-paragraph lm-form-forgot" OnClick="btnHelp_Click" CausesValidation="false" />
                <br />
                <br />
                <asp:Button ID="btnNewAccount" runat="server" Text="Create Account" CssClass="lm-form-register btn btn-action" OnClick="btnNewAccount_Click" CausesValidation="false" />
            </div>
        </div>
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
   

