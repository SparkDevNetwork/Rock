<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SMSLogin.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Authentication.SMSLogin" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="col-md-12">

            <legend>Mobile Phone Login</legend>
            <asp:Panel ID="pnlPhoneNumber" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" ValidationGroup="PhoneNumber" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <p>
                    <asp:Label Text="" ID="lbPrompt" runat="server" />
                </p>

                <Rock:PhoneNumberBox ID="tbPhoneNumber" runat="server" Label="Mobile Phone Number"
                    Required="true" DisplayRequiredIndicator="false" ValidationGroup="PhoneNumber" />
                <asp:RegularExpressionValidator ID="validateEmail" runat="server" ErrorMessage="Please enter a valid phone number." ControlToValidate="tbPhoneNumber"
                    ValidationExpression="^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$" ValidationGroup="PhoneNumber" Display="None" />

                <Rock:BootstrapButton ID="btnGenerate" runat="server" Text="Send Code" CssClass="btn btn-primary" OnClick="btnGenerate_Click" ValidationGroup="PhoneNumber" CausesValidation="true" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlCode" Visible="false">
                <p>We have sent you a code please enter it here to login.</p>
                <Rock:NotificationBox ID="nbError" Visible="false" NotificationBoxType="Danger" Text="Sorry, the code you entered did not match the code we generated." runat="server" />
                <Rock:RockTextBox runat="server" Label="Code" ID="tbCode" Required="true" DisplayRequiredIndicator="false" ValidationGroup="Code" />
                <Rock:BootstrapButton runat="server" ID="btnLogin" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" ValidationGroup="Code" CausesValidation="true" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlNoNumber" Visible="false">
                <p>
                    <asp:Label runat="server" ID="lbNoNumber" />
                </p>
                <Rock:BootstrapButton runat="server" ID="btnNoNmber" CssClass="btn btn-default" Text="Go Back" OnClick="btnNoNmber_Click" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlDuplicateNumber" Visible="false">
                <p>
                    <asp:Label runat="server" ID="lbDuplicateNumber" />
                </p>
                <Rock:BootstrapButton runat="server" ID="btnResolution" CssClass="btn btn-primary" Text="Request Resolution" OnClick="btnResolution_Click"></Rock:BootstrapButton>
                <Rock:BootstrapButton runat="server" ID="btnDuplicateNumber" CssClass="btn btn-default" Text="Go Back" OnClick="btnDuplicateNumber_Click" />
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlRateLimited" Visible="false">
                <p>
                    We're sorry. You have exceeded the number of attempts to log in with SMS.
                </p>
                <Rock:BootstrapButton runat="server" ID="btnRateLimited" CssClass="btn btn-default" Text="Go Back" OnClick="btnRateLimited_Click" />
            </asp:Panel>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>


