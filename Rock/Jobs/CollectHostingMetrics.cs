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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that collects hosting metrics.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Collect Hosting Metrics" )]
    [Description( @"This job will collect a few hosting metrics regarding the usage of resources such as the database connection pool. Note that this Job can be activated/deactivated by navigating to ""Admin Tools > System Settings > System Configuration > Web.Config Settings"" and toggling the ""Enable Database Performance Counters"" setting." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class CollectHostingMetrics : IJob
    {
        #region AttributeKeys

        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        #endregion

        #region PerformanceCounterMetricValue

        private class PerformanceCounterMetricValue
        {
            public float Value { get; set; }

            public DateTime MetricValueDateTime { get; } = RockDateTime.Now;
        }

        #endregion

        #region Fields

        private int _commandTimeout;

        private const string PerformanceCounterCategoryAdoNet = ".NET Data Provider for SqlServer";
        
        /*
         * 2020-04-22 - JPH
         *
         * The ADO.NET PerformanceCounters represent just one category of many that we can tap into if desired.
         * Leveraging the CategoryAttribute and DescriptionAttribute will allow us to easily tap into those other
         * counters in the future.
         *
         */
        private enum PerformanceCounterName
        {
            #region ADO.NET PerformanceCounters

            [Category(PerformanceCounterCategoryAdoNet)]
            [Description("HardConnectsPerSecond")]
            HardConnectsPerSecond,

            [Category(PerformanceCounterCategoryAdoNet)]
            [Description("NumberOfActiveConnections")]
            NumberOfActiveConnections,

            [Category(PerformanceCounterCategoryAdoNet)]
            [Description("NumberOfFreeConnections")]
            NumberOfFreeConnections,

            [Category(PerformanceCounterCategoryAdoNet)]
            [Description("SoftConnectsPerSecond")]
            SoftConnectsPerSecond

            #endregion
        }

        // Map each PerformanceCounterName to the appropriate Metric, where its value will be recorded.
        private static readonly IDictionary<string, Guid> MetricGuidByPerformanceCounterNames = new Dictionary<string, Guid>
        {
            { PerformanceCounterName.HardConnectsPerSecond.GetDescription(), SystemGuid.Metric.HOSTING_HARD_CONNECTS_PER_SECOND.AsGuid() },
            { PerformanceCounterName.NumberOfActiveConnections.GetDescription(), SystemGuid.Metric.HOSTING_NUMBER_OF_ACTIVE_CONNECTIONS.AsGuid() },
            { PerformanceCounterName.NumberOfFreeConnections.GetDescription(), SystemGuid.Metric.HOSTING_NUMBER_OF_FREE_CONNECTIONS.AsGuid() },
            { PerformanceCounterName.SoftConnectsPerSecond.GetDescription(), SystemGuid.Metric.HOSTING_SOFT_CONNECTS_PER_SECOND.AsGuid() }
        };

        // Static, so this collection will live across instances of the CollectHostingMetrics class.
        private static IList<PerformanceCounter> _performanceCounters;

        // For each Job run, the value of each PerformanceCounter will be stored here, before being saved as a new MetricValue.
        private IDictionary<Guid, PerformanceCounterMetricValue> _performanceCounterMetricValues;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/>s of interest.
        /// </summary>
        /// <value>
        /// The <see cref="PerformanceCounter"/>s of interest.
        /// </value>
        public static IList<PerformanceCounter> PerformanceCounters
        {
            get
            {
                if ( _performanceCounters == null )
                {
                    _performanceCounters = new List<PerformanceCounter>();
                }

                return _performanceCounters;
            }
        }

        #endregion

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            try
            {
                /*
                 * 2020-04-23 - JPH
                 *
                 * If we add more (non ADO.NET) PerformanceCounters to this Job in the future, not only will the following simple check need to be enhanced,
                 * but we'll also want to change the Description for this Job to accurately reflect when the Job will be Active/Inactive.
                 *
                 */
                if ( !AreAdoNetPerformanceCountersEnabled() )
                {
                    throw new Exception( @"You must first navigate to ""Admin Tools > System Settings > System Configuration > Web.Config Settings"" and enable the ""Enable Database Performance Counters"" setting." );
                }

                JobDataMap dataMap = context.JobDetail.JobDataMap;

                // Get the configured timeout, or default to 60 minutes if it is blank.
                _commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

                SetUpPerformanceCounters();
                ReadPerformanceCounters();

                SaveMetricValues( context );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Determines if the ADO.NET <see cref="PerformanceCounter"/>s are enabled.
        /// </summary>
        /// <returns>Whether the ADO.NET <see cref="PerformanceCounter"/>s are enabled.</returns>
        private bool AreAdoNetPerformanceCountersEnabled()
        {
            return SystemSettings.GetValue( SystemSetting.SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS ).AsBoolean();
        }

        /// <summary>
        /// Gets the current Process ID.
        /// </summary>
        /// <returns>The Process ID of the ASP.NET application.</returns>
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern int GetCurrentProcessId();

        /// <summary>
        /// Gets the name of the instance, which will be used to locate the <see cref="PerformanceCounter"/>s of relevance.
        /// </summary>
        /// <returns>FriendlyName[Process ID] of the ASP.NET application.</returns>
        private string GetInstanceName()
        {
            // For ASP.NET applications the instanceName will be the CurrentDomain's FriendlyName[Process ID].
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/performance-counters#example
            string instanceName = AppDomain.CurrentDomain.FriendlyName.ToString()
                .Replace( '(', '[' )
                .Replace( ')', ']' )
                .Replace( '#', '_' )
                .Replace( '/', '_' )
                .Replace( '\\', '_' );

            int pid = GetCurrentProcessId();

            instanceName = $"{instanceName}[{pid}]";

            return instanceName;
        }

        /// <summary>
        /// Sets up the <see cref="PerformanceCounter"/>s of interest.
        /// </summary>
        private void SetUpPerformanceCounters()
        {
            if ( _performanceCounters != null )
            {
                // The PerformanceCounters have already been set up.
                return;
            }

            string instanceName = GetInstanceName();

            foreach ( PerformanceCounterName name in Enum.GetValues( typeof( PerformanceCounterName ) ) )
            {
                PerformanceCounters.Add( new PerformanceCounter
                {
                    CategoryName = name.GetAttribute<CategoryAttribute>()?.Category,
                    CounterName = name.GetDescription(),
                    InstanceName = instanceName
                } );
            }
        }

        /// <summary>
        /// Reads the <see cref="PerformanceCounter"/> values of interest.
        /// </summary>
        private void ReadPerformanceCounters()
        {
            _performanceCounterMetricValues = new Dictionary<Guid, PerformanceCounterMetricValue>();

            foreach ( PerformanceCounter performanceCounter in PerformanceCounters )
            {
                if ( MetricGuidByPerformanceCounterNames.TryGetValue( performanceCounter.CounterName, out Guid metricGuid ))
                {
                    _performanceCounterMetricValues.Add( metricGuid, new PerformanceCounterMetricValue { Value = performanceCounter.NextValue() } );
                }
            }
        }

        /// <summary>
        /// Saves the <see cref="MetricValue"/>s and updates the <see cref="IJobExecutionContext"/>'s Result property.
        /// </summary>
        /// <param name="context">The <see cref="IJobExecutionContext"/>.</param>
        /// <exception cref="Exception">Unable to find the "Hosting Metrics" Category ID.</exception>
        private void SaveMetricValues( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = _commandTimeout;

            var hostingMetricsCategoryId = CategoryCache.GetId( SystemGuid.Category.METRIC_HOSTING_METRICS.AsGuid() );
            if ( !hostingMetricsCategoryId.HasValue )
            {
                throw new Exception( @"Unable to find the ""Hosting Metrics"" Category ID." );
            }

            var metricIdsQuery = new MetricCategoryService( rockContext ).Queryable()
                .Where( mc => mc.CategoryId == hostingMetricsCategoryId )
                .Select( mc => mc.MetricId );

            // Get all of the Metrics tied to the "Hosting Metrics" Category
            var metrics = new MetricService( rockContext ).Queryable( "MetricPartitions" )
                .Where( m => metricIdsQuery.Contains( m.Id ) )
                .ToList();

            var metricValues = new List<MetricValue>();

            foreach ( var metric in metrics )
            {
                // Attempt to add any PerformanceCounter MetricValues
                TryAddPerformanceCounterMetricValue( metric, metricValues );
            }

            /*
             * 2020-04-22 - JPH
             *
             * The Metrics being collected by the first revision of this Job each have a single, default MetricPartition.
             * If we add Metrics to this Job in the future that are more complicated, we'll want to revisit the below logic.
             *
             */

            var metricValueService = new MetricValueService( rockContext );

            foreach ( var metricValue in metricValues )
            {
                foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
                {
                    metricValue.MetricValuePartitions.Add( new MetricValuePartition { MetricPartitionId = metricPartition.Id } );
                }

                metricValueService.Add( metricValue );
            }

            rockContext.SaveChanges();

            context.Result = $"Calculated a total of {metricValues.Count} metric values for {metrics.Count} metrics";
        }

        /// <summary>
        /// Attempts to Add <see cref="MetricValue"/>s for the <see cref="PerformanceCounter"/>s of interest.
        /// </summary>
        /// <param name="metric">The <see cref="Metric"/> in question.</param>
        /// <param name="metricValues">The <see cref="MetricValue"/> collection to which any new <see cref="MetricValue"/>s should be added.</param>
        private void TryAddPerformanceCounterMetricValue( Metric metric, IList<MetricValue> metricValues )
        {
            if ( _performanceCounterMetricValues != null && _performanceCounterMetricValues.TryGetValue( metric.Guid, out PerformanceCounterMetricValue performanceCounterMetricValue) )
            {
                metricValues.Add( new MetricValue
                {
                    Metric = metric,
                    MetricId = metric.Id,
                    MetricValueType = MetricValueType.Measure,
                    YValue = (decimal)performanceCounterMetricValue.Value,
                    MetricValueDateTime = performanceCounterMetricValue.MetricValueDateTime
                } );
            }
        }
    }
}
