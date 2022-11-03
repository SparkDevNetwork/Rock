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
using System.Reflection;
using System.ServiceModel.Channels;

using Microsoft.AspNet.SignalR.Hubs;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// The ASP.Net implementation for <see cref="ITopicClients{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    internal class TopicClients<T> : ITopicClients<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The context used to access the clients.
        /// </summary>
        private readonly IHubConnectionContext<IRockHubClientProxy> _context;

        /// <summary>
        /// The proxy factory for sending messages to the topic.
        /// </summary>
        private readonly Lazy<TopicProxyFactory<IClientProxy>> _proxyFactory;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the identifier of the topic this instance represents.
        /// </summary>
        /// <value>
        /// The identifier of the topic this instance represents.
        /// </value>
        protected string TopicIdentifier { get; }

        /// <summary>
        /// Gets the proxy factory for sending messages to the topic.
        /// </summary>
        /// <value>
        /// The proxy factory for sending messages to the topic.
        /// </value>
        protected TopicProxyFactory<IClientProxy> ProxyFactory => _proxyFactory.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicClients{T}"/> to facilitate
        /// communication with the specified topic.
        /// </summary>
        /// <param name="context">The SignalR context used to access the clients.</param>
        /// <param name="topicIdentifier">The identifier of the topic this instance will represent.</param>
        /// <param name="lazyProxyFactory">The proxy factory for sending messages to this topic.</param>
        public TopicClients( IHubConnectionContext<IRockHubClientProxy> context, string topicIdentifier, Lazy<TopicProxyFactory<IClientProxy>> lazyProxyFactory )
        {
            _context = context;
            TopicIdentifier = topicIdentifier;
            _proxyFactory = lazyProxyFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the proxy object <typeparamref name="T"/> that will be used to
        /// facilitate communication to <paramref name="proxy"/>.
        /// </summary>
        /// <param name="proxy">The SignalR proxy object representing a set of client connections.</param>
        /// <returns>An implementation for <typeparamref name="T"/> that can be used to send messages to the clients.</returns>
        protected T GetProxy( IRockHubClientProxy proxy )
        {
            var dynamicProxyField = proxy.GetType().GetField( "_proxy", BindingFlags.Instance | BindingFlags.NonPublic );
            var clientProxy = ( IClientProxy ) dynamicProxyField.GetValue( proxy );

            return ProxyFactory.GetDynamicProxy<T>( clientProxy, TopicIdentifier );
        }

        #endregion

        #region ITopicClients

        /// <inheritdoc/>
        public T All => GetProxy( _context.Group( TopicIdentifier ) );

        /// <inheritdoc/>
        public T AllExcept( IReadOnlyList<string> excludedConnectionIds )
        {
            return GetProxy( _context.Group( TopicIdentifier, excludedConnectionIds.ToArray() ) );
        }

        /// <inheritdoc/>
        public T Client( string connectionId )
        {
            return GetProxy( _context.Client( connectionId ) );
        }

        /// <inheritdoc/>
        public T Clients( IReadOnlyList<string> connectionIds )
        {
            return GetProxy( _context.Clients( connectionIds.ToArray() ) );
        }

        /// <inheritdoc/>
        public T Channel( string channelName )
        {
            return GetProxy( _context.Group( $"{TopicIdentifier}-{channelName}" ) );
        }

        /// <inheritdoc/>
        public T ChannelExcept( string channelName, IReadOnlyList<string> excludedConnectionIds )
        {
            return GetProxy( _context.Groups( new[] { $"{TopicIdentifier}-{channelName}" }, excludedConnectionIds.ToArray() ) );
        }

        /// <inheritdoc/>
        public T Channels( IReadOnlyList<string> channelNames )
        {
            return GetProxy( _context.Groups( channelNames.Select( cn => $"{TopicIdentifier}-{cn}" ).ToArray() ) );
        }

        /// <inheritdoc/>
        public T Person( int personId )
        {
            return GetProxy( _context.Group( $"{TopicIdentifier}-rock:person:{personId}" ) );
        }

        /// <inheritdoc/>
        public T People( IReadOnlyList<int> personIds )
        {
            return GetProxy( _context.Groups( personIds.Select( id => $"{TopicIdentifier}-rock:person:{id}" ).ToArray() ) );
        }

        /// <inheritdoc/>
        public T Visitor( int visitorAliasId )
        {
            return GetProxy( _context.Group( $"{TopicIdentifier}-rock:visitor:{visitorAliasId}" ) );
        }

        #endregion
    }
}
