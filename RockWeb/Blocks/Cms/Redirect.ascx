<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Redirect.ascx.cs" Inherits="RockWeb.Blocks.Cms.Redirect" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Danger" />
    </ContentTemplate>
</asp:UpdatePanel>
