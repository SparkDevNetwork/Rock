<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageDetailLava.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
