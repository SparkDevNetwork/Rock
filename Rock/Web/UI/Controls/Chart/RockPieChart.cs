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
    /// A control that displays a chart showing data values represented as vertical bars.
    /// </summary>
    public class RockPieChart : ChartJsChart
    {
        /// <summary>
        /// Gets or sets the radius as a percentage of available space
        /// Defaults to .75 if there is a legend, or 1 if there isn't
        /// </summary>
        /// <value>
        /// The radius.
        /// </value>
        public double? Radius { get; set; }

        /// <summary>
        /// Sets the radius of the donut hole. If value is between 0 and 1 (inclusive) then it will use that as a percentage of the radius, otherwise it will use the value as a direct pixel length.
        /// </summary>
        /// <value>
        /// The radius.
        /// </value> 
        public double? InnerRadius { get; set; }

        /// <summary>
        /// Gets or sets a flag to indicate if labels should be shown for individual segments of the pie chart.
        /// </summary>
        public bool ShowSegmentLabels { get; set; }

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
                throw new Exception( "Time series data not implemented for Pie chart." );
            }
            else if ( dataSource is List<ChartJsCategorySeriesDataset> categoryDatasets )
            {
                var chartFactory = new ChartJsPieChartDataFactory<ChartJsCategorySeriesDataPoint>();
                chartFactory.Datasets = categoryDatasets;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetDataFactoryArgs();

                chartDataJson = chartFactory.GetChartDataJson( args );
            }

            return chartDataJson;
        }

        private string GetFromChartDataItems( IEnumerable<IChartData> chartDataItems )
        {
            var chartDataJson = string.Empty;
            var isTimeSeries = chartDataItems.Any( x => x.DateTimeStamp != 0 );
            if ( isTimeSeries )
            {
                throw new Exception( "Time series data not implemented for Pie chart." );
            }
            else
            {
                var categoryDatasets = GetCategorySeriesFromChartData( chartDataItems );
                var chartFactory = new ChartJsPieChartDataFactory<ChartJsCategorySeriesDataPoint>();

                chartFactory.Datasets = categoryDatasets;
                chartFactory.ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Pie;

                // Add client script to construct the chart.
                var args = GetDataFactoryArgs();
                chartDataJson = chartFactory.GetChartDataJson( args );
            }

            return chartDataJson;
        }

        /// <summary>
        /// Get the arguments for the chart factory from the control settings.
        /// </summary>
        /// <returns></returns>
        private ChartJsPieChartDataFactory.ChartJsonArgs GetDataFactoryArgs()
        {
            var args = new ChartJsPieChartDataFactory.ChartJsonArgs();

            args.ChartStyle = GetChartStyle();
            args.ContainerControlId = this.ClientID;
            args.DisableAnimation = this.Page.IsPostBack;
            args.ShowSegmentLabels = this.ShowSegmentLabels;

            // Set the inner radius of the pie.
            // Interpret a setting between 0 and 1 as a fraction, and 1.
            var innerRadius = ( decimal ) this.InnerRadius.GetValueOrDefault( 0 );
            if ( innerRadius > 0 && innerRadius < 1 )
            {
                args.CutoutPercentage = innerRadius * 100;
            }
            else if ( innerRadius <= 0 )
            {
                args.CutoutPercentage = 0;
            }
            else if ( innerRadius >= 99 )
            {
                args.CutoutPercentage = 99;
            }
            else
            {
                args.CutoutPercentage = innerRadius;
            }

            return args;
        }
    }
}
