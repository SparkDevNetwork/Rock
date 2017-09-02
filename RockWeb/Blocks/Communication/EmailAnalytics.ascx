<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfCommunicationListGroupId" runat="server" />
        <div class="panel panel-block panel-analytics">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-line-chart"></i>&nbsp;<asp:Literal ID="lTitle" runat="server" Text="Email Analytics" /></h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-12">
                        <%-- Main Opens/Clicks Line Chart --%>
                        <div class="chart-container">
                            <canvas id="openClicksLineChartCanvas" runat="server" />
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <%-- Opens/Clicks PieChart --%>
                        <div class="chart-container">
                            <canvas id="opensClicksPieChartCanvas" runat="server" />
                        </div>
                    </div>
                    <div class="col-md-8">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lUniqueOpens" runat="server" Label="Unique Opens" Text="TODO" />
                                <Rock:RockLiteral ID="lTotalOpens" runat="server" Label="Total Opens" Text="TODO" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lTotalClicks" runat="server" Label="Total Clicks" Text="TODO" />
                                <Rock:RockLiteral ID="lClickThroughRate" runat="server" Label="Click Through Rate (CTR)" Text="TODO" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <%-- Clients Doughnut Chart --%>
                        <div class="chart-container">
                            <canvas id="clientsDoughnutChartCanvas" runat="server" />
                        </div>
                    </div>
                    <div class="col-md-8">
                        <h4>Clients In Use</h4>
                        <Rock:Grid ID="gClientsInUse" runat="server" DisplayType="Light">
                            <Columns>
                                <Rock:RockBoundField DataField="ClientName" HeaderText="" />
                                <Rock:RockBoundField DataField="ClientPercent" HeaderText="" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
                <h3>Most Popular Links</h3>
                <table class="grid-table table table-bordered table-striped table-hover">
                    <thead>
                        <tr>
                            <th>Link</th>
                            <th>Uniques</th>
                            <th>CTR</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <p>http://mychurch.com/this/is/a/test</p>
                                <div class="progress">
                                    <div class="progress-bar" role="progressbar" aria-valuenow="100"
                                        aria-valuemin="0" aria-valuemax="100" style="width: 100%">
                                        <span class="sr-only">100%</span>
                                    </div>
                                </div>
                            </td>

                            <td>1985</td>
                            <td>20.7</td>
                        </tr>
                        <tr>
                            <td>
                                <p>http://mychurch.com/this/is/a/test2</p>
                                <div class="progress">
                                    <div class="progress-bar" role="progressbar" aria-valuenow="70"
                                        aria-valuemin="0" aria-valuemax="100" style="width: 70%">
                                        <span class="sr-only">70%</span>
                                    </div>
                                </div>
                            </td>

                            <td>1285</td>
                            <td>15.7</td>
                        </tr>
                        <tr>
                            <td>
                                <p>http://mychurch.com/this/is/a/test3</p>
                                <div class="progress">
                                    <div class="progress-bar" role="progressbar" aria-valuenow="70"
                                        aria-valuemin="0" aria-valuemax="100" style="width: 70%">
                                        <span class="sr-only">70%</span>
                                    </div>
                                </div>
                            </td>

                            <td>1285</td>
                            <td>15.7</td>
                        </tr>
                    </tbody>
                </table>
                <Rock:Grid ID="gMostPopularLinks" runat="server" DisplayType="Light" OnRowDataBound="gMostPopularLinks_RowDataBound">
                    <Columns>
                        <Rock:RockLiteralField ID="lLink" HeaderText="Link" />
                        <Rock:RockBoundField DataField="Uniques" HeaderText="Uniques" />
                        <Rock:RockBoundField DataField="CTR" HeaderText="CTR" />
                    </Columns>
                </Rock:Grid>

            </div>
        </div>



        <script>
            Sys.Application.add_load(function () {
                // Workaround for Chart.js not working in IE11 (supposed to be fixed in chart.js 2.7)
                // see https://github.com/chartjs/Chart.js/issues/4633
                Number.MAX_SAFE_INTEGER = Number.MAX_SAFE_INTEGER || 9007199254740991;
                Number.MIN_SAFE_INTEGER = Number.MIN_SAFE_INTEGER || -9007199254740991;

                var chartSeriesColors = [
                    "#8498ab",
                    "#a4b4c4",
                    "#b9c7d5",
                    "#c6d2df",
                    "#d8e1ea"
                ]

                debugger
                var chartDataLabels = <%=this.ChartDataLabelsJSON%>;
                var chartDataClicks = <%=this.ChartDataClicksJSON%>;
                var chartDataOpens = <%=this.ChartDataOpensJSON%>;

                // Main Linechart
                var linechartCtx = $('#<%=openClicksLineChartCanvas.ClientID%>')[0].getContext('2d');
                var clicksLineChart = new Chart(linechartCtx, {
                    type: 'line',
                    data: {
                        labels: chartDataLabels,
                        datasets: [{
                            type: 'line',
                            label: 'Opens',
                            backgroundColor: chartSeriesColors[0],
                            borderColor: chartSeriesColors[0],
                            data: chartDataOpens,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Clicks',
                            backgroundColor: chartSeriesColors[1],
                            borderColor: chartSeriesColors[1],
                            data: chartDataClicks,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Unopened',
                            backgroundColor: chartSeriesColors[2],
                            borderColor: chartSeriesColors[2],
                            data: [],
                            fill: false
                        }],
                    },
                    options: {
                        scales: {
                            xAxes: [{
                                type: 'time',
                                time: {
                                    unit: 'day',
                                    //round: 'week',
                                }
                            }]
                        }
                    }


                });

                // ClicksOpens Pie Chart
                var opensClicksPieChartCanvasCtx = $('#<%=opensClicksPieChartCanvas.ClientID%>')[0].getContext('2d');
                var opensClicksPieChart = new Chart(opensClicksPieChartCanvasCtx, {
                    type: 'pie',
                    options: {
                        legend: {
                            position: 'right'
                        }
                    },
                    data: {
                        labels: [
                            'Opens',
                            'Clicks',
                            'Unopened'
                        ],
                        datasets: [{
                            type: 'pie',
                            data: [
                                200,
                                110,
                                450
                            ],
                            backgroundColor: [
                                chartSeriesColors[0],
                                chartSeriesColors[1],
                                chartSeriesColors[2],
                            ]
                        }],
                    }
                });

                // Clients Doughnut Chart
                var clientsDoughnutChartCanvasCtx = $('#<%=clientsDoughnutChartCanvas.ClientID%>')[0].getContext('2d');
                var clientsDoughnutChart = new Chart(clientsDoughnutChartCanvasCtx, {
                    type: 'doughnut',
                    options: {
                        legend: {
                            position: 'right'
                        },
                        cutoutPercentage: 80
                    },
                    data: {
                        labels: [
                            'Mobile',
                            'Desktop',
                            'Web'
                        ],
                        datasets: [{
                            type: 'doughnut',
                            data: [
                                23,
                                54,
                                23
                            ],
                            backgroundColor: [
                                chartSeriesColors[0],
                                chartSeriesColors[1],
                                chartSeriesColors[2],
                            ]
                        }],
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
