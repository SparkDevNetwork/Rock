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
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.RealTime
{
    /// <summary>
    /// Helper class that provides a single point of access to the RealTime
    /// system in Rock.
    /// </summary>
    public static partial class RealTimeHelper
    {
        #region Properties

        /// <summary>
        /// The engine to use when accessing the real-time system.
        /// </summary>
        internal static Engine Engine { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the real-time system with the provided engine.
        /// </summary>
        /// <param name="engine">The engine to use to provide the real-time communication system.</param>
        internal static void Initialize( Engine engine )
        {
            Engine = engine;
        }

        /// <summary>
        /// Get the context to send messages to connections on a topic.
        /// </summary>
        /// <typeparam name="TTopicClient">The type that identifies the interface associated with the topic.</typeparam>
        /// <returns>An instance of <see cref="ITopicContext{T}"/> scoped to the interface <typeparamref name="TTopicClient"/>.</returns>
        public static ITopicContext<TTopicClient> GetTopicContext<TTopicClient>()
            where TTopicClient : class
        {
            if ( Engine == null )
            {
                throw new Exception( $"{nameof( RealTimeHelper )} has not been initialized properly." );
            }

            return Engine.GetTopicContext<TTopicClient>();
        }

        /// <summary>
        /// Get an instance of the topic specified by its identifier. Each time
        /// an incoming message from a connection is handled a new instance
        /// is created to respond to the message. This is the same way as the
        /// base SignalR works.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="topicIdentifier">The identifier of the topic that should be created.</param>
        /// <returns>A new instance of the topic class that will handle the request.</returns>
        internal static ITopicInternal GetTopicInstance( object realTimeHub, string topicIdentifier )
        {
            if ( Engine == null )
            {
                throw new Exception( $"{nameof( RealTimeHelper )} has not been initialized properly." );
            }

            return Engine.GetTopicInstance( realTimeHub, topicIdentifier );
        }

        /// <summary>
        /// Sends a message to the specified clients scoped to the topic.
        /// </summary>
        /// <param name="proxy">The proxy object that will send the actual message.</param>
        /// <param name="topicIdentifier">The identifier of the topic that the message will be scoped to.</param>
        /// <param name="messageName">The name that identifies the message to be sent.</param>
        /// <param name="parameters">The parameters to be passed in the message.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.14.1", true )]
        public static Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken )
        {
            if ( Engine == null )
            {
                throw new Exception( $"{nameof( RealTimeHelper )} has not been initialized properly." );
            }

            var camelMessageName = messageName.Substring( 0, 1 ).ToLower() + messageName.Substring( 1 );

            return Engine.SendMessageAsync( proxy, topicIdentifier, camelMessageName, parameters, cancellationToken );
        }

        #endregion
    }
}
