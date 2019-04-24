<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SentryTest.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Sentry.SentryTest" %>
<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbInformation" runat="server" NotificationBoxType="Info" Visible="false"
            Text="Your error has been sent to sentry!" />

        <Rock:RockTextBox ID="tbError" runat="server" Text="" Label="Enter the error message to send to Sentry" />
        <Rock:BootstrapButton ID="btnSubmitError" runat="server" Text="Submit" OnClick="btnSubmitError_Click" />
    </ContentTemplate>
</asp:UpdatePanel>