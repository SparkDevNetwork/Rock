<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemPersonalListLava.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemPersonalListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lMessages" runat="server" />

        <asp:Literal ID="lContent" runat="server" />
        
        <asp:Literal ID="lDebug" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
