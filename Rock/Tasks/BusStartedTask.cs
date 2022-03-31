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

using System;
using System.Threading.Tasks;

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Tasks
{
    /// <summary>
    /// A message for carrying arguments needed to execute a task. This should be a small simple POCO class.
    /// For larger tasks, use <see cref="Rock.Transactions.ITransaction"></see> instead.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: The serialized size of a BusStartedTaskMessage needs to less than 64KB.
    /// See https://stackoverflow.com/questions/66664488/what-is-the-maximum-size-in-a-masstransit-message which says
    /// "From a guidance perspective, messages over 64k can really slow down broker performance, messages over 256k are a definite no for many systems."
    /// </remarks>
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
        /// <inheritdoc cref="RockMessageBus.SendAsync{TQueue, TMessage}(TMessage)"/>
        public static void Send( this BusStartedTaskMessage message )
        {
            _ = RockMessageBus.SendAsync( message, message.GetType() );
        }

        /// <summary>
        /// Sends the command message when the specified task completes successfully. See also <seealso cref="RockMessageBus.SendAsync{TQueue, TMessage}(TMessage)" />
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="taskCompletionSource">The task completion source.</param>
        public static void SendWhen( this BusStartedTaskMessage message, TaskCompletionSource<bool> taskCompletionSource)
        {
            Task.Run( async () =>
            {
                // Wait for specified task to complete, then send the message if the task completed successfully
                var completedSuccessfully = await taskCompletionSource.Task;
                if ( completedSuccessfully )
                {
                    message.Send();
                }
            } );
        }
    }

    /// <summary>
    /// Bus Started Task for the <see cref="BusStartedTaskMessage" />
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="BusStartedTaskMessage"/>
    /// </remarks>
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
