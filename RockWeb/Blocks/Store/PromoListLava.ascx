<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PromoListLava.ascx.cs" Inherits="RockWeb.Blocks.Store.PromoListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
