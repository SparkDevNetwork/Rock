<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SurveyResultCharts.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.SurveySystem.SurveyResultCharts" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorizedMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlResults" runat="server">
            <h2><asp:Literal ID="ltTitle" runat="server" /></h2>

            <Rock:AccordionPanel ID="pnlFilter" runat="server" Title="Filter" TitleIcon="fa fa-filter" Collapsed="true">
                <Body>
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <Rock:DateRangePicker ID="drpDateCompleted" runat="server" Label="Completed Date" />

                    <asp:PlaceHolder ID="phFilterControls" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSearch" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="lbApplyFilter_Click" />
                        <asp:LinkButton ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-link" OnClick="lbClearFilter_Click" />
                    </div>
                </Body>
            </Rock:AccordionPanel>
            
            <asp:HiddenField ID="hfData" runat="server" />
            <asp:HiddenField ID="hfMaxValues" runat="server" />

            <div class="js-questions-container"></div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        var json = $('#<%= hfData.ClientID %>').val();
        var data = JSON.parse(json);

        for (var i = 0; i < data.length; i++) {
            setupQuestion(data[i]);
        }

        /**
         * Setup a new panel for the specified question.
         * @param question The question to be displayed.
         */
        function setupQuestion(question) {
            var $panel = $('<div class="panel panel-block"><div class="panel-heading"><h2 class="panel-title"></h2><div class="pull-right" style="white-space: nowrap;"><span class="label label-success"></span> <a href="#" class="btn btn-xs btn-default js-btn-chart-bar"><i class="fa fa-chart-bar"></i></a> <a href="#" class="btn btn-xs btn-default js-btn-chart-horizontal-bar"><i class="fa fa-chart-bar fa-rotate-90"></i></a> <a href="#" class="btn btn-xs btn-default js-btn-chart-pie"><i class="fa fa-chart-pie"></i></a></div></div><div class="panel-body"></div></div>');

            $panel.find('.panel-title').text(question.title);
            if (question.passRate !== null) {
                $panel.find('.label-success').text((Math.round(question.passRate * 10000) / 100) + '% Correct');
            }

            $panel.find('.panel-body').append($('<div class="js-chart-container" style="position: relative; height: 300px;"></div>'));
            $panel.appendTo($('div.js-questions-container'));


            renderHorizontalBarChart(question, $panel.find('.js-chart-container'));

            $panel.find('.js-btn-chart-bar').on('click', function (e) {
                e.preventDefault();

                renderVerticalBarChart(question, $panel.find('.js-chart-container'));
            });

            $panel.find('.js-btn-chart-horizontal-bar').on('click', function (e) {
                e.preventDefault();

                renderHorizontalBarChart(question, $panel.find('.js-chart-container'));
            });

            $panel.find('.js-btn-chart-pie').on('click', function (e) {
                e.preventDefault();

                renderPieChart(question, $panel.find('.js-chart-container'));
            });
        }

        /**
         * Get an object that contains the color values for the given index.
         * @param index The index in the chart.
         */
        function getColor(index) {
            var colors = [
                '#54278f', '#41ab5d', '#d94801', '#4292c6',
                '#807dba', '#fd8d3c', '#2171b5', '#238b45',
                '#9ecae1', '#a1d99b', '#6a51a3', '#f16913',
                '#9e9ac8', '#fdae6b', '#6baed6', '#74c476',
                '#c7e9c0', '#fdd0a2', '#bcbddc', '#c6dbef',
            ];

            var color = colors[index % colors.length];

            m = color.match(/^#([0-9a-f]{6})$/i)[1];
            return {
                red: parseInt(m.substr(0, 2), 16),
                green: parseInt(m.substr(2, 2), 16),
                blue: parseInt(m.substr(4, 2), 16)
            };
        }

        /**
         * Get the chart data for the question specified.
         * @param question The question.
         */
        function getChartData(question) {
            var maxValues = parseInt($('#<%= hfMaxValues.ClientID %>').val());
            var data = {
                labels: [],
                datasets: [
                    {
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 2,
                        data: []
                    }
                ]
            };

            var index = 0;
            var otherValue = 0;
            for (var key in question.values) {
                if (index >= maxValues && key !== '') {
                    otherValue += question.values[key];
                    continue;
                }

                data.labels.push(key !== '' ? key : '[No Answer]');
                data.datasets[0].data.push(question.values[key]);

                var color = getColor(index)
                data.datasets[0].backgroundColor.push('rgb(' + color.red + ',' + color.green + ',' + color.blue + ',0.6)');
                data.datasets[0].borderColor.push('rgb(' + color.red + ',' + color.green + ',' + color.blue + ',1)');

                index += 1;
            }

            if (otherValue > 0) {
                data.labels.push('[Other]');
                data.datasets[0].data.push(otherValue);

                var color = getColor(index)
                data.datasets[0].backgroundColor.push('rgb(' + color.red + ',' + color.green + ',' + color.blue + ',0.6)');
                data.datasets[0].borderColor.push('rgb(' + color.red + ',' + color.green + ',' + color.blue + ',1)');

                index += 1;
            }

            return data;
        }

        /**
         * Get the standard ChartJS options to be used by all chart types.
         */
        function getBaseOptions() {
            return {
                animation: {
                    duration: 1000
                },
                legend: {
                    display: false
                },
                responsive: true,
                maintainAspectRatio: false
            };
        }

        /**
         * Renders a "no data" alert in the container.
         * @param $container
         */
        function renderNoData($container) {
            $container.append($('<div class="alert alert-warning">No data</div>'));
            $container.css('height', '');
        }

        /**
         * Render a vertical bar chart into the canvas.
         * @param question The question to be rendered.
         * @param $container The chart container.
         */
        function renderVerticalBarChart(question, $container) {
            var data = getChartData(question);

            if (data.labels.length === 0) {
                renderNoData($container);
                return;
            }

            var options = getBaseOptions();
            options.scales = {
                yAxes: [
                    {
                        ticks: {
                            beginAtZero: true
                        }
                    }
                ]
            };

            $container.html('<canvas width="100%"></canvas>');
            $container.css('height', '300px');

            new Chart($container.find('canvas').get(0).getContext("2d"), {
                type: 'bar',
                data: data,
                options: options
            });
        }

        /**
         * Render a horizontal bar chart into the canvas.
         * @param question The question to be rendered.
         * @param $container The chart container.
         */
        function renderHorizontalBarChart(question, $container) {
            var data = getChartData(question);

            if (data.labels.length === 0) {
                renderNoData($container);
                return;
            }

            var options = getBaseOptions();
            options.scales = {
                xAxes: [
                    {
                        ticks: {
                            beginAtZero: true
                        }
                    }
                ]
            };

            $container.html('<canvas width="100%"></canvas>');
            $container.css('height', (data.labels.length * 50) + 25 + 'px');

            new Chart($container.find('canvas').get(0).getContext("2d"), {
                type: 'horizontalBar',
                data: data,
                options: options
            });
        }

        /**
         * Render a pie chart into the canvas.
         * @param question The question to be rendered.
         * @param $container The chart container.
         */
        function renderPieChart(question, $container) {
            var data = getChartData(question);

            if (data.labels.length === 0) {
                renderNoData($container);
                return;
            }

            var sortedData = [];
            for (index = 0; index < data.labels.length; index++) {
                sortedData.push({
                    label: data.labels[index],
                    backgroundColor: data.datasets[0].backgroundColor[index],
                    borderColor: data.datasets[0].borderColor[index],
                    data: data.datasets[0].data[index]
                });
            }

            sortedData.sort(function (a, b) {
                if (a.data > b.data) {
                    return 1;
                }
                else if (b.data > a.data) {
                    return -1;
                }
                else {
                    return 0;
                }
            }).reverse();

            data.labels = sortedData.map(function (a) { return a.label });
            data.datasets[0].backgroundColor = sortedData.map(function (a) { return a.backgroundColor; });
            data.datasets[0].borderColor = sortedData.map(function (a) { return a.borderColor; });
            data.datasets[0].data = sortedData.map(function (a) { return a.data; });


            var options = getBaseOptions();
            options.legend.display = true;
            options.legend.position = 'left';

            $container.html('<canvas width="100%"></canvas>');
            $container.css('height', '300px');

            new Chart($container.find('canvas').get(0).getContext("2d"), {
                type: 'pie',
                data: data,
                options: options
            });
        }
    });
</script>