<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SMSLogin.ascx.cs" Inherits="RockWeb.Blocks.Security.SMSLogin" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="col-md-12">

            <legend>Mobile Login</legend>
            <asp:Panel ID="pnlPhoneNumber" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="PhoneNumber" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <p>
                    <asp:Label Text="" ID="lbPrompt" runat="server" />
                </p>

                <Rock:PhoneNumberBox ID="tbPhoneNumber" runat="server" Label="Mobile Phone Number"
                    Required="true" DisplayRequiredIndicator="false" ValidationGroup="PhoneNumber" />
                <asp:RegularExpressionValidator ID="validateEmail" runat="server" ErrorMessage="Please enter a valid phone number." ControlToValidate="tbPhoneNumber"
                    ValidationExpression="^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$" ValidationGroup="PhoneNumber" Display="None" />

                <Rock:BootstrapButton ID="btnGenerate" runat="server" Text="Generate Code" CssClass="btn btn-primary" OnClick="btnGenerate_Click" ValidationGroup="PhoneNumber" CausesValidation="true" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlCode" Visible="false">
                <p>We have sent you a code please enter it here to login.</p>
                <Rock:NotificationBox ID="nbError" Visible="false" NotificationBoxType="Danger" runat="server" />
                <Rock:RockTextBox runat="server" Label="Code" ID="tbCode" Required="true" DisplayRequiredIndicator="false" ValidationGroup="Code" />
                <Rock:BootstrapButton runat="server" ID="btnLogin" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" ValidationGroup="Code" CausesValidation="true" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlResolve" Visible="false">
                <p>
                    <asp:Label runat="server" ID="lbResolve" />
                </p>
                <Rock:BootstrapButton runat="server" ID="btnResolve" Visible="false" CssClass="btn btn-primary" Text="Update Mobile Phone Number" OnClick="btnResolve_Click" />
                <Rock:BootstrapButton runat="server" ID="btnCancel" CssClass="btn btn-default" Text="Cancel" OnClick="btnCancel_Click" />
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


