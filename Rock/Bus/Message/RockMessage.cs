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
using System.Linq;
using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// A Rock Bus Message
    /// </summary>
    public interface IRockMessage<TQueue>
        where TQueue : IRockQueue, new()
    {
        /// <summary>
        /// Gets or sets the sending rock instance node name.
        /// </summary>
        /// <value>
        /// The rock instance node name.
        /// </value>
        string SenderNodeName { get; set; }
    }

    /// <summary>
    /// Rock Message Static Helpers
    /// </summary>
    public static class RockMessage
    {
        private static readonly Type _genericInterfaceType = typeof( IRockMessage<> );

        /// <summary>
        /// Determines if the type is a Rock MEssage type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock message] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockMessage( Type type )
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
        /// Gets the queue for the message type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        public static IRockQueue GetQueue( Type messageType )
        {
            var queueType = GetQueueType( messageType );
            return RockQueue.Get( queueType );
        }

        /// <summary>
        /// Gets the queue type for the message type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        public static Type GetQueueType( Type messageType )
        {
            var queueInterface = typeof( IRockQueue );
            var typeInterfaces = messageType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There is one generic type arguments, which is the queue
                    return typeInterface.GenericTypeArguments[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the log string.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string GetLogString<TQueue>( IRockMessage<TQueue> message )
            where TQueue : IRockQueue, new()
        {
            var messageJson = message.ToJson();
            var queueName = RockQueue.Get<TQueue>().Name;
            var messageType = message.GetType().FullName;

            return $"Queue: {queueName}\nMessageType: {messageType}\n{messageJson}";
        }
    }
}
