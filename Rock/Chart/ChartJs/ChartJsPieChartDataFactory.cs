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
using Newtonsoft.Json.Linq;
using Rock.Utility;

namespace Rock.Chart
{
    /// <summary>
    /// Provides base functionality for factories that build a data model for a pie chart that can be rendered by the Chart.js library.
    /// The pie chart shows the numerical propotion of the total represented by each category and value.
    /// </summary>
    /// <remarks>
    /// Compatible with ChartJS v2.8.0.
    /// </remarks>
    public class ChartJsPieChartDataFactory<TDataPoint> : ChartJsCategorySeriesDataFactory<TDataPoint>
        where TDataPoint : IChartJsCategorySeriesDataPoint
    {
        /// <summary>
        /// Gets the Json data for a data structure that can be used by Chart.js to construct a pie chart.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetChartDataJson( ChartJsPieChartDataFactory.ChartJsonArgs args )
        {
            // Apply the argument settings.
            this.SetChartStyle( args.ChartStyle );

            // Adjust the legend position to the left or right of the pie chart.
            var legendPosition = args.ChartStyle?.Legend?.Position;
            if ( legendPosition == "nw" || legendPosition == "sw" )
            {
                legendPosition = "left";
            }
            else
            {
                legendPosition = "right";
            }
            this.LegendPosition = legendPosition;

            this.MaintainAspectRatio = args.MaintainAspectRatio;
            this.SizeToFitContainerWidth = args.SizeToFitContainerWidth;

            // Create the data structure for Chart.js parameter "data.datasets".
            var chartData = GetChartDataJsonObjectForSpecificCategoryScale( args );

            // Create the data structure for Chart.js parameter "options".
            var optionsLegend = this.GetLegendConfigurationObject();
            var tooltipsConfiguration = this.GetTooltipsConfigurationObject( args.ContainerControlId, args.YValueFormatString );

            // Set segment labels.
            var pluginsConfiguration = new
            {
                datalabels = new
                {
                    display = args.ShowSegmentLabels,
                    formatter = new JRaw( @"
function(value, context) {
    if ( value >= 15 ) {
        return value + '%';
    }
    return '';
}" )
                }
            };

            var optionsData = new
            {
                maintainAspectRatio = this.MaintainAspectRatio,
                responsive = this.SizeToFitContainerWidth,
                animation = new { duration = args.DisableAnimation ? 0 : 1000 },
                legend = optionsLegend,
                tooltips = tooltipsConfiguration,
                cutoutPercentage = args.CutoutPercentage,
                plugins = pluginsConfiguration
            };

            // Set the chart style to doughnut if there is a cutout.
            var chartStyle = args.CutoutPercentage == 0 ? "pie" : "doughnut";

            var chartStructure = new { type = chartStyle, plugins = new JRaw( "[ChartDataLabels]" ), data = chartData, options = optionsData };

            // Return the JSON representation of the Chart.js data structure.
            return SerializeJsonObject( chartStructure );
        }

        /// <summary>
        /// Gets a JavaScript data object that represents the configuration for the ChartJs tooltip chart element.
        /// </summary>
        /// <returns></returns>
        protected override dynamic GetTooltipsConfigurationObject( string containerControlId, string valueFormatString )
        {
            if ( containerControlId == null )
            {
                return new { enabled = true };
            }

            dynamic tooltipConfiguration;

            // Enable custom tooltips if a custom script is specified.
            var enableCustomTooltip = !string.IsNullOrEmpty( this.CustomTooltipScript );
            if ( enableCustomTooltip )
            {
                // If this is a custom tooltip, use the default implementation.
                return base.GetTooltipsConfigurationObject( containerControlId, valueFormatString );
            }
            else
            {
                // Use the default Chart.js tooltip, with a label appropriate to the datapoint values unit of measure.
                string tooltipsCallbackStr;
                valueFormatString = valueFormatString ?? string.Empty;
                valueFormatString = valueFormatString.Trim().ToLower();
                if ( valueFormatString == "currency" )
                {
                    var currencyCode = RockCurrencyCodeInfo.GetCurrencyCode();
                    tooltipsCallbackStr = string.Format( @"
function (tooltipItem, data) {{
    let label = data.labels[tooltipItem.index] || '';
    if (label) {{
        label += ': ';
    }}
    let dataValue = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
    label = label + Intl.NumberFormat( undefined, {{ style: 'currency', currency: '{0}' }}).format( dataValue );
    return label;
}}
",
                    currencyCode );
                }
                else if ( valueFormatString == "percentage" )
                {
                    tooltipsCallbackStr = @"
function (tooltipItem, data) {
    var label = data.labels[tooltipItem.index] + ': ' + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]) + '%';
    return label;
}";
                }
                else
                {
                    tooltipsCallbackStr = @"
function (tooltipItem, data) {
    var label = data.labels[tooltipItem.index] + ': ' + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
    return label;
}";
                }
                tooltipConfiguration = new
                {
                    enabled = true,
                    callbacks = new
                    {
                        label = new JRaw( tooltipsCallbackStr )
                    }
                };
            }

            return tooltipConfiguration;
        }

