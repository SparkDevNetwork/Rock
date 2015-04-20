<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>

    <asp:Panel ID="pnlLogin" runat="server">

        <fieldset>
            <legend>Login</legend>

            <div class="row">
                <div id="divSocialLogin" runat="server" class="col-sm-6 margin-b-lg">
                    <p>Login with social account</p>
                    <asp:PlaceHolder ID="phExternalLogins" runat="server"></asp:PlaceHolder>
                </div>
                <div id="divOrgLogin" runat="server" class="col-sm-6">
                 
                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>

                        <asp:Literal ID="lPromptMessage" runat="server" />
                        <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" DisplayRequiredIndicator="false" TextMode="Password" ></Rock:RockTextBox>
                        <Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Remember me on this computer" />        
                    
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
                        <asp:Button ID="btnNewAccount" runat="server" Text="Register" CssClass="btn btn-action" OnClick="btnNewAccount_Click" CausesValidation="false" />
                        <asp:Button ID="btnHelp" runat="server" Text="Forgot Account" CssClass="btn btn-link" OnClick="btnHelp_Click" CausesValidation="false" />

                        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message margin-t-md"/>
                    
                </div>
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
   

