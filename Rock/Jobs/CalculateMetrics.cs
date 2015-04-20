// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            var metricValueService = new MetricValueService( rockContext );

            var metricSourceValueTypeDataviewGuid = Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();
            var metricSourceValueTypeSqlGuid = Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();

            var metricsQry = metricService.Queryable( "Schedule" ).Where(
                a => a.ScheduleId.HasValue
                && a.SourceValueTypeId.HasValue
                && ( a.SourceValueType.Guid == metricSourceValueTypeDataviewGuid || a.SourceValueType.Guid == metricSourceValueTypeSqlGuid ) );

            var metricsList = metricsQry.OrderBy( a => a.Title ).ThenBy( a => a.Subtitle ).ToList();

            var metricExceptions = new List<Exception>();

            foreach ( var metric in metricsList )
            {
                try
                {
                    var lastRunDateTime = metric.LastRunDateTime ?? metric.CreatedDateTime ?? metric.ModifiedDateTime;
                    if ( lastRunDateTime.HasValue )
                    {
                        var currentDateTime = RockDateTime.Now;
                        // get all the schedule times that were supposed to run since that last time it was scheduled to run
                        var scheduledDateTimesToProcess = metric.Schedule.GetScheduledStartTimes( lastRunDateTime.Value, currentDateTime ).Where( a => a > lastRunDateTime.Value ).ToList();
                        foreach ( var scheduleDateTime in scheduledDateTimesToProcess )
                        {
                            Dictionary<int, decimal> resultValues = new Dictionary<int, decimal>();
                            if ( metric.SourceValueType.Guid == metricSourceValueTypeDataviewGuid )
                            {
                                // get the metric value from the DataView
                                if ( metric.DataView != null )
                                {
                                    var errorMessages = new List<string>();
                                    var qry = metric.DataView.GetQuery( null, null, out errorMessages );
                                    if ( metric.EntityTypeId.HasValue )
                                    {
                                        throw new NotImplementedException( "Partitioned Metrics using DataViews is not supported." );
                                    }
                                    else
                                    {
                                        resultValues.Add( 0, qry.Count() );
                                    }
                                }
                            }
                            else if ( metric.SourceValueType.Guid == metricSourceValueTypeSqlGuid )
                            {
                                // calculate the metricValue assuming that the SQL returns one row with one numeric field
                                if ( !string.IsNullOrWhiteSpace( metric.SourceSql ) )
                                {
                                    string formattedSql = metric.SourceSql.ResolveMergeFields( metric.GetMergeObjects( scheduleDateTime ) );
                                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );
                                    foreach ( var row in tableResult.Rows.OfType<System.Data.DataRow>() )
                                    {
                                        int entityId = 0;
                                        decimal countValue;
                                        if ( tableResult.Columns.Count >= 2 )
                                        {
                                            if ( tableResult.Columns.Contains( "EntityId" ) )
                                            {
                                                entityId = Convert.ToInt32( row["EntityId"] );
                                            }
                                            else
                                            {
                                                // assume SQL is in the form "SELECT Count(*), EntityId FROM ..."
                                                entityId = Convert.ToInt32( row[1] );
                                            }
                                        }

                                        if ( tableResult.Columns.Contains( "Value" ) )
                                        {
                                            countValue = Convert.ToDecimal( row["Value"] );
                                        }
                                        else
                                        {
                                            // assume SQL is in the form "SELECT Count(*), EntityId FROM ..."
                                            countValue = Convert.ToDecimal( row[0] );
                                        }

                                        resultValues.Add( entityId, countValue );
                                    }
                                }
                            }

                            metric.LastRunDateTime = scheduleDateTime;

                            if ( resultValues.Any() )
                            {
                                foreach ( var resultValue in resultValues )
                                {
                                    var metricValue = new MetricValue();
                                    metricValue.MetricId = metric.Id;
                                    metricValue.MetricValueDateTime = scheduleDateTime;
                                    metricValue.MetricValueType = MetricValueType.Measure;
                                    metricValue.YValue = resultValue.Value;
                                    metricValue.EntityId = resultValue.Key > 0 ? resultValue.Key : (int?)null;

                                    metricValueService.Add( metricValue );
                                }
                            }

                            rockContext.SaveChanges();
                        }
                    }
                }
                catch ( Exception ex )
                {
                    metricExceptions.Add( new Exception( string.Format( "Exception when calculating metric for ", metric ), ex ) );
                }
            }

            if ( metricExceptions.Any() )
            {
                throw new AggregateException( "One or more metric calculations failed ", metricExceptions );
            }
        }
    }
}
