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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Rock.Observability
{
    /// <summary>
    /// This is an internal processor that is used to capture trace information
    /// for per-session debugging. This is used when a page load has been
    /// requested to include page timings.
    /// </summary>
    internal class DebugTraceProcessor : BaseProcessor<Activity>
    {
        #region Fields

        /// <summary>
        /// The shared trace provider that is used by all requests that have
        /// requested tracing be enabled. When no more traces are being monitored
        /// then this will be disposed after a short delay.
        /// </summary>
        private static TracerProvider _sharedTraceProvider;

        /// <summary>
        /// The current number of active traces that are being monitored.
        /// </summary>
        private static int _sharedTraceProviderUsageCount = 0;

        /// <summary>
        /// Used to cancel the disposal of the shared trace provider if a new
        /// trace has been started.
        /// </summary>
        private static CancellationTokenSource _sharedTraceProviderDisposeCancellationTokenSource;

        /// <summary>
        /// The shared lock for access to the other static fields.
        /// </summary>
        private static readonly object _sharedTraceProviderLock = new object();

        /// <summary>
        /// Stores the currently active trace trackers, indexed by their
        /// trace identifiers. This does not need to be accessed from within
        /// the <see cref="_sharedTraceProviderLock"/> lock.
        /// </summary>
        private static ConcurrentDictionary<string, Tracker> _activeTraces = new ConcurrentDictionary<string, Tracker>();

        /// <summary>
        /// Stores the linked traces so that child traces can be associated
        /// with their parent traces for retrieval. This does not need to be
        /// accessed from within the <see cref="_sharedTraceProviderLock"/> lock.
        /// </summary>
        private static ConcurrentDictionary<string, string> _linkedTraces = new ConcurrentDictionary<string, string>();

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// </summary>
        private DebugTraceProcessor()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnEnd( Activity data )
        {
            var traceId = data.TraceId.ToString();

            if ( _linkedTraces.TryGetValue( traceId, out var parentTraceId ) )
            {
                traceId = parentTraceId;
            }

            if ( _activeTraces.TryGetValue( traceId, out var tracker ) )
            {
                tracker.Queue.Enqueue( data );
            }
        }

        /// <summary>
        /// Installs the shared trace provider if it is not already active
        /// in the system.
        /// </summary>
        public static void BeginTracing()
        {
            lock( _sharedTraceProviderLock )
            {
                if ( _sharedTraceProvider == null )
                {
                    _sharedTraceProvider = Sdk.CreateTracerProviderBuilder()
                        .AddProcessor( new DebugTraceProcessor() )
                        .AddSource( ObservabilityHelper.ServiceName )
                        .Build();
                }

                _sharedTraceProviderUsageCount++;
                _sharedTraceProviderDisposeCancellationTokenSource?.Cancel();
                _sharedTraceProviderDisposeCancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Begins monitoring the specified trace identifier.
        /// </summary>
        /// <param name="traceId">The identifier of the trace to be recorded.</param>
        public static void MonitorTrace( string traceId )
        {
            _activeTraces.AddOrReplace( traceId, new Tracker() );
        }

        /// <summary>
        /// Marks the trace as valid for retrieval. We start monitoring the
        /// trace before we have validated that the page being requested matches
        /// the page specified in the query string. Once we get into the page
        /// logic we can then validate that the query string matches the actual
        /// page being displayed. Only then do we mark the trace as validated.
        /// This prevents a malicious user from being able to request traces
        /// by passing a different page they do have access to in the query
        /// string.
        /// </summary>
        /// <param name="traceId">The identifier of the trace associated with the page load.</param>
        public static void ValidateTrace( string traceId )
        {
            if ( _activeTraces.TryGetValue( traceId, out var tracker ) )
            {
                tracker.IsValidated = true;
            }
        }

        /// <summary>
        /// Links a child trace to a parent trace so that their activities
        /// are related and can be retrieved together.
        /// </summary>
        /// <param name="childTraceId">The child (current) trace identifier.</param>
        /// <param name="parentTraceId">The parent trace identifier.</param>
        public static void LinkTrace( string childTraceId, string parentTraceId )
        {
            _linkedTraces.AddOrReplace( childTraceId, parentTraceId );
        }

        /// <summary>
        /// Determines if the specified trace has been validated for retrieval.
        /// </summary>
        /// <param name="traceId">The identifier of the trace.</param>
        /// <returns><c>true</c> if the trace has been previously validated; otherwise <c>false</c>.</returns>
        public static bool IsValidTrace( string traceId )
        {
            if ( _activeTraces.TryGetValue( traceId, out var tracker ) )
            {
                return tracker.IsValidated;
            }

            return false;
        }

        /// <summary>
        /// Requests that tracing be stopped. If this is the last active trace
        /// then the shared trace provider will be disposed after a short delay.
        /// </summary>
        public static void EndTracing()
        {
            lock ( _sharedTraceProviderLock )
            {
                if ( _sharedTraceProviderUsageCount > 0 )
                {
                    _sharedTraceProviderUsageCount--;

                    if ( _sharedTraceProviderUsageCount == 0 )
                    {
                        // Shouldn't need to do this, but just in case something crazy happens.
                        _sharedTraceProviderDisposeCancellationTokenSource?.Cancel();

                        // Create the cancellation token source that can be
                        // used when a new trace starts to abort the disposal.
                        _sharedTraceProviderDisposeCancellationTokenSource = new CancellationTokenSource();

                        var token = _sharedTraceProviderDisposeCancellationTokenSource.Token;

                        Task.Run( async () =>
                        {
                            try
                            {
                                await Task.Delay( TimeSpan.FromMinutes( 1 ), token );

                                lock ( _sharedTraceProviderLock )
                                {
                                    // Make sure the usage count is still zero.
                                    if ( _sharedTraceProviderUsageCount == 0 )
                                    {
                                        _sharedTraceProvider.Dispose();
                                        _sharedTraceProvider = null;
                                        _activeTraces = new ConcurrentDictionary<string, Tracker>();
                                        _linkedTraces = new ConcurrentDictionary<string, string>();
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore exceptions, this is probably a cancellation.
                            }
                        } );
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the activities associated with the specified trace. This
        /// may include activities for other traces that are linked to the
        /// specified trace. Activities will be removed when they are retrieved.
        /// </summary>
        /// <param name="traceId">The identifier of the trace to retrieve activities for.</param>
        /// <returns>A set of <see cref="Activity"/> records for the trace.</returns>
        public static List<Activity> GetTraceActivities( string traceId )
        {
            var activities = new List<Activity>();

            if ( _activeTraces.TryGetValue( traceId, out var tracker ) && tracker.IsValidated )
            {
                while ( tracker.Queue.TryDequeue( out var activity ) )
                {
                    activities.Add( activity );
                }
            }

            return activities;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Simple class to track the activities associated with a trace and
        /// if it is safe to let the client retrieve them.
        /// </summary>
        private class Tracker
        {
            /// <summary>
            /// Indicates if the trace has been validated as being safe to
            /// return to the client.
            /// </summary>
            public bool IsValidated { get; set; }

            /// <summary>
            /// The queue of traces that have been recorded for this trace.
            /// </summary>
            public ConcurrentQueue<Activity> Queue { get; } = new ConcurrentQueue<Activity>();
        }

        #endregion
    }
}
