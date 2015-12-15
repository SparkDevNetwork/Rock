<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleRegistration.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Calendar.SimpleRegistration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfTriggerScroll" runat="server" Value="" />

        <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <Rock:NotificationBox ID="nbMain" runat="server" Visible="false"></Rock:NotificationBox>

        <asp:Panel ID="pnlRegistrant" runat="server" Visible="true" CssClass="registrationentry-registrant">

            <h1>
                <asp:Literal ID="lRegistrantTitle" runat="server" /></h1>

            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
            <h2>Email or Phone Number</h2>
            <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
            <Rock:PhoneNumberBox ID="tbPhoneNumber" runat="server" Label="Phone Number" />

            <div class="actions">
                <Rock:BootstrapButton ID="lbRegister" runat="server" AccessKey="n" Text="Register" CssClass="btn btn-primary" CausesValidation="true" OnClick="lbRegister_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <asp:Literal ID="lSuccess" runat="server" />
            <asp:Literal ID="lSuccessDebug" runat="server" Visible="false" />

            <Rock:BootstrapButton ID="lbAddRegistrant" runat="server" AccessKey="a" Text="Register Another Person" CssClass="btn btn-primary" CausesValidation="true" OnClick="lbAddRegistrant_Click" />

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
