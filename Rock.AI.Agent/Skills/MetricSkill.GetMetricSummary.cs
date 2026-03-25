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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.MetricSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class MetricSkill
{
    #region Tool(s)

    [Description( "Returns a summary of the values for a metric." )]
    [AgentPurpose( "Retrieves a summary of the values for a metric." )]
    [AgentToolGuid( "e233f5cf-469d-4f76-b159-5c6d70f62394" )]
    public IAgentToolResult GetMetricSummary(
        string metricIdKey,
        DateTime startDate,
        DateTime endDate )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var metric = helper.GetRequiredEntity<Model.Metric>( metricIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var query = new MetricValueService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( mv => mv.MetricId == metric.Id
                && mv.MetricValueDateTime.HasValue );

        query = helper.WhereRequiredPropertyBetween( query, mv => mv.MetricValueDateTime, startDate, endDate );

        if ( ( endDate - startDate ).TotalDays > ( 30 * 18 ) )
        {
            helper.AddError( "The date range cannot be longer than 18 months." );
        }

        var entityLookup = new Dictionary<(int EntityTypeId, int EntityId), int>();

        var summaryResult = new MetricSummaryResult
        {
            Partitions = [],
            PartitionValues = [],
            UnitOfMeasure = metric.YAxisLabel,
            UnitType = metric.UnitType,
        };

        var metricValueList = query
            .Select( mv => new MetricValueInfo
            {
                ValueDateTime = mv.MetricValueDateTime.Value,
                Value = mv.YValue,
                Partitions = mv.MetricValuePartitions.Select( mvp => new MetricValuePartitionInfo
                {
                    EntityTypeId = mvp.MetricPartition.EntityTypeId,
                    EntityId = mvp.EntityId,
                    Order = mvp.MetricPartition.Order,
                } ).ToList(),
            } )
            .ToList();

        BuildPartitionEntityLookup( metricValueList, entityLookup, metric, summaryResult );

        summaryResult.Data = metricValueList
            .GroupBy( mv => mv.ValueDateTime )
            .Select( g =>
            {
                var date = g.Key.ToShortDateString();

                // Simple case, no metric partitions. Just sum up the values for
                // this date and return a single value result.
                if ( metric.MetricPartitions.Count == 0 || metric.MetricPartitions.Any( mp => !mp.EntityTypeId.HasValue ) )
                {
                    return new MetricSummaryValueResult
                    {
                        Date = date,
                        Value = g.Sum( v => v.Value ?? 0 ),
                    };
                }

                return GetCompoundValueResult( date, g, metric, entityLookup );
            } )
            .Where( v => v != null )
            .ToList();

        // Cleanup the result if we don't have partitions. This helps the
        // language model understand the results better.
        if ( summaryResult.Partitions.Count == 0 )
        {
            summaryResult.Partitions = null;
            summaryResult.PartitionValues = null;
        }

        return Success( summaryResult ).WithoutHistoryContent();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Builds a lookup of entity identifiers for each partition and populates
    /// the summary result with partition labels and their corresponding
    /// entity names.
    /// </summary>
    /// <param name="metricValueList">A list of metric value information objects that contain partition data used to identify entities for each partition.</param>
    /// <param name="entityLookup">A dictionary that maps a tuple of entity type ID and entity ID to the index of the entity within the partition values.</param>
    /// <param name="metric">The metric definition that specifies the partitions to be processed.</param>
    /// <param name="summaryResult">The result object that is updated with partition labels and their associated entity names.</param>
    private void BuildPartitionEntityLookup( List<MetricValueInfo> metricValueList, Dictionary<(int EntityTypeId, int EntityId), int> entityLookup, Model.Metric metric, MetricSummaryResult summaryResult )
    {
        var entityIdSet = metricValueList
            .SelectMany( mv => mv.Partitions )
            .Where( mvp => mvp.EntityTypeId.HasValue && mvp.EntityId.HasValue )
            .Select( mvp => (EntityTypeId: mvp.EntityTypeId.Value, EntityId: mvp.EntityId.Value) )
            .Distinct()
            .ToList();

        foreach ( var partition in metric.MetricPartitions.OrderBy( mp => mp.Order ) )
        {
            var partitionValues = new List<string>();

            foreach ( var key in entityIdSet.Where( e => e.EntityTypeId == partition.EntityTypeId ) )
            {
                if ( !entityLookup.ContainsKey( key ) )
                {
                    var entity = Reflection.GetIEntityForEntityType( key.EntityTypeId, key.EntityId, AgentRequestContext.RockContext );
                    var name = entity?.ToString() ?? string.Empty;

                    if ( name.IsNotNullOrWhiteSpace() )
                    {
                        entityLookup[key] = partitionValues.Count;
                        partitionValues.Add( name );
                    }
                }
            }

            summaryResult.Partitions.Add( partition.Label );
            summaryResult.PartitionValues[partition.Label] = partitionValues;
        }
    }

    /// <summary>
    /// Calculates and aggregates metric values for the specified date and
    /// metric definition, returning a summary result that includes partitioned
    /// values and the overall total. Actual aggregation logic only happens
    /// in rare cases where multiple values have been entered on the same date
    /// for the same set of partitions.
    /// </summary>
    /// <param name="date">The date string for which the metric summary is calculated.</param>
    /// <param name="values">A collection of metric value information, where each item contains partition data and an associated value.</param>
    /// <param name="metric">The metric definition that specifies the required partitions and structure for the aggregation.</param>
    /// <param name="entityLookup">A dictionary mapping a tuple of entity type ID and entity ID to their corresponding partition index, used to efficiently locate partition values.</param>
    /// <returns>A <see cref="MetricSummaryValueResult"/> instance that contains the metric data or null if no valid metric values are found.</returns>
    private MetricSummaryValueResult GetCompoundValueResult( string date, IEnumerable<MetricValueInfo> values, Model.Metric metric, Dictionary<(int EntityTypeId, int EntityId), int> entityLookup )
    {
        var valueList = new List<(int[] Indexes, decimal Value)>();

        foreach ( var metricValue in values )
        {
            var hasRequiredPartitionData = metricValue.Partitions.All( p => p.EntityId.HasValue )
                && metricValue.Partitions.All( p => p.EntityTypeId.HasValue );

            if ( !hasRequiredPartitionData || metricValue.Partitions.Count != metric.MetricPartitions.Count )
            {
                continue;
            }

            var partitionValueIndexes = metricValue.Partitions
                .OrderBy( p => p.Order )
                .Select( p =>
                {
                    var lookupKey = (p.EntityTypeId.Value, p.EntityId.Value);

                    if ( entityLookup.TryGetValue( lookupKey, out var index ) )
                    {
                        return index;
                    }

                    return -1;
                } )
                .ToList();

            if ( partitionValueIndexes.Any( i => i == -1 ) )
            {
                continue;
            }

            // Look for existing match in valueList and update value if
            // found, otherwise add new entry.
            var existingValue = valueList.FirstOrDefault( v => v.Indexes.SequenceEqual( partitionValueIndexes ) );
            if ( existingValue != default )
            {
                existingValue.Value += metricValue.Value ?? 0;
            }
            else
            {
                valueList.Add( (partitionValueIndexes.ToArray(), metricValue.Value ?? 0) );
            }
        }

        if ( valueList.Count == 0 )
        {
            return null;
        }

        return new MetricSummaryValueResult
        {
            Date = date,
            Values = [.. valueList.Select( vl => ( List<object> ) [.. vl.Indexes, vl.Value] )],
            Total = valueList.Sum( v => v.Value ),
        };
    }

    #endregion

    #region Classes

    private class MetricValueInfo
    {
        public DateTime ValueDateTime { get; set; }

        public decimal? Value { get; set; }

        public List<MetricValuePartitionInfo> Partitions { get; set; }
    }

    private class MetricValuePartitionInfo
    {
        public int? EntityTypeId { get; set; }

        public int? EntityId { get; set; }

        public int Order { get; set; }
    }
    #endregion
}
