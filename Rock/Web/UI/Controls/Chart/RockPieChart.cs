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
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.ChartClickScript = $@"function (event, pos, item) {{
    var activePoints = _chart.getElementsAtEvent(event);
    var chartData = activePoints[0]['_chart'].config.data;
    var dataset = chartData.datasets[activePoints[0]['_datasetIndex']];
    var dataItem = dataset.data[activePoints[0]['_index']];
    if (dataItem) {{
        postbackArg = 'YValue=' + dataItem;
    }}
    else
    {{
        // no point was clicked
        postbackArg =  'YValue=';
    }}
    window.location = ""javascript:__doPostBack('{this.ChartContainerControl.UniqueID}', '"" +  postbackArg + ""')"";
}}";
        }

        /// <inheritdoc/>
        protected override string OnGenerateChartJson( object dataSource, string defaultSeriesName = null )
        {
            var chartFactory = new ChartJsPieChartDataFactory();
            chartFactory.CustomTooltipScript = this.TooltipContentScript;

            var args = GetDataFactoryArgs();

            if ( dataSource is List<ChartJsCategorySeriesDataset> categoryDatasets )
            {
                // If the datasource is a list that contains single dataset, extract it.
                if ( categoryDatasets.Count == 1 )
                {
                    dataSource = categoryDatasets[0];
                }
            }

            string chartDataJson = null;
            if ( dataSource is IEnumerable<IChartData> chartDataItems )
            {
                chartDataJson = chartFactory.GetChartDataJson( chartDataItems, defaultSeriesName, args );
            }
            else if ( dataSource is ChartJsCategorySeriesDataset categoryDataset )
            {
                // For a single data set, each datapoint represents a pie slice.
                chartDataJson = chartFactory.GetChartDataJson( categoryDataset, args );
            }
            else
            {
                throw new Exception( "Pie Chart unavailable. The data source is invalid." );
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

            args.ContainerControlId = this.ClientID;

            GetChartJsLegendLocationSettings( out string legendPosition, out string legendAlignment );

            args.LegendPosition = legendPosition;
            args.LegendAlignment = legendAlignment;
            args.DisplayLegend = this.ShowLegend;

            args.MaintainAspectRatio = this.MaintainAspectRatio;

            args.DisableAnimation = this.Page.IsPostBack;
            args.ShowSegmentLabels = this.ShowSegmentLabels;

            // Set the inner radius of the pie.
            // Interpret a setting between 0 and 1 as a fraction for backward compatibility.
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
