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
    public class RockBarChart : ChartJsChart
    {
        /// <summary>
        /// Gets or sets the width of the bar.
        /// Note that if the xaxis is time, BarWidth should be in milliseconds
        /// </summary>
        /// <value>
        /// The width of the bar.
        /// </value>
        public double? BarWidth
        {
            get
            {
                return ViewState["BarWidth"] as double? ?? 0.6;
            }

            set
            {
                ViewState["BarWidth"] = value;
            }
        }

        /// <inheritdoc/>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.ChartClickScript = $@"function (event, pos, item) {{
    var activePoints = _chart.getElementsAtEvent(event);
    var chartData = activePoints[0]['_chart'].config.data;
    var dataset = chartData.datasets[activePoints[0]['_datasetIndex']];
    var dataItem = dataset.data[activePoints[0]['_index']];
    var customData = dataItem.customData;
    if (dataItem) {{
        postbackArg = 'SeriesId=' + customData.SeriesName
            + ';DateStamp=' + customData.DateTimeStamp
            + ';YValue=' + ( customData.hasOwnProperty('YValue') ? customData.YValue : customData.Value );
    }}
    else
    {{
        // no point was clicked
        postbackArg =  'DateStamp=;YValue=;SeriesId=';
    }}
    window.location = ""javascript:__doPostBack('{ ChartContainerControl.UniqueID }', '"" +  postbackArg + ""')"";
}}";
        }

        /// <inheritdoc/>
        protected override string OnGenerateChartJson( object dataSource, string defaultSeriesName = null )
        {
            var chartDataJson = string.Empty;
            if ( dataSource is IEnumerable<IChartData> chartData )
            {
                chartDataJson = GetFromChartDataItems( chartData, defaultSeriesName );
            }
            else if ( dataSource is List<ChartJsTimeSeriesDataset> datasets )
            {
                var chartFactory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

                chartFactory.Datasets = datasets;
                chartFactory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Bar;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetTimeChartDataFactoryArgs();
                chartDataJson = chartFactory.GetJson( args );
            }
            else if ( dataSource is List<ChartJsCategorySeriesDataset> categoryDatasets )
            {
                var chartFactory = new ChartJsCategorySeriesDataFactory<ChartJsCategorySeriesDataPoint>();
                chartFactory.Datasets = categoryDatasets;
                chartFactory.ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Bar;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetCategoryChartDataFactoryArgs();
                chartDataJson = chartFactory.GetJson( args );
            }

            return chartDataJson;
        }

        private string GetFromChartDataItems( IEnumerable<IChartData> chartDataItems, string defaultSeriesName )
        {
            var chartDataJson = string.Empty;
            var isTimeSeries = chartDataItems.Any( x => x.DateTimeStamp != 0 );
            if ( isTimeSeries )
            {
                var timeDatasets = ChartDataFactory.GetTimeSeriesFromChartData( chartDataItems, defaultSeriesName );
                var chartFactory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();
                chartFactory.Datasets = timeDatasets;
                chartFactory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Bar;
                // TODO: Adjust the scale to suit the data.
                chartFactory.TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Year;
                chartFactory.CustomTooltipScript = this.TooltipContentScript;

                // Add client script to construct the chart.
                var args = GetTimeChartDataFactoryArgs();
                chartDataJson = chartFactory.GetJson( args );

                return chartDataJson;
            }
            else
            {
                var categoryDatasets = ChartDataFactory.GetCategorySeriesFromChartData( chartDataItems, defaultSeriesName );
                var chartFactory = new ChartJsCategorySeriesDataFactory<ChartJsCategorySeriesDataPoint>();

                chartFactory.Datasets = categoryDatasets;
                chartFactory.ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Bar;

                // Add client script to construct the chart.
                var args = GetCategoryChartDataFactoryArgs();
                chartDataJson = chartFactory.GetJson( args );
            }

            return chartDataJson;
        }

        /// <summary>
        /// Get the arguments for the chart factory from the control settings.
        /// </summary>
        /// <returns></returns>
        private ChartJsTimeSeriesDataFactory.GetJsonArgs GetTimeChartDataFactoryArgs()
        {
            var args = new ChartJsTimeSeriesDataFactory.GetJsonArgs();

            args.ContainerControlId = this.ClientID;

            GetChartJsLegendLocationSettings( out string legendPosition, out string legendAlignment );

            args.LegendPosition = legendPosition;
            args.LegendAlignment = legendAlignment;
            args.DisplayLegend = this.ShowLegend;

            args.DisableAnimation = this.Page.IsPostBack;
            args.YValueFormatString = this.YValueFormatString;

            args.MaintainAspectRatio = this.MaintainAspectRatio;

            return args;
        }

        /// <summary>
        /// Get the arguments for the chart factory from the control settings.
        /// </summary>
        /// <returns></returns>
        private ChartJsCategorySeriesDataFactory.GetJsonArgs GetCategoryChartDataFactoryArgs()
        {
            var args = new ChartJsCategorySeriesDataFactory.GetJsonArgs();

            args.ContainerControlId = this.ClientID;

            GetChartJsLegendLocationSettings( out string legendPosition, out string legendAlignment );

            args.LegendPosition = legendPosition;
            args.LegendAlignment = legendAlignment;
            args.DisplayLegend = this.ShowLegend;

            args.DisableAnimation = this.Page.IsPostBack;
            args.YValueFormatString = this.YValueFormatString;

            args.MaintainAspectRatio = this.MaintainAspectRatio;

            return args;
        }
    }
}
