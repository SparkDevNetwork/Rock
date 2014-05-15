(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.controls.charts = (function () {
        var exports = {

            ///
            /// handles putting chartData into a Line/Bar/Points chart
            ///
            plotChartData: function (chartData, chartOptions, plotSelector, xaxisLabelText, getSeriesUrl, combineValues) {

                var chartSeriesLookup = {};
                var chartSeriesList = [];

                var chartGoalPoints = {
                    label: xaxisLabelText + ' goal',
                    chartData: [],
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
                        chartGoalPoints.chartData.push(chartData[i]);
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
                                });
                            }

                            seriesName = seriesName || xaxisLabelText + '(seriesId:' + chartData[i].SeriesId + ')';

                            chartSeriesLookup[chartData[i].SeriesId] = {
                                label: seriesName,
                                chartData: [],
                                data: []
                            };
                        }

                        chartSeriesLookup[chartData[i].SeriesId].data.push([chartData[i].DateTimeStamp, chartData[i].YValue]);
                        chartSeriesLookup[chartData[i].SeriesId].chartData.push(chartData[i]);
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
                    var chartMeasurePoints = null;

                    for (var chartMeasurePointsId in chartSeriesList) {

                        chartMeasurePoints = chartSeriesList[chartMeasurePointsId];

                        $.each(chartMeasurePoints.data, function (indexInArray, dataPair) {
                            var dateTimeIndex = dataPair[0];
                            var combinedItem = combinedDataLookup[dateTimeIndex];
                            if (combinedItem) {
                                // sum the YValue of the .data
                                combinedItem.chartData.YValue += chartMeasurePoints.chartData[indexInArray].YValue;
                                combinedItem.data[1] += dataPair[1];
                            }
                            else {
                                combinedDataLookup[dateTimeIndex] = {
                                    chartData: chartMeasurePoints.chartData[indexInArray],
                                    data: dataPair
                                };
                            }
                        });
                    }

                    var combinedData = [];
                    var combinedChartData = [];

                    for (var dataLookupId in combinedDataLookup) {
                        combinedData.push(combinedDataLookup[dataLookupId].data);
                        combinedChartData.push(combinedDataLookup[dataLookupId].chartData);
                    }

                    chartSeriesList = [];
                    var chartCombinedMeasurePoints = {
                        label: xaxisLabelText + ' combined',
                        chartData: combinedChartData,
                        data: combinedData
                    };


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

            ///
            /// handles putting chart data into a piechart
            ///
            plotPieChartData: function (chartData, chartOptions, plotSelector, combineValues) {
                var workingChartData = null;
                var pieData = [];
                if (combineValues) {
                    var combinedChartDataLookup = {};

                    $.each(chartData, function (indexInArray, chartDataRow) {
                        var combinedDataIndex = chartDataRow.XValue + '|' + chartDataRow.DateTimeStamp;
                        var combinedChartDataRow = combinedChartDataLookup[combinedDataIndex];
                        if (combinedChartDataRow) {

                            // inc YValue for combined row
                            combinedChartDataRow.YValue += chartDataRow.YValue;

                            // take first non-null Note for the combined data row
                            combinedChartDataRow.Note = combinedChartDataRow.Note || chartDataRow.Note;
                        }
                        else {
                            combinedChartDataRow = chartDataRow;
                            combinedChartDataLookup[combinedDataIndex] = combinedChartDataRow;
                        }
                    });

                    var combinedChartData = [];

                    for (var lookupId in combinedChartDataLookup) {
                        var combinedChartDataItem = combinedChartDataLookup[lookupId];
                        combinedChartData.push(combinedChartDataItem);
                    }

                    workingChartData = combinedChartData;
                }
                else {
                    workingChartData = chartData;
                }

                // populate the chartMeasurePoints data array with data from the REST result for pie data
                for (var i = 0; i < workingChartData.length; i++) {
                    var pieSeriesLabel = workingChartData[i].Note || workingChartData[i].XValue;
                    if (!pieSeriesLabel || pieSeriesLabel == "0") {
                        pieSeriesLabel = new Date(workingChartData[i].DateTimeStamp).toLocaleDateString();
                    }

                    pieData.push({
                        label: pieSeriesLabel,
                        data: workingChartData[i].YValue,
                        chartData: [workingChartData[i]]
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

            ///
            /// attaches a bootstrap style tooltip to the 'plothover' of a chart
            ///
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
                        var tipTop = pos.pageY - ($toolTip.height() / 2);
                        $toolTip.css({ top: tipTop, left: pos.pageX + 5, opacity: 1 });
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


