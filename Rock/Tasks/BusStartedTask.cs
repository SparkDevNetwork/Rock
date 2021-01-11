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

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Tasks
{
    /// <summary>
    /// A bus started task message.
    /// For carrying arguments needed to execute a task.
    /// </summary>
    public abstract class BusStartedTaskMessage : ICommandMessage<StartTaskQueue>
    {
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
    public static class BusStartedTaskMessageExtensions
    {
        /// <summary>
        /// Sends the messages to the bus.
        /// </summary>
        public static void Send( this BusStartedTaskMessage message )
        {
            _ = RockMessageBus.SendAsync( message, message.GetType() );
        }
    }

    /// <summary>
    /// Bus Started Task
    /// </summary>
    public abstract class BusStartedTask<TMessage> : RockConsumer<StartTaskQueue, TMessage>
        where TMessage : BusStartedTaskMessage
    {
        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( TMessage message )
        {
            Execute( message );
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute( TMessage message );
    }
}
