<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemLava.ascx.cs" Inherits="RockWeb.Blocks.Event.EventItemLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
