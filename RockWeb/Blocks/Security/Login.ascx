<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>


    <asp:Login ID="login1" runat="server" RenderOuterTable="false" >
        <LayoutTemplate>
            
            <asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" CssClass="failureNotification" 
                    ValidationGroup="LoginUserValidationGroup"/>
                
                <fieldset class="stacked">
				    <legend>Login</legend> 
                    <dl>
					    <dt><asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">Username</asp:Label></dt>
    				    <dd>
                            <asp:TextBox ID="Username" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" 
                                    CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required." 
                                    ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </dd>
                    </dl>
                    <dl>
                        <dt><asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password</asp:Label></dt>
    				    <dd>
                            <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" 
                                    CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required." 
                                    ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </dd>
    				</dl>
                    <dl>
                        <dt></dt>
                        <dd>
                            <ul class="inputs-list">
				                <li>
				                  <label>
				                    <asp:CheckBox ID="RememberMe" runat="server"/>
				                    <span>Remember me on this computer</span>
				                  </label>
				                </li>
                            </ul>
                        </dd>
                    </dl>

                    <div class="alert-message warning">
                        <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                    </div>

                    <div class="actions">
                        <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Login" ValidationGroup="LoginUserValidationGroup" CssClass="btn primary" />
                    </div>
                </fieldset>

                
        </LayoutTemplate>
    </asp:Login>
    <span class="forgot">
        Help: <a href="needToImplement">I forgot my username/password</a>
    </span>


<asp:PlaceHolder ID="phFacebookLogin" runat="server">
    <div class="facebook-login">
        <asp:ImageButton ID="ibFacebookLogin" runat="server" ImageUrl="~/Assets/Images/facebook-login.png" BorderStyle="None" OnClick="ibFacebookLogin_Click" class="image" />
    </div>
</asp:PlaceHolder>