<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BarChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.BarChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." />
        <Rock:BarChart ID="bcExample" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
