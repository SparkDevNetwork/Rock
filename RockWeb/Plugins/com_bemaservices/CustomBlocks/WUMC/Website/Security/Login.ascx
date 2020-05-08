<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Security.Login" %>


    <div class="row">
		
    <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin">
		<div class="col-md-8 mb-5">
        <fieldset>

         
                <div id="divSocialLogin" runat="server" class="col-sm-6 margin-b-lg">
                    <p>Login with social account</p>
                    <asp:PlaceHolder ID="phExternalLogins" runat="server"></asp:PlaceHolder>
                </div>
                <div id="divOrgLogin" runat="server" class="col-sm-6">

                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"/>

                        <asp:Literal ID="lPromptMessage" runat="server" />
                        <asp:Literal ID="lInvalidPersonTokenText" runat="server" />
                        <Rock:RockTextBox ID="tbUserName" runat="server" Placeholder="Username" Required="true" DisplayRequiredIndicator="false"  CssClass="mb-10"></Rock:RockTextBox><br/>
                        <Rock:RockTextBox ID="tbPassword" runat="server" Placeholder="Password" autocomplete="off" Required="true" DisplayRequiredIndicator="false" ValidateRequestMode="Disabled" TextMode="Password" ></Rock:RockTextBox>
                        
						<br/>
						<Rock:RockCheckBox ID="cbRememberMe" runat="server" Text="Stay logged in on this device" />        
                    
						<br/>
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary mb-1" OnClick="btnLogin_Click" />
						<asp:Button ID="btnHelp" runat="server" Text="Forgot Password?" CssClass="btn btn-primary ml-1 mb-1" OnClick="btnHelp_Click" CausesValidation="false" />
                        <asp:Button ID="btnNewAccount" runat="server" Text="Create Account" CssClass="btn btn-primary ml-1 mb-1" OnClick="btnNewAccount_Click" CausesValidation="false" />
						<asp:Button ID="btnGuest" runat="server" Text="Continue as Guest" CssClass="btn btn-primary ml-1 mb-1" OnClick="btnGuest_Click1" CausesValidation="false" Visible="false" />
                       

                        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-warning block-message margin-t-md"/>
                    
                </div>
          
        </fieldset>
	</div>

			<div class="col-md-4 " style="margin-bottom:20px;">
		Welcome to your ‘My TWUMC’ user account. Please sign in to manage your TWUMC online activity. Don’t have an account yet? Please click below to ‘Create’ a new account.
<br/><br/>
		<b>Benefits of having an account?</b><br>• Update family contact information <br> • Set-up and manage giving <br> • View and manage event registrations
		</div>
	



    </asp:Panel>
	
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
   

