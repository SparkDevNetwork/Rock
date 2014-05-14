(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.controls.charts = (function () {
        var exports = {
            plotChartData: function (chartData, chartOptions, plotSelector, xaxisLabelText, getSeriesUrl, combineValues) {

                var chartSeriesLookup = {};
                var chartSeriesList = [];

                var chartGoalPoints = {
                    label: xaxisLabelText + ' goal',
                    chartData: chartData,
                    data: []
                }

                if (chartOptions.customSettings) {
                    chartGoalPoints.color = chartOptions.customSettings.goalSeriesColor;
                }

                // populate the chartMeasurePoints data array with data from the REST result
                for (var i = 0; i < chartData.length; i++) {
                    if (chartData[i].MetricValueType && chartData[i].MetricValueType == 1) {
                        // if the chartdata is marked as a Metric Goal Value Type, populate the chartGoalPoints
                        chartGoalPoints.data.push([chartData[i].DateTimeStamp, chartData[i].YValue]);
                    }
                    else {
                        if (!chartSeriesLookup[chartData[i].SeriesId]) {

                            var seriesName = chartData[i].SeriesId;

                            if (getSeriesUrl) {
                                $.ajax({
                                    url: getSeriesUrl + chartData[i].SeriesId,
                                    async: false
                                })
                                .done(function (data) {
                                    seriesName = data;
                                })
                                .fail(function (jqXHR, textStatus, errorThrown) {
                                    //debugger
                                });
                            }

                            seriesName = seriesName || xaxisLabelText;

                            chartSeriesLookup[chartData[i].SeriesId] = {
                                label: seriesName,
                                chartData: chartData,
                                data: []
                            };
                        }

                        chartSeriesLookup[chartData[i].SeriesId].data.push([chartData[i].DateTimeStamp, chartData[i].YValue]);
                    }
                }

                // setup the series list (goal points last, if they exist)
                for (var seriesId in chartSeriesLookup) {

                    var chartMeasurePoints = chartSeriesLookup[seriesId];
                    if (chartMeasurePoints.data.length) {
                        chartSeriesList.push(chartMeasurePoints);
                    }
                }

                if (combineValues && chartSeriesList.length != 1) {
                    var combinedDataLookup = {};
                    for (var chartMeasurePointsId in chartSeriesList) {
                        var chartMeasurePoints = chartSeriesList[chartMeasurePointsId];
                        $.each(chartMeasurePoints.data, function (indexInArray, dataPair) {
                            var combinedDataPair = combinedDataLookup[dataPair[0]];
                            if (combinedDataPair) {
                                combinedDataPair[1] += dataPair[1];
                            }
                            else {
                                combinedDataPair = dataPair;
                                combinedDataLookup[dataPair[0]] = combinedDataPair;
                            }
                        });
                    }

                    chartSeriesList = [];
                    var chartCombinedMeasurePoints = {
                        label: xaxisLabelText + ' combined',
                        chartData: chartData,
                        data: []
                    };

                    for (var dataPairId in combinedDataLookup) {
                        var dataPair = combinedDataLookup[dataPairId];
                        chartCombinedMeasurePoints.data.push(dataPair);
                    }

                    chartSeriesList.push(chartCombinedMeasurePoints);
                }

                if (chartGoalPoints.data.length) {
                    chartSeriesList.push(chartGoalPoints);
                }

                // plot the chart
                if (chartSeriesList.length > 0) {
                    $.plot(plotSelector, chartSeriesList, chartOptions);
                }
                else {
                    $(plotSelector).html('<div class="alert alert-info">No Data Found</div>');
                }
            },

            plotPieChartData: function (chartData, chartOptions, plotSelector) {
                var pieData = [];
                // populate the chartMeasurePoints data array with data from the REST result for pie data
                for (var i = 0; i < chartData.length; i++) {
                    pieData.push({
                        label: chartData[i].Note,
                        data: chartData[i].YValue,
                        chartData: [chartData[i]]
                    });
                }

                if (pieData.length > 0) {
                    // plot the pie chart
                    $.plot(plotSelector, pieData, chartOptions);
                }
                else {
                    $(plotSelector).html('<div class="alert alert-info">No Data Found</div>');
                }
            },

            bindTooltip: function (plotContainerId) {
                // setup of bootstrap tooltip which we'll show on the plothover event
                var toolTipId = 'tooltip_' + plotContainerId;
                var chartContainer = '#' + plotContainerId;

                $("<div id=" + toolTipId + " class='tooltip right'><div class='tooltip-inner'></div><div class='tooltip-arrow'></div></div>").css({
                    position: 'absolute',
                    display: 'none',
                }).appendTo('body');

                var $toolTip = $('#' + toolTipId);

                $(chartContainer).bind('plothover', function (event, pos, item) {

                    if (item) {
                        var yaxisLabel = $(chartContainer).find('.js-yaxis-value').val();
                        var tooltipText = '';

                        if (yaxisLabel) {
                            tooltipText += yaxisLabel + '<br />';
                        }

                        tooltipText += new Date(item.series.chartData[item.dataIndex].DateTimeStamp).toLocaleDateString();

                        if (item.series.label) {
                            tooltipText += '<br />' + item.series.label;
                        }

                        if (item.series.chartData[item.dataIndex].MetricValueType) {
                            // if it's a goal measure...
                            tooltipText += ' goal';
                        }

                        tooltipText += ': ' + item.series.chartData[item.dataIndex].YValue;
                        $toolTip.find('.tooltip-inner').html(tooltipText);
                        $toolTip.css({ top: item.pageY + 5, left: item.pageX + 5, opacity: 1 });
                        $toolTip.show();
                    }
                    else {
                        $toolTip.hide();
                    }
                });

            }
        };

        return exports;
    }());
}(jQuery));


