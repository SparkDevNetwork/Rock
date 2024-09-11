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
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// This command is used to request that a server handling cloud print
    /// connections print the specified labels.
    /// </summary>
    [RockInternal( "1.16.7", true )]
    public class CloudPrintLabelMessage : ICommandMessage<CloudPrintCommandQueue>
    {
        /// <inheritdoc/>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// The identifier of the <see cref="Model.Device"/> that represents the proxy.
        /// </summary>
        public int ProxyDeviceId { get; set; }

        /// <summary>
        /// The identifier of the <see cref="Model.Device"/> that represents the printer.
        /// </summary>
        public int PrinterDeviceId { get; set; }

        /// <summary>
        /// The data to be printed.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets as debug string.
        /// </summary>
        /// <returns></returns>
        internal string ToDebugString()
        {
            return $"ProxyDeviceId = {ProxyDeviceId}; PrinterDeviceId = {PrinterDeviceId}";
        }

        /// <summary>
        /// Requests a server on the bus print the labels and return any error
        /// messages.
        /// </summary>
        /// <param name="proxyDeviceId">The identifier of the <see cref="Model.Device"/> that represents the proxy.</param>
        /// <param name="printerId">The identifier of the <see cref="Model.Device"/> that represents the printer.</param>
        /// <param name="data">The data to be printed.</param>
        /// <param name="cancellationToken">A token that will abort the request and cancel the task.</param>
        /// <returns>The response returned by the node handling the command.</returns>
        internal static async Task<Response> RequestAsync( int proxyDeviceId, int printerId, byte[] data, CancellationToken cancellationToken = default )
        {
            var message = new CloudPrintLabelMessage
            {
                ProxyDeviceId = proxyDeviceId,
                PrinterDeviceId = printerId,
                Data = data
            };

            var queueName = $"rock-cloud-print-command-queue-{proxyDeviceId}";

            return await RockMessageBus.RequestAsync<CloudPrintCommandQueue, CloudPrintLabelMessage, Response>( message, queueName, cancellationToken );
        }

        /// <summary>
        /// Identifies the response that can be returned by the message.
        /// </summary>
        internal class Response
        {
            /// <summary>
            /// Any message that resulted from the print operation.
            /// </summary>
            public string Message { get; set; }
        }
    }
}
