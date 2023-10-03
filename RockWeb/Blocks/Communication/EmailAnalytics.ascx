<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailAnalytics" %>

<style>
    .progress-bar-link {
        background-color: #60BD68;
    }

    .label-clicked {
        background-color: #60bd68;
    }

    .label-opened {
        background-color: #5da5da;
    }

    .label-unopened {
        background-color: #ffb70f;
    }
</style>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfCommunicationListGroupId" runat="server" />

        <asp:HiddenField ID="hfLineChartDataLabelsJSON" runat="server" />
        <asp:HiddenField ID="hfLineChartDataOpensJSON" runat="server" />
        <asp:HiddenField ID="hfLineChartDataClicksJSON" runat="server" />
        <asp:HiddenField ID="hfLineChartDataUnOpenedJSON" runat="server" />
        <asp:HiddenField ID="hfPieChartDataOpenClicksJSON" runat="server" />
        <asp:HiddenField ID="hfPieChartDataClientLabelsJSON" runat="server" />
        <asp:HiddenField ID="hfPieChartDataClientCountsJSON" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-line-chart"></i>&nbsp;<asp:Literal ID="lTitle" runat="server" Text="Email Analytics" /></h1>

                <div class="panel-labels">

                    <asp:HiddenField ID="hfSelectedMonthsDateRange" runat="server" />
                    <%-- If not specific communication(s) were selected, limit the date range --%>
                    <asp:Panel runat="server" ID="pnlSelectedMonthsDateRange" class="btn-group panel-toggle pull-right" Visible="false">
                        <asp:LinkButton ID="btnDateRangeOneMonth" CssClass="btn btn-xs btn-primary" runat="server" Text="One Month" OnClick="btnDateRange_Click" />
                        <asp:LinkButton ID="btnDateRangeThreeMonths" CssClass="btn btn-xs btn-primary" runat="server" Text="Three Months" OnClick="btnDateRange_Click" />
                        <asp:LinkButton ID="btnDateRangeSixMonths" CssClass="btn btn-xs btn-primary" runat="server" Text="Six Months" OnClick="btnDateRange_Click" />
                    </asp:Panel>
                </div>
            </div>

            <Rock:NotificationBox ID="nbWarningMessage" runat="server" CssClass="margin-all-md" NotificationBoxType="Warning" Visible="false" />

            <asp:Panel ID="pnlCharts" runat="server" class="panel-body">
                <%-- Main Opens/Clicks Line Chart --%>
                <div class="chart-container">
                    <Rock:NotificationBox ID="nbOpenClicksLineChartMessage" runat="server" CssClass="my-4" NotificationBoxType="Info" Text="No Communication Activity" />
                    <canvas id="openClicksLineChartCanvas" runat="server" style="height: 450px;" />
                </div>

                <hr />
                <h1 class="text-center">Actions</h1>
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
                            <div class="col-sm-4">
                                <Rock:RockLiteral ID="lDelivered" runat="server" />
                                <Rock:RockLiteral ID="lUnopened" runat="server" />
                                <Rock:RockLiteral ID="lFailedRecipients" runat="server" />
                            </div>
                            <div class="col-sm-4">
                                <Rock:RockLiteral ID="lUniqueOpens" runat="server" />
                                <Rock:RockLiteral ID="lTotalOpens" runat="server" />
                                <Rock:RockLiteral ID="lPercentOpened" runat="server" />
                            </div>
                            <div class="col-sm-4">
                                <Rock:RockLiteral ID="lUniqueClicks" runat="server" />
                                <Rock:RockLiteral ID="lTotalClicks" runat="server" />
                                <Rock:RockLiteral ID="lClickThroughRate" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>

                <hr />
                <h1 class="text-center">Clients</h1>

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
                    <hr />
                    <h1 class="text-center">Popular Links</h1>

                    <div class="row hidden-xs">
                        <div class="col-sm-10"><strong>URL</strong></div>
                        <div class="col-sm-1"><strong>Uniques</strong></div>
                        <div id="pnlCTRHeader" runat="server" class="col-sm-1"><strong>CTR</strong></div>
                    </div>
                    <asp:Repeater ID="rptMostPopularLinks" runat="server" OnItemDataBound="rptMostPopularLinks_ItemDataBound">
                        <ItemTemplate>
                            <div class="row margin-b-lg">
                                <div class="col-sm-10 col-xs-12">
                                    <p>
                                        <asp:Literal ID="lUrl" runat="server" />
                                    </p>
                                    <asp:Literal ID="lUrlProgressHTML" runat="server" />
                                </div>
                                <div class="col-sm-1 col-xs-6">
                                    <label class="visible-xs margin-r-sm pull-left">Uniques:</label><asp:Literal ID="lUniquesCount" runat="server" />
                                </div>
                                <div id="pnlCTRData" runat="server" class="col-sm-1 col-xs-6">
                                    <label class="visible-xs margin-r-sm pull-left">CTR:</label><asp:Literal ID="lCTRPercent" runat="server" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

            </asp:Panel>
        </div>

        <script>
            Sys.Application.add_load(function () {

                var chartSeriesColors = <%=this.SeriesColorsJSON%>;

                var getSeriesColors = function (numberOfColors) {

                    var result = chartSeriesColors;
                    while (result.length < numberOfColors) {
                        result = result.concat(chartSeriesColors);
                    }

                    return result;
                };

                // Main Linechart
                var lineChartDataLabels = eval($('#<%=hfLineChartDataLabelsJSON.ClientID%>').val());
                var lineChartDataOpens = JSON.parse($('#<%=hfLineChartDataOpensJSON.ClientID%>').val());
                var lineChartDataClicks = JSON.parse($('#<%=hfLineChartDataClicksJSON.ClientID%>').val());
                var lineChartDataUnopened = JSON.parse($('#<%=hfLineChartDataUnOpenedJSON.ClientID%>').val());

                var linechartCtx = $('#<%=openClicksLineChartCanvas.ClientID%>')[0].getContext('2d');

                var clicksLineChart = new Chart(linechartCtx, {
                    type: 'line',
                    data: {
                        labels: lineChartDataLabels,
                        datasets: [{
                            type: 'line',
                            label: 'Opens',
                            backgroundColor: '#5DA5DA',
                            borderColor: '#5DA5DA',
                            data: lineChartDataOpens,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Clicks',
                            backgroundColor: '#60BD68',
                            borderColor: '#60BD68',
                            data: lineChartDataClicks,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Unopened',
                            backgroundColor: '#FFBF2F',
                            borderColor: '#FFBF2F',
                            data: lineChartDataUnopened,
                            hidden: lineChartDataUnopened == null,
                            spanGaps: true,
                            fill: false
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'bottom',
                            labels: {
                                filter: function (item, data) {
                                    // don't include the label if the dataset is hidden
                                    if (data.datasets[item.datasetIndex].hidden) {
                                        return false;
                                    }

                                    return true;
                                }
                            }
                        },
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
                var pieChartDataOpenClicks = JSON.parse($('#<%=hfPieChartDataOpenClicksJSON.ClientID%>').val());

                var opensClicksPieChartCanvasCtx = $('#<%=opensClicksPieChartCanvas.ClientID%>')[0].getContext('2d');
                var opensClicksPieChart = new Chart(opensClicksPieChartCanvasCtx, {
                    type: 'pie',
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'right',
                            labels: {
                                filter: function (item, data) {
                                    // don't include the label if the dataset isn't defined
                                    if (data.datasets[0].data[item.index] == null) {
                                        return false;
                                    }

                                    return true;
                                }
                            }
                        }
                    },
                    data: {
                        labels: [
                            'Opens',
                            'Clicked',
                            'Unopened'
                        ],
                        datasets: [{
                            type: 'pie',
                            data: pieChartDataOpenClicks,
                            backgroundColor: ['#5DA5DA', '#60BD68', '#FFBF2F'],
                        }],
                    }
                });

                // Clients Doughnut Chart
                var pieChartDataClientCounts = JSON.parse($('#<%=hfPieChartDataClientCountsJSON.ClientID%>').val());
                var pieChartDataClientLabels = JSON.parse($('#<%=hfPieChartDataClientLabelsJSON.ClientID%>').val());

                var clientsDoughnutChartCanvasCtx = $('#<%=clientsDoughnutChartCanvas.ClientID%>')[0].getContext('2d');
                var clientsDoughnutChart = new Chart(clientsDoughnutChartCanvasCtx, {
                    type: 'doughnut',
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'right'
                        },
                        cutoutPercentage: 50,
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem, data) {
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
                            backgroundColor: getSeriesColors(pieChartDataClientCounts.length)
                        }],
                    }
                });

                // Tooltips
                $('.js-actions-statistic').tooltip();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
