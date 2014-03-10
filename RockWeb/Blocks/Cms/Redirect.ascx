<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Redirect.ascx.cs" Inherits="RockWeb.Blocks.Cms.Redirect" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lRedirect" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
