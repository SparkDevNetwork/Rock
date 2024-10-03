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
using System.Threading.Tasks;

using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// This command is used to request the proxy status from whichever server
    /// is handling cloud print proxy connections.
    /// </summary>
    internal class CloudPrintSendProxyStatusMessage : IEventMessage<CloudPrintEventQueue>
    {
        /// <inheritdoc/>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Requests all server send any data they have on cloud print proxy
        /// connections to the RealTime engine.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the operation.</returns>
        public static Task PublishAsync()
        {
            var message = new CloudPrintSendProxyStatusMessage();

            return RockMessageBus.PublishAsync<CloudPrintEventQueue, CloudPrintSendProxyStatusMessage>( message );
        }
    }
}
