<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.StarkDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="alert alert-info">
            <h4>Stark Widget Template Block</h4>
            <p>Here is a simple line chart</p>
            <i>run 'Dev Tools/Sql/Populate_Metrics.sql' to populate this metric</i>
        </div>

        <Rock:LineChart ID="lcExample" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
