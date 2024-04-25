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
using Newtonsoft.Json;

namespace Rock.Chart
{
    /// <summary>
    /// Creates data structures suitable for plotting a value-over-Category data series on a Cartesian grid using ChartJs.
    /// </summary>
    /// <remarks>
    /// This factory can generate the following data formats:
    /// * A JSON object compatible with the ChartJs constructor 'data' parameter: new Chart([chartContainer], [data]);
    /// 
    /// NOTE: For future development, this factory should be superseded by new factories that are style-specific - ChartJsLineChartDataFactory and ChartJsBarChartDataFactory.
    /// Each chart style may need to process specific data types in a different way.
    /// Refer to the ChartJsPieChartDataFactory for an example of the preferred implementation - it handles both category and time series data.
    /// </remarks>
    public class ChartJsCategorySeriesDataFactory<TDataPoint> : ChartJsDataFactory
        where TDataPoint : IChartJsCategorySeriesDataPoint
    {
        private List<ChartJsCategorySeriesDataset> _Datasets = new List<ChartJsCategorySeriesDataset>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartJsCategorySeriesDataFactory{TDataPoint}"/> class.
        /// </summary>
        public ChartJsCategorySeriesDataFactory()
        {
            this.Datasets = new List<ChartJsCategorySeriesDataset>();
        }

        #region Properties

        /// <summary>
        /// The style of chart to display.
        /// </summary>
        public ChartJsCategorySeriesChartStyleSpecifier ChartStyle { get; set; } = ChartJsCategorySeriesChartStyleSpecifier.Line;

        /// <summary>
        /// A collection of data points that are displayed on the chart as one or more series of data.
        /// </summary>
        public List<ChartJsCategorySeriesDataset> Datasets
        {
            get { return _Datasets; }
            set
            {
                _Datasets = value ?? new List<ChartJsCategorySeriesDataset>();
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
            return GetJson( new ChartJsCategorySeriesDataFactory.GetJsonArgs
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
        public string GetJson( ChartJsCategorySeriesDataFactory.GetJsonArgs args )
        {
            this.MaintainAspectRatio = args.MaintainAspectRatio;
            this.SizeToFitContainerWidth = args.SizeToFitContainerWidth;

            // Create the data structure for Chart.js parameter "data.datasets".
            var chartData = GetChartDataJsonObjectForSpecificCategoryScale( args );

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

            var isStacked = ( this.ChartStyle == ChartJsCategorySeriesChartStyleSpecifier.StackedLine );

            // The suggested scale is from 0 to the maximum value in the data set, +10% to allow for a top margin.
            var suggestedMax = Math.Ceiling( maxValue * 1.1M );

            // Create the data structure for Chart.js parameter "options".
            var optionsLegend = this.GetLegendConfigurationObject( args.LegendPosition, args.LegendAlignment, args.DisplayLegend );
            var tooltipsConfiguration = this.GetTooltipsConfigurationObject( args.ContainerControlId, args.YValueFormatString );
            var optionsYaxis = this.GetYAxisConfigurationObject( args.YValueFormatString, suggestedMax, stepSize, isStacked );

            var optionsData = new
            {
                maintainAspectRatio = this.MaintainAspectRatio,
                responsive = this.SizeToFitContainerWidth,
                animation = new { duration = args.DisableAnimation ? 0 : 1000 },
                legend = optionsLegend,
                tooltips = tooltipsConfiguration,
                scales = new { yAxes = optionsYaxis }
            };

            // Create the data structure for Chartjs parameter "chart".
            var chartStyle = GetChartJsStyleParameterValue( this.ChartStyle );

            var chartStructure = new { type = chartStyle, data = chartData, options = optionsData };

            // Return the JSON representation of the Chart.js data structure.
            return SerializeJsonObject( chartStructure );
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
        protected dynamic GetChartDataJsonObjectForSpecificCategoryScale( ChartJsCategorySeriesDataFactory.GetJsonArgs args )
        {
            var categoryNames = this.Datasets.SelectMany( x => x.DataPoints ).Select( x => x.Category ).ToList();
            var colorGenerator = new ChartColorPaletteGenerator( this.ChartColors );

            var jsDatasets = new List<object>();

            foreach ( var dataset in this.Datasets )
            {
                // Create a sequence of datapoints, ensuring there is a value for each of the categories.
                dynamic dataValues;
                dynamic jsDataset;

                if ( this.ChartStyle == ChartJsCategorySeriesChartStyleSpecifier.Pie )
                {
                    throw new Exception( "Use ChartJsPieChartDataFactory instead." );
                }
                else
                {
                    dataValues = GetDataPointsForAllCategories( dataset, categoryNames );

                    var borderColorString = dataset.BorderColor;
                    var fillColorString = dataset.FillColor;

                    GetDatasetColorSettings( ref borderColorString, ref fillColorString, out var fillStyle );

                    jsDataset = new
                    {
                        label = dataset.Name,
                        borderColor = borderColorString,
                        borderWidth = 2,
                        backgroundColor = fillColorString,
                        fill = fillStyle,
                        lineTension = args.LineTension,
                        data = dataValues
                    };
                }

                jsDatasets.Add( jsDataset );
            }

            var chartData = new { datasets = jsDatasets, labels = categoryNames };

            return chartData;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Get a sequence of datapoints corresponding to a specific category, ensuring there is a value for each of the categories.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="categoryNames"></param>
        /// <returns></returns>
        protected List<decimal> GetDataPointsForAllCategories( ChartJsCategorySeriesDataset dataset, List<string> categoryNames )
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
        /// Convert the ChartJsCategorySeriesChartStyleSpecifier enumeration to a Chart.js parameter.
        /// </summary>
        /// <param name="chartStyle"></param>
        /// <returns></returns>
        private string GetChartJsStyleParameterValue( ChartJsCategorySeriesChartStyleSpecifier chartStyle )
        {
            if ( chartStyle == ChartJsCategorySeriesChartStyleSpecifier.Bar )
            {
                return "bar";
            }
            else if ( chartStyle == ChartJsCategorySeriesChartStyleSpecifier.Bubble )
            {
                return "bubble";
            }
            else if ( chartStyle == ChartJsCategorySeriesChartStyleSpecifier.Pie )
            {
                return "pie";
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
        protected decimal GetMaximumDataValue()
        {
            decimal maxValue = 0;

            bool isStacked = ( this.ChartStyle == ChartJsCategorySeriesChartStyleSpecifier.StackedLine );

            if ( isStacked )
            {
                // If the datasets are stacked, the maximum value of each Y-axis category is the sum of the data values.
                var dataPoints = this.Datasets.SelectMany( x => x.DataPoints );

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

        #region Obsolete

        /// <summary>
        /// Get the chart configuration in JSON format that is compatible for use with the Chart.js component.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [Obsolete( "Use the GetJson(ChartJsCategorySeriesDataFactory.GetJsonArgs args) method instead." )]
        [RockObsolete( "1.14" )]
        public string GetChartDataJson( ChartJsDataFactory.GetJsonArgs args )
        {
            return GetJson( ( ChartJsCategorySeriesDataFactory.GetJsonArgs ) args );
        }

        #endregion
    }

    #region Enumerations

    /// <summary>
    /// Specifies the chart style for a value-over-Category data series in Chart.js
    /// </summary>
    public enum ChartJsCategorySeriesChartStyleSpecifier
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
        StackedLine = 10,

        /// <summary>
        /// Pie chart
        /// </summary>
        Pie = 11,
    }

    #endregion

    #region Helper Classes and Interfaces

    /// <summary>
    /// Provides helper classes in a non-generic context for the generic factory ChartJsCategorySeriesDataFactory.
    /// </summary>
    public static class ChartJsCategorySeriesDataFactory
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
    /// An implementation of the Chart.js dataset for a value-by-category data series.
    /// </summary>
    public class ChartJsCategorySeriesDataset : ChartJsDataset<IChartJsCategorySeriesDataPoint>
    {
    }

    #endregion
}