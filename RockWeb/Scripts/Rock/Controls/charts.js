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
                        var lookupKey = chartData[i].MetricValuePartitionEntityIds;
                        if (!lookupKey || lookupKey == '') {
                            lookupKey = chartData[i].SeriesName;
                        }

                        if (!chartSeriesLookup[lookupKey]) {

                            // If SeriesName is specified, that can be the name of the series if MetricValuePartitionEntityIds is blank
                            var seriesName = chartData[i].SeriesName;

                            // if we aren't combining values, we'll have to lookup the series name based on MetricValuePartitionEntityIds
                            if (chartData[i].MetricValuePartitionEntityIds && chartData[i].MetricValuePartitionEntityIds != '' && !combineValues)
                            {
                                // MetricValuePartitionEntityIds is not blank so get the seriesName from the getSeriesPartitionNameUrl
                                if (getSeriesPartitionNameUrl) {
                                    var metricValuePartitionEntityIdDataJSON = JSON.stringify(chartData[i].MetricValuePartitionEntityIds.split(','));

                                    $.ajax({
                                        type: 'POST',
                                        url: getSeriesPartitionNameUrl,
                                        data: metricValuePartitionEntityIdDataJSON,
                                        async: false,
                                        success: null,
                                        contentType: 'application/json'
                                    })
                                    .done(function (data) {
                                        seriesName = data;
                                    });
                                }
                            }

                            // either either seriesName, yaxisLabelText or just 'value' if the seriesname isn't defined
                            seriesName = seriesName || yaxisLabelText || 'value';

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
            plotPieChartData: function (chartData, chartOptions, plotSelector, getSeriesPartitionNameUrl) {
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
            plotBarChartData: function (chartData, chartOptions, plotSelector, getSeriesPartitionNameUrl) {
                var barData = [];
                var combinedData = {};
                var seriesLabels = [];
                var seriesNameLookup = {};

                // populate the chartMeasurePoints data array with data from the REST result for bar chart data
                for (var i = 0; i < chartData.length; i++) {

                    var seriesCategory = chartData[i].SeriesName;
                    if (chartData[i].MetricValuePartitionEntityIds)
                    {
                        if (!seriesNameLookup[chartData[i].MetricValuePartitionEntityIds])
                        {
                            // MetricValuePartitionEntityIds is not blank so get the seriesName from the getSeriesPartitionNameUrl
                            if (getSeriesPartitionNameUrl) {
                                var metricValuePartitionEntityIdDataJSON = JSON.stringify(chartData[i].MetricValuePartitionEntityIds.split(','));

                                $.ajax({
                                    type: 'POST',
                                    url: getSeriesPartitionNameUrl,
                                    data: metricValuePartitionEntityIdDataJSON,
                                    async: false,
                                    success: null,
                                    contentType: 'application/json'
                                })
                                .done(function (data) {
                                    seriesNameLookup[chartData[i].MetricValuePartitionEntityIds] = data;
                                });
                            }
                        }

                        seriesCategory = seriesNameLookup[chartData[i].MetricValuePartitionEntityIds] || seriesCategory;
                    }

                    if (!combinedData[seriesCategory])
                    {
                        combinedData[seriesCategory] = 0;
                    }

                    combinedData[seriesCategory] += chartData[i].YValue;
                }

                var seriesId = 0;
                for (var combinedDataKey in combinedData) {

                    barData.push([combinedDataKey, combinedData[combinedDataKey]]);
                    seriesId++;
                    seriesLabels.push(combinedDataKey);
                }

                seriesLabels.push(seriesCategory);

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

                                if (item.series.chartData[item.dataIndex].MetricTitle) {
                                    tooltipText = item.series.chartData[item.dataIndex].MetricTitle;
                                }
                            }

                            if (tooltipText) {
                                tooltipText += '<br />';
                            }

                            if (item.series.label) {
                                tooltipText += item.series.label;
                            }

                            if (item.series.chartData) {
                                var pointValue = item.series.chartData[item.dataIndex].YValueFormatted || item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '';

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


