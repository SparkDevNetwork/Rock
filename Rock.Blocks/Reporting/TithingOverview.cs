﻿// <copyright>
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
using Rock.Utility;
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

    #region Block Attributes

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "This setting filters the list of campuses by type that are displayed in the chart.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 0 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This setting filters the list of campuses by statuses that are displayed in the chart.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 1 )]

    #endregion

    [SystemGuid.EntityTypeGuid( "1e44b061-7767-487d-a98f-16912e8c7de7" )]
    [SystemGuid.BlockTypeGuid( "db756565-8a35-42e2-bc79-8d11f57e4004" )]
    public class TithingOverview : RockBlockType
    {
        #region Fields

        /*
           6/25/2024 - KA

           Ideally the colors should be identified by their css classes but the Chart.js
           prefers the colors be either hex or string, and since the Chart.js data is generated
           server-side replacing the css class with their hex components client-side will be difficult.           

           #techdebt: Use hex from the Theme's AdditionalSettingsJson when they become available in a upcoming version of Rock.
        */
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

        private static class AttributeKey
        {
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

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
            var metricValues = GetTithingOverviewMetricValues( rockContext, chartType );

            var box = new TithingOverviewInitializationBox
            {
                ChartDataJson = json,
                ChartType = chartType,
                ToolTipData = GetToolTipData( rockContext, chartType ),
                LegendData = GetLegendLabelColors(),
                HasData = metricValues.Count > 0,
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
                SizeToFitContainerWidth = true,
                IncludeNullDatapoints = false,
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
                TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day,
                AreaFillOpacity = 0,
                DateFormatString = "o"
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
            foreach ( var dataSeriesDataset in dataSeriesDatasets.Where( x => !string.IsNullOrWhiteSpace( x ) ) )
            {
                var seriesNameValue = GetSeriesPartitionName( dataSeriesDataset );
                seriesNameKeyValue.Add( dataSeriesDataset, seriesNameValue );
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

        private string GetChartDataJson( List<ChartJsCategorySeriesDataset> datasets )
        {
            var jsDataset = new
            {
                labels = datasets.SelectMany( ds => ds.DataPoints ).Select( x => x.Category ).ToList(),
                datasets = new List<dynamic>
                {
                    new
                    {
                        data = datasets.SelectMany( ds => ds.DataPoints.Select( dp => dp.Value ) ),
                        backgroundColor = datasets.Select( ds => ds.FillColor ),
                        borderColor = datasets.Select( ds => ds.BorderColor ),
                        borderWidth = 2,
                        lineTension = 0.4m,
                    }
                }
            };

            return jsDataset.ToJson();
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
                        Value = x.Value.FirstOrDefault(),
                    } )
                    .ToList();
            }

            var campusTypeIds = GetAttributeValues( AttributeKey.CampusTypes )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var campusStatusIds = GetAttributeValues( AttributeKey.CampusStatuses )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var filteredCampusIds = CampusCache.All( false )
                .Where( c => ( !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                    && ( !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) ) )
                .Select( c => c.Id )
                .ToList();

            _tithingOverviewMetricValues = _tithingOverviewMetricValues.Where( m => m.CampusId.HasValue && filteredCampusIds.Contains( m.CampusId.Value ) ).ToList();

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
                    .Where( m => m.Metric.Guid == metricGuid && m.MetricValueType == MetricValueType.Measure )
                    .OrderByDescending( m => m.MetricValueDateTime )
                    .Select( m => m.MetricValueDateTime )
                    .FirstOrDefault();

                if ( lastRunDate.HasValue )
                {
                    lastRunDate = lastRunDate.Value.Date;
                    metricValuesQry = metricValuesQry.Where( m => DbFunctions.TruncateTime( m.MetricValueDateTime ) == lastRunDate );
                }
            }

            return metricValuesQry.OrderByDescending( m => m.MetricValueDateTime );
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
                .Where( a => a.Metric.Guid == metricGuid && a.MetricValueType == MetricValueType.Measure );

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
                    return GetChartDataJson( GetCategorySeriesDataset( rockContext ) );
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
                var campus = CampusCache.Get( entityTypeEntity.EntityId ?? 0 );
                if ( campus != null )
                {
                    var partitionValue = string.IsNullOrWhiteSpace( campus.ShortCode ) ? campus.Name : campus.ShortCode;
                    seriesPartitionValues.Add( partitionValue );
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
                var campusColorAttribute = campus.GetAttributeTextValue( "core_CampusColor" );

                if ( !string.IsNullOrWhiteSpace( campusColorAttribute ) )
                {
                    return campusColorAttribute;
                }
                else
                {
                    return FillColorSource().Skip( campus?.Id ?? 0 ).FirstOrDefault();
                }
            }
            else
            {
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
            var tithingOverviewDataset = GetTithingOverviewMetricValues( rockContext, chartType );

            var toolTipData = new Dictionary<string, TithingOverviewToolTipBag>();
            var currencyInfo = new RockCurrencyCodeInfo();

            foreach ( var dataset in tithingOverviewDataset )
            {
                var campusId = dataset.CampusId ?? 0;
                var campus = CampusCache.Get( campusId );

                if ( campus != null )
                {
                    var givingHouseHolds = givingHouseHoldsDatasets.Find( d => d.CampusId == campusId );
                    var tithingHouseHolds = tithingHouseHoldsDatasets.Find( d => d.CampusId == campusId );
                    var campusKey = string.IsNullOrWhiteSpace( campus.ShortCode ) ? campus.Name : campus.ShortCode;

                    var toolTipInfo = new TithingOverviewToolTipBag()
                    {
                        CampusId = campus.Id,
                        Campus = campus.Name,
                        CampusAge = GetCampusAge( campus ),
                        Date = dataset.DateTime.ToShortDateString(),
                        GivingHouseHolds = givingHouseHolds?.Value,
                        TithingHouseHolds = tithingHouseHolds?.Value,
                        TitheMetric = campus.TitheMetric,
                        Value = dataset.Value,
                        CampusClosedDate = campus.ClosedDate,
                        CampusOpenedDate = campus.OpenedDate,
                        CampusShortCode = campus.ShortCode,
                        CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
                        {
                            Symbol = currencyInfo.Symbol,
                            DecimalPlaces = currencyInfo.DecimalPlaces,
                            SymbolLocation = currencyInfo.SymbolLocation
                        }
                    };

                    toolTipData.AddOrReplace( campusKey, toolTipInfo );
                }
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
            if ( campus == null || !campus.OpenedDate.HasValue )
            {
                return null;
            }

            var openedDate = campus.OpenedDate.Value;
            var closedDate = campus.ClosedDate ?? RockDateTime.Now;

            var ageDifference = closedDate.TotalMonths( openedDate );
            var age = ageDifference / 12;

            return age;
        }

        /// <summary>
        /// Gets the legend label colors.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetLegendLabelColors()
        {
            var legendLabelColorMap = new Dictionary<string, string>
            {
                { "0-2 yrs", "#BAE6FD" },
                { "3-6 yrs", "#38BDF8" },
                { "7-11 yrs", "#0284C7" },
                { "11+", "#075985" },
                { "Unknown", "#A3A3A3" }
            };

            return legendLabelColorMap;
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
