<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Logout.ascx.cs" Inherits="RockWeb.Blocks.Security.Oidc.Logout" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotificationBox" runat="server" NotificationBoxType="Danger" Visible="false" Title="Error" />
    </ContentTemplate>
</asp:UpdatePanel>