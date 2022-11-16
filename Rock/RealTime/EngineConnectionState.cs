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

namespace Rock.RealTime
{
    /// <summary>
    /// State tracking for connections used by the internal Engine instance.
    /// </summary>
    internal class EngineConnectionState
    {
        /// <summary>
        /// Gets the topics this connection has connected to. This is used to
        /// check if they are allowed to send messages to the topic as well as
        /// to inform topics when a client disconnects.
        /// </summary>
        /// <value>The topics this connection has connected to.</value>
        public ConcurrentDictionary<string, bool> ConnectedTopics { get; } = new ConcurrentDictionary<string, bool>();
    }
}
