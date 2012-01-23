<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>

    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

    <fieldset class="stacked">
	    <legend>Login</legend> 
        <Rock:LabeledTextBox ID="tbUserName" runat="server" LabelText="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:LabeledTextBox>
        <Rock:LabeledTextBox ID="tbPassword" runat="server" LabelText="Password" Required="true" DisplayRequiredIndicator="false" ></Rock:LabeledTextBox>
        <dl>
            <dt></dt>
            <dd>
                <ul class="inputs-list">
				    <li>
				        <label>
				            <asp:CheckBox ID="cbRememberMe" runat="server"/>
				            <span>Remember me on this computer</span>
				        </label>
				    </li>
                </ul>
            </dd>
        </dl>
    </fieldset>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage warning"/>

    <div class="actions">
        <asp:Button ID="LoginButton" runat="server" Text="Login" CssClass="btn primary" OnClick="btnLogin_Click" />
        <asp:Button ID="NewAccountButton" runat="server" Text="Create New Account" CssClass="btn secondary" OnClick="btnNewAccount_Click" />
        <asp:Button ID="Cancel" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancel_Click" />
    </div>

    <span class="forgot">
        Help: <a href='<%= Page.ResolveUrl("~") + "ForgotUserName" %>'>I forgot my username/password</a>
    </span>

    <asp:PlaceHolder ID="phFacebookLogin" runat="server">
        <div class="facebook-login">
            <asp:LinkButton ID="lbFacebookLogin" runat="server" OnClick="lbFacebookLogin_Click" CausesValidation="false"><img src="<%= Page.ResolveUrl("~/Assets/Images/facebook-login.png") %>" style="border:none" /></asp:LinkButton>
        </div>
    </asp:PlaceHolder>

