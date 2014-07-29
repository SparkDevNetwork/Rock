<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BarChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.BarChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </div>
        <div class="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </div>
        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." />
        <Rock:BarChart ID="bcChart" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
