<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CreateAccount.ascx.cs" Inherits="Rock.Web.Blocks.Cms.CreateAccount" %>

<h2>
    Create a New Account</h2>
<p>
    In order to visit the secured pages you must have an account and be logged in.
    Use this page to create a new user account. </p>
<p>

<section class="facebook-register">
    <fieldset>
        <legend>Sign Up through Facebook</legend>
        <asp:HiddenField ID="fbFacebookId" runat="server" />
        <asp:Label ID="fbUserNameLabel" runat="server" Text="UserName" AssociatedControlID="fbUserName" />
        <asp:TextBox ID="fbUserName" runat="server" />
        <asp:RequiredFieldValidator ID="fbUserNameRequired" runat="server" 
            ControlToValidate="fbUserName" ErrorMessage="User Name is required." 
            ToolTip="User Name is required." ValidationGroup="CreateUserWizard2">*</asp:RequiredFieldValidator>
        <div id="availabilityMessage" class="form-input-warning"></span>
        <div id="usernameUnavailableRow"></div>
        <asp:Button ID="fbSubmit" runat="server" Text="Register with Facebook" />
    </fieldset>
</section>

    <asp:CreateUserWizard ID="cuWizard" runat="server" CancelDestinationPageUrl="~/Page/1"
        ContinueDestinationPageUrl="~/Page/1" DisplayCancelButton="True">
        <WizardSteps>
            <asp:CreateUserWizardStep runat="server">
                <ContentTemplate>
                    <Rock:NotificationBox ID="ErrorMessage" EnableViewState="false"  runat="server" Title="Warning" Text="Yellow" NotificationBoxType="Error" />
                    <fieldset>
                        <legend>Sign Up for an Account</legend>
                         <ol>
                            <li>
                                 <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label>
                                 <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" 
                                            ControlToValidate="UserName" ErrorMessage="User Name is required." 
                                            ToolTip="User Name is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                                        <div id="availabilityMessage" class="form-input-warning"></span>
                                        <div id="usernameUnavailableRow"></div>
                            </li>
                            <li>
                                <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label>
                                <asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" 
                                            ControlToValidate="Password" ErrorMessage="Password is required." 
                                            ToolTip="Password is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                            </li>
                            <li>
                                <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Confirm Password:</asp:Label>
                                <asp:TextBox ID="ConfirmPassword" runat="server" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" 
                                            ControlToValidate="ConfirmPassword" 
                                            ErrorMessage="Confirm Password is required." 
                                            ToolTip="Confirm Password is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                            </li>
                            <li>
                                <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email">E-mail:</asp:Label>
                                <asp:TextBox ID="Email" runat="server"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="EmailRequired" runat="server" 
                                            ControlToValidate="Email" ErrorMessage="E-mail is required." 
                                            ToolTip="E-mail is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                            </li>
                            <li>
                                <asp:Label ID="QuestionLabel" runat="server" AssociatedControlID="Question">Security Question:</asp:Label>
                                <asp:TextBox ID="Question" runat="server"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="QuestionRequired" runat="server" 
                                            ControlToValidate="Question" ErrorMessage="Security question is required." 
                                            ToolTip="Security question is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                            </li>
                            <li>
                                <asp:Label ID="AnswerLabel" runat="server" AssociatedControlID="Answer">Security Answer:</asp:Label>
                                <asp:TextBox ID="Answer" runat="server"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="AnswerRequired" runat="server" 
                                            ControlToValidate="Answer" ErrorMessage="Security answer is required." 
                                            ToolTip="Security answer is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                            </li>
                            <li>
                                <asp:CompareValidator ID="PasswordCompare" runat="server" 
                                            ControlToCompare="Password" ControlToValidate="ConfirmPassword" 
                                            Display="Dynamic" 
                                            ErrorMessage="The Password and Confirmation Password must match." 
                                            ValidationGroup="CreateUserWizard1"></asp:CompareValidator>
                            </li>
                        </ol>                            
                    </fieldset> 
                </ContentTemplate>
            </asp:CreateUserWizardStep>
            <asp:CompleteWizardStep runat="server">
            </asp:CompleteWizardStep>
        </WizardSteps>
    </asp:CreateUserWizard>
</p>


