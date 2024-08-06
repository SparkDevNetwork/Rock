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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Cache Update Message
    /// </summary>
    [RockInternal( "1.16.7", true )]
    public class CloudPrintProxyStatusMessage : ICommandMessage<CloudPrintQueue>
    {
        /// <inheritdoc/>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// The identifier of the <see cref="Model.Device"/> that represents the proxy.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets as debug string.
        /// </summary>
        /// <returns></returns>
        internal string ToDebugString()
        {
            return $"DeviceId = {DeviceId}";
        }

        /// <summary>
        /// Requests a server on the bus respond with the list of proxies it has
        /// connected to the proxy device identifier.
        /// </summary>
        /// <param name="deviceId">The proxy <see cref="Model.Device"/> identifier.</param>
        /// <param name="cancellationToken">A token that will abort the request and cancel the task.</param>
        /// <returns>The response returned by the node handling the command.</returns>
        public static async Task<Response> RequestAsync( int deviceId, CancellationToken cancellationToken = default )
        {
            var message = new CloudPrintProxyStatusMessage
            {
                DeviceId = deviceId
            };

            return await RockMessageBus.RequestAsync<CloudPrintQueue, CloudPrintProxyStatusMessage, Response>( message, cancellationToken );
        }

        /// <summary>
        /// Identifies the response that can be returned by the message.
        /// </summary>
        public class Response
        {
            /// <summary>
            /// The <see cref="Model.Device"/> identifier this response is for.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the node sending the response.
            /// </summary>
            public string NodeName { get; set; }

            /// <summary>
            /// A list of connections the node has for the proxy device.
            /// </summary>
            public List<Connection> Connections { get; set; }
        }

        /// <summary>
        /// A single connection that is being tracked by the proxy server node.
        /// </summary>
        public class Connection
        {
            /// <summary>
            /// The friendly name of the remote proxy service.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The priority used to decide which connection to send a print
            /// request to.
            /// </summary>
            public int Priority { get; set; }
        }
    }
}