        /// <summary>
        /// Get a JSON data structure that represents Chart.js options to show discrete Category categories on the X-axis.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <remarks>
        /// Using discrete categories for the Category scale allows the data series to be padded with zero-values where no data is found.
        /// This often shows a more accurate representation of the data rather than allowing Chart.js to interpolate the empty intervals.
        /// </remarks>
        protected dynamic GetChartDataJsonObjectForSpecificCategoryScale( ChartJsPieChartDataFactory.ChartJsonArgs args )
        {
            var categoryNames = this.Datasets.SelectMany( x => x.DataPoints ).Select( x => x.Category ).ToList();
            var colorGenerator = new ChartColorPaletteGenerator( this.ChartColors );

            var jsDatasets = new List<object>();

            foreach ( var dataset in this.Datasets )
            {
                // Create a sequence of datapoints, ensuring there is a value for each of the categories.
                dynamic dataValues;
                dynamic jsDataset;

                // For a pie chart, each data point represents a slice and therefore should have a different color.
                var pieColorGenerator = new ChartColorPaletteGenerator( this.ChartColors );
                var pieColors = new List<string>();
                var fillColors = new List<string>();
                int fadePercentage = this.AreaFillOpacity.ToIntSafe() * 100;

                for ( int i = 0; i < categoryNames.Count; i++ )
                {
                    var nextColor = pieColorGenerator.GetNextColor();
                    pieColors.Add( nextColor.ToRGBA() );

                    if ( fadePercentage > 0 )
                    {
                        nextColor.FadeOut( fadePercentage );
                        fillColors.Add( nextColor.ToRGBA() );
                    }
                }

                dataValues = GetDataPointsForAllCategories( dataset, categoryNames );

                jsDataset = new
                {
                    label = dataset.Name,
                    borderColor = pieColors,
                    borderWidth = 2,
                    backgroundColor = pieColors,
                    fill = fadePercentage > 0 ? "origin" : "false",
                    data = dataValues
                };

                jsDatasets.Add( jsDataset );
            }

            var chartData = new { datasets = jsDatasets, labels = categoryNames };

            return chartData;
        }
    }

    /// <summary>
    /// Provides helper classes in a non-generic context for the generic factory ChartJsCategorySeriesDataFactory.
    /// </summary>
    public static class ChartJsPieChartDataFactory
    {
        /// <summary>
        /// Arguments for the methods that build the Chart Json data structures.
        /// </summary>
        public sealed class ChartJsonArgs : ChartJsDataFactory.GetJsonArgs
        {
            /// <summary>
            /// The percentage of the inner segment that should be cut out of the whole.
            /// To create a pie chart, use the default value of 0.
            /// To create a doughnut chart, use a value of 50.
            /// </summary>
            public decimal CutoutPercentage { get; set; }

            /// <summary>
            /// Gets or sets a flag to indicate if labels should be shown for individual segments of the pie chart.
            /// </summary>
            public bool ShowSegmentLabels { get; set; }
        }
    }
}