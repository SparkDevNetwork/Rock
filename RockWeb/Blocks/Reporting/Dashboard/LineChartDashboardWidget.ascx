<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LineChartDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.LineChartDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfColumns" runat="server" />
        <asp:HiddenField ID="hfRestUrlParams" runat="server" />
        <asp:HiddenField ID="hfOptions" runat="server" />

        <script type="text/javascript">

            // Load the Visualization API and the corechart package.
            google.load('visualization', '1.0', { 'packages': ['corechart'] });

            // Set a callback to run when the Google Visualization API is loaded.
            google.setOnLoadCallback(function() {

                var restUrl = '<%= ResolveUrl( "~/api/MetricValues/GetChartData/" ) %>';

                $.ajax({
                    url: restUrl + $('#<%= hfRestUrlParams.ClientID%>').val(),
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (chartDataJS) {
                    // define chart
                    var columnsText = $('#<%=hfColumns.ClientID%>').val();
                    var columnsData = $.parseJSON(columnsText);

                    var dataTable = new google.visualization.DataTable(
                        {
                            cols: columnsData
                        });

                    // data for chart is in JS object literal notation, so we need to eval it first
                    var chartDataArray = eval(chartDataJS);

                    dataTable.addRows(chartDataArray);

                    // options for chart
                    var optionsText = $('#<%=hfOptions.ClientID%>').val();
                    var options = $.parseJSON(optionsText);

                    // create and draw chart
                    var chartDiv = document.getElementById('<%=pnlLineChartHolder.ClientID%>');
                    var chart = new google.visualization.LineChart(chartDiv);
                    chart.draw(dataTable, options);

                    $(window).smartresize(function () {
                        {
                            chart.draw(dataTable, options);
                        }
                    });
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    debugger
                });
            });

        </script>

        <!-- chart holder -->
        <asp:Panel ID="pnlLineChartHolder" runat="server"></asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
