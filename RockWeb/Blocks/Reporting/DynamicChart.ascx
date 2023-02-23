<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicChart.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicChart" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <Rock:RockLineChart ID="lcLineChart" runat="server" />
        <Rock:RockPieChart ID="pcPieChart" runat="server" />
        <Rock:RockBarChart ID="bcBarChart" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
