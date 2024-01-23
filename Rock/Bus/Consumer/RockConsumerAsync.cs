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

using MassTransit;

using Microsoft.Extensions.Logging;

using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Bus.Consumer
{
    /// <summary>
    /// Rock Consumer
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="Rock.Bus.Consumer.IRockConsumer{TQueue, TMessage}" />
    public abstract class RockConsumerAsync<TQueue, TMessage> : IRockConsumer<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The context
        /// </summary>
        protected ConsumeContext<TMessage> ConsumeContext { get; private set; } = null;

        /// <summary>
        /// Gets the logger for this instance.
        /// </summary>
        /// <value>The logger for this instance.</value>
        protected ILogger Logger
        {
            get
            {
                if ( _logger == null )
                {
                    _logger = RockLogger.LoggerFactory.CreateLogger( GetType().FullName );
                }

                return _logger;
            }
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract Task ConsumeAsync( TMessage message );

        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual async Task Consume( ConsumeContext<TMessage> context )
        {
            Logger.LogDebug( "Rock Task Consumer: {0} TMessage Type: {1} Context: {@context}", GetType(), typeof( TMessage ), context );
            ConsumeContext = context;
            await ConsumeAsync( context.Message );
        }

        /// <summary>
        /// Gets an instance of the queue.
        /// </summary>
        /// <returns></returns>
        public static IRockQueue GetQueue()
        {
            return RockQueue.Get<TQueue>();
        }

        /// <summary>
        /// The instance of this consumer that will be used to consume the messages.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public virtual IRockConsumer Instance => Activator.CreateInstance( GetType() ) as IRockConsumer;
    }
}
