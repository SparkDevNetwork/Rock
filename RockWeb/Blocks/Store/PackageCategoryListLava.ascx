<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageCategoryListLava.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageCategoryListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
