<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingGroupsPersonalizedLava.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Fundraising.FundraisingGroupsPersonalizedLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lViewHtml" runat="server" />

        <asp:Literal ID="lDebug" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>