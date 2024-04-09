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
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;

using Rock.Data;

namespace Rock.Observability
{
    internal static class RockMetricSource
    {
        #region Private Members

        internal static Meter MeterInstance;
        static TagList _commonTags = new TagList();

        // Internal variables for metrics on the garbage collector
        private const int NumberOfGenerations = 3;
        private static readonly string[] GenNames = new string[] { "gen0", "gen1", "gen2", "loh", "poh" };

        // Performance counters
        private static readonly PerformanceCounter _cpuPerformancCounter = null;

        private static readonly string _rockVersion = VersionInfo.VersionInfo.GetRockProductVersionFullName();

        #endregion

        #region Properties

        /// <summary>
        /// Counter for tracking the number of HTTP requests for Rock.
        /// </summary>
        public static Counter<long> AllRequestCounter { get; private set; }

        /// <summary>
        /// Counter for tracking the number of page loads for Rock.
        /// </summary>
        public static Counter<long> PageRequestCounter { get;  private set; }

        /// <summary>
        /// Counter for tracking the number of API requests for Rock.
        /// </summary>
        public static Counter<long> ApiRequestCounter { get; private set; }

        /// <summary>
        /// Counter for tracking the number of Handler requests for Rock.
        /// </summary>
        public static Counter<long> HandlerRequestCounter { get; private set; }

        /// <summary>
        /// Counter for tracking the number of mobile app requests for Rock.
        /// </summary>
        public static Counter<long> MobileAppRequestCounter { get; private set; }

        /// <summary>
        /// Counter for tracking the number of TV app requests for Rock.
        /// </summary>
        public static Counter<long> TvAppRequestCounter { get; private set; }

        /// <summary>
        /// Counter for tracking database queries.
        /// </summary>
        public static Counter<long> DatabaseQueriesCounter { get; private set; }

        /// <summary>
        /// List of tags that should be applied to all metrics coming from Rock.
        /// </summary>
        public static TagList CommonTags { get { return _commonTags; } }

        #endregion

        #region Constructor

        static RockMetricSource()
        {
            // Initialize the global metric
            MeterInstance = new Meter( ObservabilityHelper.ServiceName, "1.0.0" );

            _commonTags.Add( "rock.version", _rockVersion );

            // Create needed Performance Counters
            try
            {
                _cpuPerformancCounter = new PerformanceCounter( "Processor", "% Processor Time", "_Total" );
            }
            catch
            {
                _cpuPerformancCounter = null;
            }
        }

        #endregion

        internal static void StartCoreMetrics()
        {
            // Create hosting environment metrics
            // Code from: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/5f45bc6d662ba20b87cb6b48831599905f7d7d77/src/OpenTelemetry.Instrumentation.Process/ProcessMetrics.cs

            MeterInstance.CreateObservableCounter(
                "process.cpu.time",
                () =>
                {
                    var process = Process.GetCurrentProcess();

                    var userStateTags = _commonTags;
                    userStateTags.Add( "state", "user" );

                    var systemStateTags = _commonTags;
                    systemStateTags.Add( "state", "system" );

                    return new[]
                    {
                        new Measurement<double>(process.UserProcessorTime.TotalSeconds, userStateTags ),
                        new Measurement<double>(process.PrivilegedProcessorTime.TotalSeconds, systemStateTags ),
                    };
                },
                unit: "s",
                description: "Total CPU seconds broken down by different states." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.cpu.count",
                () => new Measurement<int>( Environment.ProcessorCount, _commonTags ),
                unit: "{processors}",
                description: "The number of processors (CPU cores) available to the current process." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.memory.usage",
                () => new Measurement<long>( Process.GetCurrentProcess().WorkingSet64, _commonTags ),
                unit: "By",
                description: "The amount of physical memory allocated for this process." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.memory.virtual",
                () => new Measurement<long>( Process.GetCurrentProcess().VirtualMemorySize64, _commonTags ),
                unit: "By",
                description: "The amount of committed virtual memory for this process." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.threads",
                () => new Measurement<int>( Process.GetCurrentProcess().Threads.Count, _commonTags ),
                unit: "{threads}",
                description: "Process threads count." );



            // Code for the metrics below was pulled from:
            // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.Runtime/RuntimeMetrics.cs#L39

            MeterInstance.CreateObservableCounter(
                "process.runtime.dotnet.gc.collections.count",
                () => GetGarbageCollectionCounts(),
                unit: "count",
                description: "Number of garbage collections that have occurred since process start." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.runtime.dotnet.gc.objects.size",
                () => new Measurement<long>( GC.GetTotalMemory( false ), _commonTags ),
                unit: "bytes",
                description: "Count of bytes currently in use by objects in the GC heap that haven't been collected yet. Fragmentation and other GC committed memory pools are excluded." );

            MeterInstance.CreateObservableUpDownCounter(
                "process.runtime.dotnet.assemblies.count",
                () => new Measurement<long>( ( long ) AppDomain.CurrentDomain.GetAssemblies().Length, _commonTags ),
                description: "The number of .NET assemblies that are currently loaded." );

            var exceptionCounter = MeterInstance.CreateCounter<long>(
                "process.runtime.dotnet.exceptions.count",
                unit: "count",
                description: "Count of exceptions that have been thrown in managed code, since the observation started. The value will be unavailable until an exception has been thrown after OpenTelemetry.Instrumentation.Runtime initialization." );

                AppDomain.CurrentDomain.FirstChanceException += ( source, e ) =>
                {
                    exceptionCounter.Add( 1 );
                };

            /* 
                The following are missing because we need .Net 6+
                    + process.runtime.dotnet.gc.allocations.size
                    + process.runtime.dotnet.gc.committed_memory.size
                    + process.runtime.dotnet.gc.heap.fragmentation.size
                    + process.runtime.dotnet.jit.*
            */

            // Setup Rock specific counters
            PageRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.pages",
                unit: "count",
                description: "Count of Rock page views." );

            ApiRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.api",
                unit: "count",
                description: "Count of API requests." );

            HandlerRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.handler",
                unit: "count",
                description: "Count of Handler requests." );

            MobileAppRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.mobile",
                unit: "count",
                description: "Count of mobile application requests." );

            TvAppRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.tv",
                unit: "count",
                description: "Count of TV application requests." );

            AllRequestCounter = MeterInstance.CreateCounter<long>(
                "rock.requests.all",
                unit: "count",
                description: "Count of all HTTP requests." );

            DatabaseQueriesCounter = MeterInstance.CreateCounter<long>(
                "rock.database.queries",
                unit: "count",
                description: "Count of the number of database queries." );

            // Setup Hosting metrics (metrics about the entire server environment)
            if ( _cpuPerformancCounter != null )
            {
                MeterInstance.CreateObservableGauge<float>( "hosting.cpu.total", () => GetCpuMeasure(),
                    unit: "%",
                    description: "The percent CPU of the web VM." );
            }

            MeterInstance.CreateObservableGauge(
                "hosting.volumes.space",
                () =>
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();

                    var measures = new Measurement<double>[allDrives.Length];
                    var measureCount = 0;

                    foreach ( DriveInfo d in allDrives )
                    {
                        // If the device is not ready, ignore it to avoid an access error.
                        if ( !d.IsReady )
                        {
                            continue;
                        }
                        var tags = _commonTags;
                        tags.Add( "volume", d.Name );
                        measures[measureCount++] = new Measurement<double>( d.TotalFreeSpace, tags );
                    }

                    return measures;
                },
                unit: "bytes",
                description: "Total number of bytes free on the volume." );

            MeterInstance.CreateObservableGauge( "hosting.sql.cpu",
                GetSqlCpuMeasure,
                unit: "%",
                description: "The percent of CPU being used by the SQL database." );

            MeterInstance.CreateObservableGauge( "hosting.sql.dtu.total",
                GetSqlDtuTotalMeasure,
                unit: "dtu",
                description: "The current DTU size of the database instance." );

            MeterInstance.CreateObservableGauge( "hosting.sql.memory.usage",
                GetSqlMemoryUsageMeasure,
                unit: "bytes",
                description: "The number of bytes allocated by the SQL database." );

        }

        #region Counter Callbacks

        /// <summary>
        /// Gets the number of garbage collections that have occurred since process start.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Measurement<long>> GetGarbageCollectionCounts()
        {
            long collectionsFromHigherGeneration = 0;

            var measures = new List<Measurement<long>>();

            for ( int gen = NumberOfGenerations - 1; gen >= 0; --gen )
            {
                long collectionsFromThisGeneration = GC.CollectionCount( gen );

                var tags = _commonTags;
                tags.Add( "generation", GenNames[gen] );

                measures.Add( new Measurement<long>( collectionsFromThisGeneration - collectionsFromHigherGeneration, tags ) );
            }

            return measures;
        }

        /// <summary>
        /// Returns the total CPU time.
        /// </summary>
        /// <returns></returns>
        private static Measurement<float> GetCpuMeasure()
        {
            return new Measurement<float>( _cpuPerformancCounter.NextValue(), _commonTags );
        }

        /// <summary>
        /// Gets the SQL cpu usage as a percentage between 0 and 100.
        /// </summary>
        /// <returns>A measurement value.</returns>
        private static Measurement<float> GetSqlCpuMeasure()
        {
            using ( var rockContext = new RockContext() )
            {
                var result = rockContext.Database.SqlQuery<double>( @"
If EXISTS (SELECT * FROM [sys].[system_objects] WHERE [name] = 'dm_db_resource_stats')
    SELECT TOP 1
        CAST([avg_cpu_percent] AS FLOAT) AS [Value]
    FROM [sys].[dm_db_resource_stats]
    ORDER BY [end_time] DESC
ELSE
BEGIN TRY
    SELECT
        CASE WHEN [PBase].[cntr_value] = 0
            THEN CAST(0 AS FLOAT)
            ELSE (CAST([PCount].[cntr_value] AS FLOAT) / [PBase].[cntr_value]) * 100
        END AS [Value]
    FROM [sys].[dm_os_performance_counters] AS [PCount]
    INNER JOIN [sys].[dm_os_performance_counters] AS [PBase]
        ON [PBase].[object_name] = [PCount].[object_name]
        AND [PBase].[instance_name] = [PCount].[instance_name]
    WHERE [PCount].[object_name] = 'SQLServer:Resource Pool Stats'
    AND [PCount].[instance_name] = 'default'
    AND [PCount].[counter_name] = 'CPU usage %'
    AND [PBase].[counter_name] = 'CPU usage % base'
END TRY
BEGIN CATCH
    SELECT CAST(-1 AS FLOAT) AS [Value]
END CATCH
" ).FirstOrDefault();

                return new Measurement<float>( ( float ) result, _commonTags );
            }
        }

        /// <summary>
        /// Gets the SQL total DTU reserved for the database instance. If the
        /// database is provisioned as a 400 DTU tier database, this value will
        /// be 400.
        /// </summary>
        /// <returns>A measurement value.</returns>
        private static Measurement<float> GetSqlDtuTotalMeasure()
        {
            using ( var rockContext = new RockContext() )
            {
                var result = rockContext.Database.SqlQuery<double>( @"
If EXISTS (SELECT * FROM [sys].[system_objects] WHERE [name] = 'dm_db_resource_stats')
    SELECT TOP 1
        CAST([dtu_limit] AS FLOAT) AS [Value]
        FROM [sys].[dm_db_resource_stats]
        ORDER BY [end_time] DESC
ELSE
    SELECT CAST(0 AS FLOAT) AS [Value]
" ).FirstOrDefault();

                return new Measurement<float>( ( float ) result, _commonTags );
            }
        }

        /// <summary>
        /// Gets the SQL cpu usage as a percentage between 0 and 100.
        /// </summary>
        /// <returns>A measurement value.</returns>
        private static Measurement<long> GetSqlMemoryUsageMeasure()
        {
            using ( var rockContext = new RockContext() )
            {
                var result = rockContext.Database.SqlQuery<long>( @"
BEGIN TRY
    SELECT
        CAST([cntr_value] AS BIGINT) * 8 * 1024 AS [Value]
    FROM [sys].[dm_os_performance_counters]
    WHERE [object_name] LIKE '%:Buffer Manager%'
      AND [counter_name] = 'Database Pages'
END TRY
BEGIN CATCH
    SELECT CAST(-1 AS BIGINT) AS [Value]
END CATCH
" ).FirstOrDefault();

                return new Measurement<long>( result, _commonTags );
            }
        }

        #endregion
    }
}
