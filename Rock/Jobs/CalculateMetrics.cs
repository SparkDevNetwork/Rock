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
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

using Quartz;

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to calculate metric values for metrics that are based on a schedule and have a database or sql datasource type
    /// Only Metrics that need to be populated (based on their Schedule) will be processed
    /// </summary>
    [DisallowConcurrentExecution]
    public class CalculateMetrics : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateMetrics()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var metricSourceValueTypeDataviewGuid = Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();
            var metricSourceValueTypeSqlGuid = Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();
            var metricSourceValueTypeLavaGuid = Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_LAVA.AsGuid();

            Guid[] calculatedSourceTypes = new Guid[] {
                metricSourceValueTypeDataviewGuid,
                metricSourceValueTypeSqlGuid,
                metricSourceValueTypeLavaGuid
            };

            var metricsQry = new MetricService( new RockContext() ).Queryable().AsNoTracking().Where(
                a => a.ScheduleId.HasValue
                && a.SourceValueTypeId.HasValue
                && calculatedSourceTypes.Contains( a.SourceValueType.Guid ) );

            var metricIdList = metricsQry.OrderBy( a => a.Title ).ThenBy( a => a.Subtitle ).Select( a => a.Id ).ToList();

            var metricExceptions = new List<Exception>();
            int metricsCalculated = 0;
            int metricValuesCalculated = 0;
            Metric metric = null;

            foreach ( var metricId in metricIdList )
            {
                try
                {
                    using ( var rockContextForMetricEntity = new RockContext() )
                    {
                        var metricService = new MetricService( rockContextForMetricEntity );

                        metric = metricService.Get( metricId );
                        var lastRunDateTime = metric.LastRunDateTime ?? metric.CreatedDateTime ?? metric.ModifiedDateTime;
                        if ( lastRunDateTime.HasValue )
                        {
                            var currentDateTime = RockDateTime.Now;

                            // get all the schedule times that were supposed to run since that last time it was scheduled to run
                            var scheduledDateTimesToProcess = metric.Schedule.GetScheduledStartTimes( lastRunDateTime.Value, currentDateTime ).Where( a => a > lastRunDateTime.Value ).ToList();
                            foreach ( var scheduleDateTime in scheduledDateTimesToProcess )
                            {
                                using ( var rockContextForMetricValues = new RockContext() )
                                {
                                    var metricPartitions = new MetricPartitionService( rockContextForMetricValues ).Queryable().Where( a => a.MetricId == metric.Id ).ToList();
                                    var metricValueService = new MetricValueService( rockContextForMetricValues );
                                    List<ResultValue> resultValues = new List<ResultValue>();
                                    bool getMetricValueDateTimeFromResultSet = false;
                                    if ( metric.SourceValueType.Guid == metricSourceValueTypeDataviewGuid )
                                    {
                                        // get the metric value from the DataView
                                        if ( metric.DataView != null )
                                        {
                                            var errorMessages = new List<string>();
                                            var qry = metric.DataView.GetQuery( null, null, out errorMessages );
                                            if ( metricPartitions.Count > 1 || metricPartitions.First().EntityTypeId.HasValue )
                                            {
                                                throw new NotImplementedException( "Partitioned Metrics using DataViews is not supported." );
                                            }
                                            else
                                            {
                                                var resultValue = new ResultValue();
                                                resultValue.Value = Convert.ToDecimal( qry.Count() );
                                                resultValue.Partitions = new List<ResultValuePartition>();
                                                resultValue.MetricValueDateTime = scheduleDateTime;
                                                resultValues.Add( resultValue );
                                            }
                                        }
                                    }
                                    else if ( metric.SourceValueType.Guid == metricSourceValueTypeSqlGuid )
                                    {
                                        //// calculate the metricValue using the results from the SQL
                                        //// assume SQL is in one of the following forms:
                                        //// -- "SELECT Count(*) FROM ..."
                                        //// -- "SELECT Count(*), [MetricValueDateTime] FROM ..."
                                        //// -- "SELECT Count(*), Partition0EntityId, Partition1EntityId, Partition2EntityId,.. FROM ..."
                                        //// -- "SELECT Count(*), [MetricValueDateTime], Partition0EntityId, Partition1EntityId, Partition2EntityId,.. FROM ..."
                                        if ( !string.IsNullOrWhiteSpace( metric.SourceSql ) )
                                        {
                                            string formattedSql = metric.SourceSql.ResolveMergeFields( metric.GetMergeObjects( scheduleDateTime ) );
                                            var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );

                                            if ( tableResult.Columns.Count >= 2 && tableResult.Columns[1].ColumnName == "MetricValueDateTime" )
                                            {
                                                getMetricValueDateTimeFromResultSet = true;
                                            }

                                            foreach ( var row in tableResult.Rows.OfType<System.Data.DataRow>() )
                                            {
                                                var resultValue = new ResultValue();

                                                resultValue.Value = Convert.ToDecimal( row[0] );
                                                if ( getMetricValueDateTimeFromResultSet )
                                                {
                                                    resultValue.MetricValueDateTime = Convert.ToDateTime( row[1] );
                                                }
                                                else
                                                {
                                                    resultValue.MetricValueDateTime = scheduleDateTime;
                                                }

                                                resultValue.Partitions = new List<ResultValuePartition>();
                                                int partitionPosition = 0;
                                                int partitionFieldIndex = getMetricValueDateTimeFromResultSet ? 2 : 1;
                                                int partitionColumnCount = tableResult.Columns.Count - 1;
                                                while ( partitionFieldIndex <= partitionColumnCount )
                                                {
                                                    resultValue.Partitions.Add( new ResultValuePartition
                                                    {
                                                        PartitionPosition = partitionPosition,
                                                        EntityId = row[partitionFieldIndex] as int?
                                                    } );

                                                    partitionPosition++;
                                                    partitionFieldIndex++;
                                                }

                                                resultValues.Add( resultValue );
                                            }
                                        }
                                    }
                                    else if ( metric.SourceValueType.Guid == metricSourceValueTypeLavaGuid )
                                    {
                                        //// calculate the metricValue using the results from Lava
                                        //// assume Lava Output is in one of the following forms:
                                        //// A single Count
                                        //// 42
                                        //// A List of Count, MetricValueDateTime
                                        //// 42, 1/1/2017
                                        //// 40, 1/2/2017
                                        //// 49, 1/3/2017
                                        //// A List of Count, Partition0EntityId, Partition1EntityId, Partition2EntityId
                                        //// 42, 201, 450, 654
                                        //// 42, 202, 450, 654
                                        //// 42, 203, 450, 654
                                        //// A List of Count, MetricValueDateTime,  Partition0EntityId, Partition1EntityId, Partition2EntityId
                                        //// 42, 1/1/2017, 201, 450, 654
                                        //// 42, 1/2/2017, 202, 450, 654
                                        //// 42, 1/3/2017, 203, 450, 654
                                        if ( !string.IsNullOrWhiteSpace( metric.SourceLava ) )
                                        {
                                            var mergeObjects = metric.GetMergeObjects( scheduleDateTime );
                                            string lavaResult = metric.SourceLava.ResolveMergeFields( mergeObjects, enabledLavaCommands:"All", throwExceptionOnErrors:true );
                                            List<string> resultLines = lavaResult.Split( new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries )
                                                .Select( a => a.Trim() ).Where( a => !string.IsNullOrEmpty( a ) ).ToList();
                                            List<string[]> resultList = resultLines.Select( a => a.SplitDelimitedValues() ).ToList();

                                            if ( resultList.Any() )
                                            {
                                                if ( resultList[0].Length >= 2 )
                                                {
                                                    // if the value of the data in the 2nd column is a Date, assume that is is the MetricValueDateTime
                                                    if ( resultList[0][1].AsDateTime().HasValue )
                                                    {
                                                        getMetricValueDateTimeFromResultSet = true;
                                                    }
                                                }
                                            }

                                            foreach ( var row in resultList )
                                            {
                                                var resultValue = new ResultValue();

                                                resultValue.Value = row[0].AsDecimal();
                                                if ( getMetricValueDateTimeFromResultSet )
                                                {
                                                    resultValue.MetricValueDateTime = row[1].AsDateTime() ?? scheduleDateTime;
                                                }
                                                else
                                                {
                                                    resultValue.MetricValueDateTime = scheduleDateTime;
                                                }

                                                resultValue.Partitions = new List<ResultValuePartition>();
                                                int partitionPosition = 0;
                                                int partitionFieldIndex = getMetricValueDateTimeFromResultSet ? 2 : 1;
                                                int partitionColumnCount = row.Length - 1;
                                                while ( partitionFieldIndex <= partitionColumnCount )
                                                {
                                                    resultValue.Partitions.Add( new ResultValuePartition
                                                    {
                                                        PartitionPosition = partitionPosition,
                                                        EntityId = row[partitionFieldIndex].AsIntegerOrNull()
                                                    } );

                                                    partitionPosition++;
                                                    partitionFieldIndex++;
                                                }

                                                resultValues.Add( resultValue );
                                            }
                                        }
                                    }

                                    metric.LastRunDateTime = scheduleDateTime;
                                    metricsCalculated++;
                                    metricValuesCalculated += resultValues.Count();

                                    if ( resultValues.Any() )
                                    {
                                        List<MetricValue> metricValuesToAdd = new List<MetricValue>();
                                        foreach ( var resultValue in resultValues )
                                        {
                                            var metricValue = new MetricValue();
                                            metricValue.MetricId = metric.Id;
                                            metricValue.MetricValueDateTime = resultValue.MetricValueDateTime;
                                            metricValue.MetricValueType = MetricValueType.Measure;
                                            metricValue.YValue = resultValue.Value;
                                            metricValue.CreatedDateTime = RockDateTime.Now;
                                            metricValue.ModifiedDateTime = RockDateTime.Now;
                                            metricValue.MetricValuePartitions = new List<MetricValuePartition>();
                                            var metricPartitionsByPosition = metricPartitions.OrderBy( a => a.Order ).ToList();

                                            if ( !resultValue.Partitions.Any() && metricPartitionsByPosition.Count() == 1 && !metricPartitionsByPosition[0].EntityTypeId.HasValue )
                                            {
                                                // a metric with just the default partition (not partitioned by Entity)
                                                var metricPartition = metricPartitionsByPosition[0];
                                                var metricValuePartition = new MetricValuePartition();
                                                metricValuePartition.MetricPartition = metricPartition;
                                                metricValuePartition.MetricPartitionId = metricPartition.Id;
                                                metricValuePartition.MetricValue = metricValue;
                                                metricValuePartition.EntityId = null;
                                                metricValue.MetricValuePartitions.Add( metricValuePartition );
                                            }
                                            else
                                            {
                                                foreach ( var partitionResult in resultValue.Partitions )
                                                {
                                                    if ( metricPartitionsByPosition.Count > partitionResult.PartitionPosition )
                                                    {
                                                        var metricPartition = metricPartitionsByPosition[partitionResult.PartitionPosition];
                                                        var metricValuePartition = new MetricValuePartition();
                                                        metricValuePartition.MetricPartition = metricPartition;
                                                        metricValuePartition.MetricPartitionId = metricPartition.Id;
                                                        metricValuePartition.MetricValue = metricValue;
                                                        metricValuePartition.EntityId = partitionResult.EntityId;
                                                        metricValue.MetricValuePartitions.Add( metricValuePartition );
                                                    }
                                                }
                                            }

                                            if ( metricValue.MetricValuePartitions == null || !metricValue.MetricValuePartitions.Any() )
                                            {
                                                // shouldn't happen, but just in case
                                                throw new Exception( "MetricValue requires at least one Partition Value" );
                                            }
                                            else
                                            {
                                                metricValuesToAdd.Add( metricValue );
                                            }
                                        }

                                        // if a single metricValueDateTime was specified, delete any existing metric values for this date and refresh with the current results
                                        var dbTransaction = rockContextForMetricValues.Database.BeginTransaction();
                                        if ( getMetricValueDateTimeFromResultSet )
                                        {
                                            var metricValueDateTimes = metricValuesToAdd.Select( a => a.MetricValueDateTime ).Distinct().ToList();
                                            foreach ( var metricValueDateTime in metricValueDateTimes )
                                            {
                                                bool alreadyHasMetricValues = metricValueService.Queryable().Where( a => a.MetricId == metric.Id && a.MetricValueDateTime == metricValueDateTime ).Any();
                                                if ( alreadyHasMetricValues )
                                                {
                                                    // use direct SQL to clean up any existing metric values
                                                    rockContextForMetricValues.Database.ExecuteSqlCommand( @"
                                                        DELETE
                                                        FROM MetricValuePartition
                                                        WHERE MetricValueId IN (
                                                            SELECT Id
                                                            FROM MetricValue
                                                            WHERE MetricId = @metricId
                                                            AND MetricValueDateTime = @metricValueDateTime
                                                        )
                                                    ", new SqlParameter( "@metricId", metric.Id ), new SqlParameter( "@metricValueDateTime", metricValueDateTime ) );

                                                    rockContextForMetricValues.Database.ExecuteSqlCommand( @"
                                                        DELETE
                                                        FROM MetricValue
                                                        WHERE MetricId = @metricId
                                                        AND MetricValueDateTime = @metricValueDateTime
                                                    ", new SqlParameter( "@metricId", metric.Id ), new SqlParameter( "@metricValueDateTime", metricValueDateTime ) );
                                                }
                                            }
                                        }

                                        metricValueService.AddRange( metricValuesToAdd );

                                        // disable savechanges PrePostProcessing since there could be hundreds or thousands of metric values getting inserted/updated
                                        rockContextForMetricValues.SaveChanges( true );
                                        dbTransaction.Commit();
                                    }

                                    rockContextForMetricEntity.SaveChanges();
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    metricExceptions.Add( new Exception( string.Format( "Exception when calculating metric for {0} ", metric ), ex ) );
                }
            }

            context.Result = string.Format( "Calculated a total of {0} metric values for {1} metrics", metricValuesCalculated, metricsCalculated );

            if ( metricExceptions.Any() )
            {
                throw new AggregateException( "One or more metric calculations failed ", metricExceptions );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct ResultValue
        {
            public List<ResultValuePartition> Partitions { get; set; }

            public decimal Value { get; set; }

            public DateTime MetricValueDateTime { get; set; }
        }

        private struct ResultValuePartition
        {
            // Zero-based partition position
            public int PartitionPosition { get; set; }

            public int? EntityId { get; set; }
        }
    }
}
