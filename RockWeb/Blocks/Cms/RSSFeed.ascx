<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSSFeed.ascx.cs" Inherits="RockWeb.Blocks.Cms.RSSFeed" %>
<asp:UpdatePanel ID="upConntent" runat="server" >
    <ContentTemplate>
        <Rock:NotificationBox ID="nbRSSFeed" runat="server" NotificationBoxType="Info" Visible="false" />
        <asp:Panel ID="pnlContent" runat="server" Visible="false">
            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>