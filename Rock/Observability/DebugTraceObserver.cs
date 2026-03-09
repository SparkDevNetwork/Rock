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
    /// This is used to interact with the debug trace system for managing
    /// single page debug trace requests.
    /// </summary>
    internal class DebugTraceObserver
    {
        #region Fields

        /// <summary>
        /// The shared trace provider that is used by all requests that have
        /// requested tracing be enabled. When no more traces are being monitored
        /// then this will be disposed after a short delay.
        /// </summary>
        private TracerProvider _sharedTraceProvider;

        /// <summary>
        /// The current number of active traces that are being monitored.
        /// </summary>
        private int _sharedTraceProviderUsageCount = 0;

        /// <summary>
        /// Used to cancel the disposal of the shared trace provider if a new
        /// trace has been started.
        /// </summary>
        private CancellationTokenSource _sharedTraceProviderDisposeCancellationTokenSource;

        /// <summary>
        /// The shared lock for access to the other static fields.
        /// </summary>
        private readonly object _sharedTraceProviderLock = new object();

        #endregion

        #region Methods

        /// <summary>
        /// Installs the shared trace provider if it is not already active
        /// in the system.
        /// </summary>
        public void BeginTracing()
        {
            lock ( _sharedTraceProviderLock )
            {
                if ( _sharedTraceProvider == null )
                {
                    _sharedTraceProvider = Sdk.CreateTracerProviderBuilder()
                        .AddProcessor( new DebugTraceProcessor() )
                        .AddSource( ObservabilityHelper.ServiceName )
                        .AddSource( "Microsoft.SemanticKernel*" )
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
        public void MonitorTrace( string traceId )
        {
            DebugTraceProcessor.ActiveTraces.AddOrReplace( traceId, new DebugTraceTracker() );
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
        public void ValidateTrace( string traceId )
        {
            if ( DebugTraceProcessor.ActiveTraces.TryGetValue( traceId, out var tracker ) )
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
        public void LinkTrace( string childTraceId, string parentTraceId )
        {
            DebugTraceProcessor.LinkedTraces.AddOrReplace( childTraceId, parentTraceId );
        }

        /// <summary>
        /// Determines if the specified trace has been validated for retrieval.
        /// </summary>
        /// <param name="traceId">The identifier of the trace.</param>
        /// <returns><c>true</c> if the trace has been previously validated; otherwise <c>false</c>.</returns>
        public virtual bool IsValidTrace( string traceId )
        {
            if ( DebugTraceProcessor.ActiveTraces.TryGetValue( traceId, out var tracker ) )
            {
                return tracker.IsValidated;
            }

            return false;
        }

        /// <summary>
        /// Requests that tracing be stopped. If this is the last active trace
        /// then the shared trace provider will be disposed after a short delay.
        /// </summary>
        public void EndTracing()
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
                                        DebugTraceProcessor.ActiveTraces = new ConcurrentDictionary<string, DebugTraceTracker>();
                                        DebugTraceProcessor.LinkedTraces = new ConcurrentDictionary<string, string>();
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
        public List<Activity> GetTraceActivities( string traceId )
        {
            var activities = new List<Activity>();

            if ( DebugTraceProcessor.ActiveTraces.TryGetValue( traceId, out var tracker ) && tracker.IsValidated )
            {
                while ( tracker.Queue.TryDequeue( out var activity ) )
                {
                    activities.Add( activity );
                }
            }

            return activities;
        }

        #endregion
    }
}
