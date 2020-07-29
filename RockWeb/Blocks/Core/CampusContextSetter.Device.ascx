<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusContextSetter.Device.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server" />

        <asp:Literal ID="lDebug" runat="server" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
