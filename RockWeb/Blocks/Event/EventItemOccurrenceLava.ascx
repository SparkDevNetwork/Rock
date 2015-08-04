<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemOccurrenceLava.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
