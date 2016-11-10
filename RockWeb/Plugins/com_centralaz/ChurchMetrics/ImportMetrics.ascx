<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ImportMetrics.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.ChurchMetrics.ImportMetrics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Danger" />

        <asp:Button ID="btnGrabJson" runat="server" Text="Import Metrics" CssClass="btn btn-primary" OnClick="btnGrabJson_Click" />

        <asp:Literal ID="lContent" runat="server"></asp:Literal>


    </ContentTemplate>
</asp:UpdatePanel>
