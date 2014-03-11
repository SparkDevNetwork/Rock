<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>

    <asp:Panel ID="pnlLogin" runat="server">

        <fieldset>

            <legend>Login</legend> 

            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>

            <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" DisplayRequiredIndicator="false" TextMode="Password" ></Rock:RockTextBox>
            <Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Remember me on this computer" />        
    
        </fieldset>

        <div class="alt-authentication">
            <asp:PlaceHolder ID="phExternalLogins" runat="server">
    <%--            <div class="facebook-login">
                    <asp:LinkButton ID="lbFacebookLogin" runat="server" OnClick="lbFacebookLogin_Click" CausesValidation="false"><img src="<%= Page.ResolveUrl("~/Assets/Images/facebook-login.png") %>" style="border:none" /></asp:LinkButton>
                </div>--%>
            </asp:PlaceHolder>
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message "/>

        <div class="form-actions">
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
            <asp:Button ID="btnNewAccount" runat="server" Text="Create New Account" CssClass="btn btn-action" OnClick="btnNewAccount_Click" CausesValidation="false" />
            <asp:Button ID="btnHelp" runat="server" Text="Forgot Account" CssClass="btn btn-action" OnClick="btnHelp_Click" CausesValidation="false" />
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
   

