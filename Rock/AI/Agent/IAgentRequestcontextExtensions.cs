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

using Rock.Configuration;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Extension methods for <see cref="IAgentRequestContext"/>. These provide
    /// additional convenience methods that build on the core methods provided
    /// by the interface.
    /// </summary>
    /// <remarks>
    /// This allows unit tests to provide mocked <see cref="IAgentRequestContext"/>
    /// implementations without having to implement all the various overloads.
    /// </remarks>
    internal static class IAgentRequestContextExtensions
    {
        /// <summary>
        /// Resolves ~/ and ~~/ to the proper URL format. For Chat agents this
        /// will return a relative URL. For MCP agents this will return a full
        /// URL using the appropriate application root.
        /// </summary>
        /// <param name="context">The agent request context.</param>
        /// <param name="url">The relative URL to be formatted.</param>
        /// <returns>The resolved URL.</returns>
        public static string ResolveRockUrl( this IAgentRequestContext context, string url )
        {
            if ( url.IsNullOrWhiteSpace() || url.Contains( "://" ) )
            {
                return url;
            }

            url = RockApp.Current.ResolveRockUrl( url );

            // Chat agents should get relative URLs.
            if ( context.AgentType == AgentType.Chat )
            {
                return url;
            }

            // MCP agents will need full URLs using the appropriate application root.
            return context.RootUrlPath + url;
        }
    }
}
