<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Login.ascx.cs" Inherits="Rock.Web.Blocks.Cms.Login" %>

<section class="facebook-login">
    <asp:Button ID="btnFbLogOn" runat="server" Text="Login With Facebook" OnClick="btnFbLogOn_Click" />
</section>

<asp:Login ID="login1" runat="server" RenderOuterTable="false" >
    <LayoutTemplate>
        <span class="failureNotification">
            <asp:Literal ID="FailureText" runat="server"></asp:Literal>
        </span>
        <asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" CssClass="failureNotification" 
                ValidationGroup="LoginUserValidationGroup"/>
            <fieldset>
                <legend>Account Information</legend>
                <ol>
                    <li>
                        <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">Username:</asp:Label>
                        <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" 
                                CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required." 
                                ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                    </li>
                    <li>
                        <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label>
                        <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" 
                                CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required." 
                                ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                    </li>
                    <li>
                        <asp:Label ID="RememberMeLabel" runat="server" AssociatedControlID="RememberMe" CssClass="inline">Keep me logged in</asp:Label>
                        <asp:CheckBox ID="RememberMe" runat="server"/>
                    </li>
                </ol>
            </fieldset>
            <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Log In" ValidationGroup="LoginUserValidationGroup" />
    </LayoutTemplate>
</asp:Login>