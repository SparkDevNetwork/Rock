<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PieChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.PieChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>
        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one metric in the block settings." />
        <Rock:RockPieChart ID="metricChart" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
