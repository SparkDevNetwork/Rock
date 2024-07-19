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

using System.Linq;
using System.Threading.Tasks;

using MassTransit;

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Handles <see cref="CloudPrintProxyStatusMessage"/> messages by checking
    /// if this node has any connected services for the indicated proxy device
    /// and returns those to the original node.
    /// </summary>
    internal class CloudPrintProxyStatusConsumer : RockConsumer<CloudPrintQueue, CloudPrintProxyStatusMessage>
    {
        /// <inheritdoc/>
        public override Task Consume( ConsumeContext<CloudPrintProxyStatusMessage> context )
        {
            var proxies = CloudPrintSocket.GetAllProxiesForDevice( context.Message.DeviceId );

            // Don't send a response if we don't have anything. This is how we
            // make sure only the right server responds. In the future, we can
            // use an elected print server to know if we should respond or not.
            if ( proxies == null || proxies.Count == 0 )
            {
                return Task.CompletedTask;
            }

            var response = new CloudPrintProxyStatusMessage.Response
            {
                Id = context.Message.DeviceId,
                NodeName = RockMessageBus.NodeName,
                Connections = proxies.Select( p => new CloudPrintProxyStatusMessage.Connection
                {
                    Name = p.Name,
                    Priority = p.Priority
                } ).ToList()
            };

            context.Respond( response );

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Consume( CloudPrintProxyStatusMessage message )
        {
            // This will never be called.
        }
    }
}
