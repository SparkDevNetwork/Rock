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

using Rock.Attribute;

namespace Rock.Bus.Queue
{
    /// <summary>
    /// A Rock Message Bus Queue for Cloud Print commands.
    /// </summary>
    [RockInternal( "1.16.7", true )]
    public sealed class CloudPrintQueue : SendCommandQueue
    {
        /// <summary>
        /// Gets the queue name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => "rock-cloud-print-queue";

        /// <summary>
        /// Creates a new instance of <see cref="CloudPrintQueue"/>.
        /// </summary>
        public CloudPrintQueue()
        {
            TimeToLiveSeconds = 10;
        }
    }
}
