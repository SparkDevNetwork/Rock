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
using Rock.Model;

namespace Rock.Chart
{
    /// <summary>
    /// Creates data structures suitable for plotting a value-over-time data series on a Cartesian grid using ChartJs.
    /// </summary>
    /// <remarks>
    /// This factory can generate the following data formats:
    /// * A JSON object compatible with the ChartJs constructor 'data' parameter: new Chart([chartContainer], [data]);
    /// * A Lava Chart Shortcode text block.
    ///
    /// NOTE: For future development, this factory should be superseded by new factories that are style-specific - ChartJsLineChartDataFactory and ChartJsBarChartDataFactory.
    /// Each chart style may need to process specific data types in a different way.
    /// Refer to the ChartJsPieChartDataFactory for an example of the preferred implementation - it handles both category and time series data.
    /// </remarks>
    public class ChartJsTimeSeriesDataFactory<TDataPoint> : ChartJsDataFactory
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
        /// Create the data structure for Chart.js parameter "data.datasets".
        /// </summary>
        /// <returns></returns>
        public string GetChartDataJson( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            var chartData = GetChartDataJsonObject( args );
            return SerializeJsonObject( chartData );
        }

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public string GetJson( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            this.MaintainAspectRatio = args.MaintainAspectRatio;
            this.SizeToFitContainerWidth = args.SizeToFitContainerWidth;

            // Create the data structure for Chart.js parameter "data.datasets".
            dynamic chartData = GetChartDataJsonObject( args );

            //
            // Get options for the X-axis, showing the time.
            //
            dynamic optionsXaxis;

            if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                // Allow Chart.js to scale the X-axis to best fit.
                optionsXaxis = this.GetChartXaxisOptionsForAutoTimeScale();
            }
            else
            {
                // Display discrete time periods on the X-axis using the category labels defined in the chart data.
                optionsXaxis = null;
            }

            //
            // Get options for the Y-axis, showing the values.
            //

            // Prevent Chart.js from displaying decimal values in the y-axis by forcing the step size to 1 if the value range is below 10.
            var maxValue = GetMaximumDataValue();
            decimal? stepSize = null;
            if ( maxValue < 10 )
            {
                stepSize = 1;
            }

            var isStacked = ( this.ChartStyle == ChartJsTimeSeriesChartStyleSpecifier.StackedLine );

            // The suggested scale is from 0 to the maximum value in the data set, +10% to allow for a top margin.
            var suggestedMax = Math.Ceiling( maxValue * 1.1M );

            var optionsYaxis = this.GetYAxisConfigurationObject( args.YValueFormatString, suggestedMax, stepSize, isStacked );

            // Create the data structure for Chart.js parameter "options".
            var optionsLegend = this.GetLegendConfigurationObject( args.LegendPosition, args.LegendAlignment, args.DisplayLegend );
            var tooltipsConfiguration = this.GetTooltipsConfigurationObject( args.ContainerControlId, args.YValueFormatString );
            var optionsData = new
            {
                maintainAspectRatio = this.MaintainAspectRatio,
                responsive = this.SizeToFitContainerWidth,
                animation = new { duration = args.DisableAnimation ? 0 : 1000 },
                legend = optionsLegend,
                tooltips = tooltipsConfiguration,
                scales = new
                {
                    xAxes = optionsXaxis,
                    yAxes = optionsYaxis
                }
            };

            // Create the data structure for ChartJS parameter "chart".
            string chartStyle = GetChartJsStyleParameterValue( this.ChartStyle );

            dynamic chartStructure = new { type = chartStyle, data = chartData, options = optionsData };

