<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PieChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.PieChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." />
        <Rock:PieChart ID="pcExample" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
