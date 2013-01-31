<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicQuery.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicQuery" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gReport" runat="server" AllowSorting="true" AutoGenerateColumns="true"></Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
