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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Linq;
using Rock.Attribute;

namespace Rock.Chart
{
    /// <summary>
    /// Builds the data for a Chart that displays a Metric.
    /// </summary>
    [RockInternal("1.16.4")]
    public class MetricChartDataSourceBuilder
    {
        /// <summary>
        /// The list of Metric identifiers displayed on the chart.
        /// </summary>
        public List<int> MetricIdList { get; set; }

        /// <summary>
        /// The type of value displayed on the chart.
        /// If not specified, each value type should be displayed as a separate series.
        /// </summary>
        public MetricValueType ValueType { get; set; } = Rock.Model.MetricValueType.Measure;

        /// <summary>
        /// The first date of the reporting period.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The last date of the reporting period.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// A flag indicating if metric values should be combined.
        /// </summary>
        public bool CombineValues { get; set; }

        /// <summary>
        /// The name of the default data set if metric values are combined into a single series.
        /// </summary>
        public string DefaultSeriesName { get; set; }

        /// <summary>
        /// A collection of identifiers that specify the metric partition values to be included.
        /// If not specified, values from all partitions will be included.
        /// </summary>
        public List<MetricService.EntityIdentifierByTypeAndId> PartitionValues { get; set; }

        /// <summary>
        /// Create time-series datasets for the configured Metrics.
        /// </summary>
        /// <returns></returns>
        public List<ChartJsTimeSeriesDataset> GetTimeSeriesDatasets()
        {
            var rockContext = new RockContext();

            // Get all of the Metric Values that are associated with the specified partitions.
            // If a filter is not specified for a partition, select all values associated with that partition.
            var dataSets = new List<ChartJsTimeSeriesDataset>();

            var metricService = new MetricService( rockContext );

            var qryMetricValues = metricService.GetMetricValuesQuery( this.MetricIdList,
                this.ValueType,
                this.StartDate,
                this.EndDate,
                this.PartitionValues );

            qryMetricValues = qryMetricValues.Where( v => v.YValue != null && v.MetricValueDateTime != null );

            if ( this.CombineValues )
            {
                // Create a Chart Data Set that aggregates all of the requested Metric Values into a single value for each day.
                var partitionDataSet = new ChartJsTimeSeriesDataset();

                partitionDataSet.Name = this.DefaultSeriesName;

                var partitionDataPoints = qryMetricValues
                    .Select( x => new ChartJsTimeSeriesDataPoint
                    {
                        DateTime = x.MetricValueDateTime.Value,
                        Value = x.YValue.Value
                    } )
                    .ToList()
                    .Cast<IChartJsTimeSeriesDataPoint>()
                    .ToList();

                partitionDataSet.DataPoints = partitionDataPoints;

                partitionDataSet = ChartDataFactory.GetQuantizedTimeSeries( partitionDataSet, ChartJsTimeSeriesTimeScaleSpecifier.Day );

                dataSets.Add( partitionDataSet );
            }
            else
            {
                // Create a Chart Data Set that contains a series for each unique set of partition values.
                if ( !this.PartitionValues.Any() )
                {
                    return dataSets;
                }

                // Retrieve all of the metric values that are associated with the selected partitions.
                var dataPoints = qryMetricValues
                    .SelectMany( v => v.MetricValuePartitions,
                        ( v, p ) => new MetricDataPoint
                        {
                            MetricValueDateTime = v.MetricValueDateTime,
                            Value = ( decimal? ) v.YValue,
                            EntityTypeId = p.MetricPartition.EntityTypeId,
                            EntityId = p.EntityId,
                            MetricValueId = p.MetricValueId
                        } )
                    .ToList();

                CreateEntityValueLookups( this.MetricIdList );

                var dataPointsByValue = dataPoints.GroupBy( k => new { k.MetricValueId } );

                foreach ( var valueDataPoints in dataPointsByValue )
                {
                    var seriesName = GetSeriesName( valueDataPoints );
                    foreach ( var valueDataPoint in valueDataPoints )
                    {
                        valueDataPoint.SeriesName = seriesName;
                    }
                }

                var partitionNames = dataPoints.Select( p => p.SeriesName ).Distinct().ToList();

                foreach ( var partitionName in partitionNames )
                {
                    var partitionDataPoints = dataPoints.Where( p => p.SeriesName == partitionName )
                        .DistinctBy( p => p.MetricValueId )
                        .Select( x => new ChartJsTimeSeriesDataPoint
                        {
                            DateTime = x.MetricValueDateTime.Value,
                            Value = x.Value.Value
                        } )
                        .Cast<IChartJsTimeSeriesDataPoint>()
                        .ToList();

                    var partitionDataSet = new ChartJsTimeSeriesDataset();
                    partitionDataSet.Name = partitionName;

                    partitionDataSet.DataPoints = partitionDataPoints;

                    dataSets.Add( partitionDataSet );
                }
            }

            return dataSets;
        }

