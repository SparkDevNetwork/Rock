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
using System.Collections.Concurrent;
using System.Diagnostics;

using OpenTelemetry;

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
        /// Stores the currently active trace trackers, indexed by their
        /// trace identifiers.
        /// </summary>
        internal static ConcurrentDictionary<string, DebugTraceTracker> ActiveTraces { get; set; } = new ConcurrentDictionary<string, DebugTraceTracker>();

        /// <summary>
        /// Stores the linked traces so that child traces can be associated
        /// with their parent traces for retrieval.
        /// </summary>
        internal static ConcurrentDictionary<string, string> LinkedTraces { get; set; } = new ConcurrentDictionary<string, string>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnEnd( Activity data )
        {
            var traceId = data.TraceId.ToString();

            if ( LinkedTraces.TryGetValue( traceId, out var parentTraceId ) )
            {
                traceId = parentTraceId;
            }

            if ( ActiveTraces.TryGetValue( traceId, out var tracker ) )
            {
                tracker.Queue.Enqueue( data );
            }
        }

        #endregion
    }
}
