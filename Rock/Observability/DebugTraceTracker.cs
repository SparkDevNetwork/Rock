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

namespace Rock.Observability
{
    /// <summary>
    /// Simple class to track the activities associated with a trace and
    /// if it is safe to let the client retrieve them.
    /// </summary>
    internal class DebugTraceTracker
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
}
