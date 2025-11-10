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

namespace Rock.AI.Agent
{
    /// <summary>
    /// <para>
    /// Defines a builder for constructing <see cref="IChatAgent"/> instances
    /// for a given agent identifier.
    /// </para>
    /// <para>
    /// This should not be inherited by plugins as additional methods and
    /// properties may be added in the future.
    /// </para>
    /// </summary>
    internal interface IChatAgentBuilder
    {
        /// <summary>
        /// Builds and returns an <see cref="IChatAgent"/> instance for the specified agent ID.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent to build.</param>
        /// <param name="options">The options that describe how the chat agent should be constructed.</param>
        /// <returns>An initialized chat agent instance.</returns>
        IChatAgent Build( int agentId, ChatAgentOptions options );
    }
}
