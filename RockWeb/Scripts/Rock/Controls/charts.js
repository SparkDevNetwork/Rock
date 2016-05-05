(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.controls.charts = (function () {
        var exports = {

            ///
            /// handles putting chartData into a Line/Bar/Points chart
            ///
            plotChartData: function (chartData, chartOptions, plotSelector, yaxisLabelText, getSeriesPartitionNameUrl, combineValues) {

                var chartSeriesLookup = {};
                var chartSeriesList = [];

                var chartGoalPoints = {
                    label: yaxisLabelText + ' Goal',
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
                        var lookupKey = chartData[i].MetricValuePartitionIds;
                        if (!lookupKey || lookupKey == '') {
                            lookupKey = chartData[i].SeriesName;
                        }
                        
                        if (!chartSeriesLookup[lookupKey]) {

                            // If SeriesName is specified, that can be the name of the series if MetricValuePartitionIds is blank
                            var seriesName = chartData[i].SeriesName;
                            if (chartData[i].MetricValuePartitionIds && chartData[i].MetricValuePartitionIds != '')
                            {
                                // MetricValuePartitionIds is not blank so get the seriesName from the getSeriesPartitionNameUrl
                                if (getSeriesPartitionNameUrl) {
                                    $.ajax({
                                        url: getSeriesPartitionNameUrl + chartData[i].MetricValuePartitionIds,
                                        async: false
                                    })
                                    .done(function (data) {
                                        seriesName = data;
                                    });
                                }
                            }

                            // if we weren't able to determine the seriesName for some reason, 
                            // this could happen if there is no longer a record of the entity (Campus, Group, etc) with that value or if the getSeriesPartitionNameUrl failed
                            seriesName = seriesName || yaxisLabelText || 'null';

                            chartSeriesLookup[lookupKey] = {
                                label: seriesName,
                                chartData: [],
                                data: []
                            };
                        }

                        chartSeriesLookup[lookupKey].data.push([chartData[i].DateTimeStamp, chartData[i].YValue]);
                        chartSeriesLookup[lookupKey].chartData.push(chartData[i]);
                    }
                }

                // setup the series list (goal points last, if they exist)
                for (var chartSeriesKey in chartSeriesLookup) {
                    var chartMeasurePoints = chartSeriesLookup[chartSeriesKey];
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

                    // sort data after combining
                    combinedChartData.sort(function (item1, item2) { return item2.DateTimeStamp - item1.DateTimeStamp });
                    combinedData.sort(function (item1, item2) { return item2[0] - item1[0]; });

                    var chartCombinedMeasurePoints = {
                        label: yaxisLabelText + ' Total',
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
            plotPieChartData: function (chartData, chartOptions, plotSelector) {
                var pieData = [];

                // populate the chartMeasurePoints data array with data from the REST result for pie data
                for (var i = 0; i < chartData.length; i++) {

                    pieData.push({
                        label: chartData[i].MetricTitle,
                        data: chartData[i].YValueTotal,
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

            ///
            /// handles putting chart data into a bar chart
            ///
            plotBarChartData: function (chartData, chartOptions, plotSelector) {
                var barData = [];
                var seriesLabels = [];

                // populate the chartMeasurePoints data array with data from the REST result for pie data
                for (var i = 0; i < chartData.length; i++) {

                    var seriesCategory = chartData[i].MetricValuePartitionIds || chartData[i].SeriesName;
                    barData.push([seriesCategory, chartData[i].YValue])
                    seriesLabels.push(seriesCategory);
                }

                if (barData.length > 0) {
                    // plot the bar chart
                    chartOptions.series.chartData = chartData;
                    chartOptions.series.labels = seriesLabels;
                    $.plot(plotSelector, [barData], chartOptions);
                }
                else {
                    $(plotSelector).html('<div class="alert alert-info">No Data Found</div>');
                }
            },

            ///
            /// attaches a bootstrap style tooltip to the 'plothover' of a chart
            ///
            bindTooltip: function (plotContainerId, tooltipFormatter) {
                // setup of bootstrap tooltip which we'll show on the plothover event
                var toolTipId = 'tooltip_' + plotContainerId;
                var chartContainer = '#' + plotContainerId;

                $("<div id=" + toolTipId + " class='tooltip top'><div class='tooltip-inner'></div><div class='tooltip-arrow'></div></div>").css({
                    position: 'absolute',
                    display: 'none',
                }).appendTo('body');

                var $toolTip = $('#' + toolTipId);

                $(chartContainer).bind('plothover', function (event, pos, item) {

                    if (item) {

                        var tooltipText = "";

                        // if a tooltipFormatter is specified, use that
                        if (tooltipFormatter) {
                            tooltipText = tooltipFormatter(item);
                        }
                        else {
                            if (item.series.chartData) {
                                if (item.series.chartData[item.dataIndex].DateTimeStamp) {
                                    tooltipText = new Date(item.series.chartData[item.dataIndex].DateTimeStamp).toLocaleDateString();
                                };

                                if (item.series.chartData[item.dataIndex].StartDateTimeStamp) {
                                    tooltipText = new Date(item.series.chartData[item.dataIndex].StartDateTimeStamp).toLocaleDateString();
                                }

                                if (item.series.chartData[item.dataIndex].EndDateTimeStamp) {
                                    tooltipText += " to " + new Date(item.series.chartData[item.dataIndex].EndDateTimeStamp).toLocaleDateString();
                                }
                            }

                            if (tooltipText) {
                                tooltipText += '<br />';
                            }

                            if (item.series.label) {
                                tooltipText += item.series.label;
                            }

                            if (item.series.chartData) {
                                var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '';

                                tooltipText += ': ' + pointValue;

                                if (item.series.chartData[item.dataIndex].Note) {
                                    tooltipText += '<br />' + item.series.chartData[item.dataIndex].Note;
                                }
                            }
                        }

                        $toolTip.find('.tooltip-inner').html(tooltipText);
                        
                        var tipTop = pos.pageY - $toolTip.height() - 10;

                        var windowWidth = $(window).width();
                        var tooltipWidth = $toolTip.width();
                        var tipLeft = pos.pageX - ( tooltipWidth / 2);
                        if (tipLeft + tooltipWidth + 10 >= windowWidth) {
                            tipLeft = tipLeft - (tooltipWidth / 2);
                            $toolTip.removeClass("top");
                            $toolTip.addClass("left");
                        }
                        else {
                            $toolTip.removeClass("left");
                            $toolTip.addClass("top");
                        }

                        $toolTip.css({ top: tipTop, left: tipLeft, opacity: 1 });
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


