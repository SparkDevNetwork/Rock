<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="RockWeb.Blocks.Security.Login" %>

<div class="classic-login">
    <asp:Login ID="login1" runat="server" RenderOuterTable="false" >
        <LayoutTemplate>
            <span class="failureNotification">
                <asp:Literal ID="FailureText" runat="server"></asp:Literal>
            </span>
            <asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" CssClass="failureNotification" 
                    ValidationGroup="LoginUserValidationGroup"/>
                <fieldset class="verticle">
                    <legend>Login</legend>
                    <ol>
                        <li>
                            <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">Username</asp:Label>
                            <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" 
                                    CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required." 
                                    ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </li>
                        <li>
                            <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password</asp:Label>
                            <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" 
                                    CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required." 
                                    ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </li>
                        <li>
                            <asp:CheckBox ID="RememberMe" runat="server"/> <span class="remember">Remember me on this computer</span>
                        </li>
                    </ol>
                </fieldset>
                <br />
                <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Login" ValidationGroup="LoginUserValidationGroup" CssClass="button" />
        </LayoutTemplate>
    </asp:Login>
    <span class="forgot">
        Help: <a href="needToImplement">I forgot my username/password</a>
    </span>
</div>

<asp:PlaceHolder ID="phFacebookLogin" runat="server">
    <div class="facebook-login">
        <asp:ImageButton ID="ibFacebookLogin" runat="server" ImageUrl="~/Assets/Images/fb-login.png" BorderStyle="None" OnClick="ibFacebookLogin_Click" class="image" />
    </div>
</asp:PlaceHolder>