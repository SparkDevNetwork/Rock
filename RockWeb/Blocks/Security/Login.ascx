<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>

    

    <fieldset>
	    <legend>Login</legend> 
        <Rock:LabeledTextBox ID="tbUserName" runat="server" LabelText="Username" Required="true" DisplayRequiredIndicator="false" ></Rock:LabeledTextBox>
        <Rock:LabeledTextBox ID="tbPassword" runat="server" LabelText="Password" Required="true" DisplayRequiredIndicator="false" TextMode="Password" ></Rock:LabeledTextBox>
        
        <div class="control-group">
        	<div class="controls">
        		<label class="checkbox">
        			<asp:CheckBox ID="cbRememberMe" runat="server"/> Remember me on this computer
        		</label>
        	</div>
        </div>
        
    </fieldset>

    <div class="alt-authentication">
        
        <asp:PlaceHolder ID="phFacebookLogin" runat="server">
            <!-- Facebook -->
            <div class="facebook-login">
                <asp:LinkButton ID="lbFacebookLogin" runat="server" OnClick="lbFacebookLogin_Click" CausesValidation="false"><img src="<%= Page.ResolveUrl("~/Assets/Images/facebook-login.png") %>" style="border:none" /></asp:LinkButton>
            </div>
        </asp:PlaceHolder>

    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage warning"/>

    <div class="form-actions">
        <asp:Button ID="LoginButton" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
        <asp:Button ID="NewAccountButton" runat="server" Text="Create New Account" CssClass="btn" OnClick="btnNewAccount_Click" CausesValidation="false" />
        <asp:Button ID="Cancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" CausesValidation="false" />
    </div>

    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-info"/>


   

