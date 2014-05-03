<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ColumnChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.ColumnChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ColumnChart ID="colchrtExample" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
