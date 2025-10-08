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

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Extension methods for <see cref="IChatAgentBuilder"/>. These provide
    /// additional convenience methods that build on the core methods provided
    /// by the interface.
    /// </summary>
    [RockInternal( "18.0" )]
    internal static class IChatAgentBuilderExtensions
    {
        /// <summary>
        /// Builds and returns an <see cref="IChatAgent"/> instance for the specified agent ID.
        /// </summary>
        /// <param name="builder">The chat agent builder instance.</param>
        /// <param name="agentId">The unique identifier of the agent to build.</param>
        /// <returns>An initialized chat agent instance.</returns>
        public static IChatAgent Build( this IChatAgentBuilder builder, int agentId )
        {
            return builder.Build( agentId, new ChatAgentOptions() );
        }
    }
}
