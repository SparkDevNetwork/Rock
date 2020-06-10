<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinderLava.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Groups.GroupFinderLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>

