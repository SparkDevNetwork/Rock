// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Chart;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A control that displays a chart showing data values represented as data points on a line.
    /// </summary>
    public class RockLineChart : ChartJsChart
    {
        /// <inheritdoc/>
        protected override string OnGenerateChartJson( object dataSource )
        {
            var chartDataJson = string.Empty;
            if ( dataSource is IEnumerable<IChartData> chartData )
            {
                chartDataJson = GetFromChartDataItems( chartData );
            }
            else if ( dataSource is List<ChartJsTimeSeriesDataset> datasets )
            {
                var chartFactory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

                chartFactory.Datasets = datasets;
                chartFactory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetTimeChartDataFactoryArgs();
                chartDataJson = chartFactory.GetJson( args );
            }
            else if ( dataSource is List<ChartJsCategorySeriesDataset> categoryDatasets )
            {
                var chartFactory = new ChartJsCategorySeriesDataFactory<ChartJsCategorySeriesDataPoint>();

                chartFactory.Datasets = categoryDatasets;
                chartFactory.ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Line;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = new ChartJsCategorySeriesDataFactory.GetJsonArgs();
                chartDataJson = chartFactory.GetJson( args );
            }

            return chartDataJson;
        }

        private string GetFromChartDataItems( IEnumerable<IChartData> chartDataItems )
        {
            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );

            // Determine the chart interval type.
            var intervalType = this.SeriesGroupIntervalType.ToStringSafe().Trim().ToLower();
            ChartJsTimeSeriesTimeScaleSpecifier? timeScale = null;

            if ( string.IsNullOrWhiteSpace( intervalType ) )
            {
                // Query the data to determine if it is a time series.
                if ( chartDataItems.Any( x => x.DateTimeStamp != 0 ) )
                {
                    timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Auto;
                }
            }
            else if ( intervalType == "day" )
            {
                timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day;
            }
            else if ( intervalType == "month" )
            {
                timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }
            else if ( intervalType == "year" )
            {
                timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Year;
            }
            else
            {
                timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Auto;
            }

            var firstItem = chartDataItems.FirstOrDefault();
            if ( timeScale != null )
            {
                var interval = this.SeriesGroupIntervalSize;

                var isChartJsDataPoint = firstItem is IChartJsTimeSeriesDataPoint;
                var timeDatasets = new List<ChartJsTimeSeriesDataset>();
                foreach ( var series in itemsBySeries )
                {
                    List<IChartJsTimeSeriesDataPoint> dataPoints;
                    if ( isChartJsDataPoint )
                    {
                        dataPoints = chartDataItems.Cast<IChartJsTimeSeriesDataPoint>().ToList();
                    }
                    else
                    {
                        dataPoints = chartDataItems.Where( x => x.SeriesName == series.Key )
                            .Select( x => ( IChartJsTimeSeriesDataPoint ) new ChartJsTimeSeriesDataPoint
                            {
                                DateTime = GetDateTimeFromJavascriptMilliseconds( x.DateTimeStamp ),
                                Value = x.YValue ?? 0
                            } )
                            .ToList();
                    }
                    var dataset = new ChartJsTimeSeriesDataset
                    {
                        Name = series.Key,
                        DataPoints = dataPoints
                    };

                    timeDatasets.Add( dataset );
                }

                var chartFactory = new ChartJsTimeSeriesDataFactory<IChartJsTimeSeriesDataPoint>();

                chartFactory.StartDateTime = this.StartDate;
                chartFactory.EndDateTime = this.EndDate;
                chartFactory.Datasets = timeDatasets;
                chartFactory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;
                chartFactory.TimeScale = timeScale.Value;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetTimeChartDataFactoryArgs();
                var chartDataJson = chartFactory.GetJson( args );
                return chartDataJson;
            }
            else
            {
                var isChartJsDataPoint = firstItem is IChartJsCategorySeriesDataPoint;
                var categoryDatasets = new List<ChartJsCategorySeriesDataset>();
                foreach ( var series in itemsBySeries )
                {
                    List<IChartJsCategorySeriesDataPoint> dataPoints;
                    if ( isChartJsDataPoint )
                    {
                        dataPoints = chartDataItems.Cast<IChartJsCategorySeriesDataPoint>().ToList();
                    }
                    else
                    {
                        dataPoints = chartDataItems.Where( x => x.SeriesName == series.Key )
                            .Select( x => ( IChartJsCategorySeriesDataPoint ) new ChartJsCategorySeriesDataPoint
                            {
                                Category = x.SeriesName,
                                Value = x.YValue ?? 0,
                            } )
                            .ToList();
                    }
                    var dataset = new ChartJsCategorySeriesDataset
                    {
                        Name = series.Key,
                        DataPoints = dataPoints
                    };

                    categoryDatasets.Add( dataset );
                }

                var chartFactory = new ChartJsCategorySeriesDataFactory<IChartJsCategorySeriesDataPoint>();

                chartFactory.Datasets = categoryDatasets;
                chartFactory.ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Line;

                // Add client script to construct the chart.
                var args = GetCategoryChartDataFactoryArgs();
                var chartDataJson = chartFactory.GetJson( args );
                return chartDataJson;
            }
        }

        /// <summary>
        /// Get the arguments for the chart factory from the control settings.
        /// </summary>
        /// <returns></returns>
        private ChartJsTimeSeriesDataFactory.GetJsonArgs GetTimeChartDataFactoryArgs()
        {
            var args = new ChartJsTimeSeriesDataFactory.GetJsonArgs();
            args.ChartStyle = GetChartStyle();
            args.ContainerControlId = this.ClientID;
            args.DisableAnimation = this.Page.IsPostBack;
            args.YValueFormatString = this.YValueFormatString;
            return args;
        }

        /// <summary>
        /// Get the arguments for the chart factory from the control settings.
        /// </summary>
        /// <returns></returns>
        private ChartJsCategorySeriesDataFactory.GetJsonArgs GetCategoryChartDataFactoryArgs()
        {
            var args = new ChartJsCategorySeriesDataFactory.GetJsonArgs();
            args.ChartStyle = GetChartStyle();
            args.ContainerControlId = this.ClientID;
            args.DisableAnimation = this.Page.IsPostBack;
            args.YValueFormatString = this.YValueFormatString;
            return args;
        }
    }
}