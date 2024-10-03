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

using System.Threading.Tasks;

using MassTransit;

using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.RealTime.Topics;
using Rock.RealTime;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Handles <see cref="CloudPrintSendProxyStatusMessage"/> messages by checking
    /// if this node has any connected proxies and sending out the details over
    /// the RealTime engine.
    /// </summary>
    internal class CloudPrintSendProxyStatusConsumer : RockConsumer<CloudPrintEventQueue, CloudPrintSendProxyStatusMessage>
    {
        /// <inheritdoc/>
        public override Task Consume( ConsumeContext<CloudPrintSendProxyStatusMessage> context )
        {
            var status = CloudPrintSocket.GetProxyStatus();
            var topicClients = RealTimeHelper.GetTopicContext<ICloudPrint>().Clients;

            return topicClients
                .Channel( CloudPrintTopic.ProxyStatusChannel )
                .ProxyStatus( status );
        }

        /// <inheritdoc/>
        public override void Consume( CloudPrintSendProxyStatusMessage message )
        {
            // This will never be called.
        }
    }
}