            // Return the JSON representation of the Chart.js data structure.
            return SerializeJsonObject( chartStructure );
        }

        private dynamic GetChartDataJsonObject( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
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

            return chartData;
        }

        /// <summary>
        /// Get a JSON data structure that represents Chart.js options to show auto-scaled time intervals on the X-axis.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private dynamic GetChartJsonObjectForAutoTimeScale( ChartJsTimeSeriesDataFactory.GetJsonArgs args )
        {
            var jsDatasets = new List<object>();
            var colorGenerator = this.GetChartColorGenerator();

            foreach ( var dataset in this.Datasets )
            {
                // Create the datapoints used by ChartJs to construct the chart.
                // Include the source datapoint model in the "customData" property so it can be accessed
                // for constructing tooltips and responding to click events.
                var dataPoints = dataset.DataPoints.Select( dp => new
                {
                    x = dp.DateTime.ToISO8601DateString(),
                    y = dp.Value,
                    customData = dp
                } )
                .OrderBy( dp => dp.x )
                .ToList();
                var borderColorString = dataset.BorderColor;
                var fillColorString = dataset.FillColor;

                GetDatasetColorSettings( ref borderColorString, ref fillColorString, out var fillStyle );

                var jsDataset = new
                {
                    label = dataset.Name,
                    borderColor = borderColorString,
                    backgroundColor = fillColorString,
                    fill = fillStyle,
                    lineTension = args.LineTension,
                    data = dataPoints
                };

                jsDatasets.Add( jsDataset );
            }

            var chartData = new { datasets = jsDatasets };
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
            var colorGenerator = new ChartColorPaletteGenerator( this.ChartColors );

            var jsDatasets = new List<object>();
            foreach ( var dataset in datasets )
            {
                // Create a sequence of datapoints, ensuring there is a value for each of the categories.
                var dataPoints = GetDataPointsForAllCategories( dataset, categoryNames );
                var borderColorString = dataset.BorderColor;
                var fillColorString = dataset.FillColor;

                GetDatasetColorSettings( ref borderColorString, ref fillColorString, out var fillStyle );

                var jsDataset = new
                {
                    label = dataset.Name,
                    borderColor = borderColorString,
                    backgroundColor = fillColorString,
                    fill = fillStyle,
                    lineTension = args.LineTension,
                    data = dataPoints
                };

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
            var unit = "month";

            // TODO: Calculate the appropriate unit.

            if ( this.StartDateTime != null )
            {
                minDate = this.StartDateTime.Value.ToJavascriptMilliseconds();
            }

            if ( this.EndDateTime != null )
            {
                maxDate = this.EndDateTime.Value.ToJavascriptMilliseconds();
            }

            // Allow Chart.js to scale the X-axis to best fit.
            var dateFormat = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
            dateFormat = dateFormat.Replace( "d", "D" ).Replace( "y", "Y" );

            dynamic optionsXaxes = new List<object>()
            {
                new
                {
                    type = "time",
                    unit = unit,
                    round = unit,
                    time = new { tooltipFormat = dateFormat, min = minDate, max = maxDate },
                }
            };

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
            var colorGenerator = new ChartColorPaletteGenerator( this.ChartColors );

            sbLava.AppendFormat( "{{[ chart type:'line' labels:'{0}' legendshow:'true' chartheight:'{1}px' ]}}", categoryNames.AsDelimited( "," ), chartHeight );

            foreach ( var dataset in datasets )
            {
                // Use the color specifically assigned to this dataset, or get the next color from the queue.
                string borderColor = dataset.BorderColor;

                if ( string.IsNullOrWhiteSpace( borderColor ) )
                {
                    borderColor = colorGenerator.GetNextColor().ToRGBA();
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
        private List<ChartJsCategorySeriesDataPoint> GetTimescaleCategories( ChartJsTimeSeriesTimeScaleSpecifier timeScale )
        {
            // Determine the date range.
            var allDataPoints = this.Datasets.SelectMany( x => x.DataPoints );

            if ( !allDataPoints.Any() )
            {
                return new List<ChartJsCategorySeriesDataPoint>();
            }

            var startDate = this.StartDateTime ?? allDataPoints.Min( x => x.DateTime );
            var endDate = this.EndDateTime ?? allDataPoints.Max( x => x.DateTime );

            var thisDate = startDate;

            var categoryDataPoints = new List<ChartJsCategorySeriesDataPoint>();

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
            {
                // To test for the last date of the reporting period, get the next day.
                var lastDateNextDay = endDate.AddDays( 1 );

                while ( thisDate < lastDateNextDay )
                {
                    var categoryDataPoint = new ChartJsCategorySeriesDataPoint() { Category = thisDate.ToString( DateFormatStringDayMonthYear ), SortKey = thisDate.ToString( "yyyyMMdd" ) };

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
                    var categoryDataPoint = new ChartJsCategorySeriesDataPoint() { Category = thisDate.ToString( DateFormatStringMonthYear ), SortKey = thisDate.ToString( "yyyyMM" ) };

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
                    var categoryDataPoint = new ChartJsCategorySeriesDataPoint() { Category = thisDate.ToString( "yyyy" ), SortKey = thisDate.ToString( "yyyy" ) };

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
        private List<dynamic> GetDataPointsForAllCategories( ChartJsCategorySeriesDataset dataset, List<string> categoryNames )
        {
            var dataValues = new List<dynamic>();

            foreach ( var categoryName in categoryNames )
            {
                var datapoint = dataset.DataPoints.FirstOrDefault( x => x.Category == categoryName );

                var dataValue = new
                {
                    x = categoryName,
                    y = datapoint?.Value ?? 0,
                    customData = datapoint
                };

                dataValues.Add( dataValue );
            }

            return dataValues;
        }

        /// <summary>
        /// Convert a collection of time series datasets to category-value datasets, where the categories represent discrete periods in the specified time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        private List<ChartJsCategorySeriesDataset> GetCategoryDatasets( ChartJsTimeSeriesTimeScaleSpecifier timeScale )
        {
            var quantizedDatasets = new List<ChartJsCategorySeriesDataset>();

            foreach ( var dataset in this.Datasets )
            {
                var datapoints = dataset.DataPoints;

                var datasetQuantized = new ChartJsCategorySeriesDataset();

                datasetQuantized.Name = dataset.Name;
                datasetQuantized.BorderColor = dataset.BorderColor;
                datasetQuantized.FillColor = dataset.FillColor;

                if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
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

                quantizedDatasets.Add( datasetQuantized );
            }

            return quantizedDatasets;
        }

        ///// <summary>
        ///// Return the datasets with all data points quantized according to the time scale.
        ///// </summary>
        ///// <remarks>
        ///// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        ///// </remarks>
        //private List<ChartJsTimeSeriesDataset> GetTimescaleDatasets()
        //{
        //    var quantizedDatasets = new List<ChartJsTimeSeriesDataset>();

        //    foreach ( var dataset in this.Datasets )
        //    {
        //        var datapoints = dataset.DataPoints;

        //        var datasetQuantized = new ChartJsTimeSeriesDataset();

        //        datasetQuantized.Name = dataset.Name;
        //        datasetQuantized.BorderColor = dataset.BorderColor;
        //        datasetQuantized.FillColor = dataset.FillColor;

        //        if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
        //        {
        //            var quantizedDataPoints = datapoints
        //                .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
        //                .Select( x => new ChartJsTimeSeriesDataPoint
        //                {
        //                    DateTime = x.Key.Month,
        //                    Value = x.Sum( y => y.Value )
        //                } )
        //                .OrderBy( x => x.DateTime )
        //                .ToList();

        //            datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsTimeSeriesDataPoint>().ToList();
        //        }
        //        else if ( this.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
        //        {
        //            var quantizedDataPoints = datapoints
        //                .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
        //                .Select( x => new ChartJsTimeSeriesDataPoint
        //                {
        //                    DateTime = x.Key.Year,
        //                    Value = x.Sum( y => y.Value )
        //                } )
        //                .OrderBy( x => x.DateTime )
        //                .ToList();

        //            datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsTimeSeriesDataPoint>().ToList();
        //        }
        //        else
        //        {
        //            throw new NotImplementedException( "Timescale is not implemented" );
        //        }

        //        quantizedDatasets.Add( datasetQuantized );
        //    }

        //    return quantizedDatasets;
        //}

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

        /// <summary>
        /// Month time scale
        /// </summary>
        Month = 3,

        /// <summary>
        /// Year time scale
        /// </summary>
        Year = 4,
    }

    #endregion

    #region Helper Classes and Interfaces

    /// <summary>
    /// Provides helper classes in a non-generic context for the generic factory ChartJsTimeSeriesDataFactory.
    /// </summary>
    public static class ChartJsTimeSeriesDataFactory
    {
        /// <summary>
        /// Args for the GetJson method
        /// </summary>
        public sealed class GetJsonArgs : ChartJsDataFactory.GetJsonArgs
        {
            // Add any arguments specific to this chart factory here.
        }
    }

    /// <summary>
    /// An implementation of the Chart.js dataset for a value-over-time data series.
    /// </summary>
    public class ChartJsTimeSeriesDataset : ChartJsDataset<IChartJsTimeSeriesDataPoint>
    {
    }

    #endregion
}