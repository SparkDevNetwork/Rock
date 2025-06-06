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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    internal class ChatAgentBuilder : IChatAgentBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly RockContext _rockContext;

        public ChatAgentBuilder( IServiceProvider serviceProvider, RockContext rockContext )
        {
            _serviceProvider = serviceProvider;
            _rockContext = rockContext;
        }

        public IChatAgent Build( int agentId )
        {
            // Disabled cache for now since we need to be able to respond to
            // configuration changes on the provider.

            //var factories = ( ConcurrentDictionary<int, ChatAgentFactory> ) RockCache.GetOrAddExisting( "Rock.AI.Agent.ChatAgentBuilder.Factories",
            //    () => new ConcurrentDictionary<int, ChatAgentFactory>() );

            //var factory = factories.GetOrAdd( agentId, ( id, ctx ) => new ChatAgentFactory( id, ctx, _serviceProvider.GetService<ILoggerFactory>() ), _rockContext );
            var factory = new ChatAgentFactory( agentId, _rockContext, _serviceProvider.GetRequiredService<IRockRequestContextAccessor>(), _serviceProvider.GetRequiredService<ILoggerFactory>() );

            return factory.Build( _serviceProvider );
        }

        //internal void FlushCache()
        //{
        //    RockCache.Remove( "Rock.AI.Agent.ChatAgentBuilder.Factories" );
        //}
    }
}
