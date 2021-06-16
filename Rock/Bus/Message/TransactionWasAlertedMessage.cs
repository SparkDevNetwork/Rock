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
    /// Alerted Transaction Message Interface
    /// </summary>
    public interface ITransactionWasAlertedMessage : IEventMessage<TransactionAlertQueue>
    {
        /// <summary>
        /// Gets the transaction alert identifier.
        /// </summary>
        int FinancialTransactionAlertId { get; set; }
    }

    /// <summary>
    /// Alerted Transaction Message Class
    /// </summary>
    public class TransactionWasAlertedMessage : ITransactionWasAlertedMessage
    {
        /// <summary>
        /// Gets the alert identifier.
        /// </summary>
        public int FinancialTransactionAlertId { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender node.
        /// </summary>
        /// <value>
        /// The name of the sender node.
        /// </value>
        public string SenderNodeName { get; set; }
    }

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class TransactionWasAlertedMessageExtensions
    {
        /// <summary>
        /// Sends the messages to the bus.
        /// </summary>
        public static void Publish( this ITransactionWasAlertedMessage message )
        {
            _ = RockMessageBus.PublishAsync( message, message.GetType() );
        }
    }
}
