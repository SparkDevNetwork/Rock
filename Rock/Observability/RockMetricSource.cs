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
using System.Text;
using System.Threading.Tasks;
#if REVIEW_WEBFORMS
using ImageResizer.Plugins.Basic;
using Microsoft.Ajax.Utilities;
#endif
using Rock.Bus;

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
        private static PerformanceCounter _cpuPerformancCounter = null;

        private static readonly Lazy<string> _rockVersion = new Lazy<string>( () => VersionInfo.VersionInfo.GetRockProductVersionFullName() );
        private static readonly Lazy<string> _machineName = new Lazy<string>( () => Environment.MachineName.ToLower() );

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

            // Define common tags
            var nodeName = RockMessageBus.NodeName.ToLower();
            var machineName = _machineName.Value;

            _commonTags.Add( "rock-node", nodeName );
            if ( nodeName != machineName )
            {
                _commonTags.Add( "service.instance.id", $"{machineName} ({nodeName})" );
            }
            else
            {
                _commonTags.Add( "service.instance.id", machineName );
            }
            
            _commonTags.Add( "rock-version", _rockVersion.Value );

            // Create needed Performance Counters
            _cpuPerformancCounter = new PerformanceCounter( "Processor", "% Processor Time", "_Total" );
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
            MeterInstance.CreateObservableGauge<float>( "hosting.cpu.total", () => GetCpuMeasure(),
                unit: "%",
                description: "The percent CPU of the web VM." );

            MeterInstance.CreateObservableCounter(
                "hosting.volumes.space",
                () =>
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();

                    var measures = new Measurement<double>[allDrives.Length];
                    var measureCount = 0;

                    foreach ( DriveInfo d in allDrives )
                    {
                        var tags = _commonTags;
                        tags.Add( "volume", d.Name );
                        measures[measureCount++] = new Measurement<double>( d.TotalFreeSpace, tags );
                    }

                    return measures;
                },
                unit: "bytes",
                description: "Total CPU seconds broken down by different states." );

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

        #endregion
    }
}
