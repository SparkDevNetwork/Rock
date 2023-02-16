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

using Microsoft.AspNet.SignalR.Hubs;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// The ASP.Net implementation for <see cref="ITopicCallerClients{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    internal class TopicCallerClients<T> : TopicClients<T>, ITopicCallerClients<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The context used to access the clients.
        /// </summary>
        private readonly IHubCallerConnectionContext<IRockHubClientProxy> _context;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicCallerClients{T}"/> to facilitate
        /// communication with the specified topic.
        /// </summary>
        /// <param name="context">The SignalR context used to access the clients.</param>
        /// <param name="topicIdentifier">The identifier of the topic this instance will represent.</param>
        /// <param name="lazyProxyFactory">The proxy factory for sending messages to this topic.</param>
        public TopicCallerClients( IHubCallerConnectionContext<IRockHubClientProxy> context, string topicIdentifier, Lazy<TopicProxyFactory<IClientProxy>> lazyProxyFactory )
            : base( context, topicIdentifier, lazyProxyFactory )
        {
            _context = context;
        }

        #endregion

        #region ITopicCallerClients

        /// <inheritdoc/>
        public T Current => GetProxy( _context.Caller );

        /// <inheritdoc/>
        public T Others => GetProxy( _context.Others );

        /// <inheritdoc/>
        public T OthersInChannel( string channelName )
        {
            return GetProxy( _context.OthersInGroup( channelName ) );
        }

        #endregion
    }
}
