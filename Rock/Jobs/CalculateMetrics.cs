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

            var metricsQry = new MetricService( new RockContext() ).Queryable().AsNoTracking().Where(
                a => a.ScheduleId.HasValue
                && a.SourceValueTypeId.HasValue
                && ( a.SourceValueType.Guid == metricSourceValueTypeDataviewGuid || a.SourceValueType.Guid == metricSourceValueTypeSqlGuid ) );

            var metricIdList = metricsQry.OrderBy( a => a.Title ).ThenBy( a => a.Subtitle ).Select( a => a.Id ).ToList();

            var metricExceptions = new List<Exception>();
            int metricsCalculated = 0;
            int metricValuesCalculated = 0;
            Metric metric = null;

            foreach ( var metricId in metricIdList )
            {
                try
                {
                    var rockContext = new RockContext();
                    var metricService = new MetricService( rockContext );
                    var metricValueService = new MetricValueService( rockContext );
                    metric = metricService.Get( metricId );
                    var lastRunDateTime = metric.LastRunDateTime ?? metric.CreatedDateTime ?? metric.ModifiedDateTime;
                    if ( lastRunDateTime.HasValue )
                    {
                        var currentDateTime = RockDateTime.Now;

                        // get all the schedule times that were supposed to run since that last time it was scheduled to run
                        var scheduledDateTimesToProcess = metric.Schedule.GetScheduledStartTimes( lastRunDateTime.Value, currentDateTime ).Where( a => a > lastRunDateTime.Value ).ToList();
                        foreach ( var scheduleDateTime in scheduledDateTimesToProcess )
                        {
                            List<ResultValue> resultValues = new List<ResultValue>();
                            if ( metric.SourceValueType.Guid == metricSourceValueTypeDataviewGuid )
                            {
                                // get the metric value from the DataView
                                if ( metric.DataView != null )
                                {
                                    var errorMessages = new List<string>();
                                    var qry = metric.DataView.GetQuery( null, null, out errorMessages );
                                    if ( metric.MetricPartitions.Count > 1 || metric.MetricPartitions.First().EntityTypeId.HasValue )
                                    {
                                        throw new NotImplementedException( "Partitioned Metrics using DataViews is not supported." );
                                    }
                                    else
                                    {
                                        var resultValue = new ResultValue();
                                        resultValue.Value = Convert.ToDecimal( qry.Count() );
                                        resultValue.Partitions = new List<ResultValuePartition>();
                                        resultValues.Add( resultValue );
                                    }
                                }
                            }
                            else if ( metric.SourceValueType.Guid == metricSourceValueTypeSqlGuid )
                            {
                                // calculate the metricValue assuming that the SQL returns one row with one or more numeric fields
                                if ( !string.IsNullOrWhiteSpace( metric.SourceSql ) )
                                {
                                    string formattedSql = metric.SourceSql.ResolveMergeFields( metric.GetMergeObjects( scheduleDateTime ) );
                                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );
                                    foreach ( var row in tableResult.Rows.OfType<System.Data.DataRow>() )
                                    {
                                        // assume SQL is in the form "SELECT Count(*), Partion0EntityId, Partion1EntityId, Partion2EntityId,.. FROM ..."
                                        var resultValue = new ResultValue();
                                        resultValue.Value = Convert.ToDecimal( row[0] );
                                        resultValue.Partitions = new List<ResultValuePartition>();
                                        int partitionPosition = 0;
                                        int partitionColumnCount = tableResult.Columns.Count - 1;
                                        while ( partitionPosition < partitionColumnCount )
                                        {
                                            resultValue.Partitions.Add( new ResultValuePartition
                                            {
                                                PartitionPosition = partitionPosition,
                                                EntityId = row[partitionPosition+1] as int?
                                            } );

                                            partitionPosition++;
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
                                    metricValue.MetricValueDateTime = scheduleDateTime;
                                    metricValue.MetricValueType = MetricValueType.Measure;
                                    metricValue.YValue = resultValue.Value;
                                    metricValue.MetricValuePartitions = new List<MetricValuePartition>();
                                    var metricPartitionsByPosition = metric.MetricPartitions.OrderBy(a => a.Order).ToList();


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

                                metricValueService.AddRange(metricValuesToAdd);
                            }

                            rockContext.SaveChanges();
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
        }

        private struct ResultValuePartition
        {
            // Zero-based partition position
            public int PartitionPosition { get; set; }
            
            public int? EntityId { get; set; }
        }
    }
}
