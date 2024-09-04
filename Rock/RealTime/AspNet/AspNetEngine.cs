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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

using Rock.RealTime.AspNet;

namespace Rock.RealTime
{
    /// <summary>
    /// ASP.Net implementation of the RealTime engine.
    /// </summary>
    internal class AspNetEngine : Engine
    {
        #region Fields

        /// <summary>
        /// The proxy factory that will be used to create message sending proxies.
        /// </summary>
        private readonly Lazy<TopicProxyFactory<IClientProxy>> _proxyFactory;

        /// <summary>
        /// The standard context that provides communication to connected clients.
        /// </summary>
        private readonly IHubContext<IRockHubClientProxy> _rockHubContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AspNetEngine"/> and
        /// sets the values to default.
        /// </summary>
        /// <param name="hubConfiguration">The <see cref="HubConfiguration"/> that describes the ASP.Net hub.</param>
        public AspNetEngine( object hubConfiguration )
        {
            if ( !( hubConfiguration is HubConfiguration configuration ) )
            {
                throw new ArgumentException( "Invalid configuration object.", nameof( hubConfiguration ) );
            }

            var connectionManager = configuration.Resolver.Resolve<IConnectionManager>();

            _rockHubContext = connectionManager.GetHubContext<RealTimeHub, IRockHubClientProxy>();
            _proxyFactory = new Lazy<TopicProxyFactory<IClientProxy>>( () => new TopicProxyFactory<IClientProxy>( RegisteredTopics ) );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Task ClientConnectedAsync( object realTimeHub, string connectionIdentifier )
        {
            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            var requestWrapper = new SignalRRequestWrapper( ( realTimeHub as RealTimeHub ).Context.Request );
            state.Request = new Net.RockRequestContext( requestWrapper, new Net.NullRockResponseContext(), Model.UserLoginService.GetCurrentUser( false ) );

            return base.ClientConnectedAsync( realTimeHub, connectionIdentifier );
        }

        /// <inheritdoc/>
        protected override TopicConfiguration GetTopicConfiguration( Type topicType )
        {
            var topicConfiguration = new TopicConfigurationAspNet( _rockHubContext, topicType, _proxyFactory );

            topicConfiguration.TopicContext.Engine = this;

            return topicConfiguration;
        }

        /// <inheritdoc/>
        protected override void ConfigureTopicInstance( object realTimeHub, TopicConfiguration topicConfiguration, ITopicInternal topicInstance )
        {
            if ( !( realTimeHub is RealTimeHub hub ) )
            {
                throw new ArgumentException( "Invalid hub object.", nameof( realTimeHub ) );
            }

            topicInstance.Channels = new TopicChannelManager( hub.Groups, topicConfiguration.TopicIdentifier );
            topicInstance.Clients = Activator.CreateInstance( topicConfiguration.CallerClientsType, hub.Clients, topicConfiguration.TopicIdentifier, _proxyFactory );
            topicInstance.Context = new Context( hub.Context.ConnectionId, hub.Context.User, this );
            topicInstance.Engine = this;
        }

        /// <inheritdoc/>
        public override Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken )
        {
            if ( proxy is IClientProxy clientProxy )
            {
                return clientProxy.Invoke( "message", new object[] { topicIdentifier, messageName, parameters } );
            }
            else
            {
                throw new ArgumentException( "Invalid proxy type.", nameof( proxy ) );
            }
        }

        /// <inheritdoc/>
        protected override async Task RemoveFromTopicChannelsAsync( object realTimeHub, string connectionIdentifier, string topicIdentifier, IEnumerable<string> channelNames )
        {
            if ( !( realTimeHub is RealTimeHub hub ) )
            {
                throw new ArgumentException( "Invalid hub object.", nameof( realTimeHub ) );
            }

            foreach ( var channelName in channelNames )
            {
                await hub.Groups.Remove( connectionIdentifier, GetQualifiedChannelName( topicIdentifier, channelName ) );
            }

            await hub.Groups.Remove( connectionIdentifier, GetQualifiedAllChannelName( topicIdentifier ) );
        }

        #endregion
    }
}
