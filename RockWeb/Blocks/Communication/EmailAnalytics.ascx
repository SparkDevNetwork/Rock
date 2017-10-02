﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailAnalytics" %>

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
                        <Rock:NotificationBox ID="nbCommunicationorCommunicationListFound" runat="server" NotificationBoxType="Warning" Text="Invalid Communication or CommunicationList Specified" Visible="false" />

                        <%-- Main Opens/Clicks Line Chart --%>
                        <div class="chart-container">
                            <Rock:NotificationBox ID="nbOpenClicksLineChartMessage" runat="server" NotificationBoxType="Info" Text="No Communication Activity" />
                            <canvas id="openClicksLineChartCanvas" runat="server" />
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <%-- Opens/Clicks PieChart --%>
                        <div class="chart-container">
                            <Rock:NotificationBox ID="nbOpenClicksPieChartMessage" runat="server" NotificationBoxType="Info" Text="No Communication Activity" />
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
                            <Rock:NotificationBox ID="nbClientsDoughnutChartMessage" runat="server" NotificationBoxType="Info" Text="No Client Communication Activity" />
                            <canvas id="clientsDoughnutChartCanvas" runat="server" />
                        </div>
                    </div>
                    <div class="col-md-8">
                        <asp:Panel ID="pnlClientApplicationUsage" runat="server">
                            <h4>Clients In Use</h4>
                            <asp:Repeater ID="rptClientApplicationUsage" runat="server" OnItemDataBound="rptClientApplicationUsage_ItemDataBound">
                                <ItemTemplate>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <asp:Literal ID="lApplicationName" runat="server" />
                                        </div>
                                        <div class="col-md-6">
                                            <asp:Literal ID="lUsagePercent" runat="server" />
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                    </div>
                </div>

                <asp:Panel ID="pnlMostPopularLinks" runat="server">
                    <h3>Most Popular Links</h3>
                    <div class="row">
                        <div class="col-md-10">Url</div>
                        <div class="col-md-1">Uniques</div>
                        <div class="col-md-1">CTR</div>
                    </div>
                    <asp:Repeater ID="rptMostPopularLinks" runat="server" OnItemDataBound="rptMostPopularLinks_ItemDataBound">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-md-10">
                                    <p>
                                        <asp:Literal ID="lUrl" runat="server" />
                                    </p>
                                    <asp:Literal ID="lUrlProgressHTML" runat="server" />
                                </div>
                                <div class="col-md-1">
                                    <asp:Literal ID="lUniquesCount" runat="server" />
                                </div>
                                <div class="col-md-1">
                                    <asp:Literal ID="lCTRPercent" runat="server" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

            </div>
        </div>

        <script>
            Sys.Application.add_load(function () {
                // Workaround for Chart.js not working in IE11 (supposed to be fixed in chart.js 2.7)
                // see https://github.com/chartjs/Chart.js/issues/4633
                Number.MAX_SAFE_INTEGER = Number.MAX_SAFE_INTEGER || 9007199254740991;
                Number.MIN_SAFE_INTEGER = Number.MIN_SAFE_INTEGER || -9007199254740991;
                
                var chartSeriesColors = <%=this.SeriesColorsJSON%>;

                var getSeriesColors = function(numberOfColors) {

                    var result = chartSeriesColors;
                    while (result.length < numberOfColors)
                    {
                        result = result.concat(chartSeriesColors);
                    }

                    return result;
                };

                // Main Linechart
                var lineChartDataLabels = <%=this.LineChartDataLabelsJSON%>;
                var lineChartDataOpens = <%=this.LineChartDataOpensJSON%>;
                var lineChartDataClicks = <%=this.LineChartDataClicksJSON%>;
                var lineChartDataUnopened = <%=this.LineChartDataUnOpenedJSON%>;

                var linechartCtx = $('#<%=openClicksLineChartCanvas.ClientID%>')[0].getContext('2d');
                var clicksLineChart = new Chart(linechartCtx, {
                    type: 'line',
                    data: {
                        labels: lineChartDataLabels,
                        datasets: [{
                            type: 'line',
                            label: 'Opens',
                            backgroundColor: chartSeriesColors[0],
                            borderColor: chartSeriesColors[0],
                            data: lineChartDataOpens,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Clicks',
                            backgroundColor: chartSeriesColors[1],
                            borderColor: chartSeriesColors[1],
                            data: lineChartDataClicks,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Unopened',
                            backgroundColor: chartSeriesColors[2],
                            borderColor: chartSeriesColors[2],
                            data: lineChartDataUnopened,
                            spanGaps: true,
                            fill: false
                        }],
                    },
                    options: {
                        scales: {
                            xAxes: [{
                                type: 'time',
                                time: {
                                    unit: false,
                                    tooltipFormat: '<%=this.LineChartTimeFormat%>',
                                }
                            }]
                        }
                    }
                });

                // ClicksOpens Pie Chart
                var pieChartDataOpenClicks = <%=this.PieChartDataOpenClicksJSON%>;

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
                            data: pieChartDataOpenClicks,
                            backgroundColor: getSeriesColors(pieChartDataOpenClicks.length),
                        }],
                    }
                });

                // Clients Doughnut Chart
                var pieChartDataClientCounts = <%=this.PieChartDataClientCountsJSON%>;
                var pieChartDataClientLabels = <%=this.PieChartDataClientLabelsJSON%>;

                var clientsDoughnutChartCanvasCtx = $('#<%=clientsDoughnutChartCanvas.ClientID%>')[0].getContext('2d');
                var clientsDoughnutChart = new Chart(clientsDoughnutChartCanvasCtx, {
                    type: 'doughnut',
                    options: {
                        legend: {
                            position: 'right'
                        },
                        cutoutPercentage: 80,
                        tooltips: {
                            callbacks: {
                                label: function(tooltipItem, data) {
                                    var dataValue = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                    var labelText = data.labels[tooltipItem.index];
                                    return labelText + ": " + dataValue + "%";
                                }
                            }
                        }
                    },
                    data: {
                        labels: pieChartDataClientLabels,
                        datasets: [{
                            type: 'doughnut',
                            data: pieChartDataClientCounts,
                            backgroundColor:getSeriesColors(pieChartDataClientCounts.length)
                        }],
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
