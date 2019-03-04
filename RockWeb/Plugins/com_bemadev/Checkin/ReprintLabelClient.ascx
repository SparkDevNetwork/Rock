<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabelClient.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CheckIn.ReprintLabelClient" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <Rock:NotificationBox runat="server" ID="nbMessage" CssClass="margin-t-md margin-b-md"></Rock:NotificationBox>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
