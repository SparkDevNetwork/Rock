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
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Handles <see cref="CloudPrintLabelMessage"/> messages by checking
    /// if this node has any connected services for the indicated proxy device
    /// and sends the label data to the proxy.
    /// </summary>
    [DynamicConsumer]
    internal class CloudPrintLabelConsumer : RockConsumer<CloudPrintCommandQueue, CloudPrintLabelMessage>
    {
        /// <inheritdoc/>
        public override async Task Consume( ConsumeContext<CloudPrintLabelMessage> context )
        {
            var proxy = CloudPrintSocket.GetBestProxyForDevice( context.Message.ProxyDeviceId );
            var printer = DeviceCache.Get( context.Message.PrinterDeviceId );

            if ( proxy == null || printer == null )
            {
                return;
            }

            var message = await proxy.PrintAsync( printer, context.Message.Data );

            var response = new CloudPrintLabelMessage.Response
            {
                Message = message
            };

            context.Respond( response );
        }

        /// <inheritdoc/>
        public override void Consume( CloudPrintLabelMessage message )
        {
            // This will never be called.
        }
    }
}
