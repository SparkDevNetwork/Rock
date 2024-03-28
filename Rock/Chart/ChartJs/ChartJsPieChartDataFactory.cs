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
    /// A factory that builds a data model for a pie chart that can be rendered by the Chart.js library.
    /// The pie chart shows a numerical proportion of the total represented by each category and value.
    /// </summary>
    public class ChartJsPieChartDataFactory : ChartJsDataFactory
    {
        /// <summary>
        /// Gets the Json data for a data structure that can be used by Chart.js to construct a pie chart.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetChartDataJson( ChartJsCategorySeriesDataset dataset, ChartJsonArgs args )
        {
            // Create the data structure for Chart.js parameter "data.datasets".
            var chartData = GetChartDataJsonObject( dataset, args );

            var json = GetChartDataJsonInternal( chartData, args );
            return json;
        }

        /// <summary>
        /// Gets the Json data for a data structure that can be used by Chart.js to construct a pie chart.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetChartDataJson( ChartJsTimeSeriesDataset dataset, ChartJsonArgs args )
        {
            // Quantize the dataset by dividing the time series into pie slices such as days/months/years.
            var quantizedDataset = GetCategorySeriesFromTimeSeries( dataset, null );  
            var chartData = GetChartDataJsonObject( quantizedDataset, args );

            var json = GetChartDataJsonInternal( chartData, args );
            return json;
        }

        /// <summary>
        /// Create a category series suitable for display in a pie chart from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetChartDataJson( IEnumerable<IChartData> chartDataItems, string defaultSeriesName, ChartJsonArgs args )
        {
            if ( string.IsNullOrWhiteSpace( defaultSeriesName ) )
            {
                defaultSeriesName = "(unknown)";
            }

            var firstItem = chartDataItems.FirstOrDefault();
            var isChartJsDataPoint = firstItem is IChartJsCategorySeriesDataPoint;
            var isTimeSeries = chartDataItems.Any( x => x.DateTimeStamp != 0 );

            var dataset = new ChartJsCategorySeriesDataset();
            dataset.DataPoints = new List<IChartJsCategorySeriesDataPoint>();

            if ( isTimeSeries )
            {
                // Create a time series from the data items, and quantize the data points to create time-based categories.
                var timeDatasets = ChartDataFactory.GetTimeSeriesFromChartData( chartDataItems, defaultSeriesName );
                // The pie chart can only represent one dataset, so get the first available.
                var timeDataset = timeDatasets.FirstOrDefault();
                dataset = ChartDataFactory.GetCategorySeriesFromTimeSeries( timeDataset );
            }
            else
            {
                var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );
                foreach ( var series in itemsBySeries )
                {
                    var categoryName = string.IsNullOrWhiteSpace( series.Key ) ? defaultSeriesName : series.Key;
                    IChartJsCategorySeriesDataPoint dataPoint;
                    // Each series represents a slice of the pie chart, so get the sum of datapoints for each series.
                    dataPoint = new ChartJsCategorySeriesDataPoint
                    {
                        Category = categoryName,
                        Value = chartDataItems.Where( x => x.SeriesName == series.Key ).Sum( i => i.YValue ?? 0 )
                    };

                    dataset.DataPoints.Add( dataPoint );
                }
            }

            var chartData = GetChartDataJsonObject( dataset, args );

            var json = GetChartDataJsonInternal( chartData, args );
            return json;
        }

        private string GetChartDataJsonInternal( dynamic chartData, ChartJsonArgs args )
        {
            // Apply the argument settings.
            this.MaintainAspectRatio = args.MaintainAspectRatio;
            this.SizeToFitContainerWidth = args.SizeToFitContainerWidth;

            // Create the data structure for Chart.js parameter "options".
            var optionsLegend = this.GetLegendConfigurationObject( args.LegendPosition, args.LegendAlignment, args.DisplayLegend );
            var tooltipsConfiguration = this.GetTooltipsConfigurationObject( args.ContainerControlId, args.YValueFormatString );

            // Set segment labels.
            string segmentScript;
            if ( args.YValueFormatString == "percentage" )
            {
                // Display percentage if > 15%.
                segmentScript = @"
function( value, context ) {
    if ( value >= 15 )
    {
        return value + '%';
    }
    return '';
}
";
            }
            else
            {
                // Display raw value.
                segmentScript = @"
function( value, context ) {
    return Intl.NumberFormat().format(value);
}
";
            }

            var pluginsConfiguration = new
            {
                datalabels = new
                {
                    display = args.ShowSegmentLabels,
                    formatter = new JRaw( segmentScript )
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
        /// Get a JSON data structure that represents Chart.js datapoints suitable for constructing a pie chart.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <remarks>
        /// Using discrete categories for the Category scale allows the data series to be padded with zero-values where no data is found.
        /// This often shows a more accurate representation of the data rather than allowing Chart.js to interpolate the empty intervals.
        /// </remarks>
        protected dynamic GetChartDataJsonObject( ChartJsCategorySeriesDataset dataset, ChartJsonArgs args )
        {
            var categoryNames = dataset.DataPoints.Select( x => x.Category ).ToList();
            var colorGenerator = new ChartColorPaletteGenerator( this.ChartColors );

            var jsDatasets = new List<object>();

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

            var chartData = new { datasets = jsDatasets, labels = categoryNames };

            return chartData;
        }

        /// <summary>
        /// Get a sequence of datapoints corresponding to a specific category, ensuring there is a value for each of the categories.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="categoryNames"></param>
        /// <returns></returns>
        private List<decimal> GetDataPointsForAllCategories( ChartJsCategorySeriesDataset dataset, List<string> categoryNames )
        {
            var dataValues = new List<decimal>();

            foreach ( var categoryName in categoryNames )
            {
                var datapoint = dataset.DataPoints.FirstOrDefault( x => x.Category == categoryName );

                if ( datapoint == null )
                {
                    dataValues.Add( 0 );
                }
                else
                {
                    dataValues.Add( datapoint.Value );
                }
            }

            return dataValues;
        }

        /// <summary>
        /// Convert a collection of time series datasets to category-value datasets, where the categories represent discrete periods in the specified time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        private ChartJsCategorySeriesDataset GetCategorySeriesFromTimeSeries( ChartJsTimeSeriesDataset dataset, ChartJsTimeSeriesTimeScaleSpecifier? timeScale )
        {
            const string DateFormatStringMonthYear = "MMM yyyy";
            const string DateFormatStringDayMonthYear = "d";

            var datapoints = dataset.DataPoints;

            var datasetQuantized = new ChartJsCategorySeriesDataset();

            datasetQuantized.Name = dataset.Name;
            datasetQuantized.BorderColor = dataset.BorderColor;
            datasetQuantized.FillColor = dataset.FillColor;

            if ( timeScale == null )
            {
                // Get the sum of all datapoints.
                var quantizedDataPoints = datapoints
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = dataset.Name,
                        Value = datapoints.Sum( y => y.Value ),
                        SortKey = dataset.Name
                    } )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Day = x.DateTime } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Day.ToString( DateFormatStringDayMonthYear ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Day.ToString( "yyyyMMdd" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Month.ToString( DateFormatStringMonthYear ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Month.ToString( "yyyyMM" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Year.ToString( "yyyy" ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Year.ToString( "yyyy" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else
            {
                throw new NotImplementedException( "Timescale is not implemented" );
            }

            return datasetQuantized;
        }

        /// <summary>
        /// Arguments for the methods that build the Chart Json data structures for a pie chart.
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