        /// <summary>
        /// Create a dataset suitable for plotting as a Category vs Value chart.
        /// Each Metric is represented as a Category in the dataset, and the Value of that category is the sum of the metric values.
        /// </summary>
        /// <returns></returns>
        public ChartJsCategorySeriesDataset GetCategorySeriesDataset()
        {
            var rockContext = new RockContext();

            // Get all of the Metric Values that are associated with the specified partitions.
            // If a filter is not specified for a partition, select all values associated with that partition.
            var dataSets = new List<ChartJsTimeSeriesDataset>();

            var metricService = new MetricService( rockContext );

            var qryMetricValues = metricService.GetMetricValuesQuery( this.MetricIdList,
                this.ValueType,
                this.StartDate,
                this.EndDate,
                this.PartitionValues );

            var itemsByMetricId = qryMetricValues.Where( v => v.MetricValueType == this.ValueType )
                .GroupBy( k => k.MetricId, v => v );

            var defaultSeriesName = this.DefaultSeriesName;
            if ( string.IsNullOrWhiteSpace( defaultSeriesName ) )
            {
                defaultSeriesName = "(unknown)";
            }

            var dataset = new ChartJsCategorySeriesDataset();
            dataset.DataPoints = new List<IChartJsCategorySeriesDataPoint>();
            foreach ( var metricIdGroup in itemsByMetricId )
            {
                var metric = metricService.Get( metricIdGroup.Key );

                var categoryName = string.IsNullOrWhiteSpace( metric.Title ) ? defaultSeriesName : metric.Title;
                if ( this.ValueType == Rock.Model.MetricValueType.Goal )
                {
                    categoryName += " Goal";
                }
                var datapoint = new ChartJsCategorySeriesDataPoint
                {
                    Category = categoryName,
                    Value = metricIdGroup.Sum( v => v.YValue ?? 0 )
                };

                dataset.DataPoints.Add( datapoint );
            }

            return dataset;
        }

        private Dictionary<int, IQueryable<IEntity>> _entityTypeEntityLookupQry = null;
        private Dictionary<int, Dictionary<int, string>> _entityTypeEntityNameLookup;

        private string GetSeriesName( IEnumerable<MetricDataPoint> metricValuePartitions )
        {
            if ( _entityTypeEntityNameLookup == null || metricValuePartitions == null )
            {
                return null;
            }

            var seriesNames = new List<string>();
            foreach ( var metricValuePartition in metricValuePartitions.Where( a => a.EntityId.HasValue && a.EntityTypeId.HasValue ) )
            {
                if ( !_entityTypeEntityNameLookup.ContainsKey( metricValuePartition.EntityTypeId.Value ) )
                {
                    continue;
                }

                var entityNameLookup = _entityTypeEntityNameLookup[metricValuePartition.EntityTypeId.Value];
                if ( !entityNameLookup.ContainsKey( metricValuePartition.EntityId.Value ) )
                {
                    var value = string.Empty;

                    var entityItem = _entityTypeEntityLookupQry[metricValuePartition.EntityTypeId.Value].FirstOrDefault( a => a.Id == metricValuePartition.EntityId.Value );
                    if ( entityItem != null )
                    {
                        value = entityItem.ToString();
                    }

                    entityNameLookup.TryAdd( metricValuePartition.EntityId.Value, value );
                }

                seriesNames.Add( entityNameLookup[metricValuePartition.EntityId.Value] );
            }

            return seriesNames.AsDelimited( ", " );
        }

        private void CreateEntityValueLookups( IEnumerable<int> metricIdList )
        {
            _entityTypeEntityNameLookup = new Dictionary<int, Dictionary<int, string>>();
            _entityTypeEntityLookupQry = new Dictionary<int, IQueryable<IEntity>>();

            var rockContext = new RockContext();

            foreach ( var metricId in metricIdList )
            {
                AddEntityValueLookups( metricId, rockContext );
            }
        }

        /// <summary>
        /// Creates the entity value lookups.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="rockContext"></param>
        private void AddEntityValueLookups( int metricId, RockContext rockContext )
        {
            var metric = new MetricService( rockContext ).Get( metricId );
            if ( metric == null )
            {
                return;
            }

            foreach ( var metricPartition in metric.MetricPartitions.Where( a => a.EntityTypeId.HasValue ) )
            {
                var entityTypeCache = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0, rockContext );
                if ( entityTypeCache == null )
                {
                    continue;
                }

                _entityTypeEntityNameLookup.TryAdd( entityTypeCache.Id, new Dictionary<int, string>() );
                _entityTypeEntityLookupQry.TryAdd( entityTypeCache.Id, null );

                if ( entityTypeCache.GetEntityType() == typeof( Rock.Model.Group ) )
                {
                    _entityTypeEntityLookupQry[entityTypeCache.Id] = new GroupService( rockContext ).Queryable();
                }
                else
                {
                    var modelType = new Type[] { entityTypeCache.GetEntityType() };
                    var genericServiceType = typeof( Rock.Data.Service<> );
                    var modelServiceType = genericServiceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                    var qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    _entityTypeEntityLookupQry[entityTypeCache.Id] = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;
                }
            }
        }

        private class MetricDataPoint
        {
            public DateTime? MetricValueDateTime;
            public decimal? Value;
            public string SeriesName;

            public int? MetricValueId;
            public int? EntityTypeId;
            public int? EntityId;
        }
    }
}