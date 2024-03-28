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

namespace Rock.Chart
{
    /// <summary>
    /// A factory that produces datasets for use with Rock charts.
    /// </summary>
    /// <remarks>
    /// This factory replicates the data functions of the ChartJsTimeSeriesDataFactory,
    /// with the ChartJs-specific presentation elements removed to allow lower-level manipulation of chart data.
    /// </remarks>
    public static class ChartDataFactory
    {
        /// <summary>
        /// Gets the recommended time interval for quantizing a set of datapoints in a time series into categories.
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static ChartJsTimeSeriesTimeScaleSpecifier GetRecommendedCategoryIntervalForTimeSeries( ChartJsTimeSeriesDataset dataset )
        {
            if ( dataset?.DataPoints == null || !dataset.DataPoints.Any() )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
            }

            var minDate = dataset.DataPoints.Min( dp => dp.DateTime );
            var maxDate = dataset.DataPoints.Max( dp => dp.DateTime );

            var interval = GetRecommendedCategoryIntervalForTimeSeries( new List<DateTime> { minDate, maxDate } );
            return interval;
        }

        /// <summary>
        /// Gets the recommended time interval for quantizing a set of datapoints in a time series into categories.
        /// </summary>
        /// <param name="dataPointDateTimeValues"></param>
        /// <returns></returns>
        public static ChartJsTimeSeriesTimeScaleSpecifier GetRecommendedCategoryIntervalForTimeSeries( List<DateTime> dataPointDateTimeValues )
        {
            if ( dataPointDateTimeValues == null || !dataPointDateTimeValues.Any() )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
            }

            var minDate = dataPointDateTimeValues.Min();
            var maxDate = dataPointDateTimeValues.Max();

            if ( minDate.Year != maxDate.Year )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Year;
            }
            else if ( minDate.Month != maxDate.Month )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }
            else if ( minDate.Day != minDate.Day )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Day;
            }
            return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
        }

        /// <summary>
        /// Convert a collection of time series datasets to category-value datasets, where the categories represent discrete periods in the specified time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        /// <param name="dataset"></param>
        /// <param name="timeScale">
        /// The timescale used to separate the datapoints into categories.
        /// If not specified, the scale is determined as the best fit for the date range of the datapoints.
        /// </param>
        /// <returns></returns>
        public static ChartJsCategorySeriesDataset GetCategorySeriesFromTimeSeries( ChartJsTimeSeriesDataset dataset, ChartJsTimeSeriesTimeScaleSpecifier? timeScale = null )
        {
            const string DateFormatStringMonthYear = "MMM yyyy";
            const string DateFormatStringDayMonthYear = "d";

            var datapoints = dataset.DataPoints;

            var datasetQuantized = new ChartJsCategorySeriesDataset();

            datasetQuantized.Name = dataset.Name;
            datasetQuantized.BorderColor = dataset.BorderColor;
            datasetQuantized.FillColor = dataset.FillColor;

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                timeScale = GetRecommendedCategoryIntervalForTimeSeries( dataset );
            }

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

            return datasetQuantized;
        }

        /// <summary>
        /// Quantize a collection of time series datasets to discrete periods in the specified time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        /// <param name="dataset"></param>
        /// <param name="timeScale">
        /// The timescale used to separate the datapoints into categories.
        /// If not specified, the scale is determined as the best fit for the date range of the datapoints.
        /// </param>
        /// <returns></returns>
        public static ChartJsTimeSeriesDataset GetQuantizedTimeSeries( ChartJsTimeSeriesDataset dataset, ChartJsTimeSeriesTimeScaleSpecifier timeScale )
        {
            var datapoints = dataset.DataPoints;

            var datasetQuantized = new ChartJsTimeSeriesDataset();

            datasetQuantized.Name = dataset.Name;
            datasetQuantized.BorderColor = dataset.BorderColor;
            datasetQuantized.FillColor = dataset.FillColor;

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                timeScale = GetRecommendedCategoryIntervalForTimeSeries( dataset );
            }

            List<ChartJsTimeSeriesDataPoint> quantizedDataPoints;

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
            {
                quantizedDataPoints = datapoints
                    .GroupBy( x => new { Day = x.DateTime } )
                    .Select( x => new ChartJsTimeSeriesDataPoint
                    {
                        DateTime = x.Key.Day,
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Day.ToString( "yyyyMMdd" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
            {
                quantizedDataPoints = datapoints
                    .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
                    .Select( x => new ChartJsTimeSeriesDataPoint
                    {
                        DateTime = x.Key.Month,
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Month.ToString( "yyyyMM" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
            {
                quantizedDataPoints = datapoints
                    .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
                    .Select( x => new ChartJsTimeSeriesDataPoint
                    {
                        DateTime = x.Key.Year,
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Year.ToString( "yyyy" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();
            }
            else
            {
                // Get the sum of all datapoints.
                quantizedDataPoints = datapoints
                    .Select( x => new ChartJsTimeSeriesDataPoint
                    {
                        DateTime = datapoints.Max( p => p.DateTime ),
                        Value = datapoints.Sum( p => p.Value ),
                        SortKey = dataset.Name
                    } )
                    .ToList();
            }

            datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsTimeSeriesDataPoint>().ToList();

            return datasetQuantized;
        }

        /// <summary>
        /// Create a category series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        public static List<ChartJsCategorySeriesDataset> GetCategorySeriesFromChartData( IEnumerable<IChartData> chartDataItems, string defaultSeriesName = null )
        {
            if ( string.IsNullOrWhiteSpace( defaultSeriesName ) )
            {
                defaultSeriesName = "(unknown)";
            }

            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );

            var firstItem = chartDataItems.FirstOrDefault();
            var isChartJsDataPoint = firstItem is IChartJsCategorySeriesDataPoint;
            var datasets = new List<ChartJsCategorySeriesDataset>();
            foreach ( var series in itemsBySeries )
            {
                var categoryName = string.IsNullOrWhiteSpace( series.Key ) ? defaultSeriesName : series.Key;
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
                            Category = categoryName,
                            Value = x.YValue ?? 0,
                        } )
                        .ToList();
                }
                var dataset = new ChartJsCategorySeriesDataset
                {
                    Name = categoryName,
                    DataPoints = dataPoints
                };

                datasets.Add( dataset );
            }

            return datasets;
        }

        /// <summary>
        /// Create a time series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        public static List<ChartJsTimeSeriesDataset> GetTimeSeriesFromChartData( IEnumerable<IChartData> chartDataItems, string defaultSeriesName )
        {
            var firstItem = chartDataItems.FirstOrDefault();
            var isChartJsDataPoint = firstItem is IChartJsTimeSeriesDataPoint;

            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );
            var timeDatasets = new List<ChartJsTimeSeriesDataset>();
            foreach ( var series in itemsBySeries )
            {
                List<IChartJsTimeSeriesDataPoint> dataPoints;
                if ( isChartJsDataPoint )
                {
                    dataPoints = series.Cast<IChartJsTimeSeriesDataPoint>().ToList();
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
                    Name = string.IsNullOrWhiteSpace( series.Key ) ? defaultSeriesName : series.Key,
                    DataPoints = dataPoints
                };

                timeDatasets.Add( dataset );
            }

            return timeDatasets;
        }

        internal static DateTime GetDateTimeFromJavascriptMilliseconds( long millisecondsAfterEpoch )
        {
            return new DateTime( 1970, 1, 1 ).AddTicks( millisecondsAfterEpoch * 10000 );
        }
    }
}