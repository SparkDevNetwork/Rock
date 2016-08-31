<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicChart.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicChart" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>

        <Rock:LineChart ID="lcLineChart" runat="server" />
        <Rock:PieChart ID="pcPieChart" runat="server" />
        <Rock:BarChart ID="bcBarChart" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
