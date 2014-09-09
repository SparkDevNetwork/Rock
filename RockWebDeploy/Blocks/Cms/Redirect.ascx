<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.Redirect, RockWeb" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Danger" />
    </ContentTemplate>
</asp:UpdatePanel>
