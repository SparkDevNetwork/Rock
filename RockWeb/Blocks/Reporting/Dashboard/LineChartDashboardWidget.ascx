<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LineChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.LineChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfDataTable" runat="server" />

        <script type="text/javascript">

            // Load the Visualization API and the corechart package.
            google.load('visualization', '1.0', { 'packages': ['corechart'] });

            // Set a callback to run when the Google Visualization API is loaded.
            google.setOnLoadCallback(drawChart);

            function drawChart() {
                // data for chart
                debugger
                var arrayText = $('#<%=hfDataTable.ClientID%>').val();
                var arrayData = eval(arrayText);
                var data = new google.visualization.arrayToDataTable(arrayData);

                // options for chart
                var options = {
                    'title': 'Some Data in a chart',
                    'width': 1200,
                    'height': 300
                };

                // creat and draw chart
                var chartDiv = document.getElementById('dashboard-widget-line-chart');
                var chart = new google.visualization.LineChart(chartDiv);
                chart.draw(data, options);
            }

        </script>

        <!-- chart holder -->
        <div id="dashboard-widget-line-chart"></div>

    </ContentTemplate>
</asp:UpdatePanel>
