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

using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Web Farm Was Updated Message
    /// </summary>
    internal interface IWebFarmWasUpdatedMessage : IEventMessage<WebFarmQueue>
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        string Payload { get; set; }
    }

    /// <summary>
    /// Cache Update Message
    /// </summary>
    public sealed class WebFarmWasUpdatedMessage : IWebFarmWasUpdatedMessage
    {
        /// <summary>
        /// Gets the node name.
        /// </summary>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipient node.
        /// </summary>
        public string RecipientNodeName { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public string Payload { get; set; }

        /// <summary>
        /// Publishes the specified entity.
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        /// <param name="messageType">The message.</param>
        /// <param name="recipientNodeName">Name of the recipient node.</param>
        /// <param name="payload">The payload.</param>
        public static void Publish( string senderNodeName, string messageType, string recipientNodeName = "", string payload = "" )
        {
            var webFarmWasUpdatedMessage = new WebFarmWasUpdatedMessage
            {
                SenderNodeName = senderNodeName,
                MessageType = messageType,
                RecipientNodeName = recipientNodeName,
                Payload = payload
            };

            _ = RockMessageBus.PublishAsync<WebFarmQueue, WebFarmWasUpdatedMessage>( webFarmWasUpdatedMessage );
        }
    }
}
