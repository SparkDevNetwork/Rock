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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Reporting.TithingOverview;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays the details of a particular achievement type.
    /// </summary>
    [DisplayName( "Tithing Overview" )]
    [Category( "Reporting" )]
    [Description( "Shows high-level statistics of the tithing overview." )]
    [IconCssClass( "fa fa-question" )]
    //[SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "1e44b061-7767-487d-a98f-16912e8c7de7" )]
    [SystemGuid.BlockTypeGuid( "db756565-8a35-42e2-bc79-8d11f57e4004" )]
    public class TithingOverview : RockBlockType
    {
        #region Fields

        /// <summary>
        /// The available colors for the charts
        /// </summary>
        private readonly List<string> _availableColors = new List<string>()
        {
             "#0C4A6E",  // dataviz-info-900
             "#075985",  // dataviz-info-800
             "#0369A1",  // dataviz-info-700
             "#0284C7",  // dataviz-info-600
             "#0EA5E9",  // dataviz-info-500
             "#38BDF8",  // dataviz-info-400
             "#E0F2FE",  // dataviz-info-100
             "#BAE6FD",  // dataviz-info-200
             "#7DD3FC",  // dataviz-info-300
        };
        private List<ChartDatasetInfo> _givingHouseHoldsMetricValues;
        private List<ChartDatasetInfo> _tithingHouseHoldsMetricValues;
        private List<ChartDatasetInfo> _tithingOverviewMetricValues;

        #endregion

        #region Keys

        private static class ChartTypeKey
        {
            public const string BarChart = "bar";
            public const string LineChart = "line";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = GetInitializationBox( RockContext, ChartTypeKey.BarChart );
                return box;
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box.
        /// </summary>
        private TithingOverviewInitializationBox GetInitializationBox( RockContext rockContext, string chartType )
        {
            var json = GetChartData( rockContext, chartType );

            var box = new TithingOverviewInitializationBox
            {
                ChartDataJson = json,
                ChartType = chartType,
                ToolTipData = GetToolTipData( rockContext, chartType )
            };

            return box;
        }

        #region Line Chart

        /// <summary>
        /// Gets the line chart factory data arguments.
        /// </summary>
        /// <returns></returns>
        private static ChartJsTimeSeriesDataFactory.GetJsonArgs GetLineChartDataArgs()
        {
            return new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                DisplayLegend = true,
                LineTension = 0,
                MaintainAspectRatio = false,
                SizeToFitContainerWidth = true
            };
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the line chart.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetLineChartDataFactory( RockContext rockContext )
        {
            var chartFactory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>
            {
                Datasets = GetTimeSeriesDataset( rockContext ),
                ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line,
                TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Week,
                AreaFillOpacity = 0
            };

            return chartFactory;
        }

        /// <summary>
        /// Gets the dataset for the line chart.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ChartJsTimeSeriesDataset> GetTimeSeriesDataset( RockContext rockContext )
        {
            var datasets = new List<ChartJsTimeSeriesDataset>();

            var dataPoints = GetTithingOverviewMetricValues( rockContext, ChartTypeKey.LineChart );

            var dataSeriesDatasets = dataPoints
                .Select( x => x.MetricValueCampusIds )
                .Distinct();

            var seriesNameKeyValue = new Dictionary<string, string>();
            foreach ( var dataSeriesDataset in dataSeriesDatasets )
            {
                var seriesNamValue = GetSeriesPartitionName( dataSeriesDataset );
                seriesNameKeyValue.Add( dataSeriesDataset, seriesNamValue );
            }

            foreach ( var dataSeriesName in seriesNameKeyValue.Keys )
            {
                var name = seriesNameKeyValue[dataSeriesName];

                var campusId = dataPoints.Find( d => d.MetricValueCampusIds == dataSeriesName )?.CampusId ?? 0;
                var campus = CampusCache.Get( campusId );
                var color = GetFillColor( campus, ChartTypeKey.LineChart );

                var dataset = new ChartJsTimeSeriesDataset
                {
                    Name = name,
                    FillColor = string.Empty,
                    BorderColor = color,
                    DataPoints = dataPoints
                        .Where( x => x.MetricValueCampusIds == dataSeriesName )
                        .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.Value ?? 0 } )
                        .Cast<IChartJsTimeSeriesDataPoint>()
                        .ToList()
                };

                datasets.Add( dataset );
            }

            return datasets;
        }

        #endregion

        #region Bar Chart

        /// <summary>
        /// Gets the bar chart data arguments.
        /// </summary>
        /// <returns></returns>
        private static ChartJsCategorySeriesDataFactory.GetJsonArgs GetBarChartDataArgs()
        {
            return new ChartJsCategorySeriesDataFactory.GetJsonArgs
            {
                DisplayLegend = true,
                LineTension = 0.4m,
                MaintainAspectRatio = false,
                SizeToFitContainerWidth = true
            };
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the bar chart.
        /// </summary>
        public ChartJsCategorySeriesDataFactory<ChartJsCategorySeriesDataPoint> GetBarChartDataFactory( RockContext rockContext )
        {
            var chartFactory = new ChartJsCategorySeriesDataFactory<ChartJsCategorySeriesDataPoint>
            {
                Datasets = GetCategorySeriesDataset( rockContext ),
                ChartStyle = ChartJsCategorySeriesChartStyleSpecifier.Bar,
            };

            return chartFactory;
        }

        /// <summary>
        /// Gets the dataset for the bar chart.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ChartJsCategorySeriesDataset> GetCategorySeriesDataset( RockContext rockContext )
        {
            var datasets = new List<ChartJsCategorySeriesDataset>();

            var dataPoints = GetTithingOverviewMetricValues( rockContext, ChartTypeKey.BarChart );

            var dataSeriesDatasets = dataPoints
                .Select( x => x.MetricValueCampusIds )
                .Distinct();

            var seriesNameKeyValue = new Dictionary<string, string>();
            foreach ( var dataSeriesDataset in dataSeriesDatasets )
            {
                var seriesNameValue = GetSeriesPartitionName( dataSeriesDataset );
                seriesNameKeyValue.Add( dataSeriesDataset, seriesNameValue );
            }

            foreach ( var dataSeriesName in seriesNameKeyValue.Keys )
            {
                var name = seriesNameKeyValue[dataSeriesName];

                var campusId = dataPoints.Find( d => d.MetricValueCampusIds == dataSeriesName )?.CampusId ?? 0;
                var campus = CampusCache.Get( campusId );
                var color = GetFillColor( campus, ChartTypeKey.BarChart );

                var dataset = new ChartJsCategorySeriesDataset
                {
                    Name = name,
                    FillColor = color,
                    BorderColor = color,
                    DataPoints = dataPoints
                        .Where( x => x.MetricValueCampusIds == dataSeriesName )
                        .Select( x => new ChartJsCategorySeriesDataPoint { Category = name, Value = x.Value ?? 0 } )
                        .Cast<IChartJsCategorySeriesDataPoint>()
                        .ToList()
                };

                datasets.Add( dataset );
            }

            return datasets;
        }

        #endregion

        #region Metrics

        /// <summary>
        /// Gets the number of giving households metric values partitioned by campus, the values are used as the denominator when calculating the tithing percentage of each campus.
        /// </summary>
        /// <param name="rockContext">The rock context</param>
        /// <param name="chartType">The current chart type configuration</param>
        /// <returns></returns>
        private List<ChartDatasetInfo> GetGivingHouseholdsMetricValues( RockContext rockContext, string chartType )
        {
            if ( _givingHouseHoldsMetricValues == null )
            {
                var metricGuid = SystemGuid.Metric.GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID.AsGuid();
                var query = GetMetricValues( rockContext, metricGuid );

                query = FilterMetricValues( query, chartType, metricGuid );
                var metricValues = query.ToList();

                _givingHouseHoldsMetricValues = metricValues.ConvertAll( m => new ChartDatasetInfo()
                {
                    CampusId = m.MetricValuePartitions.FirstOrDefault()?.EntityId,
                    Value = m.YValue,
                    DateTime = m.MetricValueDateKey.Value.GetDateKeyDate(),
                } );
            }

            return _givingHouseHoldsMetricValues;
        }

        /// <summary>
        /// Gets the number of tithing households metric values partitioned by campus, the values are used as the numerator when calculating the tithing percentage of each campus.
        /// </summary>
        /// <param name="rockContext">The rock context</param>
        /// <param name="chartType">The current chart type configuration</param>
        /// <returns></returns>
        private List<ChartDatasetInfo> GetTithingHouseholdsMetricValues( RockContext rockContext, string chartType )
        {
            if ( _tithingHouseHoldsMetricValues == null )
            {
                var metricGuid = SystemGuid.Metric.TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID.AsGuid();
                var query = GetMetricValues( rockContext, metricGuid );

                query = FilterMetricValues( query, chartType, metricGuid );
                var metricValues = query.ToList();

                _tithingHouseHoldsMetricValues = metricValues.ConvertAll( m => new ChartDatasetInfo()
                {
                    CampusId = m.MetricValuePartitions.FirstOrDefault()?.EntityId,
                    Value = m.YValue,
                    DateTime = m.MetricValueDateKey.Value.GetDateKeyDate(),
                } );
            }

            return _tithingHouseHoldsMetricValues;
        }

        /// <summary>
        /// Gets the percentage of tithing families metric values partitioned by campus,
        /// the value is derived from dividing the number of tithing households by the number of giving households in a given campus.
        /// </summary>
        /// <param name="rockContext">The rock context</param>
        /// <param name="chartType">The current chart type configuration</param>
        /// <returns></returns>
        private List<ChartDatasetInfo> GetTithingOverviewMetricValues( RockContext rockContext, string chartType )
        {
            if ( _tithingOverviewMetricValues == null )
            {
                var metricGuid = SystemGuid.Metric.TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID.AsGuid();
                var query = GetMetricValues( rockContext, metricGuid );

                query = FilterMetricValues( query, chartType, SystemGuid.Metric.TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID.AsGuid() );

                var metricValues = query.ToList();

                _tithingOverviewMetricValues = metricValues
                    .Where( a => a.YValue.HasValue )
                    .GroupBy( x => new
                    {
                        DateKey = x.MetricValueDateKey.Value,
                        x.MetricValueType,
                        x.MetricValuePartitionEntityIds,
                    } )
                    .Select( x => new
                    {
                        x.Key,
                        Value = x.Select( a => a.YValue.Value ),
                        PartitionEntityId = x.FirstOrDefault()?.MetricValuePartitions.Select( p => p.EntityId ).FirstOrDefault()
                    } )
                    .Select( x => new ChartDatasetInfo
                    {
                        MetricValueCampusIds = x.Key.MetricValuePartitionEntityIds,
                        DateTime = x.Key.DateKey.GetDateKeyDate(), // +1 to get first day of month
                        CampusId = x.PartitionEntityId,
                    } )
                    .ToList();

                foreach ( var dataset in _tithingOverviewMetricValues )
                {
                    dataset.Value = GetPercentValue( rockContext, dataset, chartType );
                }
            }

            return _tithingOverviewMetricValues;
        }

        /// <summary>
        /// Filters the specified metric values by date based in the current chart type configuration.
        /// When in Line mode, data over the current year is displayed, when in Bar mode data from the last run date is displayed.
        /// </summary>
        /// <param name="metricValuesQry">The specified metric value, can be the GivingHousehold, TithingHousehold or TithingOverview metric value.</param>
        /// <param name="chartType">The current chart type configuration, determines the date used for filtering.</param>
        /// <param name="metricGuid">The metric whose values are to be pulled from the database.</param>
        /// <returns></returns>
        private IQueryable<MetricValue> FilterMetricValues( IQueryable<MetricValue> metricValuesQry, string chartType, Guid metricGuid )
        {
            if ( chartType == ChartTypeKey.LineChart )
            {
                var endDate = RockDateTime.Now.Date.AddDays( 1 );
                var startDate = RockDateTime.Now.AddMonths( -12 ).Date;

                metricValuesQry = metricValuesQry.Where( a => a.MetricValueDateTime >= startDate && a.MetricValueDateTime <= endDate );
            }
            else
            {
                var lastRunDate = new MetricValueService( RockContext ).Queryable()
                    .Where( m => m.Metric.Guid == metricGuid )
                    .OrderByDescending( m => m.MetricValueDateTime )
                    .Select( m => m.MetricValueDateTime )
                    .FirstOrDefault();

                if ( lastRunDate.HasValue )
                {
                    lastRunDate = lastRunDate.Value.Date;
                    metricValuesQry = metricValuesQry.Where( m => DbFunctions.TruncateTime( m.MetricValueDateTime ) == lastRunDate );
                }
            }

            return metricValuesQry;
        }

        /// <summary>
        /// Calculates the Tithing Overview metric value as a percentage by dividing the TithingHouseholds metric value by the GivingHouseholds metric value.
        /// </summary>
        /// <param name="rockContext">The RockContext.</param>
        /// <param name="datasetInfo">The TithingOverview dataset.</param>
        /// <param name="chartType">The current chart type configuration.</param>
        /// <returns></returns>
        private decimal? GetPercentValue( RockContext rockContext, ChartDatasetInfo datasetInfo, string chartType )
        {
            var givingHouseHoldsMetricValues = GetGivingHouseholdsMetricValues( rockContext, chartType );
            var tithingHouseHoldsMetricValues = GetTithingHouseholdsMetricValues( rockContext, chartType );

            var givingHouseHolds = givingHouseHoldsMetricValues.Find( d => d.CampusId == datasetInfo.CampusId && d.DateTime.Date == datasetInfo.DateTime.Date );
            var tithingHouseHolds = tithingHouseHoldsMetricValues.Find( d => d.CampusId == datasetInfo.CampusId && d.DateTime.Date == datasetInfo.DateTime.Date );

            var percentValue = givingHouseHolds == null || tithingHouseHolds == null ? 0 : ( tithingHouseHolds.Value / givingHouseHolds.Value ) * 100;
            return Math.Round( percentValue ?? 0, 1 );
        }

        /// <summary>
        /// Gets the Metric Values for the specified metric.
        /// </summary>
        /// <param name="rockContext">The RockContext.</param>
        /// <param name="metricGuid">The specified metric.</param>
        /// <returns></returns>
        private IQueryable<MetricValue> GetMetricValues( RockContext rockContext, Guid metricGuid )
        {
            var metricValuesQry = new MetricValueService( rockContext )
                .Queryable()
                .Include( a => a.MetricValuePartitions.Select( b => b.MetricPartition ) )
                .Where( a => a.Metric.Guid == metricGuid );

            return metricValuesQry.OrderBy( a => a.MetricValueDateTime );
        }

        #endregion

        #region Chart

        /// <summary>
        /// Gets the Chart data for the client side using a chart factory derived from <see cref="ChartJsDataFactory"/> based on the <paramref name="chartType"/>.
        /// </summary>
        /// <param name="rockContext">The RockContext.</param>
        /// <param name="chartType">The current chart type configuration.</param>
        /// <returns></returns>
        private string GetChartData( RockContext rockContext, string chartType )
        {
            switch ( chartType )
            {
                case ChartTypeKey.BarChart:
                    return GetBarChartDataFactory( rockContext ).GetChartDataJson( GetBarChartDataArgs() );
                default:
                    return GetLineChartDataFactory( rockContext ).GetChartDataJson( GetLineChartDataArgs() );
            }
        }

        /// <summary>
        /// Gets the labels for the chart based on the configured metric value partition.
        /// </summary>
        /// <param name="dataSeriesDataset"></param>
        /// <returns></returns>
        private string GetSeriesPartitionName( string dataSeriesDataset )
        {
            string seriesNameValue = null;

            List<string> seriesPartitionValues = new List<string>();

            var entityTypeEntityIdList = dataSeriesDataset
                .SplitDelimitedValues( "," )
                .Select( a => a.Split( '|' ) )
                .Select( a =>
                new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } );

            // We already know partitions are by campus , if partition entity type changes an update will be required.
            foreach ( var entityTypeEntity in entityTypeEntityIdList )
            {
                var campus = CampusCache.Get( entityTypeEntity.EntityId.Value );
                if ( campus != null )
                {
                    seriesPartitionValues.Add( campus.Name );
                }
            }

            if ( seriesPartitionValues.Any() )
            {
                seriesNameValue = seriesPartitionValues.AsDelimited( "," );
            }

            return seriesNameValue;
        }

        /// <summary>
        /// Gets the fill color of the charts.
        /// </summary>
        /// <param name="campus">The campus.</param>
        /// <returns></returns>
        private string GetFillColor( CampusCache campus, string chartType )
        {
            var campusAge = GetCampusAge( campus );

            if ( chartType == ChartTypeKey.LineChart )
            {
                return FillColorSource().Skip( campus.Id ).FirstOrDefault();
            }

            if ( !campusAge.HasValue )
            {
                return "#A3A3A3";
            }

            if ( campusAge >= 0 && campusAge <= 2 )
            {
                return "#BAE6FD";
            }
            else if ( campusAge >= 3 && campusAge <= 6 )
            {
                return "#38BDF8";
            }
            else if ( campusAge >= 7 && campusAge <= 11 )
            {
                return "#0284C7";
            }
            else if ( campusAge >= 11 )
            {
                return "#075985";
            }
            else
            {
                return "A3A3A3";
            }
        }

        /// <summary>
        /// Perpetually yields a color from the available colors source so we can skip and take in a loop.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> FillColorSource()
        {
            foreach ( var color in _availableColors )
            {
                yield return color;
            }
        }

        /// <summary>
        /// Gets the data used to construct the Chart tooltip for each label/Campus.
        /// </summary>
        /// <param name="rockContext">The RockContext</param>
        /// <param name="chartType">The current chart type configuration.</param>
        /// <returns></returns>
        private Dictionary<string, TithingOverviewToolTipBag> GetToolTipData( RockContext rockContext, string chartType )
        {
            var givingHouseHoldsDatasets = GetGivingHouseholdsMetricValues( rockContext, chartType );
            var tithingHouseHoldsDatasets = GetTithingHouseholdsMetricValues( rockContext, chartType );
            var tithingOverviewDataset = GetTithingOverviewMetricValues( rockContext, ChartTypeKey.BarChart );

            var toolTipData = new Dictionary<string, TithingOverviewToolTipBag>();

            foreach ( var dataset in tithingOverviewDataset )
            {
                var campusId = dataset.CampusId ?? 0;
                var campus = CampusCache.Get( campusId );
                var givingHouseHolds = givingHouseHoldsDatasets.Find( d => d.CampusId == campusId );
                var tithingHouseHolds = tithingHouseHoldsDatasets.Find( d => d.CampusId == campusId );

                var toolTipInfo = new TithingOverviewToolTipBag()
                {
                    Campus = campus.Name,
                    CampusAge = GetCampusAge( campus ),
                    Date = dataset.DateTime.ToShortDateString(),
                    GivingHouseHolds = givingHouseHolds.Value,
                    TithingHouseHolds = tithingHouseHolds.Value,
                    TitheMetric = campus.TitheMetric,
                    Value = dataset.Value,
                    CampusClosedDate = campus.ClosedDate,
                    CampusOpenedDate = campus.OpenedDate,
                };

                toolTipData.AddOrReplace( campus.Name, toolTipInfo );
            }

            return toolTipData;
        }

        /// <summary>
        /// Gets the specified campus's age in years.
        /// </summary>
        /// <param name="campus">The campus</param>
        /// <returns></returns>
        private int? GetCampusAge( CampusCache campus )
        {
            if ( !campus.OpenedDate.HasValue )
            {
                return null;
            }

            var openedDate = campus.OpenedDate.Value;
            var closedDate = campus.ClosedDate ?? RockDateTime.Now;

            var ageDifference = closedDate.TotalMonths( openedDate );
            var age = ageDifference / 12;

            return age;
        }

        #endregion

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="chartType">Type of the chart.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ChartData( string chartType )
        {
            using ( var rockContext = new RockContext() )
            {
                var box = GetInitializationBox( rockContext, chartType );
                return ActionOk( box );
            }
        }

        #endregion

        /// <summary>
        /// Stores information about a dataset to be displayed on a chart.
        /// </summary>
        private sealed class ChartDatasetInfo
        {
            public string MetricValueCampusIds { get; set; }

            public DateTime DateTime { get; set; }

            public decimal? Value { get; set; }

            public int? CampusId { get; set; }
        }
    }
}
