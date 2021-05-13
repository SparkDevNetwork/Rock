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
using System.Text;
using Newtonsoft.Json;
using Rock.Utility;

namespace Rock.Chart
{
    /// <summary>
    /// Provides helper classes in a non-generic context for the generic factory
    /// </summary>
    public static class ChartJsTimeSeriesDataFactory
    {
        /// <summary>
        /// Args for the GetJson method
        /// </summary>
        public sealed class GetJsonArgs
        {
            /// <summary>
            /// Size to fit container width?
            /// </summary>
            public bool SizeToFitContainerWidth { get; set; } = true;

            /// <summary>
            /// Maintain aspect ratio?
            /// </summary>
            public bool MaintainAspectRatio { get; set; } = false;

            /// <summary>
            /// Display legend?
            /// </summary>
            public bool DisplayLegend { get; set; } = true;

            /// <summary>
            /// Bezier curve tension of the line. Set to 0 to draw straightlines.
            /// This option is ignored if monotone cubic interpolation is used.
            /// </summary>
            public decimal LineTension { get; set; } = 0m;
        }
    }

    /// <summary>
    /// Creates data structures suitable for plotting a value-over-time data series on a Cartesian grid using ChartJs.
    /// </summary>
    /// <remarks>
    /// This factory can generate the following data formats:
    /// * A JSON object compatible with the ChartJs constructor 'data' parameter: new Chart([chartContainer], [data]);
    /// * A Lava Chart Shortcode text block.
    /// </remarks>
    public class ChartJsTimeSeriesDataFactory<TDataPoint>
        where TDataPoint : IChartJsTimeSeriesDataPoint
    {

        private const string DateFormatStringMonthYear = "MMM yyyy";
        private const string DateFormatStringDayMonthYear = "d";

        private List<ChartJsTimeSeriesDataset> _Datasets = new List<ChartJsTimeSeriesDataset>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartJsTimeSeriesDataFactory{TDataPoint}"/> class.
        /// </summary>
        public ChartJsTimeSeriesDataFactory()
        {
            this.Datasets = new List<ChartJsTimeSeriesDataset>();

            this.ChartColors = ChartJsConstants.Colors.DefaultPalette;
        }

        #region Properties

        /// <summary>
        /// The style of chart to display.
        /// </summary>
        public ChartJsTimeSeriesChartStyleSpecifier ChartStyle { get; set; } = ChartJsTimeSeriesChartStyleSpecifier.Line;

        /// <summary>
        /// The minimum time divisions show on the Y axis.
        /// </summary>
        public ChartJsTimeSeriesTimeScaleSpecifier TimeScale { get; set; } = ChartJsTimeSeriesTimeScaleSpecifier.Auto;

        /// <summary>
        /// The level of opacity for the area bounded by a dataset on a scale from 0 to 1, where 0 represents complete transparency.
        /// </summary>
        public double AreaFillOpacity { get; set; } = 0.5;

        /// <summary>
        /// The start date for the time series.
        /// If not specified, the date of the earliest data point will be used.
        /// </summary>
        public DateTime? StartDateTime { get; set; } = null;

        /// <summary>
        /// The end date for the time series.
        /// If not specified, the date of the latest data point will be used.
        /// </summary>
        public DateTime? EndDateTime { get; set; } = null;

        /// <summary>
        /// A collection of HTML Colors that represent the default palette for datasets in the chart.
        /// Colors are selected in order from this list for each dataset that does not have a specified color.
        /// </summary>
        public List<string> ChartColors { get; set; }

        /// <summary>
        /// A collection of data points that are displayed on the chart as one or more series of data.
        /// </summary>
        public List<ChartJsTimeSeriesDataset> Datasets
        {
            get { return _Datasets; }
            set
            {
                _Datasets = value ?? new List<ChartJsTimeSeriesDataset>();
            }
        }

        /// <summary>
        /// Does the chart data contain any data points?
        /// </summary>
        public bool HasData
        {
            get
            {
                return _Datasets != null
                       && _Datasets.SelectMany( x => x.DataPoints ).Any();
            }
        }

        #endregion

        #region Chart.js JSON Renderer

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// The width is determined by the container and aspect ratio is not preserved.
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            // Return the chart configuration using the default layout - width determined by container, aspect ratio not preserved.
            return GetJson( sizeToFitContainerWidth: true, maintainAspectRatio: false );
        }

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// The aspect ratio is not preserved.
        /// </summary>
        /// <param name="sizeToFitContainerWidth">if set to <c>true</c> [size to fit container width].</param>
        /// <returns></returns>
        public string GetJson( bool sizeToFitContainerWidth )
        {
            return GetJson( sizeToFitContainerWidth, false );
        }

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// </summary>
        /// <param name="sizeToFitContainerWidth">if set to <c>true</c> [size to fit container width].</param>
        /// <param name="maintainAspectRatio">if set to <c>true</c> [maintain aspect ratio].</param>
        /// <returns></returns>
        public string GetJson( bool sizeToFitContainerWidth, bool maintainAspectRatio )
        {
            return GetJson( sizeToFitContainerWidth, maintainAspectRatio, true );
        }

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// </summary>
        /// <param name="sizeToFitContainerWidth">if set to <c>true</c> [size to fit container width].</param>
        /// <param name="maintainAspectRatio">if set to <c>true</c> [maintain aspect ratio].</param>
        /// <param name="displayLegend">Should the legend be displayed</param>
        /// <returns></returns>
        public string GetJson( bool sizeToFitContainerWidth, bool maintainAspectRatio, bool displayLegend )
        {
            return GetJson( new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                DisplayLegend = displayLegend,
                LineTension = 0m,
                MaintainAspectRatio = maintainAspectRatio,
                SizeToFitContainerWidth = sizeToFitContainerWidth
            } );
        }

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public string GetJson( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            // Create the data structure for Chart.js parameter "data.datasets".
            dynamic chartData;

            if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                // Get the datapoints as a time/value series.
                chartData = this.GetChartJsonObjectForAutoTimeScale( args );
            }
            else
            {
                // Get the datapoints grouped by category according to the TimeScale setting.
                chartData = this.GetChartJsonObjectForSpecificTimeScale( args );
            }

            // Get options for the X-axis, showing the time either by discrete category or using a best-fit scale determined by Chart.js.
            dynamic optionsXaxes;

            if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                // Allow Chart.js to scale the X-axis to best fit.
                optionsXaxes = this.GetChartXaxisOptionsForAutoTimeScale();
            }
            else
            {
                // Display discrete time periods on the X-axis using the category labels defined in the chart data.
                optionsXaxes = null;
            }

            // Prevent Chart.js from displaying decimal values in the y-axis by forcing the step size to 1 if the value range is below 10.
            var maxValue = GetMaximumDataValue();

            decimal? stepSize = null;

            if ( maxValue < 10 )
            {
                stepSize = 1;
            }

            var isStacked = ( this.ChartStyle == ChartJsTimeSeriesChartStyleSpecifier.StackedLine );

            // Get options for the Y-axis, showing the values.
            // The suggested scale is from 0 to the maximum value in the data set, +10% to allow for a top margin.
            var suggestedMax = Math.Ceiling( maxValue * 1.1M );

            var optionsYaxes = new List<object>() { new { ticks = new { beginAtZero = true, suggestedMax, stepSize }, stacked = isStacked } };

            var optionsLegend = new { position = "bottom", display = args.DisplayLegend };

            // Create the data structure for Chart.js parameter "options".

            // If "maintainAspectRatio" is enabled, responsive mode must also be enabled to avoid a Chart.js resizing bug detailed here:
            // https://github.com/chartjs/Chart.js/issues/1006
            // Until this issue is resolved, avoid this invalid combination of settings.
            if ( args.MaintainAspectRatio &&
                 !args.SizeToFitContainerWidth )
            {
                args.SizeToFitContainerWidth = true;
            }

            var optionsData = new
            {
                maintainAspectRatio = args.MaintainAspectRatio,
                responsive = args.SizeToFitContainerWidth,
                legend = optionsLegend,
                scales = new
                {
                    xAxes = optionsXaxes,
                    yAxes = optionsYaxes
                }
            };

            // Create the data structure for Chartjs parameter "chart".
            string chartStyle = GetChartJsStyleParameterValue( this.ChartStyle );

            dynamic chartStructure = new { type = chartStyle, data = chartData, options = optionsData };

            // Return the JSON representation of the Chart.js data structure.
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            string chartParametersJson = JsonConvert.SerializeObject( chartStructure, Formatting.None, jsonSetting );

            return chartParametersJson;
        }

        /// <summary>
        /// Get a JSON data structure that represents Chart.js options to show auto-scaled time intervals on the X-axis.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private dynamic GetChartJsonObjectForAutoTimeScale( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            var jsDatasets = new List<object>();

            var availableColors = GetDatasetColors();

            foreach ( var dataset in this.Datasets )
            {
                var dataPoints = dataset.DataPoints.Select( dp => new { x = dp.DateTime.ToISO8601DateString(), y = dp.Value } ).OrderBy( dp => dp.x ).ToList();

                // Use the color specifically assigned to this dataset, or get the next color from the queue.
                string borderColor = dataset.BorderColor;

                if ( string.IsNullOrWhiteSpace( borderColor ) )
                {
                    borderColor = GetNextQueueItem( availableColors );
                }

                string backColor = dataset.FillColor;

                bool hasFill = !string.IsNullOrWhiteSpace( backColor );

                if ( !hasFill )
                {
                    // If a FillColor is not specified, disable fill but assign a color so a filled square is shown on the legend.
                    backColor = borderColor;
                }

                var jsDataset = new { lineTension = args.LineTension, label = dataset.Name, borderColor, backgroundColor = backColor, fill = hasFill, data = dataPoints };

                jsDatasets.Add( jsDataset );
            }

            dynamic chartData = new { datasets = jsDatasets };

            return chartData;
        }

        /// <summary>
        /// Get a JSON data structure that represents Chart.js options to show discrete time categories on the X-axis.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <remarks>
        /// Using discrete categories for the time scale allows the data series to be padded with zero-values where no data is found.
        /// This often shows a more accurate representation of the data rather than allowing Chart.js to interpolate the empty intervals.
        /// </remarks>
        private dynamic GetChartJsonObjectForSpecificTimeScale( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            // Group the datapoints according to the TimeScale setting.
            var datasets = GetCategoryDatasets( this.TimeScale );

            var categories = GetTimescaleCategories( this.TimeScale );

            var categoryNames = categories.Select( x => x.Category ).ToList();

            var availableColors = GetDatasetColors();

            var jsDatasets = new List<object>();

            foreach ( var dataset in datasets )
            {
                // Use the line color specifically assigned to this dataset, or get the next color from the queue.
                string borderColor = dataset.BorderColor;

                if ( string.IsNullOrWhiteSpace( borderColor ) )
                {
                    borderColor = GetNextQueueItem( availableColors );
                }

                // Create a sequence of datapoints, ensuring there is a value for each of the categories.
                var dataValues = GetDataPointsForAllCategories( dataset, categoryNames );

                // Calculate the fill color for the area bounded by this dataset.
                string fillColorText = dataset.FillColor;

                string fillOption;

                if ( string.IsNullOrWhiteSpace( dataset.FillColor ) )
                {
                    // Fill color not specified, so disable fill but assign a backcolor so a filled square is shown on the legend.
                    fillOption = "false";

                    fillColorText = borderColor;

                }
                else
                {
                    fillOption = "origin";

                    fillColorText = borderColor;
                }

                // Get an opacity value between 0 and 1.
                var alpha = this.AreaFillOpacity;

                if ( alpha < 0 )
                {
                    alpha = 0;
                }
                else if ( alpha > 1 )
                {
                    alpha = 1;
                }

                if ( alpha >= 0.1 )
                {
                    fillOption = "origin";
                }
                else
                {
                    // Opacity is set to near 0, so disable fill.
                    // A backcolor must be specified to show a filled square in the legend.
                    fillOption = "false";
                }

                var backColor = new RockColor( fillColorText );

                // Add the alpha component to set the transparency level.
                if ( alpha >= 0.1 )
                {
                    backColor = new RockColor( backColor.R, backColor.G, backColor.B, alpha );
                }

                var jsDataset = new { label = dataset.Name, borderColor, backgroundColor = backColor.ToRGBA(), fill = fillOption, lineTension = args.LineTension, data = dataValues };

                jsDatasets.Add( jsDataset );
            }

            var chartData = new { datasets = jsDatasets, labels = categoryNames };

            return chartData;
        }

        /// <summary>
        /// Get a JSON data structure that represents Chart.js options to show auto-scaled time ticks on the X-axis.
        /// </summary>
        /// <returns></returns>
        private dynamic GetChartXaxisOptionsForAutoTimeScale()
        {
            // Create the data structure for Chart.js parameter "options".
            long? minDate = null;
            long? maxDate = null;

            if ( this.StartDateTime != null )
            {
                minDate = this.StartDateTime.Value.ToJavascriptMilliseconds();
            }

            if ( this.EndDateTime != null )
            {
                maxDate = this.EndDateTime.Value.ToJavascriptMilliseconds();
            }

            // Allow Chart.js to scale the X-axis to best fit.
            dynamic optionsXaxes = new List<object>() { new { type = "time", time = new { tooltipFormat = "MM/DD/YYYY" }, min = minDate, max = maxDate } };

            return optionsXaxes;
        }

        #endregion

        #region Lava ShortCode Renderer

        /// <summary>
        /// Get a Lava Chart Shortcode block that describes the chart.
        /// </summary>
        /// <param name="chartHeight">The height of the chart, measured in pixels.</param>
        /// <returns></returns>
        public string GetLavaChartShortCodeText( int chartHeight )
        {
            var sbLava = new StringBuilder();

            // Group the datapoints according to the TimeScale setting.
            var timeScale = this.TimeScale;

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                // The Lava Chart ShortCode requires a category.
                timeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }

            var datasets = GetCategoryDatasets( timeScale );

            var categories = GetTimescaleCategories( timeScale );

            var categoryNames = categories.Select( x => x.Category ).ToList();

            sbLava.AppendFormat( "{{[ chart type:'line' labels:'{0}' legendshow:'true' chartheight:'{1}px' ]}}", categoryNames.AsDelimited( "," ), chartHeight );

            var availableColors = GetDatasetColors();

            foreach ( var dataset in datasets )
            {
                // Use the color specifically assigned to this dataset, or get the next color from the queue.
                string borderColor = dataset.BorderColor;

                if ( string.IsNullOrWhiteSpace( borderColor ) )
                {
                    borderColor = GetNextQueueItem( availableColors );
                }

                // Create a sequence of datapoints, ensuring there is a value for each of the categories.
                var dataValues = GetDataPointsForAllCategories( dataset, categoryNames );

                var dataPointsList = dataValues.AsDelimited( "," );

                // Although area fill is disabled, the fillcolor must be specified for the Legend color to display correctly.
                sbLava.AppendFormat( "[[ dataset label:'{0}' data:'{1}' bordercolor:'{2}' fillcolor:'{2}' ]] [[enddataset]]", dataset.Name, dataPointsList, borderColor );
            }

            sbLava.AppendLine( "{[ endchart ]}" );

            return sbLava.ToString();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Get a set of X-axis categories defined by the selected timescale and time period.
        /// </summary>
        /// <param name="timeScale">The time scale.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Timescale is not implemented</exception>
        private List<ChartJsCategoryValuesDataPoint> GetTimescaleCategories( ChartJsTimeSeriesTimeScaleSpecifier timeScale )
        {
            // Determine the date range.
            var allDataPoints = this.Datasets.SelectMany( x => x.DataPoints );

            if ( !allDataPoints.Any() )
            {
                return new List<ChartJsCategoryValuesDataPoint>();
            }

            var startDate = this.StartDateTime ?? allDataPoints.Min( x => x.DateTime );
            var endDate = this.EndDateTime ?? allDataPoints.Max( x => x.DateTime );

            var thisDate = startDate;

            var categoryDataPoints = new List<ChartJsCategoryValuesDataPoint>();

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
            {
                // To test for the last date of the reporting period, get the next day.
                var lastDateNextDay = endDate.AddDays( 1 );

                while ( thisDate < lastDateNextDay )
                {
                    var categoryDataPoint = new ChartJsCategoryValuesDataPoint() { Category = thisDate.ToString( DateFormatStringDayMonthYear ), SortKey = thisDate.ToString( "yyyyMMdd" ) };

                    categoryDataPoints.Add( categoryDataPoint );

                    thisDate = thisDate.AddDays( 1 );
                }
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
            {
                // To test for the last date of the reporting period, get the first day of the following month.
                var lastDateNextDay = new DateTime( endDate.Year, endDate.Month, 1 ).AddMonths( 1 );

                while ( thisDate < lastDateNextDay )
                {
                    var categoryDataPoint = new ChartJsCategoryValuesDataPoint() { Category = thisDate.ToString( DateFormatStringMonthYear ), SortKey = thisDate.ToString( "yyyyMM" ) };

                    categoryDataPoints.Add( categoryDataPoint );

                    thisDate = thisDate.AddMonths( 1 );
                }
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
            {
                // To test for the last date of the reporting period, get the first day of the following year.
                var lastDateNextDay = new DateTime( endDate.Year, 1, 1 ).AddYears( 1 );

                while ( thisDate < lastDateNextDay )
                {
                    var categoryDataPoint = new ChartJsCategoryValuesDataPoint() { Category = thisDate.ToString( "yyyy" ), SortKey = thisDate.ToString( "yyyy" ) };

                    categoryDataPoints.Add( categoryDataPoint );

                    thisDate = thisDate.AddYears( 1 );
                }
            }
            else
            {
                throw new NotImplementedException( "Timescale is not implemented" );
            }

            return categoryDataPoints;
        }

        /// <summary>
        /// Get a sequence of datapoints corresponding to a specific category, ensuring there is a value for each of the categories.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="categoryNames"></param>
        /// <returns></returns>
        private List<decimal> GetDataPointsForAllCategories( ChartJsCategoryValuesDataset dataset, List<string> categoryNames )
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
        private List<ChartJsCategoryValuesDataset> GetCategoryDatasets( ChartJsTimeSeriesTimeScaleSpecifier timeScale )
        {
            var quantizedDatasets = new List<ChartJsCategoryValuesDataset>();

            foreach ( var dataset in this.Datasets )
            {
                var datapoints = dataset.DataPoints;

                var datasetQuantized = new ChartJsCategoryValuesDataset();

                datasetQuantized.Name = dataset.Name;
                datasetQuantized.BorderColor = dataset.BorderColor;
                datasetQuantized.FillColor = dataset.FillColor;

                if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
                {
                    var quantizedDataPoints = datapoints
                        .GroupBy( x => new { Day = x.DateTime } )
                        .Select( x => new ChartJsCategoryValuesDataPoint
                        {
                            Category = x.Key.Day.ToString( DateFormatStringDayMonthYear ),
                            Value = x.Sum( y => y.Value ),
                            SortKey = x.Key.Day.ToString( "yyyyMMdd" ),
                        } )
                        .OrderBy( x => x.SortKey )
                        .ToList();

                    datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategoryValuesDataPoint>().ToList();
                }
                else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
                {
                    var quantizedDataPoints = datapoints
                        .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
                        .Select( x => new ChartJsCategoryValuesDataPoint
                        {
                            Category = x.Key.Month.ToString( DateFormatStringMonthYear ),
                            Value = x.Sum( y => y.Value ),
                            SortKey = x.Key.Month.ToString( "yyyyMM" ),
                        } )
                        .OrderBy( x => x.SortKey )
                        .ToList();

                    datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategoryValuesDataPoint>().ToList();
                }
                else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
                {
                    var quantizedDataPoints = datapoints
                        .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
                        .Select( x => new ChartJsCategoryValuesDataPoint
                        {
                            Category = x.Key.Year.ToString( "yyyy" ),
                            Value = x.Sum( y => y.Value ),
                            SortKey = x.Key.Year.ToString( "yyyy" ),
                        } )
                        .OrderBy( x => x.SortKey )
                        .ToList();

                    datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategoryValuesDataPoint>().ToList();
                }
                else
                {
                    throw new NotImplementedException( "Timescale is not implemented" );
                }

                quantizedDatasets.Add( datasetQuantized );
            }

            return quantizedDatasets;
        }

        /// <summary>
        /// Return the datasets with all data points quantized according to the time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        private List<ChartJsTimeSeriesDataset> GetTimescaleDatasets()
        {
            var quantizedDatasets = new List<ChartJsTimeSeriesDataset>();

            foreach ( var dataset in this.Datasets )
            {
                var datapoints = dataset.DataPoints;

                var datasetQuantized = new ChartJsTimeSeriesDataset();

                datasetQuantized.Name = dataset.Name;
                datasetQuantized.BorderColor = dataset.BorderColor;
                datasetQuantized.FillColor = dataset.FillColor;

                if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
                {
                    var quantizedDataPoints = datapoints
                        .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
                        .Select( x => new ChartJsTimeSeriesDataPoint
                        {
                            DateTime = x.Key.Month,
                            Value = x.Sum( y => y.Value )
                        } )
                        .OrderBy( x => x.DateTime )
                        .ToList();

                    datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsTimeSeriesDataPoint>().ToList();
                }
                else if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
                {
                    var quantizedDataPoints = datapoints
                        .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
                        .Select( x => new ChartJsTimeSeriesDataPoint
                        {
                            DateTime = x.Key.Year,
                            Value = x.Sum( y => y.Value )
                        } )
                        .OrderBy( x => x.DateTime )
                        .ToList();

                    datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsTimeSeriesDataPoint>().ToList();
                }
                else
                {
                    throw new NotImplementedException( "Timescale is not implemented" );
                }

                quantizedDatasets.Add( datasetQuantized );
            }

            return quantizedDatasets;
        }

        /// <summary>
        /// Gets the next item in a queue, returning to the head of the queue when the last item has been dequeued.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        private string GetNextQueueItem( Queue<string> queue )
        {
            var item = queue.Dequeue();

            queue.Enqueue( item );

            return item;
        }

        /// <summary>
        /// Convert the ChartJsTimeSeriesChartStyleSpecifier enumeration to a Chart.js parameter.
        /// </summary>
        /// <param name="chartStyle"></param>
        /// <returns></returns>
        private string GetChartJsStyleParameterValue( ChartJsTimeSeriesChartStyleSpecifier chartStyle )
        {
            if ( chartStyle == ChartJsTimeSeriesChartStyleSpecifier.Bar )
            {
                return "bar";
            }
            else if ( chartStyle == ChartJsTimeSeriesChartStyleSpecifier.Bubble )
            {
                return "bubble";
            }
            else
            {
                return "line";
            }
        }

        /// <summary>
        /// Gets the maximum value that will be plotted for the current set of data points.
        /// </summary>
        /// <returns></returns>
        private decimal GetMaximumDataValue()
        {
            decimal maxValue = 0;

            bool isStacked = ( this.ChartStyle == ChartJsTimeSeriesChartStyleSpecifier.StackedLine );

            if ( isStacked )
            {
                // If the datasets are stacked, the maximum value of each Y-axis category is the sum of the data values.
                var datasets = GetCategoryDatasets( this.TimeScale );

                var dataPoints = datasets.SelectMany( x => x.DataPoints );

                var categoryNames = dataPoints.Select( x => x.Category );

                foreach ( var categoryName in categoryNames )
                {
                    var localMaxValue = dataPoints.Where( x => x.Category == categoryName ).Sum( x => x.Value );

                    if ( localMaxValue > maxValue )
                    {
                        maxValue = localMaxValue;
                    }
                }
            }
            else
            {
                foreach ( var dataset in this.Datasets )
                {
                    if ( !dataset.DataPoints.Any() )
                    {
                        continue;
                    }

                    var localMaxValue = dataset.DataPoints.Max( x => x.Value );

                    if ( localMaxValue > maxValue )
                    {
                        maxValue = localMaxValue;
                    }
                }
            }

            return maxValue;
        }

        /// <summary>
        /// Creates a queue of colors to be used as the palette for the chart datasets.
        /// </summary>
        /// <returns></returns>
        private Queue<string> GetDatasetColors()
        {
            var availableColors = this.ChartColors ?? ChartJsConstants.Colors.DefaultPalette;

            var colorQueue = new Queue<string>( availableColors );

            return colorQueue;
        }

        #endregion
    }

    #region Enumerations

    /// <summary>
    /// Specifies the chart style for a value-over-time data series in Chart.js
    /// </summary>
    public enum ChartJsTimeSeriesChartStyleSpecifier
    {
        /// <summary>
        /// Chart style line
        /// </summary>
        Line = 0,

        /// <summary>
        /// Chart style bar
        /// </summary>
        Bar = 1,

        /// <summary>
        /// Chart style bubble
        /// </summary>
        Bubble = 2,

        /// <summary>
        /// Chart style stacked line
        /// </summary>
        StackedLine = 10
    }

    /// <summary>
    /// Specifies the time scale for a value-over-time data series in Chart.js
    /// </summary>
    public enum ChartJsTimeSeriesTimeScaleSpecifier
    {
        /// <summary>
        /// The time scale is automatically determined by Chart.js based on the data points.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Day time scale
        /// </summary>
        Day = 1,

        //Week = 2,

        /// <summary>
        /// Month time scale
        /// </summary>
        Month = 3,

        /// <summary>
        /// Year time scale
        /// </summary>
        Year = 4
    }

    #endregion

    #region Helper Classes and Interfaces

    /// <summary>
    /// A set of data points and configuration options for a dataset that can be plotted on a ChartJs chart.
    /// </summary>
    public class ChartJsDataset<TDataPoint>
    {
        /// <summary>
        /// The name of the dataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The color of the border or outline of the region included in this dataset.
        /// </summary>
        public string BorderColor { get; set; }

        /// <summary> 
        /// The fill color of the region described by this dataset.
        /// </summary>
        public string FillColor { get; set; }

        /// <summary>
        /// The set of data points that are used to plot the chart.
        /// </summary>
        public List<TDataPoint> DataPoints { get; set; }
    }

    /// <summary>
    /// An implementation of the Chart.js dataset for a value-over-time data series.
    /// </summary>
    public class ChartJsTimeSeriesDataset : ChartJsDataset<IChartJsTimeSeriesDataPoint>
    {
    }

    /// <summary>
    /// An implementation of the Chart.js dataset for a value-by-category data series.
    /// </summary>
    public class ChartJsCategoryValuesDataset : ChartJsDataset<IChartJsCategoryValuesDataPoint>
    {
    }

    /// <summary>
    /// A chart data point that represents a value for a specific category, suitable for use with a chart that represents a set of values summarised by categories.
    /// </summary>
    public interface IChartJsCategoryValuesDataPoint
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        string Category { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        decimal Value { get; set; }
    }

    /// <summary>
    /// A chart data point that represents a value for a specific category, and allows sorting by a specified key.
    /// </summary>
    public class ChartJsCategoryValuesDataPoint : IChartJsCategoryValuesDataPoint
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the sort key.
        /// </summary>
        /// <value>
        /// The sort key.
        /// </value>
        public string SortKey { get; set; }
    }

    /// <summary>
    /// A chart data point that represents a value at a specific instant in time, suitable for use with a value-over-time chart.
    /// </summary>
    public interface IChartJsTimeSeriesDataPoint
    {
        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        decimal Value { get; set; }
    }

    /// <summary>
    /// A chart data point that represents a value at a specific instant in time, and allows sorting by a specified key.
    /// </summary>
    public class ChartJsTimeSeriesDataPoint : IChartJsTimeSeriesDataPoint
    {
        private DateTime _DateTime = DateTime.MinValue;
        private long _DateTimeStamp = 0;

        /// <summary>
        /// The date of this data point, plotted on the X-axis.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return _DateTime;
            }
            set
            {
                _DateTime = value;

                // Set the DateTimeStamp property as a JavaScript datetime stamp (number of milliseconds elapsed since 1/1/1970 00:00:00 UTC)
                // This measure is required by the Chart.js component.
                if ( _DateTime == null )
                {
                    _DateTimeStamp = 0;
                }
                else
                {
                    _DateTimeStamp = _DateTime.Date.ToJavascriptMilliseconds();
                }
            }
        }

        /// <summary>
        /// The value of this data point, plotted on the Y-axis.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets an arbitrary sort key that can be used to sort this data point within the data set.
        /// </summary>
        public string SortKey { get; set; }
    }

    /// <summary>
    /// Defines a set of constant values that can be used with the Chart.js charting component.
    /// </summary>
    public static class ChartJsConstants
    {
        /// <summary>
        /// Default color palette
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// Hex value for the color gray
            /// </summary>
            public static readonly string Gray = "#4D4D4D";
            /// <summary>
            /// Hex value for the color blue
            /// </summary>
            public static readonly string Blue = "#5DA5DA";
            /// <summary>
            /// Hex value for the color orange
            /// </summary>
            public static readonly string Orange = "#FAA43A";
            /// <summary>
            /// Hex value for the color green
            /// </summary>
            public static readonly string Green = "#60BD68";
            /// <summary>
            /// Hex value for the color pink
            /// </summary>
            public static readonly string Pink = "#F17CB0";
            /// <summary>
            /// Hex value for the color brown
            /// </summary>
            public static readonly string Brown = "#B2912F";
            /// <summary>
            /// Hex value for the color purple
            /// </summary>
            public static readonly string Purple = "#B276B2";
            /// <summary>
            /// Hex value for the color yellow
            /// </summary>
            public static readonly string Yellow = "#DECF3F";
            /// <summary>
            /// Hex value for the color red
            /// </summary>
            public static readonly string Red = "#F15854";

            /// <summary>
            /// Get the default color palette.
            /// </summary>
            /// <remarks>
            /// Color defaults are based on recommendations in this article: http://www.mulinblog.com/a-color-palette-optimized-for-data-visualization/
            /// </remarks>
            public static List<string> DefaultPalette = new List<string> { Blue, Green, Pink, Brown, Purple, Yellow, Red, Orange, Gray };
        }
    }

    #endregion
}