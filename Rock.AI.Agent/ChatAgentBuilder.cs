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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides functionality to build instances of <see cref="IChatAgent"/>.
    /// </summary>
    internal class ChatAgentBuilder : IChatAgentBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentBuilder"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies for the chat agent.</param>
        public ChatAgentBuilder( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IChatAgent Build( int agentId, ChatAgentOptions options )
        {
            var rockContext = _serviceProvider.GetRequiredService<RockContext>();
            var requestContextAccessor = _serviceProvider.GetRequiredService<IRockRequestContextAccessor>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var rockContextFactory = _serviceProvider.GetRequiredService<IRockContextFactory>();

            if ( options.IsDebugEnabled )
            {
                loggerFactory = new ChatAgentDebugLoggerFactory( loggerFactory );
            }

            var factory = new ChatAgentFactory( agentId, _serviceProvider, rockContext, requestContextAccessor, loggerFactory, rockContextFactory, options  );

            return factory.Build();
        }
    }
}
