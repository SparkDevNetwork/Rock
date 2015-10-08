<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FollowingGroups.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.FollowingGroups" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lContent" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
