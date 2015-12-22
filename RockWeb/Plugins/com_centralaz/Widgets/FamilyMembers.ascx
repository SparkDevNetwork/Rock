<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyMembers.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.FamilyMembers" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lContent" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
