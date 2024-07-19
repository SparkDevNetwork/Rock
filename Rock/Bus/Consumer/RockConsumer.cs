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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MassTransit;

using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;
using Rock.Utility.ExtensionMethods;

namespace Rock.Bus.Consumer
{
    /// <summary>
    /// Rock Consumer Interface.
    /// </summary>
    public interface IRockConsumer
    {
        /// <summary>
        /// The instance of this consumer that will be used to consume the messages.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        IRockConsumer Instance { get; }
    }

    /// <summary>
    /// Rock Consumer Interface
    /// </summary>
    /// <seealso cref="IConsumer" />
    public interface IRockConsumer<TQueue, TMessage> : IRockConsumer, IConsumer<TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
    }

    /// <summary>
    /// Rock Consumer
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="Rock.Bus.Consumer.IRockConsumer{TQueue, TMessage}" />
    public abstract class RockConsumer<TQueue, TMessage> : IRockConsumer<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// The context
        /// </summary>
        protected ConsumeContext<TMessage> ConsumeContext { get; private set; } = null;

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void Consume( TMessage message );

        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual Task Consume( ConsumeContext<TMessage> context )
        {
            RockLogger.Log.Debug( RockLogDomains.Core, "Rock Task Consumer: {0} TMessage Type: {1} Context: {@context}", GetType(), typeof( TMessage ), context );
            ConsumeContext = context;
            var oldActivity = System.Diagnostics.Activity.Current;
            try
            {
                // Bus messages should not inherit the parent activity, but this
                // can happen if the bus message comes from the same server.
                System.Diagnostics.Activity.Current = null;
                Consume( context.Message );
            }
            finally
            {
                System.Diagnostics.Activity.Current = oldActivity;
            }
            return RockMessageBus.GetCompletedTask();
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

    /// <summary>
    /// Rock Message Bus Consumer Helpers
    /// </summary>
    public static class RockConsumer
    {
        private static readonly Type _genericInterfaceType = typeof( IRockConsumer<,> );

        /// <summary>
        /// Configures the rock consumers.
        /// </summary>
        public static void ConfigureRockConsumers( IBusFactoryConfigurator configurator )
        {
            var consumersByQueue = GetConsumerTypes()
                .GroupBy( c => GetQueue( c ) )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var queues = consumersByQueue.Keys;

            foreach ( var queue in queues )
            {
                var consumerTypes = consumersByQueue[queue];

                configurator.ReceiveEndpoint( queue.NameForConfiguration, e =>
                {
                    foreach ( var consumerType in consumerTypes )
                    {
                        e.Consumer( consumerType, ConsumerFactory );
                    }
                } );
            }
        }

        /// <summary>
        /// Gets the consumer types.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetConsumerTypes()
        {
            var consumerTypes = new Dictionary<string, Type>();
            var assemblies = Reflection.GetRockAndPluginAssemblies();

            var types = assemblies
                .SelectMany( a => a.GetTypesSafe()
                .Where( t => t.IsClass && !t.IsNestedPrivate && !t.IsAbstract ) );

            foreach ( var type in types )
            {
                if ( IsRockConsumer( type ) )
                {
                    consumerTypes.TryAdd( type.FullName, type );
                }
            }

            var consumerTypeList = consumerTypes.Select( kvp => kvp.Value ).ToList();
            return consumerTypeList;
        }

        /// <summary>
        /// Determines if the type is a Rock Consumer type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock consumer] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockConsumer( Type type )
        {
            if ( type.IsAbstract || type.ContainsGenericParameters )
            {
                return false;
            }

            var typeInterfaces = type.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the queue for the consumer type.
        /// </summary>
        /// <param name="consumerType">Type of the consumer.</param>
        /// <returns></returns>
        public static IRockQueue GetQueue( Type consumerType )
        {
            var queueType = GetQueueType( consumerType );
            return RockQueue.Get( queueType );
        }

        /// <summary>
        /// Gets the queue type for the consumer type.
        /// </summary>
        /// <param name="consumerType">Type of the consumer.</param>
        /// <returns></returns>
        public static Type GetQueueType( Type consumerType )
        {
            var queueInterface = typeof( IRockQueue );
            var typeInterfaces = consumerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There are two generic type arguments. The first is the queue and the second is the message
                    return typeInterface.GenericTypeArguments[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the message type for the consumer type.
        /// </summary>
        /// <param name="consumerType">Type of the consumer.</param>
        /// <returns></returns>
        public static Type GetMessageType( Type consumerType )
        {
            var messageInterface = typeof( IRockMessage<> );
            var typeInterfaces = consumerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There are two generic type arguments. The first is the queue and the second is the message
                    return typeInterface.GenericTypeArguments[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Create a consumer instance
        /// </summary>
        /// <returns></returns>
        private static object ConsumerFactory( Type consumerType )
        {
            var consumer = Activator.CreateInstance( consumerType ) as IRockConsumer;
            return consumer.Instance;
        }
    }
}
