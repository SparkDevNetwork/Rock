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

using Rock.WebFarm;

namespace Rock.Bus.Queue
{
    /// <summary>
    /// A Rock Message Bus Queue for the Web Farm
    /// </summary>
    public sealed class WebFarmQueue : PublishEventQueue
    {
        /// <summary>
        /// Gets the queue name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => "rock-web-farm-queue";

        /// <summary>
        /// Gets the name for configuration. For the web farm we want each node/process
        /// combo to be a unique consumer. This is because of IIS process recycling.
        /// </summary>
        /// <value>
        /// The name for configuration.
        /// </value>
        public override string NameForConfiguration => $"{Name}_{RockMessageBus.NodeName}_{RockWebFarm.ProcessId}";
    }
}
