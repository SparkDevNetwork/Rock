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
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to calculate metric values for metrics that are based on a schedule and have a database or sql datasource type
    /// Only Metrics that need to be populated (based on their Schedule) will be processed
    /// </summary>
    [DisplayName( "Calculate Metrics" )]
    [Description( "A job that processes any metrics with schedules." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for any SQL based operations to complete. Leave blank to use the default for this job (300). Note, some metrics do not use SQL so this timeout will only apply to metrics that are SQL based.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 5,
        Category = "General",
        Order = 7 )]

    public class CalculateMetrics : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300;
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
            MetricService metricService = new MetricService( new RockContext() );
            foreach ( var metricId in metricIdList )
            {
                var metricResult = metricService.CalculateMetric( metricId, commandTimeout, false );
                metricValuesCalculated += metricResult.MetricValuesCalculated;
                metricsCalculated++;
                if ( metricResult.MetricException != null )
                {
                    metricExceptions.Add( metricResult.MetricException );
                }
            }

            var results = new System.Text.StringBuilder();
            results.AppendFormat( "Calculated a total of {0} metric values for {1} metrics", metricValuesCalculated, metricsCalculated ).AppendLine();

            if ( metricExceptions.Any() )
            {
                results.Append( $"Skipped: {metricExceptions.Count} metrics due to encountered errors:" );
                results.Append( "<ul>" );
                metricExceptions.ForEach( e => results.Append( $"<li>{e.Message}</li>" ) );
                results.Append( "</ul>" );
            }

            this.Result = results.ToString();

            if ( metricExceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more metric calculations failed ", metricExceptions );

                // Throw aggregate exception if every metric failed, otherwise show message displaying successful and failed calculations. 
                if ( metricIdList.Count == metricExceptions.Count )
                {
                    throw exceptionList;
                }
                else
                {
                    throw new RockJobWarningException( "Calculate Metrics completed with warnings", exceptionList );
                }
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
