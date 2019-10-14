<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemOccurrenceLava.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Event.EventItemOccurrenceLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
