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

using Rock.Data;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Defines a builder for constructing <see cref="IChatAgent"/> instances for a given agent ID.
    /// </summary>
    public interface IChatAgentBuilder
    {
        /// <summary>
        /// Builds and returns an <see cref="IChatAgent"/> instance for the specified agent ID.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent to build.</param>
        /// <returns>An initialized chat agent instance.</returns>
        IChatAgent Build( int agentId );
    }
}
