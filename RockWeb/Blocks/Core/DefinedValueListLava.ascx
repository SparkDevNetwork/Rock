<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedValueListLava.ascx.cs" Inherits="RockWeb.Blocks.Core.DefinedValueListLiquid" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lContent" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
