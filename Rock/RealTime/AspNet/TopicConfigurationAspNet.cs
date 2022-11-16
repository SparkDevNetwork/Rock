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

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// ASP.Net implementation of <see cref="TopicConfiguration"/>.
    /// </summary>
    internal class TopicConfigurationAspNet : TopicConfiguration
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicConfigurationAspNet"/> and
        /// configures values specific to ASP.Net implementations.
        /// </summary>
        /// <param name="rockHubContext">The main realtime hub that will handle access to connections.</param>
        /// <param name="topicType">The <see cref="Type"/> that describes the topic to be configured.</param>
        /// <param name="proxyFactory">The proxy factory that will create proxies for sending messages to specific connections.</param>
        public TopicConfigurationAspNet( IHubContext<IRockHubClientProxy> rockHubContext, Type topicType, Lazy<TopicProxyFactory<IClientProxy>> proxyFactory )
            : base( topicType )
        {
            var topicIdentifier = topicType.FullName;
            var clientsType = typeof( TopicClients<> ).MakeGenericType( ClientInterfaceType );

            CallerClientsType = typeof( TopicCallerClients<> ).MakeGenericType( ClientInterfaceType );
            TopicContext.Channels = new TopicChannelManager( rockHubContext.Groups, TopicIdentifier );
            TopicContext.Clients = Activator.CreateInstance( clientsType, rockHubContext.Clients, topicIdentifier, proxyFactory );
        }

        #endregion
    }
}
