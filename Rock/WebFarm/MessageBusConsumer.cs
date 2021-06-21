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

using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Model;

namespace Rock.WebFarm
{
    /// <summary>
    /// Web Farm Message Bus Consumer
    /// </summary>
    public sealed class MessageBusConsumer : RockConsumer<WebFarmQueue, WebFarmWasUpdatedMessage>
    {
        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( WebFarmWasUpdatedMessage message )
        {
            if ( !RockWebFarm.IsRunning() )
            {
                // Don't act on any messages until this node is fully started
                return;
            }

            switch ( message.MessageType )
            {
                case RockWebFarm.EventType.Ping:
                    RockWebFarm.OnReceivedPing( message.SenderNodeName, message.Payload.AsGuidOrNull() );
                    break;
                case RockWebFarm.EventType.Pong:
                    RockWebFarm.OnReceivedPong( message.SenderNodeName, message.RecipientNodeName, message.Payload.AsGuidOrNull() );
                    break;
                case RockWebFarm.EventType.Startup:
                    RockWebFarm.OnReceivedStartup( message.SenderNodeName );
                    break;
                case RockWebFarm.EventType.Shutdown:
                    RockWebFarm.OnReceivedShutdown( message.SenderNodeName, message.Payload );
                    break;
                case RockWebFarm.EventType.Availability:
                    RockWebFarm.OnReceivedAvailabilityChange( message.SenderNodeName, message.Payload );
                    break;
                case RockWebFarm.EventType.Error:
                    RockWebFarm.OnReceivedError( message.SenderNodeName, message.Payload );
                    break;
                case RockWebFarm.EventType.RecyclePing:
                    RockWebFarm.OnReceivedRecyclePing( message.SenderNodeName, message.RecipientNodeName, message.Payload.AsGuidOrNull() );
                    break;
                case RockWebFarm.EventType.RecyclePong:
                    RockWebFarm.OnReceivedRecyclePong( message.SenderNodeName, message.RecipientNodeName, message.Payload.AsGuidOrNull() );
                    break;
                default:
                    ExceptionLogService.LogException( $"Web farm received a message with an unexpected type: {message.MessageType}" );
                    break;
            }
        }
    }
}
