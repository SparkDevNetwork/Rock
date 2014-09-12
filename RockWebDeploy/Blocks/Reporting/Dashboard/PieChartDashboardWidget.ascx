<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Reporting.Dashboard.PieChartDashboardWidget, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>
        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one metric in the block settings." />
        <Rock:PieChart ID="pcChart" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
